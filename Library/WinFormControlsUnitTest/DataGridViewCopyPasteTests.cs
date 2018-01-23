using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Windows.Forms;

namespace CsvTools.Tests
{
  [TestClass]
  public class DataGridViewCopyPasteTests
  {
    [TestMethod]
    public void SelectedDataIntoClipboardAllTest()
    {
      using (var dgv = new DataGridView())
      {
        dgv.AutoGenerateColumns = true;
        dgv.DataSource = Static.GetDataTable(100);
        using (var frm = new Form())
        {
          frm.Controls.Add(dgv);
          frm.Show();
          dgv.SelectAll();
          dgv.SelectedDataIntoClipboard(CancellationToken.None, true, false);
        }
      }
    }

    [TestMethod]
    public void SelectedDataIntoClipboardTest()
    {
      using (var dgv = new DataGridView())
      {
        dgv.AutoGenerateColumns = true;
        dgv.DataSource = Static.GetDataTable(100);
        using (var frm = new Form())
        {
          frm.Controls.Add(dgv);
          frm.Show();

          dgv.SelectedDataIntoClipboard(CancellationToken.None, true, false);
        }
      }
    }
  }
}