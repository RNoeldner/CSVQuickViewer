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
    private CsvFileDummy? m_FileSetting;
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

      FunctionalDI.FileReaderWriterFactory = new ViewerFileReaderWriterFactory(StandardTimeZoneAdjust.ChangeTimeZone,
        m_ViewSettings.FillGuessSettings);

#if SupportPGP
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
#endif

      // add the not button not visible in designer to the detail control
      detailControl.AddToolStripItem(0, m_ToolStripButtonSettings);
      detailControl.AddToolStripItem(0, m_ToolStripButtonLoadFile);

      detailControl.AddToolStripItem(int.MaxValue, m_ToolStripButtonAsText);
      detailControl.AddToolStripItem(int.MaxValue, m_ToolStripButtonShowLog);


      detailControl.WriteFileAsync = async (ct, reader) =>
      {
        if (m_FileSetting == null)
          return;
        try
        {
          var split = FileSystemUtils.SplitPath(m_FileSetting.FullPath);

          var fileName = WindowsAPICodePackWrapper.Save(split.DirectoryName, "Delimited File",
            "Text file|*.txt|Comma delimited (*.csv)|*.csv|Tab delimited (*.tab;*.tsv)|*.tab;*.tsv|All files (*.*)|*.*",
            ".csv", true, split.FileName);

          if (fileName is null || fileName.Length == 0)
            return;
          m_ViewSettings.WriteSetting.FileName = fileName;

          var skippedLines = new StringBuilder();
          // in case we skipped lines read them as Header, so we do not lose them
          if (m_FileSetting.SkipRows > 0)
          {
#if NET5_0_OR_GREATER
            await
#endif
              using var iStream = FunctionalDI.GetStream(new SourceAccess(m_FileSetting.FullPath));
            using var sr = new ImprovedTextReader(iStream, m_FileSetting.CodePageId);
            for (var i = 0; i < m_FileSetting.SkipRows; i++)
              skippedLines.AppendLine(await sr.ReadLineAsync());
          }

          using var formProgress = new FormProgress("Writing file", true, new FontConfig(Font.Name, Font.Size), ct);

          formProgress.Show(this);
          fileSystemWatcher.Changed -= FileSystemWatcher_Changed;
          fileSystemWatcher.EnableRaisingEvents = false;

          var writer = new CsvFileWriter(fileName, m_ViewSettings.WriteSetting.HasFieldHeader,
            m_ViewSettings.WriteSetting.ValueFormatWrite,
            m_ViewSettings.WriteSetting.CodePageId,
            m_ViewSettings.WriteSetting.ByteOrderMark,
            m_ViewSettings.WriteSetting.ColumnCollection, m_ViewSettings.WriteSetting.IdentifierInContainer,
            skippedLines.ToString(),
            m_FileSetting.Footer,
            string.Empty, m_ViewSettings.WriteSetting.NewLine, m_ViewSettings.WriteSetting.FieldDelimiterChar,
            m_ViewSettings.WriteSetting.FieldQualifierChar,
            m_ViewSettings.WriteSetting.EscapePrefixChar,
            m_ViewSettings.WriteSetting.NewLinePlaceholder,
            m_ViewSettings.WriteSetting.DelimiterPlaceholder,
            m_ViewSettings.WriteSetting.QualifierPlaceholder, m_ViewSettings.WriteSetting.QualifyAlways,
            m_ViewSettings.WriteSetting.QualifyOnlyIfNeeded,
            m_ViewSettings.WriteSetting.WriteFixedLength, StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id,
            FunctionalDI.GetKeyAndPassphraseForFile(fileName).keyFile, m_ViewSettings.WriteSetting.KeepUnencrypted);

          // can not use filteredDataGridView.Columns directly
          await writer.WriteAsync(reader, formProgress.CancellationToken);

          fileSystemWatcher.Changed += FileSystemWatcher_Changed;
          fileSystemWatcher.EnableRaisingEvents = true;

          m_ShouldReloadData = false;
        }
        catch (Exception ex)
        {
          this.ShowError(ex);
        }
      };

      detailControl.DisplaySourceAsync = async ct =>
      {
        using var sourceDisplay = new FormCsvTextDisplay(m_FileSetting!.FullPath, null);
        sourceDisplay.FontConfig = new FontConfig(Font.Name, Font.Size);
        if (m_FileSetting.IsCsv)
          await sourceDisplay.OpenFileAsync(false, m_FileSetting.FieldQualifierChar, m_FileSetting.FieldDelimiterChar,
            m_FileSetting.EscapePrefixChar,
            m_FileSetting.CodePageId, m_FileSetting.SkipRows, m_FileSetting.CommentLine, ct);
        else
          await sourceDisplay.OpenFileAsync(m_FileSetting.IsJson, '\0', '\0', '\0', 65001, m_FileSetting.SkipRows, "",
            ct);
        sourceDisplay.ShowDialog();
      };
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

        m_ToolStripButtonAsText.Visible = m_FileSetting.ColumnCollection.Any(x =>
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
      if (IsDisposed || string.IsNullOrEmpty(fileName))
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
            if (list.Count == 1)
              return list.First();
            var res = string.Empty;
            this.SafeInvoke(() =>
            {
              using var frm = new FormSelectInDropdown(list, list.FirstOrDefault(x => x.AssumeDelimited()));
              if (frm.ShowWithFont(this, true) == DialogResult.Cancel)
                throw new OperationCanceledException();
              res = frm.SelectedText;
            });
            return res;
          }, m_ViewSettings.DefaultInspectionResult,
