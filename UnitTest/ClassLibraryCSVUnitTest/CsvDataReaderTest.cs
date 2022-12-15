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
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  [SuppressMessage("ReSharper", "UseAwaitUsing")]
  [SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
  public class CsvDataReaderUnitTest
  {
    private static readonly TimeZoneChangeDelegate m_TimeZoneAdjust = StandardTimeZoneAdjust.ChangeTimeZone;

    private readonly CsvFile m_ValidSetting =
      new CsvFile(id: string.Empty, fileName: UnitTestStatic.GetTestPath("BasicCSV.txt"))
      {
        FieldDelimiter = ",", CommentLine = "#"
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
    public async Task CheckEvents()
    {
      var openFinished = false;
      var onOpenCalled = false;
      var readFinished = false;

      using var test = new CsvFileReader(m_ValidSetting.FullPath, m_ValidSetting.CodePageId, m_ValidSetting.SkipRows,
        m_ValidSetting.HasFieldHeader,
        m_ValidSetting.ColumnCollection, m_ValidSetting.TrimmingOption,
        m_ValidSetting.FieldDelimiter, m_ValidSetting.FieldQualifier, m_ValidSetting.EscapePrefix,
        m_ValidSetting.RecordLimit, m_ValidSetting.AllowRowCombining,
        m_ValidSetting.ContextSensitiveQualifier, m_ValidSetting.CommentLine,
        m_ValidSetting.NumWarnings, m_ValidSetting.DuplicateQualifierToEscape, m_ValidSetting.NewLinePlaceholder,
        m_ValidSetting.DelimiterPlaceholder,
        m_ValidSetting.QualifierPlaceholder, m_ValidSetting.SkipDuplicateHeader,
        m_ValidSetting.TreatLfAsSpace, m_ValidSetting.TreatUnknownCharacterAsSpace,
        m_ValidSetting.TryToSolveMoreColumns, m_ValidSetting.WarnDelimiterInValue,
        m_ValidSetting.WarnLineFeed, m_ValidSetting.WarnNBSP,
        m_ValidSetting.WarnQuotes, m_ValidSetting.WarnUnknownCharacter, m_ValidSetting.WarnEmptyTailingColumns,
        m_ValidSetting.TreatNBSPAsSpace,
        m_ValidSetting.TreatTextAsNull, m_ValidSetting.SkipEmptyLines,
        m_ValidSetting.ConsecutiveEmptyRows, m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      test.OpenFinished += (o, a) => openFinished = true;
      test.ReadFinished += (o, a) => readFinished = true;
      test.OnOpenAsync = async () => await Task.FromResult(onOpenCalled = true);
      Assert.IsFalse(openFinished);
      Assert.IsFalse(readFinished);
      Assert.IsFalse(onOpenCalled);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsFalse(readFinished);
      Assert.IsTrue(onOpenCalled);
      Assert.IsTrue(openFinished);
      while (await test.ReadAsync(UnitTestStatic.Token))
      {
      }

      Assert.IsTrue(readFinished);
    }


    [TestMethod]
    public async Task AllFormatsPipeReaderAsync()
    {
      var setting =
        new CsvFile(id: string.Empty, fileName: UnitTestStatic.GetTestPath("AllFormatsPipe.txt"))
        {
          HasFieldHeader = true, FieldDelimiter = "|", FieldQualifier = "\"", SkipEmptyLines = false
        };

      using var test = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader,
        setting.ColumnCollection,
        setting.TrimmingOption, setting.FieldDelimiter, setting.FieldQualifier, setting.EscapePrefix,
        setting.RecordLimit, setting.AllowRowCombining,
        setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
        setting.NewLinePlaceholder,
        setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
        setting.TreatUnknownCharacterAsSpace,
        setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
        setting.WarnQuotes, setting.WarnUnknownCharacter,
        setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
        setting.ConsecutiveEmptyRows,
        setting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);

      Assert.AreEqual(10, test.FieldCount);
      await test.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual(2, test.StartLineNumber);
      Assert.AreEqual(3, test.EndLineNumber);
      Assert.AreEqual(1, test.RecordNumber);
      Assert.AreEqual("-22477", test.GetString(1));
      await test.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual(3, test.StartLineNumber);
      Assert.AreEqual(4, test.EndLineNumber);
      Assert.AreEqual(2, test.RecordNumber);
      Assert.AreEqual("22435", test.GetString(1));
      for (var line = 3; line < 25; line++)
        await test.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("-21928", test.GetString(1));
      Assert.IsTrue(test.GetString(4).EndsWith("twpapulfffy"));
      Assert.AreEqual(25, test.StartLineNumber);
      Assert.AreEqual(27, test.EndLineNumber);
      Assert.AreEqual(24, test.RecordNumber);

      for (var line = 25; line < 47; line++)
        await test.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual(49, test.EndLineNumber);
      Assert.AreEqual("4390", test.GetString(1));

      Assert.AreEqual(46, test.RecordNumber);
    }

    [TestMethod]
    public async Task IssueReaderAsync()
    {
      var basIssues = new CsvFile(id: "Csv", fileName: UnitTestStatic.GetTestPath("BadIssues.csv"))
      {
        TreatLfAsSpace = true,
        TryToSolveMoreColumns = true,
        AllowRowCombining = true,
        FieldDelimiter = "Tab",
        FieldQualifier = string.Empty
      };
      basIssues.ColumnCollection.Add(new Column("effectiveDate",
        new ValueFormat(DataTypeEnum.DateTime, "yyyy/MM/dd", "-")));
      basIssues.ColumnCollection.Add(new Column("timestamp",
        new ValueFormat(DataTypeEnum.DateTime, "yyyy/MM/ddTHH:mm:ss", "-")));

      basIssues.ColumnCollection.Add(new Column("version", new ValueFormat(DataTypeEnum.Integer)));
      basIssues.ColumnCollection.Add(new Column("retrainingRequired",
        new ValueFormat(DataTypeEnum.Boolean)));

      basIssues.ColumnCollection.Add(new Column("classroomTraining",
        new ValueFormat(DataTypeEnum.Boolean)));
      basIssues.ColumnCollection.Add(new Column("webLink", new ValueFormat(DataTypeEnum.TextToHtml)));


      using var test = new CsvFileReader(basIssues.FullPath, basIssues.CodePageId, basIssues.SkipRows,
        basIssues.HasFieldHeader, basIssues.ColumnCollection,
        basIssues.TrimmingOption, basIssues.FieldDelimiter, basIssues.FieldQualifier, basIssues.EscapePrefix,
        basIssues.RecordLimit,
        basIssues.AllowRowCombining, basIssues.ContextSensitiveQualifier, basIssues.CommentLine, basIssues.NumWarnings,
        basIssues.DuplicateQualifierToEscape,
        basIssues.NewLinePlaceholder, basIssues.DelimiterPlaceholder, basIssues.QualifierPlaceholder,
        basIssues.SkipDuplicateHeader, basIssues.TreatLfAsSpace,
        basIssues.TreatUnknownCharacterAsSpace, basIssues.TryToSolveMoreColumns, basIssues.WarnDelimiterInValue,
        basIssues.WarnLineFeed, basIssues.WarnNBSP,
        basIssues.WarnQuotes, basIssues.WarnUnknownCharacter, basIssues.WarnEmptyTailingColumns,
        basIssues.TreatNBSPAsSpace, basIssues.TreatTextAsNull,
        basIssues.SkipEmptyLines, basIssues.ConsecutiveEmptyRows, basIssues.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      var warningList = new RowErrorCollection(test);
      await test.OpenAsync(UnitTestStatic.Token);
      warningList.HandleIgnoredColumns(test);
      // need 22 columns
      Assert.AreEqual(22, test.GetSchemaTable().Rows.Count());

      // This should work
      await test.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual(0, warningList.CountRows);

      Assert.AreEqual("Eagle_sop020517", test.GetValue(0));
      Assert.AreEqual("de-DE", test.GetValue(2));

      // There are more columns
      await test.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual(1, warningList.CountRows);
      Assert.AreEqual("Eagle_SRD-0137699", test.GetValue(0));
      Assert.AreEqual("de-DE", test.GetValue(2));
      Assert.AreEqual(3, test.StartLineNumber);

      await test.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("Eagle_600.364", test.GetValue(0));
      Assert.AreEqual(4, test.StartLineNumber);

      await test.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("Eagle_spt029698", test.GetValue(0));
      Assert.AreEqual(5, test.StartLineNumber);

      await test.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("Eagle_SRD-0137698", test.GetValue(0));
      Assert.AreEqual(2, warningList.CountRows);
      Assert.AreEqual(6, test.StartLineNumber);

      await test.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("Eagle_SRD-0138074", test.GetValue(0));
      Assert.AreEqual(7, test.StartLineNumber);

      await test.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("Eagle_SRD-0125563", test.GetValue(0));
      Assert.AreEqual(8, test.StartLineNumber);

      await test.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("doc_1004040002982", test.GetValue(0));
      Assert.AreEqual(3, warningList.CountRows);
      Assert.AreEqual(9, test.StartLineNumber);

      await test.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("doc_1004040002913", test.GetValue(0));
      Assert.AreEqual(10, test.StartLineNumber, "StartLineNumber");
      Assert.AreEqual(4, warningList.CountRows);

      await test.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("doc_1003001000427", test.GetValue(0));
      Assert.AreEqual(12, test.StartLineNumber);

      await test.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("doc_1008017000611", test.GetValue(0));

      await test.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("doc_1004040000268", test.GetValue(0));

      await test.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("doc_1008011000554", test.GetValue(0));
      await test.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("doc_1003001000936", test.GetValue(0));

      await test.ReadAsync(UnitTestStatic.Token);
      Assert.AreEqual("doc_1200000124471", test.GetValue(0));

      //await test.ReadAsync(UnitTestStatic.Token);
      //Assert.AreEqual("doc_1200000134529", test.GetValue(0));

      //await test.ReadAsync(UnitTestStatic.Token);
      //Assert.AreEqual("doc_1004040003504", test.GetValue(0));

      //await test.ReadAsync(UnitTestStatic.Token);
      //Assert.AreEqual("doc_1200000016068", test.GetValue(0));

      await test.ReadAsync(UnitTestStatic.Token);
    }

    [TestMethod]
    public async Task TestGetDataTypeNameAsync()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.AreEqual("String", test.GetDataTypeName(0));
    }

    [TestMethod]
    public async Task TestWarningsRecordNoMappingAsync()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      var dataTable = new DataTable { TableName = "DataTable", Locale = CultureInfo.InvariantCulture };

      dataTable.Columns.Add(test.GetName(0), test.GetFieldType(0));

      var recordNumberColumn = dataTable.Columns.Add(ReaderConstants.cRecordNumberFieldName, typeof(long));
      recordNumberColumn.AllowDBNull = true;

      var lineNumberColumn = dataTable.Columns.Add(ReaderConstants.cEndLineNumberFieldName, typeof(long));
      lineNumberColumn.AllowDBNull = true;

      _ = dataTable.NewRow();
      await test.ReadAsync(UnitTestStatic.Token);
      _ = new Dictionary<int, string> { { -1, "Test1" }, { 0, "Test2" } };

      //test.AssignNumbersAndWarnings(dataRow, null, recordNumberColumn, lineNumberColumn, null, warningsList);
      //Assert.AreEqual("Test1", dataRow.RowError);
      //Assert.AreEqual("Test2", dataRow.GetColumnError(0));
    }

    [TestMethod]
    public void GetInteger32And64()
    {
      var column = new Column("test",
        new ValueFormat(DataTypeEnum.Integer, groupSeparator: ",", decimalSeparator: "."));


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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      var inputValue = "17";

      var value32 = test.GetInt32Null(inputValue, column);
      Assert.IsTrue(value32.HasValue);
      Assert.AreEqual(17, value32.Value);

      var value64 = test.GetInt64Null(inputValue, column);
      Assert.IsTrue(value64.HasValue);
      Assert.AreEqual(17, value64.Value);

#pragma warning disable CS8625
      value32 = test.GetInt32Null(null, column);
      Assert.IsFalse(value32.HasValue);

      value64 = test.GetInt64Null(null, column);
#pragma warning restore CS8625
      Assert.IsFalse(value64.HasValue);
    }

    [TestMethod]
    public async Task TestBatchFinishedNotifcationAsync()
    {
      var finished = false;

      using (var test = new CsvFileReader(m_ValidSetting.FullPath, m_ValidSetting.CodePageId, m_ValidSetting.SkipRows,
               m_ValidSetting.HasFieldHeader,
               m_ValidSetting.ColumnCollection, m_ValidSetting.TrimmingOption, m_ValidSetting.FieldDelimiter,
               m_ValidSetting.FieldQualifier,
               m_ValidSetting.EscapePrefix, m_ValidSetting.RecordLimit, m_ValidSetting.AllowRowCombining,
               m_ValidSetting.ContextSensitiveQualifier,
               m_ValidSetting.CommentLine, m_ValidSetting.NumWarnings, m_ValidSetting.DuplicateQualifierToEscape,
               m_ValidSetting.NewLinePlaceholder,
               m_ValidSetting.DelimiterPlaceholder, m_ValidSetting.QualifierPlaceholder,
               m_ValidSetting.SkipDuplicateHeader, m_ValidSetting.TreatLfAsSpace,
               m_ValidSetting.TreatUnknownCharacterAsSpace, m_ValidSetting.TryToSolveMoreColumns,
               m_ValidSetting.WarnDelimiterInValue, m_ValidSetting.WarnLineFeed,
               m_ValidSetting.WarnNBSP, m_ValidSetting.WarnQuotes, m_ValidSetting.WarnUnknownCharacter,
               m_ValidSetting.WarnEmptyTailingColumns,
               m_ValidSetting.TreatNBSPAsSpace, m_ValidSetting.TreatTextAsNull, m_ValidSetting.SkipEmptyLines,
               m_ValidSetting.ConsecutiveEmptyRows,
               m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id))
      {
        test.ReadFinished += delegate { finished = true; };
        await test.OpenAsync(UnitTestStatic.Token);

        while (await test.ReadAsync(UnitTestStatic.Token))
        {
        }
      }

      Assert.IsTrue(finished, "ReadFinished");
    }

    [TestMethod]
    public async Task TestReadFinishedNotificationAsync()
    {
      var finished = false;

      using (var test = new CsvFileReader(m_ValidSetting.FullPath, m_ValidSetting.CodePageId, m_ValidSetting.SkipRows,
               m_ValidSetting.HasFieldHeader,
               m_ValidSetting.ColumnCollection, m_ValidSetting.TrimmingOption, m_ValidSetting.FieldDelimiter,
               m_ValidSetting.FieldQualifier,
               m_ValidSetting.EscapePrefix, m_ValidSetting.RecordLimit, m_ValidSetting.AllowRowCombining,
               m_ValidSetting.ContextSensitiveQualifier,
               m_ValidSetting.CommentLine, m_ValidSetting.NumWarnings, m_ValidSetting.DuplicateQualifierToEscape,
               m_ValidSetting.NewLinePlaceholder,
               m_ValidSetting.DelimiterPlaceholder, m_ValidSetting.QualifierPlaceholder,
               m_ValidSetting.SkipDuplicateHeader, m_ValidSetting.TreatLfAsSpace,
               m_ValidSetting.TreatUnknownCharacterAsSpace, m_ValidSetting.TryToSolveMoreColumns,
               m_ValidSetting.WarnDelimiterInValue, m_ValidSetting.WarnLineFeed,
               m_ValidSetting.WarnNBSP, m_ValidSetting.WarnQuotes, m_ValidSetting.WarnUnknownCharacter,
               m_ValidSetting.WarnEmptyTailingColumns,
               m_ValidSetting.TreatNBSPAsSpace, m_ValidSetting.TreatTextAsNull, m_ValidSetting.SkipEmptyLines,
               m_ValidSetting.ConsecutiveEmptyRows,
               m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id))
      {
        test.ReadFinished += delegate { finished = true; };
        await test.OpenAsync(UnitTestStatic.Token);
        while (await test.ReadAsync(UnitTestStatic.Token))
        {
        }
      }

      Assert.IsTrue(finished);
    }

    [TestMethod]
    public void ColumnFormat()
    {
      var target = new CsvFile(id: string.Empty, fileName: string.Empty);
      m_ValidSetting.CopyTo(target);

      Assert.IsNotNull(target.ColumnCollection.GetByName("Score"));
      var cf = target.ColumnCollection.GetByName("Score");
      Assert.AreEqual(cf?.Name, "Score");

      // Remove the one filed
      target.ColumnCollection.Remove(target.ColumnCollection.GetByName("Score")!);
      Assert.IsNull(target.ColumnCollection.GetByName("Score"));
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public async Task GetDateTimeTestAsync()
    {
      var csvFile = new CsvFile(id: "csv", fileName: UnitTestStatic.GetTestPath("TestFile.txt"))
      {
        CodePageId = 65001, FieldDelimiter = "tab"
      };

      csvFile.ColumnCollection.Add(new Column("Title", new ValueFormat(DataTypeEnum.DateTime)));


      using var test = new CsvFileReader(csvFile.FullPath, csvFile.CodePageId, csvFile.SkipRows, csvFile.HasFieldHeader,
        csvFile.ColumnCollection,
        csvFile.TrimmingOption, csvFile.FieldDelimiter, csvFile.FieldQualifier, csvFile.EscapePrefix,
        csvFile.RecordLimit, csvFile.AllowRowCombining,
        csvFile.ContextSensitiveQualifier, csvFile.CommentLine, csvFile.NumWarnings, csvFile.DuplicateQualifierToEscape,
        csvFile.NewLinePlaceholder,
        csvFile.DelimiterPlaceholder, csvFile.QualifierPlaceholder, csvFile.SkipDuplicateHeader, csvFile.TreatLfAsSpace,
        csvFile.TreatUnknownCharacterAsSpace,
        csvFile.TryToSolveMoreColumns, csvFile.WarnDelimiterInValue, csvFile.WarnLineFeed, csvFile.WarnNBSP,
        csvFile.WarnQuotes, csvFile.WarnUnknownCharacter,
        csvFile.WarnEmptyTailingColumns, csvFile.TreatNBSPAsSpace, csvFile.TreatTextAsNull, csvFile.SkipEmptyLines,
        csvFile.ConsecutiveEmptyRows,
        csvFile.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      await test.ReadAsync(UnitTestStatic.Token);
      _ = test.GetDateTime(1);
    }

    [TestMethod]
    public async Task CsvDataReaderImportFileEmptyNullNotExisting()
    {
      var setting = new CsvFile(String.Empty, string.Empty);

      try
      {
        using (new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader,
                 setting.ColumnCollection,
                 setting.TrimmingOption, setting.FieldDelimiter, setting.FieldQualifier, setting.EscapePrefix,
                 setting.RecordLimit, setting.AllowRowCombining,
                 setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings,
                 setting.DuplicateQualifierToEscape, setting.NewLinePlaceholder,
                 setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader,
                 setting.TreatLfAsSpace, setting.TreatUnknownCharacterAsSpace,
                 setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
                 setting.WarnQuotes, setting.WarnUnknownCharacter,
                 setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull,
                 setting.SkipEmptyLines, setting.ConsecutiveEmptyRows,
                 setting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id))
        {
        }

        Assert.Fail("Exception expected");
      }
      catch (ArgumentException)
      {
      }
      catch (FileReaderException)
      {
      }
      catch (Exception ex)
      {
        Assert.Fail($"Wrong Exception Type {ex.GetType()}, Empty Filename");
      }

      try
      {
        setting.FileName = @"b;dslkfg;sldfkgjs;ldfkgj;sldfkg.sdfgsfd";

        using (var reader = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows,
                 setting.HasFieldHeader, setting.ColumnCollection,
                 setting.TrimmingOption, setting.FieldDelimiter, setting.FieldQualifier, setting.EscapePrefix,
                 setting.RecordLimit, setting.AllowRowCombining,
                 setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings,
                 setting.DuplicateQualifierToEscape, setting.NewLinePlaceholder,
                 setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader,
                 setting.TreatLfAsSpace, setting.TreatUnknownCharacterAsSpace,
                 setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
                 setting.WarnQuotes, setting.WarnUnknownCharacter,
                 setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull,
                 setting.SkipEmptyLines, setting.ConsecutiveEmptyRows,
                 setting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id))
        {
          await reader.OpenAsync(UnitTestStatic.Token);
        }

        Assert.Fail("Exception expected");
      }
      catch (ArgumentException)
      {
      }
      catch (FileNotFoundException)
      {
      }
      catch (Exception ex)
      {
        Assert.Fail($"Wrong Exception Type {ex.GetType()}, Invalid Filename");
      }
    }

    [TestMethod]
    public async Task CsvDataReaderRecordNumberEmptyLinesAsync()
    {
      var setting =
        new CsvFile(fileName: UnitTestStatic.GetTestPath("BasicCSVEmptyLine.txt")) { HasFieldHeader = true };


      using var test = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader,
        setting.ColumnCollection,
        setting.TrimmingOption, setting.FieldDelimiter, setting.FieldQualifier, setting.EscapePrefix,
        setting.RecordLimit, setting.AllowRowCombining,
        setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
        setting.NewLinePlaceholder,
        setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
        setting.TreatUnknownCharacterAsSpace,
        setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
        setting.WarnQuotes, setting.WarnUnknownCharacter,
        setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
        setting.ConsecutiveEmptyRows,
        setting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      var row = 0;
      while (await test.ReadAsync(UnitTestStatic.Token))
        row++;
      Assert.AreEqual(row, test.RecordNumber);
      Assert.AreEqual(2, row, "total Rows");
    }

    [TestMethod]
    public async Task CsvDataReaderRecordNumberEmptyLinesSkipEmptyLinesAsync()
    {
      var setting = new CsvFile(fileName: UnitTestStatic.GetTestPath("BasicCSVEmptyLine.txt"))
      {
        HasFieldHeader = true, SkipEmptyLines = false, ConsecutiveEmptyRows = 3
      };
      /*
       * ID,LangCode,ExamDate,Score,Proficiency,IsNativeLang
1
2 00001,German,20/01/2010,276,0.94,Y
3 ,,,,,
4 ,,,,,
5 00001,English,22/01/2012,190,,N
6 ,,,,,
7 ,,,,,
8 ,,,,,
9 ,,,,,
10
*/


      using var test = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader,
        setting.ColumnCollection,
        setting.TrimmingOption, setting.FieldDelimiter, setting.FieldQualifier, setting.EscapePrefix,
        setting.RecordLimit, setting.AllowRowCombining,
        setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
        setting.NewLinePlaceholder,
        setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
        setting.TreatUnknownCharacterAsSpace,
        setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
        setting.WarnQuotes, setting.WarnUnknownCharacter,
        setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
        setting.ConsecutiveEmptyRows,
        setting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      var row = 0;
      while (await test.ReadAsync(UnitTestStatic.Token))
        row++;
      Assert.AreEqual(row, test.RecordNumber, "Compare with RecordNumber");
      Assert.AreEqual(7, row, "Read");
    }

    [TestMethod]
    public async Task CsvDataReaderPropertiesAsync()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);

      Assert.AreEqual(0, test.Depth, "Depth");
      Assert.AreEqual(6, test.FieldCount, "FieldCount");
      Assert.AreEqual(0U, test.RecordNumber, "RecordNumber");
      Assert.AreEqual(-1, test.RecordsAffected, "RecordsAffected");

      Assert.IsFalse(test.EndOfFile, "EndOfFile");
      Assert.IsFalse(test.IsClosed, "IsClosed");
    }

    [TestMethod]
    public async Task CsvDataReaderGetNameAsync()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.AreEqual("ID", test.GetName(0));
      Assert.AreEqual("LangCodeID", test.GetName(1));
      Assert.AreEqual("ExamDate", test.GetName(2));
      Assert.AreEqual("Score", test.GetName(3));
      Assert.AreEqual("Proficiency", test.GetName(4));
      Assert.AreEqual("IsNativeLang", test.GetName(5));
    }

    [TestMethod]
    public async Task CsvDataReaderGetOrdinalAsync()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.AreEqual(0, test.GetOrdinal("ID"));
      Assert.AreEqual(1, test.GetOrdinal("LangCodeID"));
      Assert.AreEqual(2, test.GetOrdinal("ExamDate"));
      Assert.AreEqual(3, test.GetOrdinal("Score"));
      Assert.AreEqual(4, test.GetOrdinal("Proficiency"));
      Assert.AreEqual(5, test.GetOrdinal("IsNativeLang"));
      Assert.AreEqual(-1, test.GetOrdinal("Not Existing"));
    }

    [TestMethod]
    public async Task CsvDataReaderUseIndexerAsync()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.AreEqual("1", test["ID"]);
      Assert.AreEqual("German", test[1]);
      Assert.AreEqual(new DateTime(2010, 01, 20), test["ExamDate"]);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.AreEqual(DBNull.Value, test["Proficiency"]);
    }

    [TestMethod]
    public async Task CsvDataReaderGetValueNullAsync()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.AreEqual(DBNull.Value, test.GetValue(4));
    }

