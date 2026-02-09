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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable StringLiteralTypo

namespace CsvTools.Tests;

[TestClass]
[SuppressMessage("ReSharper", "UseAwaitUsing")]
public class CsvFileReaderTest
{
  private static readonly TimeZoneChangeDelegate TimeZoneAdjust = StandardTimeZoneAdjust.ChangeTimeZone;

  [TestMethod]
  public async Task AlternateTextQualifiersDoubleQuotesAsync()
  {
    var setting = new CsvFileDummy()
    {
      HasFieldHeader = false,
      FieldDelimiterChar = ',',
      ContextSensitiveQualifier = true,
      DuplicateQualifierToEscape = true,
    };

    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("AlternateTextQualifiersDoubleQuote.txt"),
      65001, 0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, true);
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("This is a \"Test\" of doubled quoted Text", test.GetString(1),
      " \"\"should be regarded as \"");
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("This is a \"Test\" of not repeated quotes", test.GetString(1), "Training space not trimmed");
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    var result = test.GetString(1).HandleCrlfCombinations();
    Assert.AreEqual("Tricky endig duplicated quotes that close...\nLine \"Test\"", result,
      "Ending with two double quotes but one is a closing quote");
  }

  [TestMethod]
  public async Task AlternateTextQualifiersTrimQuoted()
  {
    var setting = new CsvFileDummy()
    {
      HasFieldHeader = false,
      FieldDelimiterChar = ',',
      ContextSensitiveQualifier = true,
      TrimmingOption = TrimmingOptionEnum.All,
    };

    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("AlternateTextQualifiers.txt"), 65001,
      0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, true);
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("a", test.GetString(0), "Start of file with quote");
    Assert.AreEqual("a", test.GetValue(0), "Start of file with quote");
    Assert.AreEqual("b \"", test.GetString(1), "Containing Quote");
    Assert.AreEqual("d", test.GetString(3), "Leading space not trimmed");
    Assert.AreEqual("This is a\n\" in Quotes", test.GetString(4).HandleCrlfCombinations(),
      "Linefeed in quotes");
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("13", test.GetString(0), "Start of line with quote");
    Assert.AreEqual("This is a \"Test\"", test.GetString(3), "Containing Quote and quote at the end");
    Assert.AreEqual("18", test.GetString(5), "Line ending with quote");

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("19", test.GetString(0), "Training space not trimmed");
    Assert.AreEqual("20,21", test.GetString(1), "Delimiter in quotes");
    Assert.AreEqual("Another\nLine \"Test\"", test.GetString(2).HandleCrlfCombinations(),
      "Linefeed as last char in quotes");
    Assert.AreEqual("22", test.GetString(3), "Leading whitespace before start");
    Assert.AreEqual("24", test.GetString(5), "Leading space not trimmed & EOF");
  }

  [TestMethod]
  public async Task AlternateTextQualifiersTrimUnquoted()
  {
    var setting = new CsvFileDummy()
    {
      HasFieldHeader = false,
      WarnQuotes = true,
      FieldDelimiterChar = ',',
      ContextSensitiveQualifier = true,
      TrimmingOption = TrimmingOptionEnum.Unquoted,
    };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("AlternateTextQualifiers.txt"), 65001,
      0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, true);
    var warningList = new RowErrorCollection();
    await test.OpenAsync(UnitTestStatic.Token);
    test.Warning += warningList.Add;

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("a", test.GetString(0), "Start of file with quote");
    Assert.AreEqual("b \"  ", test.GetString(1), "Containing Quote");
    Assert.IsTrue(warningList.Display.Contains("Field qualifier"));
    Assert.AreEqual("   d", test.GetString(3), "Leading space not trimmed");
    Assert.AreEqual("This is a\n\" in Quotes", test.GetString(4).HandleCrlfCombinations(),
      "Linefeed in quotes");
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("13", test.GetString(0), "Start of line with quote");
    Assert.AreEqual("This is a \"Test\"", test.GetString(3), "Containing Quote and quote at the end");
    Assert.AreEqual("18", test.GetString(5), "Line ending with quote");

    /*
"19  ','20,21','Another
Line "Test"", "22",23,"  24"
*/
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("19  ", test.GetString(0), "Training space not trimmed");
    Assert.AreEqual("20,21", test.GetString(1), "Delimiter in quotes");
    Assert.AreEqual("Another\nLine \"Test\"", test.GetString(2).HandleCrlfCombinations(),
      "Linefeed as last char in quotes");
    Assert.AreEqual("22", test.GetString(3), "Leading whitespace before start");
    Assert.AreEqual("  24", test.GetString(5), "Leading space not trimmed & EOF");
  }

  [TestMethod]
  public async Task BasicEscapedCharacters()
  {
    var setting = new CsvFileDummy()
    {
      HasFieldHeader = false,
      FieldDelimiterChar = ',',
      CommentLine = "#",
      EscapePrefixChar = '\\'
    };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("BasicEscapedCharacters.txt"), 65001,
      0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, true);
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("a\"", test.GetString(0), @"a\""");
    Assert.AreEqual("b", test.GetString(1), "b");
    Assert.AreEqual("c", test.GetString(2), "c");
    Assert.AreEqual("d", test.GetString(3), "d");
    Assert.AreEqual("e", test.GetString(4), "e");
    Assert.AreEqual("f", test.GetString(5), "f");
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(",9", test.GetString(2), @"\,9");
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("\\\\vy", test.GetString(5), @"\\\vy");
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("24\"", test.GetString(5), @"24\""");
    Assert.AreEqual(6, test.FieldCount);
  }

  [TestMethod]
  public async Task ComplexDataDelimiter()
  {
    var setting =
      new CsvFileDummy()
      {
        HasFieldHeader = false,
        FieldDelimiterChar = ',',
        CommentLine = "#",
        EscapePrefixChar = '\\',
        FieldQualifierChar = '"'
      };
    setting.FieldDelimiterChar = ',';
    setting.TrimmingOption = TrimmingOptionEnum.Unquoted;


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("ComplexDataDelimiter.txt"), 65001,
      0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, true);
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(6, test.FieldCount);
    Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token), "ReadAsync");
    Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
    Assert.AreEqual("5\"", test.GetString(4));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token), "ReadAsync2"); // 2
    Assert.AreEqual("a", test.GetString(0));
    Assert.AreEqual("bta", test.GetString(1));
    Assert.AreEqual("c", test.GetString(2));
    Assert.AreEqual("\"d", test.GetString(3), "\"d");
    Assert.AreEqual("e", test.GetString(4));
    Assert.AreEqual("f\n  , ", test.GetString(5).HandleCrlfCombinations());

    Assert.AreEqual(4, test.StartLineNumber, "StartLineNumber");
    Assert.AreEqual(6, test.EndLineNumber, "EndLineNumber");

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token), "ReadAsync3"); // 3
    Assert.AreEqual(6U, test.StartLineNumber, "StartLineNumber");
    Assert.AreEqual("6\"", test.GetString(5));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token), "ReadAsync4"); // 4
    Assert.AreEqual("k\n", test.GetString(4).HandleCrlfCombinations());
    Assert.AreEqual(9, test.StartLineNumber, "StartLineNumber");

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token)); // 5
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token)); // 6
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token)); // 7
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token)); // 8

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token)); // 9
    Assert.AreEqual("19", test.GetString(0));
    Assert.AreEqual("20", test.GetString(1));
    Assert.AreEqual("21", test.GetString(2));
    Assert.AreEqual("2\"2", test.GetString(3));
    Assert.AreEqual("23", test.GetString(4));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token)); // 10
    Assert.AreEqual("f\n", test.GetString(5).HandleCrlfCombinations());
    Assert.AreEqual(23, test.StartLineNumber, "StartLineNumber");
  }

  [TestMethod]
  public async Task ComplexDataDelimiter2()
  {
    var setting = new CsvFileDummy()
    {
      HasFieldHeader = false,
      CommentLine = "#",
      FieldQualifierChar = '"',
      FieldDelimiterChar = ',',
      TrimmingOption = TrimmingOptionEnum.Unquoted,
    };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("QuoteInText.txt"), 65001,
      0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, true);
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(6, test.FieldCount);

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token)); // 1
    Assert.AreEqual("4", test.GetString(3));
    Assert.AreEqual(" 5\"", test.GetString(4));
    Assert.AreEqual("6", test.GetString(5));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token)); // 2
    Assert.AreEqual("21", test.GetString(2));
    Assert.AreEqual("2\"2", test.GetString(3));
    Assert.AreEqual("23", test.GetString(4));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token)); // 3
    Assert.AreEqual("e", test.GetString(4));
    Assert.AreEqual("f\n", test.GetString(5).HandleCrlfCombinations());
  }

  [TestMethod]
  public async Task ComplexDataDelimiterTrimQuotes()
  {
    var setting =
      new CsvFileDummy()
      {
        HasFieldHeader = false,
        ConsecutiveEmptyRows = 5,
        CommentLine = "#",
        EscapePrefixChar = '\\',
        FieldQualifierChar = '"',
        FieldDelimiterChar = ',',
        TrimmingOption = TrimmingOptionEnum.All
      };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("ComplexDataDelimiter.txt"), 65001,
      0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, true);
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(6, test.FieldCount);

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token), "ReadAsync1"); // 1
    Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
    Assert.AreEqual("1", test.GetString(0));
    Assert.AreEqual("5\"", test.GetString(4));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token), "ReadAsync2"); // 2
    Assert.AreEqual("a", test.GetString(0));
    Assert.AreEqual("bta", test.GetString(1));
    Assert.AreEqual("c", test.GetString(2));
    Assert.AreEqual("\"d", test.GetString(3), "\"d");
    Assert.AreEqual("e", test.GetString(4));
    Assert.AreEqual("f\n  ,", test.GetString(5).HandleCrlfCombinations());
    // Line stretches over two line both are fine
    Assert.AreEqual(4, test.StartLineNumber, "LineNumber");

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token), "ReadAsync3"); // 3
    Assert.AreEqual(6U, test.StartLineNumber, "LineNumber");
    Assert.AreEqual("6\"", test.GetString(5));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token)); // 4
    Assert.AreEqual("g", test.GetString(0));
    Assert.AreEqual("k", test.GetString(4));
    Assert.AreEqual(9, test.StartLineNumber, "LineNumber");

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token)); // 5
    Assert.AreEqual("7", test.GetString(0));
    Assert.AreEqual(11, test.StartLineNumber, "LineNumber");

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token)); // 6
    Assert.AreEqual("m", test.GetString(0));
    Assert.AreEqual(12, test.StartLineNumber, "LineNumber");

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token)); // 7
    Assert.AreEqual("13", test.GetString(0));
    Assert.AreEqual(17, test.StartLineNumber, "LineNumber");

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token)); // 8
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token)); // 9
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token)); // 10
    Assert.AreEqual("f", test.GetString(5));
    Assert.AreEqual(23, test.StartLineNumber, "LineNumber");
  }

  [TestMethod]
  public async Task CsvDataReaderCancellationOnOpenAsync()
  {
    var setting = new CsvFileDummy()
    {
      HasFieldHeader = false,
      ContextSensitiveQualifier = true,
      TrimmingOption = TrimmingOptionEnum.All,
    };
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(UnitTestStatic.Token);

    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("AlternateTextQualifiers.txt"), 65001,
      0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, true);
    await test.OpenAsync(cts.Token);
    cts.Cancel();
    Assert.IsFalse(await test.ReadAsync(cts.Token));
  }

  [TestMethod]
  public async Task CSVTestEmpty()
  {
    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("CSVTestEmpty.txt"), 65001,
      0, 0, false);
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(0, test.FieldCount);
    Assert.IsFalse(await test.ReadAsync(UnitTestStatic.Token));
  }

  [TestMethod]
  public async Task DifferentColumnDelimiter()
  {
    var setting = new CsvFileDummy() { HasFieldHeader = false, FieldDelimiterChar = "PIPE".FromText(), };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("DifferentColumnDelimiter.txt"), 65001,
      0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, true);
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(6, test.FieldCount);
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
    Assert.AreEqual("a", test.GetString(0));
    Assert.AreEqual("f", test.GetString(5));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("19", test.GetString(0));
    Assert.AreEqual("24", test.GetString(5));
    Assert.AreEqual(8U, test.StartLineNumber, "LineNumber");
    Assert.IsFalse(await test.ReadAsync(UnitTestStatic.Token));
  }

  [TestMethod]
  public async Task EscapedCharacterAtEndOfFile()
  {
    var setting = new CsvFileDummy() { HasFieldHeader = false, FieldDelimiterChar = ',', EscapePrefixChar = '\\' };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("EscapedCharacterAtEndOfFile.txt"),
      65001, 0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, true);
    await test.OpenAsync(UnitTestStatic.Token);

    Assert.AreEqual(6, test.FieldCount);
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("a\"", test.GetString(0), @"a\""");
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(",9", test.GetString(2), @"\,9");
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("\\\\vy", test.GetString(5), @"\\\vy");
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("24\\", test.GetString(5), @"24\");
  }

  [TestMethod]
  public async Task EscapedCharacterAtEndOfRowDelimiter()
  {
    var setting = new CsvFileDummy()
    {
      HasFieldHeader = false,
      TrimmingOption = TrimmingOptionEnum.None,
      TreatLfAsSpace = false,
      FieldDelimiterChar = ',',
      EscapePrefixChar = '\\'
    };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("EscapedCharacterAtEndOfRowDelimiter.txt"),
      65001, 0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, false);
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(6, test.FieldCount, "FieldCount");
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("l\\", test.GetString(5)); // Important to not trim the value otherwise the linefeed is gone
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("7", test.GetString(0), @"Next Row");
    Assert.AreEqual(4U, test.StartLineNumber, "StartLineNumber");
    Assert.AreEqual(4U, test.RecordNumber, "RecordNumber");
  }

  [TestMethod]
  public async Task EscapedCharacterAtEndOfRowDelimiterNoEscape()
  {
    var setting =
      new CsvFileDummy() { HasFieldHeader = false, FieldDelimiterChar = ',', EscapePrefixChar = "".FromText() };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("EscapedCharacterAtEndOfRowDelimiter.txt"),
      65001, 0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, true);
    await test.OpenAsync(UnitTestStatic.Token);

    Assert.AreEqual(6, test.FieldCount);
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(@"l\", test.GetString(5), @"l\");
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("7", test.GetString(0), @"Next Row");
    Assert.AreEqual(4U, test.StartLineNumber);
    Assert.AreEqual(4U, test.RecordNumber);
  }

  [TestMethod]
  public async Task EscapeWithoutTextQualifier()
  {
    var setting = new CsvFileDummy()
    {
      HasFieldHeader = false,
      FieldDelimiterChar = ',',
      EscapePrefixChar = '\\',
      FieldQualifierChar = string.Empty.FromText()
    };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("EscapeWithoutTextQualifier.txt"),
      65001, 0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, true);
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(6, test.FieldCount);
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(@"a\", test.GetString(0), @"a\\");
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(",9", test.GetString(2), @"\,9");
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("\\\\vy", test.GetString(5), @"\\\\vy");
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("24\\", test.GetString(5), @"24\\");
  }

  [TestMethod]
  public async Task GetValue()
  {
    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("BasicCSV.txt"), 65001,
      0, 0, true,
      new[] { new Column("ExamDate",
          new ValueFormat(DataTypeEnum.DateTime, @"dd/MM/yyyy")),
        new Column("ID", new ValueFormat(DataTypeEnum.Integer)),
        new Column("IsNativeLang", new ValueFormat(DataTypeEnum.Boolean))
      },
TrimmingOptionEnum.Unquoted, ',');
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(6, test.FieldCount);
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(new DateTime(2010, 1, 20), test.GetValue(2));
    Assert.AreEqual(true, test.GetValue(5));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));

    Assert.AreEqual(Convert.ToInt32(2), Convert.ToInt32(test.GetValue(0), CultureInfo.InvariantCulture));
    Assert.AreEqual("English", test.GetString(1));
    Assert.AreEqual("22/01/2012", test.GetString(2));
    Assert.AreEqual(new DateTime(2012, 1, 22), test.GetValue(2));
    Assert.AreEqual(false, test.GetValue(5));
  }

  [TestMethod]
  public async Task HandlingDuplicateColumnNames()
  {
    var setting = new CsvFileDummy() { HasFieldHeader = true, FieldDelimiterChar = ',' };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("HandlingDuplicateColumnNames.txt"),
      65001, 0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, true);
    var message = string.Empty;
    test.Warning += delegate (object? _, WarningEventArgs args) { message = args.Message; };
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.IsTrue(message.Contains("exists more than once"));
  }

  [TestMethod]
  public void InvalidCtor()
  {
    try
    {
      using var reader = new CsvFileReader(string.Empty, 65001,
        0,
        0, true,
        null,
        TrimmingOptionEnum.Unquoted,
        '\t',
        '"',
        char.MinValue,
        0,
        false,
        false,
        "",
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
        consecutiveEmptyRowsMax: 4,
        identifierInContainer: String.Empty,
timeZoneAdjust: StandardTimeZoneAdjust.ChangeTimeZone, destinationTimeZone: TimeZoneInfo.Local.Id, allowPercentage: true, removeCurrency: true);
    }
    catch (ArgumentException)
    {
    }
    catch (Exception ex)
    {
      Assert.Fail("Wrong Exception Type: " + ex.GetType());
    }

    try
    {
#pragma warning disable CS8625
      // ReSharper disable once RedundantCast
      using (new CsvFileReader((string?) null, 65001,
               0,
               0, true,
               null,
               TrimmingOptionEnum.Unquoted,
               '\t',
               '"',
               char.MinValue,
               0,
               false,
               false,
               "",
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
               null,
StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, true, false))
#pragma warning restore CS8625
      {
      }
    }
    catch (ArgumentNullException) { }
    catch (ArgumentException) { }
    catch (Exception ex)
    {
      Assert.Fail("Wrong Exception Type: " + ex.GetType());
    }

    try
    {
      using (new CsvFileReader("(string) null.txt", 65001,
               0,
               0, true,
               null,
               TrimmingOptionEnum.Unquoted,
               '\t',
               '"',
               char.MinValue,
               0,
               false,
               false,
               "",
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
               skipEmptyLines: true,
               consecutiveEmptyRowsMax: 4,
               identifierInContainer: String.Empty,
timeZoneAdjust: StandardTimeZoneAdjust.ChangeTimeZone, destinationTimeZone: TimeZoneInfo.Local.Id, allowPercentage: true,
               removeCurrency: true))
      {
      }
    }
    catch (FileNotFoundException)
    {
    }
    catch (Exception ex)
    {
      Assert.Fail("Wrong Exception Type: " + ex.GetType());
    }

    try
    {
#pragma warning disable CS8600
#pragma warning disable CS8625
      // ReSharper disable once RedundantCast
      using (new CsvFileReader((Stream) null, 65001,
               0,
               0, true,
               null,
               TrimmingOptionEnum.Unquoted,
               '\t',
               '"',
               char.MinValue,
               0,
               false,
               false,
               "",
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
StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, true, false))
#pragma warning restore CS8625
#pragma warning restore CS8600
      {
      }
    }
    catch (ArgumentNullException)
    {
    }
    catch (Exception ex)
    {
      Assert.Fail("Wrong Exception Type: " + ex.GetType());
    }
  }

  [TestMethod]
  public async Task LastRowWithRowDelimiter()
  {
    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("LastRowWithRowDelimiter.txt"), 65001,
      0, 0, true, Array.Empty<Column>(), TrimmingOptionEnum.Unquoted
      , ',');
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(6, test.FieldCount, "FieldCount");

    Assert.AreEqual("a", test.GetName(0));
    Assert.AreEqual("f", test.GetName(5));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(2U, test.StartLineNumber, "LineNumber");
    Assert.AreEqual("1", test.GetString(0));
    Assert.AreEqual("6", test.GetString(5));
  }

  [TestMethod]
  public async Task LastRowWithRowDelimiterNoHeader()
  {
    var setting = new CsvFileDummy() { HasFieldHeader = false, FieldDelimiterChar = ',' };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("LastRowWithRowDelimiter.txt"), 65001,
      0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, false);
    await test.OpenAsync(UnitTestStatic.Token);

    Assert.AreEqual(6, test.FieldCount);

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
    Assert.AreEqual("a", test.GetString(0));
    Assert.AreEqual("f", test.GetString(5));
  }

  [TestMethod]
  public async Task LongHeaders()
  {
    var setting = new CsvFileDummy() { HasFieldHeader = true, FieldDelimiterChar = ',', CommentLine = "#" };

    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("LongHeaders.txt"), 65001,
      0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, false);
    var warningsList = new RowErrorCollection();
    test.Warning += warningsList.Add;
    await test.OpenAsync(UnitTestStatic.Token);

    Assert.AreEqual(6, test.FieldCount);
    Assert.AreEqual("a", test.GetName(0));
    Assert.AreEqual("b", test.GetName(1));
    Assert.AreEqual(
      "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquya"
        .Substring(0, 128), test.GetName(2));
    Assert.AreEqual("d", test.GetName(3));
    Assert.AreEqual("e", test.GetName(4));
    Assert.AreEqual("f", test.GetName(5));

