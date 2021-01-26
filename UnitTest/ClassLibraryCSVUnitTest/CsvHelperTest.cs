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
using System.Text;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public class CsvHelperTest
  {
    [TestMethod]
    public async Task NewCsvFileGuessAllSmallFile()
    {
      using (var display = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        var det = await CsvHelper.GetDetectionResultFromFile(UnitTestInitializeCsv.GetTestPath("employee.txt"), display, true);
        Assert.AreEqual(RecordDelimiterType.CRLF, det.NewLine);
      }
    }

    [TestMethod]
    public async Task GetCsvFileSettingAsync()
    {
      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        var tuple = await CsvHelper.AnalyseFileAsync(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"), true, true, true,
                    true, true, true, true, new FillGuessSettings(), processDisplay);
        Assert.AreEqual(1200, tuple.Item1.CodePageId);
      }
    }

    [TestMethod]
    public async Task GetCsvFileSettingFromExtensionAsync()
    {
      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        var tuple = await CsvHelper.AnalyseFileAsync(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt" + CsvFile.cCsvSettingExtension), true, true, true,
                     true, true, true, true, new FillGuessSettings(), processDisplay);
        Assert.AreEqual(1200, tuple.Item1.CodePageId);
      }
    }

    [TestMethod]
    public async Task GuessCodePageAsync()
    {
      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"))))
      {
        Assert.AreEqual(1200, (await improvedStream.GuessCodePage(UnitTestInitializeCsv.Token)).Item1);
      }

      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("UnicodeUTF16BE.txt"))))
      {
        Assert.AreEqual(1201, (await improvedStream.GuessCodePage(UnitTestInitializeCsv.Token)).Item1);
      }

      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("UnicodeUTF8.txt"))))
      {
        Assert.AreEqual(65001, (await improvedStream.GuessCodePage(UnitTestInitializeCsv.Token)).Item1);
      }
    }

    [TestMethod]
    public async Task GuessDelimiterAsync()
    {
      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"))))
        Assert.AreEqual(",", (await improvedStream.GuessDelimiter(-1, 0, string.Empty, UnitTestInitializeCsv.Token)).Item1);

      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("AllFormatsPipe.txt"))))

        Assert.AreEqual("|", (await improvedStream.GuessDelimiter(-1, 0, string.Empty, UnitTestInitializeCsv.Token)).Item1);

      ICsvFile test = new CsvFile(UnitTestInitializeCsv.GetTestPath("LateStartRow.txt")) { SkipRows = 10, CodePageId = 20127 };
      test.FileFormat.FieldQualifier = "\"";
      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(test)))
        Assert.AreEqual("|", (await improvedStream.GuessDelimiter(20127, 10, string.Empty, UnitTestInitializeCsv.Token)).Item1);
    }

    [TestMethod]
    public async Task GuessDelimiterCommaAsync()
    {
      ICsvFile test = new CsvFile(UnitTestInitializeCsv.GetTestPath("AlternateTextQualifiers.txt")) { CodePageId = -1 };
      test.FileFormat.EscapeCharacter = "\\";
      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(test)))
        Assert.AreEqual(",", (await improvedStream.GuessDelimiter(-1, 0, "\\", UnitTestInitializeCsv.Token)).Item1);
    }

    [TestMethod]
    public async Task GuessDelimiterPipeAsync()
    {
      ICsvFile test = new CsvFile(UnitTestInitializeCsv.GetTestPath("DifferentColumnDelimiter.txt")) { CodePageId = -1 };
      test.FileFormat.EscapeCharacter = string.Empty;
      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(test)))
        Assert.AreEqual("|", (await improvedStream.GuessDelimiter(-1, 0, string.Empty, UnitTestInitializeCsv.Token)).Item1);
    }

    [TestMethod]
    public async Task GuessDelimiterQualifierAsync()
    {
      ICsvFile test = new CsvFile(UnitTestInitializeCsv.GetTestPath("TextQualifiers.txt")) { CodePageId = -1 };
      test.FileFormat.EscapeCharacter = string.Empty;
      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(test)))
        Assert.AreEqual(",", (await improvedStream.GuessDelimiter(test.CodePageId, test.SkipRows, test.FileFormat.EscapeCharacter, UnitTestInitializeCsv.Token)).Item1);
    }

    [TestMethod]
    public async Task GuessDelimiterTabAsync()
    {
      ICsvFile test = new CsvFile(UnitTestInitializeCsv.GetTestPath("txTranscripts.txt")) { CodePageId = -1 };
      test.FileFormat.EscapeCharacter = "\\";
      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(test)))
        Assert.AreEqual("TAB", (await improvedStream.GuessDelimiter(test.CodePageId, test.SkipRows, test.FileFormat.EscapeCharacter, UnitTestInitializeCsv.Token)).Item1);
    }

    [TestMethod]
    public void GuessHasHeader()
    {
      using (var stream = new ImprovedStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"))))
      using (var reader = new ImprovedTextReader(stream))
      {
        reader.ToBeginning();
        Assert.IsTrue(reader.GuessHasHeader("#", ",", UnitTestInitializeCsv.Token).Item1);
      }

      using (var stream =
        new ImprovedStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("HandlingDuplicateColumnNames.txt"))))
      using (var reader = new ImprovedTextReader(stream))
      {
        reader.ToBeginning();
        Assert.IsFalse(reader.GuessHasHeader("#", ",", UnitTestInitializeCsv.Token).Item1);
      }
    }

    [TestMethod]
    public async Task GuessJsonFileAsync()
    {
      using (var stream = new ImprovedStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("Jason1.json"))))
        Assert.IsTrue(await stream.IsJsonReadable(Encoding.UTF8, UnitTestInitializeCsv.Token));
    }

    [TestMethod]
    public async System.Threading.Tasks.Task GetCsvFileSetting()
    {
      using (IProcessDisplay process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        var result = await CsvHelper.AnalyseFileAsync(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"), true, true, true, true, true, true, false,
                       new FillGuessSettings(), process);
        Assert.IsTrue(result.Item1.HasFieldHeader);
        Assert.AreEqual(1200, result.Item1.CodePageId);
      }
    }

    [TestMethod]
    public async Task GuessHeaderBasicCSV()
    {
      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"))))
      {
        var result = await improvedStream.GuessHasHeader(1200, 0, "", ",", UnitTestInitializeCsv.Token);
        Assert.IsTrue(result.Item1);
        Assert.AreEqual("Header seems present", result.Item2);
      }
    }

    [TestMethod]
    public async Task GuessHeaderAllFormatsAsync()
    {
      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("AllFormats.txt"))))
      {
        var result = await improvedStream.GuessHasHeader(65001, 0, "", "\t", UnitTestInitializeCsv.Token);
        Assert.IsTrue(result.Item1);
        Assert.AreEqual("Header seems present", result.Item2);
      }
    }

    [TestMethod]
    public async Task GuessHeaderBasicEscapedCharactersAsync()
    {
      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("BasicEscapedCharacters.txt"))))
      {
        var result = await improvedStream.GuessHasHeader(65001, 0, "", ",", UnitTestInitializeCsv.Token);
        Assert.IsFalse(result.Item1);
        Assert.AreEqual("Headers 'a\\', 'b', 'c', 'd', 'e', 'f' very short", result.Item2);
      }
    }

    [TestMethod]
    public async Task GuessHeaderLongHeadersAsync()
    {
      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("LongHeaders.txt"))))
      {
        var result = await improvedStream.GuessHasHeader(65001, 0, "#", ",", UnitTestInitializeCsv.Token);
        Assert.IsFalse(result.Item1);
        Assert.IsTrue(result.Item2.StartsWith("Headers", System.StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(result.Item2.EndsWith("very short", System.StringComparison.OrdinalIgnoreCase));
      }
    }

    [TestMethod]
    public async Task GuessHeaderSkippingEmptyRowsWithDelimiterAsync()
    {
      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("SkippingEmptyRowsWithDelimiter.txt"))))
      {
        var result = await improvedStream.GuessHasHeader(65001, 0, "#", ",", UnitTestInitializeCsv.Token);
        Assert.IsFalse(result.Item1);
      }
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
          await file.WriteAsync("3T22A\t3 Phase 2\tf9a99add-4cc2-4e41-a29f-a01f5b3b61b2\n");
          file.Write("3T25C\t3 Phase 2\tab416221-9f79-484e-a7c9-bc9a375a6147\n");
          file.Write("7S721A\t\"7 راز\"\t2b9d291f-ce76-4947-ae7b-fec3531d1766\n");
          file.Write("#Hello\t7th Heaven\t1d5b894b-95e6-4026-9ffe-64197e79c3d1\n");
        }

        var test = new CsvFile(path) { CodePageId = 65001, FileFormat = { FieldQualifier = "\"" } };
        using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(test)))
          Assert.AreEqual(RecordDelimiterType.LF, await improvedStream.GuessNewline(test.CodePageId, test.SkipRows, test.FileFormat.FieldQualifier, UnitTestInitializeCsv.Token));

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
        using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(test)))
          Assert.AreEqual(RecordDelimiterType.RS, await improvedStream.GuessNewline(test.CodePageId, test.SkipRows, test.FileFormat.FieldQualifier, UnitTestInitializeCsv.Token));

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
        using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(test)))
          Assert.AreEqual(RecordDelimiterType.LFCR, await improvedStream.GuessNewline(test.CodePageId, test.SkipRows, test.FileFormat.FieldQualifier, UnitTestInitializeCsv.Token));

        FileSystemUtils.FileDelete(path);
        using (var file = File.CreateText(path))
        {
          await file.WriteAsync("ID\tTitle\tObject ID\r\n");
          await file.WriteAsync("12367890\t\"5\nOverview\"\tc41f21c8-d2cc-472b-8cd9-707ddd8d24fe\r\n");
          file.Write("3ICC\t10/14/2010\t0e413ed0-3086-47b6-90f3-836a24f7cb2e\r\n");
          file.Write("3SOF\t\"3 Overview\"\taff9ed00-016e-4202-a3df-27a3ce443e80\r\n");
          file.Write("3T1SA\t3 Phase 1\t8d527a23-2777-4754-a73d-029f67abe715\r\n");
          await file.WriteAsync("3T22A\t3 Phase 2\tf9a99add-4cc2-4e41-a29f-a01f5b3b61b2\r\n");
          file.Write("3T25C\t3 Phase 2\tab416221-9f79-484e-a7c9-bc9a375a6147\r\n");
          file.Write("7S721A\t\"7 راز\"\t2b9d291f-ce76-4947-ae7b-fec3531d1766\r\n");
          await file.WriteAsync("#Hello\t7th Heaven\t1d5b894b-95e6-4026-9ffe-64197e79c3d1\r\n");
        }
        using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(test)))
          Assert.AreEqual(RecordDelimiterType.CRLF, await improvedStream.GuessNewline(test.CodePageId, test.SkipRows, test.FileFormat.FieldQualifier, UnitTestInitializeCsv.Token));
      }
      finally
      {
        FileSystemUtils.FileDelete(path);
      }
    }

    [TestMethod]
    public async Task GuessStartRowAsync()
    {
      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"))))
        Assert.AreEqual(
        0,
        await improvedStream.GuessStartRow(65001, "\t", "\"", "", UnitTestInitializeCsv.Token), "BasicCSV.txt");
    }

    [TestMethod]
    public async Task GuessStartRowComment()
    {
      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("TrimmingHeaders.txt"))))
        Assert.AreEqual(
        0,
        await improvedStream.GuessStartRow(65001, "\t", "\"", "#", UnitTestInitializeCsv.Token), "TrimmingHeaders.txt");
    }

    [TestMethod]
    public async Task GuessStartRow0Async()
    {
      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("TextQualifiers.txt"))))
        Assert.AreEqual(
        0,
        await improvedStream.GuessStartRow(65001, ",", "\"", "", UnitTestInitializeCsv.Token), "TextQualifiers.txt");
    }

    [TestMethod]
    public async Task GuessStartRow12Async()
    {
      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("SkippingEmptyRowsWithDelimiter.txt"))))
        Assert.AreEqual(
        12,
        await improvedStream.GuessStartRow(-1, ",", "\"", "", UnitTestInitializeCsv.Token), "SkippingEmptyRowsWithDelimiter.txt");
    }

    [TestMethod]
    public async Task GuessStartRowWithCommentsAsync()
    {
      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("LongHeaders.txt"))))
        Assert.AreEqual(
        0,
        await improvedStream.GuessStartRow(65001, ",", "\"", "#", UnitTestInitializeCsv.Token), "LongHeaders.txt");
    }

    [TestMethod]
    public async Task HasUsedQualifierFalseAsync()
    {
      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"))))
        Assert.IsFalse(await improvedStream.HasUsedQualifier(65001, 0, "\t", "\"", UnitTestInitializeCsv.Token));
    }

    [TestMethod]
    public async Task HasUsedQualifierTrueAsync()
    {
      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("AlternateTextQualifiers.txt"))))
        Assert.IsTrue(await improvedStream.HasUsedQualifier(65001, 0, "\t", "\"", UnitTestInitializeCsv.Token));
    }

    [TestMethod]
    public async Task NewCsvFileGuessAllHeadingsAsync()
    {
      using (var display = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        var det = await CsvHelper.GetDetectionResultFromFile(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"), display);
        Assert.AreEqual(0, det.SkipRows);
        Assert.AreEqual(",", det.FieldDelimiter);
        Assert.AreEqual(1200, det.CodePageId); // UTF16_LE
      }
    }

    [TestMethod]
    public async Task NewCsvFileGuessAllTestEmptyAsync()
    {
      using (var display = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        var det = await CsvHelper.GetDetectionResultFromFile(UnitTestInitializeCsv.GetTestPath("CSVTestEmpty.txt"), display);
        Assert.AreEqual(0, det.SkipRows);
      }
    }

    [TestMethod]
    public async Task GuessQualifierAsync()
    {
      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("TextQualifiers.txt"))))
        Assert.AreEqual("\"", await improvedStream.GuessQualifier(65001, 0, "\t", UnitTestInitializeCsv.Token));
    }

    [TestMethod]
    public async Task RefreshCsvFileAsync()
    {
      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        var det = await CsvHelper.GetDetectionResultFromFile(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"), processDisplay);
        Assert.AreEqual(1200, det.CodePageId);
        Assert.AreEqual(",", det.FieldDelimiter);
      }

      foreach (var fileName in Directory.EnumerateFiles(UnitTestInitializeCsv.ApplicationDirectory.LongPathPrefix(),
        "AllFor*.txt", SearchOption.TopDirectoryOnly))
      {
        using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
        {
          var det = await CsvHelper.GetDetectionResultFromFile(fileName, processDisplay);
        }
      }
    }

    [TestMethod]
    public async Task TestGuessStartRow1Async()
    {
      ICsvFile test = new CsvFile(UnitTestInitializeCsv.GetTestPath("LateStartRow.txt")) { SkipRows = 10, CodePageId = 20127 };
      test.FileFormat.FieldDelimiter = "|";
      test.FileFormat.FieldQualifier = "\"";

      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var reader = new CsvFileReader(test, processDisplay))
      {
        await reader.OpenAsync(processDisplay.CancellationToken);
        Assert.AreEqual("RecordNumber", reader.GetName(0));
        await reader.ReadAsync(processDisplay.CancellationToken);
        Assert.AreEqual("0F8C40DB-EE2C-4C7C-A226-3C43E72584B0", reader.GetString(1));

        await reader.ReadAsync(processDisplay.CancellationToken);
        Assert.AreEqual("4DCD85E1-64FB-4D33-B33F-4EEB36675666", reader.GetString(1));

        await reader.ReadAsync(processDisplay.CancellationToken);
        Assert.AreEqual("0F8C40DB-EE2C-4C7C-A226-3C43E72584B0", reader.GetString(1));

        await reader.ReadAsync(processDisplay.CancellationToken);
        Assert.AreEqual("4DCD85E1-64FB-4D33-B33F-4EEB36675666", reader.GetString(1));
        // the footer row is ignored with the record number
        var next = await reader.ReadAsync(processDisplay.CancellationToken);
        Assert.IsFalse(next);
      }
    }

    [TestMethod]
    public async Task TestGuessStartRow2Async()
    {
      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"))))
        Assert.AreEqual(0, await improvedStream.GuessStartRow(65001, "\t", "\"", string.Empty, UnitTestInitializeCsv.Token));
    }
  }
}