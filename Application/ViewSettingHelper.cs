using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CsvTools
{
  public static class ViewSettingHelper
  {
    private static readonly string
      m_SettingFolder = Environment.ExpandEnvironmentVariables("%APPDATA%\\CSVQuickViewer");

    private static readonly string m_SettingPath = m_SettingFolder + "\\Setting.json";

    public static async Task<ViewSettings> LoadViewSettingsAsync()
    {
      try
      {
        return await m_SettingPath.DeserializeFileAsync<ViewSettings>();
      }
      catch (Exception ex)
      {
        Logger.Error(ex, "Loading ViewSettings {path}", m_SettingPath);
      }

      return new ViewSettings();
    }

    /// <summary>
    /// </summary>
    public static async Task SaveViewSettingsAsync(this ViewSettings viewSettings)
    {
      if (!FileSystemUtils.DirectoryExists(m_SettingFolder))
        FileSystemUtils.CreateDirectory(m_SettingFolder);

      if (await viewSettings.SerializeAsync(m_SettingPath))
      {
        // Remove backups older than a 8 hours if there are more than 10 backups
        var olderFiles = new List<string>();
        int numFiles = 0;
        foreach (var fileName in Directory.EnumerateFiles(
                   m_SettingFolder.LongPathPrefix(),
                   "Setting*.bak",
                   SearchOption.TopDirectoryOnly))
        {
          numFiles++;
          if (new FileInfo(fileName.LongPathPrefix()).LastWriteTimeUtc < DateTime.UtcNow.AddHours(-8))
            olderFiles.Add(fileName);
        }

        if (numFiles > 10)
          foreach (var fileName in olderFiles)
            FileSystemUtils.FileDelete(fileName);
      }
    }
  }
}