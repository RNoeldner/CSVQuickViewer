/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Threading;

namespace CsvTools;

/// <inheritdoc cref="CsvTools.ObservableObject" />
/// <summary>
///   Class containing the all configuration, used in serialization to store the settings
/// </summary>
public sealed class ViewSettings : ObservableObject, IFontConfig
{
  private bool m_AllowJson = true;
  private bool m_AllowRowCombining = false;
  private bool m_AutoStartRemaining = false;
  private bool m_DetectFileChanges = true;
  private bool m_DisplayRecordNo;
  private bool m_DisplayStartLineNo = true;
  private FillGuessSettings m_FillGuessSettings = FillGuessSettings.Default;
  private string m_Font = "Tahoma";
  private float m_FontSize = 8.25f;
  private bool m_GuessCodePage = true;
  private bool m_GuessComment = true;
  private bool m_GuessDelimiter = true;
  private bool m_GuessEscapePrefix = true;
  private bool m_GuessHasHeader = true;
  private bool m_GuessNewLine = true;
  private bool m_GuessQualifier = true;
  private bool m_GuessStartRow = true;
  private HtmlStyle m_HtmlStyle = HtmlStyle.Default;
#if SupportPGP
    private string m_KeyFileRead = string.Empty;
    private string m_KeyFileWrite = string.Empty;
#endif
  private Duration m_LimitDuration = Duration.FiveSecond;
  private bool m_MenuDown;
  private int m_NumWarnings = 0;
  private int m_ShowButtonAtLength = 2000;
  private bool m_SkipEmptyLines = true;
  private bool m_StoreSettingsByFile;
  private bool m_TreatLfAsSpace = false;
  private bool m_TreatNBSPAsSpace = false;
  private string m_TreatTextAsNull = "NULL";
  private bool m_TreatUnknownCharacterAsSpace;
  private bool m_TryToSolveMoreColumns = false;
  private bool m_WarnDelimiterInValue;
  private bool m_WarnEmptyTailingColumns = true;
  private bool m_WarnLineFeed = true;
  private bool m_WarnNbsp = true;
  private bool m_WarnQuotes = true;
  private bool m_WarnUnknownCharacter = true;
  public enum Duration
  {
    [Description("unlimited")]
    [ShortDescription("∞")]
    Unlimited,

    [Description("1/2 second")]
    [ShortDescription("½ s")]
    HalfSecond,

    [Description("1 second")]
    [ShortDescription("1 s")]
    Second,

    [Description("2 seconds")]
    [ShortDescription("2 s")]
    TwoSecond,

    [Description("5 seconds")]
    [ShortDescription("5 s")]
    FiveSecond,

    [Description("10 seconds")]
    [ShortDescription("10 s")]
    TenSecond
  }

  [DefaultValue(true)]
  public bool AllowJson
  {
    get => m_AllowJson;
    set => SetProperty(ref m_AllowJson, value);
  }

  [DefaultValue(false)]
  public bool AllowRowCombining
  {
    get => m_AllowRowCombining;
    set => SetProperty(ref m_AllowRowCombining, value);
  }

  [DefaultValue(false)]
  public bool AutoStartMode
  {
    get => m_AutoStartRemaining;
    set => SetProperty(ref m_AutoStartRemaining, value);
  }

  [JsonIgnore]
  public InspectionResult DefaultInspectionResult { get; } = new InspectionResult();

  [DefaultValue(true)]
  public bool DetectFileChanges
  {
    get => m_DetectFileChanges;
    set => SetProperty(ref m_DetectFileChanges, value);
  }

  [DefaultValue(false)]
  public bool DisplayRecordNo
  {
    get => m_DisplayRecordNo;
    set => SetProperty(ref m_DisplayRecordNo, value);
  }

  [DefaultValue(true)]
  public bool DisplayStartLineNo
  {
    get => m_DisplayStartLineNo;
    set => SetProperty(ref m_DisplayStartLineNo, value);
  }

  [JsonIgnore]
  public TimeSpan DurationTimeSpan
  {
    get
    {
      return LimitDuration switch
      {
        Duration.HalfSecond => TimeSpan.FromSeconds(.5),
        Duration.Second => TimeSpan.FromSeconds(1),
        Duration.TwoSecond => TimeSpan.FromSeconds(2),
        Duration.FiveSecond => TimeSpan.FromSeconds(5),
        Duration.TenSecond => TimeSpan.FromSeconds(10),
        _ => Timeout.InfiniteTimeSpan,
      };
    }
  }

