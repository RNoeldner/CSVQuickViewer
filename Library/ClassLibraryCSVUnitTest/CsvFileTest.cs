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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml;

namespace CsvTools.Tests
{
  [TestClass]
  public class CsvFileTest
  {
    private readonly CsvFile m_CsvFile = new CsvFile();

    [TestMethod]
    public void CheckDefaults()
    {
      var test = new CsvFile();
      Assert.AreEqual(test.ByteOrderMark, true, "ByteOrderMark");
      Assert.AreEqual(test.CodePageId, 65001, "CodePageId");
      Assert.IsTrue(test.ConsecutiveEmptyRows > 1, "ConsecutiveEmptyRows");
      Assert.AreEqual(test.CurrentEncoding, Encoding.UTF8, "CurrentEncoding");
      Assert.AreEqual(test.DisplayStartLineNo, true, "DisplayStartLineNo");
      Assert.AreEqual(test.DisplayEndLineNo, false, "DisplayEndLineNo");
      Assert.AreEqual(test.DisplayRecordNo, false, "DisplayRecordNo");
      Assert.AreEqual(test.FileName, string.Empty, "FileName");
      Assert.AreEqual(test.HasFieldHeader, true, "HasFieldHeader");
      Assert.AreEqual(test.ID, string.Empty, "ID");
      Assert.AreEqual(test.InOverview, false, "IsCritical");
      Assert.AreEqual(test.IsEnabled, true, "IsEnabled");
      Assert.AreEqual(test.NumWarnings, 0, "NumWarnings");
      Assert.AreEqual(test.RecordLimit, 0U, "RecordLimit");
      Assert.AreEqual(test.TreatUnknowCharaterAsSpace, false, "ReplaceUnknowCharater");
      Assert.AreEqual(test.SkipRows, 0, "SkipRows");
      Assert.AreEqual(string.Empty, test.SqlStatement, "SqlStatement");
      Assert.IsTrue(test.Timeout > 0, "Timeout");
      Assert.AreEqual(string.Empty, test.TemplateName, "TemplateName");
      Assert.IsFalse(test.TreatNBSPAsSpace, "TreatNBSPAsSpace");
      Assert.IsTrue(test.WarnEmptyTailingColumns, "WarnEmptyTailingColumns");
      Assert.IsTrue(test.WarnNBSP, "WarnNBSP");
      Assert.IsTrue(test.ShowProgress, "ShowProgress");
      Assert.IsTrue(test.DisplayStartLineNo);
      Assert.IsFalse(test.WarnLineFeed);
    }

    [TestMethod()]
    public void ToStringTest()
    {
      var setting = new CsvFile()
      {
        ID = "TestID",
        FileName = "MyTest.txt"
      };
      var result = setting.ToString();
      Assert.IsTrue(result.Contains(setting.GetType().Name));
      Assert.IsTrue(result.Contains(setting.ID));
      Assert.IsTrue(result.Contains(setting.FileName));
    }

    [TestMethod]
    public void GetEncryptedPassphraseFunction()
    {
      var test = new CsvFile
      {
        GetEncryptedPassphraseFunction = delegate () { return "Hello World"; }
      };
      Assert.IsNotNull(test.GetEncryptedPassphraseFunction);
      Assert.AreEqual("Hello World", test.GetEncryptedPassphraseFunction.Invoke());
    }

    [TestMethod]
    public void Ctor()
    {
      var test = new CsvFile();
      Assert.AreEqual(string.Empty, test.FileName, "Empty FileName");
      Assert.AreEqual(string.Empty, test.ID, "Empty ID");

      var test2 = new CsvFile(m_CsvFile.FileName);
      Assert.AreEqual(m_CsvFile.FileName, test2.FileName, "Filename2");

      var test3 = new CsvFile(".\\Test.txt");
      Assert.AreEqual("Test.txt", test3.FileName, "Filename3");
    }

