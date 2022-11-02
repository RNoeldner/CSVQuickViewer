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
using System;
using System.ComponentModel;
using System.Xml.Serialization;
// ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract

namespace CsvTools
{
  /// <summary>
  ///   This is a helper class to edit and to serialize into XML
  /// </summary>
  [Serializable]
  public sealed class ValueFormatMut : NotifyPropertyChangedBase, IEquatable<ValueFormatMut>
  {
    private DataTypeEnum m_DataType;
    private string m_DateFormat;
    private string m_DateSeparator;
    private string m_DecimalSeparator;
    private string m_DisplayNullAs;
    private string m_False;
    private string m_FileOutPutPlaceholder;
    private string m_GroupSeparator;
    private string m_NumberFormat;
    private bool m_Overwrite;
    private int m_Part;
    private string m_PartSplitter;
    private bool m_PartToEnd;
    private string m_ReadFolder;
    private string m_RegexReplacement;
    private string m_RegexSearchPattern;
    private string m_TimeSeparator;
    private string m_True;
    private string m_WriteFolder;
    /// <summary>
    ///   Initializes a new instance of the <see cref="CsvTools.ValueFormatMut" /> class.
    /// </summary>
    [Obsolete("Only needed for XML Serialization")]
    public ValueFormatMut() : this(DataTypeEnum.String)
    {
    }

