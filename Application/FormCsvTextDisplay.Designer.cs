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
      this.CSVTextBox = new CsvTools.CSVRichTextBox();
      this.SuspendLayout();
      // 
      // ScrollBarVertical
      // 
      this.ScrollBarVertical.Dock = System.Windows.Forms.DockStyle.Right;
      this.ScrollBarVertical.Location = new System.Drawing.Point(778, 0);
      this.ScrollBarVertical.Name = "ScrollBarVertical";
      this.ScrollBarVertical.Size = new System.Drawing.Size(21, 532);
      this.ScrollBarVertical.TabIndex = 0;
      //this.ScrollBarVertical.Scroll += new System.Windows.Forms.ScrollEventHandler(this.ScrollEvent);
      this.ScrollBarVertical.ValueChanged += new System.EventHandler(this.ValueChangedEvent);
      // 
      // CSVTextBox
      // 
      this.CSVTextBox.AcceptsTab = true;
      this.CSVTextBox.AutoWordSelection = true;
      this.CSVTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.CSVTextBox.DetectUrls = false;
      this.CSVTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.CSVTextBox.Location = new System.Drawing.Point(0, 0);
      this.CSVTextBox.Margin = new System.Windows.Forms.Padding(2);
      this.CSVTextBox.Name = "CSVTextBox";
      this.CSVTextBox.ReadOnly = true;
      this.CSVTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Horizontal;
      this.CSVTextBox.Size = new System.Drawing.Size(778, 532);
      this.CSVTextBox.TabIndex = 1;
      this.CSVTextBox.WordWrap = false;
      // 
      // FormCsvTextDisplay
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.ControlLightLight;
      this.ClientSize = new System.Drawing.Size(799, 532);
      this.Controls.Add(this.CSVTextBox);
      this.Controls.Add(this.ScrollBarVertical);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Margin = new System.Windows.Forms.Padding(2);
      this.Name = "FormCsvTextDisplay";
      this.ResumeLayout(false);

    }

    private CsvTools.CSVRichTextBox CSVTextBox;
    private System.Windows.Forms.VScrollBar ScrollBarVertical;

    #endregion
  }
}
