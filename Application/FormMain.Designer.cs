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
      m_CancellationTokenSource?.Cancel();
      if (disposing)
      {
#if SupportPGP
        PgpHelper.ClearPgpInfo();
#endif
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
      var dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      var resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
      var dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      fileSystemWatcher = new System.IO.FileSystemWatcher();
      loggerDisplay = new LoggerDisplay();
      detailControl = new DetailControl();
      toolStripLog = new System.Windows.Forms.ToolStrip();
      m_ToolStripButtonLoadFile2 = new System.Windows.Forms.ToolStripButton();
      m_ToolStripButtonShowLog2 = new System.Windows.Forms.ToolStripButton();
      textPanel = new System.Windows.Forms.ToolStripContainer();
      toolStripDataGrid = new System.Windows.Forms.ToolStrip();
      m_ToolStripButtonLoadFile = new System.Windows.Forms.ToolStripButton();
      m_ToolStripButtonAsText = new System.Windows.Forms.ToolStripButton();
      m_ToolStripButtonShowLog = new System.Windows.Forms.ToolStripButton();
      m_ToolStripButtonSettings = new System.Windows.Forms.ToolStripButton();
      ((System.ComponentModel.ISupportInitialize) fileSystemWatcher).BeginInit();
      toolStripLog.SuspendLayout();
      textPanel.BottomToolStripPanel.SuspendLayout();
      textPanel.ContentPanel.SuspendLayout();
      textPanel.SuspendLayout();
      toolStripDataGrid.SuspendLayout();
      SuspendLayout();
      // 
      // fileSystemWatcher
      // 
      fileSystemWatcher.EnableRaisingEvents = true;
      fileSystemWatcher.Filter = "*.*";
      fileSystemWatcher.NotifyFilter =  System.IO.NotifyFilters.Size | System.IO.NotifyFilters.LastWrite;
      fileSystemWatcher.SynchronizingObject = this;
      fileSystemWatcher.Changed += FileSystemWatcher_Changed;
      // 
      // loggerDisplay
      // 
      loggerDisplay.BackColor = System.Drawing.SystemColors.Window;
      loggerDisplay.CausesValidation = false;
      loggerDisplay.Cursor = System.Windows.Forms.Cursors.IBeam;
      loggerDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
      loggerDisplay.Location = new System.Drawing.Point(0, 0);
      loggerDisplay.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
      loggerDisplay.MinLevel = Microsoft.Extensions.Logging.LogLevel.Debug;
      loggerDisplay.Multiline = true;
      loggerDisplay.Name = "loggerDisplay";
      loggerDisplay.ReadOnly = true;
      loggerDisplay.Size = new System.Drawing.Size(913, 400);
      loggerDisplay.TabIndex = 2;
      // 
      // detailControl
      // 
      dataGridViewCellStyle1.BackColor = System.Drawing.Color.Gainsboro;
      detailControl.AlternatingRowDefaultCellStyle = dataGridViewCellStyle1;
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
      dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point,  0);
      dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
      dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      detailControl.DefaultCellStyle = dataGridViewCellStyle2;
      detailControl.Dock = System.Windows.Forms.DockStyle.Fill;
      detailControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point,  0);
      detailControl.Location = new System.Drawing.Point(0, 0);
      detailControl.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
      detailControl.MenuDown = false;
      detailControl.Name = "detailControl";
      detailControl.ShowButtonAtLength = 1000;
      detailControl.Size = new System.Drawing.Size(913, 431);
      detailControl.TabIndex = 1;
      // 
      // toolStripLog
      // 
      toolStripLog.Dock = System.Windows.Forms.DockStyle.None;
      toolStripLog.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
      toolStripLog.ImageScalingSize = new System.Drawing.Size(24, 24);
      toolStripLog.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { m_ToolStripButtonLoadFile2, m_ToolStripButtonShowLog2 });
      toolStripLog.Location = new System.Drawing.Point(4, 0);
      toolStripLog.Name = "toolStripLog";
      toolStripLog.Size = new System.Drawing.Size(59, 31);
      toolStripLog.TabIndex = 5;
      toolStripLog.Text = "toolStripLog";
      // 
      // m_ToolStripButtonLoadFile2
      // 
      m_ToolStripButtonLoadFile2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      m_ToolStripButtonLoadFile2.Image = Properties.Resources.LoadFile;
      m_ToolStripButtonLoadFile2.ImageTransparentColor = System.Drawing.Color.Magenta;
      m_ToolStripButtonLoadFile2.Name = "m_ToolStripButtonLoadFile2";
      m_ToolStripButtonLoadFile2.Size = new System.Drawing.Size(28, 28);
      m_ToolStripButtonLoadFile2.Text = "Open File";
      m_ToolStripButtonLoadFile2.Click += ToolStripButtonLoadFile_Click;
      // 
      // m_ToolStripButtonShowLog2
      // 
      m_ToolStripButtonShowLog2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      m_ToolStripButtonShowLog2.Image = Properties.Resources.ShowLog;
      m_ToolStripButtonShowLog2.ImageTransparentColor = System.Drawing.Color.Magenta;
      m_ToolStripButtonShowLog2.Name = "m_ToolStripButtonShowLog2";
      m_ToolStripButtonShowLog2.Size = new System.Drawing.Size(28, 28);
      m_ToolStripButtonShowLog2.Text = "Log";
      m_ToolStripButtonShowLog2.Click += ToggleShowLog;
      // 
      // textPanel
      // 
      // 
      // textPanel.BottomToolStripPanel
      // 
      textPanel.BottomToolStripPanel.Controls.Add(toolStripLog);
      textPanel.BottomToolStripPanel.Controls.Add(toolStripDataGrid);
      // 
      // textPanel.ContentPanel
      // 
      textPanel.ContentPanel.Controls.Add(loggerDisplay);
      textPanel.ContentPanel.Margin = new System.Windows.Forms.Padding(2);
      textPanel.ContentPanel.Size = new System.Drawing.Size(913, 400);
      textPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      textPanel.LeftToolStripPanelVisible = false;
      textPanel.Location = new System.Drawing.Point(0, 0);
      textPanel.Margin = new System.Windows.Forms.Padding(4);
      textPanel.Name = "textPanel";
      textPanel.Size = new System.Drawing.Size(913, 431);
      textPanel.TabIndex = 6;
      textPanel.Text = "toolStripContainer2";
      textPanel.TopToolStripPanelVisible = false;
      // 
      // toolStripDataGrid
      // 
      toolStripDataGrid.Dock = System.Windows.Forms.DockStyle.None;
      toolStripDataGrid.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
      toolStripDataGrid.ImageScalingSize = new System.Drawing.Size(24, 24);
      toolStripDataGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { m_ToolStripButtonLoadFile, m_ToolStripButtonAsText, m_ToolStripButtonShowLog, m_ToolStripButtonSettings });
      toolStripDataGrid.Location = new System.Drawing.Point(239, 0);
      toolStripDataGrid.Name = "toolStripDataGrid";
      toolStripDataGrid.Size = new System.Drawing.Size(115, 31);
      toolStripDataGrid.TabIndex = 3;
      toolStripDataGrid.Text = "toolStrip";
      // 
      // m_ToolStripButtonLoadFile
      // 
      m_ToolStripButtonLoadFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      m_ToolStripButtonLoadFile.Image = Properties.Resources.LoadFile;
      m_ToolStripButtonLoadFile.ImageTransparentColor = System.Drawing.Color.Magenta;
      m_ToolStripButtonLoadFile.Name = "m_ToolStripButtonLoadFile";
      m_ToolStripButtonLoadFile.Size = new System.Drawing.Size(28, 28);
      m_ToolStripButtonLoadFile.Text = "Open File";
      m_ToolStripButtonLoadFile.Click += ToolStripButtonLoadFile_Click;
      // 
      // m_ToolStripButtonAsText
      // 
      m_ToolStripButtonAsText.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      m_ToolStripButtonAsText.Enabled = false;
      m_ToolStripButtonAsText.Image = Properties.Resources.AsText;
      m_ToolStripButtonAsText.ImageTransparentColor = System.Drawing.Color.Magenta;
      m_ToolStripButtonAsText.Name = "m_ToolStripButtonAsText";
      m_ToolStripButtonAsText.Size = new System.Drawing.Size(28, 28);
      m_ToolStripButtonAsText.Text = "Text";
      m_ToolStripButtonAsText.Click += ToggleDisplayAsText;
      // 
      // m_ToolStripButtonShowLog
      // 
      m_ToolStripButtonShowLog.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      m_ToolStripButtonShowLog.Image = Properties.Resources.ShowLog;
      m_ToolStripButtonShowLog.ImageTransparentColor = System.Drawing.Color.Magenta;
      m_ToolStripButtonShowLog.Name = "m_ToolStripButtonShowLog";
      m_ToolStripButtonShowLog.Size = new System.Drawing.Size(28, 28);
      m_ToolStripButtonShowLog.Text = "Log";
      m_ToolStripButtonShowLog.Click += ToggleShowLog;
      // 
      // m_ToolStripButtonSettings
      // 
      m_ToolStripButtonSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      m_ToolStripButtonSettings.Image = Properties.Resources.Settings;
      m_ToolStripButtonSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
      m_ToolStripButtonSettings.Name = "m_ToolStripButtonSettings";
      m_ToolStripButtonSettings.Size = new System.Drawing.Size(28, 28);
      m_ToolStripButtonSettings.Text = "Setting";
      m_ToolStripButtonSettings.Click += ShowSettings;
      // 
      // FormMain
      // 
      AllowDrop = true;
      ClientSize = new System.Drawing.Size(913, 431);
      Controls.Add(textPanel);
      Controls.Add(detailControl);
      HelpButton = true;
      KeyPreview = true;
      Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      MinimumSize = new System.Drawing.Size(637, 152);
      Name = "FormMain";
      Activated += FormMain_Activated;
      FormClosing += FormMain_FormClosing;
      Load += FormMain_Loaded;
      DragDrop += FileDragDrop;
      DragEnter += FileDragEnter;
      KeyUp += FormMain_KeyUpAsync;
      ((System.ComponentModel.ISupportInitialize) fileSystemWatcher).EndInit();
      toolStripLog.ResumeLayout(false);
      toolStripLog.PerformLayout();
      textPanel.BottomToolStripPanel.ResumeLayout(false);
      textPanel.BottomToolStripPanel.PerformLayout();
      textPanel.ContentPanel.ResumeLayout(false);
      textPanel.ContentPanel.PerformLayout();
      textPanel.ResumeLayout(false);
      textPanel.PerformLayout();
      toolStripDataGrid.ResumeLayout(false);
      toolStripDataGrid.PerformLayout();
      ResumeLayout(false);

    }

    private CsvTools.DetailControl detailControl;
    private System.IO.FileSystemWatcher fileSystemWatcher;
    private CsvTools.LoggerDisplay loggerDisplay;

    #endregion

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

