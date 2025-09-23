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
using System.Windows.Forms;

namespace CsvTools.Tests
{
  [TestClass]
  public class FoundEventArgsTests
  {
    [TestMethod]
    public void FoundEventArgs()
    {
      var cell = new DataGridViewTextBoxCell();
      var test = new FoundEventArgs(123, cell);
      Assert.AreEqual(123, test.Index);
      Assert.AreEqual(cell, test.Cell);
    }

    [TestMethod]
    public void SearchEventArgs()
    {
      var test = new SearchEventArgs("test", 2);
      Assert.AreEqual(2, test.Result);
      Assert.AreEqual("test", test.SearchText);

      var test2 = new SearchEventArgs("test2");
      Assert.AreEqual(1, test2.Result);
      Assert.AreEqual("test2", test2.SearchText);
    }
  }
}
