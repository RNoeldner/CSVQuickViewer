/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvTools;

/// <inheritdoc cref="UserControl" />
/// <summary>
///   Windows from to show detail information for a dataTable
/// </summary>
public sealed partial class DetailControl : UserControl
{
  /// <summary>
  /// Async Action to be invoked when Display Source Button is pressed
  /// </summary>
  public Func<CancellationToken, Task>? DisplaySourceAsync;

  /// <summary>
  /// Async Action to be invoked when Write File Button is pressed
  /// </summary>
  public Func<CancellationToken, IFileReader, Task>? WriteFileAsync;

  // Storing foundCells cells for search next / previous
  private List<(int rowIndex, int colIndex)> m_FoundCells = new List<(int rowIndex, int colIndex)>();
  private readonly SteppedDataTableLoader m_SteppedDataTableLoader;
  private readonly List<ToolStripItem> m_ToolStripItems = new List<ToolStripItem>();
  private DataTable m_DataTable = new DataTable();
  private FilterDataTable m_FilterDataTable = new FilterDataTable(new DataTable());
  private FormDuplicatesDisplay? m_FormDuplicatesDisplay;
  private FormShowMaxLength? m_FormShowMaxLength;
  private FormUniqueDisplay? m_FormUniqueDisplay;
  private FormHierarchyDisplay? m_HierarchyDisplay;
  private bool m_IsSyncing = false;
  private bool m_MenuDown;

  // Token source for managing cancellation of the current search
  private CancellationTokenSource? m_SearchCancellation;
  private bool m_ShowButtons = true;
  private bool m_ShowFilter = true;
  private bool m_UpdateVisibility = true;

  public DetailControl()
  {
    InitializeComponent();
    m_SteppedDataTableLoader = new SteppedDataTableLoader();
    foreach (var item in new ToolStripItem[] {m_ToolStripComboBoxFilterType,
               m_ToolStripButtonUniqueValues,
               m_ToolStripButtonDuplicates,
               m_ToolStripButtonHierarchy,
               m_ToolStripButtonColumnLength,
               m_ToolStripButtonSource,
               m_ToolStripButtonStore
             })
      m_ToolStripItems.Add(item);
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
    private get => FilteredDataGridView.CancellationToken;
    set => FilteredDataGridView.CancellationToken = value;
  }

