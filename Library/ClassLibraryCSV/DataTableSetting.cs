namespace CsvTools
{
  public class DataTableSetting : BaseSettings, IFileSetting
  {
    public IFileSetting Clone()
    {
      var other = new DataTableSetting();
      CopyTo(other);
      return other;
    }

    public virtual bool Equals(IFileSetting other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return BaseSettingsEquals(other as BaseSettings);
    }

    public void CopyTo(IFileSetting other)
    {
      if (other == null)
        return;
      BaseSettingsCopyTo((BaseSettings) other);
    }
  }
}