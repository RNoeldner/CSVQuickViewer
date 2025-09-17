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
using System;
using System.Collections.Generic;

namespace CsvTools.Tests
{
  [TestClass]
  public class WarningListTests
  {
    [TestMethod]
    public void WarningListAddAndClear()
    {
      var messageList = new RowErrorCollection();
      Assert.AreEqual(0, messageList.CountRows);

      messageList.Add(null, new WarningEventArgs(1, 2, "Line1", 0, 0, null));
      Assert.AreEqual(1, messageList.CountRows);
      messageList.Clear();
      Assert.AreEqual(0, messageList.CountRows);
    }

    [TestMethod]
    public void WarningListAddEmpty()
    {
      var messageList = new RowErrorCollection();
      Assert.AreEqual(0, messageList.CountRows);

      try
      {
        // ReSharper disable once AssignNullToNotNullAttribute
#pragma warning disable CS8625 // Ein NULL-Literal kann nicht in einen Non-Nullable-Verweistyp konvertiert werden.
        messageList.Add(null, new WarningEventArgs(1, 2, null, 0, 0, null));
#pragma warning restore CS8625 // Ein NULL-Literal kann nicht in einen Non-Nullable-Verweistyp konvertiert werden.
        Assert.Fail("Exception not thrown");
      }
      catch (ArgumentException)
      {
      }

      try
      {
        messageList.Add(null, new WarningEventArgs(1, 2, string.Empty, 0, 0, null));
        Assert.Fail("Exception not thrown");
      }
      catch (ArgumentException)
      {
      }
    }
    

    [TestMethod]
    public void WarningListDisplay2()
    {
      var messageList = new RowErrorCollection();
      messageList.Add(null, new WarningEventArgs(1, 1, "Text1", 0, 1, null));
      messageList.Add(null, new WarningEventArgs(1, 2, "Text2", 0, 2, null));
      Assert.IsTrue(
        "Text1" + ErrorInformation.cSeparator + "Text2" == messageList.Display ||
        "Text2" + ErrorInformation.cSeparator + "Text1" == messageList.Display);
    }
  }
}