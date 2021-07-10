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
using System.Data;
using System.Linq;

namespace CsvTools.Tests
{
  [TestClass]
  public class ErrorInformationTests : IDisposable
  {
    private readonly DataTable m_DataTable = new DataTable();

    public void Dispose() => m_DataTable.Dispose();

    [TestInitialize]
    public void Init()
    {
      m_DataTable.Clear();
      m_DataTable.Columns.Clear();
      m_DataTable.Columns.Add(new DataColumn { ColumnName = "ID", AutoIncrement = true });
      m_DataTable.Columns.Add(new DataColumn { ColumnName = "Fld1" });
      m_DataTable.Columns.Add(new DataColumn { ColumnName = "Fld2" });
      m_DataTable.Columns.Add(new DataColumn { ColumnName = "Fld3" });
      m_DataTable.Columns.Add(new DataColumn { ColumnName = "Fld4" });
    }

    [TestMethod]
    public void GetErrorInformationTest()
    {
      var row = m_DataTable.NewRow();
      Assert.AreEqual(string.Empty, row.GetErrorInformation());
      row.RowError = "RowError";
      Assert.AreEqual("[] RowError", row.GetErrorInformation());
    }

    [TestMethod]
    public void ReadErrorInformationTestSetErrorInformation()
    {
      var columnErrors = new ColumnErrorDictionary();

      var colNames = new List<string>();
      foreach (DataColumn col in m_DataTable.Columns)
        colNames.Add(col.ColumnName);

      Assert.IsTrue(string.IsNullOrEmpty(ErrorInformation.ReadErrorInformation(columnErrors, colNames)));

      columnErrors.Add(-1, "Error on Row");
      columnErrors.Add(0, "Error on Column".AddWarningId());
      columnErrors.Add(1, "Error on ColumnA");
      columnErrors.Add(1, "Another Error on Column");

      columnErrors.Add(2, "Warning on ColumnB".AddWarningId());

      columnErrors.Add(3, "Warning on Fld4".AddWarningId());
      columnErrors.Add(3, "Error on Fld4");

      var errorInfo = ErrorInformation.ReadErrorInformation(columnErrors, colNames);
      Assert.IsNotNull(errorInfo);

      var row = m_DataTable.NewRow();
      row.SetErrorInformation(errorInfo);
      Assert.AreEqual("Error on Row", row.RowError);
      Assert.AreEqual("Error on Column", row.GetColumnError(0).WithoutWarningId());
      Assert.AreEqual("Error on ColumnA\nAnother Error on Column", row.GetColumnError(1));
      Assert.AreEqual("Warning on ColumnB", row.GetColumnError(2).WithoutWarningId());

      var res = errorInfo.GetErrorsAndWarnings();
      Assert.AreEqual(4, res.Item1.Count(x => x == ErrorInformation.cSeparator) + 1);
      Assert.AreEqual(3, res.Item2.Count(x => x == ErrorInformation.cSeparator) + 1);
    }

    [TestMethod]
    public void CombineColumnAndErrorTest() =>
      Assert.AreEqual("[Col1] Error", ErrorInformation.CombineColumnAndError("Col1", "Error"));

    [TestMethod]
    public void ReadErrorInformationFromDataRow()

    {
      var dr = m_DataTable.NewRow();

      dr[0] = 1;
      dr[1] = 2;
      dr[2] = 3;

      Assert.AreEqual(string.Empty, dr.GetErrorInformation());
      dr.RowError = "Test";
      Assert.AreEqual("[] Test", dr.GetErrorInformation());
      dr.SetColumnError(1, "Hello");
      Assert.IsTrue(dr.GetErrorInformation().Contains(dr.RowError));
      Assert.IsTrue(dr.GetErrorInformation().Contains(dr.GetColumnError(1)));
    }

    [TestMethod]
    public void WriteErrorInformationToDataRowColAndRow()
    {
      var dr = m_DataTable.NewRow();
      dr[0] = 1;
      dr[1] = 2;
      dr[2] = 3;
      dr.SetColumnError(1, "ColumnError");
      dr.RowError = "RowError";

      var dr2 = m_DataTable.NewRow();

      dr2.SetErrorInformation(dr.GetErrorInformation());
      Assert.AreEqual(dr.RowError, dr2.RowError);
      Assert.AreEqual(dr.GetColumnError(0), dr2.GetColumnError(0), "Col0");
      Assert.AreEqual(dr.GetColumnError(1), dr2.GetColumnError(1), "Col1");
      Assert.AreEqual(dr.GetColumnError(2), dr2.GetColumnError(2), "Col2");
    }

    [TestMethod]
    public void WriteErrorInformationToDataRowColOnly()
    {
      var dr = m_DataTable.NewRow();
      dr[0] = 1;
      dr[1] = 2;
      dr[2] = 3;
      dr.SetColumnError(1, "Hello");

      var dr2 = m_DataTable.NewRow();

      dr2.SetErrorInformation(dr.GetErrorInformation());
      Assert.AreEqual(dr.RowError, dr2.RowError);
      Assert.AreEqual(dr.GetColumnError(0), dr2.GetColumnError(0), "Col0");
      Assert.AreEqual(dr.GetColumnError(1), dr2.GetColumnError(1), "Col1");
      Assert.AreEqual(dr.GetColumnError(2), dr2.GetColumnError(2), "Col2");
    }

    [TestMethod]
    public void WriteErrorInformationToDataRowRowOnly()
    {
      var dr = m_DataTable.NewRow();
      dr[0] = 1;
      dr[1] = 2;
      dr[2] = 3;
      dr.RowError = "Test";

      var dr2 = m_DataTable.NewRow();

      dr2.SetErrorInformation(dr.GetErrorInformation());
      Assert.AreEqual(dr.RowError, dr2.RowError);
      Assert.AreEqual(dr.GetColumnError(0), dr2.GetColumnError(0));
      Assert.AreEqual(dr.GetColumnError(1), dr2.GetColumnError(1));
      Assert.AreEqual(dr.GetColumnError(2), dr2.GetColumnError(2));
    }
  }
}