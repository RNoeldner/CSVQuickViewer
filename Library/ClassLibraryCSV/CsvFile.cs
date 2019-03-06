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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace CsvTools
{
  /// <summary>
  ///   Setting file for CSV files, its an implementation of <see cref="BaseSettings" />
  /// </summary>
  [Serializable]
  [DebuggerDisplay("CsvFile: {ID} {m_FullPath} ({m_Column.Count()} Columns)")]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  public class CsvFile : BaseSettings, ICsvFile
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  {
    /// <summary>
    ///   File ending for a setting file
    /// </summary>
    public const string cCsvSettingExtension = ".setting";

    private bool m_AlternateQuoting;
    private bool m_ByteOrderMark = true;
    private int m_CodePageId = 65001;
    private Encoding m_CurrentEncoding = Encoding.UTF8;
    private bool m_DoubleDecode;
    private bool m_NoDelimitedFile;
    private int m_NumWarnings;
    private bool m_TreatUnknowCharaterAsSpace;
    private bool m_WarnDelimiterInValue;
    private bool m_WarnEmptyTailingColumns = true;
    private bool m_WarnLineFeed;
    private bool m_WarnNbsp = true;
    private bool m_TreatLFAsSpace = false;
    private bool m_TryToSolveMoreColumns = false;
    private bool m_AllowRowCombining = false;
    private bool m_WarnQuotes;
    private bool m_WarnQuotesInQuotes = true;
    private bool m_WarnUnknowCharacter = true;

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
    {
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
    public static bool UnknowCharaterReplacementSpecified => false;

    /// <summary>
    ///   Gets or sets a value indicating whether the byte order mark should be written in Unicode files.
    /// </summary>
    /// <value><c>true</c> write byte order mark; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool AlternateQuoting
    {
      get => m_AlternateQuoting;
      set => m_AlternateQuoting = value;
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

      set => m_ByteOrderMark = value;
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
        if (m_CodePageId.Equals(value)) return;
        m_CodePageId = value;
        NotifyPropertyChanged(nameof(CodePageId));
      }
    }

    /// <summary>
    ///   Gets current encoding.
    /// </summary>
    /// <value>The current encoding.</value>
    [XmlIgnore]
    public virtual Encoding CurrentEncoding
    {
      get
      {
        Contract.Ensures(Contract.Result<Encoding>() != null);
        return m_CurrentEncoding;
      }

      set => m_CurrentEncoding = value ?? Encoding.UTF8;
    }

    /// <summary>
    ///   Gets or sets a value indicating whether we have a text that was double encoded and needs a double decoding.
    /// </summary>
    /// <value><c>true</c> if while reading double decode the file otherwise <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool DoubleDecode
    {
      get => m_DoubleDecode;
      set => m_DoubleDecode = value;
    }

    /// <summary>
    ///   Gets or sets a value indicating whether a file is most likely not a delimited file
    /// </summary>
    /// <value>
    ///   <c>true</c> if the file is assumed to be a non delimited file; otherwise, <c>false</c>.
    /// </value>
    [XmlIgnore]
    public virtual bool NoDelimitedFile
    {
      get => m_NoDelimitedFile;

      set
      {
        if (m_NoDelimitedFile.Equals(value)) return;
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
        if (m_NumWarnings.Equals(value)) return;
        m_NumWarnings = value;
        NotifyPropertyChanged(nameof(NumWarnings));
      }
    }

    /// <summary>
    ///   Gets or sets a value indicating whether to replace unknown charters.
    /// </summary>
    /// <value><c>true</c> if unknown character should be replaced; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool TreatUnknowCharaterAsSpace
    {
      get => m_TreatUnknowCharaterAsSpace;

      set
      {
        if (m_TreatUnknowCharaterAsSpace.Equals(value)) return;
        m_TreatUnknowCharaterAsSpace = value;
        NotifyPropertyChanged(nameof(TreatUnknowCharaterAsSpace));
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
        if (m_WarnDelimiterInValue.Equals(value)) return;
        m_WarnDelimiterInValue = value;
        NotifyPropertyChanged(nameof(WarnDelimiterInValue));
      }
    }

    /// <summary>
    ///   Gets or sets a value indicating whether to warn empty tailing columns.
    /// </summary>
    /// <value><c>true</c> if [warn empty tailing columns]; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(true)]
    public virtual bool WarnEmptyTailingColumns
    {
      get => m_WarnEmptyTailingColumns;

      set
      {
        if (m_WarnEmptyTailingColumns.Equals(value)) return;
        m_WarnEmptyTailingColumns = value;
        NotifyPropertyChanged(nameof(WarnEmptyTailingColumns));
      }
    }

    /// <summary>
    ///   Gets or sets a value indicating whether to warn unknown character.
    /// </summary>
    /// <value>
    ///   <c>true</c> if unknown character should issue a warning; otherwise, <c>false</c>.
    /// </value>
    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool WarnLineFeed
    {
      get => m_WarnLineFeed;

      set
      {
        if (m_WarnLineFeed.Equals(value)) return;
        m_WarnLineFeed = value;
        NotifyPropertyChanged(nameof(WarnLineFeed));
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to treat a single LF as space
    /// </summary>
    /// <value>
    ///   <c>true</c> if LF should be treated as space; otherwise, <c>false</c>.
    /// </value>
    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool TreatLFAsSpace
    {
      get => m_TreatLFAsSpace;

      set
      {
        if (m_TreatLFAsSpace.Equals(value)) return;
        m_TreatLFAsSpace = value;
        NotifyPropertyChanged(nameof(TreatLFAsSpace));
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the reader should try to solve more columns.
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
        if (m_TryToSolveMoreColumns.Equals(value)) return;
        m_TryToSolveMoreColumns = value;
        NotifyPropertyChanged(nameof(TryToSolveMoreColumns));
      }
    }

    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool AllowRowCombining
    {
      get => m_AllowRowCombining;

      set
      {
        if (m_AllowRowCombining.Equals(value)) return;
        m_AllowRowCombining = value;
        NotifyPropertyChanged(nameof(AllowRowCombining));
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
        if (m_WarnNbsp.Equals(value)) return;
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
        if (m_WarnQuotes.Equals(value)) return;
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
        if (m_WarnQuotesInQuotes.Equals(value)) return;
        m_WarnQuotesInQuotes = value;
        NotifyPropertyChanged(nameof(WarnQuotesInQuotes));
      }
    }

    /// <summary>
    ///   Gets or sets a value indicating whether to warn unknown character.
    /// </summary>
    /// <value>
    ///   <c>true</c> if unknown character should issue a warning; otherwise, <c>false</c>.
    /// </value>
    [XmlAttribute]
    [DefaultValue(true)]
    public virtual bool WarnUnknowCharater
    {
      get => m_WarnUnknowCharacter;

      set
      {
        if (m_WarnUnknowCharacter.Equals(value)) return;
        m_WarnUnknowCharacter = value;
        NotifyPropertyChanged(nameof(WarnUnknowCharater));
      }
    }

    /// <summary>
    ///   Clones this instance.
    /// </summary>
    /// <returns></returns>
    public override IFileSetting Clone()
    {
      Contract.Ensures(Contract.Result<IFileSetting>() != null);
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
      if (other == null)
        return;
      base.CopyTo((BaseSettings)other);

      if (!(other is ICsvFile csv)) return;
      csv.ByteOrderMark = m_ByteOrderMark;
      csv.AlternateQuoting = m_AlternateQuoting;
      csv.DoubleDecode = m_DoubleDecode;
      csv.WarnQuotes = m_WarnQuotes;
      csv.WarnDelimiterInValue = m_WarnDelimiterInValue;
      csv.WarnEmptyTailingColumns = m_WarnEmptyTailingColumns;
      csv.WarnQuotesInQuotes = m_WarnQuotesInQuotes;
      csv.WarnUnknowCharater = m_WarnUnknowCharacter;
      csv.WarnLineFeed = m_WarnLineFeed;
      csv.WarnNBSP = m_WarnNbsp;
      csv.TreatLFAsSpace = m_TreatLFAsSpace;
      csv.TryToSolveMoreColumns = m_TryToSolveMoreColumns;
      csv.AllowRowCombining = m_AllowRowCombining;

      csv.TreatUnknowCharaterAsSpace = m_TreatUnknowCharaterAsSpace;
      csv.CodePageId = m_CodePageId;
      csv.NumWarnings = m_NumWarnings;
      csv.CurrentEncoding = m_CurrentEncoding;
      csv.NoDelimitedFile = m_NoDelimitedFile;
    }

    public override bool Equals(IFileSetting other)
    {
      return Equals(other as ICsvFile);
    }

    /// <summary>
    ///   Gets the file reader.
    /// </summary>
    /// <returns></returns>
    public override IFileReader GetFileReader()
    {
      return new CsvFileReader(this);
    }

    /// <summary>
    ///   Gets the file writer.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public override IFileWriter GetFileWriter(CancellationToken cancellationToken)
    {
      return new CsvFileWriter(this, cancellationToken);
    }

    /*
    /// <summary>Serves as the default hash function. </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = GetBaseHashCode();
        hashCode = (hashCode * 397) ^ m_AlternateQuoting.GetHashCode();
        hashCode = (hashCode * 397) ^ m_ByteOrderMark.GetHashCode();
        hashCode = (hashCode * 397) ^ m_CodePageId.GetHashCode();
        hashCode = (hashCode * 397) ^ m_CurrentEncoding.GetHashCode();
        hashCode = (hashCode * 397) ^ m_DoubleDecode.GetHashCode();
        hashCode = (hashCode * 397) ^ m_NoDelimitedFile.GetHashCode();
        hashCode = (hashCode * 397) ^ m_NumWarnings.GetHashCode();
        hashCode = (hashCode * 397) ^ m_TreatUnknowCharaterAsSpace.GetHashCode();
        hashCode = (hashCode * 397) ^ m_WarnDelimiterInValue.GetHashCode();
        hashCode = (hashCode * 397) ^ m_WarnEmptyTailingColumns.GetHashCode();
        hashCode = (hashCode * 397) ^ m_WarnLineFeed.GetHashCode();
        hashCode = (hashCode * 397) ^ m_WarnNbsp.GetHashCode();
        hashCode = (hashCode * 397) ^ m_WarnQuotes.GetHashCode();
        hashCode = (hashCode * 397) ^ m_WarnQuotesInQuotes.GetHashCode();
        hashCode = (hashCode * 397) ^ m_WarnUnknowCharacter.GetHashCode();
        return hashCode;
      }
    }
    */

    public virtual bool Equals(ICsvFile other)
    {
      if (other is null) return false;
      if (ReferenceEquals(this, other)) return true;
      return m_AlternateQuoting == other.AlternateQuoting &&
             m_ByteOrderMark == other.ByteOrderMark && m_CodePageId == other.CodePageId &&
             Equals(m_CurrentEncoding, other.CurrentEncoding) && m_DoubleDecode == other.DoubleDecode &&
             m_NoDelimitedFile == other.NoDelimitedFile && m_NumWarnings == other.NumWarnings &&
             m_TreatUnknowCharaterAsSpace == other.TreatUnknowCharaterAsSpace &&
             m_WarnDelimiterInValue == other.WarnDelimiterInValue &&
             m_WarnEmptyTailingColumns == other.WarnEmptyTailingColumns && m_WarnLineFeed == other.WarnLineFeed &&
             m_TryToSolveMoreColumns == other.TryToSolveMoreColumns &&
             m_AllowRowCombining == other.AllowRowCombining &&
             m_TreatLFAsSpace == other.TreatLFAsSpace &&
             m_WarnNbsp == other.WarnNBSP && m_WarnQuotes == other.WarnQuotes &&
             m_WarnQuotesInQuotes == other.WarnQuotesInQuotes &&
             m_WarnUnknowCharacter == other.WarnUnknowCharater &&
             Equals(other as BaseSettings);
    }

    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <param name="obj">The object to compare with the current object. </param>
    /// <returns>
    ///   <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object obj)
    {
      if (obj is null) return false;
      if (ReferenceEquals(this, obj)) return true;
      return (obj is ICsvFile typed) && Equals(typed);
    }
  }
}