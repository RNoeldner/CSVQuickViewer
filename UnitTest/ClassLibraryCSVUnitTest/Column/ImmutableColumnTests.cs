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

namespace CsvTools.Tests
{
  [TestClass()]
  public class ImmutableColumnTests
  {
    [TestMethod()]
    public void ImmutableColumnTest()
    {
      var ic = new Column("Name", new ValueFormat(DataTypeEnum.Integer), 0);
      Assert.AreEqual("Name", ic.Name);
      Assert.AreEqual(DataTypeEnum.Integer, ic.ValueFormat.DataType);
    }

    [TestMethod()]
    public void ImmutableColumnTest2()
    {
      var ic = new Column("Name", new ValueFormat(DataTypeEnum.Integer), 1);
      Assert.AreEqual("Name", ic.Name);
      Assert.AreEqual(DataTypeEnum.Integer, ic.ValueFormat.DataType);
    }

    [TestMethod()]
    public void EqualsTest()
    {
      var ic = new Column("Name", new ValueFormat(DataTypeEnum.Integer), 1);
      var ic2 = new Column("Name", new ValueFormat(DataTypeEnum.Integer), 2);
      Assert.IsTrue(ic.Equals(ic),"ic==ic");
      Assert.IsFalse(ic.Equals(ic2),"ic!=ic2");
      Assert.IsFalse(ic2.Equals(null), "ic2!=null");
    }


    [TestMethod()]
    public void GetHashCodeTest()
    {
      var ic = new Column("Name", new ValueFormat(DataTypeEnum.Integer), 1);
      Assert.AreNotEqual(0, ic.GetHashCode());
      var ic2 = new Column("Name", new ValueFormat(DataTypeEnum.Integer), 1);
      Assert.AreEqual(ic2.GetHashCode(), ic.GetHashCode());
    }

    [TestMethod()]
    public void ToStringTest()
    {
      var ic = new Column("Name", new ValueFormat(DataTypeEnum.Integer), 1);
      Assert.IsNotNull((ic.ToString()));
    }
  }
}
