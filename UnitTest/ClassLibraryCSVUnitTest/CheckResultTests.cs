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
	public class CheckResultTests
	{
		[TestMethod]
		public void CombineCheckResultTest()
		{
			var test1 = new CheckResult();
			test1.AddNonMatch("Test1");
			test1.AddNonMatch("Test2");
			test1.PossibleMatch = true;

			var test2 = new CheckResult();
			test2.AddNonMatch("Test3");
			test2.PossibleMatch = true;

			var test3 = new CheckResult { PossibleMatch = true };

			test1.KeepBestPossibleMatch(test1);
			Assert.AreEqual(2, test1.ExampleNonMatch.Count());
			Assert.IsTrue(test1.PossibleMatch);

			test1.KeepBestPossibleMatch(test2);
			Assert.AreEqual(1, test1.ExampleNonMatch.Count());
			Assert.IsTrue(test1.PossibleMatch);

			test1.KeepBestPossibleMatch(test3);
			Assert.AreEqual(0, test1.ExampleNonMatch.Count());
		}
	}
}