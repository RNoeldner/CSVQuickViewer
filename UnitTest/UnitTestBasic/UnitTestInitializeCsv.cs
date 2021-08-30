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
#nullable enable

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net;
using System.Threading;

namespace CsvTools.Tests
{
  [TestClass]
	public static class UnitTestInitializeCsv
	{
		public static HTMLStyle HTMLStyle { get; } = new HTMLStyle();
		public static CancellationToken Token;
		public static readonly string ApplicationDirectory = Path.Combine(FileSystemUtils.ExecutableDirectoryName(), "TestFiles");

		public static MimicSQLReader MimicSQLReader { get; } = new MimicSQLReader();

		public static string GetTestPath(string fileName) =>
			Path.Combine(ApplicationDirectory, fileName.TrimStart(' ', '\\', '/'));

		public static void MimicSql() => FunctionalDI.SQLDataReader = MimicSQLReader.ReadDataAsync;

		public static void AssemblyInitialize(TestContext context)
		{
			MimicSql();
			Token = context.CancellationTokenSource.Token;

			ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12;

			AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs args)
			{
				if (!context.CancellationTokenSource.IsCancellationRequested)
					context.Write(args.ExceptionObject.ToString());
			};
		}
	}
}