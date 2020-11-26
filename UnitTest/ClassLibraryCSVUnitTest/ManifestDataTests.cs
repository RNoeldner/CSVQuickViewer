using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class ManifestDataTests
  {
    [TestMethod]
    public void ReadManifestPropertiesTest()
    {
      var m = new ManifestData
      {
        Desc = "desc",
        Heading = "heading",
        PubName = "pubname",
        Delta = true,
        HasUserDefinedFields = true,
        Hydration = "hydration",
        Fields = new[] { new ManifestData.ManifestField("desc2", "heading2", 1, "pubname2", "type2") }
      };

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
      var setting =
        ManifestData.ReadManifestFileSystem(UnitTestInitializeCsv.GetTestPath("training_relation.manifest.json"));

      Assert.AreEqual(false, setting.HasFieldHeader);
      Assert.AreEqual(19, setting.ColumnCollection.Count);
      using (var reader = new CsvFileReader(setting, null))
      {
        await reader.OpenAsync(UnitTestInitializeCsv.Token);
        Assert.AreEqual("object_id", reader.GetColumn(0).Name);
        reader.Read();
      }
    }

    [TestMethod]
    public async Task ReadManifestZip()
    {
      var setting =
        ManifestData.ReadManifestZip(UnitTestInitializeCsv.GetTestPath("ces_xxx_v879548171_lo_exempt_status_reason_approver_local_full.zip"));

      Assert.AreEqual(false, setting.HasFieldHeader);
      Assert.AreEqual(3, setting.ColumnCollection.Count);
      using (var reader = new CsvFileReader(setting, null))
      {
        await reader.OpenAsync(UnitTestInitializeCsv.Token);
        Assert.AreEqual("lesrlA_reason_id", reader.GetColumn(0).Name);
        reader.Read();
        Assert.AreEqual("Other", reader.GetValue(1));        
      }
    }
  }
}