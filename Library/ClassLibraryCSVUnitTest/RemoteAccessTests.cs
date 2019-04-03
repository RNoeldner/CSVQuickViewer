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
  [TestClass()]
  public class RemoteAccessTests
  {
    public void CopyToTest()
    {
      // Covered by generic test
    }

    public void EqualsTest()
    {
      // Covered by generic test
    }

    [TestMethod()]
    public void RemoteAccessTest()
    {
      var testCase = new RemoteAccess();
      bool hasFired = false;
      testCase.PropertyChanged += delegate (object sender, System.ComponentModel.PropertyChangedEventArgs e)
      {
        hasFired = true;
      };
      testCase.HostName = "Test";
      Assert.AreEqual("Test", testCase.HostName);
      Assert.IsTrue(hasFired);
      hasFired = false;
      testCase.HostName = "Test";
      Assert.IsFalse(hasFired);

      testCase.User = "Hello";
      Assert.IsTrue(hasFired);
      Assert.AreEqual("Hello", testCase.User);
      hasFired = false;
      testCase.User = "Hello";
      Assert.IsFalse(hasFired);

      testCase.Password = "World";
      Assert.IsTrue(hasFired);
      Assert.AreEqual("World", testCase.Password);
      hasFired = false;
      testCase.Password = "World";
      Assert.IsFalse(hasFired);
    }
  }
}