#if DEBUG
    // This works in debug mode but in release it does raise an error
    Assert.AreEqual(1, warningsList.CountRows, "Number of Warnings");
    Assert.IsTrue(warningsList.Display.Contains("too long"));
#endif

    // check if we read the right line , and we do not end up in a commented line of read the
    // header again
    await test.ReadAsync(UnitTestStatic.Token);
    Assert.AreEqual("1", test.GetString(0));
    Assert.AreEqual("6", test.GetString(5));
  }

  [TestMethod]
  public async Task MoreColumnsThanHeaders()
  {
    var setting = new CsvFileDummy()
    {
      HasFieldHeader = false,
      WarnEmptyTailingColumns = true,
      FieldDelimiterChar = ','
    };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("MoreColumnsThanHeaders.txt"), 65001,
      0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, false);
    var warningList = new RowErrorCollection();
    await test.OpenAsync(UnitTestStatic.Token);
    test.Warning += warningList.Add;

    Assert.AreEqual(6, test.FieldCount);
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
    Assert.AreEqual("a", test.GetString(0));
    Assert.AreEqual("f", test.GetString(5));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(0, warningList.CountRows);

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    // Still only 6 fields
    Assert.AreEqual(6, test.FieldCount);

    Assert.AreEqual(1, warningList.CountRows, "warningList.CountRows");
    Assert.IsTrue(warningList.Display.Contains(CsvFileReader.cMoreColumns));
    // Assert.IsTrue(warningList.Display.Contains("The existing data in these extra columns is not read"));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));

    // Row 3 and Row 4 have extra columns
    Assert.AreEqual(2, warningList.CountRows, "warningList.CountRows");
  }

  [TestMethod]
  public async Task NextResult()
  {
    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("Sessions.txt"),
      codePageId: 65001,
      skipRows: 0,
      skipRowsAfterHeader: 0, hasFieldHeader: true,
      columnDefinition: new[]
      {
        new Column("Start Date", new ValueFormat(DataTypeEnum.DateTime),
          timePart: "Start Time", timePartFormat: "HH:mm:ss")
      },
      trimmingOption: TrimmingOptionEnum.Unquoted,
      fieldDelimiterChar: '\t',
      fieldQualifierChar: '"',
      escapeCharacterChar: char.MinValue,
      recordLimit: 0,
      allowRowCombining: false,
      contextSensitiveQualifier: false,
      commentLine: "",
      numWarning: 0,
      duplicateQualifierToEscape: true,
      newLinePlaceholder: "",
      delimiterPlaceholder: "",
      quotePlaceholder: "",
      skipDuplicateHeader: true,
      treatLinefeedAsSpace: false,
      treatUnknownCharacterAsSpace: false,
      tryToSolveMoreColumns: true,
      warnDelimiterInValue: true,
      warnLineFeed: false,
      warnNbsp: true,
      warnQuotes: true,
      warnUnknownCharacter: true,
      warnEmptyTailingColumns: true,
      treatNbspAsSpace: false,
      treatTextAsNull: "NULL",
      skipEmptyLines: true,
      consecutiveEmptyRowsMax: 4,
identifierInContainer: "", timeZoneAdjust: StandardTimeZoneAdjust.ChangeTimeZone,
destinationTimeZone: TimeZoneInfo.Local.Id, allowPercentage: true, removeCurrency: true);
    Assert.IsFalse(test.NextResult());
    Assert.IsFalse(await test.NextResultAsync());

    await test.OpenAsync(UnitTestStatic.Token);

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
  }

  [TestMethod]
  public async Task NoFieldQualifier()
  {
    var setting = new CsvFileDummy()
    {
      HasFieldHeader = false,
      FieldDelimiterChar = ',',
      FieldQualifierChar = string.Empty.FromText()
    };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("TextQualifierDataPastClosingQuote.txt"),
      65001, 0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, false);
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(6, test.FieldCount);
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token), "ReadAsync1");
    //"a"a,b,c,d,e,f
    Assert.AreEqual("\"a\"a", test.GetString(0));
    Assert.AreEqual("b", test.GetString(1));
    Assert.AreEqual("c", test.GetString(2));
    Assert.AreEqual("d", test.GetString(3));
    Assert.AreEqual("e", test.GetString(4));
    Assert.AreEqual("f", test.GetString(5));
    //1,2,"3" ignore me,4,5,6
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token), "ReadAsync2");
    Assert.AreEqual("1", test.GetString(0));
    Assert.AreEqual("2", test.GetString(1));
    Assert.AreEqual("\"3\" ignore me", test.GetString(2));
    Assert.AreEqual("4", test.GetString(3));
    Assert.AreEqual("5", test.GetString(4));
    Assert.AreEqual("6", test.GetString(5));
    Assert.IsFalse(await test.ReadAsync(UnitTestStatic.Token), "ReadAsync3");
  }

  [TestMethod]
  public async Task OpenByParams()
  {
    using var reader = new CsvFileReader(
      UnitTestStatic.GetTestPath("AllFormats.txt"), Encoding.UTF8.CodePage, 0,
      0, true,
new Column[]
      {
        new Column("DateTime", new ValueFormat(DataTypeEnum.DateTime), 0, true, true),
        new Column("Integer", new ValueFormat(DataTypeEnum.Integer), 0, true, true)
      }, TrimmingOptionEnum.All,
      '\t',
      '"',
      char.MinValue,
      0,
      false,
      false,
      "",
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
      treatTextAsNull: "NULL",
      skipEmptyLines: true,
      consecutiveEmptyRowsMax: 4,
      identifierInContainer: String.Empty,
timeZoneAdjust: StandardTimeZoneAdjust.ChangeTimeZone, destinationTimeZone: TimeZoneInfo.Local.Id, allowPercentage: true, removeCurrency: true);
    await reader.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(false, reader.IsClosed, "IsClosed");
    Assert.AreEqual(1, reader.Percent, "Percent");
    Assert.AreEqual(10 - 2, reader.VisibleFieldCount);
#pragma warning disable CS0618
    Assert.IsTrue(reader.Read());
#pragma warning restore CS0618

    Assert.AreEqual(true, reader.HasRows);
    while (await reader.ReadAsync(UnitTestStatic.Token))
    {
    }

    Assert.AreEqual(100, reader.Percent);
  }

  [TestMethod]
  public async Task OpenBySetting()
  {
    var setting =
      new CsvFileDummy() { HasFieldHeader = true, FieldDelimiterChar = "Tab".FromText() };
    new ColumnCollection().Add(new Column("DateTime", new ValueFormat(DataTypeEnum.DateTime)));
    new ColumnCollection().Add(new Column("Integer", new ValueFormat(DataTypeEnum.Integer)));

    using var reader = new CsvFileReader(UnitTestStatic.GetTestPath("AllFormats.txt"), 65001,
      0,
      0, setting.HasFieldHeader, new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, false);
    await reader.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(false, reader.IsClosed);

    Assert.AreEqual(10, reader.VisibleFieldCount);
#pragma warning disable CS0618
    Assert.IsTrue(reader.Read());
#pragma warning restore CS0618
    Assert.IsTrue(reader.IsDBNull(0));
    Assert.IsFalse(await reader.IsDBNullAsync(1));
    Assert.AreEqual(-22477, reader.GetInt32(1));
    Assert.AreEqual("-22477", reader.GetString(1));
    Assert.AreEqual(6, reader.GetStream(1).Length);
    Assert.AreEqual("-22477", await reader.GetTextReader(1).ReadToEndAsync());

    Assert.IsTrue(await reader.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(true, reader.HasRows);
    // Read all
    var readEnumerator = reader.GetEnumerator();
    Assert.IsNotNull(readEnumerator);
    while (readEnumerator.MoveNext()) Assert.IsNotNull(readEnumerator.Current);
  }

  [TestMethod]
  public async Task OpenByStream()
  {
    using var stream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("AllFormats.txt")));
    using var reader = new CsvFileReader(stream, Encoding.UTF8.CodePage, 0, 0, true,
new Column[]
      {
        new Column("DateTime", new ValueFormat(DataTypeEnum.DateTime), 0, true, true),
        new Column("Integer", new ValueFormat(DataTypeEnum.Integer), 0, true)
      }, TrimmingOptionEnum.All, '\t',
      '"',
      char.MinValue,
      0,
      false,
      false,
      "",
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
StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, true, false);
    await reader.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(false, reader.IsClosed);
    Assert.AreEqual(1, reader.Percent);
    Assert.AreEqual(10 - 2, reader.VisibleFieldCount);
