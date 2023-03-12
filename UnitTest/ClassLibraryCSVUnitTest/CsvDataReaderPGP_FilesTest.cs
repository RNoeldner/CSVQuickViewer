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
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public class CsvDataReaderPgpFilesTest
  {
    // ReSharper disable UseAwaitUsing

    [TestMethod]
    public async Task ReadGZipAsync()
    {
      var setting = new CsvFile(id: "Csv", fileName: "BasicCSV.txt.gz")
      {
        HasFieldHeader = true, RootFolder = UnitTestStatic.ApplicationDirectory, ContextSensitiveQualifier = true
      };
      setting.ColumnCollection.Add(new Column("ExamDate", new ValueFormat(DataTypeEnum.DateTime, @"dd/MM/yyyy")));
      setting.ColumnCollection.Add(new Column("ID", new ValueFormat(DataTypeEnum.Integer)));
      setting.ColumnCollection.Add(new Column("IsNativeLang", new ValueFormat(DataTypeEnum.Boolean)));

      using var test = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader,
        setting.ColumnCollection, setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar,
        setting.EscapePrefixChar, setting.RecordLimit, setting.AllowRowCombining, setting.ContextSensitiveQualifier,
        setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape, setting.NewLinePlaceholder,
        setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
        setting.TreatUnknownCharacterAsSpace, setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue,
        setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter,
        setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
        setting.ConsecutiveEmptyRows, setting.IdentifierInContainer, StandardTimeZoneAdjust.ChangeTimeZone,
        System.TimeZoneInfo.Local.Id, true, false);
      await test.OpenAsync(UnitTestStatic.Token);
      var row = 0;
      while (await test.ReadAsync(UnitTestStatic.Token))
        row++;
      Assert.AreEqual(row, test.RecordNumber);
      Assert.AreEqual(7, row);
    }

    [TestMethod, Timeout(1000) ]
    public async Task ReadPGPAsync()
    {
      UnitTestInitialize.SetApplicationPGPSetting();

      var setting = new CsvFile(id: "Csv", fileName: UnitTestStatic.GetTestPath("BasicCSV.pgp"))
      {
        HasFieldHeader = true, ContextSensitiveQualifier = true,
      };

      setting.ColumnCollection.Add(new Column("ExamDate", new ValueFormat(DataTypeEnum.DateTime, @"dd/MM/yyyy")));
      setting.ColumnCollection.Add(new Column("ID", new ValueFormat(DataTypeEnum.Integer)));
      setting.ColumnCollection.Add(new Column("IsNativeLang", new ValueFormat(DataTypeEnum.Boolean)));

      using var test = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader,
        setting.ColumnCollection, setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar,
        setting.EscapePrefixChar, setting.RecordLimit, setting.AllowRowCombining, setting.ContextSensitiveQualifier,
        setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape, setting.NewLinePlaceholder,
        setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
        setting.TreatUnknownCharacterAsSpace, setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue,
        setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter,
        setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
        setting.ConsecutiveEmptyRows, setting.IdentifierInContainer, StandardTimeZoneAdjust.ChangeTimeZone,
        System.TimeZoneInfo.Local.Id, true, false);
      await test.OpenAsync(UnitTestStatic.Token);
      var row = 0;
      while (await test.ReadAsync(UnitTestStatic.Token))
        row++;
      Assert.AreEqual(row, test.RecordNumber);
      Assert.AreEqual(7, row);
    }

    [TestMethod]
    public async Task ReadGpgAsync()
    {
      UnitTestInitialize.SetApplicationPGPSetting();


      var setting = new CsvFile(id: "csv", fileName: UnitTestStatic.GetTestPath("BasicCSV.pgp"))
      {
        HasFieldHeader = true, ContextSensitiveQualifier = true
      };
      setting.ColumnCollection.Add(new Column("ExamDate", new ValueFormat(DataTypeEnum.DateTime, @"dd/MM/yyyy")));
      setting.ColumnCollection.Add(new Column("ID", new ValueFormat(DataTypeEnum.Integer)));
      setting.ColumnCollection.Add(new Column("IsNativeLang", new ValueFormat(DataTypeEnum.Boolean)));

      using var test = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader,
        setting.ColumnCollection, setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar,
        setting.EscapePrefixChar, setting.RecordLimit, setting.AllowRowCombining, setting.ContextSensitiveQualifier,
        setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape, setting.NewLinePlaceholder,
        setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
        setting.TreatUnknownCharacterAsSpace, setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue,
        setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter,
        setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
        setting.ConsecutiveEmptyRows, setting.IdentifierInContainer, StandardTimeZoneAdjust.ChangeTimeZone,
        System.TimeZoneInfo.Local.Id, true, false);
      await test.OpenAsync(UnitTestStatic.Token);
      var row = 0;
      while (await test.ReadAsync(UnitTestStatic.Token))
        row++;
      Assert.AreEqual(row, test.RecordNumber);
      Assert.AreEqual(7, row);
    }
  }
}