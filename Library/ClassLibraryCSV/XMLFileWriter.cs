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

using System;
using System.Collections.Generic;
using System.Security;

namespace CsvTools
{
  /// <inheritdoc />
  /// <summary>
  ///   A class to write structured XML Files
  /// </summary>
  public class XmlFileWriter : StructuredFileWriter
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="XmlFileWriter" /> class.
    /// </summary>
    public XmlFileWriter(in string id,
      in string fullPath,
      long pgpKeyId,
      bool unencrypted,
      string? identifierInContainer,
      string? footer,
      string? header,
      int codePageId,
      bool byteOrderMark,
      IEnumerable<IColumn>? columnDefinition,
      string fileSettingDisplay,
      string row,
      TimeZoneChangeDelegate timeZoneAdjust,
      string sourceTimeZone,
      IProcessDisplay? processDisplay)
      : base(
        id, fullPath, pgpKeyId, unencrypted,
        identifierInContainer, footer, header, codePageId,
        byteOrderMark, columnDefinition, fileSettingDisplay, row, 
        timeZoneAdjust, sourceTimeZone, processDisplay)

    {
    }

    protected override string ElementName(string input) => HtmlStyle.XmlElementName(input);

    protected override string Escape(object? input, in WriterColumn columnInfo, in IFileReader reader)
    {
      if (input is null || input == DBNull.Value)
        return string.Empty;
      return SecurityElement.Escape(TextEncodeField(input, columnInfo, reader));
    }

    protected override string RecordDelimiter() => "";
  }
}