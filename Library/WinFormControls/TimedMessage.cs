using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvTools
{
  public partial class TimedMessage : Form
  {
    private double m_Duration = 3.0;

    public TimedMessage()
    {
      InitializeComponent();
    }

    private int counter = 0;

    public double Duration
    {
      get => m_Duration;
      set => m_Duration = value;
    }

    public string Message
    {
      get => richTextBox.Text;
      set => richTextBox.Text = value;
    }

    public string MessageRtf
    {
      get => richTextBox.Rtf;
      set => richTextBox.Rtf = value;
    }

    public DialogResult Show(Form owner, string message, string title, double timeout = 3.0)
    {
      Text = title;
      Message = message;
      m_Duration = timeout;
      timer.Enabled = true;
      return this.ShowDialog(owner);
    }

    private void timer_Tick(object sender, EventArgs e)
    {
      counter++;
      label.Text = $"Closing in {(counter * timer.Interval) / 1000:N0} seconds";
      if ((counter * timer.Interval) / 1000 > m_Duration)
        button1_Click(sender, e);
    }

    private void button1_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
      Close();
    }
  }
}