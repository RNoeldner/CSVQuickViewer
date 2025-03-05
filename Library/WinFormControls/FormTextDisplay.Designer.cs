using System;

namespace CsvTools
{
  partial class FormTextDisplay
  {

    /// <summary>
    /// The components
    /// </summary>
    private System.ComponentModel.IContainer components = null;


    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {

      if (disposing)
      {
        if (m_HighLighter is IDisposable disposable)
          disposable.Dispose();
        components?.Dispose();
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
      System.Windows.Forms.FlowLayoutPanel flowLayoutPanelDisplay;
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTextDisplay));
      this.radioButtonText = new System.Windows.Forms.RadioButton();
      this.radioButtonJson = new System.Windows.Forms.RadioButton();
      this.radioButtonXml = new System.Windows.Forms.RadioButton();
      this.radioButtonHtml = new System.Windows.Forms.RadioButton();
      this.buttonSave = new System.Windows.Forms.Button();
      this.buttonCancel = new System.Windows.Forms.Button();
      this.textBox = new FastColoredTextBoxNS.FastColoredTextBox();
      this.webBrowser = new System.Windows.Forms.WebBrowser();
      this.fastColoredTextBoxRO = new FastColoredTextBoxNS.FastColoredTextBox();
      this.statusStrip1 = new System.Windows.Forms.StatusStrip();
      flowLayoutPanelDisplay = new System.Windows.Forms.FlowLayoutPanel();
      flowLayoutPanelDisplay.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.textBox)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.fastColoredTextBoxRO)).BeginInit();
      this.SuspendLayout();
      // 
      // flowLayoutPanelDisplay
      // 
      flowLayoutPanelDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      flowLayoutPanelDisplay.AutoSize = true;
      flowLayoutPanelDisplay.Controls.Add(this.radioButtonText);
      flowLayoutPanelDisplay.Controls.Add(this.radioButtonJson);
      flowLayoutPanelDisplay.Controls.Add(this.radioButtonXml);
      flowLayoutPanelDisplay.Controls.Add(this.radioButtonHtml);
      flowLayoutPanelDisplay.Controls.Add(this.buttonSave);
      flowLayoutPanelDisplay.Controls.Add(this.buttonCancel);
      flowLayoutPanelDisplay.Location = new System.Drawing.Point(482, 463);
      flowLayoutPanelDisplay.Name = "flowLayoutPanelDisplay";
      flowLayoutPanelDisplay.Size = new System.Drawing.Size(338, 31);
      flowLayoutPanelDisplay.TabIndex = 8;
      // 
      // radioButtonText
      // 
      this.radioButtonText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.radioButtonText.AutoSize = true;
      this.radioButtonText.BackColor = System.Drawing.Color.Transparent;
      this.radioButtonText.Checked = true;
      this.radioButtonText.ForeColor = System.Drawing.SystemColors.InfoText;
      this.radioButtonText.Location = new System.Drawing.Point(3, 11);
      this.radioButtonText.Name = "radioButtonText";
      this.radioButtonText.Size = new System.Drawing.Size(46, 17);
      this.radioButtonText.TabIndex = 2;
      this.radioButtonText.TabStop = true;
      this.radioButtonText.Text = "&Text";
      this.radioButtonText.UseVisualStyleBackColor = false;
      this.radioButtonText.CheckedChanged += new System.EventHandler(this.RadioButton1_CheckedChanged);
      // 
      // radioButtonJson
      // 
      this.radioButtonJson.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.radioButtonJson.AutoSize = true;
      this.radioButtonJson.BackColor = System.Drawing.Color.Transparent;
      this.radioButtonJson.ForeColor = System.Drawing.SystemColors.InfoText;
      this.radioButtonJson.Location = new System.Drawing.Point(55, 11);
      this.radioButtonJson.Name = "radioButtonJson";
      this.radioButtonJson.Size = new System.Drawing.Size(47, 17);
      this.radioButtonJson.TabIndex = 3;
      this.radioButtonJson.Text = "&Json";
      this.radioButtonJson.UseVisualStyleBackColor = false;
      this.radioButtonJson.CheckedChanged += new System.EventHandler(this.RadioButton2_CheckedChanged);
      // 
      // radioButtonXml
      // 
      this.radioButtonXml.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.radioButtonXml.AutoSize = true;
      this.radioButtonXml.BackColor = System.Drawing.Color.Transparent;
      this.radioButtonXml.ForeColor = System.Drawing.SystemColors.InfoText;
      this.radioButtonXml.Location = new System.Drawing.Point(108, 11);
      this.radioButtonXml.Name = "radioButtonXml";
      this.radioButtonXml.Size = new System.Drawing.Size(42, 17);
      this.radioButtonXml.TabIndex = 4;
      this.radioButtonXml.Text = "&Xml";
      this.radioButtonXml.UseVisualStyleBackColor = false;
      this.radioButtonXml.CheckedChanged += new System.EventHandler(this.RadioButton3_CheckedChanged);
      // 
      // radioButtonHtml
      // 
      this.radioButtonHtml.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.radioButtonHtml.AutoSize = true;
      this.radioButtonHtml.BackColor = System.Drawing.Color.Transparent;
      this.radioButtonHtml.ForeColor = System.Drawing.SystemColors.InfoText;
      this.radioButtonHtml.Location = new System.Drawing.Point(156, 11);
      this.radioButtonHtml.Name = "radioButtonHtml";
      this.radioButtonHtml.Size = new System.Drawing.Size(46, 17);
      this.radioButtonHtml.TabIndex = 5;
      this.radioButtonHtml.Text = "&Html";
      this.radioButtonHtml.UseVisualStyleBackColor = false;
      this.radioButtonHtml.CheckedChanged += new System.EventHandler(this.RadioButton4_CheckedChanged);
      // 
      // buttonSave
      // 
      this.buttonSave.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.buttonSave.Enabled = false;
      this.buttonSave.Location = new System.Drawing.Point(208, 3);
      this.buttonSave.Name = "buttonSave";
      this.buttonSave.Size = new System.Drawing.Size(62, 25);
      this.buttonSave.TabIndex = 6;
      this.buttonSave.Text = "&Save";
      this.buttonSave.UseVisualStyleBackColor = true;
      this.buttonSave.Click += new System.EventHandler(this.ButtonSave_Click);
      // 
      // buttonCancel
      // 
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(276, 3);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(59, 25);
      this.buttonCancel.TabIndex = 7;
      this.buttonCancel.Text = "&Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
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
      this.textBox.AutoScrollMinSize = new System.Drawing.Size(0, 14);
      this.textBox.BackBrush = null;
      this.textBox.CaretColor = System.Drawing.Color.Silver;
      this.textBox.CharHeight = 14;
      this.textBox.CharWidth = 8;
      this.textBox.CommentPrefix = "--";
      this.textBox.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.textBox.DelayedEventsInterval = 50;
      this.textBox.DelayedTextChangedInterval = 50;
      this.textBox.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
      this.textBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textBox.IsReplaceMode = false;
      this.textBox.Location = new System.Drawing.Point(0, 0);
      this.textBox.Margin = new System.Windows.Forms.Padding(2);
      this.textBox.Name = "textBox";
      this.textBox.Paddings = new System.Windows.Forms.Padding(0);
      this.textBox.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
      this.textBox.ShowFoldingLines = true;
      this.textBox.Size = new System.Drawing.Size(832, 465);
      this.textBox.TabIndex = 1;
      this.textBox.WordWrap = true;
      this.textBox.Zoom = 100;
      this.textBox.TextChanged += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.TextBox_TextChanged);
      // 
      // webBrowser
      // 
      this.webBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
      this.webBrowser.Location = new System.Drawing.Point(0, 0);
      this.webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
      this.webBrowser.Name = "webBrowser";
      this.webBrowser.Size = new System.Drawing.Size(832, 465);
      this.webBrowser.TabIndex = 6;
      // 
      // fastColoredTextBoxRO
      // 
      this.fastColoredTextBoxRO.AutoCompleteBracketsList = new char[] {
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
      this.fastColoredTextBoxRO.AutoIndentCharsPatterns = "\r\n^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>.+)\r\n";
      this.fastColoredTextBoxRO.AutoScrollMinSize = new System.Drawing.Size(0, 14);
      this.fastColoredTextBoxRO.BackBrush = null;
      this.fastColoredTextBoxRO.CaretColor = System.Drawing.Color.Silver;
      this.fastColoredTextBoxRO.CharHeight = 14;
      this.fastColoredTextBoxRO.CharWidth = 8;
      this.fastColoredTextBoxRO.CommentPrefix = "--";
      this.fastColoredTextBoxRO.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.fastColoredTextBoxRO.DelayedEventsInterval = 50;
      this.fastColoredTextBoxRO.DelayedTextChangedInterval = 50;
      this.fastColoredTextBoxRO.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
      this.fastColoredTextBoxRO.Dock = System.Windows.Forms.DockStyle.Fill;
      this.fastColoredTextBoxRO.IsReplaceMode = false;
      this.fastColoredTextBoxRO.Location = new System.Drawing.Point(0, 0);
      this.fastColoredTextBoxRO.Margin = new System.Windows.Forms.Padding(2);
      this.fastColoredTextBoxRO.Name = "fastColoredTextBoxRO";
      this.fastColoredTextBoxRO.Paddings = new System.Windows.Forms.Padding(0);
      this.fastColoredTextBoxRO.ReadOnly = true;
      this.fastColoredTextBoxRO.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
      this.fastColoredTextBoxRO.ShowFoldingLines = true;
      this.fastColoredTextBoxRO.Size = new System.Drawing.Size(832, 465);
      this.fastColoredTextBoxRO.TabIndex = 2;
      this.fastColoredTextBoxRO.WordWrap = true;
      this.fastColoredTextBoxRO.Zoom = 100;
      this.fastColoredTextBoxRO.TextChangedDelayed += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.TextBox_TextChangedDelayed);
      this.fastColoredTextBoxRO.VisibleRangeChangedDelayed += new System.EventHandler(this.TextBox_VisibleRangeChangedDelayed);
      // 
      // statusStrip1
      // 
      this.statusStrip1.Location = new System.Drawing.Point(0, 465);
      this.statusStrip1.MinimumSize = new System.Drawing.Size(0, 30);
      this.statusStrip1.Name = "statusStrip1";
      this.statusStrip1.Size = new System.Drawing.Size(832, 30);
      this.statusStrip1.TabIndex = 7;
      this.statusStrip1.Text = "statusStrip1";
      // 
      // FormTextDisplay
      // 
      this.ClientSize = new System.Drawing.Size(832, 495);
      this.Controls.Add(flowLayoutPanelDisplay);
      this.Controls.Add(this.textBox);
      this.Controls.Add(this.webBrowser);
      this.Controls.Add(this.fastColoredTextBoxRO);
      this.Controls.Add(this.statusStrip1);
      this.Margin = new System.Windows.Forms.Padding(2);
      this.Name = "FormTextDisplay";
      this.Shown += new System.EventHandler(this.FormTextDisplay_Shown);
      flowLayoutPanelDisplay.ResumeLayout(false);
      flowLayoutPanelDisplay.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.textBox)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.fastColoredTextBoxRO)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    #endregion

    private FastColoredTextBoxNS.FastColoredTextBox textBox;
    private System.Windows.Forms.RadioButton radioButtonText;
    private System.Windows.Forms.RadioButton radioButtonJson;
    private System.Windows.Forms.RadioButton radioButtonXml;
    private System.Windows.Forms.RadioButton radioButtonHtml;
    private System.Windows.Forms.WebBrowser webBrowser;
    private FastColoredTextBoxNS.FastColoredTextBox fastColoredTextBoxRO;
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.Button buttonSave;
    private System.Windows.Forms.Button buttonCancel;
  }
}
