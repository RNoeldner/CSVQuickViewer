using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.IO;
using System.Threading;

namespace CsvTools.Tests
{
  [TestClass]
  public class DetermineColumnFormatTest
  {
    private readonly string m_ApplicationDirectory = FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles";

    [TestMethod]
    public void FillGuessColumnFormat_DoNotIgnoreID()
    {
      var setting = new CsvFile();
      setting.FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt");
      setting.FileFormat.FieldDelimiter = ",";
      setting.HasFieldHeader = true;
      // setting.TreatTextNullAsNull = true;
      ApplicationSetting.FillGuessSettings.IgnoreIdColums = false;
      using (var processDisplay = new DummyProcessDisplay())
      {
        setting.FillGuessColumnFormatReader(false, processDisplay);
        Assert.AreEqual(DataType.Integer, setting.GetColumn("ID").DataType);
        Assert.AreEqual(DataType.DateTime, setting.GetColumn("ExamDate").DataType);
        Assert.AreEqual(DataType.Boolean, setting.GetColumn("IsNativeLang").DataType);
      }
    }

    [TestMethod]
    public void DetermineColumnFormatGetSampleValues()
    {
      using (var dt = new DataTable())
      {
        using (var processDisplay = new DummyProcessDisplay())
        {
          var res = DetermineColumnFormat.GetSampleValues(dt, processDisplay.CancellationToken, 0, 20, string.Empty);
          Assert.AreEqual(0, res.Count());
        }
      }
    }

    [TestMethod]
    public void DetermineColumnFormatGetSampleValues2()
    {
      using (var dt = new DataTable())
      {
        dt.Columns.Add(new DataColumn
        {
          ColumnName = "ID",
          DataType = typeof(string)
        });
        for (var i = 0; i < 150; i++)
        {
          var row = dt.NewRow();
          if (i == 10 || i == 47)
            row[0] = "NULL";
          row[0] = (i / 3).ToString();
          dt.Rows.Add(row);
        }

        using (var processDisplay = new DummyProcessDisplay())
        {
          var res = DetermineColumnFormat.GetSampleValues(dt, processDisplay.CancellationToken, 0, 20, string.Empty);
          Assert.AreEqual(20, res.Count());
        }
      }
    }

    [TestMethod]
    public void DetermineColumnFormatFillGuessColumnFormatWriter()
    {
      var reader = new CsvFile();
      reader.ID = "Reader";
      reader.FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt");
      reader.HasFieldHeader = true;
      ApplicationSetting.ToolSetting.Input.Clear();
      ApplicationSetting.ToolSetting.Input.Add(reader);

      reader.FileFormat.FieldDelimiter = ",";
      var writer = new CsvFile();
      writer.SourceSetting = "Reader";
      ApplicationSetting.ToolSetting.Output.Clear();
      ApplicationSetting.ToolSetting.Output.Add(writer);

      // setting.TreatTextNullAsNull = true;
      ApplicationSetting.FillGuessSettings.IgnoreIdColums = false;
      using (var processDisplay = new DummyProcessDisplay())
      {
        writer.FillGuessColumnFormatWriter(processDisplay.CancellationToken, true);
        Assert.AreEqual(6, writer.Column.Count);
      }
    }

    [TestMethod]
    public void FillGuessColumnFormat_GermanDateAndNumbers()
    {
      var setting = new CsvFile();
      setting.FileName = Path.Combine(m_ApplicationDirectory, "DateAndNumber.csv");
      setting.HasFieldHeader = true;
      setting.FileFormat.FieldQualifier = "Quote";
      setting.CodePageId = 1252;
      setting.FileFormat.FieldDelimiter = "TAB";
      // setting.TreatTextNullAsNull = true;
      using (var processDisplay = new DummyProcessDisplay())
      {
        setting.FillGuessColumnFormatReader(false, processDisplay);
      }

      Assert.IsNotNull(setting.GetColumn(@"Betrag Brutto (2 Nachkommastellen)"), "Data Type recognized");

      Assert.AreEqual(DataType.Numeric, setting.GetColumn(@"Betrag Brutto (2 Nachkommastellen)").DataType,
        "Is Numeric");
      Assert.AreEqual(",", setting.GetColumn(@"Betrag Brutto (2 Nachkommastellen)").DecimalSeparator,
        "Decimal Separator found");

      Assert.AreEqual(DataType.DateTime, setting.GetColumn(@"Erstelldatum Rechnung").DataType);
    }

