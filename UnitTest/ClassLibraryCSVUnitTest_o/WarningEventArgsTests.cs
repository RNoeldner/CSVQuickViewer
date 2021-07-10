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
  public class WarningEventArgsTests
  {
    [TestMethod]
    public void WarningEventArgsTest()
    {
      var args = new WarningEventArgs(1, 2, "mess", 3, 4, "colName");
      Assert.AreEqual(1, args.RecordNumber);
      args.RecordNumber = 10;
      Assert.AreEqual(10, args.RecordNumber);
      Assert.AreEqual(2, args.ColumnNumber);
      args.ColumnNumber = 11;
      Assert.AreEqual(11, args.ColumnNumber);

      Assert.AreEqual(3, args.LineNumberStart);
      args.LineNumberStart = 12;
      Assert.AreEqual(12, args.LineNumberStart);

      Assert.AreEqual(4, args.LineNumberEnd);
      args.LineNumberEnd = 13;
      Assert.AreEqual(13, args.LineNumberEnd);

      Assert.AreEqual("mess", args.Message);
      args.Message = "mess2";
      Assert.AreEqual("mess2", args.Message);

      Assert.AreEqual("colName", args.ColumnName);
      args.ColumnName = "colName2";
      Assert.AreEqual("colName2", args.ColumnName);
    }

    [TestMethod]
    public void DisplayTest()
    {
      var args = new WarningEventArgs(1, 2, "mess", 3, 4, "colName");
      Assert.AreEqual("mess", args.Display(false, false));
      Assert.AreEqual("Line 3 - 4: mess", args.Display(true, false));
      Assert.AreEqual("Line 3 - 4 Column [colName]: mess", args.Display(true, true));
      Assert.AreEqual("Column [colName]: mess", args.Display(false, true));
    }
  }
}