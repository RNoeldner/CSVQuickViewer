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
	public class FillGuessSettingsTests
	{
		[TestMethod]
		public void CloneTest()
		{
			var a = new FillGuessSettings
			{
				CheckedRecords = 10,
				CheckNamedDates = true,
				DetectNumbers = true,
				DetectPercentage = true,
				DetectBoolean = true,
				DetectDateTime = true,
				DetectGUID = true,
				FalseValue = "Never",
				TrueValue = "Always",
				IgnoreIdColumns = false,
				MinSamples = 5,
				SampleValues = 5,
				SerialDateTime = true
			};
			var b = a.Clone();
			Assert.AreNotSame(b, a);
			a.CheckAllPropertiesEqual(b);
		}

		[TestMethod]
		public void CopyToTest()
		{
			var a = new FillGuessSettings
			{
				CheckedRecords = 10,
				CheckNamedDates = true,
				DetectNumbers = true,
				DetectPercentage = true,
				DetectBoolean = true,
				DetectDateTime = true,
				DetectGUID = true,
				FalseValue = "Never",
				TrueValue = "Always",
				IgnoreIdColumns = false,
				MinSamples = 5,
				SampleValues = 5,
				SerialDateTime = true
			};

			var b = new FillGuessSettings
			{
				CheckedRecords = 11,
				CheckNamedDates = !a.CheckNamedDates,
				DetectNumbers = !a.DetectNumbers,
				DetectPercentage = !a.DetectPercentage,
				DetectBoolean = !a.DetectBoolean,
				DetectDateTime = !a.DetectDateTime,
				DetectGUID = !a.DetectGUID,
				FalseValue = "false",
				TrueValue = "true",
				IgnoreIdColumns = !a.IgnoreIdColumns,
				MinSamples = a.MinSamples + 1,
				SampleValues = a.SampleValues + 2,
				SerialDateTime = false
			};

			a.CopyTo(b);
			Assert.AreNotSame(b, a);
			a.CheckAllPropertiesEqual(b);
		}

		[TestMethod]
		public void NotifyPropertyChangedTest()
		{
			var a = new FillGuessSettings
			{
				CheckedRecords = 10,
				CheckNamedDates = true,
				DetectNumbers = true,
				DetectPercentage = true,
				DetectBoolean = true,
				DetectDateTime = true,
				DetectGUID = true,
				FalseValue = "Never",
				TrueValue = "Always",
				IgnoreIdColumns = false,
				MinSamples = 5,
				SampleValues = 5,
				SerialDateTime = true
			};
			var fired = false;
			a.PropertyChanged += delegate { fired = true; };
			Assert.IsFalse(fired);
			a.CheckedRecords = 11;
			Assert.IsTrue(fired);
		}
	}
}