    public ValueFormatMut(ValueFormat source) : this(source.DataType, source.DateFormat, source.DateSeparator,
      source.TimeSeparator, source.NumberFormat, source.GroupSeparator, source.DecimalSeparator, source.True,
      source.False, source.DisplayNullAs, source.Part, source.PartSplitter, source.PartToEnd, source.RegexSearchPattern,
      source.RegexReplacement, source.ReadFolder, source.WriteFolder, source.FileOutPutPlaceholder, source.Overwrite)
    {
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="CsvTools.ValueFormatMut" /> class.
    /// </summary>
    /// <param name="dataType">Type of the data.</param>
    /// <param name="dateFormat">The date format.</param>
    /// <param name="dateSeparator">The date separator (usually /).</param>
    /// <param name="timeSeparator">The time separator.</param>
    /// <param name="numberFormat">The number format.</param>
    /// <param name="groupSeparator">The group separator.</param>
    /// <param name="decimalSeparator">The decimal separator.</param>
    /// <param name="asTrue">Text to be regarded as true.</param>
    /// <param name="asFalse">Text to be regarded as false.</param>
    /// <param name="displayNullAs">While writing display a null values as this</param>
    /// <param name="part">The part number in case of splitting.</param>
    /// <param name="partSplitter">The part splitter.</param>
    /// <param name="partToEnd">
    ///   if set to <c>true</c> the part will contain everything from the start of the part to the end.
    /// </param>
    /// <param name="regexSearchPattern">The regex search pattern.</param>
    /// <param name="regexReplacement">The regex replacement.</param>
    /// <param name="readFolder">The read folder.</param>
    /// <param name="writeFolder">The write folder.</param>
    /// <param name="fileOutPutPlaceholder">The file out put placeholder.</param>
    /// <param name="overwrite">if set to <c>true</c> we should overwrite.</param>
    /// <exception cref="CsvTools.FileReaderException">
    ///   Decimal and Group separator must be different
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    ///   dateFormat or dateSeparator or decimalSeparator or groupSeparator or displayNullAs or
    ///   asFalse or numberFormat or timeSeparator or asTrue or partSplitter or regexSearchPattern
    ///   or regexReplacement or readFolder or writeFolder or fileOutPutPlaceholder
    /// </exception>
    public ValueFormatMut(
      in DataTypeEnum dataType = DataTypeEnum.String,
      in string dateFormat = ValueFormat.cDateFormatDefault,
      in string dateSeparator = ValueFormat.cDateSeparatorDefault,
      in string timeSeparator = ValueFormat.cTimeSeparatorDefault,
      in string numberFormat = ValueFormat.cNumberFormatDefault,
      in string groupSeparator = ValueFormat.cGroupSeparatorDefault,
      in string decimalSeparator = ValueFormat.cDecimalSeparatorDefault,
      in string asTrue = ValueFormat.cTrueDefault,
      in string asFalse = ValueFormat.cFalseDefault,
      in string displayNullAs = "",
      int part = ValueFormat.cPartDefault,
      in string partSplitter = ValueFormat.cPartSplitterDefault,
      bool partToEnd = ValueFormat.cPartToEndDefault,
      in string regexSearchPattern = "",
      in string regexReplacement = "",
      in string readFolder = "",
      in string writeFolder = "",
      in string fileOutPutPlaceholder = "",
      in bool overwrite = true)
    {
      m_DecimalSeparator = (decimalSeparator ?? ValueFormat.cDecimalSeparatorDefault).WrittenPunctuation();
      m_GroupSeparator = (groupSeparator ?? ValueFormat.cGroupSeparatorDefault).WrittenPunctuation();
      if (!string.IsNullOrEmpty(m_DecimalSeparator) && m_DecimalSeparator.Equals(m_GroupSeparator))
        throw new FileReaderException("Decimal and Group separator must be different");
      m_DataType = dataType;
      m_DateFormat = dateFormat ?? ValueFormat.cDateFormatDefault;
      m_DateSeparator = (dateSeparator ?? ValueFormat.cDateSeparatorDefault).WrittenPunctuation();
      m_TimeSeparator = (timeSeparator ?? ValueFormat.cTimeSeparatorDefault).WrittenPunctuation();

      m_DisplayNullAs = displayNullAs ?? string.Empty;
      m_NumberFormat = numberFormat ?? ValueFormat.cNumberFormatDefault;

      m_True = asTrue ?? ValueFormat.cTrueDefault;
      m_False = asFalse ?? ValueFormat.cFalseDefault;
      m_Part = part;
      m_PartSplitter = (partSplitter ?? ValueFormat.cPartSplitterDefault).WrittenPunctuation();
      m_PartToEnd = partToEnd;
      m_RegexSearchPattern = regexSearchPattern ?? string.Empty;
      m_RegexReplacement = regexReplacement ?? string.Empty;
      m_ReadFolder = readFolder ?? string.Empty;
      m_WriteFolder = writeFolder ?? string.Empty;
      m_FileOutPutPlaceholder = fileOutPutPlaceholder ?? string.Empty;
      m_Overwrite = overwrite;
    }

    [XmlAttribute]
    [DefaultValue(DataTypeEnum.String)]
    public DataTypeEnum DataType
    {
      get => m_DataType;
      set => SetField(ref m_DataType, value);
    }

    [XmlElement]
    [DefaultValue(ValueFormat.cDateFormatDefault)]
    public string DateFormat
    {
      get => m_DateFormat;
      set => SetField(ref m_DateFormat, value, StringComparison.Ordinal);
    }

    [XmlElement]
    [DefaultValue(ValueFormat.cDateSeparatorDefault)]
    public string DateSeparator
    {
      get => m_DateSeparator;
      set => SetField(ref m_DateSeparator, (value ?? string.Empty).WrittenPunctuation(), StringComparison.Ordinal);
    }

    [XmlElement]
    [DefaultValue(ValueFormat.cDecimalSeparatorDefault)]
    public string DecimalSeparator
    {
      get => m_DecimalSeparator;
      set
      {
        if (!SetField(ref m_DecimalSeparator, (value ?? string.Empty).WrittenPunctuation(),
              StringComparison.Ordinal)) return;
        if (m_GroupSeparator.Equals(m_DecimalSeparator))
          SetField(ref m_GroupSeparator, "", StringComparison.Ordinal, false, nameof(GroupSeparator));
      }
    }

    [XmlAttribute]
    [DefaultValue("")]
    public string DisplayNullAs
    {
      get => m_DisplayNullAs;
      set => SetField(ref m_DisplayNullAs, value, StringComparison.Ordinal);
    }

    [XmlElement]
    [DefaultValue(ValueFormat.cFalseDefault)]
    public string False
    {
      get => m_False;
      set => SetField(ref m_False, value, StringComparison.OrdinalIgnoreCase);
    }

    [XmlAttribute]
    [DefaultValue("")]
    public string FileOutPutPlaceholder
    {
      get => m_FileOutPutPlaceholder;
      set => SetField(ref m_FileOutPutPlaceholder, value, StringComparison.Ordinal);
    }

    [XmlElement]
    [DefaultValue(ValueFormat.cGroupSeparatorDefault)]
    public string GroupSeparator
    {
      get => m_GroupSeparator;
      set
      {
        var oldGroup = m_GroupSeparator;
        if (SetField(ref m_GroupSeparator, (value ?? string.Empty).WrittenPunctuation(), StringComparison.Ordinal))
        {
          if (m_DecimalSeparator.Equals(m_DecimalSeparator))
            SetField(ref m_DecimalSeparator, oldGroup, StringComparison.Ordinal, false, nameof(DecimalSeparator));
        }
      }
    }

    [XmlElement]
    [DefaultValue(ValueFormat.cNumberFormatDefault)]
    public string NumberFormat
    {
      get => m_NumberFormat;
      set => SetField(ref m_NumberFormat, value, StringComparison.Ordinal);
    }

    [XmlAttribute]
    [DefaultValue(true)]
    public bool Overwrite
    {
      get => m_Overwrite;
      set => SetField(ref m_Overwrite, value);
    }

    [XmlAttribute]
    [DefaultValue(ValueFormat.cPartDefault)]
    public int Part
    {
      get => m_Part;
      set => SetField(ref m_Part, value);
    }

#if NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif

    [XmlAttribute]
    [DefaultValue(ValueFormat.cPartSplitterDefault)]
    public string PartSplitter
    {
      get => m_PartSplitter;
      set => SetField(ref m_PartSplitter, (value ?? string.Empty).WrittenPunctuation(), StringComparison.Ordinal);
    }

    [XmlAttribute]
    [DefaultValue(ValueFormat.cPartToEndDefault)]
    public bool PartToEnd
    {
      get => m_PartToEnd;
      set => SetField(ref m_PartToEnd, value);
    }

    [XmlAttribute]
    [DefaultValue("")]
    public string ReadFolder
    {
      get => m_ReadFolder;
      set => SetField(ref m_ReadFolder, value, StringComparison.Ordinal);
    }

    [XmlAttribute]
    [DefaultValue("")]
    public string RegexReplacement
    {
      get => m_RegexReplacement;
      set => SetField(ref m_RegexReplacement, value, StringComparison.Ordinal);
    }

    [XmlAttribute]
    [DefaultValue("")]
    public string RegexSearchPattern
    {
      get => m_RegexSearchPattern;
      set => SetField(ref m_RegexSearchPattern, value, StringComparison.Ordinal);
    }

    [XmlElement]
    [DefaultValue(ValueFormat.cTimeSeparatorDefault)]
    public string TimeSeparator
    {
      get => m_TimeSeparator;
      set => SetField(ref m_TimeSeparator, (value ?? string.Empty).WrittenPunctuation(), StringComparison.Ordinal);
    }

    [XmlElement]
    [DefaultValue(ValueFormat.cTrueDefault)]
    public string True
    {
      get => m_True;
      set => SetField(ref m_True, value, StringComparison.InvariantCulture);
    }
    [XmlAttribute]
    [DefaultValue("")]
    public string WriteFolder
    {
      get => m_WriteFolder;
      set => SetField(ref m_WriteFolder, value, StringComparison.Ordinal);
    }

    /// <summary>
    ///   On Mutable classes prefer CopyFrom to CopyTo, overwrites the properties from the
    ///   properties in the provided class
    /// </summary>
    /// <param name="other"></param>
    public void CopyFrom(ValueFormat? other)
    {
      if (other is null)
        return;
      DataType = other.DataType;
      DateFormat = other.DateFormat;
      DateSeparator = other.DateSeparator;
      TimeSeparator = other.TimeSeparator;
      NumberFormat = other.NumberFormat;
      GroupSeparator = other.GroupSeparator;
      DecimalSeparator = other.DecimalSeparator;
      True = other.True;
      False = other.False;
      DisplayNullAs = other.DisplayNullAs;
      Part = other.Part;
      PartSplitter = other.PartSplitter;
      PartToEnd = other.PartToEnd;
      RegexSearchPattern = other.RegexSearchPattern;
      RegexReplacement = other.RegexReplacement;
      ReadFolder = other.ReadFolder;
      WriteFolder = other.WriteFolder;
      Overwrite = other.Overwrite;
      FileOutPutPlaceholder = other.FileOutPutPlaceholder;
    }

    public bool Equals(ValueFormatMut? other)
    {
      if (other == null) return false;
      if (ReferenceEquals(this, other)) return true;
      return DataType == other.DataType
             && DateFormat == other.DateFormat
             && DateSeparator == other.DateSeparator
             && DecimalSeparator == other.DecimalSeparator
             && DisplayNullAs == other.DisplayNullAs
             && False == other.False
             && GroupSeparator == other.GroupSeparator
             && NumberFormat == other.NumberFormat
             && Part == other.Part
             && PartSplitter == other.PartSplitter
             && PartToEnd == other.PartToEnd
             && TimeSeparator == other.TimeSeparator
             && True == other.True
             && RegexSearchPattern == other.RegexSearchPattern
             && RegexReplacement == other.RegexReplacement
             && ReadFolder == other.ReadFolder
             && WriteFolder == other.WriteFolder
             && FileOutPutPlaceholder == other.FileOutPutPlaceholder
             && Overwrite == other.Overwrite;
    }

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is ValueFormatMut other && Equals(other);

    /// <summary>
    /// Returns  an immutable ValueFormat if the source column was immutable the very same is returned, not copy is created
    /// </summary>
    /// <returns>and immutable column</returns>
    public ValueFormat ToImmutable()
      => new ValueFormat(DataType, DateFormat,
        DateSeparator, TimeSeparator, NumberFormat, GroupSeparator,
        DecimalSeparator, True, False, DisplayNullAs,
        Part, PartSplitter, PartToEnd, RegexSearchPattern,
        RegexReplacement, ReadFolder, WriteFolder, FileOutPutPlaceholder, Overwrite);
  }
}