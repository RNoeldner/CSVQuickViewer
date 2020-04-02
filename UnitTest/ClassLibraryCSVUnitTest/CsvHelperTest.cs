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
using System.IO;
using System.Linq;
using System.Threading;

namespace CsvTools.Tests
{
  [TestClass]
  public class CsvHelperTest
  {
    [TestMethod]
    public void GuessCodePage()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitialize.GetTestPath("BasicCSV.txt")
      };
      CsvHelper.GuessCodePageAsync(setting).WaitToCompleteTask(5);
      Assert.AreEqual(1200, setting.CodePageId);

      var setting2 = new CsvFile
      {
        FileName = UnitTestInitialize.GetTestPath("UnicodeUTF16BE.txt")
      };
      CsvHelper.GuessCodePageAsync(setting2).WaitToCompleteTask(5);
      Assert.AreEqual(1201, setting2.CodePageId);

      var setting3 = new CsvFile
      {
        FileName = UnitTestInitialize.GetTestPath("Test.csv")
      };
      CsvHelper.GuessCodePageAsync(setting3).WaitToCompleteTask(5);
      Assert.AreEqual(65001, setting3.CodePageId);
    }

    [TestMethod]
    public void GuessHasHeader()
    {

      using (var stream = ImprovedStream.OpenRead(UnitTestInitialize.GetTestPath("BasicCSV.txt")))
      using (var reader = new ImprovedTextReader(stream, 65001, 0))
        Assert.IsTrue(CsvHelper.GuessHasHeaderAsync(reader, "#", ',', CancellationToken.None).WaitToCompleteTask(2));

      using (var stream = ImprovedStream.OpenRead(UnitTestInitialize.GetTestPath("HandlingDuplicateColumnNames.txt")))
      using (var reader = new ImprovedTextReader(stream, 65001, 0))
        Assert.IsFalse(CsvHelper.GuessHasHeaderAsync(reader, "#", ',', CancellationToken.None).WaitToCompleteTask(2));
    }

    [TestMethod]
    public void GuessStartRow() => Assert.AreEqual(0, CsvHelper.GuessStartRowAsync(new CsvFile
    {
      FileName = UnitTestInitialize.GetTestPath("BasicCSV.txt")
    }, CancellationToken.None).WaitToCompleteTask(2), "BasicCSV.txt");

    [TestMethod]
    public void GuessStartRowWithComments()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitialize.GetTestPath("LongHeaders.txt")
      };
      setting.FileFormat.CommentLine = "#";
      Assert.AreEqual(0, CsvHelper.GuessStartRowAsync(setting, CancellationToken.None).WaitToCompleteTask(2), "LongHeaders.txt");
    }

    [TestMethod]
    public void GuessDelimiter()
    {
      Assert.AreEqual(",", CsvHelper.GuessDelimiterAsync(new CsvFile(UnitTestInitialize.GetTestPath("BasicCSV.txt")), CancellationToken.None).WaitToCompleteTask(2));
      Assert.AreEqual("|", CsvHelper.GuessDelimiterAsync(new CsvFile(UnitTestInitialize.GetTestPath("AllFormatsPipe.txt")), CancellationToken.None).WaitToCompleteTask(2));

      ICsvFile test = new CsvFile(UnitTestInitialize.GetTestPath("LateStartRow.txt"))
      {
        SkipRows = 10,
        CodePageId = 20127
      };
      test.FileFormat.FieldQualifier = "\"";
      Assert.AreEqual("|", CsvHelper.GuessDelimiterAsync(test, CancellationToken.None).WaitToCompleteTask(2));
    }

    [TestMethod]
    public void RefreshCsvFile()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitialize.GetTestPath("BasicCSV.txt")
      };
      using (var processDisplay = new DummyProcessDisplay())
      {
        CsvHelper.RefreshCsvFileAsync(setting, processDisplay).WaitToCompleteTask(10);
      }

      Assert.AreEqual(1200, setting.CodePageId);
      Assert.AreEqual(",", setting.FileFormat.FieldDelimiter);

      foreach (var fileName in FileSystemUtils.GetFiles(UnitTestInitialize.ApplicationDirectory, "AllFor*.txt"))
      {
        var testSetting = new CsvFile(fileName);
        using (var processDisplay = new DummyProcessDisplay())
        {
          CsvHelper.RefreshCsvFileAsync(testSetting, processDisplay).WaitToCompleteTask(10);
        }
      }
    }

    [TestMethod]
    public void GuessDelimiterComma()
    {
      ICsvFile test = new CsvFile(UnitTestInitialize.GetTestPath("AlternateTextQualifiers.txt"))
      {
        CodePageId = -1
      };
      test.FileFormat.EscapeCharacter = "\\";

      Assert.AreEqual(",", CsvHelper.GuessDelimiterAsync(test, CancellationToken.None).WaitToCompleteTask(2));
    }

    [TestMethod]
    public void GuessDelimiterPipe()
    {
      ICsvFile test = new CsvFile(UnitTestInitialize.GetTestPath("DifferentColumnDelimiter.txt"))
      {
        CodePageId = -1
      };
      test.FileFormat.EscapeCharacter = "";
      Assert.AreEqual("|", CsvHelper.GuessDelimiterAsync(test, CancellationToken.None).WaitToCompleteTask(2));
    }

    [TestMethod]
    public void GuessDelimiterQualifier()
    {
      ICsvFile test = new CsvFile(UnitTestInitialize.GetTestPath("TextQualifiers.txt"))
      {
        CodePageId = -1
      };
      test.FileFormat.EscapeCharacter = "";
      Assert.AreEqual(",", CsvHelper.GuessDelimiterAsync(test, CancellationToken.None).WaitToCompleteTask(2));
    }

    [TestMethod]
    public void GuessDelimiterTab()
    {
      ICsvFile test = new CsvFile(UnitTestInitialize.GetTestPath("txTranscripts.txt"))
      {
        CodePageId = -1
      };
      test.FileFormat.EscapeCharacter = "\\";
      Assert.AreEqual("TAB", CsvHelper.GuessDelimiterAsync(test, CancellationToken.None).WaitToCompleteTask(2));
    }

    [TestMethod]
    public void GuessStartRow0()
    {
      ICsvFile test = new CsvFile(UnitTestInitialize.GetTestPath("TextQualifiers.txt"))
      {
        CodePageId = -1
      };
      test.FileFormat.FieldDelimiter = ",";
      test.FileFormat.FieldQualifier = "\"";
      Assert.AreEqual(0, CsvHelper.GuessStartRowAsync(test, CancellationToken.None).WaitToCompleteTask(2));
    }

    [TestMethod]
    public void GuessStartRow12()
    {
      ICsvFile test = new CsvFile(UnitTestInitialize.GetTestPath("SkippingEmptyRowsWithDelimiter.txt"))
      {
        CodePageId = -1
      };
      test.FileFormat.FieldDelimiter = ",";
      test.FileFormat.FieldQualifier = "\"";
      Assert.AreEqual(12, CsvHelper.GuessStartRowAsync(test, CancellationToken.None).WaitToCompleteTask(2));
    }

    [TestMethod]
    public void HasUsedQualifierFalse()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitialize.GetTestPath("BasicCSV.txt"),
        HasFieldHeader = true
      };

      Assert.IsFalse(CsvHelper.HasUsedQualifierAsync(setting, CancellationToken.None).WaitToCompleteTask(10));
    }

    [TestMethod]
    public void HasUsedQualifierTrue()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitialize.GetTestPath("AlternateTextQualifiers.txt")
      };
      Assert.IsTrue(CsvHelper.HasUsedQualifierAsync(setting, CancellationToken.None).WaitToCompleteTask(10));
    }

    [TestMethod]
    public void NewCsvFileGuessAllHeadings()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitialize.GetTestPath("BasicCSV.txt")
      };
      using (var display = new DummyProcessDisplay())
      {
        setting.RefreshCsvFileAsync(display).WaitToCompleteTask(10);
      }

      Assert.AreEqual(0, setting.SkipRows);
      Assert.AreEqual(",", setting.FileFormat.FieldDelimiter);
      Assert.AreEqual(1200, setting.CodePageId); //UTF16_LE
    }

    [TestMethod]
    public void NewCsvFileGuessAllTestEmpty()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitialize.GetTestPath("CSVTestEmpty.txt")
      };
      using (var display = new DummyProcessDisplay())
      {
        setting.RefreshCsvFileAsync(display).WaitToCompleteTask(30);
      }

      Assert.AreEqual(0, setting.SkipRows);
    }

    [TestMethod]
    public void TestGuessStartRow()
    {
      ICsvFile test = new CsvFile(UnitTestInitialize.GetTestPath("LateStartRow.txt"))
      {
        CodePageId = 20127
      };
      test.FileFormat.FieldDelimiter = "|";
      test.FileFormat.FieldQualifier = "\"";
      test.SkipRows = CsvHelper.GuessStartRowAsync(test, CancellationToken.None).WaitToCompleteTask(2);

      using (var display = new DummyProcessDisplay())
      using (var reader = new CsvFileReader(test, TimeZoneInfo.Local.Id, display))
      {
        reader.Open();
        Assert.AreEqual("RecordNumber", reader.GetName(0));
        reader.Read();
        Assert.AreEqual("0F8C40DB-EE2C-4C7C-A226-3C43E72584B0", reader.GetString(1));
      }
      Assert.AreEqual(10, test.SkipRows);
    }

  

    [TestMethod]
    public void GuessJsonFile()
    {
      var setting = new CsvFile
      {
        JsonFormat = true,
        FileName = UnitTestInitialize.GetTestPath("Jason1.json"),
      };

      Assert.IsTrue(CsvHelper.GuessJsonFileAsync(setting, CancellationToken.None).WaitToCompleteTask(2));
    }

    [TestMethod]
    public void GetEmptyColumnHeaderTest()
    {
      var setting = new CsvFile
      {
        FileFormat = { FieldDelimiter = "," },
        FileName = UnitTestInitialize.GetTestPath("EmptyColumns.txt"),
        HasFieldHeader = true
      };
      using (var disp = new DummyProcessDisplay())
      {
        using (var read = new CsvFileReader(setting, TimeZoneInfo.Local.Id, disp))
        {
          read.Open();
          var res = CsvHelper.GetEmptyColumnHeader(read, disp.CancellationToken);

          Assert.IsFalse(res.Count == 0);
          Assert.AreEqual("ID", res.First());
        }
      }
    }

    [TestMethod]
    public void GuessNewlineTest()
    {
      // Storing Text file with given line ends is tricky, editor and source control might change
      // them therefore creating the text files in code

      var path = UnitTestInitialize.GetTestPath("TestFile.txt");
      try
      {
        FileSystemUtils.FileDelete(path);
        using (var file = File.CreateText(path))
        {
          file.Write("ID\tTitle\tObject ID\n");
          file.Write("12367890\t\"5\rOverview\"\tc41f21c8-d2cc-472b-8cd9-707ddd8d24fe\n");
          file.Write("3ICC\t10/14/2010\t0e413ed0-3086-47b6-90f3-836a24f7cb2e\n");
          file.Write("3SOF\t\"3 Overview\"\taff9ed00-016e-4202-a3df-27a3ce443e80\n");
          file.Write("3T1SA\t3 Phase 1\t8d527a23-2777-4754-a73d-029f67abe715\n");
          file.Write("3T22A\t3 Phase 2\tf9a99add-4cc2-4e41-a29f-a01f5b3b61b2\n");
          file.Write("3T25C\t3 Phase 2\tab416221-9f79-484e-a7c9-bc9a375a6147\n");
          file.Write("7S721A\t\"7 راز\"\t2b9d291f-ce76-4947-ae7b-fec3531d1766\n");
          file.Write("#Hello\t7th Heaven\t1d5b894b-95e6-4026-9ffe-64197e79c3d1\n");
        }

        var test = new CsvFile(path) { CodePageId = 65001, FileFormat = { FieldQualifier = "\"" } };

        Assert.AreEqual("LF", CsvHelper.GuessNewlineAsync(test, CancellationToken.None).WaitToCompleteTask(2));

        FileSystemUtils.FileDelete(path);
        using (var file = File.CreateText(path))
        {
          file.Write("ID\tTitle\tObject ID\n\r");
          file.Write("12367890\t\"5\nOverview\"\tc41f21c8-d2cc-472b-8cd9-707ddd8d24fe\n\r");
          file.Write("3ICC\t10/14/2010\t0e413ed0-3086-47b6-90f3-836a24f7cb2e\n\r");
          file.Write("3SOF\t\"3 Overview\"\taff9ed00-016e-4202-a3df-27a3ce443e80\n\r");
          file.Write("3T1SA\t3 Phase 1\t8d527a23-2777-4754-a73d-029f67abe715\n\r");
          file.Write("3T22A\t3 Phase 2\tf9a99add-4cc2-4e41-a29f-a01f5b3b61b2\n\r");
          file.Write("3T25C\t3 Phase 2\tab416221-9f79-484e-a7c9-bc9a375a6147\n\r");
          file.Write("7S721A\t\"7 راز\"\t2b9d291f-ce76-4947-ae7b-fec3531d1766\n\r");
          file.Write("#Hello\t7th Heaven\t1d5b894b-95e6-4026-9ffe-64197e79c3d1\n\r");
        }
        Assert.AreEqual("LFCR", CsvHelper.GuessNewlineAsync(test, CancellationToken.None).WaitToCompleteTask(2));

        FileSystemUtils.FileDelete(path);
        using (var file = File.CreateText(path))
        {
          file.Write("ID\tTitle\tObject ID\r\n");
          file.Write("12367890\t\"5\nOverview\"\tc41f21c8-d2cc-472b-8cd9-707ddd8d24fe\r\n");
          file.Write("3ICC\t10/14/2010\t0e413ed0-3086-47b6-90f3-836a24f7cb2e\r\n");
          file.Write("3SOF\t\"3 Overview\"\taff9ed00-016e-4202-a3df-27a3ce443e80\r\n");
          file.Write("3T1SA\t3 Phase 1\t8d527a23-2777-4754-a73d-029f67abe715\r\n");
          file.Write("3T22A\t3 Phase 2\tf9a99add-4cc2-4e41-a29f-a01f5b3b61b2\r\n");
          file.Write("3T25C\t3 Phase 2\tab416221-9f79-484e-a7c9-bc9a375a6147\r\n");
          file.Write("7S721A\t\"7 راز\"\t2b9d291f-ce76-4947-ae7b-fec3531d1766\r\n");
          file.Write("#Hello\t7th Heaven\t1d5b894b-95e6-4026-9ffe-64197e79c3d1\r\n");
        }
        Assert.AreEqual("CRLF", CsvHelper.GuessNewlineAsync(test, CancellationToken.None).WaitToCompleteTask(2));
      }
      finally
      {
        FileSystemUtils.FileDelete(path);
      }
    }
  }
}