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

#nullable enable

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
#if XmlSerialization
using System.Xml.Serialization;
#endif

// ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract

namespace CsvTools
{
  /// <inheritdoc cref="CsvTools.ICsvFile" />
  /// <summary>
  ///   Setting file for CSV files, its an implementation of <see cref="T:CsvTools.BaseSettings" />
  /// </summary>
  [Serializable]
  public sealed class CsvFile : BaseSettingPhysicalFile, ICsvFile
  {
    private const bool cContextSensitiveQualifierDefault = false;
    private const bool cQualifyAlwaysDefault = false;
    private const RecordDelimiterTypeEnum cNewLineDefault = RecordDelimiterTypeEnum.Crlf;
    private const string cCommentLineDefault = "";
    private const string cDelimiterPlaceholderDefault = "";
    private const string cNewLinePlaceholderDefault = "";
    private const string cQuotePlaceholderDefault = "";
    private const char cFieldDelimiterDefault = ',';
    private const char cFieldQualifierDefault = '"';
    private const char cEscapePrefixDefault = '\0';
    private const bool cQualifyOnlyIfNeededDefault = true;
    private const bool cDuplicateQualifierToEscapeDefault = true;

    /// <summary>
    ///   File ending for a setting file
    /// </summary>
    public const string cCsvSettingExtension = ".setting";

    private bool m_AllowRowCombining;
    private string m_CommentLine = cCommentLineDefault;
    private bool m_ContextSensitiveQualifier = cContextSensitiveQualifierDefault;
    private Encoding m_CurrentEncoding = Encoding.UTF8;
    private string m_DelimiterPlaceholder = cDelimiterPlaceholderDefault;
    private bool m_DuplicateQualifierToEscape = cDuplicateQualifierToEscapeDefault;
    private char m_EscapePrefixPunc = cEscapePrefixDefault;
    private char m_FieldDelimiterPunc = cFieldDelimiterDefault;
    private char m_FieldQualifierPunc = cFieldQualifierDefault;
    private RecordDelimiterTypeEnum m_NewLine = cNewLineDefault;
    private string m_NewLinePlaceholder = cNewLinePlaceholderDefault;
    private bool m_NoDelimitedFile;
    private int m_NumWarnings;
    private string m_QualifierPlaceholder = cQuotePlaceholderDefault;
    private bool m_QualifyAlways = cQualifyAlwaysDefault;
    private bool m_QualifyOnlyIfNeeded = cQualifyOnlyIfNeededDefault;
    private bool m_TreatLfAsSpace;
    private bool m_TreatUnknownCharacterAsSpace;
    private bool m_TryToSolveMoreColumns;
    private bool m_WarnDelimiterInValue;
    private bool m_WarnEmptyTailingColumns = true;
    private bool m_WarnLineFeed;
    private bool m_WarnNbsp = true;
    private bool m_WarnQuotes;
    private bool m_WarnQuotesInQuotes = true;
    private bool m_WarnUnknownCharacter = true;

    /// <summary>
    ///   Initializes a new instance of the <see cref="CsvFile" /> class.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="fileName">Name of the file.</param>
    [JsonConstructor]
    public CsvFile(in string? id = "", in string? fileName = "")
      : base(id ?? string.Empty, fileName ?? string.Empty)
    {
    }

#if XmlSerialization
    /// <inheritdoc />
    /// <summary>
    ///   Initializes a new instance of the <see cref="T:CsvTools.CsvFile" /> class.
    /// </summary>
    [Obsolete("Only needed for XML Serialization")]
    public CsvFile()
      : this(id: string.Empty, fileName: string.Empty)
    {
    }
#endif

