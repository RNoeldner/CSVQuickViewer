
namespace CsvTools
{
  partial class FilteredDataGridView
  {

    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Initializes the component.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FilteredDataGridView));
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
      this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
      this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
      this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
      this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
      this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
      this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
      this.contextMenuStripCell = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.toolStripMenuItemCopyError = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemCopy = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
      this.contextMenuStripFilter = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.toolStripMenuItemApplyFilter = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemFilterAdd = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemFilterThisValue = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemFilterRemove = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
      this.contextMenuStripColumns = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.toolStripMenuItemShowAllColumns = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemHideAllColumns = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemFilled = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemColumns = new System.Windows.Forms.ToolStripMenuItem();
      this.contextMenuStripHeader = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.toolStripMenuItemCF = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparatorCF = new System.Windows.Forms.ToolStripSeparator();
      this.toolStripMenuItemRemoveOne = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemFilterRemoveAllFilter = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemSortAscending = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemSortDescending = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemSortRemove = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemFreeze = new System.Windows.Forms.ToolStripMenuItem();
      this.timerColumsFilter = new System.Windows.Forms.Timer(this.components);
      this.toolStripMenuItemColumnVisibility = new CsvTools.ToolStripCheckedListBox();
      this.contextMenuStripCell.SuspendLayout();
      this.contextMenuStripFilter.SuspendLayout();
      this.contextMenuStripColumns.SuspendLayout();
      this.contextMenuStripHeader.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
      this.SuspendLayout();
      // 
      // toolStripSeparator4
      // 
      this.toolStripSeparator4.Name = "toolStripSeparator4";
      this.toolStripSeparator4.Size = new System.Drawing.Size(98, 6);
      // 
      // toolStripSeparator2
      // 
      this.toolStripSeparator2.Name = "toolStripSeparator2";
      this.toolStripSeparator2.Size = new System.Drawing.Size(234, 6);
      // 
      // toolStripSeparator3
      // 
      this.toolStripSeparator3.Name = "toolStripSeparator3";
      this.toolStripSeparator3.Size = new System.Drawing.Size(234, 6);
      // 
      // toolStripSeparator6
      // 
      this.toolStripSeparator6.Name = "toolStripSeparator6";
      this.toolStripSeparator6.Size = new System.Drawing.Size(167, 6);
      // 
      // toolStripSeparator1
      // 
      this.toolStripSeparator1.Name = "toolStripSeparator1";
      this.toolStripSeparator1.Size = new System.Drawing.Size(287, 6);
      // 
      // toolStripSeparator7
      // 
      this.toolStripSeparator7.Name = "toolStripSeparator7";
      this.toolStripSeparator7.Size = new System.Drawing.Size(287, 6);
      // 
      // contextMenuStripCell
      // 
      this.contextMenuStripCell.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemCopyError,
            this.toolStripMenuItemCopy,
            this.toolStripSeparator1,
            this.toolStripMenuItem2,
            this.toolStripMenuItemFilterThisValue,
            this.toolStripMenuItemFilterRemove,
            this.toolStripSeparator7,
            this.toolStripMenuItem1});
      this.contextMenuStripCell.Name = "contextMenuStripDropDownCopy";
      this.contextMenuStripCell.Size = new System.Drawing.Size(291, 148);
      // 
      // toolStripMenuItemCopyError
      // 
      this.toolStripMenuItemCopyError.Name = "toolStripMenuItemCopyError";
      this.toolStripMenuItemCopyError.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
      this.toolStripMenuItemCopyError.Size = new System.Drawing.Size(290, 22);
      this.toolStripMenuItemCopyError.Text = "Copy";
      this.toolStripMenuItemCopyError.Click += new System.EventHandler(this.ToolStripMenuItemCopyError_Click);
      // 
      // toolStripMenuItemCopy
      // 
      this.toolStripMenuItemCopy.Name = "toolStripMenuItemCopy";
      this.toolStripMenuItemCopy.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.C)));
      this.toolStripMenuItemCopy.Size = new System.Drawing.Size(290, 22);
      this.toolStripMenuItemCopy.Text = "Copy (without error information)";
      this.toolStripMenuItemCopy.Click += new System.EventHandler(this.ToolStripMenuItemCopy_Click);
      // 
      // toolStripMenuItem2
      // 
      this.toolStripMenuItem2.DropDown = this.contextMenuStripFilter;
      this.toolStripMenuItem2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItem2.Image")));
      this.toolStripMenuItem2.Name = "toolStripMenuItem2";
      this.toolStripMenuItem2.Size = new System.Drawing.Size(290, 22);
      this.toolStripMenuItem2.Text = "Filter";
      // 
      // contextMenuStripFilter
      // 
      this.contextMenuStripFilter.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator4,
            this.toolStripMenuItemApplyFilter});
      this.contextMenuStripFilter.Name = "contextMenuStripFilter";
      this.contextMenuStripFilter.OwnerItem = this.toolStripMenuItemFilterAdd;
      this.contextMenuStripFilter.Size = new System.Drawing.Size(102, 32);
      this.contextMenuStripFilter.Text = "contextMenuStripFilter";
      // 
      // toolStripMenuItemApplyFilter
      // 
      this.toolStripMenuItemApplyFilter.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemApplyFilter.Image")));
      this.toolStripMenuItemApplyFilter.Name = "toolStripMenuItemApplyFilter";
      this.toolStripMenuItemApplyFilter.Size = new System.Drawing.Size(101, 22);
      this.toolStripMenuItemApplyFilter.Text = "&Apply";
      this.toolStripMenuItemApplyFilter.Click += new System.EventHandler(this.ToolStripMenuItemApply_Click);
      // 
      // toolStripMenuItemFilterAdd
      // 
      this.toolStripMenuItemFilterAdd.DropDown = this.contextMenuStripFilter;
      this.toolStripMenuItemFilterAdd.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemFilterAdd.Image")));
      this.toolStripMenuItemFilterAdd.Name = "toolStripMenuItemFilterAdd";
      this.toolStripMenuItemFilterAdd.Size = new System.Drawing.Size(237, 22);
      this.toolStripMenuItemFilterAdd.Text = "Filter";
      // 
      // toolStripMenuItemFilterThisValue
      // 
      this.toolStripMenuItemFilterThisValue.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemFilterThisValue.Image")));
      this.toolStripMenuItemFilterThisValue.Name = "toolStripMenuItemFilterThisValue";
      this.toolStripMenuItemFilterThisValue.Size = new System.Drawing.Size(290, 22);
      this.toolStripMenuItemFilterThisValue.Text = "Filter for this value";
      this.toolStripMenuItemFilterThisValue.Click += new System.EventHandler(this.ToolStripMenuItemFilterValue_Click);
      // 
      // toolStripMenuItemFilterRemove
      // 
      this.toolStripMenuItemFilterRemove.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemFilterRemove.Image")));
      this.toolStripMenuItemFilterRemove.Name = "toolStripMenuItemFilterRemove";
      this.toolStripMenuItemFilterRemove.Size = new System.Drawing.Size(290, 22);
      this.toolStripMenuItemFilterRemove.Text = "Remove all Filter";
      this.toolStripMenuItemFilterRemove.Click += new System.EventHandler(this.ToolStripMenuItemFilterRemoveAll_Click);
      // 
      // toolStripMenuItem1
      // 
      this.toolStripMenuItem1.DropDown = this.contextMenuStripColumns;
      this.toolStripMenuItem1.Name = "toolStripMenuItem1";
      this.toolStripMenuItem1.Size = new System.Drawing.Size(290, 22);
      this.toolStripMenuItem1.Text = "Columns";
      // 
      // contextMenuStripColumns
      // 
      this.contextMenuStripColumns.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemShowAllColumns,
            this.toolStripMenuItemHideAllColumns,
            this.toolStripMenuItemFilled,
            this.toolStripMenuItemColumnVisibility});
      this.contextMenuStripColumns.Name = "contextMenuStripColumns";
      this.contextMenuStripColumns.OwnerItem = this.toolStripMenuItem1;
      this.contextMenuStripColumns.ShowImageMargin = false;
      this.contextMenuStripColumns.Size = new System.Drawing.Size(147, 96);
      // 
      // toolStripMenuItemShowAllColumns
      // 
      this.toolStripMenuItemShowAllColumns.Name = "toolStripMenuItemShowAllColumns";
      this.toolStripMenuItemShowAllColumns.Size = new System.Drawing.Size(146, 22);
      this.toolStripMenuItemShowAllColumns.Text = "Show All Columns";
      this.toolStripMenuItemShowAllColumns.Click += new System.EventHandler(this.ToolStripMenuItemShowAllColumns_Click);
      // 
      // toolStripMenuItemHideAllColumns
      // 
      this.toolStripMenuItemHideAllColumns.Name = "toolStripMenuItemHideAllColumns";
      this.toolStripMenuItemHideAllColumns.Size = new System.Drawing.Size(146, 22);
      this.toolStripMenuItemHideAllColumns.Text = "Hide Other Columns";
      this.toolStripMenuItemHideAllColumns.Click += new System.EventHandler(this.ToolStripMenuItemHideAllColumns_Click);
      // 
      // toolStripMenuItemFilled
      // 
      this.toolStripMenuItemFilled.Name = "toolStripMenuItemFilled";
      this.toolStripMenuItemFilled.Size = new System.Drawing.Size(146, 22);
      this.toolStripMenuItemFilled.Text = "Hide Empty Columns";
      this.toolStripMenuItemFilled.Click += new System.EventHandler(this.ToolStripMenuItemFilled_Click);
      // 
      // toolStripMenuItemColumns
      // 
      this.toolStripMenuItemColumns.DropDown = this.contextMenuStripColumns;
      this.toolStripMenuItemColumns.Name = "toolStripMenuItemColumns";
      this.toolStripMenuItemColumns.Size = new System.Drawing.Size(237, 22);
      this.toolStripMenuItemColumns.Text = "Columns";
      // 
      // contextMenuStripHeader
      // 
      this.contextMenuStripHeader.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemCF,
            this.toolStripSeparatorCF,
            this.toolStripMenuItemFilterAdd,
            this.toolStripMenuItemRemoveOne,
            this.toolStripMenuItemFilterRemoveAllFilter,
            this.toolStripSeparator2,
            this.toolStripMenuItemSortAscending,
            this.toolStripMenuItemSortDescending,
            this.toolStripMenuItemSortRemove,
            this.toolStripSeparator3,
            this.toolStripMenuItemColumns,
            this.toolStripMenuItemFreeze});
      this.contextMenuStripHeader.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Table;
      this.contextMenuStripHeader.Name = "contextMenuStripHeader";
      this.contextMenuStripHeader.Size = new System.Drawing.Size(238, 220);
      // 
      // toolStripMenuItemCF
      // 
      this.toolStripMenuItemCF.Name = "toolStripMenuItemCF";
      this.toolStripMenuItemCF.Size = new System.Drawing.Size(237, 22);
      this.toolStripMenuItemCF.Text = "Change Format";
      this.toolStripMenuItemCF.Visible = false;
      this.toolStripMenuItemCF.Click += new System.EventHandler(this.ToolStripMenuItemCF_Click);
      // 
      // toolStripSeparatorCF
      // 
      this.toolStripSeparatorCF.Name = "toolStripSeparatorCF";
      this.toolStripSeparatorCF.Size = new System.Drawing.Size(234, 6);
      this.toolStripSeparatorCF.Visible = false;
      // 
      // toolStripMenuItemRemoveOne
      // 
      this.toolStripMenuItemRemoveOne.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemRemoveOne.Image")));
      this.toolStripMenuItemRemoveOne.Name = "toolStripMenuItemRemoveOne";
      this.toolStripMenuItemRemoveOne.ShortcutKeys = System.Windows.Forms.Keys.Delete;
      this.toolStripMenuItemRemoveOne.Size = new System.Drawing.Size(237, 22);
      this.toolStripMenuItemRemoveOne.Text = "Remove Filter";
      this.toolStripMenuItemRemoveOne.Click += new System.EventHandler(this.ToolStripMenuItemFilterRemoveOne_Click);
      // 
      // toolStripMenuItemFilterRemoveAllFilter
      // 
      this.toolStripMenuItemFilterRemoveAllFilter.Enabled = false;
      this.toolStripMenuItemFilterRemoveAllFilter.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemFilterRemoveAllFilter.Image")));
      this.toolStripMenuItemFilterRemoveAllFilter.Name = "toolStripMenuItemFilterRemoveAllFilter";
      this.toolStripMenuItemFilterRemoveAllFilter.Size = new System.Drawing.Size(237, 22);
      this.toolStripMenuItemFilterRemoveAllFilter.Text = "Remove all Filter";
      this.toolStripMenuItemFilterRemoveAllFilter.Click += new System.EventHandler(this.ToolStripMenuItemFilterRemoveAll_Click);
      // 
      // toolStripMenuItemSortAscending
      // 
      this.toolStripMenuItemSortAscending.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemSortAscending.Image")));
      this.toolStripMenuItemSortAscending.Name = "toolStripMenuItemSortAscending";
      this.toolStripMenuItemSortAscending.Size = new System.Drawing.Size(237, 22);
      this.toolStripMenuItemSortAscending.Tag = "Sort ascending by \'{0}\'";
      this.toolStripMenuItemSortAscending.Text = "Sort ascending by \'Column name\'";
      this.toolStripMenuItemSortAscending.Click += new System.EventHandler(this.ToolStripMenuItemSortAscending_Click);
      // 
      // toolStripMenuItemSortDescending
      // 
      this.toolStripMenuItemSortDescending.Image = ((System.Drawing.Image)(resources.GetObject("toolStripMenuItemSortDescending.Image")));
      this.toolStripMenuItemSortDescending.Name = "toolStripMenuItemSortDescending";
      this.toolStripMenuItemSortDescending.Size = new System.Drawing.Size(237, 22);
      this.toolStripMenuItemSortDescending.Tag = "Sort descending by \'{0}\'";
      this.toolStripMenuItemSortDescending.Text = "Sort descending by \'Column name\'";
      this.toolStripMenuItemSortDescending.Click += new System.EventHandler(this.ToolStripMenuItemSortDescending_Click);
      // 
      // toolStripMenuItemSortRemove
      // 
      this.toolStripMenuItemSortRemove.Name = "toolStripMenuItemSortRemove";
      this.toolStripMenuItemSortRemove.Size = new System.Drawing.Size(237, 22);
      this.toolStripMenuItemSortRemove.Text = "Unsort";
      this.toolStripMenuItemSortRemove.Click += new System.EventHandler(this.ToolStripMenuItemSortRemove_Click);
      // 
      // toolStripMenuItemFreeze
      // 
      this.toolStripMenuItemFreeze.Name = "toolStripMenuItemFreeze";
      this.toolStripMenuItemFreeze.Size = new System.Drawing.Size(237, 22);
      this.toolStripMenuItemFreeze.Text = "Freeze";
      this.toolStripMenuItemFreeze.Click += new System.EventHandler(this.ToolStripMenuItemFreeze_Click);
      // 
      // timerColumsFilter
      // 
      this.timerColumsFilter.Interval = 500;
      this.timerColumsFilter.Tick += new System.EventHandler(this.TimerColumsFilter_Tick);
      // 
      // toolStripMenuItemColumnVisibility
      // 
      this.toolStripMenuItemColumnVisibility.BackColor = System.Drawing.SystemColors.Window;
      this.toolStripMenuItemColumnVisibility.Name = "toolStripMenuItemColumnVisibility";
      this.toolStripMenuItemColumnVisibility.Overflow = System.Windows.Forms.ToolStripItemOverflow.Always;
      this.toolStripMenuItemColumnVisibility.Size = new System.Drawing.Size(38, 23);
      // 
      // FilteredDataGridView
      // 
      this.AutoGenerateColumns = false;
      dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
      dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
      dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.DefaultCellStyle = dataGridViewCellStyle2;
      dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
      this.contextMenuStripCell.ResumeLayout(false);
      this.contextMenuStripFilter.ResumeLayout(false);
      this.contextMenuStripColumns.ResumeLayout(false);
      this.contextMenuStripHeader.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
      this.ResumeLayout(false);

    }

    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCopy;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCopyError;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFilterRemove;
    private System.Windows.Forms.ContextMenuStrip contextMenuStripHeader;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFilterAdd;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSortAscending;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSortDescending;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemHideAllColumns;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemShowAllColumns;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFilled;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFilterRemoveAllFilter;
    private System.Windows.Forms.ContextMenuStrip contextMenuStripCell;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFilterThisValue;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemApplyFilter;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSortRemove;

    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRemoveOne;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCF;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparatorCF;
    private ToolStripCheckedListBox toolStripMenuItemColumnVisibility;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemColumns;
    private System.Windows.Forms.ContextMenuStrip contextMenuStripColumns;
    private System.Windows.Forms.ContextMenuStrip contextMenuStripFilter;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
    private System.Windows.Forms.Timer timerColumsFilter;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFreeze;
  }
}
