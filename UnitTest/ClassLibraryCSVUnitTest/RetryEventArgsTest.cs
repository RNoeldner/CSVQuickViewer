using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CsvTools.Tests
{
  [TestClass]
	public class RetryEventArgsTest
	{
		[TestMethod]
		public void Ctor()
		{
			var args = new RetryEventArgs(new ArgumentException("Hello"));
			Assert.AreEqual("Hello", args.Exception.Message);
			Assert.AreEqual(false, args.Retry);
			args.Retry = true;
			Assert.AreEqual(true, args.Retry);
		}
	}
}