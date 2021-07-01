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
  /// <summary>
  ///   Abstract calls containing the basic setting for an IFileSetting if contains <see
  ///   cref="ColumnCollection" /> , <see cref="MappingCollection" /> and <see cref="FileFormat" />
  /// </summary>
#pragma warning disable CS0659

  [DebuggerDisplay("Settings: {ID} ({ColumnCollection.Count()} Columns)")]
  public abstract class BaseSettings : IFileSetting
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  {
    public const string cTreatTextAsNull = "NULL";
    public static readonly DateTime ZeroTime = new DateTime(0, DateTimeKind.Utc);
    private readonly FileFormat m_FileFormat = new FileFormat();
    private int m_ConsecutiveEmptyRows = 5;
    private bool m_DisplayEndLineNo;
    private bool m_DisplayRecordNo;
    private bool m_DisplayStartLineNo = true;
    private long m_ErrorCount;
    /// <summary>
    /// This information is only used by the Validator but since we can not extend classes with new properties, it needs to be defined here
    /// </summary>
    private SampleAndErrorsInformation m_SamplesErrors = new SampleAndErrorsInformation();
    private string m_Footer = string.Empty;
    private bool m_HasFieldHeader = true;
    private string m_Header = string.Empty;
    private string m_Id = string.Empty;
    private bool m_InOverview;
    private bool m_IsEnabled = true;
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
    ///   Gets a value indicating whether field mapping specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>

    public bool MappingSpecified => MappingCollection.Count > 0;

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
    public IReadOnlyCollection<IFileSetting>? SourceFileSettings
    {
      get => m_SourceFileSettings;
      set
      {
        if (m_SourceFileSettings == null && value == null) return;
        if (value != null && value.CollectionEqual(m_SourceFileSettings)) return;
        // do not notify if we change from null to an empty list
        var notify = value?.Count() > 0 || m_SourceFileSettings != null;
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

    public bool FileFormatSpecified => !FileFormat.Equals(new FileFormat());

    /// <summary>
    ///   Gets a value indicating whether FileLastWriteTimeUtc is specified.
    /// </summary>
    /// <value><c>true</c> if specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>

    public bool FileLastWriteTimeUtcSpecified => ProcessTimeUtc != ZeroTime;


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
      set => SetSqlStatementRename(value.Value);
    }

    /// <summary>
    ///   Gets a value indicating whether SqlStatementCData is specified.
    /// </summary>
    /// <value><c>true</c> if specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>

    public bool SqlStatementCDataSpecified => !string.IsNullOrEmpty(SqlStatement);

    /// <summary>
    ///   Gets or sets the number consecutive empty rows that should finish a read
    /// </summary>
    /// <value>The consecutive empty rows.</value>
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
    /// Storage for validation and samples record, only used in the validator but as we can not extend classes it has to live here
    /// </summary>
    [XmlElement]
    public SampleAndErrorsInformation SamplesAndErrors
    {
      get => m_SamplesErrors;
      set => m_SamplesErrors = value;
    }

    public bool SampleAndErrorsInformationSpecified => m_SamplesErrors.ErrorsSpecified ||
                                                       m_SamplesErrors.SamplesSpecified ||
                                                       m_SamplesErrors.NumErrors != -1;

    /// <summary>
    ///   Gets or sets the file format.
    /// </summary>
    /// <value>The file format.</value>
    [XmlElement]
    public virtual FileFormat FileFormat
    {
      get => m_FileFormat;
      set => value.CopyTo(m_FileFormat);
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

      set
      {
        if (m_ProcessTimeUtc.Equals(value))
          return;
        m_ProcessTimeUtc = value;
        NotifyPropertyChanged(nameof(ProcessTimeUtc));
      }
    }

    public bool ProcessTimeUtcSpecified => m_ProcessTimeUtc != ZeroTime;

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

      set
      {
        if (m_LatestSourceTimeUtc == value)
          return;
        m_LatestSourceTimeUtc = value;
        NotifyPropertyChanged(nameof(LatestSourceTimeUtc));
      }
    }

    /// <summary>
    ///   Gets or sets the Footer.
    /// </summary>
    /// <value>The Footer for outbound data.</value>
    [DefaultValue("")]
    public virtual string Footer
    {
      get => m_Footer;
      set
      {
        var newVal = (value ?? string.Empty).HandleCRLFCombinations(Environment.NewLine);
        if (m_Footer.Equals(newVal, StringComparison.Ordinal))
          return;
        m_Footer = newVal;
        NotifyPropertyChanged(nameof(Footer));
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
      get => m_Header;
      set
      {
        var newVal = (value ?? string.Empty).HandleCRLFCombinations(Environment.NewLine);
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
        if (oldValueInternal!= InternalID)
          NotifyPropertyChangedString(nameof(InternalID), oldValue, newVal);
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
    public virtual string InternalID => ID;

    /// <summary>
    ///   Gets or sets a value indicating whether this instance is enabled.
    /// </summary>
    /// <value><c>true</c> if this file is enabled; otherwise, <c>false</c>.</value>
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

    /// <summary>
    ///   Gets or sets the ID.
    /// </summary>
    /// <value>The ID.</value>
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

    /// <summary>
    ///   Gets or sets the ID.
    /// </summary>
    /// <value>The ID.</value>
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

    /// <summary>
    ///   Gets or sets a value indicating whether to show progress.
    /// </summary>
    /// <value><c>true</c> if progress should be shown; otherwise, <c>false</c>.</value>
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

    /// <summary>
    ///   Gets or sets a value indicating if the reader will skip empty lines.
    /// </summary>
    /// <value>if <c>true</c> the reader will skip empty lines.</value>
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

    /// <summary>
    ///   Gets or sets the number of rows that should be skipped at the start of the file
    /// </summary>
    /// <value>The skip rows.</value>
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

    /// <summary>
    ///   Gets or sets the SQL statement.
    /// </summary>
    /// <value>The SQL statement.</value>
    [XmlIgnore]
    [DefaultValue("")]
    public virtual string SqlStatement
    {
      get => m_SqlStatement;
      set
      {
        var newVal = value == null ? string.Empty : value.NoControlCharacters().HandleCRLFCombinations();
        if (newVal.Equals(m_SqlStatement, StringComparison.Ordinal))
          return;
        m_SqlStatement = newVal;

        m_LatestSourceTimeUtc = ZeroTime;
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

    /// <summary>
    ///   Gets or sets a value indicating whether to treat NBSP as space.
    /// </summary>
    /// <value><c>true</c> if NBSP should be treated as space; otherwise, <c>false</c>.</value>
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

    /// <summary>
    ///   Gets or sets a value indicating whether this instance should treat any text listed here as Null
    /// </summary>
    [XmlAttribute]
    [DefaultValue(cTreatTextAsNull)]
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

    /// <summary>
    ///   Gets or sets a value indicating of and if training and leading spaces should be trimmed.
    /// </summary>
    /// <value><c>true</c> ; otherwise, <c>false</c>.</value>
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

      set
      {
        if (m_Validate.Equals(value))
          return;
        m_Validate = value;
        NotifyPropertyChanged(nameof(Validate));
      }
    }

    /// <summary>
    ///   Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other" />
    ///   parameter; otherwise, <see langword="false" />.
    /// </returns>
    protected virtual bool BaseSettingsEquals(BaseSettings? other)
    {
      if (other==null)
        return false;
      if (!other.ID.Equals(ID, StringComparison.OrdinalIgnoreCase))
        return false;
      if (other.SkipRows != SkipRows || other.HasFieldHeader != HasFieldHeader)
        return false;
      if (other.RecentlyLoaded != RecentlyLoaded || other.IsEnabled != IsEnabled || other.InOverview != InOverview || other.Validate != Validate || other.ShowProgress != ShowProgress)
        return false;
      if (other.NumRecords != NumRecords || other.WarningCount != WarningCount || other.ErrorCount != ErrorCount)
        return false;
      if (other.TreatNBSPAsSpace != TreatNBSPAsSpace || other.ConsecutiveEmptyRows != ConsecutiveEmptyRows)
        return false;
      if (other.DisplayStartLineNo != DisplayStartLineNo || other.DisplayEndLineNo != DisplayEndLineNo || other.DisplayRecordNo != DisplayRecordNo)
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
      if (!other.TreatTextAsNull.Equals(TreatTextAsNull, StringComparison.OrdinalIgnoreCase) || other.TrimmingOption != TrimmingOption)
        return false;
      if (!other.TemplateName.Equals(TemplateName, StringComparison.Ordinal) ||  !other.SqlStatement.Equals(SqlStatement, StringComparison.OrdinalIgnoreCase))
        return false;
      if (!other.Footer.Equals(Footer, StringComparison.Ordinal) ||  !other.Header.Equals(Header, StringComparison.OrdinalIgnoreCase))
        return false;
      if (!other.FileFormat.Equals(FileFormat))
        return false;
      if (!other.MappingCollection.Equals(MappingCollection))
        return false;
      if (!other.SamplesAndErrors.Equals(SamplesAndErrors))
        return false;

      return other.ColumnCollection.Equals(ColumnCollection);
    }

    /// <summary>
    ///   Occurs after a property value changes.
    /// </summary>
    public virtual event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///   Occurs when a string value property changed providing information on old and new value
    /// </summary>
    public virtual event EventHandler<PropertyChangedEventArgs<string>>? PropertyChangedString;

    /// <summary>
    ///   As this might be a time consuming process, do this only if the time was not determined before
    /// </summary>
    /// <remarks>
    ///   For a physical file ist possibly easiest as it the file time, overwritten for more complex
    ///   things like a Query
    /// </remarks>
    public virtual void CalculateLatestSourceTime()
    {
      if (this is IFileSettingPhysicalFile settingPhysicalFile)
      {
        var fileName = FileSystemUtils.ResolvePattern(settingPhysicalFile.FullPath);
        var fi = new FileSystemUtils.FileInfo(fileName);
        m_LatestSourceTimeUtc = fi.LastWriteTimeUtc;
      }
      else
      // in case the source is not a physical file, assume it's the processing time
      {
        m_LatestSourceTimeUtc = ProcessTimeUtc;
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
    ///   Copies all values to other instance
    /// </summary>
    /// <param name="other">The other.</param>
    protected virtual void BaseSettingsCopyTo(BaseSettings other)
    {
      FileFormat.CopyTo(other.FileFormat);
      MappingCollection.CopyTo(other.MappingCollection);

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
      other.LatestSourceTimeUtc = LatestSourceTimeUtc;

      other.Footer = Footer;
      other.Header = Header;
      SamplesAndErrors.CopyTo(other.SamplesAndErrors);

      other.ID = ID;
      other.NumRecords = NumRecords;
      other.WarningCount = WarningCount;
      other.ErrorCount = ErrorCount;
    }

    /// <summary>
    ///   Notifies the completed property changed.
    /// </summary>
    /// <param name="info">The property name.</param>
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

    /// <summary>
    ///   Notifies on changed property strings
    /// </summary>
    /// <param name="info">The property name.</param>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newVal">The new value.</param>
    protected void NotifyPropertyChangedString(string info, string oldValue, string newVal)
    {
      if (PropertyChangedString == null)
        return;
      try
      {
        // ReSharper disable once PolymorphicFieldLikeEventInvocation
        PropertyChangedString?.Invoke(this, new PropertyChangedEventArgs<string>(info, oldValue, newVal));
      }
      catch (TargetInvocationException)
      {
      }
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

      if (!(this is IFileSettingPhysicalFile settingPhysicalFile)) return stringBuilder.ToString();
      stringBuilder.Append(" - ");
      stringBuilder.Append(FileSystemUtils.GetShortDisplayFileName(settingPhysicalFile.FileName));
      return stringBuilder.ToString();
    }

    /// <summary>
    ///   Clones this instance into a new instance of the same type
    /// </summary>
    /// <returns></returns>
    public abstract IFileSetting Clone();

    /// <summary>
    ///   Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other" />
    ///   parameter; otherwise, <see langword="false" />.
    /// </returns>
    public abstract bool Equals(IFileSetting? other);

    /// <summary>
    ///   Copies all properties to the other instance
    /// </summary>
    /// <param name="other">The other instance</param>
    public abstract void CopyTo(IFileSetting other);
  }
}