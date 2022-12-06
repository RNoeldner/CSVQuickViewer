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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

// ReSharper disable UnusedMember.Global

// ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract

namespace CsvTools
{
  /// <inheritdoc cref="UserControl" />
  /// <summary>
  ///   Windows from to show detail information for a dataTable
  /// </summary>
  public class DetailControl : UserControl
  {
    private readonly IContainer components = new Container();
    private readonly BindingNavigator m_BindingNavigator;
    private readonly BindingSource m_BindingSource;
    private readonly List<DataGridViewCell> m_FoundCells = new();
    private readonly Search m_Search;

    private readonly List<KeyValuePair<string, DataGridViewCell>> m_SearchCells = new();

    private readonly ToolStripButton m_ToolStripButtonColumnLength = new();
    private readonly ToolStripButton m_ToolStripButtonDuplicates = new();
    private readonly ToolStripButton m_ToolStripButtonHierarchy = new();
    private readonly ToolStripButton m_ToolStripButtonMoveFirstItem = new();
    private readonly ToolStripButton m_ToolStripButtonMoveLastItem = new();
    private readonly ToolStripButton m_ToolStripButtonMoveNextItem = new();
    private readonly ToolStripButton m_ToolStripButtonMovePreviousItem = new();
    private readonly ToolStripButton m_ToolStripButtonStore = new();
    private readonly ToolStripButton m_ToolStripButtonUniqueValues = new();
    private readonly ToolStripButton m_ToolStripButtonLoadRemaining = new();
    private readonly ToolStripComboBox m_ToolStripComboBoxFilterType = new();
    private readonly ToolStripContainer m_ToolStripContainer = new();

    private readonly ObservableCollection<ToolStripItem> m_ToolStripItems = new();
    private readonly ToolStripLabel m_ToolStripLabelCount = new();
    private readonly ToolStripTextBox m_ToolStripTextBoxPos = new();
    private readonly ToolStrip m_ToolStripTop = new();
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
    private Form? m_ParentForm;
    private bool m_SearchCellsDirty = true;
    private bool m_ShowButtons = true;
    private bool m_ShowFilter = true;

    /// <inheritdoc />
    /// <summary>
    ///   Initializes a new instance of the <see cref="DetailControl" /> class.
    /// </summary>
#if !NETFRAMEWORK
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif

