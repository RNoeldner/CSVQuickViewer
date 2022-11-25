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
using System.Windows.Forms;

namespace CsvTools
{
  /// <inheritdoc />
  /// <summary>
  ///   Class containing the all configuration, used in serialization to store the settings
  /// </summary>
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
    private string m_Font = "Tahoma";
    private float m_FontSize = 8f;
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

    [DefaultValue(".")]
    public string InitialFolder
    {
      get;
      set;
    } = ".";

    [DefaultValue("Tahoma")]
    public string Font
    {
      get => m_Font;
      set => SetField(ref m_Font, value);
    }

    [DefaultValue(8f)]
    public float FontSize
    {
      get => m_FontSize;
      set => SetField(ref m_FontSize, value);
    }

    [DefaultValue(false)]
    public bool DisplayRecordNo
    {
      get => m_DisplayRecordNo;
      set => SetField(ref m_DisplayRecordNo, value);
    }

    [DefaultValue(true)]
    public bool DisplayStartLineNo
    {
      get => m_DisplayStartLineNo;
      set => SetField(ref m_DisplayStartLineNo, value);
    }

    public WindowState WindowPosition
    {
      get;
      set;
    } = new WindowState(10, 10, 600, 600, FormWindowState.Normal, 0, "");


    [DefaultValue(true)]
    public bool AllowJson
    {
      get => m_AllowJson;
      set => SetField(ref m_AllowJson, value);
    }

    [DefaultValue(true)]
    public bool DetectFileChanges
    {
      get => m_DetectFileChanges;
      set => SetField(ref m_DetectFileChanges, value);
    }

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
#if NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public FillGuessSettings FillGuessSettings
    {
      get => m_FillGuessSettings;
      set => SetField(ref m_FillGuessSettings, value);
    }


    [DefaultValue(true)]
    public bool GuessCodePage
    {
      get => m_GuessCodePage;
      set => SetField(ref m_GuessCodePage, value);
    }

    [DefaultValue(true)]
    public bool GuessDelimiter
    {
      get => m_GuessDelimiter;
      set => SetField(ref m_GuessDelimiter, value);
    }

    [DefaultValue(true)]
    public bool GuessHasHeader
    {
      get => m_GuessHasHeader;
      set => SetField(ref m_GuessHasHeader, value);
    }

    [DefaultValue(true)]
    public bool GuessNewLine
    {
      get => m_GuessNewLine;
      set => SetField(ref m_GuessNewLine, value);
    }

    [DefaultValue(true)]
    public bool GuessComment
    {
      get => m_GuessComment;
      set => SetField(ref m_GuessComment, value);
    }

    [DefaultValue(false)]
    public bool GuessQualifier
    {
      get => m_GuessQualifier;
      set => SetField(ref m_GuessQualifier, value);
    }


    [DefaultValue(true)]
    public bool GuessStartRow
    {
      get => m_GuessStartRow;
      set => SetField(ref m_GuessStartRow, value);
    }

    [DefaultValue(Duration.Second)]
    public Duration LimitDuration
    {
      get;
      set;
    } = Duration.Second;


    [DefaultValue(false)]
    public bool MenuDown
    {
      get => m_MenuDown;
      set => SetField(ref m_MenuDown, value);
    }


    [DefaultValue(false)]
    public bool StoreSettingsByFile
    {
      get => m_StoreSettingsByFile;
      set => SetField(ref m_StoreSettingsByFile, value);
    }

    public HtmlStyle HtmlStyle
    {
      get => m_HtmlStyle;
      set => SetField(ref m_HtmlStyle, value);
    }
  }
}