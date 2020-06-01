using System;

namespace CsvTools
{
  public class DataTableSetting : BaseSettings, IFileSetting
  {
    public DataTableSetting(string tableName)
    {
      base.FileName = tableName;
      base.ID = Guid.NewGuid().ToString();
      base.HasFieldHeader = true;
    }

    public IFileSetting Clone()
    {
      var other = new DataTableSetting(FileName);
      CopyTo(other);
      return other;
    }

    public virtual bool Equals(IFileSetting other)
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