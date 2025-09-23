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
using System;
using System.Collections.Generic;

namespace CsvTools.Tests
{
  [TestClass]
	public class AaaPerfromanceTest
	{
		private const string cTest =
			@"Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Duis autem vel eum iriure dolor in hendrerit in vulputate velit esse molestie consequat, vel illum dolore eu feugiat nulla facilisis at vero eros et accumsan et iusto odio dignissim qui blandit praesent luptatum zzril delenit augue duis dolore te feugait nulla facilisi. Lorem ipsum dolor sit amet, consectetuer adipiscing elit, sed diam nonummy nibh euismod tincidunt ut laoreet dolore magna aliquam erat volutpat. Ut wisi enim ad minim veniam, quis nostrud exerci tation ullamcorper suscipit lobortis nisl ut aliquip ex ea commodo consequat. Duis autem vel eum iriure dolor in hendrerit in vulputate velit esse molestie consequat, vel illum dolore eu feugiat nulla facilisis at vero eros et accumsan et iusto odio dignissim qui blandit praesent luptatum zzril delenit augue duis dolore te feugait nulla facilisi. Nam liber tempor cum soluta nobis eleifend option congue nihil imperdiet doming id quod mazim placerat facer";

		[TestMethod]
		public void WithoutUpper()
		{
			var words = cTest.Split(' ');

			var set = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
			for (var counter = 0; counter < 100; counter++)
				for (var word = 0; word < words.Length; word++)
					set.Add(words[word] + counter);
			var ok = 0;
			for (var counter = 0; counter < 100; counter++)
				for (var word = 0; word < words.Length; word++)
					if (set.Contains(words[word] + counter))
						ok++;
			Assert.AreEqual(29400, ok);
		}

		[TestMethod]
		public void WithUpper()
		{
			var words = cTest.Split(' ');

			var set = new HashSet<string>(StringComparer.Ordinal);
			for (var counter = 0; counter < 100; counter++)
				for (var word = 0; word < words.Length; word++)
					set.Add(words[word].ToUpperInvariant() + counter);
			var ok = 0;
			for (var counter = 0; counter < 100; counter++)
				for (var word = 0; word < words.Length; word++)
					if (set.Contains(words[word].ToUpperInvariant() + counter))
						ok++;
			Assert.AreEqual(29400, ok);
		}
	}
}
