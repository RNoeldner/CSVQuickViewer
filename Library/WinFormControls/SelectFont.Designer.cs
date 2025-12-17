namespace CsvTools
{
  partial class SelectFont
  {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      components = new System.ComponentModel.Container();
      comboBoxFont = new System.Windows.Forms.ComboBox();
      labelFontSize = new System.Windows.Forms.Label();
      labelFont = new System.Windows.Forms.Label();
      tableLayoutPanelFont = new System.Windows.Forms.TableLayoutPanel();
      comboBoxSize = new System.Windows.Forms.ComboBox();
      buttonDefault = new System.Windows.Forms.Button();
      toolTip = new System.Windows.Forms.ToolTip(components);
      tableLayoutPanelFont.SuspendLayout();
      SuspendLayout();
      // 
      // comboBoxFont
      // 
      comboBoxFont.Dock = System.Windows.Forms.DockStyle.Top;
      comboBoxFont.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      comboBoxFont.FormattingEnabled = true;
      comboBoxFont.Location = new System.Drawing.Point(60, 0);
      comboBoxFont.Margin = new System.Windows.Forms.Padding(0);
      comboBoxFont.Name = "comboBoxFont";
      comboBoxFont.Size = new System.Drawing.Size(262, 21);
      comboBoxFont.TabIndex = 12;
      toolTip.SetToolTip(comboBoxFont, "All available fonts will be listed, Symbol like fonts should not be used");
      comboBoxFont.SelectedIndexChanged += ComboBoxFont_SelectedIndexChanged;
      // 
      // labelFontSize
      // 
      labelFontSize.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelFontSize.AutoSize = true;
      labelFontSize.Location = new System.Drawing.Point(3, 25);
      labelFontSize.Margin = new System.Windows.Forms.Padding(3);
      labelFontSize.Name = "labelFontSize";
      labelFontSize.Size = new System.Drawing.Size(54, 13);
      labelFontSize.TabIndex = 15;
      labelFontSize.Text = "Font Size:";
      // 
      // labelFont
      // 
      labelFont.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelFont.AutoSize = true;
      labelFont.Location = new System.Drawing.Point(26, 4);
      labelFont.Margin = new System.Windows.Forms.Padding(3);
      labelFont.Name = "labelFont";
      labelFont.Size = new System.Drawing.Size(31, 13);
      labelFont.TabIndex = 13;
      labelFont.Text = "Font:";
      // 
      // tableLayoutPanelFont
      // 
      tableLayoutPanelFont.ColumnCount = 3;
      tableLayoutPanelFont.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanelFont.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanelFont.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanelFont.Controls.Add(labelFont, 0, 0);
      tableLayoutPanelFont.Controls.Add(comboBoxFont, 1, 0);
      tableLayoutPanelFont.Controls.Add(labelFontSize, 0, 1);
      tableLayoutPanelFont.Controls.Add(comboBoxSize, 1, 1);
      tableLayoutPanelFont.Controls.Add(buttonDefault, 2, 0);
      tableLayoutPanelFont.Dock = System.Windows.Forms.DockStyle.Fill;
      tableLayoutPanelFont.Location = new System.Drawing.Point(0, 0);
      tableLayoutPanelFont.Margin = new System.Windows.Forms.Padding(0);
      tableLayoutPanelFont.Name = "tableLayoutPanelFont";
      tableLayoutPanelFont.RowCount = 3;
      tableLayoutPanelFont.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelFont.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelFont.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanelFont.Size = new System.Drawing.Size(327, 47);
      tableLayoutPanelFont.TabIndex = 16;
      // 
      // comboBoxSize
      // 
      comboBoxSize.Dock = System.Windows.Forms.DockStyle.Top;
      comboBoxSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      comboBoxSize.FormattingEnabled = true;
      comboBoxSize.Location = new System.Drawing.Point(60, 21);
      comboBoxSize.Margin = new System.Windows.Forms.Padding(0);
      comboBoxSize.Name = "comboBoxSize";
      comboBoxSize.Size = new System.Drawing.Size(262, 21);
      comboBoxSize.TabIndex = 16;
      toolTip.SetToolTip(comboBoxSize, "Size of the font");
      comboBoxSize.SelectedIndexChanged += ComboBoxSize_SelectedIndexChanged;
      // 
      // buttonDefault
      // 
      buttonDefault.Location = new System.Drawing.Point(322, 0);
      buttonDefault.Margin = new System.Windows.Forms.Padding(0);
      buttonDefault.Name = "buttonDefault";
      tableLayoutPanelFont.SetRowSpan(buttonDefault, 2);
      buttonDefault.Size = new System.Drawing.Size(5, 3);
      buttonDefault.TabIndex = 17;
      buttonDefault.Text = "&Default";
      toolTip.SetToolTip(buttonDefault, "Use system default font");
      buttonDefault.UseVisualStyleBackColor = true;
      buttonDefault.Click += ButtonDefault_Click;
      // 
      // SelectFont
      // 
      Controls.Add(tableLayoutPanelFont);
      Margin = new System.Windows.Forms.Padding(0);
      Name = "SelectFont";
      Size = new System.Drawing.Size(327, 47);
      tableLayoutPanelFont.ResumeLayout(false);
      tableLayoutPanelFont.PerformLayout();
      ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ComboBox comboBoxFont;
    private System.Windows.Forms.Label labelFontSize;
    private System.Windows.Forms.Label labelFont;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanelFont;
    private System.Windows.Forms.ComboBox comboBoxSize;
    private System.Windows.Forms.Button buttonDefault;
    private System.Windows.Forms.ToolTip toolTip;
  }
}
