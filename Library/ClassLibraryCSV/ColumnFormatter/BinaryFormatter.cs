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
    private readonly string m_FileOutPutPlaceholder;
    private readonly bool m_Overwrite;
    private readonly string m_RootFolderRead;
    private readonly string m_RootFolderWrite;

    public BinaryFormatter(string? rootFolderRead = null, string? rootFolderWrite = null, string? fileOutPutPlaceholder = null, bool overwrite = true)
    {
      m_RootFolderRead = rootFolderRead ?? string.Empty;
      m_RootFolderWrite = rootFolderWrite ?? string.Empty;
      m_FileOutPutPlaceholder = fileOutPutPlaceholder ?? string.Empty;
      m_Overwrite = overwrite;
    }

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

      if (fi.Length <= 256000)
        return Convert.ToBase64String(File.ReadAllBytes(fileName.LongPathPrefix()));
      if (RaiseWarning)
        handleWarning?.Invoke($"File {inputString} too large {fi.Length:N0} > {256000:N0}");
      return string.Empty;

    }


    /// <inheritdoc/>
    public override string Write(in object? binaryContet, in IDataRecord? dataRow, in Action<string>? handleWarning)
    {
      var fileName = m_FileOutPutPlaceholder;

      if (dataRow != null)
      {
        // try to replace all placeholders
        for (var colOrdinal = 0; colOrdinal<dataRow.FieldCount; colOrdinal++)
          fileName = fileName.PlaceholderReplace2(dataRow.GetName(colOrdinal), dataRow.GetValue(colOrdinal).ToString() ?? dataRow.GetName(colOrdinal));

        if ((fileName == m_FileOutPutPlaceholder || fileName.IndexOfAny(new[] { '{', '[' })!=-1) && RaiseWarning)
          handleWarning?.Invoke("Placeholder columns not found");

        // Need to make sure the resulting filename does not contain invalid characters 
        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
          var index = fileName.IndexOf(invalid);
          if (index != -1)
          {
            if (RaiseWarning)
              handleWarning?.Invoke($"Invalid charcater {invalid} removed from {fileName} ");
            fileName = fileName.Remove(index);
          }

        }

        // make sure the folder exists
#if !QUICK
        if (!FileSystemUtils.DirectoryExists(m_RootFolderWrite))
          FileSystemUtils.CreateDirectory(m_RootFolderWrite);
#endif
        var fullPath = Path.Combine(m_RootFolderWrite, fileName);
        if (m_Overwrite)
          FileSystemUtils.FileDelete(fullPath);

        if (binaryContet is byte[] bytes)
          FileSystemUtils.WriteAllBytes(fullPath, bytes);
        else if (binaryContet is string base64 && !string.IsNullOrEmpty(base64))
          FileSystemUtils.WriteAllBytes(fullPath, Convert.FromBase64String(base64));
      }
      return fileName;
    }
  }
}