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
using System.Diagnostics;
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
    private readonly List<ToolStripDataGridViewColumnFilter?> m_Filter;
    private BindingSource? m_BindingSource;

    private bool m_DisposedValue;
    private IFileSetting? m_FileSetting;
    private int m_ShowButtonAtLength = 1000;
    private int m_MenuItemColumnIndex;

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
      if (e.Value is not DateTime cellValue)
        return;

      e.Value = StringConversion.DisplayDateTime(cellValue, CultureInfo.CurrentCulture);
    }

    public FilteredDataGridView()
    {
      m_CancellationTokenSource = new CancellationTokenSource();

      InitializeComponent();
#pragma warning disable CA1416
      CellFormatting += CellFormatDate;
#pragma warning restore CA1416
      FontChanged += PassOnFontChanges;
      m_Filter = new List<ToolStripDataGridViewColumnFilter?>();

      Scroll += (_, _) => SetRowHeight();
      var resources = new ComponentResourceManager(typeof(FilteredDataGridView));
      m_ImgFilterIndicator = (resources.GetObject("toolStripMenuItem2.Image") as Image) ??
                             throw new InvalidOperationException("Resource not found");

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
      contextMenuStripFilter.Closing += delegate (object? sender, ToolStripDropDownClosingEventArgs e)
      {
        if (e.CloseReason != ToolStripDropDownCloseReason.AppClicked
            && e.CloseReason != ToolStripDropDownCloseReason.ItemClicked
            && e.CloseReason != ToolStripDropDownCloseReason.Keyboard)
          return;

        e.Cancel = true;
        if (sender is ToolStripDropDownMenu downMenu)
          downMenu.Invalidate();
      };
      contextMenuStripFilter.KeyPress += ContextMenuStripFilter_KeyPress;
#pragma warning disable CA1416
      SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
#pragma warning restore CA1416

      FontChanged += (_, _) =>
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
        toolStripMenuItemSaveCol.Enabled = m_FileSetting != null;
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
    ///   Applies the filters.
    /// </summary>
    public void ApplyFilters() =>
      this.RunWithHourglass(() =>
        {
          var filter = new StringBuilder();
          foreach (var filterLogic in from toolStripFilter in m_Filter
                                      where toolStripFilter != null
                                      select toolStripFilter.ColumnFilterLogic
                   into filterLogic
                                      where filterLogic.Active && !string.IsNullOrEmpty(filterLogic.FilterExpression)
                                      select filterLogic)
          {
            if (filter.Length > 0)
              filter.Append("\nAND\n");
            filter.Append("(" + filterLogic.FilterExpression + ")");
          }

          var bindingSourceFilter = filter.ToString();
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
        var filter = m_Filter[m_MenuItemColumnIndex];
        if (filter is null) return;
        filter.ColumnFilterLogic.SetFilter(CurrentCell.Value);
        if (!filter.ColumnFilterLogic.Active)
          filter.ColumnFilterLogic.Active = true;
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
      if (Columns.Count == 0 || DataView == null)
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
        if (!ColumnVisibilityChanged())
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
        foreach (var toolStripFilter in m_Filter.Where(toolStripFilter =>
                   toolStripFilter?.ColumnFilterLogic.Active ?? false))
          toolStripFilter!.ColumnFilterLogic.Active = false;

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
      if (m_FileSetting is BaseSettingPhysicalFile basePhysical)
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
    ///   Build the Column Filter the given Column
    /// </summary>
    /// <param name="columnIndex"></param>
    public ToolStripDataGridViewColumnFilter? SetFilterMenu(int columnIndex)
    {
      if (DataView is null)
        return null;

      var filter = GetColumnFilter(columnIndex);

      contextMenuStripFilter.Close();
      contextMenuStripFilter.SuspendLayout();
      while (contextMenuStripFilter.Items[0] is not ToolStripSeparator)
        contextMenuStripFilter.Items.RemoveAt(0);

      contextMenuStripFilter.Items.Insert(0, filter);

      var col = filter.ValueClusterCollection;

      var result = col.BuildValueClusters(DataView, Columns[columnIndex].ValueType, columnIndex);
      {
        var newMenuItem = new ToolStripMenuItem();
        switch (result)
        {
          case BuildValueClustersResult.Error:
          case BuildValueClustersResult.NotRun:
            newMenuItem.Text = @"Values can not be clustered";
            newMenuItem.ToolTipText = @"Error has occurred while clustering the value";
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
        var newMenuItem =
          new ToolStripMenuItem(StringUtils.GetShortDisplay(item.Display, 40))
          {
            Tag = item,
            Checked = item.Active,
            CheckOnClick = true
          };
        newMenuItem.CheckStateChanged += delegate (object? menuItem, EventArgs _)
        {
          if (menuItem is not ToolStripMenuItem sendItem)
            return;
          if (sendItem.Tag is ValueCluster itemVc)
            itemVc.Active = sendItem.Checked;
        };
        if (item.Count > 0)
          newMenuItem.ShortcutKeyDisplayString = $@"{item.Count} items";
        contextMenuStripFilter.Items.Insert(2, newMenuItem);
      }

      contextMenuStripFilter.ResumeLayout(true);
      return filter;
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
        {
          var filter = SetFilterMenu(columnIndex);
          toolStripMenuItemRemoveOne.Enabled = filter?.ColumnFilterLogic.Active ?? false;
        }

        if (mouseButtons == MouseButtons.Right && rowIndex == -1)
        {
          toolStripMenuItemFreeze.Text = Columns[columnIndex].Frozen ? "Unfreeze" : "Freeze";

          // toolStripMenuItemFilterAdd.Enabled = columnIndex > -1;
          toolStripMenuItemSortAscending.Enabled = columnIndex > -1;
          toolStripMenuItemSortDescending.Enabled = columnIndex > -1;

#pragma warning disable CS8604 // Possible null reference argument.
          toolStripMenuItemSortAscending.Text = columnIndex > -1
            ? string.Format(
              CultureInfo.CurrentCulture,
              Convert.ToString(toolStripMenuItemSortAscending.Tag),
              Columns[columnIndex].DataPropertyName)
            : "Sort ascending";

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

    public void ShowAllColumns()
    {
      foreach (DataGridViewColumn col in Columns)
        if (!col.Visible)
          col.Visible = true;

      if (!ColumnVisibilityChanged())
        return;
      SetRowHeight();
      DataViewChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///   Updated the Columns CheckedListBoxControl according to the visibility of the columns
    /// </summary>
    /// <returns><c>true</c> if something was updated</returns>
    internal bool ColumnVisibilityChanged()
    {
      var hasChanges = false;

      var showShowAll = false;
      foreach (DataGridViewColumn col in Columns)
        if (!col.Visible)
          showShowAll = true;

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

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      if (m_DisposedValue) return;
      CloseFilter();
      if (disposing)
      {
        m_DisposedValue = true;
        components?.Dispose();
        foreach (var item in m_Filter)
          item?.Dispose();
#pragma warning disable CA1416
        m_ImgFilterIndicator.Dispose();
#pragma warning restore CA1416
        m_CancellationTokenSource.Dispose();
      }

      base.Dispose(disposing);
    }

    private static string DefFileNameColSetting(IFileSetting fileSetting, string extension)
    {
      var defFileName = fileSetting.ID;
      var index = defFileName.LastIndexOf('.');
      return (index == -1 ? defFileName : defFileName.Substring(0, index)) + extension;
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
      if (DataView == null)
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

    /// <summary>
    ///   Handles the ItemCheck event of the CheckedListBox control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="ItemCheckEventArgs" /> instance containing the event data.</param>
    private void CheckedListBox_ItemCheck(object? sender, ItemCheckEventArgs e)
    {
      timerColumsFilterChecked.Stop();
      timerColumsFilterChecked.Start();
    }

    /// <summary>
    ///   Closes the filter.
    /// </summary>
    private void CloseFilter()
    {
      for (var i = 0; i < m_Filter.Count; i++)
      {
        m_Filter[i]?.Dispose();
        m_Filter[i] = null;
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
      if (toolStripMenuItemColumnVisibility?.CheckedListBoxControl.Items != null)
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
    private void ContextMenuStripFilter_KeyPress(object? sender, KeyPressEventArgs e)
    {
      if (e.KeyChar != 13)
        return;
      ToolStripMenuItemFilterRemoveOne_Click(sender, EventArgs.Empty);
      e.Handled = true;
    }

    private void ContextMenuStripFilter_Opened(object? sender, EventArgs e)
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
    private void FilteredDataGridView_CellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
      try
      {
        SetToolStripMenu(e.ColumnIndex, e.RowIndex, e.Button);
        if (e is { Button: MouseButtons.Left, RowIndex: >= 0 } && Columns[e.ColumnIndex] is DataGridViewButtonColumn)
        {
          using var frm = new FormTextDisplay(CurrentCell.Value?.ToString() ?? string.Empty);

          // ReSharper disable once LocalizableElement
          frm.Text = $"{Columns[e.ColumnIndex].DataPropertyName} - Row {e.RowIndex + 1:D}";
          frm.SaveAction = s =>
          {
            if (s.Equals(CurrentCell.Value))
              return;
            CurrentCell.Value = s;
            CurrentCell.ErrorText = CurrentCell.ErrorText.AddMessage(
              "Value was modified".AddWarningId());
          };
          frm.ShowWithFont(this, true);
        }
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
    private void FilteredDataGridView_ColumnRemoved(object? sender, DataGridViewColumnEventArgs e)
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
      if (!e.Control || e.KeyCode != Keys.C)
        return;
      Copy(!e.Alt, e.Shift);
      e.Handled = true;
    }

    /// <summary>
    ///   Generates the data grid view column.
    /// </summary>
    private void GenerateDataGridViewColumn()
    {
      // close and remove all pop ups
      CloseFilter();

      var oldWith = new Dictionary<string, int>();
      foreach (DataGridViewColumn column in Columns)
        if (!oldWith.ContainsKey(column.DataPropertyName))
          oldWith.Add(column.DataPropertyName, column.Width);

      ColumnRemoved -= FilteredDataGridView_ColumnRemoved;
      try
      {
        // remove all columns
        Columns.Clear();

        // along with the entries in the context menu
        toolStripMenuItemColumnVisibility.CheckedListBoxControl.Items.Clear();

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
          DataGridViewColumn newColumn =
            col.DataType == typeof(bool)
              ? new DataGridViewCheckBoxColumn()
              : showAsButton.Contains(col)
                ? new DataGridViewButtonColumn()
                : new DataGridViewTextBoxColumn();


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
          Columns.Add(newColumn);
        }
      }
      finally
      {
        ColumnRemoved += FilteredDataGridView_ColumnRemoved;
      }
    }

    private ToolStripDataGridViewColumnFilter GetColumnFilter(int columnIndex) => m_Filter[columnIndex] ??=
      new ToolStripDataGridViewColumnFilter(Columns[columnIndex], ToolStripMenuItemApply_Click);

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
#pragma warning disable CA1416
    private void HighlightCellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
      try
      {
        if (e is { RowIndex: -1, ColumnIndex: >= 0 } && m_Filter[e.ColumnIndex] != null
                                                     && m_Filter[e.ColumnIndex]!.ColumnFilterLogic.Active)
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
        var val = e.FormattedValue.ToString();
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
                           && e.CellStyle.Alignment == DataGridViewContentAlignment.MiddleLeft)
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

            var before = val.Substring(0, nbspIndex);
            if (before.Length > 0)
              hlRect.X = (e.CellBounds.X
                          + TextRenderer.MeasureText(e.Graphics, before, e.CellStyle.Font, e.CellBounds.Size)
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
            && (e.CellStyle.Alignment == DataGridViewContentAlignment.MiddleLeft
                || e.CellStyle.Alignment == DataGridViewContentAlignment.MiddleRight) && highlightIndex >= 0)
        {
          using var hlBrush = new SolidBrush(Color.MediumSpringGreen);
          var hlRect = new Rectangle();
          while (highlightIndex >= 0 && (linefeedIndex == -1 || highlightIndex < linefeedIndex))
          {
            var highlight = TextRenderer.MeasureText(
              e.Graphics,
              val.Substring(highlightIndex, HighlightText.Length),
              e.CellStyle.Font,
              e.CellBounds.Size);

            hlRect.Y = e.CellBounds.Y + (e.CellBounds.Height - highlight.Height) / 2;
            hlRect.Height = highlight.Height + 1;
            hlRect.Width = highlight.Width - 6;

            if (e.CellStyle.Alignment == DataGridViewContentAlignment.MiddleLeft)
            {
              var before = val.Substring(0, highlightIndex);
              if (before.Length > 0)
                hlRect.X = (e.CellBounds.X + TextRenderer.MeasureText(
                  e.Graphics,
                  before,
                  e.CellStyle.Font,
                  e.CellBounds.Size).Width) - 4;
              else
                hlRect.X = e.CellBounds.X + 2;
            }
            else
            {
              var after = val.Substring(highlightIndex + HighlightText.Length);
              if (after.Length > 0)
                hlRect.X = ((e.CellBounds.X + e.CellBounds.Width)
                            - TextRenderer.MeasureText(e.Graphics, after, e.CellStyle.Font,
                              e.CellBounds.Size).Width
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
#pragma warning restore CA1416

    /// <summary>
    ///   Resets the data source.
    /// </summary>
    private void ResetDataSource()
    {
      try
      {
        CloseFilter();
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

        while (dataSource is not System.Data.DataView && maxIteration > 0)
        {
          if (dataSource is BindingSource bs)
          {
            m_BindingSource = ((BindingSource) DataSource!);
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

    private void TimerColumnsFilter_Tick(object? sender, EventArgs e)
    {
      try
      {
        timerColumsFilterChecked.Stop();

        var items = new Dictionary<string, bool>();
        for (var i = 0; i < toolStripMenuItemColumnVisibility.CheckedListBoxControl.Items.Count; i++)
        {
          var text = toolStripMenuItemColumnVisibility.CheckedListBoxControl.Items[i].ToString();
          if (!string.IsNullOrEmpty(text))
            items.Add(text, toolStripMenuItemColumnVisibility.CheckedListBoxControl.GetItemChecked(i));
        }

        this.SafeInvoke(() => SetColumnVisibility(items));
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "TimerColumnsFilter_Tick");
      }
    }

    /// <summary>
    ///   Handles the Click event of the toolStripMenuItemApply control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ToolStripMenuItemApply_Click(object? sender, EventArgs e)
    {
      try
      {
        if (m_Filter[m_MenuItemColumnIndex] != null)
          m_Filter[m_MenuItemColumnIndex]!.ColumnFilterLogic.Active = true;

        ApplyFilters();
        contextMenuStripCell.Close();
        contextMenuStripHeader.Close();
        contextMenuStripFilter.Close();
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "Apply Click");
      }
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
            false, false);

          if (form.ShowWithFont(this, true) == DialogResult.Cancel)
            return;

          m_FileSetting.ColumnCollection.Replace(form.UpdatedColumn);
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
        if (m_Filter[m_MenuItemColumnIndex] is null ||
            !m_Filter[m_MenuItemColumnIndex]!.ColumnFilterLogic.Active)
          return;

        m_Filter[m_MenuItemColumnIndex]!.ColumnFilterLogic.Active = false;
        ApplyFilters();
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "ToolStripMenuItemFilterRemoveOne_Click");
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
        ColumnVisibilityChanged();
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "ToolStripMenuItemHideThisColumn_Click");
      }
    }

    private void ToolStripMenuItemLoadCol_Click(object? sender, EventArgs e)
    {
      if (m_FileSetting == null)
        return;

      try
      {
        toolStripMenuItemLoadCol.Enabled = false;
        var fileName = WindowsAPICodePackWrapper.Open(
          m_FileSetting is IFileSettingPhysicalFile phy ? phy.FullPath.GetDirectoryName() : ".", "Load Column Setting",
          "Column Config|*.col;*.conf|All files|*.*", DefFileNameColSetting(m_FileSetting, ".col"));
        if (fileName != null)
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

    /// <summary>
    /// Get an Array of ColumnSetting serialized as Json Text
    /// </summary>
    public string GetViewStatus => ViewSetting.StoreViewSetting(Columns, m_Filter, SortedColumn, SortOrder);

    public void SetViewStatus(string newSetting)
    {
      try
      {
        SuspendLayout();
        if (ViewSetting.ReStoreViewSetting(newSetting, Columns, m_Filter, GetColumnFilter,
              Sort))
          ApplyFilters();
        ColumnVisibilityChanged();
        ResumeLayout(true);
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "SetViewStatus");
      }
    }

    private async void ToolStripMenuItemSaveCol_Click(object? sender, EventArgs e)
    {
      if (m_FileSetting is null)
        return;
      try
      {
        toolStripMenuItemSaveCol.Enabled = false;
        var text = GetViewStatus;
        if (!string.IsNullOrEmpty(text))
        {
          // Select Path
          var fileName = WindowsAPICodePackWrapper.Save(
            m_FileSetting is IFileSettingPhysicalFile phy ? phy.FullPath.GetDirectoryName() : ".", "Save Column Setting",
            "Column Config|*.col;*.conf|All files|*.*", ".col", false, DefFileNameColSetting(m_FileSetting, ".col"));

          if (fileName is null || fileName.Length == 0)
            return;

#if NET5_0_OR_GREATER
          await
#endif          
          using var stream = new ImprovedStream(new SourceAccess(fileName, false));

#if NET5_0_OR_GREATER
          await
#endif
          using var writer = new StreamWriter(stream, Encoding.UTF8, 1024);
          await writer.WriteAsync(GetViewStatus);
          await writer.FlushAsync();

          if (m_FileSetting is BaseSettingPhysicalFile basePhysical)
            basePhysical.ColumnFile = fileName;
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

    /// <summary>
    ///   Handles the Click event of the toolStripMenuItemAllCol control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ToolStripMenuItemShowAllColumns_Click(object? sender, EventArgs e) => ShowAllColumns();

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

    private void TimerColumnsFilterText_Tick(object? sender, EventArgs e)
    {
      try
      {
        timerColumsFilterText.Stop();
        if (toolStripTextBoxColFilter.Text.Length <= 1) return;
        toolStripTextBoxColFilter.RunWithHourglass(() =>
        {
          bool allVisible = true;
          foreach (DataGridViewColumn col in Columns)
          {
            if (!col.Visible)
            {
              allVisible = false;
              break;
            }
          }

          foreach (DataGridViewColumn col in Columns)
            if (col.DataPropertyName.IndexOf(toolStripTextBoxColFilter.Text, StringComparison.OrdinalIgnoreCase) != -1)
              col.Visible = true;
            else if (allVisible)
              col.Visible = false;

          if (!ColumnVisibilityChanged())
            return;

          SetRowHeight();
        }, null);
        toolStripTextBoxColFilter.Focus();
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "TimerColumnsFilterText_Tick");
      }
    }
  }
}