    /// <summary>
    ///   Gets current encoding.
    /// </summary>
    /// <value>The current encoding.</value>
#if XmlSerialization
    [XmlIgnore]
#endif
    [JsonIgnore]
    public Encoding CurrentEncoding
    {
      get => m_CurrentEncoding;
      set => m_CurrentEncoding = value;
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(cContextSensitiveQualifierDefault)]
    public bool ContextSensitiveQualifier
    {
      get => m_ContextSensitiveQualifier;
      set => SetProperty(ref m_ContextSensitiveQualifier, value);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(cCommentLineDefault)]
    public string CommentLine
    {
      get => m_CommentLine;
      set => SetProperty(ref m_CommentLine, value);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(cDelimiterPlaceholderDefault)]
    public string DelimiterPlaceholder
    {
      get => m_DelimiterPlaceholder;
      set => SetProperty(ref m_DelimiterPlaceholder, value);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(cDuplicateQualifierToEscapeDefault)]
    public bool DuplicateQualifierToEscape
    {
      get => m_DuplicateQualifierToEscape;
      set => SetProperty(ref m_DuplicateQualifierToEscape, value);
    }

   
    [JsonIgnore]    
    [Obsolete("Use EscapePrefixChar")]
    public string EscapePrefix
    {
      get => m_EscapePrefixPunc.Text();
      set => SetProperty(ref m_EscapePrefixPunc, value.FromText());
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(cEscapePrefixDefault)]
    public char EscapePrefixChar
    {
      get => m_EscapePrefixPunc;
      set => SetProperty(ref m_EscapePrefixPunc, value);
    }
    
    [JsonIgnore]    
    [Obsolete("Use FieldDelimiterChar")]
    public string FieldDelimiter
    {
      get => m_FieldDelimiterPunc.Text();
      set => SetProperty(ref m_FieldDelimiterPunc, value.FromText());
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(cFieldDelimiterDefault)]
    public char FieldDelimiterChar
    {
      get => m_FieldDelimiterPunc;
      set => SetProperty(ref m_FieldDelimiterPunc, value);
    }
    
    [JsonIgnore]    
    [Obsolete("Use FieldQualifierChar")]
    public string FieldQualifier
    {
      get => m_FieldQualifierPunc.Text();
      set => SetProperty(ref m_FieldQualifierPunc, value.FromText());
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(cFieldQualifierDefault)]
    public char FieldQualifierChar
    {
      get => m_FieldQualifierPunc;
      set => SetProperty(ref m_FieldQualifierPunc, value);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(cNewLineDefault)]
    public RecordDelimiterTypeEnum NewLine
    {
      get => m_NewLine;
      set => SetProperty(ref m_NewLine, value);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(cNewLinePlaceholderDefault)]
    public string NewLinePlaceholder
    {
      get => m_NewLinePlaceholder;
      set => SetProperty(ref m_NewLinePlaceholder, value ?? cNewLinePlaceholderDefault);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(cQualifyAlwaysDefault)]
    public bool QualifyAlways
    {
      get => m_QualifyAlways;
      set
      {
        if (SetProperty(ref m_QualifyAlways, value))
        {
          m_QualifyAlways = value;
          if (m_QualifyAlways)
            QualifyOnlyIfNeeded = false;
        }
      }
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(cQualifyOnlyIfNeededDefault)]
    public bool QualifyOnlyIfNeeded
    {
      get => m_QualifyOnlyIfNeeded;

      set
      {
        if (SetProperty(ref m_QualifyOnlyIfNeeded, value))
        {
          m_QualifyOnlyIfNeeded = value;
          if (m_QualifyOnlyIfNeeded)
            QualifyAlways = false;
        }
      }
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(cQuotePlaceholderDefault)]
    public string QualifierPlaceholder
    {
      get => m_QualifierPlaceholder;
      set => SetProperty(ref m_QualifierPlaceholder, value);
    }

#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(false)]
    public bool AllowRowCombining
    {
      get => m_AllowRowCombining;
      set => SetProperty(ref m_AllowRowCombining, value);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlIgnore]
#endif
    [JsonIgnore]
    public bool NoDelimitedFile
    {
      get => m_NoDelimitedFile;
      set => SetProperty(ref m_NoDelimitedFile, value);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlElement]
#endif
    [DefaultValue(0)]
    public int NumWarnings
    {
      get => m_NumWarnings;
      set => SetProperty(ref m_NumWarnings, value);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(false)]
    public bool TreatLfAsSpace
    {
      get => m_TreatLfAsSpace;
      set => SetProperty(ref m_TreatLfAsSpace, value);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(false)]
    public bool TreatUnknownCharacterAsSpace
    {
      get => m_TreatUnknownCharacterAsSpace;
      set => SetProperty(ref m_TreatUnknownCharacterAsSpace, value);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(false)]
    public bool TryToSolveMoreColumns
    {
      get => m_TryToSolveMoreColumns;
      set => SetProperty(ref m_TryToSolveMoreColumns, value);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(false)]
    public bool WarnDelimiterInValue
    {
      get => m_WarnDelimiterInValue;
      set => SetProperty(ref m_WarnDelimiterInValue, value);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute(AttributeName = "WarnEmptyTailingColumns")]
#endif
    [DefaultValue(true)]
    public bool WarnEmptyTailingColumns
    {
      get => m_WarnEmptyTailingColumns;
      set => SetProperty(ref m_WarnEmptyTailingColumns, value);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(false)]
    public bool WarnLineFeed
    {
      get => m_WarnLineFeed;
      set => SetProperty(ref m_WarnLineFeed, value);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(true)]
    public bool WarnNBSP
    {
      get => m_WarnNbsp;
      set => SetProperty(ref m_WarnNbsp, value);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(false)]
    public bool WarnQuotes
    {
      get => m_WarnQuotes;
      set => SetProperty(ref m_WarnQuotes, value);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(true)]
    public bool WarnQuotesInQuotes
    {
      get => m_WarnQuotesInQuotes;
      set => SetProperty(ref m_WarnQuotesInQuotes, value);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(true)]
    public bool WarnUnknownCharacter
    {
      get => m_WarnUnknownCharacter;
      set => SetProperty(ref m_WarnUnknownCharacter, value);
    }

