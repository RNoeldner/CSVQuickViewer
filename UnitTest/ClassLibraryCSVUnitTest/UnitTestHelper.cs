﻿/*
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

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  public static class UnitTestHelper
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

    public static CsvFile ReaderGetAllFormats(string id = "AllFormats")
    {
      var readFile = new CsvFile
      {
        ID = id,
        FileName = Path.Combine(UnitTestInitializeCsv.GetTestPath("AllFormats.txt")),
        HasFieldHeader = true,
        FileFormat = {FieldDelimiter = "TAB"}
      };

      var timeFld = new Column("DateTime", new ValueFormat(DataType.DateTime) {DateFormat = @"dd/MM/yyyy"});
      readFile.ColumnCollection.AddIfNew(timeFld);

      timeFld.TimePart = "Time";
      timeFld.TimePartFormat = "HH:mm:ss";
      readFile.ColumnCollection.AddIfNew(new Column("Integer", DataType.Integer));

      readFile.ColumnCollection.AddIfNew(new Column("Numeric", DataType.Numeric));

      var numericFld = readFile.ColumnCollection.Get("Numeric");
      Debug.Assert(numericFld != null);
      numericFld.ValueFormatMutable.DecimalSeparator = ".";

      var doubleFld = new Column("Double", new ValueFormat(DataType.Double) {DecimalSeparator = "."});
      readFile.ColumnCollection.AddIfNew(doubleFld);
      Debug.Assert(doubleFld != null);
      readFile.ColumnCollection.AddIfNew(new Column("Boolean", DataType.Boolean));
      readFile.ColumnCollection.AddIfNew(new Column("GUID", DataType.Guid));

      var timeFld2 = new Column("Time",
        new ValueFormat(DataType.DateTime) {DateFormat = "HH:mm:ss"}) {Ignore = true};
      readFile.ColumnCollection.AddIfNew(timeFld2);

      return readFile;
    }

    public static CsvFile ReaderGetBasicCSV(string id = "BasicCSV")
    {
      var readFile = new CsvFile
      {
        ID = id,
        FileFormat = {CommentLine = "#"},
        FileName = Path.Combine(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"))
      };
      var examDateFld = new Column("ExamDate", DataType.DateTime);
      readFile.ColumnCollection.AddIfNew(examDateFld);

      Debug.Assert(examDateFld != null);
      examDateFld.ValueFormatMutable.DateFormat = @"dd/MM/yyyy";

      readFile.ColumnCollection.AddIfNew(new Column("Score", DataType.Integer));

      readFile.ColumnCollection.AddIfNew(new Column("Proficiency", DataType.Numeric));

      readFile.ColumnCollection.AddIfNew(new Column("IsNativeLang", DataType.Boolean));

      return readFile;
    }
  }
}