using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Windows.Forms;

namespace CsvTools.Tests
{
  [TestClass]
  public class FilteredDataGridViewTests
  {
    [TestMethod]
    public void FilteredDataGridViewTest()
    {
      using (var fdgv = new FilteredDataGridView())
      {
        Assert.IsNotNull(fdgv);
      }
    }

    [TestMethod]
    public void ApplyFiltersTest()
    {
      using (var fdgv = new FilteredDataGridView())
      {
        fdgv.ApplyFilters();
      }

      using (var fdgv = new FilteredDataGridView())
      {
        var comboBoxColumn = new DataGridViewComboBoxColumn();
        comboBoxColumn.Items.AddRange(Color.Red, Color.Yellow, Color.Green);
        comboBoxColumn.ValueType = typeof(Color);

        var boolColumn = new DataGridViewCheckBoxColumn();
        fdgv.Columns.Add(boolColumn);

        fdgv.ApplyFilters();
      }

      using (var filteredDataGridView = new FilteredDataGridView())
      {
        filteredDataGridView.DataSource = UnitTestStatic.GetDataTable(100);
        using (var frm = new Form())
        {
          frm.Controls.Add(filteredDataGridView);
          frm.Show();
          filteredDataGridView.ApplyFilters();
        }
      }
    }

    [TestMethod]
    public void HideEmptyColumnsTest()
    {
      using (var filteredDataGridView = new FilteredDataGridView())
      {
        var dt = UnitTestStatic.GetDataTable(100);
        filteredDataGridView.DataSource = dt;
        using (var frm = new Form())
        {
          frm.Controls.Add(filteredDataGridView);
          frm.Show();
          var numCol = 0;
          foreach (DataGridViewColumn col in filteredDataGridView.Columns)
            if (col.Visible)
              numCol++;
          Assert.AreEqual(numCol, dt.Columns.Count);
          filteredDataGridView.HideEmptyColumns();

          numCol = 0;
          foreach (DataGridViewColumn col in filteredDataGridView.Columns)
            if (col.Visible)
              numCol++;

          Assert.AreEqual(numCol + 1, dt.Columns.Count);
        }
      }
    }

    [TestMethod]
    public void SetRowHeightTest()
    {
      using (var filteredDataGridView = new FilteredDataGridView())
      {
        filteredDataGridView.DataSource = UnitTestStatic.GetDataTable(100);
        using (var frm = new Form())
        {
          frm.Controls.Add(filteredDataGridView);
          frm.Show();
          filteredDataGridView.SetRowHeight();
        }
      }
    }
  }
}