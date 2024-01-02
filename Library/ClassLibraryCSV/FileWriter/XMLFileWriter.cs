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
using System.Data;
using System.Security;
using System.Text;

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
      in string publicKey,
      bool unencrypted
      )
      : base(
        id, fullPath,
        identifierInContainer, footer, header, codePageId,
        byteOrderMark, columnDefinition, fileSettingDisplay, row,
        timeZoneAdjust, sourceTimeZone, publicKey, unencrypted
        )
    {
    }

    /// <inheritdoc />
    protected override string ElementName(string input) => HtmlStyle.XmlElementName(input);

    /// <inheritdoc />
    protected override string Escape(object? input, in WriterColumn columnInfo, in IDataRecord? reader)
    {
      if (input is null || input == DBNull.Value)
        return string.Empty;
      return SecurityElement.Escape(TextEncodeField(input, columnInfo, reader)) ?? string.Empty;
    }

    /// <inheritdoc />
    protected override string RecordDelimiter() => "";

    /// <summary>
    /// Gets the header and row.
    /// </summary>
    /// <param name="cols">The cols.</param>
    /// <returns></returns>
    public static (string Header, string Row) GetXMLHeaderAndRow(IEnumerable<Column> cols)
    {
      var sb = new StringBuilder();
      var sbRow = new StringBuilder();
      sb.AppendLine("<?xml version=\"1.0\"?>\n");
      sb.AppendLine(
        "  <xs:element name=\"rowset\">\n    <xs:complexType>\n     <xs:sequence>\r\n      <xs:element ref=\"row\"/>\n     </xs:sequence>\r\n   </xs:complexType>\r\n  </xs:element>");
      sb.AppendLine(
        "  <xs:schema elementFormDefault=\"qualified\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n  <xs:element name=\"row\">\n    <xs:complexType>\n      <xs:sequence>");
      sbRow.AppendLine("  <row>");

      foreach (var col in cols)
      {
        sbRow.AppendFormat("    <{0}>{1}</{0}>\n", HtmlStyle.XmlElementName(col.Name),
         string.Format(StructuredFileWriter.cFieldPlaceholderByName, col.Name));
        string type;
        switch (col.ValueFormat.DataType)
        {
          case DataTypeEnum.Integer:
            type = "xs:integer";
            break;

          case DataTypeEnum.Numeric:
          case DataTypeEnum.Double:
            type = "xs:decimal";
            break;

          case DataTypeEnum.DateTime:
            type = "xs:dateTime";
            break;

          case DataTypeEnum.Boolean:
            type = "xs:boolean";
            break;

          default:
            type = "xs:string";
            break;
        }

        sb.AppendFormat("          <xs:element name=\"{0}\" type=\"{1}\" />\n",
          HtmlStyle.XmlElementName(col.Name),
          type);
      }

      sb.AppendLine("        </xs:sequence>\n    </xs:complexType>\n  </xs:element>\n</xs:schema>");
      sb.AppendLine("<rowset>");
      sbRow.AppendLine("  </row>");
      return (sb.ToString(), sbRow.ToString());
    }

  }
}