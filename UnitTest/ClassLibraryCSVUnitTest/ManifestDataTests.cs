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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace CsvTools.Tests;

[TestClass]
[SuppressMessage("ReSharper", "UseAwaitUsing")]
public class ManifestDataTests
{
  [TestMethod]
  [Timeout(1000)]
  public void ReadManifestPropertiesTest()
  {
    var m = new ManifestData("pubname", "heading", "desc", true, "hydration", true,
      new[] { new ManifestData.ManifestField("pubname2", "heading2", "desc2", "type2", 1) });

    Assert.AreEqual("desc", m.Desc);
    Assert.AreEqual("heading", m.Heading);
    Assert.AreEqual("pubname", m.PubName);
    Assert.AreEqual("hydration", m.Hydration);
    Assert.IsTrue(m.Delta);
    Assert.IsTrue(m.HasUserDefinedFields);
    Assert.AreEqual("desc2", m.Fields[0].Desc);
    Assert.AreEqual("heading2", m.Fields[0].Heading);
    Assert.AreEqual("pubname2", m.Fields[0].PubName);
    Assert.AreEqual("type2", m.Fields[0].Type);
    Assert.AreEqual(1, m.Fields[0].Ordinal);
  }

  [TestMethod]
  [Timeout(1000)]
  public async Task ReadManifestAsync()
  {
    var manifest =
      await ManifestData.ReadManifestFileSystem(UnitTestStatic.GetTestPath("training_relation.manifest.json"));
    var setting = new CsvFileDummy();
    manifest.CopyToCsv(setting);
      
    Assert.AreEqual(false, manifest.HasFieldHeader);
#pragma warning disable CS8602
    Assert.AreEqual(19, setting.ColumnCollection.Count());
#pragma warning restore CS8602
    using var reader = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows,
      0, setting.HasFieldHeader, setting.ColumnCollection,
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
setting.IdentifierInContainer, System.TimeZoneInfo.Local.Id, true, false);
    await reader.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual("object_id", reader.GetColumn(0).Name);
    await reader.ReadAsync(UnitTestStatic.Token);
  }

  [TestMethod]
  //[Timeout(1000)]
  public async Task ReadManifestZip()
  {
    var manifest =
      await ManifestData.ReadManifestZip(
        UnitTestStatic.GetTestPath("ces_xxx_v879548171_lo_exempt_status_reason_approver_local_full.zip"));

    var setting = new CsvFileDummy();
    manifest!.CopyToCsv(setting);

    Assert.AreEqual(false, manifest.HasFieldHeader);
#pragma warning disable 8602
    Assert.AreEqual(3, setting.ColumnCollection.Count());
#pragma warning restore 8602
    using var reader = new CsvFileReader(setting.FullPath, setting.CodePageId, setting.SkipRows,
      0, setting.HasFieldHeader, setting.ColumnCollection,
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
setting.IdentifierInContainer, System.TimeZoneInfo.Local.Id, true, false);
    await reader.OpenAsync(UnitTestStatic.Token);
    Assert.AreEqual("lesrlA_reason_id", reader.GetColumn(0).Name);

    await reader.ReadAsync(UnitTestStatic.Token);
    Assert.AreEqual("Other", reader.GetValue(1));
  }
}