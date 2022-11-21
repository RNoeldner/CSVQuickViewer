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
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CsvTools
{
  /// <summary>
  ///   A static class to help with Serialization of classes in the library
  /// </summary>
  public static class SerializedFilesLib
  {
    /// <summary>
    /// Deserialize a file, looking at teh file its determined if it should be read as json or xml
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileName">Name of the file</param>
    /// <returns></returns>
    public static async Task<T> DeserializeAsync<T>(this string fileName) where T : class
    {
      Logger.Debug("Loading information from file {filename}", fileName);
      using var improvedStream = new ImprovedStream(new SourceAccess(fileName));
      using var reader = new StreamReader(improvedStream, Encoding.UTF8, true);

      var text = await reader.ReadToEndAsync().ConfigureAwait(false);
      if (text.StartsWith("<?xml "))
        return (T) new XmlSerializer(typeof(T)).Deserialize(new StringReader(text));

      return JsonConvert.DeserializeObject<T>(text,
        ClassLibraryCsvExtensionMethods.JsonSerializerSettings.Value)!;
    }

    private static async Task<string?> GetNewContentJsonAsync(string fileName, object data)
    {
      string content = data.SerializeIndentedJson();
      if (!FileSystemUtils.FileExists(fileName))
        return content;

      using var improvedStream = new ImprovedStream(new SourceAccess(fileName));
      using var sr = new StreamReader(improvedStream, Encoding.UTF8, true);
      if (await sr.ReadToEndAsync().ConfigureAwait(false) != content) return content;
      Logger.Debug("No change to file {filename}", fileName);
      return null;
    }

    /// <summary>
    /// Serialize the class 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="fileName"></param>
    /// <param name="askOverwrite"></param>
    /// <returns></returns>
    public static async Task SerializeAsync<T>(this T data, string fileName, Func<bool>? askOverwrite = null)
      where T : class
    {
      try
      {
        Logger.Information($"Getting content for file {FileSystemUtils.GetShortDisplayFileName(fileName)}");
        var content = await GetNewContentJsonAsync(fileName, data).ConfigureAwait(false);

        if (!string.IsNullOrEmpty(content))
        {
          var delete = false;
          if (FileSystemUtils.FileExists(fileName))
          {
            if (askOverwrite?.Invoke() ?? true)
              delete = true;
            else
              // Exit here no overwrite allowed
              return;
          }

          Logger.Debug("Updating file {filename}", fileName);
          Logger.Information($"Writing file {FileSystemUtils.GetShortDisplayFileName(fileName)}");
          if (delete)
            FileSystemUtils.DeleteWithBackup(fileName, true);
          using var improvedStream = new ImprovedStream(new SourceAccess(fileName, false));
          using var sr = new StreamWriter(improvedStream, Encoding.UTF8);
          await sr.WriteAsync(content).ConfigureAwait(false);
        }
      }
      catch (Exception ex)
      {
        Logger.Error(ex, "Error writing json file {filename}", fileName);
      }
    }

    /// <summary>
    ///   Loads the CSV file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    public static Task<CsvFile> LoadCsvFileAsync(in string fileName) => DeserializeAsync<CsvFile>(fileName);

    /// <summary>
    ///   Saves the setting for a physical file
    /// </summary>
    /// <param name="fileSettingPhysicalFile">The file setting to serialize.</param>
    /// <param name="askOverwrite">
    ///   The function to decide if we want to overwrite, usually a user prompt
    /// </param>
    /// <param name="cancellationToken"></param>
    public static async Task SaveSettingFileAsync(IFileSettingPhysicalFile fileSettingPhysicalFile,
      Func<bool> askOverwrite, CancellationToken cancellationToken)
    {
      var fileName = fileSettingPhysicalFile.FileName + CsvFile.cCsvSettingExtension;

      if (!(fileSettingPhysicalFile.Clone() is CsvFile saveSetting))
        return;
      // Remove possibly set but irrelevant properties for reading
      saveSetting.FileName = string.Empty;
      saveSetting.ID = string.Empty;
      saveSetting.Header = string.Empty;
      saveSetting.Footer = string.Empty;

      // remove not needed Columns so they do not play into comparison
      saveSetting.ColumnCollection.Clear();
      foreach (var col in fileSettingPhysicalFile.ColumnCollection)
        if (col.Ignore || (col.ValueFormat.DataType == DataTypeEnum.String && col.Convert)
                       || col.ValueFormat.DataType != DataTypeEnum.String)
          saveSetting.ColumnCollection.Add(col);

      if (!cancellationToken.IsCancellationRequested)
        await saveSetting.SerializeAsync(fileName, askOverwrite).ConfigureAwait(false);
    }
  }
}