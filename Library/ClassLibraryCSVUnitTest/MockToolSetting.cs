using System.Collections.Generic;

namespace CsvTools.Tests
{
  public class MockToolSetting : IToolSetting
  {
    private readonly List<IFileSetting> m_Input = new List<IFileSetting>();
    private readonly List<IFileSetting> m_Output = new List<IFileSetting>();

    public string EncryptedPassphase => ".";

    public ICollection<IFileSetting> Input => m_Input;

    public ICollection<IFileSetting> Output => m_Output;

    public string RootFolder => string.Empty;

    public ICache<string, ValidationResult> ValidationResultCache => null;

    public PGPKeyStorage PGPInformation => new PGPKeyStorage();

    public string DestinationTimeZone => TimeZoneMapping.cIdLocal;
  }
}