    [TestMethod]
    public void CsvFileCopyTo()
    {
      var test = new CsvFile();
      m_CsvFile.CopyTo(test);
      m_CsvFile.AllPropertiesEqual(test);
      // Test Properties that are not tested
      Assert.AreEqual(m_CsvFile.MappingCollection.Count, test.MappingCollection.Count, "FieldMapping");
      Assert.AreEqual(TrimmingOption.Unquoted, test.TrimmingOption, "TrimmingOption");
      Assert.IsTrue(m_CsvFile.MappingCollection.CollectionEqualWithOrder(test.MappingCollection), "Mapping");
      Assert.IsTrue(m_CsvFile.ColumnCollection.CollectionEqualWithOrder(test.ColumnCollection), "Column");
      Assert.IsTrue(m_CsvFile.FileFormat.Equals(test.FileFormat), "FileFormat");
      Assert.IsTrue(test.Equals(m_CsvFile), "Equals");
    }

    [TestMethod]
    public void CsvFileClone()
    {
      var test = m_CsvFile.Clone();
      Assert.AreNotSame(m_CsvFile, test);
      Assert.IsInstanceOfType(test, typeof(CsvFile));

      m_CsvFile.AllPropertiesEqual(test);
      // Test Properties that are not tested

      Assert.AreEqual(m_CsvFile.MappingCollection.Count, test.MappingCollection.Count, "FieldMapping");
      Assert.AreEqual(TrimmingOption.Unquoted, test.TrimmingOption, "TrimmingOption");
      Assert.IsTrue(m_CsvFile.MappingCollection.CollectionEqualWithOrder(test.MappingCollection), "Mapping");
      //Assert.IsTrue(m_CsvFile.Errors.CollectionEqualWithOrder(test.Errors), "Errors");
      Assert.IsTrue(m_CsvFile.ColumnCollection.CollectionEqualWithOrder(test.ColumnCollection), "Column");
      Assert.IsTrue(m_CsvFile.FileFormat.Equals(test.FileFormat), "FileFormat");

      Assert.IsTrue(test.Equals(m_CsvFile), "Equals");
    }

    [TestMethod]
    public void SqlStatementCData()
    {
      var doc = new XmlDocument();
      m_CsvFile.SqlStatementCData = doc.CreateCDataSection("Hello World");

      Assert.AreEqual("Hello World", m_CsvFile.SqlStatement);
      m_CsvFile.SqlStatement = null;
      Assert.AreEqual(string.Empty, m_CsvFile.SqlStatementCData.Value);
    }

    [TestMethod]
    public void CsvFileFieldFieldMappingRemove()
    {
      var test = new CsvFile();
      var fm1 = new Mapping
      {
        FileColumn = "Source1",
        TemplateField = "Destination1"
      };
      Assert.IsFalse(test.MappingCollection.AddIfNew(null));

      Assert.IsTrue(test.MappingCollection.AddIfNew(fm1));
      Assert.IsFalse(test.MappingCollection.AddIfNew(fm1));

      Assert.AreEqual(1, test.MappingCollection.Count);

      var res = test.MappingCollection.GetByColumn("Source1");
      Assert.AreEqual(fm1, res.First());

      test.MappingCollection.RemoveColumn("Source");
      Assert.AreEqual(1, test.MappingCollection.Count);

      test.MappingCollection.RemoveColumn("Source1");
      Assert.AreEqual(0, test.MappingCollection.Count);
    }

    [TestMethod]
    public void GetFileReader()
    {
      m_CsvFile.FileName = "BasicCSV.txt";
      using (var processDisplay = new DummyProcessDisplay())
      using (var res = m_CsvFile.GetFileReader(processDisplay))
        Assert.IsInstanceOfType(res, typeof(IFileReader));
    }

    [TestMethod]
    public void GetFileWriter()
    {
      using (var processDisplay = new DummyProcessDisplay())
      {
        var res = m_CsvFile.GetFileWriter(processDisplay);
        Assert.IsInstanceOfType(res, typeof(IFileWriter));
      }
    }

