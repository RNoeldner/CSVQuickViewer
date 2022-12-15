using Microsoft.VisualStudio.TestTools.UnitTesting;
using FastColoredTextBoxNS;

namespace CsvTools.Tests
{
  [TestClass()]
  public class SyntaxHighlighterTests
  {
    [TestMethod()]
    [Timeout(10000)]
    public void SyntaxHighlighterDelimitedTextTest()
    {
      var textBox = new FastColoredTextBox();

      var highlighter = new SyntaxHighlighterDelimitedText(textBox, "\"", ",", "\\", "##");
      textBox.TextChangedDelayed += (sender, _) =>
      {
        if (!(sender is FastColoredTextBox text))
          return;
        highlighter.Highlight(text.Range);
      };
      UnitTestStaticForms.ShowControl(textBox, .2, (tb) =>
      {
        tb.Text =
          "skipped line\r## CommentLine\r\"Header\t1\",Header 2\r\nLine\t1 Column1,Line 1 Column 2\rLine\t2 Column1,Line 2 \\\"Column 2";
        highlighter.Comment(new Range(tb, 0, 0, 0, 1));
      }, 2);
    }

    [TestMethod()]
    [Timeout(10000)]
    public void SyntaxHighlighterDelimitedJsonTest()
    {
      var textBox = new FastColoredTextBox();
      using var highlighter = new SyntaxHighlighterJson(textBox);
      textBox.TextChangedDelayed += (sender, _) =>
      {
        if (sender is FastColoredTextBox text)
        {
          // ReSharper disable once AccessToDisposedClosure
          highlighter.Highlight(text.Range);
        }
      };

      UnitTestStaticForms.ShowControl(textBox, .2, (text) =>
      {
        text.Text =
          "{\n	\"glossary\": {\n		\"title\": \"example glossary\",\n		\"GlossDiv\": {\n			\"title\": \"S\",\n			\"GlossList\": {\n				\"GlossEntry\": {\n					\"ID\": \"SGML\",\n					\"SortAs\": \"SGML\",\n					\"GlossTerm\": \"Standard Generalized Markup Language\",\n					\"Acronym\": \"SGML\",\n					\"Abbrev\": \"ISO 8879:1986\",\n					\"GlossDef\": {\n						\"para\": \"A meta-markup language, used to create markup languages such as DocBook.\",\n						\"GlossSeeAlso\": [\n							\"GML\",\n							\"XML\"\n						]\n					},\n					\"GlossSee\": \"markup\"\n				}\n			}\n		}\n	}\n}";
        // ReSharper disable once AccessToDisposedClosure
        highlighter.Comment(new Range(text, 0, 0, 0, 1));
      }, 2);
    }
  }
}