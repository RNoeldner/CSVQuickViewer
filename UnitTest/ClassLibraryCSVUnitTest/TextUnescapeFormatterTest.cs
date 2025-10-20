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
  [TestClass]
  public class TextUnescapeFormatterTest
  {
    [TestMethod]
    public void UnescapeText()
    {
      Assert.AreEqual("This is a test", TextUnescapeFormatter.Unescape("This is a test"));
      Assert.AreEqual("This is \a test", TextUnescapeFormatter.Unescape("This is \\a test"));
      Assert.AreEqual("This is \t test", TextUnescapeFormatter.Unescape("This is \\t test"));
      Assert.AreEqual("This is \n test\r", TextUnescapeFormatter.Unescape("This is \\n test\\r"));      
    }

    [TestMethod]
    public void UnescapeTextNumbers()
    {
      Assert.AreEqual("Unicode \ud835b", TextUnescapeFormatter.Unescape("Unicode \\ud835b"));
      Assert.AreEqual("Unicode \ud835", TextUnescapeFormatter.Unescape("Unicode \\ud835"));
      Assert.AreEqual("Unicode \udcd9", TextUnescapeFormatter.Unescape("Unicode \\udcd9"));
      Assert.AreEqual("\udcd9 Yeah", TextUnescapeFormatter.Unescape("\\udcd9 Yeah"));
    }

    [TestMethod]
    public void UnescapeTextNumbers2()
    {
      Assert.AreEqual("Unicode \x0020", TextUnescapeFormatter.Unescape("Unicode \\x0020"));
      Assert.AreEqual("Unicode \x020", TextUnescapeFormatter.Unescape("Unicode \\x020"));
      Assert.AreEqual("\x20", TextUnescapeFormatter.Unescape("\\x20"));
    }
  }
}
