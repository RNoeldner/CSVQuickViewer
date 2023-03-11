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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable UnusedVariable

// ReSharper disable MethodHasAsyncOverload

// ReSharper disable UseAwaitUsing

namespace CsvTools.Tests
{
  [TestClass]
  public class CsvHelperTest
  {

    [TestMethod]
    public async Task GetDetectionResult()
    {
      var stream = File.OpenRead(UnitTestStatic.GetTestPath("MultipleDelimiter.txt"));
      var res1 = new InspectionResult { FileName = "MultipleDelimiter.txt" };
      await stream.UpdateInspectionResultAsync(res1, false, false, false, false, false, false, false,
        false, false,
        new List<char>(), UnitTestStatic.Token);
      stream.Seek(0, SeekOrigin.Begin);
      await stream.UpdateInspectionResultAsync(res1, true, false, false, false, false, false, false,
        false, false,
        new List<char>(), UnitTestStatic.Token);
      stream.Seek(0, SeekOrigin.Begin);
      await stream.UpdateInspectionResultAsync(res1, false, false, true, false, false, false, false,
        false, false,
        new List<char>(), UnitTestStatic.Token);
      stream.Seek(0, SeekOrigin.Begin);
      await stream.UpdateInspectionResultAsync(res1, false, false, false, false, false, true, true,
        false, false,
        new List<char>(), UnitTestStatic.Token);
      stream.Seek(0, SeekOrigin.Begin);
      await stream.UpdateInspectionResultAsync(res1, false, false, false, false, false, false, false,
        true, false,
        new List<char>(), UnitTestStatic.Token);
      stream.Seek(0, SeekOrigin.Begin);
      await stream.UpdateInspectionResultAsync(res1, false, true, true, true, false, true, false,
        false, false,
        new List<char>(new[] { ':' }), UnitTestStatic.Token);
    }

    [TestMethod]
    public void GuessStartRow()
    {
      using var stream = File.OpenRead(UnitTestStatic.GetTestPath("MultipleDelimiter.txt"));
      ImprovedTextReader imp = new ImprovedTextReader(stream);
      imp.InspectStartRow('|', '"', '\\', "##", UnitTestStatic.Token);
      imp.InspectStartRow('|', char.MinValue, char.MinValue, string.Empty, UnitTestStatic.Token);
    }

    [TestMethod]
    public async Task AnalyzeFileAsyncOtherDelimiter()
    {
      var tuple = await UnitTestStatic.GetTestPath("MultipleDelimiter.txt")
        .InspectFileAsync(false, true, true, 
        true, true, true, true, true, true, FillGuessSettings.Default, new InspectionResult(), string.Empty, UnitTestStatic.Token);

      Assert.IsNotNull(tuple);
      Assert.AreEqual('|', tuple.FieldDelimiter);
    }

    [TestMethod]
    public async Task AnalyzeFileAsyncZip()
    {
      var tuple = await UnitTestStatic.GetTestPath("AllFormatsPipe.zip").InspectFileAsync(false, true, true, true,
        true, true, true, true, true, FillGuessSettings.Default, new InspectionResult(), string.Empty,UnitTestStatic.Token);

      Assert.IsNotNull(tuple);
      Assert.AreEqual('|', tuple.FieldDelimiter);
    }

    [TestMethod]
    public async Task NewCsvFileGuessAllSmallFile()
    {
      var det = await UnitTestStatic.GetTestPath("employee.txt")
        .GetInspectionResultFromFileAsync(true, true, true, true, true, true, true, true, true, new InspectionResult(), new FillGuessSettings(false), string.Empty,UnitTestStatic.Token);
      //TODO: check if this is Environment dependent, looks like windows has CRLF and Mac as LF
      Assert.AreEqual(RecordDelimiterTypeEnum.Crlf, det.NewLine);
    }

