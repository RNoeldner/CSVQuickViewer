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
        DataTable?.Dispose();
        m_CancellationTokenSource?.Dispose();
        m_SettingsChangedTimerChange?.Dispose();
        m_SourceDisplay?.Dispose();
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      this.fileSystemWatcher = new System.IO.FileSystemWatcher();
      this.textBoxProgress = new CsvTools.LoggerDisplay();
      this.textPanel = new System.Windows.Forms.Panel();
      this.detailControl = new CsvTools.DetailControl();
      this.m_ToolStripButtonAsText = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripButtonSettings = new System.Windows.Forms.ToolStripButton();
      this.m_ToolStripButtonSource = new System.Windows.Forms.ToolStripButton();
      ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.textBoxProgress)).BeginInit();
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
      // textBoxProgress
      // 
      this.textBoxProgress.AutoCompleteBracketsList = new char[] {
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
      this.textBoxProgress.AutoScrollMinSize = new System.Drawing.Size(2, 14);
      this.textBoxProgress.BackBrush = null;
      this.textBoxProgress.BackColor = System.Drawing.SystemColors.Window;
      this.textBoxProgress.CausesValidation = false;
      this.textBoxProgress.CharHeight = 14;
      this.textBoxProgress.CharWidth = 8;
      this.textBoxProgress.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.textBoxProgress.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
      this.textBoxProgress.IsReplaceMode = false;
      this.textBoxProgress.Location = new System.Drawing.Point(2, 2);
      this.textBoxProgress.Margin = new System.Windows.Forms.Padding(2);
      this.textBoxProgress.MinLevel = CsvTools.Logger.Level.Debug;
      this.textBoxProgress.Name = "textBoxProgress";
      this.textBoxProgress.Paddings = new System.Windows.Forms.Padding(0);
      this.textBoxProgress.ReadOnly = true;
      this.textBoxProgress.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
      this.textBoxProgress.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("textBoxProgress.ServiceColors")));
      this.textBoxProgress.ShowLineNumbers = false;
      this.textBoxProgress.Size = new System.Drawing.Size(100, 130);
      this.textBoxProgress.TabIndex = 2;
      this.textBoxProgress.Zoom = 100;
      // 
      // textPanel
      // 
      this.textPanel.Controls.Add(this.textBoxProgress);
      this.textPanel.Location = new System.Drawing.Point(7, 32);
      this.textPanel.Margin = new System.Windows.Forms.Padding(2);
      this.textPanel.Name = "textPanel";
      this.textPanel.Size = new System.Drawing.Size(311, 146);
      this.textPanel.TabIndex = 4;
      this.textPanel.Visible = false;
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
      this.detailControl.Size = new System.Drawing.Size(753, 438);
      this.detailControl.TabIndex = 1;
      // 
      // m_ToolStripButtonAsText
      // 
      this.m_ToolStripButtonAsText.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonAsText.Image")));
      this.m_ToolStripButtonAsText.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.m_ToolStripButtonAsText.Name = "m_ToolStripButtonAsText";
      this.m_ToolStripButtonAsText.Size = new System.Drawing.Size(60, 25);
      this.m_ToolStripButtonAsText.Text = "Text";
      this.m_ToolStripButtonAsText.Click += new System.EventHandler(this.DetailControl_ButtonAsText);
      // 
      // m_ToolStripButtonSettings
      // 
      this.m_ToolStripButtonSettings.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonSettings.Image")));
      this.m_ToolStripButtonSettings.Name = "m_ToolStripButtonSettings";
      this.m_ToolStripButtonSettings.Size = new System.Drawing.Size(86, 25);
      this.m_ToolStripButtonSettings.Text = "Settings";
      this.m_ToolStripButtonSettings.ToolTipText = "Show CSV Settings";
      this.m_ToolStripButtonSettings.Click += new System.EventHandler(this.ShowSettings);
      // 
      // m_ToolStripButtonSource
      // 
      this.m_ToolStripButtonSource.Image = ((System.Drawing.Image)(resources.GetObject("m_ToolStripButtonSource.Image")));
      this.m_ToolStripButtonSource.Name = "m_ToolStripButtonSource";
      this.m_ToolStripButtonSource.Size = new System.Drawing.Size(114, 25);
      this.m_ToolStripButtonSource.Text = "View Source";
      this.m_ToolStripButtonSource.Click += new System.EventHandler(this.DetailControl_ButtonShowSource);
      // 
      // FormMain
      // 
      this.AllowDrop = true;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(753, 438);
      this.Controls.Add(this.detailControl);
      this.Controls.Add(this.textPanel);
      this.HelpButton = true;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.KeyPreview = true;
      this.Margin = new System.Windows.Forms.Padding(2);
      this.MinimumSize = new System.Drawing.Size(453, 119);
      this.Name = "FormMain";
      this.Activated += new System.EventHandler(this.FormMain_Activated);
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
      this.DragDrop += new System.Windows.Forms.DragEventHandler(this.DataGridView_DragDropAsync);
      this.DragEnter += new System.Windows.Forms.DragEventHandler(this.DataGridView_DragEnter);
      this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FormMain_KeyUpAsync);
      ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.textBoxProgress)).EndInit();
      this.textPanel.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    private CsvTools.DetailControl detailControl;
    private System.IO.FileSystemWatcher fileSystemWatcher;
    private CsvTools.LoggerDisplay textBoxProgress;
    private System.Windows.Forms.Panel textPanel;

    #endregion

    private System.Boolean m_DisposedValue;
  }
}

