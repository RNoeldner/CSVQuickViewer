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
using System.ComponentModel;

namespace CsvTools.Tests
{
  [TestClass]
  public class FileFormatTest
  {
    private readonly CsvFile m_FileFormat = new CsvFile();

    [TestMethod]
    public void CloneTest()
    {
      var clone = m_FileFormat.Clone();
      Assert.AreNotSame(clone, m_FileFormat);
      m_FileFormat.CheckAllPropertiesEqual(clone);
    }

    [TestMethod]
    public void FileFormatCheckDefaults()
    {
      var test = new CsvFile();
      //Assert.AreEqual("#", test.CommentLine, "CommentLine");
      Assert.AreEqual(string.Empty, test.DelimiterPlaceholder, "DelimiterPlaceholder");
      Assert.AreEqual(",", test.FieldDelimiter, "FieldDelimiter");
      Assert.AreEqual("\"", test.FieldQualifier, "FieldQualifier");
    }

    [TestMethod]
    public void FileFormatCopyTo()
    {
      var target = new CsvFile();
      m_FileFormat.CopyTo(target);
      Assert.AreEqual("##", target.CommentLine, "CommentLine");
      Assert.AreEqual("\\", m_FileFormat.EscapeCharacter, "EscapeCharacter");
    }


    [TestMethod]
    public void FileFormatEscapeCharacter()
    {
      var target = new CsvFile { EscapeCharacter = "Tab" };
      Assert.AreEqual("tab", target.EscapeCharacter, true);

      target.EscapeCharacter = "+";
      Assert.AreEqual(target.EscapeCharacter, "+", true);

      target.EscapeCharacter = "";
      Assert.AreEqual(target.EscapeCharacter, "", true);
    }

    [TestMethod]
    public void FileFormatFieldDelimiter()
    {
      var target = new CsvFile { FieldDelimiter = "Tab" };
      Assert.AreEqual(target.FieldDelimiter, "tab", true);
      Assert.AreEqual(target.FieldDelimiterChar, '\t');

      target.FieldDelimiter = "comma";
      Assert.AreEqual(target.FieldDelimiter, "comma", true);
      Assert.AreEqual(target.FieldDelimiterChar, ',');

      target.FieldDelimiter = "Pipe";
      Assert.AreEqual(target.FieldDelimiter, "Pipe", true);
      Assert.AreEqual(target.FieldDelimiterChar, '|');
    }

    [TestMethod]
    public void FileFormatFieldQualifier()
    {
      var target = new CsvFile { FieldQualifier = "Tab" };
      Assert.AreEqual(target.FieldQualifier, "tab", true);
      Assert.AreEqual(target.FieldQualifierChar, '\t');

      target.FieldQualifier = "+";
      Assert.AreEqual(target.FieldQualifier, "+", true);
      Assert.AreEqual(target.FieldQualifierChar, '+');

      target.FieldQualifier = "";
      Assert.AreEqual(target.FieldQualifier, "", true);
      Assert.AreEqual(target.FieldQualifierChar, '\0');
    }

    [TestInitialize]
    public void FileFormatInit()
    {
      //m_FileFormat.ColumnFormat = null;
      m_FileFormat.CommentLine = "##";
      m_FileFormat.DelimiterPlaceholder = "{d}";
      m_FileFormat.EscapeCharacter = "\\";
      m_FileFormat.FieldDelimiter = "|";
      m_FileFormat.FieldQualifier = "#";
      m_FileFormat.EscapeCharacter = "\\";
      m_FileFormat.NewLine = RecordDelimiterType.LF;
      m_FileFormat.NewLinePlaceholder = "{n}";
      m_FileFormat.QualifyOnlyIfNeeded = false;
      m_FileFormat.QualifyAlways = true;
      m_FileFormat.QuotePlaceholder = "{q}";

      Assert.IsFalse(m_FileFormat.QualifyOnlyIfNeeded, "QualifyOnlyIfNeeded");
      Assert.IsTrue(m_FileFormat.QualifyAlways, "QualifyAlways");
      Assert.AreEqual(RecordDelimiterType.LF, m_FileFormat.NewLine, "NewLine");
      Assert.AreEqual("##", m_FileFormat.CommentLine, "CommentLine");
      Assert.AreEqual("{d}", m_FileFormat.DelimiterPlaceholder, "DelimiterPlaceholder");
      Assert.AreEqual("{n}", m_FileFormat.NewLinePlaceholder, "NewLinePlaceholder");
      Assert.AreEqual("{q}", m_FileFormat.QuotePlaceholder, "QuotePlaceholder");
      Assert.AreEqual("|", m_FileFormat.FieldDelimiter, "FieldDelimiter");
      Assert.AreEqual("#", m_FileFormat.FieldQualifier, "FieldQualifier");
      Assert.AreEqual("\\", m_FileFormat.EscapeCharacter, "EscapeCharacter");
    }

    [TestMethod]
    public void FileFormatIsFixedLength()
    {
      var target = new CsvFile();
      Assert.IsFalse(target.IsFixedLength);
      target.FieldDelimiter = "";
      Assert.IsTrue(target.IsFixedLength);
    }

    [TestMethod]
    public void FileFormatNotEquals()
    {
      var target = new CsvFile();
      Assert.IsFalse(m_FileFormat.Equals(target));
    }

    [TestMethod]
    public void FileFormatNotEqualsNull() => Assert.IsFalse(m_FileFormat.Equals(null));

    [TestMethod]
    public void FileFormatNotEqualsSelf() => Assert.IsTrue(m_FileFormat.Equals(m_FileFormat));

    [TestMethod]
    public void FileFormatPropertyChanged()
    {
      var numCalled = 0;
      var test = new CsvFile();
      test.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
      {
        Assert.AreEqual("QuotePlaceholder", e.PropertyName, true);
        numCalled++;
      };
      test.QuotePlaceholder = "&&";
      Assert.AreEqual(numCalled, 1);
    }


    [TestMethod]
    public void CommentLine()
    {
      var test = new CsvFile { CommentLine = "a comment Line" };
      Assert.AreEqual("a comment Line", test.CommentLine);
    }

    [TestMethod]
    public void FieldDelimiter()
    {
      var test = new CsvFile { FieldDelimiter = "Tabulator" };
      Assert.AreEqual("Tabulator", test.FieldDelimiter);
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