  public FillGuessSettings FillGuessSettings
  {
    get => m_FillGuessSettings;
    set => SetProperty(ref m_FillGuessSettings, value);
  }

  // ReSharper disable once StringLiteralTypo
  [DefaultValue("Tahoma")]
  public string Font
  {
    get => m_Font;
    set => SetProperty(ref m_Font, value);
  }

  [DefaultValue(8.25f)]
  public float FontSize
  {
    get => m_FontSize;
    set => SetProperty(ref m_FontSize, value);
  }

  [DefaultValue(true)]
  public bool GuessCodePage
  {
    get => m_GuessCodePage;
    set => SetProperty(ref m_GuessCodePage, value);
  }
  [DefaultValue(true)]
  public bool GuessComment
  {
    get => m_GuessComment;
    set => SetProperty(ref m_GuessComment, value);
  }

  [DefaultValue(true)]
  public bool GuessDelimiter
  {
    get => m_GuessDelimiter;
    set => SetProperty(ref m_GuessDelimiter, value);
  }

  [DefaultValue(true)]
  public bool GuessEscapePrefix
  {
    get => m_GuessEscapePrefix;
    set => SetProperty(ref m_GuessEscapePrefix, value);
  }

  [DefaultValue(true)]
  public bool GuessHasHeader
  {
    get => m_GuessHasHeader;
    set => SetProperty(ref m_GuessHasHeader, value);
  }

  [DefaultValue(true)]
  public bool GuessNewLine
  {
    get => m_GuessNewLine;
    set => SetProperty(ref m_GuessNewLine, value);
  }

  [DefaultValue(true)]
  public bool GuessQualifier
  {
    get => m_GuessQualifier;
    set => SetProperty(ref m_GuessQualifier, value);
  }

  [DefaultValue(true)]
  public bool GuessStartRow
  {
    get => m_GuessStartRow;
    set => SetProperty(ref m_GuessStartRow, value);
  }

  public HtmlStyle HtmlStyle
  {
    get => m_HtmlStyle;
    set => SetProperty(ref m_HtmlStyle, value);
  }

  [JsonIgnore]
  public string InitialFolder
  {
    get;
    set;
  } = ".";

  [DefaultValue(Duration.FiveSecond)]
  public Duration LimitDuration
  {
    get => m_LimitDuration;
    set => SetProperty(ref m_LimitDuration, value);
  }

  [DefaultValue(false)]
  public bool MenuDown
  {
    get => m_MenuDown;
    set => SetProperty(ref m_MenuDown, value);
  }

  [DefaultValue(0)]
  public int NumWarnings
  {
    get => m_NumWarnings;
    set => SetProperty(ref m_NumWarnings, value);
  }

  [DefaultValue(500)]
  public int ShowButtonAtLength
  {
    get => m_ShowButtonAtLength;
    set => SetProperty(ref m_ShowButtonAtLength, value);
  }

  [DefaultValue(true)] 
  public bool SkipEmptyLines 
  { 
    get => m_SkipEmptyLines; 
    set => SetProperty(ref m_SkipEmptyLines, value); }

  [DefaultValue(false)]
  public bool StoreSettingsByFile
  {
    get => m_StoreSettingsByFile;
    set => SetProperty(ref m_StoreSettingsByFile, value);
  }

  [DefaultValue(false)]
  public bool TreatLfAsSpace
  {
    get => m_TreatLfAsSpace;
    set => SetProperty(ref m_TreatLfAsSpace, value);
  }

  [DefaultValue(false)]
  public bool TreatNBSPAsSpace
  {
    get => m_TreatNBSPAsSpace;
    set => SetProperty(ref m_TreatNBSPAsSpace, value);
  }

  [DefaultValue("NULL")]
  public string TreatTextAsNull 
  { 
    get => m_TreatTextAsNull;
    set => SetProperty(ref m_TreatTextAsNull, value);
  }
  [DefaultValue(false)]
  public bool TreatUnknownCharacterAsSpace
  {
    get => m_TreatUnknownCharacterAsSpace;
    set => SetProperty(ref m_TreatUnknownCharacterAsSpace, value);
  }

  [DefaultValue(false)]
  public bool TryToSolveMoreColumns
  {
    get => m_TryToSolveMoreColumns;
    set => SetProperty(ref m_TryToSolveMoreColumns, value);
  }

#if SupportPGP
    [DefaultValue("")]
    public string KeyFileRead
    {
      get => m_KeyFileRead;
      set => SetProperty(ref m_KeyFileRead, value);
    }

