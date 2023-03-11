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
using System;
using System.Collections.Generic;
using System.Security;

namespace CsvTools
{
  /// <inheritdoc />
  /// <summary>
  ///   A class to write structured XML Files
  /// </summary>
  public sealed class XmlFileWriter : StructuredFileWriter
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="XmlFileWriter" /> class.
    /// </summary>
    public XmlFileWriter(in string id,
      in string fullPath,
      bool unencrypted,
      in string? identifierInContainer,
      in string? footer,
      in string? header,
      int codePageId,
      bool byteOrderMark,
      IEnumerable<Column>? columnDefinition,
      in string fileSettingDisplay,
      in string row,
      TimeZoneChangeDelegate timeZoneAdjust,
      in string sourceTimeZone,
      in string publicKey)
      : base(
        id, fullPath, unencrypted,
        identifierInContainer, footer, header, codePageId,
        byteOrderMark, columnDefinition, fileSettingDisplay, row,
        timeZoneAdjust, sourceTimeZone, publicKey)

    {
    }

    protected override string ElementName(string input) => HtmlStyle.XmlElementName(input);

    protected override string Escape(object? input, in WriterColumn columnInfo, in IFileReader reader)
    {
      if (input is null || input == DBNull.Value)
        return string.Empty;
      return SecurityElement.Escape(TextEncodeField(input, columnInfo, reader)) ?? string.Empty;
    }

    protected override string RecordDelimiter() => "";
  }
}