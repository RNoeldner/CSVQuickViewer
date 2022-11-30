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
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;


namespace CsvTools
{
  /// <summary>
  ///   A static class to help with Serialization of classes in the library
  /// </summary>
  public static class SerializedFilesLib
  {
    private static readonly Lazy<Regex> m_RemoveEmpty =
      new Lazy<Regex>(() => new Regex(@"\s*""[^$][^""]+"":\s*\[\s*\]\,?"));

    private static readonly Lazy<Regex>
      m_RemoveEmpty2 = new Lazy<Regex>(() => new Regex("\\s*\"[^\"]+\":\\s*{\\s*},?"));

    private static readonly Lazy<Regex> m_RemoveComma = new Lazy<Regex>(() => new Regex(",(?=\\s*})"));

    private static readonly Lazy<XmlSerializerNamespaces> m_EmptyXmlSerializerNamespaces =
      new Lazy<XmlSerializerNamespaces>(
        () =>
        {
          var xmlSerializerNamespaces = new XmlSerializerNamespaces();
          xmlSerializerNamespaces.Add(string.Empty, string.Empty);
          return xmlSerializerNamespaces;
        });

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
    /// <param name="serializer">The serializer for the passed in object</param>
    /// <returns>The XML string</returns>
    public static string SerializeIndentedXml<T>(this T data, in XmlSerializer serializer) where T : class
    {
      using var stringWriter = new StringWriter();
      using var textWriter = new XmlTextWriter(stringWriter);
      textWriter.Formatting = System.Xml.Formatting.Indented;
      serializer.Serialize(textWriter, data, m_EmptyXmlSerializerNamespaces.Value);
      return stringWriter.ToString();
    }


    /// <summary>
    ///   Serialize an object with formatting
    /// </summary>
    /// <param name="data">The object to be serialized</param>
    /// <returns>The resulting Json string</returns>
    public static string SerializeIndentedJson<T>(this T data) where T : class
    {
      var json = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented, JsonSerializerSettings.Value);
      // remove empty array or class
      return m_RemoveComma.Value.Replace(
        m_RemoveEmpty2.Value.Replace(m_RemoveEmpty.Value.Replace(json, string.Empty), string.Empty), string.Empty);
    }

    /// <summary>
    ///   Deserialize a file, looking at teh file its determined if it should be read as json or xml
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileName">Name of the file</param>
    /// <returns></returns>
    public static async Task<T> DeserializeFileAsync<T>(this string fileName) where T : class
    {
      Logger.Debug("Loading information from file {filename}", FileSystemUtils.GetShortDisplayFileName(fileName));
#if NETSTANDARD2_1_OR_GREATER
      await
#endif
      using var improvedStream = new ImprovedStream(new SourceAccess(fileName));
      using var reader = new StreamReader(improvedStream, Encoding.UTF8, true);

      var text = await reader.ReadToEndAsync().ConfigureAwait(false);
      if (text.StartsWith("<?xml "))
        return (T) new XmlSerializer(typeof(T)).Deserialize(new StringReader(text));

      return DeserializeText<T>(text);
    }

    /// <summary>
    ///   Deserializes the text as specific type
    /// </summary>
    /// <param name="content">The Json content as text</param>
    public static T DeserializeText<T>(this string content) where T : class =>
      JsonConvert.DeserializeObject<T>(content, JsonSerializerSettings.Value)!;

    /// <summary>
    ///   Deserializes the text as Json Object
    /// </summary>
    /// <param name="content">The Json content as text</param>
    /// <returns>A <see cref="JObject" /> when teh text could be parsed</returns>
    /// <exception cref="JsonException">$"Returned content xxx could not be read as Json</exception>
    public static JContainer DeserializeJson(this string content)
    {
      if (JsonConvert.DeserializeObject(content, JsonSerializerSettings.Value) is JContainer jsonData)
        return jsonData;
      throw new JsonException($"Content '{content.Substring(0, 150)}' could not be read as Json");
    }

    /*
    public static JContainer DeserializeJson(this Stream stream)
    {
      if (stream is null)
        throw new ArgumentNullException(nameof(stream));
      using var sr = new StreamReader(stream);
      using var reader = new JsonTextReader(sr);
      var serializer = new JsonSerializer();
      if (serializer.Deserialize(reader) is JContainer jsonData)
        return jsonData;

      if (!stream.CanSeek)
        throw new JsonException("Stream could not be read as Json");
      sr.DiscardBufferedData();
      stream.Seek(0, System.IO.SeekOrigin.Begin);
      throw new JsonException($"Stream '{sr.ReadLine()?.Substring(0, 150)}' could not be read as Json");
    }
    */

    private static async Task<string?> GetNewContentJsonAsync(string fileName, object data)
    {
      var content = data.SerializeIndentedJson();
      if (!FileSystemUtils.FileExists(fileName))
        return content;
#if NETSTANDARD2_1_OR_GREATER
      await
#endif
      using var improvedStream = new ImprovedStream(new SourceAccess(fileName));
      using var sr = new StreamReader(improvedStream, Encoding.UTF8, true);
      if (await sr.ReadToEndAsync().ConfigureAwait(false) != content) return content;
      Logger.Debug("No change to file {filename}", fileName);
      return null;
    }

    /// <summary>
    ///   Serialize the data class and store teh result in a file. If the file exists it will be checked if there are changes to teh current content.
    /// </summary>
    /// <param name="data">The class to be serialized</param>
    /// <param name="fileName">The filename to store the serialization text</param>
    /// <param name="askOverwrite">Function to call if teh file does exists, if left empty the file will be overwritten</param>
    /// <returns></returns>
    public static async Task<bool> SerializeAsync<T>(this T data, string fileName, Func<bool>? askOverwrite = null)
      where T : class
    {
      try
      {
        Logger.Information($"Getting content for file {FileSystemUtils.GetShortDisplayFileName(fileName)}");
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
        Logger.Information($"Writing file {FileSystemUtils.GetShortDisplayFileName(fileName)}");
        if (delete)
          FileSystemUtils.DeleteWithBackup(fileName, true);
#if NETSTANDARD2_1_OR_GREATER
          await
#endif
        using var improvedStream = new ImprovedStream(new SourceAccess(fileName, false));
#if NETSTANDARD2_1_OR_GREATER
          await
#endif
        using var sr = new StreamWriter(improvedStream, Encoding.UTF8);
        await sr.WriteAsync(content).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
        Logger.Error(ex, "Error writing json file {filename}", fileName);
      }

      return true;
    }

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