#if SupportPGP
          PgpHelper.GetKeyAndValidate(fileName, m_ViewSettings.KeyFileRead),
#else
          string.Empty,
#endif
          cancellationToken);

        m_FileSetting = new CsvFileDummy
        {
          FileName = fileName,
          CommentLine = detection.CommentLine,
          EscapePrefixChar = detection.EscapePrefix,
          FieldDelimiterChar = detection.FieldDelimiter,
          FieldQualifierChar = detection.FieldQualifier,
          ContextSensitiveQualifier = detection.ContextSensitiveQualifier,
          DuplicateQualifierToEscape = detection.DuplicateQualifierToEscape,
          NewLine = detection.NewLine,
          ByteOrderMark = detection.ByteOrderMark,
          CodePageId = detection.CodePageId,
          HasFieldHeader = detection.HasFieldHeader,
          NoDelimitedFile = detection.NoDelimitedFile,
          IdentifierInContainer = detection.IdentifierInContainer,
          SkipRows = detection.SkipRows,
          IsJson = detection.IsJson,
          IsXml = detection.IsXml,
        };

        m_FileSetting.ColumnCollection.AddRangeNoClone(detection.Columns);
        m_FileSetting.ColumnFile = detection.ColumnFile;

#if SupportPGP
        // If keyFile was set in the process it's not yet stored
        m_FileSetting.KeyFile = PgpHelper.LookupKeyFile(fileName);
