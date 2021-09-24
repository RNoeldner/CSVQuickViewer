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
  /// <summary>
  ///   Setting class for a general file format
  /// </summary>
  [Serializable]
  public class FileFormat : INotifyPropertyChanged, IFileFormat, IEquatable<FileFormat>, ICloneable
  {
    public const string cEscapeCharacterDefault = "";
    public const RecordDelimiterType cNewLineDefault = RecordDelimiterType.CRLF;
    private const string c_CommentLineDefault = "";
    private const string c_DelimiterPlaceholderDefault = "";
    private const string c_FieldDelimiterDefault = ",";
    private const string c_FieldQualifierDefault = "\"";
    private const string c_NewLinePlaceholderDefault = "";
    private const bool c_QualifyOnlyIfNeededDefault = true;
    private const string c_QuotePlaceholderDefault = "";

    private bool m_AlternateQuoting;
    private string m_CommentLine;
    private string m_DelimiterPlaceholder;
    private bool m_DuplicateQuotingToEscape;
    private char m_EscapeChar;
    private string m_EscapeCharacter;
    private string m_FieldDelimiter;
    private char m_FieldDelimiterChar;
    private string m_FieldQualifier;
    private char m_FieldQualifierChar;
    private RecordDelimiterType m_NewLine;
    private string m_NewLinePlaceholder;
    private bool m_QualifyAlways;
    private bool m_QualifyOnlyIfNeeded;
    private string m_QualifierPlaceholder;

    public FileFormat() : this(false, c_QualifyOnlyIfNeededDefault, false, cEscapeCharacterDefault, true, c_FieldDelimiterDefault, c_DelimiterPlaceholderDefault, c_FieldQualifierDefault, c_QuotePlaceholderDefault, cNewLineDefault, c_NewLinePlaceholderDefault, c_CommentLineDefault)
    { }

    public FileFormat(
      bool qualifyAlways,
      bool qualifyOnlyIfNeeded,
      bool alternateQuoting,
      string escapeCharacter,
      bool duplicateQuotingToEscape,
      in string fieldDelimiter,
      in string delimiterPlaceholder,
      in string fieldQualifier,
      in string quotePlaceholder,
      RecordDelimiterType newLine,
      in string newLinePlaceholder,
      in string commentLine)
    {
      m_CommentLine = commentLine;
      m_DelimiterPlaceholder = delimiterPlaceholder;
      m_DuplicateQuotingToEscape = duplicateQuotingToEscape;
      m_EscapeChar = escapeCharacter.WrittenPunctuationToChar();
      m_EscapeCharacter = escapeCharacter;
      m_FieldDelimiter = fieldDelimiter;
      m_FieldDelimiterChar = fieldDelimiter.WrittenPunctuationToChar();
      m_FieldQualifier = fieldQualifier;
      m_FieldQualifierChar = fieldQualifier.WrittenPunctuationToChar();
      m_NewLine = newLine;
      m_NewLinePlaceholder = newLinePlaceholder;
      m_QualifyAlways = qualifyAlways;
      m_QualifierPlaceholder = quotePlaceholder;
      m_QualifyOnlyIfNeeded = qualifyOnlyIfNeeded;
      m_AlternateQuoting = alternateQuoting;
    }

    /// <summary>
    ///   Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///   Gets or sets a value indicating whether the byte order mark should be written in Unicode files.
    /// </summary>
    /// <value><c>true</c> write byte order mark; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool AlternateQuoting
    {
      get => m_AlternateQuoting;
      set
      {
        if (m_AlternateQuoting.Equals(value))
          return;
        m_AlternateQuoting = value;
        NotifyPropertyChanged(nameof(AlternateQuoting));

        // If Alternate Quoting is disabled, enable DuplicateQuotingToEscape automatically
        if (!m_AlternateQuoting && !DuplicateQuotingToEscape)
          DuplicateQuotingToEscape = true;

        // If Alternate Quoting is enabled, disable DuplicateQuotingToEscape automatically
        if (m_AlternateQuoting && DuplicateQuotingToEscape)
          DuplicateQuotingToEscape = false;
      }
    }

    /// <summary>
    ///   Gets a value indicating whether column format specified.
    /// </summary>
    /// <value>Always <c>false</c>.</value>
    [XmlIgnore]
    public virtual bool ColumnFormatSpecified => false;

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
    [DefaultValue(true)]
    public virtual bool DuplicateQuotingToEscape
    {
      get => m_DuplicateQuotingToEscape;
      set
      {
        if (m_DuplicateQuotingToEscape.Equals(value))
          return;
        m_DuplicateQuotingToEscape = value;
        NotifyPropertyChanged(nameof(DuplicateQuotingToEscape));
      }
    }

    [XmlIgnore]
    public virtual char EscapeChar => m_EscapeChar;

    /// <summary>
    ///   Gets or sets the escape character.
    /// </summary>
    /// <value>The escape character.</value>
    [XmlAttribute]
    [DefaultValue(cEscapeCharacterDefault)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public virtual string EscapeCharacter
    {
      get => m_EscapeCharacter;
      set
      {
        var newVal = (value ?? string.Empty).Trim();
        if (m_EscapeCharacter.Equals(newVal, StringComparison.Ordinal))
          return;
        m_EscapeChar = newVal.WrittenPunctuationToChar();
        m_EscapeCharacter = newVal;
        NotifyPropertyChanged(nameof(EscapeCharacter));
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
    public virtual bool IsFixedLength => string.IsNullOrEmpty(m_FieldDelimiter);

    /// <summary>
    ///   Gets or sets the newline.
    /// </summary>
    /// <value>The newline.</value>
    [XmlAttribute]
    [DefaultValue(cNewLineDefault)]
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
    [DefaultValue(false)]
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
    public virtual string QuotePlaceholder
    {
      get => m_QualifierPlaceholder;
      set
      {
        var newVal = (value ?? string.Empty).Trim();
        if (m_QualifierPlaceholder.Equals(newVal, StringComparison.Ordinal))
          return;
        m_QualifierPlaceholder = newVal;
        NotifyPropertyChanged(nameof(QuotePlaceholder));
      }
    }

    /// <summary>
    ///   Clones this instance into a new instance of the same type
    /// </summary>
    /// <returns></returns>
    public object Clone()
    {
      return new FileFormat(QualifyAlways, QualifyOnlyIfNeeded, AlternateQuoting, EscapeCharacter, DuplicateQuotingToEscape, FieldDelimiter,
        DelimiterPlaceholder, FieldQualifier, QuotePlaceholder, NewLine, NewLinePlaceholder, CommentLine);
    }

    /// <summary>
    ///   Copies to.
    /// </summary>
    /// <param name="other">The other.</param>
    public virtual void CopyTo(FileFormat other)
    {
      other.CommentLine = CommentLine;
      other.AlternateQuoting = AlternateQuoting;
      other.DuplicateQuotingToEscape = DuplicateQuotingToEscape;
      other.DelimiterPlaceholder = DelimiterPlaceholder;
      other.EscapeCharacter = EscapeCharacter;
      other.FieldDelimiter = FieldDelimiter;
      other.FieldQualifier = FieldQualifier;
      other.NewLine = NewLine;
      other.NewLinePlaceholder = NewLinePlaceholder;
      other.QualifyOnlyIfNeeded = QualifyOnlyIfNeeded;
      other.QualifyAlways = QualifyAlways;
      other.QuotePlaceholder = QuotePlaceholder;
    }

    /// <summary>
    ///   Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other" />
    ///   parameter; otherwise, <see langword="false" />.
    /// </returns>
    public bool Equals(FileFormat? other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return AlternateQuoting == other.AlternateQuoting && DuplicateQuotingToEscape == other.DuplicateQuotingToEscape
                                                        && string.Equals(
                                                          CommentLine,
                                                          other.CommentLine,
                                                          StringComparison.Ordinal)
                                                        && string.Equals(
                                                          DelimiterPlaceholder,
                                                          other.DelimiterPlaceholder,
                                                          StringComparison.Ordinal)
                                                        && string.Equals(
                                                          EscapeCharacter,
                                                          other.EscapeCharacter,
                                                          StringComparison.Ordinal)
                                                        && string.Equals(
                                                          FieldDelimiter,
                                                          other.FieldDelimiter,
                                                          StringComparison.Ordinal)
                                                        && FieldDelimiterChar == other.FieldDelimiterChar
                                                        && string.Equals(
                                                          FieldQualifier,
                                                          other.FieldQualifier,
                                                          StringComparison.Ordinal)
                                                        && FieldQualifierChar == other.FieldQualifierChar
                                                        && NewLine.Equals(other.NewLine)
                                                        && string.Equals(
                                                          NewLinePlaceholder,
                                                          other.NewLinePlaceholder,
                                                          StringComparison.Ordinal)
                                                        && QualifyAlways == other.QualifyAlways
                                                        && QualifyOnlyIfNeeded == other.QualifyOnlyIfNeeded
                                                        && string.Equals(
                                                          QuotePlaceholder,
                                                          other.QuotePlaceholder,
                                                          StringComparison.Ordinal);
    }

    /// <summary>
    ///   Notifies the property changed.
    /// </summary>
    /// <param name="info">The info.</param>
    public virtual void NotifyPropertyChanged(string info) =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

    /// <summary>
    ///   Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="string" /> that represents this instance.</returns>
    public override string ToString()
    {
      if (IsFixedLength)
        return "FixedLength";
      return m_FieldDelimiter + " " + m_FieldQualifier;
    }

    /*
    /// <summary>
    ///   Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = m_CommentLine != null ? m_CommentLine.GetHashCode() : 0;
        hashCode = (hashCode * 397) ^ (m_DelimiterPlaceholder != null ? m_DelimiterPlaceholder.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (m_EscapeCharacter != null ? m_EscapeCharacter.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ m_EscapeCharacterChar.GetHashCode();
        hashCode = (hashCode * 397) ^ (m_FieldDelimiter != null ? m_FieldDelimiter.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ m_FieldDelimiterChar.GetHashCode();
        hashCode = (hashCode * 397) ^ (m_FieldQualifier != null ? m_FieldQualifier.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ m_FieldQualifierChar.GetHashCode();
        hashCode = (hashCode * 397) ^ (m_NewLine != null ? m_NewLine.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (m_NewLinePlaceholder != null ? m_NewLinePlaceholder.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ m_QualifyAlways.GetHashCode();
        hashCode = (hashCode * 397) ^ m_QualifyOnlyIfNeeded.GetHashCode();
        hashCode = (hashCode * 397) ^ (m_QuotePlaceholder != null ? m_QuotePlaceholder.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (m_ValueFormatMutable != null ? m_ValueFormatMutable.GetHashCode() : 0);
        return hashCode;
      }
    }
    */
  }
}