using System.Text.RegularExpressions;
using FastColoredTextBoxNS;

namespace CsvTools
{
  public class SyntaxHighlighterJson : SyntaxHighlighterBase
  {
    private readonly Regex m_JsonKeywordRegex = new Regex(@"(?<range>""([^\\""]|\\"")*"")\s*:",
      RegexOptions.Singleline | RegexCompiledOption);

    private readonly Regex m_JsonNumberRegex = new Regex(@"\b(\d+[\.]?\d*|true|false|null)\b",
      RegexOptions.Singleline | RegexCompiledOption);

    private readonly Regex m_JsonStringRegex =
      new Regex(@"""([^\\""]|\\"")*""", RegexOptions.Singleline | RegexCompiledOption);

    public SyntaxHighlighterJson(FastColoredTextBox fastColoredTextBox) : base(fastColoredTextBox)
    {
    }

    public override void Highlight(Range range)
    {
      range.tb.LeftBracket = '[';
      range.tb.RightBracket = ']';
      range.tb.LeftBracket2 = '{';
      range.tb.RightBracket2 = '}';
      range.tb.BracketsHighlightStrategy = BracketsHighlightStrategy.Strategy2;

      range.tb.AutoIndentCharsPatterns = @"^\s*[\w\.]+(\s\w+)?\s*(?<range>=)\s*(?<range>[^;]+);";

      //clear style of changed range
      range.ClearStyle(StyleIndex.All);

      //keyword highlighting
      range.SetStyle(BlueStyle, m_JsonKeywordRegex);
      //string highlighting
      range.SetStyle(BrownStyle, m_JsonStringRegex);
      //number highlighting
      range.SetStyle(MagentaStyle, m_JsonNumberRegex);
      //clear folding markers
      range.ClearFoldingMarkers();
      //set folding markers
      range.SetFoldingMarkers("{", "}"); //allow to collapse brackets block
      range.SetFoldingMarkers(@"\[", @"\]"); //allow to collapse comment block
    }
  }
}