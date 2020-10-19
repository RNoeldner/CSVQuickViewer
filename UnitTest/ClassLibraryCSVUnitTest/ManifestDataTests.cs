using Microsoft.VisualStudio.TestTools.UnitTesting;
using CsvTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass()]
  public class ManifestDataTests
  {
    [TestMethod()]
    public void ReadManifestPropertiesTest()
    {
      var m = new ManifestData()
      {
        desc="desc",
        heading="heading",
        pubname="pubname",
        delta = true,
        hasuserdefinedfields =true,
        hydration="hydration",
        fields = new ManifestData.ManifestField[] {
          new ManifestData.ManifestField()
          {
            desc="desc2",
            heading="heading2",
            pubname="pubname2",
            type="type2",
            ordinal=1
          }
        }
      };

      Assert.AreEqual("desc", m.desc);
      Assert.AreEqual("heading", m.heading);
      Assert.AreEqual("pubname", m.pubname);
      Assert.AreEqual("hydration", m.hydration);
      Assert.IsTrue(m.delta);
      Assert.IsTrue(m.hasuserdefinedfields);
      Assert.AreEqual("desc2", m.fields[0].desc);
      Assert.AreEqual("heading2", m.fields[0].heading);
      Assert.AreEqual("pubname2", m.fields[0].pubname);
      Assert.AreEqual("type2", m.fields[0].type);
      Assert.AreEqual(1, m.fields[0].ordinal);
    }

    [TestMethod()]
    public async Task ReadManifestAsync()
    {
      var setting = ManifestData.ReadManifest(UnitTestInitializeCsv.GetTestPath("training_relation.manifest.json"));

      Assert.AreEqual(false, setting.HasFieldHeader);
      Assert.AreEqual(19, setting.ColumnCollection.Count);
      using (var reader = new CsvFileReader(setting, null))
      {
        await reader.OpenAsync(UnitTestInitializeCsv.Token);
        Assert.AreEqual("object_id", reader.GetColumn(0).Name);
        reader.Read();      
      }
    }
  }
}
