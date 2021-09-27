using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public class ManifestDataTests
  {
    [TestMethod]
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
    public async Task ReadManifestAsync()
    {
      var manifest = await ManifestData.ReadManifestFileSystem(UnitTestStatic.GetTestPath("training_relation.manifest.json"));
      var setting = manifest.PhysicalFile() as CsvFile;
      Assert.AreEqual(false, manifest.HasFieldHeader);
      Assert.AreEqual(19, setting.ColumnCollection.Count());
      using var reader = new CsvFileReader(setting!.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader, setting.ColumnCollection,
        setting.TrimmingOption, setting.FieldDelimiter, setting.FieldQualifier, setting.EscapePrefix, setting.RecordLimit, setting.AllowRowCombining,
        setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape, setting.NewLinePlaceholder,
        setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLFAsSpace, setting.TreatUnknownCharacterAsSpace,
        setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter,
        setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines, setting.ConsecutiveEmptyRows,
        setting.IdentifierInContainer, null);
      await reader.OpenAsync(UnitTestStatic.Token);
      Assert.AreEqual("object_id", reader.GetColumn(0).Name);
      reader.Read();
    }

    [TestMethod]
    public async Task ReadManifestZip()
    {
      var manifest = await ManifestData.ReadManifestZip(UnitTestStatic.GetTestPath("ces_xxx_v879548171_lo_exempt_status_reason_approver_local_full.zip"));
      var setting = manifest.PhysicalFile() as CsvFile;
      Assert.AreEqual(false, manifest.HasFieldHeader);
      Assert.AreEqual(3, setting.ColumnCollection.Count());
      using var reader = new CsvFileReader(setting!.FullPath, setting.CodePageId, setting.SkipRows, setting.HasFieldHeader, setting.ColumnCollection,
        setting.TrimmingOption, setting.FieldDelimiter, setting.FieldQualifier, setting.EscapePrefix, setting.RecordLimit, setting.AllowRowCombining,
        setting.ContextSensitiveQualifier, setting.CommentLine, setting.NumWarnings, setting.DuplicateQualifierToEscape, setting.NewLinePlaceholder,
        setting.DelimiterPlaceholder, setting.QualifierPlaceholder, setting.SkipDuplicateHeader, setting.TreatLFAsSpace, setting.TreatUnknownCharacterAsSpace,
        setting.TryToSolveMoreColumns, setting.WarnDelimiterInValue, setting.WarnLineFeed, setting.WarnNBSP, setting.WarnQuotes, setting.WarnUnknownCharacter,
        setting.WarnEmptyTailingColumns, setting.TreatNBSPAsSpace, setting.TreatTextAsNull, setting.SkipEmptyLines, setting.ConsecutiveEmptyRows,
        setting.IdentifierInContainer, null);
      await reader.OpenAsync(UnitTestStatic.Token);
      Assert.AreEqual("lesrlA_reason_id", reader.GetColumn(0).Name);
      reader.Read();
      Assert.AreEqual("Other", reader.GetValue(1));
    }
  }
}