#pragma warning disable CS0618
    Assert.IsTrue(reader.Read());
#pragma warning restore CS0618

    Assert.AreEqual(true, reader.HasRows);
    while (await reader.ReadAsync(UnitTestStatic.Token))
    {
    }

    Assert.AreEqual(100, reader.Percent);
  }

  [TestMethod]
  public async Task PlaceHolderTest()
  {
    var setting = new CsvFileDummy()
    {
      HasFieldHeader = true,
      FieldDelimiterChar = ',',
      EscapePrefixChar = char.MinValue,
      DelimiterPlaceholder = @"<\d>",
      QualifierPlaceholder = @"<\q>",
      NewLinePlaceholder = @"<\r>"
    };

    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("Placeholder.txt"), 65001,
      0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, false);
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));

    Assert.AreEqual("A \nLine\nBreak", test.GetString(1));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("Two ,Delimiter,", test.GetString(1));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("Two \"Quote\"", test.GetString(1));
  }

  [TestMethod]
  public async Task ProgressUpdateShowProgress()
  {
    var setting = new CsvFileDummy() { HasFieldHeader = true, FieldDelimiterChar = '\t', };
    // columns from the file
    new ColumnCollection().AddRange(
      new Column[]
      {
        new Column("DateTime", new ValueFormat(dataType: DataTypeEnum.DateTime, dateFormat: @"dd/MM/yyyy"),
          timePart: "Time", timePartFormat: "HH:mm:ss"),
        new Column("Integer", new ValueFormat(DataTypeEnum.Integer)),
        new Column("Numeric", new ValueFormat(DataTypeEnum.Numeric, decimalSeparator: ".")),
        new Column("Double", new ValueFormat(dataType: DataTypeEnum.Double, decimalSeparator: ".")),
        new Column("Boolean", new ValueFormat(DataTypeEnum.Boolean)),
        new Column("GUID", new ValueFormat(DataTypeEnum.Guid)),
        new Column("Time", new ValueFormat(dataType: DataTypeEnum.DateTime, dateFormat: "HH:mm:ss"), ignore: true)
      });

    var progress = new MockProgress();
    var stopped = false;
    progress.ProgressStopEvent += delegate { stopped = true; };
    Assert.IsTrue(string.IsNullOrEmpty(progress.Text));
    using (var test = new CsvFileReader(UnitTestStatic.GetTestPath("AllFormats.txt"), 65001,
             0,
             0, setting.HasFieldHeader, new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings,
setting.DuplicateQualifierToEscape, setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader,
             setting.TreatLfAsSpace,
             setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
             setting.WarnQuotes,
             setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull,
setting.SkipEmptyLines, setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, true))
    {
      await test.OpenAsync(UnitTestStatic.Token);

      for (var i = 0; i < 500; i++)
        Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    }

    Assert.IsNotNull(progress.Text);
    Assert.IsFalse(stopped);
  }

  [TestMethod]
  public async Task ReadDateWithTimeAndTimeZoneAsync()
  {
    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("Sessions.txt"), 65001, 0, 0, true,
new[] { new Column("Start Date", new ValueFormat(DataTypeEnum.DateTime), timePart: "Start Time", timePartFormat: "HH:mm:ss", timeZonePart: "Time Zone") }, TrimmingOptionEnum.Unquoted, '\t');
    await test.OpenAsync(UnitTestStatic.Token);
    await test.ReadAsync(UnitTestStatic.Token);
    var cultureInfo = new CultureInfo("en-US");

