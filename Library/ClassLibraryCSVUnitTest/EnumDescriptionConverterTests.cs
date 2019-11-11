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

namespace CsvTools.Tests
{
  [TestClass]
  public class EnumDescriptionConverterTests
  {
    [TestMethod]
    public void EnumDescriptionConverterTest()
    {
      Assert.IsNotNull(new EnumDescriptionConverter(typeof(TestEnum)));
    }

    [TestMethod]
    public void CanConvertFromTest()
    {
      var conv = new EnumDescriptionConverter(typeof(TestEnum));
      Assert.IsTrue(conv.CanConvertFrom(typeof(string)));
    }

    [TestMethod]
    public void CanConvertToTest()
    {
      var conv = new EnumDescriptionConverter(typeof(TestEnum));
      Assert.IsTrue(conv.CanConvertTo(typeof(string)));
    }

    [TestMethod]
    public void ConvertFromTest()
    {
      var conv = new EnumDescriptionConverter(typeof(TestEnum));
      Assert.AreEqual(TestEnum.Value1, conv.ConvertFromString("Value1"));
    }

    [TestMethod]
    public void ConvertToTest()
    {
      var conv = new EnumDescriptionConverter(typeof(TestEnum));
      Assert.AreEqual("Value1", conv.ConvertToString(TestEnum.Value1));
    }

    /// <summary>
    ///   Error Placement
    /// </summary>
    private enum TestEnum
    {
      Value1 = 0,
      Value2 = 1,
      AnotherValue = 2,
      YetAnotherValue = 3
    }
  }
}