    [TestMethod]
    public async Task GetCsvFileSettingAsync()
    {
      var tuple = await UnitTestStatic.GetTestPath("BasicCSV.txt").InspectFileAsync(true, true, true, true,
        true, true, true, true, true, FillGuessSettings.Default, new InspectionResult(), string.Empty,UnitTestStatic.Token); Assert.IsNotNull(tuple);
      Assert.AreEqual(1200, tuple.CodePageId);
    }

#if XmlSerialization
    [TestMethod]
    public async Task GetCsvFileSettingFromExtensionAsync()
    {
      var tuple = await UnitTestStatic.GetTestPath("BasicCSV.txt" + CsvFile.cCsvSettingExtension).InspectFileAsync(true,
        true, true,
        true, true, true, true, true, FillGuessSettings.Default, UnitTestStatic.Token);
      Assert.IsNotNull(tuple);
      Assert.AreEqual(1200, tuple.CodePageId);
    }
#endif

    [TestMethod]
    public async Task GuessCodePageAsync()
    {
      using (var improvedStream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCSV.txt"))))
      {
        Assert.AreEqual(1200, (await improvedStream.InspectCodePageAsync(UnitTestStatic.Token)).codePage);
      }

      using (var improvedStream =
             new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("UnicodeUTF16BE.txt"))))
      {
        Assert.AreEqual(1201, (await improvedStream.InspectCodePageAsync(UnitTestStatic.Token)).codePage);
      }