    [TestMethod]
    public void FillGuessColumnFormat_IgnoreID()
    {
      var setting = new CsvFile();
      setting.FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt");
      setting.FileFormat.FieldDelimiter = ",";
      setting.HasFieldHeader = true;
      // setting.TreatTextNullAsNull = true;
      ApplicationSetting.FillGuessSettings.IgnoreIdColums = true;
      using (var processDisplay = new DummyProcessDisplay())
      {
        setting.FillGuessColumnFormatReader(false, processDisplay);
      }

      Assert.IsTrue(setting.GetColumn("ID") == null || setting.GetColumn("ID").Convert == false);
      Assert.AreEqual(DataType.DateTime, setting.GetColumn("ExamDate").DataType);
      Assert.AreEqual(DataType.Boolean, setting.GetColumn("IsNativeLang").DataType);
    }

    [TestMethod]
    public void FillGuessColumnFormat_TrailingColumns()
    {
      var setting = new CsvFile();
      setting.FileName = Path.Combine(m_ApplicationDirectory, "Test.csv");
      setting.HasFieldHeader = true;
      setting.ByteOrderMark = true;
      setting.FileFormat.FieldDelimiter = ",";
      setting.SkipRows = 1;
      setting.FillGuessColumnFormatReader(false, null);

      Assert.AreEqual(DataType.DateTime, setting.Column[2].DataType);
      Assert.AreEqual(DataType.DateTime, setting.Column[3].DataType);
      Assert.AreEqual(DataType.DateTime, setting.Column[4].DataType);
    }

