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
  /// <inheritdoc cref="CsvTools.IValueFormat" />
  /// <summary>
  ///   Setting for a value format
  /// </summary>
  [Serializable]
  public sealed class ValueFormatMutable : NotifyPropertyChangedBase, IValueFormat
  {
    private DataTypeEnum m_DataType;
    private string m_DateFormat;
    private string m_DateSeparator;
    private string m_DecimalSeparator;
    private string m_DisplayNullAs;
    private string m_False;
    private string m_GroupSeparator;
    private string m_NumberFormat;
    private int m_Part;
    private string m_PartSplitter;
    private bool m_PartToEnd;
    private string m_TimeSeparator;
    private string m_True;
    private string m_RegexSearchPattern;
    private string m_RegexReplacement;
    private string m_ReadFolder;
    private string m_WriteFolder;
    private string m_FileOutPutPlaceholder;
    private bool m_Overwrite;


    public ValueFormatMutable() : this(
      DataTypeEnum.String,
      ValueFormatExtension.cDateFormatDefault,
      ValueFormatExtension.cDateSeparatorDefault,
      ValueFormatExtension.cTimeSeparatorDefault,
      ValueFormatExtension.cNumberFormatDefault,
      ValueFormatExtension.cGroupSeparatorDefault,
      ValueFormatExtension.cDecimalSeparatorDefault,
      ValueFormatExtension.cTrueDefault,
      ValueFormatExtension.cFalseDefault,
      string.Empty,
      ValueFormatExtension.cPartDefault,
      ValueFormatExtension.cPartSplitterDefault,
      ValueFormatExtension.cPartToEndDefault,
      string.Empty,
      string.Empty,
      string.Empty,
      string.Empty,
      string.Empty,
      true)
    {
    }

    public ValueFormatMutable(
      in DataTypeEnum dataType,
      in string dateFormat,
      in string dateSeparator,
      in string timeSeparator,
      in string numberFormat,
      in string groupSeparator,
      in string decimalSeparator,
      in string asTrue,
      in string asFalse,
      in string displayNullAs,
      int part,
      in string partSplitter,
      bool partToEnd,
      in string regexSearchPattern,
      in string regexReplacement,
      in string readFolder,
      in string writeFolder,
      in string fileOutPutPlaceholder,
      in bool overwrite)
    {
      if (!string.IsNullOrEmpty(decimalSeparator) && decimalSeparator.Equals(groupSeparator))
        throw new FileReaderException("Decimal and Group separator must be different");
      m_DataType = dataType;
      m_DateFormat = dateFormat ?? throw new ArgumentNullException(nameof(dateFormat));
      m_DateSeparator = (dateSeparator ?? throw new ArgumentNullException(nameof(dateSeparator))).WrittenPunctuation();
      m_DecimalSeparator = (decimalSeparator ?? throw new ArgumentNullException(nameof(decimalSeparator)))
        .WrittenPunctuation();
      m_GroupSeparator =
        (groupSeparator ?? throw new ArgumentNullException(nameof(groupSeparator))).WrittenPunctuation();
      m_DisplayNullAs = displayNullAs ?? throw new ArgumentNullException(nameof(displayNullAs));
      m_False = asFalse ?? throw new ArgumentNullException(nameof(asFalse));
      m_NumberFormat = numberFormat ?? throw new ArgumentNullException(nameof(numberFormat));
      m_TimeSeparator = timeSeparator ?? throw new ArgumentNullException(nameof(timeSeparator));
      m_True = asTrue ?? throw new ArgumentNullException(nameof(asTrue));
      m_Part = part;
      m_PartSplitter = (partSplitter ?? throw new ArgumentNullException(nameof(partSplitter))).WrittenPunctuation();
      m_PartToEnd = partToEnd;
      m_RegexSearchPattern = regexSearchPattern ?? throw new ArgumentNullException(nameof(regexSearchPattern));
      m_RegexReplacement = regexReplacement ?? throw new ArgumentNullException(nameof(regexReplacement));
      m_ReadFolder = readFolder ?? throw new ArgumentNullException(nameof(readFolder));
      m_WriteFolder = writeFolder ?? throw new ArgumentNullException(nameof(writeFolder));
      m_FileOutPutPlaceholder = fileOutPutPlaceholder ?? throw new ArgumentNullException(nameof(fileOutPutPlaceholder));
      m_Overwrite = overwrite;
    }

    public ValueFormatMutable(IValueFormat other) : this(other.DataType, other.DateFormat, other.DateSeparator,
      other.TimeSeparator, other.NumberFormat,
      other.GroupSeparator, other.DecimalSeparator,
      other.True, other.False, other.DisplayNullAs, other.Part, other.PartSplitter, other.PartToEnd,
      other.RegexSearchPattern, other.RegexReplacement, other.ReadFolder, other.WriteFolder,
      other.FileOutPutPlaceholder, other.Overwrite)

    {
    }

    /// <summary>
    ///   Determines if anything is different to the default values, commonly used for
    ///   serialisation, avoiding empty elements
    /// </summary>

    public bool Specified => !(DataType == DataTypeEnum.String && DateFormat == ValueFormatExtension.cDateFormatDefault
                                                               && DateSeparator == ValueFormatExtension
                                                                 .cDateSeparatorDefault
                                                               && TimeSeparator == ValueFormatExtension
                                                                 .cTimeSeparatorDefault
                                                               && NumberFormat == ValueFormatExtension
                                                                 .cNumberFormatDefault
                                                               && DecimalSeparator == ValueFormatExtension
                                                                 .cDecimalSeparatorDefault
                                                               && GroupSeparator == ValueFormatExtension
                                                                 .cGroupSeparatorDefault
                                                               && True == ValueFormatExtension.cTrueDefault
                                                               && False == ValueFormatExtension.cFalseDefault
                                                               && Part == ValueFormatExtension.cPartDefault
                                                               && PartSplitter == ValueFormatExtension
                                                                 .cPartSplitterDefault
                                                               && PartToEnd == ValueFormatExtension.cPartToEndDefault
                                                               && RegexSearchPattern == string.Empty
                                                               && RegexReplacement == string.Empty
                                                               && ReadFolder == string.Empty
                                                               && WriteFolder == string.Empty
                                                               && FileOutPutPlaceholder == string.Empty
                                                               && DisplayNullAs == string.Empty);


    /// <inheritdoc />    
    [XmlAttribute]
    [DefaultValue(DataTypeEnum.String)]
    public DataTypeEnum DataType
    {
      get => m_DataType;
      set => SetField(ref m_DataType, value);
    }

    /// <inheritdoc />    
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cDateFormatDefault)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public string DateFormat
    {
      get => m_DateFormat;
      set => SetField(ref m_DateFormat, value, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cDateSeparatorDefault)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public string DateSeparator
    {
      get => m_DateSeparator;
      set => SetField(ref m_DateSeparator, (value ?? string.Empty).WrittenPunctuation(), StringComparison.Ordinal);
    }

    /// <inheritdoc />    
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cDecimalSeparatorDefault)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
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

    /// <inheritdoc />    
    [XmlAttribute]
    [DefaultValue("")]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public string DisplayNullAs
    {
      get => m_DisplayNullAs;
      set => SetField(ref m_DisplayNullAs, value, StringComparison.Ordinal);
    }

    /// <inheritdoc />    
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cFalseDefault)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public string False
    {
      get => m_False;
      set => SetField(ref m_False, value, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cGroupSeparatorDefault)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
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

    /// <inheritdoc />
    /// <summary>
    ///   Gets or sets the number format.
    /// </summary>
    /// <value>The number format.</value>
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cNumberFormatDefault)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public string NumberFormat
    {
      get => m_NumberFormat;
      set => SetField(ref m_NumberFormat, value, StringComparison.Ordinal);
    }

    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cPartDefault)]
    public int Part
    {
      get => m_Part;
      set => SetField(ref m_Part, value);
    }

