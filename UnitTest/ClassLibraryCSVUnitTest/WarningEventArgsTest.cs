﻿/*
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
using System;

namespace CsvTools.Tests
{
  [TestClass]
  public class WarningEventArgsTest
  {
    [TestMethod]
    public void WarningEventArgsPropertySetGet()
    {
      var test1 = new WarningEventArgs(100, 200, "Warning", 201, 202, "ColName");
      Assert.AreEqual(100, test1.RecordNumber);
      Assert.AreEqual(200, test1.ColumnNumber);
      Assert.AreEqual("Warning", test1.Message);
      Assert.AreEqual(201, test1.LineNumberStart);
      Assert.AreEqual(202, test1.LineNumberEnd);
      Assert.AreEqual("ColName", test1.ColumnName);
    }

    [TestMethod]
    public void WarningEventArgsDisplay()
    {
      var test1 = new WarningEventArgs(100, 200, "Warning", 201, 202, "ColName");
      Assert.AreEqual(test1.Message, test1.Display(false, false));
      Assert.AreEqual("Line 201 - 202 Column [ColName]: Warning", test1.Display(true, true));

      var test2 = new WarningEventArgs(101, 202, "Warning", 203, -1, "ColName");
      Assert.AreEqual("Line 203: Warning", test2.Display(true, false));
    }

    [TestMethod]
    public void WarningEventArgsError()
    {
      var exception = false;
      try
      {
        var unused = new WarningEventArgs(100, 200, "", 201, 202, "ColName");
      }
      catch (ArgumentException)
      {
        exception = true;
      }
      catch (Exception ex)
      {
        Assert.Fail("Wrong Exception Type: " + ex.GetType());
      }

      Assert.IsTrue(exception, "No Exception thrown");
    }
  }
}