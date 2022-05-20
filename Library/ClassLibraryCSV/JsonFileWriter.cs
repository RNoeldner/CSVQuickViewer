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

using Newtonsoft.Json;
using System.Collections.Generic;

namespace CsvTools
{
  /// <inheritdoc />
  /// <summary>
  ///   A class to write structured Json Files
  /// </summary>
  public class JsonFileWriter : StructuredFileWriter
  {
    /// <inheritdoc />
    /// <summary>
    ///   Initializes a new instance of the <see cref="T:CsvTools.JsonFileWriter" /> class.
    /// </summary>
    public JsonFileWriter(in string id,
      in string fullPath,
      long pgpKeyId,
      bool unencrypted,
      in string? identifierInContainer,
      in string? footer,
      in string? header,
      int codePageId,
      bool byteOrderMark,
      in IEnumerable<IColumn>? columnDefinition,
      in string fileSettingDisplay,
      in string row,
      in TimeZoneChangeDelegate? timeZoneAdjust,
      in string sourceTimeZone,
      in IProcessDisplay? processDisplay = null)
      : base(
        id,
        fullPath,
        pgpKeyId,
        unencrypted,
        identifierInContainer,
        footer,
        header,
        codePageId,
        byteOrderMark,
        columnDefinition,
        fileSettingDisplay,
        row,
        timeZoneAdjust ?? StandardTimeZoneAdjust.ChangeTimeZone, 
        sourceTimeZone,
        processDisplay)
    {
    }

    protected override string RecordDelimiter() => ",";

    protected override string ElementName(string input) => HtmlStyle.JsonElementName(input);

    protected override string Escape(object input, in WriterColumn columnInfo, in IFileReader reader) =>
      JsonConvert.ToString(input);
  }
}