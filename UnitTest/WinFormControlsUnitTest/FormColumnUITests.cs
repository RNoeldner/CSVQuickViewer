using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class FormColumnUITests
  {
    [TestMethod]
    [Timeout(2000)]
    public void FormColumnUI_TextPart()
    {
      var col = new Column("MyTest", DataType.TextPart) { PartSplitter = ':', Part = 2, PartToEnd = true };
      using (var frm = new FormColumnUI(col, false, new CsvFile(), new FillGuessSettings(), true))
      {
        UnitTestWinFormHelper.ShowFormAndClose(frm, .1, f => f.SetPartLabels(":", "2", true));
      }
    }

    [TestMethod]
    [Timeout(2000)]
    public void FormColumnUI_Boolean()
    {
      var col = new Column("MyTest", DataType.Boolean) { True = "YO", False = "NOPE" };
      using (var frm = new FormColumnUI(col, false, new CsvFile(), new FillGuessSettings(), true))
      {
        UnitTestWinFormHelper.ShowFormAndClose(frm);
      }
    }

    [TestMethod]
    [Timeout(2000)]
    public void FormColumnUI_Numeric()
    {
      var col = new Column("MyTest", DataType.Numeric) { DecimalSeparator = ".", GroupSeparator = ",", NumberFormat = "0.00" };

      using (var frm = new FormColumnUI(col, false, new CsvFile(), new FillGuessSettings(), true))
      {
        UnitTestWinFormHelper.ShowFormAndClose(frm, .1, f => f.UpdateNumericLabel(".", "00000", ""));
      }
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormColumnUI_DateTime()
    {
      var col = new Column("MyTest", DataType.DateTime) { DateFormat = "dd/MM/yyyy", DateSeparator = ".", TimeSeparator = ":" };

      var df = new ValueFormatMutable(DataType.DateTime) { DateFormat = "dd/MMM/yyy", DateSeparator = "-", TimeSeparator = "#" };
      using (var frm = new FormColumnUI(col, false, new CsvFile(), new FillGuessSettings(), true))
        UnitTestWinFormHelper.ShowFormAndClose(frm, .1, f => f.UpdateDateLabel(df, true, "HH:mm", "[UTC]"));

      using (var frm = new FormColumnUI(col, false, new CsvFile(), new FillGuessSettings(), true))
        UnitTestWinFormHelper.ShowFormAndClose(frm, .1, f => f.UpdateDateLabel(df, false, "HH:mm:ss", "OtherColumn"));

      using (var frm = new FormColumnUI(col, false, new CsvFile(), new FillGuessSettings(), true))
        UnitTestWinFormHelper.ShowFormAndClose(frm, .1, f => f.AddDateFormat("dd MMM yy HH:mm tt"));
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormColumnUI()
    {
      var csvFile = new CsvFile(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"));
      var col = new Column("ExamDate", DataType.DateTime);
      csvFile.ColumnCollection.AddIfNew(col);

      using (var frm = new FormColumnUI(col, false, csvFile, new FillGuessSettings(), false))
      {
        UnitTestWinFormHelper.ShowFormAndClose(frm);
      }
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormColumnUIGetColumnHeader2()
    {
      var csvFile = new CsvFile { ID = "Csv", FileName = UnitTestInitializeCsv.GetTestPath("BasicCSV.txt") };
      var col = new Column("Score", DataType.Double);
      csvFile.ColumnCollection.AddIfNew(col);

      using (var form = new FormColumnUI(col, false, csvFile, new FillGuessSettings(), true))
      {
        UnitTestWinFormHelper.ShowFormAndClose(form);
      }
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormColumnUIGetColumnHeaderAsync()
    {
      var csvFile = new CsvFile(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt")) { ID = "Csv" };

      csvFile.ColumnCollection.AddIfNew(new Column("ID", DataType.Integer));
      csvFile.ColumnCollection.AddIfNew(new Column("ExamDate", DataType.DateTime));
      csvFile.ColumnCollection.AddIfNew(new Column("Score", DataType.Double));

      using (var form = new FormColumnUI(csvFile.ColumnCollection.Get("ExamDate"), false, csvFile,
        new FillGuessSettings(), true))
      {
        UnitTestWinFormHelper.ShowFormAndClose(form);
      }
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormColumnUI_Opt1()
    {
      var csvFile = new CsvFile(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"));
      var col = new Column("ExamDate", DataType.DateTime);
      csvFile.ColumnCollection.AddIfNew(col);

      using (var form = new FormColumnUI(col, false, csvFile, new FillGuessSettings(), true))
      {
        form.ShowGuess = false;
        UnitTestWinFormHelper.ShowFormAndClose(form);
      }
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormColumnUI_Opt2()
    {
      var csvFile = new CsvFile(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"));
      var col = new Column("ID", DataType.Integer);
      csvFile.ColumnCollection.AddIfNew(col);

      using (var form = new FormColumnUI(col, false, csvFile, new FillGuessSettings(), false))
      {
        UnitTestWinFormHelper.ShowFormAndClose(form);
      }
    }

    [TestMethod]
    [Timeout(15000)]
    public async Task FormColumnUI_Guess()
    {
      var csvFile = new CsvFile(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"));
      var col = new Column("ExamDate", DataType.DateTime);
      csvFile.ColumnCollection.AddIfNew(col);
      using (var form = new FormColumnUI(col, false, csvFile, new FillGuessSettings(), true))
      {
        await UnitTestWinFormHelper.ShowFormAndCloseAsync(form, .2, frm => frm.Guess());
      }
    }

    [TestMethod]
    [Timeout(15000)]
    public async Task FormColumnUI_DisplayValues()
    {
      var csvFile = new CsvFile(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"));
      var col = new Column("ExamDate", DataType.DateTime);
      csvFile.ColumnCollection.AddIfNew(col);

      using (var form = new FormColumnUI(col, false, csvFile, new FillGuessSettings(), true))
      {
        await UnitTestWinFormHelper.ShowFormAndCloseAsync(form, .2, frm => frm.DisplayValues());
      }
    }
  }
}