#if Windows
      // 01/08/2013 07:00:00 IST --> 01/08/2013 01:30:00 UTC
      Assert.AreEqual("01/08/2013 01:30:00",
        test.GetDateTime(0).ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss", cultureInfo), "01/08/2013 07:00:00 IST --> 01/08/2013 01:30:00 UTC");
#endif
    await test.ReadAsync(UnitTestStatic.Token);
    // 01/19/2010 24:00:00 MST --> 01/20/2010 00:00:00 MST --> 01/20/2010 07:00:00 UTC
    Assert.AreEqual("01/20/2010 07:00:00",
      test.GetDateTime(0).ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss", cultureInfo));
    await test.ReadAsync(UnitTestStatic.Token);
    // 01/21/2013 25:00:00 GMT --> 01/22/2013 01:00:00 GMT -->01/22/2013 01:00:00 UTC
    Assert.AreEqual("01/22/2013 01:00:00",
      test.GetDateTime(0).ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss", cultureInfo));
  }

  [TestMethod]
  public async Task ReadDateWithTimeAsync()
  {
    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("Sessions.txt"), 65001,
      0, 0, true,
new[] {new Column("Start Date", new ValueFormat(DataTypeEnum.DateTime),
        timePart: "Start Time", timePartFormat: "HH:mm:ss")}, TrimmingOptionEnum.Unquoted, '\t');
    await test.OpenAsync(UnitTestStatic.Token);
    await test.ReadAsync(UnitTestStatic.Token);
    var cultureInfo = new CultureInfo("en-US");
    Assert.AreEqual("01/08/2013 07:00:00", test.GetDateTime(0).ToString("MM/dd/yyyy HH:mm:ss", cultureInfo));
    await test.ReadAsync(UnitTestStatic.Token);
    // 01/19/2010 24:00:00 --> 01/20/2010 00:00:00
    Assert.AreEqual("01/20/2010 00:00:00", test.GetDateTime(0).ToString("MM/dd/yyyy HH:mm:ss", cultureInfo));
    await test.ReadAsync(UnitTestStatic.Token);
    // 01/21/2013 25:00:00 --> 01/22/2013 01:00:00
    Assert.AreEqual("01/22/2013 01:00:00", test.GetDateTime(0).ToString("MM/dd/yyyy HH:mm:ss", cultureInfo));
  }

  [TestMethod]
  public async Task ReadingInHeaderAfterComments()
  {
    var setting = new CsvFileDummy() { HasFieldHeader = true, CommentLine = "#" };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("ReadingInHeaderAfterComments.txt"),
      65001, 0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, false);
    await test.OpenAsync(UnitTestStatic.Token);

    Assert.AreEqual(6, test.FieldCount);
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(4U, test.StartLineNumber, "LineNumber");
    Assert.AreEqual("1", test.GetString(0));
    Assert.AreEqual("6", test.GetString(5));
  }

  [TestMethod]
  public async Task RowWithoutColumnDelimiter()
  {
    var setting = new CsvFileDummy() { HasFieldHeader = true, FieldDelimiterChar = ',' };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("RowWithoutColumnDelimiter.txt"),
      65001, 0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, false);
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(1, test.FieldCount);
    Assert.AreEqual("abcdef", test.GetName(0));
    Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(2U, test.StartLineNumber, "LineNumber");
    Assert.AreEqual("123456", test.GetString(0));
  }

  [TestMethod]
  public async Task ShouldRetry()
  {
    var called = false;
    var fn = UnitTestStatic.GetTestPath("txTrans___cripts.txt");
    FileSystemUtils.FileDelete(fn);
    try
    {
#pragma warning disable IDE0063
      // ReSharper disable once ConvertToUsingDeclaration
      using (var stream = new FileStream(fn.LongPathPrefix(), FileMode.CreateNew, FileAccess.Write, FileShare.None))
#pragma warning restore IDE0063
      {
        using var reader = new CsvFileReader(stream, Encoding.UTF8.CodePage,
          0,
          0, true,
          null,
          TrimmingOptionEnum.Unquoted,
          '\t',
          '"',
          char.MinValue,
          0,
          false,
          false,
          "",
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
StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, true, false);
        // lock file for reading
        reader.OnAskRetry += (_, args) =>
        {
          // ReSharper disable once AccessToDisposedClosure
          stream.Close();
          called = true;
          args.Retry = false;
        };
        await reader.OpenAsync(UnitTestStatic.Token);
      }
    }
    catch (FileReaderException)
    {
      // all good
    }

    FileSystemUtils.FileDelete(fn);
    Assert.IsTrue(called);
  }

  [TestMethod]
  public async Task SimpleDelimiterWithControlCharacters()
  {
    var setting = new CsvFileDummy()
    {
      HasFieldHeader = true,
      FieldDelimiterChar = ',',
      ContextSensitiveQualifier = false,
      CommentLine = "#",
      WarnNBSP = true,
      WarnUnknownCharacter = true
    };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("SimpleDelimiterWithControlCharacters.txt"),
      65001, 0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, false);
    var warningList = new RowErrorCollection();
    await test.OpenAsync(UnitTestStatic.Token);
    test.Warning += warningList.Add;
    Assert.AreEqual(6, test.FieldCount);
    Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("a", test.GetString(0));
    Assert.AreEqual("b", test.GetString(1));
    Assert.AreEqual("c", test.GetString(2));
    Assert.AreEqual("d", test.GetString(3));
    Assert.AreEqual(2U, test.StartLineNumber, "LineNumber");
    // NBSP and � past " so not in the field
    Assert.IsFalse(warningList.Display.Contains("Non Breaking Space"));
    Assert.IsFalse(warningList.Display.Contains("Unknown Character"));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(3U, test.StartLineNumber, "LineNumber");
    // g,h,i , j,k,l
    Assert.AreEqual("i", test.GetString(2));
    Assert.AreEqual("j", test.GetString(3));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("7", test.GetString(0));
    // 10 has a training NBSp;
    Assert.AreEqual("10", test.GetString(3));
    Assert.AreEqual(1, warningList.CountRows);
    Assert.IsTrue(warningList.Display.Contains("Non Breaking Space"));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    // m,n,o,p ,q,r
    Assert.AreEqual("p", test.GetString(3));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("15�", test.GetString(2));
    Assert.IsTrue(warningList.Display.Contains("Unknown Character"));

    test.Warning -= warningList.Add;
  }

  [TestMethod]
  public async Task SimpleDelimiterWithControlCharactersUnknownCharReplacement()
  {
    var setting = new CsvFileDummy()
    {
      HasFieldHeader = true,
      TreatUnknownCharacterAsSpace = true,
      TreatNBSPAsSpace = true,
      WarnNBSP = true,
      WarnUnknownCharacter = true,
      ContextSensitiveQualifier = false,
      TrimmingOption = TrimmingOptionEnum.None,
      FieldDelimiterChar = ',',
      CommentLine = "#"
    };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("SimpleDelimiterWithControlCharacters.txt"),
      65001, 0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, false);
    var warningList = new RowErrorCollection();
    test.Warning += warningList.Add;
    await test.OpenAsync(UnitTestStatic.Token);
      
    Assert.AreEqual(6, test.FieldCount);
    Assert.AreEqual("1", test.GetName(0));
    Assert.AreEqual("2", test.GetName(1));
    Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(3U, test.StartLineNumber, "LineNumber");
    // g,h,i , j,k,l
    Assert.AreEqual(" j", test.GetString(3));

    // #A NBSP: Create with Alt+01602 7,8,9,10 ,11,12
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("7", test.GetString(0));
    Assert.AreEqual("10 ", test.GetString(3));
    Assert.AreEqual(2, warningList.CountRows);
    Assert.IsTrue(warningList.Display.Contains("Non Breaking Space"));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    // m,n,o,p ,q,r
    Assert.AreEqual("p						", test.GetString(3));

    // 13,14,15�,16,17,18
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("15 ", test.GetString(2));
    Assert.AreEqual(3, warningList.CountRows);
    Assert.IsTrue(warningList.Display.Contains("Unknown Character"));
    test.Warning -= warningList.Add;
  }

  [TestMethod]
  public async Task SkipAllRows()
  {
    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("BasicCSV.txt"), 65001,
      100, 0, false);
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(0, test.FieldCount);
  }

  [TestMethod]
  public async Task SkippingComments()
  {
    var setting = new CsvFileDummy() { HasFieldHeader = false, FieldDelimiterChar = ',', CommentLine = "#" };

    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("SkippingComments.txt"), 65001,
      0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, false);
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(6, test.FieldCount);
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(2U, test.StartLineNumber, "LineNumber");
    Assert.AreEqual(1U, test.RecordNumber, "RecordNumber");
    Assert.AreEqual("a", test.GetString(0));
    Assert.AreEqual("f", test.GetString(5));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(2U, test.RecordNumber, "RecordNumber");
    Assert.AreEqual(4U, test.StartLineNumber, "LineNumber");

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(3U, test.RecordNumber, "RecordNumber");
    Assert.AreEqual(5U, test.StartLineNumber, "LineNumber");

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(4U, test.RecordNumber, "RecordNumber");
    Assert.AreEqual(6U, test.StartLineNumber, "LineNumber");

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(5U, test.RecordNumber, "RecordNumber");
    Assert.AreEqual(7U, test.StartLineNumber, "LineNumber");

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(6U, test.RecordNumber, "RecordNumber");
    Assert.AreEqual(10U, test.StartLineNumber, "LineNumber");

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(7U, test.RecordNumber, "RecordNumber");
    Assert.AreEqual(11U, test.StartLineNumber, "LineNumber");

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(8U, test.RecordNumber, "RecordNumber");
    Assert.AreEqual(13U, test.StartLineNumber, "LineNumber");

    Assert.IsFalse(await test.ReadAsync(UnitTestStatic.Token));
  }

  [TestMethod]
  public async Task SkippingEmptyRowsWithDelimiter()
  {
    var setting = new CsvFileDummy();

    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("SkippingEmptyRowsWithDelimiter.txt"),
      65001, 0, 0, false,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, false);
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(6, test.FieldCount, "FieldCount");
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(3U, test.StartLineNumber, "LineNumber");
    Assert.AreEqual("a", test.GetString(0));
    Assert.AreEqual("b", test.GetString(1));
    Assert.AreEqual("f", test.GetString(5));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(14U, test.StartLineNumber, "LineNumber");
    Assert.AreEqual("19", test.GetString(0));
    // No next Line
    Assert.IsFalse(await test.ReadAsync(UnitTestStatic.Token));
  }

  [TestMethod]
  public async Task SkipRows()
  {
    var setting = new CsvFileDummy() { HasFieldHeader = false, FieldDelimiterChar = ',', SkipRows = 2 };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("BasicCSV.txt"), 65001,
      2, 0, false,
      new ColumnCollection(),
setting.TrimmingOption, ',');
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(6, test.FieldCount);
    // Start at line 2
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(3U, test.StartLineNumber, "LineNumber");
    Assert.AreEqual(1U, test.RecordNumber, "RecordNumber");

    Assert.AreEqual("English", test.GetString(1));
    Assert.AreEqual("22/01/2012", test.GetString(2));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(4U, test.StartLineNumber, "LineNumber");
    Assert.AreEqual(2U, test.RecordNumber, "RecordNumber");

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(5U, test.StartLineNumber, "LineNumber");
    Assert.AreEqual(3U, test.RecordNumber, "RecordNumber");

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(6U, test.StartLineNumber, "LineNumber");
    Assert.AreEqual(4U, test.RecordNumber, "RecordNumber");
  }

  [TestMethod]
  public async Task TestConsecutiveEmptyRows2()
  {
    var setting = new CsvFileDummy()
    {
      HasFieldHeader = true,
      ConsecutiveEmptyRows = 2,
      SkipEmptyLines = false,
      FieldDelimiterChar = ','
    };

    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("BasicCSVEmptyLine.txt"), 65001,
      0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, false);
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(6, test.FieldCount);
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token), "Read() 1");

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token), "Read() 2");
    Assert.AreEqual("20/01/2010", test.GetString(2));
    // First empty Row, continue
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token), "Read() 3");
    // Second Empty Row, Stop
    Assert.IsFalse(await test.ReadAsync(UnitTestStatic.Token), "Read() 4");
  }

  [TestMethod]
  public async Task TestConsecutiveEmptyRows3()
  {
    var setting = new CsvFileDummy()
    {
      HasFieldHeader = true,
      ConsecutiveEmptyRows = 3,
      SkipEmptyLines = false,
      FieldDelimiterChar = ','
    };

    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("BasicCSVEmptyLine.txt"), 65001,
      0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, false);
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(6, test.FieldCount);
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("20/01/2010", test.GetString(2));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("22/01/2012", test.GetString(2));
    // No other Line read they are all empty
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsFalse(await test.ReadAsync(UnitTestStatic.Token));
  }

  [TestMethod]
  public async Task TestHighMultipleQuestionMark()
  {
    var setting = new CsvFileDummy()
    {
      HasFieldHeader = false,
      WarnLineFeed = true,
      WarnUnknownCharacter = true,
      CommentLine = "#",
      EscapePrefixChar = '\\',
      FieldQualifierChar = '"',
      FieldDelimiterChar = ',',
      TrimmingOption = TrimmingOptionEnum.Unquoted
    };

    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("ComplexDataDelimiter.txt"), 65001,
      0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, false);
    await test.OpenAsync(UnitTestStatic.Token);
    await test.ReadAsync(UnitTestStatic.Token);
    await test.ReadAsync(UnitTestStatic.Token);
    var message = string.Empty;
    test.Warning += delegate (object? _, WarningEventArgs args)
    {
      message = args.Message;
    };
    await test.ReadAsync(UnitTestStatic.Token);
    Assert.IsTrue(message.Contains("occurrence") && message.Contains("?"));
  }

  [TestMethod]
  public async Task TestStartRowAndFooter()
  {
    using var improvedStream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("LateStartRow.txt")));
    string escapePrefix = string.Empty;
    using var textReader =
      await improvedStream.GetTextReaderAsync(20127, 0, UnitTestStatic.Token).ConfigureAwait(false);
    Assert.AreEqual(10, await textReader.InspectStartRowAsync('|', '"', (escapePrefix).FromText(), "#", UnitTestStatic.Token));
  }

  [TestMethod]
  public async Task TestWarnLinefeed()
  {
    var setting = new CsvFileDummy()
    {
      HasFieldHeader = false,
      WarnLineFeed = true,
      CommentLine = "#",
      EscapePrefixChar = '\\',
      FieldQualifierChar = '"',
      FieldDelimiterChar = ',',
      TrimmingOption = TrimmingOptionEnum.Unquoted
    };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("ComplexDataDelimiter.txt"), 65001,
      0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, false);
    await test.OpenAsync(UnitTestStatic.Token);
    var message = string.Empty;
    test.Warning += delegate (object? _, WarningEventArgs args) { message = args.Message; };
    await test.ReadAsync(UnitTestStatic.Token);
    await test.ReadAsync(UnitTestStatic.Token);
    Assert.IsTrue(message.Contains("Linefeed"));
  }

  [TestMethod]
  public async Task TextQualifierBeginningAndEnd()
  {
    var setting = new CsvFileDummy()
    {
      HasFieldHeader = false,
      FieldDelimiterChar = ',',
      EscapePrefixChar = '\\',
      ContextSensitiveQualifier = false,
    };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("TextQualifierBeginningAndEnd.txt"),
      65001, 0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, false);
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(5, test.FieldCount);
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
    // a,b",c"c,d""d,"e""e",""f
    Assert.AreEqual("a", test.GetString(0));
    Assert.AreEqual("b\"", test.GetString(1));
    Assert.AreEqual("c\"c", test.GetString(2));
    Assert.AreEqual("d\"\"d", test.GetString(3));
    Assert.AreEqual("e\"e", test.GetString(4));

    //"g,h,i"",j,k,l"
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(2U, test.StartLineNumber, "LineNumber");
    Assert.AreEqual("g,h,i\",j,k,l", test.GetString(0));
    // "m",n\"op\"\"qr,"s\"tu\"\"vw",\"x""""""
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(3U, test.StartLineNumber, "LineNumber");
    Assert.AreEqual("m", test.GetString(0));
    Assert.AreEqual("n\"op\"\"qr", test.GetString(1));
    Assert.AreEqual("s\"tu\"\"vw", test.GetString(2));
    Assert.AreEqual("\"x\"\"\"\"\"\"", test.GetString(3));
  }

  [TestMethod]
  public async Task TextQualifierDataPastClosingQuote()
  {
    var setting = new CsvFileDummy()
    {
      HasFieldHeader = false,
      FieldDelimiterChar = ',',
      ContextSensitiveQualifier = false
    };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("TextQualifierDataPastClosingQuote.txt"),
      65001, 0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, false);
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(6, test.FieldCount);
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    //"a"a,b,c,d,e,f
    Assert.AreEqual("a", test.GetString(0));
    Assert.AreEqual("b", test.GetString(1));
    Assert.AreEqual("c", test.GetString(2));
    Assert.AreEqual("d", test.GetString(3));
    Assert.AreEqual("e", test.GetString(4));
    Assert.AreEqual("f", test.GetString(5));
    //1,2,"3" ignore me,4,5,6
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("1", test.GetString(0));
    Assert.AreEqual("2", test.GetString(1));
    Assert.AreEqual("3", test.GetString(2));
    Assert.AreEqual("4", test.GetString(3));
    Assert.AreEqual("5", test.GetString(4));
    Assert.AreEqual("6", test.GetString(5));
    Assert.IsFalse(await test.ReadAsync(UnitTestStatic.Token));
  }

  [TestMethod]
  public async Task TextQualifierNotClosedAtEnd()
  {
    var setting = new CsvFileDummy() { HasFieldHeader = false, FieldDelimiterChar = ',', EscapePrefixChar = '\\' };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("TextQualifierNotClosedAtEnd.txt"),
      65001, 0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, true);
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(6, test.FieldCount);
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
    // "a','b ",c," , d",e,f
    Assert.AreEqual("a", test.GetString(0));
    Assert.AreEqual("b   ", test.GetString(1));
    Assert.AreEqual("c", test.GetString(2));
    Assert.AreEqual(" ,  d", test.GetString(3));
    Assert.AreEqual("e", test.GetString(4));
    Assert.AreEqual("f", test.GetString(5));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(4U, test.StartLineNumber, "LineNumber");
    //7," ,8, ",9,10,11,12
    Assert.AreEqual("7", test.GetString(0));
    Assert.AreEqual(" ,8, ", test.GetString(1));
    Assert.AreEqual("9", test.GetString(2));
    Assert.AreEqual("10", test.GetString(3));
    Assert.AreEqual("11", test.GetString(4));
    Assert.AreEqual("12", test.GetString(5));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(test.StartLineNumber >= 8 && test.StartLineNumber <= 9, "LineNumber");
    //"19 , ",20,21,22,23,"
    Assert.AreEqual("19 , ", test.GetString(0));
    Assert.AreEqual("20", test.GetString(1));
    Assert.AreEqual("21", test.GetString(2));
    Assert.AreEqual("22", test.GetString(3));
    Assert.AreEqual("23", test.GetString(4));
    //Assert.AreEqual(null, test.GetString(5));
    Assert.AreEqual(" \n 24", test.GetString(5).HandleCrlfCombinations());
  }

  [TestMethod]
  public async Task TextQualifiers()
  {
    var setting = new CsvFileDummy() { HasFieldHeader = false, FieldDelimiterChar = ',' };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("TextQualifiers.txt"), 65001,
      0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, true);
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(6, test.FieldCount);
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("a", test.GetString(0));
    Assert.AreEqual("b \"  ", test.GetString(1));
    Assert.AreEqual("c", test.GetString(2));
    Assert.AreEqual("   d", test.GetString(3));
    Assert.AreEqual("e", test.GetString(4));
    Assert.AreEqual("f", test.GetString(5));
    //1,2,3,4,5,6
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual("1", test.GetString(0));
    Assert.AreEqual("2", test.GetString(1));
    Assert.AreEqual("3", test.GetString(2));
    Assert.AreEqual("4", test.GetString(3));
    Assert.AreEqual("5", test.GetString(4));
    Assert.AreEqual("6", test.GetString(5));
  }

  [TestMethod]
  public async Task TextQualifiersWithDelimiters()
  {
    var setting = new CsvFileDummy()
    {
      HasFieldHeader = false,
      WarnDelimiterInValue = true,
      FieldDelimiterChar = ',',
      EscapePrefixChar = '\\',
    };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("TextQualifiersWithDelimiters.txt"),
      65001, 0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, true);
    var warningList = new RowErrorCollection();
    await test.OpenAsync(UnitTestStatic.Token);
    test.Warning += warningList.Add;
    Assert.AreEqual(6, test.FieldCount);
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
    // "a','b ",c," , d",e,f
    Assert.AreEqual("a", test.GetString(0));
    Assert.AreEqual("b   ", test.GetString(1));
    Assert.AreEqual("c", test.GetString(2));
    Assert.AreEqual(" ,  d", test.GetString(3));
    Assert.AreEqual("e", test.GetString(4));
    Assert.AreEqual("f", test.GetString(5));
    Assert.AreEqual(1, warningList.CountRows, "Warnings count");
    Assert.IsTrue(warningList.Display.Contains("Field delimiter"));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(4U, test.StartLineNumber, "LineNumber");
    //7," ,8, ",9,10,11,12
    Assert.AreEqual("7", test.GetString(0));
    Assert.AreEqual(" ,8, ", test.GetString(1));
    Assert.AreEqual("9", test.GetString(2));
    Assert.AreEqual("10", test.GetString(3));
    Assert.AreEqual("11", test.GetString(4));
    Assert.AreEqual("12", test.GetString(5));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.IsTrue(test.StartLineNumber >= 8 && test.StartLineNumber <= 9, "LineNumber");

    Assert.AreEqual("19 , ", test.GetString(0));
    Assert.AreEqual("20", test.GetString(1));
    Assert.AreEqual("21", test.GetString(2));
    Assert.AreEqual("22", test.GetString(3));
    Assert.AreEqual("23", test.GetString(4));
    Assert.AreEqual(" \n 24", test.GetString(5).HandleCrlfCombinations());
  }

  [TestMethod]
  public async Task TrimmingHeaders()
  {
    var setting = new CsvFileDummy() { HasFieldHeader = true, FieldDelimiterChar = ',', CommentLine = "#" };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("TrimmingHeaders.txt"), 65001,
      0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, true);
    var warningList = new RowErrorCollection();
    test.Warning += warningList.Add;
    await test.OpenAsync(UnitTestStatic.Token);
      
    Assert.AreEqual(6, test.FieldCount);
    Assert.AreEqual("a", test.GetName(0));
    Assert.AreEqual("b", test.GetName(1));
    Assert.AreEqual("c", test.GetName(2));
    Assert.AreEqual("d", test.GetName(3));
    Assert.AreEqual("Column5", test.GetName(4));
    Assert.AreEqual("f", test.GetName(5));
    Assert.IsTrue(warningList.CountRows >= 1);
    Assert.IsTrue(warningList.Display.Contains("leading or trailing spaces"));
  }

  [TestMethod]
  public async Task UnicodeUtf16Be()
  {
    var setting = new CsvFileDummy()
    {
      HasFieldHeader = false,
      CodePageId = 1201,
      ByteOrderMark = true,
      FieldDelimiterChar = ',',
    };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("UnicodeUTF16BE.txt"), 65001,
      0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, true);
    await test.OpenAsync(UnitTestStatic.Token);

    Assert.AreEqual(4, test.FieldCount);
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
    Assert.AreEqual("tölvuiðnaðarins", test.GetString(1));
    Assert.AreEqual("ũΩ₤", test.GetString(2));
    Assert.AreEqual("používat", test.GetString(3));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(2U, test.StartLineNumber, "LineNumber");

    Assert.AreEqual("různá", test.GetString(0));
    Assert.AreEqual("čísla", test.GetString(1));
    Assert.AreEqual("pro", test.GetString(2));
    Assert.AreEqual("członkowskich", test.GetString(3));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(3U, test.StartLineNumber, "LineNumber");

    Assert.AreEqual("rozumieją", test.GetString(0));
    Assert.AreEqual("přiřazuje", test.GetString(1));
    Assert.AreEqual("gemeinnützige", test.GetString(2));
    Assert.AreEqual("är också", test.GetString(3));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(4U, test.StartLineNumber, "LineNumber");

    Assert.AreEqual("sprachunabhängig", test.GetString(0));
    Assert.AreEqual("that's all", test.GetString(1));
    Assert.AreEqual("for", test.GetString(2));
    Assert.AreEqual("now", test.GetString(3));

    Assert.IsFalse(await test.ReadAsync(UnitTestStatic.Token));
  }

  [TestMethod]
  public async Task UnicodeUtf16Le()
  {
    var setting = new CsvFileDummy()
    {
      HasFieldHeader = false,
      CodePageId = 1200,
      ByteOrderMark = true,
      FieldDelimiterChar = ',',
    };


    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("UnicodeUTF16LE.txt"), 65001,
      0, 0, setting.HasFieldHeader,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, false);
    await test.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual(4, test.FieldCount);
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
    Assert.AreEqual("tölvuiðnaðarins", test.GetString(1));
    Assert.AreEqual("ũΩ₤", test.GetString(2));
    Assert.AreEqual("používat", test.GetString(3));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(2U, test.StartLineNumber, "LineNumber");
    Assert.AreEqual("různá", test.GetString(0));
    Assert.AreEqual("čísla", test.GetString(1));
    Assert.AreEqual("pro", test.GetString(2));
    Assert.AreEqual("członkowskich", test.GetString(3));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(3U, test.StartLineNumber, "LineNumber");
    Assert.AreEqual("rozumieją", test.GetString(0));
    Assert.AreEqual("přiřazuje", test.GetString(1));
    Assert.AreEqual("gemeinnützige", test.GetString(2));
    Assert.AreEqual("är också", test.GetString(3));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(4U, test.StartLineNumber, "LineNumber");
    Assert.AreEqual("sprachunabhängig", test.GetString(0));
    Assert.AreEqual("that's all", test.GetString(1));
    Assert.AreEqual("for", test.GetString(2));
    Assert.AreEqual("now", test.GetString(3));

    Assert.IsFalse(await test.ReadAsync(UnitTestStatic.Token));
  }

  [TestMethod]
  public async Task UnicodeUtf8()
  {
    var setting = new CsvFileDummy();

    using var test = new CsvFileReader(UnitTestStatic.GetTestPath("UnicodeUTF8.txt"), 65001, 0, 0, false,
      new ColumnCollection(),
setting.TrimmingOption, setting.FieldDelimiterChar, setting.FieldQualifierChar, setting.EscapePrefixChar,
setting.RecordLimit, setting.AllowRowCombining,
setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape,
      setting.NewLinePlaceholder,
setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLfAsSpace,
      setting.TreatUnknownCharacterAsSpace,
setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP,
setting.WarnQuotes, setting.WarnUnknownCharacter,
setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines,
      setting.ConsecutiveEmptyRows,
setting.IdentifierInContainer, TimeZoneAdjust, TimeZoneInfo.Local.Id, true, false);
    await test.OpenAsync(UnitTestStatic.Token);
    //Assert.AreEqual(Encoding.UTF8, setting.CurrentEncoding);
    Assert.AreEqual(4, test.FieldCount);
    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");

    Assert.AreEqual("tölvuiðnaðarins", test.GetString(1));
    Assert.AreEqual("ũΩ₤", test.GetString(2));
    Assert.AreEqual("používat", test.GetString(3));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(2U, test.StartLineNumber, "LineNumber");

    Assert.AreEqual("různá", test.GetString(0));
    Assert.AreEqual("čísla", test.GetString(1));
    Assert.AreEqual("pro", test.GetString(2));
    Assert.AreEqual("członkowskich", test.GetString(3));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(3U, test.StartLineNumber, "LineNumber");

    Assert.AreEqual("rozumieją", test.GetString(0));
    Assert.AreEqual("přiřazuje", test.GetString(1));
    Assert.AreEqual("gemeinnützige", test.GetString(2));
    Assert.AreEqual("är också", test.GetString(3));

    Assert.IsTrue(await test.ReadAsync(UnitTestStatic.Token));
    Assert.AreEqual(4U, test.StartLineNumber, "LineNumber");

    Assert.AreEqual("sprachunabhängig", test.GetString(0));
    Assert.AreEqual("that's all", test.GetString(1));
    Assert.AreEqual("for", test.GetString(2));
    Assert.AreEqual("now", test.GetString(3));

    Assert.IsFalse(await test.ReadAsync(UnitTestStatic.Token));
  }
}