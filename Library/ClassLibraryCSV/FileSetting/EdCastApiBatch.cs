namespace CsvTools.FileSetting
{
  public class EdCastApiBatchWriter : StructuredFile
  {
    
    private string m_Key = "5179d23345178c06d33de6fa5cc41306";
    private string m_Secret = "7395a8245c0fa3fe0b1fffbdc7154d7f7529f79b8aa60dcb2ce6b424d9761b55";
    private string m_Host = "https://lms-europe.edcast.io";
    private string m_EndPoint = "/api/developer/v2/user_courses.json";

    public string Key
    {
      get => m_Key;
      set => SetProperty(ref m_Key, value);
    }

    public string Secret
    {
      get => m_Secret;
      set => SetProperty(ref m_Secret, value);
    }

    public string Host
    {
      get => m_Host;
      set => SetProperty(ref m_Host, value);
    }

    public string EndPoint
    {
      get => m_EndPoint;
      set => SetProperty(ref m_EndPoint, value);
    }


    protected EdCastApiBatchWriter(in string id, in string row)
     : base(id, string.Empty, row)
    {
    }

    public override object Clone()
    {
      var other = new JsonFile(ID, FileName, Row);
      CopyTo(other);
      return other;
    }

    public override void CopyTo(IFileSetting other)
    {
      base.CopyTo(other);
      if (!(other is EdCastApiBatchWriter typed))
        return;
      typed.Host = Host;
      typed.EndPoint=EndPoint;
      typed.Secret =Secret;
      typed.Key =Key;
    }

    public override bool Equals(IFileSetting? other) =>
      other is EdCastApiBatchWriter typed && BaseSettingsEquals(typed) && typed.Host == Host &&  typed.EndPoint==EndPoint &&  typed.Secret ==Secret &&  typed.Key ==Key;
  }
}
