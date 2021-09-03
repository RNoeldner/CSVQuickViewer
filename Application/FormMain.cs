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

using Microsoft.Extensions.Logging;
using Microsoft.Win32;
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

    private readonly DetailControlLoader m_DetailControlLoader;

    private readonly Timer m_SettingsChangedTimerChange = new Timer(200);

    private ICollection<IColumn>? m_StoreColumns;

    private readonly ViewSettings m_ViewSettings;

    private bool m_ConfigChanged;

    private bool m_FileChanged;

    private IFileSettingPhysicalFile? m_FileSetting;

    private ICollection<string>? m_Headers;

    private FormCsvTextDisplay? m_SourceDisplay;

    private int m_WarningCount;
    private int m_WarningMax = 100;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormMain" /> class.
    /// </summary>
    /// <param name="viewSettings">Default view Settings</param>
    public FormMain(in ViewSettings viewSettings)
    {
      m_ViewSettings = viewSettings;

      InitializeComponent();
      Text = AssemblyTitle;

      m_DetailControlLoader = new DetailControlLoader(detailControl);
      // add the not button not visible in designer to the detail control
      detailControl.AddToolStripItem(1, m_ToolStripButtonSettings);
      detailControl.AddToolStripItem(1, m_ToolStripButtonLoadFile);
      detailControl.AddToolStripItem(int.MaxValue, m_ToolStripButtonSource);
      detailControl.AddToolStripItem(int.MaxValue, m_ToolStripButtonAsText);
      detailControl.AddToolStripItem(int.MaxValue, m_ToolStripButtonShowLog);
      detailControl.BeforeFileStored += BeforeFileStored;
      detailControl.FileStored += FileStored;
      detailControl.HTMLStyle = m_ViewSettings.HTMLStyle;
      detailControl.MenuDown = m_ViewSettings.MenuDown;

      this.LoadWindowState(m_ViewSettings.WindowPosition);
      ShowTextPanel(true);

      m_ViewSettings.FillGuessSettings.PropertyChanged += AnyPropertyChangedReload;
      SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
      SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
      m_SettingsChangedTimerChange.AutoReset = false;
      m_SettingsChangedTimerChange.Elapsed += async (sender, args) => await OpenDataReaderAsync();
      m_SettingsChangedTimerChange.Stop();
    }

    public DataTable DataTable
    {
      get => detailControl.DataTable;
    }

    private static string AssemblyTitle
    {
      get
      {
        var assembly = Assembly.GetExecutingAssembly();
        var attributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
        if (attributes.Length > 0)
        {
          var titleAttribute = (AssemblyTitleAttribute) attributes[0];
          if (titleAttribute.Title.Length != 0)
            return titleAttribute.Title + " " + assembly.GetName().Version;
        }

        return Path.GetFileNameWithoutExtension(assembly.Location);
      }
    }

    /// <summary>
    ///   Initializes the file settings.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public async Task LoadCsvFile(string fileName)
    {
      if (IsDisposed)
        return;
      ShowTextPanel(true);

      if (string.IsNullOrEmpty(fileName))
        return;

      if (!FileSystemUtils.FileExists(fileName))
      {
        Logger.Warning("Filename {filename} not found or not accessible.", fileName);
        return;
      }

      try
      {
        using var processDisplay = new CustomProcessDisplay(m_CancellationTokenSource.Token);
        DetachPropertyChanged(m_FileSetting);

        m_FileSetting = (await fileName.AnalyseFileAsync(m_ViewSettings.AllowJson,
          m_ViewSettings.GuessCodePage,
          m_ViewSettings.GuessDelimiter, m_ViewSettings.GuessQualifier, m_ViewSettings.GuessStartRow,
          m_ViewSettings.GuessHasHeader, m_ViewSettings.GuessNewLine, m_ViewSettings.GuessComment,
          m_ViewSettings.FillGuessSettings, processDisplay)).PhysicalFile();

        if (m_FileSetting is null)
          return;
        ViewSettings.CopyConfiguration(m_ViewSettings, m_FileSetting, false);

        // update the UI
        this.SafeInvoke(() =>
        {
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
          Text = title.ToString();

          m_ToolStripButtonAsText.Visible = m_FileSetting is ICsvFile &&
                                            m_FileSetting.ColumnCollection.Any(x =>
                                              x.ValueFormat.DataType != DataType.String);
          SetFileSystemWatcher(fileName);
        });

        await OpenDataReaderAsync();

        ShowTextPanel(false);
      }
      catch (Exception ex)
      {
        this.ShowError(ex, "Examining File");
      }
    }

    public async Task SelectFile(string message)
    {
      this.SafeInvoke(() =>
      {
        m_ToolStripButtonLoadFile.Enabled = false;
        m_ToolStripButtonLoadFile2.Enabled = false;
      });

      var oldCursor = Equals(Cursor.Current, Cursors.WaitCursor) ? Cursors.WaitCursor : Cursors.Default;
      try
      {
        loggerDisplay.LogInformation(message);
        var strFilter = "Common types|*.csv;*.txt;*.tab;*.json;*.ndjson;*.gz|"
                        + "Delimited files (*.csv;*.txt;*.tab;*.tsv;*.dat;*.log)|*.csv;*.txt;*.tab;*.tsv;*.dat;*.log|";

        if (m_ViewSettings.StoreSettingsByFile)
          strFilter += "Setting files (*" + CsvFile.cCsvSettingExtension + ")|*" + CsvFile.cCsvSettingExtension + "|";

        strFilter += "Json files (*.json;*.ndjson)|*.json;*.ndjson|"
                     + "Compressed files (*.gz;*.zip)|*.gz;*.zip|"
                     + "All files (*.*)|*.*";

        var fileName = WindowsAPICodePackWrapper.Open(".", "File to Display", strFilter, null);
        if (!string.IsNullOrEmpty(fileName))
        {
          Cursor.Current = Cursors.WaitCursor;
          await LoadCsvFile(fileName!);
        }
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
      }
      finally
      {
        Cursor.Current = oldCursor;
        this.SafeInvoke(() =>
        {
          m_ToolStripButtonLoadFile.Enabled = true;
          m_ToolStripButtonLoadFile2.Enabled = true;
        });
      }
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
    ///   As any property is changed this will cause a reload from file
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">
    ///   The <see cref="PropertyChangedEventArgs" /> instance containing the event data.
    /// </param>
    /// <remarks>Called by ViewSettings.FillGuessSetting or Columns</remarks>
    private void AnyPropertyChangedReload(object? sender, PropertyChangedEventArgs? e) => m_ConfigChanged = true;

    /// <summary>
    ///   Attaches the property changed handlers for the file Settings
    /// </summary>
    /// <param name="fileSetting">The file setting.</param>
    private void AttachPropertyChanged(IFileSetting fileSetting)
    {
      try
      {
        fileSetting.PropertyChanged += FileSetting_PropertyChanged;
        fileSetting.FileFormat.PropertyChanged += AnyPropertyChangedReload;
        fileSetting.ColumnCollection.CollectionChanged += ColumnCollectionOnCollectionChanged;

        if (!string.IsNullOrEmpty(fileSystemWatcher.Path))
          fileSystemWatcher.EnableRaisingEvents = m_ViewSettings.DetectFileChanges;
      }
      catch
      {
        Logger.Information("Adding file system watcher failed");
      }
    }

    private void ColumnCollectionOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => m_ConfigChanged = true;

    private async Task CheckPossibleChange()
    {
      try
      {
        if (m_ConfigChanged)
        {
          m_ConfigChanged = false;
          detailControl.MoveMenu();
          if (_MessageBox.Show(
                "The configuration has changed do you want to reload the data?",
            "Configuration changed",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            await OpenDataReaderAsync();
          else
            m_ConfigChanged = false;
        }

        if (!m_FileChanged) return;
        m_FileChanged = false;
        if (_MessageBox.Show(
              "The displayed file has changed do you want to reload the data?",
          "File changed",
          MessageBoxButtons.YesNo,
          MessageBoxIcon.Question,
          MessageBoxDefaultButton.Button2) == DialogResult.Yes)
          await OpenDataReaderAsync();
        else
          m_FileChanged = false;

        if (m_FileSetting != null && !m_ViewSettings.StoreSettingsByFile)
          FileSystemUtils.FileDelete(m_FileSetting.FileName + CsvFile.cCsvSettingExtension);
      }
      catch (Exception exception)
      {
        Logger.Warning(exception, "Checking for changes");
      }
    }

    /// <summary>
    ///   Detaches the property changed handlers for the file Setting
    /// </summary>
    /// <param name="fileSetting">The file setting.</param>
    private void DetachPropertyChanged(IFileSetting? fileSetting)
    {
      m_SettingsChangedTimerChange.Stop();
      fileSystemWatcher.EnableRaisingEvents = false;

      if (fileSetting is null) return;

      fileSetting.PropertyChanged -= FileSetting_PropertyChanged;
      fileSetting.FileFormat.PropertyChanged -= AnyPropertyChangedReload;
      fileSetting.ColumnCollection.CollectionChanged -= ColumnCollectionOnCollectionChanged;
    }

    /// <summary>
    ///   Handles the DragDrop event of the dataGridView control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="DragEventArgs" /> instance containing the event data.</param>
    private async void FileDragDrop(object? sender, DragEventArgs e)
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
    private void FileDragEnter(object? sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
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

    private void BeforeFileStored(object? sender, IFileSettingPhysicalFile e)
    {
      fileSystemWatcher.Changed -= FileSystemWatcher_Changed;
      fileSystemWatcher.EnableRaisingEvents = false;
    }

    private void FileStored(object? sender, IFileSettingPhysicalFile e)
    {
      fileSystemWatcher.Changed += FileSystemWatcher_Changed;
      fileSystemWatcher.EnableRaisingEvents =  m_ViewSettings.DetectFileChanges;
      m_ConfigChanged = false;
      if (m_ViewSettings.StoreSettingsByFile)
        SerializedFilesLib.SaveSettingFile(e, () => true);
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
    private async void FormMain_Activated(object? sender, EventArgs e) => await CheckPossibleChange();

    private void FormMain_FormClosing(object? sender, FormClosingEventArgs e)
    {
      if (!m_CancellationTokenSource.IsCancellationRequested)
      {
        m_CancellationTokenSource.Cancel();
        // Give the possibly running threads some time to exit
        Thread.Sleep(100);
      }

      if (e.CloseReason != CloseReason.UserClosing) return;
      Logger.Debug("Closing Form");

      var res = this.StoreWindowState();
      if (res != null)
        m_ViewSettings.WindowPosition = res;
      m_ViewSettings.SaveViewSettings();
      SaveIndividualFileSetting();
    }

    private async void FormMain_KeyUpAsync(object? sender, KeyEventArgs e)
    {
      if (e.KeyCode != Keys.F5 && (!e.Control || e.KeyCode != Keys.R)) return;
      e.Handled = true;
      await OpenDataReaderAsync();
    }

    /// <summary>
    ///   Opens the data reader.
    /// </summary>
    private async Task OpenDataReaderAsync()
    {
      if (m_FileSetting is null)
        return;

      var oldCursor = Equals(Cursor.Current, Cursors.WaitCursor) ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;

      // Stop Property changed events for the time this is processed We might store data in the FileSetting
      DetachPropertyChanged(m_FileSetting);

      try
      {
        var fileNameShort = FileSystemUtils.GetShortDisplayFileName(m_FileSetting.FileName, 60);

        using (var processDisplay = new FormProcessDisplay(fileNameShort, false, m_CancellationTokenSource.Token))
        {
          processDisplay.AttachTaskbarProgress();
          processDisplay.Show();
          processDisplay.SetProcess("Reading data...", -1, false);
          processDisplay.Maximum = 100;

          this.SafeInvoke(() =>
          {
            detailControl.FileSetting = m_FileSetting;
            detailControl.FillGuessSettings = m_ViewSettings.FillGuessSettings;
            detailControl.CancellationToken = m_CancellationTokenSource.Token;
            detailControl.ShowInfoButtons = false;
          });

          await m_DetailControlLoader.StartAsync(m_FileSetting, false, m_ViewSettings.Duration, processDisplay,
            AddWarning);

          m_Headers = detailControl.DataTable.GetRealColumns().ToArray();
          foreach (var columnName in m_Headers)
          {
            if (m_FileSetting.ColumnCollection.Get(columnName) is null)
              m_FileSetting.ColumnCollection.Add(new Column { Name = columnName });
          }

          FunctionalDI.GetColumnHeader = (dummy1, dummy3) => Task.FromResult(m_Headers);

          this.SafeBeginInvoke(() => { ShowTextPanel(false); });
          FunctionalDI.SignalBackground.Invoke();

          if (m_DisposedValue)
            return;
        }

        // The reader is used when data is stored through the detailControl
        FunctionalDI.SQLDataReader = async (settingName, message, timeout, token) =>
          await Task.FromResult(new DataTableWrapper(detailControl.DataTable));

        // Load View Settings
        if (m_FileSetting is BaseSettingPhysicalFile basePhys && FileSystemUtils.FileExists(basePhys.ColumnFile))
        {
          Logger.Information("Restoring view and filter setting {filename}...", basePhys.ColumnFile);
          detailControl.ReStoreViewSetting(basePhys.ColumnFile);
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
          ShowTextPanel(false);

          detailControl.ShowInfoButtons = true;
          Cursor.Current = oldCursor;

          m_ConfigChanged = false;
          m_FileChanged = false;

          // Re enable event watching
          AttachPropertyChanged(m_FileSetting);
        }
      }
    }

    private void SaveIndividualFileSetting()
    {
      try
      {
        if (m_FileSetting != null && m_ViewSettings.StoreSettingsByFile)
        {
          var fileName = m_FileSetting.FileName + CsvFile.cCsvSettingExtension;
          SerializedFilesLib.SaveSettingFile(m_FileSetting,
            () => _MessageBox.Show($"Setting {FileSystemUtils.GetShortDisplayFileName(fileName, 50)} has been changed.\nReplace with new setting? ", "Settings", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes);
        }
        m_ConfigChanged = false;
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
      if (m_FileSetting==null)
        return;
      await m_ToolStripButtonSettings.RunWithHourglassAsync(async () =>

      {
        m_ToolStripButtonSettings.Enabled = false;
        ViewSettings.CopyConfiguration(m_FileSetting, m_ViewSettings, true);
        using var frm = new FormEditSettings(m_ViewSettings);
        frm.ShowDialog(MdiParent);
        m_ViewSettings.SaveViewSettings();
        detailControl.MenuDown = m_ViewSettings.MenuDown;
        SetFileSystemWatcher(m_FileSetting.FileName);
        ViewSettings.CopyConfiguration(m_ViewSettings, m_FileSetting, true);

        await CheckPossibleChange();
      });
    }

    private void ShowSourceFile(object? sender, EventArgs e)
    {
      if (m_SourceDisplay != null) return;
      if (m_FileSetting is null) return;
      m_ToolStripButtonSource!.RunWithHourglass(() =>
      {
        m_ToolStripButtonSource.Enabled = false;
        m_SourceDisplay = new FormCsvTextDisplay(m_FileSetting.FileName);
        m_SourceDisplay.FormClosed += SourceDisplayClosed;
        m_SourceDisplay.Show();
        using var proc = new FormProcessDisplay("Display Source", false, m_CancellationTokenSource.Token);
        proc.Show(this);

        proc.Maximum = 0;
        proc.SetProcess("Reading source and applying color coding", 0, false);

        m_SourceDisplay.OpenFile(m_FileSetting is IJsonFile,
          m_FileSetting.FileFormat.FieldQualifier,
          m_FileSetting.FileFormat.FieldDelimiter, m_FileSetting.FileFormat.EscapeCharacter,
          m_FileSetting is ICsvFile csv ? csv.CodePageId : 65001, m_FileSetting.SkipRows, m_FileSetting.FileFormat.CommentLine);
        proc.Close();
      });
    }

    private void ShowTextPanel(bool visible)
    {
      textPanel!.SafeInvoke(() =>
      {
        textPanel.Visible = visible;
        textPanel.BottomToolStripPanelVisible = visible;
        detailControl.Visible = !visible;
      });
    }

    private void SourceDisplayClosed(object? sender, FormClosedEventArgs e)
    {
      m_SourceDisplay?.Dispose();
      m_SourceDisplay = null;
      m_ToolStripButtonSource.Enabled = true;
    }

    private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e) =>
      this.LoadWindowState(m_ViewSettings.WindowPosition);

    private void SystemEvents_PowerModeChanged(object? sender, PowerModeChangedEventArgs e)
    {
      switch (e.Mode)
      {
        case PowerModes.Suspend:
          Logger.Debug("Power Event Suspend");
          var res = this.StoreWindowState();
          m_ViewSettings.WindowPosition = res;
          break;

        case PowerModes.Resume:
          Logger.Debug("Power Event Resume");
          this.LoadWindowState(m_ViewSettings.WindowPosition);
          break;
      }
    }

    private async void ToggleDisplayAsText(object? sender, EventArgs e)
    {
      if (m_FileSetting==null)
        return;
      await m_ToolStripButtonAsText.RunWithHourglassAsync(async () =>
      {
        m_ToolStripButtonAsText.Enabled = false;
        detailControl.SuspendLayout();

        var store = ViewSetting.StoreViewSetting(detailControl.FilteredDataGridView,
          Array.Empty<ToolStripDataGridViewColumnFilter?>());
        // Assume data type is not recognize
        if (m_FileSetting.ColumnCollection.Any(x => x.ValueFormat.DataType != DataType.String))
        {
          Logger.Information("Showing columns as text");
          m_StoreColumns = new ColumnCollection(m_FileSetting.ColumnCollection);
          m_FileSetting.ColumnCollection.CollectionChanged -= ColumnCollectionOnCollectionChanged;
          m_FileSetting.ColumnCollection.Clear();
          // restore header names
          foreach (var col in m_StoreColumns)
            m_FileSetting.ColumnCollection.Add(new Column(col.Name) { ColumnOrdinal = col.ColumnOrdinal });
          m_FileSetting.ColumnCollection.CollectionChanged += ColumnCollectionOnCollectionChanged;
          m_ToolStripButtonAsText.Text = "As Values";
          m_ToolStripButtonAsText.Image = Properties.Resources.AsValue;
        }
        else
        {
          Logger.Information("Showing columns as values");
          m_ToolStripButtonAsText.Text = "As Text";
          m_ToolStripButtonAsText.Image = Properties.Resources.AsText;
          m_FileSetting.ColumnCollection.CollectionChanged -= ColumnCollectionOnCollectionChanged;
          m_StoreColumns?.CollectionCopy(m_FileSetting.ColumnCollection);
          m_FileSetting.ColumnCollection.CollectionChanged += ColumnCollectionOnCollectionChanged;
        }

        await OpenDataReaderAsync();

        ViewSetting.ReStoreViewSetting(store, detailControl.FilteredDataGridView.Columns,
          Array.Empty<ToolStripDataGridViewColumnFilter?>(), null, null);
        detailControl.ResumeLayout();
      });
    }

    private void ToggleShowLog(object? sender, EventArgs e)
    {
      ShowTextPanel(!textPanel.Visible);
    }

    private async void ToolStripButtonLoadFile_Click(object? sender, EventArgs e) => await SelectFile("Open File Dialog");
  }
}