    [TestMethod]
    public void FillGuessColumnFormat_DateParts()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "Sessions.txt"),
        HasFieldHeader = true,
        ByteOrderMark = true
      };
      setting.FileFormat.FieldDelimiter = "\t";

      ApplicationSetting.FillGuessSettings.DateParts = true;
      using (var prc = new DummyProcessDisplay())
      {
        setting.FillGuessColumnFormatReader(false, prc);
      }

      ApplicationSetting.FillGuessSettings.DateParts = false;

      Assert.AreEqual("Start Date", setting.Column[0].Name, "Column 1 Start date");
      Assert.AreEqual("Start Time", setting.Column[1].Name, "Column 2 Start Time");
      Assert.AreEqual("Start Time", setting.Column[0].TimePart, "TimePart is Start Time");
      Assert.AreEqual(DataType.DateTime, setting.Column[0].DataType);
      Assert.AreEqual("MM/dd/yyyy", setting.Column[0].DateFormat);
      Assert.AreEqual("HH:mm:ss", setting.Column[1].DateFormat);
    }

    [TestMethod]
    public void FillGuessColumnFormat_TextColumns()
    {
      var setting = new CsvFile();
      setting.FileName = Path.Combine(m_ApplicationDirectory, "Test.csv");
      setting.HasFieldHeader = true;
      setting.ByteOrderMark = true;
      setting.FileFormat.FieldDelimiter = ",";
      setting.SkipRows = 1;
      setting.FillGuessColumnFormatReader(true, null);

      Assert.AreEqual(DataType.DateTime, setting.Column[7].DataType);
      Assert.AreEqual(DataType.DateTime, setting.Column[8].DataType);
      Assert.AreEqual(DataType.DateTime, setting.Column[9].DataType);
      Assert.AreEqual(DataType.String, setting.Column[10].DataType);
    }

    [TestMethod]
    public void GetSampleValues_ByColIndex()
    {
      var setting = new CsvFile();
      setting.FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt");
      setting.HasFieldHeader = true;
      using (var test = new CsvFileReader(setting))
      {
        test.Open(CancellationToken.None, false);
        var samples = DetermineColumnFormat.GetSampleValues(test, 1000, CancellationToken.None, 0, 20, "NULL");
        Assert.AreEqual(7, samples.Count());

        Assert.IsTrue(samples.Contains("1"));
        Assert.IsTrue(samples.Contains("4"));
      }
    }

    [TestMethod]
    public void GetSampleValues_FileEmpty()
    {
      var setting = new CsvFile();
      setting.FileName = Path.Combine(m_ApplicationDirectory, "CSVTestEmpty.txt");
      setting.HasFieldHeader = true;
      using (var test = new CsvFileReader(setting))
      {
        test.Open(CancellationToken.None, false);
        var samples = DetermineColumnFormat.GetSampleValues(test, 100, CancellationToken.None, 0, 20, "NULL");
        Assert.AreEqual(0, samples.Count());
      }
    }

    [TestMethod]
    public void GuessColumnFormat_Boolean1()
    {
      string[] values = { "True", "False" };

      var res = DetermineColumnFormat.GuessValueFormat(CancellationToken.None, values, 4, "True", "False", true, false,
        true, true, true, false, null, false);
      Assert.AreEqual(DataType.Boolean, res.FoundValueFormat.DataType);
    }

    [TestMethod]
    public void GuessColumnFormat_Boolean2()
    {
      string[] values = { "Yes", "No" };

      var res = DetermineColumnFormat.GuessValueFormat(CancellationToken.None, values, 2, null, "False", true, false,
        true, true, true, false, null, false);
      Assert.AreEqual(DataType.Boolean, res.FoundValueFormat.DataType);
    }

    [TestMethod]
    public void GuessColumnFormat_DateNotMatching()
    {
      string[] values = { "01/02/2010", "14/02/2012", "02/14/2012" };

      var res = DetermineColumnFormat.GuessValueFormat(CancellationToken.None, values, 4, null, "false", true, false,
        true, true, true, false, null, false);
      Assert.IsNull(res);
    }

    [TestMethod]
    public void GuessColumnFormat_ddMMyyyy()
    {
      string[] values = { "01/02/2010", "14/02/2012", "01/02/2012", "12/12/2012", "16/12/2012" };

      var res = DetermineColumnFormat.GuessValueFormat(CancellationToken.None, values, 4, null, "false", true, false,
        true, true, true, false, "dd/MM/yyyy", false);
      Assert.AreEqual(DataType.DateTime, res.FoundValueFormat.DataType);
      Assert.AreEqual(@"dd/MM/yyyy", res.FoundValueFormat.DateFormat);
      Assert.AreEqual("/", res.FoundValueFormat.DateSeparator);
    }

    [TestMethod]
    public void GuessColumnFormat_ddMMyyyy2()
    {
      string[] values = { "01.02.2010", "14.02.2012", "16.02.2012", "01.04.2014", "31.12.2010" };

      var res = DetermineColumnFormat.GuessValueFormat(CancellationToken.None, values, 4, null, "false", true, false,
        true, true, true, false, "", false);
      Assert.AreEqual(DataType.DateTime, res.FoundValueFormat.DataType);
      Assert.AreEqual(@"dd/MM/yyyy", res.FoundValueFormat.DateFormat);
      Assert.AreEqual(".", res.FoundValueFormat.DateSeparator);
    }

    [TestMethod]
    public void GuessColumnFormat_Guid()
    {
      string[] values = { "{0799A029-8B85-4589-8341-C7038AFF5B48}", "99DDD263-2E2D-434F-9265-33CF893B02DF" };

      var res = DetermineColumnFormat.GuessValueFormat(CancellationToken.None, values, 4, null, "false", false, true,
        false, false, false, false, "d/M/yy", false);
      Assert.AreEqual(DataType.Guid, res.FoundValueFormat.DataType);
    }

    [TestMethod]
    public void GuessColumnFormat_Integer()
    {
      string[] values = { "1", "2", "3", "4", "5" };

      var res = DetermineColumnFormat.GuessValueFormat(CancellationToken.None, values, 4, null, "False", true, false,
        true, true, true, false, null, false);
      Assert.AreEqual(DataType.Integer, res.FoundValueFormat.DataType);
    }

    [TestMethod]
    public void GuessColumnFormat_Integer2()
    {
      string[] values = { "-1", " 2", "3 ", "4", "100", "10" };

      var res = DetermineColumnFormat.GuessValueFormat(CancellationToken.None, values, 4, null, "False", true, false,
        true, true, true, false, null, false);
      Assert.AreEqual(DataType.Integer, res.FoundValueFormat.DataType);
    }

    [TestMethod]
    public void GuessColumnFormat_MMddyyyy()
    {
      string[] values = { "01/02/2010", "02/14/2012", "02/17/2012", "02/22/2012", "03/01/2012" };

      var res = DetermineColumnFormat.GuessValueFormat(CancellationToken.None, values, 4, null, "false", true, false,
        true, true, true, false, null, false);
      Assert.AreEqual(DataType.DateTime, res.FoundValueFormat.DataType);
      Assert.AreEqual(@"MM/dd/yyyy", res.FoundValueFormat.DateFormat);
      Assert.AreEqual("/", res.FoundValueFormat.DateSeparator);
    }

    [TestMethod]
    public void GuessColumnFormat_NoSamples()
    {
      string[] values = { };

      var res = DetermineColumnFormat.GuessValueFormat(CancellationToken.None, values, 4, null, "False", true, false,
        true, true, true, false, null, false);
      Assert.IsNull(res);
    }

    [TestMethod]
    public void GuessColumnFormat_Numeric()
    {
      string[] values = { "1", "2.5", "3", "4", "5.3" };

      var res = DetermineColumnFormat.GuessValueFormat(CancellationToken.None, values, 4, null, "False", true, false,
        true, true, true, false, null, false);
      Assert.AreEqual(DataType.Numeric, res.FoundValueFormat.DataType);
    }

    [TestMethod]
    public void GuessColumnFormat_NumersAsDate()
    {
      string[] values =
      {
        "-130.66", "-7.02", "-19.99", "-131.73",
        "43478.5037152778", "35634.7884722222", "35717.2918634259", "36858.2211226852", "43177.1338425925",
        "40568.3131481481", "37576.1801273148", "42573.3813078704", "44119.8574189815", "40060.7079976852",
        "43840.2724884259", "38013.3021759259", "40422.7830671296", "37365.5321643519", "34057.8838773148",
        "36490.4011805556", "40911.5474189815"
      };

      var res = DetermineColumnFormat.GuessValueFormat(CancellationToken.None, values, 4, null, "False", false, false,
        false, true, false, true, null, false);
      Assert.AreEqual(DataType.DateTime, res.FoundValueFormat.DataType);
    }

    [TestMethod]
    public void GuessColumnFormat_Text()
    {
      string[] values = { "Hallo", "Welt" };

      var res = DetermineColumnFormat.GuessValueFormat(CancellationToken.None, values, 4, null, "false", true, false,
        true, true, true, false, null, false);
      Assert.AreEqual(DataType.String, res.FoundValueFormat.DataType);
    }

    [TestMethod]
    public void GuessColumnFormat_VersionNumbers()
    {
      string[] values = { "1.0.1.2", "1.0.2.1", "1.0.2.2", "1.0.2.3", "1.0.2.3" };
      var res = DetermineColumnFormat.GuessValueFormat(CancellationToken.None, values, 4, null, "False", false, false,
        true, false, false, false, "d / m / yyyy|dd / mm / yyyy", false);
      Assert.IsTrue(res == null || res.FoundValueFormat.DataType != DataType.Integer);
    }

    [TestInitialize]
    public void Init()
    {
      ApplicationSetting.FillGuessSettings.SampleValues = 25;
      ApplicationSetting.FillGuessSettings.CheckedRecords = 100;
      ApplicationSetting.FillGuessSettings.DectectNumbers = true;
      ApplicationSetting.FillGuessSettings.DetectDateTime = true;
      ApplicationSetting.FillGuessSettings.DetectBoolean = true;
      ApplicationSetting.FillGuessSettings.SerialDateTime = true;
    }
  }
}