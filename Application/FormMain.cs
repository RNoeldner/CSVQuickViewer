/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com/
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
 * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License along with this program.
 * If not, see http://www.gnu.org/licenses/ .
 *
 */

namespace CsvTools
{
  using Microsoft.Win32;
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.ComponentModel;
  using System.Data;
  using System.Diagnostics.CodeAnalysis;
  using System.Globalization;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Windows.Forms;
  using System.Xml.Serialization;
  using Timer = System.Timers.Timer;

  /// <summary>
  ///   Form to Display a CSV File
  /// </summary>
  public sealed partial class FormMain : ResizeForm
  {
    private static readonly string m_SettingFolder = Environment.ExpandEnvironmentVariables("%APPDATA%\\CSVQuickViewer");

    private static readonly string m_SettingPath = m_SettingFolder + "\\Setting.xml";

    private static readonly XmlSerializer m_SerializerViewSettings = new XmlSerializer(typeof(ViewSettings));

    private readonly CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();

    private readonly Timer m_SettingsChangedTimerChange = new Timer(200);

    private readonly Collection<Column> m_StoreColumns = new Collection<Column>();

    private readonly ViewSettings m_ViewSettings;

    private bool m_ConfigChanged;

    private bool m_FileChanged;

    private string m_FileName;

    private CsvFile m_FileSetting;

    private HashSet<string> m_Headers;

    private int m_WarningCount;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormMain" /> class.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    public FormMain(string fileName)
    {
      m_FileName = fileName;
      m_ViewSettings = LoadDefault();
      m_ViewSettings.PGPInformation.AllowSavingPassphrase = true;
      m_ViewSettings.FillGuessSettings.PropertyChanged += AnyPropertyChangedReload;

      ApplicationSetting.PGPKeyStorage = m_ViewSettings.PGPInformation;

      InitializeComponent();
      FillFromProperties();

      m_SettingsChangedTimerChange.AutoReset = false;
      m_SettingsChangedTimerChange.Elapsed += delegate { this.SafeInvoke(() => OpenDataReader(true)); };
      m_SettingsChangedTimerChange.Stop();

      // Done in code to be able to select controls in the designer
      textPanel.SuspendLayout();
      textPanel.Dock = DockStyle.Fill;
      ClearProcess();

      csvTextDisplay.Dock = DockStyle.Fill;
      textPanel.ResumeLayout();
      ShowTextPanel(true);

      Text = AssemblyTitle;

      SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
      SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
    }

    private static string AssemblyTitle
    {
      get
      {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        var attributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
        if (attributes.Length <= 0)
          return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
        var titleAttribute = (AssemblyTitleAttribute)attributes[0];
        if (titleAttribute.Title.Length != 0)
          return titleAttribute.Title + " " + version;

        return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
      }
    }

    private static ViewSettings LoadDefault()
    {
      try
      {
        Logger.Debug("Loading defaults {path}", m_SettingPath);
        if (FileSystemUtils.FileExists(m_SettingPath))
        {
          var serial = File.ReadAllText(m_SettingPath);
          using (TextReader reader = new StringReader(serial))
          {
            return (ViewSettings)m_SerializerViewSettings.Deserialize(reader);
          }
        }
      }
      catch (Exception ex)
      {
        Logger.Error(ex, "Loading defaults {path}", m_SettingPath);
      }

      return new ViewSettings();
    }

    private void AddWarning(object sender, WarningEventArgs args)
    {
      if (string.IsNullOrEmpty(args.Message))
        return;
      if (++m_WarningCount == m_FileSetting.NumWarnings)
        Logger.Warning("No further warnings displayed…");
      else if (m_WarningCount < m_FileSetting.NumWarnings)
        Logger.Warning(args.Display(true, true));
    }

    /// <summary>
    ///   As any property is changed this will cause a reload from file
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
    /// <remarks>Called by ViewSettings.FillGuessSetting or Columns</remarks>
    private void AnyPropertyChangedReload(object sender, PropertyChangedEventArgs e) => m_ConfigChanged = true;

