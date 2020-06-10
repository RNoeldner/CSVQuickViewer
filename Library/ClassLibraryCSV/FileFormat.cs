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

namespace CsvTools
{
  using System;
  using System.ComponentModel;
  using System.Xml.Serialization;
  using JetBrains.Annotations;

  /// <summary>
  ///   Setting class for a general file format
  /// </summary>
  [Serializable]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  public class FileFormat : INotifyPropertyChanged, IEquatable<FileFormat>, ICloneable<FileFormat>
#pragma warning restore CS0659
  {
    // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public const string cEscapeCharacterDefault = "";

    private const string c_CommentLineDefault = "";

    private const string c_DelimiterPlaceholderDefault = "";

    private const string c_FieldDelimiterDefault = ",";

    private const string c_FieldQualifierDefault = "\"";

    public const RecordDelimiterType cNewLineDefault = RecordDelimiterType.CRLF;

    private const string c_NewLinePlaceholderDefault = "";

    private const bool c_QualifyOnlyIfNeededDefault = true;

    private const string c_QuotePlaceholderDefault = "";

    private bool m_DuplicateQuotingToEscape = true;

    private bool m_AlternateQuoting;

    private string m_CommentLine = c_CommentLineDefault;

    private string m_DelimiterPlaceholder = c_DelimiterPlaceholderDefault;

    private string m_EscapeCharacter = cEscapeCharacterDefault;

    private char m_EscapeCharacterChar = GetChar(cEscapeCharacterDefault);

    private string m_FieldDelimiter = c_FieldDelimiterDefault;

    private char m_FieldDelimiterChar = GetChar(c_FieldDelimiterDefault);

    private string m_FieldQualifier = c_FieldQualifierDefault;

    private char m_FieldQualifierChar = GetChar(c_FieldQualifierDefault);

    private RecordDelimiterType m_NewLine = cNewLineDefault;

    private string m_NewLinePlaceholder = c_NewLinePlaceholderDefault;

    private bool m_QualifyAlways;

    private bool m_QualifyOnlyIfNeeded = c_QualifyOnlyIfNeededDefault;

    private string m_QuotePlaceholder = c_QuotePlaceholderDefault;

    private ValueFormat m_ValueFormat = new ValueFormat();

    /// <summary>
    ///   Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

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
    ///   Gets or sets the text to indicate that the line is comment line and not contain  data.
    ///   If a line starts with the given text, it is ignored in the data grid.
    /// </summary>
    /// <value>The startup comment line.</value>
    [XmlAttribute]
    [DefaultValue(c_CommentLineDefault)]
    public virtual string CommentLine
    {
      [NotNull]
      get => m_CommentLine;
      [CanBeNull]
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
    public virtual string DelimiterPlaceholder
    {
      [NotNull]
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

    /// <summary>
    ///   Gets or sets the escape character.
    /// </summary>
    /// <value>The escape character.</value>
    [XmlAttribute]
    [DefaultValue(cEscapeCharacterDefault)]
    public virtual string EscapeCharacter
    {
      [NotNull]
      get => m_EscapeCharacter;

      set
      {
        var newVal = (value ?? string.Empty).Trim();
        if (m_EscapeCharacter.Equals(newVal, StringComparison.Ordinal))
          return;
        m_EscapeCharacterChar = GetChar(newVal);
        m_EscapeCharacter = newVal;
        NotifyPropertyChanged(nameof(EscapeCharacter));
      }
    }

    /// <summary>
    ///   Gets the escape character char.
    /// </summary>
    /// <value>The escape character char.</value>
    [XmlIgnore]
    public virtual char EscapeCharacterChar => m_EscapeCharacterChar;

    /// <summary>
    ///   Gets or sets the field delimiter.
    /// </summary>
    /// <value>The field delimiter.</value>
    [XmlAttribute]
    [DefaultValue(c_FieldDelimiterDefault)]
    public virtual string FieldDelimiter
    {
      [NotNull]
      get => m_FieldDelimiter;

      set
      {
        var newVal = (value ?? string.Empty).Trim(StringUtils.Spaces);
        if (m_FieldDelimiter.Equals(newVal, StringComparison.Ordinal))
          return;
        m_FieldDelimiterChar = GetChar(newVal);
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
    public virtual string FieldQualifier
    {
      [NotNull]
      get => m_FieldQualifier;

      set
      {
        var newVal = (value ?? string.Empty).Trim();
        if (m_FieldQualifier.Equals(newVal, StringComparison.Ordinal))
          return;
        m_FieldQualifierChar = GetChar(newVal);
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
      [NotNull]
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
    public virtual string QuotePlaceholder
    {
      [NotNull]
      get => m_QuotePlaceholder;

      set
      {
        var newVal = (value ?? string.Empty).Trim();
        if (m_QuotePlaceholder.Equals(newVal, StringComparison.Ordinal))
          return;
        m_QuotePlaceholder = newVal;
        NotifyPropertyChanged(nameof(QuotePlaceholder));
      }
    }

    /// <summary>
    ///   Gets or sets the value format.
    /// </summary>
    /// <value>The value format.</value>
    [XmlElement]
    public virtual ValueFormat ValueFormat
    {
      [NotNull]
      get => m_ValueFormat;

      set
      {
        var newVal = value ?? new ValueFormat();
        if (m_ValueFormat.Equals(newVal))
          return;
        m_ValueFormat = newVal;
        NotifyPropertyChanged(nameof(ValueFormat));
      }
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value>
    ///   <c>true</c> if field mapping is specified; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///   Used for XML Serialization
    /// </remarks>
    [XmlIgnore]
    public virtual bool ValueFormatSpecified => !m_ValueFormat.Equals(new ValueFormat());

    /// <summary>
    ///   Gets a char from a text
    /// </summary>
    /// <param name="value">The input string.</param>
    /// <returns></returns>
    public static char GetChar(string value)
    {
      if (string.IsNullOrEmpty(value))
        return '\0';
      var punctuation = value.WrittenPunctuationToChar();
      return punctuation != '\0' ? punctuation : value[0];
    }

    /// <summary>
    ///   Gets a char from a text
    /// </summary>
    /// <param name="inputString">The input string.</param>
    /// <returns></returns>
    [NotNull]
    public static string GetDescription(string inputString)
    {
      if (string.IsNullOrEmpty(inputString))
        return string.Empty;

      switch (inputString.WrittenPunctuationToChar())
      {
        case '\t':
          return "Horizontal Tab";

        case ' ':
          return "Space";

        case (char)0xA0:
          return "Non-breaking space";

        case '\\':
          return "Backslash: \\";

        case '/':
          return "Slash: /";

        case ',':
          return "Comma: ,";

        case ';':
          return "Semicolon: ;";
        case ':':
          return "Colon: :";

        case '|':
          return "Pipe: |";

        case '\"':
          return "Quotation marks: \"";

        case '\'':
          return "Apostrophe: \'";

        case '&':
          return "Ampersand: &";

        case '*':
          return "Asterisk: *";

        case '`':
          return "Tick Mark: `";

        case '✓':
          return "Check mark: ✓";

        case '\u001F':
          return "Unit Separator: Char 31";

        case '\u001E':
          return "Record Separator: Char 30";

        case '\u001D':
          return "Group Separator: Char 29";

        case '\u001C':
          return "File Separator: Char 28";

        default:
          return inputString;
      }
    }

    /// <summary>
    ///   Clones this instance into a new instance of the same type
    /// </summary>
    /// <returns></returns>
    public FileFormat Clone()
    {
      var other = new FileFormat();
      CopyTo(other);
      return other;
    }

    /// <summary>
    ///   Copies to.
    /// </summary>
    /// <param name="other">The other.</param>
    public virtual void CopyTo(FileFormat other)
    {
      if (other == null)
        return;

      other.CommentLine = m_CommentLine;
      other.AlternateQuoting = m_AlternateQuoting;
      other.DuplicateQuotingToEscape = m_DuplicateQuotingToEscape;
      other.DelimiterPlaceholder = m_DelimiterPlaceholder;
      other.EscapeCharacter = m_EscapeCharacter;
      other.FieldDelimiter = m_FieldDelimiter;
      other.FieldQualifier = m_FieldQualifier;
      other.NewLine = m_NewLine;
      other.NewLinePlaceholder = m_NewLinePlaceholder;
      other.QualifyOnlyIfNeeded = m_QualifyOnlyIfNeeded;
      other.QualifyAlways = m_QualifyAlways;
      other.QuotePlaceholder = m_QuotePlaceholder;
      ValueFormat.CopyTo(other.ValueFormat);
    }

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise,
    ///   <see langword="false" />.
    /// </returns>
    public bool Equals(FileFormat other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return m_AlternateQuoting == other.AlternateQuoting
             && m_DuplicateQuotingToEscape == other.DuplicateQuotingToEscape
             && string.Equals(m_CommentLine, other.m_CommentLine, StringComparison.Ordinal)
             && string.Equals(m_DelimiterPlaceholder, other.m_DelimiterPlaceholder, StringComparison.Ordinal)
             && string.Equals(m_EscapeCharacter, other.m_EscapeCharacter, StringComparison.Ordinal)
             && m_EscapeCharacterChar == other.m_EscapeCharacterChar
             && string.Equals(m_FieldDelimiter, other.m_FieldDelimiter, StringComparison.Ordinal)
             && m_FieldDelimiterChar == other.m_FieldDelimiterChar
             && string.Equals(m_FieldQualifier, other.m_FieldQualifier, StringComparison.Ordinal)
             && m_FieldQualifierChar == other.m_FieldQualifierChar
             && m_NewLine.Equals(other.m_NewLine)
             && string.Equals(m_NewLinePlaceholder, other.m_NewLinePlaceholder, StringComparison.Ordinal)
             && m_QualifyAlways == other.m_QualifyAlways && m_QualifyOnlyIfNeeded == other.m_QualifyOnlyIfNeeded
             && string.Equals(m_QuotePlaceholder, other.m_QuotePlaceholder, StringComparison.Ordinal)
             && Equals(m_ValueFormat, other.m_ValueFormat);
    }

    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <param name="obj">The object to compare with the current object. </param>
    /// <returns>
    ///   <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.
    /// </returns>
#pragma warning disable 659
    public override bool Equals(object obj) => Equals(obj as FileFormat);
#pragma warning restore 659

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
    /// <summary>Serves as the default hash function. </summary>
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
        hashCode = (hashCode * 397) ^ (m_ValueFormat != null ? m_ValueFormat.GetHashCode() : 0);
        return hashCode;
      }
    }
    */
  }
}