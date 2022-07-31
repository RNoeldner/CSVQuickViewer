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
	public class CustomProcessDisplayTests
	{
		[TestMethod]
		public void CustomProcessDisplayTest()
		{
			var called = false;
      var processDisplay = new CustomProcessDisplay();
      processDisplay.Progress += (_) => { called = true; };
      processDisplay.SetProcess("Test", -1);
      Assert.IsTrue(called);
    }

		[TestMethod]
		public void DummyProcessDisplayTest()
    {
      var processDisplay = new CustomProcessDisplay();
      processDisplay.SetProcess("Test", -1);
    }


		[TestMethod]
		public void SetProcessTest()
    {
      var processDisplay = new CustomProcessDisplay();
      processDisplay.SetProcess("Test", -1);
    }

		[TestMethod]
		public void SetProcessTest1()
    {
      var processDisplay = new CustomProcessDisplay( );
      processDisplay.SetProcess("Test", 100);
    }
    
	}
}