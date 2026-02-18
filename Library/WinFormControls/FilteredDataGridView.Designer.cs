using System;
using System.Diagnostics.CodeAnalysis;

namespace CsvTools
{

  partial class FilteredDataGridView
  {

    private System.ComponentModel.IContainer components = null;

    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "RedundantCast")]
    [SuppressMessage("ReSharper", "RedundantDelegateCreation")]
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
      System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
      System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
      System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
      System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FilteredDataGridView));
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
      this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
      this.contextMenuStripCell = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.toolStripMenuItemOpenEditor = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemCopyError = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemCopy = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemFilter = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemFilterThisValue = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemFilterRemove = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemCols = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemColumns = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemFilterAdd = new System.Windows.Forms.ToolStripMenuItem();
      this.contextMenuStripHeader = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.toolStripMenuItemHideThisColumn = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemCF = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparatorCF = new System.Windows.Forms.ToolStripSeparator();
      this.toolStripMenuItemRemoveOne = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemFilterRemoveAllFilter = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemSortAscending = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemSortDescending = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemSortRemove = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemFreeze = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItemFreeze2 = new System.Windows.Forms.ToolStripMenuItem();
      toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
      toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
      toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
      toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
      toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
      this.contextMenuStripCell.SuspendLayout();
      this.contextMenuStripHeader.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
      this.SuspendLayout();
      // 
      // toolStripSeparator4
      // 
      toolStripSeparator4.Name = "toolStripSeparator4";
      toolStripSeparator4.Size = new System.Drawing.Size(110, 6);
      // 
      // toolStripSeparator3
      // 
      toolStripSeparator3.Name = "toolStripSeparator3";
      toolStripSeparator3.Size = new System.Drawing.Size(265, 6);
      // 
      // toolStripSeparator1
      // 
      toolStripSeparator1.Name = "toolStripSeparator1";
      toolStripSeparator1.Size = new System.Drawing.Size(316, 6);
      // 
      // toolStripSeparator7
      // 
      toolStripSeparator7.Name = "toolStripSeparator7";
      toolStripSeparator7.Size = new System.Drawing.Size(316, 6);
      // 
      // toolStripSeparator10
      // 
      toolStripSeparator10.Name = "toolStripSeparator10";
      toolStripSeparator10.Size = new System.Drawing.Size(316, 6);
      // 
      // toolStripSeparator2
      // 
      this.toolStripSeparator2.Name = "toolStripSeparator2";
      this.toolStripSeparator2.Size = new System.Drawing.Size(265, 6);
      // 
      // toolStripSeparator6
      // 
      this.toolStripSeparator6.Name = "toolStripSeparator6";
      this.toolStripSeparator6.Size = new System.Drawing.Size(167, 6);
      // 
      // contextMenuStripCell
      // 
      this.contextMenuStripCell.ImageScalingSize = new System.Drawing.Size(24, 24);
      this.contextMenuStripCell.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemOpenEditor,
            toolStripSeparator10,
            this.toolStripMenuItemCopyError,
            this.toolStripMenuItemCopy,
            toolStripSeparator1,
            this.toolStripMenuItemFilter,
            this.toolStripMenuItemFilterThisValue,
            this.toolStripMenuItemFilterRemove,
            toolStripSeparator7,
            this.toolStripMenuItemCols,
            this.toolStripMenuItemFreeze2});
      this.contextMenuStripCell.Name = "contextMenuStripDropDownCopy";
      this.contextMenuStripCell.Size = new System.Drawing.Size(320, 262);
      // 
      // toolStripMenuItemOpenEditor
      // 
      this.toolStripMenuItemOpenEditor.Name = "toolStripMenuItemOpenEditor";
      this.toolStripMenuItemOpenEditor.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F2)));
      this.toolStripMenuItemOpenEditor.Size = new System.Drawing.Size(319, 30);
      this.toolStripMenuItemOpenEditor.Text = "Open Editor";
      this.toolStripMenuItemOpenEditor.ToolTipText = "Edit in Popup";
      this.toolStripMenuItemOpenEditor.Click += new System.EventHandler(this.ToolStripMenuItemOpenEditor_Click);
      // 
      // toolStripMenuItemCopyError
      // 
      this.toolStripMenuItemCopyError.Name = "toolStripMenuItemCopyError";
      this.toolStripMenuItemCopyError.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
      this.toolStripMenuItemCopyError.Size = new System.Drawing.Size(319, 30);
      this.toolStripMenuItemCopyError.Text = "Copy";
      this.toolStripMenuItemCopyError.Click += new System.EventHandler(this.ToolStripMenuItemCopyError_Click);
      // 
      // toolStripMenuItemCopy
      // 
      this.toolStripMenuItemCopy.Name = "toolStripMenuItemCopy";
      this.toolStripMenuItemCopy.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.L)));
      this.toolStripMenuItemCopy.Size = new System.Drawing.Size(319, 30);
      this.toolStripMenuItemCopy.Text = "Copy (without error information)";
      this.toolStripMenuItemCopy.Click += new System.EventHandler(this.ToolStripMenuItemCopy_Click);
      // 
      // toolStripMenuItemFilter
      // 
      this.toolStripMenuItemFilter.Image = ((System.Drawing.Image)(resources.GetObject("filter")));
      this.toolStripMenuItemFilter.Name = "toolStripMenuItemFilter";
      this.toolStripMenuItemFilter.Size = new System.Drawing.Size(319, 30);
      this.toolStripMenuItemFilter.Text = "Filter";
      this.toolStripMenuItemFilter.ToolTipText = "Filter the rows based on this column ";
      this.toolStripMenuItemFilter.Click += new System.EventHandler(this.OpenFilterDialog);
      // 
      // toolStripMenuItemFilterThisValue
      // 
      this.toolStripMenuItemFilterThisValue.Image = ((System.Drawing.Image)(resources.GetObject("filter_new")));
      this.toolStripMenuItemFilterThisValue.Name = "toolStripMenuItemFilterThisValue";
      this.toolStripMenuItemFilterThisValue.Size = new System.Drawing.Size(319, 30);
      this.toolStripMenuItemFilterThisValue.Text = "Filter for this value";
      this.toolStripMenuItemFilterThisValue.ToolTipText = "Only show rows with this column value";
      this.toolStripMenuItemFilterThisValue.Click += new System.EventHandler(this.ToolStripMenuItemFilterValue_Click);
      // 
      // toolStripMenuItemFilterRemove
      // 
      this.toolStripMenuItemFilterRemove.Image = ((System.Drawing.Image)(resources.GetObject("filter_remove")));
      this.toolStripMenuItemFilterRemove.Name = "toolStripMenuItemFilterRemove";
      this.toolStripMenuItemFilterRemove.Size = new System.Drawing.Size(319, 30);
      this.toolStripMenuItemFilterRemove.Text = "Remove all Filter";
      this.toolStripMenuItemFilterRemove.Click += new System.EventHandler(this.ToolStripMenuItemFilterRemoveAll_Click);
      // 
      // toolStripMenuItemCols
      // 
      this.toolStripMenuItemCols.Name = "toolStripMenuItemCols";
      this.toolStripMenuItemCols.Size = new System.Drawing.Size(319, 30);
      this.toolStripMenuItemCols.Text = "Columns";
      this.toolStripMenuItemCols.ToolTipText = "Decide column visibility";
      this.toolStripMenuItemCols.Click += new System.EventHandler(this.OpenColumnsDialog);
      // 
      // toolStripMenuItemColumns
      // 
      this.toolStripMenuItemColumns.Name = "toolStripMenuItemColumns";
      this.toolStripMenuItemColumns.Size = new System.Drawing.Size(268, 30);
      this.toolStripMenuItemColumns.Text = "Columns";
      this.toolStripMenuItemColumns.Click += new System.EventHandler(this.OpenColumnsDialog);
      // 
      // toolStripMenuItemFilterAdd
      // 
      this.toolStripMenuItemFilterAdd.Image = ((System.Drawing.Image)(resources.GetObject("filter")));
      this.toolStripMenuItemFilterAdd.Name = "toolStripMenuItemFilterAdd";
      this.toolStripMenuItemFilterAdd.Size = new System.Drawing.Size(268, 30);
      this.toolStripMenuItemFilterAdd.Text = "Filter";
      this.toolStripMenuItemFilterAdd.Click += new System.EventHandler(this.OpenFilterDialog);
      // 
      // contextMenuStripHeader
      // 
      this.contextMenuStripHeader.ImageScalingSize = new System.Drawing.Size(24, 24);
      this.contextMenuStripHeader.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemHideThisColumn,
            this.toolStripMenuItemFilterAdd,
            this.toolStripMenuItemColumns,
            this.toolStripMenuItemCF,
            this.toolStripSeparatorCF,
            this.toolStripMenuItemRemoveOne,
            this.toolStripMenuItemFilterRemoveAllFilter,
            this.toolStripSeparator2,
            this.toolStripMenuItemSortAscending,
            this.toolStripMenuItemSortDescending,
            this.toolStripMenuItemSortRemove,
            toolStripSeparator3,
            this.toolStripMenuItemFreeze});
      this.contextMenuStripHeader.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Table;
      this.contextMenuStripHeader.Name = "contextMenuStripHeader";
      this.contextMenuStripHeader.Size = new System.Drawing.Size(269, 322);
      // 
      // toolStripMenuItemHideThisColumn
      // 
      this.toolStripMenuItemHideThisColumn.Name = "toolStripMenuItemHideThisColumn";
      this.toolStripMenuItemHideThisColumn.Size = new System.Drawing.Size(268, 30);
      this.toolStripMenuItemHideThisColumn.Text = "Hide Column";
      this.toolStripMenuItemHideThisColumn.Click += new System.EventHandler(this.ToolStripMenuItemHideThisColumn_Click);
      // 
      // toolStripMenuItemCF
      // 
      this.toolStripMenuItemCF.Name = "toolStripMenuItemCF";
      this.toolStripMenuItemCF.Size = new System.Drawing.Size(268, 30);
      this.toolStripMenuItemCF.Text = "Change Format";
      this.toolStripMenuItemCF.Visible = false;
      this.toolStripMenuItemCF.Click += new System.EventHandler(this.ToolStripMenuItemCF_Click);
      // 
      // toolStripSeparatorCF
      // 
      this.toolStripSeparatorCF.Name = "toolStripSeparatorCF";
      this.toolStripSeparatorCF.Size = new System.Drawing.Size(265, 6);
      this.toolStripSeparatorCF.Visible = false;
      // 
      // toolStripMenuItemRemoveOne
      // 
      this.toolStripMenuItemRemoveOne.Image = ((System.Drawing.Image)(resources.GetObject("filter_remove")));
      this.toolStripMenuItemRemoveOne.Name = "toolStripMenuItemRemoveOne";
      this.toolStripMenuItemRemoveOne.ShortcutKeys = System.Windows.Forms.Keys.Delete;
      this.toolStripMenuItemRemoveOne.Size = new System.Drawing.Size(268, 30);
      this.toolStripMenuItemRemoveOne.Text = "Remove Filter";
      this.toolStripMenuItemRemoveOne.Click += new System.EventHandler(this.ToolStripMenuItemFilterRemoveOne_Click);
      // 
      // toolStripMenuItemFilterRemoveAllFilter
      // 
      this.toolStripMenuItemFilterRemoveAllFilter.Enabled = false;
      this.toolStripMenuItemFilterRemoveAllFilter.Image = ((System.Drawing.Image)(resources.GetObject("filter_remove")));
      this.toolStripMenuItemFilterRemoveAllFilter.Name = "toolStripMenuItemFilterRemoveAllFilter";
      this.toolStripMenuItemFilterRemoveAllFilter.Size = new System.Drawing.Size(268, 30);
      this.toolStripMenuItemFilterRemoveAllFilter.Text = "Remove all Filter";
      this.toolStripMenuItemFilterRemoveAllFilter.ToolTipText = "Remove all currently used column filters";
      this.toolStripMenuItemFilterRemoveAllFilter.Click += new System.EventHandler(this.ToolStripMenuItemFilterRemoveAll_Click);
      // 
      // toolStripMenuItemSortAscending
      // 
      this.toolStripMenuItemSortAscending.Image = ((System.Drawing.Image)(resources.GetObject("sort_ascending")));
      this.toolStripMenuItemSortAscending.Name = "toolStripMenuItemSortAscending";
      this.toolStripMenuItemSortAscending.Size = new System.Drawing.Size(268, 30);
      this.toolStripMenuItemSortAscending.Tag = "Sort ascending by \'{0}\'";
      this.toolStripMenuItemSortAscending.Text = "Sort ascending by \'Column name\'";
      this.toolStripMenuItemSortAscending.Click += new System.EventHandler(this.ToolStripMenuItemSortAscending_Click);
      // 
      // toolStripMenuItemSortDescending
      // 
      this.toolStripMenuItemSortDescending.Image = ((System.Drawing.Image)(resources.GetObject("sort_descending")));
      this.toolStripMenuItemSortDescending.Name = "toolStripMenuItemSortDescending";
      this.toolStripMenuItemSortDescending.Size = new System.Drawing.Size(268, 30);
      this.toolStripMenuItemSortDescending.Tag = "Sort descending by \'{0}\'";
      this.toolStripMenuItemSortDescending.Text = "Sort descending by \'Column name\'";
      this.toolStripMenuItemSortDescending.Click += new System.EventHandler(this.ToolStripMenuItemSortDescending_Click);
      // 
      // toolStripMenuItemSortRemove
      // 
      this.toolStripMenuItemSortRemove.Name = "toolStripMenuItemSortRemove";
      this.toolStripMenuItemSortRemove.Size = new System.Drawing.Size(268, 30);
      this.toolStripMenuItemSortRemove.Text = "Unsort";
      this.toolStripMenuItemSortRemove.Click += new System.EventHandler(this.ToolStripMenuItemSortRemove_Click);
      // 
      // toolStripMenuItemFreeze
      // 
      this.toolStripMenuItemFreeze.Name = "toolStripMenuItemFreeze";
      this.toolStripMenuItemFreeze.Size = new System.Drawing.Size(268, 30);
      this.toolStripMenuItemFreeze.Text = "Freeze / Unfreeze Column";
      this.toolStripMenuItemFreeze.ToolTipText = "Frozen: Column will stay visible during horizontal scrolling";
      this.toolStripMenuItemFreeze.Click += new System.EventHandler(this.ToolStripMenuItemFreeze_Click);
      // 
      // toolStripMenuItemFreeze2
      // 
      this.toolStripMenuItemFreeze2.Name = "toolStripMenuItemFreeze2";
      this.toolStripMenuItemFreeze2.Size = new System.Drawing.Size(319, 30);
      this.toolStripMenuItemFreeze2.Text = "Freeze / Unfreeze Column";
      this.toolStripMenuItemFreeze2.ToolTipText = "Frozen: Column will stay visible during horizontal scrolling";
      this.toolStripMenuItemFreeze2.Click += new System.EventHandler(this.ToolStripMenuItemFreeze_Click);
      // 
      // FilteredDataGridView
      // 
      this.AutoGenerateColumns = false;
      dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
      this.RowTemplate.Height = 33;
      this.contextMenuStripCell.ResumeLayout(false);
      this.contextMenuStripHeader.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
      this.ResumeLayout(false);

    }

    private System.Windows.Forms.ContextMenuStrip contextMenuStripCell;    
    private System.Windows.Forms.ContextMenuStrip contextMenuStripHeader;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCols;    
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFilter;    
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCF;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemColumns;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCopy;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCopyError;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFilterAdd;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFilterRemove;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFilterRemoveAllFilter;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFilterThisValue;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFreeze;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemHideThisColumn;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRemoveOne;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSortAscending;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSortDescending;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSortRemove;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOpenEditor;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparatorCF;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFreeze2;
  }
}