#if COMInterface
    [TestMethod]
    public async Task CsvDataReader_GetValueADONull()
    {
      using (CsvFileReader test = new CsvFileReader())
      {
        test.Open(false);
        Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
        Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
        Assert.AreEqual(DBNull.Value, test.GetValueADO(4));
      }
    }

    [TestMethod]
    [ExpectedException(typeof(NullReferenceException))]
    public async Task CsvDataReader_GetValueADONoRead()
    {
      using (CsvFileReader test = new CsvFileReader())
      {
        test.Open(false);
        Assert.AreEqual(DBNull.Value, test.GetValueADO(0));
      }
    }

    [TestMethod]
    public async Task CsvDataReader_GetValueADO()
    {
      using (CsvFileReader test = new CsvFileReader())
      {
        test.Open(false);
        Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
        Assert.AreEqual("1", test.GetValueADO(0));
        Assert.AreEqual("German", test.GetValueADO(1));
        Assert.AreEqual(new DateTime(2010, 01, 20), test.GetValueADO(2));
      }
    }

#endif

    [TestMethod]
    public async Task CsvDataReaderGetBooleanAsync()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.IsTrue(test.GetBoolean(5));
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.IsFalse(test.GetBoolean(5));
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public async Task CsvDataReaderGetBooleanErrorAsync()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      _ = test.GetBoolean(1);
    }

    [TestMethod]
    public async Task CsvDataReaderGetDateTimeAsync()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      // 20/01/2010
      Assert.AreEqual(new DateTime(2010, 01, 20), test.GetDateTime(2));
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public async Task CsvDataReaderGetDateTimeErrorAsync()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      test.GetDateTime(1);
    }

    [TestMethod]
    public async Task CsvDataReaderGetInt32Async()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.AreEqual(276, test.GetInt32(3));
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public async Task CsvDataReaderGetInt32ErrorAsync()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      test.GetInt32(1);
    }

    [TestMethod]
    public async Task CsvDataReaderGetDecimalAsync()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.AreEqual(0.94m, test.GetDecimal(4));
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public async Task CsvDataReaderGetDecimalErrorAsync()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      test.GetDecimal(1);
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public async Task CsvDataReaderGetInt32NullAsync()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      test.GetInt32(4);
    }

    [TestMethod]
    public async Task CsvDataReaderGetBytesAsync()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      var buffer = new byte[100];
      Assert.AreEqual(-1L, test.GetBytes(0, 0, buffer, 0, buffer.Length));
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public async Task CsvDataReaderGetDataAsync()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
#pragma warning disable CS0618 // Typ oder Element ist veraltet
      test.GetData(0);
