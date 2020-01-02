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
using System.IO;
using System.Linq;
using System.Threading;

namespace CsvTools.Tests
{
  [TestClass]
  public class CsvHelperTest
  {
    private readonly string m_ApplicationDirectory = FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles";

    [TestMethod]
    public void GuessCodePage()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt")
      };
      CsvHelper.GuessCodePage(setting);
      Assert.AreEqual(1200, setting.CodePageId);

      var setting2 = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "UnicodeUTF16BE.txt")
      };
      CsvHelper.GuessCodePage(setting2);
      Assert.AreEqual(1201, setting2.CodePageId);

      var setting3 = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "Test.csv")
      };
      CsvHelper.GuessCodePage(setting3);
      Assert.AreEqual(65001, setting3.CodePageId);
    }

    [TestMethod]
    public void GuessHasHeader()
    {
      Assert.IsTrue(CsvHelper.GuessHasHeader(new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt")
      }, CancellationToken.None), "BasicCSV.txt");

      Assert.IsFalse(CsvHelper.GuessHasHeader(new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "txTranscripts.txt")
      }, CancellationToken.None), "txTranscripts.txt");
    }

    [TestMethod]
    public void GuessStartRow() => Assert.AreEqual(0, CsvHelper.GuessStartRow(new CsvFile
    {
      FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt")
    }), "BasicCSV.txt");

    [TestMethod]
    public void GuessStartRowWithComments()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "LongHeaders.txt")
      };
      setting.FileFormat.CommentLine = "#";
      Assert.AreEqual(0, CsvHelper.GuessStartRow(setting), "LongHeaders.txt");
    }

    [TestMethod]
    public void GuessDelimiter()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt")
      };
      Assert.AreEqual(",", CsvHelper.GuessDelimiter(setting));

      ICsvFile test = new CsvFile(Path.Combine(m_ApplicationDirectory, "LateStartRow.txt"))
      {
        SkipRows = 10,
        CodePageId = 20127
      };
      test.FileFormat.FieldQualifier = "\"";
      Assert.AreEqual("|", CsvHelper.GuessDelimiter(test));
    }

    [TestMethod]
    public void HasUsedQualifier()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt")
      };
      using (var processDisplay = new DummyProcessDisplay())
      {
        CsvHelper.RefreshCsvFile(setting, processDisplay);
      }
      Assert.AreEqual(1200, setting.CodePageId);
      Assert.AreEqual(",", setting.FileFormat.FieldDelimiter);
    }

    [TestMethod]
    public void GuessDelimiterComma()
    {
      ICsvFile test = new CsvFile(Path.Combine(m_ApplicationDirectory, "AlternateTextQualifiers.txt"))
      {
        CodePageId = -1
      };
      test.FileFormat.EscapeCharacter = "\\";

      Assert.AreEqual(",", CsvHelper.GuessDelimiter(test));
    }

    [TestMethod]
    public void GuessDelimiterPipe()
    {
      ICsvFile test = new CsvFile(Path.Combine(m_ApplicationDirectory, "DifferentColumnDelimiter.txt"))
      {
        CodePageId = -1
      };
      test.FileFormat.EscapeCharacter = "";
      Assert.AreEqual("|", CsvHelper.GuessDelimiter(test));
    }

    [TestMethod]
    public void GuessDelimiterQualifier()
    {
      ICsvFile test = new CsvFile(Path.Combine(m_ApplicationDirectory, "TextQualifiers.txt"))
      {
        CodePageId = -1
      };
      test.FileFormat.EscapeCharacter = "";
      Assert.AreEqual(",", CsvHelper.GuessDelimiter(test));
    }

    [TestMethod]
    public void GuessDelimiterTAB()
    {
      ICsvFile test = new CsvFile(Path.Combine(m_ApplicationDirectory, "txTranscripts.txt"))
      {
        CodePageId = -1
      };
      test.FileFormat.EscapeCharacter = "\\";
      Assert.AreEqual("TAB", CsvHelper.GuessDelimiter(test));
    }

    [TestMethod]
    public void GuessStartRow0()
    {
      ICsvFile test = new CsvFile(Path.Combine(m_ApplicationDirectory, "TextQualifiers.txt"))
      {
        CodePageId = -1
      };
      test.FileFormat.FieldDelimiter = ",";
      test.FileFormat.FieldQualifier = "\"";
      Assert.AreEqual(0, CsvHelper.GuessStartRow(test));
    }

    [TestMethod]
    public void GuessStartRow12()
    {
      ICsvFile test = new CsvFile(Path.Combine(m_ApplicationDirectory, "SkippingEmptyRowsWithDelimiter.txt"))
      {
        CodePageId = -1
      };
      test.FileFormat.FieldDelimiter = ",";
      test.FileFormat.FieldQualifier = "\"";
      Assert.AreEqual(12, CsvHelper.GuessStartRow(test));
    }

    [TestMethod]
    public void HasUsedQualifierFalse()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt"),
        HasFieldHeader = true
      };

      Assert.IsFalse(CsvHelper.HasUsedQualifier(setting, CancellationToken.None));
    }

    [TestMethod]
    public void HasUsedQualifierTrue()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "AlternateTextQualifiers.txt")
      };
      Assert.IsTrue(CsvHelper.HasUsedQualifier(setting, CancellationToken.None));
    }

    [TestMethod]
    public void NewCsvFileGuessAllHeadings()
    {
      var setting = new CsvFile
      {
        FileName = Path.Combine(m_ApplicationDirectory, "BasicCSV.txt")
      };
      using (var display = new DummyProcessDisplay())
      {
        setting.RefreshCsvFile(display);
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
        FileName = Path.Combine(m_ApplicationDirectory, "CSVTestEmpty.txt")
      };
      using (var display = new DummyProcessDisplay())
      {
        setting.RefreshCsvFile(display);
      }

      Assert.AreEqual(0, setting.SkipRows);
    }

    [TestMethod]
    public void TestGuessStartRow()
    {
      ICsvFile test = new CsvFile(Path.Combine(m_ApplicationDirectory, "LateStartRow.txt"))
      {
        CodePageId = 20127
      };
      test.FileFormat.FieldDelimiter = "|";
      test.FileFormat.FieldQualifier = "\"";
      test.SkipRows = CsvHelper.GuessStartRow(test);

      using (var display = new DummyProcessDisplay())
      using (var reader = test.GetFileReader(display))
      {
        reader.Open();
        Assert.AreEqual("RecordNumber", reader.GetName(0));
        reader.Read();
        Assert.AreEqual("0F8C40DB-EE2C-4C7C-A226-3C43E72584B0", reader.GetString(1));
      }
      Assert.AreEqual(10, test.SkipRows);
    }

    [TestMethod]
    public void GetColumnHeadersFromReaderClosed()
    {
      var setting = new CsvFile
      {
        FileFormat = { FieldDelimiter = "," },
        FileName = Path.Combine(m_ApplicationDirectory, "EmptyColumns.txt"),
        HasFieldHeader = false
      };
      using (var disp = new DummyProcessDisplay())
      {
        Assert.IsTrue(CsvHelper.GetEmptyColumnHeader(setting, disp).Count == 0);
        setting.HasFieldHeader = true;
        using (var reader = setting.GetFileReader(disp))
        {

          var res = CsvHelper.GetColumnHeadersFromReader(reader);
          Assert.AreEqual(0, res.Count);
        }


      }
    }
    [TestMethod]
    public void GuessJsonFile()
    {
      var setting = new CsvFile
      {
        JsonFormat = true,
        FileName = Path.Combine(m_ApplicationDirectory, "Jason1.json"),
      };

      Assert.IsTrue(CsvHelper.GuessJsonFile(setting));
    }

    [TestMethod]
    public void GetColumnHeadersFromReader()
    {
      var setting = new CsvFile
      {
        FileFormat = { FieldDelimiter = "," },
        FileName = Path.Combine(m_ApplicationDirectory, "EmptyColumns.txt"),
        HasFieldHeader = false
      };
      using (var disp = new DummyProcessDisplay())
      {
        using (var reader = setting.GetFileReader(disp))
        {
          reader.Open();
          var res = CsvHelper.GetColumnHeadersFromReader(reader);
          Assert.AreEqual(6, res.Count);
        }
      }
    }

    [TestMethod]
    public void GetEmptyColumnHeaderTest()
    {
      var setting = new CsvFile
      {
        FileFormat = { FieldDelimiter = "," },
        FileName = Path.Combine(m_ApplicationDirectory, "EmptyColumns.txt"),
        HasFieldHeader = false
      };
      using (var disp = new DummyProcessDisplay())
      {
        Assert.IsTrue(CsvHelper.GetEmptyColumnHeader(setting, disp).Count == 0);
        setting.HasFieldHeader = true;
        var res = CsvHelper.GetEmptyColumnHeader(setting, disp);

        Assert.IsFalse(res.Count == 0);
        Assert.AreEqual("ID", res.First());
      }
    }

    [TestMethod]
    public void GuessNewlineTest()
    {
      var path = Path.Combine(m_ApplicationDirectory, "TestFile.txt");
      FileSystemUtils.FileDelete(path);
      using (var file = File.CreateText(path))
      {
        file.Write("ID\tTitle\tObject ID\n");
        file.Write("12367890\t\"5 Overview\"\tc41f21c8-d2cc-472b-8cd9-707ddd8d24fe\n");
        file.Write("3ICC\t10/14/2010\t0e413ed0-3086-47b6-90f3-836a24f7cb2e\n");
        file.Write("3SOF\t\"3 Overview\"\taff9ed00-016e-4202-a3df-27a3ce443e80\n");
        file.Write("3T1SA\t3 Phase 1\t8d527a23-2777-4754-a73d-029f67abe715\n");
        file.Write("3T22A\t3 Phase 2\tf9a99add-4cc2-4e41-a29f-a01f5b3b61b2\n");
        file.Write("3T25C\t3 Phase 2\tab416221-9f79-484e-a7c9-bc9a375a6147\n");
        file.Write("7S721A\t\"7 راز\"\t2b9d291f-ce76-4947-ae7b-fec3531d1766\n");
        file.Write("#Hello\t7th Heaven\t1d5b894b-95e6-4026-9ffe-64197e79c3d1\n");
      }
      var Test = new CsvFile(path)
      {
        CodePageId = 65001
      };
      Test.FileFormat.FieldQualifier = "\"";

      Assert.AreEqual("LF", CsvHelper.GuessNewline(Test));
    }
  }
}