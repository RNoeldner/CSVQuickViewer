using System.Diagnostics.CodeAnalysis;

namespace CsvTools
{
  sealed partial class FromColumnsFilter
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }

      base.Dispose(disposing);
    }

    
    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.Windows.Forms.ColumnHeader colText;
      this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
      this.listViewCluster = new System.Windows.Forms.ListView();
      this.panelTop = new System.Windows.Forms.Panel();
      this.label1 = new System.Windows.Forms.Label();
      this.buttonUncheck = new System.Windows.Forms.Button();
      this.textBoxFilter = new System.Windows.Forms.TextBox();
      this.buttonEmpty = new System.Windows.Forms.Button();
      this.buttonCheck = new System.Windows.Forms.Button();
      this.buttonApply = new System.Windows.Forms.Button();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.timerFilter = new System.Windows.Forms.Timer(this.components);
      colText = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
      this.panelTop.SuspendLayout();
      this.SuspendLayout();
      // 
      // colText
      // 
      colText.Text = "Filter";
      colText.Width = -1;
      // 
      // errorProvider
      // 
      this.errorProvider.ContainerControl = this;
      // 
      // listViewCluster
      // 
      this.listViewCluster.CheckBoxes = true;
      this.listViewCluster.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            colText});
      this.listViewCluster.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listViewCluster.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.listViewCluster.HideSelection = false;
      this.listViewCluster.Location = new System.Drawing.Point(0, 32);
      this.listViewCluster.Name = "listViewCluster";
      this.listViewCluster.ShowGroups = false;
      this.listViewCluster.Size = new System.Drawing.Size(535, 376);
      this.listViewCluster.TabIndex = 0;
      this.listViewCluster.UseCompatibleStateImageBehavior = false;
      this.listViewCluster.View = System.Windows.Forms.View.List;
      // 
      // panelTop
      // 
      this.panelTop.AutoSize = true;
      this.panelTop.Controls.Add(this.label1);
      this.panelTop.Controls.Add(this.buttonUncheck);
      this.panelTop.Controls.Add(this.textBoxFilter);
      this.panelTop.Controls.Add(this.buttonEmpty);
      this.panelTop.Controls.Add(this.buttonCheck);
      this.panelTop.Controls.Add(this.buttonApply);
      this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
      this.panelTop.Location = new System.Drawing.Point(0, 0);
      this.panelTop.Name = "panelTop";
      this.panelTop.Size = new System.Drawing.Size(535, 32);
      this.panelTop.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(304, 11);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(29, 13);
      this.label1.TabIndex = 7;
      this.label1.Text = "Filter";
      // 
      // buttonUncheck
      // 
      this.buttonUncheck.AutoSize = true;
      this.buttonUncheck.Location = new System.Drawing.Point(65, 4);
      this.buttonUncheck.Name = "buttonUncheck";
      this.buttonUncheck.Size = new System.Drawing.Size(66, 23);
      this.buttonUncheck.TabIndex = 1;
      this.buttonUncheck.Text = "&Uncheck";
      this.buttonUncheck.UseVisualStyleBackColor = true;
      this.buttonUncheck.Click += new System.EventHandler(this.ButtonUncheck_Click);
      // 
      // textBoxFilter
      // 
      this.textBoxFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxFilter.Location = new System.Drawing.Point(339, 7);
      this.textBoxFilter.Name = "textBoxFilter";
      this.textBoxFilter.Size = new System.Drawing.Size(100, 20);
      this.textBoxFilter.TabIndex = 3;
      this.textBoxFilter.TextChanged += new System.EventHandler(this.TextBoxValue_TextChanged);
      // 
      // buttonEmpty
      // 
      this.buttonEmpty.AutoSize = true;
      this.buttonEmpty.Location = new System.Drawing.Point(137, 4);
      this.buttonEmpty.Name = "buttonEmpty";
      this.buttonEmpty.Size = new System.Drawing.Size(112, 23);
      this.buttonEmpty.TabIndex = 2;
      this.buttonEmpty.Text = "&Hide empty columns";
      this.buttonEmpty.UseVisualStyleBackColor = true;
      this.buttonEmpty.Click += new System.EventHandler(this.ButtonEmpty_Click);
      // 
      // buttonCheck
      // 
      this.buttonCheck.AutoSize = true;
      this.buttonCheck.Location = new System.Drawing.Point(3, 4);
      this.buttonCheck.Name = "buttonCheck";
      this.buttonCheck.Size = new System.Drawing.Size(56, 23);
      this.buttonCheck.TabIndex = 0;
      this.buttonCheck.Text = "&Check";
      this.buttonCheck.UseVisualStyleBackColor = true;
      this.buttonCheck.Click += new System.EventHandler(this.ButtonCheck_Click);
      // 
      // buttonApply
      // 
      this.buttonApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonApply.AutoSize = true;
      this.buttonApply.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.buttonApply.Location = new System.Drawing.Point(445, 6);
      this.buttonApply.Name = "buttonApply";
      this.buttonApply.Size = new System.Drawing.Size(87, 23);
      this.buttonApply.TabIndex = 4;
      this.buttonApply.Text = "&Apply";
      this.toolTip.SetToolTip(this.buttonApply, "Apply the filter for the column");
      this.buttonApply.UseVisualStyleBackColor = true;
      this.buttonApply.Click += new System.EventHandler(this.ButtonApply_Click);
      // 
      // timerFilter
      // 
      this.timerFilter.Enabled = true;
      this.timerFilter.Interval = 200;
      this.timerFilter.Tick += new System.EventHandler(this.TimerFilter_Tick);
      // 
      // FromColumnsFilter
      // 
      this.AcceptButton = this.buttonApply;
      this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
      this.BackColor = System.Drawing.SystemColors.Control;
      this.ClientSize = new System.Drawing.Size(535, 408);
      this.Controls.Add(this.listViewCluster);
      this.Controls.Add(this.panelTop);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(551, 260);
      this.Name = "FromColumnsFilter";
      this.Text = "Filter";
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
      this.panelTop.ResumeLayout(false);
      this.panelTop.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion Windows Form Designer generated code
    private System.Windows.Forms.ErrorProvider errorProvider;
    private System.Windows.Forms.ListView listViewCluster;
    private System.Windows.Forms.Panel panelTop;
    private System.Windows.Forms.ToolTip toolTip;
    private System.Windows.Forms.Timer timerFilter;
    private System.Windows.Forms.Button buttonApply;
    private System.Windows.Forms.Button buttonCheck;
    private System.Windows.Forms.TextBox textBoxFilter;
    private System.Windows.Forms.Button buttonEmpty;
    private System.Windows.Forms.Button buttonUncheck;
    private System.Windows.Forms.Label label1;
  }
}