    /// <summary>
    ///   Attaches the property changed handlers for the file Settings
    /// </summary>
    /// <param name="fileSetting">The file setting.</param>
    private void AttachPropertyChanged(ICsvFile fileSetting)
    {
      fileSystemWatcher.EnableRaisingEvents = m_ViewSettings.DetectFileChanges;
      fileSetting.PropertyChanged += FileSetting_PropertyChanged;
      fileSetting.FileFormat.PropertyChanged += AnyPropertyChangedReload;
      foreach (var col in fileSetting.ColumnCollection)
        col.PropertyChanged += AnyPropertyChangedReload;
    }

    private void ClearProcess()
    {
      m_WarningCount = 0;
      if (IsDisposed)
        return;
      if (!textPanel.Visible)
        ShowTextPanel(true);

      textBoxProgress.Clear();
      textBoxProgress.Visible = true;
      textBoxProgress.Dock = DockStyle.Fill;
      Logger.AddLog = textBoxProgress.AddLog;
    }

    /// <summary>
    ///   Handles the DragDrop event of the dataGridView control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="DragEventArgs" /> instance containing the event data.</param>
    private void DataGridView_DragDrop(object sender, DragEventArgs e)
    {
      // Set the filename
      var files = (string[])e.Data.GetData(DataFormats.FileDrop);

      // store old Setting
      if (m_FileSetting != null && !m_FileName.Equals(files[0], StringComparison.OrdinalIgnoreCase) && m_ConfigChanged)
        SaveIndividualFileSetting();

      m_FileName = files[0];

      if (InitFileSettings())
        OpenDataReader(false);
    }

    /// <summary>
    ///   Handles the DragEnter event of the dataGridView control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="DragEventArgs" /> instance containing the event data.</param>
    private void DataGridView_DragEnter(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
        e.Effect = DragDropEffects.All;
    }

    /// <summary>
    ///   Detaches the property changed handlers for the file Setting
    /// </summary>
    /// <param name="fileSetting">The file setting.</param>
    private void DetachPropertyChanged(ICsvFile fileSetting)
    {
      m_SettingsChangedTimerChange.Stop();
      fileSystemWatcher.EnableRaisingEvents = false;
      if (fileSetting != null)
      {
        fileSetting.PropertyChanged -= FileSetting_PropertyChanged;
        fileSetting.FileFormat.PropertyChanged -= AnyPropertyChangedReload;
        foreach (var col in fileSetting.ColumnCollection)
          col.PropertyChanged -= AnyPropertyChangedReload;
      }
    }

    private void DetailControl_ButtonAsText(object sender, EventArgs e)
    {
      // Assume data type is not recognize
      if (m_FileSetting.ColumnCollection.Any(x => x.DataType != DataType.String))
      {
        Logger.Debug("Showing columns as text");
        m_FileSetting.ColumnCollection.CollectionCopy(m_StoreColumns);
        m_FileSetting.ColumnCollection.Clear();
        detailControl.ButtonAsTextCaption = "Values";
      }
      else
      {
        Logger.Debug("Showing columns as values");
        detailControl.ButtonAsTextCaption = "Text";
        m_StoreColumns.CollectionCopy(m_FileSetting.ColumnCollection);
      }

      OpenDataReader(true);
    }

    private void DetailControl_ButtonShowSource(object sender, EventArgs e)
    {
      textBoxProgress.Visible = false;
      Logger.AddLog = null;
      csvTextDisplay.Visible = true;
      csvTextDisplay.CsvFile = m_FileSetting;
      ShowTextPanel(true);
      buttonCloseText.Visible = true;
    }

    private void DisableIgnoreRead()
    {
      foreach (var col in m_FileSetting.ColumnCollection)
        if (col.Ignore)
          col.Ignore = false;
    }

    /// <summary>
    ///   Handles the Activated event of the Display control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void Display_Activated(object sender, EventArgs e)
    {
      if (m_ConfigChanged)
      {
        m_ConfigChanged = false;
        detailControl.MoveMenu();
        if (_MessageBox.Show(
              this,
              "The configuration has changed do you want to reload the data?",
              "Configuration changed",
              MessageBoxButtons.YesNo,
              MessageBoxIcon.Question,
              MessageBoxDefaultButton.Button2) == DialogResult.Yes)
        {
          OpenDataReader(true);
        }
        else
        {
          m_ConfigChanged = false;
        }
      }

      if (m_FileChanged)
      {
        m_FileChanged = false;
        if (_MessageBox.Show(
              this,
              "The displayed file has changed do you want to reload the data?",
              "File changed",
              MessageBoxButtons.YesNo,
              MessageBoxIcon.Question,
              MessageBoxDefaultButton.Button2) == DialogResult.Yes)
        {
          OpenDataReader(true);
        }
        else
        {
          m_FileChanged = false;
        }

      }
    }

