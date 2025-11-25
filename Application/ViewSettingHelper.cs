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
using System;
using System.Threading.Tasks;

namespace CsvTools;

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
      try { Logger.Error(ex, "Loading ViewSettings {path}", SettingPath); } catch { }

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