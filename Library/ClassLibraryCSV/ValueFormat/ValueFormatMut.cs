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
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

// ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract

namespace CsvTools
{
  /// <summary>
  ///   This is a helper class to edit and to serialize into XML
  /// </summary>
  [Serializable]
  public sealed class ValueFormatMut : ObservableObject, IEquatable<ValueFormatMut>
  {
    private DataTypeEnum m_DataType;
    private string m_DateFormat;
    private char m_DateSeparator;
    private char m_DecimalSeparator;
    private string m_DisplayNullAs;
    private string m_False;
    private string m_FileOutPutPlaceholder;
    private char m_GroupSeparator;
    private string m_NumberFormat;
    private bool m_Overwrite;
    private int m_Part;
    private char m_PartSplitter;
    private bool m_PartToEnd;
    private string m_ReadFolder;
    private string m_RegexReplacement;
    private string m_RegexSearchPattern;
    private char m_TimeSeparator;
    private string m_True;
    private string m_WriteFolder;

    /// <summary>
    ///   Initializes a new instance of the <see cref="CsvTools.ValueFormatMut" /> class.
    /// </summary>
    [Obsolete("Only needed for XML Serialization")]
    public ValueFormatMut() : this(DataTypeEnum.String)
    {
    }

    public ValueFormatMut(ValueFormat source) : this(source.DataType, source.DateFormat, source.DateSeparator.Text(),
      source.TimeSeparator.Text(), source.NumberFormat, source.GroupSeparator.Text(), source.DecimalSeparator.Text(), source.True,
      source.False, source.DisplayNullAs, source.Part, source.PartSplitter.Text(), source.PartToEnd, source.RegexSearchPattern,
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
      in bool overwrite = ValueFormat.cOverwriteDefault)
    {
      m_DecimalSeparator = (decimalSeparator ?? ValueFormat.cDecimalSeparatorDefault).FromText();
      m_GroupSeparator = (groupSeparator ?? ValueFormat.cGroupSeparatorDefault).FromText();
      if (m_DecimalSeparator != char.MinValue && m_DecimalSeparator.Equals(m_GroupSeparator))
        throw new FileReaderException("Decimal and Group separator must be different");
      m_DataType = dataType;
      m_DateFormat = dateFormat ?? ValueFormat.cDateFormatDefault;
      m_DateSeparator = (dateSeparator ?? ValueFormat.cDateSeparatorDefault).FromText();
      m_TimeSeparator = (timeSeparator ?? ValueFormat.cTimeSeparatorDefault).FromText();

      m_DisplayNullAs = displayNullAs ?? string.Empty;
      m_NumberFormat = numberFormat ?? ValueFormat.cNumberFormatDefault;

      m_True = asTrue ?? ValueFormat.cTrueDefault;
      m_False = asFalse ?? ValueFormat.cFalseDefault;
      m_Part = part;
      m_PartSplitter = (partSplitter ?? ValueFormat.cPartSplitterDefault).FromText();
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
      set => SetProperty(ref m_DataType, value);
    }

    [XmlElement]
    [DefaultValue(ValueFormat.cDateFormatDefault)]
    public string DateFormat
    {
      get => m_DateFormat;
      set => SetProperty(ref m_DateFormat, value);
    }

    [XmlElement]
    [DefaultValue(ValueFormat.cDateSeparatorDefault)]
    public string DateSeparator
    {
      get => m_DateSeparator.Text();
      set
      {
        if (m_DateSeparator.SetText(value))
          NotifyPropertyChanged();
      }
    }

    [XmlElement]
    [DefaultValue(ValueFormat.cDecimalSeparatorDefault)]
    public string DecimalSeparator
    {
      get => m_DecimalSeparator.Text();
      set
      {
        if (!m_DecimalSeparator.SetText(value))
          return;
        NotifyPropertyChanged();
        if (m_GroupSeparator.Equals(m_DecimalSeparator))
          GroupSeparator = string.Empty;
      }
    }

    [XmlAttribute]
    [DefaultValue("")]
    public string DisplayNullAs
    {
      get => m_DisplayNullAs;
      set => SetProperty(ref m_DisplayNullAs, value);
    }

    [XmlElement]
    [DefaultValue(ValueFormat.cFalseDefault)]
    public string False
    {
      get => m_False;
      set => SetProperty(ref m_False, value);
    }

    [XmlAttribute]
    [DefaultValue("")]
    public string FileOutPutPlaceholder
    {
      get => m_FileOutPutPlaceholder;
      set => SetProperty(ref m_FileOutPutPlaceholder, value);
    }

    [XmlElement]
    [DefaultValue(ValueFormat.cGroupSeparatorDefault)]
    public string GroupSeparator
    {
      get => m_GroupSeparator.Text();
      set
      {
        var oldGroup = m_GroupSeparator;
        if (m_GroupSeparator.SetText(value))
        {
          NotifyPropertyChanged();
          if (m_GroupSeparator.Equals(m_DecimalSeparator))
          {
            m_DecimalSeparator= oldGroup;
            NotifyPropertyChanged(nameof(DecimalSeparator));
          }
        }
      }
    }

