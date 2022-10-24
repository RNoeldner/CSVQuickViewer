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
using System.ComponentModel;
using System.Xml.Serialization;

namespace CsvTools
{
  /// <inheritdoc />
  /// <summary>
  ///   Class containing the all configuration, used in serialization to store the settings
  /// </summary>
  [Serializable]
  public sealed class ViewSettings : NotifyPropertyChangedBase
  {
    private bool m_AllowJson = true;
    private bool m_DetectFileChanges = true;
    private FillGuessSettings m_FillGuessSettings = new FillGuessSettings();
    private bool m_GuessCodePage = true;
    private bool m_GuessComment = true;
    private bool m_GuessDelimiter = true;
    private bool m_GuessHasHeader = true;
    private bool m_GuessNewLine = true;
    private bool m_GuessQualifier = true;
    private bool m_GuessStartRow = true;
    private HtmlStyle m_HtmlStyle = new HtmlStyle();
    private bool m_MenuDown;
    private bool m_StoreSettingsByFile;
    private bool m_DisplayStartLineNo = true;
    private bool m_DisplayRecordNo;

    public enum Duration
    {
      [Description("unlimited")] Unlimited,

      [Description("1/2 second")] HalfSecond,

      [Description("1 second")] Second,

      [Description("2 seconds")] TwoSecond,

      [Description("10 seconds")] TenSecond,
    }

    [XmlElement]
    [DefaultValue(false)]
    public bool DisplayRecordNo
    {
      get => m_DisplayRecordNo;
      set => SetField(ref m_DisplayRecordNo, value);
    }

    [XmlElement]
    [DefaultValue(true)]
    public bool DisplayStartLineNo
    {
      get => m_DisplayStartLineNo;
      set => SetField(ref m_DisplayStartLineNo, value);
    }

    [XmlElement]
    public WindowState WindowPosition
    {
      get;
      set;
    } = new WindowState();


    [XmlAttribute]
    [DefaultValue(true)]
    public bool AllowJson
    {
      get => m_AllowJson;
      set => SetField(ref m_AllowJson, value);
    }

    [XmlAttribute]
    [DefaultValue(true)]
    public bool DetectFileChanges
    {
      get => m_DetectFileChanges;
      set => SetField(ref m_DetectFileChanges, value);
    }

    [XmlIgnore]
    public TimeSpan DurationTimeSpan
    {
      get
      {
        return LimitDuration switch
        {
          Duration.HalfSecond => TimeSpan.FromSeconds(.5),
          Duration.Second => TimeSpan.FromSeconds(1),
          Duration.TwoSecond => TimeSpan.FromSeconds(2),
          Duration.TenSecond => TimeSpan.FromSeconds(10),
          _ => TimeSpan.FromMinutes(60),
        };
      }
    }

    /// <summary>
    ///   Gets or sets the fill guess settings.
    /// </summary>
    /// <value>The fill guess settings.</value>
    [XmlElement]
#if NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public FillGuessSettings FillGuessSettings
    {
      get => m_FillGuessSettings;
      set => SetField(ref m_FillGuessSettings, value);
    }

    /// <summary>
    ///   Gets a value indicating whether fill guess settings as specified
    /// </summary>
    /// <value><c>true</c> if [fill guess settings specified]; otherwise, <c>false</c>.</value>
    [XmlIgnore]
    public bool FillGuessSettingsSpecified => !m_FillGuessSettings.Equals(new FillGuessSettings());

    [XmlAttribute]
    [DefaultValue(true)]
    public bool GuessCodePage
    {
      get => m_GuessCodePage;
      set => SetField(ref m_GuessCodePage, value);
    }

    [XmlAttribute]
    [DefaultValue(true)]
    public bool GuessDelimiter
    {
      get => m_GuessDelimiter;
      set => SetField(ref m_GuessDelimiter, value);
    }

    [XmlAttribute]
    [DefaultValue(true)]
    public bool GuessHasHeader
    {
      get => m_GuessHasHeader;
      set => SetField(ref m_GuessHasHeader, value);
    }

    [XmlAttribute]
    [DefaultValue(true)]
    public bool GuessNewLine
    {
      get => m_GuessNewLine;
      set => SetField(ref m_GuessNewLine, value);
    }

    [XmlAttribute]
    [DefaultValue(true)]
    public bool GuessComment
    {
      get => m_GuessComment;
      set => SetField(ref m_GuessComment, value);
    }

    [XmlAttribute]
    [DefaultValue(false)]
    public bool GuessQualifier
    {
      get => m_GuessQualifier;
      set => SetField(ref m_GuessQualifier, value);
    }

    [XmlAttribute]
    [DefaultValue(true)]
    public bool GuessStartRow
    {
      get => m_GuessStartRow;
      set => SetField(ref m_GuessStartRow, value);
    }

    [XmlIgnore] public HtmlStyle HtmlStyle => m_HtmlStyle;

    [XmlElement]
    [DefaultValue(Duration.Second)]
    public Duration LimitDuration
    {
      get;
      set;
    } = Duration.Second;

    [XmlAttribute]
    [DefaultValue(false)]
    public bool MenuDown
    {
      get => m_MenuDown;
      set => SetField(ref m_MenuDown, value);
    }

    [XmlAttribute]
    [DefaultValue(false)]
    public bool StoreSettingsByFile
    {
      get => m_StoreSettingsByFile;
      set => SetField(ref m_StoreSettingsByFile, value);
    }

    [XmlElement(ElementName = "HTMLStyle")]
#if NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public string Style
    {
      get => m_HtmlStyle.Style;
      set
      {
        // ReSharper disable once ConstantNullCoalescingCondition
        var newVal = value ?? HtmlStyle.cStyle;
        if (m_HtmlStyle.Style.Equals(newVal))
          return;
        SetField(ref m_HtmlStyle, new HtmlStyle(newVal));
      }
    }
  }
}