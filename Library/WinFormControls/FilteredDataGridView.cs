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
using System.Windows.Forms;

namespace CsvTools;

/// <summary>
/// An enhanced <see cref="DataGridView"/> providing:
/// <list type="bullet">
///   <item>Interactive column filtering</item>
///   <item>Automatic column sizing based on content</item>
///   <item>Highlighting and multi-line text support</item>
///   <item>Large-text expansion via button cells</item>
///   <item>Improved copy/paste behavior</item>
/// </list>
/// This control is intended for viewing CSV or structured tabular data
/// with high usability and custom formatting options.
/// </summary>
public partial class FilteredDataGridView : DataGridView
{
  private static int m_DefRowHeight = -1;
  // Keep track of the Filters per column (index)
  private readonly Dictionary<int, ColumnFilterLogic> m_FilterLogic = new Dictionary<int, ColumnFilterLogic>();
  private readonly Image m_ImgFilterIndicator;
  private DataTable m_DataTable = new DataTable();
  private bool m_Disposed;
  private IFileSetting? m_FileSetting;
  private int m_MenuItemColumnIndex;
  private int m_ShowButtonAtLength = 1000;

  /// <summary>
  /// Initializes a new instance of the <see cref="FilteredDataGridView"/> class.
  /// Sets up default behaviors for Virtual Mode and custom cell painting.
  /// </summary>
  public FilteredDataGridView()
  {
    InitializeComponent();
    FontChanged += PassOnFontChanges;

    Scroll += (o, e) => SetRowHeight();

    var resources = new ComponentResourceManager(typeof(FilteredDataGridView));
    m_ImgFilterIndicator = (resources.GetObject("filter_small") as Image) ??
                           throw new InvalidOperationException("Resource not found");

    VirtualMode = true;
    AutoGenerateColumns = false;
    AllowUserToAddRows = false;

    if (contextMenuStripHeader.LayoutSettings is TableLayoutSettings tableLayoutSettings)
      tableLayoutSettings.ColumnCount = 3;

    ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
    ColumnHeaderMouseClick += FilteredDataGridView_ColumnHeaderMouseClick;


    CellMouseClick += FilteredDataGridView_CellMouseClick;
    CellPainting += HighlightCellPainting;
    ColumnWidthChanged += FilteredDataGridView_ColumnWidthChanged;
    CellValueNeeded += FilteredDataGridView_CellValueNeeded;
    KeyDown += FilteredDataGridView_KeyDown;

    // Inside FilteredDataGridView.cs constructor or initialization logic
    DefaultCellStyle.ForeColor = Color.Black;
    DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

    Microsoft.Win32.SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

    FontChanged += (_, _) =>
    {
      AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
      SetRowHeight();
    };
  }

  private void FilteredDataGridView_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
  {
    if (e.Button != MouseButtons.Left || e.ColumnIndex < 0) return;

    var columnName = Columns[e.ColumnIndex].DataPropertyName;
    // 1. Determine direction: Toggle if already sorting this column
    var direction = (DataTable.DefaultView.Sort.StartsWith("[" + columnName + "] ASC", StringComparison.Ordinal))
        ? ListSortDirection.Descending : ListSortDirection.Ascending;

    Sort(Columns[e.ColumnIndex], direction);
  }

  public override void Sort(DataGridViewColumn dataGridViewColumn, ListSortDirection directionSort)
  {
    // 1. Apply directly to the DataView to avoid InvalidOperationException
    DataTable.DefaultView.Sort = $"[{dataGridViewColumn.DataPropertyName}] {(directionSort == ListSortDirection.Ascending ? "ASC" : "DESC")}";

    // 2. Clear glyphs from ALL columns first
    foreach (DataGridViewColumn col in Columns)
      col.HeaderCell.SortGlyphDirection = SortOrder.None;

    // 3. Set the Glyph for the active column only
    dataGridViewColumn.HeaderCell.SortGlyphDirection = directionSort == ListSortDirection.Ascending ? SortOrder.Ascending : SortOrder.Descending;

    // 4. Refresh the grid
    Invalidate();
  }


