using System.Collections.Generic;
using System.Text;

namespace CsvTools
{
  public class RtfHelper
  {
    private readonly StringBuilder m_StringBuilder = new StringBuilder();
    private int m_CurrentColor = -1;
    private readonly bool m_DisplaySpace = true;

    public RtfHelper(bool displaySpace = false, int size = 18)
    {
      m_DisplaySpace = displaySpace;
      m_StringBuilder = new StringBuilder(@"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}{\colortbl ;\red0\green0\blue0;\red255\green0\blue0;\red0\green0\blue255;\red255\green168\blue0;}");
      m_StringBuilder.AppendLine($"\\viewkind4\\uc1\\pard\\f0\\fs{size}");
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

    public void AddTable(IEnumerable<string> data, int columns = 4)
    {
      var item = 0;
      var tableData = new List<string>(data);
      var lastRow = (tableData.Count) / columns + (tableData.Count % columns == 0 ? 0 : 1);
      for (var row = 0; row < lastRow; row++)
      {
        m_StringBuilder.AppendLine(@"\trowd");
        for (var cell = 1; cell <= columns; cell++)
        {
          // tableRtf.Append(@"\cell"); All columns are 1600 wide
          m_StringBuilder.Append($"\\cellx{cell * 1600} ");
          if (item >= tableData.Count) continue;
          m_StringBuilder.Append(EscapeText(tableData[item++]));
          m_StringBuilder.Append(@" \intbl\cell");
        }
        m_StringBuilder.AppendLine(@"\row");
      }
    }

    private string EscapeText(string input)
    {
      return string.IsNullOrEmpty(input) ? string.Empty : input.Replace(@"\", @"\'5c").Replace("{", @"\'7b").Replace("}", @"\'7d");
    }

    public void AddParagraph() => AddParagraph(null);

    public void AddParagraph(string text)
    {
      m_StringBuilder.AppendLine($"{EscapeText(text)}\\par");
    }

    public string Rtf => m_StringBuilder.ToString() + @"\pard}";
  }
}