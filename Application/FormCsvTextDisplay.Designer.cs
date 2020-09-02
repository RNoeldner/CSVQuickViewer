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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCsvTextDisplay));
      this.ScrollBarVertical = new System.Windows.Forms.VScrollBar();
      this.CSVTextBox = new CsvTools.CSVRichTextBox2();
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
      // CSVTextBox
      // 
      this.CSVTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.CSVTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.CSVTextBox.EdgeMode = ScintillaNET.EdgeMode.Line;
      this.CSVTextBox.EolMode = ScintillaNET.Eol.Lf;
      this.CSVTextBox.IdleStyling = ScintillaNET.IdleStyling.ToVisible;
      this.CSVTextBox.Location = new System.Drawing.Point(0, 0);
      this.CSVTextBox.Margin = new System.Windows.Forms.Padding(2);
      this.CSVTextBox.Margins.Left = 3;
      this.CSVTextBox.Margins.Right = 3;
      this.CSVTextBox.Name = "CSVTextBox";
      this.CSVTextBox.ReadOnly = true;
      this.CSVTextBox.ScrollWidth = 3501;
      this.CSVTextBox.ShowLineNumber = true;
      this.CSVTextBox.Size = new System.Drawing.Size(942, 491);
      this.CSVTextBox.TabIndex = 2;
      this.CSVTextBox.Technology = ScintillaNET.Technology.DirectWrite;
      this.CSVTextBox.Text = resources.GetString("CSVTextBox.Text");
      this.CSVTextBox.ViewEol = true;
      this.CSVTextBox.ViewWhitespace = ScintillaNET.WhitespaceMode.VisibleAlways;
      this.CSVTextBox.WhitespaceSize = 2;
      this.CSVTextBox.WordWrap = true;
      this.CSVTextBox.WrapMode = ScintillaNET.WrapMode.Char;
      this.CSVTextBox.WrapVisualFlags = ScintillaNET.WrapVisualFlags.End;
      // 
      // FormCsvTextDisplay
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.ControlLightLight;
      this.ClientSize = new System.Drawing.Size(963, 491);
      this.Controls.Add(this.CSVTextBox);
      this.Controls.Add(this.ScrollBarVertical);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Margin = new System.Windows.Forms.Padding(2);
      this.Name = "FormCsvTextDisplay";
      this.ResumeLayout(false);

    }
    private System.Windows.Forms.VScrollBar ScrollBarVertical;

    #endregion

    private CSVRichTextBox2 CSVTextBox;
  }
}
