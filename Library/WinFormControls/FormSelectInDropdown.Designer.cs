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
      comboBox = new System.Windows.Forms.ComboBox();
      buttonOK = new System.Windows.Forms.Button();
      buttonCancel = new System.Windows.Forms.Button();
      tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      tableLayoutPanel1.SuspendLayout();
      SuspendLayout();
      // 
      // comboBox
      // 
      tableLayoutPanel1.SetColumnSpan(comboBox, 3);
      comboBox.Dock = System.Windows.Forms.DockStyle.Top;
      comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      comboBox.Location = new System.Drawing.Point(3, 3);
      comboBox.Name = "comboBox";
      comboBox.Size = new System.Drawing.Size(365, 21);
      comboBox.TabIndex = 0;
      // 
      // buttonOK
      // 
      buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Left;
      buttonOK.AutoSize = true;
      buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      buttonOK.Location = new System.Drawing.Point(224, 30);
      buttonOK.Name = "buttonOK";
      buttonOK.Size = new System.Drawing.Size(69, 25);
      buttonOK.TabIndex = 1;
      buttonOK.Text = "&OK";
      buttonOK.UseVisualStyleBackColor = true;
      // 
      // buttonCancel
      // 
      buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Left;
      buttonCancel.AutoSize = true;
      buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      buttonCancel.Location = new System.Drawing.Point(299, 30);
      buttonCancel.Name = "buttonCancel";
      buttonCancel.Size = new System.Drawing.Size(69, 25);
      buttonCancel.TabIndex = 2;
      buttonCancel.Text = "&Cancel";
      buttonCancel.UseVisualStyleBackColor = true;
      // 
      // tableLayoutPanel1
      // 
      tableLayoutPanel1.AutoSize = true;
      tableLayoutPanel1.ColumnCount = 3;
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel1.Controls.Add(comboBox, 0, 0);
      tableLayoutPanel1.Controls.Add(buttonOK, 1, 1);
      tableLayoutPanel1.Controls.Add(buttonCancel, 2, 1);
      tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
      tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      tableLayoutPanel1.Name = "tableLayoutPanel1";
      tableLayoutPanel1.RowCount = 2;
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.Size = new System.Drawing.Size(371, 58);
      tableLayoutPanel1.TabIndex = 0;
      // 
      // FormSelectInDropdown
      // 
      BackColor = System.Drawing.SystemColors.Control;
      ClientSize = new System.Drawing.Size(371, 58);
      Controls.Add(tableLayoutPanel1);
      FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      Margin = new System.Windows.Forms.Padding(3, 1, 3, 1);
      MaximizeBox = false;
      MinimizeBox = false;
      Name = "FormSelectInDropdown";
      ShowIcon = false;
      Text = "Select";
      tableLayoutPanel1.ResumeLayout(false);
      tableLayoutPanel1.PerformLayout();
      ResumeLayout(false);
      PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button buttonOK;
    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.ComboBox comboBox;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
  }
}