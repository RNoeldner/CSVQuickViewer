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

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using Timer = System.Timers.Timer;

namespace CsvTools
{
  /// <summary>
  ///   Form to Display a CSV File
  /// </summary>
  public sealed partial class FormMain : ResizeForm
  {
    private static readonly string
      m_SettingFolder = Environment.ExpandEnvironmentVariables("%APPDATA%\\CSVQuickViewer");

    private static readonly string m_SettingPath = m_SettingFolder + "\\Setting.xml";

    private static readonly XmlSerializer m_SerializerViewSettings = new XmlSerializer(typeof(ViewSettings));

    private readonly CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();

    private readonly Timer m_SettingsChangedTimerChange = new Timer(200);

    private readonly Collection<Column> m_StoreColumns = new Collection<Column>();

    private readonly ViewSettings m_ViewSettings;

    private Tuple<EncodingHelper.CodePage, bool> m_CodePage;

    private bool m_ConfigChanged;

    private bool m_FileChanged;

    private ICsvFile m_FileSetting;

    private ICollection<string> m_Headers;
    private ToolStripButton m_ToolStripButtonAsText;
    private ToolStripButton m_ToolStripButtonSettings;
    private ToolStripButton m_ToolStripButtonSource;
    private FormCsvTextDisplay m_SourceDisplay;

    private int m_WarningCount;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormMain" /> class.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    public FormMain(string fileName)
    {
      InitializeComponent();
      detailControl.AddToolStripItem(1, m_ToolStripButtonSettings);
      detailControl.AddToolStripItem(int.MaxValue, m_ToolStripButtonSource);
      detailControl.AddToolStripItem(int.MaxValue, m_ToolStripButtonAsText);
      Text = AssemblyTitle;
      m_ViewSettings = LoadViewSettings();

      textPanel.SuspendLayout();
      textPanel.Dock = DockStyle.Fill;
      ClearProcess();
      textPanel.ResumeLayout();
      ShowTextPanel(true);

      // in case there is no filename open a dialog
      if (string.IsNullOrEmpty(fileName) || !FileSystemUtils.FileExists(fileName))
      {
        var strFilter = "Supported files|*.csv;*.txt;*.tab;*.tsv;*.dat;*.json;*.gz|"
                        + "Delimited files (*.csv;*.txt;*.tab;*.tsv;*.dat)|*.csv;*.txt;*.tab;*.tsv;*.dat|"
                        + "Json files (*.json)|*.json|"
                        + "All files (*.*)|*.*";

        if (m_ViewSettings.StoreSettingsByFile)
          strFilter += "|Setting files (*" + CsvFile.cCsvSettingExtension + ")|*" + CsvFile.cCsvSettingExtension;

        fileName = WindowsAPICodePackWrapper.Open(".", "Setting File", strFilter, null);
      }

      // Just starting the task of loading the file
#pragma warning disable 4014
      LoadCsvFile(fileName);
#pragma warning restore 4014

      this.LoadWindowState(m_ViewSettings.WindowPosition);

      detailControl.MoveMenu();

      m_ViewSettings.FillGuessSettings.PropertyChanged += AnyPropertyChangedReload;
      SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
      SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
      m_SettingsChangedTimerChange.AutoReset = false;
      m_SettingsChangedTimerChange.Elapsed += async (sender, args) => await OpenDataReaderAsync(true);
      m_SettingsChangedTimerChange.Stop();
    }

    // used in Unit Tests to check loaded data
    public DataTable DataTable
    {
      get;
      private set;
    }

    // used in Unit Tests to determine when a load process is finished.
    public bool LoadFinished
    {
      get;
      private set;
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
        var titleAttribute = (AssemblyTitleAttribute) attributes[0];
        if (titleAttribute.Title.Length != 0)
          return titleAttribute.Title + " " + version;

        return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
      }
    }

