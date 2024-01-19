namespace CsvTools
{
  partial class FormSelectInDropdown
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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.comboBox = new System.Windows.Forms.ComboBox();
      this.buttonOK = new System.Windows.Forms.Button();
      this.buttonCancel = new System.Windows.Forms.Button();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // comboBox
      // 
      this.tableLayoutPanel1.SetColumnSpan(this.comboBox, 3);
      this.comboBox.Dock = System.Windows.Forms.DockStyle.Top;
      this.comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBox.Location = new System.Drawing.Point(3, 3);
      this.comboBox.Name = "comboBox";
      this.comboBox.Size = new System.Drawing.Size(368, 21);
      this.comboBox.TabIndex = 0;
      // 
      // buttonOK
      // 
      this.buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.buttonOK.Location = new System.Drawing.Point(227, 30);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(69, 25);
      this.buttonOK.TabIndex = 1;
      this.buttonOK.Text = "&OK";
      this.buttonOK.UseVisualStyleBackColor = true;
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(302, 30);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(69, 25);
      this.buttonCancel.TabIndex = 2;
      this.buttonCancel.Text = "&Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.AutoSize = true;
      this.tableLayoutPanel1.ColumnCount = 3;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.Controls.Add(this.comboBox, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.buttonCancel, 2, 1);
      this.tableLayoutPanel1.Controls.Add(this.buttonOK, 1, 1);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 2;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.Size = new System.Drawing.Size(374, 58);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // FormSelectInDropdown
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Control;
      this.ClientSize = new System.Drawing.Size(374, 58);
      this.Controls.Add(this.tableLayoutPanel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormSelectInDropdown";
      this.ShowIcon = false;
      this.Text = "Select";
      this.TopMost = true;
      this.tableLayoutPanel1.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button buttonOK;
    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.ComboBox comboBox;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
  }
}