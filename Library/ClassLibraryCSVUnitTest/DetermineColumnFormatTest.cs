using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace CsvTools.Tests
{
  [TestClass]
  public class DetermineColumnFormatTest
  {
    private readonly string m_ApplicationDirectory = FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles";

    [TestMethod]
    public void GetAllPossibleFormatsD()
    {
      var res = DetermineColumnFormat.GetAllPossibleFormats("30-Oct-18 04:26:28 AM");
      var found = false;

      foreach (var item in res)
      {
        if (item.DateFormat.Equals("dd/MMM/yy HH:mm:ss tt", System.StringComparison.InvariantCulture) && item.DateSeparator == "-")
        {
          found = true;
          break;
        }
      }

      Assert.IsTrue(found);
    }

    [TestMethod]
    public void GetAllPossibleFormatsGer()
    {
      var ci = new CultureInfo("de-DE");
      var res = DetermineColumnFormat.GetAllPossibleFormats("12/13/2019 04:26", ci).ToList();

      var found = false;
      foreach (var item in res)
      {
        if (item.DateFormat.Equals("MM/dd/yyyy HH:mm", System.StringComparison.InvariantCulture) && item.DateSeparator == "/")
        {
          found = true;
          break;
        }
      }
      Assert.IsTrue(found);

      // we should have MM/dd/yyyy HH:mm, M/d/yyyy HH:mm, MM/dd/yyyy H:mm, M/d/yyyy H:mm, 
      Assert.AreEqual(4, res.Count);
    }

    [TestMethod]
    public void GetAllPossibleFormatsFR()
    {
      var ci = new CultureInfo("fr-FR");
      var res = DetermineColumnFormat.GetAllPossibleFormats("1/2/2019 3:26:10", ci).ToList();

      var found = false;
      foreach (var item in res)
      {
        if (item.DateFormat.Equals("M/d/yyyy H:mm:ss", System.StringComparison.InvariantCulture) && item.DateSeparator == "/")
        {
          found = true;
          break;
        }
      }

      Assert.IsTrue(found);
      // we should have M/d/yyyy H:mm:ss  and d/M/yyyy H:mm:ss 
      Assert.AreEqual(2, res.Count);
    }

    [TestMethod]
    public void FillGuessColumnFormatDoNotIgnoreID()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt")
      };
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
          var res = DetermineColumnFormat.GetSampleValues(dt, 0, 20, string.Empty, processDisplay.CancellationToken);
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
          row[0] = (i / 3).ToString(CultureInfo.InvariantCulture);
          dt.Rows.Add(row);
        }

        using (var processDisplay = new DummyProcessDisplay())
        {
          var res = DetermineColumnFormat.GetSampleValues(dt, 0, 20, string.Empty, processDisplay.CancellationToken);
          Assert.AreEqual(20, res.Count());
        }
      }
    }

    [TestMethod]
    public void DetermineColumnFormatFillGuessColumnFormatWriter()
    {
      var reader = new CsvFile
      {
        ID = "Reader",
        FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt"),
        HasFieldHeader = true
      };
      ApplicationSetting.ToolSetting.Input.Clear();
      ApplicationSetting.ToolSetting.Input.Add(reader);
      var mimic = new MimicSQLReader();
      mimic.AddSetting(reader);

      reader.FileFormat.FieldDelimiter = ",";
      var writer = new CsvFile
      {
        SqlStatement = reader.ID
      };
      ApplicationSetting.ToolSetting.Output.Clear();
      ApplicationSetting.ToolSetting.Output.Add(writer);

      // setting.TreatTextNullAsNull = true;
      ApplicationSetting.FillGuessSettings.IgnoreIdColums = false;
      using (var processDisplay = new DummyProcessDisplay())
      {
        writer.FillGuessColumnFormatWriter(true, processDisplay.CancellationToken);
        Assert.AreEqual(6, writer.Column.Count);
      }
    }

    [TestMethod]
    public void FillGuessColumnFormatGermanDateAndNumbers()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "DateAndNumber.csv"),
        HasFieldHeader = true
      };
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
    public void FillGuessColumnFormatIgnoreID()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt")
      };
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
    public void FillGuessColumnFormatTrailingColumns()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "Test.csv"),
        HasFieldHeader = true,
        ByteOrderMark = true
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.SkipRows = 1;
      using (var processDisplay = new DummyProcessDisplay())
      {
        setting.FillGuessColumnFormatReader(false, processDisplay);
      }
      Assert.AreEqual(DataType.DateTime, setting.Column[2].DataType, "column2");
      Assert.AreEqual(DataType.DateTime, setting.Column[3].DataType, "column3");
      Assert.AreEqual(DataType.DateTime, setting.Column[4].DataType, "column4");
    }

    [TestMethod]
    public void FillGuessColumnFormatDateParts()
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
    public void FillGuessColumnFormatTextColumns()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "Test.csv"),
        HasFieldHeader = true,
        ByteOrderMark = true
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.SkipRows = 1;
      using (var processDisplay = new DummyProcessDisplay())
      {
        setting.FillGuessColumnFormatReader(true, processDisplay);
      }
      Assert.AreEqual(11, setting.Column.Count);
      Assert.AreEqual(DataType.DateTime, setting.Column[7].DataType);
      Assert.AreEqual(DataType.DateTime, setting.Column[8].DataType);
      Assert.AreEqual(DataType.DateTime, setting.Column[9].DataType);
      Assert.AreEqual(DataType.String, setting.Column[10].DataType);
    }

    [TestMethod]
    public void GetSampleValuesByColIndex()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt"),
        HasFieldHeader = true
      };
      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        var samples = DetermineColumnFormat.GetSampleValues(test, 1000, 0, 20, "NULL", CancellationToken.None);
        Assert.AreEqual(7, samples.Count());

        Assert.IsTrue(samples.Contains("1"));
        Assert.IsTrue(samples.Contains("4"));
      }
    }

    [TestMethod]
    public void GetSampleValuesFileEmpty()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "CSVTestEmpty.txt"),
        HasFieldHeader = true
      };
      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        var samples = DetermineColumnFormat.GetSampleValues(test, 100, 0, 20, "NULL", CancellationToken.None);
        Assert.AreEqual(0, samples.Count());
      }
    }

    [TestMethod]
    public void GuessColumnFormatBoolean1()
    {
      string[] values = { "True", "False" };

      var res = DetermineColumnFormat.GuessValueFormat(values, 4, "True", "False", true, false, true,
        true, true, false, false, null, CancellationToken.None);
      Assert.AreEqual(DataType.Boolean, res.FoundValueFormat.DataType);
    }

    [TestMethod]
    public void GuessColumnFormatBoolean2()
    {
      string[] values = { "Yes", "No" };

      var res = DetermineColumnFormat.GuessValueFormat(values, 2, null, "False", true, false, true,
        true, true, false, false, null, CancellationToken.None);
      Assert.AreEqual(DataType.Boolean, res.FoundValueFormat.DataType);
    }

    [TestMethod]
    public void GuessColumnFormatDateNotMatching()
    {
      string[] values = { "01/02/2010", "14/02/2012", "02/14/2012" };

      var res = DetermineColumnFormat.GuessValueFormat(values, 4, null, "false", true, false, true,
        true, true, false, false, null, CancellationToken.None);
      Assert.IsNull(res);
    }

    [TestMethod]
    public void GuessColumnFormatddMMyyyy()
    {
      string[] values = { "01/02/2010", "14/02/2012", "01/02/2012", "12/12/2012", "16/12/2012" };

      var res = DetermineColumnFormat.GuessValueFormat(values, 4, null, "false", true, false, true,
        true, true, false, false, null, CancellationToken.None);
      Assert.AreEqual(DataType.DateTime, res.FoundValueFormat.DataType);
      Assert.AreEqual(@"dd/MM/yyyy", res.FoundValueFormat.DateFormat);
      Assert.AreEqual("/", res.FoundValueFormat.DateSeparator);
    }

    [TestMethod]
    public void GuessColumnFormatddMMyyyy2()
    {
      string[] values = { "01.02.2010", "14.02.2012", "16.02.2012", "01.04.2014", "31.12.2010" };

      var res = DetermineColumnFormat.GuessValueFormat(values, 4, null, "false", true, false, true,
        true, true, false, false, null, CancellationToken.None);
      Assert.AreEqual(DataType.DateTime, res.FoundValueFormat.DataType);
      Assert.AreEqual(@"dd/MM/yyyy", res.FoundValueFormat.DateFormat);
      Assert.AreEqual(".", res.FoundValueFormat.DateSeparator);
    }

    [TestMethod]
    public void GuessColumnFormatGuid()
    {
      string[] values = { "{0799A029-8B85-4589-8341-C7038AFF5B48}", "99DDD263-2E2D-434F-9265-33CF893B02DF" };

      var res = DetermineColumnFormat.GuessValueFormat(values, 4, null, "false", false, true, false,
        false, false, false, false, null, CancellationToken.None);
      Assert.AreEqual(DataType.Guid, res.FoundValueFormat.DataType);
    }

    [TestMethod]
    public void GuessColumnFormatInteger()
    {
      string[] values = { "1", "2", "3", "4", "5" };

      var res = DetermineColumnFormat.GuessValueFormat(values, 4, null, "False", true, false, true,
        true, true, false, false, null, CancellationToken.None);
      Assert.AreEqual(DataType.Integer, res.FoundValueFormat.DataType);
    }

    [TestMethod]
    public void GuessColumnFormatInteger2()
    {
      string[] values = { "-1", " 2", "3 ", "4", "100", "10" };

      var res = DetermineColumnFormat.GuessValueFormat(values, 4, null, "False", true, false, true,
        true, true, false, false, null, CancellationToken.None);
      Assert.AreEqual(DataType.Integer, res.FoundValueFormat.DataType);
    }

    [TestMethod]
    public void GuessColumnFormatMMddyyyy()
    {
      string[] values = { "01/02/2010", "02/14/2012", "02/17/2012", "02/22/2012", "03/01/2012" };

      var res = DetermineColumnFormat.GuessValueFormat(values, 4, null, "false", true, false, true,
        true, true, false, false, null, CancellationToken.None);
      Assert.AreEqual(DataType.DateTime, res.FoundValueFormat.DataType);
      Assert.AreEqual(@"MM/dd/yyyy", res.FoundValueFormat.DateFormat);
      Assert.AreEqual("/", res.FoundValueFormat.DateSeparator);
    }

    [TestMethod]
    public void GuessColumnFormatMMddyyyySuggestion()
    {
      string[] values = { "01/02/2010", "02/12/2012" };

      var res = DetermineColumnFormat.GuessValueFormat(values, 4, null, "false", true, false, true,
        true, true, false, false, new ValueFormat(DataType.DateTime) { DateFormat = "MM/dd/yyyy", DateSeparator = "/" }, CancellationToken.None);
      Assert.AreEqual(DataType.DateTime, res.FoundValueFormat.DataType);
      Assert.AreEqual(@"MM/dd/yyyy", res.FoundValueFormat.DateFormat);
      Assert.AreEqual("/", res.FoundValueFormat.DateSeparator);
    }

    [TestMethod]
    public void GuessColumnFormatMMddyyyyNotenough()
    {
      string[] values = { "01/02/2010", "02/12/2012" };
      var res = DetermineColumnFormat.GuessValueFormat(values, 4, null, "false", true, false, true,
        true, true, false, false, null, CancellationToken.None);
      Assert.IsFalse(res?.PossibleMatch ?? false);
    }

    [TestMethod]
    public void GuessColumnFormatNoSamples()
    {
      string[] values = { };

      var res = DetermineColumnFormat.GuessValueFormat(values, 4, null, "False", true, false, true,
        true, true, false, false, null, CancellationToken.None);
      Assert.IsNull(res);
    }

    [TestMethod]
    public void GuessColumnFormatNumeric()
    {
      string[] values = { "1", "2.5", "3", "4", "5.3" };

      var res = DetermineColumnFormat.GuessValueFormat(values, 4, null, "False", true, false, true,
        true, true, false, false, null, CancellationToken.None);
      Assert.AreEqual(DataType.Numeric, res.FoundValueFormat.DataType);
    }

    [TestMethod]
    public void GuessColumnFormatNumersAsDate()
    {
      string[] values =
      {
        "-130.66", "-7.02", "-19.99", "-131.73",
        "43478.5037152778", "35634.7884722222", "35717.2918634259", "36858.2211226852", "43177.1338425925",
        "40568.3131481481", "37576.1801273148", "42573.3813078704", "44119.8574189815", "40060.7079976852",
        "43840.2724884259", "38013.3021759259", "40422.7830671296", "37365.5321643519", "34057.8838773148",
        "36490.4011805556", "40911.5474189815"
      };

      var res = DetermineColumnFormat.GuessValueFormat(values, 4, null, "False", false, false, false,
        true, false, true, false, null, CancellationToken.None);
      Assert.AreEqual(DataType.DateTime, res.FoundValueFormat.DataType);
    }

    [TestMethod]
    public void GuessColumnFormatText()
    {
      string[] values = { "Hallo", "Welt" };

      var res = DetermineColumnFormat.GuessValueFormat(values, 4, null, "false", true, false, true,
        true, true, false, false, null, CancellationToken.None);
      Assert.AreEqual(DataType.String, res.FoundValueFormat.DataType);
    }

    [TestMethod]
    public void GuessColumnFormatVersionNumbers()
    {

      string[] values = { "1.0.1.2", "1.0.2.1", "1.0.2.2", "1.0.2.3", "1.0.2.3" };

      var res = DetermineColumnFormat.GuessValueFormat(values, 4, null, "False", false, false, true,
        false, false, false, false, null, CancellationToken.None);
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