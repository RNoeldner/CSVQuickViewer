using Microsoft.VisualStudio.TestTools.UnitTesting; /*
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

namespace CsvTools.Tests
{
  [TestClass]
  public class ValueClusterTests
  {
    [TestMethod]
    public void ValueClusterCtor()
    {
      var tst1 = new ValueCluster("text", "SQl", 0, "text", null, false);
      Assert.AreEqual("text", tst1.Display);
      Assert.AreEqual(0, tst1.Count);

      var tst2 = new ValueCluster("dis", "cond", 10, "dis", null, false);
      Assert.AreEqual("dis", tst2.Display);
      Assert.AreEqual(10, tst2.Count);

      tst2.Active = true;
      Assert.IsTrue(tst2.Active);

      tst2.Active = false;
      Assert.IsFalse(tst2.Active);
    }


    [TestMethod]
    public void EqualsTest()
    {
      var src = new ValueCluster("dis", "cond", 10, "cond", null, false);
      var dest = new ValueCluster("dis", "cond2", 10, "cond", null, false);
      Assert.IsFalse(src.Equals(dest));
      Assert.IsTrue(src.Equals(src));
      Assert.IsTrue(src.Equals((object) src));
    }

    [TestMethod]
    public void ToStringTest()
    {
      var disp = new ValueCluster("dis2", "cond", 20, "cond", null, false).ToString();
      Assert.AreEqual("dis2 20 items", disp);
    }


    [TestMethod]
    public void GetHashCodeTest()
    {
      var disp1 = new ValueCluster("dis2", "cond", 20, "cond", null, true);
      var disp2 = new ValueCluster("dis2", "cond", 20, "cond2", "dummy", true);
      Assert.AreEqual(disp1.GetHashCode(), disp2.GetHashCode());
    }
  }
}