    public DetailControl()
    {
      ComponentResourceManager resources = new ComponentResourceManager(typeof(DetailControl));
      m_BindingNavigator = new BindingNavigator(components);
      m_BindingSource = new BindingSource(components);
      m_Search = new Search();
      m_ToolStripTop.SuspendLayout();
      m_ToolStripContainer.BottomToolStripPanel.SuspendLayout();
      m_ToolStripContainer.ContentPanel.SuspendLayout();
      m_ToolStripContainer.TopToolStripPanel.SuspendLayout();
      m_ToolStripContainer.SuspendLayout();
      ((ISupportInitialize) (m_BindingNavigator)).BeginInit();
      m_BindingNavigator.SuspendLayout();
      ((ISupportInitialize) (m_BindingSource)).BeginInit();
      SuspendLayout();
      // m_ToolStripTop
      m_ToolStripTop.Dock = DockStyle.None;
      m_ToolStripTop.GripStyle = ToolStripGripStyle.Hidden;
      m_ToolStripTop.ImageScalingSize = new Size(20, 20);
      m_ToolStripTop.Items.AddRange(new ToolStripItem[]
      {
        m_ToolStripComboBoxFilterType, m_ToolStripButtonUniqueValues, m_ToolStripButtonColumnLength,
        m_ToolStripButtonDuplicates, m_ToolStripButtonHierarchy, m_ToolStripButtonStore
      });
      m_ToolStripTop.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
      m_ToolStripTop.Location = new Point(4, 0);
      m_ToolStripTop.Size = new Size(709, 28);
      m_ToolStripTop.TabIndex = 1;
      // m_ToolStripComboBoxFilterType
      m_ToolStripComboBoxFilterType.DropDownHeight = 90;
      m_ToolStripComboBoxFilterType.DropDownWidth = 130;
      m_ToolStripComboBoxFilterType.IntegralHeight = false;
      m_ToolStripComboBoxFilterType.Items.AddRange(new object[]
      {
        "All Records", "Error or Warning", "Only Errors", "Only Warning", "No Error or Warning"
      });
      m_ToolStripComboBoxFilterType.Size = new Size(150, 28);
      m_ToolStripComboBoxFilterType.SelectedIndex = 0;
      // m_ToolStripButtonUniqueValues
      m_ToolStripButtonUniqueValues.Image = resources.GetObject("m_ToolStripButtonUniqueValues.Image") as Image;
      m_ToolStripButtonUniqueValues.Size = new Size(126, 25);
      m_ToolStripButtonUniqueValues.Text = "Unique Values";
      m_ToolStripButtonUniqueValues.ToolTipText = "Display Unique Values";
      m_ToolStripButtonUniqueValues.Click += ButtonUniqueValues_Click;

      // m_ToolStripButtonColumnLength
      m_ToolStripButtonColumnLength.Image = resources.GetObject("m_ToolStripButtonColumnLength.Image") as Image;
      m_ToolStripButtonColumnLength.Size = new Size(133, 25);
      m_ToolStripButtonColumnLength.Text = "Columns";
      m_ToolStripButtonColumnLength.ToolTipText = "Display Schema information";
      m_ToolStripButtonColumnLength.Click += ButtonColumnLength_Click;

      // m_ToolStripButtonDuplicates
      m_ToolStripButtonDuplicates.Image = resources.GetObject("m_ToolStripButtonDuplicates.Image") as Image;
      m_ToolStripButtonDuplicates.Size = new Size(103, 25);
      m_ToolStripButtonDuplicates.Text = "Duplicates";
      m_ToolStripButtonDuplicates.ToolTipText = "Display Duplicate Values";
      m_ToolStripButtonDuplicates.Click += ButtonDuplicates_Click;

      // m_ToolStripButtonHierarchy
      m_ToolStripButtonHierarchy.Image = resources.GetObject("m_ToolStripButtonHierarchy.Image") as Image;
      m_ToolStripButtonHierarchy.Size = new Size(96, 25);
      m_ToolStripButtonHierarchy.Text = "Hierarchy";
      m_ToolStripButtonHierarchy.ToolTipText = "Display a Hierarchy Structure";
      m_ToolStripButtonHierarchy.Click += ButtonHierarchy_Click;

      // m_ToolStripButtonStore
      m_ToolStripButtonStore.Image = resources.GetObject("m_ToolStripButtonStore.Image") as Image;
      m_ToolStripButtonStore.ImageTransparentColor = Color.Magenta;
      m_ToolStripButtonStore.Size = new Size(96, 25);
      m_ToolStripButtonStore.Text = "Write File";
      m_ToolStripButtonStore.ToolTipText = "Store the currently displayed data as delimited text file";
      m_ToolStripButtonStore.Click += ToolStripButtonStoreAsCsvAsync;

      // m_ToolStripContainer
      //
      //
      // m_ToolStripContainer.BottomToolStripPanel
      m_ToolStripContainer.BottomToolStripPanel.Controls.Add(m_BindingNavigator);
      // m_ToolStripContainer.ContentPanel
      m_ToolStripContainer.ContentPanel.Controls.Add(m_Search);
      m_ToolStripContainer.ContentPanel.Margin = new Padding(5, 4, 5, 4);
      m_ToolStripContainer.ContentPanel.Size = new Size(1328, 407);
      m_ToolStripContainer.Dock = DockStyle.Fill;
      m_ToolStripContainer.LeftToolStripPanelVisible = false;
      m_ToolStripContainer.Location = new Point(0, 0);
      m_ToolStripContainer.Margin = new Padding(5, 4, 5, 4);
      m_ToolStripContainer.Name = "m_ToolStripContainer";
      m_ToolStripContainer.RightToolStripPanelVisible = false;
      m_ToolStripContainer.Size = new Size(1328, 462);
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
        m_ToolStripButtonMoveFirstItem, m_ToolStripButtonMovePreviousItem, m_ToolStripTextBoxPos,
        m_ToolStripLabelCount, m_ToolStripButtonMoveNextItem, m_ToolStripButtonMoveLastItem,
        m_ToolStripButtonLoadRemaining
      });
      m_BindingNavigator.Location = new Point(4, 0);
      m_BindingNavigator.MoveFirstItem = m_ToolStripButtonMoveFirstItem;
      m_BindingNavigator.MoveLastItem = m_ToolStripButtonMoveLastItem;
      m_BindingNavigator.MoveNextItem = m_ToolStripButtonMoveNextItem;
      m_BindingNavigator.MovePreviousItem = m_ToolStripButtonMovePreviousItem;
      m_BindingNavigator.PositionItem = m_ToolStripTextBoxPos;
      m_BindingNavigator.Size = new Size(284, 27);
      m_BindingNavigator.TabIndex = 0;
      // m_ToolStripLabelCount
      m_ToolStripLabelCount.Size = new Size(45, 24);
      m_ToolStripLabelCount.Text = "of {0}";
      m_ToolStripLabelCount.TextAlign = ContentAlignment.MiddleLeft;
      m_ToolStripLabelCount.ToolTipText = "Total number of items";
      // m_ToolStripButtonMoveFirstItem
      m_ToolStripButtonMoveFirstItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
      m_ToolStripButtonMoveFirstItem.Image = resources.GetObject("m_ToolStripButtonMoveFirstItem.Image") as Image;
      m_ToolStripButtonMoveFirstItem.RightToLeftAutoMirrorImage = true;
      m_ToolStripButtonMoveFirstItem.Size = new Size(29, 24);
      m_ToolStripButtonMoveFirstItem.Text = "Move first";
      // m_ToolStripButtonMovePreviousItem
      m_ToolStripButtonMovePreviousItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
      m_ToolStripButtonMovePreviousItem.Image = resources.GetObject("m_ToolStripButtonMovePreviousItem.Image") as Image;
      m_ToolStripButtonMovePreviousItem.RightToLeftAutoMirrorImage = true;
      m_ToolStripButtonMovePreviousItem.Size = new Size(29, 24);
      m_ToolStripButtonMovePreviousItem.Text = "Move previous";
      // m_ToolStripTextBox1
      m_ToolStripTextBoxPos.AccessibleName = "Position";
      m_ToolStripTextBoxPos.Size = new Size(50, 27);
      m_ToolStripTextBoxPos.Text = "0";
      m_ToolStripTextBoxPos.ToolTipText = "Current position";
      // m_ToolStripButtonMoveNextItem
      m_ToolStripButtonMoveNextItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
      m_ToolStripButtonMoveNextItem.Image = resources.GetObject("m_ToolStripButtonMoveNextItem.Image") as Image;
      m_ToolStripButtonMoveNextItem.RightToLeftAutoMirrorImage = true;
      m_ToolStripButtonMoveNextItem.Size = new Size(29, 24);
      m_ToolStripButtonMoveNextItem.Text = "Move next";
      // m_ToolStripButtonMoveLastItem
      m_ToolStripButtonMoveLastItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
      m_ToolStripButtonMoveLastItem.Image = resources.GetObject("m_ToolStripButtonMoveLastItem.Image") as Image;
      m_ToolStripButtonMoveLastItem.RightToLeftAutoMirrorImage = true;
      m_ToolStripButtonMoveLastItem.Size = new Size(29, 24);
      m_ToolStripButtonMoveLastItem.Text = "Move last";
      // ToolStripButtonNext
      m_ToolStripButtonLoadRemaining.DisplayStyle = ToolStripItemDisplayStyle.Image;
      m_ToolStripButtonLoadRemaining.Image = resources.GetObject("ToolStripButtonNext.Image") as Image;
      m_ToolStripButtonLoadRemaining.Size = new Size(29, 24);
      m_ToolStripButtonLoadRemaining.Text = "Load More...";
      m_ToolStripButtonLoadRemaining.ToolTipText = "Not all records have been read so fra, load another set of records";
      m_ToolStripButtonLoadRemaining.TextImageRelation = TextImageRelation.TextBeforeImage;
      m_ToolStripButtonLoadRemaining.Click += ToolStripButtonLoadRemaining_Click;
      // m_Search
      m_Search.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      m_Search.AutoSize = true;
      m_Search.BackColor = SystemColors.Info;
      m_Search.BorderStyle = BorderStyle.FixedSingle;
      m_Search.Location = new Point(825, 4);
      m_Search.Margin = new Padding(5, 4, 5, 4);
      m_Search.Results = 0;
      m_Search.Size = new Size(503, 44);
      m_Search.TabIndex = 1;
      m_Search.Visible = false;
      m_Search.OnResultChanged += OnSearchResultChanged;
      m_Search.OnSearchChanged += OnSearchChanged;
      m_Search.OnSearchClear += OnSearchClear;
      // DetailControl
      AutoScaleDimensions = new SizeF(8F, 16F);
      AutoScaleMode = AutoScaleMode.Font;
      Controls.Add(m_ToolStripContainer);
      Margin = new Padding(5, 4, 5, 4);
      Name = "DetailControl";
      Size = new Size(1328, 462);
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
      ((ISupportInitialize) m_BindingNavigator).EndInit();
      m_BindingNavigator.ResumeLayout(false);
      m_BindingNavigator.PerformLayout();
      ((ISupportInitialize) m_BindingSource).EndInit();
      ResumeLayout(false);

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
      m_ToolStripContainer.ContentPanel.Controls.Add(FilteredDataGridView);