#pragma warning restore CS0618 // Typ oder Element ist veraltet
    }

    [TestMethod]
    public async Task CsvDataReaderGetFloatAsync()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.AreEqual(Convert.ToSingle(0.94), test.GetFloat(4));
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public async Task CsvDataReaderGetFloatErrorAsync()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      test.GetFloat(1);
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public async Task CsvDataReaderGetGuidAsync()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      test.GetGuid(1);
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public async Task CsvDataReaderGetDateTimeNullAsync()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      test.GetDateTime(2);
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public async Task CsvDataReaderGetDateTimeWrongTypeAsync()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      test.GetDateTime(1);
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public async Task CsvDataReaderGetDecimalFormatException()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      test.GetDecimal(4);
    }

    [TestMethod]
    public async Task CsvDataReaderGetByte()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.AreEqual(1, test.GetByte(0));
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public async Task CsvDataReaderGetByteFrormat()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.AreEqual(1, test.GetByte(1));
    }

    [TestMethod]
    public async Task CsvDataReaderGetDouble()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.AreEqual(1, test.GetDouble(0));
    }

    [TestMethod]
    [ExpectedException(typeof(FormatException))]
    public async Task CsvDataReaderGetDoubleFrormat()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.AreEqual(1, test.GetDouble(1));
    }

    [TestMethod]
    public async Task CsvDataReaderGetInt16()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.AreEqual(1, test.GetInt16(0));
    }

    [TestMethod]
    public async Task CsvDataReaderInitWarnings()
    {
      var setting = new CsvFile(fileName: UnitTestStatic.GetTestPath("BasicCSV.txt"))
      {
        HasFieldHeader = false, SkipRows = 1
      };
      setting.FieldQualifier = "XX";
      setting.FieldDelimiter = ",,";


      using var test = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader,
        setting.ColumnCollection,
        setting.TrimmingOption, setting.FieldDelimiter, setting.FieldQualifier, setting.EscapePrefix,
        setting.RecordLimit, setting.AllowRowCombining,
        setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
        setting.NewLinePlaceholder,
        setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
        setting.TreatUnknownCharacterAsSpace,
        setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
        setting.WarnQuotes, setting.WarnUnknownCharacter,
        setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
        setting.ConsecutiveEmptyRows,
        setting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      var warningList = new RowErrorCollection(test);
      await test.OpenAsync(UnitTestStatic.Token);
      warningList.HandleIgnoredColumns(test);
      // This is now check in the constructr, butr the constructor does not habe the error handling
      // set Assert.IsTrue(warningList.Display.Contains("Only the first character of 'XX' is be used
      // for quoting.")); Assert.IsTrue(warningList.Display.Contains("Only the first character of
      // ',,' is used as delimiter."));
    }

    [TestMethod]
    public async Task CsvDataReaderInitErrorFieldDelimiterCr()
    {
      var setting = new CsvFile(fileName: UnitTestStatic.GetTestPath("BasicCSV.txt"))
      {
        HasFieldHeader = false, SkipRows = 1, FieldDelimiter = "\r"
      };
      var exception = false;
      try
      {

        using var test = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows,
          setting.HasFieldHeader, setting.ColumnCollection,
          setting.TrimmingOption, setting.FieldDelimiter, setting.FieldQualifier, setting.EscapePrefix,
          setting.RecordLimit, setting.AllowRowCombining,
          setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings,
          setting.DuplicateQualifierToEscape, setting.NewLinePlaceholder,
          setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader,
          setting.TreatLfAsSpace, setting.TreatUnknownCharacterAsSpace,
          setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
          setting.WarnQuotes, setting.WarnUnknownCharacter,
          setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
          setting.ConsecutiveEmptyRows,
          setting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
        await test.OpenAsync(UnitTestStatic.Token);
      }
      catch (ArgumentException)
      {
        exception = true;
      }
      catch (FileReaderException)
      {
        exception = true;
      }
      catch (Exception)
      {
        Assert.Fail("Wrong Exception Type");
      }

      Assert.IsTrue(exception, "No Exception thrown");
    }

    [TestMethod]
    public async Task CsvDataReaderInitErrorFieldQualifierCr()
    {
      var setting = new CsvFile(fileName: UnitTestStatic.GetTestPath("BasicCSV.txt"))
      {
        HasFieldHeader = false, SkipRows = 1, FieldQualifier = "Carriage return"
      };
      var exception = false;
      try
      {

        using var test = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows,
          setting.HasFieldHeader, setting.ColumnCollection,
          setting.TrimmingOption, setting.FieldDelimiter, setting.FieldQualifier, setting.EscapePrefix,
          setting.RecordLimit, setting.AllowRowCombining,
          setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings,
          setting.DuplicateQualifierToEscape, setting.NewLinePlaceholder,
          setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader,
          setting.TreatLfAsSpace, setting.TreatUnknownCharacterAsSpace,
          setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
          setting.WarnQuotes, setting.WarnUnknownCharacter,
          setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
          setting.ConsecutiveEmptyRows,
          setting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
        await test.OpenAsync(UnitTestStatic.Token);
      }
      catch (ArgumentException)
      {
        exception = true;
      }
      catch (FileReaderException)
      {
        exception = true;
      }
      catch (Exception)
      {
        Assert.Fail("Wrong Exception Type");
      }

      Assert.IsTrue(exception, "No Exception thrown");
    }

    [TestMethod]
    public async Task CsvDataReaderInitErrorFieldQualifierLf()
    {
      var setting = new CsvFile(fileName: UnitTestStatic.GetTestPath("BasicCSV.txt"))
      {
        HasFieldHeader = false, SkipRows = 1, FieldQualifier = "Line feed"
      };
      var exception = false;
      try
      {

        using var test = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows,
          setting.HasFieldHeader, setting.ColumnCollection,
          setting.TrimmingOption, setting.FieldDelimiter, setting.FieldQualifier, setting.EscapePrefix,
          setting.RecordLimit, setting.AllowRowCombining,
          setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings,
          setting.DuplicateQualifierToEscape, setting.NewLinePlaceholder,
          setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader,
          setting.TreatLfAsSpace, setting.TreatUnknownCharacterAsSpace,
          setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
          setting.WarnQuotes, setting.WarnUnknownCharacter,
          setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
          setting.ConsecutiveEmptyRows,
          setting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
        await test.OpenAsync(UnitTestStatic.Token);
      }
      catch (ArgumentException)
      {
        exception = true;
      }
      catch (FileReaderException)
      {
        exception = true;
      }
      catch (Exception)
      {
        Assert.Fail("Wrong Exception Type");
      }

      Assert.IsTrue(exception, "No Exception thrown");
    }

    [TestMethod]
    public async Task CsvDataReaderGuessCodePage()
    {
      var setting = new CsvFile(fileName: UnitTestStatic.GetTestPath("BasicCSV.txt"))
      {
        HasFieldHeader = true, CodePageId = 0
      };
      setting.FieldDelimiter = ",";
      using (var test = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows,
               setting.HasFieldHeader, setting.ColumnCollection,
               setting.TrimmingOption, setting.FieldDelimiter, setting.FieldQualifier, setting.EscapePrefix,
               setting.RecordLimit, setting.AllowRowCombining,
               setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings,
               setting.DuplicateQualifierToEscape, setting.NewLinePlaceholder,
               setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader,
               setting.TreatLfAsSpace, setting.TreatUnknownCharacterAsSpace,
               setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
               setting.WarnQuotes, setting.WarnUnknownCharacter,
               setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull,
               setting.SkipEmptyLines, setting.ConsecutiveEmptyRows,
               setting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id))
      {
        await test.OpenAsync(UnitTestStatic.Token);
      }

      Assert.AreEqual(1200, setting.CurrentEncoding.WindowsCodePage); // UTF-16 little endian
    }

    [TestMethod]
    public async Task CsvDataReaderInitErrorFieldDelimiterLf()
    {
      var setting = new CsvFile(fileName: UnitTestStatic.GetTestPath("BasicCSV.txt"))
      {
        HasFieldHeader = false, SkipRows = 1, FieldDelimiter = "\n"
      };
      var exception = false;
      try
      {
        using var test = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows,
          setting.HasFieldHeader, setting.ColumnCollection,
          setting.TrimmingOption, setting.FieldDelimiter, setting.FieldQualifier, setting.EscapePrefix,
          setting.RecordLimit, setting.AllowRowCombining,
          setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings,
          setting.DuplicateQualifierToEscape, setting.NewLinePlaceholder,
          setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader,
          setting.TreatLfAsSpace, setting.TreatUnknownCharacterAsSpace,
          setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
          setting.WarnQuotes, setting.WarnUnknownCharacter,
          setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
          setting.ConsecutiveEmptyRows,
          setting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
        await test.OpenAsync(UnitTestStatic.Token);
      }
      catch (ArgumentException)
      {
        exception = true;
      }
      catch (FileReaderException)
      {
        exception = true;
      }
      catch (Exception)
      {
        Assert.Fail("Wrong Exception Type");
      }

      Assert.IsTrue(exception, "No Exception thrown");
    }

    [TestMethod]
    public async Task CsvDataReaderInitErrorFieldDelimiterSpace()
    {
      var setting = new CsvFile("id", UnitTestStatic.GetTestPath("BasicCSV.txt"))
      {
        HasFieldHeader = false, SkipRows = 1, FieldDelimiter = " "
      };
      var exception = false;
      try
      {
        using var test = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows,
          setting.HasFieldHeader, setting.ColumnCollection,
          setting.TrimmingOption, setting.FieldDelimiter, setting.FieldQualifier, setting.EscapePrefix,
          setting.RecordLimit, setting.AllowRowCombining,
          setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings,
          setting.DuplicateQualifierToEscape, setting.NewLinePlaceholder,
          setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader,
          setting.TreatLfAsSpace, setting.TreatUnknownCharacterAsSpace,
          setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
          setting.WarnQuotes, setting.WarnUnknownCharacter,
          setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
          setting.ConsecutiveEmptyRows,
          setting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
        await test.OpenAsync(UnitTestStatic.Token);
      }
      catch (ArgumentException)
      {
        exception = true;
      }
      catch (FileReaderException)
      {
        exception = true;
      }
      catch (Exception)
      {
        Assert.Fail("Wrong Exception Type");
      }

      Assert.IsTrue(exception, "No Exception thrown");
    }

    [TestMethod]
    public async Task CsvDataReaderInitErrorFieldQualifierIsFieldDelimiter()
    {
      var setting = new CsvFile(fileName: UnitTestStatic.GetTestPath("BasicCSV.txt"))
      {
        HasFieldHeader = false, SkipRows = 1, FieldQualifier = "\""
      };
      setting.FieldDelimiter = setting.FieldQualifier;
      var exception = false;
      try
      {
        using var test = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows,
          setting.HasFieldHeader, setting.ColumnCollection,
          setting.TrimmingOption, setting.FieldDelimiter, setting.FieldQualifier, setting.EscapePrefix,
          setting.RecordLimit, setting.AllowRowCombining,
          setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings,
          setting.DuplicateQualifierToEscape, setting.NewLinePlaceholder,
          setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader,
          setting.TreatLfAsSpace, setting.TreatUnknownCharacterAsSpace,
          setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
          setting.WarnQuotes, setting.WarnUnknownCharacter,
          setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
          setting.ConsecutiveEmptyRows,
          setting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
        await test.OpenAsync(UnitTestStatic.Token);
      }
      catch (ArgumentException)
      {
        exception = true;
      }
      catch (FileReaderException)
      {
        exception = true;
      }
      catch (Exception)
      {
        Assert.Fail("Wrong Exception Type");
      }

      Assert.IsTrue(exception, "No Exception thrown");
    }

    [TestMethod]
    public async Task CsvDataReaderGetInt16Format()
    {
      var exception = false;
      try
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
          m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
        await test.OpenAsync(UnitTestStatic.Token);
        Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
        Assert.AreEqual(1, test.GetInt16(1));
      }
      catch (FormatException)
      {
        exception = true;
      }
      catch (Exception)
      {
        Assert.Fail("Wrong Exception Type");
      }

      Assert.IsTrue(exception, "No Exception thrown");
    }

    [TestMethod]
    public async Task CsvDataReaderGetInt64()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.AreEqual(1, test.GetInt64(0));
    }

    [TestMethod]
    public async Task CsvDataReaderGetInt64Error()
    {
      var exception = false;
      try
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
          m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
        await test.OpenAsync(UnitTestStatic.Token);
        Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
        Assert.AreEqual(1, test.GetInt64(1));
      }
      catch (FormatException)
      {
        exception = true;
      }
      catch (Exception)
      {
        Assert.Fail("Wrong Exception Type");
      }

      Assert.IsTrue(exception, "No Exception thrown");
    }

    [TestMethod]
    public async Task CsvDataReaderGetChar()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.AreEqual('G', test.GetChar(1));
    }

    [TestMethod]
    public async Task CsvDataReaderGetStringColumnNotExisting()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      var exception = false;
      await test.OpenAsync(UnitTestStatic.Token);
      await test.ReadAsync(UnitTestStatic.Token);
      try
      {
        test.GetString(666);
      }
      catch (IndexOutOfRangeException)
      {
        exception = true;
      }
      catch (ArgumentOutOfRangeException)
      {
        exception = true;
      }
      catch (InvalidOperationException)
      {
        exception = true;
      }
      catch (Exception ex)
      {
        Assert.Fail($"Wrong Exception Type raised was {ex.GetType()}");
      }

      Assert.IsTrue(exception, "No Exception thrown");
    }

    [TestMethod]
    public async Task CsvDataReaderGetString()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.AreEqual("German", test.GetString(1));
      Assert.AreEqual("German", test.GetValue(1));
    }

    public void DataReaderResetPositionToFirstDataRow()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      test.ResetPositionToFirstDataRow();
    }

    [TestMethod]
    public async Task CsvDataReaderIsDBNull()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.IsFalse(test.IsDBNull(4));
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.IsTrue(test.IsDBNull(4));
      test.Close();
    }

    [TestMethod]
    public async Task CsvDataReaderTreatNullTextTrue()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));

      Assert.AreEqual(DBNull.Value, test["LangCodeID"]);
    }

    [TestMethod]
    public async Task CsvDataReaderTreatNullTextFalse()
    {
#pragma warning disable CS8625
      m_ValidSetting.TreatTextAsNull = null;
#pragma warning restore CS8625
      using var test = new CsvFileReader(m_ValidSetting.FullPath, m_ValidSetting.CodePageId, m_ValidSetting.SkipRows,
        m_ValidSetting.HasFieldHeader, m_ValidSetting.ColumnCollection, m_ValidSetting.TrimmingOption,
        m_ValidSetting.FieldDelimiter,
        m_ValidSetting.FieldQualifier, m_ValidSetting.EscapePrefix, m_ValidSetting.RecordLimit,
        m_ValidSetting.AllowRowCombining,
        m_ValidSetting.ContextSensitiveQualifier, m_ValidSetting.CommentLine, m_ValidSetting.NumWarnings,
        m_ValidSetting.DuplicateQualifierToEscape,
        m_ValidSetting.NewLinePlaceholder, m_ValidSetting.DelimiterPlaceholder, m_ValidSetting.QualifierPlaceholder,
        m_ValidSetting.SkipDuplicateHeader,
        m_ValidSetting.TreatLfAsSpace, m_ValidSetting.TreatUnknownCharacterAsSpace,
        m_ValidSetting.TryToSolveMoreColumns,
        m_ValidSetting.WarnDelimiterInValue, m_ValidSetting.WarnLineFeed, m_ValidSetting.WarnNBSP,
        m_ValidSetting.WarnQuotes,
        m_ValidSetting.WarnUnknownCharacter, m_ValidSetting.WarnEmptyTailingColumns, m_ValidSetting.TreatNBSPAsSpace,
#pragma warning disable CS8604
        m_ValidSetting.TreatTextAsNull
#pragma warning restore CS8604
        , m_ValidSetting.SkipEmptyLines, m_ValidSetting.ConsecutiveEmptyRows, m_ValidSetting.IdentifierInContainer,
        m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));

      Assert.AreEqual("NULL", test["LangCodeID"]);
    }

    [TestMethod]
    public async Task CsvDataReaderGetValues()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      var values = new object[test.FieldCount];
      Assert.AreEqual(6, test.GetValues(values));
    }

    [TestMethod]
    public async Task CsvDataReaderGetChars()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      char[] buffer = { '0', '0', '0', '0' };
      test.GetChars(1, 0, buffer, 0, 4);
      Assert.AreEqual('G', buffer[0], "G");
      Assert.AreEqual('e', buffer[1], "E");
      Assert.AreEqual('r', buffer[2], "R");
      Assert.AreEqual('m', buffer[3], "M");
    }

    [TestMethod]
    public async Task CsvDataReaderGetSchemaTable()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      var dt = test.GetSchemaTable();
      Assert.IsInstanceOfType(dt, typeof(DataTable));
      Assert.AreEqual(6, dt.Rows.Count);
    }

    [TestMethod]
    public async Task CsvDataReaderReadAfterEndAsync()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      /*
1,German,20/01/2010,276,0.94,Y
2,English,22/01/2012,190,,N
3,German,,150,0.5,N
4,German,01/04/2010,166,0.678,N
5,NULL,05/03/2001,251,0.92,Y
6,French,13/12/2000,399,0.67,N
7,Dutch,01/11/2001,234,0.89,n
         */
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      Assert.IsFalse(await test.ReadAsync(UnitTestStatic.Token));
      Assert.IsFalse(await test.ReadAsync(UnitTestStatic.Token));
    }

    [TestMethod]
    public async Task CsvDataReaderReadAfterCloseAsync()
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
      test.Close();
      Assert.IsFalse(await test.ReadAsync(UnitTestStatic.Token));
    }

    [TestMethod]
    public async Task GetDataTableAsync_LimitTrack1()
    {
      using var test = new CsvFileReader(m_ValidSetting.FullPath, m_ValidSetting.CodePageId, m_ValidSetting.SkipRows,
        m_ValidSetting.HasFieldHeader,
        m_ValidSetting.ColumnCollection, m_ValidSetting.TrimmingOption, m_ValidSetting.FieldDelimiter,
        m_ValidSetting.FieldQualifier,
        m_ValidSetting.EscapePrefix, 5, m_ValidSetting.AllowRowCombining, m_ValidSetting.ContextSensitiveQualifier,
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);


      using var dt = await test.GetDataTableAsync(TimeSpan.FromSeconds(30), false,
        false, false, false, false, null, UnitTestStatic.Token);
      Assert.AreEqual(5, dt.Rows.Count);
    }

    [TestMethod]
    public async Task GetDataTableAsync_LimitTrack2()
    {

      using var test = new CsvFileReader(m_ValidSetting.FullPath, m_ValidSetting.CodePageId, m_ValidSetting.SkipRows,
        m_ValidSetting.HasFieldHeader,
        m_ValidSetting.ColumnCollection, m_ValidSetting.TrimmingOption, m_ValidSetting.FieldDelimiter,
        m_ValidSetting.FieldQualifier,
        m_ValidSetting.EscapePrefix, 5, m_ValidSetting.AllowRowCombining, m_ValidSetting.ContextSensitiveQualifier,
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
        m_ValidSetting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);

      using var dt = await test.GetDataTableAsync(TimeSpan.FromSeconds(30), true,
        true, false, false, true, null, UnitTestStatic.Token);
      Assert.AreEqual(5, dt.Rows.Count);
    }

    [TestMethod]
    public async Task CsvDataReaderNoHeader()
    {
      var setting = new CsvFile(fileName: UnitTestStatic.GetTestPath("BasicCSV.txt"))
      {
        HasFieldHeader = false, SkipRows = 1, FieldDelimiter = ","
      };

      using var test = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader,
        setting.ColumnCollection,
        setting.TrimmingOption, setting.FieldDelimiter, setting.FieldQualifier, setting.EscapePrefix,
        setting.RecordLimit, setting.AllowRowCombining,
        setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
        setting.NewLinePlaceholder,
        setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
        setting.TreatUnknownCharacterAsSpace,
        setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
        setting.WarnQuotes, setting.WarnUnknownCharacter,
        setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
        setting.ConsecutiveEmptyRows,
        setting.IdentifierInContainer, m_TimeZoneAdjust, TimeZoneInfo.Local.Id);
      await test.OpenAsync(UnitTestStatic.Token);
      Assert.AreEqual("Column1", test.GetName(0));
      Assert.AreEqual("Column2", test.GetName(1));
      Assert.AreEqual("Column3", test.GetName(2));
      Assert.AreEqual("Column4", test.GetName(3));
      Assert.AreEqual("Column5", test.GetName(4));
      Assert.AreEqual("Column6", test.GetName(5));
    }
  }
}