using System;
using System.IO;
using System.Threading.Tasks;

namespace CsvTools
{
  public static class ViewSettingHelper
  {
    private static readonly string
      SettingFolder = Environment.ExpandEnvironmentVariables("%APPDATA%\\CSVQuickViewer");

    private static readonly string SettingPath = SettingFolder + "\\Setting.json";

    /// <summary>
    /// Load the ViewSettings in the fileSystem
    /// </summary>
    public static async Task<ViewSettings> LoadViewSettingsAsync()
    {
      try
      {
        return await SettingPath.DeserializeFileAsync<ViewSettings>();
      }
      catch (Exception ex)
      {
        Logger.Error(ex, "Loading ViewSettings {path}", SettingPath);
      }

      return new ViewSettings();
    }

    /// <summary>
    /// Store the ViewSettings in the fileSystem
    /// </summary>
    public static async Task SaveViewSettingsAsync(this ViewSettings viewSettings)
    {
      if (!FileSystemUtils.DirectoryExists(SettingFolder))
        FileSystemUtils.CreateDirectory(SettingFolder);

      await viewSettings.SerializeAsync(SettingPath, null, false);
    }
  }
}