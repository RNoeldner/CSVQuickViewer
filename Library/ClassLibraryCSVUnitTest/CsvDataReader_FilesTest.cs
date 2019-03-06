using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace CsvTools.Tests
{
  [TestClass]
  public class CsvDataReaderUnitTestReadFiles
  {
    private readonly string m_ApplicationDirectory = FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles";

    [TestMethod]
    public void ProcessDisplayTest()
    {
      var frm = new DummyProcessDisplay();
      var setting = new CsvFile
      {
        HasFieldHeader = false,
        AlternateQuoting = true
      };
      setting.FileName = "TestFiles\\BasicCSV.txt";

      using (var test = new CsvFileReader(setting))
      {
        test.ProcessDisplay = frm;
        Assert.AreEqual(frm, test.ProcessDisplay);
        test.ProcessDisplay = null;
        Assert.AreEqual(null, test.ProcessDisplay);
      }
    }

    [TestMethod]
    public void ReadDateWithTime()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "Sessions.txt"),
        HasFieldHeader = true,
        ByteOrderMark = true
      };
      setting.FileFormat.FieldDelimiter = "\t";
      setting.ColumnAdd(new Column()
      {
        Name = "Start Date",
        DataType = DataType.DateTime,
        DateFormat = "MM/dd/yyyy",
        TimePart = "Start Time",
        TimePartFormat = "HH:mm:ss"
      });

      using (var test = new CsvFileReader(setting))
      {
        test.Open(true, CancellationToken.None);
        test.Read();
        CultureInfo cultureInfo = new CultureInfo("en-US");
        Assert.AreEqual("01/08/2013 07:00:00", test.GetDateTime(0).ToString("MM/dd/yyyy HH:mm:ss", cultureInfo));
        test.Read(); // 01/19/2010	24:00:00 --> 01/20/2010	00:00:00
        Assert.AreEqual("01/20/2010 00:00:00", test.GetDateTime(0).ToString("MM/dd/yyyy HH:mm:ss", cultureInfo));
        test.Read(); // 01/21/2013	25:00:00 --> 01/22/2013	01:00:00
        Assert.AreEqual("01/22/2013 01:00:00", test.GetDateTime(0).ToString("MM/dd/yyyy HH:mm:ss", cultureInfo));
      }
    }

    [TestMethod]
    public void ReadDateWithTimeAndTimeZone()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "Sessions.txt"),
        HasFieldHeader = true,
        ByteOrderMark = true
      };
      setting.FileFormat.FieldDelimiter = "\t";
      setting.ColumnAdd(new Column()
      {
        Name = "Start Date",
        DataType = DataType.DateTime,
        DateFormat = "MM/dd/yyyy",
        TimePart = "Start Time",
        TimePartFormat = "HH:mm:ss",
        TimeZonePart = "Time Zone"
      });

      // all will be converted to TimeZoneInfo.Local, but we concert then to UTC
      using (var test = new CsvFileReader(setting))
      {
        test.Open(true, CancellationToken.None);
        test.Read();
        CultureInfo cultureInfo = new CultureInfo("en-US");
        // 01/08/2013 07:00:00 IST --> 01/08/2013 01:30:00	UTC
        Assert.AreEqual("01/08/2013 01:30:00", test.GetDateTime(0).ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss", cultureInfo));
        test.Read(); // 01/19/2010	24:00:00 MST -->  01/20/2010	00:00:00 MST --> 01/20/2010 07:00:00 UTC
        Assert.AreEqual("01/20/2010 07:00:00", test.GetDateTime(0).ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss", cultureInfo));
        test.Read(); // 01/21/2013	25:00:00 GMT --> 01/22/2013 01:00:00	GMT  -->01/22/2013	01:00:00 UTC
        Assert.AreEqual("01/22/2013 01:00:00", test.GetDateTime(0).ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss", cultureInfo));
      }
    }

    [TestMethod]
    public void CsvDataReaderCancellationOnOpen()
    {
      using (var cts = new CancellationTokenSource())
      {
        var setting = new CsvFile
        {
          HasFieldHeader = false,
          AlternateQuoting = true
        };
        setting.TrimmingOption = TrimmingOption.All;
        setting.FileName = Path.Combine(m_ApplicationDirectory, "AlternateTextQualifiers.txt");

        using (var test = new CsvFileReader(setting))
        {
          cts.Cancel();
          Assert.AreEqual(0, test.Open(true, cts.Token));
        }
      }
    }

    [TestMethod]
    public void AlternateTextQualifiersTrimQuoted()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false,
        AlternateQuoting = true
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.TrimmingOption = TrimmingOption.All;
      setting.FileName = Path.Combine(m_ApplicationDirectory, "AlternateTextQualifiers.txt");
      using (var test = new CsvFileReader(setting))
      {
        test.Open(true, CancellationToken.None);
        Assert.IsTrue(test.Read());
        Assert.AreEqual("a", test.GetString(0), "Start of file with quote");
        Assert.AreEqual("a", test.GetValue(0), "Start of file with quote");
        Assert.AreEqual("b \"", test.GetString(1), "Containing Quote");
        Assert.AreEqual("d", test.GetString(3), "Leading space not trimmed");
        Assert.AreEqual("This is a\r\n\" in Quotes", test.GetString(4), "Linefeed in quotes");
        Assert.IsTrue(test.Read());
        Assert.AreEqual("13", test.GetString(0), "Start of line with quote");
        Assert.AreEqual("This is a \"Test\"", test.GetString(3), "Containing Quote and quote at the end");
        Assert.AreEqual("18", test.GetString(5), "Line ending with quote");

        Assert.IsTrue(test.Read());
        Assert.AreEqual("19", test.GetString(0), "Training space not trimmed");
        Assert.AreEqual("20,21", test.GetString(1), "Delimiter in quotes");
        Assert.AreEqual("Another\r\nLine \"Test\"", test.GetString(2), "Linefeed as last char in quotes");
        Assert.AreEqual("22", test.GetString(3), "Leading whitespace before start");
        Assert.AreEqual("24", test.GetString(5), "Leading space not trimmed & EOF");
      }
    }

    [TestMethod]
    public void AlternateTextQualifiersTrimUnquoted()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false,
        AlternateQuoting = true,
        WarnQuotes = true
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.TrimmingOption = TrimmingOption.Unquoted;
      setting.FileName = Path.Combine(m_ApplicationDirectory, "AlternateTextQualifiers.txt");
      var warningList = new RowErrorCollection();

      using (var test = new CsvFileReader(setting))
      {
        test.Warning += warningList.Add;
        test.Open(false, CancellationToken.None);
        Assert.IsTrue(test.Read());
        Assert.AreEqual("a", test.GetString(0), "Start of file with quote");
        Assert.AreEqual("b \"  ", test.GetString(1), "Containing Quote");
        Assert.IsTrue(warningList.Display.Contains("Field qualifier"));
        Assert.AreEqual("   d", test.GetString(3), "Leading space not trimmed");
        Assert.AreEqual("This is a\r\n\" in Quotes", test.GetString(4), "Linefeed in quotes");
        Assert.IsTrue(test.Read());
        Assert.AreEqual("13", test.GetString(0), "Start of line with quote");
        Assert.AreEqual("This is a \"Test\"", test.GetString(3), "Containing Quote and quote at the end");
        Assert.AreEqual("18", test.GetString(5), "Line ending with quote");

        Assert.IsTrue(test.Read());
        Assert.AreEqual("19  ", test.GetString(0), "Training space not trimmed");
        Assert.AreEqual("20,21", test.GetString(1), "Delimiter in quotes");
        Assert.AreEqual("Another\r\nLine \"Test\"", test.GetString(2), "Linefeed as last char in quotes");
        Assert.AreEqual("22", test.GetString(3), "Leading whitespace before start");
        Assert.AreEqual("  24", test.GetString(5), "Leading space not trimmed & EOF");
      }
    }

    [TestMethod]
    public void BasicEscapedCharacters()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileFormat.CommentLine = "#";
      setting.FileFormat.EscapeCharacter = "\\";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "BasicEscapedCharacters.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.IsTrue(test.Read());
        Assert.AreEqual("a\"", test.GetString(0), @"a\""");
        Assert.AreEqual("b", test.GetString(1), "b");
        Assert.AreEqual("c", test.GetString(2), "c");
        Assert.AreEqual("d", test.GetString(3), "d");
        Assert.AreEqual("e", test.GetString(4), "e");
        Assert.AreEqual("f", test.GetString(5), "f");
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.AreEqual(",9", test.GetString(2), @"\,9");
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.AreEqual("\\vy", test.GetString(5), @"\\\vy");
        Assert.IsTrue(test.Read());
        Assert.AreEqual("24\"", test.GetString(5), @"24\""");
        Assert.AreEqual(6, test.FieldCount);
      }
    }

    [TestMethod]
    public void TestWarnLinefeed()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false
      };
      setting.WarnLineFeed = true;
      setting.FileFormat.CommentLine = "#";
      setting.FileFormat.EscapeCharacter = "\\";
      setting.FileFormat.FieldQualifier = "\"";
      setting.FileFormat.FieldDelimiter = ",";
      setting.TrimmingOption = TrimmingOption.Unquoted;
      setting.FileName = Path.Combine(m_ApplicationDirectory, "ComplexDataDelimiter.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        var message = string.Empty;
        test.Warning += delegate (object sender, WarningEventArgs args) { message = args.Message; };
        test.Read();
        test.Read();
        Assert.IsTrue(message.Contains("Linefeed"));
      }
    }

    [TestMethod]
    public void TestHighOccuranceQuestionMark()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false
      };
      setting.WarnLineFeed = true;
      setting.FileFormat.CommentLine = "#";
      setting.FileFormat.EscapeCharacter = "\\";
      setting.FileFormat.FieldQualifier = "\"";
      setting.FileFormat.FieldDelimiter = ",";
      setting.TrimmingOption = TrimmingOption.Unquoted;
      setting.FileName = Path.Combine(m_ApplicationDirectory, "ComplexDataDelimiter.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        test.Read();
        test.Read();
        var message = string.Empty;
        test.Warning += delegate (object sender, WarningEventArgs args) { message = args.Message; };
        test.Read();
        Assert.IsTrue(message.Contains("occurrence") && message.Contains("?"));
      }
    }

    [TestMethod]
    public void ComplexDataDelimiter()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileFormat.CommentLine = "#";
      setting.FileFormat.EscapeCharacter = "\\";
      setting.FileFormat.FieldQualifier = "\"";
      setting.FileFormat.FieldDelimiter = ",";
      setting.TrimmingOption = TrimmingOption.Unquoted;
      setting.FileName = Path.Combine(m_ApplicationDirectory, "ComplexDataDelimiter.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(6, test.FieldCount);
        Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");

        Assert.IsTrue(test.Read());
        Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
        Assert.AreEqual("5\"", test.GetString(4));

        Assert.IsTrue(test.Read()); // 2
        Assert.AreEqual("a", test.GetString(0));
        Assert.AreEqual("bta", test.GetString(1));
        Assert.AreEqual("c", test.GetString(2));
        Assert.AreEqual("\"d", test.GetString(3), "\"d");
        Assert.AreEqual("e", test.GetString(4));
        Assert.AreEqual("f\r\n  , ", test.GetString(5));

        Assert.AreEqual(4, test.StartLineNumber, "StartLineNumber");
        Assert.AreEqual(6, test.EndLineNumber, "EndLineNumber");

        Assert.IsTrue(test.Read()); // 3
        Assert.AreEqual(6U, test.StartLineNumber, "StartLineNumber");
        Assert.AreEqual("6\"", test.GetString(5));

        Assert.IsTrue(test.Read()); // 4
        Assert.AreEqual("k\r\n", test.GetString(4));
        Assert.AreEqual(9, test.StartLineNumber, "StartLineNumber");

        Assert.IsTrue(test.Read()); // 5
        Assert.IsTrue(test.Read()); // 6
        Assert.IsTrue(test.Read()); // 7
        Assert.IsTrue(test.Read()); // 8

        Assert.IsTrue(test.Read()); // 9
        Assert.AreEqual("19", test.GetString(0));
        Assert.AreEqual("20", test.GetString(1));
        Assert.AreEqual("21", test.GetString(2));
        Assert.AreEqual("2\"2", test.GetString(3));
        Assert.AreEqual("23", test.GetString(4));

        Assert.IsTrue(test.Read()); // 10
        Assert.AreEqual("f\r\n", test.GetString(5));
        Assert.AreEqual(23, test.StartLineNumber, "StartLineNumber");
      }
    }

    [TestMethod]
    public void ComplexDataDelimiter2()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false
      };
      setting.FileFormat.CommentLine = "#";
      setting.FileFormat.FieldQualifier = "\"";
      setting.FileFormat.FieldDelimiter = ",";
      setting.TrimmingOption = TrimmingOption.Unquoted;
      setting.FileName = Path.Combine(m_ApplicationDirectory, "QuoteInText.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(6, test.FieldCount);

        Assert.IsTrue(test.Read()); // 1
        Assert.AreEqual("4", test.GetString(3));
        Assert.AreEqual(" 5\"", test.GetString(4));
        Assert.AreEqual("6", test.GetString(5));
        Assert.IsTrue(test.Read()); // 2
        Assert.AreEqual("21", test.GetString(2));
        Assert.AreEqual("2\"2", test.GetString(3));
        Assert.AreEqual("23", test.GetString(4));
        Assert.IsTrue(test.Read()); // 3
        Assert.AreEqual("e", test.GetString(4));
        Assert.AreEqual("f\r\n", test.GetString(5));
      }
    }

    [TestMethod]
    public void ComplexDataDelimiterTrimQuotes()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false
      };
      setting.FileFormat.CommentLine = "#";
      setting.FileFormat.EscapeCharacter = "\\";
      setting.FileFormat.FieldQualifier = "\"";
      setting.FileFormat.FieldDelimiter = ",";
      setting.TrimmingOption = TrimmingOption.All;
      setting.FileName = Path.Combine(m_ApplicationDirectory, "ComplexDataDelimiter.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(6, test.FieldCount);

        Assert.IsTrue(test.Read()); // 1
        Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
        Assert.AreEqual("5\"", test.GetString(4));

        Assert.IsTrue(test.Read()); // 2
        Assert.AreEqual("a", test.GetString(0));
        Assert.AreEqual("bta", test.GetString(1));
        Assert.AreEqual("c", test.GetString(2));
        Assert.AreEqual("\"d", test.GetString(3), "\"d");
        Assert.AreEqual("e", test.GetString(4));
        Assert.AreEqual("f\r\n  ,", test.GetString(5));
        // Line streches over two line both are fine
        Assert.AreEqual(4, test.StartLineNumber, "LineNumber");

        Assert.IsTrue(test.Read()); // 3
        Assert.AreEqual(6U, test.StartLineNumber, "LineNumber");
        Assert.AreEqual("6\"", test.GetString(5));

        Assert.IsTrue(test.Read()); // 4
        Assert.AreEqual("k", test.GetString(4));
        Assert.AreEqual(9, test.StartLineNumber, "LineNumber");

        Assert.IsTrue(test.Read()); // 5
        Assert.IsTrue(test.Read()); // 6
        Assert.IsTrue(test.Read()); // 7
        Assert.IsTrue(test.Read()); // 8
        Assert.IsTrue(test.Read()); // 9
        Assert.IsTrue(test.Read()); // 10
        Assert.AreEqual("f", test.GetString(5));
        Assert.AreEqual(23, test.StartLineNumber, "LineNumber");
      }
    }

    [TestMethod]
    public void CSVTestEmpty()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false,
        ByteOrderMark = true
      };

      setting.FileName = Path.Combine(m_ApplicationDirectory, "CSVTestEmpty.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(0, test.FieldCount);
        Assert.IsFalse(test.Read());
      }
    }

    [TestMethod]
    public void DifferentColumnDelimiter()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false
      };
      setting.FileFormat.FieldDelimiter = "PIPE";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "DifferentColumnDelimiter.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(6, test.FieldCount);
        Assert.IsTrue(test.Read());
        Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
        Assert.AreEqual("a", test.GetString(0));
        Assert.AreEqual("f", test.GetString(5));
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.AreEqual("19", test.GetString(0));
        Assert.AreEqual("24", test.GetString(5));
        Assert.AreEqual(8U, test.StartLineNumber, "LineNumber");
        Assert.IsFalse(test.Read());
      }
    }

    [TestMethod]
    public void EscapedCharacterAtEndOfFile()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileFormat.EscapeCharacter = "\\";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "EscapedCharacterAtEndOfFile.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(6, test.FieldCount);
        Assert.IsTrue(test.Read());
        Assert.AreEqual("a\"", test.GetString(0), @"a\""");
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.AreEqual(",9", test.GetString(2), @"\,9");
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.AreEqual("\\vy", test.GetString(5), @"\\\vy");
        Assert.IsTrue(test.Read());
        Assert.AreEqual("24", test.GetString(5), @"24\");
      }
    }

    [TestMethod]
    public void EscapedCharacterAtEndOfRowDelimiter()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false
      };
      setting.TrimmingOption = TrimmingOption.None;
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileFormat.EscapeCharacter = "\\";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "EscapedCharacterAtEndOfRowDelimiter.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(6, test.FieldCount);
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.AreEqual("l\r", test.GetString(5));  // Important to not trim the value otherwise the \r is gone
        Assert.IsTrue(test.Read());
        Assert.AreEqual("7", test.GetString(0), @"Next Row");
        Assert.AreEqual(4U, test.StartLineNumber);
        Assert.AreEqual(4U, test.RecordNumber);
      }
    }
    [TestMethod]
    public void EscapedCharacterAtEndOfRowDelimiterNoEscape()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileFormat.EscapeCharacter = "";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "EscapedCharacterAtEndOfRowDelimiter.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(6, test.FieldCount);
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.AreEqual(@"l\", test.GetString(5), @"l\");
        Assert.IsTrue(test.Read());
        Assert.AreEqual("7", test.GetString(0), @"Next Row");
        Assert.AreEqual(4U, test.StartLineNumber);
        Assert.AreEqual(4U, test.RecordNumber);
      }
    }

    [TestMethod]
    public void EscapeWithoutTextQualifier()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileFormat.EscapeCharacter = "\\";
      setting.FileFormat.FieldQualifier = string.Empty;
      setting.FileName = Path.Combine(m_ApplicationDirectory, "EscapeWithoutTextQualifier.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(6, test.FieldCount);
        Assert.IsTrue(test.Read());
        Assert.AreEqual(@"a\", test.GetString(0), @"a\\");
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.AreEqual(",9", test.GetString(2), @"\,9");
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.AreEqual("\\vy", test.GetString(5), @"\\\vy");
        Assert.IsTrue(test.Read());
        Assert.AreEqual("24\\", test.GetString(5), @"24\\");
      }
    }

    [TestMethod]
    public void HandlingDuplicateColumnNames()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = true
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "HandlingDuplicateColumnNames.txt");

      using (var test = new CsvFileReader(setting))
      {
        var message = string.Empty;
        test.Warning += delegate (object sender, WarningEventArgs args) { message = args.Message; };
        test.Open(false, CancellationToken.None);
        Assert.IsTrue(message.Contains("exists more than once"));
      }
    }

    [TestMethod]
    public void LastRowWithRowDelimiter()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = true
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "LastRowWithRowDelimiter.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(6, test.FieldCount);

        Assert.AreEqual("a", test.GetName(0));
        Assert.AreEqual("f", test.GetName(5));

        Assert.IsTrue(test.Read());
        Assert.AreEqual(2U, test.StartLineNumber, "LineNumber");
        Assert.AreEqual("1", test.GetString(0));
        Assert.AreEqual("6", test.GetString(5));
      }
    }

    [TestMethod]
    public void LastRowWithRowDelimiterNoHeader()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "LastRowWithRowDelimiter.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(6, test.FieldCount);

        Assert.IsTrue(test.Read());
        Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
        Assert.AreEqual("a", test.GetString(0));
        Assert.AreEqual("f", test.GetString(5));
      }
    }

    [TestMethod]
    public void LongHeaders()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = true
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileFormat.CommentLine = "#";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "LongHeaders.txt");
      var warningsList = new RowErrorCollection();
      using (var test = new CsvFileReader(setting))
      {
        test.Warning += warningsList.Add;
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(6, test.FieldCount);
        Assert.AreEqual("a", test.GetName(0));
        Assert.AreEqual("b", test.GetName(1));
        Assert.AreEqual(
          "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquya"
            .Substring(0, 128), test.GetName(2));
        Assert.AreEqual("d", test.GetName(3));
        Assert.AreEqual("e", test.GetName(4));
        Assert.AreEqual("f", test.GetName(5));
        Assert.AreEqual(1, warningsList.CountRows);
        Assert.IsTrue(warningsList.Display.Contains("has been cut off"));
      }
    }

    [TestMethod]
    public void MoreColumnsThanHeaders()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false,
        WarnEmptyTailingColumns = true
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "MoreColumnsThanHeaders.txt");
      var warningList = new RowErrorCollection();
      using (var test = new CsvFileReader(setting))
      {
        test.Warning += warningList.Add;
        test.Open(false, CancellationToken.None);

        Assert.AreEqual(6, test.FieldCount);
        Assert.IsTrue(test.Read());
        Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
        Assert.AreEqual("a", test.GetString(0));
        Assert.AreEqual("f", test.GetString(5));
        Assert.IsTrue(test.Read());
        Assert.AreEqual(0, warningList.CountRows);

        Assert.IsTrue(test.Read());
        // Still only 6 fields
        Assert.AreEqual(6, test.FieldCount);

        Assert.AreEqual(1, warningList.CountRows, "warningList.CountRows");
        Assert.IsTrue(warningList.Display.Contains(CsvFileReader.cMoreColumns));
        //        Assert.IsTrue(warningList.Display.Contains("The existing data in these extra columns is not read"));

        Assert.IsTrue(test.Read());

        // no further warning
        Assert.AreEqual(1, warningList.CountRows, "warningList.CountRows");
      }
    }

    [TestMethod]
    public void NoFieldQualifier()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileFormat.FieldQualifier = string.Empty;
      setting.FileName = Path.Combine(m_ApplicationDirectory, "TextQualifierDataPastClosingQuote.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(6, test.FieldCount);
        Assert.IsTrue(test.Read());
        //"a"a,b,c,d,e,f
        Assert.AreEqual("\"a\"a", test.GetString(0));
        Assert.AreEqual("b", test.GetString(1));
        Assert.AreEqual("c", test.GetString(2));
        Assert.AreEqual("d", test.GetString(3));
        Assert.AreEqual("e", test.GetString(4));
        Assert.AreEqual("f", test.GetString(5));
        //1,2,"3" ignore me,4,5,6
        Assert.IsTrue(test.Read());
        Assert.AreEqual("1", test.GetString(0));
        Assert.AreEqual("2", test.GetString(1));
        Assert.AreEqual("\"3\" ignore me", test.GetString(2));
        Assert.AreEqual("4", test.GetString(3));
        Assert.AreEqual("5", test.GetString(4));
        Assert.AreEqual("6", test.GetString(5));
        Assert.IsFalse(test.Read());
      }
    }

    [TestMethod]
    public void PlaceHolderTest()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = true
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileFormat.EscapeCharacter = string.Empty;
      setting.FileFormat.DelimiterPlaceholder = @"<\d>";
      setting.FileFormat.QuotePlaceholder = @"<\q>";
      setting.FileFormat.NewLinePlaceholder = @"<\r>";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "Placeholder.txt");
      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.IsTrue(test.Read());
        Assert.AreEqual("A \r\nLine\r\nBreak", test.GetString(1));

        Assert.IsTrue(test.Read());
        Assert.AreEqual("Two ,Delimiter,", test.GetString(1));

        Assert.IsTrue(test.Read());
        Assert.AreEqual("Two \"Quote\"", test.GetString(1));
      }
    }

    [TestMethod]
    public void ProcessDisplayUpdateShowProgress()
    {
      var setting = Helper.ReaderGetAllFormats(null);
      var pd = new MockProcessDisplay();
      var stopped = false;
      pd.ProgressStopEvent += delegate { stopped = true; };
      Assert.AreEqual(null, pd.Text);
      using (var test = new CsvFileReader(setting))
      {
        test.ProcessDisplay = pd;
        test.Open(false, CancellationToken.None);

        for (var i = 0; i < 500; i++) Assert.IsTrue(test.Read());
      }

      Assert.AreNotEqual(null, pd.Text);
      Assert.IsFalse(stopped);
    }

    [TestMethod]
    public void ReadingInHeaderAfterComments()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = true
      };
      setting.FileFormat.CommentLine = "#";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "ReadingInHeaderAfterComments.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);

        Assert.AreEqual(6, test.FieldCount);
        Assert.IsTrue(test.Read());
        Assert.AreEqual(4U, test.StartLineNumber, "LineNumber");
        Assert.AreEqual("1", test.GetString(0));
        Assert.AreEqual("6", test.GetString(5));
      }
    }

    [TestMethod]
    public void RowWithoutColumnDelimiter()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = true
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "RowWithoutColumnDelimiter.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(1, test.FieldCount);
        Assert.AreEqual("abcdef", test.GetName(0));
        Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");

        Assert.IsTrue(test.Read());
        Assert.AreEqual(2U, test.StartLineNumber, "LineNumber");
        Assert.AreEqual("123456", test.GetString(0));
      }
    }

    [TestMethod]
    public void SimpleDelimiterWithControlCharacters()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = true
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "SimpleDelimiterWithControlCharacters.txt");
      setting.FileFormat.CommentLine = "#";
      setting.WarnNBSP = true;
      setting.WarnUnknowCharater = true;
      var warningList = new RowErrorCollection();
      using (var test = new CsvFileReader(setting))
      {
        test.Warning += warningList.Add;
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(6, test.FieldCount);
        Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");

        Assert.IsTrue(test.Read());
        Assert.AreEqual("a", test.GetString(0));
        Assert.AreEqual("b", test.GetString(1));
        Assert.AreEqual("c", test.GetString(2));
        Assert.AreEqual("d", test.GetString(3));
        Assert.AreEqual(2U, test.StartLineNumber, "LineNumber");
        // NBSP and � past " so not in the field
        Assert.IsFalse(warningList.Display.Contains("Non Breaking Space"));
        Assert.IsFalse(warningList.Display.Contains("Unknown Character"));
        Assert.IsTrue(test.Read());
        Assert.AreEqual(3U, test.StartLineNumber, "LineNumber");
        // g,h,i , j,k,l
        Assert.AreEqual("i", test.GetString(2));
        Assert.AreEqual("j", test.GetString(3));

        Assert.IsTrue(test.Read());
        Assert.AreEqual("7", test.GetString(0));
        Assert.AreEqual("10", test.GetString(3));
        Assert.AreEqual(1, warningList.CountRows);
        Assert.IsTrue(warningList.Display.Contains("Non Breaking Space"));

        Assert.IsTrue(test.Read());
        // m,n,o,p ,q,r
        Assert.AreEqual("p", test.GetString(3));

        Assert.IsTrue(test.Read());
        Assert.AreEqual("15�", test.GetString(2));
        Assert.IsTrue(warningList.Display.Contains("Unknown Character"));
      }
    }

    [TestMethod]
    public void SimpleDelimiterWithControlCharactersUnknowCharaterReplacement()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = true,
        TreatUnknowCharaterAsSpace = true,
        TreatNBSPAsSpace = true,
        WarnNBSP = true,
        WarnUnknowCharater = true,
        TrimmingOption = TrimmingOption.None
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "SimpleDelimiterWithControlCharacters.txt");
      setting.FileFormat.CommentLine = "#";
      var warningList = new RowErrorCollection();
      using (var test = new CsvFileReader(setting))
      {
        test.Warning += warningList.Add;
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(6, test.FieldCount);
        Assert.AreEqual("1", test.GetName(0));
        Assert.AreEqual("2", test.GetName(1));
        Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");

        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.AreEqual(3U, test.StartLineNumber, "LineNumber");
        // g,h,i , j,k,l
        Assert.AreEqual(" j", test.GetString(3));

        //    #A NBSP: Create with Alt+01602
        // 7,8,9,10 ,11,12
        Assert.IsTrue(test.Read());
        Assert.AreEqual("7", test.GetString(0));
        Assert.AreEqual("10 ", test.GetString(3));
        Assert.AreEqual(2, warningList.CountRows);
        Assert.IsTrue(warningList.Display.Contains("Non Breaking Space"));

        Assert.IsTrue(test.Read());
        // m,n,o,p						,q,r
        Assert.AreEqual("p						", test.GetString(3));

        // 13,14,15�,16,17,18
        Assert.IsTrue(test.Read());
        Assert.AreEqual("15 ", test.GetString(2));
        Assert.AreEqual(3, warningList.CountRows);
        Assert.IsTrue(warningList.Display.Contains("Unknown Character"));
      }
    }

    [TestMethod]
    public void SkipAllRows()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.SkipRows = 100;
      setting.FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(0, test.FieldCount);
      }
    }

    [TestMethod]
    public void SkippingComments()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "SkippingComments.txt");
      setting.FileFormat.CommentLine = "#";
      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(6, test.FieldCount);
        Assert.IsTrue(test.Read());
        Assert.AreEqual(2U, test.StartLineNumber, "LineNumber");
        Assert.AreEqual(1U, test.RecordNumber, "RecordNumber");
        Assert.AreEqual("a", test.GetString(0));
        Assert.AreEqual("f", test.GetString(5));

        Assert.IsTrue(test.Read());
        Assert.AreEqual(2U, test.RecordNumber, "RecordNumber");
        Assert.AreEqual(4U, test.StartLineNumber, "LineNumber");

        Assert.IsTrue(test.Read());
        Assert.AreEqual(3U, test.RecordNumber, "RecordNumber");
        Assert.AreEqual(5U, test.StartLineNumber, "LineNumber");

        Assert.IsTrue(test.Read());
        Assert.AreEqual(4U, test.RecordNumber, "RecordNumber");
        Assert.AreEqual(6U, test.StartLineNumber, "LineNumber");

        Assert.IsTrue(test.Read());
        Assert.AreEqual(5U, test.RecordNumber, "RecordNumber");
        Assert.AreEqual(7U, test.StartLineNumber, "LineNumber");

        Assert.IsTrue(test.Read());
        Assert.AreEqual(6U, test.RecordNumber, "RecordNumber");
        Assert.AreEqual(10U, test.StartLineNumber, "LineNumber");

        Assert.IsTrue(test.Read());
        Assert.AreEqual(7U, test.RecordNumber, "RecordNumber");
        Assert.AreEqual(11U, test.StartLineNumber, "LineNumber");

        Assert.IsTrue(test.Read());
        Assert.AreEqual(8U, test.RecordNumber, "RecordNumber");
        Assert.AreEqual(13U, test.StartLineNumber, "LineNumber");

        Assert.IsFalse(test.Read());
      }
    }

    [TestMethod]
    public void SkippingEmptyRowsWithDelimiter()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "SkippingEmptyRowsWithDelimiter.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(6, test.FieldCount);
        Assert.IsTrue(test.Read());
        Assert.AreEqual(3U, test.StartLineNumber, "LineNumber");
        Assert.AreEqual("a", test.GetString(0));
        Assert.AreEqual("b", test.GetString(1));
        Assert.AreEqual("f", test.GetString(5));

        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.AreEqual(14U, test.StartLineNumber, "LineNumber");
        Assert.AreEqual("19", test.GetString(0));
        // No next Line
        Assert.IsFalse(test.Read());
      }
    }

    [TestMethod]
    public void GetValue()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = true
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt");
      setting.ColumnAdd(new Column
      {
        Name = "ExamDate",
        DataType = DataType.DateTime,
        DateFormat = @"dd/MM/yyyy"
      });
      setting.ColumnAdd(new Column
      {
        Name = "ID",
        DataType = DataType.Integer
      });
      setting.ColumnAdd(new Column
      {
        Name = "IsNativeLang",
        DataType = DataType.Boolean
      });

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(6, test.FieldCount);
        Assert.IsTrue(test.Read());
        Assert.AreEqual(new DateTime(2010, 1, 20), test.GetValue(2));
        Assert.AreEqual(true, test.GetValue(5));
        Assert.IsTrue(test.Read());

        Assert.AreEqual(Convert.ToInt32(2), Convert.ToInt32(test.GetValue(0),CultureInfo.InvariantCulture));
        Assert.AreEqual("English", test.GetString(1));
        Assert.AreEqual("22/01/2012", test.GetString(2));
        Assert.AreEqual(new DateTime(2012, 1, 22), test.GetValue(2));
        Assert.AreEqual(false, test.GetValue(5));
      }
    }

    [TestMethod]
    public void SkipRows()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.SkipRows = 2;
      setting.FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(6, test.FieldCount);
        // Start at line 2
        Assert.IsTrue(test.Read());
        Assert.AreEqual(3U, test.StartLineNumber, "LineNumber");
        Assert.AreEqual(1U, test.RecordNumber, "RecordNumber");

        Assert.AreEqual("English", test.GetString(1));
        Assert.AreEqual("22/01/2012", test.GetString(2));
        Assert.IsTrue(test.Read());
        Assert.AreEqual(4U, test.StartLineNumber, "LineNumber");
        Assert.AreEqual(2U, test.RecordNumber, "RecordNumber");

        Assert.IsTrue(test.Read());
        Assert.AreEqual(5U, test.StartLineNumber, "LineNumber");
        Assert.AreEqual(3U, test.RecordNumber, "RecordNumber");

        Assert.IsTrue(test.Read());
        Assert.AreEqual(6U, test.StartLineNumber, "LineNumber");
        Assert.AreEqual(4U, test.RecordNumber, "RecordNumber");
      }
    }

    [TestMethod]
    public void TestConsecutiveEmptyRows2()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = true,
        ConsecutiveEmptyRows = 2,
        SkipEmptyLines = false
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "BasicCSVEmptyLine.txt");
      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(6, test.FieldCount);
        Assert.IsTrue(test.Read(), "Read() 1");

        Assert.IsTrue(test.Read(), "Read() 2");
        Assert.AreEqual("20/01/2010", test.GetString(2));
        // First empty Row, continue
        Assert.IsTrue(test.Read(), "Read() 3");
        // Second Empty Row, Stop
        Assert.IsFalse(test.Read(), "Read() 4");
      }
    }

    [TestMethod]
    public void TestConsecutiveEmptyRows3()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = true,
        ConsecutiveEmptyRows = 3,
        SkipEmptyLines = false
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "BasicCSVEmptyLine.txt");
      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(6, test.FieldCount);
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.AreEqual("20/01/2010", test.GetString(2));
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.AreEqual("22/01/2012", test.GetString(2));
        // No other Line read they are all empty
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsFalse(test.Read());
      }
    }

    [TestMethod]
    public void TextQualifierBeginningAndEnd()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileFormat.EscapeCharacter = "\\";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "TextQualifierBeginningAndEnd.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(5, test.FieldCount);
        Assert.IsTrue(test.Read());
        Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
        // a,b",c"c,d""d,"e""e",""f
        Assert.AreEqual("a", test.GetString(0));
        Assert.AreEqual("b\"", test.GetString(1));
        Assert.AreEqual("c\"c", test.GetString(2));
        Assert.AreEqual("d\"\"d", test.GetString(3));
        Assert.AreEqual("e\"e", test.GetString(4));

        //"g,h,i"",j,k,l"
        Assert.IsTrue(test.Read());
        Assert.AreEqual(2U, test.StartLineNumber, "LineNumber");
        Assert.AreEqual("g,h,i\",j,k,l", test.GetString(0));
        // "m",n\"op\"\"qr,"s\"tu\"\"vw",\"x""""""
        Assert.IsTrue(test.Read());
        Assert.AreEqual(3U, test.StartLineNumber, "LineNumber");
        Assert.AreEqual("m", test.GetString(0));
        Assert.AreEqual("n\"op\"\"qr", test.GetString(1));
        Assert.AreEqual("s\"tu\"\"vw", test.GetString(2));
        Assert.AreEqual("\"x\"\"\"\"\"\"", test.GetString(3));
      }
    }

    [TestMethod]
    public void TextQualifierDataPastClosingQuote()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "TextQualifierDataPastClosingQuote.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(6, test.FieldCount);
        Assert.IsTrue(test.Read());
        //"a"a,b,c,d,e,f
        Assert.AreEqual("a", test.GetString(0));
        Assert.AreEqual("b", test.GetString(1));
        Assert.AreEqual("c", test.GetString(2));
        Assert.AreEqual("d", test.GetString(3));
        Assert.AreEqual("e", test.GetString(4));
        Assert.AreEqual("f", test.GetString(5));
        //1,2,"3" ignore me,4,5,6
        Assert.IsTrue(test.Read());
        Assert.AreEqual("1", test.GetString(0));
        Assert.AreEqual("2", test.GetString(1));
        Assert.AreEqual("3", test.GetString(2));
        Assert.AreEqual("4", test.GetString(3));
        Assert.AreEqual("5", test.GetString(4));
        Assert.AreEqual("6", test.GetString(5));
        Assert.IsFalse(test.Read());
      }
    }

    [TestMethod]
    public void TextQualifierNotClosedAtEnd()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileFormat.EscapeCharacter = "\\";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "TextQualifierNotClosedAtEnd.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(6, test.FieldCount);
        Assert.IsTrue(test.Read());
        Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
        // "a","b ",c," , d",e,f
        Assert.AreEqual("a", test.GetString(0));
        Assert.AreEqual("b   ", test.GetString(1));
        Assert.AreEqual("c", test.GetString(2));
        Assert.AreEqual(" ,  d", test.GetString(3));
        Assert.AreEqual("e", test.GetString(4));
        Assert.AreEqual("f", test.GetString(5));

        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.AreEqual(4U, test.StartLineNumber, "LineNumber");
        //7," ,8, ",9,10,11,12
        Assert.AreEqual("7", test.GetString(0));
        Assert.AreEqual(" ,8, ", test.GetString(1));
        Assert.AreEqual("9", test.GetString(2));
        Assert.AreEqual("10", test.GetString(3));
        Assert.AreEqual("11", test.GetString(4));
        Assert.AreEqual("12", test.GetString(5));

        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.StartLineNumber >= 8 && test.StartLineNumber <= 9, "LineNumber");
        //"19 , ",20,21,22,23,"
        Assert.AreEqual("19 , ", test.GetString(0));
        Assert.AreEqual("20", test.GetString(1));
        Assert.AreEqual("21", test.GetString(2));
        Assert.AreEqual("22", test.GetString(3));
        Assert.AreEqual("23", test.GetString(4));
        //Assert.AreEqual(null, test.GetString(5));
        Assert.AreEqual(" \r\n 24", test.GetString(5));
      }
    }

    [TestMethod]
    public void TextQualifiers()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "TextQualifiers.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(6, test.FieldCount);
        Assert.IsTrue(test.Read());
        Assert.AreEqual("a", test.GetString(0));
        Assert.AreEqual("b \"  ", test.GetString(1));
        Assert.AreEqual("c", test.GetString(2));
        Assert.AreEqual("   d", test.GetString(3));
        Assert.AreEqual("e", test.GetString(4));
        Assert.AreEqual("f", test.GetString(5));
        //1,2,3,4,5,6
        Assert.IsTrue(test.Read());
        Assert.AreEqual("1", test.GetString(0));
        Assert.AreEqual("2", test.GetString(1));
        Assert.AreEqual("3", test.GetString(2));
        Assert.AreEqual("4", test.GetString(3));
        Assert.AreEqual("5", test.GetString(4));
        Assert.AreEqual("6", test.GetString(5));
      }
    }

    [TestMethod]
    public void TextQualifiersWithDelimiters()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false,
        WarnDelimiterInValue = true
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileFormat.EscapeCharacter = "\\";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "TextQualifiersWithDelimiters.txt");
      var warningList = new RowErrorCollection();
      using (var test = new CsvFileReader(setting))
      {
        test.Warning += warningList.Add;
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(6, test.FieldCount);
        Assert.IsTrue(test.Read());
        Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
        // "a","b ",c," , d",e,f
        Assert.AreEqual("a", test.GetString(0));
        Assert.AreEqual("b   ", test.GetString(1));
        Assert.AreEqual("c", test.GetString(2));
        Assert.AreEqual(" ,  d", test.GetString(3));
        Assert.AreEqual("e", test.GetString(4));
        Assert.AreEqual("f", test.GetString(5));
        Assert.AreEqual(1, warningList.CountRows, "Warnings count");
        Assert.IsTrue(warningList.Display.Contains("Field delimiter"));

        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.AreEqual(4U, test.StartLineNumber, "LineNumber");
        //7," ,8, ",9,10,11,12
        Assert.AreEqual("7", test.GetString(0));
        Assert.AreEqual(" ,8, ", test.GetString(1));
        Assert.AreEqual("9", test.GetString(2));
        Assert.AreEqual("10", test.GetString(3));
        Assert.AreEqual("11", test.GetString(4));
        Assert.AreEqual("12", test.GetString(5));

        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.StartLineNumber >= 8 && test.StartLineNumber <= 9, "LineNumber");

        Assert.AreEqual("19 , ", test.GetString(0));
        Assert.AreEqual("20", test.GetString(1));
        Assert.AreEqual("21", test.GetString(2));
        Assert.AreEqual("22", test.GetString(3));
        Assert.AreEqual("23", test.GetString(4));
        Assert.AreEqual(" \r\n 24", test.GetString(5));
      }
    }

    [TestMethod]
    public void TrimmingHeaders()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = true
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileFormat.CommentLine = "#";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "TrimmingHeaders.txt");
      var warningList = new RowErrorCollection();
      using (var test = new CsvFileReader(setting))
      {
        test.Warning += warningList.Add;
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(6, test.FieldCount);
        Assert.AreEqual("a", test.GetName(0));
        Assert.AreEqual("b", test.GetName(1));
        Assert.AreEqual("c", test.GetName(2));
        Assert.AreEqual("d", test.GetName(3));
        Assert.AreEqual("Column5", test.GetName(4));
        Assert.AreEqual("f", test.GetName(5));
        Assert.IsTrue(warningList.CountRows >= 1);
        Assert.IsTrue(warningList.Display.Contains("leading or tailing spaces"));
      }
    }

    [TestMethod]
    public void UnicodeUTF16BE()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false,
        CodePageId = 1201
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "UnicodeUTF16BE.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(Encoding.BigEndianUnicode, setting.CurrentEncoding);
        Assert.AreEqual(4, test.FieldCount);
        Assert.IsTrue(test.Read());
        Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
        Assert.AreEqual("tölvuiðnaðarins", test.GetString(1));
        Assert.AreEqual("ũΩ₤", test.GetString(2));
        Assert.AreEqual("používat", test.GetString(3));

        Assert.IsTrue(test.Read());
        Assert.AreEqual(2U, test.StartLineNumber, "LineNumber");

        Assert.AreEqual("různá", test.GetString(0));
        Assert.AreEqual("čísla", test.GetString(1));
        Assert.AreEqual("pro", test.GetString(2));
        Assert.AreEqual("członkowskich", test.GetString(3));

        Assert.IsTrue(test.Read());
        Assert.AreEqual(3U, test.StartLineNumber, "LineNumber");

        Assert.AreEqual("rozumieją", test.GetString(0));
        Assert.AreEqual("přiřazuje", test.GetString(1));
        Assert.AreEqual("gemeinnützige", test.GetString(2));
        Assert.AreEqual("är också", test.GetString(3));

        Assert.IsTrue(test.Read());
        Assert.AreEqual(4U, test.StartLineNumber, "LineNumber");

        Assert.AreEqual("sprachunabhängig", test.GetString(0));
        Assert.AreEqual("that's all", test.GetString(1));
        Assert.AreEqual("for", test.GetString(2));
        Assert.AreEqual("now", test.GetString(3));

        Assert.IsFalse(test.Read());
      }
    }

    [TestMethod]
    public void UnicodeUTF8()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false,
        CodePageId = 65001
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileName = Path.Combine(m_ApplicationDirectory, "UnicodeUTF8.txt");

      using (var test = new CsvFileReader(setting))
      {
        test.Open(false, CancellationToken.None);
        Assert.AreEqual(Encoding.UTF8, setting.CurrentEncoding);
        Assert.AreEqual(4, test.FieldCount);
        Assert.IsTrue(test.Read());
        Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");

        Assert.AreEqual("tölvuiðnaðarins", test.GetString(1));
        Assert.AreEqual("ũΩ₤", test.GetString(2));
        Assert.AreEqual("používat", test.GetString(3));

        Assert.IsTrue(test.Read());
        Assert.AreEqual(2U, test.StartLineNumber, "LineNumber");

        Assert.AreEqual("různá", test.GetString(0));
        Assert.AreEqual("čísla", test.GetString(1));
        Assert.AreEqual("pro", test.GetString(2));
        Assert.AreEqual("członkowskich", test.GetString(3));

        Assert.IsTrue(test.Read());
        Assert.AreEqual(3U, test.StartLineNumber, "LineNumber");

        Assert.AreEqual("rozumieją", test.GetString(0));
        Assert.AreEqual("přiřazuje", test.GetString(1));
        Assert.AreEqual("gemeinnützige", test.GetString(2));
        Assert.AreEqual("är också", test.GetString(3));

        Assert.IsTrue(test.Read());
        Assert.AreEqual(4U, test.StartLineNumber, "LineNumber");

        Assert.AreEqual("sprachunabhängig", test.GetString(0));
        Assert.AreEqual("that's all", test.GetString(1));
        Assert.AreEqual("for", test.GetString(2));
        Assert.AreEqual("now", test.GetString(3));

        Assert.IsFalse(test.Read());
      }
    }
  }
}