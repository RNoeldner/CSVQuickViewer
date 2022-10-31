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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace CsvTools
{
  /// <inheritdoc cref="IFileSetting" />
  /// <summary>
  ///   Abstract calls containing the basic setting for an IFileSetting if contains <see
  ///   cref="P:CsvTools.BaseSettings.ColumnCollection" /> and <see
  ///   cref="P:CsvTools.BaseSettings.MappingCollection" />
  /// </summary>
  [DebuggerDisplay("Settings: {ID} ({ColumnCollection.Count()} Columns)")]
  public abstract class BaseSettings : NotifyPropertyChangedBase, IFileSetting
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

    private TrimmingOptionEnum m_TrimmingOption = TrimmingOptionEnum.Unquoted;

    private bool m_Validate = true;

    private long m_WarningCount;
    private int m_Order = 100;
    private string m_Comment = string.Empty;
    private int m_Status;
    private readonly ReaderWriterLockSlim m_LockStatus = new ReaderWriterLockSlim();

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

      MappingCollection.CollectionChanged += (sender, e) =>
      {
        if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Add)
          NotifyPropertyChanged(nameof(MappingCollection));
      };
      MappingCollection.CollectionItemPropertyChanged +=
        (sender, e) => NotifyPropertyChanged(nameof(MappingCollection));
    }

    /// <inheritdoc />
    [XmlIgnore]
    [JsonIgnore]
    public int Status
    {
      get
      {
        try
        {
          m_LockStatus.EnterReadLock();
          return m_Status;
        }
        finally
        {
          m_LockStatus.ExitReadLock();
        }
      }
      set
      {
        if (value.Equals(Status))
          return;
        m_LockStatus.EnterWriteLock();
        m_Status = value;
        m_LockStatus.ExitWriteLock();
        NotifyPropertyChanged();
      }
    }

    /// <summary>
    ///   Workaround to serialize, the ColumnCollection
    /// </summary>
    /// <value>The column options</value>
    [XmlElement]
    [JsonIgnore]
    public virtual Column[] Format
    {
      get
      {
        var res = new Column[ColumnCollection.Count];
        for (var index = 0; index < ColumnCollection.Count; index++)
          res[index] = ColumnCollection[index].ToMutableColumn();
        return res;
      }
      set
      {
        ColumnCollection.Clear();
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (value == null) return;
        ColumnCollection.AddRange(value);
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
        SetField(ref m_DisplayRecordNo, value);
      }
    }

    /// <inheritdoc />
    [XmlElement]
    [DefaultValue(true)]
    public virtual bool DisplayStartLineNo
    {
      get => m_DisplayStartLineNo;
      set => SetField(ref m_DisplayStartLineNo, value);
    }

    [XmlElement]
    [DefaultValue(false)]
    public virtual bool SetLatestSourceTimeForWrite
    {
      get => m_SetLatestSourceTimeForWrite;
      set => SetField(ref m_SetLatestSourceTimeForWrite, value);
    }

    /// <summary>
    ///   Gets a value indicating whether FileLastWriteTimeUtc is specified.
    /// </summary>
    /// <value><c>true</c> if specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [JsonIgnore]
    public bool FileLastWriteTimeUtcSpecified => ProcessTimeUtc != ZeroTime;

    /// <summary>
    ///   Gets a value indicating whether field mapping specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [JsonIgnore]
    public bool MappingSpecified => MappingCollection.Count > 0;

    [JsonIgnore] public bool ProcessTimeUtcSpecified => m_ProcessTimeUtc != ZeroTime;

    [JsonIgnore]
    public bool SamplesAndErrorsSpecified =>
      SamplesAndErrors.ErrorsSpecified || SamplesAndErrors.SamplesSpecified || SamplesAndErrors.NumErrors != -1;

    /// <summary>
    ///   Utility calls to get or set the SQL Statement as CDataSection
    /// </summary>
    [DefaultValue("")]
    [JsonIgnore]
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
    [JsonIgnore]
    public bool SqlStatementCDataSpecified => !string.IsNullOrEmpty(SqlStatement);

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
        NotifyPropertyChanged();
      }
    }

    /// <inheritdoc />
    [XmlElement]
    [DefaultValue(false)]
    public virtual bool DisplayEndLineNo
    {
      get => m_DisplayEndLineNo;
      set => SetField(ref m_DisplayEndLineNo, value);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
    [XmlAttribute]
    [DefaultValue(0)]
    public virtual long ErrorCount
    {
      get => m_ErrorCount;
      set => SetField(ref m_ErrorCount, value);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
    [XmlElement]
    public SampleAndErrorsInformation SamplesAndErrors { get; set; } = new SampleAndErrorsInformation();

    /// <inheritdoc />
    [DefaultValue("")]
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
      set => SetField(ref m_HasFieldHeader, value);
    }

    /// <inheritdoc />
    [DefaultValue("")]
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
    public virtual string ID
    {
      get => m_Id;
      set
      {
        if (SetField(ref m_Id, value, StringComparison.Ordinal, true))
          NotifyPropertyChanged(nameof(InternalID));
      }
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
    [XmlAttribute(AttributeName = "IsCritical")]
    [DefaultValue(false)]
    public virtual bool InOverview
    {
      get => m_InOverview;
      set => SetField(ref m_InOverview, value);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
    [XmlAttribute]
    [DefaultValue(100)]
    public virtual int Order
    {
      get => m_Order;
      set => SetField(ref m_Order, value);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
    [XmlAttribute]
    [DefaultValue("")]
    public virtual string Comment
    {
      get => m_Comment;
      set => SetField(ref m_Comment, value, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
    [XmlIgnore]
    [JsonIgnore]
    public virtual string InternalID => ID;

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
    [XmlAttribute]
    [DefaultValue(true)]
    public virtual bool IsEnabled
    {
      get => m_IsEnabled;

      set => SetField(ref m_IsEnabled, value);
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(false)]
    public bool KeepUnencrypted
    {
      get => m_KeepUnencrypted;
      set => SetField(ref m_KeepUnencrypted, value);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
    [XmlIgnore]
    [JsonIgnore]
    public DateTime LatestSourceTimeUtc
    {
      get
      {
        if (m_LatestSourceTimeUtc == ZeroTime)
          CalculateLatestSourceTime();
        return m_LatestSourceTimeUtc;
      }

      set => SetField(ref m_LatestSourceTimeUtc, value);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
    [XmlElement("Mapping")]
    public MappingCollection MappingCollection { get; } = new MappingCollection();

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
    [XmlAttribute]
    [DefaultValue(0)]
    public virtual long NumRecords
    {
      get => m_NumRecords;
      set => SetField(ref m_NumRecords, value);
    }

    /// <inheritdoc />
    [XmlAttribute]
    public virtual DateTime ProcessTimeUtc
    {
      get => m_ProcessTimeUtc;
      set => SetField(ref m_ProcessTimeUtc, value);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
    [XmlIgnore]
    [JsonIgnore]
    [DefaultValue(false)]
    public virtual bool RecentlyLoaded { get; set; }

    /// <inheritdoc />
    [XmlElement]
    [DefaultValue(0)]
    public virtual long RecordLimit
    {
      get => m_RecordLimit;
      set => SetField(ref m_RecordLimit, value);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
    [XmlAttribute]
    [DefaultValue(true)]
    public virtual bool ShowProgress
    {
      get => m_ShowProgress;
      set => SetField(ref m_ShowProgress, value);
    }

    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool SkipDuplicateHeader
    {
      get => m_SkipDuplicateHeader;
      set => SetField(ref m_SkipDuplicateHeader, value);
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(true)]
    public virtual bool SkipEmptyLines
    {
      get => m_SkipEmptyLines;
      set => SetField(ref m_SkipEmptyLines, value);
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(0)]
    public virtual int SkipRows
    {
      get => m_SkipRows;
      set => SetField(ref m_SkipRows, value);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
    [XmlIgnore]
    [JsonIgnore]
    public IReadOnlyCollection<IFileSetting>? SourceFileSettings
    {
      get => m_SourceFileSettings;
      set
      {
        m_SourceFileSettings = value;
        NotifyPropertyChanged();
      }
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
    [XmlIgnore]
    [DefaultValue("")]
    public virtual string SqlStatement
    {
      get => m_SqlStatement;
      set
      {
        if (!SetField(ref m_SqlStatement, (value ?? string.Empty).NoControlCharacters().HandleCrlfCombinations(),
              StringComparison.Ordinal, true)) return;
        // Need to assume we have new sources, it has to be recalculated
        SourceFileSettings = null;
        // Reset the process time as well
        ProcessTimeUtc = ZeroTime;
        LatestSourceTimeUtc = ZeroTime;
      }
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
    [XmlElement]
    [DefaultValue("")]
    public virtual string TemplateName
    {
      get => m_TemplateName;
      set => SetField(ref m_TemplateName, value, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
    [XmlAttribute]
    [DefaultValue(90)]
    public virtual int Timeout
    {
      get => m_Timeout;
      set => SetField(ref m_Timeout, value > 0 ? value : 0);
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool TreatNBSPAsSpace
    {
      get => m_TreatNbspAsSpace;
      set => SetField(ref m_TreatNbspAsSpace, value);
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(cTreatTextAsNull)]
    public virtual string TreatTextAsNull
    {
      get => m_TreatTextAsNull;
      set => SetField(ref m_TreatTextAsNull, value, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(TrimmingOptionEnum.Unquoted)]
    public virtual TrimmingOptionEnum TrimmingOption
    {
      get => m_TrimmingOption;
      set => SetField(ref m_TrimmingOption, value);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
    [XmlAttribute(AttributeName = "IsImported")]
    [DefaultValue(true)]
    public virtual bool Validate
    {
      get => m_Validate;
      set => SetField(ref m_Validate, value);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
    [XmlAttribute]
    [DefaultValue(0)]
    public virtual long WarningCount
    {
      get => m_WarningCount;
      set => SetField(ref m_WarningCount, value);
    }

    /// <inheritdoc />
    [XmlAttribute]
    public virtual DateTime LastChange
    {
      get;
      set;
    } = DateTime.UtcNow;

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
    public virtual void CalculateLatestSourceTime() => LatestSourceTimeUtc = ProcessTimeUtc;

    /// <inheritdoc />
    public abstract object Clone();

    /// <inheritdoc />
    public abstract void CopyTo(IFileSetting other);

    /// <inheritdoc />
    public abstract bool Equals(IFileSetting? other);

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
      other.MappingCollection.Clear();
      other.MappingCollection.AddRange(MappingCollection);

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
      other.ColumnCollection.Clear();
      other.ColumnCollection.AddRange(ColumnCollection);

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
      // should be done last... but it might well be trigger later
      other.LastChange = LastChange;
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

    protected override void NotifyPropertyChanged([CallerMemberName] string name = "")
    {
      LastChange = DateTime.UtcNow;
      base.NotifyPropertyChanged(name);
    }

    /// <inheritdoc />
    public virtual IEnumerable<string> GetDifferences(IFileSetting other)
    {
      if (!other.GetType().FullName.Equals(GetType().FullName, StringComparison.OrdinalIgnoreCase))
        yield return $"Type : {GetType().FullName} - {other.GetType().FullName}";

      if (!other.ID.Equals(ID, StringComparison.OrdinalIgnoreCase))
        yield return $"{nameof(ID)}: {ID} - {other.ID}";

      if (other.SkipRows != SkipRows)
        yield return $"{nameof(SkipRows)}: {SkipRows} - {other.SkipRows}";

      if (other.HasFieldHeader != HasFieldHeader)
        yield return $"{nameof(HasFieldHeader)} : {HasFieldHeader} - {other.HasFieldHeader}";

      if (other.RecentlyLoaded != RecentlyLoaded)
        yield return $"{nameof(RecentlyLoaded)} : {RecentlyLoaded} - {other.RecentlyLoaded}";

      if (other.IsEnabled != IsEnabled)
        yield return $"{nameof(IsEnabled)} : {IsEnabled} - {other.IsEnabled}";

      if (other.InOverview != InOverview)
        yield return $"{nameof(InOverview)} : {InOverview} - {other.InOverview}";

      if (other.Validate != Validate)
        yield return $"{nameof(Validate)} : {Validate} - {other.Validate}";

      if (other.ShowProgress != ShowProgress)
        yield return $"{nameof(ShowProgress)} : {ShowProgress} - {other.ShowProgress}";

      if (other.TreatNBSPAsSpace != TreatNBSPAsSpace)
        yield return $"{nameof(TreatNBSPAsSpace)} : {TreatNBSPAsSpace} - {other.TreatNBSPAsSpace}";

      if (other.ConsecutiveEmptyRows != ConsecutiveEmptyRows)
        yield return $"{nameof(ConsecutiveEmptyRows)} : {ConsecutiveEmptyRows} - {other.ConsecutiveEmptyRows}";

      if (other.DisplayStartLineNo != DisplayStartLineNo)
        yield return $"{nameof(DisplayStartLineNo)} : {DisplayStartLineNo} - {other.DisplayStartLineNo}";

      if (other.DisplayEndLineNo != DisplayEndLineNo)
        yield return $"{nameof(DisplayEndLineNo)} : {DisplayEndLineNo} - {other.DisplayEndLineNo}";

      if (other.DisplayRecordNo != DisplayRecordNo)
        yield return $"{nameof(DisplayRecordNo)} : {DisplayRecordNo} - {other.DisplayRecordNo}";

      if (other.RecordLimit != RecordLimit)
        yield return $"{nameof(RecordLimit)} : {RecordLimit} - {other.RecordLimit}";

      if (other.SkipEmptyLines != SkipEmptyLines)
        yield return $"{nameof(SkipEmptyLines)} : {SkipEmptyLines} - {other.SkipEmptyLines}";

      if (other.SkipDuplicateHeader != SkipDuplicateHeader)
        yield return $"{nameof(SkipDuplicateHeader)} : {SkipDuplicateHeader} - {other.SkipDuplicateHeader}";

      if (other.Timeout != Timeout)
        yield return $"{nameof(Timeout)} : {Timeout} - {other.Timeout}";

      if (other.TreatTextAsNull != TreatTextAsNull)
        yield return $"{nameof(TreatTextAsNull)} : {TreatTextAsNull} - {other.TreatTextAsNull}";

      if (!other.TreatTextAsNull.Equals(TreatTextAsNull, StringComparison.OrdinalIgnoreCase))
        yield return $"{nameof(TreatTextAsNull)} : {TreatTextAsNull} - {other.TreatTextAsNull}";

      if (!other.TemplateName.Equals(TemplateName, StringComparison.Ordinal))
        yield return $"{nameof(TemplateName)} : {TemplateName} - {other.TemplateName}";

      if (!other.SqlStatement.Equals(SqlStatement, StringComparison.Ordinal))
        yield return $"{nameof(SqlStatement)} : {SqlStatement} - {other.SqlStatement}";

      if (!other.Footer.Equals(Footer, StringComparison.Ordinal))
        yield return $"{nameof(Footer)} : {Footer} - {other.Footer}";

      if (!other.Header.Equals(Header, StringComparison.Ordinal))
        yield return $"{nameof(Header)} : {Header} - {other.Header}";

      if (other.KeepUnencrypted != KeepUnencrypted)
        yield return $"{nameof(KeepUnencrypted)} : {KeepUnencrypted} - {other.KeepUnencrypted}";

      if (!other.MappingCollection.Equals(MappingCollection))
        yield return $"{nameof(MappingCollection)} different";

      if (!other.SamplesAndErrors.Equals(SamplesAndErrors))
        yield return $"{nameof(SamplesAndErrors)} different";

      if (!other.Comment.Equals(Comment, StringComparison.Ordinal))
        yield return $"{nameof(Comment)} : {Comment} - {other.Comment}";

      if (!other.ColumnCollection.Equals(ColumnCollection))
        yield return $"{nameof(ColumnCollection)} different";
    }

    [JsonIgnore] public int CollectionIdentifier => InternalID.IdentifierHash();
  }
}