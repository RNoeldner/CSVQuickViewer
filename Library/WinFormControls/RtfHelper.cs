using System.Collections.Generic;
using System.Text;

namespace CsvTools
{
  public class RtfHelper
  {
    private StringBuilder stringBuilder = new StringBuilder();

    public RtfHelper()
    {
      stringBuilder.AppendLine(@"{\rtf1\ansi\ansicpg1252\deff0\deflang1031{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}\viewkind4\uc1\pard\f0\fs18 ");
    }

    public void AddTable(IEnumerable<string> data, int columns = 4)
    {
      int item = 0;
      var tabledata = new List<string>(data);
      int lastRow = (tabledata.Count) / columns + (tabledata.Count % columns == 0 ? 0 : 1);
      for (int row = 0; row < lastRow; row++)
      {
        stringBuilder.AppendLine(@"\trowd");
        for (int cell = 1; cell <= columns; cell++)
        {
          // tableRtf.Append(@"\cell"); All olumnns are 1500 wide
          stringBuilder.Append($"\\cellx{cell * 1500} ");
          if (item < tabledata.Count)
          {
            stringBuilder.Append(EscapeText(tabledata[item++]));
            stringBuilder.Append(@" \intbl\cell");
          }
        }
        stringBuilder.AppendLine(@"\row");
      }
    }

    private string EscapeText(string input)
    {
      if (string.IsNullOrEmpty(input))
        return string.Empty;
      return input.Replace(@"\", @"\'5c").Replace("{", @"\'7b").Replace("}", @"\'7d");
    }

    public void AddParagraph() => AddParagraph(null);

    public void AddParagraph(string text)
    {
      stringBuilder.AppendLine($"{EscapeText(text)}\\par");
    }

    public string Rtf
    {
      get
      {
        return stringBuilder.ToString() + @"\pard}";
      }
    }
  }
}