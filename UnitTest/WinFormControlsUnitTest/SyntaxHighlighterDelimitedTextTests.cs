using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastColoredTextBoxNS;

namespace CsvTools.Tests
{
  [TestClass()]
  public class SyntaxHighlighterTests
  {
    [TestMethod()]
    public void SyntaxHighlighterDelimitedTextTest()
    {
      using (var textBox = new FastColoredTextBox())
      {
        var highlighter = new SyntaxHighlighterDelimitedText(textBox, '"', ',', '\\', "##");
        textBox.TextChangedDelayed += (sender, e) =>
        {
          if (!(sender is FastColoredTextBox text))
            return;
          highlighter.Highlight(text.Range);
        };
        UnitTestWinFormHelper.ShowControl(textBox, .2, (tb, frm) =>
        {
          if (!(tb is FastColoredTextBox text))
            return;
          text.Text =
            "skipped line\r## CommentLine\r\"Header\t1\",Header 2\r\nLine\t1 Column1,Line 1 Column 2\rLine\t2 Column1,Line 2 \\\"Column 2";
          highlighter.Comment(new Range(text, 0, 0, 0, 1));
        }, 2);
      }
    }

    [TestMethod()]
    public void SyntaxHighlighterDelimitedJsonTest()
    {
      using (var textBox = new FastColoredTextBox())
      {
        var highlighter = new SyntaxHighlighterJson(textBox);
        textBox.TextChangedDelayed += (sender, e) =>
        {
          if (!(sender is FastColoredTextBox text))
            return;
          highlighter.Highlight(text.Range);
        };
        ;
        UnitTestWinFormHelper.ShowControl(textBox, .2, (text, frm) =>
        {
          text.Text =
            "{\n	\"glossary\": {\n		\"title\": \"example glossary\",\n		\"GlossDiv\": {\n			\"title\": \"S\",\n			\"GlossList\": {\n				\"GlossEntry\": {\n					\"ID\": \"SGML\",\n					\"SortAs\": \"SGML\",\n					\"GlossTerm\": \"Standard Generalized Markup Language\",\n					\"Acronym\": \"SGML\",\n					\"Abbrev\": \"ISO 8879:1986\",\n					\"GlossDef\": {\n						\"para\": \"A meta-markup language, used to create markup languages such as DocBook.\",\n						\"GlossSeeAlso\": [\n							\"GML\",\n							\"XML\"\n						]\n					},\n					\"GlossSee\": \"markup\"\n				}\n			}\n		}\n	}\n}";
          highlighter.Comment(new Range(text, 0, 0, 0, 1));
        }, 2);
      }
    }

    [TestMethod()]
    public void SyntaxHighlighterDelimitedXmlTest()
    {
      using (var textBox = new FastColoredTextBox())
      {
        var highlighter = new SyntaxHighlighterXML(textBox);
        textBox.TextChangedDelayed += (sender, e) =>
        {
          if (!(sender is FastColoredTextBox text))
            return;
          highlighter.Highlight(text.Range);
        };
        ;
        UnitTestWinFormHelper.ShowControl(textBox, .2, (text, frm) =>
        {
          text.Text =
            "<menu id=\"file\" value=\"File\">\r\n  <popup>\r\n    <menuitem value=\"New\" onclick=\"CreateNewDoc()\" />\r\n    <menuitem value=\"Open\" onclick=\"OpenDoc()\" />\r\n    <menuitem value=\"Close\" onclick=\"CloseDoc()\" />\r\n  </popup>\r\n</menu>";
          highlighter.Comment(new Range(text, 0, 0, 0, 1));
        }, 2);
      }
    }
  }
}