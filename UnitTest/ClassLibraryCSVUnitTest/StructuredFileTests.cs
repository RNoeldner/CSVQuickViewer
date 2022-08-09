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
using System.IO;

namespace CsvTools.Tests
{
  [TestClass]
  public class JsonFileTests
  {
    private readonly JsonFile m_JsonFile = new JsonFile() { Row = "{0}" };

    [TestMethod]
    public void StructuredFileClone()
    {
      var test = (JsonFile) m_JsonFile.Clone();
      Assert.AreNotSame(m_JsonFile, test);
      Assert.IsInstanceOfType(test, typeof(JsonFile));

      m_JsonFile.CheckAllPropertiesEqual(test);
      // Test Properties that are not tested

      Assert.AreEqual(m_JsonFile.MappingCollection.Count, test.MappingCollection.Count, "FieldMapping");
      Assert.AreEqual(TrimmingOptionEnum.Unquoted, test.TrimmingOption, "TrimmingOption");
      Assert.IsTrue(m_JsonFile.MappingCollection.CollectionEqualWithOrder(test.MappingCollection), "Mapping");
      Assert.IsTrue(m_JsonFile.ColumnCollection.CollectionEqualWithOrder(test.ColumnCollection), "Column");

      Assert.IsTrue(test.Equals(m_JsonFile), "Equals");
    }

    [TestMethod]
    public void CsvFileCopyTo()
    {
      var test = new JsonFile();
      m_JsonFile.CopyTo(test);
      m_JsonFile.CheckAllPropertiesEqual(test);
      // Test Properties that are not tested
      Assert.AreEqual(m_JsonFile.MappingCollection.Count, test.MappingCollection.Count, "FieldMapping");
      Assert.AreEqual(TrimmingOptionEnum.Unquoted, test.TrimmingOption, "TrimmingOption");
      Assert.IsTrue(m_JsonFile.MappingCollection.CollectionEqualWithOrder(test.MappingCollection), "Mapping");
      Assert.IsTrue(m_JsonFile.ColumnCollection.CollectionEqualWithOrder(test.ColumnCollection),
        "ColumnCollection");
      Assert.IsTrue(test.Equals(m_JsonFile), "Equals");
    }

    [TestMethod]
    public void GetFileReader()
    {
      try
      {
        using var dummy = FunctionalDI.GetFileReader(m_JsonFile, UnitTestStatic.Token);
        Assert.Fail("Should throw error");
      }
      catch (NotImplementedException)
      {
      }
      catch (FileNotFoundException)
      {
      }
    }

    [TestMethod]
    public void GetFileWriter()
    {
      var jsonFile = new JsonFile("SomeFileName.json") { Row = "{0}" };
      m_JsonFile.SqlStatement = "dummy";
      var res = FunctionalDI.GetFileWriter(jsonFile,  UnitTestStatic.Token);
      Assert.IsInstanceOfType(res, typeof(IFileWriter));
    }

    [TestInitialize]
    public void Init()
    {
      m_JsonFile.FileName = "StructuredFile.txt";
      m_JsonFile.Header = "Header";
      Assert.AreEqual("Header", m_JsonFile.Header);
#pragma warning disable 8625
      m_JsonFile.Header = null;
#pragma warning restore 8625
      Assert.AreEqual(string.Empty, m_JsonFile.Header);

      m_JsonFile.Row = "Row";
      Assert.AreEqual("Row", m_JsonFile.Row);
#pragma warning disable CS8625
      m_JsonFile.Row = null;
#pragma warning restore CS8625
      Assert.AreEqual(string.Empty, m_JsonFile.Row);

      m_JsonFile.Footer = "Footer";
      Assert.AreEqual("Footer", m_JsonFile.Footer);
#pragma warning disable CS8625 
      m_JsonFile.Footer = null;
#pragma warning restore CS8625 
      Assert.AreEqual(string.Empty, m_JsonFile.Footer);

      m_JsonFile.MappingCollection.Clear();
      m_JsonFile.MappingCollection.Add(new Mapping("Fld1", "FldA", false, true));
      m_JsonFile.MappingCollection.Add(new Mapping("Fld2", "FldB", false, true));
      Assert.AreEqual(2, m_JsonFile.MappingCollection.Count, "FieldMapping");

      m_JsonFile.ColumnCollection.Clear();
      m_JsonFile.ColumnCollection.Add(new Column("ID", DataTypeEnum.Integer) { ColumnOrdinal = 1, Ignore = false, Convert = true });
      m_JsonFile.ColumnCollection.Add(new Column { ColumnOrdinal = 2, Name = "Name" });
    }

    [TestMethod]
    public void StructuredFileTestCtor()
    {
      var test = new JsonFile();
      Assert.IsTrue(string.IsNullOrEmpty(test.FileName));

      var test2 = new JsonFile("Hello");
      Assert.AreEqual("Hello", test2.FileName);
    }
  }
}