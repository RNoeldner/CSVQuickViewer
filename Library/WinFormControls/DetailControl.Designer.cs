using System.ComponentModel;
using System.Windows.Forms;

namespace CsvTools
{

  public partial class DetailControl
  {
    private IContainer components;
    private ToolStripButton m_ToolStripButtonColumnLength;
    private ToolStripButton m_ToolStripButtonDuplicates;
    private ToolStripButton m_ToolStripButtonHierarchy;
    private ToolStripButton toolStripButtonMoveFirstItem;
    private ToolStripButton toolStripButtonMoveLastItem;
    private ToolStripButton toolStripButtonMoveNextItem;
    private ToolStripButton toolStripButtonMovePreviousItem;
    private ToolStripButton m_ToolStripButtonStore;
    private ToolStripButton m_ToolStripButtonUniqueValues;
    private ToolStripComboBox m_ToolStripComboBoxFilterType;
    private ToolStripContainer m_ToolStripContainer;
    private ToolStripLabel m_ToolStripLabelCount;
    private ToolStripTextBox m_ToolStripTextBoxPos;
    private ToolStrip m_ToolStripTop;
    private Search m_Search;
    private System.Windows.Forms.Timer m_TimerVisibility;

    private void InitializeComponent()
    {
      components = new Container();
      var resources = new ComponentResourceManager(typeof(DetailControl));
      var dataGridViewCellStyle1 = new DataGridViewCellStyle();
      var dataGridViewCellStyle2 = new DataGridViewCellStyle();
      var dataGridViewCellStyle3 = new DataGridViewCellStyle();
      m_ToolStripLabelCount = new ToolStripLabel();
      toolStripButtonMoveFirstItem = new ToolStripButton();
      toolStripButtonMovePreviousItem = new ToolStripButton();
      m_ToolStripTextBoxPos = new ToolStripTextBox();
      toolStripButtonMoveNextItem = new ToolStripButton();
      toolStripButtonMoveLastItem = new ToolStripButton();
      FilteredDataGridView = new FilteredDataGridView();
      m_ToolStripButtonDuplicates = new ToolStripButton();
      m_ToolStripButtonHierarchy = new ToolStripButton();
      m_ToolStripButtonStore = new ToolStripButton();
      m_ToolStripButtonUniqueValues = new ToolStripButton();
      m_ToolStripComboBoxFilterType = new ToolStripComboBox();
      m_ToolStripContainer = new ToolStripContainer();
      m_ToolStripNavigation = new ToolStrip();
      m_ToolStripTextBoxTotal = new ToolStripTextBox();
      m_Search = new Search();
      m_ToolStripTop = new ToolStrip();
      m_ToolStripButtonColumnLength = new ToolStripButton();
      m_ToolStripButtonSource = new ToolStripButton();
      m_TimerVisibility = new Timer(components);
      timerLoadRemain = new Timer(components);
      m_NavRepeatTimer = new Timer(components);
      m_NavInputTimer = new Timer(components);
      ((ISupportInitialize) FilteredDataGridView).BeginInit();
      m_ToolStripContainer.BottomToolStripPanel.SuspendLayout();
      m_ToolStripContainer.ContentPanel.SuspendLayout();
      m_ToolStripContainer.TopToolStripPanel.SuspendLayout();
      m_ToolStripContainer.SuspendLayout();
      m_ToolStripNavigation.SuspendLayout();
      m_ToolStripTop.SuspendLayout();
      SuspendLayout();
      // 
      // m_ToolStripLabelCount
      // 
      m_ToolStripLabelCount.Name = "m_ToolStripLabelCount";
      m_ToolStripLabelCount.Size = new System.Drawing.Size(18, 22);
      m_ToolStripLabelCount.Text = "of";
      // 
      // toolStripButtonMoveFirstItem
      // 
      toolStripButtonMoveFirstItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
      toolStripButtonMoveFirstItem.Image = (System.Drawing.Image) resources.GetObject("toolStripButtonMoveFirstItem.Image");
      toolStripButtonMoveFirstItem.Name = "toolStripButtonMoveFirstItem";
      toolStripButtonMoveFirstItem.RightToLeftAutoMirrorImage = true;
      toolStripButtonMoveFirstItem.Size = new System.Drawing.Size(23, 22);
      toolStripButtonMoveFirstItem.Text = "Move first";
      toolStripButtonMoveFirstItem.Click += MoveFirst_Click;
      // 
      // toolStripButtonMovePreviousItem
      // 
      toolStripButtonMovePreviousItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
      toolStripButtonMovePreviousItem.Image = (System.Drawing.Image) resources.GetObject("toolStripButtonMovePreviousItem.Image");
      toolStripButtonMovePreviousItem.Name = "toolStripButtonMovePreviousItem";
      toolStripButtonMovePreviousItem.RightToLeftAutoMirrorImage = true;
      toolStripButtonMovePreviousItem.Size = new System.Drawing.Size(23, 22);
      toolStripButtonMovePreviousItem.Text = "Move previous";
      toolStripButtonMovePreviousItem.MouseDown += NavButton_MouseDown;
      toolStripButtonMovePreviousItem.MouseLeave += NavButton_MouseLeave;
      toolStripButtonMovePreviousItem.MouseUp += NavButton_MouseUp;
      // 
      // m_ToolStripTextBoxPos
      // 
      m_ToolStripTextBoxPos.AccessibleName = "Position";
      m_ToolStripTextBoxPos.AutoSize = false;
      m_ToolStripTextBoxPos.MaxLength = 10;
      m_ToolStripTextBoxPos.Name = "m_ToolStripTextBoxPos";
      m_ToolStripTextBoxPos.Size = new System.Drawing.Size(40, 25);
      m_ToolStripTextBoxPos.Text = "0";
      m_ToolStripTextBoxPos.ToolTipText = "Current record";
      // 
      // toolStripButtonMoveNextItem
      // 
      toolStripButtonMoveNextItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
      toolStripButtonMoveNextItem.Image = (System.Drawing.Image) resources.GetObject("toolStripButtonMoveNextItem.Image");
      toolStripButtonMoveNextItem.Name = "toolStripButtonMoveNextItem";
      toolStripButtonMoveNextItem.RightToLeftAutoMirrorImage = true;
      toolStripButtonMoveNextItem.Size = new System.Drawing.Size(23, 22);
      toolStripButtonMoveNextItem.Text = "Move next";
      toolStripButtonMoveNextItem.MouseDown += NavButton_MouseDown;
      toolStripButtonMoveNextItem.MouseLeave += NavButton_MouseLeave;
      toolStripButtonMoveNextItem.MouseUp += NavButton_MouseUp;
      // 
      // toolStripButtonMoveLastItem
      // 
      toolStripButtonMoveLastItem.DisplayStyle = ToolStripItemDisplayStyle.Image;
      toolStripButtonMoveLastItem.Image = (System.Drawing.Image) resources.GetObject("toolStripButtonMoveLastItem.Image");
      toolStripButtonMoveLastItem.Name = "toolStripButtonMoveLastItem";
      toolStripButtonMoveLastItem.RightToLeftAutoMirrorImage = true;
      toolStripButtonMoveLastItem.Size = new System.Drawing.Size(23, 22);
      toolStripButtonMoveLastItem.Text = "Move last";
      toolStripButtonMoveLastItem.Click += MoveLast_Click;
      // 
      // FilteredDataGridView
      // 
      FilteredDataGridView.AllowUserToAddRows = false;
      FilteredDataGridView.AllowUserToOrderColumns = true;
      dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
      FilteredDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
      FilteredDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
      dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point,  0);
      dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Black;
      dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
      FilteredDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
      FilteredDataGridView.Dock = DockStyle.Fill;
      FilteredDataGridView.HighlightText = "";
      FilteredDataGridView.Location = new System.Drawing.Point(0, 0);
      FilteredDataGridView.Margin = new Padding(0);
      FilteredDataGridView.Name = "FilteredDataGridView";
      dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
      FilteredDataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
      FilteredDataGridView.RowHeadersWidth = 51;
      FilteredDataGridView.ShowButtonAtLength = 1000;
      FilteredDataGridView.Size = new System.Drawing.Size(719, 366);
      FilteredDataGridView.TabIndex = 2;
      FilteredDataGridView.VirtualMode = true;
      // 
      // m_ToolStripButtonDuplicates
      // 
      m_ToolStripButtonDuplicates.Image = (System.Drawing.Image) resources.GetObject("m_ToolStripButtonDuplicates.Image");
      m_ToolStripButtonDuplicates.Name = "m_ToolStripButtonDuplicates";
      m_ToolStripButtonDuplicates.Size = new System.Drawing.Size(86, 24);
      m_ToolStripButtonDuplicates.Text = "Duplicates";
      m_ToolStripButtonDuplicates.ToolTipText = "Display Duplicate Values";
      m_ToolStripButtonDuplicates.Click += ButtonDuplicates_Click;
      // 
      // m_ToolStripButtonHierarchy
      // 
      m_ToolStripButtonHierarchy.Image = (System.Drawing.Image) resources.GetObject("m_ToolStripButtonHierarchy.Image");
      m_ToolStripButtonHierarchy.Name = "m_ToolStripButtonHierarchy";
      m_ToolStripButtonHierarchy.Size = new System.Drawing.Size(82, 24);
      m_ToolStripButtonHierarchy.Text = "Hierarchy";
      m_ToolStripButtonHierarchy.ToolTipText = "Display a Hierarchy Structure";
      m_ToolStripButtonHierarchy.Click += ButtonHierarchy_Click;
      // 
      // m_ToolStripButtonStore
      // 
      m_ToolStripButtonStore.Image = (System.Drawing.Image) resources.GetObject("m_ToolStripButtonStore.Image");
      m_ToolStripButtonStore.ImageTransparentColor = System.Drawing.Color.Magenta;
      m_ToolStripButtonStore.Name = "m_ToolStripButtonStore";
      m_ToolStripButtonStore.Size = new System.Drawing.Size(80, 24);
      m_ToolStripButtonStore.Text = "Write File";
      m_ToolStripButtonStore.ToolTipText = "Store the currently displayed data as delimited text file";
      m_ToolStripButtonStore.Click += ToolStripButtonStoreAsCsvAsync;
      // 
      // m_ToolStripButtonUniqueValues
      // 
      m_ToolStripButtonUniqueValues.Image = (System.Drawing.Image) resources.GetObject("m_ToolStripButtonUniqueValues.Image");
      m_ToolStripButtonUniqueValues.Name = "m_ToolStripButtonUniqueValues";
      m_ToolStripButtonUniqueValues.Size = new System.Drawing.Size(105, 24);
      m_ToolStripButtonUniqueValues.Text = "Unique Values";
      m_ToolStripButtonUniqueValues.ToolTipText = "Display Unique Values";
      m_ToolStripButtonUniqueValues.Click += ButtonUniqueValues_Click;
      // 
      // m_ToolStripComboBoxFilterType
      // 
      m_ToolStripComboBoxFilterType.DropDownHeight = 90;
      m_ToolStripComboBoxFilterType.DropDownWidth = 130;
      m_ToolStripComboBoxFilterType.Enabled = false;
      m_ToolStripComboBoxFilterType.IntegralHeight = false;
      m_ToolStripComboBoxFilterType.Items.AddRange(new object[] { "All Records" });
      m_ToolStripComboBoxFilterType.Name = "m_ToolStripComboBoxFilterType";
      m_ToolStripComboBoxFilterType.Size = new System.Drawing.Size(150, 27);
      m_ToolStripComboBoxFilterType.Text = "All Records";
      m_ToolStripComboBoxFilterType.Visible = false;
      m_ToolStripComboBoxFilterType.SelectedIndexChanged += ToolStripComboBoxFilterType_SelectedIndexChanged;
      // 
      // m_ToolStripContainer
      // 
      // 
      // m_ToolStripContainer.BottomToolStripPanel
      // 
      m_ToolStripContainer.BottomToolStripPanel.Controls.Add(m_ToolStripNavigation);
      // 
      // m_ToolStripContainer.ContentPanel
      // 
      m_ToolStripContainer.ContentPanel.Controls.Add(m_Search);
      m_ToolStripContainer.ContentPanel.Controls.Add(FilteredDataGridView);
      m_ToolStripContainer.ContentPanel.Size = new System.Drawing.Size(719, 366);
      m_ToolStripContainer.Dock = DockStyle.Fill;
      m_ToolStripContainer.LeftToolStripPanelVisible = false;
      m_ToolStripContainer.Location = new System.Drawing.Point(0, 0);
      m_ToolStripContainer.Name = "m_ToolStripContainer";
      m_ToolStripContainer.RightToolStripPanelVisible = false;
      m_ToolStripContainer.Size = new System.Drawing.Size(719, 418);
      m_ToolStripContainer.TabIndex = 13;
      m_ToolStripContainer.Text = "toolStripContainer";
      // 
      // m_ToolStripContainer.TopToolStripPanel
      // 
      m_ToolStripContainer.TopToolStripPanel.Controls.Add(m_ToolStripTop);
      // 
      // m_ToolStripNavigation
      // 
      m_ToolStripNavigation.Dock = DockStyle.None;
      m_ToolStripNavigation.GripStyle = ToolStripGripStyle.Hidden;
      m_ToolStripNavigation.Items.AddRange(new ToolStripItem[] { toolStripButtonMoveFirstItem, toolStripButtonMovePreviousItem, m_ToolStripTextBoxPos, m_ToolStripLabelCount, m_ToolStripTextBoxTotal, toolStripButtonMoveNextItem, toolStripButtonMoveLastItem });
      m_ToolStripNavigation.Location = new System.Drawing.Point(6, 0);
      m_ToolStripNavigation.Name = "m_ToolStripNavigation";
      m_ToolStripNavigation.Size = new System.Drawing.Size(197, 25);
      m_ToolStripNavigation.TabIndex = 1;
      // 
      // m_ToolStripTextBoxTotal
      // 
      m_ToolStripTextBoxTotal.AccessibleName = "Total";
      m_ToolStripTextBoxTotal.AutoSize = false;
      m_ToolStripTextBoxTotal.BackColor = System.Drawing.SystemColors.Control;
      m_ToolStripTextBoxTotal.ForeColor = System.Drawing.SystemColors.Highlight;
      m_ToolStripTextBoxTotal.MaxLength = 10;
      m_ToolStripTextBoxTotal.Name = "m_ToolStripTextBoxTotal";
      m_ToolStripTextBoxTotal.Size = new System.Drawing.Size(40, 25);
      m_ToolStripTextBoxTotal.Text = "200";
      m_ToolStripTextBoxTotal.TextChanged += ToolStripTextBoxTotal_TextChanged;
      // 
      // m_Search
      // 
      m_Search.Anchor =  AnchorStyles.Top | AnchorStyles.Right;
      m_Search.AutoSize = true;
      m_Search.BackColor = System.Drawing.SystemColors.ControlLightLight;
      m_Search.BorderStyle = BorderStyle.FixedSingle;
      m_Search.Location = new System.Drawing.Point(512, 0);
      m_Search.MinimumSize = new System.Drawing.Size(200, 25);
      m_Search.Name = "m_Search";
      m_Search.SearchText = "";
      m_Search.Size = new System.Drawing.Size(207, 27);
      m_Search.TabIndex = 1;
      m_Search.Visible = false;
      m_Search.OnSearchClear += OnSearchClear;
      m_Search.OnSearchNext += OnSearchNext;
      m_Search.OnSearchPrev += OnSearchPrev;
      // 
      // m_ToolStripTop
      // 
      m_ToolStripTop.Dock = DockStyle.None;
      m_ToolStripTop.GripStyle = ToolStripGripStyle.Hidden;
      m_ToolStripTop.ImageScalingSize = new System.Drawing.Size(20, 20);
      m_ToolStripTop.Items.AddRange(new ToolStripItem[] { m_ToolStripComboBoxFilterType, m_ToolStripButtonUniqueValues, m_ToolStripButtonColumnLength, m_ToolStripButtonDuplicates, m_ToolStripButtonHierarchy, m_ToolStripButtonSource, m_ToolStripButtonStore });
      m_ToolStripTop.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
      m_ToolStripTop.Location = new System.Drawing.Point(3, 0);
      m_ToolStripTop.Name = "m_ToolStripTop";
      m_ToolStripTop.Size = new System.Drawing.Size(502, 27);
      m_ToolStripTop.TabIndex = 1;
      // 
      // m_ToolStripButtonColumnLength
      // 
      m_ToolStripButtonColumnLength.Image = (System.Drawing.Image) resources.GetObject("m_ToolStripButtonColumnLength.Image");
      m_ToolStripButtonColumnLength.Name = "m_ToolStripButtonColumnLength";
      m_ToolStripButtonColumnLength.Size = new System.Drawing.Size(79, 24);
      m_ToolStripButtonColumnLength.Text = "Columns";
      m_ToolStripButtonColumnLength.ToolTipText = "Display Schema information";
      m_ToolStripButtonColumnLength.Click += ButtonColumnLength_Click;
      // 
      // m_ToolStripButtonSource
      // 
      m_ToolStripButtonSource.Image = (System.Drawing.Image) resources.GetObject("m_ToolStripButtonSource.Image");
      m_ToolStripButtonSource.ImageTransparentColor = System.Drawing.Color.Magenta;
      m_ToolStripButtonSource.Name = "m_ToolStripButtonSource";
      m_ToolStripButtonSource.Size = new System.Drawing.Size(67, 24);
      m_ToolStripButtonSource.Text = "Source";
      m_ToolStripButtonSource.Click += DisplaySource_Click;
      // 
      // m_TimerVisibility
      // 
      m_TimerVisibility.Enabled = true;
      m_TimerVisibility.Interval = 150;
      m_TimerVisibility.Tick += TimerVisibility_Tick;
      // 
      // timerLoadRemain
      // 
      timerLoadRemain.Interval = 500;
      timerLoadRemain.Tick += timerLoadRemain_Tick;
      // 
      // m_NavRepeatTimer
      // 
      m_NavRepeatTimer.Tick += NavRepeatTimer_Tick;
      // 
      // m_NavInputTimer
      // 
      m_NavInputTimer.Enabled = true;
      // 
      // DetailControl
      // 
      Controls.Add(m_ToolStripContainer);
      Margin = new Padding(0);
      Name = "DetailControl";
      Size = new System.Drawing.Size(719, 418);
      FontChanged += DetailControl_FontChanged;
      KeyDown += DetailControl_KeyDown;
      ParentChanged += DetailControl_ParentChanged;
      ((ISupportInitialize) FilteredDataGridView).EndInit();
      m_ToolStripContainer.BottomToolStripPanel.ResumeLayout(false);
      m_ToolStripContainer.BottomToolStripPanel.PerformLayout();
      m_ToolStripContainer.ContentPanel.ResumeLayout(false);
      m_ToolStripContainer.ContentPanel.PerformLayout();
      m_ToolStripContainer.TopToolStripPanel.ResumeLayout(false);
      m_ToolStripContainer.TopToolStripPanel.PerformLayout();
      m_ToolStripContainer.ResumeLayout(false);
      m_ToolStripContainer.PerformLayout();
      m_ToolStripNavigation.ResumeLayout(false);
      m_ToolStripNavigation.PerformLayout();
      m_ToolStripTop.ResumeLayout(false);
      m_ToolStripTop.PerformLayout();
      ResumeLayout(false);

    }


    private FilteredDataGridView FilteredDataGridView;
    private ToolStripButton m_ToolStripButtonSource;
    private Timer timerLoadRemain;
    private ToolStrip m_ToolStripNavigation;
    private ToolStripTextBox m_ToolStripTextBoxTotal;
    private Timer m_NavRepeatTimer;
    private Timer m_NavInputTimer;
  }
}
