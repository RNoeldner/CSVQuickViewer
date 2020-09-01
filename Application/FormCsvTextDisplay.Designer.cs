using System.Diagnostics.CodeAnalysis;

namespace CsvTools
{
  partial class FormCsvTextDisplay
  {

    /// <summary>
    /// The components
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

    #region

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.ScrollBarVertical = new System.Windows.Forms.VScrollBar();
      this.splitContainer = new System.Windows.Forms.SplitContainer();
      this.textBox = new System.Windows.Forms.RichTextBox();
      this.CSVTextBox = new CsvTools.CSVRichTextBox();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
      this.splitContainer.Panel1.SuspendLayout();
      this.splitContainer.Panel2.SuspendLayout();
      this.splitContainer.SuspendLayout();
      this.SuspendLayout();
      // 
      // ScrollBarVertical
      // 
      this.ScrollBarVertical.Dock = System.Windows.Forms.DockStyle.Right;
      this.ScrollBarVertical.Location = new System.Drawing.Point(942, 0);
      this.ScrollBarVertical.Name = "ScrollBarVertical";
      this.ScrollBarVertical.Size = new System.Drawing.Size(21, 491);
      this.ScrollBarVertical.TabIndex = 0;
      this.ScrollBarVertical.ValueChanged += new System.EventHandler(this.ValueChangedEvent);
      // 
      // splitContainer
      // 
      this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer.IsSplitterFixed = true;
      this.splitContainer.Location = new System.Drawing.Point(0, 0);
      this.splitContainer.Name = "splitContainer";
      // 
      // splitContainer.Panel1
      // 
      this.splitContainer.Panel1.Controls.Add(this.textBox);
      // 
      // splitContainer.Panel2
      // 
      this.splitContainer.Panel2.Controls.Add(this.CSVTextBox);
      this.splitContainer.Size = new System.Drawing.Size(942, 491);
      this.splitContainer.SplitterDistance = 46;
      this.splitContainer.TabIndex = 3;
      // 
      // textBox
      // 
      this.textBox.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
      this.textBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.textBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textBox.Enabled = false;
      this.textBox.Location = new System.Drawing.Point(0, 0);
      this.textBox.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
      this.textBox.Name = "textBox";
      this.textBox.ReadOnly = true;
      this.textBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
      this.textBox.ShortcutsEnabled = false;
      this.textBox.Size = new System.Drawing.Size(46, 491);
      this.textBox.TabIndex = 0;
      this.textBox.Text = "";
      this.textBox.WordWrap = false;
      // 
      // CSVTextBox
      // 
      this.CSVTextBox.AcceptsTab = true;
      this.CSVTextBox.AutoWordSelection = true;
      this.CSVTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.CSVTextBox.DetectUrls = false;
      this.CSVTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.CSVTextBox.Location = new System.Drawing.Point(0, 0);
      this.CSVTextBox.Margin = new System.Windows.Forms.Padding(2);
      this.CSVTextBox.Name = "CSVTextBox";
      this.CSVTextBox.ReadOnly = true;
      this.CSVTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Horizontal;
      this.CSVTextBox.Size = new System.Drawing.Size(892, 491);
      this.CSVTextBox.TabIndex = 1;
      this.CSVTextBox.WordWrap = false;
      this.CSVTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.CSVTextBox_KeyUp);
      this.CSVTextBox.Resize += new System.EventHandler(this.CSVTextBox_Resize);
      // 
      // FormCsvTextDisplay
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.ControlLightLight;
      this.ClientSize = new System.Drawing.Size(963, 491);
      this.Controls.Add(this.splitContainer);
      this.Controls.Add(this.ScrollBarVertical);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Margin = new System.Windows.Forms.Padding(2);
      this.Name = "FormCsvTextDisplay";
      this.splitContainer.Panel1.ResumeLayout(false);
      this.splitContainer.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
      this.splitContainer.ResumeLayout(false);
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.VScrollBar ScrollBarVertical;

    #endregion
    
    private CSVRichTextBox CSVTextBox;
    private System.Windows.Forms.SplitContainer splitContainer;
    private System.Windows.Forms.RichTextBox textBox;
  }
}
