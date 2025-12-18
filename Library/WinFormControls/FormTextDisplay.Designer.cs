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
      components = new System.ComponentModel.Container();
      System.Windows.Forms.FlowLayoutPanel flowLayoutPanelDisplay;
      System.Windows.Forms.StatusStrip statusStrip;
      var resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTextDisplay));
      radioButtonText = new System.Windows.Forms.RadioButton();
      radioButtonJson = new System.Windows.Forms.RadioButton();
      radioButtonXml = new System.Windows.Forms.RadioButton();
      radioButtonHtml = new System.Windows.Forms.RadioButton();
      buttonSave = new System.Windows.Forms.Button();
      buttonCancel = new System.Windows.Forms.Button();
      textBox = new FastColoredTextBoxNS.FastColoredTextBox();
      webBrowser = new System.Windows.Forms.WebBrowser();
      fastColoredTextBoxRO = new FastColoredTextBoxNS.FastColoredTextBox();
      flowLayoutPanelDisplay = new System.Windows.Forms.FlowLayoutPanel();
      statusStrip = new System.Windows.Forms.StatusStrip();
      flowLayoutPanelDisplay.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize) textBox).BeginInit();
      ((System.ComponentModel.ISupportInitialize) fastColoredTextBoxRO).BeginInit();
      SuspendLayout();
      // 
      // flowLayoutPanelDisplay
      // 
      flowLayoutPanelDisplay.Anchor =  System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
      flowLayoutPanelDisplay.AutoSize = true;
      flowLayoutPanelDisplay.Controls.Add(radioButtonText);
      flowLayoutPanelDisplay.Controls.Add(radioButtonJson);
      flowLayoutPanelDisplay.Controls.Add(radioButtonXml);
      flowLayoutPanelDisplay.Controls.Add(radioButtonHtml);
      flowLayoutPanelDisplay.Controls.Add(buttonSave);
      flowLayoutPanelDisplay.Controls.Add(buttonCancel);
      flowLayoutPanelDisplay.Location = new System.Drawing.Point(480, 261);
      flowLayoutPanelDisplay.Name = "flowLayoutPanelDisplay";
      flowLayoutPanelDisplay.Size = new System.Drawing.Size(341, 31);
      flowLayoutPanelDisplay.TabIndex = 8;
      // 
      // radioButtonText
      // 
      radioButtonText.Anchor =  System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
      radioButtonText.AutoSize = true;
      radioButtonText.BackColor = System.Drawing.Color.Transparent;
      radioButtonText.Checked = true;
      radioButtonText.ForeColor = System.Drawing.SystemColors.InfoText;
      radioButtonText.Location = new System.Drawing.Point(3, 11);
      radioButtonText.Name = "radioButtonText";
      radioButtonText.Size = new System.Drawing.Size(46, 17);
      radioButtonText.TabIndex = 2;
      radioButtonText.TabStop = true;
      radioButtonText.Text = "&Text";
      radioButtonText.UseVisualStyleBackColor = false;
      radioButtonText.CheckedChanged += RadioButton1_CheckedChanged;
      // 
      // radioButtonJson
      // 
      radioButtonJson.Anchor =  System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
      radioButtonJson.AutoSize = true;
      radioButtonJson.BackColor = System.Drawing.Color.Transparent;
      radioButtonJson.ForeColor = System.Drawing.SystemColors.InfoText;
      radioButtonJson.Location = new System.Drawing.Point(55, 11);
      radioButtonJson.Name = "radioButtonJson";
      radioButtonJson.Size = new System.Drawing.Size(47, 17);
      radioButtonJson.TabIndex = 3;
      radioButtonJson.Text = "&Json";
      radioButtonJson.UseVisualStyleBackColor = false;
      radioButtonJson.CheckedChanged += RadioButton2_CheckedChanged;
      // 
      // radioButtonXml
      // 
      radioButtonXml.Anchor =  System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
      radioButtonXml.AutoSize = true;
      radioButtonXml.BackColor = System.Drawing.Color.Transparent;
      radioButtonXml.ForeColor = System.Drawing.SystemColors.InfoText;
      radioButtonXml.Location = new System.Drawing.Point(108, 11);
      radioButtonXml.Name = "radioButtonXml";
      radioButtonXml.Size = new System.Drawing.Size(42, 17);
      radioButtonXml.TabIndex = 4;
      radioButtonXml.Text = "&Xml";
      radioButtonXml.UseVisualStyleBackColor = false;
      radioButtonXml.CheckedChanged += RadioButton3_CheckedChanged;
      // 
      // radioButtonHtml
      // 
      radioButtonHtml.Anchor =  System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
      radioButtonHtml.AutoSize = true;
      radioButtonHtml.BackColor = System.Drawing.Color.Transparent;
      radioButtonHtml.ForeColor = System.Drawing.SystemColors.InfoText;
      radioButtonHtml.Location = new System.Drawing.Point(156, 11);
      radioButtonHtml.Name = "radioButtonHtml";
      radioButtonHtml.Size = new System.Drawing.Size(46, 17);
      radioButtonHtml.TabIndex = 5;
      radioButtonHtml.Text = "&Html";
      radioButtonHtml.UseVisualStyleBackColor = false;
      radioButtonHtml.CheckedChanged += RadioButton4_CheckedChanged;
      // 
      // buttonSave
      // 
      buttonSave.AutoSize = true;
      buttonSave.DialogResult = System.Windows.Forms.DialogResult.OK;
      buttonSave.Enabled = false;
      buttonSave.Location = new System.Drawing.Point(208, 3);
      buttonSave.Name = "buttonSave";
      buttonSave.Size = new System.Drawing.Size(62, 25);
      buttonSave.TabIndex = 6;
      buttonSave.Text = "&Save";
      buttonSave.UseVisualStyleBackColor = true;
      buttonSave.Click += ButtonSave_Click;
      // 
      // buttonCancel
      // 
      buttonCancel.AutoSize = true;
      buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      buttonCancel.Location = new System.Drawing.Point(276, 3);
      buttonCancel.Name = "buttonCancel";
      buttonCancel.Size = new System.Drawing.Size(59, 25);
      buttonCancel.TabIndex = 7;
      buttonCancel.Text = "&Cancel";
      buttonCancel.UseVisualStyleBackColor = true;
      buttonCancel.Click += ButtonCancel_Click;
      // 
      // statusStrip
      // 
      statusStrip.Location = new System.Drawing.Point(0, 263);
      statusStrip.MinimumSize = new System.Drawing.Size(0, 30);
      statusStrip.Name = "statusStrip";
      statusStrip.Size = new System.Drawing.Size(833, 30);
      statusStrip.TabIndex = 7;
      statusStrip.Text = "statusStrip1";
      // 
      // textBox
      // 
      textBox.AutoCompleteBracketsList = new char[]
  {
    '(',
    ')',
    '{',
    '}',
    '[',
    ']',
    '"',
    '"',
    '\'',
    '\''
  };
      textBox.AutoIndentCharsPatterns = "\r\n^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>.+)\r\n";
      textBox.AutoScrollMinSize = new System.Drawing.Size(0, 14);
      textBox.BackBrush = null;
      textBox.CaretColor = System.Drawing.Color.Silver;
      textBox.CharHeight = 14;
      textBox.CharWidth = 8;
      textBox.CommentPrefix = "--";
      textBox.Cursor = System.Windows.Forms.Cursors.IBeam;
      textBox.DelayedEventsInterval = 50;
      textBox.DelayedTextChangedInterval = 50;
      textBox.DisabledColor = System.Drawing.Color.FromArgb(  100,   180,   180,   180);
      textBox.Dock = System.Windows.Forms.DockStyle.Fill;
      textBox.IsReplaceMode = false;
      textBox.Location = new System.Drawing.Point(0, 0);
      textBox.Margin = new System.Windows.Forms.Padding(2);
      textBox.Name = "textBox";
      textBox.Paddings = new System.Windows.Forms.Padding(0);
      textBox.SelectionColor = System.Drawing.Color.FromArgb(  60,   0,   0,   255);
      textBox.ShowFoldingLines = true;
      textBox.Size = new System.Drawing.Size(833, 263);
      textBox.TabIndex = 1;
      textBox.WordWrap = true;
      textBox.Zoom = 100;
      textBox.TextChanged += TextBox_TextChanged;
      // 
      // webBrowser
      // 
      webBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
      webBrowser.Location = new System.Drawing.Point(0, 0);
      webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
      webBrowser.Name = "webBrowser";
      webBrowser.Size = new System.Drawing.Size(833, 263);
      webBrowser.TabIndex = 6;
      // 
      // fastColoredTextBoxRO
      // 
      fastColoredTextBoxRO.AutoCompleteBracketsList = new char[]
  {
    '(',
    ')',
    '{',
    '}',
    '[',
    ']',
    '"',
    '"',
    '\'',
    '\''
  };
      fastColoredTextBoxRO.AutoIndentCharsPatterns = "\r\n^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>.+)\r\n";
      fastColoredTextBoxRO.AutoScrollMinSize = new System.Drawing.Size(0, 14);
      fastColoredTextBoxRO.BackBrush = null;
      fastColoredTextBoxRO.CaretColor = System.Drawing.Color.Silver;
      fastColoredTextBoxRO.CharHeight = 14;
      fastColoredTextBoxRO.CharWidth = 8;
      fastColoredTextBoxRO.CommentPrefix = "--";
      fastColoredTextBoxRO.Cursor = System.Windows.Forms.Cursors.IBeam;
      fastColoredTextBoxRO.DelayedEventsInterval = 50;
      fastColoredTextBoxRO.DelayedTextChangedInterval = 50;
      fastColoredTextBoxRO.DisabledColor = System.Drawing.Color.FromArgb(  100,   180,   180,   180);
      fastColoredTextBoxRO.Dock = System.Windows.Forms.DockStyle.Fill;
      fastColoredTextBoxRO.IsReplaceMode = false;
      fastColoredTextBoxRO.Location = new System.Drawing.Point(0, 0);
      fastColoredTextBoxRO.Margin = new System.Windows.Forms.Padding(2);
      fastColoredTextBoxRO.Name = "fastColoredTextBoxRO";
      fastColoredTextBoxRO.Paddings = new System.Windows.Forms.Padding(0);
      fastColoredTextBoxRO.ReadOnly = true;
      fastColoredTextBoxRO.SelectionColor = System.Drawing.Color.FromArgb(  60,   0,   0,   255);
      fastColoredTextBoxRO.ShowFoldingLines = true;
      fastColoredTextBoxRO.Size = new System.Drawing.Size(833, 263);
      fastColoredTextBoxRO.TabIndex = 2;
      fastColoredTextBoxRO.WordWrap = true;
      fastColoredTextBoxRO.Zoom = 100;
      fastColoredTextBoxRO.TextChangedDelayed += TextBox_TextChangedDelayed;
      fastColoredTextBoxRO.VisibleRangeChangedDelayed += TextBox_VisibleRangeChangedDelayed;
      // 
      // FormTextDisplay
      // 
      ClientSize = new System.Drawing.Size(833, 293);
      Controls.Add(flowLayoutPanelDisplay);
      Controls.Add(textBox);
      Controls.Add(webBrowser);
      Controls.Add(fastColoredTextBoxRO);
      Controls.Add(statusStrip);
      Margin = new System.Windows.Forms.Padding(2);
      Name = "FormTextDisplay";
      Shown += FormTextDisplay_Shown;
      flowLayoutPanelDisplay.ResumeLayout(false);
      flowLayoutPanelDisplay.PerformLayout();
      ((System.ComponentModel.ISupportInitialize) textBox).EndInit();
      ((System.ComponentModel.ISupportInitialize) fastColoredTextBoxRO).EndInit();
      ResumeLayout(false);
      PerformLayout();

    }
    #endregion

    private FastColoredTextBoxNS.FastColoredTextBox textBox;
    private System.Windows.Forms.RadioButton radioButtonText;
    private System.Windows.Forms.RadioButton radioButtonJson;
    private System.Windows.Forms.RadioButton radioButtonXml;
    private System.Windows.Forms.RadioButton radioButtonHtml;
    private System.Windows.Forms.WebBrowser webBrowser;
    private FastColoredTextBoxNS.FastColoredTextBox fastColoredTextBoxRO;
    private System.Windows.Forms.StatusStrip statusStrip;
    private System.Windows.Forms.Button buttonSave;
    private System.Windows.Forms.Button buttonCancel;
  }
}
