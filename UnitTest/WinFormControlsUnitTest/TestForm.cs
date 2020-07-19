using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace CsvTools.Tests
{
  public partial class TestForm : Form
  {
    private readonly CancellationTokenSource m_CancellationTokenSource;
    private readonly Timer m_Timer = new Timer();

    public TestForm()
    {
      m_CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(UnitTestInitializeCsv.Token);
      InitializeComponent();
    }

    public CancellationToken CancellationToken => m_CancellationTokenSource.Token;

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

      m_Timer.Interval = new TimeSpan(0, 0, 0, 10).TotalMilliseconds; // 10 Seconds max
      m_Timer.Enabled = true;
      m_Timer.Start();
      m_Timer.Elapsed += (sender, args) => Close();
      Show();
    }

    private void TestForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      m_CancellationTokenSource.Cancel();
      m_Timer.Stop();
      m_Timer.Enabled = false;
    }
  }
}