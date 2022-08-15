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
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CsvTools
{
  /// <summary>
  ///   A static class to help with Serialization of classes in the library
  /// </summary>
  public static class SerializedFilesLib
  {
  
    private static readonly Lazy<XmlSerializer> m_SerializerCurrentCsvFile =
      new Lazy<XmlSerializer>(() => new XmlSerializer(typeof(CsvFile)));

    /// <summary>
    ///   Loads the CSV file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    public static CsvFile LoadCsvFile(in string fileName)
    {
      using var improvedStream = new ImprovedStream(new SourceAccess(fileName));
      using var reader = new StreamReader(improvedStream, Encoding.UTF8, true);
      return (CsvFile) m_SerializerCurrentCsvFile.Value.Deserialize(reader);
    }

    

    /// <summary>
    ///   Saves the setting for a physical file
    /// </summary>
    /// <param name="fileSettingPhysicalFile">The file setting to serialize.</param>
    /// <param name="askOverwrite">
    ///   The function to decide if we want to overwrite, usually a user prompt
    /// </param>
    public static void SaveSettingFile(in IFileSettingPhysicalFile fileSettingPhysicalFile, Func<bool> askOverwrite)
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

      Logger.Debug("Saving setting {path}", fileName);
      string contend = m_SerializerCurrentCsvFile.Value.SerializeIndented(saveSetting);
      
      var delete = false;
      if (FileSystemUtils.FileExists(fileName))
      {
        var fileContend = FileSystemUtils.ReadAllText(fileName);
        // Check if we have actual changes
        if (fileContend.Equals(contend, StringComparison.OrdinalIgnoreCase))
          // what we want to write and what is written does mach, exit here do not save
          return;

        if (askOverwrite.Invoke())
          delete = true;
        else
          // Exit here no overwrite allowed
          return;
      }

      if (delete)
        FileSystemUtils.DeleteWithBackup(fileName, false);
      File.WriteAllText(fileName, contend);
    }
  }
}