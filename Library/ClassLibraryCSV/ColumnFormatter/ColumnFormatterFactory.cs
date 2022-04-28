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

namespace CsvTools
{
  public static class ColumnFormatterFactory
  {
    public static IColumnFormatter? GetColumnFormatter(int columnOrdinal, IValueFormat valueFormat)
    {
      return valueFormat.DataType switch
      {
        DataType.TextPart => new TextPartFormatter(valueFormat.Part, valueFormat.PartSplitter, valueFormat.PartToEnd),
        DataType.TextToHtml => new TextToHtmlFormatter(),
        DataType.TextToHtmlFull => new TextToHtmlFullFormatter(),
        DataType.TextUnescape => new TextUnescapeFormatter(),
        DataType.TextReplace => new TextReplace(valueFormat.RegexSearchPattern, valueFormat.RegexReplacement),
        DataType.Binary => new BinaryFormatter(columnOrdinal, valueFormat.ReadFolder, valueFormat.WriteFolder, valueFormat.FileOutPutPlaceholder, valueFormat.Overwrite),
#if !QUICK
        DataType.Markdown2Html => new MarkupToHtmlFormatter(),

#endif
        _ => null,
      };
    }
  }
}
