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

#nullable enable
namespace CsvTools
{
  public static class ColumnFormatterFactory
  {

    public static IColumnFormatter GetColumnFormatter(in ValueFormat valueFormat)
    {
      return valueFormat.DataType switch
      {
        DataTypeEnum.TextPart => new TextPartFormatter(valueFormat.Part, valueFormat.PartSplitter,
          valueFormat.PartToEnd),
        DataTypeEnum.TextToHtml => TextToHtmlFormatter.Instance,
        DataTypeEnum.TextToHtmlFull => TextToHtmlFullFormatter.Instance,
        DataTypeEnum.TextUnescape => TextUnescapeFormatter.Instance,
        DataTypeEnum.TextReplace => new TextReplaceFormatter(valueFormat.RegexSearchPattern, valueFormat.RegexReplacement),
        DataTypeEnum.Binary => new BinaryFormatter(valueFormat.ReadFolder, valueFormat.WriteFolder, valueFormat.FileOutPutPlaceholder, valueFormat.Overwrite),
#if !QUICK
        DataTypeEnum.Markdown2Html => Markdown2HtmlFormatter.Instance,
#endif
        _ => EmptyFormatter.Instance
      };
    }
  }
}