      using (var improvedStream =
             new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("UnicodeUTF8.txt"))))
      { 
        Assert.AreEqual(65001, (await improvedStream.InspectCodePageAsync(UnitTestStatic.Token)).codePage);
      }
    }

    [TestMethod]
    public async Task GuessDelimiterCommaBasicAsync()
    {
      using var improvedStream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCSV.txt")));
      using var textReader = await improvedStream.GetTextReaderAsync(-1, 0, UnitTestStatic.Token);
      Assert.AreEqual(',',
        (await textReader.InspectDelimiterAsync('"', char.MinValue, Array.Empty<char>(), UnitTestStatic.Token)).Delimiter);
    }

    [TestMethod]
    public async Task GuessDelimiterPipeBasicAsync()
    {
      using var improvedStream =
        new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("AllFormatsPipe.txt")));
      using var textReader = await improvedStream.GetTextReaderAsync(-1, 0, UnitTestStatic.Token);
      Assert.AreEqual('|', (await textReader.InspectDelimiterAsync('"', char.MinValue, Array.Empty<char>(), UnitTestStatic.Token)).Delimiter);
    }

    [TestMethod]
    public async Task GuessDelimiterLateStartRowAsync()
    {
      ICsvFile test =
        new CsvFile(id: "csv", fileName: UnitTestStatic.GetTestPath("LateStartRow.txt"))
        {
          SkipRows = 10,
          CodePageId = 20127
        };
      test.FieldQualifierChar = '"';
      using var improvedStream = new ImprovedStream(new SourceAccess(test));
      using var textReader = await improvedStream.GetTextReaderAsync(20127, 10, UnitTestStatic.Token);
      Assert.AreEqual('|', (await textReader.InspectDelimiterAsync('"', char.MinValue, Array.Empty<char>(), UnitTestStatic.Token)).Delimiter);
    }

    [TestMethod]
    public async Task GuessDelimiterMagicWordAsync()
    {
      using (var improvedStream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCSV.txt"))))
      {
        using var textReader = await improvedStream.GetTextReaderAsync(-1, 0, UnitTestStatic.Token);
        Assert.AreEqual(',', (await textReader.InspectDelimiterAsync('"', char.MinValue, Array.Empty<char>(), UnitTestStatic.Token)).Delimiter);
      }

      using (var improvedStream =
             new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCSVMagic.txt"))))
      {
        using var textReader = await improvedStream.GetTextReaderAsync(-1, 0, UnitTestStatic.Token);
        Assert.AreEqual('\t', (await textReader.InspectDelimiterAsync('"', char.MinValue, Array.Empty<char>(), UnitTestStatic.Token)).Delimiter);
      }
    }

    [TestMethod]
    public async Task GuessDelimiterCommaAsync()
    {
      ICsvFile test =
        new CsvFile(id: "Csv", fileName: UnitTestStatic.GetTestPath("AlternateTextQualifiers.txt")) { CodePageId = -1 };
      using var improvedStream = new ImprovedStream(new SourceAccess(test));
      using var textReader = await improvedStream.GetTextReaderAsync(-1, 0, UnitTestStatic.Token);
      Assert.AreEqual(',', (await textReader.InspectDelimiterAsync('"', '\\', Array.Empty<char>(), UnitTestStatic.Token)).Delimiter);
    }

    [TestMethod]
    public async Task GuessDelimiterPipeAsync()
    {
      ICsvFile test =
        new CsvFile(id: "Csv",
          fileName: UnitTestStatic.GetTestPath("DifferentColumnDelimiter.txt"))
        { CodePageId = -1 };
      test.EscapePrefixChar = string.Empty.FromText();
      using var improvedStream = new ImprovedStream(new SourceAccess(test));

      using var textReader = await improvedStream.GetTextReaderAsync(-1, 0, UnitTestStatic.Token);
      Assert.AreEqual('|', (await textReader.InspectDelimiterAsync('"', char.MinValue, Array.Empty<char>(), UnitTestStatic.Token)).Delimiter);
    }

    [TestMethod]
    public async Task GuessDelimiterQualifierAsync()
    {
      ICsvFile test =
        new CsvFile(id: "Csv", fileName: UnitTestStatic.GetTestPath("TextQualifiers.txt")) { CodePageId = -1 };
      test.EscapePrefixChar = string.Empty.FromText();
      using var improvedStream = new ImprovedStream(new SourceAccess(test));
      using var textReader = await improvedStream.GetTextReaderAsync(test.CodePageId, test.SkipRows, UnitTestStatic.Token);
      Assert.AreEqual(',', (await textReader.InspectDelimiterAsync(test.FieldQualifierChar, test.EscapePrefixChar, Array.Empty<char>(), UnitTestStatic.Token)).Delimiter);
    }

    [TestMethod]
    public async Task GuessDelimiterTabAsync()
    {
      ICsvFile test =
        new CsvFile(id: "Csv", fileName: UnitTestStatic.GetTestPath("txTranscripts.txt")) { CodePageId = -1 };
      test.EscapePrefixChar = '\\';
      using var improvedStream = new ImprovedStream(new SourceAccess(test));
      DetectionDelimiter.DelimiterDetection ret;
      using (var textReader = await improvedStream.GetTextReaderAsync(test.CodePageId, test.SkipRows, UnitTestStatic.Token))
      {
        ret = await textReader.InspectDelimiterAsync('"', test.EscapePrefixChar, Array.Empty<char>(), UnitTestStatic.Token).ConfigureAwait(false);
      }

      Assert.AreEqual('\t', (ret).Delimiter);
    }

    [TestMethod]
    public async Task GuessJsonFileAsync()
    {
      using var stream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("Jason1.json")));
      Assert.IsTrue(await stream.InspectIsJsonReadableAsync(Encoding.UTF8, UnitTestStatic.Token));
    }

    [TestMethod]
    public async Task GetCsvFileSetting()
    {
      var result = await UnitTestStatic.GetTestPath("BasicCSV.txt").InspectFileAsync(true, true, true, true, true, true, true,
        false, true,
        FillGuessSettings.Default, new InspectionResult(),string.Empty, UnitTestStatic.Token);
      Assert.IsNotNull(result);
      Assert.IsTrue(result.HasFieldHeader);
      Assert.AreEqual(1200, result.CodePageId);
    }



    [TestMethod]
    public async Task GuessHeaderSkippingEmptyRowsWithDelimiterAsync()
    {
      using var improvedStream =
        new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("SkippingEmptyRowsWithDelimiter.txt")));
      using var textReader = await improvedStream.GetTextReaderAsync(65001, 0, UnitTestStatic.Token);
      var result = await textReader.InspectHasHeaderAsync(',', '"', char.MinValue, "#", UnitTestStatic.Token);
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

        var test = new CsvFile(id: "csv", fileName: path) { CodePageId = 65001, FieldQualifierChar = '"' };
        using (var improvedStream = new ImprovedStream(new SourceAccess(test)))
        {
          using var textReader = await improvedStream.GetTextReaderAsync(test.CodePageId, test.SkipRows, UnitTestStatic.Token).ConfigureAwait(false);
          Assert.AreEqual(RecordDelimiterTypeEnum.Lf, textReader.InspectRecordDelimiter(test.FieldQualifierChar, UnitTestStatic.Token));
        }

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

        using (var improvedStream = new ImprovedStream(new SourceAccess(test)))
        {
          using var textReader = await improvedStream.GetTextReaderAsync(test.CodePageId, test.SkipRows, UnitTestStatic.Token).ConfigureAwait(false);
          Assert.AreEqual(RecordDelimiterTypeEnum.Rs, textReader.InspectRecordDelimiter(test.FieldQualifierChar, UnitTestStatic.Token));
        }

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

        using (var improvedStream = new ImprovedStream(new SourceAccess(test)))
        {
          using (var textReader = await improvedStream.GetTextReaderAsync(test.CodePageId, test.SkipRows, UnitTestStatic.Token).ConfigureAwait(false))
          {
            Assert.AreEqual(RecordDelimiterTypeEnum.Lfcr,
              textReader.InspectRecordDelimiter(test.FieldQualifierChar, UnitTestStatic.Token));
          }
        }

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

        using (var improvedStream = new ImprovedStream(new SourceAccess(test)))
        {
          using (var textReader = await improvedStream.GetTextReaderAsync(test.CodePageId, test.SkipRows, UnitTestStatic.Token).ConfigureAwait(false))
          {
            Assert.AreEqual(RecordDelimiterTypeEnum.Crlf,
              textReader.InspectRecordDelimiter(test.FieldQualifierChar, UnitTestStatic.Token));
          }
        }
      }
      finally
      {
        FileSystemUtils.FileDelete(path);
      }
    }

    [TestMethod]
    public async Task GuessStartRowAsync()
    {
      using var improvedStream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCSV.txt")));
      int ret;
      using (var textReader = await improvedStream.GetTextReaderAsync(65001, 0, UnitTestStatic.Token).ConfigureAwait(false))
      {
        ret = textReader.InspectStartRow('\t', '"', '\\', "", UnitTestStatic.Token);
      }

      Assert.AreEqual(
        0,
        ret, "BasicCSV.txt");
    }

    [TestMethod]
    public async Task GuessStartRowComment()
    {
      using var improvedStream =
        new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("TrimmingHeaders.txt")));
      int ret;  
      using (var textReader = await improvedStream.GetTextReaderAsync(65001, 0, UnitTestStatic.Token).ConfigureAwait(false))
      {
        ret = textReader.InspectStartRow('\t', '"', '\\', "#", UnitTestStatic.Token);
      }

      Assert.AreEqual(
        0,
        ret, "TrimmingHeaders.txt");
    }

    [TestMethod]
    public async Task GuessStartRow0Async()
    {
      using var improvedStream =
        new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("TextQualifiers.txt")));
      int ret;
      using (var textReader = await improvedStream.GetTextReaderAsync(65001, 0, UnitTestStatic.Token).ConfigureAwait(false))
      {
        ret = textReader.InspectStartRow(',', '"', '\\', "", UnitTestStatic.Token);
      }

      Assert.AreEqual(
        0,
        ret, "TextQualifiers.txt");
    }

    [TestMethod]
    public async Task GuessStartRow12Async()
    {
      using var improvedStream =
        new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("SkippingEmptyRowsWithDelimiter.txt")));
      using var textReader = await improvedStream.GetTextReaderAsync(-1, 0, UnitTestStatic.Token).ConfigureAwait(false);
      Assert.AreEqual(
        12, textReader.InspectStartRow(',', '"', '\\', "", UnitTestStatic.Token),
        "SkippingEmptyRowsWithDelimiter.txt");
    }

    [TestMethod]
    public async Task GuessStartRowWithCommentsAsync()
    {
      using var improvedStream =
        new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("LongHeaders.txt")));

      using var textReader = await improvedStream.GetTextReaderAsync(65001, 0, UnitTestStatic.Token).ConfigureAwait(false);
      Assert.AreEqual(
        0, textReader.InspectStartRow(',', '"', char.MinValue, "#", UnitTestStatic.Token), "LongHeaders.txt");
    }

    [TestMethod]
    public async Task HasUsedQualifierFalseAsync()
    {
      using var improvedStream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCSV.txt")));
      Assert.IsFalse(await improvedStream.HasUsedQualifierAsync(65001, 0, '\t', '"', UnitTestStatic.Token));
    }

    [TestMethod]
    public async Task HasUsedQualifierTrueAsync()
    {
      using var improvedStream =
        new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("AlternateTextQualifiers.txt")));
      Assert.IsTrue(await improvedStream.HasUsedQualifierAsync(65001, 0, '\t', '"', UnitTestStatic.Token));
    }

    [TestMethod]
    public async Task NewCsvFileGuessAllHeadingsAsync()
    {
      var det = await UnitTestStatic.GetTestPath("BasicCSV.txt")
        .GetInspectionResultFromFileAsync(false, true, true, true, true, true, true, true, true, new InspectionResult(), new FillGuessSettings(false), string.Empty,UnitTestStatic.Token);
      Assert.AreEqual(0, det.SkipRows);
      Assert.AreEqual(',', det.FieldDelimiter);
      Assert.AreEqual(1200, det.CodePageId); // UTF16_LE
    }

    [TestMethod]
    public async Task NewCsvFileGuessAllTestEmptyAsync()
    {
      var det = await UnitTestStatic.GetTestPath("CSVTestEmpty.txt")
        .GetInspectionResultFromFileAsync(false, true, true, true, true, true, true, true, true, new InspectionResult(), new FillGuessSettings(false),string.Empty, UnitTestStatic.Token);
      Assert.AreEqual(0, det.SkipRows);
    }

    [TestMethod]
    public async Task GuessQualifier1()
    {
      using var improvedStream =
        new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("TextQualifiers.txt")));

      using var textReader = await improvedStream.GetTextReaderAsync(65001, 0, UnitTestStatic.Token).ConfigureAwait(false);
      Assert.AreEqual('"', (textReader.InspectQualifier('\t', '\\', StaticCollections.PossibleQualifiers, UnitTestStatic.Token)).QuoteChar);
    }

    [TestMethod]
    public async Task GuessQualifier2()
    {
      using var improvedStream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("Quoting1.txt")));

      using var textReader = await improvedStream.GetTextReaderAsync(65001, 0, UnitTestStatic.Token).ConfigureAwait(false);
      Assert.AreEqual('"', (textReader.InspectQualifier('\t', '\\', StaticCollections.PossibleQualifiers, UnitTestStatic.Token)).QuoteChar);
    }

    [TestMethod]
    public async Task GuessQualifier3()
    {
      using var improvedStream =
        new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("Quoting1Reverse.txt")));

      using var textReader = await improvedStream.GetTextReaderAsync(65001, 0, UnitTestStatic.Token).ConfigureAwait(false);
      Assert.AreEqual("'", textReader.InspectQualifier('\t', '\\', StaticCollections.PossibleQualifiers, UnitTestStatic.Token).QuoteChar.ToString());
    }

    [TestMethod]
    public async Task RefreshCsvFileAsync()
    {
      var fgs = new FillGuessSettings(true);

      var det = await UnitTestStatic.GetTestPath("BasicCSV.txt")
        .GetInspectionResultFromFileAsync(false, true, true, true, true, true, true, true, true, new InspectionResult(), fgs, string.Empty,UnitTestStatic.Token);
      Assert.AreEqual(1200, det.CodePageId);
      Assert.AreEqual(',', det.FieldDelimiter);

      foreach (var fileName in Directory.EnumerateFiles(UnitTestStatic.ApplicationDirectory.LongPathPrefix(),
                 "AllFor*.txt", SearchOption.TopDirectoryOnly))
      {
        await fileName.GetInspectionResultFromFileAsync(false, true, true, true, true, true, true, true, true, new InspectionResult(), fgs,
          string.Empty, UnitTestStatic.Token);
      }
    }

    [TestMethod]
    public async Task TestGuessStartRow1Async()
    {
      ICsvFile test =
        new CsvFile(id: "Csv", fileName: UnitTestStatic.GetTestPath("LateStartRow.txt"))
        {
          SkipRows = 10,
          CodePageId = 20127
        };
      test.FieldDelimiterChar = '|';
      test.FieldQualifierChar = '"';

      using var reader = new CsvFileReader(test.FullPath, test.CodePageId, test.SkipRows, test.HasFieldHeader,
        test.ColumnCollection, test.TrimmingOption,
        test.FieldDelimiterChar,
        test.FieldQualifierChar, test.EscapePrefixChar, test.RecordLimit, test.AllowRowCombining,
        test.ContextSensitiveQualifier, test.CommentLine, test.NumWarnings,
        test.DuplicateQualifierToEscape,
        test.NewLinePlaceholder, test.DelimiterPlaceholder, test.QualifierPlaceholder, test.SkipDuplicateHeader,
        test.TreatLfAsSpace,
        test.TreatUnknownCharacterAsSpace, test.TryToSolveMoreColumns,
        test.WarnDelimiterInValue, test.WarnLineFeed, test.WarnNBSP, test.WarnQuotes, test.WarnUnknownCharacter,
        test.WarnEmptyTailingColumns,
        test.TreatNBSPAsSpace, test.TreatTextAsNull,
        test.SkipEmptyLines, test.ConsecutiveEmptyRows, test.IdentifierInContainer,
        StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, true, false);
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
      using var improvedStream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCSV.txt")));

      using var textReader = await improvedStream.GetTextReaderAsync(65001, 0, UnitTestStatic.Token).ConfigureAwait(false);
      Assert.AreEqual(0, textReader.InspectStartRow('\t', '"', char.MinValue, string.Empty, UnitTestStatic.Token));
    }

    [TestMethod]
    public async Task TestGuessLineCommentAsync()
    {
      using var improvedStream =
        new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("ComplexDataDelimiter.txt")));
      using var textReader = await improvedStream.GetTextReaderAsync(65001, 0, UnitTestStatic.Token).ConfigureAwait(false);
      Assert.AreEqual("#", await textReader.InspectLineCommentAsync(UnitTestStatic.Token).ConfigureAwait(false));
    }

    [TestMethod]
    public async Task TestValidateLineCommentInvalidAsync()
    {
      using var improvedStream =
        new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("txTranscripts.txt")));
      using var textReader = new ImprovedTextReader(improvedStream);
      Assert.IsFalse(await textReader.InspectLineCommentIsValidAsync("#", '\t', UnitTestStatic.Token));
    }

    [TestMethod]
    public async Task TestValidateLineCommentValidAsync()
    {
      using (var improvedStream =
             new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("ComplexDataDelimiter.txt"))))
      using (var textReader = new ImprovedTextReader(improvedStream))
      {
        Assert.IsTrue(await textReader.InspectLineCommentIsValidAsync("#", ',', UnitTestStatic.Token));
      }

      using (var improvedStream =
             new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("ReadingInHeaderAfterComments.txt"))))
      using (var textReader = new ImprovedTextReader(improvedStream))
      {
        Assert.IsTrue(await textReader.InspectLineCommentIsValidAsync("#", ',', UnitTestStatic.Token));
      }
    }
  }
}