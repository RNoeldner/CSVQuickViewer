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
			var test = new ProgressTime { Maximum = 100 };

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
			var test = new ProgressTime { Maximum = 5 };
			Assert.AreEqual(5, test.Maximum);
			test.Maximum = 100;
			Assert.AreEqual(100, test.Maximum);

		}
	}
}