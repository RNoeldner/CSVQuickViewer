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
using System.Text;
using System.Xml.Serialization;

namespace CsvTools
{
  /// <summary>
  ///   Setting file for CSV files, its an implementation of <see cref="BaseSettings" />
  /// </summary>
  [Serializable]
  public class CsvFile : BaseSettingPhysicalFile, ICsvFile
  {
    private const bool c_ContextSensitiveQualifierDefault = false;
    private const bool c_QualifyAlwaysDefault = false;
    private const string c_EscapePrefixDefault = "";
    private const RecordDelimiterType c_NewLineDefault = RecordDelimiterType.CRLF;
    private const string c_CommentLineDefault = "";
    private const string c_DelimiterPlaceholderDefault = "";
    private const string c_FieldDelimiterDefault = ",";
    private const string c_FieldQualifierDefault = "\"";
    private const string c_NewLinePlaceholderDefault = "";
    private const bool c_QualifyOnlyIfNeededDefault = true;
    private const string c_QuotePlaceholderDefault = "";
    private const bool c_DuplicateQualifierToEscapeDefault = true;

    /// <summary>
    ///   File ending for a setting file
    /// </summary>
    public const string cCsvSettingExtension = ".setting";

    private bool m_AllowRowCombining;

    private bool m_ByteOrderMark = true;

    private int m_CodePageId = 65001;
    private string m_CommentLine = c_CommentLineDefault;

    private bool m_ContextSensitiveQualifier = c_ContextSensitiveQualifierDefault;

    [NonSerialized] private Encoding m_CurrentEncoding = Encoding.UTF8;
    private string m_DelimiterPlaceholder = c_DelimiterPlaceholderDefault;
    private bool m_DuplicateQualifierToEscape = c_DuplicateQualifierToEscapeDefault;
    private string m_EscapePrefix = c_EscapePrefixDefault;
    private char m_EscapePrefixChar = '\0';
    private string m_FieldDelimiter = c_FieldDelimiterDefault;
    private char m_FieldDelimiterChar = c_FieldDelimiterDefault[0];
    private string m_FieldQualifier = c_FieldQualifierDefault;
    private char m_FieldQualifierChar = c_FieldQualifierDefault[0];
    private RecordDelimiterType m_NewLine = c_NewLineDefault;
    private string m_NewLinePlaceholder = c_NewLinePlaceholderDefault;

    private bool m_NoDelimitedFile;

    private int m_NumWarnings;
    private string m_QualifierPlaceholder = c_QuotePlaceholderDefault;
    private bool m_QualifyAlways = c_QualifyAlwaysDefault;
    private bool m_QualifyOnlyIfNeeded = c_QualifyOnlyIfNeededDefault;

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

    /// <summary>
    ///   Initializes a new instance of the <see cref="CsvFile" /> class.
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

    /// <summary>
    ///   Gets or sets a value indicating whether the byte order mark should be written in Unicode files.
    /// </summary>
    /// <value><c>true</c> write byte order mark; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(c_ContextSensitiveQualifierDefault)]
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

    /// <summary>
    ///   Gets or sets the text to indicate that the line is comment line and not contain data. If a
    ///   line starts with the given text, it is ignored in the data grid.
    /// </summary>
    /// <value>The startup comment line.</value>
    [XmlAttribute]
    [DefaultValue(c_CommentLineDefault)]
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

    /// <summary>
    ///   Gets or sets the delimiter placeholder.
    /// </summary>
    /// <value>The delimiter placeholder.</value>
    [XmlAttribute]
    [DefaultValue(c_DelimiterPlaceholderDefault)]
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

    /// <summary>
    ///   Gets or sets a value indicating whether the byte order mark should be written in Unicode files.
    /// </summary>
    /// <value><c>true</c> write byte order mark; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(c_DuplicateQualifierToEscapeDefault)]
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

    /// <summary>
    ///   Gets or sets the escape character.
    /// </summary>
    /// <value>The escape character.</value>
    [XmlAttribute]
    [DefaultValue(c_EscapePrefixDefault)]
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

    /// <summary>
    ///   Gets or sets the field delimiter.
    /// </summary>
    /// <value>The field delimiter.</value>
    [XmlAttribute]
    [DefaultValue(c_FieldDelimiterDefault)]
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

    /// <summary>
    ///   Gets the field delimiter char from the FieldDelimiter.
    /// </summary>
    /// <value>The field delimiter char.</value>
    [XmlIgnore]
    public virtual char FieldDelimiterChar => m_FieldDelimiterChar;

