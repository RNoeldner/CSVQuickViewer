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
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace CsvTools
{
  /// <summary>
  ///   Form to Display a CSV File
  /// </summary>
  public sealed partial class FormMain : Form
  {
    private readonly CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();
    private readonly StringBuilder m_Messages = new StringBuilder();
    private readonly Timer m_SettingsChangedTimerChange = new Timer(100);
    private readonly Collection<Column> m_StoreColumns = new Collection<Column>();
    private bool m_ConfigChanged;
    private CancellationTokenSource m_CurrentCancellationTokenSource;
    private bool m_FileChanged;
    private string m_FileName;
    private CsvFile m_FileSetting;
    private string m_LastMessage = string.Empty;
    private int m_WarningCount;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormMain" /> class.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    public FormMain(string fileName)
    {
      m_FileName = fileName;
      FillFromProperites(true);

      InitializeComponent();
      m_SettingsChangedTimerChange.AutoReset = false;
      m_SettingsChangedTimerChange.Elapsed += delegate { this.SafeInvoke(() => OpenDataReader(true)); };
      m_SettingsChangedTimerChange.Start();

      // Done in code to be able to select controls in the designer
      textPanel.SuspendLayout();
      textPanel.Dock = DockStyle.Fill;
      textBoxProgress.Dock = DockStyle.Fill;
      csvTextDisplay.Dock = DockStyle.Fill;
      textPanel.ResumeLayout();
      ShowTextPanel(true);

#if ExtendedVersion
      detailControl.ExtendedVersion = true;
#endif

      Text = AssemblyTitle;

      SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
      SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
    }

    public TimeZoneInfo DestionationTimeZone => TimeZoneInfo.Local;

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

    private void DetailControl_ButtonAsText(object sender, EventArgs e)
    {
      // Assume data type is not recognize
      if (m_FileSetting.Column.Any(x => x.DataType != DataType.String))
      {
        m_FileSetting.Column.CollectionCopy(m_StoreColumns);
        m_FileSetting.Column.Clear();
        detailControl.ButtonAsTextCaption = "Values";
      }
      else
      {
        detailControl.ButtonAsTextCaption = "Text";
        m_FileSetting.Column.Clear();
        m_FileSetting.Column.CollectionCopy(m_StoreColumns);
      }
    }

    private void AddWarning(object sender, WarningEventArgs args)
    {
      if (string.IsNullOrEmpty(args.Message)) return;
      if (++m_WarningCount == m_FileSetting.NumWarnings)
        SetProcess("No further warnings displayed…");
      else if (m_WarningCount < m_FileSetting.NumWarnings)
        SetProcess(args.Display(true, true));
    }

    private void ClearProcess()
    {
      m_WarningCount = 0;
      textBoxProgress.SafeInvoke(() =>
      {
        if (IsDisposed) return;
        if (!textPanel.Visible)
          ShowTextPanel(true);

        textBoxProgress.Text = string.Empty;
        m_Messages.Length = 0;
        textBoxProgress.Refresh();
      });
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
      foreach (var col in m_FileSetting.Column)
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
      if (MessageBox.Show(this, "The displayed file has changed do you want to reload the data?", "File changed",
            MessageBoxButtons.YesNo) == DialogResult.Yes) OpenDataReader(true);
    }

    private void Display_FormClosing(object sender, FormClosingEventArgs e)
    {
      m_CancellationTokenSource.Cancel();

      var res = this.StoreWindowState();
      if (res != null)
      {
        Settings.Default.WindowPosition = res.Item1;
        Settings.Default.WindowState = res.Item2;
      }

      if (m_ConfigChanged && Settings.Default.UseFileSettings)
        SaveSetting();
    }

    /// <summary>
    ///   Handles the Shown event of the Display control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void Display_Shown(object sender, EventArgs e)
    {
      this.LoadWindowState(Settings.Default.WindowPosition, Settings.Default.WindowState);

      if (string.IsNullOrEmpty(m_FileName) || !FileSystemUtils.FileExists(m_FileName))
        using (var openFileDialog = new OpenFileDialog())
        {
          if (Settings.Default.UseFileSettings)
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
      fileSystemWatcher.EnableRaisingEvents = Settings.Default.DetectFileChanges;
      OpenDataReader(false);
    }

    private void FileSetting_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      m_ConfigChanged = true;
      if (e.PropertyName != "ColumnFormat") return;
      // reload the data
      m_SettingsChangedTimerChange.Stop();
      m_SettingsChangedTimerChange.Start();
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
      ApplicationSetting.FillGuessSettings.SampleValues = Settings.Default.SampleValues;
      ApplicationSetting.FillGuessSettings.CheckedRecords = Settings.Default.CheckedRecords;
      ApplicationSetting.FillGuessSettings.DectectNumbers = Settings.Default.DectectNumbers;
      ApplicationSetting.FillGuessSettings.DectectPercentage = Settings.Default.DectectPercentage;
      ApplicationSetting.FillGuessSettings.DetectBoolean = Settings.Default.DetectBoolean;
      ApplicationSetting.FillGuessSettings.DetectDateTime = Settings.Default.DetectDateTime;
      ApplicationSetting.FillGuessSettings.DetectGUID = Settings.Default.DetectGUID;
      ApplicationSetting.FillGuessSettings.SerialDateTime = Settings.Default.ExcelSerialDateTime;
      ApplicationSetting.FillGuessSettings.TrueValue = Settings.Default.TrueValue;
      ApplicationSetting.FillGuessSettings.FalseValue = Settings.Default.FalseValue;
      ApplicationSetting.FillGuessSettings.IgnoreIdColums = Settings.Default.IgnoreIdColums;
      ApplicationSetting.FillGuessSettings.DateTimeValue = Settings.Default.DateTimeValue;
      ApplicationSetting.FillGuessSettings.MinSamplesForIntDate = Settings.Default.MinSamplesForIntDate;
      ApplicationSetting.FillGuessSettings.CheckNamedDates = Settings.Default.CheckNamedDates;
      ApplicationSetting.FillGuessSettings.DateParts = Settings.Default.DateParts;
      ApplicationSetting.MenuDown = Settings.Default.MenuDown;

      ApplicationSetting.ToolSetting.PGPInformation.EncryptedPassphase = Settings.Default.DefaultPassphrase;
      if (Settings.Default.PrivateKey != null)
        ApplicationSetting.ToolSetting.PGPInformation.PrivateKeys =
          Settings.Default.PrivateKey.OfType<string>().ToArray();

      if (m_FileSetting == null || !changeSetting)
        return;

      SetFileSettingDefault(m_FileSetting);
    }

    private static void SetFileSettingDefault(CsvFile fileSetting)
    {
      if (fileSetting == null)
        return;
      fileSetting.TreatTextAsNull = Settings.Default.TreatTextAsNull;
      fileSetting.FileFormat.CommentLine = Settings.Default.Comment;
      fileSetting.FileFormat.DelimiterPlaceholder = Settings.Default.DelimiterPlaceholder;
      fileSetting.FileFormat.NewLinePlaceholder = Settings.Default.NLPlaceholder;
      fileSetting.NumWarnings = Settings.Default.NumWarnings;
      fileSetting.TreatUnknowCharaterAsSpace = Settings.Default.TreatUnknowCharaterAsSpace;
      fileSetting.TreatNBSPAsSpace = Settings.Default.TreatNBSPAsSpace;
      fileSetting.ShowProgress = Settings.Default.ShowProgress;
      fileSetting.WarnLineFeed = Settings.Default.WarnLineFeed;
      fileSetting.WarnDelimiterInValue = Settings.Default.WarnDelimiterInValue;
      fileSetting.WarnQuotes = Settings.Default.WarnQuotes;
      fileSetting.WarnEmptyTailingColumns = Settings.Default.WarnEmptyTailingColumns;
      fileSetting.SkipEmptyLines = Settings.Default.SkipEmptyLines;
      fileSetting.AlternateQuoting = Settings.Default.AlternateQuoting;
      fileSetting.DisplayStartLineNo = Settings.Default.DisplayStartLineNo;
      fileSetting.FileFormat.EscapeCharacter = Settings.Default.EscapeCharacter;
      fileSetting.FileFormat.QuotePlaceholder = Settings.Default.QuotePlaceholder;
      fileSetting.TreatTextAsNull = Settings.Default.TreatTextAsNull;
      fileSetting.WarnNBSP = Settings.Default.WarnNBSP;
      fileSetting.WarnUnknowCharater = Settings.Default.WarnUnknowCharater;

      if (Settings.Default.TrimmingOptions.Equals("None", StringComparison.OrdinalIgnoreCase))
        fileSetting.TrimmingOption = TrimmingOption.None;
      if (Settings.Default.TrimmingOptions.Equals("All", StringComparison.OrdinalIgnoreCase))
        fileSetting.TrimmingOption = TrimmingOption.All;
      if (Settings.Default.TrimmingOptions.Equals("Unquoted", StringComparison.OrdinalIgnoreCase))
        fileSetting.TrimmingOption = TrimmingOption.Unquoted;
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

      ShowTextPanel(true);
      ClearProcess();
      SetProcess($"Examining file {m_FileName}");
      Text = $"{AssemblyTitle} : {FileSystemUtils.GetShortDisplayFileName(m_FileName, 80)}";

      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        var analyse = true;

        m_FileSetting = new CsvFile(m_FileName);
        m_FileSetting.GetEncryptedPassphraseFunction = m_FileSetting.GetEncryptedPassphrase;
        SetFileSettingDefault(m_FileSetting);
        var fileInfo = FileSystemUtils.FileInfo(m_FileName);

        m_FileSetting.ID = m_FileName.GetIdFromFileName();
        SetProcess($"Size of file: {StringConversion.DynamicStorageSize(fileInfo.Length)}");

        using (var cancellationTokenSource =
          CancellationTokenSource.CreateLinkedTokenSource(m_CancellationTokenSource.Token))
        {
          try
          {
            m_CurrentCancellationTokenSource = cancellationTokenSource;
            var fromRecordLimitSet = true;
            FrmLimitSize limitSizeForm;
            // run in background...
            if (fileInfo.Length > maxsize)
            {
              fromRecordLimitSet = false;
              limitSizeForm = new FrmLimitSize();

              // As the form closes it will store the information
              limitSizeForm.FormClosing += (sender, args) =>
              {
                if (limitSizeForm.DialogResult == DialogResult.Cancel)
                  cancellationTokenSource.Cancel();
                fromRecordLimitSet = true;
                m_FileSetting.RecordLimit = limitSizeForm.RecordLimit.ToUint();
              };
              limitSizeForm.Show();
            }

            try
            {
              if (FileSystemUtils.FileExists(m_FileName + CsvFile.cCsvSettingExtension))
              {
                m_FileSetting = SerializedFilesLib.LoadCsvFile(m_FileName + CsvFile.cCsvSettingExtension);
                m_FileSetting.FileName = m_FileName;
                SetProcess("Configuration read from setting file");
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

            if (analyse && Settings.Default.GuessCodePage)
            {
              CsvHelper.GuessCodePage(m_FileSetting);
              SetProcess("Detected Code Page: " + EncodingHelper.GetEncodingName(m_FileSetting.CurrentEncoding.CodePage,
                           true, m_FileSetting.ByteOrderMark));
            }

            if (analyse) m_FileSetting.NoDelimitedFile = CsvHelper.GuessNotADelimitedFile(m_FileSetting);
            if (analyse && Settings.Default.GuessDelimiter)
            {
              m_FileSetting.FileFormat.FieldDelimiter = CsvHelper.GuessDelimiter(m_FileSetting);
              SetProcess("Delimiter: " + m_FileSetting.FileFormat.FieldDelimiter);
            }

            if (analyse && Settings.Default.GuessStartRow)
            {
              m_FileSetting.SkipRows = CsvHelper.GuessStartRow(m_FileSetting);
              SetProcess("Start Row: " + m_FileSetting.SkipRows.ToString(CultureInfo.InvariantCulture));
            }

            if (analyse && Settings.Default.GuessHasHeader)
              m_FileSetting.HasFieldHeader = CsvHelper.GuessHasHeader(m_FileSetting, cancellationTokenSource.Token);

            if (analyse)
            {
              using (var processDisplay = m_FileSetting.GetProcessDisplay(this, cancellationTokenSource.Token))
              {
                processDisplay.Progress += SetProcess;
                m_FileSetting.FillGuessColumnFormatReader(false, processDisplay);
              }

              if (m_FileSetting.Column.Any(x => x.DataType != DataType.String))
              {
                detailControl.ButtonShowSource += DetailControl_ButtonShowSource;
                detailControl.ButtonAsText += DetailControl_ButtonAsText;
              }
            }

            // Wait for the RecordLimit to be set or a cancellation caused by the outside
            while (!fromRecordLimitSet && !cancellationTokenSource.IsCancellationRequested)
            {
              Thread.Sleep(50);
              Application.DoEvents();
            }

            if (cancellationTokenSource.IsCancellationRequested)
              return false;
          }
          finally
          {
            Cursor.Current = oldCursor;
            m_CurrentCancellationTokenSource = null;
          }
        }

        if (Settings.Default.DetectFileChanges)
        {
          fileSystemWatcher.Filter = fileInfo.Name;
          fileSystemWatcher.Path = fileInfo.DirectoryName;
        }

        // Add a ColumnProperty for each column
        m_FileSetting.PropertyChanged += FileSetting_PropertyChanged;
      }
      catch (Exception ex)
      {
        MessageBox.Show(this, ex.ExceptionMessages(), "Opening File", MessageBoxButtons.OK, MessageBoxIcon.Stop);
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
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;

      // Stop Property changed events for the time this is processed
      // We might store data in the FileSetting
      m_FileSetting.PropertyChanged -= FileSetting_PropertyChanged;

      m_SettingsChangedTimerChange.Stop();
      try
      {
        if (clear)
          ClearProcess();
        SetProcess("Opening File…");

        m_FileChanged = false;

        Text =
          $"{AssemblyTitle} : {FileSystemUtils.GetShortDisplayFileName(m_FileSetting.FileName, 80)}  - {EncodingHelper.GetEncodingName(m_FileSetting.CurrentEncoding.CodePage, true, m_FileSetting.ByteOrderMark)}";

        var warnings = new RowErrorCollection();
        DataTable data;
        using (var processDisplay = m_FileSetting.GetProcessDisplay(this, m_CancellationTokenSource.Token))
        {
          using (var csvDataReader = m_FileSetting.GetFileReader())
          {
            csvDataReader.ProcessDisplay = processDisplay;
            csvDataReader.Warning += warnings.Add;
            csvDataReader.Warning += AddWarning;
            csvDataReader.Open(processDisplay.CancellationToken, false, null);
            if (warnings.CountRows > 0)
              MessageBox.Show(this, warnings.Display, "Opening CSV File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if (!textPanel.Visible)
              ShowTextPanel(true);

            SetProcess("Reading data…");
            data = csvDataReader.WriteToDataTable(m_FileSetting, m_FileSetting.RecordLimit, warnings,
              processDisplay.CancellationToken);

            foreach (var columnName in data.GetRealColumns())
              if (m_FileSetting.GetColumn(columnName) == null)
                m_FileSetting.ColumnAdd(new Column { Name = columnName });
            if (processDisplay.CancellationToken.IsCancellationRequested)
            {
              SetProcess("Cancellation was requested.");
              if (MessageBox.Show(this,
                    "The load was not completed since cancellation was requested.\rDo you want to display the already loaded data?",
                    "Cancellation Requested", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                Close();
            }
          }
        }

        if (data != null)
        {
          SetProcess("Showing loaded data…");
          // Show the data
          detailControl.DataTable = data;
        }

        detailControl.FileSetting = m_FileSetting;

        // if (m_FileSetting.NoDelimitedFile)
        //  detailControl_ButtonShowSource(this, null);
      }
      catch (Exception exc)
      {
        MessageBox.Show(this, exc.ExceptionMessages(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
      finally
      {
        if (detailControl.DataTable == null)
          SetProcess("No data…");
        else
          // if (!m_FileSetting.NoDelimitedFile)
          ShowTextPanel(false);
        Cursor.Current = oldCursor;

        // Re enable event watching
        m_FileSetting.PropertyChanged += FileSetting_PropertyChanged;
      }
    }

    /// <summary>
    ///   Sets the process.
    /// </summary>
    /// <param name="text">The text.</param>
    private void SetProcess(string text)
    {
      if (string.IsNullOrEmpty(text))
        return;
      var appended = false;
      var posSlash = text.IndexOf('–', 0);
      if (posSlash != -1 && m_LastMessage.StartsWith(text.Substring(0, posSlash + 1), StringComparison.Ordinal))
      {
        textBoxProgress.AppendText(text.Substring(posSlash - 1));
        appended = true;
      }

      m_LastMessage = text;
      if (!appended)
      {
        if (textBoxProgress.Text.Length > 0)
          textBoxProgress.AppendText(Environment.NewLine);
        textBoxProgress.AppendText(text);
      }

      textBoxProgress.SelectionStart = textBoxProgress.Text.Length;
      textBoxProgress.ScrollToCaret();
      // Application.DoEvents();
    }

    /// <summary>
    ///   Set the progress used by Events
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SetProcess(object sender, ProgressEventArgs e)
    {
      SetProcess(e.Text);
    }

    private void ShowGrid(object sender, EventArgs e)
    {
      m_CurrentCancellationTokenSource?.Cancel();
      ShowTextPanel(false);
    }

    private void ShowSettings(object sender, EventArgs e)
    {
      if (m_FileSetting == null) m_FileSetting = new CsvFile();
      using (var frm = new FormEditSettings(m_FileSetting))
      {
        var res = frm.ShowDialog(MdiParent);
        if (res == DialogResult.Cancel) return;
        FillFromProperites(false);
        detailControl.MoveMenu();
      }
    }

    private void ShowTextPanel(bool visible)
    {
      textPanel.Visible = visible;
      detailControl.Visible = !visible;
    }

    private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
    {
      this.LoadWindowState(Settings.Default.WindowPosition, Settings.Default.WindowState);
    }

    private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
      switch (e.Mode)
      {
        case PowerModes.Suspend:
          var res = this.StoreWindowState();
          if (res == null) return;
          Settings.Default.WindowPosition = res.Item1;
          Settings.Default.WindowState = res.Item2;
          break;

        case PowerModes.Resume:
          this.LoadWindowState(Settings.Default.WindowPosition, Settings.Default.WindowState);
          break;
      }
    }

    private void SaveSetting()
    {
      try
      {
        var pathSetting = m_FileSetting.FileName + CsvFile.cCsvSettingExtension;
        m_FileSetting.FileName = m_FileSetting.FileName.Substring(m_FileSetting.FileName.LastIndexOf('\\'));
        var answer = DialogResult.No;

        if (FileSystemUtils.FileExists(pathSetting))
        {
          // No need to save if nothing has changed
          var compare = SerializedFilesLib.LoadCsvFile(pathSetting);
          // These entries can be ignored...
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
          answer = MessageBox.Show(this,
            $"Store settings in {pathSetting} for faster processing next time?", "Settings", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        if (answer != DialogResult.Yes) return;
        SerializedFilesLib.SaveCsvFile(pathSetting, m_FileSetting);
        m_ConfigChanged = false;
      }
      catch (Exception ex)
      {
        MessageBox.Show(this, ex.ExceptionMessages(), "Storing Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }
  }
}