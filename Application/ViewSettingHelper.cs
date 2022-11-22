using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


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
        return await m_SettingPath.DeserializeAsync<ViewSettings>();
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
      await viewSettings.SerializeAsync(m_SettingPath, null);
    }
  }
}