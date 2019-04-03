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

using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using File = Pri.LongPath.File;

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

    #region CSV

    /// <summary>
    ///   Loads the CSV file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    public static CsvFile LoadCsvFile(string fileName)
    {
      Contract.Requires(fileName != null);
      var serial = File.ReadAllText(fileName);
      using (TextReader reader = new StringReader(serial))
      {
        return (CsvFile)m_SerializerCurrentCsvFile.Value.Deserialize(reader);
      }
    }

    /// <summary>
    ///   Saves the CSV file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="csvFile">The CSV file.</param>
    public static void SaveCsvFile(string fileName, CsvFile csvFile)
    {
      Contract.Requires(fileName != null);
      FileSystemUtils.DeleteWithBackup(fileName, false);
      using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
      {
        m_SerializerCurrentCsvFile.Value.Serialize(stringWriter, csvFile, EmptyXmlSerializerNamespaces.Value);
        File.WriteAllText(fileName, stringWriter.ToString());
      }
    }

    #endregion CSV
  }
}