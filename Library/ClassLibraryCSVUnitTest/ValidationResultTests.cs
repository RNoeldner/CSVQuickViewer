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

using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class ValidationResultTests
  {
    private readonly ValidationResult m_ValidationResult = new ValidationResult();

    [TestMethod]
    public void ValidationResultFileSizeTest()
    {
      m_ValidationResult.FileSize = 0;
      Assert.AreEqual(0, m_ValidationResult.FileSize);
      m_ValidationResult.FileSize = 12345;
      Assert.AreEqual(12345, m_ValidationResult.FileSize);
    }

    [TestMethod]
    public void ValidationResultSetValidationResultTest()
    {
      m_ValidationResult.TableName = "Hallo";
      Assert.AreEqual("Hallo", m_ValidationResult.TableName);
    }

    [TestMethod]
    public void ValidationResultRatiosTest()
    {
      Assert.AreEqual(-1, new ValidationResult().ErrorCount);
      Assert.AreEqual(0, new ValidationResult().NumberRecords);
      m_ValidationResult.NumberRecords = 0;

      m_ValidationResult.ErrorCount = 10;
      Assert.AreEqual(10, m_ValidationResult.ErrorCount);
      m_ValidationResult.WarningCount = 40;
      Assert.AreEqual(40, m_ValidationResult.WarningCount);

      m_ValidationResult.NumberRecords = 1000;
      Assert.AreEqual(1000, m_ValidationResult.NumberRecords);
    }

    [TestMethod]
    public void ValidationResultNotifyPropertyChangedTest()
    {
      var property = string.Empty;
      m_ValidationResult.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
      {
        property += "-" + e.PropertyName + "-";
      };
      m_ValidationResult.NumberRecords = 1;
      m_ValidationResult.NumberRecords = 2;
      Assert.IsTrue(property.Contains("-NumberRecords-"));
    }
  }
}