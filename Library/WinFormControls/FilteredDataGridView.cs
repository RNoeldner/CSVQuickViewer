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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

// ReSharper disable MemberCanBePrivate.Global

namespace CsvTools
{
  /// <summary>
  ///   A better DataGridView allowing to filter and have a nice Copy and Paste
  /// </summary>
  public partial class FilteredDataGridView : DataGridView
  {
    private static int m_DefRowHeight = -1;
    private readonly Image m_ImgFilterIndicator;
    private readonly CancellationTokenSource m_CancellationTokenSource;
    private readonly Dictionary<int, ColumnFilterLogic> m_FilterLogic;
    private BindingSource? m_BindingSource;

    private bool m_DisposedValue;
    private IFileSetting? m_FileSetting;
    private int m_ShowButtonAtLength = 1000;
    private int m_MenuItemColumnIndex;
    private bool m_DataLoaded = true;

    public bool DataLoaded
    {
      get => m_DataLoaded;
      set
      {
        if (m_DataLoaded == value)
          return;
        m_DataLoaded = value;
      }
    }

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
    ///   Initializes a new instance of the <see cref="FilteredDataGridView" /> class.
    /// </summary>
#if !NETFRAMEWORK
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
    private void CellFormatDate(object? sender, DataGridViewCellFormattingEventArgs e)
    {
      if (!(e.Value is DateTime cellValue))
        return;

      e.Value = StringConversion.DisplayDateTime(cellValue, CultureInfo.CurrentCulture);
    }

    public FilteredDataGridView()
    {
      m_CancellationTokenSource = new CancellationTokenSource();

      InitializeComponent();
      CellFormatting += CellFormatDate;
      FontChanged += PassOnFontChanges;
      m_FilterLogic = new Dictionary<int, ColumnFilterLogic>();

      Scroll += (o, e) => SetRowHeight();
      var resources = new ComponentResourceManager(typeof(FilteredDataGridView));
      m_ImgFilterIndicator = (resources.GetObject("toolStripMenuItemFilterAdd.Image") as Image) ??
                             throw new InvalidOperationException("Resource not found");

      DataError += FilteredDataGridView_DataError;

      if (contextMenuStripHeader.LayoutSettings is TableLayoutSettings tableLayoutSettings)
        tableLayoutSettings.ColumnCount = 3;

      ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

      CellMouseClick += FilteredDataGridView_CellMouseClick;
      CellPainting += HighlightCellPainting;
      ColumnWidthChanged += FilteredDataGridView_ColumnWidthChanged;
      KeyDown += FilteredDataGridView_KeyDown;
      ColumnAdded += FilteredDataGridView_ColumnAdded;

      DefaultCellStyle.ForeColor = Color.Black;
      DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

      SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

      FontChanged += (o, e) =>
      {
        AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        SetRowHeight();
      };
    }

    /// <summary>
    ///   Occurs when the next result should be shown
    /// </summary>
    public event EventHandler? DataViewChanged;

    /// <summary>
    ///   Gets the current filter.
    /// </summary>
    /// <value>The current filter.</value>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Bindable(false)]
    [Browsable(false)]
    public string CurrentFilter =>
      (m_BindingSource != null ? m_BindingSource?.Filter : DataView?.RowFilter) ?? string.Empty;

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
    ///   Gets or sets the data source that the <see cref="DataGridView" /> is displaying data for.
    /// </summary>
    /// <returns>The object that contains data for the <see cref="DataGridView" /> to display.</returns>
    /// <exception cref="Exception">
    ///   An error occurred in the data source and either there is no handler for the event or the handler has set the <see cref="Exception" /> property to
    ///   true. The exception object can typically be cast to type <see cref="FormatException" />.
    /// </exception>
    public new object? DataSource
    {
      get => base.DataSource;

      set
      {
        if (value is null)
        {
          ResetDataSource();
        }
        else
        {
          base.DataSource = value;
          SetBoundDataView(true);
          GenerateDataGridViewColumn();
        }
      }
    }

