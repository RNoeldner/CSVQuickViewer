/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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
using System.Linq;

// ReSharper disable UseAwaitUsing

namespace CsvTools.Tests;

[TestClass]
public class WorkWithStream
{
  [TestMethod]
  public async System.Threading.Tasks.Task AnalyseStreamAsyncFile()
  {
    var stream = FileSystemUtils.OpenRead(UnitTestStatic.GetTestPath("BasicCSV.txt"));

    ICollection<Column> determinedColumns;
    // Not closing the stream

    using var impStream = new ImprovedStream(new SourceAccess(stream, FileTypeEnum.Stream));
    var result = new InspectionResult();
    await impStream.UpdateInspectionResultAsync(result, false, true, true, true, true, true, true, false, true,
      '\0', Array.Empty<char>(), UnitTestStatic.TesterProgress);
    impStream.Seek(0, SeekOrigin.Begin);

    using (var reader = new CsvFileReader(impStream, result.CodePageId, result.SkipRows, 0, result.HasFieldHeader,
new ColumnCollection(), TrimmingOptionEnum.Unquoted, result.FieldDelimiter, result.FieldQualifier,
result.EscapePrefix, 0, false, false, result.CommentLine, 0, true, string.Empty, string.Empty,
string.Empty, true, false, true, false, false, false, false, false, false, true, true, "", false, 0,
StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, true, false))
    {
      await reader.OpenAsync(UnitTestStatic.Token);
      var columns = await reader.FillGuessColumnFormatReaderAsyncReader(FillGuessSettings.Default,
        new ColumnCollection(), false, true, "null", UnitTestStatic.TesterProgress);
      determinedColumns = columns.ToList();
      Assert.AreEqual(6, determinedColumns.Count(), "Recognized columns");
    }

    impStream.Seek(0, SeekOrigin.Begin);

    using (var reader = new CsvFileReader(impStream, result.CodePageId, result.SkipRows, 0, result.HasFieldHeader,
determinedColumns, TrimmingOptionEnum.Unquoted, result.FieldDelimiter, result.FieldQualifier,
result.EscapePrefix, 0, false, false, result.CommentLine, 0, true, string.Empty, string.Empty,
string.Empty, true, false, true, false, false, false, false, false, false, true, true, "", false, 0,
StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, true, false))
    {
      await reader.OpenAsync(UnitTestStatic.Token);
      Assert.AreEqual(6, reader.FieldCount);
    }
  }

  [TestMethod]
  public async System.Threading.Tasks.Task AnalyseStreamAsyncGZip()
  { 
    // Can not use Stream but must use Improved stream for gzip, otherwise Seek(0, SeekOrigin.Begin); will fail
    using var impStream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCSV.txt.gz")));
      
    var result = new InspectionResult();
    await impStream.UpdateInspectionResultAsync(result, false, true, true, true, true, true, true, false,
      false, '\0', Array.Empty<char>(), UnitTestStatic.TesterProgress);

    impStream.Seek(0, SeekOrigin.Begin);
    ICollection<Column> determinedColumns;
    using (var reader = new CsvFileReader(impStream, result.CodePageId, result.SkipRows, 0, result.HasFieldHeader,
new ColumnCollection(), TrimmingOptionEnum.Unquoted, result.FieldDelimiter, result.FieldQualifier,
result.EscapePrefix, 0, false, false, result.CommentLine, 0, true, string.Empty, string.Empty,
string.Empty, true, false, true, false, false, false, false, false, false, true, true, "", false, 0,
StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, true, false))
    {
      await reader.OpenAsync(UnitTestStatic.Token);
      var columns = await reader.FillGuessColumnFormatReaderAsyncReader(FillGuessSettings.Default,
        new ColumnCollection(), false, true, "null", UnitTestStatic.TesterProgress);        
      Assert.AreEqual(6, columns.Count(), "6 Columns: ID,LangCodeID,ExamDate,Score,Proficiency,IsNativeLang");
      determinedColumns = columns.ToList();
    }

    impStream.Seek(0, SeekOrigin.Begin);

    using (var reader = new CsvFileReader(impStream, result.CodePageId, result.SkipRows, 0, result.HasFieldHeader,
determinedColumns, TrimmingOptionEnum.Unquoted, result.FieldDelimiter, result.FieldQualifier,
result.EscapePrefix, 0, false, false, result.CommentLine, 0, true, string.Empty, string.Empty,
string.Empty, true, false, true, false, false, false, false, false, false, true, true, "", false, 0,
StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, true, false))
    {
      await reader.OpenAsync(UnitTestStatic.Token);
      Assert.AreEqual(6, reader.FieldCount);
    }
  }
}