/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.IO;

namespace CsvTools;

/// <summary>
/// Extension Methods for IFileSetting (or similar)
/// </summary>
public static class FileSettingsExtensionMethods
{
  /// <summary>
  /// Get Key Information on Setting for a physical file
  /// </summary>
  /// <param name="fileSetting">The FileSetting</param>
  /// <param name="additionalInfo">Information that should be outputted as well</param>
  /// <param name="numRecords">Number of records that have been read, null if it's not know or should not be displayed</param>
  /// <param name="isDelimited">True if its a delimited text file</param>
  /// <param name="cancellationToken">Cancellation Token to stop a possibly long running process</param>
  /// <returns>HTML Table</returns>
  public static async Task<string> GetFileInformationHtml(this IFileSettingPhysicalFile fileSetting,
    IEnumerable<string> additionalInfo, int? numRecords, bool isDelimited,
    CancellationToken cancellationToken)
  {
    var buffer = new StringBuilder();
    var sbHtml = new StringBuilder(HtmlStyle.TableOpen);
    var fi = new FileInfo(fileSetting.FileName);

    void AddInfo(string label, string text)
    {
      buffer.Append(label.Replace("\t", "  "));
      buffer.Append('\t');
      buffer.AppendLine(text.Replace("\t", "  "));

      sbHtml.Append(HtmlStyle.TrOpen);
      if (!string.IsNullOrEmpty(text))
      {
        sbHtml.Append(HtmlStyle.AddTd(HtmlStyle.Td, label));
        sbHtml.Append(HtmlStyle.AddTd(HtmlStyle.Td, text));
      }
      else
      {
        sbHtml.Append(HtmlStyle.AddTd("<td colspan=2>{0}</td>", label));
      }

      sbHtml.Append(HtmlStyle.TrClose);
    }

    AddInfo("File Name", fi.Name);
    if (fileSetting.IdentifierInContainer.Length > 0)
      AddInfo("Name in Container", fileSetting.IdentifierInContainer);

    AddInfo("File Size", fi.Length + " Bytes");
    AddInfo("File Date", fi.LastWriteTimeUtc.ToString("R"));

    AddInfo("File Encoding", EncodingHelper.GetEncodingName(fileSetting.CodePageId,
      fileSetting.ByteOrderMark));

    if (fileSetting.SkipRows > 0)
      AddInfo("Skip Rows", fileSetting.SkipRows.ToString());

    var rawHeader = string.Empty;
    var join = ", ";

    if (fileSetting is ICsvFile csvFile && isDelimited)
    {
      if (csvFile.CommentLine.Length > 0)
        AddInfo("Commented Row", csvFile.CommentLine);

      AddInfo("Field Delimiter", csvFile.FieldDelimiterChar.Description());
      if (csvFile.EscapePrefixChar != '\0')
      {
        AddInfo("Escape Prefix", csvFile.EscapePrefixChar.Description());
      }

      AddInfo("Has Header Row", fileSetting.HasFieldHeader.ToString());

      if (csvFile.FieldQualifierChar == '\0')
        AddInfo("Quoting / Qualifier", "No field qualifier");
      else
      {
        // if we do not have information on qualifierUsed, retrieve it
        var qualifierUsed = true;
        try
        {
          using var improvedStream = FunctionalDI.GetStream(new SourceAccess(csvFile));
          using var textReader = await improvedStream
            .GetTextReaderAsync(csvFile.CodePageId, csvFile.SkipRows, cancellationToken)
            .ConfigureAwait(false);
          qualifierUsed = await improvedStream.HasUsedQualifierAsync(csvFile.CodePageId, csvFile.SkipRows,
              csvFile.FieldDelimiterChar,
              csvFile.FieldQualifierChar, cancellationToken)
            .ConfigureAwait(false);
        }
        catch
        {
          // ignored
        }

        if (qualifierUsed)
        {
          var info = csvFile.FieldQualifierChar.Description();
          if (csvFile.DuplicateQualifierToEscape)
            info += " (Duplicate qualifier inside qualification)";
          AddInfo("Quoting / Qualifier", info);
        }
      }

      try
      {
        using (var improvedStream = FunctionalDI.GetStream(new SourceAccess(csvFile)))
        {
          rawHeader = improvedStream.GetRawHeaderLine(csvFile.CodePageId, csvFile.SkipRows,
            csvFile.FieldDelimiterChar,
            csvFile.FieldQualifierChar, csvFile.EscapePrefixChar, csvFile.CommentLine);

          improvedStream.Position = 0;

          using var streamReader =
            new StreamReader(improvedStream, Encoding.GetEncoding(csvFile.CodePageId),
              false, 4096, false);
          var numLines = 0;
          while (!streamReader.EndOfStream)
          {
            numLines++;
            await streamReader.ReadLineAsync().ConfigureAwait(false);
          }

          AddInfo("Number of Lines", numLines.ToString());
        }
      }
      catch
      {
        // Ignore
      }

      join = csvFile.FieldDelimiterChar.ToString();
    }

    if (numRecords.HasValue)
      AddInfo("Number of Records", numRecords.Value.ToString());

    using var fileReader =
      FunctionalDI.FileReaderWriterFactory.GetFileReader(fileSetting, cancellationToken);
    await fileReader.OpenAsync(cancellationToken).ConfigureAwait(false);

    var columnNames = string.Join(join,
      fileReader.GetColumnsOfReader().Select(x => x.Name));
    fileReader.Close();

    AddInfo("Number of Columns", fileReader.FieldCount.ToString());
    if (!string.IsNullOrEmpty(rawHeader) && !rawHeader.Equals(columnNames))
      AddInfo("Header", rawHeader);
    AddInfo("Column Names", columnNames);
    var first = true;
    foreach (var line in additionalInfo)
    {
      if (first)
      {
        AddInfo(string.Empty, string.Empty);
        first = false;
      }

      AddInfo(line, string.Empty);
    }


    sbHtml.AppendLine(HtmlStyle.TableClose);
    return sbHtml.ToString();
  }
}