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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

// ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract

namespace CsvTools
{
  /// <inheritdoc cref="IFileSetting" />
  /// <summary>
  ///   Abstract calls containing the basic setting for an IFileSetting if contains <see
  ///   cref="P:CsvTools.BaseSettings.ColumnCollection" /> and <see
  ///   cref="P:CsvTools.BaseSettings.MappingCollection" />
  /// </summary>
  [DebuggerDisplay("Settings: ({ColumnCollection.Count()} Columns)")]
  public abstract class BaseSettings : ObservableObject, IFileSetting
  {
    /// <summary>
    /// The default text to be treated as NULL
    /// </summary>
    public const string cTreatTextAsNull = "NULL";
    private int m_ConsecutiveEmptyRows = 5;
    private bool m_DisplayEndLineNo;
    private bool m_DisplayRecordNo;
    private bool m_DisplayStartLineNo = true;
    private string m_Footer = string.Empty;
    private bool m_HasFieldHeader = true;
    private string m_Header = string.Empty;
    private bool m_KeepUnencrypted;
    private long m_RecordLimit;
    private bool m_SkipDuplicateHeader;
    private bool m_SkipEmptyLines = true;
    private int m_SkipRows;
    private bool m_TreatNbspAsSpace;
    private string m_TreatTextAsNull = cTreatTextAsNull;
    private TrimmingOptionEnum m_TrimmingOption = TrimmingOptionEnum.Unquoted;

    /// <summary>
    ///   Initializes a new instance of the <see cref="BaseSettings" /> class.
    /// </summary>
    protected BaseSettings()
    {
      // adding or removing columns should cause a property changed for ColumnCollection
      ColumnCollection.CollectionChanged += (sender, e) =>
      {
        if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Add)
          NotifyPropertyChanged(nameof(ColumnCollection));
      };
      ColumnCollection.CollectionItemPropertyChanged += (sender, e) => NotifyPropertyChanged(nameof(ColumnCollection));
    }
    /// <inheritdoc />
    public ColumnCollection ColumnCollection { get; } = new ColumnCollection();

    /// <inheritdoc />
    [DefaultValue(5)]
    public virtual int ConsecutiveEmptyRows
    {
      get => m_ConsecutiveEmptyRows;
      set => SetProperty(ref m_ConsecutiveEmptyRows, (value<0) ? 0 : value);
    }

    /// <inheritdoc />
    [DefaultValue(false)]
    public virtual bool DisplayEndLineNo
    {
      get => m_DisplayEndLineNo;
      set => SetProperty(ref m_DisplayEndLineNo, value);
    }

    /// <inheritdoc />
    [DefaultValue(false)]
    public virtual bool DisplayRecordNo
    {
      get => m_DisplayRecordNo;
      set => SetProperty(ref m_DisplayRecordNo, value);
    }

    /// <inheritdoc />
    [DefaultValue(true)]
    public virtual bool DisplayStartLineNo
    {
      get => m_DisplayStartLineNo;
      set => SetProperty(ref m_DisplayStartLineNo, value);
    }

    /// <inheritdoc />
    [DefaultValue("")]
    public virtual string Footer
    {
      get => m_Footer;
      set => SetProperty(ref m_Footer, (value ?? string.Empty).HandleCrlfCombinations(Environment.NewLine));
    }

    /// <inheritdoc />
    [DefaultValue(true)]
    public virtual bool HasFieldHeader
    {
      get => m_HasFieldHeader;
      set => SetProperty(ref m_HasFieldHeader, value);
    }

    /// <inheritdoc />
    [DefaultValue("")]
    public virtual string Header
    {
      get => m_Header;
      set => SetProperty(ref m_Header, (value ?? string.Empty).HandleCrlfCombinations(Environment.NewLine));
    }