    [XmlElement]
    [DefaultValue(ValueFormat.cNumberFormatDefault)]
    public string NumberFormat
    {
      get => m_NumberFormat;
      set => SetProperty(ref m_NumberFormat, value);
    }

    [XmlAttribute]
    [DefaultValue(ValueFormat.cOverwriteDefault)]
    public bool Overwrite
    {
      get => m_Overwrite;
      set => SetProperty(ref m_Overwrite, value);
    }

    [XmlAttribute]
    [DefaultValue(ValueFormat.cPartDefault)]
    public int Part
    {
      get => m_Part;
      set => SetProperty(ref m_Part, value);
    }

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif

    [XmlAttribute]
    [DefaultValue(ValueFormat.cPartSplitterDefault)]
    public string PartSplitter
    {
      get => m_PartSplitter.Text();
      set
      {
        if (m_PartSplitter.SetText(value))
          NotifyPropertyChanged();
      }
    }

    [XmlAttribute]
    [DefaultValue(ValueFormat.cPartToEndDefault)]
    public bool PartToEnd
    {
      get => m_PartToEnd;
      set => SetProperty(ref m_PartToEnd, value);
    }

    [XmlAttribute]
    [DefaultValue("")]
    public string ReadFolder
    {
      get => m_ReadFolder;
      set => SetProperty(ref m_ReadFolder, value);
    }

    [XmlAttribute]
    [DefaultValue("")]
    public string RegexReplacement
    {
      get => m_RegexReplacement;
      set => SetProperty(ref m_RegexReplacement, value);
    }

    [XmlAttribute]
    [DefaultValue("")]
    public string RegexSearchPattern
    {
      get => m_RegexSearchPattern;
      set => SetProperty(ref m_RegexSearchPattern, value);
    }

    [XmlElement]
    [DefaultValue(ValueFormat.cTimeSeparatorDefault)]
    public string TimeSeparator
    {
      get => m_TimeSeparator.Text();
      set
      {
        if (m_TimeSeparator.SetText(value))
          NotifyPropertyChanged();
      }
    }

    [XmlElement]
    [DefaultValue(ValueFormat.cTrueDefault)]
    public string True
    {
      get => m_True;
      set => SetProperty(ref m_True, value);
    }

    [XmlAttribute]
    [DefaultValue("")]
    public string WriteFolder
    {
      get => m_WriteFolder;
      set => SetProperty(ref m_WriteFolder, value);
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
      DateSeparator = other.DateSeparator.Text();
      TimeSeparator = other.TimeSeparator.Text();
      NumberFormat = other.NumberFormat;
      GroupSeparator = other.GroupSeparator.Text();
      DecimalSeparator = other.DecimalSeparator.Text();
      True = other.True;
      False = other.False;
      DisplayNullAs = other.DisplayNullAs;
      Part = other.Part;
      PartSplitter = other.PartSplitter.Text();
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
      if (other is null) return false;

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

#pragma warning disable CS0659
    public override bool Equals(object? obj) =>
#pragma warning restore CS0659
      obj is ValueFormatMut other && Equals(other);

    public override int GetHashCode()
    {
      var hashCode = -373284191;
      hashCode=hashCode*-1521134295+DataType.GetHashCode();
      hashCode=hashCode*-1521134295+EqualityComparer<string>.Default.GetHashCode(DateFormat);
      hashCode=hashCode*-1521134295+EqualityComparer<string>.Default.GetHashCode(DateSeparator);
      hashCode=hashCode*-1521134295+EqualityComparer<string>.Default.GetHashCode(DecimalSeparator);
      hashCode=hashCode*-1521134295+EqualityComparer<string>.Default.GetHashCode(DisplayNullAs);
      hashCode=hashCode*-1521134295+EqualityComparer<string>.Default.GetHashCode(False);
      hashCode=hashCode*-1521134295+EqualityComparer<string>.Default.GetHashCode(FileOutPutPlaceholder);
      hashCode=hashCode*-1521134295+EqualityComparer<string>.Default.GetHashCode(GroupSeparator);
      hashCode=hashCode*-1521134295+EqualityComparer<string>.Default.GetHashCode(NumberFormat);
      hashCode=hashCode*-1521134295+Overwrite.GetHashCode();
      hashCode=hashCode*-1521134295+Part.GetHashCode();
      hashCode=hashCode*-1521134295+EqualityComparer<string>.Default.GetHashCode(PartSplitter);
      hashCode=hashCode*-1521134295+PartToEnd.GetHashCode();
      hashCode=hashCode*-1521134295+EqualityComparer<string>.Default.GetHashCode(ReadFolder);
      hashCode=hashCode*-1521134295+EqualityComparer<string>.Default.GetHashCode(RegexReplacement);
      hashCode=hashCode*-1521134295+EqualityComparer<string>.Default.GetHashCode(RegexSearchPattern);
      hashCode=hashCode*-1521134295+EqualityComparer<string>.Default.GetHashCode(TimeSeparator);
      hashCode=hashCode*-1521134295+EqualityComparer<string>.Default.GetHashCode(True);
      hashCode=hashCode*-1521134295+EqualityComparer<string>.Default.GetHashCode(WriteFolder);
      return hashCode;
    }


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