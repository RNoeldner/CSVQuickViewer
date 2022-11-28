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
      this.comboBoxFont = new System.Windows.Forms.ComboBox();
      this.labelFontSize = new System.Windows.Forms.Label();
      this.labelFont = new System.Windows.Forms.Label();
      this.tableLayoutPanelFont = new System.Windows.Forms.TableLayoutPanel();
      this.comboBoxSize = new System.Windows.Forms.ComboBox();
      this.buttonDefault = new System.Windows.Forms.Button();
      this.tableLayoutPanelFont.SuspendLayout();
      this.SuspendLayout();
      // 
      // comboBoxFont
      // 
      this.comboBoxFont.Dock = System.Windows.Forms.DockStyle.Top;
      this.comboBoxFont.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxFont.FormattingEnabled = true;
      this.comboBoxFont.Location = new System.Drawing.Point(63, 3);
      this.comboBoxFont.Name = "comboBoxFont";
      this.comboBoxFont.Size = new System.Drawing.Size(534, 21);
      this.comboBoxFont.TabIndex = 12;
      this.comboBoxFont.SelectedIndexChanged += new System.EventHandler(this.comboBoxFont_SelectedIndexChanged);
      // 
      // labelFontSize
      // 
      this.labelFontSize.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.labelFontSize.AutoSize = true;
      this.labelFontSize.Location = new System.Drawing.Point(3, 34);
      this.labelFontSize.Name = "labelFontSize";
      this.labelFontSize.Size = new System.Drawing.Size(54, 13);
      this.labelFontSize.TabIndex = 15;
      this.labelFontSize.Text = "Font Size:";
      // 
      // labelFont
      // 
      this.labelFont.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.labelFont.AutoSize = true;
      this.labelFont.Location = new System.Drawing.Point(26, 7);
      this.labelFont.Name = "labelFont";
      this.labelFont.Size = new System.Drawing.Size(31, 13);
      this.labelFont.TabIndex = 13;
      this.labelFont.Text = "Font:";
      // 
      // tableLayoutPanelFont
      // 
      this.tableLayoutPanelFont.ColumnCount = 3;
      this.tableLayoutPanelFont.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanelFont.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanelFont.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanelFont.Controls.Add(this.labelFont, 0, 0);
      this.tableLayoutPanelFont.Controls.Add(this.comboBoxFont, 1, 0);
      this.tableLayoutPanelFont.Controls.Add(this.labelFontSize, 0, 1);
      this.tableLayoutPanelFont.Controls.Add(this.comboBoxSize, 1, 1);
      this.tableLayoutPanelFont.Controls.Add(this.buttonDefault, 2, 0);
      this.tableLayoutPanelFont.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanelFont.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanelFont.Name = "tableLayoutPanelFont";
      this.tableLayoutPanelFont.RowCount = 3;
      this.tableLayoutPanelFont.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanelFont.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanelFont.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanelFont.Size = new System.Drawing.Size(657, 215);
      this.tableLayoutPanelFont.TabIndex = 16;
      // 
      // comboBoxSize
      // 
      this.comboBoxSize.Dock = System.Windows.Forms.DockStyle.Top;
      this.comboBoxSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxSize.FormattingEnabled = true;
      this.comboBoxSize.Location = new System.Drawing.Point(63, 30);
      this.comboBoxSize.Name = "comboBoxSize";
      this.comboBoxSize.Size = new System.Drawing.Size(534, 21);
      this.comboBoxSize.TabIndex = 16;
      this.comboBoxSize.SelectedIndexChanged += new System.EventHandler(this.comboBoxSize_SelectedIndexChanged);
      // 
      // buttonDefault
      // 
      this.buttonDefault.AutoSize = true;
      this.buttonDefault.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.buttonDefault.Location = new System.Drawing.Point(603, 3);
      this.buttonDefault.Name = "buttonDefault";
      this.tableLayoutPanelFont.SetRowSpan(this.buttonDefault, 2);
      this.buttonDefault.Size = new System.Drawing.Size(51, 23);
      this.buttonDefault.TabIndex = 17;
      this.buttonDefault.Text = "&Default";
      this.buttonDefault.UseVisualStyleBackColor = true;
      this.buttonDefault.Click += new System.EventHandler(this.buttonDefault_Click);
      // 
      // SelectFont
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tableLayoutPanelFont);
      this.Name = "SelectFont";
      this.Size = new System.Drawing.Size(657, 215);
      this.tableLayoutPanelFont.ResumeLayout(false);
      this.tableLayoutPanelFont.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ComboBox comboBoxFont;
    private System.Windows.Forms.Label labelFontSize;
    private System.Windows.Forms.Label labelFont;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanelFont;
    private System.Windows.Forms.ComboBox comboBoxSize;
    private System.Windows.Forms.Button buttonDefault;
  }
}
