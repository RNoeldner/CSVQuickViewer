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

using JetBrains.Annotations;
using System;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;

namespace CsvTools
{
  /// <summary>
  ///   A static class to help with Serialization of classes in the library
  /// </summary>
  public static class SerializedFilesLib
  {
    /// <summary>
    ///   The a XML serialize namespace, used for all serialization
    /// </summary>
    public static readonly Lazy<XmlSerializerNamespaces> EmptyXmlSerializerNamespaces =
      new Lazy<XmlSerializerNamespaces>(() =>
      {
        var xmlSerializerNamespaces = new XmlSerializerNamespaces();
        xmlSerializerNamespaces.Add(string.Empty, string.Empty);
        return xmlSerializerNamespaces;
      });

    private static readonly Lazy<XmlSerializer> m_SerializerCurrentCsvFile =
      new Lazy<XmlSerializer>(() => new XmlSerializer(typeof(CsvFile)));

    /// <summary>
    ///   Loads the CSV file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    public static CsvFile LoadCsvFile([NotNull] string fileName)
    {
      var serial = FileSystemUtils.ReadAllText(fileName);
      using (TextReader reader = new StringReader(serial))
      {
        return (CsvFile) m_SerializerCurrentCsvFile.Value.Deserialize(reader);
      }
    }

    /// <summary>
    ///   Saves the setting for a physical file
    /// </summary>
    /// <param name="fileSetting">The filesetting to serialize.</param>
    /// <param name="askOverwrite">The ask overwrite.</param>
    public static void SaveSettingFile([NotNull] CsvFile fileSetting, [NotNull] Func<bool> askOverwrite)
    {
      if (fileSetting == null)
        return;
      var fileName = fileSetting.FileName + CsvFile.cCsvSettingExtension;

      var saveSetting = fileSetting.Clone() as CsvFile;
      saveSetting.FileName = string.Empty;
      saveSetting.ID = string.Empty;

      Logger.Debug("Saving setting {path}", fileName);
      string contens = null;
      using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
      {
        m_SerializerCurrentCsvFile.Value.Serialize(stringWriter, saveSetting, EmptyXmlSerializerNamespaces.Value);
        contens= stringWriter.ToString();
      }

      var delete = false;
      if (FileSystemUtils.FileExists(fileName))
      {
        var fileContend = FileSystemUtils.ReadAllText(fileName);
        if (fileContend.Equals(contens))
          return;

        if (askOverwrite.Invoke())
          delete = true;
        else
          return;
      }

      if (delete)
        FileSystemUtils.DeleteWithBackup(fileName, false);
      File.WriteAllText(fileName, contens);
    }
  }
}