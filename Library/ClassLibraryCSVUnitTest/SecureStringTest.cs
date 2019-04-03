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
  /// <summary>
  ///   Summary description for SecureStringTest
  /// </summary>
  [TestClass]
  public class SecureStringTest
  {
    [TestMethod]
    public void Encyrpt()
    {
      var encyrpted1 = "This is a a test".Encrypt();
      var encyrpted2 = "This is a a test".Encrypt();
      Assert.AreNotEqual(encyrpted1, encyrpted2);
    }

    [TestMethod]
    public void Decrypt()
    {
      const string testValue = "This is a a test";
      var encyrpted1 = testValue.Encrypt();
      Assert.AreEqual(testValue, encyrpted1.Decrypt());
    }

    [TestMethod]
    public void DecryptEmptyAndNull()
    {
      Assert.AreEqual(string.Empty, string.Empty.Decrypt());
      Assert.AreEqual(string.Empty, SecureString.Decrypt(null));
    }

    #region Additional test attributes

    //
    // You can use the following additional attributes as you write your tests:
    //
    // Use ClassInitialize to run code before running the first test in the class
    // [ClassInitialize()]
    // public static void MyClassInitialize(TestContext testContext) { }
    //
    // Use ClassCleanup to run code after all tests in a class have run
    // [ClassCleanup()]
    // public static void MyClassCleanup() { }
    //
    // Use TestInitialize to run code before running each test
    // [TestInitialize()]
    // public void MyTestInitialize() { }
    //
    // Use TestCleanup to run code after each test has run
    // [TestCleanup()]
    // public void MyTestCleanup() { }
    //

    #endregion Additional test attributes
  }
}