    /// <summary>
    ///   Sets the file setting.
    /// </summary>
    /// <value>The file setting.</value>
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
    ///   Sets the frozen columns.
    /// </summary>
    /// <value>The frozen columns.</value>
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
    ///   Sets the text that should be highlighted
    /// </summary>
    /// <value>The highlight text.</value>
    public string HighlightText { get; set; } = string.Empty;

    /// <summary>
    ///   Gets or sets the HTML style.
    /// </summary>
    /// <value>The HTML style.</value>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Bindable(false)]
    [Browsable(false)]
    public HtmlStyle HtmlStyle { get; set; } = HtmlStyle.Default;

    /// <summary>
    ///   The current DataView
    /// </summary>
    internal DataView? DataView { get; private set; }

    /// <summary>
    /// Get the filter for all columns but the one cloumn specified
    /// </summary>
    /// <param name="exclude">The column index</param>
    /// <returns>The filter statement</returns>
    public string GetFilterExpression(int exclude) => m_FilterLogic.Where(x => x.Key != exclude && x.Value.Active && !string.IsNullOrEmpty(x.Value.FilterExpression)).Select(x => x.Value.FilterExpression).Join("\nAND\n");

    /// <summary>
    ///   Applies the filters.
    /// </summary>
    public void ApplyFilters() =>
      this.RunWithHourglass(() =>
        {
          var bindingSourceFilter = GetFilterExpression(-1);
          toolStripMenuItemFilterRemoveAllFilter.Enabled = bindingSourceFilter.Length > 0;

          // Apply the filter only if any changes occurred
          if (m_BindingSource != null)
          {
            if (bindingSourceFilter.Equals(m_BindingSource.Filter, StringComparison.Ordinal))
              return;
            m_BindingSource.Filter = bindingSourceFilter;
            DataViewChanged?.Invoke(this, EventArgs.Empty);
          }
          else
          {
            if (DataView is null ||
                bindingSourceFilter.Equals(DataView.RowFilter, StringComparison.Ordinal))
              return;
            DataView.RowFilter = bindingSourceFilter;
            DataViewChanged?.Invoke(this, EventArgs.Empty);
          }
        }
      );

    public void FilterCurrentCell()
    {
      try
      {
        if (CurrentCell is null)
          return;
        var filter = m_FilterLogic[m_MenuItemColumnIndex];
        if (filter is null) return;
        filter.SetFilter(CurrentCell.Value);
        ApplyFilters();
      }
      catch
      {
        // ignored
      }
    }

    /// <summary>
    ///   Hides empty columns in the Data Grid
    /// </summary>
    /// <returns><c>true</c> if column visibility has changed</returns>
    public bool HideEmptyColumns()
    {
      if (Columns.Count == 0 || DataView is null)
        return false;

      var hasChanges = false;
      foreach (DataGridViewColumn col in Columns)
        if (col.Visible)
        {
          var hasData = DataView.Cast<DataRowView>()
            .Any(dataRow => dataRow[col.DataPropertyName] != DBNull.Value);
          if (!hasData && col.Visible)
            col.Visible = false;
          hasChanges = true;
        }

      return hasChanges;
    }

    public void RefreshUI()
    {
      try
      {
        if (!HideEmptyColumns())
          return;
        SetRowHeight();
        DataViewChanged?.Invoke(this, EventArgs.Empty);
      }
      catch
      {
        // ignored
      }
    }

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

    public void ReStoreViewSetting(string fileName)
    {
      if (string.IsNullOrEmpty(fileName) || !FileSystemUtils.FileExists(fileName) || Columns.Count == 0)
        return;
      if (m_FileSetting is IFileSettingPhysicalFile basePhysical)
        basePhysical.ColumnFile = fileName;

      SetViewStatus(FileSystemUtils.ReadAllText(fileName));
    }

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
      DataViewChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///   Sets the height of the row.
    /// </summary>
    private void SetRowHeight()
    {
      try
      {
        // Determine each column that could contain a text and is not hidden
        var visible = Columns.Cast<DataGridViewColumn>()
          .Where(column => column.Visible && column.ValueType == typeof(string)).ToList();

        var visibleRowsCount = DisplayedRowCount(true);
        var firstDisplayedRowIndex = FirstDisplayedCell?.RowIndex ?? 0;

        for (int rowIndex = firstDisplayedRowIndex; rowIndex < firstDisplayedRowIndex + visibleRowsCount; rowIndex++)
          Rows[rowIndex].Height = GetDesiredRowHeight(Rows[rowIndex], visible);
      }
      catch
      {
        // ignore        
      }
    }

