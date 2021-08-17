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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   Windows from to show detail information for a dataTable
  /// </summary>
  public class DetailControl : UserControl
  {
    private readonly List<DataGridViewCell> m_FoundCells = new List<DataGridViewCell>();

    private readonly List<KeyValuePair<string, DataGridViewCell>> m_SearchCells =
      new List<KeyValuePair<string, DataGridViewCell>>();

    private readonly ObservableCollection<ToolStripItem> m_ToolStripItems = new ObservableCollection<ToolStripItem>();
    private IContainer? components;
    public Func<bool>? EndOfFile;
    public EventHandler<IFileSettingPhysicalFile>? BeforeFileStored = null;
    public EventHandler<IFileSettingPhysicalFile>? FileStored = null;

    public Func<IProcessDisplay, Task>? LoadNextBatchAsync;

    // private EventHandler m_BatchSizeChangedEvent;
    private BindingNavigator m_BindingNavigator = new BindingNavigator();

    private BindingSource m_BindingSource = new BindingSource();

    private CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();

    private DataTable m_DataTable = new DataTable();
    private DataColumnCollection Columns => m_DataTable.Columns;

    private ProcessInformation? m_CurrentSearch;

    private bool m_DisposedValue; // To detect redundant calls

    private FillGuessSettings? m_FillGuessSettings;

    private FilterDataTable? m_FilterDataTable;

    private FormHierarchyDisplay? m_HierarchyDisplay;
    private FormShowMaxLength? m_FormShowMaxLength;
    private FormDuplicatesDisplay? m_FormDuplicatesDisplay;
    private FormUniqueDisplay? m_FormUniqueDisplay;

    private Form? m_ParentForm;

    private Search m_Search = new Search();

    private bool m_SearchCellsDirty = true;

    private bool m_ShowButtons = true;

    private bool m_ShowFilter = true;

    private ToolStripButton m_ToolStripButtonColumnLength = new ToolStripButton();

    private ToolStripButton m_ToolStripButtonDuplicates = new ToolStripButton();

    private ToolStripButton m_ToolStripButtonHierarchy = new ToolStripButton();

    private ToolStripButton m_ToolStripButtonMoveFirstItem = new ToolStripButton();

    private ToolStripButton m_ToolStripButtonMoveLastItem = new ToolStripButton();

    private ToolStripButton m_ToolStripButtonMoveNextItem = new ToolStripButton();

    private ToolStripButton m_ToolStripButtonMovePreviousItem = new ToolStripButton();

    private ToolStripButton m_ToolStripButtonStore = new ToolStripButton();

    private ToolStripButton m_ToolStripButtonUniqueValues = new ToolStripButton();

    private ToolStripComboBox m_ToolStripComboBoxFilterType = new ToolStripComboBox();

    private ToolStripContainer m_ToolStripContainer;

    private ToolStripLabel m_ToolStripLabelCount = new ToolStripLabel();

    private ToolStripTextBox m_ToolStripTextBox1 = new ToolStripTextBox();

    private ToolStrip m_ToolStripTop = new ToolStrip();

    public ToolStripButton ToolStripButtonNext = new ToolStripButton();

    /// <summary>
    ///   Initializes a new instance of the <see cref="DetailControl" /> class.
    /// </summary>
    public DetailControl()
    {
      InitializeComponent();

      // For some reason if its part of InitializeComponent the designer will not work
      FilteredDataGridView = new FilteredDataGridView
      {
        AllowUserToOrderColumns = true,
        ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
        Dock = DockStyle.Fill,
        Location = new Point(0, 0),
        Margin = new Padding(2),
        RowHeadersWidth = 51,
        RowTemplate = { Height = 33 },
        Size = new Size(996, 320),
        TabIndex = 2
      };

      FilteredDataGridView.DataViewChanged += DataViewChanged;
      FilteredDataGridView.CellFormatting += FilteredDataGridView_CellFormatting;
      FilteredDataGridView.KeyDown += DetailControl_KeyDown;
      m_ToolStripContainer!.ContentPanel.Controls.Add(FilteredDataGridView);

      m_ToolStripItems.Add(m_ToolStripComboBoxFilterType!);
      m_ToolStripItems.Add(m_ToolStripButtonUniqueValues!);
      m_ToolStripItems.Add(m_ToolStripButtonDuplicates!);
      m_ToolStripItems.Add(m_ToolStripButtonHierarchy!);
      m_ToolStripItems.Add(m_ToolStripButtonColumnLength!);
      m_ToolStripItems.Add(m_ToolStripButtonStore!);

      m_ToolStripItems.CollectionChanged += (sender, e) => MoveMenu();
      MoveMenu();
    }

    /// <summary>
    ///   Gets or sets the HTML style.
    /// </summary>
    /// <value>The HTML style.</value>
    public HTMLStyle HTMLStyle { get => FilteredDataGridView.HTMLStyle; set => FilteredDataGridView.HTMLStyle=value; }

    private bool m_MenuDown;

    /// <summary>
    ///   General Setting that determines if the menu is display in the bottom of a detail control
    /// </summary>
    public bool MenuDown
    {
      get => m_MenuDown;
      set
      {
        if (m_MenuDown==value) return;
        m_MenuDown=value;
        MoveMenu();
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
      get => FilteredDataGridView.AlternatingRowsDefaultCellStyle;
      set => FilteredDataGridView.AlternatingRowsDefaultCellStyle = value;
    }

    public CancellationToken CancellationToken
    {
      set => m_CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(value);
    }

    /// <summary>
    ///   Gets the data grid view.
    /// </summary>
    /// <value>The data grid view.</value>
    public FilteredDataGridView FilteredDataGridView { get; }

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
        m_DataTable = value ?? new DataTable();
        m_FilterDataTable?.Dispose();
        m_FilterDataTable = null;
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
        SetButtonVisibility();
      }
    }

    /// <summary>
    ///   A File Setting
    /// </summary>
    public FillGuessSettings? FillGuessSettings
    {
      set
      {
        m_FillGuessSettings = value;
        FilteredDataGridView.FillGuessSettings = m_FillGuessSettings;
      }
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
    public bool ReadOnly
    {
      get => FilteredDataGridView.ReadOnly;
      set
      {
        if (FilteredDataGridView.ReadOnly == value)
          return;
        FilteredDataGridView.ReadOnly = value;
        FilteredDataGridView.AllowUserToAddRows = !value;
        FilteredDataGridView.AllowUserToDeleteRows = !value;
        SetButtonVisibility();
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
      get => m_ShowFilter;
      set
      {
        if (m_ShowFilter == value)
          return;
        m_ShowFilter = value;
        SetButtonVisibility();
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
      get => m_ShowButtons;
      set
      {
        if (m_ShowButtons == value)
          return;
        m_ShowButtons = value;
        SetButtonVisibility();
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
        if ((value is null || !value.Any()))
          return;
        if (m_FilterDataTable != null)
          m_FilterDataTable.UniqueFieldName = value;
      }
    }

    /// <summary>
    ///   Called when [show errors].
    /// </summary>
    public bool OnlyShowErrors
    {
      set
      {
        try
        {
          m_ToolStripComboBoxFilterType!.SelectedIndex = value ? 1 : 0;
        }
        catch
        {
          // ignored
        }
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
      }
    }

    /// <summary>
    ///   Moves the menu in the lower or upper tool-bar
    /// </summary>
    public void MoveMenu()
    {
      var source = (m_MenuDown) ? m_ToolStripTop : m_BindingNavigator;
      var target = (m_MenuDown) ? m_BindingNavigator : m_ToolStripTop;
      target!.SuspendLayout();
      source!.SuspendLayout();
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

      foreach (var item in m_ToolStripItems)
      {
        item.DisplayStyle = (m_MenuDown)
                              ? ToolStripItemDisplayStyle.Image
                              : ToolStripItemDisplayStyle.ImageAndText;
        target.Items.Add(item);
      }

      source.ResumeLayout(true);
      target.ResumeLayout(true);
      SetButtonVisibility();
    }

    /// <summary>
    ///   Sorts the data grid view on a given column
    /// </summary>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="direction">The direction.</param>
    public void Sort(string columnName, ListSortDirection direction)
    {
      foreach (DataGridViewColumn col in FilteredDataGridView.Columns)
        if (col.DataPropertyName.Equals(columnName, StringComparison.OrdinalIgnoreCase) && col.Visible)
        {
          FilteredDataGridView.Sort(col, direction);
          break;

          // col.HeaderCell.SortGlyphDirection = direction == ListSortDirection.Ascending ?
          // SortOrder.Ascending : SortOrder.Descending;
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
        m_FormShowMaxLength?.Dispose();
        m_FormDuplicatesDisplay?.Dispose();
        m_FormUniqueDisplay?.Dispose();
        m_CurrentSearch?.Dispose();
        m_DataTable.Dispose();
        m_FilterDataTable?.Dispose();
        m_HierarchyDisplay?.Dispose();
        m_CancellationTokenSource.Dispose();
      }

      base.Dispose(disposing);
    }

    /// <summary>
    /// </summary>
    /// <param name="e">An <see cref="EventArgs" /> that contains the event data.</param>
    protected override void OnParentChanged(EventArgs e)
    {
      base.OnParentChanged(e);

      if (m_ParentForm != null)
        m_ParentForm.Closing -= ParentForm_Closing;
      m_ParentForm = FindForm();

      if (m_ParentForm != null)
        m_ParentForm.Closing += ParentForm_Closing;
    }

    private void AutoResizeColumns(DataTable source)
    {
      if (source.Rows.Count < 10000 && source.Columns.Count < 50)
        FilteredDataGridView.AutoResizeColumns(
          source.Rows.Count < 1000 && source.Columns.Count < 20
            ? DataGridViewAutoSizeColumnsMode.AllCells
            : DataGridViewAutoSizeColumnsMode.DisplayedCells);
    }

    private void BackgroundSearchThread(object? obj)
    {
      var processInformation = obj as ProcessInformation;
      if (processInformation == null)
        return;
      processInformation.IsRunning = true;
      var oldCursor = Cursors.WaitCursor.Equals(Cursor.Current) ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        if (string.IsNullOrEmpty(processInformation.SearchText))
          return;

        // Do not search for an text shorter than 2 if we have a lot of data
        if (processInformation.SearchText.Length < 2 && m_SearchCells.Count() > 10000)
          return;

        foreach (var cell in m_SearchCells)
        {
          if (processInformation.CancellationTokenSource?.IsCancellationRequested ?? false)
            return;

          if (cell.Key.IndexOf(processInformation.SearchText, StringComparison.OrdinalIgnoreCase) <= -1)
            continue;
          processInformation.FoundResultEvent?.Invoke(this, new FoundEventArgs(processInformation.Found, cell.Value));
          processInformation.Found++;
        }
      }
      finally
      {
        Cursor.Current = oldCursor;
        processInformation.IsRunning = false;
        processInformation.SearchCompleteEvent?.Invoke(this, processInformation.SearchEventArgs);
      }
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
      m_ToolStripButtonColumnLength!.RunWithHourglass(() =>
      {
        var visible = FilteredDataGridView.Columns.Cast<DataGridViewColumn>()
                                          .Where(col => col.Visible && !string.IsNullOrEmpty(col.DataPropertyName)).OrderBy(col => col.DisplayIndex)
                                          .Select(col => col.DataPropertyName).ToList();
        m_FormShowMaxLength?.Close();
        m_FormShowMaxLength =new FormShowMaxLength(m_DataTable, m_DataTable!.Select(FilteredDataGridView.CurrentFilter), visible, HTMLStyle)
        { Icon = ParentForm?.Icon };
        m_FormShowMaxLength.Show(ParentForm);

        m_FormShowMaxLength.FormClosed += (ob, ar) => this.SafeInvoke(() => m_ToolStripButtonColumnLength!.Enabled = true);
      });
      m_ToolStripButtonColumnLength!.Enabled = false;
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
      m_ToolStripButtonDuplicates!.RunWithHourglass(() =>
      {
        var columnName = FilteredDataGridView.CurrentCell != null
                           ? FilteredDataGridView.Columns[FilteredDataGridView.CurrentCell.ColumnIndex].Name
                           : FilteredDataGridView.Columns[0].Name;
        try
        {
          m_FormDuplicatesDisplay?.Close();
          m_FormDuplicatesDisplay = new FormDuplicatesDisplay(m_DataTable!.Clone(), m_DataTable.Select(FilteredDataGridView.CurrentFilter), columnName, HTMLStyle)
          { Icon = ParentForm?.Icon };
          m_FormDuplicatesDisplay.Show(ParentForm);
          m_FormDuplicatesDisplay.FormClosed += (ob, ar) => this.SafeInvoke(() => m_ToolStripButtonDuplicates!.Enabled = true);
        }
        catch (Exception ex)
        {
          ParentForm.ShowError(ex);
        }
      });
      m_ToolStripButtonDuplicates!.Enabled = false;
    }

    /// <summary>
    ///   Handles the Click event of the buttonHierarchy control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ButtonHierarchy_Click(object? sender, EventArgs e)
    {
      m_ToolStripButtonHierarchy!.RunWithHourglass(() =>
      {
        try
        {
          m_HierarchyDisplay?.Close();
          m_HierarchyDisplay =
            new FormHierarchyDisplay(m_DataTable!.Clone(), m_DataTable.Select(FilteredDataGridView.CurrentFilter), HTMLStyle) { Icon = ParentForm?.Icon };
          m_HierarchyDisplay.Show(ParentForm);
          m_HierarchyDisplay.FormClosed += (ob, ar) => this.SafeInvoke(() => m_ToolStripButtonHierarchy!.Enabled = true);
        }
        catch (Exception ex)
        {
          ParentForm.ShowError(ex);
        }
      });
      m_ToolStripButtonHierarchy!.Enabled = false;
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
      m_ToolStripButtonUniqueValues!.RunWithHourglass(() =>
      {
        try
        {
          var columnName = FilteredDataGridView.CurrentCell != null
                             ? FilteredDataGridView.Columns[FilteredDataGridView.CurrentCell.ColumnIndex].Name
                             : FilteredDataGridView.Columns[0].Name;
          m_FormUniqueDisplay?.Close();
          m_FormUniqueDisplay = new FormUniqueDisplay(
           m_DataTable!.Clone(),
           m_DataTable.Select(FilteredDataGridView.CurrentFilter),
           columnName, HTMLStyle)
          { Icon = ParentForm?.Icon };
          m_FormUniqueDisplay.ShowDialog(ParentForm);
        }
        catch (Exception ex)
        {
          ParentForm.ShowError(ex);
        }
      });
      m_ToolStripButtonUniqueValues.Enabled = false;
    }

    private void ClearSearch(object? sender, EventArgs e)
    {
      (this).SafeInvoke(
        () =>
        {
          FilteredDataGridView.HighlightText = string.Empty;
          m_FoundCells.Clear();
          FilteredDataGridView.Refresh();
          m_Search.Results = 0;
        });
      m_CurrentSearch?.Dispose();
    }

    private void DataViewChanged(object? sender, EventArgs args)
    {
      m_SearchCellsDirty = true;
      if (!m_Search.Visible)
        return;
      if (m_CurrentSearch != null && m_CurrentSearch.IsRunning)
        m_CurrentSearch.Cancel();
      m_Search.Results = 0;
      m_Search.Hide();
      ClearSearch(sender, args);
    }

    private void DetailControl_KeyDown(object? sender, KeyEventArgs e)
    {
      if (!e.Control || e.KeyCode != Keys.F)
        return;
      m_Search.Visible = true;
      PopulateSearchCellList();
      m_Search.Focus();
      e.Handled = true;
    }

    /// <summary>
    ///   Filters the columns.
    /// </summary>
    private void FilterColumns(bool onlyErrors)
    {
      if (!onlyErrors)
      {
        foreach (DataGridViewColumn col in FilteredDataGridView.Columns)
        {
          if (!col.Visible)
          {
            col.Visible = true;
            m_SearchCellsDirty = true;
          }

          col.MinimumWidth = 64;
        }

        return;
      }

      if (m_FilterDataTable?.FilterTable != null && (m_FilterDataTable is null || m_FilterDataTable.FilterTable.Rows.Count <= 0))
        return;
      if (m_FilterDataTable != null && m_FilterDataTable.ColumnsWithoutErrors.Count == Columns!.Count)
        return;
      foreach (DataGridViewColumn dgCol in FilteredDataGridView.Columns)
      {
        if (m_FilterDataTable != null && (!dgCol.Visible || !m_FilterDataTable.ColumnsWithoutErrors.Contains(dgCol.DataPropertyName))) continue;
        dgCol.Visible = false;
        m_SearchCellsDirty = true;
      }
    }

    private void FilteredDataGridView_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
      if (!(e.Value is DateTime cellValue))
        return;

      e.Value = StringConversion.DisplayDateTime(cellValue, CultureInfo.CurrentCulture);
    }

    /// <summary>
    ///   Required method for Designer support - do not modify the contents of this method with the
    ///   code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DetailControl));
      this.m_ToolStripTop = new System.Windows.Forms.ToolStrip();
      this.m_ToolStripComboBoxFilterType = new System.Windows.Forms.ToolStripComboBox();
      this.m_ToolStripButtonUniqueValues = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripButtonColumnLength = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripButtonDuplicates = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripButtonHierarchy = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripButtonStore = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripContainer = new System.Windows.Forms.ToolStripContainer();
      this.m_BindingNavigator = new System.Windows.Forms.BindingNavigator(this.components);
      this.m_BindingSource = new System.Windows.Forms.BindingSource(this.components);
      this.m_ToolStripLabelCount = new System.Windows.Forms.ToolStripLabel();
      this.m_ToolStripButtonMoveFirstItem = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripButtonMovePreviousItem = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripTextBox1 = new System.Windows.Forms.ToolStripTextBox();
      this.m_ToolStripButtonMoveNextItem = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripButtonMoveLastItem = new System.Windows.Forms.ToolStripButton();
      this.ToolStripButtonNext = new System.Windows.Forms.ToolStripButton();
      this.m_Search = new CsvTools.Search();
      this.m_ToolStripTop.SuspendLayout();
      this.m_ToolStripContainer.BottomToolStripPanel.SuspendLayout();
      this.m_ToolStripContainer.ContentPanel.SuspendLayout();
      this.m_ToolStripContainer.TopToolStripPanel.SuspendLayout();
      this.m_ToolStripContainer.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize) (this.m_BindingNavigator)).BeginInit();
      this.m_BindingNavigator.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize) (this.m_BindingSource)).BeginInit();
      this.SuspendLayout();
      // m_ToolStripTop
      this.m_ToolStripTop.Dock = System.Windows.Forms.DockStyle.None;
      this.m_ToolStripTop.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
      this.m_ToolStripTop.ImageScalingSize = new System.Drawing.Size(20, 20);
      this.m_ToolStripTop.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
      {
        this.m_ToolStripComboBoxFilterType, this.m_ToolStripButtonUniqueValues, this.m_ToolStripButtonColumnLength, this.m_ToolStripButtonDuplicates,
        this.m_ToolStripButtonHierarchy, this.m_ToolStripButtonStore
      });
      this.m_ToolStripTop.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
      this.m_ToolStripTop.Location = new System.Drawing.Point(4, 0);
      this.m_ToolStripTop.Name = "m_ToolStripTop";
      this.m_ToolStripTop.Size = new System.Drawing.Size(709, 28);
      this.m_ToolStripTop.TabIndex = 1;
      this.m_ToolStripTop.Text = "toolStripTop";
      // m_ToolStripComboBoxFilterType
      this.m_ToolStripComboBoxFilterType.DropDownHeight = 90;
      this.m_ToolStripComboBoxFilterType.DropDownWidth = 130;
      this.m_ToolStripComboBoxFilterType.IntegralHeight = false;
      this.m_ToolStripComboBoxFilterType.Items.AddRange(
        new object[] { "All Records", "Error or Warning", "Only Errors", "Only Warning", "No Error or Warning" });
      this.m_ToolStripComboBoxFilterType.Name = "m_ToolStripComboBoxFilterType";
      this.m_ToolStripComboBoxFilterType.Size = new System.Drawing.Size(150, 28);
      // m_ToolStripButtonUniqueValues
      this.m_ToolStripButtonUniqueValues.Image = ((System.Drawing.Image) (resources.GetObject("m_ToolStripButtonUniqueValues.Image")));
      this.m_ToolStripButtonUniqueValues.Name = "m_ToolStripButtonUniqueValues";
      this.m_ToolStripButtonUniqueValues.Size = new System.Drawing.Size(126, 25);
      this.m_ToolStripButtonUniqueValues.Text = "Unique Values";
      this.m_ToolStripButtonUniqueValues.ToolTipText = "Display Unique Values";
      this.m_ToolStripButtonUniqueValues.Click += new System.EventHandler(this.ButtonUniqueValues_Click);
      // m_ToolStripButtonColumnLength
      this.m_ToolStripButtonColumnLength.Image = ((System.Drawing.Image) (resources.GetObject("m_ToolStripButtonColumnLength.Image")));
      this.m_ToolStripButtonColumnLength.Name = "m_ToolStripButtonColumnLength";
      this.m_ToolStripButtonColumnLength.Size = new System.Drawing.Size(133, 25);
      this.m_ToolStripButtonColumnLength.Text = "Column Length";
      this.m_ToolStripButtonColumnLength.ToolTipText = "Display Schema information including Length";
      this.m_ToolStripButtonColumnLength.Click += new System.EventHandler(this.ButtonColumnLength_Click);
      // m_ToolStripButtonDuplicates
      this.m_ToolStripButtonDuplicates.Image = ((System.Drawing.Image) (resources.GetObject("m_ToolStripButtonDuplicates.Image")));
      this.m_ToolStripButtonDuplicates.Name = "m_ToolStripButtonDuplicates";
      this.m_ToolStripButtonDuplicates.Size = new System.Drawing.Size(103, 25);
      this.m_ToolStripButtonDuplicates.Text = "Duplicates";
      this.m_ToolStripButtonDuplicates.ToolTipText = "Display Duplicate Values";
      this.m_ToolStripButtonDuplicates.Click += new System.EventHandler(this.ButtonDuplicates_Click);
      // m_ToolStripButtonHierarchy
      this.m_ToolStripButtonHierarchy.Image = ((System.Drawing.Image) (resources.GetObject("m_ToolStripButtonHierarchy.Image")));
      this.m_ToolStripButtonHierarchy.Name = "m_ToolStripButtonHierarchy";
      this.m_ToolStripButtonHierarchy.Size = new System.Drawing.Size(96, 25);
      this.m_ToolStripButtonHierarchy.Text = "Hierarchy";
      this.m_ToolStripButtonHierarchy.ToolTipText = "Display a Hierarchy Structure";
      this.m_ToolStripButtonHierarchy.Click += new System.EventHandler(this.ButtonHierarchy_Click);
      // m_ToolStripButtonStore
      this.m_ToolStripButtonStore.Image = ((System.Drawing.Image) (resources.GetObject("m_ToolStripButtonStore.Image")));
      this.m_ToolStripButtonStore.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.m_ToolStripButtonStore.Name = "m_ToolStripButtonStore";
      this.m_ToolStripButtonStore.Size = new System.Drawing.Size(96, 25);
      this.m_ToolStripButtonStore.Text = "Write File";
      this.m_ToolStripButtonStore.ToolTipText = "Store the currently displayed data as delimited text file";
      this.m_ToolStripButtonStore.Click += new System.EventHandler(this.ToolStripButtonStoreAsCsvAsync);
      // m_ToolStripContainer
      //
      //
      // m_ToolStripContainer.BottomToolStripPanel
      this.m_ToolStripContainer.BottomToolStripPanel.Controls.Add(this.m_BindingNavigator);
      // m_ToolStripContainer.ContentPanel
      this.m_ToolStripContainer.ContentPanel.Controls.Add(this.m_Search);
      this.m_ToolStripContainer.ContentPanel.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
      this.m_ToolStripContainer.ContentPanel.Size = new System.Drawing.Size(1328, 407);
      this.m_ToolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_ToolStripContainer.LeftToolStripPanelVisible = false;
      this.m_ToolStripContainer.Location = new System.Drawing.Point(0, 0);
      this.m_ToolStripContainer.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
      this.m_ToolStripContainer.Name = "m_ToolStripContainer";
      this.m_ToolStripContainer.RightToolStripPanelVisible = false;
      this.m_ToolStripContainer.Size = new System.Drawing.Size(1328, 462);
      this.m_ToolStripContainer.TabIndex = 13;
      this.m_ToolStripContainer.Text = "toolStripContainer";
      // m_ToolStripContainer.TopToolStripPanel
      this.m_ToolStripContainer.TopToolStripPanel.Controls.Add(this.m_ToolStripTop);
      // m_BindingNavigator
      this.m_BindingNavigator.AddNewItem = null;
      this.m_BindingNavigator.BindingSource = this.m_BindingSource;
      this.m_BindingNavigator.CountItem = this.m_ToolStripLabelCount;
      this.m_BindingNavigator.DeleteItem = null;
      this.m_BindingNavigator.Dock = System.Windows.Forms.DockStyle.None;
      this.m_BindingNavigator.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
      this.m_BindingNavigator.ImageScalingSize = new System.Drawing.Size(20, 20);
      this.m_BindingNavigator.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
      {
        this.m_ToolStripButtonMoveFirstItem, this.m_ToolStripButtonMovePreviousItem, this.m_ToolStripTextBox1, this.m_ToolStripLabelCount,
        this.m_ToolStripButtonMoveNextItem, this.m_ToolStripButtonMoveLastItem, this.ToolStripButtonNext
      });
      this.m_BindingNavigator.Location = new System.Drawing.Point(4, 0);
      this.m_BindingNavigator.MoveFirstItem = this.m_ToolStripButtonMoveFirstItem;
      this.m_BindingNavigator.MoveLastItem = this.m_ToolStripButtonMoveLastItem;
      this.m_BindingNavigator.MoveNextItem = this.m_ToolStripButtonMoveNextItem;
      this.m_BindingNavigator.MovePreviousItem = this.m_ToolStripButtonMovePreviousItem;
      this.m_BindingNavigator.Name = "m_BindingNavigator";
      this.m_BindingNavigator.PositionItem = this.m_ToolStripTextBox1;
      this.m_BindingNavigator.Size = new System.Drawing.Size(284, 27);
      this.m_BindingNavigator.TabIndex = 0;
      // m_ToolStripLabelCount
      this.m_ToolStripLabelCount.Name = "m_ToolStripLabelCount";
      this.m_ToolStripLabelCount.Size = new System.Drawing.Size(45, 24);
      this.m_ToolStripLabelCount.Text = "of {0}";
      this.m_ToolStripLabelCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.m_ToolStripLabelCount.ToolTipText = "Total number of items";
      // m_ToolStripButtonMoveFirstItem
      this.m_ToolStripButtonMoveFirstItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.m_ToolStripButtonMoveFirstItem.Image = ((System.Drawing.Image) (resources.GetObject("m_ToolStripButtonMoveFirstItem.Image")));
      this.m_ToolStripButtonMoveFirstItem.Name = "m_ToolStripButtonMoveFirstItem";
      this.m_ToolStripButtonMoveFirstItem.RightToLeftAutoMirrorImage = true;
      this.m_ToolStripButtonMoveFirstItem.Size = new System.Drawing.Size(29, 24);
      this.m_ToolStripButtonMoveFirstItem.Text = "Move first";
      // m_ToolStripButtonMovePreviousItem
      this.m_ToolStripButtonMovePreviousItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.m_ToolStripButtonMovePreviousItem.Image = ((System.Drawing.Image) (resources.GetObject("m_ToolStripButtonMovePreviousItem.Image")));
      this.m_ToolStripButtonMovePreviousItem.Name = "m_ToolStripButtonMovePreviousItem";
      this.m_ToolStripButtonMovePreviousItem.RightToLeftAutoMirrorImage = true;
      this.m_ToolStripButtonMovePreviousItem.Size = new System.Drawing.Size(29, 24);
      this.m_ToolStripButtonMovePreviousItem.Text = "Move previous";
      // m_ToolStripTextBox1
      this.m_ToolStripTextBox1.AccessibleName = "Position";
      this.m_ToolStripTextBox1.Font = new System.Drawing.Font("Segoe UI", 9F);
      this.m_ToolStripTextBox1.Name = "m_ToolStripTextBox1";
      this.m_ToolStripTextBox1.Size = new System.Drawing.Size(50, 27);
      this.m_ToolStripTextBox1.Text = "0";
      this.m_ToolStripTextBox1.ToolTipText = "Current position";
      // m_ToolStripButtonMoveNextItem
      this.m_ToolStripButtonMoveNextItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.m_ToolStripButtonMoveNextItem.Image = ((System.Drawing.Image) (resources.GetObject("m_ToolStripButtonMoveNextItem.Image")));
      this.m_ToolStripButtonMoveNextItem.Name = "m_ToolStripButtonMoveNextItem";
      this.m_ToolStripButtonMoveNextItem.RightToLeftAutoMirrorImage = true;
      this.m_ToolStripButtonMoveNextItem.Size = new System.Drawing.Size(29, 24);
      this.m_ToolStripButtonMoveNextItem.Text = "Move next";
      // m_ToolStripButtonMoveLastItem
      this.m_ToolStripButtonMoveLastItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.m_ToolStripButtonMoveLastItem.Image = ((System.Drawing.Image) (resources.GetObject("m_ToolStripButtonMoveLastItem.Image")));
      this.m_ToolStripButtonMoveLastItem.Name = "m_ToolStripButtonMoveLastItem";
      this.m_ToolStripButtonMoveLastItem.RightToLeftAutoMirrorImage = true;
      this.m_ToolStripButtonMoveLastItem.Size = new System.Drawing.Size(29, 24);
      this.m_ToolStripButtonMoveLastItem.Text = "Move last";
      // ToolStripButtonNext
      this.ToolStripButtonNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.ToolStripButtonNext.Enabled = false;
      this.ToolStripButtonNext.Image = ((System.Drawing.Image) (resources.GetObject("ToolStripButtonNext.Image")));
      this.ToolStripButtonNext.Name = "ToolStripButtonNext";
      this.ToolStripButtonNext.Size = new System.Drawing.Size(29, 24);
      this.ToolStripButtonNext.Text = "Load More...";
      this.ToolStripButtonNext.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
      this.ToolStripButtonNext.Click += new System.EventHandler(this.ToolStripButtonNext_Click);
      // m_Search
      this.m_Search.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.m_Search.AutoSize = true;
      this.m_Search.BackColor = System.Drawing.SystemColors.Info;
      this.m_Search.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.m_Search.Location = new System.Drawing.Point(825, 4);
      this.m_Search.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
      this.m_Search.Name = "m_Search";
      this.m_Search.Results = 0;
      this.m_Search.Size = new System.Drawing.Size(503, 44);
      this.m_Search.TabIndex = 1;
      this.m_Search.Visible = false;
      this.m_Search.OnResultChanged += new System.EventHandler<CsvTools.SearchEventArgs>(this.OnSearchResultChanged);
      this.m_Search.OnSearchChanged += new System.EventHandler<CsvTools.SearchEventArgs>(this.OnSearchChanged);
      this.m_Search.OnSearchClear += new System.EventHandler(this.ClearSearch);
      // DetailControl
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.m_ToolStripContainer);
      this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
      this.Name = "DetailControl";
      this.Size = new System.Drawing.Size(1328, 462);
      this.m_ToolStripTop.ResumeLayout(false);
      this.m_ToolStripTop.PerformLayout();
      this.m_ToolStripContainer.BottomToolStripPanel.ResumeLayout(false);
      this.m_ToolStripContainer.BottomToolStripPanel.PerformLayout();
      this.m_ToolStripContainer.ContentPanel.ResumeLayout(false);
      this.m_ToolStripContainer.ContentPanel.PerformLayout();
      this.m_ToolStripContainer.TopToolStripPanel.ResumeLayout(false);
      this.m_ToolStripContainer.TopToolStripPanel.PerformLayout();
      this.m_ToolStripContainer.ResumeLayout(false);
      this.m_ToolStripContainer.PerformLayout();
      ((System.ComponentModel.ISupportInitialize) (this.m_BindingNavigator)).EndInit();
      this.m_BindingNavigator.ResumeLayout(false);
      this.m_BindingNavigator.PerformLayout();
      ((System.ComponentModel.ISupportInitialize) (this.m_BindingSource)).EndInit();
      this.ResumeLayout(false);
    }

    /// <summary>
    ///   Called when search changes.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="SearchEventArgs" /> instance containing the event data.</param>
    private void OnSearchChanged(object? sender, SearchEventArgs e)
    {
      // Stop any current searches
      if (m_CurrentSearch is { IsRunning: true })
      {
        m_CurrentSearch.SearchEventArgs = e;

        // Tell the current search to carry on with a new search after its done / canceled
        m_CurrentSearch.SearchCompleteEvent += StartSearch;

        // Cancel the current search
        m_CurrentSearch.Cancel();
      }
      else
      {
        StartSearch(this, e);
      }
    }

    /// <summary>
    ///   Called when search result number changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="SearchEventArgs" /> instance containing the event data.</param>
    private void OnSearchResultChanged(object? sender, SearchEventArgs e)
    {
      if (e.Result <= 0 || e.Result >= m_FoundCells.Count)
        return;
      FilteredDataGridView.SafeInvoke(
        () =>
        {
          try
          {
            FilteredDataGridView.CurrentCell = m_FoundCells[e.Result - 1];
          }
          catch (Exception ex)
          {
            Debug.WriteLine(ex.InnerExceptionMessages());
          }
        });
      Extensions.ProcessUIElements();
    }

    /// <summary>
    ///   Handles the Closing event of the parentForm control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///   The <see cref="System.ComponentModel.CancelEventArgs" /> instance containing the event data.
    /// </param>
    private void ParentForm_Closing(object? sender, CancelEventArgs e)
    {
      if (m_ParentForm != null)
      {
        m_ParentForm.Closing -= ParentForm_Closing;
        m_ParentForm = null;
      }
      if (m_CurrentSearch?.IsRunning ?? false)
        m_CurrentSearch.Cancel();
      if (m_FilterDataTable?.Filtering ?? false)
        m_FilterDataTable.Cancel();
    }

    private void PopulateSearchCellList()
    {
      if (!m_SearchCellsDirty)
        return;
      var oldCursor = Cursors.WaitCursor.Equals(Cursor.Current) ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        // m_SearchCells = from r in filteredDataGridView.Rows.Cast<DataGridViewRow>() from c in
        // r.Cells.Cast<DataGridViewCell>() where c.Visible &&
        // !string.IsNullOrEmpty(c.FormattedValue.ToString()) select new KeyValuePair<string,
        // DataGridViewCell>(c.FormattedValue.ToString(), c);
        m_SearchCells.Clear();
        var visible = FilteredDataGridView.Columns.Cast<DataGridViewColumn>()
                                          .Where(col => col.Visible && !string.IsNullOrEmpty(col.DataPropertyName)).ToList();

        foreach (DataGridViewRow row in FilteredDataGridView.Rows)
        {
          if (!row.Visible)
            continue;
          foreach (var cell in visible.Select(col => row.Cells[col.Index])
                                      .Where(cell => !string.IsNullOrEmpty(cell.FormattedValue?.ToString())))
            if (cell.FormattedValue != null)
            {
              var formatted = cell.FormattedValue.ToString();
              if (!string.IsNullOrEmpty(formatted))
                m_SearchCells.Add(new KeyValuePair<string, DataGridViewCell>(formatted, cell));
            }
        }

        m_SearchCellsDirty = false;
      }
      finally
      {
        Cursor.Current = oldCursor;
      }
    }

    private void ResultFound(object? sender, FoundEventArgs args)
    {
      m_FoundCells.Add(args.Cell);
      (this).SafeBeginInvoke(() =>
      {
        m_Search.Results = args.Index;
        FilteredDataGridView.InvalidateCell(args.Cell);
      });
    }

    private void SearchComplete(object? sender, SearchEventArgs e) =>
      this.SafeBeginInvoke(() => { m_Search.Results = m_CurrentSearch?.Found ?? 0; });

    private void SetButtonVisibility() =>
      this.SafeBeginInvoke(() =>
      {
        m_ToolStripContainer.TopToolStripPanelVisible = !m_MenuDown;

        // Need to set the control containing the buttons to visible Regular
        m_ToolStripButtonColumnLength.Visible = m_ShowButtons;
        m_ToolStripButtonDuplicates.Visible = m_ShowButtons;
        m_ToolStripButtonUniqueValues.Visible = m_ShowButtons;

        // Extended
        m_ToolStripButtonHierarchy.Visible = m_ShowButtons;

        if (EndOfFile?.Invoke() ?? true)
        {
          m_ToolStripLabelCount.ForeColor = SystemColors.ControlText;
          m_ToolStripLabelCount.ToolTipText = "Total number of items";
          ToolStripButtonNext.Visible = false;
        }
        else
        {
          m_ToolStripLabelCount.ForeColor = SystemColors.MenuHighlight;
          m_ToolStripLabelCount.ToolTipText = "Total number of items (loaded so far)";
          ToolStripButtonNext.Visible = m_ShowButtons;
        }

        m_ToolStripButtonStore.Visible = m_ShowButtons && (FileSetting != null);
        try
        {
          m_ToolStripTop.Visible = m_ShowButtons;

          // Filter
          m_ToolStripComboBoxFilterType.Visible = m_ShowButtons && m_ShowFilter;
        }
        catch (InvalidOperationException)
        {
          // ignore error in regards to cross thread issues, SafeBeginInvoke should have handled
          // this though
        }
      });

    /// <summary>
    ///   Sets the data source.
    /// </summary>
    public async Task RefreshDisplayAsync(FilterType type, CancellationToken cancellationToken)
    {
      m_BindingNavigator.SafeInvoke(
        () =>
        {
          m_ToolStripComboBoxFilterType.SelectedIndexChanged -= ToolStripComboBoxFilterType_SelectedIndexChanged;

          // update the drop down
          if (type == FilterType.All)
            m_ToolStripComboBoxFilterType.SelectedIndex = 0;
          else if (type == FilterType.ErrorsAndWarning)
            m_ToolStripComboBoxFilterType.SelectedIndex = 1;
          else if (type == FilterType.ShowErrors)
            m_ToolStripComboBoxFilterType.SelectedIndex = 2;
          else if (type == FilterType.ShowWarning)
            m_ToolStripComboBoxFilterType.SelectedIndex = 3;
          else if (type == FilterType.ShowIssueFree)
            m_ToolStripComboBoxFilterType.SelectedIndex = 4;

          m_ToolStripComboBoxFilterType.SelectedIndexChanged += ToolStripComboBoxFilterType_SelectedIndexChanged;
        });

      var oldSortedColumn = FilteredDataGridView.SortedColumn?.DataPropertyName;
      var oldOrder = FilteredDataGridView.SortOrder;

      // Cancel the current search
      if (m_CurrentSearch is { IsRunning: true })
        m_CurrentSearch.Cancel();

      // Hide any showing search
      m_Search.Visible = false;

      var newDt = m_DataTable;
      if (m_FilterDataTable is null)
        m_FilterDataTable = new FilterDataTable(m_DataTable);
      if (m_FilterDataTable != null && type != FilterType.All)
      {
        if (type != m_FilterDataTable.FilterType)
          await m_FilterDataTable.FilterAsync(int.MaxValue, type, cancellationToken);
        newDt = m_FilterDataTable.FilterTable;
      }

      if (ReferenceEquals(m_BindingSource.DataSource, newDt))
        return;

      this.SafeInvokeNoHandleNeeded(() =>
      {
        FilteredDataGridView.DataSource = null;
        m_BindingSource.DataSource = newDt;
        try
        {
          FilteredDataGridView.DataSource = m_BindingSource;
          FilterColumns(!type.HasFlag(FilterType.ShowIssueFree));
          AutoResizeColumns(newDt);
        }
        catch (Exception ex)
        {
          ParentForm?.ShowError(ex, "Error setting the DataSource of the grid");
        }

        FilteredDataGridView.ColumnVisibilityChanged();
        FilteredDataGridView.SetRowHeight();

        if (oldOrder != SortOrder.None && !string.IsNullOrEmpty(oldSortedColumn))
          Sort(oldSortedColumn!,
            oldOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
      });
      ShowFilter = m_FilterDataTable?.FilterTable != null && m_FilterDataTable.FilterTable.Rows.Count > 0;
    }

    private void StartSearch(object? sender, SearchEventArgs e)
    {
      ClearSearch(this, EventArgs.Empty);
      FilteredDataGridView.HighlightText = e.SearchText;

      var processInformation = new ProcessInformation
      {
        SearchText = e.SearchText,
        CancellationTokenSource =
          CancellationTokenSource.CreateLinkedTokenSource(m_CancellationTokenSource.Token)
      };

      processInformation.FoundResultEvent += ResultFound;
      processInformation.SearchCompleteEvent += SearchComplete;
      processInformation.SearchEventArgs = e;
      m_CurrentSearch = processInformation;
      ThreadPool.QueueUserWorkItem(BackgroundSearchThread, processInformation);
    }

    public async Task SafeCurrentFile(string fileName, bool adjustDelimiter)
    {
      // This will always write a delimited text file
      ICsvFile writeFile = new CsvFile();
      writeFile.CopyTo(writeFile);

      // in case the extension is changed change the delimiter accordingly
      if (adjustDelimiter)
      {
        if (fileName.EndsWith("tab", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith("tsv", StringComparison.OrdinalIgnoreCase))
          writeFile.FileFormat.FieldDelimiter = "\t";

        if (fileName.EndsWith("csv", StringComparison.OrdinalIgnoreCase))
          writeFile.FileFormat.FieldDelimiter = ",";
      }

      writeFile.FileName = fileName;
      writeFile.ID = string.Empty;
      // in case we skipped lines read them as Header so we do not loose them
      if (writeFile is ICsvFile src && src.SkipRows > 0 && string.IsNullOrEmpty(writeFile.Header))
      {
        using var iStream = FunctionalDI.OpenStream(new SourceAccess(src));
        using var sr = new ImprovedTextReader(iStream, src.CodePageId);
        sr.ToBeginning();
        for (var i = 0; i < src.SkipRows; i++)
          writeFile.Header += sr.ReadLine() + '\n';
      }

      using var processDisplay = new FormProcessDisplay(writeFile.ToString(), true, m_CancellationTokenSource.Token);
      try
      {
        processDisplay.Show(ParentForm);

        BeforeFileStored?.Invoke(this, writeFile);
        var writer = new CsvFileWriter(writeFile.ID, writeFile.FullPath, writeFile.HasFieldHeader, writeFile.FileFormat.ValueFormatMutable, writeFile.FileFormat, writeFile.CodePageId,
        writeFile.ByteOrderMark, writeFile.ColumnCollection, writeFile.Recipient, writeFile.KeepUnencrypted, writeFile.IdentifierInContainer,
        writeFile.Header, writeFile.Footer);

        using var dt = new DataTableWrapper(
          FilteredDataGridView!.DataView!.ToTable(false,
            // Restrict to shown data
            FilteredDataGridView.Columns.Cast<DataGridViewColumn>()
              .Where(col => col.Visible && !ReaderConstants.ArtificialFields.Contains(col.DataPropertyName))
              .OrderBy(col => col.DisplayIndex)
              .Select(col => col.DataPropertyName).ToArray()));
        // can not use filteredDataGridView.Columns directly
        await writer.WriteAsync(dt, processDisplay.CancellationToken);
      }
      catch (Exception ex)
      {
        ParentForm.ShowError(ex);
      }
      finally
      {
        FileStored?.Invoke(this, writeFile);
      }
    }

    private async void ToolStripButtonStoreAsCsvAsync(object? sender, EventArgs e)
    {
      try
      {
        FileSystemUtils.SplitResult split;

        if ((FileSetting is IFileSettingPhysicalFile settingPhysicalFile))
          split = FileSystemUtils.SplitPath(settingPhysicalFile.FullPath);
        else
          split = new FileSystemUtils.SplitResult(Directory.GetCurrentDirectory(),
            $"{FileSetting!.ID}.txt");

        var fileName = WindowsAPICodePackWrapper.Save(split.DirectoryName, "Delimited File",
          "Text file (*.txt)|*.txt|Comma delimited (*.csv)|*.csv|Tab delimited (*.tab;*.tsv)|*.tab;*.tsv|All files (*.*)|*.*", ".csv",
          false,
          split.FileName);

        if (string.IsNullOrEmpty(fileName))
          return;

        await SafeCurrentFile(fileName!, !fileName!.EndsWith(split.Extension, StringComparison.OrdinalIgnoreCase));
      }
      catch (Exception ex)
      {
        ParentForm.ShowError(ex);
      }
    }

    public void ReStoreViewSetting(string fileName) => FilteredDataGridView.ReStoreViewSetting(fileName);

    private async void ToolStripComboBoxFilterType_SelectedIndexChanged(object? sender, EventArgs e)
    {
      /*
       * All Records
       * Error or Warning
       * Only Errors
       * Only Warning
       * No Error or Warning
      */
      if (m_ToolStripComboBoxFilterType.SelectedIndex == 0)
        await RefreshDisplayAsync(FilterType.All, m_CancellationTokenSource.Token);
      if (m_ToolStripComboBoxFilterType.SelectedIndex == 1)
        await RefreshDisplayAsync(FilterType.ErrorsAndWarning, m_CancellationTokenSource.Token);
      if (m_ToolStripComboBoxFilterType.SelectedIndex == 2)
        await RefreshDisplayAsync(FilterType.ShowErrors, m_CancellationTokenSource.Token);
      if (m_ToolStripComboBoxFilterType.SelectedIndex == 3)
        await RefreshDisplayAsync(FilterType.ShowWarning, m_CancellationTokenSource.Token);
      if (m_ToolStripComboBoxFilterType.SelectedIndex == 4)
        await RefreshDisplayAsync(FilterType.ShowIssueFree, m_CancellationTokenSource.Token);
    }

    private async void ToolStripButtonNext_Click(object? sender, EventArgs e)
    {
      if (LoadNextBatchAsync is null || (EndOfFile?.Invoke() ?? true))
        return;
      await ToolStripButtonNext.RunWithHourglassAsync(async () =>
      {
        m_ToolStripLabelCount.Text = " loading...";
        try
        {
          using var processDisplay = new FormProcessDisplay("Load more...", false, m_CancellationTokenSource.Token);
          processDisplay.Show();
          processDisplay.Maximum = 100;
          await LoadNextBatchAsync(processDisplay);
        }
        finally
        {
          var eof = EndOfFile();
          if (eof)
          {
            ToolStripButtonNext.Text = @"All records have been loaded";
            m_ToolStripLabelCount.ForeColor = SystemColors.ControlText;
            m_ToolStripLabelCount.ToolTipText = "Total number of items";
          }
          m_ToolStripLabelCount.Text = m_DataTable.Rows.Count.ToString();
          ToolStripButtonNext.Visible = !eof && m_ShowButtons;
        }
      });
    }

    private class ProcessInformation : IDisposable
    {
      public CancellationTokenSource? CancellationTokenSource;

      public int Found;

      public EventHandler<FoundEventArgs>? FoundResultEvent;

      public bool IsRunning;

      public EventHandler<SearchEventArgs>? SearchCompleteEvent;

      public SearchEventArgs SearchEventArgs = new SearchEventArgs(string.Empty);

      public string SearchText = string.Empty;

      public void Dispose() => CancellationTokenSource?.Dispose();

      public void Cancel() => CancellationTokenSource?.Cancel();
    }
  }
}