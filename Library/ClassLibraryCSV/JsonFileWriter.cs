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
  /// <summary>
  ///   A class to write structured Json Files
  /// </summary>
  public class JsonFileWriter : StructuredFileWriter
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="JsonFileWriter" /> class.
    /// </summary>
    public JsonFileWriter(
      string id,
      string fullPath,
      IValueFormat? valueFormatGeneral = null,
      IFileFormat? fileFormat = null,
      string? recipient = null,
      bool unencrypted = false,
      string? identifierInContainer = null,
      string? footer = null,
      string? header = null,
      IEnumerable<IColumn>? columnDefinition = null,
      string fileSettingDisplay = "",
      string row = "",
      IProcessDisplay? processDisplay = null)
      : base(
        id,
        fullPath,
        valueFormatGeneral,
        fileFormat,
        recipient,
        unencrypted,
        identifierInContainer,
        footer,
        header,
        columnDefinition,
        fileSettingDisplay,
        row,
        processDisplay)
    {
    }

    protected override string ElementName(string input) => HTMLStyle.JsonElementName(input);

    protected override string Escape(object input, WriterColumn columnInfo, IFileReader reader) =>
      JsonConvert.ToString(input);
  }
}