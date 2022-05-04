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
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace CsvTools
{
  /// <inheritdoc cref="CsvTools.ICsvFile" />
  /// <summary>
  ///   Setting file for CSV files, its an implementation of <see cref="T:CsvTools.BaseSettings" />
  /// </summary>
  [Serializable]
  public class CsvFile : BaseSettingPhysicalFile, ICsvFile
  {
    private const bool cContextSensitiveQualifierDefault = false;
    private const bool cQualifyAlwaysDefault = false;
    private const string cEscapePrefixDefault = "";
    private const RecordDelimiterTypeEnum cNewLineDefault = RecordDelimiterTypeEnum.Crlf;
    private const string cCommentLineDefault = "";
    private const string cDelimiterPlaceholderDefault = "";
    private const string cFieldDelimiterDefault = ",";
    private const string cFieldQualifierDefault = "\"";
    private const string cNewLinePlaceholderDefault = "";
    private const bool cQualifyOnlyIfNeededDefault = true;
    private const string cQuotePlaceholderDefault = "";
    private const bool cDuplicateQualifierToEscapeDefault = true;

    /// <summary>
    ///   File ending for a setting file
    /// </summary>
    public const string cCsvSettingExtension = ".setting";

    private bool m_AllowRowCombining;
    private string m_CommentLine = cCommentLineDefault;

    private bool m_ContextSensitiveQualifier = cContextSensitiveQualifierDefault;

    [NonSerialized] private Encoding m_CurrentEncoding = Encoding.UTF8;
    private string m_DelimiterPlaceholder = cDelimiterPlaceholderDefault;
    private bool m_DuplicateQualifierToEscape = cDuplicateQualifierToEscapeDefault;
    private string m_EscapePrefix = cEscapePrefixDefault;
    private char m_EscapePrefixChar = '\0';
    private string m_FieldDelimiter = cFieldDelimiterDefault;
    private char m_FieldDelimiterChar = cFieldDelimiterDefault[0];
    private string m_FieldQualifier = cFieldQualifierDefault;
    private char m_FieldQualifierChar = cFieldQualifierDefault[0];
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
    /// <param name="fileName">Name of the file.</param>
    public CsvFile(string fileName)
      : base(fileName)
    {
    }

    /// <inheritdoc />
    /// <summary>
    ///   Initializes a new instance of the <see cref="T:CsvTools.CsvFile" /> class.
    /// </summary>
    public CsvFile()
      : this(string.Empty)
    {
    }

    /// <summary>
    ///   Gets current encoding.
    /// </summary>
    /// <value>The current encoding.</value>
    [XmlIgnore]
    public virtual Encoding CurrentEncoding
    {
      get => m_CurrentEncoding;
      set => m_CurrentEncoding = value;
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(cContextSensitiveQualifierDefault)]
    public virtual bool ContextSensitiveQualifier
    {
      get => m_ContextSensitiveQualifier;
      set
      {
        if (m_ContextSensitiveQualifier.Equals(value))
          return;
        m_ContextSensitiveQualifier = value;
        NotifyPropertyChanged(nameof(ContextSensitiveQualifier));

        // If Alternate Qualifier is disabled, enable DuplicateQualifierToEscape automatically
        if (!m_ContextSensitiveQualifier && !DuplicateQualifierToEscape)
          DuplicateQualifierToEscape = true;

        // If Alternate Qualifier is enabled, disable DuplicateQualifierToEscape automatically
        if (m_ContextSensitiveQualifier && DuplicateQualifierToEscape)
          DuplicateQualifierToEscape = false;
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(cCommentLineDefault)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public virtual string CommentLine
    {
      get => m_CommentLine;
      set
      {
        var newVal = (value ?? string.Empty).Trim();
        if (m_CommentLine.Equals(value, StringComparison.Ordinal))
          return;
        m_CommentLine = newVal;
        NotifyPropertyChanged(nameof(CommentLine));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(cDelimiterPlaceholderDefault)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public virtual string DelimiterPlaceholder
    {
      get => m_DelimiterPlaceholder;
      set
      {
        var newVal = (value ?? string.Empty).Trim();
        if (m_DelimiterPlaceholder.Equals(newVal, StringComparison.Ordinal))
          return;
        m_DelimiterPlaceholder = newVal;
        NotifyPropertyChanged(nameof(DelimiterPlaceholder));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(cDuplicateQualifierToEscapeDefault)]
    public virtual bool DuplicateQualifierToEscape
    {
      get => m_DuplicateQualifierToEscape;
      set
      {
        if (m_DuplicateQualifierToEscape.Equals(value))
          return;
        m_DuplicateQualifierToEscape = value;
        NotifyPropertyChanged(nameof(DuplicateQualifierToEscape));
      }
    }

    [XmlIgnore] public virtual char EscapePrefixChar => m_EscapePrefixChar;

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(cEscapePrefixDefault)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public virtual string EscapePrefix
    {
      get => m_EscapePrefix;
      set
      {
        var newVal = (value ?? string.Empty).Trim();
        if (m_EscapePrefix.Equals(newVal, StringComparison.Ordinal))
          return;
        m_EscapePrefixChar = newVal.WrittenPunctuationToChar();
        m_EscapePrefix = newVal;
        NotifyPropertyChanged(nameof(EscapePrefix));
        NotifyPropertyChanged(nameof(EscapePrefixChar));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(cFieldDelimiterDefault)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public virtual string FieldDelimiter
    {
      get => m_FieldDelimiter;
      set
      {
        var newVal = (value ?? string.Empty).Trim(StringUtils.Spaces);
        if (m_FieldDelimiter.Equals(newVal, StringComparison.Ordinal))
          return;
        m_FieldDelimiterChar = newVal.WrittenPunctuationToChar();
        m_FieldDelimiter = newVal;
        NotifyPropertyChanged(nameof(FieldDelimiter));
        NotifyPropertyChanged(nameof(FieldDelimiterChar));
      }
    }

    /// <inheritdoc />
    [XmlIgnore]
    public virtual char FieldDelimiterChar => m_FieldDelimiterChar;

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(cFieldQualifierDefault)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public virtual string FieldQualifier
    {
      get => m_FieldQualifier;
      set
      {
        var newVal = (value ?? string.Empty).Trim();
        if (m_FieldQualifier.Equals(newVal, StringComparison.Ordinal))
          return;
        m_FieldQualifierChar = newVal.WrittenPunctuationToChar();
        m_FieldQualifier = newVal;
        NotifyPropertyChanged(nameof(FieldQualifier));
        NotifyPropertyChanged(nameof(m_FieldQualifierChar));
      }
    }

    /// <inheritdoc />
    [XmlIgnore]
    public virtual char FieldQualifierChar => m_FieldQualifierChar;

    /// <inheritdoc />
    [XmlIgnore]
    [Obsolete("Check FieldDelimiterChar instead")]
    public virtual bool IsFixedLength => string.IsNullOrEmpty(m_FieldDelimiter);

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(cNewLineDefault)]
    public virtual RecordDelimiterTypeEnum NewLine
    {
      get => m_NewLine;

      set
      {
        if (m_NewLine.Equals(value))
          return;
        m_NewLine = value;
        NotifyPropertyChanged(nameof(NewLine));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(cNewLinePlaceholderDefault)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public virtual string NewLinePlaceholder
    {
      get => m_NewLinePlaceholder;
      set
      {
        var newVal = value ?? cNewLinePlaceholderDefault;
        if (m_NewLinePlaceholder.Equals(newVal, StringComparison.OrdinalIgnoreCase))
          return;
        m_NewLinePlaceholder = newVal;
        NotifyPropertyChanged(nameof(NewLinePlaceholder));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(cQualifyAlwaysDefault)]
    public virtual bool QualifyAlways
    {
      get => m_QualifyAlways;
      set
      {
        if (m_QualifyAlways.Equals(value))
          return;
        m_QualifyAlways = value;
        if (m_QualifyAlways)
          QualifyOnlyIfNeeded = false;
        NotifyPropertyChanged(nameof(QualifyAlways));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(cQualifyOnlyIfNeededDefault)]
    public virtual bool QualifyOnlyIfNeeded
    {
      get => m_QualifyOnlyIfNeeded;

      set
      {
        if (m_QualifyOnlyIfNeeded.Equals(value))
          return;
        m_QualifyOnlyIfNeeded = value;
        if (m_QualifyOnlyIfNeeded)
          QualifyAlways = false;
        NotifyPropertyChanged(nameof(QualifyOnlyIfNeeded));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(cQuotePlaceholderDefault)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public virtual string QualifierPlaceholder
    {
      get => m_QualifierPlaceholder;
      set
      {
        var newVal = (value ?? string.Empty).Trim();
        if (m_QualifierPlaceholder.Equals(newVal, StringComparison.Ordinal))
          return;
        m_QualifierPlaceholder = newVal;
        NotifyPropertyChanged(nameof(QualifierPlaceholder));
      }
    }

    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool AllowRowCombining
    {
      get => m_AllowRowCombining;

      set
      {
        if (m_AllowRowCombining.Equals(value))
          return;
        m_AllowRowCombining = value;
        NotifyPropertyChanged(nameof(AllowRowCombining));
      }
    }


    /// <inheritdoc />
    [XmlIgnore]
    public virtual bool NoDelimitedFile
    {
      get => m_NoDelimitedFile;

      set
      {
        if (m_NoDelimitedFile.Equals(value))
          return;
        m_NoDelimitedFile = value;
        NotifyPropertyChanged(nameof(NoDelimitedFile));
      }
    }

    /// <inheritdoc />
    [XmlElement]
    [DefaultValue(0)]
    public virtual int NumWarnings
    {
      get => m_NumWarnings;

      set
      {
        if (m_NumWarnings.Equals(value))
          return;
        m_NumWarnings = value;
        NotifyPropertyChanged(nameof(NumWarnings));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool TreatLfAsSpace
    {
      get => m_TreatLfAsSpace;

      set
      {
        if (m_TreatLfAsSpace.Equals(value))
          return;
        m_TreatLfAsSpace = value;
        NotifyPropertyChanged(nameof(TreatLfAsSpace));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool TreatUnknownCharacterAsSpace
    {
      get => m_TreatUnknownCharacterAsSpace;

      set
      {
        if (m_TreatUnknownCharacterAsSpace.Equals(value))
          return;
        m_TreatUnknownCharacterAsSpace = value;
        NotifyPropertyChanged(nameof(TreatUnknownCharacterAsSpace));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool TryToSolveMoreColumns
    {
      get => m_TryToSolveMoreColumns;

      set
      {
        if (m_TryToSolveMoreColumns.Equals(value))
          return;
        m_TryToSolveMoreColumns = value;
        NotifyPropertyChanged(nameof(TryToSolveMoreColumns));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool WarnDelimiterInValue
    {
      get => m_WarnDelimiterInValue;

      set
      {
        if (m_WarnDelimiterInValue.Equals(value))
          return;
        m_WarnDelimiterInValue = value;
        NotifyPropertyChanged(nameof(WarnDelimiterInValue));
      }
    }

    /// <inheritdoc />
    [XmlAttribute(AttributeName = "WarnEmptyTailingColumns")]
    [DefaultValue(true)]
    public virtual bool WarnEmptyTailingColumns
    {
      get => m_WarnEmptyTailingColumns;

      set
      {
        if (m_WarnEmptyTailingColumns.Equals(value))
          return;
        m_WarnEmptyTailingColumns = value;
        NotifyPropertyChanged(nameof(WarnEmptyTailingColumns));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool WarnLineFeed
    {
      get => m_WarnLineFeed;

      set
      {
        if (m_WarnLineFeed.Equals(value))
          return;
        m_WarnLineFeed = value;
        NotifyPropertyChanged(nameof(WarnLineFeed));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(true)]
    public virtual bool WarnNBSP
    {
      get => m_WarnNbsp;

      set
      {
        if (m_WarnNbsp.Equals(value))
          return;
        m_WarnNbsp = value;
        NotifyPropertyChanged(nameof(WarnNBSP));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool WarnQuotes
    {
      get => m_WarnQuotes;

      set
      {
        if (m_WarnQuotes.Equals(value))
          return;
        m_WarnQuotes = value;
        NotifyPropertyChanged(nameof(WarnQuotes));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(true)]
    public virtual bool WarnQuotesInQuotes
    {
      get => m_WarnQuotesInQuotes;

      set
      {
        if (m_WarnQuotesInQuotes.Equals(value))
          return;
        m_WarnQuotesInQuotes = value;
        NotifyPropertyChanged(nameof(WarnQuotesInQuotes));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(true)]
    public virtual bool WarnUnknownCharacter
    {
      get => m_WarnUnknownCharacter;

      set
      {
        if (m_WarnUnknownCharacter.Equals(value))
          return;
        m_WarnUnknownCharacter = value;
        NotifyPropertyChanged(nameof(WarnUnknownCharacter));
      }
    }

    /// <inheritdoc />
    public override object Clone()
    {
      var other = new CsvFile();
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
      csv.EscapePrefix = EscapePrefix;
      csv.FieldDelimiter = FieldDelimiter;
      csv.FieldQualifier = FieldQualifier;
      csv.NewLine = NewLine;
      csv.NewLinePlaceholder = NewLinePlaceholder;
      csv.QualifyOnlyIfNeeded = QualifyOnlyIfNeeded;
      csv.QualifyAlways = QualifyAlways;
      csv.QualifierPlaceholder = QualifierPlaceholder;

      csv.LastChange = LastChange;
    }

    /// <inheritdoc />
    public override bool Equals(IFileSetting? other) => Equals(other as ICsvFile);

    /// <inheritdoc />
    public virtual bool Equals(ICsvFile? other)
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
          yield return $"NoDelimitedFile: {NoDelimitedFile} {csv.NoDelimitedFile}";
        
        if (m_NumWarnings != csv.NumWarnings) 
          yield return $"NumWarnings: {NumWarnings} {csv.NumWarnings}";
        
        if (m_TreatUnknownCharacterAsSpace != csv.TreatUnknownCharacterAsSpace) 
          yield return $"TreatUnknownCharacterAsSpace: {TreatUnknownCharacterAsSpace} {csv.TreatUnknownCharacterAsSpace}";
        
        if (m_WarnDelimiterInValue != csv.WarnDelimiterInValue) 
          yield return $"WarnDelimiterInValue: {WarnDelimiterInValue} {csv.WarnDelimiterInValue}";
        
        if (m_WarnEmptyTailingColumns != csv.WarnEmptyTailingColumns) 
          yield return $"WarnEmptyTailingColumns: {WarnEmptyTailingColumns} {csv.WarnEmptyTailingColumns}";
        
        if (m_WarnLineFeed != csv.WarnLineFeed) 
          yield return $"WarnLineFeed: {WarnLineFeed} {csv.WarnLineFeed}";
        
        if (m_TryToSolveMoreColumns != csv.TryToSolveMoreColumns) 
          yield return $"TryToSolveMoreColumns: {TryToSolveMoreColumns} {csv.TryToSolveMoreColumns}";
        
        if (m_AllowRowCombining != csv.AllowRowCombining) 
          yield return $"AllowRowCombining: {AllowRowCombining} {csv.AllowRowCombining}";
        
        if (m_TreatLfAsSpace != csv.TreatLfAsSpace) 
          yield return $"TreatLFAsSpace: {TreatLfAsSpace} {csv.TreatLfAsSpace}";
        
        if (m_WarnNbsp != csv.WarnNBSP) 
          yield return $"WarnNBSP: {WarnNBSP} {csv.WarnNBSP}";
        
        if (m_WarnQuotesInQuotes != csv.WarnQuotesInQuotes) 
          yield return $"WarnQuotesInQuotes: {WarnQuotesInQuotes} {csv.WarnQuotesInQuotes}";
        
        if (m_WarnUnknownCharacter != csv.WarnUnknownCharacter) 
          yield return $"WarnUnknownCharacter: {WarnUnknownCharacter} {csv.WarnUnknownCharacter}";
        
        if (m_WarnQuotes != csv.WarnQuotes) 
          yield return $"WarnQuotes: {WarnQuotes} {csv.WarnQuotes}";
        
        if (ContextSensitiveQualifier != csv.ContextSensitiveQualifier) 
          yield return $"ContextSensitiveQualifier: {ContextSensitiveQualifier} {csv.ContextSensitiveQualifier}";
        
        if (DuplicateQualifierToEscape != csv.DuplicateQualifierToEscape) 
          yield return $"DuplicateQualifierToEscape: {DuplicateQualifierToEscape} {csv.DuplicateQualifierToEscape}";
        
        if (!string.Equals(CommentLine, csv.CommentLine, StringComparison.Ordinal)) 
          yield return $"CommentLine: {CommentLine} {csv.CommentLine}";
        
        if (!string.Equals(DelimiterPlaceholder, csv.DelimiterPlaceholder, StringComparison.Ordinal)) 
          yield return $"DelimiterPlaceholder: {DelimiterPlaceholder} {csv.DelimiterPlaceholder}";
        
        if (EscapePrefixChar != csv.EscapePrefixChar) 
          yield return $"EscapePrefixChar: {EscapePrefixChar} {csv.EscapePrefixChar}";
        
        if (FieldDelimiterChar != csv.FieldDelimiterChar) 
          yield return $"FieldDelimiterChar: {FieldDelimiterChar} {csv.FieldDelimiterChar}";
        
        if (FieldQualifierChar != csv.FieldQualifierChar) 
          yield return $"FieldQualifierChar: {FieldQualifierChar} {csv.FieldQualifierChar}";
        
        if (!NewLine.Equals(csv.NewLine)) 
          yield return $"NewLine: {NewLine} {csv.NewLine}";

        if (!string.Equals(NewLinePlaceholder, csv.NewLinePlaceholder, StringComparison.Ordinal)) 
          yield return $"NewLinePlaceholder: {NewLinePlaceholder} {csv.NewLinePlaceholder}";
        
        if (QualifyAlways != csv.QualifyAlways) 
          yield return $"QualifyAlways: {QualifyAlways} {csv.QualifyAlways}";
        
        if (QualifyOnlyIfNeeded != csv.QualifyOnlyIfNeeded) 
          yield return $"QualifyOnlyIfNeeded: {QualifyOnlyIfNeeded} {csv.QualifyOnlyIfNeeded}";
        
        if (!string.Equals(QualifierPlaceholder, csv.QualifierPlaceholder, StringComparison.Ordinal)) 
          yield return $"QualifierPlaceholder: {QualifierPlaceholder} {csv.QualifierPlaceholder}";
      }
      foreach (var res in base.GetDifferences(other))
        yield return res;
    }
    #region backwardscompatibility

    [XmlElement]
    [DefaultValue(null)]
#pragma warning disable CS0618 // Type or member is obsolete
    public FileFormatStore? FileFormat
#pragma warning restore CS0618 // Type or member is obsolete
    {
      get;
      set;
    }

    [Obsolete("Only used for backwards compatibility of Serialization")]
    public virtual void OverwriteFromFileFormatStore()
    {
      if (FileFormat is null)
        return;

      ContextSensitiveQualifier = FileFormat.AlternateQuoting;
      DuplicateQualifierToEscape = FileFormat.DuplicateQuotingToEscape;
      CommentLine = FileFormat.CommentLine;
      DelimiterPlaceholder = FileFormat.DelimiterPlaceholder;
      EscapePrefix = FileFormat.EscapeCharacter;
      FieldDelimiter = FileFormat.FieldDelimiter;
      FieldQualifier = FileFormat.FieldQualifier;
      NewLine = FileFormat.NewLine;
      NewLinePlaceholder = FileFormat.NewLinePlaceholder;
      QualifyAlways = FileFormat.QualifyAlways;
      QualifyOnlyIfNeeded = FileFormat.QualifyOnlyIfNeeded;
      QualifierPlaceholder = FileFormat.QuotePlaceholder;

      FileFormat = null;
    }

    [Obsolete("Only used for backwards compatibility of Serialization")]
    [Serializable]
    public class FileFormatStore
    {
      /// <summary>
      ///   Gets or sets a value indicating whether the byte order mark should be written in Unicode files.
      /// </summary>
      /// <value><c>true</c> write byte order mark; otherwise, <c>false</c>.</value>
      [XmlAttribute]
      [DefaultValue(cContextSensitiveQualifierDefault)]
      public bool AlternateQuoting
      {
        get;
        set;
      } = cContextSensitiveQualifierDefault;

      /// <summary>
      ///   Gets or sets the text to indicate that the line is comment line and not contain data. If a
      ///   line starts with the given text, it is ignored in the data grid.
      /// </summary>
      /// <value>The startup comment line.</value>
      [XmlAttribute]
      [DefaultValue(cCommentLineDefault)]

      public string CommentLine
      {
        get;
        set;
      } = cCommentLineDefault;

      /// <summary>
      ///   Gets or sets the delimiter placeholder.
      /// </summary>
      /// <value>The delimiter placeholder.</value>
      [XmlAttribute]
      [DefaultValue(cDelimiterPlaceholderDefault)]
      public string DelimiterPlaceholder
      {
        get;
        set;
      } = cDelimiterPlaceholderDefault;

      /// <summary>
      ///   Gets or sets a value indicating whether the byte order mark should be written in Unicode files.
      /// </summary>
      /// <value><c>true</c> write byte order mark; otherwise, <c>false</c>.</value>
      [XmlAttribute]
      [DefaultValue(cDuplicateQualifierToEscapeDefault)]
      public bool DuplicateQuotingToEscape
      {
        get;
        set;
      } = cDuplicateQualifierToEscapeDefault;

      /// <summary>
      ///   Gets or sets the escape character.
      /// </summary>
      /// <value>The escape character.</value>
      [XmlAttribute]
      [DefaultValue(cEscapePrefixDefault)]
      public string EscapeCharacter
      {
        get;
        set;
      } = cEscapePrefixDefault;

      /// <summary>
      ///   Gets or sets the field delimiter.
      /// </summary>
      /// <value>The field delimiter.</value>
      [XmlAttribute]
      [DefaultValue(cFieldDelimiterDefault)]
      public string FieldDelimiter
      {
        get;
        set;
      } = cFieldDelimiterDefault;

      /// <summary>
      ///   Gets or sets the field qualifier.
      /// </summary>
      /// <value>The field qualifier.</value>
      [XmlAttribute]
      [DefaultValue(cFieldQualifierDefault)]
      public string FieldQualifier
      {
        get;
        set;
      } = cFieldQualifierDefault;

      /// <summary>
      ///   Gets or sets the newline.
      /// </summary>
      /// <value>The newline.</value>
      [XmlAttribute]
      [DefaultValue(cNewLineDefault)]
      public RecordDelimiterTypeEnum NewLine
      {
        get;
        set;
      } = cNewLineDefault;

      [XmlAttribute]
      [DefaultValue(cNewLinePlaceholderDefault)]
      public string NewLinePlaceholder
      {
        get;
        set;
      } = cNewLinePlaceholderDefault;

      [XmlAttribute]
      [DefaultValue(cQualifyAlwaysDefault)]
      public bool QualifyAlways
      {
        get;
        set;
      } = cQualifyAlwaysDefault;

      [XmlAttribute]
      [DefaultValue(cQualifyOnlyIfNeededDefault)]
      public bool QualifyOnlyIfNeeded
      {
        get;
        set;
      } = cQualifyOnlyIfNeededDefault;

      [XmlAttribute]
      [DefaultValue(cQuotePlaceholderDefault)]
      public string QuotePlaceholder
      {
        get;
        set;
      } = cQuotePlaceholderDefault;
    }

    #endregion
  }
}