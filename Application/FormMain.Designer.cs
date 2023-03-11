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
        PgpHelper.ClearPgpInfo();

        m_DisposedValue = true;
        components?.Dispose();

        m_CancellationTokenSource?.Dispose();
        m_SettingsChangedTimerChange?.Dispose();
      }
      this.SafeInvoke(() => base.Dispose(disposing));
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
      this.fileSystemWatcher = new System.IO.FileSystemWatcher();
      this.loggerDisplay = new CsvTools.LoggerDisplay();
      this.detailControl = new CsvTools.DetailControl();
      this.toolStripLog = new System.Windows.Forms.ToolStrip();
      this.m_ToolStripButtonLoadFile2 = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripButtonShowLog2 = new System.Windows.Forms.ToolStripButton();
      this.textPanel = new System.Windows.Forms.ToolStripContainer();
      this.toolStripDataGrid = new System.Windows.Forms.ToolStrip();
      this.m_ToolStripButtonLoadFile = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripButtonAsText = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripButtonShowLog = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripButtonSettings = new System.Windows.Forms.ToolStripButton();
      ((System.ComponentModel.ISupportInitialize) (this.fileSystemWatcher)).BeginInit();
      ((System.ComponentModel.ISupportInitialize) (this.loggerDisplay)).BeginInit();
      this.toolStripLog.SuspendLayout();
      this.textPanel.BottomToolStripPanel.SuspendLayout();
      this.textPanel.ContentPanel.SuspendLayout();
      this.textPanel.SuspendLayout();
      this.toolStripDataGrid.SuspendLayout();
      this.SuspendLayout();
      // 
      // fileSystemWatcher
      // 
      this.fileSystemWatcher.EnableRaisingEvents = true;
      this.fileSystemWatcher.NotifyFilter = ((System.IO.NotifyFilters) ((System.IO.NotifyFilters.Size | System.IO.NotifyFilters.LastWrite)));
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
      this.loggerDisplay.DisabledColor = System.Drawing.Color.FromArgb(((int) (((byte) (100)))), ((int) (((byte) (180)))), ((int) (((byte) (180)))), ((int) (((byte) (180)))));
      this.loggerDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
      this.loggerDisplay.IsReplaceMode = false;
      this.loggerDisplay.Location = new System.Drawing.Point(0, 0);
      this.loggerDisplay.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
      this.loggerDisplay.MinLevel = Microsoft.Extensions.Logging.LogLevel.Debug;
      this.loggerDisplay.Name = "loggerDisplay";
      this.loggerDisplay.Paddings = new System.Windows.Forms.Padding(0);
      this.loggerDisplay.ReadOnly = true;
      this.loggerDisplay.SelectionColor = System.Drawing.Color.FromArgb(((int) (((byte) (60)))), ((int) (((byte) (0)))), ((int) (((byte) (0)))), ((int) (((byte) (255)))));
      this.loggerDisplay.ShowLineNumbers = false;
      this.loggerDisplay.Size = new System.Drawing.Size(900, 398);
      this.loggerDisplay.TabIndex = 2;
      this.loggerDisplay.Zoom = 100;
      // 
      // detailControl
      // 
      dataGridViewCellStyle1.BackColor = System.Drawing.Color.Gainsboro;
      this.detailControl.AlternatingRowDefaultCellStyle = dataGridViewCellStyle1;
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
      dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (254)));
      dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
      dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.detailControl.DefaultCellStyle = dataGridViewCellStyle2;
      this.detailControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.detailControl.FileSetting = null;
      this.detailControl.Location = new System.Drawing.Point(0, 0);
      this.detailControl.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
      this.detailControl.MenuDown = false;
      this.detailControl.Name = "detailControl";
      this.detailControl.Size = new System.Drawing.Size(900, 429);
      this.detailControl.TabIndex = 1;
      this.detailControl.WriteSetting = null;
      // 
      // toolStripLog
      // 
      this.toolStripLog.Dock = System.Windows.Forms.DockStyle.None;
      this.toolStripLog.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
      this.toolStripLog.ImageScalingSize = new System.Drawing.Size(24, 24);
      this.toolStripLog.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ToolStripButtonLoadFile2,
            this.m_ToolStripButtonShowLog2});
      this.toolStripLog.Location = new System.Drawing.Point(4, 0);
      this.toolStripLog.Name = "toolStripLog";
      this.toolStripLog.Size = new System.Drawing.Size(59, 31);
      this.toolStripLog.TabIndex = 5;
      this.toolStripLog.Text = "toolStripLog";
      // 
      // m_ToolStripButtonLoadFile2
      // 
      this.m_ToolStripButtonLoadFile2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.m_ToolStripButtonLoadFile2.Image = global::CsvTools.Properties.Resources.LoadFile;
      this.m_ToolStripButtonLoadFile2.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.m_ToolStripButtonLoadFile2.Name = "m_ToolStripButtonLoadFile2";
      this.m_ToolStripButtonLoadFile2.Size = new System.Drawing.Size(28, 28);
      this.m_ToolStripButtonLoadFile2.Text = "Open File";
      this.m_ToolStripButtonLoadFile2.Click += new System.EventHandler(this.ToolStripButtonLoadFile_Click);
      // 
      // m_ToolStripButtonShowLog2
      // 
      this.m_ToolStripButtonShowLog2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.m_ToolStripButtonShowLog2.Image = global::CsvTools.Properties.Resources.ShowLog;
      this.m_ToolStripButtonShowLog2.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.m_ToolStripButtonShowLog2.Name = "m_ToolStripButtonShowLog2";
      this.m_ToolStripButtonShowLog2.Size = new System.Drawing.Size(28, 28);
      this.m_ToolStripButtonShowLog2.Text = "Log";
      this.m_ToolStripButtonShowLog2.Click += new System.EventHandler(this.ToggleShowLog);
      // 
      // textPanel
      // 
      // 
      // textPanel.BottomToolStripPanel
      // 
      this.textPanel.BottomToolStripPanel.Controls.Add(this.toolStripLog);
      this.textPanel.BottomToolStripPanel.Controls.Add(this.toolStripDataGrid);
      // 
      // textPanel.ContentPanel
      // 
      this.textPanel.ContentPanel.Controls.Add(this.loggerDisplay);
      this.textPanel.ContentPanel.Margin = new System.Windows.Forms.Padding(2);
      this.textPanel.ContentPanel.Size = new System.Drawing.Size(900, 398);
      this.textPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textPanel.LeftToolStripPanelVisible = false;
      this.textPanel.Location = new System.Drawing.Point(0, 0);
      this.textPanel.Margin = new System.Windows.Forms.Padding(4);
      this.textPanel.Name = "textPanel";
      this.textPanel.Size = new System.Drawing.Size(900, 429);
      this.textPanel.TabIndex = 6;
      this.textPanel.Text = "toolStripContainer2";
      this.textPanel.TopToolStripPanelVisible = false;
      // 
      // toolStripDataGrid
      // 
      this.toolStripDataGrid.Dock = System.Windows.Forms.DockStyle.None;
      this.toolStripDataGrid.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
      this.toolStripDataGrid.ImageScalingSize = new System.Drawing.Size(24, 24);
      this.toolStripDataGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ToolStripButtonLoadFile,
            this.m_ToolStripButtonAsText,
            this.m_ToolStripButtonShowLog,
            this.m_ToolStripButtonSettings});
      this.toolStripDataGrid.Location = new System.Drawing.Point(239, 0);
      this.toolStripDataGrid.Name = "toolStripDataGrid";
      this.toolStripDataGrid.Size = new System.Drawing.Size(115, 31);
      this.toolStripDataGrid.TabIndex = 3;
      this.toolStripDataGrid.Text = "toolStrip";
      // 
      // m_ToolStripButtonLoadFile
      // 
      this.m_ToolStripButtonLoadFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.m_ToolStripButtonLoadFile.Image = global::CsvTools.Properties.Resources.LoadFile;
      this.m_ToolStripButtonLoadFile.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.m_ToolStripButtonLoadFile.Name = "m_ToolStripButtonLoadFile";
      this.m_ToolStripButtonLoadFile.Size = new System.Drawing.Size(28, 28);
      this.m_ToolStripButtonLoadFile.Text = "Open File";
      this.m_ToolStripButtonLoadFile.Click += new System.EventHandler(this.ToolStripButtonLoadFile_Click);
      // 
      // m_ToolStripButtonAsText
      // 
      this.m_ToolStripButtonAsText.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.m_ToolStripButtonAsText.Enabled = false;
      this.m_ToolStripButtonAsText.Image = global::CsvTools.Properties.Resources.AsText;
      this.m_ToolStripButtonAsText.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.m_ToolStripButtonAsText.Name = "m_ToolStripButtonAsText";
      this.m_ToolStripButtonAsText.Size = new System.Drawing.Size(28, 28);
      this.m_ToolStripButtonAsText.Text = "Text";
      this.m_ToolStripButtonAsText.Click += new System.EventHandler(this.ToggleDisplayAsText);
      // 
      // m_ToolStripButtonShowLog
      // 
      this.m_ToolStripButtonShowLog.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.m_ToolStripButtonShowLog.Image = global::CsvTools.Properties.Resources.ShowLog;
      this.m_ToolStripButtonShowLog.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.m_ToolStripButtonShowLog.Name = "m_ToolStripButtonShowLog";
      this.m_ToolStripButtonShowLog.Size = new System.Drawing.Size(28, 28);
      this.m_ToolStripButtonShowLog.Text = "Log";
      this.m_ToolStripButtonShowLog.Click += new System.EventHandler(this.ToggleShowLog);
      // 
      // m_ToolStripButtonSettings
      // 
      this.m_ToolStripButtonSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.m_ToolStripButtonSettings.Image = global::CsvTools.Properties.Resources.Settings;
      this.m_ToolStripButtonSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.m_ToolStripButtonSettings.Name = "m_ToolStripButtonSettings";
      this.m_ToolStripButtonSettings.Size = new System.Drawing.Size(28, 28);
      this.m_ToolStripButtonSettings.Text = "Setting";
      this.m_ToolStripButtonSettings.Click += new System.EventHandler(this.ShowSettings);
      // 
      // FormMain
      // 
      this.AllowDrop = true;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(900, 429);
      this.Controls.Add(this.textPanel);
      this.Controls.Add(this.detailControl);
      this.HelpButton = true;
      this.KeyPreview = true;
      this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.MinimumSize = new System.Drawing.Size(637, 152);
      this.Name = "FormMain";
      this.Activated += new System.EventHandler(this.FormMain_Activated);
      this.Load += new System.EventHandler(this.FormMain_Loaded);
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
      this.DragDrop += new System.Windows.Forms.DragEventHandler(this.FileDragDrop);
      this.DragEnter += new System.Windows.Forms.DragEventHandler(this.FileDragEnter);
      this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FormMain_KeyUpAsync);
      ((System.ComponentModel.ISupportInitialize) (this.fileSystemWatcher)).EndInit();
      ((System.ComponentModel.ISupportInitialize) (this.loggerDisplay)).EndInit();
      this.toolStripLog.ResumeLayout(false);
      this.toolStripLog.PerformLayout();
      this.textPanel.BottomToolStripPanel.ResumeLayout(false);
      this.textPanel.BottomToolStripPanel.PerformLayout();
      this.textPanel.ContentPanel.ResumeLayout(false);
      this.textPanel.ResumeLayout(false);
      this.textPanel.PerformLayout();
      this.toolStripDataGrid.ResumeLayout(false);
      this.toolStripDataGrid.PerformLayout();
      this.ResumeLayout(false);

    }

    private CsvTools.DetailControl detailControl;
    private System.IO.FileSystemWatcher fileSystemWatcher;
    private CsvTools.LoggerDisplay loggerDisplay;

    #endregion

    private System.Boolean m_DisposedValue;
    private System.Windows.Forms.ToolStrip toolStripLog;
    private System.Windows.Forms.ToolStripButton m_ToolStripButtonAsText;
    private System.Windows.Forms.ToolStripButton m_ToolStripButtonShowLog;
    private System.Windows.Forms.ToolStripButton m_ToolStripButtonSettings;
    private System.Windows.Forms.ToolStripContainer textPanel;
    private System.Windows.Forms.ToolStripButton m_ToolStripButtonLoadFile;
    private System.Windows.Forms.ToolStripButton m_ToolStripButtonLoadFile2;
    private System.Windows.Forms.ToolStripButton m_ToolStripButtonShowLog2;
    private System.Windows.Forms.ToolStrip toolStripDataGrid;
  }
}

