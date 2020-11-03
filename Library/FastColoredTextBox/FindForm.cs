using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FastColoredTextBoxNS
{
  public partial class FindForm : Form
  {
    private bool firstSearch = true;
    private Place startPlace;
    private readonly FastColoredTextBox tb;

    public FindForm(FastColoredTextBox tb)
    {
      InitializeComponent();
      this.tb = tb;
    }

    private void btClose_Click(object sender, EventArgs e) => Close();

    private void btFindNext_Click(object sender, EventArgs e) => FindNext(tbFind.Text);

    public virtual void FindNext(string pattern)
    {
      try
      {
        var opt = cbMatchCase.Checked ? RegexOptions.None : RegexOptions.IgnoreCase;
        if (!cbRegex.Checked)
          pattern = Regex.Escape(pattern);
        if (cbWholeWord.Checked)
          pattern = "\\b" + pattern + "\\b";
        //
        var range = tb.Selection.Clone();
        range.Normalize();
        //
        if (firstSearch)
        {
          startPlace = range.Start;
          firstSearch = false;
        }
        //
        range.Start = range.End;
        if (range.Start >= startPlace)
          range.End = new Place(tb.GetLineLength(tb.LinesCount - 1), tb.LinesCount - 1);
        else
          range.End = startPlace;
        //
        foreach (var r in range.GetRangesByLines(pattern, opt))
        {
          tb.Selection = r;
          tb.DoSelectionVisible();
          tb.Invalidate();
          return;
        }
        //
        if (range.Start >= startPlace && startPlace > Place.Empty)
        {
          tb.Selection.Start = new Place(0, 0);
          FindNext(pattern);
          return;
        }
        MessageBox.Show("Not found");
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message);
      }
    }

    private void tbFind_KeyPress(object sender, KeyPressEventArgs e)
    {
      if (e.KeyChar == '\r')
      {
        btFindNext.PerformClick();
        e.Handled = true;
        return;
      }
      if (e.KeyChar == '\x1b')
      {
        Hide();
        e.Handled = true;
        return;
      }
    }

    private void FindForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      if (e.CloseReason == CloseReason.UserClosing)
      {
        e.Cancel = true;
        Hide();
      }
      tb.Focus();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
      if (keyData == Keys.Escape)
      {
        Close();
        return true;
      }
      return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnActivated(EventArgs e)
    {
      tbFind.Focus();
      ResetSerach();
    }

    private void ResetSerach() => firstSearch = true;

    private void cbMatchCase_CheckedChanged(object sender, EventArgs e) => ResetSerach();
  }
}
