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
using System.Data;
using System.IO;

namespace CsvTools
{
  public class BinaryFormatter : BaseColumnFormatter
  {
    private readonly int m_ColumnOrdinal;
    private readonly string m_FileOutPutPlaceholder;
    private readonly bool m_Overwrite;
    private readonly string m_RootFolderRead;
    private readonly string m_RootFolderWrite;

    public BinaryFormatter(int columnOrdinal = -1, string? rootFolderRead = null, string? rootFolderWrite = null,
      string? fileOutPutPlaceholder = null, bool overwrite = true)
    {
      m_ColumnOrdinal = columnOrdinal;
      m_RootFolderRead = rootFolderRead ?? string.Empty;
      m_RootFolderWrite = rootFolderWrite ?? string.Empty;
      m_FileOutPutPlaceholder = fileOutPutPlaceholder ?? string.Empty;
      m_Overwrite = overwrite;
    }

    public static string CombineNameAndContent(in string fileName, in byte[] content) =>
      $"{fileName}\0{Convert.ToBase64String(content)}";

    private static byte[] GetContentFromNameAndContent(in string contentsWithFileName) =>
      Convert.FromBase64String(contentsWithFileName.Substring(contentsWithFileName.IndexOf('\0') + 1));

    public static string GetNameFromNameAndContent(in string contentsWithFileName) =>
      contentsWithFileName.Substring(0, contentsWithFileName.IndexOf('\0'));


    /// <inheritdoc/>
    public override string FormatInputText(in string inputString, in Action<string>? handleWarning)
    {
      var fileName = inputString.FullPath(m_RootFolderRead);
      var fi = new FileSystemUtils.FileInfo(fileName);
      if (!fi.Exists)
      {
        if (RaiseWarning)
          handleWarning?.Invoke($"File {inputString} not found");
        return string.Empty;
      }

      if (fi.Length > 256000)
      {
        if (RaiseWarning)
          handleWarning?.Invoke($"File {inputString} too large {fi.Length:N0} > {256000:N0}");
        return string.Empty;
      }

      return CombineNameAndContent(inputString, File.ReadAllBytes(fileName.LongPathPrefix()));
    }

    /// <inheritdoc/>
    public override string Write(in object? contentsWithFileName, in IDataRecord? dataRow, in Action<string>? handleWarning)
    {
      var fileName = m_FileOutPutPlaceholder;
      if (dataRow != null)
      {
        if (string.IsNullOrEmpty(m_FileOutPutPlaceholder))
          fileName = GetNameFromNameAndContent(dataRow.GetString(m_ColumnOrdinal));

        else
          // The fileNamePattern could contain placeholders that will be replaced with the value of another column
          for (var colOrdinal = 0; colOrdinal < dataRow.FieldCount; colOrdinal++)
          {
            if (colOrdinal == m_ColumnOrdinal)
            {
              var originalName = GetNameFromNameAndContent(dataRow.GetString(m_ColumnOrdinal));
              fileName = fileName.PlaceholderReplace(dataRow.GetName(colOrdinal), originalName);
              fileName = fileName.PlaceholderReplace("0", originalName);
              continue;
            }

            if (fileName.IndexOf(dataRow.GetName(colOrdinal), StringComparison.CurrentCultureIgnoreCase) == -1)
              continue;
            if (dataRow.GetFieldType(colOrdinal).GetDataType() == DataTypeEnum.Binary)
              continue;

            fileName = fileName.PlaceholderReplace(dataRow.GetName(colOrdinal),
              dataRow.IsDBNull(colOrdinal) ? string.Empty : dataRow.GetValue(colOrdinal).ToString());
          }
      }

      // Need to make sure the resulting filename does not contain invalid characters 
      foreach (var invalid in Path.GetInvalidFileNameChars())
      {
        var index = fileName.IndexOf(invalid);
        if (index != -1)
          fileName = fileName.Remove(index);
      }

      if (contentsWithFileName != null)
      {
        var fullPath = Path.Combine(m_RootFolderWrite, fileName);
        if (m_Overwrite)
          FileSystemUtils.FileDelete(fullPath);
        FileSystemUtils.WriteAllBytes(fullPath, GetContentFromNameAndContent(contentsWithFileName.ToString()));
      }

      return fileName;
    }
  }
}