    [TestMethod]
    public void CsvFileFieldMappingAddUpdate()
    {
      var test = new CsvFile();
      var fm1 = new Mapping
      {
        FileColumn = "Source",
        TemplateField = "Destination1"
      };
      var fm2 = new Mapping
      {
        FileColumn = "Source",
        TemplateField = "Destination2"
      };
      test.MappingCollection.AddIfNew(fm1);
      test.MappingCollection.AddIfNew(fm2);

      Assert.AreEqual("Destination1", test.MappingCollection.First().TemplateField);
      Assert.AreEqual("Destination2", test.MappingCollection.Last().TemplateField);
    }

    [TestMethod]
    public void CsvFileFieldMappingAddUpdateAdd()
    {
      var test = new CsvFile();
      var fm1 = new Mapping
      {
        FileColumn = "Source",
        TemplateField = "Destination1"
      };
      test.MappingCollection.AddIfNew(fm1);
      Assert.AreEqual(fm1, test.MappingCollection.First());
    }

    [TestMethod]
    public void CsvFileFieldMappingAddUpdateAdd2()
    {
      var test = new CsvFile();
      var fm1 = new Mapping
      {
        FileColumn = "Source1",
        TemplateField = "Destination1"
      };
      var fm2 = new Mapping
      {
        FileColumn = "Source2",
        TemplateField = "Destination2"
      };
      test.MappingCollection.AddIfNew(fm1);
      Assert.AreEqual(fm1, test.MappingCollection.First());
      test.MappingCollection.AddIfNew(fm2);
      Assert.AreEqual(fm2, test.MappingCollection.Last());
    }

    [TestMethod]
    public void CsvFileNotEquals()
    {
      var test = new CsvFile();
      Assert.IsFalse(test.Equals(m_CsvFile));
    }

    [TestMethod]
    public void CsvFileNotEqualsNull()
    {
      var test = new CsvFile();
      Assert.IsFalse(test.Equals(null));
    }

    [TestMethod]
    public void CsvFilePropertyChanged()
    {
      var numCalled = 0;
      var test = new CsvFile();
      test.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
      {
        Assert.AreEqual("FileName", e.PropertyName);
        numCalled++;
      };
      test.FileName = "new";
      Assert.AreEqual(numCalled, 1);
    }

    [TestMethod]
    public void FileFormatColumnFormatAddExisting()
    {
      var m_Column = new Column
      {
        Name = "Name"
      };
      m_CsvFile.ColumnCollection.AddIfNew(m_Column);
      Assert.AreEqual(2, m_CsvFile.ColumnCollection.Count);
    }

    [TestMethod]
    public void FileFormat()
    {
      var csv = m_CsvFile.Clone() as CsvFile;
      csv.FileFormat = new FileFormat()
      {
        QualifyAlways = true
      };
      Assert.IsTrue(csv.FileFormat.QualifyAlways);
      csv.FileFormat = new FileFormat()
      {
        QualifyAlways = false
      };
      Assert.IsFalse(csv.FileFormat.QualifyAlways);
    }

    [TestMethod]
    public void FileFormatColumnFormatAddNew()
    {
      var m_Column = new Column
      {
        Name = "Name2"
      };
      m_CsvFile.ColumnCollection.AddIfNew(m_Column);
      Assert.AreEqual(3, m_CsvFile.ColumnCollection.Count);
    }

    [TestMethod]
    public void Errors()
    {
      Assert.AreEqual(-1, m_CsvFile.NumErrors);
      Assert.IsFalse(m_CsvFile.ErrorsSpecified);
      Assert.AreEqual(0, m_CsvFile.Errors.Count);
      m_CsvFile.Errors.Add(new SampleRecordEntry(177, "Error"));
      Assert.AreEqual(1, m_CsvFile.NumErrors);
      m_CsvFile.NumErrors = 100;
      Assert.AreEqual(100, m_CsvFile.NumErrors);
    }

