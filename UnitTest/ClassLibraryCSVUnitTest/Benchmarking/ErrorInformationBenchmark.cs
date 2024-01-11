using CsvTools;
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
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CsvTools.Tests
{
  [TestClass, Ignore]
  public class ErrorInformationBenchmark
  {
    private static readonly ColumnErrorDictionary columnErrors = new ColumnErrorDictionary();
    private static readonly ColumnErrorDictionary columnErrors2 = new ColumnErrorDictionary();
    private static readonly ColumnErrorDictionary columnErrors3 = new ColumnErrorDictionary();
    private static readonly List<string> colNames = new List<string>(new string[] { "Col1", "Col2", "Col3", "Col4" });

    public ErrorInformationBenchmark()
    {
      columnErrors.Add(-1, "Error on Row");
      columnErrors.Add(0, "Error on Column".AddWarningId());
      columnErrors.Add(1, "Error on ColumnA");
      columnErrors.Add(1, "Another Error on Column");
      columnErrors.Add(2, "Warning on ColumnB".AddWarningId());
      columnErrors.Add(3, "Warning on Fld4".AddWarningId());
      columnErrors.Add(3, "Error on Fld4");

      columnErrors2.Add(0, "ErrorA");
      columnErrors2.Add(0, "ErrorB".AddWarningId());
      columnErrors2.Add(0, "ErrorC");

      columnErrors.Add(-1, "ErrorRow");
      columnErrors3.Add(0, "ErrorA");
      columnErrors3.Add(1, "ErrorB1");
      columnErrors3.Add(1, "ErrorB2".AddWarningId());
      columnErrors3.Add(2, "ErrorC1");
      columnErrors3.Add(2, "ErrorC2");
      columnErrors3.Add(3, "ErrorD1".AddWarningId());
      columnErrors3.Add(3, "ErrorD2");
      columnErrors3.Add(3, "ErrorD3");

    }

    //[TestMethod]
    //public void ReadErrorInformationTestSetErrorInformationOld()
    //{
    //  for (int i = 0; i<10000; i++)
    //  {
    //    var errorInfo1 = ErrorInformation.ReadErrorInformationOld(columnErrors, (i) => i>=0 && i <= colNames.Count ? colNames[i] : string.Empty);
    //    var errorInfo2 = ErrorInformation.ReadErrorInformationOld(columnErrors2, (i) => i>=0 && i <= colNames.Count ? colNames[i] : string.Empty);
    //    var errorInfo3 = ErrorInformation.ReadErrorInformationOld(columnErrors3, (i) => i>=0 && i <= colNames.Count ? colNames[i] : string.Empty);
    //  }
    //}

    [TestMethod]
    public void ReadErrorInformationTestSetErrorInformationNew()
    {
      for (int i = 0; i<10000; i++)
      {
        var errorInfo1 = ErrorInformation.ReadErrorInformation(columnErrors, (i) => i>=0 && i <= colNames.Count ? colNames[i] : string.Empty);
        var errorInfo2 = ErrorInformation.ReadErrorInformation(columnErrors2, (i) => i>=0 && i <= colNames.Count ? colNames[i] : string.Empty);
        var errorInfo3 = ErrorInformation.ReadErrorInformation(columnErrors3, (i) => i>=0 && i <= colNames.Count ? colNames[i] : string.Empty);
      }
    }

  }
}