  /// <summary>
  /// Gets or sets the <see cref="CancellationToken"/> used to interrupt long-running operations like column width measurement.
  /// </summary>
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Bindable(false)]
  [Browsable(false)]
  public CancellationToken CancellationToken
  {
    get;
    set;
  } = CancellationToken.None;

  /// <summary>
  /// Gets the current RowFilter string applied to the <see cref="DataTable.DefaultView"/>.
  /// </summary>
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Bindable(false)]
  [Browsable(false)]
  public string CurrentFilter => DataTable.DefaultView.RowFilter ?? string.Empty;

  /// <summary>
  /// Gets or sets a value indicating whether data loading is complete. 
  /// Suppresses expensive UI updates when <c>false</c>.
  /// </summary>
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Bindable(false)]
  [Browsable(false)]
  public bool DataLoaded { get; set; } = true;


  /// <summary>
  /// Gets or sets the <see cref="DataTable"/> that provides data for this control.
  /// Updating this property triggers column generation and row count synchronization.
  /// </summary>
  [RefreshProperties(RefreshProperties.Repaint)]
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  public DataTable DataTable
  {
    get => m_DataTable;
    set
    {
      m_DataTable = value ?? new DataTable();
      GenerateDataGridViewColumn();
      base.RowCount = m_DataTable?.Rows.Count ?? 0; // Update the UI engine here!
      // OnDataSourceChanged(EventArgs.Empty);
    }
  }

  /// <summary>
  /// Gets or sets the file settings containing column metadata and formatting rules.
  /// </summary>
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Bindable(false)]
  [Browsable(false)]
  public IFileSetting? FileSetting
  {
    get => m_FileSetting;
    set
    {
      m_FileSetting = value;
      toolStripMenuItemCF.Enabled = m_FileSetting != null;
    }
  }

  /// <summary>
  /// Sets the settings for guessing data types during filling.
  /// </summary>
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Bindable(false)]
  [Browsable(false)]
  public FillGuessSettings? FillGuessSettings
  {
    private get;
    set;
  }

  /// <summary>
  /// Sets the number of columns to freeze at the left of the grid based on display order.
  /// </summary>
  public int FrozenColumns
  {
    set
    {
      foreach (DataGridViewColumn col in Columns)
        col.Frozen = false;

      var max = value;
      foreach (var col in Columns.OfType<DataGridViewColumn>().OrderBy(x => x.DisplayIndex))
        if (max-- > 0)
          col.Frozen = true;
        else
          break;
    }
  }

  /// <summary>
  /// Gets a JSON-serialized representation of the current view settings (columns, filters, sort).
  /// </summary>
  public string GetViewStatus => ViewSetting.StoreViewSetting(Columns, m_FilterLogic.Values, SortedColumn, SortOrder);

  /// <summary>
  /// Gets or sets the text string to be visually highlighted within cells.
  /// </summary>
  public string HighlightText { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the HTML style applied during copy/paste operations.
  /// </summary>
  [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
  [EditorBrowsable(EditorBrowsableState.Never)]
  [Bindable(false)]
  [Browsable(false)]
  public HtmlStyle HtmlStyle { get; set; } = HtmlStyle.Default;

  /// <summary>
  /// Gets or sets the character threshold for string cells. 
  /// Cells exceeding this length are converted to <see cref="DataGridViewButtonColumn"/> to prevent UI lag.
  /// </summary>
  [Bindable(true)]
  [Browsable(true)]
  [DefaultValue(500)]
  public int ShowButtonAtLength
  {
    get => m_ShowButtonAtLength;
    set
    {
      var newVal = value < 10 ? 10 : value;
      if (m_ShowButtonAtLength == newVal)
        return;
      m_ShowButtonAtLength = newVal;
      GenerateDataGridViewColumn();
    }
  }

  /// <summary>
  /// Aggregates all active column filters and applies them to the <see cref="DataTable.DefaultView"/>.
  /// Synchronizes the <see cref="DataGridView.RowCount"/> and raises <see cref="DataViewChanged"/>.
  /// </summary>
  public void ApplyFilters() =>
      this.RunWithHourglass(() =>
      {
        // 1. Generate the filter string from your ColumnFilterLogic dictionary
        var bindingSourceFilter = GetFilterExpression(-1);
        toolStripMenuItemFilterRemoveAllFilter.Enabled = bindingSourceFilter.Length > 0;

        // 2. Apply the filter to the DataView
        // Only update if the filter string has actually changed
        if (!bindingSourceFilter.Equals(DataTable.DefaultView.RowFilter, StringComparison.Ordinal))
        {
          DataTable.DefaultView.RowFilter = bindingSourceFilter;

          base.RowCount = DataTable.DefaultView.Count; // Sync count after filtering
                                                       // 4. Force the grid to refresh its visible cells
          Invalidate();
        }
      });

  /// <summary>
  /// Resizes column widths based on the actual content of the <see cref="DataTable"/> rather than just visible cells.
  /// </summary>
  /// <param name="autoSizeColumnsMode">The sizing mode.</param>
  public new void AutoResizeColumns(DataGridViewAutoSizeColumnsMode autoSizeColumnsMode)
  {
    foreach (DataColumn dataColumn in DataTable.Columns)
      foreach (DataGridViewColumn gridColumn in Columns)
        if (string.Equals(gridColumn.DataPropertyName, dataColumn.ColumnName, StringComparison.OrdinalIgnoreCase))
        {
          gridColumn.Width = GetColumnWith(dataColumn, DataTable.Rows) + 5;
          break;
        }
  }

  /// <summary>
  /// Sets a filter based on the value of the currently selected cell.
  /// </summary>
  public void FilterCurrentCell()
  {
    try
    {
      if (CurrentCell is null)
        return;
      var filter = m_FilterLogic[m_MenuItemColumnIndex];
      filter.SetFilter(CurrentCell.Value);
      ApplyFilters();
    }
    catch (Exception ex)
    {
      Logger.Warning("Error in FilterCurrentCell", ex);
    }
  }

  /// <summary>
  /// Retrieves the filter logic instance for a specific column.
  /// </summary>
  /// <param name="columnIndex">The zero-based index of the column.</param>
  /// <returns>The <see cref="ColumnFilterLogic"/> for the specified column.</returns>
  public ColumnFilterLogic GetColumnFilter(int columnIndex) => m_FilterLogic[columnIndex];

  /// <summary>
  /// Combines the filter expressions from all columns into a single string, optionally excluding one column.
  /// </summary>
  /// <param name="exclude">The index of the column to exclude from the expression, or -1 to include all.</param>
  /// <returns>A combined SQL-like filter statement.</returns>
  public string GetFilterExpression(int exclude) =>
    m_FilterLogic.Where(x => x.Key != exclude && x.Value.Active && !string.IsNullOrEmpty(x.Value.FilterExpression)).Select(x => x.Value.FilterExpression).Join("\nAND\n");

  /// <summary>
  /// Iterates through visible columns and hides those that contain only <see cref="DBNull"/> values across the entire dataset.
  /// </summary>
  /// <returns><c>true</c> if any columns were hidden; otherwise, <c>false</c>.</returns>
  public bool HideEmptyColumns()
  {
    if (Columns.Count == 0)
      return false;

    var hasChanges = false;
    foreach (DataGridViewColumn col in Columns)
      if (col.Visible)
      {
        var hasData = DataTable.DefaultView.Cast<DataRowView>()
          .Any(dataRow => dataRow[col.DataPropertyName] != DBNull.Value);
        if (!hasData && col.Visible)
          col.Visible = false;
        hasChanges = true;
      }

    return hasChanges;
  }

  /// <summary>
  /// Refreshes the UI by hiding empty columns and recalculating row heights.
  /// </summary>
  public void RefreshUI()
  {
    try
    {
      if (!HideEmptyColumns())
        return;
      SetRowHeight();
    }
    catch
    {
      // ignored
    }
  }

  /// <summary>
  /// Deactivates filters on all columns and refreshes the data view.
  /// </summary>
  public void RemoveAllFilter()
  {
    try
    {
      foreach (var toolStripFilter in m_FilterLogic.Values)
        toolStripFilter.Active = false;

      ApplyFilters();
    }
    catch
    {
      // ignored
    }
  }

  /// <summary>
  /// Restores view settings from a specified file.
  /// </summary>
  /// <param name="fileName">Path to the settings file.</param>
  public void ReStoreViewSetting(string fileName)
  {
    if (string.IsNullOrEmpty(fileName) || !FileSystemUtils.FileExists(fileName) || Columns.Count == 0)
      return;
    if (m_FileSetting is IFileSettingPhysicalFile basePhysical)
      basePhysical.ColumnFile = fileName;

    SetViewStatus(FileSystemUtils.ReadAllText(fileName));
  }

  /// <summary>
  /// Freezes or unfreezes a specific column. If freezing, the column is moved to the first available non-frozen position.
  /// </summary>
  /// <param name="colNum">The column index.</param>
  /// <param name="newStatus"><c>true</c> to freeze; <c>false</c> to unfreeze.</param>
  public void SetColumnFrozen(int colNum, bool newStatus)
  {
    if (newStatus)
    {
      var colFirstNoFrozen =
        (from col in Columns.OfType<DataGridViewColumn>().OrderBy(x => x.DisplayIndex)
         where !col.Frozen
         select col.DisplayIndex).FirstOrDefault();
      Columns[m_MenuItemColumnIndex].DisplayIndex = colFirstNoFrozen;
    }

    Columns[colNum].Frozen = newStatus;
  }

  /// <summary>
  /// Updates the visibility of multiple columns based on a dictionary of column names and boolean flags.
  /// </summary>
  /// <param name="items">Dictionary mapping column names to visibility status.</param>
  public void SetColumnVisibility(IDictionary<string, bool> items)
  {
    bool changes = false;
    foreach (KeyValuePair<string, bool> keyValuePair in items)
    {
      var dataGridViewColumn = Columns[keyValuePair.Key];
      if (dataGridViewColumn is null || dataGridViewColumn.Visible == keyValuePair.Value)
        continue;
      dataGridViewColumn.Visible = keyValuePair.Value;
      changes = true;
    }

    if (!changes)
      return;

    SetRowHeight();
  }

  public void SetToolStripMenu(int columnIndex, int rowIndex, MouseButtons mouseButtons)
  {
    try
    {
      m_MenuItemColumnIndex = columnIndex;
      if (mouseButtons == MouseButtons.Right && columnIndex > -1)
        toolStripMenuItemRemoveOne.Enabled =  m_FilterLogic[columnIndex].Active;

      if (mouseButtons == MouseButtons.Right && rowIndex == -1)
      {
        toolStripMenuItemFreeze.Text = Columns[columnIndex].Frozen ? "Unfreeze" : "Freeze";

        // toolStripMenuItemFilterAdd.Enabled = columnIndex > -1;
        toolStripMenuItemSortAscending.Enabled = columnIndex > -1;
        toolStripMenuItemSortDescending.Enabled = columnIndex > -1;

        toolStripMenuItemSortAscending.Text = columnIndex > -1
          ? string.Format(
            CultureInfo.CurrentCulture,
            Convert.ToString(toolStripMenuItemSortAscending.Tag) ?? string.Empty,
            Columns[columnIndex].DataPropertyName)
          : "Sort ascending";

        toolStripMenuItemSortDescending.Text = columnIndex > -1
          ? string.Format(
            CultureInfo.CurrentCulture,
            Convert.ToString(toolStripMenuItemSortDescending.Tag) ?? string.Empty,
            Columns[columnIndex].DataPropertyName)
          : "Sort descending";
        var columnFormat = GetColumnFormat(columnIndex);
        toolStripMenuItemCF.Visible = columnFormat != null;
        toolStripSeparatorCF.Visible = columnFormat != null;
        if (columnFormat != null)
          toolStripMenuItemCF.Text = $@"Change column format: {columnFormat.ValueFormat.DataType.Description()}";

        toolStripMenuItemRemoveOne.Enabled &= columnIndex != -1;
        contextMenuStripHeader.Show(Cursor.Position);
      }

      if (mouseButtons != MouseButtons.Right || rowIndex <= -1 || columnIndex <= -1)
        return;
      CurrentCell = Rows[rowIndex].Cells[columnIndex];
      contextMenuStripCell.Show(Cursor.Position);
    }
    catch (Exception ex)
    {
      Logger.Warning("Error in SetToolStripMenu ", ex);
    }
  }

  public void SetViewStatus(string newSetting)
  {
    this.RunWithHourglass(() =>
      {
        SuspendLayout();
        ViewSetting.ReStoreViewSetting(newSetting, Columns, m_FilterLogic, Sort);
        ApplyFilters();
        ResumeLayout(true);
      }
    );
  }

  /// <inheritdoc />
  protected override void Dispose(bool disposing)
  {
    if (m_Disposed)
      return;
    if (disposing)
    {
      // Unsubscribe from static event
      Microsoft.Win32.SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;

      components?.Dispose();
      m_ImgFilterIndicator.Dispose();
    }
    m_Disposed = true;

    base.Dispose(disposing);
  }

  // To detect redundant calls
  /// <summary>
  ///   Get the height of the row based on the content
  /// </summary>
  /// <param name="row">The row.</param>
  /// <param name="checkedColumns">The checked columns.</param>
  /// <returns></returns>
  private static int GetDesiredRowHeight(DataGridViewRow row, IEnumerable<DataGridViewColumn> checkedColumns)
  {
    // Actually depend on scaling, best approach is to get the initial row.Height of the very
    // first call
    if (m_DefRowHeight == -1)
      m_DefRowHeight = row.Height;
    // in case the row is not bigger than normal check if it would need to be higher
    if (row.Height != m_DefRowHeight) return m_DefRowHeight;
    if (checkedColumns.Any(column => row.Cells[column.Index].Value?.ToString()?.IndexOf('\n') != -1))
      return m_DefRowHeight * 2;

    return m_DefRowHeight;
  }

  /// <summary>
  /// Measures the required width for a column based on its content and header text.
  /// Uses a sampling strategy to maintain performance in large virtual datasets.
  /// </summary>
  /// <param name="graphics">The graphics context used for text measurement.</param>
  /// <param name="font">The font used to render the cell text.</param>
  /// <param name="maxWidth">The maximum allowed width for the column.</param>
  /// <param name="col">The DataColumn being measured.</param>  
  /// <param name="valueSelector">A function that extracts the display text from a cell value and indicates if scanning should stop.</param>
  /// <returns>The calculated width in pixels, clamped to <paramref name="maxWidth"/>.</returns>  
  private static int MeasureStrings(IDeviceContext graphics, Font font, int maxWidth, DataColumn col, DataRowCollection rows,
      Func<object, (string Text, bool Stop)> valueSelector)
  {
    // Start with the column header width as the minimum
    var headerWidth = TextRenderer.MeasureText(graphics, col.ColumnName, font).Width + 10; // +10 for sort glyph space
    var max = Math.Min(headerWidth, maxWidth);

    var counter = 0;
    var lastIncrease = 0;
    int rowCount = rows.Count;

    // OPTIMIZATION: If we have thousands of rows, we don't need to check every single one.
    // We check every row initially, then start skipping to sample the data.
    for (int i = 0; i < rowCount; i++)
    {
      if (max >= maxWidth)
        return maxWidth;

      // Sampling logic: 
      // 0-500: Check every row
      // 500-1000: Check every 10th row
      // 1000+: Check every 100th row
      if (i > 500 && i <= 1000 && i % 10 != 0) continue;
      if (i > 1000 && i % 100 != 0) continue;

      var check = valueSelector(rows[i][col]);
      if (check.Stop)
        break;

      if (!string.IsNullOrEmpty(check.Text))
      {
        // Measure the text with a small buffer for the cell margins
        var width = TextRenderer.MeasureText(graphics, check.Text, font).Width + 6;

        if (width > max)
        {
          lastIncrease = counter;
          max = Math.Min(width, maxWidth);
        }
      }

      // Break if we haven't found a wider value in the last 500 checked rows
      // if we checked 2000 times we are at row 145900
      if (counter++ > 2000 || counter - lastIncrease > 500)
        break;
    }

    return max;
  }

  private void Copy(bool addErrorInfo, bool cutLength)
  {
    try
    {
      var html = new DataGridViewCopyPaste(HtmlStyle);
      html.SelectedDataIntoClipboard(this, addErrorInfo, cutLength, CancellationToken);
    }
    catch (Exception ex)
    {
      Debug.WriteLine($"Issue during Copy {ex.Message}");
    }
  }

  /// <summary>
  ///   Shows the pop up when user right-clicks a column header
  /// </summary>
  /// <param name="sender">The event source.</param>
  /// <param name="e">
  ///   The <see cref="System.Windows.Forms.DataGridViewCellMouseEventArgs" /> instance containing
  ///   the event data.
  /// </param>
  private void FilteredDataGridView_CellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
  {
    try
    {
      SetToolStripMenu(e.ColumnIndex, e.RowIndex, e.Button);
      if (e.Button==MouseButtons.Left && e.RowIndex >= 0 && Columns[e.ColumnIndex] is DataGridViewButtonColumn)
        OpenEditor(CurrentCell);
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex);
    }
  }

  /// <summary>
  ///   Handles the CellValueNeeded event to provide data for Virtual Mode.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">A <see cref="DataGridViewCellValueEventArgs"/> containing the row and column index.</param>
  /// <remarks>
  ///   This method uses a local reference to the <see cref="DataView"/> to ensure thread safety 
  ///   during data swaps and respects current sorting and filtering.
  /// </remarks>
  private void FilteredDataGridView_CellValueNeeded(object? sender, DataGridViewCellValueEventArgs e)
  {
    // Do not attempt to provide values if the control is shutting down
    if (IsDisposed)
      return;
    try
    {
      // Capture reference locally to prevent NullReferenceException if the 
      // DataView is replaced by another thread during execution.
      var localView = DataTable.DefaultView;

      if (e.RowIndex < localView.Count && e.ColumnIndex < Columns.Count)
      {
        var dataRow = localView[e.RowIndex].Row; // Get the actual DataRow        
        // Access via the View to respect active Sort and Filter settings
        var propertyName = Columns[e.ColumnIndex].DataPropertyName;

        if (!string.IsNullOrEmpty(propertyName))
        {
          e.Value = dataRow[propertyName];

          // Set the cell-specific error from the DataRow
          Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = dataRow.GetColumnError(propertyName);

          // Set the row-level error
          Rows[e.RowIndex].ErrorText = dataRow.RowError;

          if (e.Value is DateTime dateTime)
            e.Value = StringConversion.DisplayDateTime(dateTime, CultureInfo.CurrentCulture);
        }
      }
    }
    catch (Exception ex)
    {
      // Log the error but don't crash the UI thread; Virtual Mode can be sensitive
      // to rapid data changes during background loading.
      Logger.Warning($"Error in CellValueNeeded at Row: {e.RowIndex}, Column: {e.ColumnIndex}", ex);
    }
  }

  /// <summary>
  ///   Handles the ColumnWidthChanged event of the FilteredDataGridView control.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">
  ///   The <see cref="System.Windows.Forms.DataGridViewColumnEventArgs" /> instance containing
  ///   the event data.
  /// </param>
  private void FilteredDataGridView_ColumnWidthChanged(object? sender, DataGridViewColumnEventArgs e)
  {
    if (e.Column.Width > (Width * 3) / 4)
      e.Column.Width = (Width * 3) / 4;
  }

  /// <summary>
  ///   Handles the KeyDown event of the mDataGridView control.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">
  ///   The <see cref="System.Windows.Forms.KeyEventArgs" /> instance containing the event data.
  /// </param>
  private void FilteredDataGridView_KeyDown(object? sender, KeyEventArgs e)
  {
    if (e.Control && e.KeyCode == Keys.F2)
    {
      OpenEditor(CurrentCell);
      e.Handled = true;
    }
    else if (e.Control && e.KeyCode == Keys.C)
    {
      Copy(!e.Alt, e.Shift);
      e.Handled = true;
    }
  }

  /// <summary>
  /// Rebuilds all DataGridView columns based on the current DataView,  
  /// determining wrap behavior, button-cell conversion and width 
  /// calculation. Existing column widths are preserved where possible.
  /// </summary>
  private void GenerateDataGridViewColumn()
  {
    // Store old Width
    var oldWidth = new DictionaryIgnoreCase<int>();
    foreach (DataGridViewColumn column in Columns)
      if (!oldWidth.ContainsKey(column.DataPropertyName))
        oldWidth.Add(column.DataPropertyName, column.Width);

    // remove all columns
    Columns.Clear();
    m_FilterLogic.Clear();

    var wrapColumns = new List<DataColumn>();
    var showAsButton = new List<DataColumn>();
    // Determine which columns should be wrapped
    // or shown as buttons based on content
    foreach (DataColumn col in DataTable.Columns)
    {
      if (col.DataType != typeof(string))
        continue;
      int maxRows = 1000;
      foreach (DataRow row in DataTable.Rows)
      {
        if (--maxRows <= 0)
          break;
        var text = row[col].ToString();
        if (string.IsNullOrWhiteSpace(text))
          continue;
        if (m_ShowButtonAtLength >0 && text.Length > m_ShowButtonAtLength)
        {
          showAsButton.Add(col);
          break;
        }
        if (text.IndexOf('\n') != -1)
        {
          wrapColumns.Add(col);
          break;
        }
      }
    }
    // Create a new column for each data column,
    // preserving the old width if possible
    foreach (DataColumn col in DataTable.Columns)
    {
      var newColumn = (col.DataType == typeof(bool))
             ? new DataGridViewCheckBoxColumn() : showAsButton.Contains(col)
             ? new DataGridViewButtonColumn() as DataGridViewColumn
             : new DataGridViewTextBoxColumn();
      // Make sure we have enough in the Po pup List
      if (col.DataType != typeof(string))
      {
        newColumn.DefaultCellStyle.ForeColor = Color.MidnightBlue;
        newColumn.DefaultCellStyle.Alignment = newColumn.ValueType == typeof(bool)
          ? DataGridViewContentAlignment.MiddleCenter
          : DataGridViewContentAlignment.MiddleRight;
      }
      else
      {
        newColumn.DefaultCellStyle.ForeColor = Color.Black;
        newColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
      }
      newColumn.ValueType = col.DataType;
      newColumn.Name = col.ColumnName;
      newColumn.DataPropertyName = col.ColumnName;
      newColumn.SortMode = DataGridViewColumnSortMode.Programmatic;

      if (wrapColumns.Contains(col))
        newColumn.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
      if (showAsButton.Contains(col))
        newColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

      newColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
      newColumn.Width = oldWidth.TryGetValue(newColumn.DataPropertyName, out var value)
                        ? value : GetColumnWith(col, DataTable.Rows);

      // The Index does not change when moving or hiding columns DisplayIndex does though.
      var colIndex = Columns.Add(newColumn);
      m_FilterLogic[colIndex]= new ColumnFilterLogic(col.DataType, col.ColumnName);
    }
  }


  /// <summary>
  ///   Gets the column format.
  /// </summary>
  /// <param name="colIndex">The column index.</param>
  /// <returns></returns>
  private Column? GetColumnFormat(int colIndex)
  {
    // need to map columnIndex to Column Collection
    if (m_FileSetting is null || colIndex < 0 || colIndex > Columns.Count)
      return null;
    return m_FileSetting.ColumnCollection.GetByName(Columns[colIndex].DataPropertyName);
  }

  /// <summary>
  /// Determines an optimal display width for a column by sampling data,
  /// respecting maximum width limits and early termination thresholds for
  /// performance on large datasets.
  /// </summary>
  private int GetColumnWith(DataColumn col, DataRowCollection rowCollection)
  {
    using var graphics = CreateGraphics();

    return col.DataType switch
    {
      var t when t == typeof(Guid) => Math.Max(
        TextRenderer.MeasureText(graphics, "4B3D8135-5EA3-4AFC-A912-A768BDB4795E", Font).Width,
        TextRenderer.MeasureText(graphics, col.ColumnName, Font).Width),
      var t when t == typeof(int) ||
                 t == typeof(long) ||
                 t == typeof(decimal) ||
                 t == typeof(bool) => Math.Max(
        TextRenderer.MeasureText(graphics, "123456789.11", Font).Width,
        TextRenderer.MeasureText(graphics, col.ColumnName, Font).Width),
      var t when t == typeof(DateTime) => MeasureStrings(graphics, Font, Width / 2, col, rowCollection,
        value => (
          (value is DateTime dtm) ? StringConversion.DisplayDateTime(dtm, CultureInfo.CurrentCulture) : string.Empty,
          CancellationToken.IsCancellationRequested)),
      var t when t == typeof(string) =>
        MeasureStrings(graphics, Font, Width / 2, col, rowCollection,
          value =>
          {
            var txt = value?.ToString() ?? string.Empty;
            return (txt, CancellationToken.IsCancellationRequested || txt.Length > m_ShowButtonAtLength);
          }),
      _ => Math.Min(Width / 2,
              Math.Max(TextRenderer.MeasureText(graphics, "12345678", Font).Width,
                       TextRenderer.MeasureText(graphics, col.ColumnName, Font).Width))
    };
  }

  /// <summary>
  /// Handles custom cell painting including:
  /// <list type="bullet">
  ///   <item>Drawing filter indicator icons in column headers</item>
  ///   <item>Highlighting occurrences of <see cref="HighlightText"/></item>
  ///   <item>Delegating default rendering where appropriate</item>
  /// </list>
  /// </summary>
  private void HighlightCellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
  {
    try
    {
      if (e.Graphics == null)
        return;

      if (e.RowIndex== -1 &&  e.ColumnIndex >= 0 && m_FilterLogic[e.ColumnIndex].Active)
      {
        e.Handled = true;
        e.PaintBackground(e.CellBounds, true);

        // Display a Filter Symbol
        var pt = e.CellBounds.Location;
        var offset = e.CellBounds.Width - 26;
        pt.X += offset;
        pt.Y = (e.CellBounds.Height / 2) - 4;
        e.Graphics.DrawImageUnscaled(m_ImgFilterIndicator, pt);
        e.PaintContent(e.CellBounds);
      }

      if (e.RowIndex < 0 || e.ColumnIndex < 0)
        return;
      var val = e.FormattedValue?.ToString() ?? string.Empty;
      if (string.IsNullOrEmpty(val))
        return;

      var nbspIndex = val.IndexOf((char) 0xA0);
      var linefeedIndex = val.IndexOf('\n');
      var highlightIndex = HighlightText.Length > 0
        ? val.IndexOf(HighlightText, StringComparison.InvariantCultureIgnoreCase)
        : -1;

      if (nbspIndex == -1 && highlightIndex == -1)
        return;

      e.Handled = true;
      e.PaintBackground(e.CellBounds, true);

      if (nbspIndex >= 0 && (linefeedIndex == -1 || nbspIndex < linefeedIndex)
                         && e.CellStyle?.Alignment == DataGridViewContentAlignment.MiddleLeft)
      {
        var hlRect = new Rectangle();
        var widthSpace = TextRenderer.MeasureText(e.Graphics, @" ", e.CellStyle.Font, e.ClipBounds.Size).Width;
        // Only do this as long the NBSP is before a linefeed
        while (nbspIndex >= 0 && (linefeedIndex == -1 || nbspIndex < linefeedIndex))
        {
          if (linefeedIndex == -1)

            // Middle Alignment (this goes wrong if the have a linefeed)
            hlRect.Y = (e.CellBounds.Top + (e.CellBounds.Height / 2)) - 2;
          else
            hlRect.Y = (e.CellBounds.Top + Font.Height) - 4;

          if (nbspIndex > 0)
            hlRect.X = (e.CellBounds.X
                        + TextRenderer.MeasureText(e.Graphics,
#if NET7_0_OR_GREATER
                            val.AsSpan(0, nbspIndex),
#else
                          val.Substring(0, nbspIndex),
#endif
                            e.CellStyle.Font, e.CellBounds.Size)
                          .Width) - 6;
          else
            hlRect.X = e.CellBounds.X;

          // if we are outside the bound stop
          if (hlRect.X > e.CellBounds.X + e.CellBounds.Width)
            break;

          e.Graphics.DrawLines(new Pen(Brushes.LightSalmon, 2),
            new[]
            {
              new Point(hlRect.X, e.CellBounds.Bottom - 10), new Point(hlRect.X, e.CellBounds.Bottom - 5),
              new Point(hlRect.X + widthSpace, e.CellBounds.Bottom - 5),
              new Point(hlRect.X + widthSpace, e.CellBounds.Bottom - 10)
            });
          nbspIndex = val.IndexOf((char) 0xA0, nbspIndex + 1);
        }
      }

      if (HighlightText.Length > 0
          && (e.CellStyle?.Alignment == DataGridViewContentAlignment.MiddleLeft
              || e.CellStyle?.Alignment == DataGridViewContentAlignment.MiddleRight) && highlightIndex >= 0)
      {
        using var hlBrush = new SolidBrush(Color.MediumSpringGreen);
        var hlRect = new Rectangle();
        while (highlightIndex >= 0 && (linefeedIndex == -1 || highlightIndex < linefeedIndex))
        {
          var highlight = TextRenderer.MeasureText(
            e.Graphics,
#if NET7_0_OR_GREATER
            val.AsSpan(highlightIndex, HighlightText.Length),
#else
              val.Substring(highlightIndex, HighlightText.Length),
#endif
            e.CellStyle.Font,
            e.CellBounds.Size);

          hlRect.Y = e.CellBounds.Y + (e.CellBounds.Height - highlight.Height) / 2;
          hlRect.Height = highlight.Height + 1;
          hlRect.Width = highlight.Width - 6;

          if (e.CellStyle.Alignment == DataGridViewContentAlignment.MiddleLeft)
          {
            if (highlightIndex > 0)
              hlRect.X = (e.CellBounds.X + TextRenderer.MeasureText(
                e.Graphics,
#if NET7_0_OR_GREATER
                val.AsSpan(0, highlightIndex),
#else
              val.Substring(0, highlightIndex),
#endif
                e.CellStyle.Font,
                e.CellBounds.Size).Width) - 4;
            else
              hlRect.X = e.CellBounds.X + 2;
          }
          else
          {
            if (highlightIndex + HighlightText.Length < val.Length)
              hlRect.X = ((e.CellBounds.X + e.CellBounds.Width)
                          - TextRenderer.MeasureText(e.Graphics,
#if NET7_0_OR_GREATER
                            val.AsSpan(highlightIndex + HighlightText.Length),
#else
              val.Substring(highlightIndex + HighlightText.Length),
#endif
                            e.CellStyle.Font, e.CellBounds.Size).Width
                          - hlRect.Width) + 3;
            else
              hlRect.X = (e.CellBounds.X + e.CellBounds.Width) - hlRect.Width - 4;
          }

          e.Graphics.FillRectangle(hlBrush, hlRect);
          highlightIndex = val.IndexOf(
            HighlightText,
            highlightIndex + HighlightText.Length,
            StringComparison.InvariantCultureIgnoreCase);
        }
      }

      // paint the content as usual
      e.PaintContent(e.CellBounds);
    }
    catch (Exception ex)
    {
      e.Handled = false;
      Debug.WriteLine(ex);
    }
  }

  private void OpenColumnsDialog(object? sender, EventArgs e)
  {
    this.RunWithHourglass(() =>
    {
      // This does not work properly
      var filterExpression = GetFilterExpression(m_MenuItemColumnIndex);
      using var filterPopup = new FromColumnsFilter(Columns,
        DataTable?.Select(filterExpression) ?? Array.Empty<DataRow>(),
        m_FilterLogic.Where(x => x.Value.Active).Select(x => x.Key),
        DataLoaded);
      if (filterPopup.ShowDialog() == DialogResult.OK)
        SetRowHeight();
    });
  }

  private void OpenEditor(DataGridViewCell cell)
  {
    try
    {
      using var frm = new FormTextDisplay(cell.Value?.ToString() ?? string.Empty);

      // ReSharper disable once LocalizableElement
      frm.Text = $"{Columns[cell.ColumnIndex].DataPropertyName} - Row {cell.OwningRow.Index + 1:D}";
      frm.SaveAction = s =>
      {
        if (s.Equals(cell.Value))
          return;
        cell.Value = s;
        cell.ErrorText = CurrentCell.ErrorText.AddMessage(
          "Value was modified".AddWarningId());
      };
      frm.ShowWithFont(this, true);
    }
    catch (Exception ex)
    {
      Extensions.ShowError(ex);
    }
  }

  private void OpenFilterDialog(object? sender, EventArgs e)
  {
    if (!DataLoaded &&
        MessageBox.Show("Some data is not yet loaded from file.\nOnly already processed data will be used.",
          "Incomplete data", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1,
          3) == DialogResult.Cancel)
      return;

    this.RunWithHourglass(() =>
    {
      var filter = m_FilterLogic[m_MenuItemColumnIndex];
      var filterExpression = GetFilterExpression(m_MenuItemColumnIndex);
      var data = DataTable.Select(filterExpression).Select(x => x[m_MenuItemColumnIndex]).ToArray();
      using var filterPopup = new FromRowsFilter(filter, data, 22);
      if (filterPopup.ShowDialog() == DialogResult.OK)
      {
        ApplyFilters();
        contextMenuStripCell.Close();
        contextMenuStripHeader.Close();
      }
    });
  }

  /// <summary>
  /// Applies font changes to all relevant DataGridView style sections so
  /// the control visually remains consistent when the base font changes.
  /// </summary>
  private void PassOnFontChanges(object? sender, EventArgs e)
  {
    this.SafeInvoke(() =>
    {
      DefaultCellStyle.Font = Font;
      ColumnHeadersDefaultCellStyle.Font = Font;
      RowHeadersDefaultCellStyle.Font = Font;
    });
  }

  /// <summary>
  ///   Sets the height of the row.
  /// </summary>
  private void SetRowHeight()
  {
    try
    {
      // 1. Pre-filter the columns that could contain multi-line text to avoid redundant checks inside the loop 
      var textColumns = Columns.Cast<DataGridViewColumn>()
          .Where(column => column.Visible && column.ValueType == typeof(string))
          .ToList();

      if (textColumns.Count == 0) return;

      // 2. Identify the range of rows currently visible to the user 
      int firstVisible = FirstDisplayedScrollingRowIndex;
      int visibleCount = DisplayedRowCount(true);

      // Safety check: if no rows are visible, exit 
      if (firstVisible < 0) return;

      // 3. Only iterate through the rows the user is actually looking at 
      for (int i = 0; i < visibleCount; i++)
      {
        int rowIndex = firstVisible + i;

        // Ensure we don't exceed the actual data count (especially after filtering) 
        if (rowIndex >= RowCount) break;

        // In Virtual Mode, use the Row object sparingly. 
        // We get the desired height based on cell content 
        var row = Rows[rowIndex];
        int desiredHeight = GetDesiredRowHeight(row, textColumns);

        if (row.Height != desiredHeight)
          row.Height = desiredHeight;
      }
    }
    catch (Exception ex)
    {
      Logger.Warning("Error in SetRowHeight ", ex);
    }
  }
  /// <summary>
  ///   Called when the preferences are changed by a user. In case the Local was changed we need
  ///   to clear the cache so date and number are displayed correctly again
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">
  ///   The <see cref="Microsoft.Win32.UserPreferenceChangedEventArgs" /> instance containing the
  ///   event data.
  /// </param>
#if !NETFRAMEWORK
  [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif

  private void SystemEvents_UserPreferenceChanged(object? sender, Microsoft.Win32.UserPreferenceChangedEventArgs e)
  {
    if (e.Category == Microsoft.Win32.UserPreferenceCategory.Locale)
      CultureInfo.CurrentCulture.ClearCachedData();
  }
  /// <summary>
  ///   Handles the Click event of the toolStripMenuItemCF control.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
  private void ToolStripMenuItemCF_Click(object? sender, EventArgs e)
  {
    try
    {
      var columnFormat = GetColumnFormat(m_MenuItemColumnIndex);
      if (columnFormat is null)
        return;
      if (m_FileSetting != null && FillGuessSettings != null)
      {

        using var form = new FormColumnUiRead(columnFormat, m_FileSetting, FillGuessSettings,
          false, false, false);

        if (form.ShowWithFont(this, true) == DialogResult.Cancel)
          return;

        // Update the  columns
        m_FileSetting.ColumnCollection.Replace(form.UpdatedColumn);

        // Handle Filter in case of Type change
        if (form.UpdatedColumn.ValueFormat.DataType != columnFormat.ValueFormat.DataType)
          m_FilterLogic.Remove(m_MenuItemColumnIndex);
      }
    }
    catch (Exception ex)
    {
      Debug.WriteLine($"Error in ToolStripMenuItemCF_Click {ex.Message}");
    }
  }
  /// <summary>
  ///   Handles the Click event of the toolStripMenuItemCopy control.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
  private void ToolStripMenuItemCopy_Click(object? sender, EventArgs e) => Copy(false, false);

  /// <summary>
  ///   Handles the Click event of the toolStripMenuItemCopyError control.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
  private void ToolStripMenuItemCopyError_Click(object? sender, EventArgs e) => Copy(true, false);

  /// <summary>
  ///   Handles the Click event of the toolStripMenuItemFilterRemoveAll control.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
  private void ToolStripMenuItemFilterRemoveAll_Click(object? sender, EventArgs e) => RemoveAllFilter();

  /// <summary>
  ///   Handles the Click event of the toolStripMenuItemFilterRemove control.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
  private void ToolStripMenuItemFilterRemoveOne_Click(object? sender, EventArgs? e)
  {
    try
    {
      var filterLogic = m_FilterLogic[m_MenuItemColumnIndex];
      if (!filterLogic.Active)
        return;

      filterLogic.Active = false;
      ApplyFilters();
    }
    catch (Exception ex)
    {
      Debug.WriteLine($"Issue during ToolStripMenuItemFilterRemoveOne_Click {ex.Message}");
    }
  }

  /// <summary>
  ///   Handles the Click event of the toolStripMenuItemFilterValue control.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
  private void ToolStripMenuItemFilterValue_Click(object? sender, EventArgs e) => FilterCurrentCell();

  private void ToolStripMenuItemFreeze_Click(object? sender, EventArgs e) =>
    SetColumnFrozen(m_MenuItemColumnIndex, !Columns[m_MenuItemColumnIndex].Frozen);

  private void ToolStripMenuItemHideThisColumn_Click(object? sender, EventArgs e)
  {
    try
    {
      if (!Columns.Cast<DataGridViewColumn>().Any(col => col.Visible && col.Index != m_MenuItemColumnIndex)) return;
      Columns[m_MenuItemColumnIndex].Visible = false;
    }
    catch (Exception ex)
    {
      Debug.WriteLine($"Issue during ToolStripMenuItemHideThisColumn_Click {ex.Message}");
    }
  }

  private void ToolStripMenuItemOpenEditor_Click(object? sender, EventArgs e)
  {
    try
    {
      OpenEditor(CurrentCell);
    }
    catch (Exception ex)
    {
      Debug.WriteLine($"Issue during ToolStripMenuItemFilterRemoveOne_Click {ex.Message}");
    }
  }

  /// <summary>
  ///   Handles the Click event of the toolStripMenuItemSortAscending control.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
  private void ToolStripMenuItemSortAscending_Click(object? sender, EventArgs e) =>

    // Column was set on showing context menu
    Sort(Columns[m_MenuItemColumnIndex], ListSortDirection.Ascending);

  /// <summary>
  ///   Handles the Click event of the toolStripMenuItemSortDescending control.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
  private void ToolStripMenuItemSortDescending_Click(object? sender, EventArgs e) =>

    // Column was set on showing context menu
    Sort(Columns[m_MenuItemColumnIndex], ListSortDirection.Descending);

  private void ToolStripMenuItemSortRemove_Click(object? sender, EventArgs e)
  {
    DataTable.DefaultView.Sort = string.Empty;
    foreach (DataGridViewColumn col in Columns)
      col.HeaderCell.SortGlyphDirection = SortOrder.None;
    Invalidate(); 
  } 
}