    [TestInitialize]
    public void Init()
    {
      m_CsvFile.ByteOrderMark = false;
      Assert.AreEqual(false, m_CsvFile.ByteOrderMark, "ByteOrderMark");

      m_CsvFile.CodePageId = 1;
      Assert.AreEqual(1, m_CsvFile.CodePageId, "CodePageId");

      m_CsvFile.CurrentEncoding = Encoding.UTF8;
      Assert.AreEqual(Encoding.UTF8, m_CsvFile.CurrentEncoding, "CurrentEncoding");

      m_CsvFile.ConsecutiveEmptyRows = 1;
      m_CsvFile.DisplayStartLineNo = false;
      m_CsvFile.DisplayEndLineNo = true;
      m_CsvFile.DisplayRecordNo = true;
      m_CsvFile.FileName = "FileName";
      m_CsvFile.HasFieldHeader = false;
      m_CsvFile.ID = "ID";
      m_CsvFile.InOverview = true;
      m_CsvFile.IsEnabled = false;
      m_CsvFile.NumWarnings = 5;
      Assert.AreEqual(5, m_CsvFile.NumWarnings, "NumWarnings");

      m_CsvFile.RecordLimit = 5;
      m_CsvFile.TreatUnknowCharaterAsSpace = true;
      Assert.AreEqual(true, m_CsvFile.TreatUnknowCharaterAsSpace, "TreatUnknowCharaterAsSpace");

      m_CsvFile.ShowProgress = true;
      m_CsvFile.SkipRows = 1;
      m_CsvFile.SqlStatement = "SqlStatement";
      m_CsvFile.Timeout = 5;
      m_CsvFile.TemplateName = "TemplateName";
      m_CsvFile.WarnLineFeed = false;
      Assert.IsFalse(m_CsvFile.WarnLineFeed);
      m_CsvFile.AlternateQuoting = false;

      Assert.IsFalse(m_CsvFile.TreatNBSPAsSpace, "TreatNBSPAsSpace");
      //Assert.IsFalse(m_CsvFile.TreatNBSPAsUnknowCharater, "TreatNBSPAsUnknowCharater");

      m_CsvFile.ShowProgress = true;
      Assert.IsTrue(m_CsvFile.ShowProgress);

      //m_CsvFile.TreatNBSPAsUnknowCharater = true;
      //Assert.IsFalse(m_CsvFile.TreatNBSPAsSpace, "TreatNBSPAsSpace");
      //Assert.IsTrue(m_CsvFile.TreatNBSPAsUnknowCharater, "TreatNBSPAsUnknowCharater");

      m_CsvFile.TreatNBSPAsSpace = true;
      Assert.IsTrue(m_CsvFile.TreatNBSPAsSpace, "TreatNBSPAsSpace");
      //Assert.IsFalse(m_CsvFile.TreatNBSPAsUnknowCharater, "TreatNBSPAsUnknowCharater");

      m_CsvFile.TrimmingOption = TrimmingOption.Unquoted;
      //m_CsvFile.UnknowCharaterReplacement = '/';
      m_CsvFile.WarnDelimiterInValue = true;
      Assert.IsTrue(m_CsvFile.WarnDelimiterInValue, "WarnDelimiterInValue");

      m_CsvFile.MappingCollection.Clear();
      m_CsvFile.MappingCollection.Add(new Mapping { FileColumn = "Fld1", TemplateField = "FldA", Attention = true, Update = true });
      m_CsvFile.MappingCollection.Add(new Mapping { FileColumn = "Fld2", TemplateField = "FldB", Attention = true });
      Assert.AreEqual(2, m_CsvFile.MappingCollection.Count, "FieldMapping");

      m_CsvFile.ColumnCollection.Clear();
      m_CsvFile.ColumnCollection.AddIfNew(new Column
      {
        ColumnOrdinal = 1,
        DataType = DataType.Integer,
        Ignore = false,
        Convert = true,
        Name = "ID"
      });
      m_CsvFile.ColumnCollection.AddIfNew(new Column { ColumnOrdinal = 2, Name = "Name" });

      m_CsvFile.WarnEmptyTailingColumns = false;
      Assert.IsFalse(m_CsvFile.WarnEmptyTailingColumns, "WarnEmptyTailingColumns");

      m_CsvFile.WarnNBSP = false;
      Assert.IsFalse(m_CsvFile.WarnNBSP, "WarnNBSP");

      m_CsvFile.WarnQuotes = false;
      Assert.IsFalse(m_CsvFile.WarnQuotes, "WarnQuotes");

      m_CsvFile.WarnQuotesInQuotes = true;
      Assert.IsTrue(m_CsvFile.WarnQuotesInQuotes, "WarnQuotesInQuotes");
      m_CsvFile.NoDelimitedFile = true;
      Assert.IsTrue(m_CsvFile.NoDelimitedFile, "NoDelimitedFile");
      m_CsvFile.WarnUnknowCharater = false;
      Assert.IsFalse(m_CsvFile.WarnUnknowCharater, "WarnUnknowCharater");

      Assert.IsFalse(m_CsvFile.ByteOrderMark, "ByteOrderMark");

      Assert.AreEqual(1, m_CsvFile.ConsecutiveEmptyRows, "ConsecutiveEmptyRows");
      Assert.IsFalse(m_CsvFile.DisplayStartLineNo, "DisplayStartLineNo");
      Assert.IsTrue(m_CsvFile.DisplayEndLineNo, "DisplayEndLineNo");
      Assert.IsTrue(m_CsvFile.DisplayRecordNo, "DisplayRecordNo");
      Assert.AreEqual("FileName", m_CsvFile.FileName, "FileName");
      Assert.IsTrue(m_CsvFile.WarnDelimiterInValue, "WarnDelimiterInValue");
      Assert.IsFalse(m_CsvFile.HasFieldHeader, "HasFieldHeader");
      Assert.AreEqual("ID", m_CsvFile.ID, "ID");
      Assert.IsTrue(m_CsvFile.InOverview, "IsCritical");
      Assert.IsFalse(m_CsvFile.IsEnabled, "IsEnabled");
      Assert.AreEqual(TrimmingOption.Unquoted, m_CsvFile.TrimmingOption, "TrimmingOption");
      Assert.AreEqual(5, m_CsvFile.NumWarnings, "NumWarnings");

      Assert.AreEqual(5U, m_CsvFile.RecordLimit, "RecordLimit");
      Assert.IsTrue(m_CsvFile.TreatUnknowCharaterAsSpace, "ReplaceUnknowCharater");
      Assert.IsTrue(m_CsvFile.ShowProgress, "ShowProgress");
      Assert.AreEqual(1, m_CsvFile.SkipRows, "SkipRows");
      Assert.AreEqual("SqlStatement", m_CsvFile.SqlStatement, "SqlStatement");
      Assert.AreEqual("SqlStatement", m_CsvFile.SqlStatementCData.InnerText, "SqlStatementCData");
      Assert.IsTrue(m_CsvFile.SqlStatementCDataSpecified, "SqlStatementCDataSpecified");

      Assert.AreEqual(5, m_CsvFile.Timeout, "Timeout");
      Assert.AreEqual("TemplateName", m_CsvFile.TemplateName, "TemplateName");

      //Assert.AreEqual('/', m_CsvFile.UnknowCharaterReplacement, "UnknowCharaterReplacement");
      Assert.IsFalse(m_CsvFile.WarnEmptyTailingColumns, "WarnEmptyTailingColumns");
      Assert.IsFalse(m_CsvFile.WarnNBSP, "WarnNBSP");
      Assert.IsTrue(m_CsvFile.WarnQuotesInQuotes, "WarnQuotesInQuotes");
      Assert.IsFalse(m_CsvFile.WarnQuotes, "WarnQuotes");

      Assert.IsFalse(m_CsvFile.WarnUnknowCharater, "WarnUnknowCharater");
    }
  }
}