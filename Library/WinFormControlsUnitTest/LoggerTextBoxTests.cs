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

using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace CsvTools.Tests
{
  [TestClass()]
  public class LoggerTextBoxTests
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    [TestMethod()]
    public void BeginSectionTest()
    {
      var test = new LoggerDisplay();
      test.BeginSection("Hello");
    }

    [TestMethod()]
    public void ClearTest()
    {
      var test = new LoggerDisplay();
      test.Clear();
    }

    [TestMethod()]
    public void LoggerTextBoxTest()
    {
      var test = new LoggerDisplay();
      Log.Debug("Debug");
      Log.Debug("Debug – NewInformation");
      Log.Info("Info");
      Log.Warn("Warning");
      Log.Error("Error");
      Log.Fatal("Fatal");
    }
  }
}