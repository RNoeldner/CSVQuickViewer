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
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CsvTools.Tests
{
  public class MimicSQLReader
  {
    private readonly Dictionary<IFileSetting,DataTable> m_ReadSetting = new Dictionary<IFileSetting, DataTable>();

    public void AddSetting(IFileSetting setting)
    {
      if (setting == null || string.IsNullOrEmpty(setting.ID))
      {
        throw new ArgumentNullException(nameof(setting));
      }

      if (!m_ReadSetting.Any(x => x.Key.ID.Equals(setting.ID, StringComparison.OrdinalIgnoreCase)))
        m_ReadSetting.Add(setting, null);
    }

    public void AddSetting(string name, DataTable dt)
    {
      if (dt == null) throw new ArgumentNullException(nameof(dt));
      if (string.IsNullOrEmpty(name))
      {
        throw new ArgumentNullException(nameof(name));
      }

      if (!m_ReadSetting.Any(x => x.Key.ID.Equals(name, StringComparison.OrdinalIgnoreCase)))
        m_ReadSetting.Add(new CsvFile(name) {ID = name}, dt);
    }
    public ICollection<IFileSetting> ReadSettings => m_ReadSetting.Keys;

    public IDataReader ReadData(string settingName, IProcessDisplay processDisplay, int timeout)
    {
      var  setting = m_ReadSetting.Any(x => x.Key.ID == settingName)? m_ReadSetting.First(x => x.Key.ID == settingName) : m_ReadSetting.First();
      if (setting.Value == null)
      {

        var reader = setting.Key.GetFileReader(processDisplay);
        reader.Open();
        return reader;
      }
      else
      {
        return setting.Value.CreateDataReader();
      }
    }
  }
}