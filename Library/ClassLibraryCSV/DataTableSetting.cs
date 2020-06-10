using System;

namespace CsvTools
{
  public sealed class DataTableSetting : BaseSettings, IFileSetting
  {
    public DataTableSetting(string tableName)
    {
      FileName = tableName;
      ID = Guid.NewGuid().ToString();
      HasFieldHeader = true;
    }

    public IFileSetting Clone()
    {
      var other = new DataTableSetting(FileName);
      CopyTo(other);
      return other;
    }

    public bool Equals(IFileSetting other)
    {
      if (other is null)
        return false;
      return ReferenceEquals(this, other) || BaseSettingsEquals(other as BaseSettings);
    }

    public void CopyTo(IFileSetting other)
    {
      if (other == null)
        return;
      BaseSettingsCopyTo((BaseSettings) other);
    }
  }
}