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

namespace CsvTools.Tests
{
  [TestClass]
  public class FileFormatTest
  {
    private readonly CsvFile m_CsvFile = new CsvFile("id");

    [TestMethod]
    public void CloneTest()
    {
      var clone = m_CsvFile.Clone();
      Assert.AreNotSame(clone, m_CsvFile);
      m_CsvFile.CheckAllPropertiesEqual(clone);
    }

    [TestMethod]
    public void FileFormatCheckDefaults()
    {
      var test = new CsvFile("1");
      //Assert.AreEqual("#", test.CommentLine, "CommentLine");
      Assert.AreEqual(string.Empty, test.DelimiterPlaceholder, "DelimiterPlaceholder");
      Assert.AreEqual(",", test.FieldDelimiter, "FieldDelimiter");
      Assert.AreEqual("\"", test.FieldQualifier, "FieldQualifier");
    }

    [TestMethod]
    public void FileFormatCopyTo()
    {
      var target = new CsvFile("2");
      m_CsvFile.CopyTo(target);
      Assert.AreEqual("##", target.CommentLine, "CommentLine");
      Assert.AreEqual("\\", m_CsvFile.EscapePrefix, "EscapeCharacter");
    }

    [TestMethod]
    public void FileFormatEscapeCharacter()
    {
      m_CsvFile.EscapePrefix = "\\";
      Assert.AreEqual("\\", m_CsvFile.EscapePrefix);

      m_CsvFile.EscapePrefix = "+";
      Assert.AreEqual("+", m_CsvFile.EscapePrefix);

      m_CsvFile.EscapePrefix = "";
      Assert.AreEqual("", m_CsvFile.EscapePrefix);
    }

    [TestMethod]
    public void FileFormatFieldDelimiter()
    {
      m_CsvFile.FieldDelimiter = "Tab";
      Assert.AreEqual("tab", m_CsvFile.FieldDelimiter, true);
      Assert.AreEqual('\t', m_CsvFile.FieldDelimiterChar);

      m_CsvFile.FieldDelimiter = "comma";
      Assert.AreEqual(",", m_CsvFile.FieldDelimiter, true);
      Assert.AreEqual(',', m_CsvFile.FieldDelimiterChar);

      m_CsvFile.FieldDelimiter = "Pipe";
      Assert.AreEqual("|", m_CsvFile.FieldDelimiter, true);
      Assert.AreEqual('|', m_CsvFile.FieldDelimiterChar);
    }

    [TestMethod]
    public void FileFormatFieldQualifier()
    {
      m_CsvFile.FieldQualifier = "Tab";

      Assert.AreEqual(m_CsvFile.FieldQualifier, "tab", true);
      Assert.AreEqual(m_CsvFile.FieldQualifierChar, '\t');

      m_CsvFile.FieldQualifier = "+";
      Assert.AreEqual(m_CsvFile.FieldQualifier, "+", true);
      Assert.AreEqual(m_CsvFile.FieldQualifierChar, '+');

      m_CsvFile.FieldQualifier = "";
      Assert.AreEqual(m_CsvFile.FieldQualifier, "", true);
      Assert.AreEqual(m_CsvFile.FieldQualifierChar, char.MinValue);
    }

    [TestInitialize]
    public void FileFormatInit()
    {
      //m_FileFormat.ColumnFormat = null;
      m_CsvFile.CommentLine = "##";
      m_CsvFile.DelimiterPlaceholder = "{d}";
      m_CsvFile.EscapePrefix = "\\";
      m_CsvFile.FieldDelimiter = "|";
      m_CsvFile.FieldQualifier = "#";
      m_CsvFile.EscapePrefix = "\\";
      m_CsvFile.NewLine = RecordDelimiterTypeEnum.Lf;
      m_CsvFile.NewLinePlaceholder = "{n}";
      m_CsvFile.QualifyOnlyIfNeeded = false;
      m_CsvFile.QualifyAlways = true;
      m_CsvFile.QualifierPlaceholder = "{q}";

      Assert.IsFalse(m_CsvFile.QualifyOnlyIfNeeded, "QualifyOnlyIfNeeded");
      Assert.IsTrue(m_CsvFile.QualifyAlways, "QualifyAlways");
      Assert.AreEqual(RecordDelimiterTypeEnum.Lf, m_CsvFile.NewLine, "NewLine");
      Assert.AreEqual("##", m_CsvFile.CommentLine, "CommentLine");
      Assert.AreEqual("{d}", m_CsvFile.DelimiterPlaceholder, "DelimiterPlaceholder");
      Assert.AreEqual("{n}", m_CsvFile.NewLinePlaceholder, "NewLinePlaceholder");
      Assert.AreEqual("{q}", m_CsvFile.QualifierPlaceholder, "QuotePlaceholder");
      Assert.AreEqual("|", m_CsvFile.FieldDelimiter, "FieldDelimiter");
      Assert.AreEqual("#", m_CsvFile.FieldQualifier, "FieldQualifier");
      Assert.AreEqual("\\", m_CsvFile.EscapePrefix, "EscapeCharacter");
    }

    [TestMethod]
    public void FileFormatNotEquals()
    {
      var target = new CsvFile("1");
      Assert.IsFalse(m_CsvFile.Equals(target));
    }

    [TestMethod]
    public void FileFormatNotEqualsNull() => Assert.IsFalse(m_CsvFile.Equals(null));

    [TestMethod]
    public void FileFormatNotEqualsSelf() => Assert.IsTrue(m_CsvFile.Equals(m_CsvFile));

    [TestMethod]
    public void FileFormatPropertyChanged()
    {
      var numCalled = 0;
      var test = new CsvFile("1");
      test.PropertyChanged += (_, e) =>
      {
        Assert.AreEqual(nameof(CsvFile.QualifierPlaceholder), e.PropertyName, true);
        numCalled++;
      };
      test.QualifierPlaceholder = "&&";
      Assert.AreEqual(numCalled, 1);
    }

    [TestMethod]
    public void CommentLine()
    {
      var test = new CsvFile("id2") { CommentLine = "a comment Line" };
      Assert.AreEqual("a comment Line", test.CommentLine);
    }

    [TestMethod]
    public void FieldDelimiter()
    {
      var test = new CsvFile("id2") { FieldDelimiter = "Tabulator" };
      Assert.AreEqual("Tab", test.FieldDelimiter);
      Assert.AreEqual('\t', test.FieldDelimiterChar);

      test.FieldDelimiter = "hash";
      Assert.AreEqual('#', test.FieldDelimiterChar);

      test.FieldDelimiter = "@";
      Assert.AreEqual('@', test.FieldDelimiterChar);

      test.FieldDelimiter = "underscore";
      Assert.AreEqual('_', test.FieldDelimiterChar);

      test.FieldDelimiter = "dot";
      Assert.AreEqual('.', test.FieldDelimiterChar);

      test.FieldDelimiter = "ampersand";
      Assert.AreEqual('&', test.FieldDelimiterChar);

      test.FieldDelimiter = "Pipe";
      Assert.AreEqual('|', test.FieldDelimiterChar);

      test.FieldDelimiter = "Semicolon";
      Assert.AreEqual(';', test.FieldDelimiterChar);

      test.FieldDelimiter = "Doublequotes";
      Assert.AreEqual('\"', test.FieldDelimiterChar);
    }
  }
}