#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif

    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cPartSplitterDefault)]
    public string PartSplitter
    {
      get => m_PartSplitter;
      set => SetField(ref m_PartSplitter, (value ?? string.Empty).WrittenPunctuation(), StringComparison.Ordinal);
    }

    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cPartToEndDefault)]
    public bool PartToEnd
    {
      get => m_PartToEnd;
      set => SetField(ref m_PartToEnd, value);
    }

    /// <inheritdoc />
    /// <summary>
    ///   Gets or sets the time separator.
    /// </summary>
    /// <value>The time separator.</value>
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cTimeSeparatorDefault)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public string TimeSeparator
    {
      get => m_TimeSeparator;
      set => SetField(ref m_TimeSeparator, (value ?? string.Empty).WrittenPunctuation(), StringComparison.Ordinal);
    }

    /// <inheritdoc />
    /// <summary>
    ///   Gets or sets the representation for true.
    /// </summary>
    /// <value>The true.</value>
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cTrueDefault)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public string True
    {
      get => m_True;
      set => SetField(ref m_True, value, StringComparison.InvariantCulture);
    }


    [XmlAttribute]
    [DefaultValue("")]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif

    public string RegexSearchPattern
    {
      get => m_RegexSearchPattern;
      set => SetField(ref m_RegexSearchPattern, value, StringComparison.Ordinal);
    }

    [XmlAttribute]
    [DefaultValue("")]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public string RegexReplacement
    {
      get => m_RegexReplacement;
      set => SetField(ref m_RegexReplacement, value, StringComparison.Ordinal);
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
    public string WriteFolder
    {
      get => m_WriteFolder;
      set => SetField(ref m_WriteFolder, value, StringComparison.Ordinal);
    }

    [XmlAttribute]
    [DefaultValue("")]
    public string FileOutPutPlaceholder
    {
      get => m_FileOutPutPlaceholder;
      set => SetField(ref m_FileOutPutPlaceholder, value, StringComparison.Ordinal);
    }

    [XmlAttribute]
    [DefaultValue(true)]
    public bool Overwrite
    {
      get => m_Overwrite;
      set => SetField(ref m_Overwrite, value);
    }

    /// <summary>
    ///   On Mutable classes prefer CopyFrom to CopyTo, overwrites the properties from the
    ///   properties in the provided class
    /// </summary>
    /// <param name="other"></param>
    public void CopyFrom(IValueFormat? other)
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
  }
}