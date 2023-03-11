/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace CsvTools
{
  /// <inheritdoc cref="UserControl" />
  /// <summary>
  ///   Windows from to show detail information for a dataTable
  /// </summary>
  public sealed partial class DetailControl : UserControl
  {
    private readonly List<DataGridViewCell> m_FoundCells = new();
    private readonly List<ToolStripItem> m_ToolStripItems = new();
    private CancellationToken m_CancellationToken = CancellationToken.None;
    private DataTable m_DataTable = new();
    private bool m_DisposedValue; // To detect redundant calls
    private FilterDataTable m_FilterDataTable = new FilterDataTable(new DataTable());
    private FormDuplicatesDisplay? m_FormDuplicatesDisplay;
    private FormShowMaxLength? m_FormShowMaxLength;
    private FormUniqueDisplay? m_FormUniqueDisplay;
    private FormHierarchyDisplay? m_HierarchyDisplay;
    private bool m_MenuDown;
    private bool m_ShowButtons = true;
    private bool m_ShowFilter = true;
    private bool m_UpdateVisibility = true;
    private readonly SteppedDataTableLoader m_SteppedDataTableLoader;

    public async Task LoadSettingAsync(IFileSetting fileSetting,
      bool addErrorField, bool restoreError, TimeSpan durationInitial, FilterTypeEnum filterType,
      IProgress<ProgressInfo>? progress, EventHandler<WarningEventArgs>? addWarning,
      CancellationToken cancellationToken) =>
      await m_SteppedDataTableLoader.StartAsync(fileSetting, dataTable => DataTable = dataTable,
        t => RefreshDisplay(filterType, t), addErrorField, restoreError,
        durationInitial, progress, addWarning, cancellationToken);

    /// <inheritdoc />
    /// <summary>
    ///   Initializes a new instance of the <see cref="DetailControl" /> class.
    /// </summary>
#if !NETFRAMEWORK
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif

    public DetailControl()
    {
      InitializeComponent();

      // This created a BatchLoader
      m_SteppedDataTableLoader = new SteppedDataTableLoader();

      m_ToolStripItems.Add(m_ToolStripComboBoxFilterType);
      m_ToolStripItems.Add(m_ToolStripButtonUniqueValues);
      m_ToolStripItems.Add(m_ToolStripButtonDuplicates);
      m_ToolStripItems.Add(m_ToolStripButtonHierarchy);
      m_ToolStripItems.Add(m_ToolStripButtonColumnLength);
      m_ToolStripItems.Add(m_ToolStripButtonSource);
      m_ToolStripItems.Add(m_ToolStripButtonStore);
    }

    public EventHandler<IFileSettingPhysicalFile>? BeforeFileStored;
    public EventHandler<IFileSettingPhysicalFile>? FileStored;

    /// <summary>
    ///   Gets or sets the HTML style.
    /// </summary>
    /// <value>The HTML style.</value>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Bindable(false)]
    [Browsable(false)]
    public HtmlStyle HtmlStyle { get => FilteredDataGridView.HtmlStyle; set => FilteredDataGridView.HtmlStyle = value; }


    [Bindable(false)]
    [Browsable(true)]
    [DefaultValue(2000)]
    public int ShowButtonAtLength { get => FilteredDataGridView.ShowButtonAtLength; set => FilteredDataGridView.ShowButtonAtLength = value; }

    /// <summary>
    /// Sort the data by this column ascending 
    /// </summary>
    /// <param name="dataColumnName">The name of the data column</param>
    /// <param name="direction">Direction for sorting, by default it is Ascending</param>
    public void Sort(string dataColumnName, ListSortDirection direction = ListSortDirection.Ascending)
    {
      try
      {
        FilteredDataGridView.SafeInvoke(
          () =>
          {
            var col = GetViewColumn(dataColumnName);
            if (col != null)
              FilteredDataGridView.Sort(col, direction);
          }
        );
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "Processing Sorting {exception}", ex.InnerExceptionMessages());
      }
    }

    /// <summary>
    /// Search the displayed data for a specific text, and highlight the found items 
    /// </summary>
    /// <param name="searchText"></param>
    // ReSharper disable once MemberCanBePrivate.Global
    public void SearchText(string searchText)
    {
      searchBackgroundWorker.CancelAsync();
      searchBackgroundWorker.RunWorkerAsync(searchText);
    }

    private DataGridViewColumn? GetViewColumn(string dataColumnName) =>
      FilteredDataGridView.Columns.Cast<DataGridViewColumn>().FirstOrDefault(col =>
        col.DataPropertyName.Equals(dataColumnName, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Set a filter on a data column
    /// </summary>
    /// <param name="dataColumnName">The name of the data column</param>
    /// <param name="op">The operator for teh filter e.G. =</param>
    /// <param name="value">The value to compare to</param>
    // ReSharper disable once UnusedMember.Global
    public void SetFilter(string dataColumnName, string op, object value)
    {
      try
      {
        FilteredDataGridView.SafeInvoke(
          () =>
          {
            var col = GetViewColumn(dataColumnName);
            if (col == null) return;
            var columnFilters = FilteredDataGridView.SetFilterMenu(col.Index);

            if (columnFilters == null) return;
            columnFilters.ColumnFilterLogic.Operator = op;
            if (value is DateTime dateTime)
              columnFilters.ColumnFilterLogic.ValueDateTime = dateTime;
            else
              columnFilters.ColumnFilterLogic.ValueText = Convert.ToString(value) ?? string.Empty;
            columnFilters.ColumnFilterLogic.Active = true;

            FilteredDataGridView.ApplyFilters();
          }
        );
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "Processing Filter {exception}", ex.InnerExceptionMessages());
      }
    }

    public string GetViewStatus() => FilteredDataGridView.GetViewStatus;
    public void SetViewStatus(string newStatus) => FilteredDataGridView.SetViewStatus(newStatus);

    /// <summary>
    ///   General Setting that determines if the menu is display in the bottom of a detail control
    /// </summary>
    public bool MenuDown
    {
      // ReSharper disable once UnusedMember.Global : Needed for Forms Designer
      get => m_MenuDown;
      set
      {
        if (m_MenuDown == value) return;
        m_MenuDown = value;
        m_UpdateVisibility = true;
      }
    }

    /// <summary>
    ///   AlternatingRowDefaultCellStyle of data grid
    /// </summary>
    [Browsable(true)]
    [TypeConverter(typeof(DataGridViewCellStyleConverter))]
    [Category("Appearance")]
    public DataGridViewCellStyle AlternatingRowDefaultCellStyle
    {
      // ReSharper disable once UnusedMember.Global : Needed for Forms Designer
      get => FilteredDataGridView.AlternatingRowsDefaultCellStyle;
      set => FilteredDataGridView.AlternatingRowsDefaultCellStyle = value;
    }

    public CancellationToken CancellationToken
    {
      set
      {
        m_CancellationToken = value;
      }
    }


    /// <summary>
    ///   Allows setting the data table
    /// </summary>
    /// <value>The data table.</value>
    public DataTable DataTable
    {
      get => m_DataTable;
      set
      {
        if (ReferenceEquals(m_DataTable, value))
          return;

        m_DataTable.Dispose();
        m_FilterDataTable.Dispose();

        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        m_DataTable = value ?? new DataTable();
        m_FilterDataTable = new FilterDataTable(m_DataTable);
      }
    }

    /// <summary>
    ///   DefaultCellStyle of data grid
    /// </summary>
    [Browsable(true)]
    [TypeConverter(typeof(DataGridViewCellStyleConverter))]
    [Category("Appearance")]
    public DataGridViewCellStyle DefaultCellStyle
    {
      // ReSharper disable once UnusedMember.Global
      get => FilteredDataGridView.DefaultCellStyle;
      set => FilteredDataGridView.DefaultCellStyle = value;
    }

    /// <summary>
    ///   A File Setting
    /// </summary>
    public IFileSetting? FileSetting
    {
      get => FilteredDataGridView.FileSetting;
      set
      {
        FilteredDataGridView.FileSetting = value;
        //        m_UpdateVisibility = true;
      }
    }

    /// <summary>
    ///   A File Setting
    /// </summary>
    public ICsvFile? WriteSetting
    {
      get;
      set;
    }

    /// <summary>
    ///   A File Setting
    /// </summary>
    public FillGuessSettings? FillGuessSettings
    {
      set => FilteredDataGridView.FillGuessSettings = value;
    }

    public int FrozenColumns
    {
      set => FilteredDataGridView.FrozenColumns = value;
    }

    /// <summary>
    ///   Gets or sets a value indicating whether this is a read only.
    /// </summary>
    /// <value><c>true</c> if read only; otherwise, <c>false</c>.</value>
    [Browsable(true)]
    [DefaultValue(false)]
    [Category("Behavior")]
    public bool ReadOnly
    {
      set
      {
        if (FilteredDataGridView.ReadOnly == value)
          return;
        FilteredDataGridView.ReadOnly = value;
        FilteredDataGridView.AllowUserToAddRows = !value;
        FilteredDataGridView.AllowUserToDeleteRows = !value;
      }
    }

    /// <summary>
    ///   Gets or sets a value indicating whether to allow filtering.
    /// </summary>
    /// <value><c>true</c> if filter button should be shown; otherwise, <c>false</c>.</value>
    [Browsable(true)]
    [DefaultValue(true)]
    [Category("Appearance")]
    public bool ShowFilter
    {
      set
      {
        if (m_ShowFilter == value)
          return;
        m_ShowFilter = value;
        m_UpdateVisibility = true;
      }
    }

    /// <summary>
    ///   Gets or sets a value indicating whether to show buttons.
    /// </summary>
    /// <value><c>true</c> if buttons are to be shown; otherwise, <c>false</c>.</value>
    [Browsable(true)]
    [DefaultValue(true)]
    [Category("Appearance")]
    public bool ShowInfoButtons
    {
      set
      {
        if (m_ShowButtons == value)
          return;
        m_ShowButtons = value;
        m_UpdateVisibility = true;
      }
    }

    /// <summary>
    ///   Sets the name of the unique field.
    /// </summary>
    /// <value>The name of the unique field.</value>
    public IEnumerable<string>? UniqueFieldName
    {
      set
      {
        // in case we do not have unique names and the table is not loaded do nothing
        if (value is null || !value.Any())
          return;
        m_FilterDataTable.UniqueFieldName = value;
      }
    }

    public void AddToolStripItem(int index, ToolStripItem item)
    {
      if (item is null)
        throw new ArgumentNullException(nameof(item));
      if (!m_ToolStripItems.Contains(item))
      {
        if (index >= m_ToolStripItems.Count)
          m_ToolStripItems.Add(item);
        else
          m_ToolStripItems.Insert(index, item);
        m_UpdateVisibility = true;
      }
    }


    private void DetailControl_FontChanged(object? sender, EventArgs e)
    {
      this.SafeInvoke(() =>
      {
        FilteredDataGridView.Font = Font;
        m_BindingNavigator.Font = Font;
        m_ToolStripTop.Font = Font;
      });
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      if (m_DisposedValue) return;

      if (disposing)
      {
        m_DisposedValue = true;
        components.Dispose();
        m_FormShowMaxLength?.Dispose();
        m_FormDuplicatesDisplay?.Dispose();
        m_FormUniqueDisplay?.Dispose();
        m_DataTable.Dispose();
        m_FilterDataTable.Dispose();
        m_HierarchyDisplay?.Dispose();
        m_SteppedDataTableLoader.Dispose();
        m_SourceDisplay?.Dispose();
      }

      base.Dispose(disposing);
    }

    private void AutoResizeColumns(DataTable source)
    {
      if (source.Rows.Count < 10000 && source.Columns.Count < 50)
        FilteredDataGridView.AutoResizeColumns(
          source.Rows.Count < 1000 && source.Columns.Count < 20
            ? DataGridViewAutoSizeColumnsMode.AllCells
            : DataGridViewAutoSizeColumnsMode.DisplayedCells);
    }

    private void searchBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
    {
      // Start the search
      if (sender is not BackgroundWorker bw) return;
      if (e.Argument is not string searchText) return;

      List<DataGridViewCell> found = new();
      if (searchText.Length > 0)
        foreach (var viewColumn in FilteredDataGridView.Columns.Cast<DataGridViewColumn>()
                   .Where(col => col.Visible && !string.IsNullOrEmpty(col.DataPropertyName)))
        {
          if (found.Count > 999)
            break;
          foreach (DataGridViewRow row in FilteredDataGridView.Rows)
          {
            if (bw.CancellationPending)
              return;

            var cell = row.Cells[viewColumn.Index];
            if (cell.FormattedValue?.ToString()?.IndexOf(searchText, 0, StringComparison.CurrentCultureIgnoreCase) ==
                -1)
              continue;

            found.Add(cell);
            if (found.Count > 999)
              break;
          }
        }

      e.Result = new Tuple<IEnumerable<DataGridViewCell>, string>(found, searchText);
    }

    private void searchBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      m_FoundCells.Clear();
      FilteredDataGridView.HighlightText = string.Empty;
      if (!e.Cancelled)
      {
        if (e.Result is Tuple<IEnumerable<DataGridViewCell>, string> tpl)
        {
          foreach (var cell in tpl.Item1)
          {
            if (cell.Displayed)
              FilteredDataGridView.InvalidateCell(cell);
            m_FoundCells.Add(cell);
          }

          FilteredDataGridView.HighlightText = tpl.Item2;
        }

        m_Search.Results = m_FoundCells.Count;
      }

      FilteredDataGridView.Refresh();
    }

    /// <summary>
    ///   Handles the Click event of the buttonTableSchema control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ButtonColumnLength_Click(object? sender, EventArgs e)
    {
      if (FilteredDataGridView.Columns.Count <= 0)
        return;
      m_ToolStripButtonColumnLength.RunWithHourglass(() =>
      {
        var visible = FilteredDataGridView.Columns.Cast<DataGridViewColumn>()
          .Where(col => col.Visible && !string.IsNullOrEmpty(col.DataPropertyName)).OrderBy(col => col.DisplayIndex)
          .Select(col => col.DataPropertyName).ToList();
        m_FormShowMaxLength?.Close();
        m_FormShowMaxLength =
          new FormShowMaxLength(m_DataTable, m_DataTable.Select(FilteredDataGridView.CurrentFilter), visible,
            HtmlStyle);
        m_FormShowMaxLength.ShowWithFont(this);
        m_FormShowMaxLength.FormClosed += (_, _) => this.SafeInvoke(() => m_ToolStripButtonColumnLength.Enabled = true);
      }, ParentForm);
      m_ToolStripButtonColumnLength.Enabled = false;
    }

    /// <summary>
    ///   Handles the Click event of the duplicate control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ButtonDuplicates_Click(object? sender, EventArgs e)
    {
      if (FilteredDataGridView.Columns.Count <= 0)
        return;
      m_ToolStripButtonDuplicates.RunWithHourglass(() =>
      {
        var columnName = FilteredDataGridView.CurrentCell != null
          ? FilteredDataGridView.Columns[FilteredDataGridView.CurrentCell.ColumnIndex].Name
          : FilteredDataGridView.Columns[0].Name;
        try
        {
          m_FormDuplicatesDisplay?.Close();
          m_FormDuplicatesDisplay =
            new FormDuplicatesDisplay(m_DataTable.Clone(), m_DataTable.Select(FilteredDataGridView.CurrentFilter),
              columnName, HtmlStyle)
            { Icon = ParentForm?.Icon };
          m_FormDuplicatesDisplay.ShowWithFont(this);
          m_FormDuplicatesDisplay.FormClosed +=
            (_, _) => this.SafeInvoke(() => m_ToolStripButtonDuplicates.Enabled = true);
        }
        catch (Exception ex)
        {
          ParentForm.ShowError(ex);
        }
      }, ParentForm);
      m_ToolStripButtonDuplicates.Enabled = false;
    }

    /// <summary>
    ///   Handles the Click event of the buttonHierarchy control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ButtonHierarchy_Click(object? sender, EventArgs e)
    {
      m_ToolStripButtonHierarchy.RunWithHourglass(() =>
      {
        try
        {
          m_HierarchyDisplay?.Close();
          m_HierarchyDisplay =
            new FormHierarchyDisplay(m_DataTable.Clone(), m_DataTable.Select(FilteredDataGridView.CurrentFilter),
              HtmlStyle)
            { Icon = ParentForm?.Icon };
          m_HierarchyDisplay.ShowWithFont(this);
          m_HierarchyDisplay.FormClosed += (_, _) => this.SafeInvoke(() => m_ToolStripButtonHierarchy.Enabled = true);
        }
        catch (Exception ex)
        {
          ParentForm.ShowError(ex);
        }
      }, ParentForm);
      m_ToolStripButtonHierarchy.Enabled = false;
    }

    /// <summary>
    ///   Handles the Click event of the buttonValues control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ButtonUniqueValues_Click(object? sender, EventArgs e)
    {
      if (FilteredDataGridView.Columns.Count <= 0)
        return;
      m_ToolStripButtonUniqueValues.RunWithHourglass(() =>
      {
        try
        {
          var columnName = FilteredDataGridView.CurrentCell != null
            ? FilteredDataGridView.Columns[FilteredDataGridView.CurrentCell.ColumnIndex].Name
            : FilteredDataGridView.Columns[0].Name;
          m_FormUniqueDisplay?.Close();
          m_FormUniqueDisplay = new FormUniqueDisplay(
            m_DataTable.Clone(),
            m_DataTable.Select(FilteredDataGridView.CurrentFilter),
            columnName, HtmlStyle);
          m_FormUniqueDisplay.ShowWithFont(this);
        }
        catch (Exception ex)
        {
          ParentForm.ShowError(ex);
        }
      }, ParentForm);
    }

    private void OnSearchClear(object? sender, EventArgs e)
    {
      // Cancel the current search
      searchBackgroundWorker.CancelAsync();
      m_Search.Hide();
      FilteredDataGridView.HighlightText = string.Empty;
    }

    private void DataViewChanged(object? sender, EventArgs args)
    {
      if (!m_Search.Visible)
        return;
      OnSearchClear(sender, args);
    }

    private void DetailControl_KeyDown(object? sender, KeyEventArgs e)
    {
      if (!e.Control || e.KeyCode != Keys.F)
        return;
      m_Search.Visible = true;
      m_Search.Focus();
      e.Handled = true;
    }

    /// <summary>
    ///   Filters the columns.
    /// </summary>
    private void FilterColumns(FilterTypeEnum filterType)
    {
      if (filterType == FilterTypeEnum.All || filterType== FilterTypeEnum.None)
      {
        foreach (DataGridViewColumn col in FilteredDataGridView.Columns)
        {
          if (!col.Visible)
            col.Visible = true;
          // col.MinimumWidth = 64;
        }
        return;
      }

      var cols = m_FilterDataTable.GetColumns(filterType);
      foreach (DataGridViewColumn dgCol in FilteredDataGridView.Columns)
        dgCol.Visible = cols.Contains(dgCol.DataPropertyName);
    }

    /// <summary>
    ///   Called when search changes.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="SearchEventArgs" /> instance containing the event data.</param>
    private void OnSearchChanged(object? sender, SearchEventArgs e)
    {
      SearchText(e.SearchText);
    }

    /// <summary>
    ///   Called when search result number changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="SearchEventArgs" /> instance containing the event data.</param>
    private void OnSearchResultChanged(object? sender, SearchEventArgs e)
    {
      if (e.Result < 1 || e.Result > m_FoundCells.Count)
        return;
      FilteredDataGridView.SafeInvoke(
        () =>
        {
          try
          {
            FilteredDataGridView.HighlightText = e.SearchText;
            FilteredDataGridView.CurrentCell = m_FoundCells[e.Result - 1];
            FilteredDataGridView.InvalidateCell(FilteredDataGridView.CurrentCell);
          }
          catch
          {
            //  Non essential UI operation
          }
        });
    }


    /// <summary>
    ///   Sets the data source.
    /// </summary>
    public void RefreshDisplay(FilterTypeEnum filterType, CancellationToken cancellationToken)
    {
      var oldSortedColumn = FilteredDataGridView.SortedColumn?.DataPropertyName;
      var oldOrder = FilteredDataGridView.SortOrder;

      // Cancel the current search
      searchBackgroundWorker.CancelAsync();

      // Hide any showing search
      m_Search.Visible = false;

      var newDt = m_FilterDataTable.Filter(int.MaxValue, filterType, cancellationToken);

      if (ReferenceEquals(m_BindingSource.DataSource, newDt))
      {
        m_UpdateVisibility = true;
        return;
      }

      this.SafeInvokeNoHandleNeeded(() =>
      {
        // Now apply filter
        FilteredDataGridView.DataSource = null;

        m_BindingSource.DataSource = newDt;
        FilteredDataGridView.DataSource = m_BindingSource;

        FilterColumns(filterType);

        AutoResizeColumns(newDt);
        FilteredDataGridView.ColumnVisibilityChanged();
        FilteredDataGridView.SetRowHeight();

        if (oldOrder != SortOrder.None && !(oldSortedColumn is null || oldSortedColumn.Length == 0))
          Sort(oldSortedColumn,
            oldOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);

        var newIndex = filterType switch
        {
          FilterTypeEnum.ErrorsAndWarning => 1,
          FilterTypeEnum.ShowErrors => 2,
          FilterTypeEnum.ShowWarning => 3,
          FilterTypeEnum.None => 4,
          _ => 0
        };
        if (m_ToolStripComboBoxFilterType.SelectedIndex == newIndex)
          return;
        m_ToolStripComboBoxFilterType.SelectedIndexChanged -= ToolStripComboBoxFilterType_SelectedIndexChanged;
        m_ToolStripComboBoxFilterType.SelectedIndex = newIndex;
        m_ToolStripComboBoxFilterType.SelectedIndexChanged += ToolStripComboBoxFilterType_SelectedIndexChanged;
      });

      m_UpdateVisibility = true;
    }


    // ReSharper disable once MemberCanBePrivate.Global
    public async Task SafeCurrentFile(string fileName)
    {
      if (FilteredDataGridView.DataView is null)
        return;
      if (WriteSetting == null)
      {
        WriteSetting = new CsvFile(id: string.Empty, fileName: string.Empty);
        FileSetting?.CopyTo(WriteSetting);
      }

      var skippedLines = new StringBuilder();
      // in case we skipped lines read them as Header so we do not loose them
      if (WriteSetting.SkipRows >0 &&  FileSetting is IFileSettingPhysicalFile physSource && physSource.SkipRows > 0)
      {
#if NET5_0_OR_GREATER
        await
#endif
          using var iStream = new ImprovedStream(new SourceAccess(physSource.FullPath, true, "ReadSkippedRows"));
        using var sr = new ImprovedTextReader(iStream, physSource.CodePageId);
        for (var i = 0; i < physSource.SkipRows; i++)
          skippedLines.AppendLine(await sr.ReadLineAsync());
      }
      using var formProgress = new FormProgress("Writing file", true, m_CancellationToken);
      try
      {
        formProgress.ShowWithFont(this);
        BeforeFileStored?.Invoke(this, WriteSetting);

        var writer = new CsvFileWriter(FileSetting?.ID ?? string.Empty, fileName, WriteSetting.HasFieldHeader,
          WriteSetting.ValueFormatWrite,
          WriteSetting.CodePageId,
          WriteSetting.ByteOrderMark, WriteSetting.ColumnCollection, WriteSetting.KeepUnencrypted,
          WriteSetting.IdentifierInContainer,
          skippedLines.ToString(), WriteSetting.Footer, string.Empty, WriteSetting.NewLine,
          WriteSetting.FieldDelimiterChar,
          WriteSetting.FieldQualifierChar,
          WriteSetting.EscapePrefixChar,
          WriteSetting.NewLinePlaceholder, WriteSetting.DelimiterPlaceholder, WriteSetting.QualifierPlaceholder,
          WriteSetting.QualifyAlways, WriteSetting.QualifyOnlyIfNeeded, StandardTimeZoneAdjust.ChangeTimeZone,
          TimeZoneInfo.Local.Id, PgpHelper.GetKeyAndValidate(fileName, WriteSetting.KeyFile));

#if NET5_0_OR_GREATER
        await
#endif
        using var dt = new DataTableWrapper(
          FilteredDataGridView.DataView.ToTable(false,
            // Restrict to shown data
            FilteredDataGridView.Columns.Cast<DataGridViewColumn>()
              .Where(col => col.Visible && col.DataPropertyName.NoArtificialField())
              .OrderBy(col => col.DisplayIndex)
              .Select(col => col.DataPropertyName).ToArray()));
        // can not use filteredDataGridView.Columns directly
        await writer.WriteAsync(dt, formProgress.CancellationToken);
      }
      catch (Exception ex)
      {
        ParentForm.ShowError(ex);
      }
      finally
      {
        FileStored?.Invoke(this, WriteSetting);
      }
    }

    private async void ToolStripButtonStoreAsCsvAsync(object? sender, EventArgs e)
    {
      try
      {
        FileSystemUtils.SplitResult split;

        if (FileSetting is IFileSettingPhysicalFile settingPhysicalFile)
          split = FileSystemUtils.SplitPath(settingPhysicalFile.FullPath);
        else
          split = new FileSystemUtils.SplitResult(Directory.GetCurrentDirectory(),
            $"{FileSetting!.ID}.txt");

        var fileName = WindowsAPICodePackWrapper.Save(split.DirectoryName, "Delimited File",
          "Text file (*.txt)|*.txt|Comma delimited (*.csv)|*.csv|Tab delimited (*.tab;*.tsv)|*.tab;*.tsv|All files (*.*)|*.*",
          ".csv",
          false,
          split.FileName);

        if (fileName is null || fileName.Length == 0)
          return;

        await SafeCurrentFile(fileName);
      }
      catch (Exception ex)
      {
        ParentForm.ShowError(ex);
      }
    }

    public void ReStoreViewSetting(string fileName) => FilteredDataGridView.ReStoreViewSetting(fileName);


    private FilterTypeEnum GetCurrentFilter()
    {
      int index = 0;
      this.SafeInvoke(() => index = m_ToolStripComboBoxFilterType.SelectedIndex);
      if (index == 1)
        return FilterTypeEnum.ErrorsAndWarning;
      if (index == 2)
        return FilterTypeEnum.ShowErrors;
      if (index == 3)
        return FilterTypeEnum.ShowWarning;
      return index == 4 ? FilterTypeEnum.None : FilterTypeEnum.All;
    }

    private void ToolStripComboBoxFilterType_SelectedIndexChanged(object? sender, EventArgs e)
    {
      RefreshDisplay(GetCurrentFilter(), m_CancellationToken);
    }

    private async void ToolStripButtonLoadRemaining_Click(object? sender, EventArgs e)
    {
      if (m_SteppedDataTableLoader.EndOfFile)
        return;
      await m_ToolStripButtonLoadRemaining.RunWithHourglassAsync(async () =>
      {
        // ReSharper disable once LocalizableElement
        m_ToolStripLabelCount.Text = " loading...";

        using var formProgress = new FormProgress("Load more...", false, m_CancellationToken);
        formProgress.ShowWithFont(this);
        formProgress.Maximum = 100;

        await m_SteppedDataTableLoader.GetNextBatch(formProgress, TimeSpan.FromSeconds(60), true,
          dataTable => DataTable.Merge(dataTable),
          token => RefreshDisplay(GetCurrentFilter(), token), formProgress.CancellationToken);
      }, ParentForm);
    }

    private void TimerVisibility_Tick(object? sender, EventArgs e)
    {
      if (!m_UpdateVisibility)
        return;
      if (ActiveControl == m_ToolStripContainer)
      {
        if (!m_ToolStripComboBoxFilterType.Focused)
          m_ToolStripComboBoxFilterType.Focus();
        SendKeys.Send("{ESC}");
      }

      // do not do this again
      m_TimerVisibility.Enabled = false;
      m_UpdateVisibility = false;
      try
      {
        var source = (m_MenuDown) ? m_ToolStripTop : m_BindingNavigator;
        var target = (m_MenuDown) ? m_BindingNavigator : m_ToolStripTop;
        target.SuspendLayout();
        source.SuspendLayout();
        target.Font = Font;
        foreach (var item in m_ToolStripItems)
        {
          item.DisplayStyle = (m_MenuDown)
            ? ToolStripItemDisplayStyle.Image
            : ToolStripItemDisplayStyle.ImageAndText;
          if (source.Items.Contains(item))
            source.Items.Remove(item);
          if (target.Items.Contains(item))
            target.Items.Remove(item);
        }

        var hasData = m_SteppedDataTableLoader.EndOfFile && (m_DataTable.Rows.Count > 0);
        foreach (var item in m_ToolStripItems)
        {
          item.DisplayStyle = (m_MenuDown)
            ? ToolStripItemDisplayStyle.Image
            : ToolStripItemDisplayStyle.ImageAndText;
          target.Items.Add(item);
        }

        source.ResumeLayout(true);
        target.ResumeLayout(true);

        m_ToolStripContainer.TopToolStripPanelVisible = !m_MenuDown;

        // Need to set the control containing the buttons to visible Regular
        m_ToolStripButtonColumnLength.Visible = m_ShowButtons;
        m_ToolStripButtonDuplicates.Visible = m_ShowButtons;
        m_ToolStripButtonUniqueValues.Visible = m_ShowButtons;
        m_ToolStripButtonStore.Visible = m_ShowButtons && (FileSetting != null);
        m_ToolStripButtonSource.Visible = m_ShowButtons && (FileSetting is IFileSettingPhysicalFile);
        m_ToolStripButtonHierarchy.Visible = m_ShowButtons;

        m_ToolStripButtonUniqueValues.Enabled = hasData;
        m_ToolStripButtonDuplicates.Enabled = hasData;
        m_ToolStripButtonColumnLength.Enabled = hasData;
        m_ToolStripButtonHierarchy.Enabled = hasData;
        m_ToolStripButtonStore.Enabled = hasData;
        m_ToolStripButtonStore.Enabled = hasData;

        toolStripButtonMoveLastItem.Enabled = hasData;
        FilteredDataGridView.toolStripMenuItemFilterAdd.Enabled = hasData;

        m_ToolStripButtonLoadRemaining.Visible = !m_SteppedDataTableLoader.EndOfFile && (m_DataTable.Rows.Count > 0);

        m_ToolStripLabelCount.ForeColor =
          m_SteppedDataTableLoader.EndOfFile ? SystemColors.ControlText : SystemColors.MenuHighlight;
        m_ToolStripLabelCount.ToolTipText =
          m_SteppedDataTableLoader.EndOfFile ? "Total number of records" : "Total number of records (loaded so far)";

        m_ToolStripTop.Visible = m_ShowButtons;

        // Filter
        m_ToolStripComboBoxFilterType.Visible = m_ShowButtons && m_ShowFilter;
        m_ToolStripComboBoxFilterType.Enabled = hasData;
      }
      catch (InvalidOperationException exception)
      {
        Logger.Warning(exception, "Issue updating the UI");
        // ignore error in regards to cross thread issues, SafeBeginInvoke should have handled
        // this though
      }
      finally
      {
        // now you could run this again
        m_TimerVisibility.Enabled = true;
      }
    }

    private void DetailControl_ParentChanged(object sender, EventArgs e)
    {
      var frm = this.ParentForm;
      if (frm == null)
        return;
      if (!frm.KeyPreview)
        frm.KeyPreview = true;
      frm.KeyDown += DetailControl_KeyDown;
      Font = frm.Font;
    }

    private FormCsvTextDisplay? m_SourceDisplay;

    private void SourceDisplayClosed(object? sender, FormClosedEventArgs e)
    {
      m_SourceDisplay?.Dispose();
      m_SourceDisplay = null;
    }

    private async void DisplaySource_Click(object sender, EventArgs e)
    {
      if (m_SourceDisplay != null) return;
      if (FileSetting is not IFileSettingPhysicalFile phys)
        return;
      await m_ToolStripButtonSource.RunWithHourglassAsync(async () =>
      {
        m_ToolStripButtonSource.Enabled = false;
        m_SourceDisplay = new FormCsvTextDisplay(phys.FileName, null);
        m_SourceDisplay.ShowWithFont(this);
        m_SourceDisplay.FormClosed += SourceDisplayClosed;

        if (FileSetting is ICsvFile csv)
          await m_SourceDisplay.OpenFileAsync(false, csv.FieldQualifierChar, csv.FieldDelimiterChar, csv.EscapePrefixChar,
            csv.CodePageId,
            FileSetting.SkipRows, csv.CommentLine, m_CancellationToken);
        else
          await m_SourceDisplay.OpenFileAsync(FileSetting is IJsonFile, '\0', '\0', '\0', 65001, FileSetting.SkipRows, "",
            m_CancellationToken);
      }, ParentForm);
    }
  }
}