using System.Collections.Generic;
using System.Text;

namespace CsvTools
{
  public class RtfHelper
  {
    private readonly StringBuilder m_StringBuilder = new StringBuilder();

    public RtfHelper()
    {
      m_StringBuilder.AppendLine(@"{\rtf1\ansi\ansicpg1252\deff0\deflang1031{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}\viewkind4\uc1\pard\f0\fs18 ");
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