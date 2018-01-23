namespace CsvTools
{
  partial class CsvTextDisplay
  {

    /// <summary>
    /// The components
    /// </summary>
    private System.ComponentModel.IContainer components = null;


    /// <summary>
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
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
    /// Initializes the component.
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
      this.ScrollBarVertical.Location = new System.Drawing.Point(387, 0);
      this.ScrollBarVertical.Name = "ScrollBarVertical";
      this.ScrollBarVertical.Size = new System.Drawing.Size(17, 203);
      this.ScrollBarVertical.TabIndex = 0;
      this.ScrollBarVertical.Scroll += new System.Windows.Forms.ScrollEventHandler(this.ScrollEvent);
      this.ScrollBarVertical.ValueChanged += new System.EventHandler(this.ValueChangedEvent);
      // 
      // CSVTextBox
      // 
      this.CSVTextBox.AcceptsTab = true;
      this.CSVTextBox.AutoWordSelection = true;
      this.CSVTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.CSVTextBox.Delimiter = ',';
      this.CSVTextBox.DisplaySpace = true;
      this.CSVTextBox.Dock = System.Windows.Forms.DockStyle.Left;
      this.CSVTextBox.Escape = '\\';
      this.CSVTextBox.Location = new System.Drawing.Point(0, 0);
      this.CSVTextBox.Name = "CSVTextBox";
      this.CSVTextBox.Quote = '\"';
      this.CSVTextBox.ReadOnly = true;
      this.CSVTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Horizontal;
      this.CSVTextBox.Size = new System.Drawing.Size(384, 203);
      this.CSVTextBox.TabIndex = 1;
      this.CSVTextBox.Text = "";
      // 
      // CsvTextDisplay
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.CSVTextBox);
      this.Controls.Add(this.ScrollBarVertical);
      this.Name = "CsvTextDisplay";
      this.Size = new System.Drawing.Size(404, 203);
      this.SizeChanged += new System.EventHandler(this.SizeChangedEvent);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.VScrollBar ScrollBarVertical;
    private CSVRichTextBox CSVTextBox;
  }
}
