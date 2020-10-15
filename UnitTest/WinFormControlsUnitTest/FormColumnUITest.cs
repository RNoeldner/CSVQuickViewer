using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class FormColumnUITest
  {
    [TestMethod]
    [Timeout(5000)]
    public void FormColumnUI()
    {
      var csvFile = new CsvFile(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"));
      var col = new Column("ExamDate", DataType.DateTime);
      csvFile.ColumnCollection.AddIfNew(col);
      Extensions.RunSTAThread(() =>
      {
        using (var frm = new FormColumnUI(col, false, csvFile, new FillGuessSettings(), false))
        {
          UnitTestWinFormHelper.ShowFormAndClose(frm);
        }
      });
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormColumnUIGetColumnHeader2()
    {
      var csvFile = new CsvFile {ID = "Csv", FileName = UnitTestInitializeCsv.GetTestPath("BasicCSV.txt")};
      var col = new Column("Score", DataType.Double);
      csvFile.ColumnCollection.AddIfNew(col);

      Extensions.RunSTAThread(() =>
      {
        using (var form = new FormColumnUI(col, false, csvFile, new FillGuessSettings(), true))

        {
          UnitTestWinFormHelper.ShowFormAndClose(form);
        }
      });
    }


    [TestMethod]
    [Timeout(5000)]
    public void FormColumnUIGetColumnHeaderAsync()
    {
      var csvFile = new CsvFile(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt")) {ID = "Csv"};

      csvFile.ColumnCollection.AddIfNew(new Column("ID", DataType.Integer));
      csvFile.ColumnCollection.AddIfNew(new Column("ExamDate", DataType.DateTime));
      csvFile.ColumnCollection.AddIfNew(new Column("Score", DataType.Double));

      Extensions.RunSTAThread(() =>
      {
        using (var form = new FormColumnUI(csvFile.ColumnCollection.Get("ExamDate"), false, csvFile,
          new FillGuessSettings(), true))
        {
          UnitTestWinFormHelper.ShowFormAndClose(form);
        }
      });
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormColumnUI_Opt1()
    {
      var csvFile = new CsvFile(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"));
      var col = new Column("ExamDate", DataType.DateTime);
      csvFile.ColumnCollection.AddIfNew(col);
      Extensions.RunSTAThread(() =>
      {
        using (var form = new FormColumnUI(col, false, csvFile, new FillGuessSettings(), true))
        {
          form.ShowGuess = false;
          UnitTestWinFormHelper.ShowFormAndClose(form);
        }
      });
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormColumnUI_Opt2()
    {
      var csvFile = new CsvFile(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"));
      var col = new Column("ID", DataType.Integer);
      csvFile.ColumnCollection.AddIfNew(col);
      Extensions.RunSTAThread(() =>
      {
        using (var form = new FormColumnUI(col, false, csvFile, new FillGuessSettings(), false))
        {
          UnitTestWinFormHelper.ShowFormAndClose(form);
        }
      });
    }

    [TestMethod]
    [Timeout(15000)]
    public void FormColumnUI_ButtonGuessClick()
    {
      var csvFile = new CsvFile(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"));
      var col = new Column("ExamDate", DataType.DateTime);
      csvFile.ColumnCollection.AddIfNew(col);
      Extensions.RunSTAThread(() =>
      {
        using (var form = new FormColumnUI(col, false, csvFile, new FillGuessSettings(), true))
        {
          UnitTestWinFormHelper.ShowFormAndClose(form, .2, (frm) => frm.ButtonGuessClick(null, null));
        }
      });
    }
  }
}