    /// <inheritdoc />
    [DefaultValue(false)]
    public bool KeepUnencrypted
    {
      get => m_KeepUnencrypted;
      set => SetProperty(ref m_KeepUnencrypted, value);
    }
    /// <inheritdoc />
    [DefaultValue(0)]
    public virtual long RecordLimit
    {
      get => m_RecordLimit;
      set => SetProperty(ref m_RecordLimit, value > 0 ? value : 0);
    }


    /// <inheritdoc />    
    [DefaultValue(false)]
    public virtual bool SkipDuplicateHeader
    {
      get => m_SkipDuplicateHeader;
      set => SetProperty(ref m_SkipDuplicateHeader, value);
    }

    /// <inheritdoc />
    [DefaultValue(true)]
    public virtual bool SkipEmptyLines
    {
      get => m_SkipEmptyLines;
      set => SetProperty(ref m_SkipEmptyLines, value);
    }

    /// <inheritdoc />
    [DefaultValue(0)]
    public virtual int SkipRows
    {
      get => m_SkipRows;
      set => SetProperty(ref m_SkipRows, value > 0 ? value : 0);
    }


    /// <inheritdoc />
    [DefaultValue(false)]
    public virtual bool TreatNBSPAsSpace
    {
      get => m_TreatNbspAsSpace;
      set => SetProperty(ref m_TreatNbspAsSpace, value);
    }

    /// <inheritdoc />
    [DefaultValue(cTreatTextAsNull)]
    public virtual string TreatTextAsNull
    {
      get => m_TreatTextAsNull;
      set => SetProperty(ref m_TreatTextAsNull, value ?? cTreatTextAsNull);
    }

    /// <inheritdoc />
    [DefaultValue(TrimmingOptionEnum.Unquoted)]
    public virtual TrimmingOptionEnum TrimmingOption
    {
      get => m_TrimmingOption;
      set => SetProperty(ref m_TrimmingOption, value);
    }

    /// <inheritdoc />
    public abstract object Clone();

    /// <inheritdoc />
    public abstract void CopyTo(IFileSetting other);

    /// <inheritdoc />
    public abstract bool Equals(IFileSetting? other);

#if !CsvQuickViewer
    /// <summary>
    /// List all difference between two instances
    /// </summary>
    /// <param name="other">The other IFileSetting </param>
    public virtual IEnumerable<string> GetDifferences(IFileSetting other)
    {
      if (!other.ColumnCollection.Equals(ColumnCollection)) yield return $"{nameof(ColumnCollection)} different";
      if (!other.Footer.Equals(Footer, StringComparison.Ordinal)) yield return $"{nameof(Footer)} : {Footer} - {other.Footer}";
      if (!other.Header.Equals(Header, StringComparison.Ordinal)) yield return $"{nameof(Header)} : {Header} - {other.Header}";
      if (!other.TreatTextAsNull.Equals(TreatTextAsNull, StringComparison.OrdinalIgnoreCase)) yield return $"{nameof(TreatTextAsNull)} : {TreatTextAsNull} - {other.TreatTextAsNull}";
      if (other.ConsecutiveEmptyRows != ConsecutiveEmptyRows) yield return $"{nameof(ConsecutiveEmptyRows)} : {ConsecutiveEmptyRows} - {other.ConsecutiveEmptyRows}";
      if (other.DisplayEndLineNo != DisplayEndLineNo) yield return $"{nameof(DisplayEndLineNo)} : {DisplayEndLineNo} - {other.DisplayEndLineNo}";
      if (other.DisplayRecordNo != DisplayRecordNo) yield return $"{nameof(DisplayRecordNo)} : {DisplayRecordNo} - {other.DisplayRecordNo}";
      if (other.DisplayStartLineNo != DisplayStartLineNo) yield return $"{nameof(DisplayStartLineNo)} : {DisplayStartLineNo} - {other.DisplayStartLineNo}";
      if (other.HasFieldHeader != HasFieldHeader) yield return $"{nameof(HasFieldHeader)} : {HasFieldHeader} - {other.HasFieldHeader}";
      if (other.RecordLimit != RecordLimit) yield return $"{nameof(RecordLimit)} : {RecordLimit} - {other.RecordLimit}";
      if (other.SkipDuplicateHeader != SkipDuplicateHeader) yield return $"{nameof(SkipDuplicateHeader)} : {SkipDuplicateHeader} - {other.SkipDuplicateHeader}";
      if (other.SkipEmptyLines != SkipEmptyLines) yield return $"{nameof(SkipEmptyLines)} : {SkipEmptyLines} - {other.SkipEmptyLines}";
      if (other.SkipRows != SkipRows) yield return $"{nameof(SkipRows)}: {SkipRows} - {other.SkipRows}";
      if (other.TreatNBSPAsSpace != TreatNBSPAsSpace) yield return $"{nameof(TreatNBSPAsSpace)} : {TreatNBSPAsSpace} - {other.TreatNBSPAsSpace}";
      if (other.TreatTextAsNull != TreatTextAsNull) yield return $"{nameof(TreatTextAsNull)} : {TreatTextAsNull} - {other.TreatTextAsNull}";
      if (other.KeepUnencrypted != KeepUnencrypted) yield return $"{nameof(KeepUnencrypted)} : {KeepUnencrypted} - {other.KeepUnencrypted}";
    }
#endif

