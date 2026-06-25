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
#if SupportPGP
    private string m_KeyFileRead = string.Empty;
    private string m_KeyFileWrite = string.Empty;
#endif
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
    get;
    set => SetProperty(ref field, value);
  } = true;

  [DefaultValue(false)]
  public bool AllowRowCombining
  {
    get;
    set => SetProperty(ref field, value);
  } = false;

  [DefaultValue(false)]
  public bool AutoStartMode
  {
    get;
    set => SetProperty(ref field, value);
  } = false;

  [JsonIgnore]
  public InspectionResult DefaultInspectionResult { get; } = new InspectionResult();

  [DefaultValue(true)]
  public bool DetectFileChanges
  {
    get;
    set => SetProperty(ref field, value);
  } = true;

  [DefaultValue(false)]
  public bool DisplayRecordNo
  {
    get;
    set => SetProperty(ref field, value);
  }

  [DefaultValue(true)]
  public bool DisplayStartLineNo
  {
    get;
    set => SetProperty(ref field, value);
  } = true;

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
    get;
    set => SetProperty(ref field, value);
  } = FillGuessSettings.Default;

  // ReSharper disable once StringLiteralTypo
  [DefaultValue("Tahoma")]
  public string Font
  {
    get;
    set => SetProperty(ref field, value);
  } = "Tahoma";

  [DefaultValue(8.25f)]
  public float FontSize
  {
    get;
    set => SetProperty(ref field, value);
  } = 8.25f;

  [DefaultValue(true)]
  public bool GuessCodePage
  {
    get;
    set => SetProperty(ref field, value);
  } = true;

  [DefaultValue(true)]
  public bool GuessComment
  {
    get;
    set => SetProperty(ref field, value);
  } = true;

  [DefaultValue(true)]
  public bool GuessDelimiter
  {
    get;
    set => SetProperty(ref field, value);
  } = true;

  [DefaultValue(true)]
  public bool GuessEscapePrefix
  {
    get;
    set => SetProperty(ref field, value);
  } = true;

  [DefaultValue(true)]
  public bool GuessHasHeader
  {
    get;
    set => SetProperty(ref field, value);
  } = true;

  [DefaultValue(true)]
  public bool GuessNewLine
  {
    get;
    set => SetProperty(ref field, value);
  } = true;

  [DefaultValue(true)]
  public bool GuessQualifier
  {
    get;
    set => SetProperty(ref field, value);
  } = true;

  [DefaultValue(true)]
  public bool GuessStartRow
  {
    get;
    set => SetProperty(ref field, value);
  } = true;

  public HtmlStyle HtmlStyle
  {
    get;
    set => SetProperty(ref field, value);
  } = HtmlStyle.Default;

  [JsonIgnore]
  public string InitialFolder
  {
    get;
    set;
  } = ".";

  [DefaultValue(Duration.FiveSecond)]
  public Duration LimitDuration
  {
    get;
    set => SetProperty(ref field, value);
  } = Duration.FiveSecond;

  [DefaultValue(false)]
  public bool MenuDown
  {
    get;
    set => SetProperty(ref field, value);
  }

  [DefaultValue(0)]
  public int NumWarnings
  {
    get;
    set => SetProperty(ref field, value);
  } = 0;

  [DefaultValue(500)]
  public int ShowButtonAtLength
  {
    get;
    set => SetProperty(ref field, value);
  } = 2000;

  [DefaultValue(true)]
  public bool SkipEmptyLines
  {
    get;
    set => SetProperty(ref field, value);
  } = true;

  [DefaultValue(false)]
  public bool StoreSettingsByFile
  {
    get;
    set => SetProperty(ref field, value);
  }

  [DefaultValue(false)]
  public bool TreatLfAsSpace
  {
    get;
    set => SetProperty(ref field, value);
  } = false;

  [DefaultValue(false)]
  public bool TreatNBSPAsSpace
  {
    get;
    set => SetProperty(ref field, value);
  } = false;

  [DefaultValue("NULL")]
  public string TreatTextAsNull
  {
    get;
    set => SetProperty(ref field, value);
  } = "NULL";

  [DefaultValue(false)]
  public bool TreatUnknownCharacterAsSpace
  {
    get;
    set => SetProperty(ref field, value);
  }

  [DefaultValue(false)]
  public bool TryToSolveMoreColumns
  {
    get;
    set => SetProperty(ref field, value);
  } = false;

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
    get;
    set => SetProperty(ref field, value);
  }

  [DefaultValue(true)]
  public bool WarnEmptyTailingColumns
  {
    get;
    set => SetProperty(ref field, value);
  } = true;

  [DefaultValue(true)]
  public bool WarnLineFeed
  {
    get;
    set => SetProperty(ref field, value);
  } = true;

  [DefaultValue(true)]
  public bool WarnNBSP
  {
    get;
    set => SetProperty(ref field, value);
  } = true;

  [DefaultValue(true)]
  public bool WarnQuotes
  {
    get;
    set => SetProperty(ref field, value);
  } = true;

  [DefaultValue(true)]
  public bool WarnUnknownCharacter
  {
    get;
    set => SetProperty(ref field, value);
  } = true;

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