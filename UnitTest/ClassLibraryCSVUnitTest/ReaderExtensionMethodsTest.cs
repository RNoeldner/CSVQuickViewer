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
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public class ReaderExtensionMethodsTest
  {
    private readonly CsvFile m_ValidSetting = new CsvFile
    {
      FileName = UnitTestStatic.GetTestPath("BasicCSV.txt"), FieldDelimiter = ",", CommentLine = "#"
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
        ",",
        "\"",
        "",
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
        StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.AreEqual(6, test.GetColumnsOfReader().Count());
    }

    [TestMethod]
    public async Task GetEmptyColumnHeaderAsyncTest()
    {
      using var test = new CsvFileReader(m_ValidSetting.FullPath, m_ValidSetting.CodePageId, m_ValidSetting.SkipRows,
        m_ValidSetting.HasFieldHeader,
        m_ValidSetting.ColumnCollection, m_ValidSetting.TrimmingOption, m_ValidSetting.FieldDelimiter,
        m_ValidSetting.FieldQualifier,
        m_ValidSetting.EscapePrefix, m_ValidSetting.RecordLimit, m_ValidSetting.AllowRowCombining,
        m_ValidSetting.ContextSensitiveQualifier,
        m_ValidSetting.CommentLine, m_ValidSetting.NumWarnings, m_ValidSetting.DuplicateQualifierToEscape,
        m_ValidSetting.NewLinePlaceholder,
        m_ValidSetting.DelimiterPlaceholder, m_ValidSetting.QualifierPlaceholder, m_ValidSetting.SkipDuplicateHeader,
        m_ValidSetting.TreatLfAsSpace,
        m_ValidSetting.TreatUnknownCharacterAsSpace, m_ValidSetting.TryToSolveMoreColumns,
        m_ValidSetting.WarnDelimiterInValue, m_ValidSetting.WarnLineFeed,
        m_ValidSetting.WarnNBSP, m_ValidSetting.WarnQuotes, m_ValidSetting.WarnUnknownCharacter,
        m_ValidSetting.WarnEmptyTailingColumns,
        m_ValidSetting.TreatNBSPAsSpace, m_ValidSetting.TreatTextAsNull, m_ValidSetting.SkipEmptyLines,
        m_ValidSetting.ConsecutiveEmptyRows,
        m_ValidSetting.IdentifierInContainer, StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      var result = await test.GetEmptyColumnHeaderAsync(UnitTestStatic.Token);
      Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetDataTableAsync2()
    {
      var test2 = (CsvFile) m_ValidSetting.Clone();
      test2.RecordLimit = 4;
      using var test = new CsvFileReader(test2.FullPath, test2.CodePageId, test2.SkipRows, test2.HasFieldHeader,
        test2.ColumnCollection, test2.TrimmingOption,
        test2.FieldDelimiter, test2.FieldQualifier, test2.EscapePrefix, test2.RecordLimit, test2.AllowRowCombining,
        test2.ContextSensitiveQualifier,
        test2.CommentLine, test2.NumWarnings, test2.DuplicateQualifierToEscape, test2.NewLinePlaceholder,
        test2.DelimiterPlaceholder,
        test2.QualifierPlaceholder, test2.SkipDuplicateHeader, test2.TreatLfAsSpace, test2.TreatUnknownCharacterAsSpace,
        test2.TryToSolveMoreColumns,
        test2.WarnDelimiterInValue, test2.WarnLineFeed, test2.WarnNBSP, test2.WarnQuotes, test2.WarnUnknownCharacter,
        test2.WarnEmptyTailingColumns,
        test2.TreatNBSPAsSpace, test2.TreatTextAsNull, test2.SkipEmptyLines, test2.ConsecutiveEmptyRows,
        test2.IdentifierInContainer, StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id);

      await test.OpenAsync(UnitTestStatic.Token);

      var dt = await test.GetDataTableAsync(TimeSpan.FromSeconds(30), false,
        false, false, false, false, null, UnitTestStatic.Token);
      Assert.AreEqual(test2.RecordLimit, dt.Rows.Count);
    }

    [TestMethod]
    public async Task GetDataTableAsync3()
    {
      var test3 = new CsvFile(UnitTestStatic.GetTestPath("WithEoFChar.txt")) { FieldDelimiter = "Tab" };
      test3.ColumnCollection.Add(new Column("Memo", new ValueFormat(), ignore: true));
      using var test = new CsvFileReader(test3.FullPath, test3.CodePageId, test3.SkipRows, test3.HasFieldHeader,
        test3.ColumnCollection, test3.TrimmingOption,
        test3.FieldDelimiter, test3.FieldQualifier, test3.EscapePrefix, test3.RecordLimit, test3.AllowRowCombining,
        test3.ContextSensitiveQualifier,
        test3.CommentLine, test3.NumWarnings, test3.DuplicateQualifierToEscape, test3.NewLinePlaceholder,
        test3.DelimiterPlaceholder,
        test3.QualifierPlaceholder, test3.SkipDuplicateHeader, test3.TreatLfAsSpace, test3.TreatUnknownCharacterAsSpace,
        test3.TryToSolveMoreColumns,
        test3.WarnDelimiterInValue, test3.WarnLineFeed, test3.WarnNBSP, test3.WarnQuotes, test3.WarnUnknownCharacter,
        test3.WarnEmptyTailingColumns,
        test3.TreatNBSPAsSpace, test3.TreatTextAsNull, test3.SkipEmptyLines, test3.ConsecutiveEmptyRows,
        test3.IdentifierInContainer, StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);

      using var dt = await test.GetDataTableAsync(TimeSpan.FromSeconds(30), true,
        true, true, true, true, null, UnitTestStatic.Token);
      // 10 columns 1 ignored one added for Start line one for Error Field one for Record No one for
      // Line end
      Assert.AreEqual(10 - 1 + 4, dt.Columns.Count);
      Assert.AreEqual(19, dt.Rows.Count);
    }
  }
}