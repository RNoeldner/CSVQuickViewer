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
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public class CsvHelperTest
  {
    [TestMethod]
    public async Task NewCsvFileGuessAllSmallFile()
    {
      var setting = new CsvFile {FileName = UnitTestInitializeCsv.GetTestPath("employee.txt")};
      using (var display = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        await setting.RefreshCsvFileAsync(display, true);
      }

      Assert.AreEqual(RecordDelimiterType.CRLF, setting.FileFormat.NewLine);
    }

    [TestMethod]
    public async Task GuessCodePageAsync()
    {
      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        var setting = new CsvFile {FileName = UnitTestInitializeCsv.GetTestPath("BasicCSV.txt")};
        await CsvHelper.GuessCodePageAsync(setting, processDisplay.CancellationToken);
        Assert.AreEqual(1200, setting.CodePageId);

        var setting2 = new CsvFile {FileName = UnitTestInitializeCsv.GetTestPath("UnicodeUTF16BE.txt")};
        await CsvHelper.GuessCodePageAsync(setting2, processDisplay.CancellationToken);
        Assert.AreEqual(1201, setting2.CodePageId);

        var setting3 = new CsvFile {FileName = UnitTestInitializeCsv.GetTestPath("Test.csv")};
        await CsvHelper.GuessCodePageAsync(setting3, processDisplay.CancellationToken);
        Assert.AreEqual(65001, setting3.CodePageId);
      }
    }

    [TestMethod]
    public async Task GuessDelimiterAsync()
    {
      Assert.AreEqual(
        ",",
        await CsvHelper.GuessDelimiterAsync(
          new CsvFile(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt")),
          UnitTestInitializeCsv.Token));
      Assert.AreEqual(
        "|",
        await CsvHelper.GuessDelimiterAsync(
          new CsvFile(UnitTestInitializeCsv.GetTestPath("AllFormatsPipe.txt")),
          UnitTestInitializeCsv.Token));

      ICsvFile test = new CsvFile(UnitTestInitializeCsv.GetTestPath("LateStartRow.txt"))
      {
        SkipRows = 10, CodePageId = 20127
      };
      test.FileFormat.FieldQualifier = "\"";
      Assert.AreEqual("|", await CsvHelper.GuessDelimiterAsync(test, UnitTestInitializeCsv.Token));
    }

    [TestMethod]
    public async Task GuessDelimiterCommaAsync()
    {
      ICsvFile test = new CsvFile(UnitTestInitializeCsv.GetTestPath("AlternateTextQualifiers.txt")) {CodePageId = -1};
      test.FileFormat.EscapeCharacter = "\\";

      Assert.AreEqual(",", await CsvHelper.GuessDelimiterAsync(test, UnitTestInitializeCsv.Token));
    }

    [TestMethod]
    public async Task GuessDelimiterPipeAsync()
    {
      ICsvFile test = new CsvFile(UnitTestInitializeCsv.GetTestPath("DifferentColumnDelimiter.txt")) {CodePageId = -1};
      test.FileFormat.EscapeCharacter = string.Empty;
      Assert.AreEqual("|", await CsvHelper.GuessDelimiterAsync(test, UnitTestInitializeCsv.Token));
    }

    [TestMethod]
    public async Task GuessDelimiterQualifierAsync()
    {
      ICsvFile test = new CsvFile(UnitTestInitializeCsv.GetTestPath("TextQualifiers.txt")) {CodePageId = -1};
      test.FileFormat.EscapeCharacter = string.Empty;
      Assert.AreEqual(",", await CsvHelper.GuessDelimiterAsync(test, UnitTestInitializeCsv.Token));
    }

    [TestMethod]
    public async Task GuessDelimiterTabAsync()
    {
      ICsvFile test = new CsvFile(UnitTestInitializeCsv.GetTestPath("txTranscripts.txt")) {CodePageId = -1};
      test.FileFormat.EscapeCharacter = "\\";
      Assert.AreEqual("TAB", await CsvHelper.GuessDelimiterAsync(test, UnitTestInitializeCsv.Token));
    }

    [TestMethod]
    public async Task GuessHasHeaderAsync()
    {
      using (var stream = new ImprovedStream(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"), true))
      using (var reader = new ImprovedTextReader(stream))
      {
        await reader.ToBeginningAsync();
        Assert.IsTrue(await CsvHelper.GuessHasHeaderAsync(reader, "#", ',', UnitTestInitializeCsv.Token));
      }

      using (var stream =
        new ImprovedStream(UnitTestInitializeCsv.GetTestPath("HandlingDuplicateColumnNames.txt"), true))
      using (var reader = new ImprovedTextReader(stream))
      {
        await reader.ToBeginningAsync();
        Assert.IsFalse(await CsvHelper.GuessHasHeaderAsync(reader, "#", ',', UnitTestInitializeCsv.Token));
      }
    }

    [TestMethod]
    public async Task GuessJsonFileAsync()
    {
      var setting = new CsvFile {JsonFormat = true, FileName = UnitTestInitializeCsv.GetTestPath("Jason1.json")};

      Assert.IsTrue(await CsvHelper.GuessJsonFileAsync(setting, UnitTestInitializeCsv.Token));
    }

    [TestMethod]
    public async Task GuessNewlineTest()
    {
      // Storing Text file with given line ends is tricky, editor and source control might change
      // them therefore creating the text files in code
      var path = UnitTestInitializeCsv.GetTestPath("writeTestFile.txt");
      try
      {
        FileSystemUtils.FileDelete(path);
        using (var file = File.CreateText(path))
        {
          await file.WriteAsync("ID\tTitle\tObject ID\n");
          await file.WriteAsync("12367890\t\"5\rOverview\"\tc41f21c8-d2cc-472b-8cd9-707ddd8d24fe\n");
          file.Write("3ICC\t10/14/2010\t0e413ed0-3086-47b6-90f3-836a24f7cb2e\n");
          file.Write("3SOF\t\"3 Overview\"\taff9ed00-016e-4202-a3df-27a3ce443e80\n");
          file.Write("3T1SA\t3 Phase 1\t8d527a23-2777-4754-a73d-029f67abe715\n");
          file.Write("3T22A\t3 Phase 2\tf9a99add-4cc2-4e41-a29f-a01f5b3b61b2\n");
          file.Write("3T25C\t3 Phase 2\tab416221-9f79-484e-a7c9-bc9a375a6147\n");
          file.Write("7S721A\t\"7 راز\"\t2b9d291f-ce76-4947-ae7b-fec3531d1766\n");
          file.Write("#Hello\t7th Heaven\t1d5b894b-95e6-4026-9ffe-64197e79c3d1\n");
        }

        var test = new CsvFile(path) {CodePageId = 65001, FileFormat = {FieldQualifier = "\""}};

        Assert.AreEqual(RecordDelimiterType.LF, await CsvHelper.GuessNewlineAsync(test, UnitTestInitializeCsv.Token));

        FileSystemUtils.FileDelete(path);
        using (var file = File.CreateText(path))
        {
          await file.WriteAsync("ID\tTitle\tObject ID\u001E");
          await file.WriteAsync("12367890\t\"5\rOverview\"\tc41f21c8-d2cc-472b-8cd9-707ddd8d24fe\u001E");
          file.Write("3ICC\t10/14/2010\t0e413ed0-3086-47b6-90f3-836a24f7cb2e\u001E");
          file.Write("3SOF\t\"3 Overview\"\taff9ed00-016e-4202-a3df-27a3ce443e80\u001E");
          file.Write("3T1SA\t3 Phase 1\t8d527a23-2777-4754-a73d-029f67abe715\u001E");
          file.Write("3T22A\t3 Phase 2\tf9a99add-4cc2-4e41-a29f-a01f5b3b61b2\u001E");
          file.Write("3T25C\t3 Phase 2\tab416221-9f79-484e-a7c9-bc9a375a6147\u001E");
          file.Write("7S721A\t\"7 راز\"\t2b9d291f-ce76-4947-ae7b-fec3531d1766\u001E");
          file.Write("#Hello\t7th Heaven\t1d5b894b-95e6-4026-9ffe-64197e79c3d1\u001E");
        }

        Assert.AreEqual(RecordDelimiterType.RS, await CsvHelper.GuessNewlineAsync(test, UnitTestInitializeCsv.Token));

        FileSystemUtils.FileDelete(path);
        using (var file = File.CreateText(path))
        {
          await file.WriteAsync("ID\tTitle\tObject ID\n\r");
          await file.WriteAsync("12367890\t\"5\nOverview\"\tc41f21c8-d2cc-472b-8cd9-707ddd8d24fe\n\r");
          file.Write("3ICC\t10/14/2010\t0e413ed0-3086-47b6-90f3-836a24f7cb2e\n\r");
          file.Write("3SOF\t\"3 Overview\"\taff9ed00-016e-4202-a3df-27a3ce443e80\n\r");
          file.Write("3T1SA\t3 Phase 1\t8d527a23-2777-4754-a73d-029f67abe715\n\r");
          file.Write("3T22A\t3 Phase 2\tf9a99add-4cc2-4e41-a29f-a01f5b3b61b2\n\r");
          file.Write("3T25C\t3 Phase 2\tab416221-9f79-484e-a7c9-bc9a375a6147\n\r");
          file.Write("7S721A\t\"7 راز\"\t2b9d291f-ce76-4947-ae7b-fec3531d1766\n\r");
          file.Write("#Hello\t7th Heaven\t1d5b894b-95e6-4026-9ffe-64197e79c3d1\n\r");
        }

        Assert.AreEqual(RecordDelimiterType.LFCR, await CsvHelper.GuessNewlineAsync(test, UnitTestInitializeCsv.Token));

        FileSystemUtils.FileDelete(path);
        using (var file = File.CreateText(path))
        {
          await file.WriteAsync("ID\tTitle\tObject ID\r\n");
          await file.WriteAsync("12367890\t\"5\nOverview\"\tc41f21c8-d2cc-472b-8cd9-707ddd8d24fe\r\n");
          file.Write("3ICC\t10/14/2010\t0e413ed0-3086-47b6-90f3-836a24f7cb2e\r\n");
          file.Write("3SOF\t\"3 Overview\"\taff9ed00-016e-4202-a3df-27a3ce443e80\r\n");
          file.Write("3T1SA\t3 Phase 1\t8d527a23-2777-4754-a73d-029f67abe715\r\n");
          file.Write("3T22A\t3 Phase 2\tf9a99add-4cc2-4e41-a29f-a01f5b3b61b2\r\n");
          file.Write("3T25C\t3 Phase 2\tab416221-9f79-484e-a7c9-bc9a375a6147\r\n");
          file.Write("7S721A\t\"7 راز\"\t2b9d291f-ce76-4947-ae7b-fec3531d1766\r\n");
          file.Write("#Hello\t7th Heaven\t1d5b894b-95e6-4026-9ffe-64197e79c3d1\r\n");
        }

        Assert.AreEqual(RecordDelimiterType.CRLF, await CsvHelper.GuessNewlineAsync(test, UnitTestInitializeCsv.Token));
      }
      finally
      {
        FileSystemUtils.FileDelete(path);
      }
    }

    [TestMethod]
    public async Task GuessStartRowAsync() =>
      Assert.AreEqual(
        0,
        await CsvHelper.GuessStartRowAsync(
          new CsvFile {FileName = UnitTestInitializeCsv.GetTestPath("BasicCSV.txt")},
          UnitTestInitializeCsv.Token),
        "BasicCSV.txt");

    [TestMethod]
    public async Task GuessStartRow0Async()
    {
      ICsvFile test = new CsvFile(UnitTestInitializeCsv.GetTestPath("TextQualifiers.txt")) {CodePageId = -1};
      test.FileFormat.FieldDelimiter = ",";
      test.FileFormat.FieldQualifier = "\"";
      Assert.AreEqual(0, await CsvHelper.GuessStartRowAsync(test, UnitTestInitializeCsv.Token));
    }

    [TestMethod]
    public async Task GuessStartRow12Async()
    {
      ICsvFile test =
        new CsvFile(UnitTestInitializeCsv.GetTestPath("SkippingEmptyRowsWithDelimiter.txt")) {CodePageId = -1};
      test.FileFormat.FieldDelimiter = ",";
      test.FileFormat.FieldQualifier = "\"";
      Assert.AreEqual(12, await CsvHelper.GuessStartRowAsync(test, UnitTestInitializeCsv.Token));
    }

    [TestMethod]
    public async Task GuessStartRowWithCommentsAsync()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitializeCsv.GetTestPath("LongHeaders.txt"), FileFormat = {CommentLine = "#"}
      };
      Assert.AreEqual(
        0,
        await CsvHelper.GuessStartRowAsync(setting, UnitTestInitializeCsv.Token),
        "LongHeaders.txt");
    }

    [TestMethod]
    public async Task HasUsedQualifierFalseAsync()
    {
      var setting = new CsvFile {FileName = UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"), HasFieldHeader = true};

      Assert.IsFalse(await CsvHelper.HasUsedQualifierAsync(setting, UnitTestInitializeCsv.Token));
    }

    [TestMethod]
    public async Task HasUsedQualifierTrueAsync()
    {
      var setting = new CsvFile {FileName = UnitTestInitializeCsv.GetTestPath("AlternateTextQualifiers.txt")};
      Assert.IsTrue(await CsvHelper.HasUsedQualifierAsync(setting, UnitTestInitializeCsv.Token));
    }

    [TestMethod]
    public async Task NewCsvFileGuessAllHeadingsAsync()
    {
      var setting = new CsvFile {FileName = UnitTestInitializeCsv.GetTestPath("BasicCSV.txt")};
      using (var display = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        await setting.RefreshCsvFileAsync(display);
      }

      Assert.AreEqual(0, setting.SkipRows);
      Assert.AreEqual(",", setting.FileFormat.FieldDelimiter);
      Assert.AreEqual(1200, setting.CodePageId); // UTF16_LE
    }

    [TestMethod]
    public async Task NewCsvFileGuessAllTestEmptyAsync()
    {
      var setting = new CsvFile {FileName = UnitTestInitializeCsv.GetTestPath("CSVTestEmpty.txt")};
      using (var display = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        await setting.RefreshCsvFileAsync(display);
      }

      Assert.AreEqual(0, setting.SkipRows);
    }

    [TestMethod]
    public async Task GuessQualifierAsync()
    {
      Assert.AreEqual("\"",
        await CsvHelper.GuessQualifierAsync(
          new CsvFile {FileName = UnitTestInitializeCsv.GetTestPath("TextQualifiers.txt")},
          UnitTestInitializeCsv.Token));
    }


    [TestMethod]
    public async Task RefreshCsvFileAsync()
    {
      var setting = new CsvFile {FileName = UnitTestInitializeCsv.GetTestPath("BasicCSV.txt")};
      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        await setting.RefreshCsvFileAsync(processDisplay);
      }

      Assert.AreEqual(1200, setting.CodePageId);
      Assert.AreEqual(",", setting.FileFormat.FieldDelimiter);

      foreach (var fileName in Directory.EnumerateFiles(UnitTestInitializeCsv.ApplicationDirectory.LongPathPrefix(),
        "AllFor*.txt", SearchOption.TopDirectoryOnly))
      {
        var testSetting = new CsvFile(fileName);
        using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
        {
          await testSetting.RefreshCsvFileAsync(processDisplay);
        }
      }
    }

    [TestMethod]
    public async Task TestGuessStartRow1Async()
    {
      ICsvFile test = new CsvFile(UnitTestInitializeCsv.GetTestPath("LateStartRow.txt")) {CodePageId = 20127};
      test.FileFormat.FieldDelimiter = "|";
      test.FileFormat.FieldQualifier = "\"";
      test.SkipRows = await CsvHelper.GuessStartRowAsync(test, UnitTestInitializeCsv.Token);
      Assert.AreEqual(10, test.SkipRows);
      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var reader = new CsvFileReader(test, processDisplay))
      {
        await reader.OpenAsync(processDisplay.CancellationToken);
        Assert.AreEqual("RecordNumber", reader.GetName(0));
        await reader.ReadAsync(processDisplay.CancellationToken);
        Assert.AreEqual("0F8C40DB-EE2C-4C7C-A226-3C43E72584B0", reader.GetString(1));
      }
    }

    [TestMethod]
    public async Task TestGuessStartRow2Async()
    {
      ICsvFile test = new CsvFile(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"));
      test.SkipRows = await CsvHelper.GuessStartRowAsync(test, UnitTestInitializeCsv.Token);
      Assert.AreEqual(0, test.SkipRows);
    }
  }
}