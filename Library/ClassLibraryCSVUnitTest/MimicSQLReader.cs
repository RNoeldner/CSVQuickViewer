using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CsvTools.Tests
{
  public class MimicSQLReader
  {
    private List<IFileSetting> m_ReadSetting = new List<IFileSetting>();

    public MimicSQLReader()
    {
      ApplicationSetting.SQLDataReader = ReadData;
    }

    public void AddSetting(IFileSetting setting)
    {
      if (setting == null || string.IsNullOrEmpty(setting.ID))
      {
        throw new ArgumentNullException(nameof(setting));
      }

      if (!m_ReadSetting.Any(x => x.ID.Equals(setting.ID)))
        m_ReadSetting.Add(setting);
    }

    public List<IFileSetting> ReadSettings { get => m_ReadSetting; }

    public IDataReader ReadData(string settingName)
    {
      var setting = m_ReadSetting.FirstOrDefault(x => x.ID == settingName);
      if (setting == null)
        throw new ApplicationException($"{settingName} not found");
      var reader = setting.GetFileReader();
      reader.Open(System.Threading.CancellationToken.None, false);
      return reader;
    }
  }
}