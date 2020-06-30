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

using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public static class UnitTestInitializeCsv
  {
    public static CancellationToken Token;

    public static string ApplicationDirectory =  ApplicationSetting.RootFolder + @"\TestFiles";

    public static MimicSQLReader MimicSQLReader { get; } = new MimicSQLReader();

    public static string GetTestPath(string fileName) =>
      Path.Combine(ApplicationDirectory, fileName.TrimStart(' ', '\\', '/'));

    public static void MimicSql()
    {
      FunctionalDI.SQLDataReader = MimicSQLReader.ReadDataAsync;
    }

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
      Token = context.CancellationTokenSource.Token;
    }

    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
      MimicSql();
      Contract.ContractFailed += (sender, e) => e.SetHandled();
      Logger.AddLog += (s, level) => context.WriteLine($"{level} - {s}");
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12;
    }
  }
}