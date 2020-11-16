namespace CsvTools
{
  sealed partial class FormMain
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
      if (m_DisposedValue) return;

      if (disposing)
      {
        m_DisposedValue = true;
        components?.Dispose();
        m_CancellationTokenSource?.Dispose();
        m_SettingsChangedTimerChange?.Dispose();
        m_SourceDisplay?.Dispose();
        m_DetailControlLoader?.Dispose();
      }
      Microsoft.Win32.SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
      Microsoft.Win32.SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
      base.Dispose(disposing);
    }


    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
			this.fileSystemWatcher = new System.IO.FileSystemWatcher();
			this.loggerDisplay = new CsvTools.LoggerDisplay();
			this.detailControl = new CsvTools.DetailControl();
			this.toolStrip = new System.Windows.Forms.ToolStrip();
			this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
			this.m_ToolStripButtonLoadFile = new System.Windows.Forms.ToolStripButton();
			this.m_ToolStripButtonAsText = new System.Windows.Forms.ToolStripButton();
			this.m_ToolStripButtonShowLog = new System.Windows.Forms.ToolStripButton();
			this.m_ToolStripButtonSettings = new System.Windows.Forms.ToolStripButton();
			this.m_ToolStripButtonSource = new System.Windows.Forms.ToolStripButton();
			this.textPanel = new System.Windows.Forms.ToolStripContainer();
			((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.loggerDisplay)).BeginInit();
			this.toolStrip.SuspendLayout();
			this.textPanel.BottomToolStripPanel.SuspendLayout();
			this.textPanel.ContentPanel.SuspendLayout();
			this.textPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// fileSystemWatcher
			// 
			this.fileSystemWatcher.EnableRaisingEvents = true;
			this.fileSystemWatcher.NotifyFilter = ((System.IO.NotifyFilters)((System.IO.NotifyFilters.Size | System.IO.NotifyFilters.LastWrite)));
			this.fileSystemWatcher.SynchronizingObject = this;
			this.fileSystemWatcher.Changed += new System.IO.FileSystemEventHandler(this.FileSystemWatcher_Changed);
			// 
			// loggerDisplay
			// 
			this.loggerDisplay.AllowDrop = false;
			this.loggerDisplay.AutoCompleteBracketsList = new char[] {
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
			this.loggerDisplay.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\n^\\s*(case|default)\\s*[^:]*(" +
    "?<range>:)\\s*(?<range>[^;]+);";
			this.loggerDisplay.AutoScrollMinSize = new System.Drawing.Size(2, 14);
			this.loggerDisplay.BackBrush = null;
			this.loggerDisplay.BackColor = System.Drawing.SystemColors.Window;
			this.loggerDisplay.CausesValidation = false;
			this.loggerDisplay.CharHeight = 14;
			this.loggerDisplay.CharWidth = 8;
			this.loggerDisplay.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.loggerDisplay.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
			this.loggerDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
			this.loggerDisplay.IsReplaceMode = false;
			this.loggerDisplay.Location = new System.Drawing.Point(0, 0);
			this.loggerDisplay.Margin = new System.Windows.Forms.Padding(2);
			this.loggerDisplay.MinLevel = CsvTools.Logger.Level.Debug;
			this.loggerDisplay.Name = "loggerDisplay";
			this.loggerDisplay.Paddings = new System.Windows.Forms.Padding(0);
			this.loggerDisplay.ReadOnly = true;
			this.loggerDisplay.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
			this.loggerDisplay.ServiceColors = null;
			this.loggerDisplay.ShowLineNumbers = false;
			this.loggerDisplay.Size = new System.Drawing.Size(971, 503);
			this.loggerDisplay.TabIndex = 2;
			this.loggerDisplay.Zoom = 100;
			// 
			// detailControl
			// 
			dataGridViewCellStyle1.BackColor = System.Drawing.Color.Gainsboro;
			this.detailControl.AlternatingRowDefaultCellStyle = dataGridViewCellStyle1;
			this.detailControl.DataTable = null;
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.detailControl.DefaultCellStyle = dataGridViewCellStyle2;
			this.detailControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailControl.FileSetting = null;
			this.detailControl.Location = new System.Drawing.Point(0, 0);
			this.detailControl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.detailControl.Name = "detailControl";
			this.detailControl.Size = new System.Drawing.Size(971, 528);
			this.detailControl.TabIndex = 1;
			// 
			// toolStrip
			// 
			this.toolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.toolStripButton2});
			this.toolStrip.Location = new System.Drawing.Point(4, 0);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Size = new System.Drawing.Size(80, 25);
			this.toolStrip.TabIndex = 5;
			this.toolStrip.Text = "toolStrip";
			// 
			// toolStripButton1
			// 
			this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonLoadFile.Image")));
			this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton1.Name = "toolStripButton1";
			this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
			this.toolStripButton1.Text = "Load File";
			this.toolStripButton1.Click += new System.EventHandler(this.SelectFile);
			// 
			// toolStripButton2
			// 
			this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonShowLog.Image")));
			this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton2.Name = "toolStripButton2";
			this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
			this.toolStripButton2.Text = "Log";
			this.toolStripButton2.Click += new System.EventHandler(this.ToggleShowLog);
			// 
			// m_ToolStripButtonLoadFile
			// 
			this.m_ToolStripButtonLoadFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_ToolStripButtonLoadFile.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonLoadFile.Image")));
			this.m_ToolStripButtonLoadFile.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_ToolStripButtonLoadFile.Name = "m_ToolStripButtonLoadFile";
			this.m_ToolStripButtonLoadFile.Size = new System.Drawing.Size(23, 22);
			this.m_ToolStripButtonLoadFile.Text = "Load File";
			this.m_ToolStripButtonLoadFile.Click += new System.EventHandler(this.SelectFile);
			// 
			// m_ToolStripButtonAsText
			// 
			this.m_ToolStripButtonAsText.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_ToolStripButtonAsText.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonAsText.Image")));
			this.m_ToolStripButtonAsText.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_ToolStripButtonAsText.Name = "m_ToolStripButtonAsText";
			this.m_ToolStripButtonAsText.Size = new System.Drawing.Size(23, 22);
			this.m_ToolStripButtonAsText.Text = "Text";
			this.m_ToolStripButtonAsText.Click += new System.EventHandler(this.ToggleDisplayAsText);
			// 
			// m_ToolStripButtonShowLog
			// 
			this.m_ToolStripButtonShowLog.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_ToolStripButtonShowLog.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonShowLog.Image")));
			this.m_ToolStripButtonShowLog.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_ToolStripButtonShowLog.Name = "m_ToolStripButtonShowLog";
			this.m_ToolStripButtonShowLog.Size = new System.Drawing.Size(23, 22);
			this.m_ToolStripButtonShowLog.Text = "Log";
			this.m_ToolStripButtonShowLog.Click += new System.EventHandler(this.ToggleShowLog);
			// 
			// m_ToolStripButtonSettings
			// 
			this.m_ToolStripButtonSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_ToolStripButtonSettings.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonSettings.Image")));
			this.m_ToolStripButtonSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_ToolStripButtonSettings.Name = "m_ToolStripButtonSettings";
			this.m_ToolStripButtonSettings.Size = new System.Drawing.Size(23, 22);
			this.m_ToolStripButtonSettings.Text = "Setting";
			this.m_ToolStripButtonSettings.Click += new System.EventHandler(this.ShowSettings);
			// 
			// m_ToolStripButtonSource
			// 
			this.m_ToolStripButtonSource.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.m_ToolStripButtonSource.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonSource.Image")));
			this.m_ToolStripButtonSource.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.m_ToolStripButtonSource.Name = "m_ToolStripButtonSource";
			this.m_ToolStripButtonSource.Size = new System.Drawing.Size(23, 22);
			this.m_ToolStripButtonSource.Text = "Source";
			this.m_ToolStripButtonSource.Click += new System.EventHandler(this.ShowSourceFile);
			// 
			// textPanel
			// 
			// 
			// textPanel.BottomToolStripPanel
			// 
			this.textPanel.BottomToolStripPanel.Controls.Add(this.toolStrip);			
			// 
			// textPanel.ContentPanel
			// 
			this.textPanel.ContentPanel.Controls.Add(this.loggerDisplay);
			this.textPanel.ContentPanel.Size = new System.Drawing.Size(971, 503);
			this.textPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textPanel.LeftToolStripPanelVisible = false;
			this.textPanel.Location = new System.Drawing.Point(0, 0);
			this.textPanel.Name = "textPanel";
			this.textPanel.RightToolStripPanelVisible = false;
			this.textPanel.Size = new System.Drawing.Size(971, 528);
			this.textPanel.TabIndex = 6;
			this.textPanel.Text = "toolStripContainer2";
			this.textPanel.TopToolStripPanelVisible = false;
			// 
			// FormMain
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(971, 528);
			this.Controls.Add(this.textPanel);
			this.Controls.Add(this.detailControl);
			this.HelpButton = true;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.Margin = new System.Windows.Forms.Padding(2);
			this.MinimumSize = new System.Drawing.Size(453, 119);
			this.Name = "FormMain";
			this.Activated += new System.EventHandler(this.FormMain_Activated);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.FileDragDrop);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.FileDragEnter);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FormMain_KeyUpAsync);
			((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.loggerDisplay)).EndInit();
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.textPanel.BottomToolStripPanel.ResumeLayout(false);
			this.textPanel.BottomToolStripPanel.PerformLayout();
			this.textPanel.ContentPanel.ResumeLayout(false);
			this.textPanel.ResumeLayout(false);
			this.textPanel.PerformLayout();
			this.ResumeLayout(false);

    }

    private CsvTools.DetailControl detailControl;
    private System.IO.FileSystemWatcher fileSystemWatcher;
    private CsvTools.LoggerDisplay loggerDisplay;

    #endregion

    private System.Boolean m_DisposedValue;
    private System.Windows.Forms.ToolStrip toolStrip;
    private System.Windows.Forms.ToolStripButton m_ToolStripButtonAsText;
    private System.Windows.Forms.ToolStripButton m_ToolStripButtonShowLog;
    private System.Windows.Forms.ToolStripButton m_ToolStripButtonSettings;
    private System.Windows.Forms.ToolStripButton m_ToolStripButtonSource;
    private System.Windows.Forms.ToolStripContainer textPanel;
    private System.Windows.Forms.ToolStripButton m_ToolStripButtonLoadFile;
    private System.Windows.Forms.ToolStripButton toolStripButton1;
    private System.Windows.Forms.ToolStripButton toolStripButton2;
  }
}

