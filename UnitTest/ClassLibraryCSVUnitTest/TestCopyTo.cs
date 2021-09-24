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
using System;
using System.Collections.Generic;
using System.Linq;

namespace CsvTools.Tests
{
  [TestClass]
  public class TestCopyTo
  {
    private static IEnumerable<Type> GetAllICloneable(string startsWith) =>
      AppDomain.CurrentDomain.GetAssemblies()
        .Where(a => a.FullName.StartsWith(startsWith, StringComparison.Ordinal))
        .SelectMany(a => a.GetExportedTypes(), (a, t) => new { a, t })
        .Where(t1 => t1.t.IsClass && !t1.t.IsAbstract)
        .SelectMany(t1 => t1.t.GetInterfaces(), (t1, i) => new { t1, i })
        .Where(t1 => t1.i.IsGenericType && t1.i.GetGenericTypeDefinition() == typeof(ICloneable))
        .Select(t1 => t1.t1.t);

  

    [TestMethod]
    public void RunCopyTo() => UnitTestStatic.RunCopyTo(GetAllICloneable("ClassLibraryCSV"));

    /*
       [TestMethod]
       public void TestSingleClass() => RunCopyTo(new[] { typeof(Column) });
    */
  }
}