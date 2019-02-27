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

    // private EventHandler m_BatchSizeChangedEvent;
    private BindingNavigator m_BindingNavigator;

    private BindingSource m_BindingSource;
    private CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();
    private DataColumnCollection m_Columns;

    private ProcessInformaton m_CurrentSearch;

    private DataTable m_DataTable;
    private IFileSetting m_FileSetting;
    private FilterDataTable m_FilterDataTable;

    /// <summary>
    ///   Icon to display
    /// </summary>
    private FilteredDataGridView m_FilteredDataGridView;

    private bool m_HasButtonAsText;
    private bool m_HasButtonShowSource;

    private FormHierachyDisplay m_HierachyDisplay;

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

    private ToolStripContainer m_ToolStripContainer;

    private ToolStripLabel m_ToolStripLabelCount;

    private ToolStripTextBox m_ToolStripTextBox1;

    //private ToolStripTextBox m_ToolStripTextBoxRecSize;
    private ToolStrip m_ToolStripTop;

    private ToolStripComboBox toolStripComboBoxFilterType;

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
#pragma warning restore CA1030 // Use events where appropriate
      {
        m_ToolStripButtonSettings.Click += value;
        m_ShowSettingsButtons = true;
        SetButtonVisibility();
      }
#pragma warning disable CA1030 // Use events where appropriate
      remove
