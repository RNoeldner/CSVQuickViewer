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
      components = new System.ComponentModel.Container();
      System.Windows.Forms.ColumnHeader colText;
      errorProvider = new System.Windows.Forms.ErrorProvider(components);
      listViewCluster = new System.Windows.Forms.ListView();
      panelTop = new System.Windows.Forms.Panel();
      label1 = new System.Windows.Forms.Label();
      buttonUncheck = new System.Windows.Forms.Button();
      textBoxFilter = new System.Windows.Forms.TextBox();
      buttonEmpty = new System.Windows.Forms.Button();
      buttonCheck = new System.Windows.Forms.Button();
      buttonApply = new System.Windows.Forms.Button();
      toolTip = new System.Windows.Forms.ToolTip(components);
      timerFilter = new System.Windows.Forms.Timer(components);
      colText = new System.Windows.Forms.ColumnHeader();
      ((System.ComponentModel.ISupportInitialize) errorProvider).BeginInit();
      panelTop.SuspendLayout();
      SuspendLayout();
      // 
      // colText
      // 
      colText.Text = "Filter";
      colText.Width = -1;
      // 
      // errorProvider
      // 
      errorProvider.ContainerControl = this;
      // 
      // listViewCluster
      // 
      listViewCluster.CheckBoxes = true;
      listViewCluster.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colText });
      listViewCluster.Dock = System.Windows.Forms.DockStyle.Fill;
      listViewCluster.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      listViewCluster.HideSelection = false;
      listViewCluster.Location = new System.Drawing.Point(0, 34);
      listViewCluster.Name = "listViewCluster";
      listViewCluster.ShowGroups = false;
      listViewCluster.Size = new System.Drawing.Size(535, 374);
      listViewCluster.TabIndex = 0;
      listViewCluster.UseCompatibleStateImageBehavior = false;
      listViewCluster.View = System.Windows.Forms.View.List;
      // 
      // panelTop
      // 
      panelTop.AutoSize = true;
      panelTop.Controls.Add(label1);
      panelTop.Controls.Add(buttonUncheck);
      panelTop.Controls.Add(textBoxFilter);
      panelTop.Controls.Add(buttonEmpty);
      panelTop.Controls.Add(buttonCheck);
      panelTop.Controls.Add(buttonApply);
      panelTop.Dock = System.Windows.Forms.DockStyle.Top;
      panelTop.Location = new System.Drawing.Point(0, 0);
      panelTop.Name = "panelTop";
      panelTop.Size = new System.Drawing.Size(535, 34);
      panelTop.TabIndex = 0;
      // 
      // label1
      // 
      label1.Anchor =  System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      label1.AutoSize = true;
      label1.Location = new System.Drawing.Point(304, 11);
      label1.Name = "label1";
      label1.Size = new System.Drawing.Size(29, 13);
      label1.TabIndex = 7;
      label1.Text = "Filter";
      // 
      // buttonUncheck
      // 
      buttonUncheck.AutoSize = true;
      buttonUncheck.Location = new System.Drawing.Point(65, 4);
      buttonUncheck.Name = "buttonUncheck";
      buttonUncheck.Size = new System.Drawing.Size(66, 25);
      buttonUncheck.TabIndex = 1;
      buttonUncheck.Text = "&Uncheck";
      buttonUncheck.UseVisualStyleBackColor = true;
      buttonUncheck.Click += ButtonUncheck_Click;
      // 
      // textBoxFilter
      // 
      textBoxFilter.Anchor =  System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      textBoxFilter.Location = new System.Drawing.Point(339, 7);
      textBoxFilter.Name = "textBoxFilter";
      textBoxFilter.Size = new System.Drawing.Size(100, 20);
      textBoxFilter.TabIndex = 3;
      textBoxFilter.TextChanged += TextBoxValue_TextChanged;
      // 
      // buttonEmpty
      // 
      buttonEmpty.AutoSize = true;
      buttonEmpty.Location = new System.Drawing.Point(137, 4);
      buttonEmpty.Name = "buttonEmpty";
      buttonEmpty.Size = new System.Drawing.Size(112, 25);
      buttonEmpty.TabIndex = 2;
      buttonEmpty.Text = "&Hide empty columns";
      buttonEmpty.UseVisualStyleBackColor = true;
      buttonEmpty.Click += ButtonEmpty_Click;
      // 
      // buttonCheck
      // 
      buttonCheck.AutoSize = true;
      buttonCheck.Location = new System.Drawing.Point(3, 4);
      buttonCheck.Name = "buttonCheck";
      buttonCheck.Size = new System.Drawing.Size(56, 25);
      buttonCheck.TabIndex = 0;
      buttonCheck.Text = "&Check";
      buttonCheck.UseVisualStyleBackColor = true;
      buttonCheck.Click += ButtonCheck_Click;
      // 
      // buttonApply
      // 
      buttonApply.Anchor =  System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      buttonApply.AutoSize = true;
      buttonApply.DialogResult = System.Windows.Forms.DialogResult.OK;
      buttonApply.Location = new System.Drawing.Point(445, 6);
      buttonApply.Name = "buttonApply";
      buttonApply.Size = new System.Drawing.Size(87, 25);
      buttonApply.TabIndex = 4;
      buttonApply.Text = "&Apply";
      toolTip.SetToolTip(buttonApply, "Apply the filter for the column");
      buttonApply.UseVisualStyleBackColor = true;
      buttonApply.Click += ButtonApply_Click;
      // 
      // timerFilter
      // 
      timerFilter.Enabled = true;
      timerFilter.Interval = 200;
      timerFilter.Tick += TimerFilter_Tick;
      // 
      // FromColumnsFilter
      // 
      AcceptButton = buttonApply;
      AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
      BackColor = System.Drawing.SystemColors.Control;
      ClientSize = new System.Drawing.Size(535, 408);
      Controls.Add(listViewCluster);
      Controls.Add(panelTop);
      FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      MaximizeBox = false;
      MinimizeBox = false;
      MinimumSize = new System.Drawing.Size(551, 260);
      Name = "FromColumnsFilter";
      Text = "Filter";
      ((System.ComponentModel.ISupportInitialize) errorProvider).EndInit();
      panelTop.ResumeLayout(false);
      panelTop.PerformLayout();
      ResumeLayout(false);
      PerformLayout();

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