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
    public async Task AnalyzeFileAsyncOtherDelimiter()
    {
      var processDisplay = new CustomProcessDisplay();
      var tuple = await UnitTestStatic.GetTestPath("MultipleDelimiter.txt").AnalyzeFileAsync(false, true, true,
                    true, true, true, true, true, new FillGuessSettings(), UnitTestStatic.Token);

      Assert.IsNotNull(tuple);
      Assert.AreEqual("Pipe", tuple.FieldDelimiter);
    }

    [TestMethod]
    public async Task NewCsvFileGuessAllSmallFile()
    {
      var display = new CustomProcessDisplay();

      var det = await UnitTestStatic.GetTestPath("employee.txt").GetDetectionResultFromFile(true, true, true, true, true, true, true, true, UnitTestStatic.Token);
      //TODO: check if this is Environment dependent, looks like windows has CRLF and Mac as LF
      Assert.AreEqual(RecordDelimiterTypeEnum.Crlf, det.NewLine);
    }

    [TestMethod]
    public async Task GetCsvFileSettingAsync()
    {
      var processDisplay = new CustomProcessDisplay();
      var tuple = await UnitTestStatic.GetTestPath("BasicCSV.txt").AnalyzeFileAsync(true, true, true,
                    true, true, true, true, true, new FillGuessSettings(), UnitTestStatic.Token);
      Assert.IsNotNull(tuple);
      Assert.AreEqual(1200, tuple.CodePageId);
    }

    [TestMethod]
    public async Task GetCsvFileSettingFromExtensionAsync()
    {
      var processDisplay = new CustomProcessDisplay();
      var tuple = await UnitTestStatic.GetTestPath("BasicCSV.txt" + CsvFile.cCsvSettingExtension).AnalyzeFileAsync(true, true, true,
                    true, true, true, true, true, new FillGuessSettings(), UnitTestStatic.Token);
      Assert.IsNotNull(tuple);
      Assert.AreEqual(1200, tuple.CodePageId);
    }

    [TestMethod]
    public async Task GuessCodePageAsync()
    {
      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCSV.txt"))))
      {
        Assert.AreEqual(1200, (await improvedStream.GuessCodePage(UnitTestStatic.Token)).Item1);
      }

      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("UnicodeUTF16BE.txt"))))
      {
        Assert.AreEqual(1201, (await improvedStream.GuessCodePage(UnitTestStatic.Token)).Item1);
      }

      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("UnicodeUTF8.txt"))))
      {
        Assert.AreEqual(65001, (await improvedStream.GuessCodePage(UnitTestStatic.Token)).Item1);
      }
    }

    [TestMethod]
    public async Task GuessDelimiterAsync()
    {
      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCSV.txt"))))
        Assert.AreEqual(",", (await improvedStream.GuessDelimiter(-1, 0, string.Empty, UnitTestStatic.Token)).Item1);

      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("AllFormatsPipe.txt"))))

        Assert.AreEqual("|", (await improvedStream.GuessDelimiter(-1, 0, string.Empty, UnitTestStatic.Token)).Item1);

      ICsvFile test = new CsvFile(UnitTestStatic.GetTestPath("LateStartRow.txt")) { SkipRows = 10, CodePageId = 20127 };
      test.FieldQualifier = "\"";
      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(test)))
        Assert.AreEqual("|", (await improvedStream.GuessDelimiter(20127, 10, string.Empty, UnitTestStatic.Token)).Item1);
    }

    [TestMethod]
    public async Task GuessDelimiterCommaAsync()
    {
      ICsvFile test = new CsvFile(UnitTestStatic.GetTestPath("AlternateTextQualifiers.txt")) { CodePageId = -1 };
      test.EscapePrefix = "\\";
      using var improvedStream = FunctionalDI.OpenStream(new SourceAccess(test));
      Assert.AreEqual(",", (await improvedStream.GuessDelimiter(-1, 0, "\\", UnitTestStatic.Token)).Item1);
    }

    [TestMethod]
    public async Task GuessDelimiterPipeAsync()
    {
      ICsvFile test = new CsvFile(UnitTestStatic.GetTestPath("DifferentColumnDelimiter.txt")) { CodePageId = -1 };
      test.EscapePrefix = string.Empty;
      using var improvedStream = FunctionalDI.OpenStream(new SourceAccess(test));
      Assert.AreEqual("|", (await improvedStream.GuessDelimiter(-1, 0, string.Empty, UnitTestStatic.Token)).Item1);
    }

    [TestMethod]
    public async Task GuessDelimiterQualifierAsync()
    {
      ICsvFile test = new CsvFile(UnitTestStatic.GetTestPath("TextQualifiers.txt")) { CodePageId = -1 };
      test.EscapePrefix = string.Empty;
      using var improvedStream = FunctionalDI.OpenStream(new SourceAccess(test));
      Assert.AreEqual(",", (await improvedStream.GuessDelimiter(test.CodePageId, test.SkipRows, test.EscapePrefix, UnitTestStatic.Token)).Item1);
    }

    [TestMethod]
    public async Task GuessDelimiterTabAsync()
    {
      ICsvFile test = new CsvFile(UnitTestStatic.GetTestPath("txTranscripts.txt")) { CodePageId = -1 };
      test.EscapePrefix = "\\";
      using var improvedStream = FunctionalDI.OpenStream(new SourceAccess(test));
      Assert.AreEqual("TAB", (await improvedStream.GuessDelimiter(test.CodePageId, test.SkipRows, test.EscapePrefix, UnitTestStatic.Token)).Item1, true);
    }

    [TestMethod]
    public async Task GuessHasHeaderAsync()
    {
      using (var stream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCSV.txt"))))
      using (var reader = new ImprovedTextReader(stream))
      {        
        Assert.IsTrue(string.IsNullOrEmpty(await reader.GuessHasHeaderAsync("#", ',', UnitTestStatic.Token)));
      }

      using (var stream =
        new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("HandlingDuplicateColumnNames.txt"))))
      using (var reader = new ImprovedTextReader(stream))
      {
        Assert.IsFalse(string.IsNullOrEmpty(await reader.GuessHasHeaderAsync("#", ',', UnitTestStatic.Token)));
      }

      using (var stream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("DifferentColumnDelimiter.txt"))))
      using (var reader = new ImprovedTextReader(stream))
      {
        Assert.IsFalse(string.IsNullOrEmpty(await reader.GuessHasHeaderAsync("", ',', UnitTestStatic.Token)));
      }

      using (var stream = new FileStream(UnitTestStatic.GetTestPath("Sessions.txt"), FileMode.Open))
      using (var reader = new ImprovedTextReader(stream))
      {        
        Assert.IsTrue(string.IsNullOrEmpty(await reader.GuessHasHeaderAsync("#", '\t', UnitTestStatic.Token)));
      }

      using (var stream = new FileStream(UnitTestStatic.GetTestPath("TrimmingHeaders.txt"), FileMode.Open))
      using (var reader = new ImprovedTextReader(stream))
      {        
        Assert.IsTrue((await reader.GuessHasHeaderAsync("#", ',', UnitTestStatic.Token)).Contains("very short"));
      }
      
    }

    [TestMethod]
    public async Task GuessJsonFileAsync()
    {
      using var stream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("Jason1.json")));
      Assert.IsTrue(await stream.IsJsonReadable(Encoding.UTF8, UnitTestStatic.Token));
    }

    [TestMethod]
    public async Task GetCsvFileSetting()
    {
      var process = new CustomProcessDisplay();
      var result = await UnitTestStatic.GetTestPath("BasicCSV.txt").AnalyzeFileAsync(true, true, true, true, true, true, false, true,
                     new FillGuessSettings(), UnitTestStatic.Token);
      Assert.IsNotNull(result);
      Assert.IsTrue(result.HasFieldHeader);
      Assert.AreEqual(1200, result.CodePageId);
    }

    [TestMethod]
    public async Task GuessHeaderBasicCSV()
    {
      using var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCSV.txt")));
      var result = await improvedStream.GuessHasHeader(1200, 0, "", ",", UnitTestStatic.Token);
      Assert.IsNotNull(result);
      Assert.IsTrue(string.IsNullOrEmpty(result));
    }

    [TestMethod]
    public async Task GuessHeaderStrangeHeaders()
    {
      using var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("StrangeHeaders.txt")));
      var result = await improvedStream.GuessHasHeader(1200, 0, "", ",", UnitTestStatic.Token);
      Assert.IsNotNull(result);
      Assert.IsFalse(string.IsNullOrEmpty(result));
    }


    [TestMethod]
    public async Task GuessHeaderAllFormatsAsync()
    {
      using var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("AllFormats.txt")));
      var result = await improvedStream.GuessHasHeader(65001, 0, "", "\t", UnitTestStatic.Token);
      Assert.IsNotNull(result);
      Assert.IsTrue(string.IsNullOrEmpty(result));
    }

    [TestMethod]
    public async Task GuessHeaderBasicEscapedCharactersAsync()
    {
      using var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicEscapedCharacters.txt")));
      var result = await improvedStream.GuessHasHeader(65001, 0, "", ",", UnitTestStatic.Token);
      Assert.IsNotNull(result);
      Assert.IsFalse(string.IsNullOrEmpty(result));
      Assert.AreEqual("Headers 'a\\', 'b', 'c', 'd', 'e', 'f' very short", result);
    }

    [TestMethod]
    public async Task GuessHeaderLongHeadersAsync()
    {
      using var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("LongHeaders.txt")));
      var result = await improvedStream.GuessHasHeader(65001, 0, "#", ",", UnitTestStatic.Token);
      Assert.IsNotNull(result);
      Assert.IsFalse(string.IsNullOrEmpty(result));
      Assert.IsTrue(result.StartsWith("Headers", System.StringComparison.OrdinalIgnoreCase));
      Assert.IsTrue(result.EndsWith("very short", System.StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task GuessHeaderSkippingEmptyRowsWithDelimiterAsync()
    {
      using var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("SkippingEmptyRowsWithDelimiter.txt")));
      var result = await improvedStream.GuessHasHeader(65001, 0, "#", ",", UnitTestStatic.Token);
      Assert.IsFalse(string.IsNullOrEmpty(result));
    }

    [TestMethod]
    public async Task GuessNewlineTest()
    {
      // Storing Text file with given line ends is tricky, editor and source control might change
      // them therefore creating the text files in code
      var path = UnitTestStatic.GetTestPath("writeTestFile.txt");
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

        var test = new CsvFile(path) { CodePageId = 65001, FieldQualifier = "\"" };
        using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(test)))
          Assert.AreEqual(RecordDelimiterTypeEnum.Lf, await improvedStream.GuessNewline(test.CodePageId, test.SkipRows, test.FieldQualifier, UnitTestStatic.Token));

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
          Assert.AreEqual(RecordDelimiterTypeEnum.Rs, await improvedStream.GuessNewline(test.CodePageId, test.SkipRows, test.FieldQualifier, UnitTestStatic.Token));

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
          Assert.AreEqual(RecordDelimiterTypeEnum.Lfcr,
            await improvedStream.GuessNewline(test.CodePageId, test.SkipRows, test.FieldQualifier, UnitTestStatic.Token));

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
          Assert.AreEqual(RecordDelimiterTypeEnum.Crlf,
            await improvedStream.GuessNewline(test.CodePageId, test.SkipRows, test.FieldQualifier, UnitTestStatic.Token));
      }
      finally
      {
        FileSystemUtils.FileDelete(path);
      }
    }

    [TestMethod]
    public async Task GuessStartRowAsync()
    {
      using var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCSV.txt")));
      Assert.AreEqual(
        0,
        await improvedStream.GuessStartRow(65001, "\t", "\"", "", UnitTestStatic.Token), "BasicCSV.txt");
    }

    [TestMethod]
    public async Task GuessStartRowComment()
    {
      using var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("TrimmingHeaders.txt")));
      Assert.AreEqual(
        0,
        await improvedStream.GuessStartRow(65001, "\t", "\"", "#", UnitTestStatic.Token), "TrimmingHeaders.txt");
    }

    [TestMethod]
    public async Task GuessStartRow0Async()
    {
      using var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("TextQualifiers.txt")));
      Assert.AreEqual(
        0,
        await improvedStream.GuessStartRow(65001, ",", "\"", "", UnitTestStatic.Token), "TextQualifiers.txt");
    }

    [TestMethod]
    public async Task GuessStartRow12Async()
    {
      using var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("SkippingEmptyRowsWithDelimiter.txt")));
      Assert.AreEqual(
        12,
        await improvedStream.GuessStartRow(-1, ",", "\"", "", UnitTestStatic.Token), "SkippingEmptyRowsWithDelimiter.txt");
    }

    [TestMethod]
    public async Task GuessStartRowWithCommentsAsync()
    {
      using var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("LongHeaders.txt")));
      Assert.AreEqual(
        0,
        await improvedStream.GuessStartRow(65001, ",", "\"", "#", UnitTestStatic.Token), "LongHeaders.txt");
    }

    [TestMethod]
    public async Task HasUsedQualifierFalseAsync()
    {
      using var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCSV.txt")));
      Assert.IsFalse(await improvedStream.HasUsedQualifier(65001, 0, "\t", "\"", UnitTestStatic.Token));
    }

    [TestMethod]
    public async Task HasUsedQualifierTrueAsync()
    {
      using var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("AlternateTextQualifiers.txt")));
      Assert.IsTrue(await improvedStream.HasUsedQualifier(65001, 0, "\t", "\"", UnitTestStatic.Token));
    }

    [TestMethod]
    public async Task NewCsvFileGuessAllHeadingsAsync()
    {
      var display = new CustomProcessDisplay();
      var det = await UnitTestStatic.GetTestPath("BasicCSV.txt").GetDetectionResultFromFile(false, true, true, true, true, true, true, true, UnitTestStatic.Token);
      Assert.AreEqual(0, det.SkipRows);
      Assert.AreEqual(",".WrittenPunctuationToChar(), det.FieldDelimiter.WrittenPunctuationToChar());
      Assert.AreEqual(1200, det.CodePageId); // UTF16_LE
    }

    [TestMethod]
    public async Task NewCsvFileGuessAllTestEmptyAsync()
    {
      var display = new CustomProcessDisplay();
      var det = await UnitTestStatic.GetTestPath("CSVTestEmpty.txt").GetDetectionResultFromFile(false, true, true, true, true, true, true, true, UnitTestStatic.Token);
      Assert.AreEqual(0, det.SkipRows);
    }

    [TestMethod]
    public async Task GuessQualifier1()
    {
      using var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("TextQualifiers.txt")));
      Assert.AreEqual("\"", await improvedStream.GuessQualifier(65001, 0, "\t", "\\", UnitTestStatic.Token));
    }

    [TestMethod]
    public async Task GuessQualifier2()
    {
      using var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("Quoting1.txt")));
      Assert.AreEqual("\"", await improvedStream.GuessQualifier(65001, 0, "\t", "\\", UnitTestStatic.Token));
    }

    [TestMethod]
    public async Task GuessQualifier3()
    {
      using var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("Quoting1Reverse.txt")));
      Assert.AreEqual("'", await improvedStream.GuessQualifier(65001, 0, "\t", "\\", UnitTestStatic.Token));
    }

    [TestMethod]
    public async Task RefreshCsvFileAsync()
    {
      var processDisplay = new CustomProcessDisplay();
      var det = await UnitTestStatic.GetTestPath("BasicCSV.txt").GetDetectionResultFromFile(false, true, true, true, true, true, true, true, UnitTestStatic.Token);
      Assert.AreEqual(1200, det.CodePageId);
      Assert.AreEqual(",".WrittenPunctuationToChar(), det.FieldDelimiter.WrittenPunctuationToChar());

      foreach (var fileName in Directory.EnumerateFiles(UnitTestStatic.ApplicationDirectory.LongPathPrefix(),
        "AllFor*.txt", SearchOption.TopDirectoryOnly))
      {
        await fileName.GetDetectionResultFromFile(false, true, true, true, true, true, true, true, UnitTestStatic.Token);
      }
    }

    [TestMethod]
    public async Task TestGuessStartRow1Async()
    {
      ICsvFile test = new CsvFile(UnitTestStatic.GetTestPath("LateStartRow.txt")) { SkipRows = 10, CodePageId = 20127 };
      test.FieldDelimiter = "|";
      test.FieldQualifier = "\"";

      var processDisplay = new CustomProcessDisplay();
      using var reader = new CsvFileReader(test.FullPath, test.CodePageId, test.SkipRows, test.HasFieldHeader, test.ColumnCollection, test.TrimmingOption,
        test.FieldDelimiter,
        test.FieldQualifier, test.EscapePrefix, test.RecordLimit, test.AllowRowCombining, test.ContextSensitiveQualifier, test.CommentLine, test.NumWarnings,
        test.DuplicateQualifierToEscape,
        test.NewLinePlaceholder, test.DelimiterPlaceholder, test.QualifierPlaceholder, test.SkipDuplicateHeader, test.TreatLfAsSpace,
        test.TreatUnknownCharacterAsSpace, test.TryToSolveMoreColumns,
        test.WarnDelimiterInValue, test.WarnLineFeed, test.WarnNBSP, test.WarnQuotes, test.WarnUnknownCharacter, test.WarnEmptyTailingColumns,
        test.TreatNBSPAsSpace, test.TreatTextAsNull,
        test.SkipEmptyLines, test.ConsecutiveEmptyRows, test.IdentifierInContainer, StandardTimeZoneAdjust.ChangeTimeZone, System.TimeZoneInfo.Local.Id, processDisplay);
      await reader.OpenAsync(UnitTestStatic.Token);
      Assert.AreEqual("RecordNumber", reader.GetName(0));
      await reader.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("0F8C40DB-EE2C-4C7C-A226-3C43E72584B0", reader.GetString(1));

      await reader.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("4DCD85E1-64FB-4D33-B33F-4EEB36675666", reader.GetString(1));

      await reader.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("0F8C40DB-EE2C-4C7C-A226-3C43E72584B0", reader.GetString(1));

      await reader.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("4DCD85E1-64FB-4D33-B33F-4EEB36675666", reader.GetString(1));
      // the footer row is ignored with the record number
      var next = await reader.ReadAsync(UnitTestStatic.Token);
      Assert.IsFalse(next);
    }

    [TestMethod]
    public async Task TestGuessStartRow2Async()
    {
      using var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCSV.txt")));
      Assert.AreEqual(0, await improvedStream.GuessStartRow(65001, "\t", "\"", string.Empty, UnitTestStatic.Token));
    }

    [TestMethod]
    public async Task TestGuessLineCommentAsync()
    {
      using var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("ComplexDataDelimiter.txt")));
      Assert.AreEqual("#", await improvedStream.GuessLineComment(65001, 0, UnitTestStatic.Token));
    }

    [TestMethod]
    public async Task TestValidateLineCommentInvalidAsync()
    {
      using var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("txTranscripts.txt")));
      using var textReader = new ImprovedTextReader(improvedStream);
      Assert.IsFalse(await textReader.CheckLineCommentIsValidAsync("#", "Tab", UnitTestStatic.Token));
    }

    [TestMethod]
    public async Task TestValidateLineCommentValidAsync()
    {
      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("ComplexDataDelimiter.txt"))))
      using (var textReader = new ImprovedTextReader(improvedStream))
      {
        Assert.IsTrue(await textReader.CheckLineCommentIsValidAsync("#", ",", UnitTestStatic.Token));
      }

      using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("ReadingInHeaderAfterComments.txt"))))
      using (var textReader = new ImprovedTextReader(improvedStream))
      {
        Assert.IsTrue(await textReader.CheckLineCommentIsValidAsync("#", ",", UnitTestStatic.Token));
      }
    }
  }
}