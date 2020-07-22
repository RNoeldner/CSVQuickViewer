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

namespace CsvTools.Tests
{
  [TestClass]
  public class StructuredFileTests
  {
    private readonly StructuredFile m_StructuredFile = new StructuredFile();

    [TestMethod]
    public void StructuredFileClone()
    {
      var test = m_StructuredFile.Clone();
      Assert.AreNotSame(m_StructuredFile, test);
      Assert.IsInstanceOfType(test, typeof(StructuredFile));

      m_StructuredFile.AllPropertiesEqual(test);
      // Test Properties that are not tested

      Assert.AreEqual(m_StructuredFile.MappingCollection.Count, test.MappingCollection.Count, "FieldMapping");
      Assert.AreEqual(TrimmingOption.Unquoted, test.TrimmingOption, "TrimmingOption");
      Assert.IsTrue(m_StructuredFile.MappingCollection.CollectionEqualWithOrder(test.MappingCollection), "Mapping");
      Assert.IsTrue(m_StructuredFile.ColumnCollection.CollectionEqualWithOrder(test.ColumnCollection), "Column");
      Assert.IsTrue(m_StructuredFile.FileFormat.Equals(test.FileFormat), "FileFormat");

      Assert.IsTrue(test.Equals(m_StructuredFile), "Equals");
    }

    [TestMethod]
    public void CsvFileCopyTo()
    {
      var test = new StructuredFile();
      m_StructuredFile.CopyTo(test);
      m_StructuredFile.AllPropertiesEqual(test);
      // Test Properties that are not tested
      Assert.AreEqual(m_StructuredFile.MappingCollection.Count, test.MappingCollection.Count, "FieldMapping");
      Assert.AreEqual(TrimmingOption.Unquoted, test.TrimmingOption, "TrimmingOption");
      Assert.IsTrue(m_StructuredFile.MappingCollection.CollectionEqualWithOrder(test.MappingCollection), "Mapping");
      Assert.IsTrue(m_StructuredFile.ColumnCollection.CollectionEqualWithOrder(test.ColumnCollection),
        "ColumnCollection");
      Assert.IsTrue(m_StructuredFile.FileFormat.Equals(test.FileFormat), "FileFormat");
      Assert.IsTrue(test.Equals(m_StructuredFile), "Equals");
    }

    [TestMethod]
    public void GetFileReader()
    {
      try
      {
        using (var dummy = FunctionalDI.GetFileReader(m_StructuredFile, TimeZoneInfo.Local.Id, null))
        {
          Assert.Fail("Should throw error");
        }
      }
      catch (NotImplementedException)
      {
      }
    }

    [TestMethod]
    public void GetFileWriter()
    {
      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      {
        m_StructuredFile.SqlStatement = "dummy";
        var res = FunctionalDI.GetFileWriter(m_StructuredFile, processDisplay);
        Assert.IsInstanceOfType(res, typeof(IFileWriter));
      }
    }

    [TestInitialize]
    public void Init()
    {
      m_StructuredFile.FileName = "StructuredFile.txt";
      m_StructuredFile.Header = "Header";
      Assert.AreEqual("Header", m_StructuredFile.Header);
      m_StructuredFile.Header = null;
      Assert.AreEqual(string.Empty, m_StructuredFile.Header);

      m_StructuredFile.JSONEncode = false;
      Assert.IsFalse(m_StructuredFile.JSONEncode);

      m_StructuredFile.Row = "Row";
      Assert.AreEqual("Row", m_StructuredFile.Row);
      m_StructuredFile.Row = null;
      Assert.AreEqual(string.Empty, m_StructuredFile.Row);

      m_StructuredFile.Footer = "Footer";
      Assert.AreEqual("Footer", m_StructuredFile.Footer);
      m_StructuredFile.Footer = null;
      Assert.AreEqual(string.Empty, m_StructuredFile.Footer);

      m_StructuredFile.MappingCollection.Clear();
      m_StructuredFile.MappingCollection.Add(new Mapping("Fld1", "FldA", false, true));
      m_StructuredFile.MappingCollection.Add(new Mapping("Fld2", "FldB", false, true));
      Assert.AreEqual(2, m_StructuredFile.MappingCollection.Count, "FieldMapping");

      m_StructuredFile.ColumnCollection.Clear();
      m_StructuredFile.ColumnCollection.AddIfNew(new Column("ID", DataType.Integer)
      {
        ColumnOrdinal = 1,
        Ignore = false,
        Convert = true
      });
      m_StructuredFile.ColumnCollection.AddIfNew(new Column { ColumnOrdinal = 2, Name = "Name" });
    }

    [TestMethod]
    public void StructuredFileTestCTOR()
    {
      var test = new StructuredFile();
      Assert.IsTrue(string.IsNullOrEmpty(test.FileName));

      var test2 = new StructuredFile("Hello");
      Assert.AreEqual("Hello", test2.FileName);
    }
  }
}