    /// <summary>
    ///   Copies all values to other instance
    /// </summary>
    /// <param name="other">The other.</param>
    protected virtual void BaseSettingsCopyTo(in BaseSettings? other)
    {
      if (other is null)
        return;

      other.ConsecutiveEmptyRows = ConsecutiveEmptyRows;
      other.TrimmingOption = TrimmingOption;
      other.DisplayStartLineNo = DisplayStartLineNo;
      other.DisplayEndLineNo = DisplayEndLineNo;
      other.DisplayRecordNo = DisplayRecordNo;
      other.HasFieldHeader = HasFieldHeader;
      other.TreatTextAsNull = TreatTextAsNull;
      other.RecordLimit = RecordLimit;
      other.SkipRows = SkipRows;
      other.SkipEmptyLines = SkipEmptyLines;
      other.SkipDuplicateHeader = SkipDuplicateHeader;
      other.KeepUnencrypted = KeepUnencrypted;
      other.TreatNBSPAsSpace = TreatNBSPAsSpace;
      other.ColumnCollection.Clear();
      other.ColumnCollection.AddRange(ColumnCollection);
      other.Footer = Footer;
      other.Header = Header;
    }

    /// <summary>
    ///   Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other" />
    ///   parameter; otherwise, <see langword="false" />.
    /// </returns>
    protected virtual bool BaseSettingsEquals(in BaseSettings? other)
    {
      if (other is null)
        return false;
      if (other.KeepUnencrypted != KeepUnencrypted) return false;
      if (other.SkipRows != SkipRows || other.HasFieldHeader != HasFieldHeader) return false;
      if (other.TreatNBSPAsSpace != TreatNBSPAsSpace || other.ConsecutiveEmptyRows != ConsecutiveEmptyRows) return false;
      if (other.DisplayStartLineNo != DisplayStartLineNo || other.DisplayEndLineNo != DisplayEndLineNo || other.DisplayRecordNo != DisplayRecordNo) return false;
      if (other.RecordLimit != RecordLimit) return false;
      if (other.SkipEmptyLines != SkipEmptyLines || other.SkipDuplicateHeader != SkipDuplicateHeader) return false;
      if (!other.TreatTextAsNull.Equals(TreatTextAsNull, StringComparison.OrdinalIgnoreCase) || other.TrimmingOption != TrimmingOption) return false;
      if (!other.Footer.Equals(Footer, StringComparison.Ordinal)           || !other.Header.Equals(Header, StringComparison.OrdinalIgnoreCase)) return false;
      return other.ColumnCollection.Equals(ColumnCollection);
    }

#if !CsvQuickViewer
#endif

  }
}