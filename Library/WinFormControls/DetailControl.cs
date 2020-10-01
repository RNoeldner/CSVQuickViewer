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

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
    private IContainer components;
    [CanBeNull] public Func<bool> EndOfFile;

    [CanBeNull] public Func<IProcessDisplay, Task> LoadNextBatchAsync;

    // private EventHandler m_BatchSizeChangedEvent;
    private BindingNavigator m_BindingNavigator;

    private BindingSource m_BindingSource;

    private CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();

    private DataColumnCollection m_Columns;

    private ProcessInformation m_CurrentSearch;

    private DataTable m_DataTable;

    private bool m_DisposedValue; // To detect redundant calls

    private FillGuessSettings m_FillGuessSettings;

    [CanBeNull] private FilterDataTable m_FilterDataTable;

    private FormHierarchyDisplay m_HierarchyDisplay;

    private Form m_ParentForm;

    private Search m_Search;

    private bool m_SearchCellsDirty = true;

    private bool m_ShowButtons = true;

    private bool m_ShowFilter = true;

    private ToolStripButton m_ToolStripButtonColumnLength;

    private ToolStripButton m_ToolStripButtonDuplicates;

    private ToolStripButton m_ToolStripButtonHierarchy;

    private ToolStripButton m_ToolStripButtonMoveFirstItem;

    private ToolStripButton m_ToolStripButtonMoveLastItem;

    private ToolStripButton m_ToolStripButtonMoveNextItem;

    private ToolStripButton m_ToolStripButtonMovePreviousItem;

    private ToolStripButton m_ToolStripButtonStore;

    private ToolStripButton m_ToolStripButtonUniqueValues;

    private ToolStripComboBox m_ToolStripComboBoxFilterType;

    private ToolStripContainer m_ToolStripContainer;

    private ToolStripLabel m_ToolStripLabelCount;

    private ToolStripTextBox m_ToolStripTextBox1;

    // private ToolStripTextBox m_ToolStripTextBoxRecSize;
    private ToolStrip m_ToolStripTop;

    public ToolStripButton toolStripButtonNext;

    /// <summary>
    ///   Initializes a new instance of the <see cref="DetailControl" /> class.
    /// </summary>
    public DetailControl()
    {
      InitializeComponent();

      m_ToolStripItems.Add(m_ToolStripComboBoxFilterType);
      m_ToolStripItems.Add(m_ToolStripButtonUniqueValues);
      m_ToolStripItems.Add(m_ToolStripButtonDuplicates);
      m_ToolStripItems.Add(m_ToolStripButtonHierarchy);
      m_ToolStripItems.Add(m_ToolStripButtonColumnLength);
      m_ToolStripItems.Add(m_ToolStripButtonStore);

      FilteredDataGridView.DataViewChanged += DataViewChanged;
      m_ToolStripItems.CollectionChanged += (sender, e) => MoveMenu();
      MoveMenu();
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

    //public string ButtonAsTextCaption
    //{
    //  set => m_ToolStripButtonAsText.Text = value;
    //}

    public CancellationToken CancellationToken
    {
      set => m_CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(value);
    }

    /// <summary>
    ///   Gets the data grid view.
    /// </summary>
    /// <value>The data grid view.</value>
    public FilteredDataGridView FilteredDataGridView { get; private set; }

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
        m_DataTable = value;
        m_FilterDataTable?.Dispose();
        m_FilterDataTable = null;

        if (value == null)
          return;

        m_Columns = m_DataTable.Columns;
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
    public IFileSetting FileSetting
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
    public FillGuessSettings FillGuessSettings
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
    ///   Number of Rows in the Data Table
    /// </summary>
    public int RowsCount => m_DataTable?.Rows.Count ?? 0;

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
    public IEnumerable<string> UniqueFieldName
    {
      set
      {
        // in case we do not have unique names and the table is not loaded do nothing
        if ((value == null || !value.Any()))
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
          m_ToolStripComboBoxFilterType.SelectedIndex = value ? 1 : 0;
        }
        catch
        {
        }

        ;
      }
    }

    public void AddToolStripItem(int index, ToolStripItem item)
    {
      if (index >= m_ToolStripItems.Count)
        m_ToolStripItems.Add(item);
      else
        m_ToolStripItems.Insert(index, item);
    }

    /// <summary>
    ///   Moves the menu in the lower or upper tool-bar
    /// </summary>
    public void MoveMenu()
    {
      var source = (ApplicationSetting.MenuDown) ? m_ToolStripTop : m_BindingNavigator;
      var target = (ApplicationSetting.MenuDown) ? m_BindingNavigator : m_ToolStripTop;
      target.SuspendLayout();
      source.SuspendLayout();
      foreach (var item in m_ToolStripItems)
      {
        item.DisplayStyle = (ApplicationSetting.MenuDown)
          ? ToolStripItemDisplayStyle.Image
          : ToolStripItemDisplayStyle.ImageAndText;
        if (source.Items.Contains(item))
          source.Items.Remove(item);
        if (target.Items.Contains(item))
          target.Items.Remove(item);
      }

      foreach (var item in m_ToolStripItems)
      {
        item.DisplayStyle = (ApplicationSetting.MenuDown)
          ? ToolStripItemDisplayStyle.Image
          : ToolStripItemDisplayStyle.ImageAndText;
        target.Items.Add(item);
      }

      source.ResumeLayout(true);
      target.ResumeLayout(true);
      m_ToolStripContainer.TopToolStripPanelVisible = !ApplicationSetting.MenuDown;
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
        m_CurrentSearch?.Dispose();
        m_DataTable?.Dispose();
        m_FilterDataTable?.Dispose();
        m_HierarchyDisplay?.Dispose();
        m_CancellationTokenSource?.Dispose();
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

    private void BackgroundSearchThread(object obj)
    {
      var processInformation = (ProcessInformation) obj;
      processInformation.IsRunning = true;
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
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
    private void ButtonColumnLength_Click(object sender, EventArgs e)
    {
      m_ToolStripButtonColumnLength.Enabled = false;
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        if (FilteredDataGridView.Columns.Count <= 0)
          return;
        var visible = FilteredDataGridView.Columns.Cast<DataGridViewColumn>()
          .Where(col => col.Visible && !string.IsNullOrEmpty(col.DataPropertyName)).OrderBy(col => col.DisplayIndex)
          .Select(col => col.DataPropertyName).ToList();
        using (var details =
          new FormShowMaxLength(m_DataTable, m_DataTable.Select(FilteredDataGridView.CurrentFilter), visible))
        {
          details.Icon = ParentForm?.Icon;
          details.ShowDialog(ParentForm);
        }
      }
      catch (Exception ex)
      {
        ParentForm.ShowError(ex, "Error trying to determine the length of the columns");
      }
      finally
      {
        m_ToolStripButtonColumnLength.Enabled = true;
        Cursor.Current = oldCursor;
      }
    }

    /// <summary>
    ///   Handles the Click event of the buttonDups control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ButtonDuplicates_Click(object sender, EventArgs e)
    {
      m_ToolStripButtonDuplicates.Enabled = false;
      try
      {
        if (FilteredDataGridView.Columns.Count <= 0)
          return;
        var columnName = FilteredDataGridView.CurrentCell != null
          ? FilteredDataGridView.Columns[FilteredDataGridView.CurrentCell.ColumnIndex].Name
          : FilteredDataGridView.Columns[0].Name;

        using (var details = new FormDuplicatesDisplay(
          m_DataTable.Clone(),
          m_DataTable.Select(FilteredDataGridView.CurrentFilter),
          columnName))
        {
          details.Icon = ParentForm?.Icon;
          details.ShowDialog(ParentForm);
        }
      }
      catch (Exception ex)
      {
        ParentForm.ShowError(ex);
      }
      finally
      {
        m_ToolStripButtonDuplicates.Enabled = true;
      }
    }

    /// <summary>
    ///   Handles the Click event of the buttonHierarchy control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ButtonHierarchy_Click(object sender, EventArgs e)
    {
      m_ToolStripButtonHierarchy.Enabled = false;

      try
      {
        m_HierarchyDisplay?.Close();
        m_HierarchyDisplay =
          new FormHierarchyDisplay(m_DataTable.Clone(), m_DataTable.Select(FilteredDataGridView.CurrentFilter))
          {
            Icon = ParentForm?.Icon
          };
        m_HierarchyDisplay.Show();
      }
      catch (Exception ex)
      {
        ParentForm.ShowError(ex);
      }
      finally
      {
        m_ToolStripButtonHierarchy.Enabled = true;
      }
    }

    /// <summary>
    ///   Handles the Click event of the buttonValues control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ButtonUniqueValues_Click(object sender, EventArgs e)
    {
      m_ToolStripButtonUniqueValues.Enabled = false;
      try
      {
        if (FilteredDataGridView.Columns.Count <= 0)
          return;
        var columnName = FilteredDataGridView.CurrentCell != null
          ? FilteredDataGridView.Columns[FilteredDataGridView.CurrentCell.ColumnIndex].Name
          : FilteredDataGridView.Columns[0].Name;
        using (var details = new FormUniqueDisplay(
          m_DataTable.Clone(),
          m_DataTable.Select(FilteredDataGridView.CurrentFilter),
          columnName))
        {
          details.Icon = ParentForm?.Icon;
          details.ShowDialog(this);
        }
      }
      catch (Exception ex)
      {
        ParentForm.ShowError(ex);
      }
      finally
      {
        m_ToolStripButtonUniqueValues.Enabled = true;
      }
    }

    private void ClearSearch(object sender, EventArgs e)
    {
      (this).SafeInvoke(
        () =>
        {
          FilteredDataGridView.HighlightText = null;
          m_FoundCells.Clear();
          FilteredDataGridView.Refresh();
          m_Search.Results = 0;
        });
      m_CurrentSearch?.Dispose();
    }

    private void DataViewChanged(object sender, EventArgs args)
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

    private void DetailControl_KeyDown(object sender, KeyEventArgs e)
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

      if (m_FilterDataTable == null || m_FilterDataTable.FilterTable.Rows.Count <= 0)
        return;
      if (m_FilterDataTable.ColumnsWithoutErrors.Count == m_Columns.Count)
        return;
      foreach (DataGridViewColumn dgCol in FilteredDataGridView.Columns)
      {
        if (!dgCol.Visible || !m_FilterDataTable.ColumnsWithoutErrors.Contains(dgCol.DataPropertyName)) continue;
        dgCol.Visible = false;
        m_SearchCellsDirty = true;
      }
    }

    private void FilteredDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
      if (!(e.Value is DateTime cellValue))
        return;

      e.Value = StringConversion.DisplayDateTime(cellValue, CultureInfo.CurrentCulture);
    }

    /// <summary>
    ///   Required method for Designer support - do not modify the contents of this method with the
    ///   code editor.
    /// </summary>
    [SuppressMessage("ReSharper", "ArrangeThisQualifier")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "RedundantDelegateCreation")]
    [SuppressMessage("ReSharper", "LocalizableElement")]
    [SuppressMessage("ReSharper", "RedundantCast")]
    private void InitializeComponent()
    {
      components = new System.ComponentModel.Container();
      var resources =
        new System.ComponentModel.ComponentResourceManager(typeof(DetailControl));
      m_ToolStripTop = new System.Windows.Forms.ToolStrip();
      m_ToolStripComboBoxFilterType = new System.Windows.Forms.ToolStripComboBox();
      m_ToolStripButtonUniqueValues = new System.Windows.Forms.ToolStripButton();
      m_ToolStripButtonColumnLength = new System.Windows.Forms.ToolStripButton();
      m_ToolStripButtonDuplicates = new System.Windows.Forms.ToolStripButton();
      m_ToolStripButtonHierarchy = new System.Windows.Forms.ToolStripButton();
      m_ToolStripButtonStore = new System.Windows.Forms.ToolStripButton();
      m_ToolStripContainer = new System.Windows.Forms.ToolStripContainer();
      m_BindingNavigator = new System.Windows.Forms.BindingNavigator(components);
      m_BindingSource = new System.Windows.Forms.BindingSource(components);
      m_ToolStripLabelCount = new System.Windows.Forms.ToolStripLabel();
      m_ToolStripButtonMoveFirstItem = new System.Windows.Forms.ToolStripButton();
      m_ToolStripButtonMovePreviousItem = new System.Windows.Forms.ToolStripButton();
      m_ToolStripTextBox1 = new System.Windows.Forms.ToolStripTextBox();
      m_ToolStripButtonMoveNextItem = new System.Windows.Forms.ToolStripButton();
      m_ToolStripButtonMoveLastItem = new System.Windows.Forms.ToolStripButton();
      toolStripButtonNext = new System.Windows.Forms.ToolStripButton();
      m_Search = new CsvTools.Search();
      FilteredDataGridView = new CsvTools.FilteredDataGridView();
      m_ToolStripTop.SuspendLayout();
      m_ToolStripContainer.BottomToolStripPanel.SuspendLayout();
      m_ToolStripContainer.ContentPanel.SuspendLayout();
      m_ToolStripContainer.TopToolStripPanel.SuspendLayout();
      m_ToolStripContainer.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize) (m_BindingNavigator)).BeginInit();
      m_BindingNavigator.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize) (m_BindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize) (FilteredDataGridView)).BeginInit();
      SuspendLayout();
      // m_ToolStripTop
      m_ToolStripTop.Dock = System.Windows.Forms.DockStyle.None;
      m_ToolStripTop.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
      m_ToolStripTop.ImageScalingSize = new System.Drawing.Size(20, 20);
      m_ToolStripTop.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
      {
        m_ToolStripComboBoxFilterType, m_ToolStripButtonUniqueValues, m_ToolStripButtonColumnLength,
        m_ToolStripButtonDuplicates, m_ToolStripButtonHierarchy, m_ToolStripButtonStore
      });
      m_ToolStripTop.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
      m_ToolStripTop.Location = new System.Drawing.Point(4, 0);
      m_ToolStripTop.Name = "m_ToolStripTop";
      m_ToolStripTop.Size = new System.Drawing.Size(734, 28);
      m_ToolStripTop.TabIndex = 1;
      m_ToolStripTop.Text = "toolStripTop";
      // m_ToolStripComboBoxFilterType
      m_ToolStripComboBoxFilterType.DropDownHeight = 90;
      m_ToolStripComboBoxFilterType.DropDownWidth = 130;
      m_ToolStripComboBoxFilterType.IntegralHeight = false;
      m_ToolStripComboBoxFilterType.Items.AddRange(new object[]
      {
        "All Records", "Error or Warning", "Only Errors", "Only Warning", "No Error or Warning"
      });
      m_ToolStripComboBoxFilterType.Name = "m_ToolStripComboBoxFilterType";
      m_ToolStripComboBoxFilterType.Size = new System.Drawing.Size(150, 28);
      // m_ToolStripButtonUniqueValues
      m_ToolStripButtonUniqueValues.Image =
        ((System.Drawing.Image) (resources.GetObject("m_ToolStripButtonUniqueValues.Image")));
      m_ToolStripButtonUniqueValues.Name = "m_ToolStripButtonUniqueValues";
      m_ToolStripButtonUniqueValues.Size = new System.Drawing.Size(126, 25);
      m_ToolStripButtonUniqueValues.Text = "Unique Values";
      m_ToolStripButtonUniqueValues.ToolTipText = "Display Unique Values";
      m_ToolStripButtonUniqueValues.Click += new System.EventHandler(ButtonUniqueValues_Click);
      // m_ToolStripButtonColumnLength
      m_ToolStripButtonColumnLength.Image =
        ((System.Drawing.Image) (resources.GetObject("m_ToolStripButtonColumnLength.Image")));
      m_ToolStripButtonColumnLength.Name = "m_ToolStripButtonColumnLength";
      m_ToolStripButtonColumnLength.Size = new System.Drawing.Size(133, 25);
      m_ToolStripButtonColumnLength.Text = "Column Length";
      m_ToolStripButtonColumnLength.ToolTipText = "Display Schema information including Length";
      m_ToolStripButtonColumnLength.Click += new System.EventHandler(ButtonColumnLength_Click);
      // m_ToolStripButtonDuplicates
      m_ToolStripButtonDuplicates.Image =
        ((System.Drawing.Image) (resources.GetObject("m_ToolStripButtonDuplicates.Image")));
      m_ToolStripButtonDuplicates.Name = "m_ToolStripButtonDuplicates";
      m_ToolStripButtonDuplicates.Size = new System.Drawing.Size(103, 25);
      m_ToolStripButtonDuplicates.Text = "Duplicates";
      m_ToolStripButtonDuplicates.ToolTipText = "Display Duplicate Values";
      m_ToolStripButtonDuplicates.Click += new System.EventHandler(ButtonDuplicates_Click);
      // m_ToolStripButtonHierarchy
      m_ToolStripButtonHierarchy.Image =
        ((System.Drawing.Image) (resources.GetObject("m_ToolStripButtonHierarchy.Image")));
      m_ToolStripButtonHierarchy.Name = "m_ToolStripButtonHierarchy";
      m_ToolStripButtonHierarchy.Size = new System.Drawing.Size(96, 25);
      m_ToolStripButtonHierarchy.Text = "Hierarchy";
      m_ToolStripButtonHierarchy.ToolTipText = "Display a Hierarchy Structure";
      m_ToolStripButtonHierarchy.Click += new System.EventHandler(ButtonHierarchy_Click);
      // m_ToolStripButtonStore
      m_ToolStripButtonStore.Image =
        ((System.Drawing.Image) (resources.GetObject("m_ToolStripButtonStore.Image")));
      m_ToolStripButtonStore.ImageTransparentColor = System.Drawing.Color.Magenta;
      m_ToolStripButtonStore.Name = "m_ToolStripButtonStore";
      m_ToolStripButtonStore.Size = new System.Drawing.Size(96, 25);
      m_ToolStripButtonStore.Text = "&Write File";
      m_ToolStripButtonStore.ToolTipText = "Store the currently displayed data as delimited text file";
      m_ToolStripButtonStore.Click += new System.EventHandler(ToolStripButtonStoreAsCsvAsync);
      // m_ToolStripContainer
      //
      //
      // m_ToolStripContainer.BottomToolStripPanel
      m_ToolStripContainer.BottomToolStripPanel.Controls.Add(m_BindingNavigator);
      // m_ToolStripContainer.ContentPanel
      m_ToolStripContainer.ContentPanel.Controls.Add(m_Search);
      m_ToolStripContainer.ContentPanel.Controls.Add(FilteredDataGridView);
      m_ToolStripContainer.ContentPanel.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
      m_ToolStripContainer.ContentPanel.Size = new System.Drawing.Size(1328, 400);
      m_ToolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
      m_ToolStripContainer.LeftToolStripPanelVisible = false;
      m_ToolStripContainer.Location = new System.Drawing.Point(0, 0);
      m_ToolStripContainer.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
      m_ToolStripContainer.Name = "m_ToolStripContainer";
      m_ToolStripContainer.RightToolStripPanelVisible = false;
      m_ToolStripContainer.Size = new System.Drawing.Size(1328, 462);
      m_ToolStripContainer.TabIndex = 13;
      m_ToolStripContainer.Text = "toolStripContainer";
      // m_ToolStripContainer.TopToolStripPanel
      m_ToolStripContainer.TopToolStripPanel.Controls.Add(m_ToolStripTop);
      // m_BindingNavigator
      m_BindingNavigator.AddNewItem = null;
      m_BindingNavigator.BindingSource = m_BindingSource;
      m_BindingNavigator.CountItem = m_ToolStripLabelCount;
      m_BindingNavigator.DeleteItem = null;
      m_BindingNavigator.Dock = System.Windows.Forms.DockStyle.None;
      m_BindingNavigator.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
      m_BindingNavigator.ImageScalingSize = new System.Drawing.Size(20, 20);
      m_BindingNavigator.Items.AddRange(new System.Windows.Forms.ToolStripItem[]
      {
        m_ToolStripButtonMoveFirstItem, m_ToolStripButtonMovePreviousItem, m_ToolStripTextBox1, m_ToolStripLabelCount,
        m_ToolStripButtonMoveNextItem, m_ToolStripButtonMoveLastItem, toolStripButtonNext
      });
      m_BindingNavigator.Location = new System.Drawing.Point(4, 0);
      m_BindingNavigator.MoveFirstItem = m_ToolStripButtonMoveFirstItem;
      m_BindingNavigator.MoveLastItem = m_ToolStripButtonMoveLastItem;
      m_BindingNavigator.MoveNextItem = m_ToolStripButtonMoveNextItem;
      m_BindingNavigator.MovePreviousItem = m_ToolStripButtonMovePreviousItem;
      m_BindingNavigator.Name = "m_BindingNavigator";
      m_BindingNavigator.PositionItem = m_ToolStripTextBox1;
      m_BindingNavigator.Size = new System.Drawing.Size(294, 31);
      m_BindingNavigator.TabIndex = 0;
      // m_ToolStripLabelCount
      m_ToolStripLabelCount.Name = "m_ToolStripLabelCount";
      m_ToolStripLabelCount.Size = new System.Drawing.Size(55, 28);
      m_ToolStripLabelCount.Text = "of {0}";
      m_ToolStripLabelCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      m_ToolStripLabelCount.ToolTipText = "Total number of items";
      // m_ToolStripButtonMoveFirstItem
      m_ToolStripButtonMoveFirstItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      m_ToolStripButtonMoveFirstItem.Image =
        ((System.Drawing.Image) (resources.GetObject("m_ToolStripButtonMoveFirstItem.Image")));
      m_ToolStripButtonMoveFirstItem.Name = "m_ToolStripButtonMoveFirstItem";
      m_ToolStripButtonMoveFirstItem.RightToLeftAutoMirrorImage = true;
      m_ToolStripButtonMoveFirstItem.Size = new System.Drawing.Size(29, 28);
      m_ToolStripButtonMoveFirstItem.Text = "Move first";
      // m_ToolStripButtonMovePreviousItem
      m_ToolStripButtonMovePreviousItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      m_ToolStripButtonMovePreviousItem.Image =
        ((System.Drawing.Image) (resources.GetObject("m_ToolStripButtonMovePreviousItem.Image")));
      m_ToolStripButtonMovePreviousItem.Name = "m_ToolStripButtonMovePreviousItem";
      m_ToolStripButtonMovePreviousItem.RightToLeftAutoMirrorImage = true;
      m_ToolStripButtonMovePreviousItem.Size = new System.Drawing.Size(29, 28);
      m_ToolStripButtonMovePreviousItem.Text = "Move previous";
      // m_ToolStripTextBox1
      m_ToolStripTextBox1.AccessibleName = "Position";
      m_ToolStripTextBox1.Font = new System.Drawing.Font("Segoe UI", 9F);
      m_ToolStripTextBox1.Name = "m_ToolStripTextBox1";
      m_ToolStripTextBox1.Size = new System.Drawing.Size(50, 31);
      m_ToolStripTextBox1.Text = "1000";
      m_ToolStripTextBox1.ToolTipText = "Current position";
      // m_ToolStripButtonMoveNextItem
      m_ToolStripButtonMoveNextItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      m_ToolStripButtonMoveNextItem.Image =
        ((System.Drawing.Image) (resources.GetObject("m_ToolStripButtonMoveNextItem.Image")));
      m_ToolStripButtonMoveNextItem.Name = "m_ToolStripButtonMoveNextItem";
      m_ToolStripButtonMoveNextItem.RightToLeftAutoMirrorImage = true;
      m_ToolStripButtonMoveNextItem.Size = new System.Drawing.Size(29, 28);
      m_ToolStripButtonMoveNextItem.Text = "Move next";
      // m_ToolStripButtonMoveLastItem
      m_ToolStripButtonMoveLastItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      m_ToolStripButtonMoveLastItem.Image =
        ((System.Drawing.Image) (resources.GetObject("m_ToolStripButtonMoveLastItem.Image")));
      m_ToolStripButtonMoveLastItem.Name = "m_ToolStripButtonMoveLastItem";
      m_ToolStripButtonMoveLastItem.RightToLeftAutoMirrorImage = true;
      m_ToolStripButtonMoveLastItem.Size = new System.Drawing.Size(29, 28);
      m_ToolStripButtonMoveLastItem.Text = "Move last";
      // toolStripButtonNext
      toolStripButtonNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      toolStripButtonNext.Image = ((System.Drawing.Image) (resources.GetObject("toolStripButtonNext.Image")));
      toolStripButtonNext.Name = "toolStripButtonNext";
      toolStripButtonNext.Size = new System.Drawing.Size(29, 28);
      toolStripButtonNext.Text = "Load More";
      toolStripButtonNext.Enabled = false;
      toolStripButtonNext.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
      toolStripButtonNext.Click += new System.EventHandler(ToolStripButtonNext_Click);
      // m_Search
      m_Search.Anchor =
        (System.Windows.Forms.AnchorStyles.Top |
         System.Windows.Forms.AnchorStyles.Right);
      m_Search.AutoSize = true;
      m_Search.BackColor = System.Drawing.SystemColors.Info;
      m_Search.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      m_Search.Location = new System.Drawing.Point(825, 4);
      m_Search.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
      m_Search.Name = "m_Search";
      m_Search.Results = 0;
      m_Search.Size = new System.Drawing.Size(503, 44);
      m_Search.TabIndex = 1;
      m_Search.Visible = false;
      m_Search.OnResultChanged += new System.EventHandler<CsvTools.SearchEventArgs>(OnSearchResultChanged);
      m_Search.OnSearchChanged += new System.EventHandler<CsvTools.SearchEventArgs>(OnSearchChanged);
      m_Search.OnSearchClear += new System.EventHandler(ClearSearch);

      // m_FilteredDataGridView
      FilteredDataGridView.AllowUserToOrderColumns = true;
      FilteredDataGridView.ColumnHeadersHeightSizeMode =
        System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      FilteredDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      FilteredDataGridView.Location = new System.Drawing.Point(0, 0);
      FilteredDataGridView.Margin = new System.Windows.Forms.Padding(2);

      FilteredDataGridView.RowHeadersWidth = 51;
      FilteredDataGridView.RowTemplate.Height = 33;
      FilteredDataGridView.Size = new System.Drawing.Size(996, 320);
      FilteredDataGridView.TabIndex = 2;
      FilteredDataGridView.CellFormatting +=
        new System.Windows.Forms.DataGridViewCellFormattingEventHandler(FilteredDataGridView_CellFormatting);
      FilteredDataGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(DetailControl_KeyDown);
      // DetailControl
      AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      Controls.Add(m_ToolStripContainer);
      Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
      Name = "DetailControl";
      Size = new System.Drawing.Size(1328, 462);
      m_ToolStripTop.ResumeLayout(false);
      m_ToolStripTop.PerformLayout();
      m_ToolStripContainer.BottomToolStripPanel.ResumeLayout(false);
      m_ToolStripContainer.BottomToolStripPanel.PerformLayout();
      m_ToolStripContainer.ContentPanel.ResumeLayout(false);
      m_ToolStripContainer.ContentPanel.PerformLayout();
      m_ToolStripContainer.TopToolStripPanel.ResumeLayout(false);
      m_ToolStripContainer.TopToolStripPanel.PerformLayout();
      m_ToolStripContainer.ResumeLayout(false);
      m_ToolStripContainer.PerformLayout();
      ((System.ComponentModel.ISupportInitialize) (m_BindingNavigator)).EndInit();
      m_BindingNavigator.ResumeLayout(false);
      m_BindingNavigator.PerformLayout();
      ((System.ComponentModel.ISupportInitialize) (m_BindingSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize) (FilteredDataGridView)).EndInit();
      ResumeLayout(false);
    }

    /// <summary>
    ///   Called when search changes.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="SearchEventArgs" /> instance containing the event data.</param>
    private void OnSearchChanged(object sender, SearchEventArgs e)
    {
      // Stop any current searches
      if (m_CurrentSearch != null && m_CurrentSearch.IsRunning)
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
    private void OnSearchResultChanged(object sender, SearchEventArgs e)
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
    private void ParentForm_Closing(object sender, CancelEventArgs e)
    {
      m_ParentForm.Closing -= ParentForm_Closing;
      m_ParentForm = null;
      if (m_CurrentSearch?.IsRunning ?? false)
        m_CurrentSearch.Cancel();
      if (m_FilterDataTable?.Filtering ?? false)
        m_FilterDataTable.Cancel();
    }

    private void PopulateSearchCellList()
    {
      if (!m_SearchCellsDirty)
        return;
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
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
              m_SearchCells.Add(new KeyValuePair<string, DataGridViewCell>(cell.FormattedValue.ToString(), cell));
        }

        m_SearchCellsDirty = false;
      }
      finally
      {
        Cursor.Current = oldCursor;
      }
    }

    private void ResultFound(object sender, FoundEventArgs args)
    {
      m_FoundCells.Add(args.Cell);
      (this).SafeBeginInvoke(() =>
      {
        m_Search.Results = args.Index;
        FilteredDataGridView.InvalidateCell(args.Cell);
      });
    }

    private void SearchComplete(object sender, SearchEventArgs e) =>
      this.SafeBeginInvoke(() => { m_Search.Results = m_CurrentSearch.Found; });

    private void SetButtonVisibility() =>
      this.SafeBeginInvoke(
        () =>
        {
          // Need to set the control containing the buttons to visible Regular
          m_ToolStripButtonColumnLength.Visible = m_ShowButtons;
          m_ToolStripButtonDuplicates.Visible = m_ShowButtons;
          m_ToolStripButtonUniqueValues.Visible = m_ShowButtons;

          // Extended
          m_ToolStripButtonHierarchy.Visible = m_ShowButtons;
          toolStripButtonNext.Visible = m_ShowButtons && !(EndOfFile?.Invoke() ?? true);
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

          // update the dropdown
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
      if (m_CurrentSearch != null && m_CurrentSearch.IsRunning)
        m_CurrentSearch.Cancel();

      // Hide any showing search
      m_Search.Visible = false;

      var newDt = m_DataTable;
      if (m_FilterDataTable == null && m_DataTable != null)
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
          Sort(oldSortedColumn,
            oldOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
      });
      ShowFilter = m_FilterDataTable?.FilterTable != null && m_FilterDataTable.FilterTable.Rows.Count > 0;
    }

    private void StartSearch(object sender, SearchEventArgs e)
    {
      ClearSearch(this, null);
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

    private async void ToolStripButtonStoreAsCsvAsync(object sender, EventArgs e)
    {
      try
      {
        FileSystemUtils.SplitResult split;
        if ((FileSetting is IFileSettingPhysicalFile settingPhysicalFile))
          split = FileSystemUtils.SplitPath(settingPhysicalFile.FullPath);
        else
          split = new FileSystemUtils.SplitResult(Pri.LongPath.Directory.GetCurrentDirectory(),
            $"{FileSetting.ID}.txt");

        // This will always write a delimited text file
        ICsvFile writeFile = new CsvFile();
        FileSetting.CopyTo(writeFile);
        if (writeFile.JsonFormat)
          writeFile.JsonFormat = false;

        var fileName = WindowsAPICodePackWrapper.Save(
          split.DirectoryName,
          "Delimited File",
          "Text file (*.txt)|*.txt|Comma delimited (*.csv)|*.csv|Tab delimited (*.tab;*.tsv)|*.tab;*.tsv|All files (*.*)|*.*",
          null,
          true,
          split.FileName);
        if (string.IsNullOrEmpty(fileName))
          return;
        if (fileName.EndsWith("tab", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith("tsv", StringComparison.OrdinalIgnoreCase))
          writeFile.FileFormat.FieldDelimiter = "\t";

        if (fileName.EndsWith("csv", StringComparison.OrdinalIgnoreCase))
          writeFile.FileFormat.FieldDelimiter = ",";
        // in case we skipped lines read them as Header so we do not loose them
        if (FileSetting is ICsvFile src && src.SkipRows > 0 && string.IsNullOrEmpty(writeFile.Header))
        {
          using (var iStream = FunctionalDI.OpenRead(src.FullPath))
          using (var sr = new ImprovedTextReader(iStream, src.CodePageId))
          {
            await sr.ToBeginningAsync();
            for (var i = 0; i < src.SkipRows; i++)
              writeFile.Header += await sr.ReadLineAsync() + '\n';
          }
        }

        writeFile.FileName = fileName;
        using (var processDisplay = new FormProcessDisplay(writeFile.ToString(), true, m_CancellationTokenSource.Token))
        {
          processDisplay.Show(ParentForm);
          var writer = new CsvFileWriter(writeFile, processDisplay);

          using (var dt = new DataTableWrapper(
            FilteredDataGridView.DataView.ToTable(false,
              // Restrict to shown data
              FilteredDataGridView.Columns.Cast<DataGridViewColumn>()
                .Where(col => col.Visible && !ReaderConstants.ArtificialFields.Contains(col.DataPropertyName))
                .OrderBy(col => col.DisplayIndex)
                .Select(col => col.DataPropertyName).ToArray())))
          {
            await dt.OpenAsync(processDisplay.CancellationToken);
            // can not use filteredDataGridView.Columns directly
            await writer.WriteAsync(dt, processDisplay.CancellationToken);
          }
        }
      }
      catch (Exception ex)
      {
        ParentForm.ShowError(ex);
      }
    }

    public void ReStoreViewSetting(string fileName) => FilteredDataGridView.ReStoreViewSetting(fileName);

    private async void ToolStripComboBoxFilterType_SelectedIndexChanged(object sender, EventArgs e)
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

    private async void ToolStripButtonNext_Click(object sender, EventArgs e)
    {
      if (LoadNextBatchAsync == null || (EndOfFile?.Invoke() ?? true))
        return;

      toolStripButtonNext.Enabled = false;
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        using (var frm = new FormProcessDisplay("Load", false, m_CancellationTokenSource.Token))
        {
          frm.Show();
          frm.Maximum = 100;
          await LoadNextBatchAsync(frm);
        }
      }
      finally
      {
        Cursor.Current = oldCursor;

        var eof = EndOfFile.Invoke();
        toolStripButtonNext.Enabled = !eof;
        if (eof)
          toolStripButtonNext.Text = "All records have been loaded";
      }
    }

    private class ProcessInformation : IDisposable
    {
      public CancellationTokenSource CancellationTokenSource;

      public int Found;

      public EventHandler<FoundEventArgs> FoundResultEvent;

      public bool IsRunning;

      public EventHandler<SearchEventArgs> SearchCompleteEvent;

      public SearchEventArgs SearchEventArgs;

      public string SearchText;

      public void Dispose() => CancellationTokenSource?.Dispose();

      public void Cancel() => CancellationTokenSource?.Cancel();
    }
  }
}