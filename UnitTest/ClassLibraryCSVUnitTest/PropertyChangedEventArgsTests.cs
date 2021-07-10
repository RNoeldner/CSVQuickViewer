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
	public class PropertyChangedEventArgsTests
	{
		[TestMethod]
		public void PropertyChangedEventArgsTest()
		{
			var args = new PropertyChangedEventArgs<string>("propName", "old", "new");
			Assert.AreEqual("old", args.OldValue);
			Assert.AreEqual("new", args.NewValue);
			Assert.AreEqual("propName", args.PropertyName);

			args.NewValue = "new2";
			Assert.AreEqual("new2", args.NewValue);

			args.OldValue = "old2";
			Assert.AreEqual("old2", args.OldValue);
		}
	}
}