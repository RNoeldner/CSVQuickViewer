using System.Text.RegularExpressions;
using FastColoredTextBoxNS;
using System;

namespace CsvTools
{
  public sealed class SyntaxHighlighterJson : SyntaxHighlighterBase
  {
    private readonly Regex m_JsonKeywordRegex = new Regex(@"(?<range>""([^\\""]|\\"")*"")\s*:",
      RegexOptions.Singleline | RegexCompiledOption);

    private readonly Regex m_JsonNumberRegex = new Regex(@"\b(\d+[\.]?\d*|true|false|null)\b",
      RegexOptions.Singleline | RegexCompiledOption);

    private readonly Regex m_JsonStringRegex =
      new Regex("\\\"((?:\\\\\\\"|(?:(?!\\\")).)*)\\\"", RegexOptions.Singleline | RegexCompiledOption);

    public SyntaxHighlighterJson(FastColoredTextBox fastColoredTextBox) : base(fastColoredTextBox)
    {
      fastColoredTextBox.LeftBracket = '[';
      fastColoredTextBox.RightBracket = ']';
      fastColoredTextBox.LeftBracket2 = '{';
      fastColoredTextBox.RightBracket2 = '}';
      fastColoredTextBox.BracketsHighlightStrategy = BracketsHighlightStrategy.Strategy2;

      fastColoredTextBox.AutoIndentCharsPatterns = @"^\s*[\w\.]+(\s\w+)?\s*(?<range>=)\s*(?<range>[^;]+);";
    }

    public override void Highlight(FastColoredTextBoxNS.Range range)
    {
      try
      {
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
      catch (Exception e)
      {
        try { Logger.Warning(e, "Highlight"); } catch { };
      }
    }
  }
}