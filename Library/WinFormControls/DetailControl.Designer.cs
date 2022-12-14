using System.ComponentModel;
using System.Windows.Forms;

namespace CsvTools
{

  public partial class DetailControl
  {
    private IContainer components;
    private BindingNavigator m_BindingNavigator;
    private BindingSource m_BindingSource;
    private ToolStripButton m_ToolStripButtonColumnLength;
    private ToolStripButton m_ToolStripButtonDuplicates;
    private ToolStripButton m_ToolStripButtonHierarchy;
    private ToolStripButton toolStripButtonMoveFirstItem;
    private ToolStripButton toolStripButtonMoveLastItem;
    private ToolStripButton toolStripButtonMoveNextItem;
    private ToolStripButton toolStripButtonMovePreviousItem;
    private ToolStripButton m_ToolStripButtonStore;
    private ToolStripButton m_ToolStripButtonUniqueValues;
    private ToolStripButton m_ToolStripButtonLoadRemaining;
    private ToolStripComboBox m_ToolStripComboBoxFilterType;
    private ToolStripContainer m_ToolStripContainer;
    private ToolStripLabel m_ToolStripLabelCount;
    private ToolStripTextBox m_ToolStripTextBoxPos;
    private ToolStrip m_ToolStripTop;
    private Search m_Search;
    private System.Windows.Forms.Timer m_TimerVisibility;

    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DetailControl));
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
      this.m_BindingNavigator = new System.Windows.Forms.BindingNavigator(this.components);
      this.m_BindingSource = new System.Windows.Forms.BindingSource(this.components);
      this.m_ToolStripLabelCount = new System.Windows.Forms.ToolStripLabel();
      this.toolStripButtonMoveFirstItem = new System.Windows.Forms.ToolStripButton();
      this.toolStripButtonMovePreviousItem = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripTextBoxPos = new System.Windows.Forms.ToolStripTextBox();
      this.toolStripButtonMoveNextItem = new System.Windows.Forms.ToolStripButton();
      this.toolStripButtonMoveLastItem = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripButtonLoadRemaining = new System.Windows.Forms.ToolStripButton();
      this.FilteredDataGridView = new CsvTools.FilteredDataGridView();
      this.m_ToolStripButtonDuplicates = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripButtonHierarchy = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripButtonStore = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripButtonUniqueValues = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripComboBoxFilterType = new System.Windows.Forms.ToolStripComboBox();
      this.m_ToolStripContainer = new System.Windows.Forms.ToolStripContainer();
      this.m_Search = new CsvTools.Search();
      this.m_ToolStripTop = new System.Windows.Forms.ToolStrip();
      this.m_ToolStripButtonColumnLength = new System.Windows.Forms.ToolStripButton();
      this.m_TimerVisibility = new System.Windows.Forms.Timer(this.components);
      this.searchBackgroundWorker = new System.ComponentModel.BackgroundWorker();
      ((System.ComponentModel.ISupportInitialize)(this.m_BindingNavigator)).BeginInit();
      this.m_BindingNavigator.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.m_BindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.FilteredDataGridView)).BeginInit();
      this.m_ToolStripContainer.BottomToolStripPanel.SuspendLayout();
      this.m_ToolStripContainer.ContentPanel.SuspendLayout();
      this.m_ToolStripContainer.TopToolStripPanel.SuspendLayout();
      this.m_ToolStripContainer.SuspendLayout();
      this.m_ToolStripTop.SuspendLayout();
      this.SuspendLayout();
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
            this.toolStripButtonMoveFirstItem,
            this.toolStripButtonMovePreviousItem,
            this.m_ToolStripTextBoxPos,
            this.m_ToolStripLabelCount,
            this.toolStripButtonMoveNextItem,
            this.toolStripButtonMoveLastItem,
            this.m_ToolStripButtonLoadRemaining});
      this.m_BindingNavigator.Location = new System.Drawing.Point(4, 0);
      this.m_BindingNavigator.MoveFirstItem = this.toolStripButtonMoveFirstItem;
      this.m_BindingNavigator.MoveLastItem = this.toolStripButtonMoveLastItem;
      this.m_BindingNavigator.MoveNextItem = this.toolStripButtonMoveNextItem;
      this.m_BindingNavigator.MovePreviousItem = this.toolStripButtonMovePreviousItem;
      this.m_BindingNavigator.Name = "m_BindingNavigator";
      this.m_BindingNavigator.PositionItem = this.m_ToolStripTextBoxPos;
      this.m_BindingNavigator.Size = new System.Drawing.Size(210, 27);
      this.m_BindingNavigator.TabIndex = 0;
      // 
      // m_ToolStripLabelCount
      // 
      this.m_ToolStripLabelCount.Name = "m_ToolStripLabelCount";
      this.m_ToolStripLabelCount.Size = new System.Drawing.Size(35, 24);
      this.m_ToolStripLabelCount.Text = "of {0}";
      this.m_ToolStripLabelCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.m_ToolStripLabelCount.ToolTipText = "Total number of items";
      // 
      // toolStripButtonMoveFirstItem
      // 
      this.toolStripButtonMoveFirstItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.toolStripButtonMoveFirstItem.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonMoveFirstItem.Image")));
      this.toolStripButtonMoveFirstItem.Name = "toolStripButtonMoveFirstItem";
      this.toolStripButtonMoveFirstItem.RightToLeftAutoMirrorImage = true;
      this.toolStripButtonMoveFirstItem.Size = new System.Drawing.Size(24, 24);
      this.toolStripButtonMoveFirstItem.Text = "Move first";
      // 
      // toolStripButtonMovePreviousItem
      // 
      this.toolStripButtonMovePreviousItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.toolStripButtonMovePreviousItem.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonMovePreviousItem.Image")));
      this.toolStripButtonMovePreviousItem.Name = "toolStripButtonMovePreviousItem";
      this.toolStripButtonMovePreviousItem.RightToLeftAutoMirrorImage = true;
      this.toolStripButtonMovePreviousItem.Size = new System.Drawing.Size(24, 24);
      this.toolStripButtonMovePreviousItem.Text = "Move previous";
      // 
      // m_ToolStripTextBoxPos
      // 
      this.m_ToolStripTextBoxPos.AccessibleName = "Position";
      this.m_ToolStripTextBoxPos.Name = "m_ToolStripTextBoxPos";
      this.m_ToolStripTextBoxPos.Size = new System.Drawing.Size(50, 27);
      this.m_ToolStripTextBoxPos.Text = "0";
      this.m_ToolStripTextBoxPos.ToolTipText = "Current position";
      // 
      // toolStripButtonMoveNextItem
      // 
      this.toolStripButtonMoveNextItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.toolStripButtonMoveNextItem.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonMoveNextItem.Image")));
      this.toolStripButtonMoveNextItem.Name = "toolStripButtonMoveNextItem";
      this.toolStripButtonMoveNextItem.RightToLeftAutoMirrorImage = true;
      this.toolStripButtonMoveNextItem.Size = new System.Drawing.Size(24, 24);
      this.toolStripButtonMoveNextItem.Text = "Move next";
      // 
      // toolStripButtonMoveLastItem
      // 
      this.toolStripButtonMoveLastItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.toolStripButtonMoveLastItem.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButtonMoveLastItem.Image")));
      this.toolStripButtonMoveLastItem.Name = "toolStripButtonMoveLastItem";
      this.toolStripButtonMoveLastItem.RightToLeftAutoMirrorImage = true;
      this.toolStripButtonMoveLastItem.Size = new System.Drawing.Size(24, 24);
      this.toolStripButtonMoveLastItem.Text = "Move last";
      // 
      // m_ToolStripButtonLoadRemaining
      // 
      this.m_ToolStripButtonLoadRemaining.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.m_ToolStripButtonLoadRemaining.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonLoadRemaining.Image")));
      this.m_ToolStripButtonLoadRemaining.Name = "m_ToolStripButtonLoadRemaining";
      this.m_ToolStripButtonLoadRemaining.Size = new System.Drawing.Size(24, 24);
      this.m_ToolStripButtonLoadRemaining.Text = "Load More...";
      this.m_ToolStripButtonLoadRemaining.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
      this.m_ToolStripButtonLoadRemaining.ToolTipText = "File is not read completely, load another set of records";
      this.m_ToolStripButtonLoadRemaining.Click += new System.EventHandler(this.ToolStripButtonLoadRemaining_Click);
      // 
      // FilteredDataGridView
      // 
      this.FilteredDataGridView.AllowUserToOrderColumns = true;
      dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.FilteredDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
      this.FilteredDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
      dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Black;
      dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.FilteredDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
      this.FilteredDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.FilteredDataGridView.HighlightText = "";
      this.FilteredDataGridView.Location = new System.Drawing.Point(0, 0);
      this.FilteredDataGridView.Margin = new System.Windows.Forms.Padding(2);
      this.FilteredDataGridView.Name = "FilteredDataGridView";
      dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.FilteredDataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
      this.FilteredDataGridView.RowHeadersWidth = 51;
      this.FilteredDataGridView.Size = new System.Drawing.Size(996, 321);
      this.FilteredDataGridView.TabIndex = 2;
      this.FilteredDataGridView.DataViewChanged += new System.EventHandler(this.DataViewChanged);
      // 
      // m_ToolStripButtonDuplicates
      // 
      this.m_ToolStripButtonDuplicates.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonDuplicates.Image")));
      this.m_ToolStripButtonDuplicates.Name = "m_ToolStripButtonDuplicates";
      this.m_ToolStripButtonDuplicates.Size = new System.Drawing.Size(86, 24);
      this.m_ToolStripButtonDuplicates.Text = "Duplicates";
      this.m_ToolStripButtonDuplicates.ToolTipText = "Display Duplicate Values";
      this.m_ToolStripButtonDuplicates.Click += new System.EventHandler(this.ButtonDuplicates_Click);
      // 
      // m_ToolStripButtonHierarchy
      // 
      this.m_ToolStripButtonHierarchy.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonHierarchy.Image")));
      this.m_ToolStripButtonHierarchy.Name = "m_ToolStripButtonHierarchy";
      this.m_ToolStripButtonHierarchy.Size = new System.Drawing.Size(82, 24);
      this.m_ToolStripButtonHierarchy.Text = "Hierarchy";
      this.m_ToolStripButtonHierarchy.ToolTipText = "Display a Hierarchy Structure";
      this.m_ToolStripButtonHierarchy.Click += new System.EventHandler(this.ButtonHierarchy_Click);
      // 
      // m_ToolStripButtonStore
      // 
      this.m_ToolStripButtonStore.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonStore.Image")));
      this.m_ToolStripButtonStore.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.m_ToolStripButtonStore.Name = "m_ToolStripButtonStore";
      this.m_ToolStripButtonStore.Size = new System.Drawing.Size(80, 24);
      this.m_ToolStripButtonStore.Text = "Write File";
      this.m_ToolStripButtonStore.ToolTipText = "Store the currently displayed data as delimited text file";
      this.m_ToolStripButtonStore.Click += new System.EventHandler(this.ToolStripButtonStoreAsCsvAsync);
      // 
      // m_ToolStripButtonUniqueValues
      // 
      this.m_ToolStripButtonUniqueValues.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonUniqueValues.Image")));
      this.m_ToolStripButtonUniqueValues.Name = "m_ToolStripButtonUniqueValues";
      this.m_ToolStripButtonUniqueValues.Size = new System.Drawing.Size(105, 24);
      this.m_ToolStripButtonUniqueValues.Text = "Unique Values";
      this.m_ToolStripButtonUniqueValues.ToolTipText = "Display Unique Values";
      this.m_ToolStripButtonUniqueValues.Click += new System.EventHandler(this.ButtonUniqueValues_Click);
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
      this.m_ToolStripComboBoxFilterType.Size = new System.Drawing.Size(150, 27);
      this.m_ToolStripComboBoxFilterType.Text = "All Records";
      this.m_ToolStripComboBoxFilterType.SelectedIndexChanged += new System.EventHandler(this.ToolStripComboBoxFilterType_SelectedIndexChanged);
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
      this.m_ToolStripContainer.ContentPanel.Controls.Add(this.FilteredDataGridView);
      this.m_ToolStripContainer.ContentPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.m_ToolStripContainer.ContentPanel.Size = new System.Drawing.Size(996, 321);
      this.m_ToolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_ToolStripContainer.LeftToolStripPanelVisible = false;
      this.m_ToolStripContainer.Location = new System.Drawing.Point(0, 0);
      this.m_ToolStripContainer.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.m_ToolStripContainer.Name = "m_ToolStripContainer";
      this.m_ToolStripContainer.RightToolStripPanelVisible = false;
      this.m_ToolStripContainer.Size = new System.Drawing.Size(996, 375);
      this.m_ToolStripContainer.TabIndex = 13;
      this.m_ToolStripContainer.Text = "toolStripContainer";
      // 
      // m_ToolStripContainer.TopToolStripPanel
      // 
      this.m_ToolStripContainer.TopToolStripPanel.Controls.Add(this.m_ToolStripTop);
      // 
      // m_Search
      // 
      this.m_Search.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.m_Search.AutoSize = true;
      this.m_Search.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.m_Search.Location = new System.Drawing.Point(643, 3);
      this.m_Search.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.m_Search.Name = "m_Search";
      this.m_Search.Results = 0;
      this.m_Search.Size = new System.Drawing.Size(354, 27);
      this.m_Search.TabIndex = 1;
      this.m_Search.Visible = false;
      this.m_Search.OnResultChanged += new System.EventHandler<CsvTools.SearchEventArgs>(this.OnSearchResultChanged);
      this.m_Search.OnSearchChanged += new System.EventHandler<CsvTools.SearchEventArgs>(this.OnSearchChanged);
      this.m_Search.OnSearchClear += new System.EventHandler(this.OnSearchClear);
      // 
      // m_ToolStripTop
      // 
      this.m_ToolStripTop.Dock = System.Windows.Forms.DockStyle.None;
      this.m_ToolStripTop.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
      this.m_ToolStripTop.ImageScalingSize = new System.Drawing.Size(20, 20);
      this.m_ToolStripTop.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ToolStripComboBoxFilterType,
            this.m_ToolStripButtonUniqueValues,
            this.m_ToolStripButtonColumnLength,
            this.m_ToolStripButtonDuplicates,
            this.m_ToolStripButtonHierarchy,
            this.m_ToolStripButtonStore});
      this.m_ToolStripTop.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
      this.m_ToolStripTop.Location = new System.Drawing.Point(3, 0);
      this.m_ToolStripTop.Name = "m_ToolStripTop";
      this.m_ToolStripTop.Size = new System.Drawing.Size(587, 27);
      this.m_ToolStripTop.TabIndex = 1;
      // 
      // m_ToolStripButtonColumnLength
      // 
      this.m_ToolStripButtonColumnLength.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonColumnLength.Image")));
      this.m_ToolStripButtonColumnLength.Name = "m_ToolStripButtonColumnLength";
      this.m_ToolStripButtonColumnLength.Size = new System.Drawing.Size(79, 24);
      this.m_ToolStripButtonColumnLength.Text = "Columns";
      this.m_ToolStripButtonColumnLength.ToolTipText = "Display Schema information";
      this.m_ToolStripButtonColumnLength.Click += new System.EventHandler(this.ButtonColumnLength_Click);
      // 
      // m_TimerVisibility
      // 
      this.m_TimerVisibility.Enabled = true;
      this.m_TimerVisibility.Interval = 150;
      this.m_TimerVisibility.Tick += new System.EventHandler(this.TimerVisibility_Tick);
      // 
      // searchBackgroundWorker
      // 
      this.searchBackgroundWorker.WorkerSupportsCancellation = true;
      this.searchBackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.searchBackgroundWorker_DoWork);
      this.searchBackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.searchBackgroundWorker_RunWorkerCompleted);
      // 
      // DetailControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.m_ToolStripContainer);
      this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.Name = "DetailControl";
      this.Size = new System.Drawing.Size(996, 375);
      this.FontChanged += new System.EventHandler(this.DetailControl_FontChanged);
      this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DetailControl_KeyDown);
      this.ParentChanged += new System.EventHandler(this.DetailControl_ParentChanged);
      ((System.ComponentModel.ISupportInitialize)(this.m_BindingNavigator)).EndInit();
      this.m_BindingNavigator.ResumeLayout(false);
      this.m_BindingNavigator.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.m_BindingSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.FilteredDataGridView)).EndInit();
      this.m_ToolStripContainer.BottomToolStripPanel.ResumeLayout(false);
      this.m_ToolStripContainer.BottomToolStripPanel.PerformLayout();
      this.m_ToolStripContainer.ContentPanel.ResumeLayout(false);
      this.m_ToolStripContainer.ContentPanel.PerformLayout();
      this.m_ToolStripContainer.TopToolStripPanel.ResumeLayout(false);
      this.m_ToolStripContainer.TopToolStripPanel.PerformLayout();
      this.m_ToolStripContainer.ResumeLayout(false);
      this.m_ToolStripContainer.PerformLayout();
      this.m_ToolStripTop.ResumeLayout(false);
      this.m_ToolStripTop.PerformLayout();
      this.ResumeLayout(false);

    }

    private FilteredDataGridView FilteredDataGridView;
    private BackgroundWorker searchBackgroundWorker;
  }
}
