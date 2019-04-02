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

using CsvTools.Properties;
using log4net;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using Timer = System.Timers.Timer;

namespace CsvTools
{
  /// <summary>
  ///   Form to Display a CSV File
  /// </summary>
  public sealed partial class FormMain : Form
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private static readonly XmlSerializer m_SerializerViewSettings = new XmlSerializer(typeof(ViewSettings));
    private static string cSettingFolder = Environment.ExpandEnvironmentVariables("%APPDATA%\\CSVQuickViewer");
    private static string cSettingPath = cSettingFolder + "\\Setting.xml";
    private readonly CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();
    private readonly Timer m_SettingsChangedTimerChange = new Timer(200);
    private readonly Collection<Column> m_StoreColumns = new Collection<Column>();
    private readonly ViewSettings m_ViewSettings;
    private string m_FileName;
    private bool m_ConfigChanged;
    private CancellationTokenSource m_CurrentCancellationTokenSource;
    private bool m_FileChanged;
    private CsvFile m_FileSetting;
    private int m_WarningCount;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormMain" /> class.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    public FormMain(string fileName)
    {
      m_FileName = fileName;
      m_ViewSettings = LoadDefault();
      m_ViewSettings.FileName = string.Empty;
      m_ViewSettings.PGPInformation.AllowSavingPassphrase = true;

      ApplicationSetting.FillGuessSettings = m_ViewSettings.FillGuessSettings;
      ApplicationSetting.PGPKeyStorage = m_ViewSettings.PGPInformation;
         
      
      InitializeComponent();
      textBoxProgress.Threshold = log4net.Core.Level.Info;
      FillFromProperites(true);

      m_SettingsChangedTimerChange.AutoReset = false;
      m_SettingsChangedTimerChange.Elapsed += delegate { this.SafeInvoke(() => OpenDataReader(true)); };
      m_SettingsChangedTimerChange.Stop();

      // Done in code to be able to select controls in the designer
      textPanel.SuspendLayout();
      textPanel.Dock = DockStyle.Fill;
      textBoxProgress.Dock = DockStyle.Fill;
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
        if (attributes.Length <= 0) return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
        var titleAttribute = (AssemblyTitleAttribute)attributes[0];
        if (titleAttribute.Title.Length != 0)
          return titleAttribute.Title + " " + version;

        return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
      }
    }

    private void AddWarning(object sender, WarningEventArgs args)
    {
      if (string.IsNullOrEmpty(args.Message)) return;
      if (++m_WarningCount == m_FileSetting.NumWarnings)
        Log.Warn("No further warnings displayed…");
      else if (m_WarningCount < m_FileSetting.NumWarnings)
        Log.Warn(args.Display(true, true));
    }

    private void ClearProcess()
    {
      m_WarningCount = 0;
      if (IsDisposed) return;
      if (!textPanel.Visible)
        ShowTextPanel(true);

      textBoxProgress.Clear();
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
        SaveSetting();

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
      if (e.Data.GetDataPresent(DataFormats.FileDrop, false)) e.Effect = DragDropEffects.All;
    }

    private void DetailControl_ButtonAsText(object sender, EventArgs e)
    {

      // Assume data type is not recognize
      if (m_FileSetting.ColumnCollection.Any(x => x.DataType != DataType.String))
      {
        Log.Debug($"Showing columns as text");
        m_FileSetting.ColumnCollection.CollectionCopy(m_StoreColumns);
        m_FileSetting.ColumnCollection.Clear();
        detailControl.ButtonAsTextCaption = "Values";
      }
      else
      {
        Log.Debug($"Showing columns as values");
        detailControl.ButtonAsTextCaption = "Text";
        m_StoreColumns.CollectionCopy(m_FileSetting.ColumnCollection);
      }
      OpenDataReader(true);
    }

    private void DetailControl_ButtonShowSource(object sender, EventArgs e)
    {
      textBoxProgress.Visible = false;
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
      if (!m_FileChanged) return;
      m_FileChanged = false;
      if (_MessageBox.Show(this, "The displayed file has changed do you want to reload the data?", "File changed", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
        OpenDataReader(true);
    }

    private void Display_FormClosing(object sender, FormClosingEventArgs e)
    {
      Log.Debug($"Closing Form");
      m_CancellationTokenSource.Cancel();
      var res = this.StoreWindowState();
      if (res != null)
        m_ViewSettings.WindowPosition = res;

      SaveDefault();
      if (m_ConfigChanged && m_ViewSettings.StoreSettingsByFile)
        SaveSetting();
    }

    /// <summary>
    ///   Handles the Shown event of the Display control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void Display_Shown(object sender, EventArgs e)
    {
      this.LoadWindowState(m_ViewSettings.WindowPosition);
      Log.Debug($"Show {m_FileName}");
      if (string.IsNullOrEmpty(m_FileName) || !FileSystemUtils.FileExists(m_FileName))
        using (var openFileDialog = new OpenFileDialog())
        {
          if (m_ViewSettings.StoreSettingsByFile)
            openFileDialog.Filter =
              "Delimited files (*.csv;*.txt;*.tab;*.tsv;*.dat)|*.csv;*.txt;*.tab;*.tsv;*.dat|Setting files (*" +
              CsvFile.cCsvSettingExtension + ")|*" + CsvFile.cCsvSettingExtension + "|All files (*.*)|*.*";
          else
            openFileDialog.Filter =
              "Delimited files (*.csv;*.txt;*.tab;*.tsv;*.dat)|*.csv;*.txt;*.tab;*.tsv;*.dat|All files (*.*)|*.*";
          openFileDialog.ValidateNames = false;
          if (openFileDialog.ShowDialog(MdiParent) == DialogResult.OK) m_FileName = openFileDialog.FileName.LongFileName();
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
          m_ConfigChanged = false;
        }
        else
        {
          m_ConfigChanged = true;
          doClose = !InitFileSettings();
        }
      }

      if (doClose) return;
      OpenDataReader(false);
    }

    private void FileSetting_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      m_ConfigChanged = true;
      // if (e.PropertyName != "ColumnFormat") return;
      // reload the data
      if (e.PropertyName == nameof(Column.DataType))
      {
        m_SettingsChangedTimerChange.Stop();
        m_SettingsChangedTimerChange.Start();
      }
    }

    /// <summary>
    ///   Handles the Changed event of the fileSystemWatcher control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///   The <see cref="FileSystemEventArgs" /> instance containing the event data.
    /// </param>
    private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
    {
      m_FileChanged |= e.FullPath == m_FileSetting.FileName && e.ChangeType == WatcherChangeTypes.Changed;
    }

    private void FillFromProperites(bool changeSetting)
    {
      ApplicationSetting.MenuDown = m_ViewSettings.MenuDown;
      detailControl.MoveMenu();
      if (m_FileSetting == null || !changeSetting)
        return;
      m_ViewSettings.CopyTo(m_FileSetting);
      m_FileSetting.FileName = m_FileName;
    }

    /// <summary>
    ///   Initializes the file settings.
    /// </summary>
    /// <returns></returns>
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    private bool InitFileSettings()
    {
      const int maxsize = 1048576 * 20;

      if (string.IsNullOrEmpty(m_FileName) || !FileSystemUtils.FileExists(m_FileName))
        return false;

      ClearProcess();
      Log.Info($"Examining file {m_FileName}");
      Text = $"{AssemblyTitle} : {FileSystemUtils.GetShortDisplayFileName(m_FileName, 80)}";

      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      if (m_FileSetting != null)
        m_FileSetting.PropertyChanged -= FileSetting_PropertyChanged;

      m_FileSetting = new CsvFile();
      m_ViewSettings.CopyTo(m_FileSetting);
      m_FileSetting.FileName = m_FileName;

      if (m_FileName.AssumePgp() && (ApplicationSetting.PGPKeyStorage?.PrivateKeys?.IsEmpty() ?? false))
      {
        var res = _MessageBox.Show(this, "The private key for decryption has not been setup.\n\nDo you want to add them now ?", "Decryption", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, timeout: 5);
        if (res == DialogResult.Cancel)
          return false;

        if (res == DialogResult.Yes)
        {
          using (var frm = new FormEditSettings(m_ViewSettings))
          {
            frm.tabControl.SelectedTab = frm.tabPagePGP;
            frm.ShowDialog(this);
          }
          SaveDefault();
        }
      }

      m_FileSetting.GetEncryptedPassphraseFunction = m_FileSetting.GetEncryptedPassphraseOpenForm;

      try
      {
        var analyse = true;
        var fileInfo = FileSystemUtils.FileInfo(m_FileName);
        m_FileSetting.ID = m_FileName.GetIdFromFileName();
        Log.Info($"Size of file: {StringConversion.DynamicStorageSize(fileInfo.Length)}");

        using (var cancellationTokenSource =
          CancellationTokenSource.CreateLinkedTokenSource(m_CancellationTokenSource.Token))
        {
          try
          {
            m_CurrentCancellationTokenSource = cancellationTokenSource;
            FrmLimitSize limitSizeForm = null;
            if (fileInfo.Length > maxsize)
            {
              limitSizeForm = new FrmLimitSize();

              // As the form closes it will store the information
              limitSizeForm.FormClosing += (sender, args) =>
              {
                if (limitSizeForm.DialogResult == DialogResult.Cancel)
                  cancellationTokenSource.Cancel();
                m_FileSetting.RecordLimit = limitSizeForm.RecordLimit.ToUint();
                limitSizeForm = null;
              };
              limitSizeForm.Show();
            }

            try
            {
              if (FileSystemUtils.FileExists(m_FileName + CsvFile.cCsvSettingExtension))
              {
                m_FileSetting = SerializedFilesLib.LoadCsvFile(m_FileName + CsvFile.cCsvSettingExtension);
                m_FileSetting.FileName = m_FileName;
                Log.Info("Configuration read from setting file");
                DisableIgnoreRead();
                analyse = false;
                // Add all columns as string

                m_ConfigChanged = false;
              }
            }
            catch (Exception exc)
            {
              MessageBox.Show(this, exc.ExceptionMessages(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (analyse && m_ViewSettings.GuessCodePage)
            {
              CsvHelper.GuessCodePage(m_FileSetting);
            }

            if (analyse) m_FileSetting.NoDelimitedFile = CsvHelper.GuessNotADelimitedFile(m_FileSetting);
            if (analyse && m_ViewSettings.GuessDelimiter) m_FileSetting.FileFormat.FieldDelimiter = CsvHelper.GuessDelimiter(m_FileSetting);
            if (analyse && m_ViewSettings.GuessStartRow) m_FileSetting.SkipRows = CsvHelper.GuessStartRow(m_FileSetting);

            if (analyse)
            {
              if (m_ViewSettings.GuessHasHeader)
              {
                using (var processDisplay = new DummyProcessDisplay(cancellationTokenSource.Token))
                {
                  m_FileSetting.HasFieldHeader = CsvHelper.GuessHasHeader(m_FileSetting, processDisplay);
                }
              }

              using (var processDisplay = m_FileSetting.GetProcessDisplay(this, false, cancellationTokenSource.Token))
              {
                if (processDisplay is Form frm && limitSizeForm != null)
                {
                  frm.Left = limitSizeForm.Left + limitSizeForm.Width;
                }
                m_FileSetting.FillGuessColumnFormatReader(false, processDisplay);
              }

              if (m_FileSetting.ColumnCollection.Any(x => x.DataType != DataType.String))
              {
                detailControl.ButtonShowSource += DetailControl_ButtonShowSource;
                detailControl.ButtonAsText += DetailControl_ButtonAsText;
              }
            }

            Extensions.TimeOutWait(() => { return limitSizeForm != null; }, 100, 5 / 60, false, cancellationTokenSource.Token);

            if (limitSizeForm != null)
              limitSizeForm.Close();

            if (cancellationTokenSource.IsCancellationRequested)
              return false;
          }
          finally
          {
            Cursor.Current = oldCursor;
            m_FileSetting.FileFormat.PropertyChanged += FileSetting_PropertyChanged;
            foreach (var col in m_FileSetting.ColumnCollection)
              col.PropertyChanged += FileSetting_PropertyChanged;
            m_CurrentCancellationTokenSource = null;
          }
        }

        if (m_ViewSettings.DetectFileChanges)
        {
          fileSystemWatcher.Filter = fileInfo.Name;
          fileSystemWatcher.Path = FileSystemUtils.GetDirectoryName(fileInfo.FullName);
        }
      }
      catch (Exception ex)
      {
        this.ShowError(ex, "Opening File");
        return false;
      }
      finally
      {
        m_FileSetting.PropertyChanged += FileSetting_PropertyChanged;
        Cursor.Current = Cursors.Default;
        ShowTextPanel(false);
      }

      return true;
    }

    private static ViewSettings LoadDefault()
    {
      try
      {
        Log.Debug($"Loading defaults {cSettingPath}");
        if (FileSystemUtils.FileExists(cSettingPath))
        {
          var serial = File.ReadAllText(cSettingPath);
          using (TextReader reader = new StringReader(serial))
          {
#pragma warning disable CA3075 // Insecure DTD processing in XML
            return (ViewSettings)m_SerializerViewSettings.Deserialize(reader);
#pragma warning restore CA3075 // Insecure DTD processing in XML
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
      return new ViewSettings();
    }

    /// <summary>
    ///   Opens the data reader.
    /// </summary>
    private void OpenDataReader(bool clear)
    {
      if (m_FileSetting == null) return;

      m_SettingsChangedTimerChange.Stop();
      fileSystemWatcher.EnableRaisingEvents = false;

      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;

      // Stop Property changed events for the time this is processed
      // We might store data in the FileSetting

      m_FileSetting.PropertyChanged -= FileSetting_PropertyChanged;

      try
      {
        if (clear)
          ClearProcess();
        Log.Info("Opening File…");

        Text =
          $"{AssemblyTitle} : {FileSystemUtils.GetShortDisplayFileName(m_FileSetting.FileName, 80)}  - {EncodingHelper.GetEncodingName(m_FileSetting.CurrentEncoding.CodePage, true, m_FileSetting.ByteOrderMark)}";

        var warnings = new RowErrorCollection();
        DataTable data;
        using (var processDisplay = m_FileSetting.GetProcessDisplay(this, false, m_CancellationTokenSource.Token))
        {
          using (var csvDataReader = m_FileSetting.GetFileReader(processDisplay))
          {
            csvDataReader.Warning += warnings.Add;
            csvDataReader.Warning += AddWarning;
            csvDataReader.Open();
            if (warnings.CountRows > 0)
              _MessageBox.Show(this, warnings.Display, "Opening CSV File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if (!textPanel.Visible)
              ShowTextPanel(true);

            data = csvDataReader.WriteToDataTable(m_FileSetting, m_FileSetting.RecordLimit, warnings,
                processDisplay.CancellationToken);

            foreach (var columnName in data.GetRealColumns())
              if (m_FileSetting.ColumnCollection.Get(columnName) == null)
                m_FileSetting.ColumnCollection.AddIfNew(new Column { Name = columnName });
            if (processDisplay.CancellationToken.IsCancellationRequested)
            {
              Log.Info("Cancellation was requested.");
              if (_MessageBox.Show(this,
                    "The load was not completed, cancellation was requested.\rDo you want to display the already loaded data?",
                    "Cancellation Requested", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                Close();
            }
          }
        }
        detailControl.CancellationToken = m_CancellationTokenSource.Token;

        if (data != null)
        {
          Log.Info("Showing loaded data…");
          // Show the data
          detailControl.DataTable = data;
        }

        detailControl.FileSetting = m_FileSetting;

        // if (m_FileSetting.NoDelimitedFile)
        //  detailControl_ButtonShowSource(this, null);
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
          Log.Info("No data…");
        else
          // if (!m_FileSetting.NoDelimitedFile)
          ShowTextPanel(false);
        Cursor.Current = oldCursor;
        foreach (var col in m_FileSetting.ColumnCollection)
        {
          col.PropertyChanged += FileSetting_PropertyChanged;
        }
        m_ConfigChanged = false;
        fileSystemWatcher.EnableRaisingEvents = m_ViewSettings.DetectFileChanges;
        m_FileChanged = false;
        // Re enable event watching
        m_FileSetting.PropertyChanged += FileSetting_PropertyChanged;
      }
    }

    private void SaveDefault()
    {
      try
      {
        if (!FileSystemUtils.DirectoryExists(cSettingFolder))
          FileSystemUtils.CreateDirectory(cSettingFolder);

        FileSystemUtils.DeleteWithBackup(cSettingPath, false);
        using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
        {
          m_SerializerViewSettings.Serialize(stringWriter, m_ViewSettings, SerializedFilesLib.EmptyXmlSerializerNamespaces.Value);
          File.WriteAllText(cSettingPath, stringWriter.ToString());
        }
      }
      catch (Exception)
      {
      }
    }

    private void SaveSetting()
    {
      try
      {
        var pathSetting = m_FileSetting.FileName + CsvFile.cCsvSettingExtension;

        m_FileSetting.FileName = FileSystemUtils.SplitPath(m_FileSetting.FileName).FileName;
        var answer = DialogResult.No;

        if (FileSystemUtils.FileExists(pathSetting))
        {
          // No need to save if nothing has changed
          var compare = SerializedFilesLib.LoadCsvFile(pathSetting);
          // These entries can be ignored
          compare.ID = m_FileSetting.ID;
          compare.FileName = m_FileSetting.FileName;

          if (!compare.Equals(m_FileSetting))
            answer = MessageBox.Show(this,
              $"Replace changed settings in {pathSetting} ?", "Settings",
              MessageBoxButtons.YesNo, MessageBoxIcon.Question);

          if (answer == DialogResult.Yes)
            FileSystemUtils.FileDelete(pathSetting);
        }
        else
        {
          answer = _MessageBox.Show(this,
            $"Store settings in {pathSetting} for faster processing next time?", "Settings", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        if (answer != DialogResult.Yes) return;
        Log.Debug($"Saving setting {pathSetting}");
        SerializedFilesLib.SaveCsvFile(pathSetting, m_FileSetting);
        m_ConfigChanged = false;
      }
      catch (Exception ex)
      {
        this.ShowError(ex, "Storing Settings");
      }
    }

    private void ShowGrid(object sender, EventArgs e)
    {
      m_CurrentCancellationTokenSource?.Cancel();
      ShowTextPanel(false);
    }

    private void ShowSettings(object sender, EventArgs e)
    {
      using (var frm = new FormEditSettings(m_ViewSettings))
      {
        frm.ShowDialog(MdiParent);
        FillFromProperites(false);
        if (m_ConfigChanged)
        {
          if (_MessageBox.Show(this, "The configuration has changed do you want to reload the data?", "Configuration changed", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes) OpenDataReader(true);
        }
      }
      detailControl.MoveMenu();
      SaveDefault();
    }

    private void ShowTextPanel(bool visible)
    {
      textPanel.Visible = visible;
      detailControl.Visible = !visible;
    }

    private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
    {
      this.LoadWindowState(m_ViewSettings.WindowPosition);
    }

    private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
      switch (e.Mode)
      {
        case PowerModes.Suspend:
          Log.Debug($"Power Event Suspend");
          var res = this.StoreWindowState();
          if (res == null) return;
          m_ViewSettings.WindowPosition = res;
          break;

        case PowerModes.Resume:
          Log.Debug($"Power Event Resume");
          this.LoadWindowState(m_ViewSettings.WindowPosition);
          break;
      }
    }
  }
}