      m_ToolStripItems.Add(m_ToolStripComboBoxFilterType);
      m_ToolStripItems.Add(m_ToolStripButtonUniqueValues);
      m_ToolStripItems.Add(m_ToolStripButtonDuplicates);
      m_ToolStripItems.Add(m_ToolStripButtonHierarchy);
      m_ToolStripItems.Add(m_ToolStripButtonColumnLength);
      m_ToolStripItems.Add(m_ToolStripButtonStore);

      m_ToolStripItems.CollectionChanged += (_, _) => MoveMenu();
      FontChanged += DetailControl_FontChanged;
      m_ToolStripComboBoxFilterType.SelectedIndexChanged += ToolStripComboBoxFilterType_SelectedIndexChanged;

      MoveMenu();
    }

    public Func<bool>? EndOfFile { get; set; }
    public EventHandler<IFileSettingPhysicalFile>? BeforeFileStored;
    public EventHandler<IFileSettingPhysicalFile>? FileStored;
    public Func<IProgress<ProgressInfo>, CancellationToken, Task>? LoadNextBatchAsync { get; set; }
    private DataColumnCollection Columns => m_DataTable.Columns;


    /// <summary>
    ///   Gets or sets the HTML style.
    /// </summary>
    /// <value>The HTML style.</value>
    public HtmlStyle HtmlStyle { get => FilteredDataGridView.HtmlStyle; set => FilteredDataGridView.HtmlStyle = value; }

    /// <summary>
    ///   General Setting that determines if the menu is display in the bottom of a detail control
    /// </summary>
    public bool MenuDown
    {
      get => m_MenuDown;
      set
      {
        if (m_MenuDown == value) return;
        m_MenuDown = value;
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
      set
      {
        m_CancellationToken = value;
      }
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

        m_DataTable.Dispose();
        m_FilterDataTable?.Dispose();
        m_FilterDataTable = null;

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
          m_ToolStripComboBoxFilterType.SelectedIndex = value ? 1 : 0;
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
      SetButtonVisibility();
    }

    private void DetailControl_FontChanged(object? sender, EventArgs e)
    {
      this.SafeInvoke(() =>
      {
        FilteredDataGridView.DefaultCellStyle.Font = this.Font;
        ((m_MenuDown) ? m_BindingNavigator : m_ToolStripTop).Font = this.Font;
      });
    }

    /// <summary>
    ///   Sorts the data grid view on a given column
    /// </summary>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="direction">The direction.</param>
    public void Sort(string columnName, ListSortDirection direction)
    {
      try
      {
        foreach (DataGridViewColumn col in FilteredDataGridView.Columns)
          if (col.DataPropertyName.Equals(columnName, StringComparison.OrdinalIgnoreCase) && col.Visible)
          {
            FilteredDataGridView.Sort(col, direction);
            break;
          }
      }
      catch
      {
        // ignore
      }
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
      }

      base.Dispose(disposing);
    }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
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
              columnName, HtmlStyle) { Icon = ParentForm?.Icon };
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
              HtmlStyle) { Icon = ParentForm?.Icon };
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

    private void FilteredDataGridView_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
      if (!(e.Value is DateTime cellValue))
        return;

      e.Value = StringConversion.DisplayDateTime(cellValue, CultureInfo.CurrentCulture);
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

      if (m_CurrentSearchProcessInformation?.IsRunning ?? false)
        m_CurrentSearchProcessInformation.Cancel();
      if (m_FilterDataTable?.Filtering ?? false)
        m_FilterDataTable.Cancel();
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

    private void SetButtonVisibility() =>
      this.SafeBeginInvoke(() =>
      {
        m_ToolStripContainer.TopToolStripPanelVisible = !m_MenuDown;

        // Need to set the control containing the buttons to visible Regular
        m_ToolStripButtonColumnLength.Visible = m_ShowButtons;
        m_ToolStripButtonDuplicates.Visible = m_ShowButtons;
        m_ToolStripButtonUniqueValues.Visible = m_ShowButtons;
        m_ToolStripButtonStore.Visible = m_ShowButtons && (FileSetting != null);
        m_ToolStripButtonHierarchy.Visible = m_ShowButtons;

        var hasData = !m_DataMissing && (m_DataTable.Rows.Count > 0);
        m_ToolStripButtonUniqueValues.Enabled = hasData;
        m_ToolStripButtonDuplicates.Enabled = hasData;
        m_ToolStripButtonColumnLength.Enabled = hasData;
        m_ToolStripButtonHierarchy.Enabled = hasData;
        m_ToolStripButtonStore.Enabled = hasData;
        m_ToolStripComboBoxFilterType.Enabled = hasData;
        m_ToolStripComboBoxFilterType.Visible = hasData;
        m_ToolStripButtonMoveLastItem.Enabled = hasData;
        FilteredDataGridView.toolStripMenuItemFilterAdd.Enabled = hasData;

        m_ToolStripButtonLoadRemaining.Visible = m_DataMissing && m_ShowButtons;
        m_ToolStripLabelCount.ForeColor = !m_DataMissing ? SystemColors.ControlText : SystemColors.MenuHighlight;
        m_ToolStripLabelCount.ToolTipText =
          !m_DataMissing ? "Total number of items" : "Total number of items (loaded so far)";
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

    private bool m_DataMissing;

    public bool DataMissing
    {
      set
      {
        if (m_DataMissing != value)
        {
          m_DataMissing = value;
          SetButtonVisibility();
        }
      }
    }

    /// <summary>
    ///   Sets the data source.
    /// </summary>
    public async Task RefreshDisplayAsync(FilterTypeEnum type, CancellationToken cancellationToken)
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
      if (type != FilterTypeEnum.All)
      {
        if (type != m_FilterDataTable.FilterType)
          await m_FilterDataTable.FilterAsync(int.MaxValue, type, cancellationToken);
        newDt = m_FilterDataTable.FilterTable;
      }

      if (ReferenceEquals(m_BindingSource.DataSource, newDt))
        return;


      var newIndex = type switch
      {
        FilterTypeEnum.ErrorsAndWarning => 1,
        FilterTypeEnum.ShowErrors => 2,
        FilterTypeEnum.ShowWarning => 3,
        FilterTypeEnum.ShowIssueFree => 4,
        _ => 0
      };

      this.SafeInvokeNoHandleNeeded(() =>
      {
        // Now apply filter
        FilteredDataGridView.DataSource = null;

        m_BindingSource.DataSource = newDt;
        FilteredDataGridView.DataSource = m_BindingSource;

        FilterColumns(!type.HasFlag(FilterTypeEnum.ShowIssueFree));

        AutoResizeColumns(newDt);
        FilteredDataGridView.ColumnVisibilityChanged();
        FilteredDataGridView.SetRowHeight();

        if (oldOrder != SortOrder.None && !(oldSortedColumn is null || oldSortedColumn.Length == 0))
          Sort(oldSortedColumn,
            oldOrder == SortOrder.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending);
      });

      this.SafeInvoke(() =>
      {
        if (m_ToolStripComboBoxFilterType.SelectedIndex == newIndex) return;
        m_ToolStripComboBoxFilterType.SelectedIndexChanged -= ToolStripComboBoxFilterType_SelectedIndexChanged;
        m_ToolStripComboBoxFilterType.SelectedIndex = newIndex;
        m_ToolStripComboBoxFilterType.SelectedIndexChanged += ToolStripComboBoxFilterType_SelectedIndexChanged;
        if (m_ToolStripComboBoxFilterType.Focused)
          SendKeys.Send("{ESC}");
      });
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
        WriteSetting = new CsvFile(string.Empty, string.Empty);
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
        await RefreshDisplayAsync(FilterTypeEnum.All, m_CancellationToken);
      if (m_ToolStripComboBoxFilterType.SelectedIndex == 1)
        await RefreshDisplayAsync(FilterTypeEnum.ErrorsAndWarning, m_CancellationToken);
      if (m_ToolStripComboBoxFilterType.SelectedIndex == 2)
        await RefreshDisplayAsync(FilterTypeEnum.ShowErrors, m_CancellationToken);
      if (m_ToolStripComboBoxFilterType.SelectedIndex == 3)
        await RefreshDisplayAsync(FilterTypeEnum.ShowWarning, m_CancellationToken);
      if (m_ToolStripComboBoxFilterType.SelectedIndex == 4)
        await RefreshDisplayAsync(FilterTypeEnum.ShowIssueFree, m_CancellationToken);
      if (m_ToolStripComboBoxFilterType.Focused)
        SendKeys.Send("{ESC}");
    }

    private async void ToolStripButtonLoadRemaining_Click(object? sender, EventArgs e)
    {
      if (LoadNextBatchAsync is null || (EndOfFile?.Invoke() ?? true))
        return;
      await m_ToolStripButtonLoadRemaining.RunWithHourglassAsync(async () =>
      {
        m_ToolStripLabelCount.Text = " loading...";
        try
        {
          using var formProgress = new FormProgress("Load more...", false, m_CancellationToken);
          formProgress.Show();
          formProgress.Maximum = 100;
          await LoadNextBatchAsync(formProgress, formProgress.CancellationToken);
        }
        finally
        {
          var eof = EndOfFile();
          //          m_ToolStripLabelCount.Text = m_DataTable.Rows.Count.ToString();
          DataMissing = !eof;
        }
      }, ParentForm);
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