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
  public class ColumnMutTests
  {
    [TestMethod()]
    public void ColumnMutPropertyChangedTest()
    {
      var cvm = new ColumnMut("Name", new ValueFormat());
      var changed = false;
      cvm.PropertyChanged += (o, e) => changed = true;
      cvm.Name = "Name2";
      Assert.AreEqual("Name2", cvm.Name);
      Assert.IsTrue(changed);
    }

    
    [TestMethod()]
    public void EqualsTest()
    {
      var cvm1 = new ColumnMut("Name", new ValueFormat());
      var cvm2 = new ColumnMut("Name", new ValueFormat());
      Assert.IsTrue(cvm1.Equals(cvm2));
    }

    [TestMethod()]
    public void CopyToTest()
    {
      var cvm1 = new ColumnMut("Name", new ValueFormat());
      var cvm2 = new ColumnMut("Name2", new ValueFormat());
      cvm1.CopyTo(cvm2);
      Assert.AreEqual(cvm1.Name, cvm2.Name);
    }

    [TestMethod()]
    public void ToStringTest()
    {
      var cvm1 = new ColumnMut("Name", new ValueFormat());
      Assert.AreEqual("Name (Text)", cvm1.ToString());
    }

    [TestMethod()]
    public void ToImmutableColumnTest()
    {
      var cvm1 = new ColumnMut("Name", new ValueFormat());
      var cv2 = cvm1.ToImmutableColumn();
      Assert.AreEqual(cvm1.Name, cv2.Name);
    }
  }
}