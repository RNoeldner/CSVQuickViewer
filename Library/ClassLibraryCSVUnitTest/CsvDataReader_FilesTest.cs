/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
 * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License along with this program.
 * If not, see http://www.gnu.org/licenses/ .
 *
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace CsvTools.Tests
{
  [TestClass]
  public class CsvDataReaderUnitTestReadFiles
  {
    [TestMethod]
    public void ReadDateWithTime()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitialize.GetTestPath("Sessions.txt"),
        HasFieldHeader = true,
        ByteOrderMark = true
      };
      setting.FileFormat.FieldDelimiter = "\t";
      setting.ColumnCollection.AddIfNew(new Column("Start Date", DataType.DateTime, "MM/dd/yyyy")
      {
        TimePart = "Start Time",
        TimePartFormat = "HH:mm:ss"
      });

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        test.Read();
        var cultureInfo = new CultureInfo("en-US");
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
        FileName = UnitTestInitialize.GetTestPath("Sessions.txt"),
        HasFieldHeader = true,
        ByteOrderMark = true
      };
      setting.FileFormat.FieldDelimiter = "\t";
      setting.ColumnCollection.AddIfNew(new Column("Start Date", DataType.DateTime, "MM/dd/yyyy")
      {
        TimePart = "Start Time",
        TimePartFormat = "HH:mm:ss",
        TimeZonePart = "Time Zone"
      });

      // all will be converted to TimeZoneInfo.Local, but we concert then to UTC
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        test.Read();
        var cultureInfo = new CultureInfo("en-US");
        // 01/08/2013 07:00:00 IST --> 01/08/2013 01:30:00 UTC
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
      var setting = new CsvFile
      {
        HasFieldHeader = false,
      };
      setting.FileFormat.AlternateQuoting = true;
      setting.TrimmingOption = TrimmingOption.All;
      setting.FileName = UnitTestInitialize.GetTestPath("AlternateTextQualifiers.txt");

      using (var processDisplay = new DummyProcessDisplay())
      {
        using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
        {
          test.Open();
          processDisplay.Cancel();
          Assert.IsFalse(test.Read());
        }
      }
    }

    [TestMethod]
    public void AlternateTextQualifiersDoubleQuotes()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false
      };

      setting.FileFormat.FieldDelimiter = ",";
      setting.FileFormat.AlternateQuoting = true;
      setting.FileFormat.DuplicateQuotingToEscape = true;
      setting.FileName = UnitTestInitialize.GetTestPath("AlternateTextQualifiersDoubleQuote.txt");
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        Assert.AreEqual("This is a \"Test\" of doubled quoted Text", test.GetString(1), " \"\"should be regarded as \"");
        Assert.IsTrue(test.Read());
        Assert.AreEqual("This is a \"Test\" of not repeated quotes", test.GetString(1), "Training space not trimmed");
        Assert.IsTrue(test.Read());
        Assert.AreEqual("Tricky endig duplicated quotes that close...\nLine \"Test\"", StringUtils.HandleCRLFCombinations(test.GetString(1)), "Ending with two doble quotes but one is a closing quote");
      }
    }

    [TestMethod]
    public void AlternateTextQualifiersTrimQuoted()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileFormat.AlternateQuoting = true;
      setting.TrimmingOption = TrimmingOption.All;
      setting.FileName = UnitTestInitialize.GetTestPath("AlternateTextQualifiers.txt");
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.IsTrue(test.Read());
        Assert.AreEqual("a", test.GetString(0), "Start of file with quote");
        Assert.AreEqual("a", test.GetValue(0), "Start of file with quote");
        Assert.AreEqual("b \"", test.GetString(1), "Containing Quote");
        Assert.AreEqual("d", test.GetString(3), "Leading space not trimmed");
        Assert.AreEqual("This is a\n\" in Quotes", StringUtils.HandleCRLFCombinations(test.GetString(4)), "Linefeed in quotes");
        Assert.IsTrue(test.Read());
        Assert.AreEqual("13", test.GetString(0), "Start of line with quote");
        Assert.AreEqual("This is a \"Test\"", test.GetString(3), "Containing Quote and quote at the end");
        Assert.AreEqual("18", test.GetString(5), "Line ending with quote");

        Assert.IsTrue(test.Read());
        Assert.AreEqual("19", test.GetString(0), "Training space not trimmed");
        Assert.AreEqual("20,21", test.GetString(1), "Delimiter in quotes");
        Assert.AreEqual("Another\nLine \"Test\"", StringUtils.HandleCRLFCombinations(test.GetString(2)), "Linefeed as last char in quotes");
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
        WarnQuotes = true
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileFormat.AlternateQuoting = true;
      setting.TrimmingOption = TrimmingOption.Unquoted;
      setting.FileName = UnitTestInitialize.GetTestPath("AlternateTextQualifiers.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        var warningList = new RowErrorCollection(test);
        test.Open();
        warningList.HandleIgnoredColumns(test);

        Assert.IsTrue(test.Read());
        Assert.AreEqual("a", test.GetString(0), "Start of file with quote");
        Assert.AreEqual("b \"  ", test.GetString(1), "Containing Quote");
        Assert.IsTrue(warningList.Display.Contains("Field qualifier"));
        Assert.AreEqual("   d", test.GetString(3), "Leading space not trimmed");
        Assert.AreEqual("This is a\n\" in Quotes", StringUtils.HandleCRLFCombinations(test.GetString(4)), "Linefeed in quotes");
        Assert.IsTrue(test.Read());
        Assert.AreEqual("13", test.GetString(0), "Start of line with quote");
        Assert.AreEqual("This is a \"Test\"", test.GetString(3), "Containing Quote and quote at the end");
        Assert.AreEqual("18", test.GetString(5), "Line ending with quote");

        Assert.IsTrue(test.Read());
        Assert.AreEqual("19  ", test.GetString(0), "Training space not trimmed");
        Assert.AreEqual("20,21", test.GetString(1), "Delimiter in quotes");
        Assert.AreEqual("Another\nLine \"Test\"", StringUtils.HandleCRLFCombinations(test.GetString(2)), "Linefeed as last char in quotes");
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
      setting.FileName = UnitTestInitialize.GetTestPath("BasicEscapedCharacters.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
      setting.FileName = UnitTestInitialize.GetTestPath("ComplexDataDelimiter.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        var message = string.Empty;
        test.Warning += delegate (object sender, WarningEventArgs args)
        { message = args.Message; };
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
      setting.FileName = UnitTestInitialize.GetTestPath("ComplexDataDelimiter.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        test.Read();
        test.Read();
        var message = string.Empty;
        test.Warning += delegate (object sender, WarningEventArgs args)
        { message = args.Message; };
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
      setting.FileName = UnitTestInitialize.GetTestPath("ComplexDataDelimiter.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
        Assert.AreEqual("f\n  , ", StringUtils.HandleCRLFCombinations(test.GetString(5)));

        Assert.AreEqual(4, test.StartLineNumber, "StartLineNumber");
        Assert.AreEqual(6, test.EndLineNumber, "EndLineNumber");

        Assert.IsTrue(test.Read()); // 3
        Assert.AreEqual(6U, test.StartLineNumber, "StartLineNumber");
        Assert.AreEqual("6\"", test.GetString(5));

        Assert.IsTrue(test.Read()); // 4
        Assert.AreEqual("k\n", StringUtils.HandleCRLFCombinations(test.GetString(4)));
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
        Assert.AreEqual("f\n", StringUtils.HandleCRLFCombinations(test.GetString(5)));
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
      setting.FileName = UnitTestInitialize.GetTestPath("QuoteInText.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
        Assert.AreEqual("f\n", StringUtils.HandleCRLFCombinations(test.GetString(5)));
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
      setting.FileName = UnitTestInitialize.GetTestPath("ComplexDataDelimiter.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
        Assert.AreEqual("f\n  ,", StringUtils.HandleCRLFCombinations(test.GetString(5)));
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

      setting.FileName = UnitTestInitialize.GetTestPath("CSVTestEmpty.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
      setting.FileName = UnitTestInitialize.GetTestPath("DifferentColumnDelimiter.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
      setting.FileName = UnitTestInitialize.GetTestPath("EscapedCharacterAtEndOfFile.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
      setting.TreatLFAsSpace = false;
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileFormat.EscapeCharacter = "\\";
      setting.FileName = UnitTestInitialize.GetTestPath("EscapedCharacterAtEndOfRowDelimiter.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.AreEqual(6, test.FieldCount);
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.AreEqual("l", test.GetString(5));  // Important to not trim the value otherwise the linefeed is gone
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
      setting.FileName = UnitTestInitialize.GetTestPath("EscapedCharacterAtEndOfRowDelimiter.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
      setting.FileName = UnitTestInitialize.GetTestPath("EscapeWithoutTextQualifier.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
      setting.FileName = UnitTestInitialize.GetTestPath("HandlingDuplicateColumnNames.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        var message = string.Empty;
        test.Warning += delegate (object sender, WarningEventArgs args)
        { message = args.Message; };
        test.Open();
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
      setting.FileName = UnitTestInitialize.GetTestPath("LastRowWithRowDelimiter.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
      setting.FileName = UnitTestInitialize.GetTestPath("LastRowWithRowDelimiter.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
      setting.FileName = UnitTestInitialize.GetTestPath("LongHeaders.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        var warningsList = new RowErrorCollection(test);
        test.Open();

        Assert.AreEqual(6, test.FieldCount);
        Assert.AreEqual("a", test.GetName(0));
        Assert.AreEqual("b", test.GetName(1));
        Assert.AreEqual(
          "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquya"
            .Substring(0, 128), test.GetName(2));
        Assert.AreEqual("d", test.GetName(3));
        Assert.AreEqual("e", test.GetName(4));
        Assert.AreEqual("f", test.GetName(5));
        Assert.AreEqual(1, warningsList.CountRows, "Warnings");
        Assert.IsTrue(warningsList.Display.Contains("has been cut off"));

        // check if we read the right line , and we do not end up in a commented line of read the
        // header ahgin
        test.Read();
        Assert.AreEqual("1", test.GetString(0));
        Assert.AreEqual("6", test.GetString(5));
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
      setting.FileName = UnitTestInitialize.GetTestPath("MoreColumnsThanHeaders.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        var warningList = new RowErrorCollection(test);
        test.Open();

        warningList.HandleIgnoredColumns(test);

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
        // Assert.IsTrue(warningList.Display.Contains("The existing data in these extra columns is
        // not read"));

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
      setting.FileName = UnitTestInitialize.GetTestPath("TextQualifierDataPastClosingQuote.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
      setting.FileName = UnitTestInitialize.GetTestPath("Placeholder.txt");
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
      pd.ProgressStopEvent += delegate
      { stopped = true; };
      Assert.AreEqual(null, pd.Text);
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, pd))
      {
        test.Open();

        for (var i = 0; i < 500; i++)
          Assert.IsTrue(test.Read());
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
      setting.FileName = UnitTestInitialize.GetTestPath("ReadingInHeaderAfterComments.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();

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
      setting.FileName = UnitTestInitialize.GetTestPath("RowWithoutColumnDelimiter.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
      setting.FileName = UnitTestInitialize.GetTestPath("SimpleDelimiterWithControlCharacters.txt");
      setting.FileFormat.CommentLine = "#";
      setting.WarnNBSP = true;
      setting.WarnUnknowCharater = true;

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        var warningList = new RowErrorCollection(test);
        test.Open();
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
      setting.FileName = UnitTestInitialize.GetTestPath("SimpleDelimiterWithControlCharacters.txt");
      setting.FileFormat.CommentLine = "#";

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        var warningList = new RowErrorCollection(test);
        test.Open();
        Assert.AreEqual(6, test.FieldCount);
        Assert.AreEqual("1", test.GetName(0));
        Assert.AreEqual("2", test.GetName(1));
        Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");

        Assert.IsTrue(test.Read());
        Assert.IsTrue(test.Read());
        Assert.AreEqual(3U, test.StartLineNumber, "LineNumber");
        // g,h,i , j,k,l
        Assert.AreEqual(" j", test.GetString(3));

        // #A NBSP: Create with Alt+01602 7,8,9,10 ,11,12
        Assert.IsTrue(test.Read());
        Assert.AreEqual("7", test.GetString(0));
        Assert.AreEqual("10 ", test.GetString(3));
        Assert.AreEqual(2, warningList.CountRows);
        Assert.IsTrue(warningList.Display.Contains("Non Breaking Space"));

        Assert.IsTrue(test.Read());
        // m,n,o,p ,q,r
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
      setting.FileName = UnitTestInitialize.GetTestPath("BasicCSV.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
      setting.FileName = UnitTestInitialize.GetTestPath("SkippingComments.txt");
      setting.FileFormat.CommentLine = "#";
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
      setting.FileName = UnitTestInitialize.GetTestPath("SkippingEmptyRowsWithDelimiter.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
      setting.FileName = UnitTestInitialize.GetTestPath("BasicCSV.txt");
      setting.ColumnCollection.AddIfNew(new Column("ExamDate", DataType.DateTime, @"dd/MM/yyyy"));
      setting.ColumnCollection.AddIfNew(new Column("ID", DataType.Integer));
      setting.ColumnCollection.AddIfNew(new Column("IsNativeLang", DataType.Boolean));

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
        Assert.AreEqual(6, test.FieldCount);
        Assert.IsTrue(test.Read());
        Assert.AreEqual(new DateTime(2010, 1, 20), test.GetValue(2));
        Assert.AreEqual(true, test.GetValue(5));
        Assert.IsTrue(test.Read());

        Assert.AreEqual(Convert.ToInt32(2), Convert.ToInt32(test.GetValue(0), CultureInfo.InvariantCulture));
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
      setting.FileName = UnitTestInitialize.GetTestPath("BasicCSV.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
      setting.FileName = UnitTestInitialize.GetTestPath("BasicCSVEmptyLine.txt");
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
      setting.FileName = UnitTestInitialize.GetTestPath("BasicCSVEmptyLine.txt");
      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
      setting.FileName = UnitTestInitialize.GetTestPath("TextQualifierBeginningAndEnd.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
      setting.FileName = UnitTestInitialize.GetTestPath("TextQualifierDataPastClosingQuote.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
      setting.FileName = UnitTestInitialize.GetTestPath("TextQualifierNotClosedAtEnd.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
        Assert.AreEqual(" \n 24", StringUtils.HandleCRLFCombinations(test.GetString(5)));
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
      setting.FileName = UnitTestInitialize.GetTestPath("TextQualifiers.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
      setting.FileName = UnitTestInitialize.GetTestPath("TextQualifiersWithDelimiters.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        var warningList = new RowErrorCollection(test);
        test.Open();
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
        Assert.AreEqual(" \n 24", StringUtils.HandleCRLFCombinations(test.GetString(5)));
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
      setting.FileName = UnitTestInitialize.GetTestPath("TrimmingHeaders.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        var warningList = new RowErrorCollection(test);
        test.Open();
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
        CodePageId = 1201,
        ByteOrderMark = true
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileName = UnitTestInitialize.GetTestPath("UnicodeUTF16BE.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
    public void UnicodeUTF16LE()
    {
      var setting = new CsvFile
      {
        HasFieldHeader = false,
        CodePageId = 1200,
        ByteOrderMark = true
      };
      setting.FileFormat.FieldDelimiter = ",";
      setting.FileName = UnitTestInitialize.GetTestPath("UnicodeUTF16LE.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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
      setting.FileName = UnitTestInitialize.GetTestPath("UnicodeUTF8.txt");

      using (var processDisplay = new DummyProcessDisplay())
      using (var test = new CsvFileReader(setting, TimeZoneInfo.Local.Id, processDisplay))
      {
        test.Open();
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