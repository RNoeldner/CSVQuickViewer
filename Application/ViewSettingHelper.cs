﻿using System;
using System.Globalization;
using System.IO;

using System.Xml.Serialization;

namespace CsvTools
{
  public static class ViewSettingHelper
  {
    private static readonly XmlSerializer m_SerializerViewSettings = new XmlSerializer(typeof(ViewSettings));

    private static readonly string
      m_SettingFolder = Environment.ExpandEnvironmentVariables("%APPDATA%\\CSVQuickViewer");

    private static readonly string m_SettingPath = m_SettingFolder + "\\Setting.xml";

    public static ViewSettings LoadViewSettings()
    {
      try
      {
        Logger.Debug("Loading defaults {path}", m_SettingPath);
        if (FileSystemUtils.FileExists(m_SettingPath))
        {
          var serial = File.ReadAllText(m_SettingPath.LongPathPrefix());
          using (TextReader reader = new StringReader(serial))
          {
            var vs = m_SerializerViewSettings.Deserialize(reader) as ViewSettings;            
            return vs ?? new ViewSettings();
          }
        }
      }
      catch (Exception ex)
      {
        Logger.Error(ex, "Loading defaults {path}", m_SettingPath);
      }

      return new ViewSettings();
    }

    /// <summary>
    /// </summary>
    public static void SaveViewSettings(this ViewSettings viewSettings)
    {
      try
      {
        if (!FileSystemUtils.DirectoryExists(m_SettingFolder))
          FileSystemUtils.CreateDirectory(m_SettingFolder);

        FileSystemUtils.DeleteWithBackup(m_SettingPath, false);
        using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
        {
          // Remove and Restore FileName
          var oldFileName = viewSettings.FileName;
          viewSettings.FileName = string.Empty;

          m_SerializerViewSettings.Serialize(
            stringWriter,
            viewSettings,
            SerializedFilesLib.EmptyXmlSerializerNamespaces.Value);
          Logger.Debug("Saving defaults {path}", m_SettingPath);
          File.WriteAllText(m_SettingPath, stringWriter.ToString());

          viewSettings.FileName = oldFileName;
        }
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "Save Default Settings");
      }
    }
  }
}
