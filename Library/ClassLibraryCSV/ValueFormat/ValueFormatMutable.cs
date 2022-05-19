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
  public sealed class ValueFormatMutable : IValueFormat, INotifyPropertyChanged
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
    /// <summary>
    ///   Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />    
    [XmlAttribute]
    [DefaultValue(DataTypeEnum.String)]
    public DataTypeEnum DataType
    {
      get => m_DataType;
      set
      {
        if (m_DataType.Equals(value))
          return;
        m_DataType = value;
        NotifyPropertyChanged(nameof(DataType));
      }
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
      set
      {
        var newVal = value ?? string.Empty;
        if (m_DateFormat.Equals(newVal, StringComparison.Ordinal))
          return;
        m_DateFormat = newVal;
        NotifyPropertyChanged(nameof(DateFormat));
      }
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
      set
      {
        // Translate written punctuation into a character
        var newVal = (value ?? string.Empty).WrittenPunctuation();
        if (m_DateSeparator.Equals(newVal, StringComparison.Ordinal))
          return;
        m_DateSeparator = newVal;
        NotifyPropertyChanged(nameof(DateSeparator));
      }
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
        // Translate written punctuation into a character
        var newValDecimal = (value ?? string.Empty).WrittenPunctuation();
        if (m_DecimalSeparator.Equals(newValDecimal))
          return;

        var newValGroup = m_GroupSeparator;
        if (newValGroup.Equals(newValDecimal))
        {
          m_GroupSeparator = "";
          NotifyPropertyChanged(nameof(GroupSeparator));
        }

        m_DecimalSeparator = newValDecimal;
        NotifyPropertyChanged(nameof(DecimalSeparator));
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

      set
      {
        var newVal = value ?? string.Empty;
        if (m_DisplayNullAs.Equals(newVal))
          return;
        m_DisplayNullAs = newVal;
        NotifyPropertyChanged(nameof(DisplayNullAs));
      }
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
      set
      {
        var newVal = value ?? string.Empty;
        if (m_False.Equals(newVal, StringComparison.OrdinalIgnoreCase))
          return;
        m_False = newVal;
        NotifyPropertyChanged(nameof(False));
      }
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
        var newValGroup = (value ?? string.Empty).WrittenPunctuation();
        if (m_GroupSeparator.Equals(newValGroup))
          return;
        // If we set the GroupSeparator to be the decimal separator, do not save
        var newValDecimal = m_DecimalSeparator;
        if (newValGroup.Equals(newValDecimal))
        {
          m_DecimalSeparator = m_GroupSeparator;
          NotifyPropertyChanged(nameof(DecimalSeparator));
        }

        m_GroupSeparator = newValGroup;
        NotifyPropertyChanged(nameof(GroupSeparator));
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
      set
      {
        var newVal = value ?? string.Empty;
        if (m_NumberFormat.Equals(newVal, StringComparison.Ordinal))
          return;
        m_NumberFormat = newVal;
        NotifyPropertyChanged(nameof(NumberFormat));
      }
    }

    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cPartDefault)]
    public int Part
    {
      get => m_Part;
      set
      {
        if (m_Part == value)
          return;
        m_Part = value;
        NotifyPropertyChanged(nameof(Part));
      }
    }

#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif

    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cPartSplitterDefault)]
    public string PartSplitter
    {
      get => m_PartSplitter;
      set
      {
        var newVal = (value ?? string.Empty).WrittenPunctuation();
        if (m_PartSplitter.Equals(newVal, StringComparison.Ordinal))
          return;
        m_PartSplitter = newVal;
        NotifyPropertyChanged(nameof(PartSplitter));
      }
    }

    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cPartToEndDefault)]
    public bool PartToEnd
    {
      get => m_PartToEnd;
      set
      {
        if (m_PartToEnd.Equals(value))
          return;
        m_PartToEnd = value;
        NotifyPropertyChanged(nameof(PartToEnd));
      }
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
      set
      {
        var newVal = (value ?? string.Empty).WrittenPunctuation();
        if (m_TimeSeparator.Equals(newVal, StringComparison.Ordinal))
          return;
        m_TimeSeparator = newVal;
        NotifyPropertyChanged(nameof(TimeSeparator));
      }
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
      set
      {
        var newVal = value ?? string.Empty;
        if (m_True.Equals(newVal, StringComparison.OrdinalIgnoreCase))
          return;
        m_True = newVal;
        NotifyPropertyChanged(nameof(True));
      }
    }


    [XmlAttribute]
    [DefaultValue("")]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif

    public string RegexSearchPattern
    {
      get => m_RegexSearchPattern;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_RegexSearchPattern.Equals(newVal, StringComparison.OrdinalIgnoreCase))
          return;
        m_RegexSearchPattern = newVal;
        NotifyPropertyChanged(nameof(RegexSearchPattern));
      }
    }

    [XmlAttribute]
    [DefaultValue("")]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public string RegexReplacement
    {
      get => m_RegexReplacement;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_RegexReplacement.Equals(newVal, StringComparison.OrdinalIgnoreCase))
          return;
        m_RegexReplacement = newVal;
        NotifyPropertyChanged(nameof(RegexReplacement));
      }
    }

    [XmlAttribute]
    [DefaultValue("")]
    public string ReadFolder
    {
      get => m_ReadFolder;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_ReadFolder.Equals(newVal))
          return;
        m_ReadFolder = newVal;
        NotifyPropertyChanged(nameof(ReadFolder));
      }
    }

    [XmlAttribute]
    [DefaultValue("")]
    public string WriteFolder
    {
      get => m_WriteFolder;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_WriteFolder.Equals(newVal))
          return;
        m_WriteFolder = newVal;
        NotifyPropertyChanged(nameof(WriteFolder));
      }
    }

    [XmlAttribute]
    [DefaultValue("")]
    public string FileOutPutPlaceholder
    {
      get => m_FileOutPutPlaceholder;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_FileOutPutPlaceholder.Equals(newVal))
          return;
        m_FileOutPutPlaceholder = newVal;
        NotifyPropertyChanged(nameof(FileOutPutPlaceholder));
      }
    }

    [XmlAttribute]
    [DefaultValue(true)]
    public bool Overwrite
    {
      get => m_Overwrite;
      set
      {
        if (m_Overwrite.Equals(value))
          return;
        m_Overwrite = value;
        NotifyPropertyChanged(nameof(Overwrite));
      }
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

    /// <summary>
    ///   Notifies the property changed.
    /// </summary>
    /// <param name="info">The info.</param>
    public void NotifyPropertyChanged(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
  }
}