    private void Display_FormClosing(object sender, FormClosingEventArgs e)
    {
      Logger.Debug("Closing Form");
      m_CancellationTokenSource.Cancel();
      var res = this.StoreWindowState();
      if (res != null)
        m_ViewSettings.WindowPosition = res;

      SaveDefault();
      if (m_ViewSettings.StoreSettingsByFile)
        SaveIndividualFileSetting();
    }

    /// <summary>
    ///   Handles the Shown event of the Display control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void Display_Shown(object sender, EventArgs e)
    {
      this.LoadWindowState(m_ViewSettings.WindowPosition);
      if (string.IsNullOrEmpty(m_FileName) || !FileSystemUtils.FileExists(m_FileName))
      {
        var strFilter = (m_ViewSettings.StoreSettingsByFile)
                          ? "Delimited files (*.csv;*.txt;*.tab;*.tsv;*.dat)|*.csv;*.txt;*.tab;*.tsv;*.dat|Setting files (*"
                            + CsvFile.cCsvSettingExtension + ")|*" + CsvFile.cCsvSettingExtension
                            + "|All files (*.*)|*.*"
                          : "Delimited files (*.csv;*.txt;*.tab;*.tsv;*.dat)|*.csv;*.txt;*.tab;*.tsv;*.dat|All files (*.*)|*.*";
        m_FileName = WindowsAPICodePackWrapper.Open(".", "Setting File", strFilter, null);
      }

      var doClose = false;
      if (string.IsNullOrEmpty(m_FileName) || !FileSystemUtils.FileExists(m_FileName))
      {
        doClose = true;
      }
      else
      {
        if (m_FileName.EndsWith(CsvFile.cCsvSettingExtension, StringComparison.OrdinalIgnoreCase))
        {
          m_FileSetting = SerializedFilesLib.LoadCsvFile(m_FileName);

          // Ignore all information in m_FileSetting.FileName
          m_FileSetting.FileName = m_FileName.Substring(0, m_FileName.Length - CsvFile.cCsvSettingExtension.Length);
          m_FileName = m_FileSetting.FileName;
          DisableIgnoreRead();
        }
        else
        {
          doClose = !InitFileSettings();
        }
      }

      if (doClose)
        return;
      OpenDataReader(false);
    }

