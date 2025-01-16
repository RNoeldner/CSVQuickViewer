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


// Ignore Spelling: Serializer Deserialize

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  ///   A static class to help with Serialization of classes in the library
  /// </summary>
  public static class SerializedFilesLib
  {
    /// <summary>
    ///   File ending for a setting file
    /// </summary>
    public const string cSettingExtension = ".setting";

    private static readonly Lazy<Regex> RemoveEmpty =
      new Lazy<Regex>(() => new Regex(@"\s*""[^$][^""]+"":\s*\[\s*\]\,?"));

    private static readonly Lazy<Regex>
      RemoveEmpty2 = new Lazy<Regex>(() => new Regex("\\s*\"[^\"]+\":\\s*{\\s*},?"));

    private static readonly Lazy<Regex> RemoveComma = new Lazy<Regex>(() => new Regex(",(?=\\s*})"));

    /// <summary>
    /// Serialization Settings
    /// </summary>
    public static readonly Lazy<JsonSerializerSettings> JsonSerializerSettings = new Lazy<JsonSerializerSettings>(
      () =>
      {
        var setting = new JsonSerializerSettings
        {
          TypeNameHandling = TypeNameHandling.Auto,
          DefaultValueHandling = DefaultValueHandling.Ignore,
          ContractResolver = new CamelCasePropertyNamesContractResolver(),
          NullValueHandling = NullValueHandling.Ignore,
          ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
          DateFormatHandling = DateFormatHandling.IsoDateFormat,
          DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };
        setting.Converters.Add(new StringEnumConverter());
        return setting;
      });

    /// <summary>
    ///   Serialize an object with formatting
    /// </summary>
    /// <param name="data">The object to be serialized</param>
    /// <returns>The resulting Json string</returns>
    public static string SerializeIndentedJson<T>(this T data) where T : class
    {
      var json = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented, JsonSerializerSettings.Value);
      // remove empty array or class
      return RemoveComma.Value.Replace(
        RemoveEmpty2.Value.Replace(RemoveEmpty.Value.Replace(json, string.Empty), string.Empty), string.Empty);
    }

    /// <summary>
    ///   De serialize a file, looking at the file its determined if it should be read as json or xml
    /// </summary>
    /// <typeparam name="T">A class</typeparam>
    /// <param name="fileName">Name of the file</param>
    /// <returns>New instance of the class</returns>
    public static async Task<T> DeserializeFileAsync<T>(this string fileName) where T : class
    {
      Logger.Debug("Loading information from file {filename}", fileName.GetShortDisplayFileName());
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      using var improvedStream = new ImprovedStream(new SourceAccess(fileName));
      using var reader = new StreamReader(improvedStream, Encoding.UTF8, true);

      var text = await reader.ReadToEndAsync().ConfigureAwait(false);
      // Moved some classes across library, this need to be adjusted here
      foreach (var className in new[] { ".CsvFile", ".JsonFile", ".XMLFile" })
        text = text.Replace(className + ", CsvTools.ClassLibraryCSV\"",
          className + ", CsvTools.ClassLibraryValidator\"");

      if (text.StartsWith("<?xml"))
        throw new JsonReaderException("XML files are no longer supported.");
      return DeserializeText<T>(text);
    }

    /// <summary>
    ///   De serialize the text as specific type
    /// </summary>
    /// <param name="content">The Json content as text</param>
    public static T DeserializeText<T>(this string content) where T : class =>
      JsonConvert.DeserializeObject<T>(content, JsonSerializerSettings.Value)!;

    /// <summary>
    ///   De serializes the text as Json Object
    /// </summary>
    /// <param name="content">The Json content as text</param>
    /// <returns>A <see cref="JObject" /> when the text could be parsed</returns>
    /// <exception cref="JsonException">$"Returned content could not be read as Json</exception>
    public static JContainer DeserializeJson(this string content)
    {
      if (JsonConvert.DeserializeObject(content, JsonSerializerSettings.Value) is JContainer jsonData)
        return jsonData;
      throw new JsonException($"Content '{content.Substring(0, 150)}' could not be read as Json");
    }

    private static async Task<string?> GetNewContentJsonAsync(string fileName, object data)
    {
      var newContent = data.SerializeIndentedJson();
      if (!FileSystemUtils.FileExists(fileName))
        return newContent;
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      using var improvedStream = new ImprovedStream(new SourceAccess(fileName));
      using var sr = new StreamReader(improvedStream, Encoding.UTF8, true);
      var oldContent = await sr.ReadToEndAsync().ConfigureAwait(false);
      if (oldContent != newContent)
        return newContent;
      Logger.Debug("No change to file {filename}", fileName);
      return null;
    }

    /// <summary>
    ///   Serialize the data class and store the result in a file. If the file exists it will be checked if there are changes to the current content.
    /// </summary>
    /// <param name="data">The class to be serialized</param>
    /// <param name="fileName">The filename to store the serialization text</param>
    /// <param name="askOverwrite">Function to call if the file does exist, if left empty the file will be overwritten</param>
    /// <param name="withBackup">If <c>true</c> backups are </param>
    /// <returns></returns>
    public static async Task<bool> SerializeAsync<T>(this T data, string fileName, Func<bool>? askOverwrite = null,
      bool withBackup = true)
      where T : class
    {
      try
      {
        Logger.Information($"Getting content for file {fileName.GetShortDisplayFileName()}");
        var content = await GetNewContentJsonAsync(fileName, data).ConfigureAwait(false);

        // no update
        if (string.IsNullOrEmpty(content))
          return false;

        var delete = false;
        if (FileSystemUtils.FileExists(fileName))
        {
          if (askOverwrite?.Invoke() ?? true)
            delete = true;
          else
            // Exit here no overwrite allowed
            return false;
        }

        Logger.Debug("Updating file {filename}", fileName);
        if (delete)
          FileSystemUtils.DeleteWithBackup(fileName, withBackup);

        using (var improvedStream = new ImprovedStream(new SourceAccess(fileName, false)))
        using (var sr = new StreamWriter(improvedStream, Encoding.UTF8))
          await sr.WriteAsync(content);
        
        Logger.Information($"Written file {fileName.GetShortDisplayFileName()}");
      }
      catch (Exception ex)
      {
        Logger.Error(ex, "Error writing json file {filename}", fileName);
      }

      return true;
    }
  }
}