    public void SetToolStripMenu(int columnIndex, int rowIndex, MouseButtons mouseButtons)
    {
      try
      {
        m_MenuItemColumnIndex = columnIndex;
        if (mouseButtons == MouseButtons.Right && columnIndex > -1)
          toolStripMenuItemRemoveOne.Enabled =  GetColumnFilter(columnIndex).Active;

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

#pragma warning disable CS8604 // Possible null reference argument.
          toolStripMenuItemSortDescending.Text = columnIndex > -1
            ? string.Format(
              CultureInfo.CurrentCulture,
              Convert.ToString(toolStripMenuItemSortDescending.Tag),
              Columns[columnIndex].DataPropertyName)
            : "Sort descending";
#pragma warning restore CS8604 // Possible null reference argument.
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
        Logger.Warning(ex, "Click in DataGrid");
      }
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      if (m_DisposedValue) return;
      if (disposing)
      {
        m_DisposedValue = true;
        components?.Dispose();
        m_ImgFilterIndicator.Dispose();
        m_CancellationTokenSource.Dispose();
      }

      base.Dispose(disposing);
    }
    
    private static int Measure(Graphics grap, Font font, int maxWidth, DataColumn col, DataRowCollection rows,
      Func<object, (string Text, bool Stop)> checkValue, CancellationToken token)
    {
      var max = Math.Min(
        TextRenderer.MeasureText(grap, col.ColumnName, font).Width,
        maxWidth);
      var counter = 0;
      var lastIncrease = 0;
      foreach (DataRow dataRow in rows)
      {
        if (max >= maxWidth)
          return maxWidth;
        if (token.IsCancellationRequested)
          break;
        var check = checkValue(dataRow[col]);
        if (check.Stop)
          break;
        if (check.Text.Length>0)
        {
          var width = TextRenderer.MeasureText(grap, check.Text, font).Width;
          if (width > max)
          {
            lastIncrease= counter;
            max = width;
          }
        }
        if (counter++ > 20000 || counter - lastIncrease > 500)
          break;
      }
      return max;
    }

    /// <summary>
    ///   Determine a default column with based on the data type and the values in provided
    /// </summary>
    /// <param name="col"></param>
    /// <param name="rowCollection"></param>
    /// <returns>A number for DataGridViewColumn.With</returns>
    private int GetColumnWith(DataColumn col, DataRowCollection rowCollection, CancellationToken token)
    {
      using var grap = CreateGraphics();

      if (col.DataType == typeof(Guid))
        return Math.Max(TextRenderer.MeasureText(grap, "4B3D8135-5EA3-4AFC-A912-A768BDB4795E", Font).Width,
                        TextRenderer.MeasureText(grap, col.ColumnName, Font).Width);

      if (col.DataType == typeof(int) || col.DataType == typeof(bool) || col.DataType == typeof(long) || col.DataType == typeof(decimal))
        return Math.Max(TextRenderer.MeasureText(grap, "626727278.00", Font).Width,
                        TextRenderer.MeasureText(grap, col.ColumnName, Font).Width);

      if (col.DataType == typeof(DateTime))
        return Measure(grap, Font, Width /2, col, rowCollection,
          value => ((value is DateTime dtm) ? StringConversion.DisplayDateTime(dtm, CultureInfo.CurrentCulture) : string.Empty, false), token);

      if (col.DataType == typeof(string))
        return Measure(grap, Font, Width /2, col, rowCollection,
          value =>
          {
            var txt = value?.ToString() ?? string.Empty;
            return (txt, txt.Length > m_ShowButtonAtLength);
          }, token);

      return Math.Min(Width /2, Math.Max(TextRenderer.MeasureText(grap, "dummy", Font).Width, TextRenderer.MeasureText(grap, col.ColumnName, Font).Width));
    }