  /// <summary>
  ///   Allows setting the data table
  /// </summary>
  /// <value>The data table.</value>
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public DataTable DataTable
  {
    get => m_DataTable;
    set
    {
      if (m_DataTable == value)
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

  public bool EndOfFile => m_SteppedDataTableLoader.EndOfFile;

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
  ///   Gets or sets the HTML style.
  /// </summary>
  /// <value>The HTML style.</value>
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Bindable(false)]
  [Browsable(false)]
  public HtmlStyle HtmlStyle { get => FilteredDataGridView.HtmlStyle; set => FilteredDataGridView.HtmlStyle = value; }

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

  //public EventHandler<IFileSettingPhysicalFile>? BeforeFileStored;
  //public EventHandler<IFileSettingPhysicalFile>? FileStored;
  [Bindable(false)]
  [Browsable(true)]
  [DefaultValue(500)]
  public int ShowButtonAtLength
  {
    get => FilteredDataGridView.ShowButtonAtLength;
    set => FilteredDataGridView.ShowButtonAtLength = value;
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
      // in case we do not have unique names and the table is not loaded, do nothing
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

  public string GetViewStatus() => FilteredDataGridView.GetViewStatus;

  /// <summary>
  /// Asynchronously loads a CSV or data setting into the detail control and updates the display.
  /// Optionally triggers background loading of remaining data if <paramref name="autoLoad"/> is true.
  /// </summary>
  /// <param name="fileSetting">The file setting describing the source and format of the data.</param>
  /// <param name="durationInitial">The initial duration used for stepped loading operations.</param>
  /// <param name="autoLoad">
  /// If true, automatically triggers loading of remaining data in the background after the initial load
  /// with a short delay to ensure the control has finished rendering.
  /// </param>
  /// <param name="filterType">Specifies the filter type to apply when refreshing the display.</param>
  /// <param name="progress">A progress reporter to receive progress updates during loading.</param>
  /// <param name="addWarning">
  /// Optional event handler to receive warnings that may occur during loading.
  /// </param>
  /// <remarks>
  /// The background load triggered by <paramref name="autoLoad"/> is asynchronous but intentionally not awaited,
  /// allowing it to run without blocking the UI. A 500ms delay ensures the UI is ready before starting the background load.
  /// </remarks>
  public async Task LoadSettingAsync(IFileSetting fileSetting, TimeSpan durationInitial, bool autoLoad,
    RowFilterTypeEnum filterType, IProgressWithCancellation progress,
    EventHandler<WarningEventArgs>? addWarning)
  {
    try
    {
      // Need to set FileSetting so we can change Formats
      FilteredDataGridView.FileSetting = fileSetting;

      m_ToolStripLabelCount.Text = " loading...";
      DataTable = await m_SteppedDataTableLoader.StartAsync(fileSetting, durationInitial, progress, addWarning);
    }
    finally
    {
      RefreshDisplay(filterType, progress.CancellationToken);
    }
    timerLoadRemain.Enabled=autoLoad;
  }


  /// <inheritdoc />
  /// <summary>
  ///   Initializes a new instance of the <see cref="DetailControl" /> class.
  /// </summary>
#if !NETFRAMEWORK
  [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
  /// <summary>
  ///   Sets the data source.
  /// </summary>
  public void RefreshDisplay(RowFilterTypeEnum filterType, CancellationToken cancellationToken)
  {
    var oldSortedColumn = FilteredDataGridView.SortedColumn?.DataPropertyName;
    var oldOrder = FilteredDataGridView.SortOrder;

    // Cancel the current search
    OnSearchClear(this, EventArgs.Empty);

    var newDt = m_FilterDataTable.Filter(int.MaxValue, filterType, cancellationToken);

    if (m_BindingSource.DataSource == newDt)
    {
      m_UpdateVisibility = true;
      return;
    }

    this.SafeInvokeNoHandleNeeded(() =>
    {
      // Now apply filter
      FilteredDataGridView.DataTable = newDt;
      m_BindingSource.DataSource = newDt;

      m_BindingSource.PositionChanged += (s, e) =>
      {
        if (m_IsSyncing) return;
        m_IsSyncing = true;
        try
        {
          if (m_BindingSource.Position >= 0)
          {
            var oldColIndex = FilteredDataGridView.CurrentCell.ColumnIndex;
            FilteredDataGridView.CurrentCell = FilteredDataGridView.Rows[m_BindingSource.Position].Cells[oldColIndex];
          }
        }
        finally { m_IsSyncing = false; }
      };

      FilteredDataGridView.SelectionChanged += (s, e) =>
      {
        if (m_IsSyncing) return;
        m_IsSyncing = true;
        try
        {
          if (FilteredDataGridView.CurrentRow != null)
            m_BindingSource.Position = FilteredDataGridView.CurrentRow.Index;
        }
        finally { m_IsSyncing = false; }
      };
      FilterColumns(filterType);

      if (oldOrder != SortOrder.None && !(oldSortedColumn is null || oldSortedColumn.Length == 0))
        Sort(oldSortedColumn,
          oldOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);

      var newIndex = filterType switch
      {
        RowFilterTypeEnum.ErrorsAndWarning => 1,
        RowFilterTypeEnum.ShowErrors => 2,
        RowFilterTypeEnum.ShowWarning => 3,
        RowFilterTypeEnum.None => 4,
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

  public void ReStoreViewSetting(string fileName) => FilteredDataGridView.ReStoreViewSetting(fileName);

  /// <summary>
  /// Performs a case-insensitive search across all visible columns.
  /// Cancels any ongoing search before starting a new one.
  /// Limits the result set to the first 1000 matching cells to
  /// prevent excessive processing.
  /// Runs asynchronously to avoid UI freezes and updates the
  /// highlight and result count upon completion.
  /// </summary>
  public async Task SearchTextAsync(string searchText)
  {
    // Cancel any ongoing search
    m_SearchCancellation?.Cancel();
    m_SearchCancellation?.Dispose();
    m_SearchCancellation = new CancellationTokenSource();

    FilteredDataGridView.HighlightText = searchText;
    m_FoundCells.Clear();
    m_Search.Results = 0;

    if (string.IsNullOrWhiteSpace(searchText))
      return;
    try
    {
      m_FoundCells = await Task.Run(() =>
      {
        var foundPositions = new List<(int rowIndex, int colIndex)>();
        var view = FilteredDataGridView.DataTable?.DefaultView;
        if (view == null) return foundPositions;

        // Pre-calculate visible column mappings
        var visibleCols = FilteredDataGridView.Columns.Cast<DataGridViewColumn>()
            .Where(c => c.Visible && !string.IsNullOrEmpty(c.DataPropertyName))
            .ToList();

        for (int rowIndex = 0; rowIndex < view.Count; rowIndex++)
        {
          // Check cancellation frequently
          if (rowIndex % 50 == 0)
            m_SearchCancellation.Token.ThrowIfCancellationRequested();

          foreach (var column in visibleCols)
          {
            var cellValue = view[rowIndex][column.DataPropertyName]?.ToString();
            if (cellValue != null && cellValue.Contains(searchText, StringComparison.OrdinalIgnoreCase))
              foundPositions.Add((rowIndex, column.Index));
          }
          if (foundPositions.Count >= 1000)
            break;
        }
        return foundPositions;
      });

      m_SearchCancellation.Token.ThrowIfCancellationRequested();
      // Apply result back to UI, but only for visible cells
      // Back on UI Thread: Convert positions to actual Cell objects
      m_Search.Results = m_FoundCells.Count;

      if (m_FoundCells.Count > 0)
        OnSearchResultChanged(this, new SearchEventArgs(searchText, 0));

      this.SafeInvoke(FilteredDataGridView.Invalidate);
    }
    catch (OperationCanceledException) { /* Ignore */ }
  }


  /// <summary>
  /// Set a filter on a data column
  /// </summary>
  /// <param name="dataColumnName">The name of the data column</param>
  /// <param name="op">The operator for the filter e.G. =</param>
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
          if (col is null) return;
          var columnFilters = FilteredDataGridView.GetColumnFilter(col.Index);

          columnFilters.Operator = op;
          if (value is DateTime dateTime)
            columnFilters.ValueDateTime = dateTime;
          else
            columnFilters.ValueText = Convert.ToString(value) ?? string.Empty;
          columnFilters.ApplyFilter();
        }
      );
    }
    catch (Exception ex)
    {
      Debug.WriteLine($"Processing Filter {ex.Message}");
    }
  }

  public void SetViewStatus(string newStatus) => FilteredDataGridView.SetViewStatus(newStatus);

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
      Debug.WriteLine($"Processing Sorting {ex.Message}");
    }
  }

  /// <inheritdoc />
  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      m_SearchCancellation?.Dispose();
      components?.Dispose();
      m_FormShowMaxLength?.Dispose();
      m_FormDuplicatesDisplay?.Dispose();
      m_FormUniqueDisplay?.Dispose();
      m_HierarchyDisplay?.Dispose();
      m_DataTable = null!;
      m_FilterDataTable?.Dispose();
      m_FilterDataTable = null!;
      m_SteppedDataTableLoader?.Dispose();
    }
    base.Dispose(disposing);
  }

  /// <summary>
  ///   Handles the Click event of the buttonTableSchema control.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
  private void ButtonColumnLength_Click(object? sender, EventArgs e)
  {
    if (CancelMissingData()) return;

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
      m_FormShowMaxLength.FormClosed += (o, formClosedEventArgs) =>
        this.SafeInvoke(() => m_ToolStripButtonColumnLength.Enabled = true);
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
    if (CancelMissingData()) return;

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
          (o, formClosedEventArgs) => this.SafeInvoke(() => m_ToolStripButtonDuplicates.Enabled = true);
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
    if (CancelMissingData()) return;
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
        m_HierarchyDisplay.FormClosed += (o, formClosedEventArgs) =>
          this.SafeInvoke(() => m_ToolStripButtonHierarchy.Enabled = true);
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
    if (CancelMissingData()) return;
    m_ToolStripButtonUniqueValues.RunWithHourglass(() =>
    {
      try
      {
        var columnName = FilteredDataGridView.CurrentCell != null
          ? FilteredDataGridView.Columns[FilteredDataGridView.CurrentCell.ColumnIndex].Name
          : FilteredDataGridView.Columns[0].Name;
        m_FormUniqueDisplay?.Close();
        m_FormUniqueDisplay = new FormUniqueDisplay(m_DataTable.Clone(),
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

  private bool CancelMissingData()
  {
    if (FilteredDataGridView.Columns.Count <= 0)
      return true;

    if (EndOfFile)
      return false;

    var result = MessageBox.Show(
      "Some data is not yet loaded from file.\nOnly already processed data will be used.",
      "Data", MessageBoxButtons.OKCancel, MessageBoxIcon.Information,
      MessageBoxDefaultButton.Button1, 3);
    return result == DialogResult.Cancel;
  }

  private void DataViewChanged(object? sender, EventArgs args)
  {
    if (!m_Search.Visible)
      return;
    OnSearchClear(sender, args);
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

  private void DetailControl_KeyDown(object? sender, KeyEventArgs e)
  {
    if (!e.Control || e.KeyCode != Keys.F)
      return;
    m_Search.Visible = true;
    m_Search.Focus();
    e.Handled = true;
  }

  private void DetailControl_ParentChanged(object sender, EventArgs e)
  {
    var frm = this.ParentForm;
    if (frm is null)
      return;
    if (!frm.KeyPreview)
      frm.KeyPreview = true;
    frm.KeyDown += DetailControl_KeyDown;
    Font = frm.Font;
  }

  private async void DisplaySource_Click(object sender, EventArgs e)
  {
    if (DisplaySourceAsync == null)
      return;

    await m_ToolStripButtonSource.RunWithHourglassAsync(async () =>
    {
      await DisplaySourceAsync.Invoke(CancellationToken);
    }, ParentForm);
  }

  /// <summary>
  ///   Filters the columns.
  /// </summary>
  private void FilterColumns(RowFilterTypeEnum filterType)
  {
    if (filterType == RowFilterTypeEnum.All || filterType == RowFilterTypeEnum.None)
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

  private RowFilterTypeEnum GetCurrentFilter()
  {
    int index = 0;
    this.SafeInvoke(() => index = m_ToolStripComboBoxFilterType.SelectedIndex);
    return index switch
    {
      1 => RowFilterTypeEnum.ErrorsAndWarning,
      2 => RowFilterTypeEnum.ShowErrors,
      3 => RowFilterTypeEnum.ShowWarning,
      4 => RowFilterTypeEnum.None,
      _ => RowFilterTypeEnum.All
    };
  }

  private DataGridViewColumn? GetViewColumn(string dataColumnName) =>
                              FilteredDataGridView.Columns.Cast<DataGridViewColumn>().FirstOrDefault(col =>
      col.DataPropertyName.Equals(dataColumnName, StringComparison.OrdinalIgnoreCase));
  private async Task LoadBatch(bool backgroundLoad)
  {
    // Cancel the current search
    OnSearchClear(this, EventArgs.Empty);

    var timeSpan = backgroundLoad ? TimeSpan.MaxValue : TimeSpan.FromSeconds(5);
    using var progress = new FormProgress("Load more...", CancellationToken);
    try
    {
      m_ToolStripButtonLoadRemaining.Enabled = false;
      if (backgroundLoad)
      {
        // Show but one can go back to the main form
        progress.Show();
        progress.SendToBack();
      }
      else
        // Show over the main form
        progress.Show(FindForm());

      var newDt = await m_SteppedDataTableLoader.GetNextBatch(timeSpan, progress);
      if (newDt.Rows.Count > 0)
      {
        progress.Report($"Populating the data grid with the new {newDt.Rows.Count:N0} new records.\nThis may take a moment…");
        DataTable.Merge(newDt, false, MissingSchemaAction.Error);
      }

      RefreshDisplay(GetCurrentFilter(), progress.CancellationToken);
    }
    finally
    {
      m_ToolStripButtonLoadRemaining.Enabled = true;
    }
  }

  /// <summary>
  ///   Called when search changes.
  /// </summary>
  /// <param name="sender">The sender.</param>
  /// <param name="e">The <see cref="SearchEventArgs" /> instance containing the event data.</param>
  private async void OnSearchChanged(object? sender, SearchEventArgs e)
  {
    await SearchTextAsync(e.SearchText);
  }

  /// <summary>
  /// Clears the search and cancels any running search operation.
  /// </summary>
  private void OnSearchClear(object? sender, EventArgs e)
  {
    // Cancel the current search
    m_SearchCancellation?.Cancel();
    m_Search.Hide();
    FilteredDataGridView.HighlightText = string.Empty;
    FilteredDataGridView.SafeInvoke(FilteredDataGridView.Invalidate);
  }

  /// <summary>
  ///   Called when search result number changed.
  /// </summary>
  /// <param name="sender">The sender.</param>
  /// <param name="e">The <see cref="SearchEventArgs" /> instance containing the event data.</param>
  private void OnSearchResultChanged(object? sender, SearchEventArgs e)
  {
    // Validate bounds
    if (e.Result < 0 || e.Result >= m_FoundCells.Count) return;

    FilteredDataGridView.SafeBeginInvoke(() =>
    {
      try
      {
        var targetPos = m_FoundCells[e.Result];

        // Safety Check: Ensure the index is still valid for the current grid state
        if (targetPos.rowIndex >= FilteredDataGridView.RowCount ||
            targetPos.colIndex >= FilteredDataGridView.ColumnCount)
        {
          return;
        }

        var cell = FilteredDataGridView.Rows[targetPos.rowIndex].Cells[targetPos.colIndex];

        // Final check: Does the cell still contain the text? 
        // (In case the user changed filters/sorts in the meantime)
        if (cell.FormattedValue?.ToString()?.Contains(e.SearchText, StringComparison.OrdinalIgnoreCase) == true)
        {
          FilteredDataGridView.CurrentCell = cell;
        }
      }
      catch (Exception ex)
      {
        Logger.Warning($"Navigation failed: {ex.Message}");
      }
    });

    FilteredDataGridView.HighlightText = e.SearchText;
  }

  private void timerLoadRemain_Tick(object sender, EventArgs e)
  {
    timerLoadRemain.Enabled=false;
    _ = LoadBatch(true);
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

      var hasData = EndOfFile && (m_DataTable.Rows.Count > 0);
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
      m_ToolStripButtonStore.Visible = m_ShowButtons && (WriteFileAsync != null);
      m_ToolStripButtonSource.Visible = m_ShowButtons && (DisplaySourceAsync != null);
      m_ToolStripButtonHierarchy.Visible = m_ShowButtons;

      m_ToolStripButtonStore.Enabled = hasData;
      toolStripButtonMoveLastItem.Enabled = hasData;
      FilteredDataGridView.DataLoaded = hasData;

      m_ToolStripButtonLoadRemaining.Visible = !EndOfFile && (m_DataTable.Rows.Count > 0);

      m_ToolStripLabelCount.ForeColor = EndOfFile ? SystemColors.ControlText : SystemColors.MenuHighlight;
      m_ToolStripLabelCount.ToolTipText = EndOfFile
        ? "Total number of records"
        : "Total number of records (loaded so far)";

      m_ToolStripTop.Visible = m_ShowButtons;

      // Filter
      m_ToolStripComboBoxFilterType.Visible = m_ShowButtons && m_ShowFilter;
      m_ToolStripComboBoxFilterType.Enabled = hasData;
    }
    catch (InvalidOperationException ex)
    {
      Debug.WriteLine($"Issue updating UI: {ex.Message}");
      // ignore error in regard to cross thread issues, SafeBeginInvoke should have handled
      // this though
    }
    finally
    {
      // now you could run this again
      m_TimerVisibility.Enabled = true;
    }
  }

  private void ToolStripButtonLoadRemaining_Click(object? sender, EventArgs e)
  {
    if (EndOfFile)
      return;
    _ = LoadBatch(false);
  }

  private async void ToolStripButtonStoreAsCsvAsync(object? sender, EventArgs e)
  {
    if (WriteFileAsync == null || FilteredDataGridView.DataTable == null)
      return;

    await m_ToolStripButtonStore.RunWithHourglassAsync(async () =>
    {
      // Extract the column names first
      string[] columnsToExport = FilteredDataGridView.Columns.Cast<DataGridViewColumn>()
          .Where(col => col.Visible && col.DataPropertyName.NoArtificialField())
          .OrderBy(col => col.DisplayIndex)
          .Select(col => col.DataPropertyName)
          .ToArray();
      // Create the snapshot of the filtered/sorted data
      using var exportTable = FilteredDataGridView.DataTable.DefaultView.ToTable(false, columnsToExport);

      using var wrapper = new DataTableWrapper(exportTable);

      // Use the wrapper to satisfy the ICsvReader requirement
      await WriteFileAsync.Invoke(CancellationToken, wrapper);


    }, ParentForm);
  }

  private void ToolStripComboBoxFilterType_SelectedIndexChanged(object? sender, EventArgs e)
  {
    RefreshDisplay(GetCurrentFilter(), CancellationToken);
  }
}