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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCsvTextDisplay));
      this.textBox = new FastColoredTextBoxNS.FastColoredTextBox();
      ((System.ComponentModel.ISupportInitialize)(this.textBox)).BeginInit();
      this.SuspendLayout();
      // 
      // textBox
      // 
      this.textBox.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
      this.textBox.AutoIndentCharsPatterns = "\r\n^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>.+)\r\n";
      this.textBox.AutoScrollMinSize = new System.Drawing.Size(0, 16);
      this.textBox.BackBrush = null;
      this.textBox.CaretColor = System.Drawing.Color.Silver;
      this.textBox.CharHeight = 16;
      this.textBox.CharWidth = 9;
      this.textBox.CommentPrefix = "--";
      this.textBox.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.textBox.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
      this.textBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textBox.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.textBox.IsReplaceMode = false;
      this.textBox.Location = new System.Drawing.Point(0, 0);
      this.textBox.Name = "textBox";
      this.textBox.Paddings = new System.Windows.Forms.Padding(0);
      this.textBox.ReadOnly = true;
      this.textBox.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
      this.textBox.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("textBox.ServiceColors")));
      this.textBox.ShowFoldingLines = true;
      this.textBox.Size = new System.Drawing.Size(957, 446);
      this.textBox.TabIndex = 1;
      this.textBox.WordWrap = true;
      this.textBox.Zoom = 100;
      this.textBox.TextChangedDelayed += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.textBox_TextChangedDelayed);
      this.textBox.VisibleRangeChangedDelayed += new System.EventHandler(this.textBox_VisibleRangeChangedDelayed);
      // 
      // FormCsvTextDisplay
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.ControlLightLight;
      this.ClientSize = new System.Drawing.Size(957, 446);
      this.Controls.Add(this.textBox);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.Name = "FormCsvTextDisplay";
      ((System.ComponentModel.ISupportInitialize)(this.textBox)).EndInit();
      this.ResumeLayout(false);

    }


    #endregion

    private FastColoredTextBoxNS.FastColoredTextBox textBox;
  }
}
