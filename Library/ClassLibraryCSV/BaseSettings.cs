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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace CsvTools
{
  // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  /// <summary>
  ///   Abstract calls containing the basic setting for an IFileSetting if contains <see
  ///   cref="ColumnCollection" />, <see cref="MappingCollection" /> and <see cref="FileFormat" />
  /// </summary>
#pragma warning disable CS0659

  public abstract class BaseSettings
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  {
    public static readonly DateTime ZeroTime = new DateTime(0, DateTimeKind.Utc);

    private readonly FileFormat m_FileFormat = new FileFormat();
    private int m_ConsecutiveEmptyRows = 5;
    private bool m_DisplayEndLineNo;
    private bool m_DisplayRecordNo;
    private bool m_DisplayStartLineNo = true;
    private bool m_SetLatestSourceTimeForWrite;
    private DateTime m_ProcessTimeUtc = ZeroTime;
    private DateTime m_LatestSourceTimeUtc = ZeroTime;
    private string m_FileName;
    private long m_FileSize;
    private string m_Footer = string.Empty;
    private bool m_FullPathInitialized;
    private string m_FullPath = string.Empty;
    private bool m_HasFieldHeader = true;
    private string m_Header = string.Empty;
    private string m_Id = string.Empty;
    private bool m_InOverview;
    private bool m_IsEnabled = true;
    private int m_EvidenceNumberOrIssues = -1;
    private long m_NumRecords;
    private long m_WarningCount = -1;
    private long m_ErrorCount = -1;
    private string m_Passphrase = string.Empty;
    private string m_Recipient = string.Empty;
    private long m_RecordLimit;
    private string m_RemoteFileName = string.Empty;
    private bool m_ThrowErrorIfNotExists = true;
    private bool m_ShowProgress = true;
    private int m_SkipRows;
    private string m_SqlStatement = string.Empty;
    private int m_Timeout = 90;
    private string m_TemplateName = string.Empty;
    private bool m_TreatNbspAsSpace;
    private string m_TreatTextAsNull = "NULL";
    private bool m_Validate = true;
    private bool m_SkipDuplicateHeader;
    private bool m_SkipEmptyLines = true;
    private ObservableCollection<SampleRecordEntry> m_Samples = new ObservableCollection<SampleRecordEntry>();
    private ObservableCollection<SampleRecordEntry> m_Errors = new ObservableCollection<SampleRecordEntry>();
    private TrimmingOption m_TrimmingOption = TrimmingOption.Unquoted;
    private IReadOnlyCollection<IFileSetting> m_SourceFileSettings;

    /// <summary>
    ///   Initializes a new instance of the <see cref="BaseSettings" /> class.
    /// </summary>
    /// <param name="fileName">The filename.</param>
    protected BaseSettings(string fileName)
    {
      m_FileName = FileNameFix(fileName);
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
    ///   Initializes a new instance of the <see cref="BaseSettings" /> class.
    /// </summary>
    protected BaseSettings() : this(string.Empty)
    {
    }

    /// <summary>
    ///   Gets a value indicating whether field mapping specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [XmlIgnore]
    public bool MappingSpecified => MappingCollection.Count > 0;

    [XmlIgnore]
    public bool ErrorsSpecified => Errors.Count > 0;

    /// <summary>
    ///   Storage for the settings used as direct or indirect sources.
    /// </summary>
    /// <remarks>
    ///   This is used for queries that might refer to data that is produced by other settings but
    ///   not for file setting pointing to a specific physical file
    /// </remarks>
    /// <example>
    ///   A setting A using setting B that is dependent on C1 and C2 both dependent on D-&gt; A is
    ///   {B,C1,C2,D}. B is {C1,C2,D}, C1 is {D} C2 is {D}
    /// </example>
    [XmlIgnore]
    [CanBeNull]
    public IReadOnlyCollection<IFileSetting> SourceFileSettings
    {
      get => m_SourceFileSettings;
      [NotifyPropertyChangedInvocator]
      set
      {
        if (m_SourceFileSettings == null && value == null) return;
        if (value != null && value.CollectionEqual(m_SourceFileSettings)) return;
        // do not notify if we change from null to an empty list
        var notify = (value?.Count() > 0 || m_SourceFileSettings != null);
        m_SourceFileSettings = value;
        if (notify)
          NotifyPropertyChanged(nameof(SourceFileSettings));
      }
    }

    /// <summary>
    ///   Gets a value indicating whether FileFormat is specified.
    /// </summary>
    /// <value><c>true</c> if specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [XmlIgnore]
    public bool FileFormatSpecified => !FileFormat.Equals(new FileFormat());

    /// <summary>
    ///   Gets a value indicating whether FileLastWriteTimeUtc is specified.
    /// </summary>
    /// <value><c>true</c> if specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [XmlIgnore]
    public bool FileLastWriteTimeUtcSpecified => ProcessTimeUtc != ZeroTime;

    /// <summary>
    ///   Gets or sets the name of the file.
    /// </summary>
    /// <value>The name of the file.</value>
    [XmlAttribute]
    [DefaultValue("")]
    public virtual string RemoteFileName
    {
      [NotNull]
      get => m_RemoteFileName;
      [CanBeNull]
      [NotifyPropertyChangedInvocator]
      set
      {
        var newVal = value ?? string.Empty;
        if (m_RemoteFileName.Equals(newVal, StringComparison.Ordinal))
          return;

        m_RemoteFileName = newVal;
        NotifyPropertyChanged(nameof(RemoteFileName));
      }
    }

    /// <summary>
    ///   Gets or sets the name of the file.
    /// </summary>
    /// <value>The name of the file.</value>
    [XmlAttribute]
    [DefaultValue(true)]
    public virtual bool ThrowErrorIfNotExists
    {
      get => m_ThrowErrorIfNotExists;
      [NotifyPropertyChangedInvocator]
      set
      {
        if (m_ThrowErrorIfNotExists.Equals(value))
          return;
        m_ThrowErrorIfNotExists = value;
        NotifyPropertyChanged(nameof(ThrowErrorIfNotExists));
      }
    }

    [XmlIgnore]
    public bool SamplesSpecified => Samples.Count > 0;

    /// <summary>
    ///   Utility calls to get or set the SQL Statement as CDataSection
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId =
     "System.Xml.XmlNode")]
    [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public virtual methods", MessageId = "0")]
    [DefaultValue("")]
    public XmlCDataSection SqlStatementCData
    {
      get
      {
        var doc = new XmlDocument();
        return doc.CreateCDataSection(SqlStatement);
      }
      set => SetSqlStatementRename(value.Value);
    }

    /// <summary>
    ///   Gets a value indicating whether SqlStatementCData is specified.
    /// </summary>
    /// <value><c>true</c> if specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [XmlIgnore]
    public bool SqlStatementCDataSpecified => !string.IsNullOrEmpty(SqlStatement);

    /// <summary>
    ///   Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other" />
    ///   parameter; otherwise, <see langword="false" />.
    /// </returns>
    protected virtual bool BaseSettingsEquals(BaseSettings other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;

      if (!(other is IFileSettingPhysicalFile otherRemote))
        return string.Equals(other.TemplateName, TemplateName, StringComparison.OrdinalIgnoreCase) &&
               other.SkipRows == SkipRows &&
               other.IsEnabled == IsEnabled &&
               other.TreatNBSPAsSpace == TreatNBSPAsSpace &&
               other.DisplayStartLineNo == DisplayStartLineNo &&
               other.DisplayEndLineNo == DisplayEndLineNo &&
               other.HasFieldHeader == HasFieldHeader &&
               other.InOverview == InOverview &&
               other.Validate == Validate &&
               other.TrimmingOption == TrimmingOption &&
               other.ConsecutiveEmptyRows == m_ConsecutiveEmptyRows &&
               string.Equals(other.TreatTextAsNull, TreatTextAsNull, StringComparison.OrdinalIgnoreCase) &&
               other.DisplayRecordNo == DisplayRecordNo &&
               other.RecordLimit == RecordLimit &&
               other.ShowProgress == ShowProgress &&
               string.Equals(other.ID, ID, StringComparison.OrdinalIgnoreCase) &&
               other.FileFormat.Equals(FileFormat) &&
               other.Passphrase.Equals(Passphrase, StringComparison.Ordinal) &&
               other.Recipient.Equals(Recipient, StringComparison.Ordinal) &&
               other.EvidenceNumberOrIssues == EvidenceNumberOrIssues &&
               other.SkipEmptyLines == SkipEmptyLines &&
               other.SkipDuplicateHeader == SkipDuplicateHeader &&
               other.Timeout == Timeout &&
               other.ProcessTimeUtc == ProcessTimeUtc &&
               other.SetLatestSourceTimeForWrite == SetLatestSourceTimeForWrite &&
               string.Equals(other.SqlStatement, SqlStatement, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(other.Footer, Footer, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(other.Header, Header, StringComparison.OrdinalIgnoreCase) &&
               MappingCollection.Equals(other.MappingCollection) && Samples.CollectionEqual(other.Samples) &&
               Errors.CollectionEqual(other.Errors) &&
               ColumnCollection.Equals(other.ColumnCollection);
      if (otherRemote.RemoteFileName != RemoteFileName ||
          otherRemote.ThrowErrorIfNotExists != ThrowErrorIfNotExists ||
          otherRemote.FileSize != FileSize ||
          !string.Equals(otherRemote.FileName, FileName, StringComparison.OrdinalIgnoreCase)
      )
        return false;

      return string.Equals(other.TemplateName, TemplateName, StringComparison.OrdinalIgnoreCase) &&

             other.SkipRows == SkipRows &&
             other.IsEnabled == IsEnabled &&
             other.TreatNBSPAsSpace == TreatNBSPAsSpace &&
             other.DisplayStartLineNo == DisplayStartLineNo &&
             other.DisplayEndLineNo == DisplayEndLineNo &&
             other.HasFieldHeader == HasFieldHeader &&
             other.InOverview == InOverview &&
             other.Validate == Validate &&
             other.TrimmingOption == TrimmingOption &&
             other.ConsecutiveEmptyRows == m_ConsecutiveEmptyRows &&
             string.Equals(other.TreatTextAsNull, TreatTextAsNull, StringComparison.OrdinalIgnoreCase) &&
             other.DisplayRecordNo == DisplayRecordNo &&
             other.RecordLimit == RecordLimit &&
             other.ShowProgress == ShowProgress &&
             string.Equals(other.ID, ID, StringComparison.OrdinalIgnoreCase) &&
             other.FileFormat.Equals(FileFormat) &&
             other.Passphrase.Equals(Passphrase, StringComparison.Ordinal) &&
             other.Recipient.Equals(Recipient, StringComparison.Ordinal) &&
             other.EvidenceNumberOrIssues == EvidenceNumberOrIssues &&
             other.SkipEmptyLines == SkipEmptyLines &&
             other.SkipDuplicateHeader == SkipDuplicateHeader &&
             other.Timeout == Timeout &&
             other.ProcessTimeUtc == ProcessTimeUtc &&
             other.LatestSourceTimeUtc == LatestSourceTimeUtc &&
             other.SetLatestSourceTimeForWrite == SetLatestSourceTimeForWrite &&
             string.Equals(other.SqlStatement, SqlStatement, StringComparison.OrdinalIgnoreCase) &&
             string.Equals(other.Footer, Footer, StringComparison.OrdinalIgnoreCase) &&
             string.Equals(other.Header, Header, StringComparison.OrdinalIgnoreCase) &&
             MappingCollection.Equals(other.MappingCollection) && Samples.CollectionEqual(other.Samples) &&
             Errors.CollectionEqual(other.Errors) &&
             ColumnCollection.Equals(other.ColumnCollection);
    }

    /// <summary>
    ///   Occurs after a property value changes.
    /// </summary>
    public virtual event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///   Occurs when a string value property changed providing information on old and new value
    /// </summary>
    public virtual event EventHandler<PropertyChangedEventArgs<string>> PropertyChangedString;

    /// <summary>
    ///   Gets or sets the number consecutive empty rows that should finish a read
    /// </summary>
    /// <value>The consecutive empty rows.</value>
    [XmlAttribute]
    [DefaultValue(5)]
    public virtual int ConsecutiveEmptyRows
    {
      get => m_ConsecutiveEmptyRows;
      [NotifyPropertyChangedInvocator]
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

    /// <summary>
    ///   Gets or sets the options for a column
    /// </summary>
    /// <value>The column options</value>
    [XmlElement("Format")]
    public ColumnCollection ColumnCollection { get; } = new ColumnCollection();

    /// <summary>
    ///   Gets a value indicating whether column format specified.
    /// </summary>
    /// <value><c>true</c> if column format specified; otherwise, <c>false</c>.</value>
    [XmlIgnore]
    public bool ColumnSpecified => ColumnCollection.Count > 0;

    /// <summary>
    ///   Gets or sets a value indicating whether to display end line numbers.
    /// </summary>
    /// <value><c>true</c> if end line no should be displayed; otherwise, <c>false</c>.</value>
    [XmlElement]
    [DefaultValue(false)]
    public virtual bool DisplayEndLineNo
    {
      get => m_DisplayEndLineNo;
      [NotifyPropertyChangedInvocator]
      set
      {
        if (m_DisplayEndLineNo.Equals(value))
          return;
        m_DisplayEndLineNo = value;
        NotifyPropertyChanged(nameof(DisplayEndLineNo));
      }
    }

    /// <summary>
    ///   Gets or sets a value indicating whether to display record no.
    /// </summary>
    /// <value><c>true</c> if record number should be displayed; otherwise, <c>false</c>.</value>
    [XmlElement]
    [DefaultValue(false)]
    public virtual bool DisplayRecordNo
    {
      get => m_DisplayRecordNo;
      [NotifyPropertyChangedInvocator]
      set
      {
        if (m_DisplayRecordNo.Equals(value))
          return;
        m_DisplayRecordNo = value;
        NotifyPropertyChanged(nameof(DisplayRecordNo));
      }
    }

    /// <summary>
    ///   Gets or sets a value indicating whether to display start line numbers
    /// </summary>
    /// <value><c>true</c> if start line no should be displayed; otherwise, <c>false</c>.</value>
    [XmlElement]
    [DefaultValue(true)]
    public virtual bool DisplayStartLineNo
    {
      get => m_DisplayStartLineNo;
      [NotifyPropertyChangedInvocator]
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
      [NotifyPropertyChangedInvocator]
      set
      {
        if (m_SetLatestSourceTimeForWrite.Equals(value))
          return;
        m_SetLatestSourceTimeForWrite = value;
        NotifyPropertyChanged(nameof(SetLatestSourceTimeForWrite));
      }
    }

    public ObservableCollection<SampleRecordEntry> Errors
    {
      [NotNull]
      get => m_Errors;
      [CanBeNull]
      [NotifyPropertyChangedInvocator]
      set
      {
        var newVal = value ?? new ObservableCollection<SampleRecordEntry>();
        if (m_Errors.CollectionEqualWithOrder(newVal))
          return;
        m_Errors = newVal;
        NotifyPropertyChanged(nameof(Errors));
        if (m_EvidenceNumberOrIssues > 0 && Errors.Count > m_EvidenceNumberOrIssues)
          EvidenceNumberOrIssues = Errors.Count;
      }
    }

    /// <summary>
    ///   Gets or sets the file format.
    /// </summary>
    /// <value>The file format.</value>
    [XmlElement]
    public virtual FileFormat FileFormat
    {
      [NotNull]
      get => m_FileFormat;
      [CanBeNull]
      set => value?.CopyTo(m_FileFormat);
    }

    /// <summary>
    ///   The UTC time the file was last written to, or when it was last read, this is different to
    ///   <see cref="LatestSourceTimeUtc" />. Changes to this date should not be considered as
    ///   changes to the configuration
    /// </summary>
    [XmlAttribute]
    public virtual DateTime ProcessTimeUtc
    {
      get => m_ProcessTimeUtc;
      [NotifyPropertyChangedInvocator]
      set
      {
        if (m_ProcessTimeUtc.Equals(value))
          return;
        m_ProcessTimeUtc = value;
        NotifyPropertyChanged(nameof(ProcessTimeUtc));
      }
    }

    [XmlIgnore] public bool ProcessTimeUtcSpecified => (m_ProcessTimeUtc != ZeroTime);


    /// <summary>
    ///   The time of the source, either a file time, or in case the setting is dependent on
    ///   multiple sources the time of the last source Changes to this date should not be considered
    ///   as changes to the configuration
    /// </summary>
    [XmlIgnore]

    public DateTime LatestSourceTimeUtc
    {
      get
      {
        if (m_LatestSourceTimeUtc == ZeroTime)
          CalculateLatestSourceTime();
        return m_LatestSourceTimeUtc;
      }
      [NotifyPropertyChangedInvocator]
      set
      {
        if (m_LatestSourceTimeUtc == value)
          return;
        m_LatestSourceTimeUtc = value;
        NotifyPropertyChanged(nameof(LatestSourceTimeUtc));
      }
    }

    [XmlIgnore]
    public bool HasLatestSourceTimeUtc => m_LatestSourceTimeUtc != ZeroTime;

    /// <summary>
    ///   As this might be a time consuming process, do this only if the time was not determined before
    /// </summary>
    /// <remarks>
    ///   For a physical file ist possibly easiest as it teh file time, overwritten for more complex
    ///   things like a Query
    /// </remarks>
    public virtual void CalculateLatestSourceTime()
    {
      if (this is IFileSettingPhysicalFile settingPhysicalFile)
      {
        var fileName = FileSystemUtils.ResolvePattern(settingPhysicalFile.FullPath);
        m_LatestSourceTimeUtc = string.IsNullOrEmpty(fileName) ? ZeroTime : FileSystemUtils.GetLastWriteTimeUtc(fileName);
      }
      else
        // in case the source is not a physical file, assume it's the processing time
        m_LatestSourceTimeUtc = ProcessTimeUtc;
    }

    [NotNull]
    private static string FileNameFix([CanBeNull] string value)
    {
      var newVal = value ?? string.Empty;
      if (newVal.StartsWith(".\\", StringComparison.Ordinal))
        newVal = newVal.Substring(2);
      return newVal;
    }

    /// <summary>
    ///   Gets or sets the name of the file.
    /// </summary>
    /// <value>The name of the file.</value>
    [XmlAttribute]
    [DefaultValue("")]
    public virtual string FileName
    {
      [NotNull]
      get => m_FileName;
      [CanBeNull]
      [NotifyPropertyChangedInvocator]
      set
      {
        var newVal = FileNameFix(value);

        if (m_FileName.Equals(newVal, StringComparison.Ordinal))
          return;
        var oldValue = m_FileName;
        m_FileName = newVal;
        m_FullPath = null;
        NotifyPropertyChanged(nameof(FileName));
        PropertyChangedString?.Invoke(this, new PropertyChangedEventArgs<string>(nameof(FileName), oldValue, newVal));
      }
    }

    /// <summary>
    ///   Gets or sets the date the file when it was read
    /// </summary>
    /// <value>The consecutive empty rows.</value>
    [XmlAttribute]
    [DefaultValue(0)]
    public virtual long FileSize
    {
      get => m_FileSize;
      [NotifyPropertyChangedInvocator]
      set
      {
        if (value == m_FileSize)
          return;
        m_FileSize = value;
        NotifyPropertyChanged(nameof(FileSize));
      }
    }

    /// <summary>
    ///   Gets or sets the Footer.
    /// </summary>
    /// <value>The Footer for outbound data.</value>
    [DefaultValue("")]
    public virtual string Footer
    {
      [NotNull]
      get => m_Footer;
      [CanBeNull]
      [NotifyPropertyChangedInvocator]
      set
      {
        var newVal = StringUtils.HandleCRLFCombinations(value ?? string.Empty, Environment.NewLine);
        if (m_Footer.Equals(newVal, StringComparison.Ordinal))
          return;
        m_Footer = newVal;
        NotifyPropertyChanged(nameof(Footer));
      }
    }

    public void ResetFullPath() => m_FullPathInitialized = false;

    [XmlIgnore]
    [NotNull]
    public virtual string FullPath
    {
      get
      {
        if (!m_FullPathInitialized)
        {
          m_FullPath = FileSystemUtils.ResolvePattern(m_FileName.GetAbsolutePath(ApplicationSetting.RootFolder));
          if (m_FullPath == null)
            m_FullPath = string.Empty;
          else
            m_FullPathInitialized = true;
        }
        return m_FullPath;
      }
    }

    /// <summary>
    ///   As the data is loaded and not further validation is done this will be set to true Once
    ///   validation is happening and validation errors are stored this is false again. This is
    ///   stored on FileSetting level even as it actually is used for determine th freshness of a
    ///   loaded data in the validator, but there is not suitable data structure
    /// </summary>
    [XmlIgnore]
    [DefaultValue(false)]
    public virtual bool RecentlyLoaded { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether this instance has field header.
    /// </summary>
    /// <value><c>true</c> if this instance has field header; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(true)]
    public virtual bool HasFieldHeader
    {
      get => m_HasFieldHeader;
      [NotifyPropertyChangedInvocator]
      set
      {
        if (m_HasFieldHeader.Equals(value))
          return;
        m_HasFieldHeader = value;
        NotifyPropertyChanged(nameof(HasFieldHeader));
      }
    }

    /// <summary>
    ///   Gets or sets the Footer.
    /// </summary>
    /// <value>The Footer for outbound data.</value>
    [DefaultValue("")]
    public virtual string Header
    {
      [NotNull]
      get => m_Header;
      [CanBeNull]
      [NotifyPropertyChangedInvocator]
      set
      {
        var newVal = StringUtils.HandleCRLFCombinations(value ?? string.Empty, Environment.NewLine);
        if (m_Header.Equals(newVal, StringComparison.Ordinal))
          return;
        m_Header = newVal;
        NotifyPropertyChanged(nameof(Header));
      }
    }

    /// <summary>
    ///   Gets or sets the ID.
    /// </summary>
    /// <value>The ID.</value>
    [XmlAttribute]
    [DefaultValue("")]
    public virtual string ID
    {
      [NotNull]
      get => m_Id;
      [CanBeNull]
      [NotifyPropertyChangedInvocator]
      set
      {
        var newVal = value ?? string.Empty;
        if (m_Id.Equals(newVal, StringComparison.Ordinal))
          return;

        var oldValue = m_Id;
        m_Id = newVal;
        NotifyPropertyChanged(nameof(ID));
        PropertyChangedString?.Invoke(this, new PropertyChangedEventArgs<string>(nameof(ID), oldValue, newVal));
      }
    }

    /// <summary>
    ///   Gets or sets a value indicating whether this instance is critical.
    /// </summary>
    /// <value><c>true</c> if this file is critical for the export; otherwise, <c>false</c>.</value>
    [XmlAttribute(AttributeName = "IsCritical")]
    [DefaultValue(false)]
    public virtual bool InOverview
    {
      get => m_InOverview;
      [NotifyPropertyChangedInvocator]
      set
      {
        if (m_InOverview.Equals(value))
          return;
        m_InOverview = value;
        NotifyPropertyChanged(nameof(InOverview));
      }
    }

    /// <summary>
    ///   The identified to find this specific instance
    /// </summary>
    [XmlIgnore]
    public virtual string InternalID => string.IsNullOrEmpty(ID) ? FileName : ID;

    /// <summary>
    ///   Gets or sets a value indicating whether this instance is enabled.
    /// </summary>
    /// <value><c>true</c> if this file is enabled; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(true)]
    public virtual bool IsEnabled
    {
      get => m_IsEnabled;
      [NotifyPropertyChangedInvocator]
      set
      {
        if (m_IsEnabled.Equals(value))
          return;
        m_IsEnabled = value;
        NotifyPropertyChanged(nameof(IsEnabled));
      }
    }

    /// <summary>
    ///   Gets or sets the field mapping.
    /// </summary>
    /// <value>The field mapping.</value>
    [XmlElement("Mapping")]
    public MappingCollection MappingCollection { get; } = new MappingCollection();

    /// <summary>
    ///   Gets or sets the ID.
    /// </summary>
    /// <value>The ID.</value>
    [XmlAttribute("NumErrors")]
    [DefaultValue(-1)]
    public virtual int EvidenceNumberOrIssues
    {
      get
      {
        if (m_EvidenceNumberOrIssues == -1 && Errors.Count > 0)
          m_EvidenceNumberOrIssues = Errors.Count;
        return m_EvidenceNumberOrIssues;
      }
      [NotifyPropertyChangedInvocator]
      set
      {
        // can not be smaller than the number of named errors
        if (Errors.Count > 0 && value < Errors.Count)
          value = Errors.Count;
        if (m_EvidenceNumberOrIssues == value)
          return;
        m_EvidenceNumberOrIssues = value;
        NotifyPropertyChanged(nameof(EvidenceNumberOrIssues));
      }
    }

    /// <summary>
    ///   Gets or sets the ID.
    /// </summary>
    /// <value>The ID.</value>
    [XmlAttribute]
    [DefaultValue(0)]
    public virtual long NumRecords
    {
      get => m_NumRecords;
      [NotifyPropertyChangedInvocator]
      set
      {
        if (m_NumRecords == value)
          return;
        m_NumRecords = value;
        NotifyPropertyChanged(nameof(NumRecords));
      }
    }

    /// <summary>
    ///   Gets or sets the ID.
    /// </summary>
    /// <value>The ID.</value>
    [XmlAttribute]
    [DefaultValue(-1)]
    public virtual long WarningCount
    {
      get => m_WarningCount;
      [NotifyPropertyChangedInvocator]
      set
      {
        if (m_WarningCount == value)
          return;
        m_WarningCount = value;
        NotifyPropertyChanged(nameof(WarningCount));
      }
    }


    /// <summary>
    ///   Gets or sets the ID.
    /// </summary>
    /// <value>The ID.</value>
    [XmlAttribute]
    [DefaultValue(-1)]
    public virtual long ErrorCount
    {
      get => m_ErrorCount;
      [NotifyPropertyChangedInvocator]
      set
      {
        if (m_ErrorCount == value)
          return;
        m_ErrorCount = value;
        NotifyPropertyChanged(nameof(ErrorCount));
      }
    }
    /// <summary>
    ///   Passphrase for Decryption, will not be stored
    /// </summary>
    [XmlIgnore]
    [DefaultValue("")]
    public virtual string Passphrase
    {
      [NotNull]
      get => m_Passphrase;
      [CanBeNull]
      set => m_Passphrase = (value ?? string.Empty).Trim();
    }

    /// <summary>
    ///   Recipient for a outbound PGP encryption
    /// </summary>
    [XmlAttribute]
    [DefaultValue("")]
    public virtual string Recipient
    {
      [NotNull]
      get => m_Recipient;
      [CanBeNull]
      [NotifyPropertyChangedInvocator]
      set
      {
        var newVal = (value ?? string.Empty).Trim();
        if (m_Recipient.Equals(newVal, StringComparison.Ordinal))
          return;
        m_Recipient = newVal;
        NotifyPropertyChanged(nameof(Recipient));
      }
    }

    /// <summary>
    ///   Gets or sets the record limit.
    /// </summary>
    /// <value>The record limit.</value>
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


    public ObservableCollection<SampleRecordEntry> Samples
    {
      [NotNull]
      get => m_Samples;
      [CanBeNull]
      [NotifyPropertyChangedInvocator]
      set
      {
        var newVal = value ?? new ObservableCollection<SampleRecordEntry>();
        if (m_Samples.CollectionEqualWithOrder(newVal))
          return;
        m_Samples = newVal;
        NotifyPropertyChanged(nameof(Samples));
      }
    }

    /// <summary>
    ///   Gets or sets a value indicating whether to show progress.
    /// </summary>
    /// <value><c>true</c> if progress should be shown; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(true)]
    public virtual bool ShowProgress
    {
      get => m_ShowProgress;
      [NotifyPropertyChangedInvocator]
      set
      {
        if (m_ShowProgress.Equals(value))
          return;
        m_ShowProgress = value;
        NotifyPropertyChanged(nameof(ShowProgress));
      }
    }

    /// <summary>
    ///   Gets or sets a value indicating if the reader will skip empty lines.
    /// </summary>
    /// <value>if <c>true</c> the reader will skip empty lines.</value>
    [XmlAttribute]
    [DefaultValue(true)]
    public virtual bool SkipEmptyLines
    {
      get => m_SkipEmptyLines;
      [NotifyPropertyChangedInvocator]
      set
      {
        if (m_SkipEmptyLines.Equals(value))
          return;
        m_SkipEmptyLines = value;
        NotifyPropertyChanged(nameof(SkipEmptyLines));
      }
    }

    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool SkipDuplicateHeader
    {
      get => m_SkipDuplicateHeader;
      [NotifyPropertyChangedInvocator]
      set
      {
        if (m_SkipDuplicateHeader.Equals(value))
          return;
        m_SkipDuplicateHeader = value;
        NotifyPropertyChanged(nameof(SkipDuplicateHeader));
      }
    }

    /// <summary>
    ///   Gets or sets the number of rows that should be skipped at the start of the file
    /// </summary>
    /// <value>The skip rows.</value>
    [XmlAttribute]
    [DefaultValue(0)]
    public virtual int SkipRows
    {
      get => m_SkipRows;
      [NotifyPropertyChangedInvocator]
      set
      {
        if (m_SkipRows.Equals(value))
          return;
        m_SkipRows = value;
        NotifyPropertyChanged(nameof(SkipRows));
      }
    }

    /// <summary>
    ///   Sets the SQL statement rename.
    /// </summary>
    /// <param name="value">The value.</param>
    public void SetSqlStatementRename(string value)
    {
      var newVal = (value ?? string.Empty).NoControlCharacters();
      m_SqlStatement = newVal;
    }

    /// <summary>
    ///   Gets or sets the SQL statement.
    /// </summary>
    /// <value>The SQL statement.</value>
    [XmlIgnore]
    [DefaultValue("")]
    public virtual string SqlStatement
    {
      get => m_SqlStatement;
      [CanBeNull]
      [NotifyPropertyChangedInvocator]
      set
      {
        var newVal = (value==null ? string.Empty : value.NoControlCharacters());
        if (newVal.Equals(m_SqlStatement, StringComparison.Ordinal))
          return;
        m_SqlStatement = newVal;

        LatestSourceTimeUtc = ZeroTime;
        SourceFileSettings = null;
        NotifyPropertyChanged(nameof(SqlStatement));
      }
    }

    /// <summary>
    ///   Gets or sets the Timeout of a call.
    /// </summary>
    /// <value>The timeout in seconds.</value>
    [XmlAttribute]
    [DefaultValue(90)]
    public virtual int Timeout
    {
      get => m_Timeout;
      [NotifyPropertyChangedInvocator]
      set
      {
        var newVal = value > 0 ? value : 0;
        if (m_Timeout.Equals(newVal))
          return;
        m_Timeout = newVal;
        NotifyPropertyChanged(nameof(Timeout));
      }
    }

    /// <summary>
    ///   Gets or sets the template used for the file
    /// </summary>
    /// <value>The connection string.</value>
    [XmlElement]
    [DefaultValue("")]
    public virtual string TemplateName
    {
      [NotNull]
      get => m_TemplateName;
      [CanBeNull]
      [NotifyPropertyChangedInvocator]
      set
      {
        var newVal = value ?? string.Empty;
        if (m_TemplateName.Equals(newVal, StringComparison.Ordinal))
          return;
        m_TemplateName = newVal;
        NotifyPropertyChanged(nameof(TemplateName));
      }
    }

    /// <summary>
    ///   Gets or sets a value indicating whether to treat NBSP as space.
    /// </summary>
    /// <value><c>true</c> if NBSP should be treated as space; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool TreatNBSPAsSpace
    {
      get => m_TreatNbspAsSpace;
      [NotifyPropertyChangedInvocator]
      set
      {
        if (m_TreatNbspAsSpace.Equals(value))
          return;
        m_TreatNbspAsSpace = value;
        NotifyPropertyChanged(nameof(TreatNBSPAsSpace));
      }
    }

    /// <summary>
    ///   Gets or sets a value indicating whether this instance should treat any text listed here as Null
    /// </summary>
    [XmlAttribute]
    [DefaultValue("NULL")]
    public virtual string TreatTextAsNull
    {
      [NotNull]
      get => m_TreatTextAsNull;
      [CanBeNull]
      [NotifyPropertyChangedInvocator]
      set
      {
        var newVal = value ?? string.Empty;
        if (m_TreatTextAsNull.Equals(newVal, StringComparison.Ordinal))
          return;
        m_TreatTextAsNull = newVal;
        NotifyPropertyChanged(nameof(TreatTextAsNull));
      }
    }

    /// <summary>
    ///   Gets or sets a value indicating of and if training and leading spaces should be trimmed.
    /// </summary>
    /// <value><c>true</c> ; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(TrimmingOption.Unquoted)]
    public virtual TrimmingOption TrimmingOption
    {
      get => m_TrimmingOption;
      [NotifyPropertyChangedInvocator]
      set
      {
        if (m_TrimmingOption.Equals(value))
          return;
        m_TrimmingOption = value;
        NotifyPropertyChanged(nameof(TrimmingOption));
      }
    }

    /// <summary>
    ///   Gets or sets a value indicating whether this instance is imported
    /// </summary>
    /// <remarks>
    ///   Only used in CSV Validator to distinguish between imported files and extracts for
    ///   reference checks
    /// </remarks>
    /// <value><c>true</c> if this file is imported; otherwise, <c>false</c>.</value>
    [XmlAttribute(AttributeName = "IsImported")]
    [DefaultValue(true)]
    public virtual bool Validate
    {
      get => m_Validate;
      [NotifyPropertyChangedInvocator]
      set
      {
        if (m_Validate.Equals(value))
          return;
        m_Validate = value;
        NotifyPropertyChanged(nameof(Validate));
      }
    }

    /// <summary>
    ///   Copies all values to other instance
    /// </summary>
    /// <param name="other">The other.</param>
    protected virtual void BaseSettingsCopyTo(BaseSettings other)
    {
      if (other == null)
        return;
      m_FileFormat.CopyTo(other.FileFormat);
      MappingCollection.CopyTo(other.MappingCollection);
      other.ConsecutiveEmptyRows = m_ConsecutiveEmptyRows;
      other.TrimmingOption = m_TrimmingOption;
      other.TemplateName = m_TemplateName;
      other.IsEnabled = m_IsEnabled;
      other.DisplayStartLineNo = m_DisplayStartLineNo;
      other.SetLatestSourceTimeForWrite = m_SetLatestSourceTimeForWrite;
      other.DisplayEndLineNo = m_DisplayEndLineNo;
      other.DisplayRecordNo = m_DisplayRecordNo;
      other.HasFieldHeader = m_HasFieldHeader;
      other.IsEnabled = IsEnabled;
      other.ShowProgress = m_ShowProgress;
      other.TreatTextAsNull = m_TreatTextAsNull;
      other.Validate = m_Validate;
      other.RecordLimit = m_RecordLimit;
      other.SkipRows = m_SkipRows;
      other.SkipEmptyLines = m_SkipEmptyLines;
      other.SkipDuplicateHeader = m_SkipDuplicateHeader;

      other.Passphrase = m_Passphrase;
      other.Recipient = m_Recipient;
      other.TreatNBSPAsSpace = m_TreatNbspAsSpace;
      ColumnCollection.CopyTo(other.ColumnCollection);
      other.SqlStatement = m_SqlStatement;
      other.InOverview = m_InOverview;
      other.Timeout = m_Timeout;
      other.ProcessTimeUtc = m_ProcessTimeUtc;
      other.LatestSourceTimeUtc = m_LatestSourceTimeUtc;

      other.Footer = m_Footer;
      other.Header = m_Header;
      m_Samples.CollectionCopy(other.Samples);

      other.EvidenceNumberOrIssues = m_EvidenceNumberOrIssues;
      m_Errors.CollectionCopy(other.Errors);

      // FileName and ID are set at the end otherwise column collection changes will invalidate the
      // column header cache of the source
      if (other is IFileSettingPhysicalFile fileSettingPhysicalFile)
      {
        fileSettingPhysicalFile.FileSize = m_FileSize;
        fileSettingPhysicalFile.FileName = m_FileName;
        fileSettingPhysicalFile.RemoteFileName = m_RemoteFileName;
        fileSettingPhysicalFile.ThrowErrorIfNotExists = m_ThrowErrorIfNotExists;
      }
      other.ID = m_Id;
      other.NumRecords = m_NumRecords;
      other.WarningCount = m_WarningCount;
      other.ErrorCount = m_ErrorCount;
    }

    /*
    /// <summary>
    ///   Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    protected virtual int GetBaseHashCode()
    {
      unchecked
      {
        var hashCode = m_ConsecutiveEmptyRows;
        hashCode = (hashCode * 397) ^ ColumnCollection.CollectionHashCode();
        hashCode = (hashCode * 397) ^ MappingCollection.CollectionHashCode();
        hashCode = (hashCode * 397) ^ m_FileFormat.GetHashCode();
        hashCode = (hashCode * 397) ^ m_DisplayEndLineNo.GetHashCode();
        hashCode = (hashCode * 397) ^ m_DisplayRecordNo.GetHashCode();
        hashCode = (hashCode * 397) ^ m_SetLatestSourceTimeForWrite.GetHashCode();
        hashCode = (hashCode * 397) ^ m_DisplayStartLineNo.GetHashCode();
        hashCode = (hashCode * 397) ^ m_ProcessTimeUtc.GetHashCode();
        hashCode = (hashCode * 397) ^ m_LatestSourceTimeUtc.GetHashCode();
        hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(m_FileName);
        hashCode = (hashCode * 397) ^ m_FileSize.GetHashCode();
        hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(m_Footer);
        hashCode = (hashCode * 397) ^ m_HasFieldHeader.GetHashCode();
        hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(m_Header);
        hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(m_Id);
        hashCode = (hashCode * 397) ^ m_InOverview.GetHashCode();
        hashCode = (hashCode * 397) ^ m_IsEnabled.GetHashCode();
        hashCode = (hashCode * 397) ^ m_NumErrors;
        hashCode = (hashCode * 397) ^ m_Passphrase.GetHashCode();
        hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(m_Recipient);
        hashCode = (hashCode * 397) ^ (int)m_RecordLimit;
        hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(m_RemoteFileName);
        hashCode = (hashCode * 397) ^ m_ShowProgress.GetHashCode();
        hashCode = (hashCode * 397) ^ m_SkipRows;
        hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(m_SqlStatement);
        hashCode = (hashCode * 397) ^ m_SqlTimeout;
        hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(m_TemplateName);
        hashCode = (hashCode * 397) ^ m_TreatNbspAsSpace.GetHashCode();
        hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(m_TreatTextAsNull);
        hashCode = (hashCode * 397) ^ m_Validate.GetHashCode();
        hashCode = (hashCode * 397) ^ (m_ValidationResult?.GetHashCode() ?? 0);
        hashCode = (hashCode * 397) ^ m_Errors.CollectionHashCode();
        hashCode = (hashCode * 397) ^ ReadToEndOfFile.GetHashCode();
        hashCode = (hashCode * 397) ^ m_Samples.CollectionHashCode();
        hashCode = (hashCode * 397) ^ m_SkipEmptyLines.GetHashCode();
        hashCode = (hashCode * 397) ^ (int)m_TrimmingOption;
        return hashCode;
      }
    }
      */

    /// <summary>
    ///   Notifies the completed property changed.
    /// </summary>
    /// <param name="info">The info.</param>
    protected void NotifyPropertyChanged(string info)
    {
      if (PropertyChanged == null)
        return;
      try
      {
        // ReSharper disable once PolymorphicFieldLikeEventInvocation
        PropertyChanged(this, new PropertyChangedEventArgs(info));
      }
      catch (TargetInvocationException)
      {
      }
    }

    public override string ToString()
    {
      var stringBuilder = new System.Text.StringBuilder();
      stringBuilder.Append(GetType().Name);
      if (!string.IsNullOrEmpty(ID))
        stringBuilder.Append(" ");
      stringBuilder.Append(ID);

      if (!(this is IFileSettingPhysicalFile settingPhysicalFile)) return stringBuilder.ToString();
      stringBuilder.Append(" - ");
      stringBuilder.Append(FileSystemUtils.GetShortDisplayFileName(settingPhysicalFile.FileName, 80));
      return stringBuilder.ToString();
    }
  }
}