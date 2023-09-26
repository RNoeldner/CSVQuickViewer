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

#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace CsvTools
{
  /// <summary>
  ///   Form to Display a CSV File
  /// </summary>
  public sealed partial class FormMain : ResizeForm
  {
    private readonly CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();
    private readonly Timer m_SettingsChangedTimerChange = new Timer(200);
    private readonly ViewSettings m_ViewSettings;
    private bool m_ShouldReloadData;
    private bool m_FileChanged;
    private bool m_RunDetection;
    private bool m_AskOpenFile = true;
    private IFileSettingPhysicalFile? m_FileSetting;
    private IList<Column>? m_StoreColumns;
    private int m_WarningCount;
    private int m_WarningMax = 100;

    private void ApplyViewSettings()
    {
      detailControl.MenuDown = m_ViewSettings.MenuDown;
      detailControl.ShowButtonAtLength = m_ViewSettings.ShowButtonAtLength;
      detailControl.HtmlStyle = m_ViewSettings.HtmlStyle;
    }

    public FormMain() : this(new ViewSettings())
    {
    }



    public FormMain(in ViewSettings viewSettings)
    {
      m_ViewSettings = viewSettings;
      FontConfig = viewSettings;
      InitializeComponent();
      Text = AssemblyTitle;

      FunctionalDI.GetKeyAndPassphraseForFile = fileName =>
        fileName.GetKeyAndPassphraseForFile(
          m_FileSetting?.Passphrase ?? string.Empty,
          m_FileSetting?.KeyFile ?? m_ViewSettings.KeyFileRead,
          () =>
          {
            using var frm = new FormPasswordAndKey();
            frm.Passphrase = m_FileSetting?.Passphrase ?? string.Empty;
            frm.FileName = m_FileSetting?.KeyFile ?? m_ViewSettings.KeyFileRead;
            if (frm.ShowWithFont(this, true) == DialogResult.OK)
            {
              var key = PgpHelper.GetKeyAndValidate(fileName, frm.FileName);
              if (key.Length > 0)
                return (frm.Passphrase, frm.FileName, key);
            }

            return (string.Empty, string.Empty, string.Empty);
          });

      FunctionalDI.GetKeyForFile = fileName =>
        fileName.GetKeyForFile(m_FileSetting?.KeyFile ?? m_ViewSettings.KeyFileRead,
          () =>
          {
            using var frm = new FormPasswordAndKey();
            frm.ShowPassphrase = false;
            if (frm.ShowWithFont(this, true) == DialogResult.OK)
            {
              var key = PgpHelper.GetKeyAndValidate(fileName, frm.FileName);
              if (key.Length > 0)
              {
                PgpHelper.StoreKeyFile(fileName, frm.FileName);
                return key;
              }
            }
            return string.Empty;
          });

      FunctionalDI.GetPassphraseForFile = fileName =>
        fileName.GetPassphraseForFile(m_FileSetting?.Passphrase ?? string.Empty, () =>
        {
          using var frm = new FormPasswordAndKey();
          frm.ShowFileName = false;
          if (frm.ShowWithFont(this, true) == DialogResult.OK && frm.Passphrase.Length > 0)
            return frm.Passphrase;
          return string.Empty;
        });

      FunctionalDI.FileReaderWriterFactory = new ClassLibraryCsvFileReaderWriterFactory(StandardTimeZoneAdjust.ChangeTimeZone, viewSettings.FillGuessSettings);

      // add the not button not visible in designer to the detail control
      detailControl.AddToolStripItem(0, m_ToolStripButtonSettings);
      detailControl.AddToolStripItem(0, m_ToolStripButtonLoadFile);

      detailControl.AddToolStripItem(int.MaxValue, m_ToolStripButtonAsText);
      detailControl.AddToolStripItem(int.MaxValue, m_ToolStripButtonShowLog);
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormMain" /> class.
    /// </summary>
#if !NETFRAMEWORK
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
    internal CancellationToken CancellationToken => m_CancellationTokenSource.Token;

    public DataTable DataTable => detailControl.DataTable;

    private static string AssemblyTitle
    {
      get
      {
        var assembly = Assembly.GetExecutingAssembly();
        var attributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
        if (attributes.Length <= 0)
          return Path.GetFileNameWithoutExtension(assembly.Location);

        var titleAttribute = (AssemblyTitleAttribute) attributes[0];
        if (titleAttribute.Title.Length != 0)
          return titleAttribute.Title + " " + assembly.GetName().Version
#if !NETFRAMEWORK
                   + "*"
#endif
        ;

        return Path.GetFileNameWithoutExtension(assembly.Location);
      }
    }

    private async Task RunDetection(CancellationToken cancellationToken)
    {
      if (m_FileSetting is null)
        return;
      m_RunDetection = false;
      ShowTextPanel(true);
      try
      {
        var (_, detected) = await m_FileSetting.FillGuessColumnFormatReaderAsync(true, true,
          m_ViewSettings.FillGuessSettings,
          cancellationToken);
        ChangeColumnsNoEvent(false, detected);

        m_ToolStripButtonAsText.Visible = m_FileSetting is ICsvFile &&
                                          m_FileSetting.ColumnCollection.Any(x =>
                                            x.ValueFormat.DataType != DataTypeEnum.String);
      }
      catch (Exception ex)
      {
        this.ShowError(ex, "Column Detection");
      }
    }

    /// <summary>
    ///   Initializes the file settings.
    /// </summary>
    /// <param name="fileName"></param>
    /// ///
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns></returns>
    public async Task LoadCsvOrZipFileAsync(string fileName, CancellationToken cancellationToken)
    {
      if (IsDisposed)
        return;

      if (string.IsNullOrEmpty(fileName))
        return;
      var split = FileSystemUtils.SplitPath(fileName);

      if (FileSystemUtils.DirectoryExists(split.DirectoryName))
        m_ViewSettings.InitialFolder = split.DirectoryName;

      var fi = new FileSystemUtils.FileInfo(fileName);
      if (!fi.Exists)
      {
        Logger.Warning("Filename {filename} not found or not accessible.", fileName);
        return;
      }

      if (fi.Length == 0)
      {
        Logger.Warning("File {filename} is empty.", fileName);
        return;
      }

      ShowTextPanel(true);
      Logger.Information("Loading {filename}", fileName);
      try
      {
        DetachPropertyChanged();
        // using (var formProgress = new FormProgress("Examining file", false, cancellationToken))
        //{
        // formProgress.Maximum = 0;
        // formProgress.ShowWithFont(this);

        // make sure old columns are removed
        m_ViewSettings.DefaultInspectionResult.Columns.Clear();

        var detection = await fileName.InspectFileAsync(m_ViewSettings.AllowJson,
          m_ViewSettings.GuessCodePage, m_ViewSettings.GuessEscapePrefix,
          m_ViewSettings.GuessDelimiter, m_ViewSettings.GuessQualifier, m_ViewSettings.GuessStartRow,
          m_ViewSettings.GuessHasHeader, m_ViewSettings.GuessNewLine, m_ViewSettings.GuessComment,
          m_ViewSettings.FillGuessSettings, list =>
          {
            if (list.Count==1)
              return list.First();                       
            using var frm = new FormSelectInDropdown(list, list.FirstOrDefault(x => x.AssumeDelimited()));
            if (frm.ShowWithFont(this, true) == DialogResult.Cancel)
              throw new OperationCanceledException();
            return frm.SelectedText;
          }, m_ViewSettings.DefaultInspectionResult,
          PgpHelper.GetKeyAndValidate(fileName, m_ViewSettings.KeyFileRead), cancellationToken);

        m_FileSetting = detection.PhysicalFile();

        // If keyFile was set in the process its not yet stored
        m_FileSetting.KeyFile = PgpHelper.LookupKeyFile(fileName);

        // formProgress.Close();
        //}

        if (m_FileSetting is null)
          return;

        m_ViewSettings.DeriveWriteSetting(m_FileSetting);

        m_FileSetting.RootFolder = fileName.GetDirectoryName();
        m_FileSetting.DisplayEndLineNo = false;
        m_ViewSettings.PassOnConfiguration(m_FileSetting);

        // update the UI
        var display = fileName;
        if (!string.IsNullOrEmpty(m_FileSetting.IdentifierInContainer))
          display += Path.DirectorySeparatorChar + m_FileSetting.IdentifierInContainer;

        var title = new StringBuilder(FileSystemUtils.GetShortDisplayFileName(display, 50));
        if (m_FileSetting is ICsvFile csv)
        {
          title.Append(" - ");
          title.Append(EncodingHelper.GetEncodingName(csv.CodePageId, csv.ByteOrderMark));
          m_WarningMax = csv.NumWarnings;
        }

        title.Append(" - ");
        title.Append(AssemblyTitle);
        this.SafeInvokeNoHandleNeeded(() => Text = title.ToString());

        m_ToolStripButtonAsText.Visible = m_FileSetting is ICsvFile &&
                                          m_FileSetting.ColumnCollection.Any(x =>
                                            x.ValueFormat.DataType != DataTypeEnum.String);
        SetFileSystemWatcher(fileName);
        cancellationToken.ThrowIfCancellationRequested();

        Directory.SetCurrentDirectory(m_FileSetting.RootFolder);
        ButtonAsText(false);

        await OpenDataReaderAsync(cancellationToken);
      }
      catch (Exception ex)
      {
        this.ShowError(ex, $"Load File {fileName}");
      }
    }

    // ReSharper disable StringLiteralTypo
    private async Task SelectFile()
    {
      var strFilter = "Common types|*.csv;*.txt;*.tab;*.json;*.ndjson;*.gz|"
                      + "Delimited files|*.csv;*.txt;*.tab;*.tsv;*.dat;*.log|";

      if (m_ViewSettings.StoreSettingsByFile)
        strFilter += "Setting files|*" + CsvFile.cCsvSettingExtension + "|";

      strFilter += "Json files|*.json;*.ndjson|"
                   + "Compressed files|*.gz;*.zip|"
                   + "All files|*.*";

      if (!FileSystemUtils.DirectoryExists(m_ViewSettings.InitialFolder))
        m_ViewSettings.InitialFolder = ".";
      var fileName = WindowsAPICodePackWrapper.Open(m_ViewSettings.InitialFolder, "File to Display", strFilter, null);
      if (!(fileName is null || fileName.Length == 0))
        await LoadCsvOrZipFileAsync(fileName, m_CancellationTokenSource.Token);
    }

    private void AddWarning(object? sender, WarningEventArgs args)
    {
      if (string.IsNullOrEmpty(args.Message))
        return;
      if (++m_WarningCount == m_WarningMax)
        Logger.Warning("No further warnings displayed");
      else if (m_WarningCount < m_WarningMax)
        Logger.Warning(args.Display(true, true));
    }

    /// <summary>
    ///   Attaches the property changed handlers for the file Settings
    /// </summary>
    private void AttachPropertyChanged()
    {
      if (m_FileSetting != null)
      {
        m_FileSetting.PropertyChanged += FileSetting_PropertyChanged;
        m_FileSetting.ColumnCollection.CollectionChanged += ColumnCollectionOnCollectionChanged;
      }

      try
      {
        if (!string.IsNullOrEmpty(fileSystemWatcher.Path))
          fileSystemWatcher.EnableRaisingEvents = m_ViewSettings.DetectFileChanges;
      }
      catch
      {
        Logger.Information("Adding file system watcher failed");
      }
    }

    private async Task CheckPossibleChange()
    {
      if (!m_ShouldReloadData && !m_FileChanged)
        return;
      try
      {
        if (m_ShouldReloadData)
        {
          m_ShouldReloadData = false;
          if (MessageBox.Show(
                "The configuration has changed do you want to reload the data?",
                "Configuration changed",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2) == DialogResult.Yes)
          {
            if (m_RunDetection)
              await RunDetection(m_CancellationTokenSource.Token);

            await OpenDataReaderAsync(m_CancellationTokenSource.Token);
            // as we have reloaded assume any file change is handled as well
            m_FileChanged = false;
          }
        }

        if (!m_FileChanged) return;
        if (m_FileSetting is null || m_FileSetting.FileName.Length == 0)
          return;

        m_FileChanged = false;

        if (!m_AskOpenFile || MessageBox.Show(
              "The displayed file has changed do you want to reload the data?",
              "File changed",
              MessageBoxButtons.YesNo,
              MessageBoxIcon.Question,
              MessageBoxDefaultButton.Button2) == DialogResult.Yes)
          await LoadCsvOrZipFileAsync(m_FileSetting.FileName, m_CancellationTokenSource.Token);
        else
          m_FileChanged = false;

        m_AskOpenFile = true;
        if (m_FileSetting != null && !m_ViewSettings.StoreSettingsByFile)
          FileSystemUtils.FileDelete(m_FileSetting.FileName + CsvFile.cCsvSettingExtension);
      }
      catch (Exception exception)
      {
        Logger.Error(exception, "Checking reload or file change");
      }
    }

    /// <summary>
    ///   Detaches the property changed handlers for the file Setting
    /// </summary>
    private void DetachPropertyChanged()
    {
      m_SettingsChangedTimerChange.Stop();

      if (m_FileSetting is null) return;
      m_FileSetting.PropertyChanged -= FileSetting_PropertyChanged;
      m_FileSetting.ColumnCollection.CollectionChanged -= ColumnCollectionOnCollectionChanged;

      try
      {
        fileSystemWatcher.EnableRaisingEvents = false;
      }
      catch
      {
        Logger.Warning("Disabling file system watcher failed");
      }
    }

    /// <summary>
    ///   Handles the DragDrop event of the dataGridView control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="DragEventArgs" /> instance containing the event data.</param>
    private async void FileDragDrop(object? sender, DragEventArgs e)
    {
      await this.RunWithHourglassAsync(async () =>
      {
        // Set the filename
        var files = (string[]) (e.Data?.GetData(DataFormats.FileDrop) ?? Array.Empty<string>());
        if (files.Length <= 0) return;
        if (WindowsAPICodePackWrapper.IsDialogOpen) return;
        await SaveIndividualFileSettingAsync();

        await LoadCsvOrZipFileAsync(files[0], m_CancellationTokenSource.Token);
      });
    }

    /// <summary>
    ///   Handles the DragEnter event of the dataGridView control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="DragEventArgs" /> instance containing the event data.</param>
    private void FileDragEnter(object? sender, DragEventArgs e)
    {
      if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop, false) &&
          !WindowsAPICodePackWrapper.IsDialogOpen)
        e.Effect = DragDropEffects.All;
    }

    /// <summary>
    ///   Handles the PropertyChanged event of the FileSetting control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///   The <see cref="PropertyChangedEventArgs" /> instance containing the event data.
    /// </param>
    private void FileSetting_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(ICsvFile.AllowRowCombining)
          || e.PropertyName == nameof(ICsvFile.ByteOrderMark)
          || e.PropertyName == nameof(ICsvFile.CodePageId)
          || e.PropertyName == nameof(ICsvFile.ConsecutiveEmptyRows)
          || e.PropertyName == nameof(ICsvFile.HasFieldHeader)
          || e.PropertyName == nameof(ICsvFile.NumWarnings)
          || e.PropertyName == nameof(ICsvFile.SkipEmptyLines)
          || e.PropertyName == nameof(ICsvFile.SkipRows)
          || e.PropertyName == nameof(ICsvFile.TreatLfAsSpace)
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
          || e.PropertyName == nameof(ICsvFile.FieldDelimiterChar)
          || e.PropertyName == nameof(ICsvFile.FieldQualifierChar)
          || e.PropertyName == nameof(ICsvFile.EscapePrefixChar)
          || e.PropertyName == nameof(ICsvFile.DelimiterPlaceholder)
          || e.PropertyName == nameof(ICsvFile.NewLinePlaceholder)
          || e.PropertyName == nameof(ICsvFile.NewLinePlaceholder)
          || e.PropertyName == nameof(ICsvFile.QualifierPlaceholder)
          || e.PropertyName == nameof(ICsvFile.CommentLine)
          || e.PropertyName == nameof(ICsvFile.ContextSensitiveQualifier)
          || e.PropertyName == nameof(ICsvFile.DuplicateQualifierToEscape)
          || e.PropertyName == nameof(ICsvFile.FileName))
        m_ShouldReloadData = true;

      // if the header information is changed, rerun detection,
      // we might not come to a different result but the column names are reset.
      if (e.PropertyName == nameof(ICsvFile.HasFieldHeader))
      {
        m_RunDetection = true;
      }
    }

    private void BeforeFileStored(object? sender, IFileSettingPhysicalFile e)
    {
      fileSystemWatcher.Changed -= FileSystemWatcher_Changed;
      fileSystemWatcher.EnableRaisingEvents = false;
    }

    private async void FileStored(object? sender, IFileSettingPhysicalFile e)
    {
      fileSystemWatcher.Changed += FileSystemWatcher_Changed;
      fileSystemWatcher.EnableRaisingEvents = m_ViewSettings.DetectFileChanges;
      m_ShouldReloadData = false;
      if (m_ViewSettings.StoreSettingsByFile)
        await SerializedFilesLib.SaveSettingFileAsync(e, () => true, m_CancellationTokenSource.Token);
    }

    /// <summary>
    ///   Handles the Changed event of the fileSystemWatcher control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///   The <see cref="FileSystemEventArgs" /> instance containing the event data.
    /// </param>
    private void FileSystemWatcher_Changed(object? sender, FileSystemEventArgs e) =>
      m_FileChanged |= e.FullPath == m_FileSetting!.FileName && e.ChangeType == WatcherChangeTypes.Changed;

    /// <summary>
    ///   Handles the Activated event of the Display control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private async void FormMain_Activated(object? sender, EventArgs e) =>
      await CheckPossibleChange();

    private void FormMain_Loaded(object? sender, EventArgs e)
    {
      // Handle Events
      detailControl.BeforeFileStored += BeforeFileStored;
      detailControl.FileStored += FileStored;

      m_ViewSettings.PropertyChanged += (o, args) =>
      {
        if (args.PropertyName == nameof(ViewSettings.MenuDown) ||
            args.PropertyName == nameof(ViewSettings.ShowButtonAtLength))
          ApplyViewSettings();
      };
      ApplyViewSettings();

      m_SettingsChangedTimerChange.AutoReset = false;
      m_SettingsChangedTimerChange.Elapsed +=
        async (o, args) => await OpenDataReaderAsync(m_CancellationTokenSource.Token);
      
      ShowTextPanel(false);
    }
    private async void FormMain_FormClosing(object? sender, FormClosingEventArgs e)
    {
      if (!m_CancellationTokenSource.IsCancellationRequested)
      {
        m_CancellationTokenSource.Cancel();
        // Give the possibly running threads some time to exit
        Thread.Sleep(100);
      }

      if (e.CloseReason != CloseReason.UserClosing) return;
      Logger.Debug("Closing Form");

      await m_ViewSettings.SaveViewSettingsAsync();
      await SaveIndividualFileSettingAsync();
    }

    private async void FormMain_KeyUpAsync(object? sender, KeyEventArgs e)
    {
      if (e.KeyCode != Keys.F5 && (!e.Control || e.KeyCode != Keys.R)) return;
      e.Handled = true;
      await OpenDataReaderAsync(m_CancellationTokenSource.Token);
    }

    /// <summary>
    ///   Opens the data reader.
    /// </summary>
    private async Task OpenDataReaderAsync(CancellationToken cancellationToken)
    {
      if (m_FileSetting is null)
        return;

      // Stop Property changed events for the time this is processed we might store data in the FileSetting
      DetachPropertyChanged();

      try
      {
        m_ToolStripButtonAsText.Enabled = true;

        await Extensions.InvokeWithHourglassAsync(async () =>
        {
          var fileNameShort = FileSystemUtils.GetShortDisplayFileName(m_FileSetting.FileName, 60);

          detailControl.SafeInvoke(() =>
          {
            ShowTextPanel(true);
            detailControl.FileSetting = m_FileSetting;
            detailControl.WriteSetting = m_ViewSettings.WriteSetting;
            detailControl.FillGuessSettings = m_ViewSettings.FillGuessSettings;
            detailControl.CancellationToken = cancellationToken;
            detailControl.ShowInfoButtons = false;
          });

          Logger.Debug("Loading Batch");
          using (var formProgress = new FormProgress(fileNameShort, false, FontConfig, cancellationToken))
          {
            formProgress.Show(this);
            await detailControl.LoadSettingAsync(m_FileSetting, false, true, m_ViewSettings.DurationTimeSpan,
              FilterTypeEnum.All, formProgress, AddWarning, formProgress.CancellationToken);

          }

          var keepVisible = new List<string>();
          if (m_FileSetting.DisplayEndLineNo)
            keepVisible.Add(ReaderConstants.cEndLineNumberFieldName);
          if (m_FileSetting.DisplayStartLineNo)
            keepVisible.Add(ReaderConstants.cStartLineNumberFieldName);
          if (m_FileSetting.DisplayRecordNo)
            keepVisible.Add(ReaderConstants.cRecordNumberFieldName);
          detailControl.UniqueFieldName = keepVisible;

          Logger.Debug("Batch Loaded");
          cancellationToken.ThrowIfCancellationRequested();

          // TODO: Is this needed ? Is the column collection not already set ?
          m_FileSetting.ColumnCollection.AddRange(detailControl.DataTable.GetRealColumns()
            .Select(dataColumn => new Column(dataColumn.ColumnName, new ValueFormat(dataColumn.DataType.GetDataType()),
              dataColumn.Ordinal)));


          // Load View Settings from file
          if (FileSystemUtils.FileExists(m_FileSetting.ColumnFile))
          {
            Logger.Information("Restoring view and filter setting {filename}...", m_FileSetting.ColumnFile);
            detailControl.ReStoreViewSetting(m_FileSetting.ColumnFile);
          }
          else
          {
            var index = m_FileSetting.FileName.LastIndexOf('.');
            var fn = (index == -1 ? m_FileSetting.FileName : m_FileSetting.FileName.Substring(0, index)) + ".col";
            var fnView = Path.Combine(m_FileSetting.FileName.GetDirectoryName(), fn);
            if (FileSystemUtils.FileExists(fnView))
            {
              Logger.Information("Restoring view and filter setting {filename}...", fn);
              detailControl.ReStoreViewSetting(fnView);
            }
          }

        });

        cancellationToken.ThrowIfCancellationRequested();
        this.SafeInvoke(() => ShowTextPanel(false));
        detailControl.ShowInfoButtons = true;
      }
      catch (Exception)
      {
        if (!cancellationToken.IsCancellationRequested)
          throw;
      }
      finally
      {
        m_ShouldReloadData = false;
        m_FileChanged = false;

        // Re enable event watching
        AttachPropertyChanged();
      }
    }

    private async void ColumnCollectionOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
      m_ShouldReloadData = true;
      await CheckPossibleChange();
    }

    private async Task SaveIndividualFileSettingAsync()
    {
      try
      {
        if (m_FileSetting != null && m_ViewSettings.StoreSettingsByFile)
        {
          var fileName = m_FileSetting.FileName + CsvFile.cCsvSettingExtension;
          await SerializedFilesLib.SaveSettingFileAsync(m_FileSetting,
            () => MessageBox.Show(
              $"Setting {FileSystemUtils.GetShortDisplayFileName(fileName, 50)} has been changed.\nReplace with new setting? ",
              "Settings",
              MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes, m_CancellationTokenSource.Token);
        }

        m_ShouldReloadData = false;
      }
      catch (Exception ex)
      {
        this.ShowError(ex, "Storing Settings");
      }
    }

    private void SetFileSystemWatcher(string fileName)
    {
      if (m_ViewSettings.DetectFileChanges)
      {
        var split = FileSystemUtils.SplitPath(fileName);
        fileSystemWatcher.Filter = split.FileName;
        fileSystemWatcher.Path = split.DirectoryName;
      }

      if (!string.IsNullOrEmpty(fileSystemWatcher.Path))
        fileSystemWatcher.EnableRaisingEvents = m_ViewSettings.DetectFileChanges;
    }

    private async void ShowSettings(object? sender, EventArgs e)
    {
      await m_ToolStripButtonSettings.RunWithHourglassAsync(async () =>
      {
        m_ToolStripButtonSettings.Enabled = false;

        var oldFillGuessSettings = (FillGuessSettings) m_ViewSettings.FillGuessSettings.Clone();
        using var frm = new FormEditSettings(m_ViewSettings, m_FileSetting);
        frm.ShowDialog(this);
        await m_ViewSettings.SaveViewSettingsAsync();
        if (m_FileSetting != null)
        {
          m_FileSetting.DisplayStartLineNo = m_ViewSettings.DisplayStartLineNo;
          m_FileSetting.DisplayRecordNo = m_ViewSettings.DisplayRecordNo;
          SetFileSystemWatcher(m_FileSetting.FileName);
          m_RunDetection = !m_ViewSettings.FillGuessSettings.Equals(oldFillGuessSettings);
          m_ShouldReloadData = m_ShouldReloadData || m_RunDetection || m_FileChanged;
        }
        else if (frm.FileSetting != null)
        {
          m_FileSetting = frm.FileSetting;
          m_FileChanged = true;
          m_AskOpenFile = false;
        }

        await CheckPossibleChange();
        ApplyViewSettings();
        await SaveIndividualFileSettingAsync();
      }, this);
    }

    private void ShowTextPanel(bool visible)
    {
      try
      {
        this.SafeInvoke(() =>
        {
          textPanel.Visible = visible;
          textPanel.BottomToolStripPanelVisible = visible;
          detailControl.Visible = !visible;
        });
      }
      catch (Exception)
      {
        // Ignore
      }
    }


    private void ButtonAsText(bool asText)
    {
      this.SafeInvoke(() =>
      {
        m_ToolStripButtonAsText.Text = asText ? "As Values" : "As Text";
        m_ToolStripButtonAsText.Image = asText ? Properties.Resources.AsValue : Properties.Resources.AsText;
      });
    }

    private void ChangeColumnsNoEvent(bool asText, IEnumerable<Column> columns)
    {
      if (m_FileSetting is null)
        return;

      ButtonAsText(asText);

      m_FileSetting.ColumnCollection.CollectionChanged -= ColumnCollectionOnCollectionChanged;
      m_FileSetting.ColumnCollection.Clear();
      m_FileSetting.ColumnCollection.AddRange(asText ? columns.Select(col =>
        new Column(col.Name, ValueFormat.Empty, columnOrdinal: col.ColumnOrdinal)) : columns);
      m_FileSetting.ColumnCollection.CollectionChanged += ColumnCollectionOnCollectionChanged;
    }

    private async void ToggleDisplayAsText(object? sender, EventArgs e)
    {
      if (m_FileSetting is null)
        return;

      await m_ToolStripButtonAsText.RunWithHourglassAsync(async () =>
      {
        m_ToolStripButtonAsText.Enabled = false;
        detailControl.SuspendLayout();
        // Current column setup
        var columnSetting = detailControl.GetViewStatus();

        var reload = false;

        if (m_FileSetting.ColumnCollection.Any(x => x.ValueFormat.DataType != DataTypeEnum.String))
        {
          m_StoreColumns = new List<Column>(m_FileSetting.ColumnCollection);

          // restore header names only
          ChangeColumnsNoEvent(true, m_StoreColumns);
          reload = true;
        }
        else if (m_StoreColumns != null)
        {
          // ReSharper disable once LocalizableElement
          ChangeColumnsNoEvent(false, m_StoreColumns);
          reload = true;
        }

        if (reload)
        {
          await OpenDataReaderAsync(m_CancellationTokenSource.Token);
          detailControl.SetViewStatus(columnSetting);
        }

        detailControl.ResumeLayout();
      }, this);
    }

    private void ToggleShowLog(object? sender, EventArgs e) => ShowTextPanel(!textPanel.Visible);

    private async void ToolStripButtonLoadFile_Click(object? sender, EventArgs e) => await m_ToolStripButtonLoadFile.RunWithHourglassAsync(async () => await SelectFile(), this);
  }
}