    /// <summary>
    ///   Gets or sets the field qualifier.
    /// </summary>
    /// <value>The field qualifier.</value>
    [XmlAttribute]
    [DefaultValue(c_FieldQualifierDefault)]
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

    /// <summary>
    ///   Gets the field qualifier char from the FieldQualifier.
    /// </summary>
    /// <value>The field qualifier char.</value>
    [XmlIgnore]
    public virtual char FieldQualifierChar => m_FieldQualifierChar;

    /// <summary>
    ///   Gets a value indicating whether this instance is fixed length.
    /// </summary>
    /// <value><c>true</c> if this instance is fixed length; otherwise, <c>false</c>.</value>
    [XmlIgnore]
    [Obsolete("Check FieldDelimiterChar instead")]
    public virtual bool IsFixedLength => string.IsNullOrEmpty(m_FieldDelimiter);

    /// <summary>
    ///   Gets or sets the newline.
    /// </summary>
    /// <value>The newline.</value>
    [XmlAttribute]
    [DefaultValue(c_NewLineDefault)]
    public virtual RecordDelimiterType NewLine
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

    /// <summary>
    ///   Gets or sets the new line placeholder.
    /// </summary>
    /// <value>The new line placeholder.</value>
    [XmlAttribute]
    [DefaultValue(c_NewLinePlaceholderDefault)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public virtual string NewLinePlaceholder
    {
      get => m_NewLinePlaceholder;
      set
      {
        var newVal = value ?? c_NewLinePlaceholderDefault;
        if (m_NewLinePlaceholder.Equals(newVal, StringComparison.OrdinalIgnoreCase))
          return;
        m_NewLinePlaceholder = newVal;
        NotifyPropertyChanged(nameof(NewLinePlaceholder));
      }
    }

    /// <summary>
    ///   Gets or sets a value indicating whether to qualify every text even if number or empty.
    /// </summary>
    /// <value><c>true</c> if qualify only if needed; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(c_QualifyAlwaysDefault)]
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

    /// <summary>
    ///   Gets or sets a value indicating whether to qualify only if needed.
    /// </summary>
    /// <value><c>true</c> if qualify only if needed; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(c_QualifyOnlyIfNeededDefault)]
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

    /// <summary>
    ///   Gets or sets the quote placeholder.
    /// </summary>
    /// <value>The quote placeholder.</value>
    [XmlAttribute]
    [DefaultValue(c_QuotePlaceholderDefault)]
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

    /// <summary>
    ///   Gets or sets a value indicating whether the byte order mark should be written in Unicode files.
    /// </summary>
    /// <value><c>true</c> write byte order mark; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(true)]
    public virtual bool ByteOrderMark
    {
      get => m_ByteOrderMark;
      set
      {
        if (m_ByteOrderMark.Equals(value))
          return;
        m_ByteOrderMark = value;
        NotifyPropertyChanged(nameof(ByteOrderMark));
      }
    }

    /// <summary>
    ///   Gets or sets the code page.
    /// </summary>
    /// <value>The code page.</value>
    [XmlAttribute]
    [DefaultValue(65001)]
    public virtual int CodePageId
    {
      get => m_CodePageId;
      set
      {
        if (m_CodePageId.Equals(value))
          return;
        m_CodePageId = value;
        NotifyPropertyChanged(nameof(CodePageId));
      }
    }

    /// <summary>
    ///   Gets or sets a value indicating whether a file is most likely not a delimited file
    /// </summary>
    /// <value><c>true</c> if the file is assumed to be a non delimited file; otherwise, <c>false</c>.</value>
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

    /// <summary>
    ///   Gets or sets the maximum number of warnings.
    /// </summary>
    /// <value>The number of warnings.</value>
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

    /// <summary>
    ///   Gets or sets a value indicating whether to treat a single LF as space
    /// </summary>
    /// <value><c>true</c> if LF should be treated as space; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool TreatLFAsSpace
    {
      get => m_TreatLfAsSpace;

      set
      {
        if (m_TreatLfAsSpace.Equals(value))
          return;
        m_TreatLfAsSpace = value;
        NotifyPropertyChanged(nameof(TreatLFAsSpace));
      }
    }

    /// <summary>
    ///   Gets or sets a value indicating whether to replace unknown charters.
    /// </summary>
    /// <value><c>true</c> if unknown character should be replaced; otherwise, <c>false</c>.</value>
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

    /// <summary>
    ///   Gets or sets a value indicating whether the reader should try to solve more columns.
    /// </summary>
    /// <value>
    ///   <c>true</c> if it should be try to solve misalignment more columns; otherwise, <c>false</c>.
    /// </value>
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

    /// <summary>
    ///   Gets or sets a value indicating whether to warn if delimiter is in a value.
    /// </summary>
    /// <value>
    ///   <c>true</c> if a warning should be issued if a delimiter is encountered; otherwise, <c>false</c>.
    /// </value>
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

    /// <summary>
    ///   Gets or sets a value indicating whether to warn empty tailing columns.
    /// </summary>
    /// <value><c>true</c> if [warn empty tailing columns]; otherwise, <c>false</c>.</value>
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

    /// <summary>
    ///   Gets or sets a value indicating whether to warn unknown character.
    /// </summary>
    /// <value><c>true</c> if unknown character should issue a warning; otherwise, <c>false</c>.</value>
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

    /// <summary>
    ///   Gets or sets a value indicating whether to warn occurrence of NBSP.
    /// </summary>
    /// <value><c>true</c> to issue a writing if there is a NBSP; otherwise, <c>false</c>.</value>
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

    /// <summary>
    ///   Gets or sets a value indicating whether the byte order mark should be written in Unicode files.
    /// </summary>
    /// <value><c>true</c> write byte order mark; otherwise, <c>false</c>.</value>
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

    /// <summary>
    ///   Gets or sets a value indicating whether the byte order mark should be written in Unicode files.
    /// </summary>
    /// <value><c>true</c> write byte order mark; otherwise, <c>false</c>.</value>
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

    /// <summary>
    ///   Gets or sets a value indicating whether to warn unknown character.
    /// </summary>
    /// <value><c>true</c> if unknown character should issue a warning; otherwise, <c>false</c>.</value>
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

    /// <summary>
    ///   Clones this instance.
    /// </summary>
    /// <returns></returns>
    public override object Clone()
    {
      var other = new CsvFile();
      CopyTo(other);
      return other;
    }

    /// <summary>
    ///   Copies all values to other instance
    /// </summary>
    /// <param name="other">The other.</param>
    public override void CopyTo(IFileSetting other)
    {
      BaseSettingsCopyTo((BaseSettings) other);

      if (!(other is CsvFile csv))
        return;
      csv.ByteOrderMark = m_ByteOrderMark;
      csv.WarnQuotes = m_WarnQuotes;
      csv.WarnDelimiterInValue = m_WarnDelimiterInValue;
      csv.WarnEmptyTailingColumns = m_WarnEmptyTailingColumns;
      csv.WarnQuotesInQuotes = m_WarnQuotesInQuotes;
      csv.WarnUnknownCharacter = m_WarnUnknownCharacter;
      csv.WarnLineFeed = m_WarnLineFeed;
      csv.WarnNBSP = m_WarnNbsp;
      csv.TreatLFAsSpace = m_TreatLfAsSpace;
      csv.TryToSolveMoreColumns = m_TryToSolveMoreColumns;
      csv.AllowRowCombining = m_AllowRowCombining;

      csv.TreatUnknownCharacterAsSpace = m_TreatUnknownCharacterAsSpace;
      csv.CodePageId = m_CodePageId;
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
    }

    public override bool Equals(IFileSetting? other) => Equals(other as ICsvFile);

    public virtual bool Equals(ICsvFile? other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return m_ByteOrderMark == other.ByteOrderMark && m_CodePageId == other.CodePageId
                                                    && m_NoDelimitedFile == other.NoDelimitedFile
                                                    && m_NumWarnings == other.NumWarnings
                                                    && m_TreatUnknownCharacterAsSpace
                                                    == other.TreatUnknownCharacterAsSpace
                                                    && m_WarnDelimiterInValue == other.WarnDelimiterInValue
                                                    && m_WarnEmptyTailingColumns == other.WarnEmptyTailingColumns
                                                    && m_WarnLineFeed == other.WarnLineFeed
                                                    && m_TryToSolveMoreColumns == other.TryToSolveMoreColumns
                                                    && m_AllowRowCombining == other.AllowRowCombining
                                                    && m_TreatLfAsSpace == other.TreatLFAsSpace
                                                    && m_WarnNbsp == other.WarnNBSP && m_WarnQuotes == other.WarnQuotes
                                                    && m_WarnQuotesInQuotes == other.WarnQuotesInQuotes
                                                    && m_WarnUnknownCharacter == other.WarnUnknownCharacter
                                                    && BaseSettingsEquals(other as BaseSettings)
                                                    && ContextSensitiveQualifier == other.ContextSensitiveQualifier
                                                    && DuplicateQualifierToEscape == other.DuplicateQualifierToEscape
                                                    && string.Equals(CommentLine,
                                                      other.CommentLine,
                                                      StringComparison.Ordinal)
                                                    && string.Equals(DelimiterPlaceholder,
                                                      other.DelimiterPlaceholder,
                                                      StringComparison.Ordinal)
                                                    && EscapePrefixChar == other.EscapePrefixChar
                                                    && FieldDelimiterChar == other.FieldDelimiterChar
                                                    && FieldQualifierChar == other.FieldQualifierChar
                                                    && NewLine.Equals(other.NewLine)
                                                    && string.Equals(NewLinePlaceholder,
                                                      other.NewLinePlaceholder,
                                                      StringComparison.Ordinal)
                                                    && QualifyAlways == other.QualifyAlways
                                                    && QualifyOnlyIfNeeded == other.QualifyOnlyIfNeeded
                                                    && string.Equals(QualifierPlaceholder,
                                                      other.QualifierPlaceholder,
                                                      StringComparison.Ordinal);
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
      [DefaultValue(c_ContextSensitiveQualifierDefault)]
      public bool AlternateQuoting
      {
        get;
        set;
      } = c_ContextSensitiveQualifierDefault;

      /// <summary>
      ///   Gets or sets the text to indicate that the line is comment line and not contain data. If a
      ///   line starts with the given text, it is ignored in the data grid.
      /// </summary>
      /// <value>The startup comment line.</value>
      [XmlAttribute]
      [DefaultValue(c_CommentLineDefault)]

      public string CommentLine
      {
        get;
        set;
      } = c_CommentLineDefault;

      /// <summary>
      ///   Gets or sets the delimiter placeholder.
      /// </summary>
      /// <value>The delimiter placeholder.</value>
      [XmlAttribute]
      [DefaultValue(c_DelimiterPlaceholderDefault)]
      public string DelimiterPlaceholder
      {
        get;
        set;
      } = c_DelimiterPlaceholderDefault;

      /// <summary>
      ///   Gets or sets a value indicating whether the byte order mark should be written in Unicode files.
      /// </summary>
      /// <value><c>true</c> write byte order mark; otherwise, <c>false</c>.</value>
      [XmlAttribute]
      [DefaultValue(c_DuplicateQualifierToEscapeDefault)]
      public bool DuplicateQuotingToEscape
      {
        get;
        set;
      } = c_DuplicateQualifierToEscapeDefault;

      /// <summary>
      ///   Gets or sets the escape character.
      /// </summary>
      /// <value>The escape character.</value>
      [XmlAttribute]
      [DefaultValue(c_EscapePrefixDefault)]
      public string EscapeCharacter
      {
        get;
        set;
      } = c_EscapePrefixDefault;

      /// <summary>
      ///   Gets or sets the field delimiter.
      /// </summary>
      /// <value>The field delimiter.</value>
      [XmlAttribute]
      [DefaultValue(c_FieldDelimiterDefault)]
      public string FieldDelimiter
      {
        get;
        set;
      } = c_FieldDelimiterDefault;

      /// <summary>
      ///   Gets or sets the field qualifier.
      /// </summary>
      /// <value>The field qualifier.</value>
      [XmlAttribute]
      [DefaultValue(c_FieldQualifierDefault)]
      public string FieldQualifier
      {
        get;
        set;
      } = c_FieldQualifierDefault;

      /// <summary>
      ///   Gets or sets the newline.
      /// </summary>
      /// <value>The newline.</value>
      [XmlAttribute]
      [DefaultValue(c_NewLineDefault)]
      public RecordDelimiterType NewLine
      {
        get;
        set;
      } = c_NewLineDefault;

      [XmlAttribute]
      [DefaultValue(c_NewLinePlaceholderDefault)]
      public string NewLinePlaceholder
      {
        get;
        set;
      } = c_NewLinePlaceholderDefault;

      [XmlAttribute]
      [DefaultValue(c_QualifyAlwaysDefault)]
      public bool QualifyAlways
      {
        get;
        set;
      } = c_QualifyAlwaysDefault;

      [XmlAttribute]
      [DefaultValue(c_QualifyOnlyIfNeededDefault)]
      public bool QualifyOnlyIfNeeded
      {
        get;
        set;
      } = c_QualifyOnlyIfNeededDefault;

      [XmlAttribute]
      [DefaultValue(c_QuotePlaceholderDefault)]
      public string QuotePlaceholder
      {
        get;
        set;
      } = c_QuotePlaceholderDefault;
    }

#endregion
  }
}