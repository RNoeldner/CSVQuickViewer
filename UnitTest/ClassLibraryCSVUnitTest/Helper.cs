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
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CsvTools.Tests
{
  internal static class Helper
  {
    internal static void AllPropertiesEqual(this object a, object b)
    {
      var properties = a.GetType().GetProperties().Where(prop => prop.GetMethod != null &&
                                                                 (prop.PropertyType == typeof(int) ||
                                                                  prop.PropertyType == typeof(long) ||
                                                                  prop.PropertyType == typeof(string) ||
                                                                  prop.PropertyType == typeof(bool) ||
                                                                  prop.PropertyType == typeof(DateTime)));
      foreach (var prop in properties)
        Assert.AreEqual(prop.GetValue(a), prop.GetValue(b),
          $"Type: {a.GetType().FullName}  Property:{prop.Name}");
    }

    internal static CsvFile ReaderGetAllFormats(string id = "AllFormats")
    {
      var readFile = new CsvFile
      {
        ID = id,
        FileName = Path.Combine(UnitTestInitialize.GetTestPath("AllFormats.txt")),
        HasFieldHeader = true,
        FileFormat = { FieldDelimiter = "TAB" }
      };

      var timeFld = readFile.ColumnCollection.AddIfNew(new Column("DateTime", DataType.DateTime));
      Debug.Assert(timeFld != null);
      timeFld.ValueFormat.DateFormat = @"dd/MM/yyyy";
      timeFld.TimePart = "Time";
      timeFld.TimePartFormat = "HH:mm:ss";
      readFile.ColumnCollection.AddIfNew(new Column("Integer", DataType.Integer));

      readFile.ColumnCollection.AddIfNew(new Column("Numeric", DataType.Numeric));

      var numericFld = readFile.ColumnCollection.Get("Numeric");
      Debug.Assert(numericFld != null);
      numericFld.ValueFormat.DecimalSeparator = ".";

      var doubleFld = readFile.ColumnCollection.AddIfNew(new Column("Double", DataType.Double));
      Debug.Assert(doubleFld != null);
      doubleFld.ValueFormat.DecimalSeparator = ".";
      readFile.ColumnCollection.AddIfNew(new Column("Boolean", DataType.Boolean));
      readFile.ColumnCollection.AddIfNew(new Column("GUID", DataType.Guid));

      var timeFld2 = readFile.ColumnCollection.AddIfNew(new Column("Time", DataType.DateTime));
      timeFld2.ValueFormat.DateFormat = "HH:mm:ss";
      timeFld2.Ignore = true;

      return readFile;
    }

    internal static CsvFile ReaderGetBasicCSV(string id = "BasicCSV")
    {
      var readFile = new CsvFile
      {
        ID = id,
        FileFormat = {CommentLine = "#"},
        FileName = Path.Combine(UnitTestInitialize.GetTestPath("BasicCSV.txt"))
      };
      var examDateFld = readFile.ColumnCollection.AddIfNew(new Column("ExamDate", DataType.DateTime));

      Debug.Assert(examDateFld != null);
      examDateFld.ValueFormat.DateFormat = @"dd/MM/yyyy";

      readFile.ColumnCollection.AddIfNew(new Column("Score", DataType.Integer));

      readFile.ColumnCollection.AddIfNew(new Column("Proficiency", DataType.Numeric));

      readFile.ColumnCollection.AddIfNew(new Column("IsNativeLang", DataType.Boolean));

      return readFile;
    }
  }
}