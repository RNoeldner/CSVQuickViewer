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
  public class ValidationResultTests
  {

    [TestMethod]
    public void ValidationResultSetValidationResultTest()

    {
      var validationResult = new ValidationResult("Hello", 0, -1, -1);
      Assert.AreEqual("Hello", validationResult.TableName);
      Assert.AreEqual(-1, validationResult.WarningCount);
      Assert.AreEqual(-1, validationResult.ErrorCount);
      Assert.AreEqual(0, validationResult.NumberRecords);

    }
    [TestMethod]
    public void ValidationResultSetValidationSetting()

    {
      var setting = new CsvFile("Name")
      { ID = "TableName", NumRecords = 10 };
      var validationResult = new ValidationResult(setting);

      Assert.AreEqual(setting.ID, validationResult.TableName);
      Assert.AreEqual(setting.NumRecords, validationResult.NumberRecords);
    }

    [TestMethod]
    public void ValidationResultSetValidationEmpty()

    {
      var validationResult = new ValidationResult();

      Assert.AreEqual("", validationResult.TableName);
      Assert.AreEqual(0, validationResult.NumberRecords);
    }
  }
}