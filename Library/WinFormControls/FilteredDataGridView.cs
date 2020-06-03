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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CsvTools
{
  /// <summary>
  ///   A better DataGridView allowing to filter and have a nice Copy and Paste
  /// </summary>
  public partial class FilteredDataGridView : DataGridView
  {
    private static Image m_ImgFilterIndicator;

    //private static Image m_ImgNbSp;

    private static int m_DefRowHeight = -1;

    private readonly CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();

    private readonly List<ToolStripDataGridViewColumnFilter> m_Filter =
      new List<ToolStripDataGridViewColumnFilter>();

    private BindingSource m_BindingSource;

    private bool m_DisposedValue;

    /// <summary>
    ///   Any Text entered here will be highlighted Filer
    /// </summary>
    private string m_HighlightText = string.Empty;

    private int m_MenuItemColumnIndex;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FilteredDataGridView" /> class.
    /// </summary>
    public FilteredDataGridView()
    {
      //Workaround as Text on Windows 8 is too small
      if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor > 1)
        Paint += FilteredDataGridView_Paint;

      var resources = new ComponentResourceManager(typeof(FilteredDataGridView));
      //try
      //{
      m_ImgFilterIndicator = (Image) resources.GetObject("toolStripMenuItem2.Image");
      // m_ImgNbSp = ((Image)(resources.GetObject("NBSP")));
      //}
      //catch
      //{
      //  //ignore
      //}

      InitializeComponent();
      DataError += FilteredDataGridView_DataError;
      toolStripMenuItemColumnVisibility.ItemCheck += CheckedListBox_ItemCheck;
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

      contextMenuStripFilter.Opened += ContextMenuStripFilter_Opened;
      contextMenuStripFilter.Closing += delegate(object sender, ToolStripDropDownClosingEventArgs e)
      {
        if (e.CloseReason != ToolStripDropDownCloseReason.AppClicked
            && e.CloseReason != ToolStripDropDownCloseReason.ItemClicked
            && e.CloseReason != ToolStripDropDownCloseReason.Keyboard)
          return;

        e.Cancel = true;
        ((ToolStripDropDownMenu) sender).Invalidate();
      };
      contextMenuStripFilter.KeyPress += ContextMenuStripFilter_KeyPress;
      SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
    }

    /// <summary>
    ///   Gets the current filter.
    /// </summary>
    /// <value>The current filter.</value>
    public string CurrentFilter
    {
      get
      {
        if (m_BindingSource != null)
          return m_BindingSource.Filter;
        return DataView?.RowFilter;
      }
    }

    /// <summary>
    ///   Gets or sets the data source that the <see cref="DataGridView" /> is displaying data for.
    /// </summary>
    /// <returns>The object that contains data for the <see cref="DataGridView" /> to display.</returns>
    /// <exception cref="Exception">
    ///   An error occurred in the data source and either there is no handler for the
    ///   <see
    ///     cref="DataError" />
    ///   event or the handler has set the <see cref="Exception" /> property to
    ///   true. The exception object can typically be cast to type <see cref="FormatException" />.
    /// </exception>
    public new object DataSource
    {
      get => base.DataSource;

      set
      {
        if (value == null)
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
    public IFileSetting FileSetting
    {
      private get;
      set;
    }

    public FillGuessSettings FillGuessSettings
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
    public string HighlightText
    {
      set => m_HighlightText = value ?? string.Empty;
    }

    /// <summary>
    ///   The current DataView
    /// </summary>
    internal DataView DataView { get; private set; }

    private void FilteredDataGridView_Paint(object sender, PaintEventArgs e) => m_DefRowHeight =
      TextRenderer.MeasureText(e.Graphics, "My Text", base.Font).Height * 120 / 100;

    /// <summary>
    ///   Occurs when the next result should be shown
    /// </summary>
    public event EventHandler DataViewChanged;

    /// <summary>
    ///   Applies the filters.
    /// </summary>
    public void ApplyFilters()
    {
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        var filter = new StringBuilder();

        foreach (var toolStripFilter in m_Filter)
        {
          if (toolStripFilter == null)
            continue;
          var filterLogic = toolStripFilter.ColumnFilterLogic;
          if (!filterLogic.Active || string.IsNullOrEmpty(filterLogic.FilterExpression))
            continue;
          if (filter.Length > 0)
            filter.Append("\nAND\n");
          filter.Append("(" + filterLogic.FilterExpression + ")");
        }

        var bindingSourceFilter = filter.ToString();
        toolStripMenuItemFilterRemoveAllFilter.Enabled = bindingSourceFilter.Length > 0;

        // Apply the filter only if any changes occurred
        try
        {
          if (m_BindingSource != null)
          {
            if (bindingSourceFilter.Equals(m_BindingSource.Filter, StringComparison.Ordinal))
              return;
            m_BindingSource.Filter = bindingSourceFilter;
            DataViewChanged?.Invoke(this, null);
          }
          else
          {
            if (DataView == null ||
                bindingSourceFilter.Equals(DataView.RowFilter, StringComparison.Ordinal))
              return;
            DataView.RowFilter = bindingSourceFilter;
            DataViewChanged?.Invoke(this, null);
          }
        }
        catch (Exception ex)
        {
          Debug.WriteLine(ex.InnerExceptionMessages());
        }
      }
      finally
      {
        Cursor.Current = oldCursor;
      }
    }

    /// <summary>
    ///   Hides empty columns in the Data Grid
    /// </summary>
    /// <returns><c>true</c> if column visibility has changed</returns>
    public bool HideEmptyColumns()
    {
      if (Columns.Count == 0)
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

    /// <summary>
    ///   Sets the height of the row.
    /// </summary>
    public void SetRowHeight()
    {
      // Determine each column that could contain a text and is not hidden
      var visible = new List<DataGridViewColumn>();
      foreach (DataGridViewColumn column in Columns)
        if (column.Visible && column.ValueType == typeof(string))
          visible.Add(column);

      // Need to stop after some time, this can take a long time
      var start = DateTime.Now;
      var lastRefresh = start;
      foreach (DataGridViewRow row in Rows)
      {
        if ((DateTime.Now - start).Ticks > 20000000)
          break;
        if ((DateTime.Now - lastRefresh).TotalSeconds > 0.2)
        {
          lastRefresh = DateTime.Now;
          Extensions.ProcessUIElements();
        }

        row.Height = GetDesiredRowHeight(row, visible);
      }
    }

    /// <summary>
    ///   Updated the Columns CheckedListBoxControl according to the visibility of the columns
    /// </summary>
    /// <returns><c>true</c> if something was updated</returns>
    internal bool ColumnVisibilityChanged()
    {
      var hasChanges = false;
      var showHideAll = false;
      var showShowAll = false;
      foreach (DataGridViewColumn col in Columns)
        if (col.Visible)
          showHideAll = true;
        else
          showShowAll = true;

      toolStripMenuItemHideAllColumns.Enabled = showHideAll;
      toolStripMenuItemShowAllColumns.Enabled = showShowAll;

      toolStripMenuItemColumnVisibility.ItemCheck -= CheckedListBox_ItemCheck;

      // update the checked state of the ToolStripMenuItem
      foreach (DataGridViewColumn col in Columns)
      {
        var itemIndex = ColumnDisplayMenuItemAdd(col.DataPropertyName);
        if (itemIndex <= -1)
          continue;
        if (col.Visible ==
            toolStripMenuItemColumnVisibility.CheckedListBoxControl.CheckedIndices.Contains(itemIndex))
          continue;

        toolStripMenuItemColumnVisibility.CheckedListBoxControl.SetItemChecked(itemIndex, col.Visible);
        hasChanges = true;
      }

      toolStripMenuItemColumnVisibility.ItemCheck += CheckedListBox_ItemCheck;
      return hasChanges;
    }

    /// <summary>
    ///   Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      CloseFilter();
      if (m_DisposedValue)
        return;
      if (disposing)
      {
        m_DisposedValue = true;
        components?.Dispose();
        m_CancellationTokenSource?.Dispose();
      }

      base.Dispose(disposing);
    }

    /// <summary>
    ///   Determine a default column with based on the data type and the values in provided
    /// </summary>
    /// <param name="col"></param>
    /// <param name="rowCollection"></param>
    /// <returns>A number for DataGridViewColumn.With</returns>
    private static int GetColumnWith(DataColumn col, DataRowCollection rowCollection)
    {
      const int c_WidthInt = 25;
      const int c_WidthDec = 50;
      const int c_WidthDate = 110;
      const int c_WidthTextMed = 225;
      const int c_WidthTextLong = 350;

      if (col.DataType == typeof(int) || col.DataType == typeof(bool) || col.DataType == typeof(long))
        return c_WidthInt;
      if (col.DataType == typeof(decimal))
        return c_WidthDec;
      if (col.DataType == typeof(DateTime))
        return c_WidthDate;
      if (col.DataType == typeof(string))
      {
        var remain = 30;
        foreach (DataRow dataRow in rowCollection)
        {
          if (dataRow[col] != DBNull.Value)
          {
            if (dataRow[col].ToString().Length > 80)
              return c_WidthTextLong;
            if (dataRow[col].ToString().Length > 15)
              return c_WidthTextMed;
          }

          if (remain-- < 0)
            break;
        }
      }

      return 100;
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
      if (checkedColumns.Any(
        column => row.Cells[column.Index].Value != null
                  && row.Cells[column.Index].Value.ToString().IndexOf('\n') != -1))
        return m_DefRowHeight * 2;

      return m_DefRowHeight;
    }

    /// <summary>
    ///   Handles the ItemCheck event of the CheckedListBox control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="ItemCheckEventArgs" /> instance containing the event data.</param>
    private void CheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
    {
      timerColumsFilter.Stop();
      timerColumsFilter.Start();
    }

    /// <summary>
    ///   Closes the filter.
    /// </summary>
    private void CloseFilter()
    {
      for (var i = 0; i < m_Filter.Count; i++)
      {
        if (m_Filter[i] == null) continue;
        try
        {
          m_Filter[i].Dispose();
        }
        finally
        {
          m_Filter[i] = null;
        }
      }
    }

    /// <summary>
    ///   Columns the display menu item add.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns></returns>
    private int ColumnDisplayMenuItemAdd(string text)
    {
      if (!Columns.Contains(text))
        return -1;
      var itemIndex = ColumnDisplayMenuItemFind(text);

      // if we have the column already do not do anything
      if (itemIndex != -1)
        return itemIndex;
      itemIndex = toolStripMenuItemColumnVisibility.CheckedListBoxControl.Items.Add(text);

      // ReSharper disable once PossibleNullReferenceException
      toolStripMenuItemColumnVisibility.CheckedListBoxControl.SetItemChecked(itemIndex, Columns[text].Visible);
      return itemIndex;
    }

    /// <summary>
    ///   Columns the display menu item find.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns></returns>
    private int ColumnDisplayMenuItemFind(string text)
    {
      if (toolStripMenuItemColumnVisibility?.CheckedListBoxControl?.Items != null)
        return toolStripMenuItemColumnVisibility.CheckedListBoxControl.Items.IndexOf(text);
      return -1;
    }

    /// <summary>
    ///   Columns the display menu item remove.
    /// </summary>
    /// <param name="text">The text.</param>
    private void ColumnDisplayMenuItemRemove(string text)
    {
      var itemIndex = ColumnDisplayMenuItemFind(text);
      if (itemIndex > -1)
        toolStripMenuItemColumnVisibility.CheckedListBoxControl.Items.RemoveAt(itemIndex);
    }

    /// <summary>
    ///   Handles the KeyPress event of the contextMenuStripFilter control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///   The <see cref="System.Windows.Forms.KeyPressEventArgs" /> instance containing the event data.
    /// </param>
    private void ContextMenuStripFilter_KeyPress(object sender, KeyPressEventArgs e)
    {
      if (e.KeyChar != 13)
        return;
      ToolStripMenuItemFilterRemoveOne_Click(sender, null);
      e.Handled = true;
    }

    private void ContextMenuStripFilter_Opened(object sender, EventArgs e)
    {
      // Set the focus to
      if (contextMenuStripFilter.Items[0] is ToolStripDataGridViewColumnFilter op)
        ((DataGridViewColumnFilterControl) op.Control).FocusInput();
    }

    /// <summary>
    ///   Shows the pop up when user right-clicks a column header
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">
    ///   The <see cref="System.Windows.Forms.DataGridViewCellMouseEventArgs" /> instance containing
    ///   the event data.
    /// </param>
    private void FilteredDataGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
    {
      try
      {
        m_MenuItemColumnIndex = e.ColumnIndex;
        if (e.Button == MouseButtons.Right && e.ColumnIndex > -1)
        {
          SetFilterMenu(e.ColumnIndex);
          toolStripMenuItemRemoveOne.Enabled = m_Filter[e.ColumnIndex].ColumnFilterLogic.Active;
        }

        if (e.Button == MouseButtons.Right && e.RowIndex == -1)
        {
          toolStripMenuItemFreeze.Text = Columns[e.ColumnIndex].Frozen ? "Unfreeze" : "Freeze";

          toolStripMenuItemFilterAdd.Enabled = e.ColumnIndex > -1;
          toolStripMenuItemSortAscending.Enabled = e.ColumnIndex > -1;
          toolStripMenuItemSortDescending.Enabled = e.ColumnIndex > -1;

          toolStripMenuItemSortAscending.Text = e.ColumnIndex > -1
            ? string.Format(
              CultureInfo.CurrentCulture,
              toolStripMenuItemSortAscending.Tag.ToString(),
              Columns[e.ColumnIndex].DataPropertyName)
            : "Sort ascending";
          toolStripMenuItemSortDescending.Text = e.ColumnIndex > -1
            ? string.Format(
              CultureInfo.CurrentCulture,
              toolStripMenuItemSortDescending.Tag.ToString(),
              Columns[e.ColumnIndex].DataPropertyName)
            : "Sort descending";
          var columnFormat = GetColumnFormat(e.ColumnIndex);
          toolStripMenuItemCF.Visible = columnFormat != null;
          toolStripSeparatorCF.Visible = columnFormat != null;
          if (columnFormat != null)
            toolStripMenuItemCF.Text = $@"Change column format: {columnFormat.ValueFormat.DataType.DataTypeDisplay()}";

          toolStripMenuItemRemoveOne.Enabled &= e.ColumnIndex != -1;
          contextMenuStripHeader.Show(Cursor.Position);
        }

        if (e.Button != MouseButtons.Right || e.RowIndex <= -1 || e.ColumnIndex <= -1)
          return;
        CurrentCell = Rows[e.RowIndex].Cells[e.ColumnIndex];
        contextMenuStripCell.Show(Cursor.Position);
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.InnerExceptionMessages());
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
    private void FilteredDataGridView_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
    {
      // Make sure we have enough in the Po pup List
      while (m_Filter.Count < Columns.Count)
        m_Filter.Add(null);
      if (e.Column.ValueType != typeof(string))
      {
        e.Column.DefaultCellStyle.ForeColor = Color.MidnightBlue;
        e.Column.DefaultCellStyle.Alignment = e.Column.ValueType == typeof(bool)
          ? DataGridViewContentAlignment.MiddleCenter
          : DataGridViewContentAlignment.MiddleRight;
      }

      ColumnDisplayMenuItemAdd(e.Column.DataPropertyName);
    }

    /// <summary>
    ///   Handles the ColumnRemoved event of the FilteredDataGridView control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///   The <see cref="System.Windows.Forms.DataGridViewColumnEventArgs" /> instance containing
    ///   the event data.
    /// </param>
    private void FilteredDataGridView_ColumnRemoved(object sender, DataGridViewColumnEventArgs e)
    {
      ColumnDisplayMenuItemRemove(e.Column.DataPropertyName);

      if (m_Filter.Count <= e.Column.Index)
        return;

      m_Filter[e.Column.Index] = null;
    }

    /// <summary>
    ///   Handles the ColumnWidthChanged event of the FilteredDataGridView control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///   The <see cref="System.Windows.Forms.DataGridViewColumnEventArgs" /> instance containing
    ///   the event data.
    /// </param>
    private void FilteredDataGridView_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
    {
      if (e.Column.Width > Width)
        e.Column.Width = Width;
    }

    private void FilteredDataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
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
    private void FilteredDataGridView_KeyDown(object sender, KeyEventArgs e)
    {
      if (!e.Control || e.KeyCode != Keys.C)
        return;
      this.SelectedDataIntoClipboard(!e.Alt, e.Shift, m_CancellationTokenSource.Token);
      e.Handled = true;
    }

    /// <summary>
    ///   Generates the data grid view column.
    /// </summary>
    private void GenerateDataGridViewColumn()
    {
      ColumnRemoved -= FilteredDataGridView_ColumnRemoved;
      var oldWith = new Dictionary<string, int>();

      // close and remove all pop ups
      CloseFilter();

      foreach (DataGridViewColumn column in Columns)
        if (!oldWith.ContainsKey(column.DataPropertyName))
          oldWith.Add(column.DataPropertyName, column.Width);

      // remove all columns
      Columns.Clear();

      // along with the entries in the context menu
      toolStripMenuItemColumnVisibility.CheckedListBoxControl?.Items.Clear();

      // if we do not have a BoundDataView exit now
      if (DataView == null)
        return;

      foreach (DataColumn col in DataView.Table.Columns)
      {
        DataGridViewColumn newColumn;

        if (col.DataType == typeof(bool))
          newColumn = new DataGridViewCheckBoxColumn();
        else
          newColumn = new DataGridViewTextBoxColumn();

        newColumn.ValueType = col.DataType;
        newColumn.Name = col.ColumnName;
        newColumn.DataPropertyName = col.ColumnName;

        if (oldWith.ContainsKey(col.ColumnName))
          newColumn.Width = oldWith[col.ColumnName];

        foreach (DataRow row in DataView.Table.Rows)
        {
          var cellValue = row[col];
          if (cellValue == null || cellValue.ToString().IndexOf('\n') == -1)
            continue;
          newColumn.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
          break;
        }

        newColumn.Width = GetColumnWith(col, DataView.Table.Rows);
        Columns.Add(newColumn);
      }

      ColumnRemoved += FilteredDataGridView_ColumnRemoved;
    }

    /// <summary>
    ///   Gets the column format.
    /// </summary>
    /// <param name="colIndex">The column index.</param>
    /// <returns></returns>
    private Column GetColumnFormat(int colIndex)
    {
      if (FileSetting == null || colIndex < 0 || colIndex > FileSetting.ColumnCollection.Count)
        return null;

      return FileSetting.ColumnCollection.Get(Columns[colIndex].DataPropertyName);
    }

    /// <summary>
    ///   Does all cell painting, doing highlighting
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">
    ///   The <see cref="DataGridViewCellPaintingEventArgs" /> instance containing the event data.
    /// </param>
    private void HighlightCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
    {
      if (e.RowIndex == -1 && e.ColumnIndex >= 0 && m_Filter[e.ColumnIndex] != null
          && m_Filter[e.ColumnIndex].ColumnFilterLogic.Active)
      {
        e.Handled = true;
        e.PaintBackground(e.CellBounds, true);

        // Display a Filter Symbol
        var pt = e.CellBounds.Location;
        var offset = e.CellBounds.Width - 22;
        pt.X += offset;
        pt.Y = e.CellBounds.Height / 2 - 4;
        e.Graphics.DrawImageUnscaled(m_ImgFilterIndicator, pt);

        e.PaintContent(e.CellBounds);
      }

      if (e.RowIndex < 0 || e.ColumnIndex < 0)
        return;
      var val = e.FormattedValue.ToString();
      if (string.IsNullOrEmpty(val))
        return;

      var nbspIndex = val.IndexOf((char) 0xA0);
      var linefeedIndex = val.IndexOf('\n');
      var highlightIndex = m_HighlightText.Length > 0
        ? val.IndexOf(m_HighlightText, StringComparison.InvariantCultureIgnoreCase)
        : -1;

      if (nbspIndex == -1 && highlightIndex == -1)
        return;

      e.Handled = true;
      e.PaintBackground(e.CellBounds, true);

      if (nbspIndex >= 0 && (linefeedIndex == -1 || nbspIndex < linefeedIndex)
                         && e.CellStyle.Alignment == DataGridViewContentAlignment.MiddleLeft)
      {
        var hlRect = new Rectangle();
        var widthSpace = TextRenderer.MeasureText(e.Graphics, @" ", e.CellStyle.Font, e.ClipBounds.Size).Width;
        // Only do this as long the NBSP is before a linefeed
        while (nbspIndex >= 0 && (linefeedIndex == -1 || nbspIndex < linefeedIndex))
        {
          if (linefeedIndex == -1)

            // Middle Alignment (this goes wrong if the have a linefeed)
            hlRect.Y = e.CellBounds.Top + e.CellBounds.Height / 2 - 2;
          else
            hlRect.Y = e.CellBounds.Top + Font.Height - 4;

          var before = val.Substring(0, nbspIndex);
          if (before.Length > 0)
            hlRect.X = e.CellBounds.X
              + TextRenderer.MeasureText(e.Graphics, before, e.CellStyle.Font, e.CellBounds.Size)
                .Width - 6;
          else
            hlRect.X = e.CellBounds.X;

          // if we are outside the bound stop
          if (hlRect.X > e.CellBounds.X + e.CellBounds.Width)
            break;
          e.Graphics.DrawLines(new Pen(Brushes.LightSalmon, 2), new[]
          {
            new Point(hlRect.X, e.CellBounds.Bottom - 10),
            new Point(hlRect.X, e.CellBounds.Bottom - 5),
            new Point(hlRect.X + widthSpace, e.CellBounds.Bottom - 5),
            new Point(hlRect.X + widthSpace, e.CellBounds.Bottom - 10)
          });
          nbspIndex = val.IndexOf((char) 0xA0, nbspIndex + 1);
        }
      }

      if (m_HighlightText.Length > 0
          && (e.CellStyle.Alignment == DataGridViewContentAlignment.MiddleLeft
              || e.CellStyle.Alignment == DataGridViewContentAlignment.MiddleRight) && highlightIndex >= 0)
        using (var hlBrush = new SolidBrush(Color.MediumSpringGreen))
        {
          var hlRect = new Rectangle();
          while (highlightIndex >= 0 && (linefeedIndex == -1 || highlightIndex < linefeedIndex))
          {
            hlRect.Y = e.CellBounds.Y + 2;

            // hl_rect.Height = e.CellBounds.Height - 5;
            var highlight = TextRenderer.MeasureText(
              e.Graphics,
              val.Substring(highlightIndex, m_HighlightText.Length),
              e.CellStyle.Font,
              e.CellBounds.Size);
            hlRect.Height = highlight.Height + 2;
            hlRect.Width = highlight.Width - 6;

            if (e.CellStyle.Alignment == DataGridViewContentAlignment.MiddleLeft)
            {
              var before = val.Substring(0, highlightIndex);
              if (before.Length > 0)
                hlRect.X = e.CellBounds.X + TextRenderer.MeasureText(
                  e.Graphics,
                  before,
                  e.CellStyle.Font,
                  e.CellBounds.Size).Width - 4;
              else
                hlRect.X = e.CellBounds.X + 2;
            }
            else
            {
              var after = val.Substring(highlightIndex + m_HighlightText.Length);
              if (after.Length > 0)
                hlRect.X = e.CellBounds.X + e.CellBounds.Width
                  - TextRenderer.MeasureText(e.Graphics, after, e.CellStyle.Font,
                    e.CellBounds.Size).Width
                  - hlRect.Width + 3;
              else
                hlRect.X = e.CellBounds.X + e.CellBounds.Width - hlRect.Width - 4;
            }

            e.Graphics.FillRectangle(hlBrush, hlRect);
            highlightIndex = val.IndexOf(
              m_HighlightText,
              highlightIndex + m_HighlightText.Length,
              StringComparison.InvariantCultureIgnoreCase);
          }
        }

      // paint the content as usual
      e.PaintContent(e.CellBounds);
    }

    /// <summary>
    ///   Resets the data source.
    /// </summary>
    private void ResetDataSource()
    {
      CloseFilter();
      Columns.Clear();
      base.DataSource = null;
      DataMember = null;
      DataView = null;
    }

    /// <summary>
    ///   Checks if the DataGridView is data bound and the data source finally resolves to a DataView.
    /// </summary>
    /// <param name="force">if set to <c>true</c> [force].</param>
    private void SetBoundDataView(bool force)
    {
      if (DataView != null && !force)
        return;
      m_BindingSource = null;
      var dataSource = DataSource;
      var dataMember = DataMember;
      var maxIteration = 5;

      while (dataSource != null && !(dataSource is DataView) && maxIteration > 0)
      {
        if (dataSource is BindingSource bs)
        {
          m_BindingSource = (BindingSource) DataSource;
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

      try
      {
        DataView = (DataView) dataSource;
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.InnerExceptionMessages());
      }
    }

    private ToolStripDataGridViewColumnFilter GetColumnFilter(int columnIndex)
    {
      if (m_Filter[columnIndex] == null)
      {
        m_Filter[columnIndex] = new ToolStripDataGridViewColumnFilter(Columns[columnIndex]);
        if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor > 1)
          m_Filter[columnIndex].Control.Font = Font;

        // as the Operator is set the filter becomes active, revoke this
        m_Filter[columnIndex].ColumnFilterLogic.Active = false;
        m_Filter[columnIndex].ColumnFilterLogic.ColumnFilterApply += ToolStripMenuItemApply_Click;
      }

      return m_Filter[columnIndex];
    }

    /// <summary>
    ///   Build the Column Filter the given Column
    /// </summary>
    /// <param name="columnIndex"></param>
    public void SetFilterMenu(int columnIndex)
    {
      if (DataView == null)
        return;
      contextMenuStripFilter.Close();
      contextMenuStripFilter.SuspendLayout();

      if (m_Filter[columnIndex] == null)
        GetColumnFilter(columnIndex);

      while (!(contextMenuStripFilter.Items[0] is ToolStripSeparator))
        contextMenuStripFilter.Items.RemoveAt(0);

      contextMenuStripFilter.Items.Insert(0, m_Filter[columnIndex]);

      var col = m_Filter[columnIndex].ValueClusterCollection;

      var result = col.BuildValueClusters(DataView, Columns[columnIndex].ValueType, columnIndex);
      {
        var newMenuItem = new ToolStripMenuItem();
        switch (result)
        {
          case BuildValueClustersResult.Error:
            newMenuItem.Text = @"Values can not be clustered";
            newMenuItem.ToolTipText = @"Error has occured while clustering the value";
            break;

          case BuildValueClustersResult.WrongType:
            newMenuItem.Text = @"Type can not be clustered";
            newMenuItem.ToolTipText = @"This type of column can not be filter by value";
            break;

          case BuildValueClustersResult.TooManyValues:
            newMenuItem.Text = @"More than 40 values";
            newMenuItem.ToolTipText = @"Too many different values found to list them all";
            break;

          case BuildValueClustersResult.NoValues:
            newMenuItem.Text = @"No values";
            newMenuItem.ToolTipText = @"This column is empty";
            break;

          case BuildValueClustersResult.ListFilled:
            newMenuItem.Text = @"Any of:";
            newMenuItem.ToolTipText = @"Check all values you want to filter for";
            break;
        }

        newMenuItem.Enabled = false;
        contextMenuStripFilter.Items.Insert(1, newMenuItem);
      }

      foreach (var item in col.ValueClusters.OrderByDescending(x => x.Sort))
      {
        var newMenuItem = new ToolStripMenuItem(StringUtils.GetShortDisplay(item.Display, 40))
        {
          Tag = item,
          Checked = item.Active,
          CheckOnClick = true
        };
        newMenuItem.CheckStateChanged += delegate(object menuItem, EventArgs args)
        {
          if (!(menuItem is ToolStripMenuItem sendItem))
            return;
          if (sendItem.Tag is ValueCluster itemVc)
            itemVc.Active = sendItem.Checked;
        };
        if (item.Count > 0)
          newMenuItem.ShortcutKeyDisplayString = $@"{item.Count} items";
        contextMenuStripFilter.Items.Insert(2, newMenuItem);
      }

      contextMenuStripFilter.ResumeLayout(true);
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
    private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
      if (e.Category == UserPreferenceCategory.Locale)
        CultureInfo.CurrentCulture.ClearCachedData();
    }

    private void TimerColumnsFilter_Tick(object sender, EventArgs e)
    {
      var changes = false;
      timerColumsFilter.Stop();
      toolStripMenuItemColumnVisibility.CheckedListBoxControl.SafeInvoke(
        () =>
        {
          foreach (DataGridViewColumn col in Columns)
          {
            if (!col.Visible)
              continue;
            for (var i = 0; i < toolStripMenuItemColumnVisibility.CheckedListBoxControl.Items.Count; i++)
              if (toolStripMenuItemColumnVisibility.CheckedListBoxControl.Items[i].ToString()
                .Equals(col.Name, StringComparison.OrdinalIgnoreCase))
              {
                if (!toolStripMenuItemColumnVisibility.CheckedListBoxControl.GetItemChecked(i))
                {
                  col.Visible = false;
                  changes = true;
                }

                break;
              }
          }

          foreach (int index in toolStripMenuItemColumnVisibility.CheckedListBoxControl.CheckedIndices)
          {
            var itemName = toolStripMenuItemColumnVisibility.CheckedListBoxControl.Items[index].ToString();
            var dataGridViewColumn = Columns[itemName];
            if (dataGridViewColumn?.Visible ?? true)
              continue;
            dataGridViewColumn.Visible = true;
            changes = true;
          }

          if (!changes)
            return;
          SetRowHeight();
          DataViewChanged?.Invoke(null, null);
        });
    }

    /// <summary>
    ///   Handles the Click event of the toolStripMenuItemApply control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ToolStripMenuItemApply_Click(object sender, EventArgs e)
    {
      if (m_Filter[m_MenuItemColumnIndex] != null)
        m_Filter[m_MenuItemColumnIndex].ColumnFilterLogic.Active = true;

      ApplyFilters();
      contextMenuStripCell.Close();
      contextMenuStripHeader.Close();
      contextMenuStripFilter.Close();
    }

    /// <summary>
    ///   Handles the Click event of the toolStripMenuItemCF control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ToolStripMenuItemCF_Click(object sender, EventArgs e)
    {
      var columnFormat = GetColumnFormat(m_MenuItemColumnIndex);
      if (columnFormat == null)
        return;
      using (var form = new FormColumnUI(columnFormat, false, FileSetting, FillGuessSettings, false))
      {
        if (form.ShowDialog() == DialogResult.Yes)
          Refresh();
      }
    }

    /// <summary>
    ///   Handles the Click event of the toolStripMenuItemCopy control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ToolStripMenuItemCopy_Click(object sender, EventArgs e) =>
      this.SelectedDataIntoClipboard(false, false, m_CancellationTokenSource.Token);

    /// <summary>
    ///   Handles the Click event of the toolStripMenuItemCopyError control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ToolStripMenuItemCopyError_Click(object sender, EventArgs e) =>
      this.SelectedDataIntoClipboard(true, false, m_CancellationTokenSource.Token);

    /// <summary>
    ///   Handles the Click event of the toolStripMenuItemFilled control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ToolStripMenuItemFilled_Click(object sender, EventArgs e)
    {
      try
      {
        if (!HideEmptyColumns())
          return;
        if (!ColumnVisibilityChanged())
          return;
        SetRowHeight();
        DataViewChanged?.Invoke(sender, null);
      }
      catch
      {
        // ignored
      }
    }

    /// <summary>
    ///   Handles the Click event of the toolStripMenuItemFilterRemoveAll control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ToolStripMenuItemFilterRemoveAll_Click(object sender, EventArgs e)
    {
      try
      {
        foreach (var toolStripFilter in m_Filter.Where(
          toolStripFilter => toolStripFilter != null && toolStripFilter.ColumnFilterLogic.Active))
          toolStripFilter.ColumnFilterLogic.Active = false;

        ApplyFilters();
      }
      catch
      {
        // ignored
      }
    }

    /// <summary>
    ///   Handles the Click event of the toolStripMenuItemFilterRemove control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ToolStripMenuItemFilterRemoveOne_Click(object sender, EventArgs e)
    {
      try
      {
        if (m_Filter[m_MenuItemColumnIndex] == null ||
            !m_Filter[m_MenuItemColumnIndex].ColumnFilterLogic.Active)
          return;

        m_Filter[m_MenuItemColumnIndex].ColumnFilterLogic.Active = false;
        ApplyFilters();
      }
      catch
      {
        // ignored
      }
    }

    /// <summary>
    ///   Handles the Click event of the toolStripMenuItemFilterValue control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ToolStripMenuItemFilterValue_Click(object sender, EventArgs e)
    {
      try
      {
        if (CurrentCell == null)
          return;

        m_Filter[m_MenuItemColumnIndex].ColumnFilterLogic.SetFilter(CurrentCell.Value);
        if (!m_Filter[m_MenuItemColumnIndex].ColumnFilterLogic.Active)
          m_Filter[m_MenuItemColumnIndex].ColumnFilterLogic.Active = true;
        ApplyFilters();
      }
      catch
      {
        // ignored
      }
    }

    private void ToolStripMenuItemFreeze_Click(object sender, EventArgs e)
    {
      // move to front
      if (!Columns[m_MenuItemColumnIndex].Frozen)
      {
        var colFirstNoFrozen =
          (from col in Columns.OfType<DataGridViewColumn>().OrderBy(x => x.DisplayIndex)
            where !col.Frozen
            select col.DisplayIndex).FirstOrDefault();
        Columns[m_MenuItemColumnIndex].DisplayIndex = colFirstNoFrozen;
      }

      Columns[m_MenuItemColumnIndex].Frozen = !Columns[m_MenuItemColumnIndex].Frozen;
    }

    /// <summary>
    ///   Handles the Click event of the toolStripMenuItemAllCol control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ToolStripMenuItemHideAllColumns_Click(object sender, EventArgs e)
    {
      // keep one column visible, otherwise we have an issue with the grid being empty
      foreach (DataGridViewColumn col in Columns)
        if (col.Visible && col.Index != m_MenuItemColumnIndex)
          col.Visible = false;

      if (!ColumnVisibilityChanged())
        return;
      SetRowHeight();
      DataViewChanged?.Invoke(sender, null);
    }

    /// <summary>
    ///   Handles the Click event of the toolStripMenuItemAllCol control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ToolStripMenuItemShowAllColumns_Click(object sender, EventArgs e)
    {
      foreach (DataGridViewColumn col in Columns)
        if (!col.Visible)
          col.Visible = true;

      if (!ColumnVisibilityChanged())
        return;
      SetRowHeight();
      DataViewChanged?.Invoke(sender, null);
    }

    /// <summary>
    ///   Handles the Click event of the toolStripMenuItemSortAscending control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ToolStripMenuItemSortAscending_Click(object sender, EventArgs e) =>

      // Column was set on showing context menu
      Sort(Columns[m_MenuItemColumnIndex], ListSortDirection.Ascending);

    /// <summary>
    ///   Handles the Click event of the toolStripMenuItemSortDescending control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ToolStripMenuItemSortDescending_Click(object sender, EventArgs e) =>

      // Column was set on showing context menu
      Sort(Columns[m_MenuItemColumnIndex], ListSortDirection.Descending);

    private void ToolStripMenuItemSortRemove_Click(object sender, EventArgs e) => DataView.Sort = string.Empty;

    public void ReStoreViewSetting(string fileName)
    {
      if (string.IsNullOrEmpty(fileName) || !FileSystemUtils.FileExists(fileName)) return;

      var stream = FileSystemUtils.OpenRead(fileName);
      using (var reader = new StreamReader(stream, Encoding.UTF8, true))
      {
        if (ViewSetting.ReStoreViewSetting(reader.ReadToEnd(), Columns, m_Filter, GetColumnFilter, Sort))
          ApplyFilters();
      }
    }

    private void ToolStripMenuItemLoadCol_Click(object sender, EventArgs e)
    {
      try
      {
        toolStripMenuItemLoadCol.Enabled = false;
        var fileName = WindowsAPICodePackWrapper.Open(
          FileSetting is IFileSettingPhysicalFile phy ? phy.FullPath.GetDirectoryName() : ".", "Load Column Setting",
          "Column Config|*.col;*.conf|All files|*.*", DefFileNameColSetting(FileSetting, ".col"));
        ReStoreViewSetting(fileName);
      }
      catch (Exception ex)
      {
        FindForm()?.ShowError(ex);
      }
      finally
      {
        toolStripMenuItemLoadCol.Enabled = true;
      }
    }

    private static string DefFileNameColSetting(IFileSetting fileSetting, string extension)
    {
      var defFileName = fileSetting.ID;
      var index = defFileName.LastIndexOf('.');
      return (index == -1 ? defFileName : defFileName.Substring(0, index)) + extension;
    }

    private async void ToolStripMenuItemSaveCol_Click(object sender, EventArgs e)
    {
      try
      {
        toolStripMenuItemSaveCol.Enabled = false;

        // Select Path
        var fileName = WindowsAPICodePackWrapper.Save(
          FileSetting is IFileSettingPhysicalFile phy ? phy.FullPath.GetDirectoryName() : ".", "Save Column Setting",
          "Column Config|*.col;*.conf|All files|*.*", ".col", DefFileNameColSetting(FileSetting, ".col"));
        if (!string.IsNullOrEmpty(fileName))
          using (var stream = ImprovedStream.OpenWrite(fileName))
          using (var writer = new StreamWriter(stream.Stream, Encoding.UTF8, 1024))
          {
            await writer.WriteAsync(ViewSetting.StoreViewSetting(Columns, m_Filter, SortedColumn, SortOrder));
          }
      }
      catch (Exception ex)
      {
        FindForm()?.ShowError(ex);
      }
      finally
      {
        toolStripMenuItemSaveCol.Enabled = true;
      }
    }

    private void ToolStripMenuItemHideThisColumn_Click(object sender, EventArgs e)
    {
      try
      {
        if (!Columns.Cast<DataGridViewColumn>().Any(col => col.Visible && col.Index != m_MenuItemColumnIndex)) return;
        Columns[m_MenuItemColumnIndex].Visible = false;
        ColumnVisibilityChanged();
      }
      catch
      {
        // ignored
      }
    }
  }
}