    public new void AutoResizeColumns(DataGridViewAutoSizeColumnsMode autoSizeColumnsMode)
    {
      if (DataView is null || DataView.Table is null)
        base.AutoResizeColumns(autoSizeColumnsMode);
      else
      {
        foreach (DataColumn col in DataView.Table.Columns)
        {
          foreach (DataGridViewColumn newColumn in Columns)
          {
            if (newColumn.DataPropertyName == col.ColumnName)
            {
              newColumn.Width = GetColumnWith(col, DataView!.Table.Rows, m_CancellationTokenSource.Token) + 5;
              break;
            }
          }
        }
      }
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

    private void OpenEditor(DataGridViewCell cell)
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
        Logger.Error(ex, "FilteredDataGridView: Mouse Click {columnIndex} {rowIndex} {button}", e.ColumnIndex, e.RowIndex, e.Button);
      }
    }

    /// <summary>
    ///   Handles the ColumnAdded event of the FilteredDataGridView control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///   The <see cref="System.Windows.Forms.DataGridViewColumnEventArgs" /> instance containing
    ///   the event data.
    /// </param>
    private void FilteredDataGridView_ColumnAdded(object? sender, DataGridViewColumnEventArgs e)
    {
      // Make sure we have enough in the Po pup List
      if (e.Column.ValueType != typeof(string))
      {
        e.Column.DefaultCellStyle.ForeColor = Color.MidnightBlue;
        e.Column.DefaultCellStyle.Alignment = e.Column.ValueType == typeof(bool)
          ? DataGridViewContentAlignment.MiddleCenter
          : DataGridViewContentAlignment.MiddleRight;
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

    private void FilteredDataGridView_DataError(object? sender, DataGridViewDataErrorEventArgs e)
    {
      // Display no Error
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
    ///   Generates the data grid view column.
    /// </summary>
    private void GenerateDataGridViewColumn()
    {
      // close and remove all pop ups
      var oldWith = new Dictionary<string, int>();
      foreach (DataGridViewColumn column in Columns)
        if (!oldWith.ContainsKey(column.DataPropertyName))
          oldWith.Add(column.DataPropertyName, column.Width);


      // remove all columns
      Columns.Clear();

      // if we do not have a BoundDataView exit now
      if (DataView is null || DataView.Table is null)
        return;

      var wrapColumns = new List<DataColumn>();
      var showAsButton = new List<DataColumn>();
      foreach (DataColumn col in DataView.Table.Columns)
      {
        if (col.DataType != typeof(string)) continue;
        foreach (DataRow row in DataView.Table.Rows)
        {
          var text = row[col].ToString();
          if (string.IsNullOrEmpty(text))
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

      foreach (DataColumn col in DataView.Table.Columns)
      {
        var newColumn = (col.DataType == typeof(bool)) ? new DataGridViewCheckBoxColumn() : showAsButton.Contains(col) ? new DataGridViewButtonColumn() as DataGridViewColumn : new DataGridViewTextBoxColumn();

        newColumn.ValueType = col.DataType;
        newColumn.Name = col.ColumnName;
        newColumn.DataPropertyName = col.ColumnName;

        if (wrapColumns.Contains(col))
          newColumn.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        if (showAsButton.Contains(col))
          newColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        newColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
        newColumn.Width =
          oldWith.TryGetValue(newColumn.DataPropertyName, out var value) ? value :
          GetColumnWith(col, DataView.Table.Rows, m_CancellationTokenSource.Token);
        var colIndex = Columns.Add(newColumn);
        // remove filter in case it does not match
        if (m_FilterLogic.TryGetValue(colIndex, out var filter))
        {
          if (filter.DataPropertyName != col.ColumnName && filter.DataType != newColumn.ValueType.GetDataType())
            m_FilterLogic.Remove(colIndex);
        }
      }
    }

    public ColumnFilterLogic GetColumnFilter(int columnIndex)
    {
      if (m_FilterLogic.TryGetValue(columnIndex, out var filter))
        return filter;
      else
      {
        var filterNew = new ColumnFilterLogic(Columns[columnIndex].ValueType, Columns[columnIndex].DataPropertyName);
        m_FilterLogic.Add(columnIndex, filterNew);
        return filterNew;
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
    ///   Does all cell painting, doing highlighting
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">
    ///   The <see cref="DataGridViewCellPaintingEventArgs" /> instance containing the event data.
    /// </param>
    private void HighlightCellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
      try
      {
        if (e.Graphics == null)
          return;

        if (e.RowIndex== -1 &&  e.ColumnIndex >= 0 && GetColumnFilter(e.ColumnIndex).Active)
        {
          e.Handled = true;
          e.PaintBackground(e.CellBounds, true);

          // Display a Filter Symbol
          var pt = e.CellBounds.Location;
          var offset = e.CellBounds.Width - 22;
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
        Logger.Warning(ex, "HighlightCellPainting");
      }
    }

    /// <summary>
    ///   Resets the data source.
    /// </summary>
    private void ResetDataSource()
    {
      try
      {
        Columns.Clear();
        base.DataSource = null;
        this.SafeInvoke(() => DataMember = null);
        DataView = null;
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "ResetDataSource");
      }
    }

    /// <summary>
    ///   Checks if the DataGridView is data bound and the data source finally resolves to a DataView.
    /// </summary>
    /// <param name="force">if set to <c>true</c> [force].</param>
    private void SetBoundDataView(bool force)
    {
      if (DataView != null && !force)
        return;
      try
      {
        m_BindingSource = null;
        var dataSource = DataSource;
        var dataMember = DataMember;
        var maxIteration = 5;

        while (!(dataSource is DataView) && maxIteration > 0)
        {
          if (dataSource is BindingSource bs)
          {
            m_BindingSource = (BindingSource) DataSource!;
            dataMember = bs.DataMember;
            dataSource = bs.DataSource;
          }
          else
          {
            if (dataSource is DataSet ds)
            {
              dataSource = ds.Tables[dataMember];
              dataMember = string.Empty;
            }
            else
            {
              if (dataSource is DataTable dt)
              {
                dataSource = dt.DefaultView;
                break;
              }
            }
          }

          maxIteration--;
        }

        DataView = dataSource as DataView;
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "SetBoundDataView");
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

    private void SystemEvents_UserPreferenceChanged(object? sender, UserPreferenceChangedEventArgs e)
    {
      if (e.Category == UserPreferenceCategory.Locale)
        CultureInfo.CurrentCulture.ClearCachedData();
    }
    private void OpenColumnsDialog(object? sender, EventArgs e)
    {
      this.RunWithHourglass(() =>
      {
        // This does not work properly
        var filterExpression = GetFilterExpression(m_MenuItemColumnIndex);
        using var filterPopup = new FromColumnsFilter(Columns, DataView?.Table?.Select(filterExpression) ?? Array.Empty<DataRow>(), m_FilterLogic.Where(x => x.Value.Active).Select(x => x.Key),
          m_DataLoaded);
        if (filterPopup.ShowDialog() == DialogResult.OK)
        {
          SetRowHeight();
          DataViewChanged?.Invoke(this, EventArgs.Empty);
        }
      });
    }

    private void OpenFilterDialog(object? sender, EventArgs e)
    {
      if (!m_DataLoaded && MessageBox.Show("Some data is not yet loaded from file.\nOnly already processed data will be used.", "Incomplete data", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, 3)== DialogResult.Cancel)
        return;

      this.RunWithHourglass(() =>
      {
        var filter = GetColumnFilter(m_MenuItemColumnIndex);
        var filterExpression = GetFilterExpression(m_MenuItemColumnIndex);
        var data = DataView?.Table?.Select(filterExpression).Select(x => x[m_MenuItemColumnIndex]).ToArray() ?? Array.Empty<DataRow>();
        using var filterPopup = new FromRowsFilter(filter, data, 50);
        if (filterPopup.ShowDialog() == DialogResult.OK)
        {
          ApplyFilters();
          contextMenuStripCell.Close();
          contextMenuStripHeader.Close();
        }
      });
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
        Logger.Warning(ex, "ToolStripMenuItemCF_Click");
      }
    }

    private void Copy(bool addErrorInfo, bool cutLength)
    {
      try
      {
        var html = new DataGridViewCopyPaste(HtmlStyle);
        html.SelectedDataIntoClipboard(this, addErrorInfo, cutLength, m_CancellationTokenSource.Token);
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "Issue during Copy");
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
    ///   Handles the Click event of the toolStripMenuItemFilled control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ToolStripMenuItemFilled_Click(object? sender, EventArgs e) => RefreshUI();

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
        var filterLogic = GetColumnFilter(m_MenuItemColumnIndex);
        if (!filterLogic.Active)
          return;

        filterLogic.Active = false;
        ApplyFilters();
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "ToolStripMenuItemFilterRemoveOne_Click");
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
        Logger.Warning(ex, "ToolStripMenuItemOpenEditor_Click");
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
        Logger.Warning(ex, "ToolStripMenuItemHideThisColumn_Click");
      }
    }

    //private void ToolStripMenuItemLoadCol_Click(object? sender, EventArgs e)
    //{
    //  if (m_FileSetting is null)
    //    return;

    //  try
    //  {
    //    var fileName = WindowsAPICodePackWrapper.Open(
    //      m_FileSetting is IFileSettingPhysicalFile phy ? phy.FullPath.GetDirectoryName() : ".", "Load Column Setting",
    //      "Column Config|*.col;*.conf|All files|*.*", DefFileNameColSetting(m_FileSetting, ".col"));
    //    if (fileName != null)
    //      ReStoreViewSetting(fileName);
    //  }
    //  catch (Exception ex)
    //  {
    //    FindForm()?.ShowError(ex);
    //  }

    //}

    /// <summary>
    /// Get an Array of ColumnSetting serialized as Json Text
    /// </summary>
    public string GetViewStatus => ViewSetting.StoreViewSetting(Columns, m_FilterLogic.Values, SortedColumn, SortOrder);

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

//    private async void ToolStripMenuItemSaveCol_Click(object? sender, EventArgs e)
//    {
//      if (m_FileSetting is null)
//        return;
//      try
//      {
//        var text = GetViewStatus;
//        if (!string.IsNullOrEmpty(text))
//        {
//          // Select Path
//          var fileName = WindowsAPICodePackWrapper.Save(
//            m_FileSetting is IFileSettingPhysicalFile phy ? phy.FullPath.GetDirectoryName() : ".", "Save Column Setting",
//            "Column Config|*.col;*.conf|All files|*.*", ".col", false, DefFileNameColSetting(m_FileSetting, ".col"));

//          if (fileName is null || fileName.Length == 0)
//            return;

//#if NET5_0_OR_GREATER
//          await
//#endif          
//          using var stream = FunctionalDI.GetStream(new SourceAccess(fileName, false));

//#if NET5_0_OR_GREATER
//          await
//#endif
//          using var writer = new StreamWriter(stream, Encoding.UTF8, 1024);
//          await writer.WriteAsync(GetViewStatus);
//          await writer.FlushAsync();

//          if (m_FileSetting is BaseSettingPhysicalFile basePhysical)
//            basePhysical.ColumnFile = fileName;
//        }
//      }
//      catch (Exception ex)
//      {
//        FindForm()?.ShowError(ex);
//      }
//    }

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

    private void ToolStripMenuItemSortRemove_Click(object? sender, EventArgs e) => DataView!.Sort = string.Empty;



  }
}