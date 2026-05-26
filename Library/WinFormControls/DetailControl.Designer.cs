using System.ComponentModel;
using System.Windows.Forms;

namespace CsvTools
{

  public partial class DetailControl
  {
    private IContainer components;
    private ToolStripButton toolStripButtonColumnLength;
    private ToolStripButton toolStripButtonDuplicates;
    private ToolStripButton toolStripButtonHierarchy;
    private ToolStripButton btnMoveFirst;
    private ToolStripButton btnMoveLast;
    private ToolStripButton btnMoveNext;
    private ToolStripButton btnMovePrevious;
    private ToolStripButton toolStripButtonStore;
    private ToolStripButton toolStripButtonUniqueValues;
    private ToolStripComboBox toolStripComboBoxFilterType;
    private ToolStripContainer toolStripContainer;
    private ToolStripTextBox toolStripTextBoxPosition;
    private ToolStrip toolStripTop;
    private Search searchControl;
    private System.Windows.Forms.Timer timerVisibility;

    private void InitializeComponent()
    {
      components = new Container();
      ToolStripLabel toolStripLabelCount;
      var resources = new ComponentResourceManager(typeof(DetailControl));
      var dataGridViewCellStyle1 = new DataGridViewCellStyle();
      var dataGridViewCellStyle2 = new DataGridViewCellStyle();
      var dataGridViewCellStyle3 = new DataGridViewCellStyle();
      btnMoveFirst = new ToolStripButton();
      btnMovePrevious = new ToolStripButton();
      toolStripTextBoxPosition = new ToolStripTextBox();
      btnMoveNext = new ToolStripButton();
      btnMoveLast = new ToolStripButton();
      mainDataGridView = new FilteredDataGridView();
      toolStripButtonDuplicates = new ToolStripButton();
      toolStripButtonHierarchy = new ToolStripButton();
      toolStripButtonStore = new ToolStripButton();
      toolStripButtonUniqueValues = new ToolStripButton();
      toolStripComboBoxFilterType = new ToolStripComboBox();
      toolStripContainer = new ToolStripContainer();
      toolStripNavigation = new ToolStrip();
      toolStripTextBoxTotal = new ToolStripTextBox();
      searchControl = new Search();
      toolStripTop = new ToolStrip();
      toolStripButtonColumnLength = new ToolStripButton();
      toolStripButtonSource = new ToolStripButton();
      timerVisibility = new Timer(components);
      timerLoadRemain = new Timer(components);
      navRepeatTimer = new Timer(components);
      navInputTimer = new Timer(components);
      toolStripLabelCount = new ToolStripLabel();
      ((ISupportInitialize) mainDataGridView).BeginInit();
      toolStripContainer.BottomToolStripPanel.SuspendLayout();
      toolStripContainer.ContentPanel.SuspendLayout();
      toolStripContainer.TopToolStripPanel.SuspendLayout();
      toolStripContainer.SuspendLayout();
      toolStripNavigation.SuspendLayout();
      toolStripTop.SuspendLayout();
      SuspendLayout();
      // 
      // m_ToolStripLabelCount
      // 
      toolStripLabelCount.Name = "toolStripLabelCount";
      toolStripLabelCount.Size = new System.Drawing.Size(18, 22);
      toolStripLabelCount.Text = "of";
      // 
      // toolStripButtonMoveFirstItem
      // 
      btnMoveFirst.DisplayStyle = ToolStripItemDisplayStyle.Image;
      btnMoveFirst.Image = (System.Drawing.Image) resources.GetObject("btnMoveFirst.Image");
      btnMoveFirst.Name = "btnMoveFirst";
      btnMoveFirst.RightToLeftAutoMirrorImage = true;
      btnMoveFirst.Size = new System.Drawing.Size(23, 22);
      btnMoveFirst.Text = "Move first";
      btnMoveFirst.Click += MoveFirst_Click;
      // 
      // toolStripButtonMovePreviousItem
      // 
      btnMovePrevious.DisplayStyle = ToolStripItemDisplayStyle.Image;
      btnMovePrevious.Image = (System.Drawing.Image) resources.GetObject("btnMovePrevious.Image");
      btnMovePrevious.Name = "btnMovePrevious";
      btnMovePrevious.RightToLeftAutoMirrorImage = true;
      btnMovePrevious.Size = new System.Drawing.Size(23, 22);
      btnMovePrevious.Text = "Move previous";
      btnMovePrevious.MouseDown += NavButton_MouseDown;
      btnMovePrevious.MouseLeave += NavButton_MouseLeave;
      btnMovePrevious.MouseUp += NavButton_MouseUp;
      // 
      // m_ToolStripTextBoxPos
      // 
      toolStripTextBoxPosition.AccessibleName = "Position";
      toolStripTextBoxPosition.AutoSize = false;
      toolStripTextBoxPosition.MaxLength = 10;
      toolStripTextBoxPosition.Name = "toolStripTextBoxPosition";
      toolStripTextBoxPosition.Size = new System.Drawing.Size(40, 25);
      toolStripTextBoxPosition.Text = "0";
      toolStripTextBoxPosition.ToolTipText = "Current record";
      // 
      // toolStripButtonMoveNextItem
      // 
      btnMoveNext.DisplayStyle = ToolStripItemDisplayStyle.Image;
      btnMoveNext.Image = (System.Drawing.Image) resources.GetObject("btnMoveNext.Image");
      btnMoveNext.Name = "btnMoveNext";
      btnMoveNext.RightToLeftAutoMirrorImage = true;
      btnMoveNext.Size = new System.Drawing.Size(23, 22);
      btnMoveNext.Text = "Move next";
      btnMoveNext.MouseDown += NavButton_MouseDown;
      btnMoveNext.MouseLeave += NavButton_MouseLeave;
      btnMoveNext.MouseUp += NavButton_MouseUp;
      // 
      // toolStripButtonMoveLastItem
      // 
      btnMoveLast.DisplayStyle = ToolStripItemDisplayStyle.Image;
      btnMoveLast.Image = (System.Drawing.Image) resources.GetObject("btnMoveLast.Image");
      btnMoveLast.Name = "btnMoveLast";
      btnMoveLast.RightToLeftAutoMirrorImage = true;
      btnMoveLast.Size = new System.Drawing.Size(23, 22);
      btnMoveLast.Text = "Move last";
      btnMoveLast.Click += MoveLast_Click;
      // 
      // mainDataGridView
      // 
      mainDataGridView.AllowUserToAddRows = false;
      mainDataGridView.AllowUserToOrderColumns = true;
      dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
      mainDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
      mainDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;      
      dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Black;
      dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
      mainDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
      mainDataGridView.Dock = DockStyle.Fill;
      mainDataGridView.HighlightText = "";
      mainDataGridView.Location = new System.Drawing.Point(0, 0);
      mainDataGridView.Margin = new Padding(0);
      mainDataGridView.Name = "mainDataGridView";
      dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
      mainDataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
      mainDataGridView.RowHeadersWidth = 51;
      mainDataGridView.ShowButtonAtLength = 1000;
      mainDataGridView.Size = new System.Drawing.Size(719, 366);
      mainDataGridView.TabIndex = 2;
      mainDataGridView.VirtualMode = true;
      // 
      // toolStripButtonDuplicates
      // 
      toolStripButtonDuplicates.Image = (System.Drawing.Image) resources.GetObject("toolStripButtonDuplicates.Image");
      toolStripButtonDuplicates.Name = "toolStripButtonDuplicates";
      toolStripButtonDuplicates.Size = new System.Drawing.Size(86, 24);
      toolStripButtonDuplicates.Text = "Duplicates";
      toolStripButtonDuplicates.ToolTipText = "Display Duplicate Values";
      toolStripButtonDuplicates.Click += ButtonDuplicates_Click;
      // 
      // m_ToolStripButtonHierarchy
      // 
      toolStripButtonHierarchy.Image = (System.Drawing.Image) resources.GetObject("toolStripButtonHierarchy.Image");
      toolStripButtonHierarchy.Name = "toolStripButtonHierarchy";
      toolStripButtonHierarchy.Size = new System.Drawing.Size(82, 24);
      toolStripButtonHierarchy.Text = "Hierarchy";
      toolStripButtonHierarchy.ToolTipText = "Display a Hierarchy Structure";
      toolStripButtonHierarchy.Click += ButtonHierarchy_Click;
      // 
      // m_ToolStripButtonStore
      // 
      toolStripButtonStore.Image = (System.Drawing.Image) resources.GetObject("toolStripButtonStore.Image");
      toolStripButtonStore.ImageTransparentColor = System.Drawing.Color.Magenta;
      toolStripButtonStore.Name = "toolStripButtonStore";
      toolStripButtonStore.Size = new System.Drawing.Size(80, 24);
      toolStripButtonStore.Text = "Write File";
      toolStripButtonStore.ToolTipText = "Store the currently displayed data as delimited text file";
      toolStripButtonStore.Click += ToolStripButtonStoreAsCsvAsync;
      // 
      // m_ToolStripButtonUniqueValues
      // 
      toolStripButtonUniqueValues.Image = (System.Drawing.Image) resources.GetObject("toolStripButtonUniqueValues.Image");
      toolStripButtonUniqueValues.Name = "toolStripButtonUniqueValues";
      toolStripButtonUniqueValues.Size = new System.Drawing.Size(105, 24);
      toolStripButtonUniqueValues.Text = "Unique Values";
      toolStripButtonUniqueValues.ToolTipText = "Display Unique Values";
      toolStripButtonUniqueValues.Click += ButtonUniqueValues_Click;
      // 
      // m_ToolStripComboBoxFilterType
      // 
      toolStripComboBoxFilterType.DropDownHeight = 90;
      toolStripComboBoxFilterType.DropDownWidth = 130;
      toolStripComboBoxFilterType.Enabled = false;
      toolStripComboBoxFilterType.IntegralHeight = false;
      toolStripComboBoxFilterType.Items.AddRange(new object[] { "All Records" });
      toolStripComboBoxFilterType.Name = "toolStripComboBoxFilterType";
      toolStripComboBoxFilterType.Size = new System.Drawing.Size(150, 27);
      toolStripComboBoxFilterType.Text = "All Records";
      toolStripComboBoxFilterType.Visible = false;
      toolStripComboBoxFilterType.SelectedIndexChanged += ToolStripComboBoxFilterType_SelectedIndexChanged;
      // 
      // m_ToolStripContainer
      // 
      // 
      // m_ToolStripContainer.BottomToolStripPanel
      // 
      toolStripContainer.BottomToolStripPanel.Controls.Add(toolStripNavigation);
      // 
      // m_ToolStripContainer.ContentPanel
      // 
      toolStripContainer.ContentPanel.Controls.Add(searchControl);
      toolStripContainer.ContentPanel.Controls.Add(mainDataGridView);
      toolStripContainer.ContentPanel.Size = new System.Drawing.Size(719, 366);
      toolStripContainer.Dock = DockStyle.Fill;
      toolStripContainer.LeftToolStripPanelVisible = false;
      toolStripContainer.Location = new System.Drawing.Point(0, 0);
      toolStripContainer.Name = "toolStripContainer";
      toolStripContainer.RightToolStripPanelVisible = false;
      toolStripContainer.Size = new System.Drawing.Size(719, 418);
      toolStripContainer.TabIndex = 13;
      toolStripContainer.Text = "toolStripContainer";
      // 
      // m_ToolStripContainer.TopToolStripPanel
      // 
      toolStripContainer.TopToolStripPanel.Controls.Add(toolStripTop);
      // 
      // m_ToolStripNavigation
      // 
      toolStripNavigation.Dock = DockStyle.None;
      toolStripNavigation.GripStyle = ToolStripGripStyle.Hidden;
      toolStripNavigation.Items.AddRange(new ToolStripItem[] { btnMoveFirst, btnMovePrevious, toolStripTextBoxPosition, toolStripLabelCount, toolStripTextBoxTotal, btnMoveNext, btnMoveLast });
      toolStripNavigation.Location = new System.Drawing.Point(6, 0);
      toolStripNavigation.Name = "toolStripNavigation";
      toolStripNavigation.Size = new System.Drawing.Size(228, 25);
      toolStripNavigation.TabIndex = 1;
      // 
      // m_ToolStripTextBoxTotal
      // 
      toolStripTextBoxTotal.AccessibleName = "Total";
      toolStripTextBoxTotal.AutoSize = false;
      toolStripTextBoxTotal.BackColor = System.Drawing.SystemColors.Control;
      toolStripTextBoxTotal.ForeColor = System.Drawing.SystemColors.Highlight;
      toolStripTextBoxTotal.MaxLength = 10;
      toolStripTextBoxTotal.Name = "toolStripTextBoxTotal";
      toolStripTextBoxTotal.Size = new System.Drawing.Size(40, 25);
      toolStripTextBoxTotal.Text = "200";
      toolStripTextBoxTotal.TextChanged += ToolStripTextBoxTotal_TextChanged;
      // 
      // m_Search
      // 
      searchControl.Anchor =  AnchorStyles.Top | AnchorStyles.Right;
      searchControl.AutoSize = true;
      searchControl.BackColor = System.Drawing.SystemColors.ControlLightLight;
      searchControl.BorderStyle = BorderStyle.FixedSingle;
      searchControl.Location = new System.Drawing.Point(512, 0);
      searchControl.MinimumSize = new System.Drawing.Size(200, 25);
      searchControl.Name = "searchControl";
      searchControl.SearchText = "";
      searchControl.Size = new System.Drawing.Size(207, 27);
      searchControl.TabIndex = 1;
      searchControl.Visible = false;
      searchControl.OnSearchClear += OnSearchClear;
      searchControl.OnSearchNext += OnSearchNext;
      searchControl.OnSearchPrev += OnSearchPrev;
      // 
      // m_ToolStripTop
      // 
      toolStripTop.Dock = DockStyle.None;
      toolStripTop.GripStyle = ToolStripGripStyle.Hidden;
      toolStripTop.ImageScalingSize = new System.Drawing.Size(20, 20);
      toolStripTop.Items.AddRange(new ToolStripItem[] { toolStripComboBoxFilterType, toolStripButtonUniqueValues, toolStripButtonColumnLength, toolStripButtonDuplicates, toolStripButtonHierarchy, toolStripButtonSource, toolStripButtonStore });
      toolStripTop.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
      toolStripTop.Location = new System.Drawing.Point(3, 0);
      toolStripTop.Name = "toolStripTop";
      toolStripTop.Size = new System.Drawing.Size(502, 27);
      toolStripTop.TabIndex = 1;
      // 
      // m_ToolStripButtonColumnLength
      // 
      toolStripButtonColumnLength.Image = (System.Drawing.Image) resources.GetObject("toolStripButtonColumnLength.Image");
      toolStripButtonColumnLength.Name = "toolStripButtonColumnLength";
      toolStripButtonColumnLength.Size = new System.Drawing.Size(79, 24);
      toolStripButtonColumnLength.Text = "Columns";
      toolStripButtonColumnLength.ToolTipText = "Display Schema information";
      toolStripButtonColumnLength.Click += ButtonColumnLength_Click;
      // 
      // m_ToolStripButtonSource
      // 
      toolStripButtonSource.Image = (System.Drawing.Image) resources.GetObject("toolStripButtonSource.Image");
      toolStripButtonSource.ImageTransparentColor = System.Drawing.Color.Magenta;
      toolStripButtonSource.Name = "toolStripButtonSource";
      toolStripButtonSource.Size = new System.Drawing.Size(67, 24);
      toolStripButtonSource.Text = "Source";
      toolStripButtonSource.Click += DisplaySource_Click;
      // 
      // m_TimerVisibility
      // 
      timerVisibility.Enabled = true;
      timerVisibility.Interval = 150;
      timerVisibility.Tick += TimerVisibility_Tick;
      // 
      // timerLoadRemain
      // 
      timerLoadRemain.Interval = 500;
      timerLoadRemain.Tick += TimerLoadRemain_Tick;
      // 
      // m_NavRepeatTimer
      // 
      navRepeatTimer.Tick += NavRepeatTimer_Tick;
      // 
      // m_NavInputTimer
      // 
      navInputTimer.Enabled = true;
      // 
      // DetailControl
      // 
      Controls.Add(toolStripContainer);
      Margin = new Padding(0);
      Name = "DetailControl";
      Size = new System.Drawing.Size(719, 418);
      FontChanged += DetailControl_FontChanged;
      KeyDown += DetailControl_KeyDown;
      ParentChanged += DetailControl_ParentChanged;
      ((ISupportInitialize) mainDataGridView).EndInit();
      toolStripContainer.BottomToolStripPanel.ResumeLayout(false);
      toolStripContainer.BottomToolStripPanel.PerformLayout();
      toolStripContainer.ContentPanel.ResumeLayout(false);
      toolStripContainer.ContentPanel.PerformLayout();
      toolStripContainer.TopToolStripPanel.ResumeLayout(false);
      toolStripContainer.TopToolStripPanel.PerformLayout();
      toolStripContainer.ResumeLayout(false);
      toolStripContainer.PerformLayout();
      toolStripNavigation.ResumeLayout(false);
      toolStripNavigation.PerformLayout();
      toolStripTop.ResumeLayout(false);
      toolStripTop.PerformLayout();
      ResumeLayout(false);

    }


    private FilteredDataGridView mainDataGridView;
    private ToolStripButton toolStripButtonSource;
    private Timer timerLoadRemain;
    private ToolStrip toolStripNavigation;
    private ToolStripTextBox toolStripTextBoxTotal;
    private Timer navRepeatTimer;
    private Timer navInputTimer;
  }
}
