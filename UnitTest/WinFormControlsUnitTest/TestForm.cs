using System;
using System.Diagnostics;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;

namespace CsvTools.Tests
{
  public partial class TestForm : Form
  {
    private readonly CancellationTokenSource m_CancellationTokenSource;
    private readonly System.Timers.Timer m_Timer = new System.Timers.Timer();

    public TestForm()
    {
      m_CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(UnitTestInitializeCsv.Token);
      InitializeComponent();
    }

    public void AddOneControl(Control ctrl)
    {
      if (ctrl == null) return;

      SuspendLayout();
      Text = ctrl.GetType().FullName;
      ctrl.Dock = DockStyle.Fill;
      ctrl.Location = new Point(0, 0);
      ctrl.Size = new Size(790, 790);
      Controls.Add(ctrl);
      ResumeLayout(false);

      m_Timer.Interval = new TimeSpan(0,0,0,10).TotalMilliseconds; // 10 Seconds max
      m_Timer.Enabled = true;
      m_Timer.Start();
      m_Timer.Elapsed += (sender, args) => Close();
      Show();
    }

    public CancellationToken CancellationToken => m_CancellationTokenSource.Token;

    private void TestForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      m_CancellationTokenSource.Cancel();
      m_Timer.Stop();
      m_Timer.Enabled = false;
    } 
  }
}