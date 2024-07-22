/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com/
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

namespace CsvTools
{
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
    /// <param name="cancellationToken">Cancellation Token to stop a possibly long running process</param>
    /// <returns>HTML Table</returns>
    public static async Task<string> GetFileInformationHtml(this IFileSettingPhysicalFile fileSetting,
      IEnumerable<string> additionalInfo, int? numRecords,
      CancellationToken cancellationToken)
    {
      var buffer = new StringBuilder();
      var sbHtml = new StringBuilder(HtmlStyle.TableOpen);
      var fi = new System.IO.FileInfo(fileSetting.FileName);

      void AddInfo(in string label, in string text)
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

      AddInfo("File Size", fi.Length.ToString() + " Bytes");
      AddInfo("File Date", fi.LastWriteTimeUtc.ToString("R"));

      AddInfo("File Encoding", EncodingHelper.GetEncodingName(fileSetting.CodePageId,
        fileSetting.ByteOrderMark));
      if (fileSetting is ICsvFile csvFile)
      {
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
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
            await
#endif
            using var improvedStream = FunctionalDI.GetStream(new SourceAccess(csvFile));
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
            await
#endif
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
            AddInfo("Quoting / Qualifier", csvFile.FieldQualifierChar.Description());
            if (csvFile.DuplicateQualifierToEscape)
              AddInfo("Quoting / Qualifier", "Duplicate qualifier inside qualification");
          }
        }
      }

      if (numRecords.HasValue)
        AddInfo("Number of records", numRecords.Value.ToString());

#if NET5_0_OR_GREATER
      await
#endif
      using var fileReader =
        FunctionalDI.FileReaderWriterFactory.GetFileReader(fileSetting, cancellationToken);
      await fileReader.OpenAsync(cancellationToken).ConfigureAwait(false);
      AddInfo("Number of Columns", fileReader.FieldCount.ToString());
      if (fileSetting is ICsvFile csvFile2)
      {
        AddInfo("Column Names", string.Join(csvFile2.FieldDelimiterChar.ToString(),
          fileReader.GetColumnsOfReader().Select(x => x.Name)));
        if (csvFile2.CommentLine.Length > 0)
          AddInfo("Commented Row", csvFile2.CommentLine);
      }
      else
      {
        AddInfo("Column Names", string.Join(", ",
          fileReader.GetColumnsOfReader().Select(x => x.Name)));
      }

      if (fileSetting.SkipRows > 0)
        AddInfo("Skip Rows", fileSetting.SkipRows.ToString());

      foreach (var line in additionalInfo)
      {
        AddInfo(line, string.Empty);
      }

      sbHtml.AppendLine(HtmlStyle.TableClose);
      return sbHtml.ToString();
    }
  }
}