#endif
        // formProgress.Close();
        //}

        if (m_FileSetting is null)
          return;

        m_ViewSettings.DeriveWriteSetting(m_FileSetting);

        m_FileSetting.RootFolder = fileName.GetDirectoryName();
        m_ViewSettings.PassOnConfiguration(m_FileSetting);

        SetFileSystemWatcher(fileName);

        // update the UI
        // ReSharper disable once AsyncVoidLambda
        this.SafeInvoke(async () =>
        {
          var display = fileName;
          if (!string.IsNullOrEmpty(m_FileSetting.IdentifierInContainer))
            display += Path.DirectorySeparatorChar + m_FileSetting.IdentifierInContainer;

          var title = new StringBuilder(display.GetShortDisplayFileName(50));
          if (m_FileSetting is ICsvFile csv)
          {
            title.Append(" - ");
            title.Append(EncodingHelper.GetEncodingName(csv.CodePageId, csv.ByteOrderMark));
            m_WarningMax = csv.NumWarnings;
          }

          title.Append(" - ");
          title.Append(AssemblyTitle);

          ToolStripButtonAsText(false);
          Text = title.ToString();
          m_ToolStripButtonSettings.Visible = m_FileSetting != null;
          m_ToolStripButtonAsText.Visible = m_FileSetting?.ColumnCollection.Any(x =>
            x.ValueFormat.DataType != DataTypeEnum.String) ?? false;
          await OpenDataReaderAsync(cancellationToken);
        });
      }
      catch (Exception ex)
      {
        this.ShowError(ex, $"Load File {fileName}");
      }
    }

    // ReSharper disable StringLiteralTypo
    private Task SelectFile()
    {
      var strFilter = "Common types|*.csv;*.txt;*.tab;*.json;*.ndjson;*.gz|"
                      + "Delimited files|*.csv;*.txt;*.tab;*.tsv;*.dat;*.log|";

      if (m_ViewSettings.StoreSettingsByFile)
        strFilter += "Setting files|*" + SerializedFilesLib.cSettingExtension + "|";

      strFilter += "Json files|*.json;*.ndjson|"
                   + "Compressed files|*.gz;*.zip|"
                   + "All files|*.*";

      if (!FileSystemUtils.DirectoryExists(m_ViewSettings.InitialFolder))
        m_ViewSettings.InitialFolder = ".";
      var fileName = WindowsAPICodePackWrapper.Open(m_ViewSettings.InitialFolder, "File to Display", strFilter, null);
      if (!(fileName is null || fileName.Length == 0))
        return LoadCsvOrZipFileAsync(fileName, m_CancellationTokenSource.Token);
      return Task.CompletedTask;
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
          if (!m_AskOpenFile || MessageBox.Show(
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
          FileSystemUtils.FileDelete(m_FileSetting.FileName + SerializedFilesLib.cSettingExtension);
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
      m_ViewSettings.PropertyChanged += (_, args) =>
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
          var fileNameShort = m_FileSetting.FileName.GetShortDisplayFileName(60);

          detailControl.SafeInvoke(() =>
          {
            ShowTextPanel(true);
            detailControl.FillGuessSettings = m_ViewSettings.FillGuessSettings;
            detailControl.CancellationToken = cancellationToken;
            detailControl.ShowInfoButtons = false;
          });

          Logger.Debug("Loading Batch");
          using (var formProgress = new FormProgress(fileNameShort, false, FontConfig, cancellationToken))
          {
            formProgress.Show(this);
            await detailControl.LoadSettingAsync(m_FileSetting, m_ViewSettings.DurationTimeSpan, FilterTypeEnum.All,
              formProgress,
              AddWarning, formProgress.CancellationToken);
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
        ShowTextPanel(false);
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
          var fileName = m_FileSetting.FileName + SerializedFilesLib.cSettingExtension;
          await new InspectionResult(m_FileSetting).SerializeAsync(fileName, () => MessageBox.Show(
            $"Setting {fileName.GetShortDisplayFileName(50)} has been changed.\nReplace with new setting? ",
            "Settings", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes).ConfigureAwait(false);
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
        var oldFillGuessSettings = (FillGuessSettings) m_ViewSettings.FillGuessSettings.Clone();

        var editSetting = m_FileSetting?.Clone() as CsvFileDummy;
        using var frm = new FormEditSettings(m_ViewSettings, editSetting);
        frm.ShowDialog(this);
        await m_ViewSettings.SaveViewSettingsAsync();
        editSetting = frm.FileSetting;

        if (frm.FileSetting == null)
          return;

        // Update Setting
        if (m_FileSetting != null)
        {
          m_FileSetting.DisplayStartLineNo = m_ViewSettings.DisplayStartLineNo;
          m_FileSetting.DisplayRecordNo = m_ViewSettings.DisplayRecordNo;
          SetFileSystemWatcher(m_FileSetting.FileName);

          // If field headers or FillGuess has changed we need to run  Detection again
          m_RunDetection = m_FileSetting.HasFieldHeader != frm.FileSetting.HasFieldHeader ||
                           !m_ViewSettings.FillGuessSettings.Equals(oldFillGuessSettings);

          // if the file has changed need to load all
          m_FileChanged = !frm.FileSetting.FileName.Equals(m_FileSetting.FileName, StringComparison.OrdinalIgnoreCase);

          m_AskOpenFile = !(m_FileChanged || m_RunDetection);
          if (m_FileSetting is ICsvFile oldCsv && frm.FileSetting is ICsvFile newCsv)
          {
            if (oldCsv.AllowRowCombining != newCsv.AllowRowCombining || !oldCsv.ColumnCollection
                                                                       .CollectionEqualWithOrder(
                                                                         newCsv.ColumnCollection)
                                                                     || oldCsv.ByteOrderMark != newCsv.ByteOrderMark ||
                                                                     oldCsv.CodePageId != newCsv.CodePageId
                                                                     || oldCsv.ConsecutiveEmptyRows !=
                                                                     newCsv.ConsecutiveEmptyRows ||
                                                                     oldCsv.HasFieldHeader != newCsv.HasFieldHeader
                                                                     || oldCsv.NumWarnings != newCsv.NumWarnings ||
                                                                     oldCsv.SkipEmptyLines != newCsv.SkipEmptyLines
                                                                     || oldCsv.SkipRows != newCsv.SkipRows ||
                                                                     oldCsv.TreatLfAsSpace != newCsv.TreatLfAsSpace
                                                                     || oldCsv.TreatNBSPAsSpace !=
                                                                     newCsv.TreatNBSPAsSpace ||
                                                                     oldCsv.TreatTextAsNull != newCsv.TreatTextAsNull
                                                                     || oldCsv.TreatUnknownCharacterAsSpace !=
                                                                     newCsv.TreatUnknownCharacterAsSpace
                                                                     || oldCsv.TryToSolveMoreColumns !=
                                                                     newCsv.TryToSolveMoreColumns ||
                                                                     oldCsv.WarnDelimiterInValue !=
                                                                     newCsv.WarnDelimiterInValue
                                                                     || oldCsv.WarnEmptyTailingColumns !=
                                                                     newCsv.WarnEmptyTailingColumns ||
                                                                     oldCsv.WarnLineFeed != newCsv.WarnLineFeed
                                                                     || oldCsv.WarnNBSP != newCsv.WarnNBSP ||
                                                                     oldCsv.WarnQuotes != newCsv.WarnQuotes
                                                                     || oldCsv.WarnQuotesInQuotes !=
                                                                     newCsv.WarnQuotesInQuotes ||
                                                                     oldCsv.WarnUnknownCharacter !=
                                                                     newCsv.WarnUnknownCharacter
                                                                     || oldCsv.DisplayStartLineNo !=
                                                                     newCsv.DisplayStartLineNo ||
                                                                     oldCsv.DisplayRecordNo != newCsv.DisplayRecordNo
                                                                     || oldCsv.FieldDelimiterChar !=
                                                                     newCsv.FieldDelimiterChar ||
                                                                     oldCsv.FieldQualifierChar !=
                                                                     newCsv.FieldQualifierChar
                                                                     || oldCsv.EscapePrefixChar !=
                                                                     newCsv.EscapePrefixChar ||
                                                                     oldCsv.DelimiterPlaceholder !=
                                                                     newCsv.DelimiterPlaceholder
                                                                     || oldCsv.NewLinePlaceholder !=
                                                                     newCsv.NewLinePlaceholder ||
                                                                     oldCsv.NewLinePlaceholder !=
                                                                     newCsv.NewLinePlaceholder
                                                                     || oldCsv.QualifierPlaceholder !=
                                                                     newCsv.QualifierPlaceholder ||
                                                                     oldCsv.CommentLine != newCsv.CommentLine
                                                                     || oldCsv.ContextSensitiveQualifier !=
                                                                     newCsv.ContextSensitiveQualifier ||
                                                                     oldCsv.DuplicateQualifierToEscape !=
                                                                     newCsv.DuplicateQualifierToEscape)
              m_ShouldReloadData = true;
          }

          m_FileSetting.ColumnCollection.CollectionChanged -= ColumnCollectionOnCollectionChanged;
          frm.FileSetting.CopyTo(m_FileSetting);
          m_FileSetting.ColumnCollection.CollectionChanged += ColumnCollectionOnCollectionChanged;
        }
        // Set Setting
        else
        {
          m_FileSetting = editSetting;
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


    private void ToolStripButtonAsText(bool asText)
    {
      m_ToolStripButtonAsText.Text = asText ? "As Values" : "As Text";
      m_ToolStripButtonAsText.Image = asText ? Properties.Resources.AsValue : Properties.Resources.AsText;
    }

    private void ChangeColumnsNoEvent(bool asText, IEnumerable<Column> columns)
    {
      if (m_FileSetting is null)
        return;
      this.SafeInvoke(() => ToolStripButtonAsText(asText));
      m_FileSetting.ColumnCollection.CollectionChanged -= ColumnCollectionOnCollectionChanged;
      m_FileSetting.ColumnCollection.Clear();
      m_FileSetting.ColumnCollection.AddRange(asText
        ? columns.Select(col =>
          new Column(col.Name, ValueFormat.Empty, columnOrdinal: col.ColumnOrdinal))
        : columns);
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

    private async void ToolStripButtonLoadFile_Click(object? sender, EventArgs e) =>
      await m_ToolStripButtonLoadFile.RunWithHourglassAsync(SelectFile, this);
  }
}