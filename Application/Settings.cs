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
    public WindowState WindowPosition { get; set; }

    [XmlIgnore]
    public string DestinationTimeZone => TimeZoneMapping.cIdLocal;

    [XmlAttribute]
    [DefaultValue(true)]
    public bool DetectFileChanges { get; set; }

    /// <summary>
    ///   Gets or sets the fill guess settings.
    /// </summary>
    /// <value>
    ///   The fill guess settings.
    /// </value>
    [XmlElement]
    public virtual FillGuessSettings FillGuessSettings { get; set; }


    /// <summary>
    ///   Gets a value indicating whether fill guess settings as specified
    /// </summary>
    /// <value>
    ///   <c>true</c> if [fill guess settings specified]; otherwise, <c>false</c>.
    /// </value>
    [XmlIgnore]
    public virtual bool FillGuessSettingsSpecified => !FillGuessSettings.Equals(new FillGuessSettings());

    [XmlAttribute]
    [DefaultValue(true)]
    public bool GuessCodePage { get; set; } = true;

    [XmlAttribute]
    [DefaultValue(true)]
    public bool GuessDelimiter { get; set; } = true;


    [XmlAttribute]
    [DefaultValue(true)]
    public bool GuessHasHeader { get; set; } = true;


    [XmlAttribute]
    [DefaultValue(true)]
    public bool GuessStartRow { get; set; } = true;

    [XmlIgnore]
    public ICollection<IFileSetting> Input { get; } = new List<IFileSetting>();

    [XmlAttribute]
    [DefaultValue(false)]
    public bool MenuDown { get; set; } = false;

    [XmlIgnore]
    public ICollection<IFileSetting> Output { get; } = new List<IFileSetting>();

    [XmlElement]
    public virtual PGPKeyStorage PGPInformation { get; set; } = new PGPKeyStorage();


    [XmlIgnore]
    public virtual bool PGPInformationSpecified => m_PGPKeyStorage.Specified;

    [XmlIgnore]
    public string RootFolder => string.Empty;

    [XmlAttribute]
    [DefaultValue(false)]
    public bool StoreSettingsByFile { get; set; } = false;

    public ICache<string, ValidationResult> ValidationResultCache => null;
  }
}