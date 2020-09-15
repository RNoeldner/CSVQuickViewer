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
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
      this.fileSystemWatcher = new System.IO.FileSystemWatcher();
      this.textBoxProgress = new CsvTools.LoggerDisplay();
      this.textPanel = new System.Windows.Forms.Panel();          
      this.detailControl = new CsvTools.DetailControl();
      ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher)).BeginInit();
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
      this.textBoxProgress.BackColor = System.Drawing.SystemColors.Window;
      this.textBoxProgress.CausesValidation = false;
      this.textBoxProgress.Location = new System.Drawing.Point(2, 3);
      this.textBoxProgress.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.textBoxProgress.MinLevel = CsvTools.Logger.Level.Debug;
      this.textBoxProgress.Name = "textBoxProgress";
      this.textBoxProgress.ReadOnly = true;
      this.textBoxProgress.Size = new System.Drawing.Size(133, 160);
      this.textBoxProgress.TabIndex = 2;
      this.textBoxProgress.Text = "";
      // 
      // textPanel
      // 
      this.textPanel.Controls.Add(this.textBoxProgress);      
      this.textPanel.Location = new System.Drawing.Point(9, 40);
      this.textPanel.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.textPanel.Name = "textPanel";
      this.textPanel.Size = new System.Drawing.Size(415, 180);
      this.textPanel.TabIndex = 4;
      this.textPanel.Visible = false;
      // 
      // buttonCloseText
      // 


      // 
      // detailControl
      // 
      dataGridViewCellStyle1.BackColor = System.Drawing.Color.Gainsboro;
      this.detailControl.AlternatingRowDefaultCellStyle = dataGridViewCellStyle1;
      this.detailControl.DataTable = null;
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
      dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
      dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.detailControl.DefaultCellStyle = dataGridViewCellStyle2;
      this.detailControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.detailControl.Location = new System.Drawing.Point(0, 0);
      this.detailControl.Margin = new System.Windows.Forms.Padding(4);
      this.detailControl.Name = "detailControl";
      this.detailControl.Size = new System.Drawing.Size(1004, 539);
      this.detailControl.TabIndex = 1;
      this.detailControl.ButtonShowSource += new System.EventHandler(this.DetailControl_ButtonShowSource);
      this.detailControl.OnSettingsClick += new System.EventHandler(this.ShowSettings);
      // 
      // FormMain
      // 
      this.AllowDrop = true;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1004, 539);
      this.Controls.Add(this.detailControl);
      this.Controls.Add(this.textPanel);
      this.HelpButton = true;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.KeyPreview = true;
      this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.MinimumSize = new System.Drawing.Size(599, 137);
      this.Name = "FormMain";
      this.Activated += new System.EventHandler(this.FormMain_Activated);
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
      this.DragDrop += new System.Windows.Forms.DragEventHandler(this.DataGridView_DragDropAsync);
      this.DragEnter += new System.Windows.Forms.DragEventHandler(this.DataGridView_DragEnter);
      this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FormMain_KeyUpAsync);
      ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher)).EndInit();
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