    private static ViewSettings LoadViewSettings()
    {
      try
      {
        Logger.Debug("Loading defaults {path}", m_SettingPath);
        if (FileSystemUtils.FileExists(m_SettingPath))
        {
          var serial = File.ReadAllText(m_SettingPath);
          using (TextReader reader = new StringReader(serial))
          {
            var vs = (ViewSettings) m_SerializerViewSettings.Deserialize(reader);
            ApplicationSetting.MenuDown = vs.MenuDown;
            return vs;
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
        Logger.Warning("No further warnings displayed");
      else if (m_WarningCount < m_FileSetting.NumWarnings)
        Logger.Warning(args.Display(true, true));
    }

    /// <summary>
    ///   As any property is changed this will cause a reload from file
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">
    ///   The <see cref="PropertyChangedEventArgs" /> instance containing the event data.
    /// </param>
    /// <remarks>Called by ViewSettings.FillGuessSetting or Columns</remarks>
    private void AnyPropertyChangedReload(object sender, PropertyChangedEventArgs e) => m_ConfigChanged = true;

    /// <summary>
    ///   Attaches the property changed handlers for the file Settings
    /// </summary>
    /// <param name="fileSetting">The file setting.</param>
    private void AttachPropertyChanged(IFileSetting fileSetting)
    {
      fileSystemWatcher.EnableRaisingEvents = m_ViewSettings.DetectFileChanges;
      fileSetting.PropertyChanged += FileSetting_PropertyChanged;
      fileSetting.FileFormat.PropertyChanged += AnyPropertyChangedReload;
      foreach (var col in fileSetting.ColumnCollection)
      {
        col.PropertyChanged += AnyPropertyChangedReload;
        col.ValueFormatMutable.PropertyChanged += AnyPropertyChangedReload;
      }
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
    private async void DataGridView_DragDropAsync(object sender, DragEventArgs e)
    {
      // Set the filename
      var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
      if (files.Length <= 0) return;
      SaveIndividualFileSetting();
      await LoadCsvFile(files[0]);
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
    private void DetachPropertyChanged(IFileSetting fileSetting)
    {
      m_SettingsChangedTimerChange.Stop();
      fileSystemWatcher.EnableRaisingEvents = false;
      if (fileSetting != null)
      {
        fileSetting.PropertyChanged -= FileSetting_PropertyChanged;
        fileSetting.FileFormat.PropertyChanged -= AnyPropertyChangedReload;
        foreach (var col in fileSetting.ColumnCollection)
        {
          col.PropertyChanged -= AnyPropertyChangedReload;
          col.ValueFormatMutable.PropertyChanged -= AnyPropertyChangedReload;
        }
      }
    }

    private async void DetailControl_ButtonAsText(object sender, EventArgs e)
    {
      // Assume data type is not recognize
      if (m_FileSetting.ColumnCollection.Any(x => x.ValueFormat.DataType != DataType.String))
      {
        Logger.Debug("Showing columns as text");
        m_FileSetting.ColumnCollection.CollectionCopy(m_StoreColumns);
        m_FileSetting.ColumnCollection.Clear();
        m_ToolStripButtonAsText.Text = "Values";
      }
      else
      {
        Logger.Debug("Showing columns as values");
        m_ToolStripButtonAsText.Text = "Text";
        m_StoreColumns.CollectionCopy(m_FileSetting.ColumnCollection);
      }

      await OpenDataReaderAsync(true);
    }

    private void DetailControl_ButtonShowSource(object sender, EventArgs e)
    {
      try
      {
        if (m_SourceDisplay == null)
        {
          m_SourceDisplay = new FormCsvTextDisplay();
          using (var proc = new FormProcessDisplay("Display Source", false, m_CancellationTokenSource.Token))
          {
            m_SourceDisplay.Show();
            proc.Show(this);

            proc.Maximum = 0;
            proc.SetProcess("Reading source and applying color coding", 0, false);

            m_SourceDisplay.OpenFile(m_FileSetting.FullPath, m_FileSetting.JsonFormat,
              m_FileSetting.FileFormat.FieldQualifierChar,
              m_FileSetting.FileFormat.FieldDelimiterChar, m_FileSetting.FileFormat.EscapeCharacterChar,
              (int) m_CodePage.Item1, m_FileSetting.SkipRows, m_FileSetting.FileFormat.CommentLine);
            proc.Close();

            m_SourceDisplay.FormClosed += SourceDisplayClosed;
          }
        }
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
        m_SourceDisplay?.Close();
        m_SourceDisplay=null;
      }
    }

    private void SourceDisplayClosed(object sender, FormClosedEventArgs e)
    {
      m_SourceDisplay?.Dispose();
      m_SourceDisplay = null;
    }

    private void DisableIgnoreRead()
    {
      foreach (var col in m_FileSetting.ColumnCollection)
        if (col.Ignore)
          col.Ignore = false;
    }

    private async Task CheckPossibleChange()
    {
      try
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
            await OpenDataReaderAsync(true);
          else
            m_ConfigChanged = false;
        }

        if (!m_FileChanged) return;
        m_FileChanged = false;
        if (_MessageBox.Show(
          this,
          "The displayed file has changed do you want to reload the data?",
          "File changed",
          MessageBoxButtons.YesNo,
          MessageBoxIcon.Question,
          MessageBoxDefaultButton.Button2) == DialogResult.Yes)
          await OpenDataReaderAsync(true);
        else
          m_FileChanged = false;
      }
      catch (Exception exception)
      {
        Logger.Warning(exception, "Checking for changes");
      }
    }

    /// <summary>
    ///   Handles the Activated event of the Display control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private async void FormMain_Activated(object sender, EventArgs e) => await CheckPossibleChange();

    private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
    {
      if (!m_CancellationTokenSource.IsCancellationRequested)
      {
        m_CancellationTokenSource.Cancel();
        // Give the possibly running threads some time to exit
        Thread.Sleep(100);
      }

      if (e.CloseReason == CloseReason.UserClosing)
      {
        Logger.Debug("Closing Form");

        var res = this.StoreWindowState();
        if (res != null)
          m_ViewSettings.WindowPosition = res;
        SaveViewSettings();
        SaveIndividualFileSetting();
      }
    }

    /// <summary>
    ///   Handles the PropertyChanged event of the FileSetting control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///   The <see cref="PropertyChangedEventArgs" /> instance containing the event data.
    /// </param>
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
          || e.PropertyName == nameof(ICsvFile.TreatUnknownCharacterAsSpace)
          || e.PropertyName == nameof(ICsvFile.TryToSolveMoreColumns)
          || e.PropertyName == nameof(ICsvFile.WarnDelimiterInValue)
          || e.PropertyName == nameof(ICsvFile.WarnEmptyTailingColumns)
          || e.PropertyName == nameof(ICsvFile.WarnLineFeed)
          || e.PropertyName == nameof(ICsvFile.WarnNBSP)
          || e.PropertyName == nameof(ICsvFile.WarnQuotes)
          || e.PropertyName == nameof(ICsvFile.WarnQuotesInQuotes)
          || e.PropertyName == nameof(ICsvFile.WarnUnknownCharacter)
          || e.PropertyName == nameof(ICsvFile.DisplayStartLineNo)
          || e.PropertyName == nameof(ICsvFile.DisplayRecordNo)
          || e.PropertyName == nameof(ICsvFile.WarnUnknownCharacter)
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

    private async void FormMain_KeyUpAsync(object sender, KeyEventArgs e)
    {
      if (e.KeyCode != Keys.F5 && (!e.Control || e.KeyCode != Keys.R)) return;
      e.Handled = true;
      await OpenDataReaderAsync(true);
    }

    /// <summary>
    ///   Initializes the file settings.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private async Task LoadCsvFile(string fileName)
    {
      if (string.IsNullOrEmpty(fileName) || !FileSystemUtils.FileExists(fileName))
        return;
      if (fileName.IndexOf('~') != -1)
        fileName = FileSystemUtils.LongFileName(fileName);
      try
      {
        if (fileName.EndsWith(CsvFile.cCsvSettingExtension, StringComparison.OrdinalIgnoreCase))
        {
          m_FileSetting = SerializedFilesLib.LoadCsvFile(fileName);

          fileName = fileName.Substring(0, fileName.Length - CsvFile.cCsvSettingExtension.Length);
          DisableIgnoreRead();
        }
        else
        {
          LoadFinished = false;
          ClearProcess();
          var sDisplay = FileSystemUtils.GetShortDisplayFileName(fileName, 40);
          Logger.Information("Examining file {filename}", fileName);
          Text = $@"{sDisplay} {AssemblyTitle}";

          Cursor.Current = Cursors.WaitCursor;
          DetachPropertyChanged(m_FileSetting);

          m_FileSetting = new CsvFile();
          ViewSettings.CopyConfiguration(m_ViewSettings, m_FileSetting);
          m_FileSetting.FileName = fileName;
          m_FileSetting.ID = fileName.GetIdFromFileName();
          var fileInfo = new FileSystemUtils.FileInfo(fileName);

          Logger.Information($"Size of file: {StringConversion.DynamicStorageSize(fileInfo.Length)}");

          if (FileSystemUtils.FileExists(fileName + CsvFile.cCsvSettingExtension))
          {
            m_FileSetting = SerializedFilesLib.LoadCsvFile(fileName + CsvFile.cCsvSettingExtension);
            m_FileSetting.FileName = fileName;
            Logger.Information(
              "Configuration read from setting file {filename}",
              fileName + CsvFile.cCsvSettingExtension);
            DisableIgnoreRead();
          }
          else
          {
            m_FileSetting.FileName = fileName;
            try
            {
              using (var processDisplay = new FormProcessDisplay(
                m_FileSetting.ToString(),
                false,
                m_CancellationTokenSource.Token))
              {
                processDisplay.Show();

                m_FileSetting.HasFieldHeader = true;

                await m_FileSetting.RefreshCsvFileAsync(
                  processDisplay,
                  m_ViewSettings.AllowJson,
                  m_ViewSettings.GuessCodePage,
                  m_ViewSettings.GuessDelimiter,
                  m_ViewSettings.GuessQualifier,
                  m_ViewSettings.GuessStartRow,
                  m_ViewSettings.GuessHasHeader,
                  m_ViewSettings.GuessNewLine);

                await m_FileSetting.FillGuessColumnFormatReaderAsync(
                  true,
                  false,
                  m_ViewSettings.FillGuessSettings,
                  processDisplay);
              }
            }
            catch (Exception ex)
            {
              this.ShowError(ex, "Inspecting file");
            }

            m_ToolStripButtonAsText.Visible= m_FileSetting.ColumnCollection.Any(x => x.ValueFormat.DataType != DataType.String);
          }
        }

        if (m_ViewSettings.DetectFileChanges)
        {
          var fileInfo = new FileInfo(fileName);
          fileSystemWatcher.Filter = fileInfo.Name;
          fileSystemWatcher.Path = fileInfo.FullName.GetDirectoryName();
        }

        await OpenDataReaderAsync(false);
      }
      catch (Exception ex)
      {
        this.ShowError(ex, "Opening File");
      }
      finally
      {
        Cursor.Current = Cursors.Default;
        ShowTextPanel(false);
      }
    }

    /// <summary>
    ///   Opens the data reader.
    /// </summary>
    private async Task OpenDataReaderAsync(bool clear)
    {
      if (m_FileSetting == null)
        return;

      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;

      // Stop Property changed events for the time this is processed We might store data in the FileSetting
      DetachPropertyChanged(m_FileSetting);

      try
      {
        if (clear)
          ClearProcess();

        using (var improvedStream = FunctionalDI.OpenRead(m_FileSetting.FullPath))
        {
          m_CodePage = await CsvHelper.GuessCodePageAsync(improvedStream, m_CancellationTokenSource.Token);
        }

        Text =
          $"{FileSystemUtils.GetShortDisplayFileName(m_FileSetting.FileName, 40)} - {EncodingHelper.GetEncodingName(m_CodePage.Item1, true, m_CodePage.Item2)} - {AssemblyTitle}";

        using (var processDisplay = m_FileSetting.GetProcessDisplay(this, false, m_CancellationTokenSource.Token))
        {
          if (processDisplay is IProcessDisplayTime pdt)
            pdt.AttachTaskbarProgress();
          processDisplay.SetProcess("Opening File...", -1, true);
          detailControl.FileSetting = m_FileSetting;
          detailControl.FillGuessSettings = m_ViewSettings.FillGuessSettings;
          detailControl.CancellationToken = m_CancellationTokenSource.Token;

          using (var fileReader = FunctionalDI.GetFileReader(m_FileSetting, TimeZoneInfo.Local.Id, processDisplay))
          {
            if (!textPanel.Visible)
              ShowTextPanel(true);
            fileReader.Warning += AddWarning;
            var warningList = new RowErrorCollection(fileReader);
            fileReader.Warning -= warningList.Add;
            await fileReader.OpenAsync(processDisplay.CancellationToken);
            warningList.HandleIgnoredColumns(fileReader);
            warningList.PassWarning += AddWarning;

            // Store the header in this might be used later on by FormColumnUI
            m_Headers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
            {
              var cf = fileReader.GetColumn(colIndex);
              if (!string.IsNullOrEmpty(cf.Name) && !cf.Ignore)
                m_Headers.Add(cf.Name);
            }

            FunctionalDI.GetColumnHeader = (dummy1, dummy3) => Task.FromResult(m_Headers);

            processDisplay.SetMaximum(0);
            processDisplay.SetProcess("Reading data...", -1, true);
            processDisplay.SetMaximum(100);
            detailControl.ShowInfoButtons = false;
            DataTable = await fileReader.GetDataTableAsync(m_FileSetting.RecordLimit, true,
              m_FileSetting.DisplayStartLineNo, m_FileSetting.DisplayRecordNo, m_FileSetting.DisplayEndLineNo, false,
              ShowDataTable, (l, i) =>
              processDisplay.SetProcess($"Reading data...\nRecord: {l:N0}", i, false),
              processDisplay.CancellationToken);

            if (m_DisposedValue)
              return;

            foreach (var columnName in DataTable.GetRealColumns())
              if (m_FileSetting.ColumnCollection.Get(columnName) == null)
                m_FileSetting.ColumnCollection.AddIfNew(new Column { Name = columnName });
          }
        }

        // The reader is used when data ist stored through the detailControl
        FunctionalDI.SQLDataReader = async (settingName, message, timeout, token) =>
        {
          var dt = new DataTableWrapper(detailControl.DataTable);
          await dt.OpenAsync(token);
          return dt;
        };

        if (DataTable != null)
          ShowDataTable(DataTable);

        // Load View Settings
        var index = m_FileSetting.ID.LastIndexOf('.');
        var fn = (index == -1 ? m_FileSetting.ID : m_FileSetting.ID.Substring(0, index)) + ".col";
        var fnView = Path.Combine(m_FileSetting.FileName.GetDirectoryName(), fn);
        if (FileSystemUtils.FileExists(fnView))
        {
          Logger.Information("Restoring view and filter setting {filename}...", fn);
          detailControl.ReStoreViewSetting(fnView);
        }
      }
      catch (Exception exc)
      {
        if (!m_DisposedValue)
          this.ShowError(exc, "Opening File");
      }
      finally
      {
        if (!m_DisposedValue)
        {
          if (DataTable == null)
            Logger.Information("No data...");
          else
            // if (!m_FileSetting.NoDelimitedFile)
            ShowTextPanel(false);

          detailControl.ShowInfoButtons = true;
          Cursor.Current = oldCursor;

          m_ConfigChanged = false;
          m_FileChanged = false;

          // Re enable event watching
          AttachPropertyChanged(m_FileSetting);

          LoadFinished = true;
        }
      }
    }

    private void ShowDataTable(DataTable dataTable)
    {
      Logger.Information("Showing loaded data...");
      this.SafeBeginInvoke(() =>
      {
        ShowTextPanel(false);
        detailControl.DataTable = dataTable;
      });
      FunctionalDI.SignalBackground();
    }

    /// <summary>
    /// </summary>
    private void SaveViewSettings()
    {
      try
      {
        ApplicationSetting.MenuDown = m_ViewSettings.MenuDown;

        if (!FileSystemUtils.DirectoryExists(m_SettingFolder))
          FileSystemUtils.CreateDirectory(m_SettingFolder);

        FileSystemUtils.DeleteWithBackup(m_SettingPath, false);
        using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
        {
          // Remove and Restore FileName
          var oldFN = m_ViewSettings.FileName;
          m_ViewSettings.FileName = string.Empty;

          m_SerializerViewSettings.Serialize(
            stringWriter,
            m_ViewSettings,
            SerializedFilesLib.EmptyXmlSerializerNamespaces.Value);
          File.WriteAllText(m_SettingPath, stringWriter.ToString());

          m_ViewSettings.FileName = oldFN;
        }
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "Save Default Settings");
      }
    }

    private void SaveIndividualFileSetting()
    {
      try
      {
        if (m_FileSetting != null && m_ConfigChanged && m_ViewSettings.StoreSettingsByFile)
        {
          var pathSetting = m_FileSetting.FileName + CsvFile.cCsvSettingExtension;
          m_FileSetting.FileName = FileSystemUtils.GetFileName(m_FileSetting.FileName);

          Logger.Debug("Saving setting {path}", pathSetting);
          SerializedFilesLib.SaveCsvFile(
            pathSetting,
            m_FileSetting,
            () => _MessageBox.Show(
              this,
              $"Replace changed settings in {pathSetting} ?",
              "Settings",
              MessageBoxButtons.YesNo,
              MessageBoxIcon.Question) == DialogResult.Yes);
        }

        m_ConfigChanged = false;
      }
      catch (Exception ex)
      {
        this.ShowError(ex, "Storing Settings");
      }
    }

    private async void ShowSettings(object sender, EventArgs e)
    {
      try
      {
        ViewSettings.CopyConfiguration(m_FileSetting, m_ViewSettings);
        using (var frm = new FormEditSettings(m_ViewSettings))
        {
          frm.ShowDialog(MdiParent);
          SaveViewSettings();
          ApplicationSetting.MenuDown = m_ViewSettings.MenuDown;
          ViewSettings.CopyConfiguration(m_ViewSettings, m_FileSetting);

          await CheckPossibleChange();
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