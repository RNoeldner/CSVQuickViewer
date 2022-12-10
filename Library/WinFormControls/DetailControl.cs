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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace CsvTools
{
  /// <inheritdoc cref="UserControl" />
  /// <summary>
  ///   Windows from to show detail information for a dataTable
  /// </summary>
  public sealed partial class DetailControl : UserControl
  {
    private readonly List<DataGridViewCell> m_FoundCells = new();
    private readonly List<KeyValuePair<string, DataGridViewCell>> m_SearchCells = new();
    private readonly List<ToolStripItem> m_ToolStripItems = new();
    private CancellationToken m_CancellationToken = CancellationToken.None;
    private ProcessInformation? m_CurrentSearchProcessInformation;
    private DataTable m_DataTable = new();
    private bool m_DisposedValue; // To detect redundant calls
    private FilterDataTable? m_FilterDataTable;
    private FormDuplicatesDisplay? m_FormDuplicatesDisplay;
    private FormShowMaxLength? m_FormShowMaxLength;
    private FormUniqueDisplay? m_FormUniqueDisplay;
    private FormHierarchyDisplay? m_HierarchyDisplay;
    private bool m_MenuDown;
    private bool m_SearchCellsDirty = true;
    private bool m_ShowButtons = true;
    private bool m_ShowFilter = true;
    private bool m_UpdateVisibility = true;
    private readonly SteppedDataTableLoader m_SteppedDataTableLoader;

    public async Task LoadSettingAsync(
      IFileSetting fileSetting,
      bool addErrorField,
      bool restoreError,
      TimeSpan durationInitial,
      FilterTypeEnum filterType,
      IProgress<ProgressInfo>? progress,
      EventHandler<WarningEventArgs>? addWarning, CancellationToken cancellationToken) =>
      await m_SteppedDataTableLoader.StartAsync(fileSetting, addErrorField, restoreError,
        durationInitial, filterType, progress, addWarning, cancellationToken);

    /// <inheritdoc />
    /// <summary>
    ///   Initializes a new instance of the <see cref="DetailControl" /> class.
    /// </summary>
#if !NETFRAMEWORK
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif

    public DetailControl()
    {
      InitializeComponent();

      // This created a BatchLoader
      m_SteppedDataTableLoader = new SteppedDataTableLoader(
        dataTable => DataTable = dataTable,
        RefreshDisplayAsync);

      m_ToolStripItems.Add(m_ToolStripComboBoxFilterType);
      m_ToolStripItems.Add(m_ToolStripButtonUniqueValues);
      m_ToolStripItems.Add(m_ToolStripButtonDuplicates);
      m_ToolStripItems.Add(m_ToolStripButtonHierarchy);
      m_ToolStripItems.Add(m_ToolStripButtonColumnLength);
      m_ToolStripItems.Add(m_ToolStripButtonStore);
    }

    public EventHandler<IFileSettingPhysicalFile>? BeforeFileStored;
    public EventHandler<IFileSettingPhysicalFile>? FileStored;
    private DataColumnCollection Columns => m_DataTable.Columns;

    /// <summary>
    ///   Gets or sets the HTML style.
    /// </summary>
    /// <value>The HTML style.</value>
    public HtmlStyle HtmlStyle { get => FilteredDataGridView.HtmlStyle; set => FilteredDataGridView.HtmlStyle = value; }

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
        Logger.Warning(ex, "Processing Sorting {exception}", ex.InnerExceptionMessages());
      }
    }

    private DataGridViewColumn? GetViewColumn(string dataColumnName) =>
      FilteredDataGridView.Columns.Cast<DataGridViewColumn>().FirstOrDefault(col => col.DataPropertyName.Equals(dataColumnName, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Set a filter on a data column
    /// </summary>
    /// <param name="dataColumnName">The name of the data column</param>
    /// <param name="op">The operator for teh filter e.G. =</param>
    /// <param name="value">The value to compare to</param>
    public void SetFilter(string dataColumnName, string op, object value)
    {
      FilteredDataGridView.SafeInvoke(
        () =>
        {
          var col = GetViewColumn(dataColumnName);
          if (col != null)
          {
            var columnFilters = new List<ToolStripDataGridViewColumnFilter>
            {
              new(col)
            };

            columnFilters[0].ColumnFilterLogic.Operator = op;
            if (value is DateTime dateTime)
              columnFilters[0].ColumnFilterLogic.ValueDateTime = dateTime;
            else
              columnFilters[0].ColumnFilterLogic.ValueText = value.ToString() ?? string.Empty;

            columnFilters[0].ColumnFilterLogic.Active = true;
          }
        }
      );
    }

    public string GetViewStatus() => FilteredDataGridView.GetViewStatus;
    public void SetViewStatus(string newStatus) => FilteredDataGridView.SetViewStatus(newStatus);

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

    public CancellationToken CancellationToken
    {
      set
      {
        m_CancellationToken = value;
      }
    }



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

        m_DataTable.Dispose();
        m_FilterDataTable?.Dispose();
        m_FilterDataTable = null;

        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        m_DataTable = value ?? new DataTable();
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

    /// <summary>
    ///   A File Setting
    /// </summary>
    public IFileSetting? FileSetting
    {
      get => FilteredDataGridView.FileSetting;
      set
      {
        FilteredDataGridView.FileSetting = value;
        //        m_UpdateVisibility = true;
      }
    }

    /// <summary>
    ///   A File Setting
    /// </summary>
    public ICsvFile? WriteSetting
    {
      get;
      set;
    }

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
    ///   Gets or sets a value indicating whether this is a read only.
    /// </summary>
    /// <value><c>true</c> if read only; otherwise, <c>false</c>.</value>
    [Browsable(true)]
    [DefaultValue(false)]
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
        // in case we do not have unique names and the table is not loaded do nothing
        if ((value is null || !value.Any()))
          return;
        if (m_FilterDataTable != null)
          m_FilterDataTable.UniqueFieldName = value;
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


    private void DetailControl_FontChanged(object? sender, EventArgs e)
    {
      this.SafeInvoke(() =>
      {
        FilteredDataGridView.Font = Font;
        m_BindingNavigator.Font = Font;
        m_ToolStripTop.Font = Font;
      });
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      if (m_DisposedValue) return;

      if (disposing)
      {
        m_DisposedValue = true;
        components.Dispose();
        m_FormShowMaxLength?.Dispose();
        m_FormDuplicatesDisplay?.Dispose();
        m_FormUniqueDisplay?.Dispose();
        m_CurrentSearchProcessInformation?.Dispose();
        m_DataTable.Dispose();
        m_FilterDataTable?.Dispose();
        m_HierarchyDisplay?.Dispose();
        m_SteppedDataTableLoader.Dispose();
      }

      base.Dispose(disposing);
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
      if (!(obj is ProcessInformation processInformation))
        return;
      processInformation.IsRunning = true;
      try
      {
        if (string.IsNullOrEmpty(processInformation.SearchText))
          return;

        // Do not search for an text shorter than 2 if we have a lot of data
        if (processInformation.SearchText.Length < 2 && m_SearchCells.Count() > 10000)
          return;
        Extensions.InvokeWithHourglass(() =>
        {
          foreach (var cell in m_SearchCells)
          {
            if (processInformation.CancellationTokenSource?.IsCancellationRequested ?? false)
              return;

            if (cell.Key.IndexOf(processInformation.SearchText, StringComparison.OrdinalIgnoreCase) <= -1)
              continue;
            processInformation.FoundResultEvent?.Invoke(this, new FoundEventArgs(processInformation.Found, cell.Value));
            processInformation.Found++;
          }
        });
      }
      finally
      {
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
      m_ToolStripButtonColumnLength.RunWithHourglass(() =>
      {
        var visible = FilteredDataGridView.Columns.Cast<DataGridViewColumn>()
          .Where(col => col.Visible && !string.IsNullOrEmpty(col.DataPropertyName)).OrderBy(col => col.DisplayIndex)
          .Select(col => col.DataPropertyName).ToList();
        m_FormShowMaxLength?.Close();
        m_FormShowMaxLength =
          new FormShowMaxLength(m_DataTable, m_DataTable.Select(FilteredDataGridView.CurrentFilter), visible,
            HtmlStyle);
        ResizeForm.SetFonts(m_FormShowMaxLength, Font);
        m_FormShowMaxLength.Show(ParentForm);

        m_FormShowMaxLength.FormClosed += (_, _) => this.SafeInvoke(() => m_ToolStripButtonColumnLength.Enabled = true);
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
      if (FilteredDataGridView.Columns.Count <= 0)
        return;
      m_ToolStripButtonDuplicates.RunWithHourglass(() =>
      {
        var columnName = FilteredDataGridView.CurrentCell != null
          ? FilteredDataGridView.Columns[FilteredDataGridView.CurrentCell.ColumnIndex].Name
          : FilteredDataGridView.Columns[0].Name;
        try
        {
          m_FormDuplicatesDisplay?.Close();
          m_FormDuplicatesDisplay =
            new FormDuplicatesDisplay(m_DataTable.Clone(), m_DataTable.Select(FilteredDataGridView.CurrentFilter),
              columnName, HtmlStyle)
            { Icon = ParentForm?.Icon };
          ResizeForm.SetFonts(m_FormDuplicatesDisplay, Font);
          m_FormDuplicatesDisplay.Show(ParentForm);
          m_FormDuplicatesDisplay.FormClosed +=
            (_, _) => this.SafeInvoke(() => m_ToolStripButtonDuplicates.Enabled = true);
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
      m_ToolStripButtonHierarchy.RunWithHourglass(() =>
      {
        try
        {
          m_HierarchyDisplay?.Close();
          m_HierarchyDisplay =
            new FormHierarchyDisplay(m_DataTable.Clone(), m_DataTable.Select(FilteredDataGridView.CurrentFilter),
              HtmlStyle)
            { Icon = ParentForm?.Icon };
          ResizeForm.SetFonts(m_HierarchyDisplay, Font);
          m_HierarchyDisplay.Show(ParentForm);
          m_HierarchyDisplay.FormClosed += (_, _) => this.SafeInvoke(() => m_ToolStripButtonHierarchy.Enabled = true);
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
      if (FilteredDataGridView.Columns.Count <= 0)
        return;
      m_ToolStripButtonUniqueValues.RunWithHourglass(() =>
      {
        try
        {
          var columnName = FilteredDataGridView.CurrentCell != null
            ? FilteredDataGridView.Columns[FilteredDataGridView.CurrentCell.ColumnIndex].Name
            : FilteredDataGridView.Columns[0].Name;
          m_FormUniqueDisplay?.Close();
          m_FormUniqueDisplay = new FormUniqueDisplay(
            m_DataTable.Clone(),
            m_DataTable.Select(FilteredDataGridView.CurrentFilter),
            columnName, HtmlStyle);
          ResizeForm.SetFonts(m_FormUniqueDisplay, Font);

          m_FormUniqueDisplay.ShowDialog(ParentForm);
        }
        catch (Exception ex)
        {
          ParentForm.ShowError(ex);
        }
      }, ParentForm);
    }

    private void OnSearchClear(object? sender, EventArgs e)
    {
      (this).SafeInvoke(
        () =>
        {
          FilteredDataGridView.HighlightText = string.Empty;
          m_FoundCells.Clear();
          FilteredDataGridView.Refresh();
          m_Search.Results = 0;
        });
      m_CurrentSearchProcessInformation?.Dispose();
    }

    private void DataViewChanged(object? sender, EventArgs args)
    {
      m_SearchCellsDirty = true;
      if (!m_Search.Visible)
        return;
      if (m_CurrentSearchProcessInformation is { IsRunning: true })
        m_CurrentSearchProcessInformation.Cancel();
      m_Search.Results = 0;
      m_Search.Hide();
      OnSearchClear(sender, args);
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

      if (m_FilterDataTable?.FilterTable != null && m_FilterDataTable.FilterTable.Rows.Count <= 0)
        return;
      if (m_FilterDataTable != null && (m_FilterDataTable.GetColumnsWithoutErrors()).Count == Columns.Count)
        return;
      foreach (DataGridViewColumn dgCol in FilteredDataGridView.Columns)
      {
        if (m_FilterDataTable != null && (!dgCol.Visible ||
                                          !(m_FilterDataTable.GetColumnsWithoutErrors()).Contains(
                                            dgCol.DataPropertyName))) continue;
        dgCol.Visible = false;
        m_SearchCellsDirty = true;
      }
    }

    /// <summary>
    ///   Called when search changes.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="SearchEventArgs" /> instance containing the event data.</param>
    private void OnSearchChanged(object? sender, SearchEventArgs e)
    {
      // Stop any current searches
      if (m_CurrentSearchProcessInformation is { IsRunning: true })
      {
        m_CurrentSearchProcessInformation.SearchEventArgs = e;

        // Tell the current search to carry on with a new search after its done / canceled
        m_CurrentSearchProcessInformation.SearchCompleteEvent += StartSearch;

        // Cancel the current search
        m_CurrentSearchProcessInformation.Cancel();
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

    private void PopulateSearchCellList()
    {
      if (!m_SearchCellsDirty)
        return;

      Extensions.InvokeWithHourglass(() =>
      {
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
      });
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
      this.SafeBeginInvoke(() => { m_Search.Results = m_CurrentSearchProcessInformation?.Found ?? 0; });


    /// <summary>
    ///   Sets the data source.
    /// </summary>
    public async Task RefreshDisplayAsync(FilterTypeEnum filterType, CancellationToken cancellationToken)
    {

      var oldSortedColumn = FilteredDataGridView.SortedColumn?.DataPropertyName;
      var oldOrder = FilteredDataGridView.SortOrder;

      // Cancel the current search
      if (m_CurrentSearchProcessInformation is { IsRunning: true })
        m_CurrentSearchProcessInformation.Cancel();

      // Hide any showing search
      m_Search.Visible = false;

      var newDt = m_DataTable;

      m_FilterDataTable ??= new FilterDataTable(m_DataTable);
      if (filterType != FilterTypeEnum.All)
      {
        if (filterType != m_FilterDataTable.FilterType)
          await m_FilterDataTable.FilterAsync(int.MaxValue, filterType, cancellationToken);
        newDt = m_FilterDataTable.FilterTable;
      }

      if (ReferenceEquals(m_BindingSource.DataSource, newDt))
        return;

      this.SafeInvokeNoHandleNeeded(() =>
      {
        // Now apply filter
        FilteredDataGridView.DataSource = null;

        m_BindingSource.DataSource = newDt;
        FilteredDataGridView.DataSource = m_BindingSource;

        FilterColumns(!filterType.HasFlag(FilterTypeEnum.ShowIssueFree));

        AutoResizeColumns(newDt);
        FilteredDataGridView.ColumnVisibilityChanged();
        FilteredDataGridView.SetRowHeight();

        if (oldOrder != SortOrder.None && !(oldSortedColumn is null || oldSortedColumn.Length == 0))
          Sort(oldSortedColumn,
            oldOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
      });


      this.SafeInvoke(() =>
      {
        var newIndex = filterType switch
        {
          FilterTypeEnum.ErrorsAndWarning => 1,
          FilterTypeEnum.ShowErrors => 2,
          FilterTypeEnum.ShowWarning => 3,
          FilterTypeEnum.ShowIssueFree => 4,
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

    private void StartSearch(object? sender, SearchEventArgs e)
    {
      OnSearchClear(this, EventArgs.Empty);
      FilteredDataGridView.HighlightText = e.SearchText;

      var processInformation = new ProcessInformation
      {
        SearchText = e.SearchText,
        CancellationTokenSource =
          CancellationTokenSource.CreateLinkedTokenSource(m_CancellationToken)
      };

      processInformation.FoundResultEvent += ResultFound;
      processInformation.SearchCompleteEvent += SearchComplete;
      processInformation.SearchEventArgs = e;
      m_CurrentSearchProcessInformation = processInformation;
      ThreadPool.QueueUserWorkItem(BackgroundSearchThread, processInformation);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public async Task SafeCurrentFile(string fileName)
    {
      if (FilteredDataGridView.DataView is null)
        return;
      if (WriteSetting == null)
      {
        WriteSetting = new CsvFile(id: string.Empty, fileName: string.Empty);
        FileSetting?.CopyTo(WriteSetting);
      }

      var headerAndSipped = new StringBuilder(WriteSetting.Header);
      // in case we skipped lines read them as Header so we do not loose them
      if (WriteSetting.SkipRows > 0 && string.IsNullOrEmpty(WriteSetting.Header))
      {
#if NET5_0_OR_GREATER
        await
#endif
        using var iStream = FunctionalDI.OpenStream(new SourceAccess(WriteSetting));
        using var sr = new ImprovedTextReader(iStream, WriteSetting.CodePageId);
        for (var i = 0; i < WriteSetting.SkipRows; i++)
          headerAndSipped.AppendLine(await sr.ReadLineAsync());
      }

      using var formProgress = new FormProgress(WriteSetting.ToString(), true, m_CancellationToken);
      try
      {
        formProgress.Show(ParentForm);

        BeforeFileStored?.Invoke(this, WriteSetting);
        var writer = new CsvFileWriter(FileSetting?.ID ?? string.Empty, fileName, WriteSetting.HasFieldHeader,
          WriteSetting.ValueFormatWrite,
          WriteSetting.CodePageId,
          WriteSetting.ByteOrderMark, WriteSetting.ColumnCollection, WriteSetting.KeyID, WriteSetting.KeepUnencrypted,
          WriteSetting.IdentifierInContainer,
          headerAndSipped.ToString(), WriteSetting.Footer, string.Empty, WriteSetting.NewLine,
          WriteSetting.FieldDelimiterChar,
          WriteSetting.FieldQualifierChar,
          WriteSetting.EscapePrefixChar,
          WriteSetting.NewLinePlaceholder, WriteSetting.DelimiterPlaceholder, WriteSetting.QualifierPlaceholder,
          WriteSetting.QualifyAlways, WriteSetting.QualifyOnlyIfNeeded, StandardTimeZoneAdjust.ChangeTimeZone,
          TimeZoneInfo.Local.Id);

#if NET5_0_OR_GREATER
        await
#endif
        using var dt = new DataTableWrapper(
          FilteredDataGridView.DataView.ToTable(false,
            // Restrict to shown data
            FilteredDataGridView.Columns.Cast<DataGridViewColumn>()
              .Where(col => col.Visible && col.DataPropertyName.NoArtificialField())
              .OrderBy(col => col.DisplayIndex)
              .Select(col => col.DataPropertyName).ToArray()));
        // can not use filteredDataGridView.Columns directly
        await writer.WriteAsync(dt, formProgress.CancellationToken);
      }
      catch (Exception ex)
      {
        ParentForm.ShowError(ex);
      }
      finally
      {
        FileStored?.Invoke(this, WriteSetting);
      }
    }

    private async void ToolStripButtonStoreAsCsvAsync(object? sender, EventArgs e)
    {
      try
      {
        FileSystemUtils.SplitResult split;

        if (FileSetting is IFileSettingPhysicalFile settingPhysicalFile)
          split = FileSystemUtils.SplitPath(settingPhysicalFile.FullPath);
        else
          split = new FileSystemUtils.SplitResult(Directory.GetCurrentDirectory(),
            $"{FileSetting!.ID}.txt");

        var fileName = WindowsAPICodePackWrapper.Save(split.DirectoryName, "Delimited File",
          "Text file (*.txt)|*.txt|Comma delimited (*.csv)|*.csv|Tab delimited (*.tab;*.tsv)|*.tab;*.tsv|All files (*.*)|*.*",
          ".csv",
          false,
          split.FileName);

        if (fileName is null || fileName.Length == 0)
          return;

        await SafeCurrentFile(fileName);
      }
      catch (Exception ex)
      {
        ParentForm.ShowError(ex);
      }
    }

    public void ReStoreViewSetting(string fileName) => FilteredDataGridView.ReStoreViewSetting(fileName);


    private FilterTypeEnum GetCurrentFilter()
    {
      int index = 0;
      this.SafeInvoke(() => index = m_ToolStripComboBoxFilterType.SelectedIndex);
      if (index == 1)
        return FilterTypeEnum.ErrorsAndWarning;
      if (index == 2)
        return FilterTypeEnum.ShowErrors;
      if (index == 3)
        return FilterTypeEnum.ShowWarning;
      return index == 4 ? FilterTypeEnum.ShowIssueFree : FilterTypeEnum.All;
    }

    private async void ToolStripComboBoxFilterType_SelectedIndexChanged(object? sender, EventArgs e)
    {
      await RefreshDisplayAsync(GetCurrentFilter(), m_CancellationToken);
    }

    private async void ToolStripButtonLoadRemaining_Click(object? sender, EventArgs e)
    {
      if (m_SteppedDataTableLoader.EndOfFile)
        return;
      await m_ToolStripButtonLoadRemaining.RunWithHourglassAsync(async () =>
      {
        m_ToolStripLabelCount.Text = " loading...";

        using var formProgress = new FormProgress("Load more...", false, m_CancellationToken);
        formProgress.Show();
        formProgress.Maximum = BaseFileReader.cMaxProgress;
        await m_SteppedDataTableLoader.GetNextBatch(GetCurrentFilter(), formProgress, formProgress.CancellationToken);

      }, ParentForm);
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
        m_ToolStripButtonStore.Visible = m_ShowButtons && (FileSetting != null);
        m_ToolStripButtonHierarchy.Visible = m_ShowButtons;

        var hasData = m_SteppedDataTableLoader.EndOfFile && (m_DataTable.Rows.Count > 0);
        m_ToolStripButtonUniqueValues.Enabled = hasData;
        m_ToolStripButtonDuplicates.Enabled = hasData;
        m_ToolStripButtonColumnLength.Enabled = hasData;
        m_ToolStripButtonHierarchy.Enabled = hasData;
        m_ToolStripButtonStore.Enabled = hasData;
        toolStripButtonMoveLastItem.Enabled = hasData;
        FilteredDataGridView.toolStripMenuItemFilterAdd.Enabled = hasData;

        m_ToolStripButtonLoadRemaining.Visible = hasData && m_ShowButtons;
        m_ToolStripLabelCount.ForeColor = m_SteppedDataTableLoader.EndOfFile ? SystemColors.ControlText : SystemColors.MenuHighlight;
        m_ToolStripLabelCount.ToolTipText =
          m_SteppedDataTableLoader.EndOfFile ? "Total number of items" : "Total number of items (loaded so far)";

        m_ToolStripTop.Visible = m_ShowButtons;

        // Filter
        m_ToolStripComboBoxFilterType.Visible = m_ShowButtons && m_ShowFilter;
        m_ToolStripComboBoxFilterType.Enabled = hasData;
      }
      catch (InvalidOperationException exception)
      {
        Logger.Warning(exception, "Issue with updating the UI");
        // ignore error in regards to cross thread issues, SafeBeginInvoke should have handled
        // this though
      }
      finally
      {
        // now you could run this again
        m_TimerVisibility.Enabled = true;
      }
    }

    private sealed class ProcessInformation : DisposableBase
    {
      public CancellationTokenSource? CancellationTokenSource;

      public int Found;

      public EventHandler<FoundEventArgs>? FoundResultEvent;

      public bool IsRunning;

      public EventHandler<SearchEventArgs>? SearchCompleteEvent;

      public SearchEventArgs SearchEventArgs = new(string.Empty);

      public string SearchText = string.Empty;

      public void Cancel() => CancellationTokenSource?.Cancel();

      protected override void Dispose(bool disposing)
      {
        if (disposing)
          CancellationTokenSource?.Dispose();
      }
    }
  }
}