    [DefaultValue("")]
    public string KeyFileWrite
    {
      get => m_KeyFileWrite;
      set => SetProperty(ref m_KeyFileWrite, value);
    }
#endif

  [DefaultValue(false)]
  public bool WarnDelimiterInValue
  {
    get => m_WarnDelimiterInValue;
    set => SetProperty(ref m_WarnDelimiterInValue, value);
  }

  [DefaultValue(true)]
  public bool WarnEmptyTailingColumns
  {
    get => m_WarnEmptyTailingColumns;
    set => SetProperty(ref m_WarnEmptyTailingColumns, value);
  }

  [DefaultValue(true)]
  public bool WarnLineFeed
  {
    get => m_WarnLineFeed;
    set => SetProperty(ref m_WarnLineFeed, value);
  }

  [DefaultValue(true)]
  public bool WarnNBSP
  {
    get => m_WarnNbsp;
    set => SetProperty(ref m_WarnNbsp, value);
  }

  [DefaultValue(true)]
  public bool WarnQuotes
  {
    get => m_WarnQuotes;
    set => SetProperty(ref m_WarnQuotes, value);
  }

  [DefaultValue(true)]
  public bool WarnUnknownCharacter
  {
    get => m_WarnUnknownCharacter;
    set => SetProperty(ref m_WarnUnknownCharacter, value);
  }

  [JsonIgnore]
  public CsvFileDummy WriteSetting { get; internal set;  } = new CsvFileDummy();

  /// <summary>
  ///   Gets or sets the fill guess settings.
  /// </summary>
  /// <value>The fill guess settings.</value>
#if NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
  public void DeriveWriteSetting(CsvFileDummy fileSetting)
  {
    WriteSetting.CodePageId = fileSetting.CodePageId;
    WriteSetting.ByteOrderMark = fileSetting.ByteOrderMark;
    WriteSetting.ColumnCollection.Overwrite(fileSetting.ColumnCollection);

    // Qualifier
    if (WriteSetting.FieldQualifierChar == 0)
      WriteSetting.FieldQualifierChar = '"';

    // Delimiter
    if (WriteSetting.FieldDelimiterChar == 0)
      WriteSetting.FieldDelimiterChar = ',';

    // Escape
    if (WriteSetting.EscapePrefixChar == 0)
      WriteSetting.EscapePrefixChar = fileSetting.EscapePrefixChar;

    // Fix No DuplicateQualifier
    if (!WriteSetting.DuplicateQualifierToEscape && WriteSetting.FieldQualifierChar == '"' &&
        WriteSetting.EscapePrefixChar == char.MinValue)
      WriteSetting.DuplicateQualifierToEscape = true;

    // NewLine depending on Environment
    if (Environment.NewLine == "\r\n")
      WriteSetting.NewLine = RecordDelimiterTypeEnum.Crlf;
    else if (Environment.NewLine == "\n")
      WriteSetting.NewLine = RecordDelimiterTypeEnum.Lf;
    else if (Environment.NewLine == "\r")
      WriteSetting.NewLine = RecordDelimiterTypeEnum.Cr;
  }

  public void PassOnConfiguration(CsvFileDummy fileSetting)
  {
    fileSetting.AllowRowCombining = AllowRowCombining;
    fileSetting.DisplayRecordNo = DisplayRecordNo;
    fileSetting.DisplayStartLineNo = DisplayStartLineNo;
    fileSetting.NumWarnings = NumWarnings;
    fileSetting.SkipEmptyLines = SkipEmptyLines;
    fileSetting.TreatLfAsSpace = TreatLfAsSpace;
    fileSetting.TreatNBSPAsSpace = TreatNBSPAsSpace;
    fileSetting.TreatTextAsNull = TreatTextAsNull;
    fileSetting.TreatUnknownCharacterAsSpace = TreatUnknownCharacterAsSpace;
    fileSetting.TryToSolveMoreColumns = TryToSolveMoreColumns;
    fileSetting.WarnDelimiterInValue = WarnDelimiterInValue;
    fileSetting.WarnEmptyTailingColumns = WarnEmptyTailingColumns;
    fileSetting.WarnLineFeed = WarnLineFeed;
    fileSetting.WarnNBSP = WarnNBSP;
    fileSetting.WarnQuotes = WarnQuotes;
    fileSetting.WarnQuotesInQuotes = WarnQuotes;
    fileSetting.WarnUnknownCharacter = WarnUnknownCharacter;
  }
}