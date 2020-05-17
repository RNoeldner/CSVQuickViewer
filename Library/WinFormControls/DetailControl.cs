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
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
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

    private IContainer components;

    // private EventHandler m_BatchSizeChangedEvent;
    private BindingNavigator m_BindingNavigator;

    private BindingSource m_BindingSource;

    private CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();

    private DataColumnCollection m_Columns;

    private ProcessInformation m_CurrentSearch;

    private DataTable m_DataTable;

    private bool m_DisposedValue; // To detect redundant calls

    private IFileSetting m_FileSetting;

    private FillGuessSettings m_FillGuessSettings;

    private FilterDataTable m_FilterDataTable;
    private bool m_HasButtonAsText;

    private bool m_HasButtonShowSource;

    private FormHierarchyDisplay m_HierarchyDisplay;

    private Form m_ParentForm;

    private Search m_Search;

    private bool m_SearchCellsDirty = true;

    private bool m_ShowButtons = true;

    private bool m_ShowFilter = true;

    private bool m_ShowSettingsButtons;

    private ToolStripButton m_ToolStripButtonAsText;

    private ToolStripButton m_ToolStripButtonColumnLength;

    private ToolStripButton m_ToolStripButtonDuplicates;

    private ToolStripButton m_ToolStripButtonHierarchy;

    private ToolStripButton m_ToolStripButtonMoveFirstItem;

    private ToolStripButton m_ToolStripButtonMoveLastItem;

    private ToolStripButton m_ToolStripButtonMoveNextItem;

    private ToolStripButton m_ToolStripButtonMovePreviousItem;

    private ToolStripButton m_ToolStripButtonSettings;

    private ToolStripButton m_ToolStripButtonSource;

    private ToolStripButton m_ToolStripButtonStore;

    private ToolStripButton m_ToolStripButtonUniqueValues;

    private ToolStripComboBox m_ToolStripComboBoxFilterType;

    private ToolStripContainer m_ToolStripContainer;

    private ToolStripLabel m_ToolStripLabelCount;

    private ToolStripTextBox m_ToolStripTextBox1;

    // private ToolStripTextBox m_ToolStripTextBoxRecSize;
    private ToolStrip m_ToolStripTop;

    /// <summary>
    ///   Initializes a new instance of the <see cref="DetailControl" /> class.
    /// </summary>
    public DetailControl()
    {
      InitializeComponent();
      DataGridView.DataViewChanged += DataViewChanged;
      SetButtonVisibility();
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
      get => DataGridView.AlternatingRowsDefaultCellStyle;

      set => DataGridView.AlternatingRowsDefaultCellStyle = value;
    }

    public string ButtonAsTextCaption
    {
      set => m_ToolStripButtonAsText.Text = value;
    }

    public CancellationToken CancellationToken
    {
      set => m_CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(value);
    }

    /// <summary>
    ///   Gets the data grid view.
    /// </summary>
    /// <value>The data grid view.</value>
    public FilteredDataGridView DataGridView { get; private set; }

    /// <summary>
    ///   Allows setting the data table
    /// </summary>
    /// <value>The data table.</value>
    public DataTable DataTable
    {
      get => m_DataTable;

      set
      {
        m_DataTable = value;
        m_FilterDataTable?.Dispose();
        m_FilterDataTable = null;

        if (value == null)
          return;
        m_Columns = m_DataTable.Columns;

        m_FilterDataTable = new FilterDataTable(m_DataTable);
        m_FilterDataTable.StartFilter(int.MaxValue, FilterType.ErrorsAndWarning, m_CancellationTokenSource.Token)
          .ContinueWith(task => { ShowFilter = false; });
        DataGridView.FileSetting = m_FileSetting;
        DataGridView.FillGuessSettings = m_FillGuessSettings;
        this.SafeInvoke(() => { m_ToolStripComboBoxFilterType.SelectedIndex = 0; });
        SetDataSource(FilterType.All);
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
      get => DataGridView.DefaultCellStyle;

      set => DataGridView.DefaultCellStyle = value;
    }

    /// <summary>
    ///   A File Setting
    /// </summary>
    public IFileSetting FileSetting
    {
      set
      {
        m_FileSetting = value;
        DataGridView.FileSetting = m_FileSetting;
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
        DataGridView.FillGuessSettings = m_FillGuessSettings;
      }
    }

    public int FrozenColumns
    {
      set => DataGridView.FrozenColumns = value;
    }

    /// <summary>
    ///   Gets or sets a value indicating whether this is a read only.
    /// </summary>
    /// <value><c>true</c> if read only; otherwise, <c>false</c>.</value>
    [Browsable(true)]
    [DefaultValue(false)]
    public bool ReadOnly
    {
      get => DataGridView.ReadOnly;

      set
      {
        if (DataGridView.ReadOnly == value)
          return;
        DataGridView.ReadOnly = value;
        DataGridView.AllowUserToAddRows = !value;
        DataGridView.AllowUserToDeleteRows = !value;
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
        if ((value == null || !value.Any()) && m_FilterDataTable == null)
          return;

        if (m_FilterDataTable == null) return;
        m_FilterDataTable.WaitCompeteFilter(5);
        m_FilterDataTable.UniqueFieldName = value;
      }
    }

    public event EventHandler ButtonAsText
    {
      add
      {
        m_ToolStripButtonAsText.Click += value;
        m_HasButtonAsText = true;
        SetButtonVisibility();
      }

      remove
      {
        m_ToolStripButtonAsText.Click -= value;
        m_HasButtonAsText = false;
        SetButtonVisibility();
      }
    }

    /// <summary>
    ///   Handled the click of the ShowSource Button
    /// </summary>
    public event EventHandler ButtonShowSource
    {
      add
      {
        m_ToolStripButtonSource.Click += value;
        m_HasButtonShowSource = true;
        SetButtonVisibility();
      }

      remove
      {
        m_ToolStripButtonSource.Click -= value;
        m_HasButtonShowSource = false;
        SetButtonVisibility();
      }
    }

    /// <summary>
    ///   Event handler called as the used clicks on settings
    /// </summary>
    public event EventHandler OnSettingsClick
    {
#pragma warning disable CA1030 // Use events where appropriate
      add
#pragma warning restore CA1030
      {
        // Use events where appropriate
        m_ToolStripButtonSettings.Click += value;
        m_ShowSettingsButtons = true;
        SetButtonVisibility();
      }

#pragma warning disable CA1030 // Use events where appropriate
      remove
#pragma warning restore CA1030
      {
        // Use events where appropriate
        m_ToolStripButtonSettings.Click -= value;
        m_ShowSettingsButtons = false;
        SetButtonVisibility();
      }
    }

    /// <summary>
    ///   Moves the menu in the lower or upper tool-bar
    /// </summary>
    public void MoveMenu()
    {
      // Move everything down to bindingNavigator
      if (ApplicationSetting.MenuDown && m_ToolStripTop.Items.Contains(m_ToolStripButtonSettings))
      {
        m_ToolStripTop.Items.Remove(m_ToolStripButtonSettings);
        m_BindingNavigator.Items.Add(m_ToolStripButtonSettings);
        m_ToolStripButtonSettings.DisplayStyle = ToolStripItemDisplayStyle.Image;
        m_ToolStripTop.Items.Remove(m_ToolStripComboBoxFilterType);
        m_BindingNavigator.Items.Add(m_ToolStripComboBoxFilterType);
        m_ToolStripTop.Items.Remove(m_ToolStripButtonUniqueValues);
        m_BindingNavigator.Items.Add(m_ToolStripButtonUniqueValues);
        m_ToolStripButtonUniqueValues.DisplayStyle = ToolStripItemDisplayStyle.Image;
        m_ToolStripTop.Items.Remove(m_ToolStripButtonDuplicates);
        m_BindingNavigator.Items.Add(m_ToolStripButtonDuplicates);
        m_ToolStripButtonDuplicates.DisplayStyle = ToolStripItemDisplayStyle.Image;
        m_ToolStripTop.Items.Remove(m_ToolStripButtonHierarchy);
        m_BindingNavigator.Items.Add(m_ToolStripButtonHierarchy);
        m_ToolStripButtonHierarchy.DisplayStyle = ToolStripItemDisplayStyle.Image;
        m_ToolStripTop.Items.Remove(m_ToolStripButtonColumnLength);
        m_BindingNavigator.Items.Add(m_ToolStripButtonColumnLength);
        m_ToolStripButtonColumnLength.DisplayStyle = ToolStripItemDisplayStyle.Image;
        m_ToolStripTop.Items.Remove(m_ToolStripButtonSource);
        m_BindingNavigator.Items.Add(m_ToolStripButtonSource);
        m_ToolStripButtonSource.DisplayStyle = ToolStripItemDisplayStyle.Image;
        m_ToolStripTop.Items.Remove(m_ToolStripButtonAsText);
        m_BindingNavigator.Items.Add(m_ToolStripButtonAsText);
        m_ToolStripButtonAsText.DisplayStyle = ToolStripItemDisplayStyle.Image;
        m_ToolStripTop.Items.Remove(m_ToolStripButtonStore);
        m_BindingNavigator.Items.Add(m_ToolStripButtonStore);
        m_ToolStripButtonStore.DisplayStyle = ToolStripItemDisplayStyle.Image;
        m_ToolStripContainer.TopToolStripPanelVisible = false;
      }

      if (!ApplicationSetting.MenuDown && m_BindingNavigator.Items.Contains(m_ToolStripButtonSettings))
      {
        m_BindingNavigator.Items.Remove(m_ToolStripButtonSettings);
        m_ToolStripTop.Items.Add(m_ToolStripButtonSettings);
        m_ToolStripButtonSettings.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
        m_BindingNavigator.Items.Remove(m_ToolStripComboBoxFilterType);
        m_ToolStripTop.Items.Add(m_ToolStripComboBoxFilterType);
        m_BindingNavigator.Items.Remove(m_ToolStripButtonUniqueValues);
        m_ToolStripTop.Items.Add(m_ToolStripButtonUniqueValues);
        m_ToolStripButtonUniqueValues.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
        m_BindingNavigator.Items.Remove(m_ToolStripButtonDuplicates);
        m_ToolStripTop.Items.Add(m_ToolStripButtonDuplicates);
        m_ToolStripButtonDuplicates.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
        m_BindingNavigator.Items.Remove(m_ToolStripButtonHierarchy);
        m_ToolStripTop.Items.Add(m_ToolStripButtonHierarchy);
        m_ToolStripButtonHierarchy.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
        m_BindingNavigator.Items.Remove(m_ToolStripButtonColumnLength);
        m_ToolStripTop.Items.Add(m_ToolStripButtonColumnLength);
        m_ToolStripButtonColumnLength.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
        m_BindingNavigator.Items.Remove(m_ToolStripButtonSource);
        m_ToolStripTop.Items.Add(m_ToolStripButtonSource);
        m_ToolStripButtonSource.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
        m_BindingNavigator.Items.Remove(m_ToolStripButtonAsText);
        m_ToolStripTop.Items.Add(m_ToolStripButtonAsText);
        m_ToolStripButtonAsText.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
        m_BindingNavigator.Items.Remove(m_ToolStripButtonStore);
        m_ToolStripTop.Items.Add(m_ToolStripButtonStore);
        m_ToolStripButtonStore.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
        m_ToolStripContainer.TopToolStripPanelVisible = true;
      }

      SetButtonVisibility();

      // toolStripContainer.TopToolStripPanel.Visible = !ApplicationSetting.MenuDown;
    }

    /// <summary>
    ///   Called when [show errors].
    /// </summary>
    public void OnlyShowErrors() => m_ToolStripComboBoxFilterType.SelectedIndex = 1;

    /// <summary>
    ///   Sorts the data grid view on a given column
    /// </summary>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="direction">The direction.</param>
    public void Sort(string columnName, ListSortDirection direction)
    {
      foreach (DataGridViewColumn col in DataGridView.Columns)
        if (col.DataPropertyName.Equals(columnName, StringComparison.OrdinalIgnoreCase) && col.Visible)
        {
          DataGridView.Sort(col, direction);
          break;

          // col.HeaderCell.SortGlyphDirection = direction == ListSortDirection.Ascending ?
          // SortOrder.Ascending : SortOrder.Descending;
        }
    }

    /// <summary>
    ///   Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
        DataGridView.AutoResizeColumns(
          source.Rows.Count < 1000 && source.Columns.Count < 20
            ? DataGridViewAutoSizeColumnsMode.AllCells
            : DataGridViewAutoSizeColumnsMode.DisplayedCells);
    }

    private void BackgroundSearchThread(object obj)
    {
      var processInformation = (ProcessInformation)obj;
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
          Contract.Assume(cell.Key != null);
          Contract.Assume(cell.Value != null);

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
        using (var details = new FormShowMaxLength(m_DataTable, m_DataTable.Select(DataGridView.CurrentFilter)))
        {
          details.Icon = ParentForm.Icon;
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
        if (DataGridView.Columns.Count <= 0)
          return;
        var columnName = DataGridView.CurrentCell != null
          ? DataGridView.Columns[DataGridView.CurrentCell.ColumnIndex].Name
          : DataGridView.Columns[0].Name;

        using (var details = new FormDuplicatesDisplay(
          m_DataTable.Clone(),
          m_DataTable.Select(DataGridView.CurrentFilter),
          columnName))
        {
          details.Icon = ParentForm.Icon;
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
          new FormHierarchyDisplay(m_DataTable.Clone(), m_DataTable.Select(DataGridView.CurrentFilter))
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
        if (DataGridView.Columns.Count <= 0)
          return;
        var columnName = DataGridView.CurrentCell != null
          ? DataGridView.Columns[DataGridView.CurrentCell.ColumnIndex].Name
          : DataGridView.Columns[0].Name;
        using (var details = new FormUniqueDisplay(
          m_DataTable.Clone(),
          m_DataTable.Select(DataGridView.CurrentFilter),
          columnName))
        {
          details.Icon = ParentForm.Icon;
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
      this.SafeInvoke(
        () =>
        {
          DataGridView.HighlightText = null;
          m_FoundCells.Clear();
          DataGridView.Refresh();
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
        foreach (DataGridViewColumn col in DataGridView.Columns)
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

      if (m_FilterDataTable.FilterTable.Rows.Count <= 0)
        return;
      if (m_FilterDataTable.ColumnsWithoutErrors.Count == m_Columns.Count)
        return;
      foreach (DataGridViewColumn dgCol in DataGridView.Columns)
      {
        if (!dgCol.Visible || !m_FilterDataTable.ColumnsWithoutErrors.Contains(dgCol.DataPropertyName)) continue;
        dgCol.Visible = false;
        m_SearchCellsDirty = true;
      }
    }

    private static void FilteredDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
      if (!(e.Value is DateTime cellValue))
        return;

      e.Value = StringConversion.DisplayDateTime(cellValue, CultureInfo.CurrentCulture);
    }

    /// <summary>
    ///   Filters the rows and hides columns that do not have errors
    /// </summary>
    /// <param name="type">The type.</param>
    private void FilterRowsAndColumns(FilterType type)
    {
      // Cancel the current search
      if (m_CurrentSearch != null && m_CurrentSearch.IsRunning)
        m_CurrentSearch.Cancel();

      // Hide any showing search
      m_Search.Visible = false;
      if (type != FilterType.All && type != m_FilterDataTable.FilterType)
      {
        m_FilterDataTable.StartFilter(int.MaxValue, type, m_CancellationTokenSource.Token);
        m_FilterDataTable.WaitCompeteFilter(60);
      }

      var newDt = type == FilterType.All ? m_DataTable : m_FilterDataTable.FilterTable;
      if (newDt == m_BindingSource.DataSource)
        return;
      DataGridView.DataSource = null;
      m_BindingSource.DataSource = newDt;
      try
      {
        DataGridView.DataSource = m_BindingSource; // bindingSource;
        FilterColumns(!type.HasFlag(FilterType.ShowIssueFree));
        AutoResizeColumns(newDt);
      }
      catch (Exception ex)
      {
        ParentForm.ShowError(ex, "Error setting the DataSource of the grid");
      }
    }

    /// <summary>
    ///   Required method for Designer support - do not modify the contents of this method with the
    ///   code editor.
    /// </summary>
    private void InitializeComponent()
    {
      components = new Container();
      var resources = new ComponentResourceManager(typeof(DetailControl));
      var dataGridViewCellStyle1 = new DataGridViewCellStyle();
      var dataGridViewCellStyle2 = new DataGridViewCellStyle();
      m_ToolStripTop = new ToolStrip();
      m_ToolStripButtonSettings = new ToolStripButton();
      m_ToolStripComboBoxFilterType = new ToolStripComboBox();
      m_ToolStripButtonUniqueValues = new ToolStripButton();
      m_ToolStripButtonColumnLength = new ToolStripButton();
      m_ToolStripButtonDuplicates = new ToolStripButton();
      m_ToolStripButtonHierarchy = new ToolStripButton();
      m_ToolStripButtonSource = new ToolStripButton();
      m_ToolStripButtonAsText = new ToolStripButton();
      m_ToolStripButtonStore = new ToolStripButton();
      m_ToolStripContainer = new ToolStripContainer();
      m_BindingNavigator = new BindingNavigator(components);
      m_BindingSource = new BindingSource(components);
      m_ToolStripLabelCount = new ToolStripLabel();
      m_ToolStripButtonMoveFirstItem = new ToolStripButton();
      m_ToolStripButtonMovePreviousItem = new ToolStripButton();
      m_ToolStripTextBox1 = new ToolStripTextBox();
      m_ToolStripButtonMoveNextItem = new ToolStripButton();
      m_ToolStripButtonMoveLastItem = new ToolStripButton();
      m_Search = new Search();
      DataGridView = new FilteredDataGridView();
      m_ToolStripTop.SuspendLayout();
      m_ToolStripContainer.BottomToolStripPanel.SuspendLayout();
      m_ToolStripContainer.ContentPanel.SuspendLayout();
      m_ToolStripContainer.TopToolStripPanel.SuspendLayout();
      m_ToolStripContainer.SuspendLayout();
      ((ISupportInitialize)m_BindingNavigator).BeginInit();
      m_BindingNavigator.SuspendLayout();
      ((ISupportInitialize)m_BindingSource).BeginInit();
      ((ISupportInitialize)DataGridView).BeginInit();
      SuspendLayout();
      // m_ToolStripTop
      m_ToolStripTop.Dock = DockStyle.None;
      m_ToolStripTop.GripStyle = ToolStripGripStyle.Hidden;
      m_ToolStripTop.ImageScalingSize = new Size(20, 20);
      m_ToolStripTop.Items.AddRange(new ToolStripItem[]
      {
        m_ToolStripButtonSettings,
        m_ToolStripComboBoxFilterType,
        m_ToolStripButtonUniqueValues,
        m_ToolStripButtonColumnLength,
        m_ToolStripButtonDuplicates,
        m_ToolStripButtonHierarchy,
        m_ToolStripButtonSource,
        m_ToolStripButtonAsText,
        m_ToolStripButtonStore
      });
      m_ToolStripTop.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
      m_ToolStripTop.Location = new Point(3, 0);
      m_ToolStripTop.Name = "m_ToolStripTop";
      m_ToolStripTop.Size = new Size(517, 27);
      m_ToolStripTop.TabIndex = 1;
      m_ToolStripTop.Text = "toolStripTop";
      // m_ToolStripButtonSettings
      m_ToolStripButtonSettings.Image = (Image)resources.GetObject("m_ToolStripButtonSettings.Image");
      m_ToolStripButtonSettings.Name = "m_ToolStripButtonSettings";
      m_ToolStripButtonSettings.Size = new Size(73, 24);
      m_ToolStripButtonSettings.Text = "Settings";
      m_ToolStripButtonSettings.ToolTipText = "Show CSV Settings";
      m_ToolStripButtonSettings.Visible = false;
      // m_ToolStripComboBoxFilterType
      m_ToolStripComboBoxFilterType.DropDownHeight = 90;
      m_ToolStripComboBoxFilterType.DropDownWidth = 130;
      m_ToolStripComboBoxFilterType.IntegralHeight = false;
      m_ToolStripComboBoxFilterType.Items.AddRange(new object[]
      {
        "All Records",
        "Error or Warning",
        "Only Errors",
        "Only Warning",
        "No Error or Warning"
      });
      m_ToolStripComboBoxFilterType.Name = "m_ToolStripComboBoxFilterType";
      m_ToolStripComboBoxFilterType.Size = new Size(125, 27);
      m_ToolStripComboBoxFilterType.SelectedIndexChanged += ToolStripComboBoxFilterType_SelectedIndexChanged;
      // m_ToolStripButtonUniqueValues
      m_ToolStripButtonUniqueValues.Image = (Image)resources.GetObject("m_ToolStripButtonUniqueValues.Image");
      m_ToolStripButtonUniqueValues.Name = "m_ToolStripButtonUniqueValues";
      m_ToolStripButtonUniqueValues.Size = new Size(105, 24);
      m_ToolStripButtonUniqueValues.Text = "Unique Values";
      m_ToolStripButtonUniqueValues.ToolTipText = "Display Unique Values";
      m_ToolStripButtonUniqueValues.Click += ButtonUniqueValues_Click;
      // m_ToolStripButtonColumnLength
      m_ToolStripButtonColumnLength.Image = (Image)resources.GetObject("m_ToolStripButtonColumnLength.Image");
      m_ToolStripButtonColumnLength.Name = "m_ToolStripButtonColumnLength";
      m_ToolStripButtonColumnLength.Size = new Size(114, 24);
      m_ToolStripButtonColumnLength.Text = "Column Length";
      m_ToolStripButtonColumnLength.ToolTipText = "Display Schema information including Length";
      m_ToolStripButtonColumnLength.Click += ButtonColumnLength_Click;
      // m_ToolStripButtonDuplicates
      m_ToolStripButtonDuplicates.Image = (Image)resources.GetObject("m_ToolStripButtonDuplicates.Image");
      m_ToolStripButtonDuplicates.Name = "m_ToolStripButtonDuplicates";
      m_ToolStripButtonDuplicates.Size = new Size(86, 24);
      m_ToolStripButtonDuplicates.Text = "Duplicates";
      m_ToolStripButtonDuplicates.ToolTipText = "Display Duplicate Values";
      m_ToolStripButtonDuplicates.Click += ButtonDuplicates_Click;
      // m_ToolStripButtonHierachy
      m_ToolStripButtonHierarchy.Image = (Image)resources.GetObject("m_ToolStripButtonHierarchy.Image");
      m_ToolStripButtonHierarchy.Name = "m_ToolStripButtonHierarchy";
      m_ToolStripButtonHierarchy.Size = new Size(82, 24);
      m_ToolStripButtonHierarchy.Text = "Hierarchy";
      m_ToolStripButtonHierarchy.ToolTipText = "Display a Hierarchy Structure";
      m_ToolStripButtonHierarchy.Click += ButtonHierarchy_Click;
      // m_ToolStripButtonSource
      m_ToolStripButtonSource.Image = (Image)resources.GetObject("m_ToolStripButtonSource.Image");
      m_ToolStripButtonSource.Name = "m_ToolStripButtonSource";
      m_ToolStripButtonSource.Size = new Size(95, 24);
      m_ToolStripButtonSource.Text = "View Source";
      m_ToolStripButtonSource.Visible = false;
      // m_ToolStripButtonAsText
      m_ToolStripButtonAsText.Image = (Image)resources.GetObject("m_ToolStripButtonAsText.Image");
      m_ToolStripButtonAsText.ImageTransparentColor = Color.Magenta;
      m_ToolStripButtonAsText.Name = "m_ToolStripButtonAsText";
      m_ToolStripButtonAsText.Size = new Size(52, 24);
      m_ToolStripButtonAsText.Text = "Text";
      m_ToolStripButtonAsText.Visible = false;
      // m_ToolStripButtonStore
      m_ToolStripButtonStore.Image = (Image)resources.GetObject("m_ToolStripButtonStore.Image");
      m_ToolStripButtonStore.ImageTransparentColor = Color.Magenta;
      m_ToolStripButtonStore.Name = "m_ToolStripButtonStore";
      m_ToolStripButtonStore.Size = new Size(80, 24);
      m_ToolStripButtonStore.Text = "&Write File";
      m_ToolStripButtonStore.ToolTipText = "Store the currently displayed data as delimited text file";
      m_ToolStripButtonStore.Visible = true;
      m_ToolStripButtonStore.Click += ToolStripButtonStoreAsCsvAsync;
      // m_ToolStripContainer
      //
      //
      // m_ToolStripContainer.BottomToolStripPanel
      m_ToolStripContainer.BottomToolStripPanel.Controls.Add(m_BindingNavigator);
      // m_ToolStripContainer.ContentPanel
      m_ToolStripContainer.ContentPanel.Controls.Add(m_Search);
      m_ToolStripContainer.ContentPanel.Controls.Add(DataGridView);
      m_ToolStripContainer.ContentPanel.Margin = new Padding(4);
      m_ToolStripContainer.ContentPanel.Size = new Size(1162, 378);
      m_ToolStripContainer.Dock = DockStyle.Fill;
      m_ToolStripContainer.LeftToolStripPanelVisible = false;
      m_ToolStripContainer.Location = new Point(0, 0);
      m_ToolStripContainer.Margin = new Padding(4);
      m_ToolStripContainer.Name = "m_ToolStripContainer";
      m_ToolStripContainer.RightToolStripPanelVisible = false;
      m_ToolStripContainer.Size = new Size(1162, 432);
      m_ToolStripContainer.TabIndex = 13;
      m_ToolStripContainer.Text = "toolStripContainer";
      // m_ToolStripContainer.TopToolStripPanel
      m_ToolStripContainer.TopToolStripPanel.Controls.Add(m_ToolStripTop);
      // m_BindingNavigator
      m_BindingNavigator.AddNewItem = null;
      m_BindingNavigator.BindingSource = m_BindingSource;
      m_BindingNavigator.CountItem = m_ToolStripLabelCount;
      m_BindingNavigator.DeleteItem = null;
      m_BindingNavigator.Dock = DockStyle.None;
      m_BindingNavigator.GripStyle = ToolStripGripStyle.Hidden;
      m_BindingNavigator.ImageScalingSize = new Size(20, 20);
      m_BindingNavigator.Items.AddRange(new ToolStripItem[]
      {
        m_ToolStripButtonMoveFirstItem,
        m_ToolStripButtonMovePreviousItem,
        m_ToolStripTextBox1,
        m_ToolStripLabelCount,
        m_ToolStripButtonMoveNextItem,
        m_ToolStripButtonMoveLastItem
      });
      m_BindingNavigator.Location = new Point(4, 0);
      m_BindingNavigator.MoveFirstItem = m_ToolStripButtonMoveFirstItem;
      m_BindingNavigator.MoveLastItem = m_ToolStripButtonMoveLastItem;
      m_BindingNavigator.MoveNextItem = m_ToolStripButtonMoveNextItem;
      m_BindingNavigator.MovePreviousItem = m_ToolStripButtonMovePreviousItem;
      m_BindingNavigator.Name = "m_BindingNavigator";
      m_BindingNavigator.PositionItem = m_ToolStripTextBox1;
      m_BindingNavigator.Size = new Size(186, 27);
      m_BindingNavigator.TabIndex = 0;
      // m_ToolStripLabelCount
      m_ToolStripLabelCount.Name = "m_ToolStripLabelCount";
      m_ToolStripLabelCount.Size = new Size(35, 24);
      m_ToolStripLabelCount.Text = "of {0}";
      m_ToolStripLabelCount.TextAlign = ContentAlignment.MiddleLeft;
      m_ToolStripLabelCount.ToolTipText = "Total number of items";
      // m_ToolStripButtonMoveFirstItem
      m_ToolStripButtonMoveFirstItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
      m_ToolStripButtonMoveFirstItem.Image = (Image)resources.GetObject("m_ToolStripButtonMoveFirstItem.Image");
      m_ToolStripButtonMoveFirstItem.Name = "m_ToolStripButtonMoveFirstItem";
      m_ToolStripButtonMoveFirstItem.RightToLeftAutoMirrorImage = true;
      m_ToolStripButtonMoveFirstItem.Size = new Size(24, 24);
      m_ToolStripButtonMoveFirstItem.Text = "Move first";
      // m_ToolStripButtonMovePreviousItem
      m_ToolStripButtonMovePreviousItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
      m_ToolStripButtonMovePreviousItem.Image = (Image)resources.GetObject("m_ToolStripButtonMovePreviousItem.Image");
      m_ToolStripButtonMovePreviousItem.Name = "m_ToolStripButtonMovePreviousItem";
      m_ToolStripButtonMovePreviousItem.RightToLeftAutoMirrorImage = true;
      m_ToolStripButtonMovePreviousItem.Size = new Size(24, 24);
      m_ToolStripButtonMovePreviousItem.Text = "Move previous";
      // m_ToolStripTextBox1
      m_ToolStripTextBox1.AccessibleName = "Position";
      m_ToolStripTextBox1.Name = "m_ToolStripTextBox1";
      m_ToolStripTextBox1.Size = new Size(50, 27);
      m_ToolStripTextBox1.Text = "0";
      m_ToolStripTextBox1.ToolTipText = "Current position";
      // m_ToolStripButtonMoveNextItem
      m_ToolStripButtonMoveNextItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
      m_ToolStripButtonMoveNextItem.Image = (Image)resources.GetObject("m_ToolStripButtonMoveNextItem.Image");
      m_ToolStripButtonMoveNextItem.Name = "m_ToolStripButtonMoveNextItem";
      m_ToolStripButtonMoveNextItem.RightToLeftAutoMirrorImage = true;
      m_ToolStripButtonMoveNextItem.Size = new Size(24, 24);
      m_ToolStripButtonMoveNextItem.Text = "Move next";
      // m_ToolStripButtonMoveLastItem
      m_ToolStripButtonMoveLastItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
      m_ToolStripButtonMoveLastItem.Image = (Image)resources.GetObject("m_ToolStripButtonMoveLastItem.Image");
      m_ToolStripButtonMoveLastItem.Name = "m_ToolStripButtonMoveLastItem";
      m_ToolStripButtonMoveLastItem.RightToLeftAutoMirrorImage = true;
      m_ToolStripButtonMoveLastItem.Size = new Size(24, 24);
      m_ToolStripButtonMoveLastItem.Text = "Move last";
      // m_Search
      m_Search.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      m_Search.AutoSize = true;
      m_Search.BackColor = SystemColors.Info;
      m_Search.BorderStyle = BorderStyle.FixedSingle;
      m_Search.Location = new Point(722, 4);
      m_Search.Margin = new Padding(4);
      m_Search.Name = "m_Search";
      m_Search.Results = 0;
      m_Search.Size = new Size(440, 41);
      m_Search.TabIndex = 1;
      m_Search.Visible = false;
      m_Search.OnResultChanged += OnSearchResultChanged;
      m_Search.OnSearchChanged += OnSearchChanged;
      m_Search.OnSearchClear += ClearSearch;
      // m_FilteredDataGridView
      DataGridView.AllowUserToOrderColumns = true;
      dataGridViewCellStyle1.BackColor = Color.Gainsboro;
      DataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
      DataGridView.BorderStyle = BorderStyle.Fixed3D;
      DataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle2.BackColor = SystemColors.Window;
      dataGridViewCellStyle2.ForeColor = Color.Black;
      dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
      dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
      dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
      DataGridView.DefaultCellStyle = dataGridViewCellStyle2;
      DataGridView.Dock = DockStyle.Fill;
      DataGridView.Location = new Point(0, 0);
      DataGridView.Margin = new Padding(4);
      DataGridView.Name = "m_FilteredDataGridView";
      DataGridView.RowHeadersWidth = 62;
      DataGridView.Size = new Size(1162, 378);
      DataGridView.TabIndex = 1;
      DataGridView.CellFormatting += FilteredDataGridView_CellFormatting;
      DataGridView.KeyDown += DetailControl_KeyDown;
      // DetailControl
      AutoScaleDimensions = new SizeF(7F, 15F);
      AutoScaleMode = AutoScaleMode.Font;
      Controls.Add(m_ToolStripContainer);
      Margin = new Padding(4);
      Name = "DetailControl";
      Size = new Size(1162, 432);
      KeyDown += DetailControl_KeyDown;
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
      ((ISupportInitialize)m_BindingNavigator).EndInit();
      m_BindingNavigator.ResumeLayout(false);
      m_BindingNavigator.PerformLayout();
      ((ISupportInitialize)m_BindingSource).EndInit();
      ((ISupportInitialize)DataGridView).EndInit();
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
      DataGridView.SafeInvoke(
        () =>
        {
          try
          {
            DataGridView.CurrentCell = m_FoundCells[e.Result - 1];
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
        var visible = DataGridView.Columns.Cast<DataGridViewColumn>()
          .Where(col => col.Visible && !string.IsNullOrEmpty(col.DataPropertyName)).ToList();

        foreach (DataGridViewRow row in DataGridView.Rows)
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
      this.SafeBeginInvoke(
        () =>
        {
          m_Search.Results = args.Index;
          DataGridView.InvalidateCell(args.Cell);
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
          m_ToolStripButtonAsText.Visible = m_ShowButtons && m_HasButtonAsText;

          // Extended
          m_ToolStripButtonHierarchy.Visible = m_ShowButtons;
          m_ToolStripButtonStore.Visible = m_ShowButtons;
          m_ToolStripButtonSource.Visible = m_ShowButtons && m_HasButtonShowSource;

          // Settings
          m_ToolStripButtonSettings.Visible = m_ShowButtons && m_ShowSettingsButtons;
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
    private void SetDataSource(FilterType type)
    {
      if (m_DataTable == null)
        return;

      this.SafeInvoke(
        () =>
        {
          // update the dropdown
          if ((type == FilterType.All) & (m_ToolStripComboBoxFilterType.SelectedIndex != 0))
            m_ToolStripComboBoxFilterType.SelectedIndex = 0;
          if ((type == FilterType.ErrorsAndWarning) & (m_ToolStripComboBoxFilterType.SelectedIndex != 1))
            m_ToolStripComboBoxFilterType.SelectedIndex = 1;
          if ((type == FilterType.ShowErrors) & (m_ToolStripComboBoxFilterType.SelectedIndex != 2))
            m_ToolStripComboBoxFilterType.SelectedIndex = 2;
          if ((type == FilterType.ShowWarning) & (m_ToolStripComboBoxFilterType.SelectedIndex != 3))
            m_ToolStripComboBoxFilterType.SelectedIndex = 3;
          if ((type == FilterType.ShowIssueFree) & (m_ToolStripComboBoxFilterType.SelectedIndex != 4))
            m_ToolStripComboBoxFilterType.SelectedIndex = 4;
        });
      var oldSortedColumn = DataGridView.SortedColumn?.DataPropertyName;
      var oldOrder = DataGridView.SortOrder;
      FilterRowsAndColumns(type);

      // bindingSource.ResumeBinding();
      DataGridView.ColumnVisibilityChanged();
      DataGridView.SetRowHeight();

      if (oldOrder != SortOrder.None && !string.IsNullOrEmpty(oldSortedColumn))
        Sort(
          oldSortedColumn,
          oldOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
    }

    private void StartSearch(object sender, SearchEventArgs e)
    {
      ClearSearch(this, null);
      DataGridView.HighlightText = e.SearchText;

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
      CsvTools.FileSystemUtils.SplitResult split = null;
      if ((m_FileSetting is IFileSettingPhysicalFile settingPhysicalFile))
        split = FileSystemUtils.SplitPath(settingPhysicalFile.FullPath);
      else
        split = new FileSystemUtils.SplitResult(Pri.LongPath.Directory.GetCurrentDirectory(), $"{m_FileSetting.ID}.txt");

      // This will always write a delimited text file
      ICsvFile writeFile = new CsvFile();
      m_FileSetting.CopyTo(writeFile);
      if (writeFile.JsonFormat)
        writeFile.JsonFormat = false;

      var fileName = WindowsAPICodePackWrapper.Save(
        split.DirectoryName,
        "Delimited File",
        "Text file (*.txt)|*.txt|Comma delimited (*.csv)|*.csv|Tab delimited (*.tab;*.tsv)|*.tab;*.tsv|All files (*.*)|*.*",
        null,
        split.FileName);
      if (string.IsNullOrEmpty(fileName))
        return;
      if (fileName.EndsWith("tab", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith("tsv", StringComparison.OrdinalIgnoreCase))
        writeFile.FileFormat.FieldDelimiter = "\t";

      if (fileName.EndsWith("csv", StringComparison.OrdinalIgnoreCase))
        writeFile.FileFormat.FieldDelimiter = ",";

      writeFile.FileName = fileName;
      using (var processDisplay = new FormProcessDisplay(writeFile.ToString(), true, m_CancellationTokenSource.Token))
      {
        processDisplay.Show(ParentForm);
        var writer = new CsvFileWriter(writeFile, TimeZoneInfo.Local.Id, processDisplay);

        using (var dt = new DataTableReader(
          DataGridView.DataView.ToTable(false,
          // Restrict to shown data
          DataGridView.Columns.Cast<DataGridViewColumn>()
          .Where(col => col.Visible && !BaseFileReader.ArtificialFields.Contains(col.DataPropertyName))
          .OrderBy(col => col.DisplayIndex)
          .Select(col => col.DataPropertyName).ToArray()), "Export",
          processDisplay))
        {
          await dt.OpenAsync();
          // can not use filteredDataGridView.Columns directly
          await writer.WriteAsync(dt);
        }
      }
    }

    private void ToolStripComboBoxFilterType_SelectedIndexChanged(object sender, EventArgs e)
    {
      /*
       * All Records
       * Error or Warning
       * Only Errors
       * Only Warning
       * No Error or Warning
      */
      if (m_ToolStripComboBoxFilterType.SelectedIndex == 0)
        SetDataSource(FilterType.All);
      if (m_ToolStripComboBoxFilterType.SelectedIndex == 1)
        SetDataSource(FilterType.ErrorsAndWarning);
      if (m_ToolStripComboBoxFilterType.SelectedIndex == 2)
        SetDataSource(FilterType.ShowErrors);
      if (m_ToolStripComboBoxFilterType.SelectedIndex == 3)
        SetDataSource(FilterType.ShowWarning);
      if (m_ToolStripComboBoxFilterType.SelectedIndex == 4)
        SetDataSource(FilterType.ShowIssueFree);
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