    /// <summary>
    ///   Handles the PropertyChanged event of the FileSetting control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
    private void FileSetting_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(ICsvFile.AllowRowCombining)
         || e.PropertyName == nameof(ICsvFile.ByteOrderMark)
         || e.PropertyName == nameof(ICsvFile.CodePageId)
         || e.PropertyName == nameof(ICsvFile.ConsecutiveEmptyRows)
         || e.PropertyName == nameof(ICsvFile.DoubleDecode)
         || e.PropertyName == nameof(ICsvFile.HasFieldHeader)
         || e.PropertyName == nameof(ICsvFile.NumWarnings)
         || e.PropertyName == nameof(ICsvFile.SkipEmptyLines)
         || e.PropertyName == nameof(ICsvFile.SkipRows)
         || e.PropertyName == nameof(ICsvFile.TreatLFAsSpace)
         || e.PropertyName == nameof(ICsvFile.TreatNBSPAsSpace)
         || e.PropertyName == nameof(ICsvFile.TreatTextAsNull)
         || e.PropertyName == nameof(ICsvFile.TreatUnknowCharaterAsSpace)
         || e.PropertyName == nameof(ICsvFile.TryToSolveMoreColumns)
         || e.PropertyName == nameof(ICsvFile.WarnDelimiterInValue)
         || e.PropertyName == nameof(ICsvFile.WarnEmptyTailingColumns)
         || e.PropertyName == nameof(ICsvFile.WarnLineFeed)
         || e.PropertyName == nameof(ICsvFile.WarnNBSP)
         || e.PropertyName == nameof(ICsvFile.WarnQuotes)
         || e.PropertyName == nameof(ICsvFile.WarnQuotesInQuotes)
         || e.PropertyName == nameof(ICsvFile.WarnUnknowCharater)
         || e.PropertyName == nameof(ICsvFile.FileName))
        m_ConfigChanged = true;
    }

    /// <summary>
    ///   Handles the Changed event of the fileSystemWatcher control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///   The <see cref="FileSystemEventArgs" /> instance containing the event data.
    /// </param>
    private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e) =>
      m_FileChanged |= e.FullPath == m_FileSetting.FileName && e.ChangeType == WatcherChangeTypes.Changed;

    private void FillFromProperties()
    {
      ApplicationSetting.MenuDown = m_ViewSettings.MenuDown;
      detailControl.MoveMenu();
      if (m_FileSetting == null)
        return;
      ViewSettings.CopyConfiuration(m_ViewSettings, m_FileSetting);
      m_FileSetting.FileName = m_FileName;
    }

    private void FormMain_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.F5 || (e.Control && e.KeyCode == Keys.R))
      {
        e.Handled = true;
        OpenDataReader(true);
      }
    }

    /// <summary>
    ///   Initializes the file settings.
    /// </summary>
    /// <returns></returns>
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    private bool InitFileSettings()
    {
      const int c_Maxsize = 1048576 * 20;

      if (string.IsNullOrEmpty(m_FileName) || !FileSystemUtils.FileExists(m_FileName))
        return false;

      ClearProcess();
      var sDisplay = FileSystemUtils.GetShortDisplayFileName(m_FileName, 80);
      Logger.Information("Examining file {filename}", m_FileName);
      Text = $@"{AssemblyTitle} : {sDisplay}";

      Cursor.Current = Cursors.WaitCursor;
      DetachPropertyChanged(m_FileSetting);

      m_FileSetting = new CsvFile();
      ViewSettings.CopyConfiuration(m_ViewSettings, m_FileSetting);
      m_FileSetting.FileName = m_FileName;

      if (m_FileName.AssumePgp() && (ApplicationSetting.PGPKeyStorage?.PrivateKeys?.Length == 0))
      {
        var res = _MessageBox.Show(
          this,
          "The private key for decryption has not been setup.\n\nDo you want to add them now ?",
          "Decryption",
          MessageBoxButtons.YesNoCancel,
          MessageBoxIcon.Question,
          timeout: 5);
        if (res == DialogResult.Cancel)
          return false;

        if (res == DialogResult.Yes)
        {
          using (var frm = new FormEditSettings(m_ViewSettings))
          {
            frm.tabControl.SelectedTab = frm.tabPagePGP;
            frm.ShowDialog(this);
            FillFromProperties();
          }

          SaveDefault();
        }
      }

      m_FileSetting.GetEncryptedPassphraseFunction = m_FileSetting.GetEncryptedPassphraseOpenForm;

      try
      {
        var analyse = true;
        var fileInfo = new FileInfo(m_FileName);
        m_FileSetting.ID = m_FileName.GetIdFromFileName();
        Logger.Information($"Size of file: {StringConversion.DynamicStorageSize(fileInfo.Length)}");

        FrmLimitSize limitSizeForm = null;
        if (fileInfo.Length > c_Maxsize)
        {
          limitSizeForm = new FrmLimitSize();
          limitSizeForm.Show();

          // As the form closes it will store the information
          limitSizeForm.FormClosing += (sender, args) =>
            {
              m_FileSetting.RecordLimit = limitSizeForm.RecordLimit;
              limitSizeForm = null;
            };
        }

        try
        {
          if (FileSystemUtils.FileExists(m_FileName + CsvFile.cCsvSettingExtension))
          {
            m_FileSetting = SerializedFilesLib.LoadCsvFile(m_FileName + CsvFile.cCsvSettingExtension);
            m_FileSetting.FileName = m_FileName;
            Logger.Information(
              "Configuration read from setting file {filename}",
              m_FileName + CsvFile.cCsvSettingExtension);
            DisableIgnoreRead();
            analyse = false;

            // Add all columns as string
            m_ConfigChanged = false;
          }
        }
        catch (Exception exc)
        {
          _MessageBox.Show(
            this,
            exc.ExceptionMessages(),
            "Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning,
            timeout: 0);
        }

        if (analyse)
        {
          try
          {
            using (var processDisplay = new FormProcessDisplay(
              m_FileSetting.ToString(),
              false,
              m_CancellationTokenSource.Token))
            {
              processDisplay.Show();
              if (limitSizeForm != null)
              {
                processDisplay.Left = limitSizeForm.Left + limitSizeForm.Width;
                limitSizeForm.Focus();
              }

              m_FileSetting.HasFieldHeader = true;
              Task.Run(
                () =>
                  {
                    m_FileSetting.RefreshCsvFile(
                      processDisplay,
                      m_ViewSettings.AllowJson,
                      m_ViewSettings.GuessCodePage,
                      m_ViewSettings.GuessDelimiter,
                      m_ViewSettings.GuessQualifier,
                      m_ViewSettings.GuessStartRow,
                      m_ViewSettings.GuessHasHeader);
                    m_FileSetting.FillGuessColumnFormatReader(
                      true,
                      false,
                      m_ViewSettings.FillGuessSettings,
                      processDisplay);
                  },
                processDisplay.CancellationToken).WaitToCompleteTaskUI(240);
            }
          }
          catch (Exception ex)
          {
            this.ShowError(ex, "Inspecting file");
          }

          if (m_FileSetting.ColumnCollection.Any(x => x.DataType != DataType.String))
          {
            detailControl.ButtonShowSource += DetailControl_ButtonShowSource;
            detailControl.ButtonAsText += DetailControl_ButtonAsText;
          }

          // wait for the size from to close (it closes automatically)
          while (limitSizeForm != null)
            Extensions.ProcessUIElements(125);
        }

        if (m_ViewSettings.DetectFileChanges)
        {
          fileSystemWatcher.Filter = fileInfo.Name;
          fileSystemWatcher.Path = fileInfo.FullName.GetDirectoryName();
        }
      }
      catch (Exception ex)
      {
        this.ShowError(ex, "Opening File");
        return false;
      }
      finally
      {
        Cursor.Current = Cursors.Default;
        ShowTextPanel(false);
      }

      return true;
    }

    /// <summary>
    ///   Opens the data reader.
    /// </summary>
    private void OpenDataReader(bool clear)
    {
      if (m_FileSetting == null)
        return;

      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;

      // Stop Property changed events for the time this is processed
      // We might store data in the FileSetting
      DetachPropertyChanged(m_FileSetting);

      try
      {
        if (clear)
          ClearProcess();
        Logger.Information("Opening File…");

        Text =
          $@"{AssemblyTitle} : {FileSystemUtils.GetShortDisplayFileName(m_FileSetting.FileName, 80)}  - {EncodingHelper.GetEncodingName(m_FileSetting.CurrentEncoding.CodePage, true, m_FileSetting.ByteOrderMark)}";

        DataTable data;
        using (var processDisplay = m_FileSetting.GetProcessDisplay(this, false, m_CancellationTokenSource.Token))
        {
          if (processDisplay is IProcessDisplayTime pdt)
            pdt.AttachTaskbarProgress();

          using (var csvDataReader = ApplicationSetting.GetFileReader(m_FileSetting, processDisplay))
          {
            var warningList = new RowErrorCollection(csvDataReader);
            csvDataReader.Open();
            warningList.HandleIgnoredColumns(csvDataReader);
            warningList.PassWarning += AddWarning;

            // Store the header in this might be used later on by FormColumnUI
            m_Headers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var colindex = 0; colindex < csvDataReader.FieldCount; colindex++)
            {
              var cf = csvDataReader.GetColumn(colindex);
              if (!string.IsNullOrEmpty(cf.Name) && !cf.Ignore)
                m_Headers.Add(cf.Name);
            }

            ApplicationSetting.GetColumnHeader = (dummy1, dummy3) => m_Headers;
            if (warningList.CountRows > 0)
              _MessageBox.Show(
                this,
                warningList.Display,
                "Opening CSV File",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            if (!textPanel.Visible)
              ShowTextPanel(true);
            csvDataReader.Warning -= warningList.Add;
            csvDataReader.Warning += AddWarning;
            Logger.Information("Reading data…");
            data = csvDataReader.WriteToDataTable(
              m_FileSetting,
              m_FileSetting.RecordLimit,
              processDisplay.CancellationToken);

            foreach (var columnName in data.GetRealColumns())
              if (m_FileSetting.ColumnCollection.Get(columnName) == null)
                m_FileSetting.ColumnCollection.AddIfNew(new Column { Name = columnName });
            if (processDisplay.CancellationToken.IsCancellationRequested)
            {
              Logger.Information("Cancellation was requested.");
              if (_MessageBox.Show(
                    this,
                    "The load was not completed, cancellation was requested.\rDo you want to display the already loaded data?",
                    "Cancellation Requested",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.No)
                Close();
            }
          }
        }

        detailControl.CancellationToken = m_CancellationTokenSource.Token;

        if (data != null)
        {
          // Show the data
          Logger.Information("Showing loaded data…");
          detailControl.DataTable = data;
        }

        ApplicationSetting.SQLDataReader =
          (settingName, processDisplay, timeout) => detailControl.DataTable.CreateDataReader();
        detailControl.FileSetting = m_FileSetting;
        detailControl.FillGuessSettings = m_ViewSettings.FillGuessSettings;

        // if (m_FileSetting.NoDelimitedFile)
        // detailControl_ButtonShowSource(this, null);
      }
      catch (ObjectDisposedException)
      {
      }
      catch (Exception exc)
      {
        this.ShowError(exc, "Storing Settings");
      }
      finally
      {
        if (detailControl.DataTable == null)
          Logger.Information("No data…");
        else

          // if (!m_FileSetting.NoDelimitedFile)
          ShowTextPanel(false);
        Cursor.Current = oldCursor;
        foreach (var col in m_FileSetting.ColumnCollection)
        {
          col.PropertyChanged += AnyPropertyChangedReload;
        }

        m_ConfigChanged = false;
        m_FileChanged = false;

        // Re enable event watching
        AttachPropertyChanged(m_FileSetting);
      }
    }

    private void SaveDefault()
    {
      try
      {
        if (!FileSystemUtils.DirectoryExists(m_SettingFolder))
          FileSystemUtils.CreateDirectory(m_SettingFolder);

        FileSystemUtils.DeleteWithBackup(m_SettingPath, false);
        using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
        {
          m_ViewSettings.FileName = string.Empty;
          m_SerializerViewSettings.Serialize(
            stringWriter,
            m_ViewSettings,
            SerializedFilesLib.EmptyXmlSerializerNamespaces.Value);
          File.WriteAllText(m_SettingPath, stringWriter.ToString());
        }

        Display_Activated(this, null);
      }
      catch (Exception)
      {
        // ignored
      }
    }

    private void SaveIndividualFileSetting()
    {
      try
      {
        var pathSetting = m_FileSetting.FileName + CsvFile.cCsvSettingExtension;
        m_FileSetting.FileName = FileSystemUtils.SplitPath(m_FileSetting.FileName).FileName;

        Logger.Debug("Saving setting {path}", pathSetting);
        SerializedFilesLib.SaveCsvFile(
          pathSetting,
          m_FileSetting,
          () => (_MessageBox.Show(
                   this,
                   $"Replace changed settings in {pathSetting} ?",
                   "Settings",
                   MessageBoxButtons.YesNo,
                   MessageBoxIcon.Question) == DialogResult.Yes));
        m_ConfigChanged = false;
      }
      catch (Exception ex)
      {
        this.ShowError(ex, "Storing Settings");
      }
    }

    private void ShowGrid(object sender, EventArgs e) => ShowTextPanel(false);

    private void ShowSettings(object sender, EventArgs e)
    {
      try
      {
        ViewSettings.CopyConfiuration(m_FileSetting, m_ViewSettings);
        using (var frm = new FormEditSettings(m_ViewSettings))
        {
          frm.ShowDialog(MdiParent);
          FillFromProperties();
          SaveDefault();
        }
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
      }
    }

    private void ShowTextPanel(bool visible)
    {
      textPanel.Visible = visible;
      detailControl.Visible = !visible;
    }

    private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e) =>
      this.LoadWindowState(m_ViewSettings.WindowPosition);

    private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
      switch (e.Mode)
      {
        case PowerModes.Suspend:
          Logger.Debug("Power Event Suspend");
          var res = this.StoreWindowState();
          if (res == null)
            return;
          m_ViewSettings.WindowPosition = res;
          break;

        case PowerModes.Resume:
          Logger.Debug("Power Event Resume");
          this.LoadWindowState(m_ViewSettings.WindowPosition);
          break;
      }
    }
  }
}