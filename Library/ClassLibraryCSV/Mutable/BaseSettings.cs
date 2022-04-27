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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CsvTools
{
  // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  /// <inheritdoc />
  /// <summary>
  ///   Abstract calls containing the basic setting for an IFileSetting if contains <see cref="P:CsvTools.BaseSettings.ColumnCollection" /> , <see cref="P:CsvTools.BaseSettings.MappingCollection" /> /&gt;
  /// </summary>
  [DebuggerDisplay("Settings: {ID} ({ColumnCollection.Count()} Columns)")]
  public abstract class BaseSettings : IFileSetting
  {
    public const string cTreatTextAsNull = "NULL";

    public static readonly DateTime ZeroTime = new DateTime(0, DateTimeKind.Utc);

    private int m_ConsecutiveEmptyRows = 5;

    private bool m_DisplayEndLineNo;

    private bool m_DisplayRecordNo;

    private bool m_DisplayStartLineNo = true;

    private long m_ErrorCount;

    private string m_Footer = string.Empty;

    private bool m_HasFieldHeader = true;

    private string m_Header = string.Empty;

    private string m_Id = string.Empty;

    private bool m_InOverview;

    private bool m_IsEnabled = true;

    private bool m_KeepUnencrypted;

    private DateTime m_LatestSourceTimeUtc = ZeroTime;

    private long m_NumRecords;

    private DateTime m_ProcessTimeUtc = ZeroTime;

    private long m_RecordLimit;
    private bool m_SetLatestSourceTimeForWrite;

    private bool m_ShowProgress = true;

    private bool m_SkipDuplicateHeader;

    private bool m_SkipEmptyLines = true;

    private int m_SkipRows;

    private IReadOnlyCollection<IFileSetting>? m_SourceFileSettings;

    private string m_SqlStatement = string.Empty;

    private string m_TemplateName = string.Empty;

    private int m_Timeout = 90;

    private bool m_TreatNbspAsSpace;

    private string m_TreatTextAsNull = cTreatTextAsNull;

    private TrimmingOption m_TrimmingOption = TrimmingOption.Unquoted;

    private bool m_Validate = true;

    private long m_WarningCount;
    private int m_Order = 100;
    private string m_Comment = string.Empty;

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
      MappingCollection.CollectionChanged += (sender, e) =>
      {
        if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Add)
          NotifyPropertyChanged(nameof(MappingCollection));
      };
    }

    /// <summary>
    ///   Workaround to serialize
    /// </summary>
    /// <value>The column options</value>
    [XmlElement]
    public virtual Column[] Format
    {
      get
      {
        var res = new Column[ColumnCollection.Count];
        for (var index = 0; index < ColumnCollection.Count; index++)
          res[index] = new Column(ColumnCollection[index]);
        return res;
      }
      set
      {
        ColumnCollection.Clear();
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (value != null)
          foreach (var col in value)
            ColumnCollection.Add(col);
      }
    }

    /// <inheritdoc />
    [XmlElement]
    [DefaultValue(false)]
    public virtual bool DisplayRecordNo
    {
      get => m_DisplayRecordNo;

      set
      {
        if (m_DisplayRecordNo.Equals(value))
          return;
        m_DisplayRecordNo = value;
        NotifyPropertyChanged(nameof(DisplayRecordNo));
      }
    }

    /// <inheritdoc />
    [XmlElement]
    [DefaultValue(true)]
    public virtual bool DisplayStartLineNo
    {
      get => m_DisplayStartLineNo;

      set
      {
        if (m_DisplayStartLineNo.Equals(value))
          return;
        m_DisplayStartLineNo = value;
        NotifyPropertyChanged(nameof(DisplayStartLineNo));
      }
    }

    [XmlElement]
    [DefaultValue(false)]
    public virtual bool SetLatestSourceTimeForWrite
    {
      get => m_SetLatestSourceTimeForWrite;

      set
      {
        if (m_SetLatestSourceTimeForWrite.Equals(value))
          return;
        m_SetLatestSourceTimeForWrite = value;
        NotifyPropertyChanged(nameof(SetLatestSourceTimeForWrite));
      }
    }

    /// <summary>
    ///   Gets a value indicating whether FileLastWriteTimeUtc is specified.
    /// </summary>
    /// <value><c>true</c> if specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>

    public bool FileLastWriteTimeUtcSpecified => ProcessTimeUtc != ZeroTime;

    /// <summary>
    ///   Gets a value indicating whether field mapping specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>

    public bool MappingSpecified => MappingCollection.Count > 0;

    public bool ProcessTimeUtcSpecified => m_ProcessTimeUtc != ZeroTime;

    public bool SamplesAndErrorsSpecified =>
      SamplesAndErrors.ErrorsSpecified || SamplesAndErrors.SamplesSpecified || SamplesAndErrors.NumErrors != -1;

    /// <summary>
    ///   Utility calls to get or set the SQL Statement as CDataSection
    /// </summary>
    [DefaultValue("")]
    public XmlCDataSection SqlStatementCData
    {
      get
      {
        var doc = new XmlDocument();
        return doc.CreateCDataSection(SqlStatement);
      }
      set => m_SqlStatement = value.Value;
    }

    /// <summary>
    ///   Gets a value indicating whether SqlStatementCData is specified.
    /// </summary>
    /// <value><c>true</c> if specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>

    public bool SqlStatementCDataSpecified => !string.IsNullOrEmpty(SqlStatement);

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    /// <summary>
    ///   Occurs when a string value property changed providing information on old and new value
    /// </summary>
    public event EventHandler<PropertyChangedEventArgs<string>>? PropertyChangedString;

    [XmlIgnore] public ColumnCollection ColumnCollection { get; } = new ColumnCollection();

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(5)]
    public virtual int ConsecutiveEmptyRows
    {
      get => m_ConsecutiveEmptyRows;

      set
      {
        if (m_ConsecutiveEmptyRows.Equals(value))
          return;
        if (value < 0)
          value = 0;
        m_ConsecutiveEmptyRows = value;
        NotifyPropertyChanged(nameof(ConsecutiveEmptyRows));
      }
    }

    /// <inheritdoc />
    [XmlElement]
    [DefaultValue(false)]
    public virtual bool DisplayEndLineNo
    {
      get => m_DisplayEndLineNo;

      set
      {
        if (m_DisplayEndLineNo.Equals(value))
          return;
        m_DisplayEndLineNo = value;
        NotifyPropertyChanged(nameof(DisplayEndLineNo));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(0)]
    public virtual long ErrorCount
    {
      get => m_ErrorCount;

      set
      {
        if (m_ErrorCount == value)
          return;
        m_ErrorCount = value;
        NotifyPropertyChanged(nameof(ErrorCount));
      }
    }

    /// <inheritdoc />
    [XmlElement]
    public SampleAndErrorsInformation SamplesAndErrors { get; set; } = new SampleAndErrorsInformation();

    /// <inheritdoc />
    [DefaultValue("")]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public virtual string Footer
    {
      get => m_Footer;
      set
      {
        var newVal = (value ?? string.Empty).HandleCrlfCombinations(Environment.NewLine);
        if (m_Footer.Equals(newVal, StringComparison.Ordinal))
          return;
        m_Footer = newVal;
        NotifyPropertyChanged(nameof(Footer));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(true)]
    public virtual bool HasFieldHeader
    {
      get => m_HasFieldHeader;

      set
      {
        if (m_HasFieldHeader.Equals(value))
          return;
        m_HasFieldHeader = value;
        NotifyPropertyChanged(nameof(HasFieldHeader));
      }
    }

    /// <inheritdoc />
    [DefaultValue("")]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public virtual string Header
    {
      get => m_Header;
      set
      {
        var newVal = (value ?? string.Empty).HandleCrlfCombinations(Environment.NewLine);
        if (m_Header.Equals(newVal, StringComparison.Ordinal))
          return;
        m_Header = newVal;
        NotifyPropertyChanged(nameof(Header));
      }
    }


    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue("")]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public virtual string ID
    {
      get => m_Id;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_Id.Equals(newVal, StringComparison.Ordinal))
          return;
        var oldValueInternal = InternalID;
        var oldValue = m_Id;
        m_Id = newVal;
        NotifyPropertyChanged(nameof(ID));
        NotifyPropertyChangedString(nameof(ID), oldValue, newVal);
        if (oldValueInternal != InternalID)
          NotifyPropertyChangedString(nameof(InternalID), oldValue, newVal);
      }
    }

    /// <inheritdoc />
    [XmlAttribute(AttributeName = "IsCritical")]
    [DefaultValue(false)]
    public virtual bool InOverview
    {
      get => m_InOverview;

      set
      {
        if (m_InOverview.Equals(value))
          return;
        m_InOverview = value;
        NotifyPropertyChanged(nameof(InOverview));
      }
    }

    [XmlAttribute]
    [DefaultValue(100)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public virtual int Order
    {
      get => m_Order;
      set
      {
        if (m_Order.Equals(value))
          return;
        m_Order = value;
        NotifyPropertyChanged(nameof(Order));
      }
    }

    [XmlAttribute]
    [DefaultValue("")]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public virtual string Comment
    {
      get => m_Comment;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_Comment.Equals(newVal, StringComparison.Ordinal))
          return;
        m_Id = newVal;
        NotifyPropertyChanged(nameof(Comment));
      }
    }

    /// <inheritdoc />
    [XmlIgnore]
    public virtual string InternalID => ID;

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(true)]
    public virtual bool IsEnabled
    {
      get => m_IsEnabled;

      set
      {
        if (m_IsEnabled.Equals(value))
          return;
        m_IsEnabled = value;
        NotifyPropertyChanged(nameof(IsEnabled));
      }
    }

    [XmlAttribute]
    [DefaultValue(false)]
    public bool KeepUnencrypted
    {
      get => m_KeepUnencrypted;
      set
      {
        if (m_KeepUnencrypted == value)
          return;
        m_KeepUnencrypted = value;
        NotifyPropertyChanged(nameof(KeepUnencrypted));
      }
    }

    /// <inheritdoc />
    [XmlIgnore]
    public DateTime LatestSourceTimeUtc
    {
      get
      {
        if (m_LatestSourceTimeUtc == ZeroTime)
          CalculateLatestSourceTime();
        return m_LatestSourceTimeUtc;
      }

      set
      {
        if (m_LatestSourceTimeUtc == value)
          return;
        m_LatestSourceTimeUtc = value;
        NotifyPropertyChanged(nameof(LatestSourceTimeUtc));
      }
    }

    /// <inheritdoc />
    [XmlElement("Mapping")]
    public MappingCollection MappingCollection { get; } = new MappingCollection();

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(0)]
    public virtual long NumRecords
    {
      get => m_NumRecords;

      set
      {
        if (m_NumRecords == value)
          return;
        m_NumRecords = value;
        NotifyPropertyChanged(nameof(NumRecords));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    public virtual DateTime ProcessTimeUtc
    {
      get => m_ProcessTimeUtc;

      set
      {
        if (m_ProcessTimeUtc.Equals(value))
          return;
        m_ProcessTimeUtc = value;
        NotifyPropertyChanged(nameof(ProcessTimeUtc));
      }
    }

    /// <inheritdoc />
    [XmlIgnore]
    [DefaultValue(false)]
    public virtual bool RecentlyLoaded { get; set; }

    /// <inheritdoc />
    [XmlElement]
    [DefaultValue(0)]
    public virtual long RecordLimit
    {
      get => m_RecordLimit;

      set
      {
        if (m_RecordLimit.Equals(value))
          return;
        m_RecordLimit = value;
        NotifyPropertyChanged(nameof(RecordLimit));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(true)]
    public virtual bool ShowProgress
    {
      get => m_ShowProgress;

      set
      {
        if (m_ShowProgress.Equals(value))
          return;
        m_ShowProgress = value;
        NotifyPropertyChanged(nameof(ShowProgress));
      }
    }

    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool SkipDuplicateHeader
    {
      get => m_SkipDuplicateHeader;

      set
      {
        if (m_SkipDuplicateHeader.Equals(value))
          return;
        m_SkipDuplicateHeader = value;
        NotifyPropertyChanged(nameof(SkipDuplicateHeader));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(true)]
    public virtual bool SkipEmptyLines
    {
      get => m_SkipEmptyLines;

      set
      {
        if (m_SkipEmptyLines.Equals(value))
          return;
        m_SkipEmptyLines = value;
        NotifyPropertyChanged(nameof(SkipEmptyLines));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(0)]
    public virtual int SkipRows
    {
      get => m_SkipRows;

      set
      {
        if (m_SkipRows.Equals(value))
          return;
        m_SkipRows = value;
        NotifyPropertyChanged(nameof(SkipRows));
      }
    }

    /// <inheritdoc />
    [XmlIgnore]
    public IReadOnlyCollection<IFileSetting>? SourceFileSettings
    {
      get => m_SourceFileSettings;
      set
      {
        if (m_SourceFileSettings is null && value is null) return;
        if (value != null && value.CollectionEqual(m_SourceFileSettings)) return;
        // do not notify if we change from null to an empty list
        var notify = value?.Count() > 0 || m_SourceFileSettings != null;
        m_SourceFileSettings = value;
        if (notify)
          NotifyPropertyChanged(nameof(SourceFileSettings));
      }
    }

    /// <inheritdoc />
    [XmlIgnore]
    [DefaultValue("")]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public virtual string SqlStatement
    {
      get => m_SqlStatement;
      set
      {
        var newVal = (value ?? string.Empty).NoControlCharacters().HandleCrlfCombinations();
        if (newVal.Equals(m_SqlStatement, StringComparison.Ordinal))
          return;
        m_SqlStatement = newVal;
        // Need to assume we have new sources, it has to be recalculated
        SourceFileSettings = null;
        // Reset the process time as well
        ProcessTimeUtc = ZeroTime;
        LatestSourceTimeUtc = ZeroTime;
        NotifyPropertyChanged(nameof(SqlStatement));
      }
    }

    /// <inheritdoc />
    [XmlElement]
    [DefaultValue("")]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public virtual string TemplateName
    {
      get => m_TemplateName;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_TemplateName.Equals(newVal, StringComparison.Ordinal))
          return;
        m_TemplateName = newVal;
        NotifyPropertyChanged(nameof(TemplateName));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(90)]
    public virtual int Timeout
    {
      get => m_Timeout;

      set
      {
        var newVal = value > 0 ? value : 0;
        if (m_Timeout.Equals(newVal))
          return;
        m_Timeout = newVal;
        NotifyPropertyChanged(nameof(Timeout));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool TreatNBSPAsSpace
    {
      get => m_TreatNbspAsSpace;

      set
      {
        if (m_TreatNbspAsSpace.Equals(value))
          return;
        m_TreatNbspAsSpace = value;
        NotifyPropertyChanged(nameof(TreatNBSPAsSpace));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(cTreatTextAsNull)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public virtual string TreatTextAsNull
    {
      get => m_TreatTextAsNull;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_TreatTextAsNull.Equals(newVal, StringComparison.Ordinal))
          return;
        m_TreatTextAsNull = newVal;
        NotifyPropertyChanged(nameof(TreatTextAsNull));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(TrimmingOption.Unquoted)]
    public virtual TrimmingOption TrimmingOption
    {
      get => m_TrimmingOption;

      set
      {
        if (m_TrimmingOption.Equals(value))
          return;
        m_TrimmingOption = value;
        NotifyPropertyChanged(nameof(TrimmingOption));
      }
    }

    /// <inheritdoc />
    [XmlAttribute(AttributeName = "IsImported")]
    [DefaultValue(true)]
    public virtual bool Validate
    {
      get => m_Validate;

      set
      {
        if (m_Validate.Equals(value))
          return;
        m_Validate = value;
        NotifyPropertyChanged(nameof(Validate));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(0)]
    public virtual long WarningCount
    {
      get => m_WarningCount;

      set
      {
        if (m_WarningCount == value)
          return;
        m_WarningCount = value;
        NotifyPropertyChanged(nameof(WarningCount));
      }
    }

    /// <inheritdoc />
    [XmlAttribute]
    public virtual DateTime LastChange
    {
      get;
      set;
    } = DateTime.UtcNow;


    /// <inheritdoc />
    public virtual void CalculateLatestSourceTime() => LatestSourceTimeUtc = ProcessTimeUtc;

    /// <inheritdoc />
    public abstract object Clone();

    /// <inheritdoc />
    public abstract void CopyTo(IFileSetting other);

    /// <inheritdoc />
    public abstract bool Equals(IFileSetting? other);

    /// <summary>
    ///   Sets the SQL statement rename.
    /// </summary>
    /// <param name="value">The value.</param>
    public void SetSqlStatementRename(string? value)
    {
      var newVal = (value ?? string.Empty).NoControlCharacters();
      m_SqlStatement = newVal;
    }

    /// <summary>
    ///   Converts to string.
    /// </summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    public override string ToString()
    {
      var stringBuilder = new StringBuilder();
      stringBuilder.Append(GetType().Name);
      if (!string.IsNullOrEmpty(ID))
        stringBuilder.Append(" ");
      stringBuilder.Append(ID);
      return stringBuilder.ToString();
    }

    /// <summary>
    ///   Copies all values to other instance
    /// </summary>
    /// <param name="other">The other.</param>
    protected virtual void BaseSettingsCopyTo(in BaseSettings? other)
    {
      if (other == null)
        return;

      MappingCollection.CopyTo(other.MappingCollection);
      other.LastChange = LastChange;
      other.ConsecutiveEmptyRows = ConsecutiveEmptyRows;
      other.TrimmingOption = TrimmingOption;
      other.TemplateName = TemplateName;
      other.IsEnabled = IsEnabled;
      other.DisplayStartLineNo = DisplayStartLineNo;
      other.SetLatestSourceTimeForWrite = SetLatestSourceTimeForWrite;
      other.DisplayEndLineNo = DisplayEndLineNo;
      other.DisplayRecordNo = DisplayRecordNo;
      other.HasFieldHeader = HasFieldHeader;
      other.IsEnabled = IsEnabled;
      other.ShowProgress = ShowProgress;
      other.TreatTextAsNull = TreatTextAsNull;
      other.Validate = Validate;
      other.RecordLimit = RecordLimit;
      other.SkipRows = SkipRows;
      other.SkipEmptyLines = SkipEmptyLines;
      other.SkipDuplicateHeader = SkipDuplicateHeader;

      other.TreatNBSPAsSpace = TreatNBSPAsSpace;
      ColumnCollection.CopyTo(other.ColumnCollection);
      other.SqlStatement = SqlStatement;
      other.InOverview = InOverview;
      other.Timeout = Timeout;
      other.ProcessTimeUtc = ProcessTimeUtc;
      other.RecentlyLoaded = RecentlyLoaded;
      other.KeepUnencrypted = KeepUnencrypted;
      other.LatestSourceTimeUtc = LatestSourceTimeUtc;
      other.Order = Order;
      other.Comment = Comment;
      other.Footer = Footer;
      other.Header = Header;
      SamplesAndErrors.CopyTo(other.SamplesAndErrors);

      other.ID = ID;
      other.NumRecords = NumRecords;
      other.WarningCount = WarningCount;
      other.ErrorCount = ErrorCount;
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
      if (!other.ID.Equals(ID, StringComparison.OrdinalIgnoreCase))
        return false;
      if (other.SkipRows != SkipRows || other.HasFieldHeader != HasFieldHeader)
        return false;
      if (other.RecentlyLoaded != RecentlyLoaded || other.IsEnabled != IsEnabled || other.InOverview != InOverview
          || other.Validate != Validate || other.ShowProgress != ShowProgress)
        return false;
      if (other.NumRecords != NumRecords || other.WarningCount != WarningCount || other.ErrorCount != ErrorCount)
        return false;
      if (other.TreatNBSPAsSpace != TreatNBSPAsSpace || other.ConsecutiveEmptyRows != ConsecutiveEmptyRows)
        return false;
      if (other.DisplayStartLineNo != DisplayStartLineNo || other.DisplayEndLineNo != DisplayEndLineNo
                                                         || other.DisplayRecordNo != DisplayRecordNo)
        return false;
      if (other.RecordLimit != RecordLimit)
        return false;
      if (other.SkipEmptyLines != SkipEmptyLines || other.SkipDuplicateHeader != SkipDuplicateHeader)
        return false;
      if (other.Timeout != Timeout || other.SetLatestSourceTimeForWrite != SetLatestSourceTimeForWrite)
        return false;
      if ((other.ProcessTimeUtc - ProcessTimeUtc).TotalSeconds > 1)
        return false;
      if ((other.LatestSourceTimeUtc - LatestSourceTimeUtc).TotalSeconds > 1)
        return false;
      if (!other.TreatTextAsNull.Equals(TreatTextAsNull, StringComparison.OrdinalIgnoreCase)
          || other.TrimmingOption != TrimmingOption)
        return false;
      if (!other.TemplateName.Equals(TemplateName, StringComparison.Ordinal)
          || !other.SqlStatement.Equals(SqlStatement, StringComparison.OrdinalIgnoreCase))
        return false;
      if (!other.Footer.Equals(Footer, StringComparison.Ordinal)
          || !other.Header.Equals(Header, StringComparison.OrdinalIgnoreCase))
        return false;
      if (other.KeepUnencrypted != KeepUnencrypted)
        return false;
      if (!other.MappingCollection.Equals(MappingCollection))
        return false;
      if (!other.SamplesAndErrors.Equals(SamplesAndErrors))
        return false;
      if (other.Order != Order || !other.Comment.Equals(Comment))
        return false;

      return other.ColumnCollection.Equals(ColumnCollection);
    }

    /// <summary>
    ///   Notifies the completed property changed.
    /// </summary>
    /// <param name="info">The property name.</param>
    protected void NotifyPropertyChanged(string info)
    {
      LastChange = DateTime.UtcNow;
      if (PropertyChanged is null)
        return;
      try
      {
        // ReSharper disable once PolymorphicFieldLikeEventInvocation
        PropertyChanged(this, new PropertyChangedEventArgs(info));
      }
      catch (TargetInvocationException)
      {
        // Ignore
      }
    }

    /// <summary>
    ///   Notifies on changed property strings
    /// </summary>
    /// <param name="info">The property name.</param>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newVal">The new value.</param>
    protected void NotifyPropertyChangedString(string info, string oldValue, string newVal)
    {
      if (PropertyChangedString is null)
        return;
      try
      {
        // ReSharper disable once PolymorphicFieldLikeEventInvocation
        PropertyChangedString?.Invoke(this, new PropertyChangedEventArgs<string>(info, oldValue, newVal));
      }
      catch (TargetInvocationException)
      {
        // Ignore
      }
    }

    /// <inheritdoc />
    public virtual IEnumerable<string> GetDifferences(IFileSetting other)
    {
      if (!other.GetType().FullName.Equals(GetType().FullName, StringComparison.OrdinalIgnoreCase))
        yield return $"Type : {GetType().FullName} - {other.GetType().FullName}";

      if (!other.ID.Equals(ID, StringComparison.OrdinalIgnoreCase))
        yield return $"ID : {ID} - {other.ID}";
      
      if (other.SkipRows != SkipRows)
        yield return $"SkipRows : {SkipRows} - {other.SkipRows}";
      
      if (other.HasFieldHeader != HasFieldHeader)
        yield return $"HasFieldHeader : {HasFieldHeader} - {other.HasFieldHeader}";
      
      if (other.RecentlyLoaded != RecentlyLoaded)
        yield return $"RecentlyLoaded : {RecentlyLoaded} - {other.RecentlyLoaded}";
      
      if (other.IsEnabled != IsEnabled)
        yield return $"IsEnabled : {IsEnabled} - {other.IsEnabled}";
      
      if (other.InOverview != InOverview)
        yield return $"InOverview : {InOverview} - {other.InOverview}";
      
      if (other.Validate != Validate)
        yield return $"Validate : {Validate} - {other.Validate}";
      
      if (other.ShowProgress != ShowProgress)
        yield return $"ShowProgress : {ShowProgress} - {other.ShowProgress}";
      
      if (other.TreatNBSPAsSpace != TreatNBSPAsSpace)
        yield return $"TreatNBSPAsSpace : {TreatNBSPAsSpace} - {other.TreatNBSPAsSpace}";
      
      if (other.ConsecutiveEmptyRows != ConsecutiveEmptyRows)
        yield return $"ConsecutiveEmptyRows : {ConsecutiveEmptyRows} - {other.ConsecutiveEmptyRows}";

      if (other.DisplayStartLineNo != DisplayStartLineNo)
        yield return $"DisplayStartLineNo : {DisplayStartLineNo} - {other.DisplayStartLineNo}";
      
      if (other.DisplayEndLineNo != DisplayEndLineNo)
        yield return $"DisplayEndLineNo : {DisplayEndLineNo} - {other.DisplayEndLineNo}";
      
      if (other.DisplayRecordNo != DisplayRecordNo)
        yield return $"DisplayRecordNo : {DisplayRecordNo} - {other.DisplayRecordNo}";
      
      if (other.RecordLimit != RecordLimit)
        yield return $"RecordLimit : {RecordLimit} - {other.RecordLimit}";
      
      if (other.SkipEmptyLines != SkipEmptyLines)
        yield return $"SkipEmptyLines : {SkipEmptyLines} - {other.SkipEmptyLines}";

      if (other.SkipDuplicateHeader != SkipDuplicateHeader)
        yield return $"SkipDuplicateHeader : {SkipDuplicateHeader} - {other.SkipDuplicateHeader}";

      if (other.Timeout != Timeout)
        yield return $"Timeout : {Timeout} - {other.Timeout}";

      if (other.TreatTextAsNull != TreatTextAsNull)
        yield return $"TreatTextAsNull : {TreatTextAsNull} - {other.TreatTextAsNull}";

      if (!other.TreatTextAsNull.Equals(TreatTextAsNull, StringComparison.OrdinalIgnoreCase))
        yield return $"TreatTextAsNull : {TreatTextAsNull} - {other.TreatTextAsNull}";

      if (!other.TemplateName.Equals(TemplateName, StringComparison.Ordinal))
        yield return $"TemplateName : {TemplateName} - {other.TemplateName}";

      if (!other.SqlStatement.Equals(SqlStatement, StringComparison.Ordinal))
        yield return $"SqlStatement : {SqlStatement} - {other.SqlStatement}";

      if (!other.Footer.Equals(Footer, StringComparison.Ordinal))
        yield return $"Footer : {Footer} - {other.Footer}";

      if (!other.Header.Equals(Header, StringComparison.Ordinal))
        yield return $"Header : {Header} - {other.Header}";

      if (other.KeepUnencrypted != KeepUnencrypted)
        yield return $"KeepUnencrypted : {KeepUnencrypted} - {other.KeepUnencrypted}";

      if (!other.MappingCollection.Equals(MappingCollection))
        yield return $"MappingCollection different";
      
      if (!other.SamplesAndErrors.Equals(SamplesAndErrors))
        yield return $"SamplesAndErrors different";
            
      if (!other.Comment.Equals(Comment, StringComparison.Ordinal))
        yield return $"Comment : {Comment} - {other.Comment}";
      
      if (!other.ColumnCollection.Equals(ColumnCollection))
        yield return $"ColumnCollection different";
    }
  }
}