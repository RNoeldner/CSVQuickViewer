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
using System.Diagnostics.Contracts;

namespace CsvTools.Tests
{
  [TestClass]
  public static class UnitTestInitialize
  {
    private static readonly string m_ApplicationDirectory = FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles";

    public static string GetTestPath(string fileName) => System.IO.Path.Combine(m_ApplicationDirectory, fileName.TrimStart(new[] { ' ', '\\', '/' }));

    public static MimicSQLReader MimicSQLReader { get; } = new MimicSQLReader();

    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
      ApplicationSetting.RootFolder = FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles";
      FunctionalDI.SQLDataReader = MimicSQLReader.ReadData;

      // avoid contract violation kill the process
      Contract.ContractFailed += Contract_ContractFailed;
    }

    private static void Contract_ContractFailed(object sender, ContractFailedEventArgs e) => e.SetHandled();
  }
}