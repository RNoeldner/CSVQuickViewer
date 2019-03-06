using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;

namespace CsvTools.Tests
{
#pragma warning disable CA1304 // Specify CultureInfo
  [TestClass]
  public class FileFormatTest
  {
    private readonly FileFormat m_FileFormat = new FileFormat();

    [TestMethod]
    public void CloneTest()
    {
      var clone = m_FileFormat.Clone();
      Assert.AreNotSame(clone, m_FileFormat);
      m_FileFormat.AllPropertiesEqual(clone);
    }

    [TestMethod]
    public void FileFormatCheckDefaults()
    {
      var test = new FileFormat();
      //Assert.AreEqual("#", test.CommentLine, "CommentLine");
      Assert.AreEqual(string.Empty, test.DelimiterPlaceholder, "DelimiterPlaceholder");
      Assert.IsFalse(test.ColumnFormatSpecified, "ColumnFormatSpecified");
      Assert.IsFalse(test.ValueFormatSpecified, "ValueFormatSpecified");
      Assert.AreEqual(",", test.FieldDelimiter, "FieldDelimiter");
      Assert.AreEqual("\"", test.FieldQualifier, "FieldQualifier");
    }

    [TestMethod]
    public void FileFormatCopyTo()
    {
      var target = new FileFormat();
      m_FileFormat.CopyTo(target);
      Assert.AreEqual("##", target.CommentLine, "CommentLine");
      Assert.AreEqual("\\", m_FileFormat.EscapeCharacter, "EscapeCharacter");
    }

    [TestMethod]
    public void FileFormatCopyToNUll() => m_FileFormat.CopyTo(null);// NO ERror !

    [TestMethod]
    public void FileFormatCopyToEquals()
    {
      var target = new FileFormat();
      m_FileFormat.CopyTo(target);
      Assert.IsTrue(m_FileFormat.Equals(target));
    }

    [TestMethod]
    public void FileFormatEquals()
    {
      var target = new FileFormat();
      var target2 = new FileFormat();
      Assert.IsTrue(target2.Equals(target));
    }

    [TestMethod]
    public void FileFormatEscapeCharacter()
    {
      var target = new FileFormat
      {
        EscapeCharacter = "Tab"
      };
      Assert.AreEqual("tab", target.EscapeCharacter, true);

      Assert.AreEqual('\t', target.EscapeCharacterChar);

      target.EscapeCharacter = "+";
      Assert.AreEqual(target.EscapeCharacter, "+", true);
      Assert.AreEqual('+', target.EscapeCharacterChar);

      target.EscapeCharacter = "";
      Assert.AreEqual(target.EscapeCharacter, "", true);
      Assert.AreEqual('\0', target.EscapeCharacterChar);
    }

    [TestMethod]
    public void FileFormatFieldDelimiter()
    {
      var target = new FileFormat
      {
        FieldDelimiter = "Tab"
      };
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
      var target = new FileFormat
      {
        FieldQualifier = "Tab"
      };
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
      m_FileFormat.NewLine = "\n";
      m_FileFormat.NewLinePlaceholder = "{n}";
      m_FileFormat.QualifyOnlyIfNeeded = false;
      m_FileFormat.QualifyAlways = true;
      m_FileFormat.QuotePlaceholder = "{q}";
      m_FileFormat.ValueFormat = null;

      Assert.IsFalse(m_FileFormat.QualifyOnlyIfNeeded, "QualifyOnlyIfNeeded");
      Assert.IsTrue(m_FileFormat.QualifyAlways, "QualifyAlways");
      Assert.AreEqual("\n", m_FileFormat.NewLine, "NewLine");
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
      var target = new FileFormat();
      Assert.IsFalse(target.IsFixedLength);
      target.FieldDelimiter = "";
      Assert.IsTrue(target.IsFixedLength);
    }

    [TestMethod]
    public void FileFormatNotEquals()
    {
      var target = new FileFormat();
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
      var test = new FileFormat();
      test.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
      {
        Assert.AreEqual("QuotePlaceholder", e.PropertyName, true);
        numCalled++;
      };
      test.QuotePlaceholder = "&&";
      Assert.AreEqual(numCalled, 1);
    }

    [TestMethod]
    public void FileFormatToString1() => Assert.AreEqual("| #", m_FileFormat.ToString());

    [TestMethod]
    public void FileFormatToString2()
    {
      var target = new FileFormat
      {
        FieldDelimiter = ""
      };
      Assert.AreEqual("FixedLength", target.ToString(), true);
    }

    [TestMethod]
    public void GetDescription()
    {
      Assert.AreEqual(string.Empty, FileFormat.GetDescription(null));
      Assert.AreEqual("Horizontal Tab", FileFormat.GetDescription("Tab"));
      Assert.AreEqual("Comma: ,", FileFormat.GetDescription(","));
      Assert.AreEqual("Pipe: |", FileFormat.GetDescription("|"));
      Assert.AreEqual("Semicolon: ;", FileFormat.GetDescription(";"));
      Assert.AreEqual("Colon: :", FileFormat.GetDescription(":"));
      Assert.AreEqual("Quotation marks: \"", FileFormat.GetDescription("\""));
      Assert.AreEqual("Apostrophe: '", FileFormat.GetDescription("'"));
      Assert.AreEqual("Space", FileFormat.GetDescription(" "));
      Assert.AreEqual("Backslash: \\", FileFormat.GetDescription("\\"));
      Assert.AreEqual("Slash: /", FileFormat.GetDescription("/"));
      Assert.AreEqual("Nothing", FileFormat.GetDescription("Nothing"));
      Assert.AreEqual("Unit Separator: Char 31", FileFormat.GetDescription("US"));
      Assert.AreEqual("Unit Separator: Char 31", FileFormat.GetDescription("Unit Separator"));
      Assert.AreEqual("Unit Separator: Char 31", FileFormat.GetDescription("char(31)"));

      Assert.AreEqual("Group Separator: Char 29", FileFormat.GetDescription("GS"));
      Assert.AreEqual("Record Separator: Char 30", FileFormat.GetDescription("RS"));
      Assert.AreEqual("File Separator: Char 28", FileFormat.GetDescription("FS"));
    }

    [TestMethod]
    public void CommentLine()
    {
      var test = new FileFormat
      {
        CommentLine = "a comment Line"
      };
      Assert.AreEqual("a comment Line", test.CommentLine);
    }

    [TestMethod]
    public void FieldDelimiter()
    {
      var test = new FileFormat
      {
        FieldDelimiter = "Tabulator"
      };
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
#pragma warning restore CA1304 // Specify CultureInfo
}