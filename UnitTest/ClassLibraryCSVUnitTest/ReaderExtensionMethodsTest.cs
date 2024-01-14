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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  [SuppressMessage("ReSharper", "UseAwaitUsing")]
  public class ReaderExtensionMethodsTest
  {
    private readonly ICsvFile m_ValidSetting = new CsvFileDummy(UnitTestStatic.GetTestPath("BasicCSV.txt"))
    {
      FieldDelimiterChar = ',',
      CommentLine = "#"
    };

    [TestInitialize]
    public void Init()
    {
      m_ValidSetting.ColumnCollection.Add(new Column("Score", new ValueFormat(DataTypeEnum.Integer)));
      m_ValidSetting.ColumnCollection.Add(new Column("Proficiency",
        new ValueFormat(DataTypeEnum.Numeric)));
      m_ValidSetting.ColumnCollection.Add(new Column("IsNativeLang",
        new ValueFormat(DataTypeEnum.Boolean)));
      var cf = new Column("ExamDate", new ValueFormat(DataTypeEnum.DateTime, @"dd/MM/yyyy"));
      m_ValidSetting.ColumnCollection.Add(cf);
    }

   
    [TestMethod]
    public async Task GetColumnsOfReaderTest()
    {
      using var test = new CsvFileReader(UnitTestStatic.GetTestPath("BasicCSV.txt"),
        650001,
        0,
        true,
        null,
        TrimmingOptionEnum.Unquoted,
        ',',
        '"',
        char.MinValue,
        0,
        false,
        false,
        "#",
        0,
        true,
        "",
        "",
        "",
        true,
        false,
        false,
        true,
        true,
        false,
        true,
        true,
        true,
        true,
        false,
        "NULL",
        true,
        4,
        "",
        StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, true, false);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.AreEqual(6, test.GetColumnsOfReader().Count());
    }

   
    [TestMethod]
    public async Task GetDataTableAsync2()
    {
      var test2 = new CsvFileDummy(UnitTestStatic.GetTestPath("BasicCSV.txt"));
      test2.RecordLimit = 4;
      using var test = new CsvFileReader(test2.FullPath, test2.CodePageId, test2.SkipRows, test2.HasFieldHeader,
        test2.ColumnCollection, test2.TrimmingOption,
        test2.FieldDelimiterChar, test2.FieldQualifierChar, test2.EscapePrefixChar, test2.RecordLimit, test2.AllowRowCombining,
        test2.ContextSensitiveQualifier,
        test2.CommentLine, test2.NumWarnings, test2.DuplicateQualifierToEscape, test2.NewLinePlaceholder,
        test2.DelimiterPlaceholder,
        test2.QualifierPlaceholder, test2.SkipDuplicateHeader, test2.TreatLfAsSpace, test2.TreatUnknownCharacterAsSpace,
        test2.TryToSolveMoreColumns,
        test2.WarnDelimiterInValue, test2.WarnLineFeed, test2.WarnNBSP, test2.WarnQuotes, test2.WarnUnknownCharacter,
        test2.WarnEmptyTailingColumns,
        test2.TreatNBSPAsSpace, test2.TreatTextAsNull, test2.SkipEmptyLines, test2.ConsecutiveEmptyRows,
        test2.IdentifierInContainer, StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, true, false);

      await test.OpenAsync(UnitTestStatic.Token);

      var dt = await test.GetDataTableAsync(TimeSpan.FromSeconds(30), false,
        false, false, false, null, UnitTestStatic.Token);
      Assert.AreEqual(test2.RecordLimit, dt.Rows.Count);
    }

    [TestMethod]
    public async Task GetDataTableAsync3()
    {
      var test3 =        new CsvFileDummy(UnitTestStatic.GetTestPath("WithEoFChar.txt")) { FieldDelimiterChar = '\t' };
      test3.ColumnCollection.Add(new Column("Memo", ValueFormat.Empty, ignore: true));
      using var test = new CsvFileReader(test3.FullPath, test3.CodePageId, test3.SkipRows, test3.HasFieldHeader,
        test3.ColumnCollection, test3.TrimmingOption,
        test3.FieldDelimiterChar, test3.FieldQualifierChar, test3.EscapePrefixChar, test3.RecordLimit, test3.AllowRowCombining,
        test3.ContextSensitiveQualifier,
        test3.CommentLine, test3.NumWarnings, test3.DuplicateQualifierToEscape, test3.NewLinePlaceholder,
        test3.DelimiterPlaceholder,
        test3.QualifierPlaceholder, test3.SkipDuplicateHeader, test3.TreatLfAsSpace, test3.TreatUnknownCharacterAsSpace,
        test3.TryToSolveMoreColumns,
        test3.WarnDelimiterInValue, test3.WarnLineFeed, test3.WarnNBSP, test3.WarnQuotes, test3.WarnUnknownCharacter,
        test3.WarnEmptyTailingColumns,
        test3.TreatNBSPAsSpace, test3.TreatTextAsNull, test3.SkipEmptyLines, test3.ConsecutiveEmptyRows,
        test3.IdentifierInContainer, StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, true, false);
      await test.OpenAsync(UnitTestStatic.Token);

      using var dt = await test.GetDataTableAsync(TimeSpan.FromSeconds(30), true,
        true, true, true, null, UnitTestStatic.Token);
      // 10 columns 1 ignored one added for Start line one for Error Field one for Record No one for
      // Line end
      Assert.AreEqual(10 - 1 + 4, dt.Columns.Count);
      Assert.AreEqual(19, dt.Rows.Count);
    }
  }
}