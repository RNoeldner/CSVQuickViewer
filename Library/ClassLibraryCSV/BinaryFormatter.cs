// /*
//  * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
//  *
//  * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
//  * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//  *
//  * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
//  * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
//  *
//  * You should have received a copy of the GNU Lesser Public License along with this program.
//  * If not, see http://www.gnu.org/licenses/ .
//  *
//  */

using System;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  public static class BinaryFormatter
  {
    public static string GetFileName(string fileNamePattern, IDataRecord? dataRow)
    {
      if (dataRow is null)
        return fileNamePattern;

      var fileName = fileNamePattern;
      for (int colOrdinal = 0; colOrdinal < dataRow.FieldCount; colOrdinal++)
      {
        if (fileName.IndexOf(dataRow.GetName(colOrdinal), StringComparison.CurrentCultureIgnoreCase) == -1)
          continue;
        if (dataRow.GetFieldType(colOrdinal).GetDataType() == DataType.Binary)
          continue;

        string repl = string.Empty;

        if (!dataRow.IsDBNull(colOrdinal))
        {
          repl = dataRow.GetValue(colOrdinal).ToString();
          foreach (var ill in Path.GetInvalidFileNameChars())
          {
            var index = repl.IndexOf(ill);
            if (index != -1)
              repl = repl.Remove(index);
          }
        }

        fileName = fileName.PlaceholderReplace(dataRow.GetName(colOrdinal), repl);
      }

      return fileName;
    }

    public static async Task WriteFileAsync(byte[]? binaryData, string folder, string fileName, bool overwrite, Action<string>? handleWarning,
                                       CancellationToken cancellationToken)
    {
      if (binaryData is null || binaryData.GetLength(0) == 0)
        return;
      try
      {
        var fullPath = Path.Combine(folder, fileName);
        if (overwrite)
          FileSystemUtils.FileDelete(fullPath);

        using var stream = FileSystemUtils.OpenWrite(fullPath);
        await stream.WriteAsync(binaryData, 0, binaryData.GetLength(0), cancellationToken).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        Logger.Warning(e, "Could not write file {filename}", fileName);
        handleWarning?.Invoke(e.Message);
      }
    }
  }
}