    /// <inheritdoc />
    public override object Clone()
    {
      var other = new CsvFile(id: ID, fileName: FileName);
      CopyTo(other);
      return other;
    }

    /// <inheritdoc />
    public override void CopyTo(IFileSetting other)
    {
      BaseSettingsCopyTo((BaseSettings) other);

      if (!(other is CsvFile csv))
        return;

      csv.WarnQuotes = m_WarnQuotes;
      csv.WarnDelimiterInValue = m_WarnDelimiterInValue;
      csv.WarnEmptyTailingColumns = m_WarnEmptyTailingColumns;
      csv.WarnQuotesInQuotes = m_WarnQuotesInQuotes;
      csv.WarnUnknownCharacter = m_WarnUnknownCharacter;
      csv.WarnLineFeed = m_WarnLineFeed;
      csv.WarnNBSP = m_WarnNbsp;
      csv.TreatLfAsSpace = m_TreatLfAsSpace;
      csv.TryToSolveMoreColumns = m_TryToSolveMoreColumns;
      csv.AllowRowCombining = m_AllowRowCombining;

      csv.TreatUnknownCharacterAsSpace = m_TreatUnknownCharacterAsSpace;

      csv.NumWarnings = m_NumWarnings;
      csv.NoDelimitedFile = m_NoDelimitedFile;

      csv.CommentLine = CommentLine;
      csv.ContextSensitiveQualifier = ContextSensitiveQualifier;
      csv.DuplicateQualifierToEscape = DuplicateQualifierToEscape;
      csv.DelimiterPlaceholder = DelimiterPlaceholder;
      csv.EscapePrefixChar = EscapePrefixChar;
      csv.FieldDelimiterChar = FieldDelimiterChar;
      csv.FieldQualifierChar = FieldQualifierChar;
      csv.NewLine = NewLine;
      csv.NewLinePlaceholder = NewLinePlaceholder;
      csv.QualifyOnlyIfNeeded = QualifyOnlyIfNeeded;
      csv.QualifyAlways = QualifyAlways;
      csv.QualifierPlaceholder = QualifierPlaceholder;
    }

