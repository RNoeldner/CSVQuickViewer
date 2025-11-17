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
using System.Threading;

namespace CsvTools.Tests
{
  [TestClass]
	public class progressTimeTest
	{
		
		[TestMethod]
		[Timeout(3000)]
		public void MeasureTimeToCompletion()
		{
			var test = new ProgressTime() { Maximum = 100 };

			for (long counter = 1; counter <= 20; counter++)
			{
				test.Report(new ProgressInfo(counter.ToString(), counter));
				Thread.Sleep(100);
			}

			Assert.AreEqual(20, test.TimeToCompletion.Percent);
			// 20 * 100ms = 2s for 100 we need 10s as 2s are passed it should be 8s
			var est = test.TimeToCompletion.EstimatedTimeRemaining.TotalSeconds;
			Assert.IsTrue(est > 7 && est < 12, $"{est}s {test.TimeToCompletion.EstimatedTimeRemainingDisplay}");
		}

		[TestMethod]
		public void Properties()
		{
			var test = new ProgressTime() { Maximum = 5 };
			Assert.AreEqual(5, test.Maximum);
			test.Maximum = 100;
			Assert.AreEqual(100, test.Maximum);

		}
	}
}
