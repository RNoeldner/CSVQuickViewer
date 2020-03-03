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

namespace CsvTools
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Data;
  using System.Diagnostics;
  using System.Diagnostics.Contracts;
  using System.Globalization;
  using System.Linq;
  using System.Threading;
  using System.Windows.Forms;

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

    private ProcessInformaton m_CurrentSearch;

    private DataTable m_DataTable;

    private bool m_DisposedValue; // To detect redundant calls

    private IFileSetting m_FileSetting;

    private FillGuessSettings m_FillGuessSettings;

    private FilterDataTable m_FilterDataTable;

    private FilteredDataGridView m_FilteredDataGridView;

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

    private ToolStripButton m_ToolStripButtonHierachy;

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
      m_FilteredDataGridView.DataViewChanged += DataViewChanged;
      SetButtonVisibility();
      MoveMenu();
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
    ///   AlternatingRowDefaultCellSyle of data grid
    /// </summary>
    [Browsable(true)]
    [TypeConverter(typeof(DataGridViewCellStyleConverter))]
    [Category("Appearance")]
    public DataGridViewCellStyle AlternatingRowDefaultCellSyle
    {
      get => m_FilteredDataGridView.AlternatingRowsDefaultCellStyle;

      set => m_FilteredDataGridView.AlternatingRowsDefaultCellStyle = value;
    }

    public string ButtonAsTextCaption
    {
      set => m_ToolStripButtonAsText.Text = value;
    }

    public CancellationToken CancellationToken
    {
      get => m_CancellationTokenSource.Token;
      set => m_CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(value);
    }

    /// <summary>
    ///   Gets the data grid view.
    /// </summary>
    /// <value>The data grid view.</value>
    public FilteredDataGridView DataGridView => m_FilteredDataGridView;

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
        m_FilterDataTable = null;

        if (value == null)
          return;
        m_Columns = m_DataTable.Columns;

        m_FilterDataTable = new FilterDataTable(m_DataTable, m_CancellationTokenSource.Token);
        m_FilterDataTable.StartFilter(int.MaxValue, FilterType.ErrorsAndWarning, () => { ShowFilter = false; });
        m_FilteredDataGridView.FileSetting = m_FileSetting;
        m_FilteredDataGridView.FillGuessSettings = m_FillGuessSettings;
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
      get => m_FilteredDataGridView.DefaultCellStyle;

      set => m_FilteredDataGridView.DefaultCellStyle = value;
    }

    /// <summary>
    ///   A File Setting
    /// </summary>
    public IFileSetting FileSetting
    {
      set
      {
        m_FileSetting = value;
        m_FilteredDataGridView.FileSetting = m_FileSetting;
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
        m_FilteredDataGridView.FillGuessSettings = m_FillGuessSettings;
      }
    }

    public int FrozenColumns
    {
      set => m_FilteredDataGridView.FrozenColumns = value;
    }

    /// <summary>
    ///   Gets or sets a value indicating whether this is a read only.
    /// </summary>
    /// <value><c>true</c> if read only; otherwise, <c>false</c>.</value>
    [Browsable(true)]
    [DefaultValue(false)]
    public bool ReadOnly
    {
      get => m_FilteredDataGridView.ReadOnly;

      set
      {
        if (DataGridView.ReadOnly == value)
          return;
        m_FilteredDataGridView.ReadOnly = value;
        m_FilteredDataGridView.AllowUserToAddRows = !value;
        m_FilteredDataGridView.AllowUserToDeleteRows = !value;
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

        // need to wait until m_FilterDataTable is set in the background
        var start = DateTime.Now;

        // wait for it to be set by background process
        while (m_FilterDataTable.Filtering && !m_CancellationTokenSource.IsCancellationRequested
                                           && (DateTime.Now - start).TotalSeconds < 5d)
          Thread.Sleep(100);

        if (m_FilterDataTable != null)
          m_FilterDataTable.UniqueFieldName = value;
      }
    }

    internal Func<string, IDataReader> ToolDataReader
    {
      get => m_FilteredDataGridView.ToolDataReader;
      set => m_FilteredDataGridView.ToolDataReader = value;
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
        m_ToolStripTop.Items.Remove(m_ToolStripButtonHierachy);
        m_BindingNavigator.Items.Add(m_ToolStripButtonHierachy);
        m_ToolStripButtonHierachy.DisplayStyle = ToolStripItemDisplayStyle.Image;
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
        m_BindingNavigator.Items.Remove(m_ToolStripButtonHierachy);
        m_ToolStripTop.Items.Add(m_ToolStripButtonHierachy);
        m_ToolStripButtonHierachy.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
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
      foreach (DataGridViewColumn col in m_FilteredDataGridView.Columns)
      {
        if (col.DataPropertyName.Equals(columnName, StringComparison.OrdinalIgnoreCase) && col.Visible)
        {
          m_FilteredDataGridView.Sort(col, direction);
          break;

          // col.HeaderCell.SortGlyphDirection = direction == ListSortDirection.Ascending ?
          // SortOrder.Ascending : SortOrder.Descending;
        }
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
      {
        m_FilteredDataGridView.AutoResizeColumns(
          source.Rows.Count < 1000 && source.Columns.Count < 20
            ? DataGridViewAutoSizeColumnsMode.AllCells
            : DataGridViewAutoSizeColumnsMode.DisplayedCells);
      }
    }

    private void BackgoundSearchThread(object obj)
    {
      var processInformation = (ProcessInformaton)obj;
      processInformation.IsRunning = true;
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        if (string.IsNullOrEmpty(processInformation.SearchText))
          return;

        // Do not search for an text shorter than 2 if we have a lot of data
        if (processInformation.SearchText.Length < 2 && ClassLibraryCsvExtensionMethods.Count(m_SearchCells) > 10000)
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
          details.Icon = ParentForm?.Icon;
          details.ShowDialog();
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
        var columnName = m_FilteredDataGridView.CurrentCell != null
                           ? m_FilteredDataGridView.Columns[DataGridView.CurrentCell.ColumnIndex].Name
                           : m_FilteredDataGridView.Columns[0].Name;

        using (var details = new FormDuplicatesDisplay(
          m_DataTable.Clone(),
          m_DataTable.Select(DataGridView.CurrentFilter),
          columnName))
        {
          details.Icon = ParentForm?.Icon;
          details.ShowDialog();
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
    ///   Handles the Click event of the buttonHierachy control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ButtonHierachy_Click(object sender, EventArgs e)
    {
      m_ToolStripButtonHierachy.Enabled = false;

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
        m_ToolStripButtonHierachy.Enabled = true;
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
        var columnName = m_FilteredDataGridView.CurrentCell != null
                           ? m_FilteredDataGridView.Columns[DataGridView.CurrentCell.ColumnIndex].Name
                           : m_FilteredDataGridView.Columns[0].Name;
        using (var details = new FormUniqueDisplay(
          m_DataTable.Clone(),
          m_DataTable.Select(DataGridView.CurrentFilter),
          columnName))
        {
          details.Icon = ParentForm?.Icon;
          details.ShowDialog();
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
            m_FilteredDataGridView.HighlightText = null;
            m_FoundCells.Clear();
            m_FilteredDataGridView.Refresh();
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
        foreach (DataGridViewColumn col in m_FilteredDataGridView.Columns)
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
      foreach (DataGridViewColumn dgcol in m_FilteredDataGridView.Columns)
      {
        if (!dgcol.Visible || !m_FilterDataTable.ColumnsWithoutErrors.Contains(dgcol.DataPropertyName)) continue;
        dgcol.Visible = false;
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
        m_FilterDataTable.Filter(int.MaxValue, type);
      }

      var newDt = (type == FilterType.All) ? m_DataTable : m_FilterDataTable.FilterTable;
      if (newDt == m_BindingSource.DataSource)
        return;
      m_FilteredDataGridView.DataSource = null;
      m_BindingSource.DataSource = newDt;
      try
      {
        m_FilteredDataGridView.DataSource = m_BindingSource; // bindingSource;
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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DetailControl));
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      this.m_ToolStripTop = new System.Windows.Forms.ToolStrip();
      this.m_ToolStripButtonSettings = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripComboBoxFilterType = new System.Windows.Forms.ToolStripComboBox();
      this.m_ToolStripButtonUniqueValues = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripButtonColumnLength = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripButtonDuplicates = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripButtonHierachy = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripButtonSource = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripButtonAsText = new System.Windows.Forms.ToolStripButton();
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
      this.m_Search = new CsvTools.Search();
      this.m_FilteredDataGridView = new CsvTools.FilteredDataGridView();
      this.m_ToolStripTop.SuspendLayout();
      this.m_ToolStripContainer.BottomToolStripPanel.SuspendLayout();
      this.m_ToolStripContainer.ContentPanel.SuspendLayout();
      this.m_ToolStripContainer.TopToolStripPanel.SuspendLayout();
      this.m_ToolStripContainer.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.m_BindingNavigator)).BeginInit();
      this.m_BindingNavigator.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.m_BindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_FilteredDataGridView)).BeginInit();
      this.SuspendLayout();
      // 
      // m_ToolStripTop
      // 
      this.m_ToolStripTop.Dock = System.Windows.Forms.DockStyle.None;
      this.m_ToolStripTop.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
      this.m_ToolStripTop.ImageScalingSize = new System.Drawing.Size(20, 20);
      this.m_ToolStripTop.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ToolStripButtonSettings,
            this.m_ToolStripComboBoxFilterType,
            this.m_ToolStripButtonUniqueValues,
            this.m_ToolStripButtonColumnLength,
            this.m_ToolStripButtonDuplicates,
            this.m_ToolStripButtonHierachy,
            this.m_ToolStripButtonSource,
            this.m_ToolStripButtonAsText,
            this.m_ToolStripButtonStore});
      this.m_ToolStripTop.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
      this.m_ToolStripTop.Location = new System.Drawing.Point(4, 0);
      this.m_ToolStripTop.Name = "m_ToolStripTop";
      this.m_ToolStripTop.Size = new System.Drawing.Size(588, 28);
      this.m_ToolStripTop.TabIndex = 1;
      this.m_ToolStripTop.Text = "toolStripTop";
      // 
      // m_ToolStripButtonSettings
      // 
      this.m_ToolStripButtonSettings.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonSettings.Image")));
      this.m_ToolStripButtonSettings.Name = "m_ToolStripButtonSettings";
      this.m_ToolStripButtonSettings.Size = new System.Drawing.Size(86, 25);
      this.m_ToolStripButtonSettings.Text = "Settings";
      this.m_ToolStripButtonSettings.ToolTipText = "Show CSV Settings";
      this.m_ToolStripButtonSettings.Visible = false;
      // 
      // m_ToolStripComboBoxFilterType
      // 
      this.m_ToolStripComboBoxFilterType.DropDownHeight = 90;
      this.m_ToolStripComboBoxFilterType.DropDownWidth = 130;
      this.m_ToolStripComboBoxFilterType.IntegralHeight = false;
      this.m_ToolStripComboBoxFilterType.Items.AddRange(new object[] {
            "All Records",
            "Error or Warning",
            "Only Errors",
            "Only Warning",
            "No Error or Warning"});
      this.m_ToolStripComboBoxFilterType.Name = "m_ToolStripComboBoxFilterType";
      this.m_ToolStripComboBoxFilterType.Size = new System.Drawing.Size(125, 28);
      this.m_ToolStripComboBoxFilterType.SelectedIndexChanged += new System.EventHandler(this.ToolStripComboBoxFilterType_SelectedIndexChanged);
      // 
      // m_ToolStripButtonUniqueValues
      // 
      this.m_ToolStripButtonUniqueValues.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonUniqueValues.Image")));
      this.m_ToolStripButtonUniqueValues.Name = "m_ToolStripButtonUniqueValues";
      this.m_ToolStripButtonUniqueValues.Size = new System.Drawing.Size(126, 25);
      this.m_ToolStripButtonUniqueValues.Text = "Unique Values";
      this.m_ToolStripButtonUniqueValues.ToolTipText = "Display Unique Values";
      this.m_ToolStripButtonUniqueValues.Click += new System.EventHandler(this.ButtonUniqueValues_Click);
      // 
      // m_ToolStripButtonColumnLength
      // 
      this.m_ToolStripButtonColumnLength.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonColumnLength.Image")));
      this.m_ToolStripButtonColumnLength.Name = "m_ToolStripButtonColumnLength";
      this.m_ToolStripButtonColumnLength.Size = new System.Drawing.Size(133, 25);
      this.m_ToolStripButtonColumnLength.Text = "Column Length";
      this.m_ToolStripButtonColumnLength.ToolTipText = "Display Schema information including Length";
      this.m_ToolStripButtonColumnLength.Click += new System.EventHandler(this.ButtonColumnLength_Click);
      // 
      // m_ToolStripButtonDuplicates
      // 
      this.m_ToolStripButtonDuplicates.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonDuplicates.Image")));
      this.m_ToolStripButtonDuplicates.Name = "m_ToolStripButtonDuplicates";
      this.m_ToolStripButtonDuplicates.Size = new System.Drawing.Size(103, 25);
      this.m_ToolStripButtonDuplicates.Text = "Duplicates";
      this.m_ToolStripButtonDuplicates.ToolTipText = "Display Duplicate Values";
      this.m_ToolStripButtonDuplicates.Click += new System.EventHandler(this.ButtonDuplicates_Click);
      // 
      // m_ToolStripButtonHierachy
      // 
      this.m_ToolStripButtonHierachy.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonHierachy.Image")));
      this.m_ToolStripButtonHierachy.Name = "m_ToolStripButtonHierachy";
      this.m_ToolStripButtonHierachy.Size = new System.Drawing.Size(96, 25);
      this.m_ToolStripButtonHierachy.Text = "Hierarchy";
      this.m_ToolStripButtonHierachy.ToolTipText = "Display a Hierarchy Structure";
      this.m_ToolStripButtonHierachy.Click += new System.EventHandler(this.ButtonHierachy_Click);
      // 
      // m_ToolStripButtonSource
      // 
      this.m_ToolStripButtonSource.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonSource.Image")));
      this.m_ToolStripButtonSource.Name = "m_ToolStripButtonSource";
      this.m_ToolStripButtonSource.Size = new System.Drawing.Size(114, 25);
      this.m_ToolStripButtonSource.Text = "View Source";
      this.m_ToolStripButtonSource.Visible = false;
      // 
      // m_ToolStripButtonAsText
      // 
      this.m_ToolStripButtonAsText.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonAsText.Image")));
      this.m_ToolStripButtonAsText.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.m_ToolStripButtonAsText.Name = "m_ToolStripButtonAsText";
      this.m_ToolStripButtonAsText.Size = new System.Drawing.Size(60, 25);
      this.m_ToolStripButtonAsText.Text = "Text";
      this.m_ToolStripButtonAsText.Visible = false;
      // 
      // m_ToolStripButtonStore
      // 
      this.m_ToolStripButtonStore.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonStore.Image")));
      this.m_ToolStripButtonStore.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.m_ToolStripButtonStore.Name = "m_ToolStripButtonStore";
      this.m_ToolStripButtonStore.Size = new System.Drawing.Size(96, 25);
      this.m_ToolStripButtonStore.Text = "&Write File";
      this.m_ToolStripButtonStore.ToolTipText = "Store the currently displayed data as delimited text file";
      this.m_ToolStripButtonStore.Visible = false;
      this.m_ToolStripButtonStore.Click += new System.EventHandler(this.ToolStripButtonStore_Click);
      // 
      // m_ToolStripContainer
      // 
      // 
      // m_ToolStripContainer.BottomToolStripPanel
      // 
      this.m_ToolStripContainer.BottomToolStripPanel.Controls.Add(this.m_BindingNavigator);
      // 
      // m_ToolStripContainer.ContentPanel
      // 
      this.m_ToolStripContainer.ContentPanel.Controls.Add(this.m_Search);
      this.m_ToolStripContainer.ContentPanel.Controls.Add(this.m_FilteredDataGridView);
      this.m_ToolStripContainer.ContentPanel.Margin = new System.Windows.Forms.Padding(4);
      this.m_ToolStripContainer.ContentPanel.Size = new System.Drawing.Size(1162, 377);
      this.m_ToolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_ToolStripContainer.LeftToolStripPanelVisible = false;
      this.m_ToolStripContainer.Location = new System.Drawing.Point(0, 0);
      this.m_ToolStripContainer.Margin = new System.Windows.Forms.Padding(4);
      this.m_ToolStripContainer.Name = "m_ToolStripContainer";
      this.m_ToolStripContainer.RightToolStripPanelVisible = false;
      this.m_ToolStripContainer.Size = new System.Drawing.Size(1162, 432);
      this.m_ToolStripContainer.TabIndex = 13;
      this.m_ToolStripContainer.Text = "toolStripContainer";
      // 
      // m_ToolStripContainer.TopToolStripPanel
      // 
      this.m_ToolStripContainer.TopToolStripPanel.Controls.Add(this.m_ToolStripTop);
      // 
      // m_BindingNavigator
      // 
      this.m_BindingNavigator.AddNewItem = null;
      this.m_BindingNavigator.BindingSource = this.m_BindingSource;
      this.m_BindingNavigator.CountItem = this.m_ToolStripLabelCount;
      this.m_BindingNavigator.DeleteItem = null;
      this.m_BindingNavigator.Dock = System.Windows.Forms.DockStyle.None;
      this.m_BindingNavigator.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
      this.m_BindingNavigator.ImageScalingSize = new System.Drawing.Size(20, 20);
      this.m_BindingNavigator.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ToolStripButtonMoveFirstItem,
            this.m_ToolStripButtonMovePreviousItem,
            this.m_ToolStripTextBox1,
            this.m_ToolStripLabelCount,
            this.m_ToolStripButtonMoveNextItem,
            this.m_ToolStripButtonMoveLastItem});
      this.m_BindingNavigator.Location = new System.Drawing.Point(4, 0);
      this.m_BindingNavigator.MoveFirstItem = this.m_ToolStripButtonMoveFirstItem;
      this.m_BindingNavigator.MoveLastItem = this.m_ToolStripButtonMoveLastItem;
      this.m_BindingNavigator.MoveNextItem = this.m_ToolStripButtonMoveNextItem;
      this.m_BindingNavigator.MovePreviousItem = this.m_ToolStripButtonMovePreviousItem;
      this.m_BindingNavigator.Name = "m_BindingNavigator";
      this.m_BindingNavigator.PositionItem = this.m_ToolStripTextBox1;
      this.m_BindingNavigator.Size = new System.Drawing.Size(216, 27);
      this.m_BindingNavigator.TabIndex = 0;
      // 
      // m_ToolStripLabelCount
      // 
      this.m_ToolStripLabelCount.Name = "m_ToolStripLabelCount";
      this.m_ToolStripLabelCount.Size = new System.Drawing.Size(45, 24);
      this.m_ToolStripLabelCount.Text = "of {0}";
      this.m_ToolStripLabelCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.m_ToolStripLabelCount.ToolTipText = "Total number of items";
      // 
      // m_ToolStripButtonMoveFirstItem
      // 
      this.m_ToolStripButtonMoveFirstItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.m_ToolStripButtonMoveFirstItem.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonMoveFirstItem.Image")));
      this.m_ToolStripButtonMoveFirstItem.Name = "m_ToolStripButtonMoveFirstItem";
      this.m_ToolStripButtonMoveFirstItem.RightToLeftAutoMirrorImage = true;
      this.m_ToolStripButtonMoveFirstItem.Size = new System.Drawing.Size(29, 24);
      this.m_ToolStripButtonMoveFirstItem.Text = "Move first";
      // 
      // m_ToolStripButtonMovePreviousItem
      // 
      this.m_ToolStripButtonMovePreviousItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.m_ToolStripButtonMovePreviousItem.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonMovePreviousItem.Image")));
      this.m_ToolStripButtonMovePreviousItem.Name = "m_ToolStripButtonMovePreviousItem";
      this.m_ToolStripButtonMovePreviousItem.RightToLeftAutoMirrorImage = true;
      this.m_ToolStripButtonMovePreviousItem.Size = new System.Drawing.Size(29, 24);
      this.m_ToolStripButtonMovePreviousItem.Text = "Move previous";
      // 
      // m_ToolStripTextBox1
      // 
      this.m_ToolStripTextBox1.AccessibleName = "Position";
      this.m_ToolStripTextBox1.Font = new System.Drawing.Font("Segoe UI", 9F);
      this.m_ToolStripTextBox1.Name = "m_ToolStripTextBox1";
      this.m_ToolStripTextBox1.Size = new System.Drawing.Size(50, 27);
      this.m_ToolStripTextBox1.Text = "0";
      this.m_ToolStripTextBox1.ToolTipText = "Current position";
      // 
      // m_ToolStripButtonMoveNextItem
      // 
      this.m_ToolStripButtonMoveNextItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.m_ToolStripButtonMoveNextItem.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonMoveNextItem.Image")));
      this.m_ToolStripButtonMoveNextItem.Name = "m_ToolStripButtonMoveNextItem";
      this.m_ToolStripButtonMoveNextItem.RightToLeftAutoMirrorImage = true;
      this.m_ToolStripButtonMoveNextItem.Size = new System.Drawing.Size(29, 24);
      this.m_ToolStripButtonMoveNextItem.Text = "Move next";
      // 
      // m_ToolStripButtonMoveLastItem
      // 
      this.m_ToolStripButtonMoveLastItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.m_ToolStripButtonMoveLastItem.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonMoveLastItem.Image")));
      this.m_ToolStripButtonMoveLastItem.Name = "m_ToolStripButtonMoveLastItem";
      this.m_ToolStripButtonMoveLastItem.RightToLeftAutoMirrorImage = true;
      this.m_ToolStripButtonMoveLastItem.Size = new System.Drawing.Size(29, 24);
      this.m_ToolStripButtonMoveLastItem.Text = "Move last";
      // 
      // m_Search
      // 
      this.m_Search.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.m_Search.AutoSize = true;
      this.m_Search.BackColor = System.Drawing.SystemColors.Info;
      this.m_Search.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.m_Search.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.m_Search.Location = new System.Drawing.Point(722, 4);
      this.m_Search.Margin = new System.Windows.Forms.Padding(4);
      this.m_Search.Name = "m_Search";
      this.m_Search.Results = 0;
      this.m_Search.Size = new System.Drawing.Size(440, 41);
      this.m_Search.TabIndex = 1;
      this.m_Search.Visible = false;
      this.m_Search.OnResultChanged += new System.EventHandler<CsvTools.SearchEventArgs>(this.OnSearchResultChanged);
      this.m_Search.OnSearchChanged += new System.EventHandler<CsvTools.SearchEventArgs>(this.OnSearchChanged);
      this.m_Search.OnSearchClear += new System.EventHandler(this.ClearSearch);
      // 
      // m_FilteredDataGridView
      // 
      this.m_FilteredDataGridView.AllowUserToOrderColumns = true;
      dataGridViewCellStyle1.BackColor = System.Drawing.Color.Gainsboro;
      this.m_FilteredDataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
      this.m_FilteredDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      this.m_FilteredDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
      dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Black;
      dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.m_FilteredDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
      this.m_FilteredDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_FilteredDataGridView.Location = new System.Drawing.Point(0, 0);
      this.m_FilteredDataGridView.Margin = new System.Windows.Forms.Padding(4);
      this.m_FilteredDataGridView.Name = "m_FilteredDataGridView";
      this.m_FilteredDataGridView.RowHeadersWidth = 62;
      this.m_FilteredDataGridView.Size = new System.Drawing.Size(1162, 377);
      this.m_FilteredDataGridView.TabIndex = 1;
      this.m_FilteredDataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.FilteredDataGridView_CellFormatting);
      this.m_FilteredDataGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DetailControl_KeyDown);
      // 
      // DetailControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.m_ToolStripContainer);
      this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.Margin = new System.Windows.Forms.Padding(4);
      this.Name = "DetailControl";
      this.Size = new System.Drawing.Size(1162, 432);
      this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DetailControl_KeyDown);
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
      ((System.ComponentModel.ISupportInitialize)(this.m_BindingNavigator)).EndInit();
      this.m_BindingNavigator.ResumeLayout(false);
      this.m_BindingNavigator.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.m_BindingSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.m_FilteredDataGridView)).EndInit();
      this.ResumeLayout(false);

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
      m_FilteredDataGridView.SafeInvoke(
        () =>
          {
            try
            {
              m_FilteredDataGridView.CurrentCell = m_FoundCells[e.Result - 1];
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
        var visible = m_FilteredDataGridView.Columns.Cast<DataGridViewColumn>()
          .Where(col => col.Visible && !string.IsNullOrEmpty(col.DataPropertyName)).ToList();

        foreach (DataGridViewRow row in m_FilteredDataGridView.Rows)
        {
          if (!row.Visible)
            continue;
          foreach (var cell in visible.Select(col => row.Cells[col.Index])
            .Where(cell => !string.IsNullOrEmpty(cell.FormattedValue?.ToString())))
          {
            if (cell.FormattedValue != null)
              m_SearchCells.Add(new KeyValuePair<string, DataGridViewCell>(cell.FormattedValue.ToString(), cell));
          }
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
            m_FilteredDataGridView.InvalidateCell(args.Cell);
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
            m_ToolStripButtonHierachy.Visible = m_ShowButtons;
            m_ToolStripButtonStore.Visible = m_ShowButtons && m_FileSetting is CsvFile;
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
            if (type == FilterType.All & m_ToolStripComboBoxFilterType.SelectedIndex != 0)
              m_ToolStripComboBoxFilterType.SelectedIndex = 0;
            if (type == FilterType.ErrorsAndWarning & m_ToolStripComboBoxFilterType.SelectedIndex != 1)
              m_ToolStripComboBoxFilterType.SelectedIndex = 1;
            if (type == FilterType.ShowErrors & m_ToolStripComboBoxFilterType.SelectedIndex != 2)
              m_ToolStripComboBoxFilterType.SelectedIndex = 2;
            if (type == FilterType.ShowWarning & m_ToolStripComboBoxFilterType.SelectedIndex != 3)
              m_ToolStripComboBoxFilterType.SelectedIndex = 3;
            if (type == FilterType.ShowIssueFree & m_ToolStripComboBoxFilterType.SelectedIndex != 4)
              m_ToolStripComboBoxFilterType.SelectedIndex = 4;
          });
      var oldSortedColumn = m_FilteredDataGridView.SortedColumn?.DataPropertyName;
      var oldOrder = m_FilteredDataGridView.SortOrder;
      FilterRowsAndColumns(type);

      // bindingSource.ResumeBinding();
      m_FilteredDataGridView.ColumnVisibilityChanged();
      m_FilteredDataGridView.SetRowHeight();

      if (oldOrder != SortOrder.None && !string.IsNullOrEmpty(oldSortedColumn))
      {
        Sort(
          oldSortedColumn,
          oldOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
      }
    }

    private void StartSearch(object sender, SearchEventArgs e)
    {
      ClearSearch(this, null);
      m_FilteredDataGridView.HighlightText = e.SearchText;

      var processInformaton = new ProcessInformaton
      {
        SearchText = e.SearchText,
        CancellationTokenSource =
                                    CancellationTokenSource.CreateLinkedTokenSource(m_CancellationTokenSource.Token)
      };

      processInformaton.FoundResultEvent += ResultFound;
      processInformaton.SearchCompleteEvent += SearchComplete;
      processInformaton.SearchEventArgs = e;
      m_CurrentSearch = processInformaton;
      ThreadPool.QueueUserWorkItem(BackgoundSearchThread, processInformaton);
    }

    private void ToolStripButtonStore_Click(object sender, EventArgs e)
    {
      if (m_FileSetting is IFileSettingPhysicalFile settingPhysicalFile)
      {
        var split = FileSystemUtils.SplitPath(settingPhysicalFile.FullPath);

        var fileName = WindowsAPICodePackWrapper.Save(
          split.DirectoryName,
          "Delimited File",
          "Text file (*.txt)|*.txt|Comma delimited (*.csv)|*.csv|Tab delimited (*.tab;*.tsv)|*.tab;*.tsv|All files (*.*)|*.*",
          split.Extension,
          split.FileName);
        if (string.IsNullOrEmpty(fileName))
          return;

        var writeFile = m_FileSetting.Clone() as IFileSettingPhysicalFile;
        if (writeFile != null)
        {
          writeFile.FileName = fileName;

          using (var processDisplay = writeFile.GetProcessDisplay(ParentForm, true, m_CancellationTokenSource.Token))
          {
            var writer = FunctionalDI.GetFileWriter(writeFile, TimeZoneInfo.Local.Id, processDisplay);

            // Restrict to shown data
            var colNames = new Dictionary<int, string>();
            foreach (DataGridViewColumn col in m_FilteredDataGridView.Columns)
            {
              if (col.Visible && !BaseFileReader.ArtificialFields.Contains(col.DataPropertyName))
                colNames.Add(col.DisplayIndex, col.DataPropertyName);
            }

            // can not use filteredDataGridView.Columns directly
            writer.WriteDataTable(
              DataGridView.DataView.ToTable(false, colNames.OrderBy(x => x.Key).Select(x => x.Value).ToArray()));
          }
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

    private class ProcessInformaton : IDisposable
    {
      public CancellationTokenSource CancellationTokenSource;

      public int Found;

      public EventHandler<FoundEventArgs> FoundResultEvent;

      public bool IsRunning;

      public EventHandler<SearchEventArgs> SearchCompleteEvent;

      public SearchEventArgs SearchEventArgs;

      public string SearchText;

      public void Cancel() => CancellationTokenSource?.Cancel();

      public void Dispose() => CancellationTokenSource?.Dispose();
    }
  }
}