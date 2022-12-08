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
#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  public class MimicSQLReader
  {
    private readonly Dictionary<IFileSetting, DataTable?> m_ReadSetting = new Dictionary<IFileSetting, DataTable?>();

    public void AddSetting(IFileSetting setting)
    {
      if (setting == null || string.IsNullOrEmpty(setting.ID)) throw new ArgumentNullException(nameof(setting));

      if (!m_ReadSetting.Any(x => x.Key.ID.Equals(setting.ID, StringComparison.OrdinalIgnoreCase)))
        m_ReadSetting.Add(setting, null);
    }

    public void RemoveSetting(IFileSetting setting)
    {
      m_ReadSetting[setting]?.Dispose();
      m_ReadSetting.Remove(setting);
    }

    public void RemoveSetting(string name)
        => RemoveSetting(m_ReadSetting.First(x => x.Key.ID.Equals(name)).Key);

    public void AddSetting(string name, DataTable dt)
    {
      if (dt == null) throw new ArgumentNullException(nameof(dt));
      if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

      if (!m_ReadSetting.Any(x => x.Key.ID.Equals(name, StringComparison.OrdinalIgnoreCase)))
        m_ReadSetting.Add(new CsvFile(id: name, fileName: name), dt);
    }

    public async Task<IFileReader> ReadDataAsync(string settingName, int timeout, long limit, CancellationToken token)
    {
      if (m_ReadSetting.Count == 0)
      {
        Logger.Information($"{settingName} not found");
        throw new ApplicationException($"{settingName} not found");
      }

      var setting = m_ReadSetting.Any(x => x.Key.ID == settingName)
        ? m_ReadSetting.First(x => x.Key.ID == settingName)
        : m_ReadSetting.First();

      var reader = setting.Value != null
        ? new DataTableWrapper(setting.Value)
        : FunctionalDI.GetFileReader(setting.Key, token);
      await reader.OpenAsync(token).ConfigureAwait(false);
      return reader;
    }
  }
}