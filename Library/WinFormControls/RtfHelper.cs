using System.Text;

namespace CsvTools
{
  public class RtfHelper
  {
    private readonly StringBuilder m_StringBuilder;
    private int m_CurrentColor = -1;
    private readonly bool m_DisplaySpace;

    public RtfHelper(bool displaySpace = false, int size = 18)
    {
      m_DisplaySpace = displaySpace;
      m_StringBuilder = new StringBuilder(@"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}{\colortbl ;\red0\green0\blue0;\red255\green0\blue0;\red0\green0\blue255;\red255\green168\blue0;}");
      m_StringBuilder.AppendLine($"\\viewkind4\\uc1\n\\pard\\f0\\fs{size}");
    }

    public static string RtfFromText(string text, bool displaySpace = false, char delimiter = ',', char quote = '"', char escape = '"', bool addPar = true, int size = 18)
    {
      var rtfHelper = new RtfHelper(displaySpace, size);
      if (string.IsNullOrEmpty(text)) return rtfHelper.Rtf;
      var curChar = '\0';
      var inQuote = false;

      for (var pos = 0; pos < text.Length; pos++)
      {
        // get the charters and the surroundings
        var lastChar = curChar;
        curChar = text[pos];
        var nextChar = pos < text.Length - 1 ? text[pos + 1] : '\0';

        if (curChar == '\r' || curChar == '\n')
        {
          if (addPar)
            rtfHelper.AddChar(4, curChar);
          rtfHelper.AddRtf(1, "\\par\n");
          if (curChar == '\r' && nextChar == '\n' || curChar == '\n' && nextChar == '\r')
            pos++;
          continue;
        }

        if (displaySpace && curChar == ' ')
        {
          rtfHelper.AddRtf(4, "\\bullet");
          continue;
        }

        if (curChar == delimiter && !inQuote)
        {
          rtfHelper.AddChar(2, curChar);
          continue;
        }

        if (curChar == quote)
        {
          // Start m_InQuote
          if (!inQuote)
          {
            rtfHelper.AddChar(3, curChar);
            inQuote = true;
            continue;
          }

          // Stop quote but skip internal Quotes
          if (!(lastChar == escape || nextChar == quote))
          {
            rtfHelper.AddChar(3, curChar);
            inQuote = false;
            continue;
          }

          if (nextChar == quote)
          {
            rtfHelper.AddChar(1, curChar);
            rtfHelper.AddChar(1, nextChar);
            pos++;
            continue;
          }
        }

        if (curChar >= 32 && curChar <= 127 || curChar == '\t')
          rtfHelper.AddChar(1, curChar);
        else

          // others need to be passed on with their decimal code
          rtfHelper.AddRtf(1, $"\\u{(int) curChar}?");
      }

      return rtfHelper.Rtf;
    }

    public void AddChar(int color, char character)
    {
      AddColor(color, character);
      if (character == '\t')
      {
        m_StringBuilder.Append("»");
      }
      else if ((character == '\r' || character == '\n') && m_DisplaySpace)
      {
        m_StringBuilder.Append("¶");
      }
      else
      {
        if (character == '\\' || character == '{' || character == '}')
          m_StringBuilder.Append('\\');
        m_StringBuilder.Append(character);
      }
    }

    public void AddRtf(int color, string text)
    {
      if (string.IsNullOrEmpty(text))
        return;
      AddColor(color, text[0]);
      m_StringBuilder.Append(text);
    }

    private void AddColor(int color, char nextChar)
    {
      if (color == m_CurrentColor)
        return;
      m_StringBuilder.AppendFormat(nextChar == '\\' ? "\\cf{0}" : "\\cf{0} ", color);
      m_CurrentColor = color;
    }

    private string EscapeText(string input) => string.IsNullOrEmpty(input) ? string.Empty : input.Replace(@"\", @"\'5c").Replace("{", @"\'7b").Replace("}", @"\'7d");

    public void AddParagraph() => AddParagraph(null);

    public void AddParagraph(string text) => m_StringBuilder.AppendLine($"{EscapeText(text)}\\par");

    public string Rtf => m_StringBuilder + "}";
  }
}