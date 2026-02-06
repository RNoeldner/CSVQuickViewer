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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
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

  // Token source for managing cancellation of longer running search operations to ensure
  // responsiveness when users initiate new searches or close the control.
  private readonly CancellationTokenSource m_ControlCancellation = new CancellationTokenSource();

  // Storing foundCells cells for search next / previous  
  private readonly SteppedDataTableLoader m_SteppedDataTableLoader;
  private readonly List<ToolStripItem> m_ToolStripItems = new List<ToolStripItem>();
  private readonly ICollection<string> m_UniqueFieldName = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
  private FilterDataTable? m_FilterDataTable;
  private FormDuplicatesDisplay? m_FormDuplicatesDisplay;
  private FormShowMaxLength? m_FormShowMaxLength;
  private FormUniqueDisplay? m_FormUniqueDisplay;
  private FormHierarchyDisplay? m_HierarchyDisplay;
  private bool m_IsSearching = false;
  private bool m_IsSyncing = false;
  private bool m_MenuDown;
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
    FilteredDataGridView.CancellationToken = m_ControlCancellation.Token;

    // Wire up synchronization ONCE here
    m_BindingSource.PositionChanged += BindingSource_PositionChanged;
    FilteredDataGridView.SelectionChanged += FilteredDataGridView_SelectionChanged;
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

  /// <summary>
  ///   Allows setting the data table, make sure you call 
  ///   RefreshDisplay after setting the DataTable to update the display
  /// </summary>
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public DataTable DataTable
  {
    get => FilteredDataGridView.DataTable;
    set
    {
      if (FilteredDataGridView.DataTable == value)
        return;
      FilteredDataGridView.DataTable= value;
      m_BindingSource.DataSource = value;
      m_FilterDataTable?.Dispose();
      m_FilterDataTable = new FilterDataTable(value);
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

  public string SearchText
  {
    get => m_Search.SearchText;
    set => m_Search.SearchText = value;
  }

  [Bindable(false)]
  [Browsable(true)]
  [Category("Appearance")]
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
      m_UniqueFieldName.Clear();
      // in case we do not have unique names and the table is not loaded, do nothing
      if (value is null || !value.Any())
        return;
      foreach (var name in value)
        m_UniqueFieldName.Add(name);
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

  /// <summary>
  /// Asynchronously finds the next or previous occurrence of the search text.
  /// </summary>
  /// <param name="forward">True to search forward; false to search backward.</param>
  public async Task FindNextAsync(bool forward)
  {
    // If a search is already running, do nothing and return
    if (m_IsSearching)
      return;
    // Use the class-level CancellationToken that is disposed with the control
    var token = m_ControlCancellation.Token;
    string searchText = m_Search.SearchText;
    FilteredDataGridView.HighlightText = searchText;

    if (string.IsNullOrWhiteSpace(searchText))
      return;
    try
    {
      m_IsSearching = true;
      await m_Search.RunWithHourglassAsync(async () =>
    {
      // 1. Get the current View (which already has sorting/filtering applied)
      var view = FilteredDataGridView.DataTable.DefaultView;
      int totalRows = view.Count;
      if (totalRows == 0) return;

      // 2. Pre-cache column metadata to avoid repeated UI lookups in the loop
      var columnMap = FilteredDataGridView.Columns.Cast<DataGridViewColumn>()
          .Where(c => c.Visible && !string.IsNullOrEmpty(c.DataPropertyName))
          .OrderBy(c => c.DisplayIndex)
          .Select(c => new
          {
            c.Index,
            c.DataPropertyName,
            c.DefaultCellStyle.Format
          })
          .ToList();

      if (columnMap.Count == 0) return;

      // 3. Determine starting positions
      int startRow = FilteredDataGridView.CurrentCell?.RowIndex ?? 0;
      int currentVisibleColIdx = columnMap.FindIndex(c => c.Index == (FilteredDataGridView.CurrentCell?.ColumnIndex ?? -1));
      int totalCells = totalRows * columnMap.Count;

      // 4. Iterate through data
      for (int i = 1; i <= totalCells; i++)
      {
        int step = forward ? i : -i;
        int flatIdx = (currentVisibleColIdx + step) % totalCells;
        if (flatIdx < 0) flatIdx += totalCells;

        int rowOffset = flatIdx / columnMap.Count;
        int colMapIdx = flatIdx % columnMap.Count;

        int targetRow = (startRow + rowOffset) % totalRows;
        var colInfo = columnMap[colMapIdx];

        // 5. High-speed Value Extraction
        var rawValue = view[targetRow][colInfo.DataPropertyName];
        if (rawValue == null || rawValue == DBNull.Value)
          continue;

        // Get the text that is displayed in the cell,
        // applying formatting for DateTime and IFormattable types
        var formattedValue = (rawValue is DateTime dateTime)
          ? StringConversion.DisplayDateTime(dateTime, CultureInfo.CurrentCulture)
            : (rawValue is IFormattable formattable && !string.IsNullOrWhiteSpace(colInfo.Format))
            // Handles Numeric based on the Grid's Column Format
            ? formattable.ToString(colInfo.Format, null)
            // Handles Guids and plain strings or any other type by calling ToString without format
            : rawValue.ToString() ?? string.Empty;

        // 6. Check for match
        if (formattedValue.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1)
        {
          var cell = FilteredDataGridView.Rows[targetRow].Cells[colInfo.Index];
          SetCurrentCell(cell);
          return;
        }
        // 7. Responsive yielding
        if (i % 500 == 0)
        {
          token.ThrowIfCancellationRequested();
          await Task.Delay(1, token);
        }
      }
    });
    }
    catch (OperationCanceledException)
    {
      // Gracefully handle the task being canceled via m_ControlCancellation
    }
    finally
    {
      // 3. Always release the lock so the next search can start (unless disposed)
      m_IsSearching = false;
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
      await RefreshDisplayAsync(filterType, progress.CancellationToken);
    }
    timerLoadRemain.Enabled=autoLoad;
  }

  private void BindingSource_PositionChanged(object? sender, EventArgs e)
  {
    if (m_IsSyncing || m_BindingSource.Position < 0) return;

    m_IsSyncing = true;
    try
    {
      // Sync BindingSource -> Grid
      if (FilteredDataGridView.CurrentRow?.Index != m_BindingSource.Position)
      {
        var oldColIndex = FilteredDataGridView.CurrentCell?.ColumnIndex ?? 0;
        FilteredDataGridView.CurrentCell = FilteredDataGridView.Rows[m_BindingSource.Position].Cells[oldColIndex];
      }
    }
    finally { m_IsSyncing = false; }
  }

  private void FilteredDataGridView_SelectionChanged(object? sender, EventArgs e)
  {
    if (m_IsSyncing || FilteredDataGridView.CurrentRow == null) return;

    m_IsSyncing = true;
    try
    {
      // Sync Grid -> BindingSource
      if (m_BindingSource.Position != FilteredDataGridView.CurrentRow.Index)
      {
        m_BindingSource.Position = FilteredDataGridView.CurrentRow.Index;
      }
    }
    finally { m_IsSyncing = false; }
  }

  /// <summary>
  /// Makes sure the data is displayed according to the filter type, e.g. show only errors or warnings
  /// </summary>
  /// <param name="filterType">The type of filter we want to see</param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public async Task RefreshDisplayAsync(RowFilterTypeEnum filterType, CancellationToken cancellationToken)
  {
    if (IsDisposed || m_FilterDataTable is null)
      return;

    // Combined token to respect both specific operation cancellation and control disposal
    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, m_ControlCancellation.Token);

    var oldSortedColumn = FilteredDataGridView.SortedColumn?.DataPropertyName;
    var oldOrder = FilteredDataGridView.SortOrder;

    // Cancel the current search
    OnSearchClear(this, EventArgs.Empty);
    var newDt = await m_FilterDataTable.FilterAsync(int.MaxValue, filterType, linkedCts.Token);
    if (FilteredDataGridView.DataTable == newDt)
    {
      m_UpdateVisibility = true;
      return;
    }

    this.SafeInvokeNoHandleNeeded(() =>
    {
      // Now apply filter
      FilteredDataGridView.DataTable = newDt;
      m_BindingSource.DataSource = newDt;

      FilterColumns(filterType);

      if (oldOrder != SortOrder.None && !(oldSortedColumn is null || oldSortedColumn.Length == 0))
        Sort(oldSortedColumn, oldOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);

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
      m_ControlCancellation.Dispose();
      components?.Dispose();
      m_FilterDataTable?.Dispose();
      m_FormShowMaxLength?.Dispose();
      m_FormDuplicatesDisplay?.Dispose();
      m_FormUniqueDisplay?.Dispose();
      m_HierarchyDisplay?.Dispose();
      m_SteppedDataTableLoader?.Dispose();
    }
    base.Dispose(disposing);
  }

  protected override void OnHandleDestroyed(EventArgs e)
  {
    // Trigger cancellation as soon as the UI handle is gone
    // This stops FindNextAsync and LoadBatch immediately.
    m_ControlCancellation.Cancel();
    base.OnHandleDestroyed(e);
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
        new FormShowMaxLength(FilteredDataGridView.DataTable, FilteredDataGridView.DataTable.Select(FilteredDataGridView.CurrentFilter), visible,
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
          new FormDuplicatesDisplay(FilteredDataGridView.DataTable.Clone(), FilteredDataGridView.DataTable.Select(FilteredDataGridView.CurrentFilter),
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
          new FormHierarchyDisplay(FilteredDataGridView.DataTable.Clone(), FilteredDataGridView.DataTable.Select(FilteredDataGridView.CurrentFilter),
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
        m_FormUniqueDisplay = new FormUniqueDisplay(FilteredDataGridView.DataTable.Clone(),
          FilteredDataGridView.DataTable.Select(FilteredDataGridView.CurrentFilter),
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

  private void DetailControl_FontChanged(object? sender, EventArgs e)
  {
    this.SafeInvoke(() =>
    {
      FilteredDataGridView.Font = Font;
      m_BindingNavigator.Font = Font;
      m_ToolStripTop.Font = Font;
    });
  }

  /// <summary>
  /// Handles control-level keyboard shortcuts for search navigation and visibility.
  /// Supports Developer standards (F3/Shift+F3), Grid standards (Alt+N/Alt+P), 
  /// and common Find behaviors (Ctrl+F).
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">A <see cref="KeyEventArgs"/> containing the key data.</param>
  private async void DetailControl_KeyDown(object? sender, KeyEventArgs e)
  {
    // Handle Ctrl+F, F3, or Alt+N (Search / Search Next) 
    if ((e.Control && e.KeyCode == Keys.F) ||
                        (e.KeyCode == Keys.F3 && !e.Shift && !e.Control && !e.Alt) ||
                        (e.Alt && e.KeyCode == Keys.N))
    {
      e.Handled = true;
      e.SuppressKeyPress = true; // Prevents the 'ding' sound

      if (!m_Search.Visible)
      {
        m_Search.Visible = true;
        m_Search.Focus();
      }
      else
      {
        // If already open and has text, jump to next occurrence
        await FindNextAsync(true);
      }
      return;
    }

    // Handle Shift+F3 or Alt+P (Search Previous)
    if (m_Search.Visible  && ((e.Shift && e.KeyCode == Keys.F3) ||
                            (e.Alt && e.KeyCode == Keys.P)))
    {
      e.Handled = true;
      e.SuppressKeyPress = true;
      await FindNextAsync(false);
    }
  }

  /// <summary>
  /// Occurs when the <see cref="Parent"/> property of the control is changed (e.g., when the control is added to a form).
  /// Used to configure the parent <see cref="Form"/> to support global keyboard shortcuts.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">An <see cref="EventArgs"/> containing the event data.</param>
  private void DetailControl_ParentChanged(object sender, EventArgs e)
  {
    var frm = this.ParentForm;
    if (frm is null)
      return;
    if (!frm.KeyPreview)
    {
      frm.KeyPreview = true;
      frm.KeyDown += DetailControl_KeyDown;
    }
    Font = frm.Font;
  }

  /// <summary>
  /// Handles the Click event for the Display Source button.
  /// Invokes the <see cref="DisplaySourceAsync"/> delegate to show the raw data source.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">An <see cref="EventArgs"/> containing the event data.</param>
  private async void DisplaySource_Click(object sender, EventArgs e)
  {
    if (DisplaySourceAsync == null)
      return;

    await m_ToolStripButtonSource.RunWithHourglassAsync(async () =>
    {
      await DisplaySourceAsync.Invoke(m_ControlCancellation.Token);
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
    var m_ColumnsInView = m_FilterDataTable?.GetColumns(filterType) ?? Array.Empty<string>();
    foreach (DataGridViewColumn dgCol in FilteredDataGridView.Columns)
      dgCol.Visible = m_UniqueFieldName.Contains(dgCol.DataPropertyName) || m_ColumnsInView.Contains(dgCol.DataPropertyName);

  }

  /// <summary>
  /// Retrieves the current filter expression applied to the data view.
  /// </summary>
  /// <returns>
  /// A <see cref="string"/> representing the active filter expression; 
  /// returns an empty string or null if no filter is currently applied.
  /// </returns>
  /// <remarks>
  /// This value is typically synchronized with the <see cref="DataTable.DefaultView"/>'s RowFilter 
  /// property to ensure the UI and data layer remain consistent.
  /// </remarks>
  private RowFilterTypeEnum GetCurrentRowFilterType()
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

    FilteredDataGridView.Columns.Cast<DataGridViewColumn>().FirstOrDefault(col => col.DataPropertyName.Equals(dataColumnName, StringComparison.OrdinalIgnoreCase));

  /// <summary>
  /// Asynchronously loads a new batch of data from the source file and merges it into the existing <see cref="DataTable"/>.
  /// </summary>
  /// <param name="backgroundLoad">
  /// If <c>true</c>, the progress dialog is shown in the background (SendToBack) to allow continued UI interaction; 
  /// if <c>false</c>, the progress dialog is shown modally over the parent form.
  /// </param>
  /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
  /// <remarks>
  /// This method performs the following actions:
  /// <list type="bullet">
  ///   <item>Clears the current search highlights to prevent index mismatches during data merging.</item>
  ///   <item>Uses <see cref="m_ControlCancellation"/> to ensure the load operation is aborted if the control is disposed.</item>
  ///   <item>Invokes the <see cref="SteppedDataTableLoader"/> to fetch the next segment of data within a specific time limit.</item>
  ///   <item>Merges newly fetched rows into the main <see cref="DataTable"/> and refreshes the display filters.</item>
  /// </list>
  /// </remarks>
  private async Task LoadBatch(bool backgroundLoad)
  {
    if (IsDisposed)
      return;

    // Cancel the current search
    OnSearchClear(this, EventArgs.Empty);

    var timeSpan = backgroundLoad ? TimeSpan.MaxValue : TimeSpan.FromSeconds(5);

    // Pass in Control cancellation into the progress dialog so it can also
    // trigger cancellation if the control is disposed while loading
    using var progress = new FormProgress("Load more...", m_ControlCancellation.Token);
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
        FilteredDataGridView.DataTable.Merge(newDt, false, MissingSchemaAction.Error);
      }

      await RefreshDisplayAsync(GetCurrentRowFilterType(), progress.CancellationToken);
    }
    finally
    {
      m_ToolStripButtonLoadRemaining.Enabled = true;
    }
  }

  /// <summary>
  /// Clears the search and cancels any running search operation.
  /// </summary>
  private void OnSearchClear(object? sender, EventArgs e)
  {
    m_Search.Hide();
    FilteredDataGridView.HighlightText = string.Empty;
    FilteredDataGridView.SafeInvoke(FilteredDataGridView.Invalidate);
  }

  private async void OnSearchNext(object? sender, EventArgs e)
  {
    if (m_IsSearching)
      return;
    await FindNextAsync(true);
  }

  private async void OnSearchPrev(object? sender, EventArgs e)
  {
    if (m_IsSearching)
      return;
    await FindNextAsync(false);
  }

  /// <summary>
  /// Selects the specified cell and ensures it is scrolled into prominent view.
  /// </summary>
  private void SetCurrentCell(DataGridViewCell cell)
  {
    FilteredDataGridView.CurrentCell = cell;

    // --- Vertical Scrolling (Center the row) ---
    int visibleRows = FilteredDataGridView.DisplayedRowCount(false);
    int firstRow = FilteredDataGridView.FirstDisplayedScrollingRowIndex;

    // Use a small buffer (e.g., 2 rows) so it doesn't wait until the very last pixel to scroll
    if (cell.RowIndex <= firstRow || cell.RowIndex >= (firstRow + visibleRows - 1))
    {
      int targetRow = Math.Max(0, cell.RowIndex - (visibleRows / 2));
      FilteredDataGridView.FirstDisplayedScrollingRowIndex = targetRow;
    }

    // --- Horizontal Scrolling (The "Far Right" Fix) ---
    // Get the display rectangle of the cell relative to the grid's client area
    Rectangle cellRect = FilteredDataGridView.GetCellDisplayRectangle(cell.ColumnIndex, cell.RowIndex, false);
    Rectangle gridRect = FilteredDataGridView.ClientRectangle;

    // Check if the cell's right edge is outside the grid's right edge
    // OR if the cell's left edge is obscured by the row headers
    bool isPartiallyHiddenRight = (cellRect.Right > gridRect.Right);
    bool isPartiallyHiddenLeft = (cellRect.Left < FilteredDataGridView.RowHeadersWidth);

    if (isPartiallyHiddenRight || isPartiallyHiddenLeft || !cell.OwningColumn.Displayed)
    {
      try
      {
        // Set this column as the first visible one to ensure it's fully on screen
        FilteredDataGridView.FirstDisplayedScrollingColumnIndex = cell.ColumnIndex;
      }
      catch (Exception)
      {
        // Fallback for frozen columns or index errors
      }
    }
  }

  private void timerLoadRemain_Tick(object? sender, EventArgs e)
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

      var hasData = EndOfFile && (FilteredDataGridView.DataTable.Rows.Count > 0);
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

      m_ToolStripButtonLoadRemaining.Visible = !EndOfFile && (FilteredDataGridView.DataTable.Rows.Count > 0);

      m_ToolStripLabelCount.ForeColor = EndOfFile ? SystemColors.ControlText : SystemColors.MenuHighlight;
      m_ToolStripLabelCount.ToolTipText = EndOfFile ? "Total number of records"
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
  /// <summary>
  /// Asynchronously exports the current filtered and sorted view of the data to a CSV file.
  /// </summary>
  private async void ToolStripButtonStoreAsCsvAsync(object? sender, EventArgs e)
  {
    if (WriteFileAsync == null)
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
      await WriteFileAsync.Invoke(m_ControlCancellation.Token, wrapper);


    }, ParentForm);
  }

  /// <summary>
  /// Triggers a display refresh when the filter type selection changes.
  /// </summary>
  private async void ToolStripComboBoxFilterType_SelectedIndexChanged(object? sender, EventArgs e)
    => await RefreshDisplayAsync(GetCurrentRowFilterType(), m_ControlCancellation.Token);
}