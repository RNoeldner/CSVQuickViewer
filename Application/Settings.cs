using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace CsvTools.Properties
{
  /// <summary>
  ///   Class containing the all configuration, used in serialization to store the settings
  /// </summary>
  [Serializable]
  public class ViewSettings : CsvFile, INotifyPropertyChanged, IToolSetting
  {
    [XmlElement]
    public WindowState WindowPosition;

    [NonSerialized]
    private readonly List<IFileSetting> m_Input = new List<IFileSetting>();
    [NonSerialized]
    private readonly List<IFileSetting> m_Output = new List<IFileSetting>();

    private bool m_DetectFileChanges = true;
    private FillGuessSettings m_FillGuessSettings = ApplicationSetting.FillGuessSettings;
    private bool m_GuessCodePage = true;
    private bool m_GuessDelimiter = true;
    private bool m_GuessHasHeader = true;
    private bool m_GuessStartRow = true;
    private bool m_MenuDown = false;
    private PGPKeyStorage m_PGPKeyStorage = new PGPKeyStorage();
    private bool m_StoreSettingsByFile = false;

    [XmlIgnore]
    public string DestinationTimeZone => TimeZoneMapping.cIdLocal;

    [XmlAttribute]
    [DefaultValue(true)]
    public bool DetectFileChanges
    {
      get => m_DetectFileChanges;
      set
      {
        if (m_DetectFileChanges == value)
          return;
        m_DetectFileChanges = value;
        NotifyPropertyChanged(nameof(DetectFileChanges));
      }
    }

    /// <summary>
    ///   Gets or sets the fill guess settings.
    /// </summary>
    /// <value>
    ///   The fill guess settings.
    /// </value>
    [XmlElement]
    public virtual FillGuessSettings FillGuessSettings
    {
      get
      {
        return m_FillGuessSettings;
      }
      set
      {
        var newVal = value ?? ApplicationSetting.FillGuessSettings;
        if (m_FillGuessSettings.Equals(newVal)) return;
        m_FillGuessSettings = newVal;
        NotifyPropertyChanged(nameof(FillGuessSettings));
      }
    }

    /// <summary>
    ///   Gets a value indicating whether fill guess settings as specified
    /// </summary>
    /// <value>
    ///   <c>true</c> if [fill guess settings specified]; otherwise, <c>false</c>.
    /// </value>
    [XmlIgnore]
    public virtual bool FillGuessSettingsSpecified => !m_FillGuessSettings.Equals(new FillGuessSettings());

    [XmlAttribute]
    [DefaultValue(true)]
    public bool GuessCodePage
    {
      get => m_GuessCodePage;
      set
      {
        if (m_GuessCodePage == value)
          return;
        m_GuessCodePage = value;
        NotifyPropertyChanged(nameof(GuessCodePage));
      }
    }

    [XmlAttribute]
    [DefaultValue(true)]
    public bool GuessDelimiter
    {
      get => m_GuessDelimiter;

      set
      {
        if (m_GuessDelimiter == value)
          return;
        m_GuessDelimiter = value;
        NotifyPropertyChanged(nameof(GuessDelimiter));
      }
    }

    [XmlAttribute]
    [DefaultValue(true)]
    public bool GuessHasHeader
    {
      get => m_GuessHasHeader;
      set
      {
        if (m_GuessHasHeader == value)
          return;
        m_GuessHasHeader = value;
        NotifyPropertyChanged(nameof(GuessHasHeader));
      }
    }

    [XmlAttribute]
    [DefaultValue(true)]
    public bool GuessStartRow
    {
      get => m_GuessStartRow;
      set
      {
        if (m_GuessStartRow == value)
          return;
        m_GuessStartRow = value;
        NotifyPropertyChanged(nameof(GuessStartRow));
      }
    }
    [XmlIgnore]
    public ICollection<IFileSetting> Input => m_Input;

    [XmlAttribute]
    [DefaultValue(false)]
    public bool MenuDown
    {
      get => m_MenuDown; set
      {
        if (m_MenuDown == value)
          return;
        m_MenuDown = value;
        NotifyPropertyChanged(nameof(MenuDown));
      }
    }

    [XmlIgnore]
    public ICollection<IFileSetting> Output => m_Output;

    [XmlElement]
    public virtual PGPKeyStorage PGPInformation
    {
      get
      {
        return m_PGPKeyStorage;
      }
      set => m_PGPKeyStorage = value ?? new PGPKeyStorage();
    }

    [XmlIgnore]
    public virtual bool PGPInformationSpecified => m_PGPKeyStorage.Specified;

    [XmlIgnore]
    public string RootFolder => string.Empty;
    [XmlAttribute]
    [DefaultValue(false)]
    public bool StoreSettingsByFile
    {
      get => m_StoreSettingsByFile; set
      {
        if (m_StoreSettingsByFile == value)
          return;
        m_StoreSettingsByFile = value;
        NotifyPropertyChanged(nameof(StoreSettingsByFile));
      }
    }

    public ICache<string, ValidationResult> ValidationResultCache => null;
  }
}