    /// <inheritdoc />
    public override bool Equals(IFileSetting? other) => Equals(other as ICsvFile);

    /// <inheritdoc />
    public bool Equals(ICsvFile? other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return m_NoDelimitedFile == other.NoDelimitedFile
             && m_NumWarnings == other.NumWarnings
             && m_TreatUnknownCharacterAsSpace == other.TreatUnknownCharacterAsSpace
             && m_WarnDelimiterInValue == other.WarnDelimiterInValue
             && m_WarnEmptyTailingColumns == other.WarnEmptyTailingColumns
             && m_WarnLineFeed == other.WarnLineFeed
             && m_TryToSolveMoreColumns == other.TryToSolveMoreColumns
             && m_AllowRowCombining == other.AllowRowCombining
             && m_TreatLfAsSpace == other.TreatLfAsSpace
             && m_WarnNbsp == other.WarnNBSP && m_WarnQuotes == other.WarnQuotes
             && m_WarnQuotesInQuotes == other.WarnQuotesInQuotes
             && m_WarnUnknownCharacter == other.WarnUnknownCharacter
             && BaseSettingsEquals(other as BaseSettings)
             && ContextSensitiveQualifier == other.ContextSensitiveQualifier
             && DuplicateQualifierToEscape == other.DuplicateQualifierToEscape
             && string.Equals(CommentLine, other.CommentLine, StringComparison.Ordinal)
             && string.Equals(DelimiterPlaceholder, other.DelimiterPlaceholder, StringComparison.Ordinal)
             && EscapePrefixChar == other.EscapePrefixChar
             && FieldDelimiterChar == other.FieldDelimiterChar
             && FieldQualifierChar == other.FieldQualifierChar
             && NewLine.Equals(other.NewLine)
             && string.Equals(NewLinePlaceholder, other.NewLinePlaceholder, StringComparison.Ordinal)
             && QualifyAlways == other.QualifyAlways
             && QualifyOnlyIfNeeded == other.QualifyOnlyIfNeeded
             && string.Equals(QualifierPlaceholder, other.QualifierPlaceholder, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override IEnumerable<string> GetDifferences(IFileSetting other)
    {
      if (other is ICsvFile csv)
      {
        if (m_NoDelimitedFile != csv.NoDelimitedFile)
          yield return $"{nameof(NoDelimitedFile)}: {NoDelimitedFile} {csv.NoDelimitedFile}";

        if (m_NumWarnings != csv.NumWarnings)
          yield return $"{nameof(NumWarnings)}: {NumWarnings} {csv.NumWarnings}";

        if (m_TreatUnknownCharacterAsSpace != csv.TreatUnknownCharacterAsSpace)
          yield return
            $"TreatUnknownCharacterAsSpace: {TreatUnknownCharacterAsSpace} {csv.TreatUnknownCharacterAsSpace}";

        if (m_WarnDelimiterInValue != csv.WarnDelimiterInValue)
          yield return $"{nameof(WarnDelimiterInValue)}: {WarnDelimiterInValue} {csv.WarnDelimiterInValue}";

        if (m_WarnEmptyTailingColumns != csv.WarnEmptyTailingColumns)
          yield return $"{nameof(WarnEmptyTailingColumns)}: {WarnEmptyTailingColumns} {csv.WarnEmptyTailingColumns}";

        if (m_WarnLineFeed != csv.WarnLineFeed)
          yield return $"{nameof(WarnLineFeed)}: {WarnLineFeed} {csv.WarnLineFeed}";

        if (m_TryToSolveMoreColumns != csv.TryToSolveMoreColumns)
          yield return $"{nameof(TryToSolveMoreColumns)}: {TryToSolveMoreColumns} {csv.TryToSolveMoreColumns}";

        if (m_AllowRowCombining != csv.AllowRowCombining)
          yield return $"{nameof(AllowRowCombining)}: {AllowRowCombining} {csv.AllowRowCombining}";

        if (m_TreatLfAsSpace != csv.TreatLfAsSpace)
          yield return $"{nameof(TreatLfAsSpace)}: {TreatLfAsSpace} {csv.TreatLfAsSpace}";

        if (m_WarnNbsp != csv.WarnNBSP)
          yield return $"{nameof(WarnNBSP)}: {WarnNBSP} {csv.WarnNBSP}";

        if (m_WarnQuotesInQuotes != csv.WarnQuotesInQuotes)
          yield return $"{nameof(WarnQuotesInQuotes)}: {WarnQuotesInQuotes} {csv.WarnQuotesInQuotes}";

        if (m_WarnUnknownCharacter != csv.WarnUnknownCharacter)
          yield return $"{nameof(WarnUnknownCharacter)}: {WarnUnknownCharacter} {csv.WarnUnknownCharacter}";

        if (m_WarnQuotes != csv.WarnQuotes)
          yield return $"{nameof(WarnQuotes)}: {WarnQuotes} {csv.WarnQuotes}";

        if (ContextSensitiveQualifier != csv.ContextSensitiveQualifier)
          yield return
            $"{nameof(ContextSensitiveQualifier)}: {ContextSensitiveQualifier} {csv.ContextSensitiveQualifier}";

        if (DuplicateQualifierToEscape != csv.DuplicateQualifierToEscape)
          yield return
            $"{nameof(DuplicateQualifierToEscape)}: {DuplicateQualifierToEscape} {csv.DuplicateQualifierToEscape}";

        if (!string.Equals(CommentLine, csv.CommentLine, StringComparison.Ordinal))
          yield return $"{nameof(CommentLine)}: {CommentLine} {csv.CommentLine}";

        if (!string.Equals(DelimiterPlaceholder, csv.DelimiterPlaceholder, StringComparison.Ordinal))
          yield return $"{nameof(DelimiterPlaceholder)}: {DelimiterPlaceholder} {csv.DelimiterPlaceholder}";

        if (EscapePrefixChar != csv.EscapePrefixChar)
          yield return $"{nameof(EscapePrefixChar)}: {EscapePrefixChar} {csv.EscapePrefixChar}";

        if (FieldDelimiterChar != csv.FieldDelimiterChar)
          yield return $"{nameof(FieldDelimiterChar)}: {FieldDelimiterChar} {csv.FieldDelimiterChar}";

        if (FieldQualifierChar != csv.FieldQualifierChar)
          yield return $"{nameof(FieldQualifierChar)}: {FieldQualifierChar} {csv.FieldQualifierChar}";

        if (!NewLine.Equals(csv.NewLine))
          yield return $"{nameof(NewLine)}: {NewLine} {csv.NewLine}";

        if (!string.Equals(NewLinePlaceholder, csv.NewLinePlaceholder, StringComparison.Ordinal))
          yield return $"{nameof(NewLinePlaceholder)}: {NewLinePlaceholder} {csv.NewLinePlaceholder}";

        if (QualifyAlways != csv.QualifyAlways)
          yield return $"{nameof(QualifyAlways)}: {QualifyAlways} {csv.QualifyAlways}";

        if (QualifyOnlyIfNeeded != csv.QualifyOnlyIfNeeded)
          yield return $"{nameof(QualifyOnlyIfNeeded)}: {QualifyOnlyIfNeeded} {csv.QualifyOnlyIfNeeded}";

        if (!string.Equals(QualifierPlaceholder, csv.QualifierPlaceholder, StringComparison.Ordinal))
          yield return $"{nameof(QualifierPlaceholder)}: {QualifierPlaceholder} {csv.QualifierPlaceholder}";
      }

      foreach (var res in base.GetDifferences(other))
        yield return res;
    }
  }
}