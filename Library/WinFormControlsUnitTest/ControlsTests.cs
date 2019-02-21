using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Windows.Forms;

namespace CsvTools.Tests
{
  [TestClass]
  public class ControlsTests
  {
    private static readonly DataTable dataTable = UnitTestStatic.GetDataTable(50);

    [TestMethod]
    public void CsvRichTextBox()
    {
      var ctrl = new CSVRichTextBox
      {
        Text = "This is a Test"
      };
      Assert.AreEqual("This is a Test", ctrl.Text);

      ctrl.Delimiter = ';';
      Assert.AreEqual(';', ctrl.Delimiter);

      ctrl.DisplaySpace = true;
      Assert.IsTrue(ctrl.DisplaySpace);
      ctrl.DisplaySpace = false;
      Assert.IsFalse(ctrl.DisplaySpace);

      ctrl.Escape = '#';
      Assert.AreEqual('#', ctrl.Escape);

      ctrl.Quote = '?';
      Assert.AreEqual('?', ctrl.Quote);
    }

    [TestMethod]
    public void CsvTextDisplay()
    {
      var ctrl = new CsvTextDisplay();
      var fileName = FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles\BasicCSV.txt";
      var file = new CsvFile(fileName);

      ctrl.CsvFile = file;
    }

    [TestMethod]
    public void FormColumnUI()
    {
      var csvFile = new CsvFile();
      csvFile.FileName = FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles\BasicCSV.txt";
      var col = new Column();
      col.Name = "ExamDate";
      col.DataType = DataType.DateTime;

      csvFile.ColumnAdd(col);
      using (var ctrl = new FormColumnUI(col, false, csvFile))
      {
        ctrl.Show();
        ctrl.Close();
      }
    }

    [TestMethod]
    public void FormHierachyDisplay()
    {
      using (var ctrl = new FormHierachyDisplay(dataTable, dataTable.Select()))
      {
        ctrl.Show();
        ctrl.Close();
      }
    }

    [TestMethod]
    public void FormDuplicatesDisplay()
    {
      using (var ctrl = new FormDuplicatesDisplay(dataTable, dataTable.Select(), dataTable.Columns[0].ColumnName))
      {
        ctrl.Show();
        ctrl.Close();
      }
    }

    [TestMethod]
    public void FormUniqueDisplay()
    {
      using (var ctrl = new FormUniqueDisplay(dataTable, dataTable.Select(), dataTable.Columns[0].ColumnName))
      {
        ctrl.Show();
        ctrl.Close();
      }
    }

    [TestMethod]
    public void FormKeyFile()
    {
      using (var ctrl = new FormKeyFile("Test", true))
      {
        ctrl.Show();
        ctrl.Close();
      }
    }

    [TestMethod]
    public void FormShowMaxLength()
    {
      using (var ctrl = new FormShowMaxLength(dataTable, dataTable.Select()))
      {
        ctrl.Show();
        ctrl.Close();
      }
    }

    [TestMethod]
    public void FormDetail()
    {
      using (var ctrl = new FormDetail(dataTable, null, null, true, false, 0))
      {
        ctrl.Show();
        ctrl.Close();
      }
    }

    [TestMethod]
    public void DataGridViewColumnFilterControl()
    {
      var col = new DataGridViewTextBoxColumn
      {
        ValueType = dataTable.Columns[0].DataType,
        Name = dataTable.Columns[0].ColumnName,
        DataPropertyName = dataTable.Columns[0].ColumnName,
        Tag = dataTable.Columns[0].DataType
      };

      using (var ctrl = new DataGridViewColumnFilterControl(dataTable.Columns[0].DataType, col))
      {
        ctrl.Show();
        ctrl.FocusInput();
      }
    }

    [TestMethod]
    public void FormPassphrase()
    {
      using (var ctrl = new FormPassphrase("Test"))
      {
        ctrl.Show();
        ctrl.Close();
      }
    }
  }
}