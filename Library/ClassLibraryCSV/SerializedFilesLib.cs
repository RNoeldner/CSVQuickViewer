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
// Ignore Spelling: Serializer Deserialize

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools;

/// <summary>
///   A static class to help with Serialization of classes in the library
/// </summary>
public static class SerializedFilesLib
{
  /// <summary>
  ///   File ending for a setting file
  /// </summary>
  public const string cSettingExtension = ".setting";

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
  /// Creates a safe, shallow copy of the specified <see cref="JsonSerializerSettings"/>.
  /// The global settings instance stays untouched, and mutable collections such as
  /// <see cref="JsonSerializerSettings.Converters"/> are duplicated.
  /// </summary>
  public static JsonSerializerSettings CloneJsonSettings(this JsonSerializerSettings original)
  {
    return new JsonSerializerSettings
    {
      TypeNameHandling = original.TypeNameHandling,
      DefaultValueHandling = original.DefaultValueHandling,
      ContractResolver = original.ContractResolver,
      NullValueHandling = original.NullValueHandling,
      ReferenceLoopHandling = original.ReferenceLoopHandling,
      DateFormatHandling = original.DateFormatHandling,
      DateTimeZoneHandling = original.DateTimeZoneHandling,
      Converters = new List<JsonConverter>(original.Converters)
    };
  }
  /// <summary>
  /// Recursively removes empty objects and empty arrays from a JSON structure.
  /// Also removes objects that contain only "$type".
  /// Returns true if the token itself is considered empty and should be removed by its parent.
  /// Nulls and empty strings are preserved.
  /// </summary>
  private static bool RemoveEmptyTokens(JToken token)
  {
    switch (token.Type)
    {
      case JTokenType.Object:
        var obj = (JObject) token;
        // copy property list to avoid modifying during enumeration
        foreach (var prop in obj.Properties().ToList().Where(prop => RemoveEmptyTokens(prop.Value)))
        {
          prop.Remove();
        }

        // Remove object if empty OR contains only "$type"
        return !obj.HasValues || (obj.Properties().Count() == 1 && obj.Property("$type") != null);

      case JTokenType.Array:
        var arr = (JArray) token;
        // copy items to avoid modifying during enumeration
        foreach (var item in arr.ToList().Where(item => RemoveEmptyTokens(item)))
        {
          item.Remove();
        }
        // if array has no items left it's empty
        return !arr.HasValues;

      case JTokenType.None:
      case JTokenType.Constructor:
      case JTokenType.Property:
      case JTokenType.Comment:
      case JTokenType.Integer:
      case JTokenType.Float:
      case JTokenType.String:
      case JTokenType.Boolean:
      case JTokenType.Null:
      case JTokenType.Undefined:
      case JTokenType.Date:
      case JTokenType.Raw:
      case JTokenType.Bytes:
      case JTokenType.Guid:
      case JTokenType.Uri:
      case JTokenType.TimeSpan:
      default:
        // Leave primitives (including nulls and empty strings)
        return false;
    }
  }

  /// <summary>
  /// Serializes an object to indented JSON using the configured serializer settings
  /// and removes empty arrays and empty objects.
  /// </summary>
  public static string SerializeIndentedJson<T>(this T data) where T : class
  {
    if (data == null) throw new ArgumentNullException(nameof(data));

    // Create a serializer with your settings
    var serializer = JsonSerializer.Create(JsonSerializerSettings.Value);

    // Convert the object into a JToken (respects all settings)
    var token = JToken.FromObject(data, serializer);

    // Remove empty objects and arrays recursively
    RemoveEmptyTokens(token);

    // Return formatted JSON
    return token.ToString(Formatting.Indented);
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
    using var improvedStream = FunctionalDI.GetStream(new SourceAccess(fileName));
    using var reader = new StreamReader(improvedStream, Encoding.UTF8, true);

    var text = await reader.ReadToEndAsync().ConfigureAwait(false);
    // Moved some classes across library, this need to be adjusted here
    text = new[] { ".CsvFile", ".JsonFile", ".XMLFile" }.Aggregate(text, (current, className) => current.Replace(className + ", CsvTools.ClassLibraryCSV\"", className + ", CsvTools.ClassLibraryValidator\""));

    if (text.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
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
    using var improvedStream = FunctionalDI.GetStream(new SourceAccess(fileName));
    using var sr = new StreamReader(improvedStream, Encoding.UTF8, true);
    var oldContent = await sr.ReadToEndAsync().ConfigureAwait(false);
    if (!string.Equals(oldContent, newContent, StringComparison.OrdinalIgnoreCase))
      return newContent;
    try { Logger.Debug("No change to file {filename}", fileName); }
    catch
    {
      // ignored
    }

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

      using (var improvedStream = FunctionalDI.GetStream(new SourceAccess(fileName, false)))
      using (var sr = new StreamWriter(improvedStream, Encoding.UTF8))
        await sr.WriteAsync(content).ConfigureAwait(false);

      Logger.Information($"Written file {fileName.GetShortDisplayFileName()}");
    }
    catch (Exception ex)
    {
      try { Logger.Error(ex, "Error writing json file {filename}", fileName); } catch { }
    }

    return true;
  }
}