#pragma warning restore CA1030 // Use events where appropriate
      {
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
        m_FilterDataTable.StartFilter(int.MaxValue, FilterType.ErrorsAndWarning, () =>
        {
          if (m_FilterDataTable.FilterTable.Rows.Count == 0)
          {
            ShowFilter = false;
          }
        });
        m_FilteredDataGridView.FileSetting = m_FileSetting;
        toolStripComboBoxFilterType.SelectedIndex = 0;

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
        if (m_FilteredDataGridView.ReadOnly == value) return;
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
        if (m_ShowFilter == value) return;
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
        if (m_ShowButtons == value) return;
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
        if (value.IsEmpty() && m_FilterDataTable == null)
          return;

        // need to wait until m_FilterDataTable is set in the background
        var start = DateTime.Now;
        // wait for it to be set by background process
        while (m_FilterDataTable.Filtering && !m_CancellationTokenSource.IsCancellationRequested && (DateTime.Now - start).TotalSeconds < 5d)
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

    public CancellationToken CancellationToken
    {
      get => m_CancellationTokenSource.Token;
      set => m_CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(value);
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
        m_ToolStripTop.Items.Remove(toolStripComboBoxFilterType);
        m_BindingNavigator.Items.Add(toolStripComboBoxFilterType);
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
        m_BindingNavigator.Items.Remove(toolStripComboBoxFilterType);
        m_ToolStripTop.Items.Add(toolStripComboBoxFilterType);
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
      //toolStripContainer.TopToolStripPanel.Visible = !ApplicationSetting.MenuDown;
    }

    /// <summary>
    ///   Called when [show errors].
    /// </summary>
    public void OnlyShowErrors() => toolStripComboBoxFilterType.SelectedIndex = 1;

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
          // col.HeaderCell.SortGlyphDirection = direction == ListSortDirection.Ascending ? SortOrder.Ascending : SortOrder.Descending;
        }
      }
    }

    /// <summary>
    ///   Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (!disposing) return;
      m_CancellationTokenSource.Cancel();
      m_CurrentSearch?.Dispose();
      m_DataTable?.Dispose();

      m_FilterDataTable?.Dispose();

      m_HierachyDisplay?.Dispose();
      m_CancellationTokenSource.Dispose();
      m_DataTable?.Dispose();
      base.Dispose(disposing);
    }

    /// <summary>
    /// </summary>
    /// <param name="e">An <see cref="EventArgs" /> that contains the event data.</param>
    protected override void OnParentChanged(EventArgs e)
    {
      base.OnParentChanged(e);

      if (m_ParentForm != null) m_ParentForm.Closing -= ParentForm_Closing;
      m_ParentForm = FindForm();

      if (m_ParentForm != null)
        m_ParentForm.Closing += ParentForm_Closing;
    }

    private void AutoResizeColumns(DataTable source)
    {
      if (source.Rows.Count < 10000 && source.Columns.Count < 50)
      {
        m_FilteredDataGridView.AutoResizeColumns(source.Rows.Count < 1000 && source.Columns.Count < 20
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
        if (processInformation.SearchText.Length < 2 && m_SearchCells.Count() > 10000)
          return;

        foreach (var cell in m_SearchCells)
        {
          Contract.Assume(cell.Key != null);
          Contract.Assume(cell.Value != null);

          if (processInformation.CancellationTokenSource?.IsCancellationRequested ?? false)
            return;

          if (cell.Key.IndexOf(processInformation.SearchText, StringComparison.OrdinalIgnoreCase) <= -1) continue;
          processInformation.FoundResultEvent?.Invoke(this,
            new FoundEventArgs(processInformation.Found, cell.Value));
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
        using (var details =
          new FormShowMaxLength(m_DataTable, m_DataTable.Select(m_FilteredDataGridView.CurrentFilter)))
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
        if (m_FilteredDataGridView.Columns.Count <= 0) return;
        var columnName = m_FilteredDataGridView.CurrentCell != null
          ? m_FilteredDataGridView.Columns[m_FilteredDataGridView.CurrentCell.ColumnIndex].Name
          : m_FilteredDataGridView.Columns[0].Name;

        using (var details = new FormDuplicatesDisplay(m_DataTable.Clone(),
          m_DataTable.Select(m_FilteredDataGridView.CurrentFilter), columnName))
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
        m_HierachyDisplay?.Close();
        m_HierachyDisplay =
          new FormHierachyDisplay(m_DataTable.Clone(), m_DataTable.Select(m_FilteredDataGridView.CurrentFilter))
          {
            Icon = ParentForm?.Icon
          };
        m_HierachyDisplay.Show();
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
        if (m_FilteredDataGridView.Columns.Count <= 0) return;
        var columnName = m_FilteredDataGridView.CurrentCell != null
          ? m_FilteredDataGridView.Columns[m_FilteredDataGridView.CurrentCell.ColumnIndex].Name
          : m_FilteredDataGridView.Columns[0].Name;
        using (var details = new FormUniqueDisplay(m_DataTable.Clone(),
          m_DataTable.Select(m_FilteredDataGridView.CurrentFilter), columnName))
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
      this.SafeInvoke(() =>
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
      if (!m_Search.Visible) return;
      if (m_CurrentSearch != null && m_CurrentSearch.IsRunning)
        m_CurrentSearch.Cancel();
      m_Search.Results = 0;
      m_Search.Hide();
      ClearSearch(sender, args);
    }

    private void DetailControl_KeyDown(object sender, KeyEventArgs e)
    {
      if (!e.Control || e.KeyCode != Keys.F) return;
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

      if (m_FilterDataTable.FilterTable.Rows.Count <= 0) return;
      if (m_FilterDataTable.ColumnsWithoutErrors.Count == m_Columns.Count) return;
      foreach (DataGridViewColumn dgcol in m_FilteredDataGridView.Columns)
      {
        if (dgcol.Visible && m_FilterDataTable.ColumnsWithoutErrors.Contains(dgcol.DataPropertyName))
        {
          dgcol.Visible = false;
          m_SearchCellsDirty = true;
        }
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
    /// <param name="onlyErrors">if set to <c>true</c> only rows with errors and columns with errors are shown.</param>
    private void FilterRowsAndColumns(FilterType type)
    {
      try
      {
        Extensions.ProcessUIElements();

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
        if (newDt == m_BindingSource.DataSource) return;
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
      finally
      {
        Extensions.ProcessUIElements();
      }
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
      if (e.Result <= 0 || e.Result >= m_FoundCells.Count) return;
      m_FilteredDataGridView.SafeInvoke(() =>
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
    /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs" /> instance containing the event data.</param>
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
        //m_SearchCells = from r in filteredDataGridView.Rows.Cast<DataGridViewRow>()
        //                from c in r.Cells.Cast<DataGridViewCell>()
        //                where c.Visible && !string.IsNullOrEmpty(c.FormattedValue.ToString())
        //                select new KeyValuePair<string, DataGridViewCell>(c.FormattedValue.ToString(), c);

        m_SearchCells.Clear();
        var visible = new List<DataGridViewColumn>();
        foreach (DataGridViewColumn col in m_FilteredDataGridView.Columns)
        {
          if (col.Visible && !string.IsNullOrEmpty(col.DataPropertyName))
            visible.Add(col);
        }

        foreach (DataGridViewRow row in m_FilteredDataGridView.Rows)
        {
          if (!row.Visible)
            continue;
          foreach (var col in visible)
          {
            var cell = row.Cells[col.Index];
            if (!string.IsNullOrEmpty(cell.FormattedValue?.ToString()))
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
      this.SafeBeginInvoke(() =>
      {
        m_Search.Results = args.Index;
        m_FilteredDataGridView.InvalidateCell(args.Cell);
      });
    }

    private void SearchComplete(object sender, SearchEventArgs e) => this.SafeBeginInvoke(() => { m_Search.Results = m_CurrentSearch.Found; });

    private void SetButtonVisibility()
    {
      this.SafeInvoke(() =>
      {
        // Need to set the control containing the buttons to visible
        //
        // Regular
        m_ToolStripButtonColumnLength.Visible = m_ShowButtons;
        m_ToolStripButtonDuplicates.Visible = m_ShowButtons;
        m_ToolStripButtonUniqueValues.Visible = m_ShowButtons;
        m_ToolStripButtonAsText.Visible = m_ShowButtons && m_HasButtonAsText;
        // Filter
        toolStripComboBoxFilterType.Visible = m_ShowButtons && m_ShowFilter;
        // Extended
        m_ToolStripButtonHierachy.Visible = m_ShowButtons;
        m_ToolStripButtonStore.Visible =
          m_ShowButtons && m_FileSetting is CsvFile;
        m_ToolStripButtonSource.Visible = m_ShowButtons && m_HasButtonShowSource;

        // Settings
        m_ToolStripButtonSettings.Visible = m_ShowButtons && m_ShowSettingsButtons;

        m_ToolStripTop.Visible = m_ShowButtons;
      });
    }

    /// <summary>
    ///   Sets the data source.
    /// </summary>
    private void SetDataSource(FilterType type)
    {
      if (m_DataTable == null)
        return;

      // update the dropdown
      this.SafeInvoke(() =>
      {
        if (type == FilterType.All & toolStripComboBoxFilterType.SelectedIndex != 0)
          toolStripComboBoxFilterType.SelectedIndex = 0;
        if (type == FilterType.ErrorsAndWarning & toolStripComboBoxFilterType.SelectedIndex != 1)
          toolStripComboBoxFilterType.SelectedIndex = 1;
        if (type == FilterType.ShowErrors & toolStripComboBoxFilterType.SelectedIndex != 2)
          toolStripComboBoxFilterType.SelectedIndex = 2;
        if (type == FilterType.ShowWarning & toolStripComboBoxFilterType.SelectedIndex != 3)
          toolStripComboBoxFilterType.SelectedIndex = 3;
        if (type == FilterType.ShowIssueFree & toolStripComboBoxFilterType.SelectedIndex != 4)
          toolStripComboBoxFilterType.SelectedIndex = 4;
      });

      var oldSortedColumn = m_FilteredDataGridView.SortedColumn?.DataPropertyName;
      var oldOrder = m_FilteredDataGridView.SortOrder;
      // bindingSource.SuspendBinding();
      FilterRowsAndColumns(type);
      // bindingSource.ResumeBinding();
      m_FilteredDataGridView.ColumnVisibilityChanged();
      m_FilteredDataGridView.SetRowHeight();

      if (oldOrder != SortOrder.None && !string.IsNullOrEmpty(oldSortedColumn))
      {
        Sort(oldSortedColumn,
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
        CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(m_CancellationTokenSource.Token)
      };

      processInformaton.FoundResultEvent += ResultFound;
      processInformaton.SearchCompleteEvent += SearchComplete;
      processInformaton.SearchEventArgs = e;
      m_CurrentSearch = processInformaton;
      ThreadPool.QueueUserWorkItem(BackgoundSearchThread, processInformaton);
    }

    private void ToolStripButtonStore_Click(object sender, EventArgs e)
    {
      var fi = FileSystemUtils.FileInfo(m_FileSetting.FileName);

      using (var saveFileDialog = new SaveFileDialog())
      {
        saveFileDialog.DefaultExt = "*" + fi.Extension;
        saveFileDialog.Filter =
          "Text file (*.txt)|*.txt|Comma delimited (*.csv)|*.csv|Tab delimited (*.tab;*.tsv)|*.tab;*.tsv|All files (*.*)|*.*";
        saveFileDialog.OverwritePrompt = true;
        saveFileDialog.Title = "Delimited File";
        saveFileDialog.InitialDirectory = FileSystemUtils.GetDirectoryName(fi.FullName).RemovePrefix();

        if (saveFileDialog.ShowDialog() != DialogResult.OK) return;
        var writeFile = m_FileSetting.Clone();
        writeFile.FileName = saveFileDialog.FileName.LongFileName();
        using (var processDisplay = writeFile.GetProcessDisplay(ParentForm, true, m_CancellationTokenSource.Token))
        {
          var writer = writeFile.GetFileWriter(processDisplay.CancellationToken);
          writer.Progress += processDisplay.SetProcess;
          // Restrict to shown data
          var colNames = new Dictionary<int, string>();
          foreach (DataGridViewColumn col in m_FilteredDataGridView.Columns)
          {
            if (col.Visible && !BaseFileReader.ArtificalFields.Contains(col.DataPropertyName))
              colNames.Add(col.DisplayIndex, col.DataPropertyName);
          }
          // can not use filteredDataGridView.Columns directly
          writer.WriteDataTable(m_FilteredDataGridView.DataView.ToTable(false,
            colNames.OrderBy(x => x.Key).Select(x => x.Value).ToArray()));
        }
      }
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///   Required method for Designer support - do not modify the contents of this method with the
    ///   code editor.
    /// </summary>
    private void InitializeComponent()
    {
      var dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      var dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      var dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
      var dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
      m_ToolStripTop = new System.Windows.Forms.ToolStrip();
      m_ToolStripButtonSettings = new System.Windows.Forms.ToolStripButton();
      toolStripComboBoxFilterType = new System.Windows.Forms.ToolStripComboBox();
      m_ToolStripButtonUniqueValues = new System.Windows.Forms.ToolStripButton();
      m_ToolStripButtonColumnLength = new System.Windows.Forms.ToolStripButton();
      m_ToolStripButtonDuplicates = new System.Windows.Forms.ToolStripButton();
      m_ToolStripButtonHierachy = new System.Windows.Forms.ToolStripButton();
      m_ToolStripButtonSource = new System.Windows.Forms.ToolStripButton();
      m_ToolStripButtonAsText = new System.Windows.Forms.ToolStripButton();
      m_ToolStripButtonStore = new System.Windows.Forms.ToolStripButton();
      m_ToolStripContainer = new System.Windows.Forms.ToolStripContainer();
      m_BindingNavigator = new System.Windows.Forms.BindingNavigator();
      m_BindingSource = new System.Windows.Forms.BindingSource();
      m_ToolStripLabelCount = new System.Windows.Forms.ToolStripLabel();
      m_ToolStripButtonMoveFirstItem = new System.Windows.Forms.ToolStripButton();
      m_ToolStripButtonMovePreviousItem = new System.Windows.Forms.ToolStripButton();
      m_ToolStripTextBox1 = new System.Windows.Forms.ToolStripTextBox();
      m_ToolStripButtonMoveNextItem = new System.Windows.Forms.ToolStripButton();
      m_ToolStripButtonMoveLastItem = new System.Windows.Forms.ToolStripButton();
      m_Search = new CsvTools.Search();
      m_FilteredDataGridView = new CsvTools.FilteredDataGridView();
      m_ToolStripTop.SuspendLayout();
      m_ToolStripContainer.BottomToolStripPanel.SuspendLayout();
      m_ToolStripContainer.ContentPanel.SuspendLayout();
      m_ToolStripContainer.TopToolStripPanel.SuspendLayout();
      m_ToolStripContainer.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(m_BindingNavigator)).BeginInit();
      m_BindingNavigator.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(m_BindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(m_FilteredDataGridView)).BeginInit();
      SuspendLayout();
      //
      // m_ToolStripTop
      //
      m_ToolStripTop.Dock = System.Windows.Forms.DockStyle.None;
      m_ToolStripTop.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
      m_ToolStripTop.ImageScalingSize = new System.Drawing.Size(20, 20);
      m_ToolStripTop.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            m_ToolStripButtonSettings,
            toolStripComboBoxFilterType,
            m_ToolStripButtonUniqueValues,
            m_ToolStripButtonColumnLength,
            m_ToolStripButtonDuplicates,
            m_ToolStripButtonHierachy,
            m_ToolStripButtonSource,
            m_ToolStripButtonAsText,
            m_ToolStripButtonStore});
      m_ToolStripTop.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
      m_ToolStripTop.Location = new System.Drawing.Point(3, 0);
      m_ToolStripTop.Name = "m_ToolStripTop";
      m_ToolStripTop.Size = new System.Drawing.Size(487, 27);
      m_ToolStripTop.TabIndex = 1;
      m_ToolStripTop.Text = "toolStripTop";
      //
      // m_ToolStripButtonSettings
      //
      m_ToolStripButtonSettings.Image = global::CsvToolLib.Resources.Settings;
      m_ToolStripButtonSettings.Name = "m_ToolStripButtonSettings";
      m_ToolStripButtonSettings.Size = new System.Drawing.Size(70, 24);
      m_ToolStripButtonSettings.Text = "Settings";
      m_ToolStripButtonSettings.ToolTipText = "Show CSV Settings";
      m_ToolStripButtonSettings.Visible = false;
      //
      // toolStripComboBoxFilterType
      //
      toolStripComboBoxFilterType.DropDownHeight = 90;
      toolStripComboBoxFilterType.DropDownWidth = 130;
      toolStripComboBoxFilterType.IntegralHeight = false;
      toolStripComboBoxFilterType.Items.AddRange(new object[] {
            "All Records",
            "Error or Warning",
            "Only Errors",
            "Only Warning",
            "No Error or Warning"});
      toolStripComboBoxFilterType.Name = "toolStripComboBoxFilterType";
      toolStripComboBoxFilterType.Size = new System.Drawing.Size(125, 27);
      toolStripComboBoxFilterType.SelectedIndexChanged += new System.EventHandler(toolStripComboBoxFilterType_SelectedIndexChanged);
      //
      // m_ToolStripButtonUniqueValues
      //
      m_ToolStripButtonUniqueValues.Image = global::CsvToolLib.Resources.Values;
      m_ToolStripButtonUniqueValues.Name = "m_ToolStripButtonUniqueValues";
      m_ToolStripButtonUniqueValues.Size = new System.Drawing.Size(98, 24);
      m_ToolStripButtonUniqueValues.Text = "Unique Values";
      m_ToolStripButtonUniqueValues.ToolTipText = "Display Unique Values";
      m_ToolStripButtonUniqueValues.Click += new System.EventHandler(ButtonUniqueValues_Click);
      //
      // m_ToolStripButtonColumnLength
      //
      m_ToolStripButtonColumnLength.Image = global::CsvToolLib.Resources.Shema;
      m_ToolStripButtonColumnLength.Name = "m_ToolStripButtonColumnLength";
      m_ToolStripButtonColumnLength.Size = new System.Drawing.Size(102, 24);
      m_ToolStripButtonColumnLength.Text = "Column Length";
      m_ToolStripButtonColumnLength.ToolTipText = "Display Schema information including Length";
      m_ToolStripButtonColumnLength.Click += new System.EventHandler(ButtonColumnLength_Click);
      //
      // m_ToolStripButtonDuplicates
      //
      m_ToolStripButtonDuplicates.Image = global::CsvToolLib.Resources.Duplicates;
      m_ToolStripButtonDuplicates.Name = "m_ToolStripButtonDuplicates";
      m_ToolStripButtonDuplicates.Size = new System.Drawing.Size(80, 24);
      m_ToolStripButtonDuplicates.Text = "Duplicates";
      m_ToolStripButtonDuplicates.ToolTipText = "Display Duplicate Values";
      m_ToolStripButtonDuplicates.Click += new System.EventHandler(ButtonDuplicates_Click);
      //
      // m_ToolStripButtonHierachy
      //
      m_ToolStripButtonHierachy.Image = global::CsvToolLib.Resources.Hierarchy;
      m_ToolStripButtonHierachy.Name = "m_ToolStripButtonHierachy";
      m_ToolStripButtonHierachy.Size = new System.Drawing.Size(77, 24);
      m_ToolStripButtonHierachy.Text = "Hierarchy";
      m_ToolStripButtonHierachy.ToolTipText = "Display a Hierarchy Structure";
      m_ToolStripButtonHierachy.Click += new System.EventHandler(ButtonHierachy_Click);
      //
      // m_ToolStripButtonSource
      //
      m_ToolStripButtonSource.Image = global::CsvToolLib.Resources.View;
      m_ToolStripButtonSource.Name = "m_ToolStripButtonSource";
      m_ToolStripButtonSource.Size = new System.Drawing.Size(89, 24);
      m_ToolStripButtonSource.Text = "View Source";
      m_ToolStripButtonSource.Visible = false;
      //
      // m_ToolStripButtonAsText
      //
      m_ToolStripButtonAsText.Image = global::CsvToolLib.Resources.text;
      m_ToolStripButtonAsText.ImageTransparentColor = System.Drawing.Color.Magenta;
      m_ToolStripButtonAsText.Name = "m_ToolStripButtonAsText";
      m_ToolStripButtonAsText.Size = new System.Drawing.Size(53, 24);
      m_ToolStripButtonAsText.Text = "Text";
      m_ToolStripButtonAsText.Visible = false;
      //
      // m_ToolStripButtonStore
      //
      m_ToolStripButtonStore.Image = global::CsvToolLib.Resources.Save;
      m_ToolStripButtonStore.ImageTransparentColor = System.Drawing.Color.Magenta;
      m_ToolStripButtonStore.Name = "m_ToolStripButtonStore";
      m_ToolStripButtonStore.Size = new System.Drawing.Size(76, 24);
      m_ToolStripButtonStore.Text = "&Write File";
      m_ToolStripButtonStore.ToolTipText = "Store the currently displayed data as delimited text file";
      m_ToolStripButtonStore.Visible = false;
      m_ToolStripButtonStore.Click += new System.EventHandler(ToolStripButtonStore_Click);
      //
      // m_ToolStripContainer
      //
      //
      // m_ToolStripContainer.BottomToolStripPanel
      //
      m_ToolStripContainer.BottomToolStripPanel.Controls.Add(m_BindingNavigator);
      //
      // m_ToolStripContainer.ContentPanel
      //
      m_ToolStripContainer.ContentPanel.Controls.Add(m_Search);
      m_ToolStripContainer.ContentPanel.Controls.Add(m_FilteredDataGridView);
      m_ToolStripContainer.ContentPanel.Size = new System.Drawing.Size(872, 297);
      m_ToolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
      m_ToolStripContainer.LeftToolStripPanelVisible = false;
      m_ToolStripContainer.Location = new System.Drawing.Point(0, 0);
      m_ToolStripContainer.Name = "m_ToolStripContainer";
      m_ToolStripContainer.RightToolStripPanelVisible = false;
      m_ToolStripContainer.Size = new System.Drawing.Size(872, 351);
      m_ToolStripContainer.TabIndex = 13;
      m_ToolStripContainer.Text = "toolStripContainer";
      //
      // m_ToolStripContainer.TopToolStripPanel
      //
      m_ToolStripContainer.TopToolStripPanel.Controls.Add(m_ToolStripTop);
      //
      // m_BindingNavigator
      //
      m_BindingNavigator.AddNewItem = null;
      m_BindingNavigator.BindingSource = m_BindingSource;
      m_BindingNavigator.CountItem = m_ToolStripLabelCount;
      m_BindingNavigator.DeleteItem = null;
      m_BindingNavigator.Dock = System.Windows.Forms.DockStyle.None;
      m_BindingNavigator.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
      m_BindingNavigator.ImageScalingSize = new System.Drawing.Size(20, 20);
      m_BindingNavigator.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            m_ToolStripButtonMoveFirstItem,
            m_ToolStripButtonMovePreviousItem,
            m_ToolStripTextBox1,
            m_ToolStripLabelCount,
            m_ToolStripButtonMoveNextItem,
            m_ToolStripButtonMoveLastItem});
      m_BindingNavigator.Location = new System.Drawing.Point(3, 0);
      m_BindingNavigator.MoveFirstItem = m_ToolStripButtonMoveFirstItem;
      m_BindingNavigator.MoveLastItem = m_ToolStripButtonMoveLastItem;
      m_BindingNavigator.MoveNextItem = m_ToolStripButtonMoveNextItem;
      m_BindingNavigator.MovePreviousItem = m_ToolStripButtonMovePreviousItem;
      m_BindingNavigator.Name = "m_BindingNavigator";
      m_BindingNavigator.PositionItem = m_ToolStripTextBox1;
      m_BindingNavigator.Size = new System.Drawing.Size(187, 27);
      m_BindingNavigator.TabIndex = 0;
      //
      // m_ToolStripLabelCount
      //
      m_ToolStripLabelCount.Name = "m_ToolStripLabelCount";
      m_ToolStripLabelCount.Size = new System.Drawing.Size(36, 24);
      m_ToolStripLabelCount.Text = "of {0}";
      m_ToolStripLabelCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      m_ToolStripLabelCount.ToolTipText = "Total number of items";
      //
      // m_ToolStripButtonMoveFirstItem
      //
      m_ToolStripButtonMoveFirstItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      m_ToolStripButtonMoveFirstItem.Image = global::CsvToolLib.Resources.MoveFirstItem;
      m_ToolStripButtonMoveFirstItem.Name = "m_ToolStripButtonMoveFirstItem";
      m_ToolStripButtonMoveFirstItem.RightToLeftAutoMirrorImage = true;
      m_ToolStripButtonMoveFirstItem.Size = new System.Drawing.Size(24, 24);
      m_ToolStripButtonMoveFirstItem.Text = "Move first";
      //
      // m_ToolStripButtonMovePreviousItem
      //
      m_ToolStripButtonMovePreviousItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      m_ToolStripButtonMovePreviousItem.Image = global::CsvToolLib.Resources.MovePreviousItem;
      m_ToolStripButtonMovePreviousItem.Name = "m_ToolStripButtonMovePreviousItem";
      m_ToolStripButtonMovePreviousItem.RightToLeftAutoMirrorImage = true;
      m_ToolStripButtonMovePreviousItem.Size = new System.Drawing.Size(24, 24);
      m_ToolStripButtonMovePreviousItem.Text = "Move previous";
      //
      // m_ToolStripTextBox1
      //
      m_ToolStripTextBox1.AccessibleName = "Position";
      m_ToolStripTextBox1.Name = "m_ToolStripTextBox1";
      m_ToolStripTextBox1.Size = new System.Drawing.Size(50, 27);
      m_ToolStripTextBox1.Text = "0";
      m_ToolStripTextBox1.ToolTipText = "Current position";
      //
      // m_ToolStripButtonMoveNextItem
      //
      m_ToolStripButtonMoveNextItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      m_ToolStripButtonMoveNextItem.Image = global::CsvToolLib.Resources.MoveNextItem;
      m_ToolStripButtonMoveNextItem.Name = "m_ToolStripButtonMoveNextItem";
      m_ToolStripButtonMoveNextItem.RightToLeftAutoMirrorImage = true;
      m_ToolStripButtonMoveNextItem.Size = new System.Drawing.Size(24, 24);
      m_ToolStripButtonMoveNextItem.Text = "Move next";
      //
      // m_ToolStripButtonMoveLastItem
      //
      m_ToolStripButtonMoveLastItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      m_ToolStripButtonMoveLastItem.Image = global::CsvToolLib.Resources.MoveLastItem;
      m_ToolStripButtonMoveLastItem.Name = "m_ToolStripButtonMoveLastItem";
      m_ToolStripButtonMoveLastItem.RightToLeftAutoMirrorImage = true;
      m_ToolStripButtonMoveLastItem.Size = new System.Drawing.Size(24, 24);
      m_ToolStripButtonMoveLastItem.Text = "Move last";
      //
      // m_Search
      //
      m_Search.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
      m_Search.AutoSize = true;
      m_Search.BackColor = System.Drawing.SystemColors.Info;
      m_Search.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      m_Search.Location = new System.Drawing.Point(542, 3);
      m_Search.Name = "m_Search";
      m_Search.Results = 0;
      m_Search.Size = new System.Drawing.Size(330, 34);
      m_Search.TabIndex = 1;
      m_Search.Visible = false;
      m_Search.OnResultChanged += new System.EventHandler<CsvTools.SearchEventArgs>(OnSearchResultChanged);
      m_Search.OnSearchChanged += new System.EventHandler<CsvTools.SearchEventArgs>(OnSearchChanged);
      m_Search.OnSearchClear += new System.EventHandler(ClearSearch);
      //
      // m_FilteredDataGridView
      //
      m_FilteredDataGridView.AllowUserToOrderColumns = true;
      dataGridViewCellStyle1.BackColor = System.Drawing.Color.Gainsboro;
      m_FilteredDataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
      m_FilteredDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
      dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      m_FilteredDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
      m_FilteredDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
      dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
      dataGridViewCellStyle3.ForeColor = System.Drawing.Color.Black;
      dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      m_FilteredDataGridView.DefaultCellStyle = dataGridViewCellStyle3;
      m_FilteredDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      m_FilteredDataGridView.Location = new System.Drawing.Point(0, 0);
      m_FilteredDataGridView.Name = "m_FilteredDataGridView";
      dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
      dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      m_FilteredDataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
      m_FilteredDataGridView.Size = new System.Drawing.Size(872, 297);
      m_FilteredDataGridView.TabIndex = 1;
      m_FilteredDataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(FilteredDataGridView_CellFormatting);
      m_FilteredDataGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(DetailControl_KeyDown);
      //
      // DetailControl
      //
      AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      Controls.Add(m_ToolStripContainer);
      Name = "DetailControl";
      Size = new System.Drawing.Size(872, 351);
      KeyDown += new System.Windows.Forms.KeyEventHandler(DetailControl_KeyDown);
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
      ((System.ComponentModel.ISupportInitialize)(m_BindingNavigator)).EndInit();
      m_BindingNavigator.ResumeLayout(false);
      m_BindingNavigator.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(m_BindingSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(m_FilteredDataGridView)).EndInit();
      ResumeLayout(false);
    }

    #endregion Windows Form Designer generated code

    private void toolStripComboBoxFilterType_SelectedIndexChanged(object sender, EventArgs e)
    {
      /*
       * All Records
       * Error or Warning
       * Only Errors
       * Only Warning
       * No Error or Warning
      */
      if (toolStripComboBoxFilterType.SelectedIndex == 0)
        SetDataSource(FilterType.All);
      if (toolStripComboBoxFilterType.SelectedIndex == 1)
        SetDataSource(FilterType.ErrorsAndWarning);
      if (toolStripComboBoxFilterType.SelectedIndex == 2)
        SetDataSource(FilterType.ShowErrors);
      if (toolStripComboBoxFilterType.SelectedIndex == 3)
        SetDataSource(FilterType.ShowWarning);
      if (toolStripComboBoxFilterType.SelectedIndex == 4)
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