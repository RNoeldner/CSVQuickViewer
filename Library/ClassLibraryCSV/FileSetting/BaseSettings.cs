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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
#if XmlSerialization
using System.Xml;
using System.Xml.Serialization;
#endif

// ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract

namespace CsvTools
{
  /// <inheritdoc cref="IFileSetting" />
  /// <summary>
  ///   Abstract calls containing the basic setting for an IFileSetting if contains <see
  ///   cref="P:CsvTools.BaseSettings.ColumnCollection" /> and <see
  ///   cref="P:CsvTools.BaseSettings.MappingCollection" />
  /// </summary>
  [DebuggerDisplay("Settings: {ID} ({ColumnCollection.Count()} Columns)")]
  public abstract class BaseSettings : ObservableObject, IFileSetting
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
    private string m_Id;
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
    private FileStettingStatus m_Status = FileStettingStatus.None;
    private readonly ReaderWriterLockSlim m_LockStatus = new ReaderWriterLockSlim();

    public  event EventHandler<PropertyChangedEventArgs<string>>? IdChanged;

    /// <summary>
    ///   Initializes a new instance of the <see cref="BaseSettings" /> class.
    /// </summary>
    protected BaseSettings(in string id)
    {
      m_Id = id ?? string.Empty;
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
#if XmlSerialization
    [XmlIgnore]
#endif
    [JsonIgnore]
    public FileStettingStatus Status
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
    ///   Workaround to serialize the ColumnCollection, only needed for XML Serialization
    /// </summary>
#if XmlSerialization
    [XmlElement]
#endif
    [JsonIgnore]
    public virtual ColumnMut[] Format
    {
      get => ColumnCollection.Select(x => new ColumnMut(x)).ToArray();
      set
      {
        ColumnCollection.Clear();
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (value == null)
          return;
        ColumnCollection.AddRange(value.Select(x => x.ToImmutableColumn()));
      }
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlElement]
#endif
    [DefaultValue(false)]
    public virtual bool DisplayRecordNo
    {
      get => m_DisplayRecordNo;
      set
      {
        SetProperty(ref m_DisplayRecordNo, value);
      }
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlElement]
#endif
    [DefaultValue(true)]
    public virtual bool DisplayStartLineNo
    {
      get => m_DisplayStartLineNo;
      set => SetProperty(ref m_DisplayStartLineNo, value);
    }

#if XmlSerialization
    [XmlElement]
#endif
    [DefaultValue(false)]
    public virtual bool SetLatestSourceTimeForWrite
    {
      get => m_SetLatestSourceTimeForWrite;
      set => SetProperty(ref m_SetLatestSourceTimeForWrite, value);
    }

#if XmlSerialization
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
#endif

#if XmlSerialization
    [XmlIgnore]
#endif
    public ColumnCollection ColumnCollection { get; } = new ColumnCollection();

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
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
#if XmlSerialization
    [XmlElement]
#endif
    [DefaultValue(false)]
    public virtual bool DisplayEndLineNo
    {
      get => m_DisplayEndLineNo;
      set => SetProperty(ref m_DisplayEndLineNo, value);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(0)]
    public virtual long ErrorCount
    {
      get => m_ErrorCount;
      set => SetProperty(ref m_ErrorCount, value);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
#if XmlSerialization
    [XmlElement]
#endif
    public SampleAndErrorsInformation SamplesAndErrors { get; set; } = new SampleAndErrorsInformation();

    /// <inheritdoc />
    [DefaultValue("")]
    public virtual string Footer
    {
      get => m_Footer;
      set => SetProperty(ref m_Footer, (value ?? string.Empty).HandleCrlfCombinations(Environment.NewLine));
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
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
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue("")]
    public virtual string ID
    {
      get => m_Id;
      set
      {
        var oldVal = m_Id;
        if (SetProperty(ref m_Id, value ?? string.Empty))
          IdChanged?.Invoke(this, new PropertyChangedEventArgs<string>(nameof(ID), oldVal, m_Id));
      }
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
#if XmlSerialization
    [XmlAttribute(AttributeName = "IsCritical")]
#endif
    [DefaultValue(false)]
    public virtual bool InOverview
    {
      get => m_InOverview;
      set => SetProperty(ref m_InOverview, value);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(100)]
    public virtual int Order
    {
      get => m_Order;
      set => SetProperty(ref m_Order, value);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue("")]
    public virtual string Comment
    {
      get => m_Comment;
      set => SetProperty(ref m_Comment, value ?? string.Empty);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(true)]
    public virtual bool IsEnabled
    {
      get => m_IsEnabled;
      set => SetProperty(ref m_IsEnabled, value);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(false)]
    public bool KeepUnencrypted
    {
      get => m_KeepUnencrypted;
      set => SetProperty(ref m_KeepUnencrypted, value);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
#if XmlSerialization
    [XmlIgnore]
#endif
    [JsonIgnore]
    public DateTime LatestSourceTimeUtc
    {
      get
      {
        if (m_LatestSourceTimeUtc == ZeroTime)
          CalculateLatestSourceTime();
        return m_LatestSourceTimeUtc;
      }

      set => SetProperty(ref m_LatestSourceTimeUtc, value);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
#if XmlSerialization
    [XmlElement("Mapping")]
#endif
    public MappingCollection MappingCollection { get; } = new MappingCollection();

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(0)]
    public virtual long NumRecords
    {
      get => m_NumRecords;
      set => SetProperty(ref m_NumRecords, value);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    public virtual DateTime ProcessTimeUtc
    {
      get => m_ProcessTimeUtc;
      set => SetProperty(ref m_ProcessTimeUtc, value);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
#if XmlSerialization
    [XmlIgnore]
#endif
    [JsonIgnore]
    [DefaultValue(false)]
    public virtual bool RecentlyLoaded { get; set; }

    /// <inheritdoc />
#if XmlSerialization
    [XmlElement]
#endif
    [DefaultValue(0)]
    public virtual long RecordLimit
    {
      get => m_RecordLimit;
      set => SetProperty(ref m_RecordLimit, value);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(true)]
    public virtual bool ShowProgress
    {
      get => m_ShowProgress;
      set => SetProperty(ref m_ShowProgress, value);
    }

#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(false)]
    public virtual bool SkipDuplicateHeader
    {
      get => m_SkipDuplicateHeader;
      set => SetProperty(ref m_SkipDuplicateHeader, value);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(true)]
    public virtual bool SkipEmptyLines
    {
      get => m_SkipEmptyLines;
      set => SetProperty(ref m_SkipEmptyLines, value);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(0)]
    public virtual int SkipRows
    {
      get => m_SkipRows;
      set => SetProperty(ref m_SkipRows, value);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
#if XmlSerialization
    [XmlIgnore]
#endif
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
#if XmlSerialization
    [XmlIgnore]
#endif
    [DefaultValue("")]
    public virtual string SqlStatement
    {
      get => m_SqlStatement;
      set
      {
        if (!SetProperty(ref m_SqlStatement, (value ?? string.Empty).NoControlCharacters().HandleCrlfCombinations())) 
          return;
        // Need to assume we have new sources, it has to be recalculated
        SourceFileSettings = null;
        // Reset the process time as well
        ProcessTimeUtc = ZeroTime;
        LatestSourceTimeUtc = ZeroTime;
      }
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
#if XmlSerialization
    [XmlElement]
#endif
    [DefaultValue("")]
    public virtual string TemplateName
    {
      get => m_TemplateName;
      set => SetProperty(ref m_TemplateName, value ?? string.Empty);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(90)]
    public virtual int Timeout
    {
      get => m_Timeout;
      set => SetProperty(ref m_Timeout, value > 0 ? value : 0);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(false)]
    public virtual bool TreatNBSPAsSpace
    {
      get => m_TreatNbspAsSpace;
      set => SetProperty(ref m_TreatNbspAsSpace, value);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(cTreatTextAsNull)]
    public virtual string TreatTextAsNull
    {
      get => m_TreatTextAsNull;
      set => SetProperty(ref m_TreatTextAsNull, value ?? cTreatTextAsNull);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(TrimmingOptionEnum.Unquoted)]
    public virtual TrimmingOptionEnum TrimmingOption
    {
      get => m_TrimmingOption;
      set => SetProperty(ref m_TrimmingOption, value);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
#if XmlSerialization
    [XmlAttribute(AttributeName = "IsImported")]
#endif
    [DefaultValue(true)]
    public virtual bool Validate
    {
      get => m_Validate;
      set => SetProperty(ref m_Validate, value);
    }

    /// <inheritdoc />
    ///<remarks>TODO: This is not used for the Viewer, ideally this should be moved to other class</remarks>
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(0)]
    public virtual long WarningCount
    {
      get => m_WarningCount;
      set => SetProperty(ref m_WarningCount, value);
    }

    /// <inheritdoc />
#if XmlSerialization
    [XmlAttribute]
#endif
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

    [JsonIgnore] public int CollectionIdentifier => ID.GetHashCode();
  }
}