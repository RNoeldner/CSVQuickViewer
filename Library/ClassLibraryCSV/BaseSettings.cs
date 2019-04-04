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
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace CsvTools
{
  // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  /// <summary>
  ///  Abstract calls containing the basic setting for an IFileSetting if contains <see cref="ColumnCollection" />,
  ///  <see cref="MappingCollection" /> and <see cref="FileFormat" />
  /// </summary>
#pragma warning disable CS0659

  public abstract class BaseSettings : IEquatable<BaseSettings>, IFileSetting
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  {
    private readonly FileFormat m_FileFormat = new FileFormat();
    private int m_ConsecutiveEmptyRows = 5;
    private bool m_DisplayEndLineNo;
    private bool m_DisplayRecordNo;
    private bool m_DisplayStartLineNo = true;
    private DateTime m_FileLastWriteTimeUtc = DateTime.MinValue;
    private string m_FileName = string.Empty;
    private long m_FileSize;
    private string m_Footer = string.Empty;
    private string m_FullPath;
    private bool m_HasFieldHeader = true;
    private string m_Header = string.Empty;
    private string m_Id = string.Empty;
    private bool m_InOverview;
    private bool m_IsEnabled = true;
    private int m_NumErrors = -1;
    private string m_Passphrase = string.Empty;
    private string m_Recipient = string.Empty;
    private uint m_RecordLimit;
    private string m_RemoteFileName = string.Empty;
    private bool m_ThrowErrorIfNotExists = true;
    private bool m_ShowProgress = true;
    private int m_SkipRows;
    private string m_SqlStatement = string.Empty;
    private int m_SqlTimeout = 360;
    private string m_TemplateName = string.Empty;
    private bool m_TreatNbspAsSpace;
    private string m_TreatTextAsNull = "NULL";
    private bool m_Validate = true;
    private ValidationResult m_ValidationResult;
    private bool m_SkipDuplicateHeader = false;
    private bool m_SkipEmptyLines = true;

    /// <summary>
    ///  Initializes a new instance of the <see cref="BaseSettings" /> class.
    /// </summary>
    /// <param name="fileName">The filename.</param>
    protected BaseSettings(string fileName) : this() => FileName = fileName;

    /// <summary>
    ///  Initializes a new instance of the <see cref="BaseSettings" /> class.
    /// </summary>
    protected BaseSettings()
    {
      GetEncryptedPassphraseFunction = delegate () { if (!string.IsNullOrEmpty(Passphrase)) return Passphrase; if (!string.IsNullOrEmpty(ApplicationSetting.PGPKeyStorage.EncryptedPassphase)) return ApplicationSetting.PGPKeyStorage.EncryptedPassphase; return string.Empty; };
      ColumnCollection.CollectionChanged += ColumnCollectionChanged;
      Samples.CollectionChanged += delegate { NotifyPropertyChanged(nameof(Samples)); };
      Errors.CollectionChanged += delegate { if (m_NumErrors > 0 && Errors.Count > m_NumErrors) NumErrors = Errors.Count; NotifyPropertyChanged(nameof(Errors)); };
    }

    private void ColumnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.NewItems != null)
      {
        foreach (Column item in e.NewItems)
        {
          item.PropertyChanged += delegate (object s_, PropertyChangedEventArgs colEvent)
          {
            if (colEvent.PropertyName == nameof(CsvTools.Column.Ignore))
              NotifyPropertyChanged(nameof(ColumnCollection));
          };
        }
      }
      if (e.NewItems != null || e.OldItems != null)
        NotifyPropertyChanged(nameof(ColumnCollection));
    }

    /// <summary>
    ///  Gets a value indicating whether field mapping specified.
    /// </summary>
    /// <value>
    ///  <c>true</c> if field mapping is specified; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///  Used for XML Serialization
    /// </remarks>
    [XmlIgnore]
    public virtual bool MappingSpecified => MappingCollection.Count > 0;

    /// <summary>
    ///  Gets a value indicating whether column format specified.
    /// </summary>
    /// <value>
    ///  <c>true</c> if column format specified; otherwise, <c>false</c>.
    /// </value>
    [XmlIgnore]
    public virtual bool ColumnSpecified => ColumnCollection.Count > 0;

    [XmlIgnore]
    public virtual bool ErrorsSpecified => Errors.Count > 0;

    [XmlIgnore]
    public ICollection<IFileSetting> SourceFileSettings { get; set; }

    /// <summary>
    ///  Gets a value indicating whether FileFormat is specified.
    /// </summary>
    /// <value>
    ///  <c>true</c> if specified; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///  Used for XML Serialization
    /// </remarks>
    [XmlIgnore]
    public virtual bool FileFormatSpecified => !FileFormat.Equals(new FileFormat());

    /// <summary>
    ///  Gets a value indicating whether FileLastWriteTimeUtc is specified.
    /// </summary>
    /// <value>
    ///  <c>true</c> if specified; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///  Used for XML Serialization
    /// </remarks>
    [XmlIgnore]
    public virtual bool FileLastWriteTimeUtcSpecified => FileLastWriteTimeUtc != DateTime.MinValue;

    [XmlIgnore]
    public virtual bool PassphraseSpecified
    {
      get
      {
        if (ApplicationSetting.PGPKeyStorage == null)
          return Passphrase.Length > 0;

        // In case the individual passphrase matches the general Passphrase do not store it
        return ApplicationSetting.PGPKeyStorage.AllowSavingPassphrase &&
            !Passphrase.Equals(ApplicationSetting.PGPKeyStorage.EncryptedPassphase, StringComparison.Ordinal);
      }
    }

    /// <summary>
    ///  Gets or sets the name of the file.
    /// </summary>
    /// <value>The name of the file.</value>
    [XmlAttribute]
    [DefaultValue("")]
    public virtual string RemoteFileName
    {
      get => m_RemoteFileName;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_RemoteFileName.Equals(newVal, StringComparison.Ordinal)) return;

        m_RemoteFileName = newVal;
        NotifyPropertyChanged(nameof(RemoteFileName));
      }
    }

    /// <summary>
    ///  Gets or sets the name of the file.
    /// </summary>
    /// <value>The name of the file.</value>
    [XmlAttribute]
    [DefaultValue(true)]
    public virtual bool ThrowErrorIfNotExists
    {
      get => m_ThrowErrorIfNotExists;
      set
      {
        if (m_ThrowErrorIfNotExists.Equals(value)) return;
        m_ThrowErrorIfNotExists = value;
        NotifyPropertyChanged(nameof(ThrowErrorIfNotExists));
      }
    }

    [XmlIgnore] public virtual bool SamplesSpecified => Samples.Count > 0;

    /// <summary>
    ///  Utility calls to get or set the SQL Statement as CDataSection
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId =
     "System.Xml.XmlNode")]
    [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public virtual methods", MessageId = "0")]
    [DefaultValue("")]
    public virtual XmlCDataSection SqlStatementCData
    {
      get
      {
        var doc = new XmlDocument();
        return doc.CreateCDataSection(SqlStatement);
      }
      set => SqlStatement = value.Value;
    }

    /// <summary>
    ///  Gets a value indicating whether SqlStatementCData is specified.
    /// </summary>
    /// <value>
    ///  <c>true</c> if specified; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///  Used for XML Serialization
    /// </remarks>
    [XmlIgnore]
    public virtual bool SqlStatementCDataSpecified => !string.IsNullOrEmpty(SqlStatement);

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///  <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise,
    ///  <see langword="false" />.
    /// </returns>
    public bool Equals(BaseSettings other)
    {
      if (other is null) return false;
      if (ReferenceEquals(this, other)) return true;

      if (other is IFileSettingRemoteDownload otherRemote)
      {
        if (otherRemote.RemoteFileName != RemoteFileName || otherRemote.ThrowErrorIfNotExists != ThrowErrorIfNotExists)
          return false;
      }
      return string.Equals(other.TemplateName, TemplateName, StringComparison.OrdinalIgnoreCase) &&
          other.SkipRows == SkipRows &&
          other.IsEnabled == IsEnabled &&
          other.TreatNBSPAsSpace == TreatNBSPAsSpace &&
          string.Equals(other.FileName, FileName, StringComparison.OrdinalIgnoreCase) &&
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
          other.NumErrors == NumErrors &&
          other.SkipEmptyLines == SkipEmptyLines &&
          other.SkipDuplicateHeader == SkipDuplicateHeader &&
          other.FileSize == FileSize &&
          other.ReadToEndOfFile == ReadToEndOfFile &&
          other.SQLTimeout == SQLTimeout &&
          other.FileLastWriteTimeUtc == FileLastWriteTimeUtc &&
          string.Equals(other.SqlStatement, SqlStatement, StringComparison.OrdinalIgnoreCase) &&
          string.Equals(other.Footer, Footer, StringComparison.OrdinalIgnoreCase) &&
          string.Equals(other.Header, Header, StringComparison.OrdinalIgnoreCase) &&
          MappingCollection.Equals(other.MappingCollection) && Samples.CollectionEqual(other.Samples) &&
          Errors.CollectionEqual(other.Errors) && ColumnCollection.Equals(other.ColumnCollection);
    }

    /// <summary>
    ///  Occurs after a property value changes.
    /// </summary>
    public virtual event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///  Occurs when a string value property changed providing information on old and new value
    /// </summary>
    public virtual event EventHandler<PropertyChangedEventArgs<string>> PropertyChangedString;

    /// <summary>
    ///  Gets or sets the number consecutive empty rows that should finish a read
    /// </summary>
    /// <value>The consecutive empty rows.</value>
    [XmlAttribute]
    [DefaultValue(5)]
    public virtual int ConsecutiveEmptyRows
    {
      get => m_ConsecutiveEmptyRows;

      set
      {
        if (m_ConsecutiveEmptyRows.Equals(value)) return;
        if (value < 0)
          value = 0;
        m_ConsecutiveEmptyRows = value;
        NotifyPropertyChanged(nameof(ConsecutiveEmptyRows));
      }
    }

    /// <summary>
    ///  Gets or sets the options for a column
    /// </summary>
    /// <value>
    ///  The column options
    /// </value>
    [XmlElement("Format")]
    public virtual ColumnCollection ColumnCollection { get; } = new ColumnCollection();

    /// <summary>
    ///  Gets or sets a value indicating whether to display end line numbers.
    /// </summary>
    /// <value><c>true</c> if end line no should be displayed; otherwise, <c>false</c>.</value>
    [XmlElement]
    [DefaultValue(false)]
    public virtual bool DisplayEndLineNo
    {
      get => m_DisplayEndLineNo;

      set
      {
        if (m_DisplayEndLineNo.Equals(value)) return;
        m_DisplayEndLineNo = value;
        NotifyPropertyChanged(nameof(DisplayEndLineNo));
      }
    }

    /// <summary>
    ///  Gets or sets a value indicating whether to display record no.
    /// </summary>
    /// <value><c>true</c> if record number should be displayed; otherwise, <c>false</c>.</value>
    [XmlElement]
    [DefaultValue(false)]
    public virtual bool DisplayRecordNo
    {
      get => m_DisplayRecordNo;

      set
      {
        if (m_DisplayRecordNo.Equals(value)) return;
        m_DisplayRecordNo = value;
        NotifyPropertyChanged(nameof(DisplayRecordNo));
      }
    }

    /// <summary>
    ///  Gets or sets a value indicating whether to display start line numbers
    /// </summary>
    /// <value><c>true</c> if start line no should be displayed; otherwise, <c>false</c>.</value>
    [XmlElement]
    [DefaultValue(true)]
    public virtual bool DisplayStartLineNo
    {
      get => m_DisplayStartLineNo;

      set
      {
        if (m_DisplayStartLineNo.Equals(value)) return;
        m_DisplayStartLineNo = value;
        NotifyPropertyChanged(nameof(DisplayStartLineNo));
      }
    }

    public ObservableCollection<SampleRecordEntry> Errors { get; } = new ObservableCollection<SampleRecordEntry>();

    /// <summary>
    ///  Gets or sets the file format.
    /// </summary>
    /// <value>
    ///  The file format.
    /// </value>
    [XmlElement]
    public virtual FileFormat FileFormat
    {
      get => m_FileFormat;
      set => value?.CopyTo(m_FileFormat);
    }

    /// <summary>
    ///  The UTC time the file was last written to
    /// </summary>
    [XmlAttribute]
    public virtual DateTime FileLastWriteTimeUtc
    {
      get => m_FileLastWriteTimeUtc;

      set
      {
        if (m_FileLastWriteTimeUtc.Equals(value)) return;
        m_FileLastWriteTimeUtc = value;
        NotifyPropertyChanged(nameof(FileLastWriteTimeUtc));
      }
    }

    /// <summary>
    ///  Gets or sets the name of the file.
    /// </summary>
    /// <value>The name of the file.</value>
    [XmlAttribute]
    [DefaultValue("")]
    public virtual string FileName
    {
      get => m_FileName;
      set
      {
        var newVal = value ?? string.Empty;
        if (newVal.StartsWith(".\\", StringComparison.Ordinal))
          newVal = newVal.Substring(2);

        if (m_FileName.Equals(newVal, StringComparison.Ordinal)) return;

        m_FileName = newVal;
        m_FullPath = null;
        NotifyPropertyChanged(nameof(FileName));
      }
    }

    /// <summary>
    ///  Gets or sets the date the file when it was read
    /// </summary>
    /// <value>The consecutive empty rows.</value>
    [XmlAttribute]
    [DefaultValue(0)]
    public virtual long FileSize
    {
      get => m_FileSize;
      set
      {
        if (value == m_FileSize) return;
        m_FileSize = value;
        NotifyPropertyChanged(nameof(FileSize));
      }
    }

    /// <summary>
    ///  Gets or sets the Footer.
    /// </summary>
    /// <value>The Footer for outbound data.</value>
    [DefaultValue("")]
    public virtual string Footer
    {
      get => m_Footer;

      set
      {
        var newVal = StringUtils.HandleCRLFCombinations(value ?? string.Empty, Environment.NewLine);
        if (m_Footer.Equals(newVal, StringComparison.Ordinal)) return;
        m_Footer = newVal;
        NotifyPropertyChanged(nameof(Footer));
      }
    }

    [XmlIgnore]
    public virtual string FullPath
    {
      get
      {
        if (m_FullPath == null || !FileSystemUtils.FileExists(m_FullPath))
        {
          m_FullPath = FileSystemUtils.ResolvePattern(m_FileName.GetAbsolutePath(ApplicationSetting.RootFolder));
        }

        return m_FullPath;
      }
    }

    /// <summary>
    /// As the data is loaded and not further validation is done this will be set to true
    /// Once validation is happening and validation errors are stored this is false again.
    /// This is stored on FileSetting level even as it actually is used for determine
    /// th freshness of a loaded data in the validator, but there is not suitable data structure
    /// </summary>
    [XmlIgnore]
    [DefaultValue(false)]
    public virtual bool RecentlyLoaded { get; set; } = false;

    /// <summary>
    ///  Gets or sets a value indicating whether this instance has field header.
    /// </summary>
    /// <value>
    ///  <c>true</c> if this instance has field header; otherwise, <c>false</c>.
    /// </value>
    [XmlAttribute]
    [DefaultValue(true)]
    public virtual bool HasFieldHeader
    {
      get => m_HasFieldHeader;
      set
      {
        if (m_HasFieldHeader.Equals(value)) return;
        m_HasFieldHeader = value;
        NotifyPropertyChanged(nameof(HasFieldHeader));
      }
    }

    /// <summary>
    ///  Gets or sets the Footer.
    /// </summary>
    /// <value>The Footer for outbound data.</value>
    [DefaultValue("")]
    public virtual string Header
    {
      get => m_Header;

      set
      {
        var newVal = StringUtils.HandleCRLFCombinations(value ?? string.Empty, Environment.NewLine);
        if (m_Header.Equals(newVal, StringComparison.Ordinal)) return;
        m_Header = newVal;
        NotifyPropertyChanged(nameof(Header));
      }
    }

    /// <summary>
    ///  Gets or sets the ID.
    /// </summary>
    /// <value>
    ///  The ID.
    /// </value>
    [XmlAttribute]
    [DefaultValue("")]
    public virtual string ID
    {
      get => m_Id;

      set
      {
        var newVal = value ?? string.Empty;
        if (m_Id.Equals(newVal, StringComparison.Ordinal)) return;

        var oldValue = m_Id;
        m_Id = newVal;
        NotifyPropertyChanged(nameof(ID));
        PropertyChangedString?.Invoke(this, new PropertyChangedEventArgs<string>(nameof(ID), oldValue, newVal));
      }
    }

    /// <summary>
    ///  Gets or sets a value indicating whether this instance is critical.
    /// </summary>
    /// <value><c>true</c> if this file is critical for the export; otherwise, <c>false</c>.</value>
    [XmlAttribute(AttributeName = "IsCritical")]
    [DefaultValue(false)]
    public virtual bool InOverview
    {
      get => m_InOverview;

      set
      {
        if (m_InOverview.Equals(value)) return;
        m_InOverview = value;
        NotifyPropertyChanged(nameof(InOverview));
      }
    }

    /// <summary>
    ///  The identified to find this specific instance
    /// </summary>
    [XmlIgnore]
    public virtual string InternalID => string.IsNullOrEmpty(ID) ? FileName : ID;

    /// <summary>
    ///  Gets or sets a value indicating whether this instance is enabled.
    /// </summary>
    /// <value><c>true</c> if this file is enabled; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(true)]
    public virtual bool IsEnabled
    {
      get => m_IsEnabled;

      set
      {
        if (m_IsEnabled.Equals(value)) return;
        m_IsEnabled = value;
        NotifyPropertyChanged(nameof(IsEnabled));
      }
    }

    /// <summary>
    ///  Gets or sets the field mapping.
    /// </summary>
    /// <value>The field mapping.</value>
    [XmlElement("Mapping")]
    public virtual MappingCollection MappingCollection { get; } = new MappingCollection();

    /// <summary>
    ///  Gets or sets the ID.
    /// </summary>
    /// <value>
    ///  The ID.
    /// </value>
    [XmlAttribute]
    [DefaultValue(-1)]
    public virtual int NumErrors
    {
      get
      {
        if (m_NumErrors == -1 && Errors.Count > 0)
          m_NumErrors = Errors.Count;
        return m_NumErrors;
      }
      set
      {
        // can not be smaller than the number of named errors
        if (Errors.Count > 0 && value < Errors.Count)
          value = Errors.Count;
        if (m_NumErrors == value) return;
        m_NumErrors = value;
        NotifyPropertyChanged(nameof(NumErrors));
      }
    }

    /// <summary>
    ///  Pass phrase for Decryption
    /// </summary>
    [XmlAttribute]
    [DefaultValue("")]
    public virtual string Passphrase
    {
      get => m_Passphrase;
      set => m_Passphrase = (value ?? string.Empty).Trim();
    }

    /// <summary>
    ///  Gets or sets a value indicating whether to display end line numbers.
    /// </summary>
    /// <value><c>true</c> if end line no should be displayed; otherwise, <c>false</c>.</value>
    [XmlIgnore]
    public virtual bool ReadToEndOfFile { get; set; }

    /// <summary>
    ///  Recipient for a outbound PGP encryption
    /// </summary>
    [XmlAttribute]
    [DefaultValue("")]
    public virtual string Recipient
    {
      get => m_Recipient;
      set
      {
        var newVal = (value ?? string.Empty).Trim();
        if (m_Recipient.Equals(newVal, StringComparison.Ordinal)) return;
        m_Recipient = newVal;
        NotifyPropertyChanged(nameof(Recipient));
      }
    }

    /// <summary>
    ///  Gets or sets the record limit.
    /// </summary>
    /// <value>The record limit.</value>
    [XmlElement]
    [DefaultValue(0)]
    public virtual uint RecordLimit
    {
      get => m_RecordLimit;

      set
      {
        if (m_RecordLimit.Equals(value)) return;
        m_RecordLimit = value;
        NotifyPropertyChanged(nameof(RecordLimit));
      }
    }

    public ObservableCollection<SampleRecordEntry> Samples { get; } = new ObservableCollection<SampleRecordEntry>();

    /// <summary>
    ///  Gets or sets a value indicating whether to show progress.
    /// </summary>
    /// <value>
    ///  <c>true</c> if progress should be shown; otherwise, <c>false</c>.
    /// </value>
    [XmlAttribute]
    [DefaultValue(true)]
    public virtual bool ShowProgress
    {
      get => m_ShowProgress;

      set
      {
        if (m_ShowProgress.Equals(value)) return;
        m_ShowProgress = value;
        NotifyPropertyChanged(nameof(ShowProgress));
      }
    }

    /// <summary>
    ///  Gets or sets a value indicating if the reader will skip empty lines.
    /// </summary>
    /// <value>if <c>true</c> the reader will skip empty lines.</value>
    [XmlAttribute]
    [DefaultValue(true)]
    public virtual bool SkipEmptyLines
    {
      get => m_SkipEmptyLines;
      set
      {
        if (m_SkipEmptyLines.Equals(value)) return;
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
        if (m_SkipDuplicateHeader.Equals(value)) return;
        m_SkipDuplicateHeader = value;
        NotifyPropertyChanged(nameof(SkipDuplicateHeader));
      }
    }

    /// <summary>
    ///  Gets or sets the number of rows that should be skipped at the start of the file
    /// </summary>
    /// <value>The skip rows.</value>
    [XmlAttribute]
    [DefaultValue(0)]
    public virtual int SkipRows
    {
      get => m_SkipRows;

      set
      {
        if (m_SkipRows.Equals(value)) return;
        m_SkipRows = value;
        NotifyPropertyChanged(nameof(SkipRows));
      }
    }

    /// <summary>
    ///  Gets or sets the SQL statement.
    /// </summary>
    /// <value>The SQL statement.</value>
    [XmlIgnore]
    [DefaultValue("")]
    public virtual string SqlStatement
    {
      get
      {
        Contract.Ensures(Contract.Result<string>() != null);
        return m_SqlStatement;
      }

      set
      {
        var newVal = (value ?? string.Empty).NoControlCharaters();
        if (m_SqlStatement.Equals(newVal, StringComparison.Ordinal)) return;
        if (!string.IsNullOrEmpty(m_SqlStatement))
          FileLastWriteTimeUtc = DateTime.MinValue;
        m_SqlStatement = newVal;
        SourceFileSettings = null;
        NotifyPropertyChanged(nameof(SqlStatement));
      }
    }

    /// <summary>
    ///  Gets or sets the SQL statement.
    /// </summary>
    /// <value>The SQL statement.</value>
    [XmlAttribute]
    [DefaultValue(360)]
    public virtual int SQLTimeout
    {
      get => m_SqlTimeout;

      set
      {
        if (m_SqlTimeout.Equals(value)) return;
        m_SqlTimeout = value;
        NotifyPropertyChanged(nameof(SQLTimeout));
      }
    }

    /// <summary>
    ///  Gets or sets the template used for the file
    /// </summary>
    /// <value>The connection string.</value>
    [XmlElement]
    [DefaultValue("")]
    public virtual string TemplateName
    {
      get
      {
        Contract.Ensures(Contract.Result<string>() != null);
        return m_TemplateName;
      }

      set
      {
        var newVal = value ?? string.Empty;
        if (m_TemplateName.Equals(newVal, StringComparison.Ordinal)) return;
        m_TemplateName = newVal;
        NotifyPropertyChanged(nameof(TemplateName));
      }
    }

    /// <summary>
    ///  Gets or sets a value indicating whether to treat NBSP as space.
    /// </summary>
    /// <value><c>true</c> if NBSP should be treated as space; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool TreatNBSPAsSpace
    {
      get => m_TreatNbspAsSpace;

      set
      {
        if (m_TreatNbspAsSpace.Equals(value)) return;
        m_TreatNbspAsSpace = value;
        NotifyPropertyChanged(nameof(TreatNBSPAsSpace));
      }
    }

    /// <summary>
    ///  Gets or sets a value indicating whether this instance should treat any text listed here as Null
    /// </summary>
    [XmlAttribute]
    [DefaultValue("NULL")]
    public virtual string TreatTextAsNull
    {
      get
      {
        Contract.Ensures(Contract.Result<string>() != null);
        return m_TreatTextAsNull;
      }
      set
      {
        var newVal = value ?? string.Empty;
        if (m_TreatTextAsNull.Equals(newVal, StringComparison.Ordinal)) return;
        m_TreatTextAsNull = newVal;
        NotifyPropertyChanged(nameof(TreatTextAsNull));
      }
    }

    /// <summary>
    ///  Gets or sets a value indicating of and if training and leading spaces should be trimmed.
    /// </summary>
    /// <value><c>true</c> ; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(TrimmingOption.Unquoted)]
    public virtual TrimmingOption TrimmingOption { get; set; } = TrimmingOption.Unquoted;

    /// <summary>
    ///  Gets or sets a value indicating whether this instance is imported
    /// </summary>
    /// <remarks>
    ///  Only used in CSV Validator to distinguish between imported files and extracts for reference checks
    /// </remarks>
    /// <value><c>true</c> if this file is imported; otherwise, <c>false</c>.</value>
    [XmlAttribute(AttributeName = "IsImported")]
    [DefaultValue(true)]
    public virtual bool Validate
    {
      get => m_Validate;

      set
      {
        if (m_Validate.Equals(value)) return;
        m_Validate = value;
        NotifyPropertyChanged(nameof(Validate));
      }
    }

    /// <summary>
    ///  Gets or sets the <see cref="ValidationResult" />
    /// </summary>
    public ValidationResult ValidationResult
    {
      get => m_ValidationResult;
      set
      {
        if (m_ValidationResult != null && m_ValidationResult.Equals(value)) return;
        m_ValidationResult = value;
        NotifyPropertyChanged(nameof(ValidationResult));
      }
    }

    /// <summary>
    ///  Clones this instance.
    /// </summary>
    /// <returns></returns>
    public abstract IFileSetting Clone();

    /// <summary>
    ///  Copies all values to other instance
    /// </summary>
    /// <param name="other">The other.</param>
    public virtual void CopyTo(IFileSetting other)
    {
      if (other == null)
        return;
      m_FileFormat.CopyTo(other.FileFormat);
      MappingCollection.CopyTo(other.MappingCollection);
      other.ConsecutiveEmptyRows = m_ConsecutiveEmptyRows;
      other.TrimmingOption = TrimmingOption;
      other.TemplateName = m_TemplateName;
      other.GetEncryptedPassphraseFunction = GetEncryptedPassphraseFunction;
      other.IsEnabled = m_IsEnabled;
      other.DisplayStartLineNo = m_DisplayStartLineNo;
      other.DisplayEndLineNo = m_DisplayEndLineNo;
      other.DisplayRecordNo = m_DisplayRecordNo;
      other.HasFieldHeader = m_HasFieldHeader;
      other.IsEnabled = IsEnabled;
      other.ShowProgress = m_ShowProgress;
      other.TreatTextAsNull = m_TreatTextAsNull;
      other.Validate = m_Validate;
      other.RecordLimit = m_RecordLimit;
      other.SkipRows = m_SkipRows;
      other.SkipEmptyLines = SkipEmptyLines;
      other.SkipDuplicateHeader = SkipDuplicateHeader;

      other.Passphrase = m_Passphrase;
      other.Recipient = m_Recipient;
      other.TreatNBSPAsSpace = m_TreatNbspAsSpace;
      ColumnCollection.CopyTo(other.ColumnCollection);
      other.SqlStatement = m_SqlStatement;
      other.InOverview = m_InOverview;
      other.SQLTimeout = m_SqlTimeout;
      other.ReadToEndOfFile = ReadToEndOfFile;
      other.FileLastWriteTimeUtc = FileLastWriteTimeUtc;
      other.FileSize = FileSize;
      other.Footer = Footer;
      other.Header = Header;
      Samples.CollectionCopy(other.Samples);
      Errors.CollectionCopy(other.Errors);
      other.NumErrors = m_NumErrors;

      // FileName and ID are set at the end otherwise column collection changes will invalidate the column header cache of the source
      other.FileName = m_FileName;
      other.ID = m_Id;

      if (!(other is IFileSettingRemoteDownload otherRemote)) return;
      otherRemote.RemoteFileName = m_RemoteFileName;
      otherRemote.ThrowErrorIfNotExists = m_ThrowErrorIfNotExists;
    }

    public abstract bool Equals(IFileSetting other);

    /// <summary>
    ///  Gets the <see cref="CsvTools.Mapping" /> with the specified source.
    /// </summary>
    /// <param name="columnName">Name of the file column</param>
    /// <returns>Return all FieldMapping for a column. There can be multiple</returns>
    public virtual IEnumerable<Mapping> GetColumnMapping(string columnName)
    {
      foreach (var mapping in MappingCollection)
      {
        if (mapping.FileColumn.Equals(columnName, StringComparison.OrdinalIgnoreCase))
          yield return mapping;
      }
    }

    public abstract IFileWriter GetFileWriter(IProcessDisplay processDisplay);

    public abstract IFileReader GetFileReader(IProcessDisplay processDisplay);

    /// <summary>
    ///  Remove a Fields mapping.
    /// </summary>
    /// <param name="source">The source name.</param>
    public virtual void RemoveMapping(string source)
    {
      var toBeRemoved = new List<Mapping>();
      foreach (var fieldMapping in MappingCollection)
      {
        if (fieldMapping.FileColumn.Equals(source, StringComparison.OrdinalIgnoreCase))
          toBeRemoved.Add(fieldMapping);
      }

      foreach (var fieldMapping in toBeRemoved)
        MappingCollection.Remove(fieldMapping);
    }

    public abstract override bool Equals(object obj);

    /*
     public abstract override int GetHashCode();

    /// <summary>Serves as the default hash function. </summary>
    /// <returns>A hash code for the current object.</returns>
    protected int GetBaseHashCode()
    {
     unchecked
     {
      var hashCode = m_ConsecutiveEmptyRows;
      hashCode = (hashCode * 397) ^ m_Column.CollectionHashCode();
      hashCode = (hashCode * 397) ^ m_ColumnMapping.CollectionHashCode();
      hashCode = (hashCode * 397) ^ m_FileFormat.GetHashCode();
      hashCode = (hashCode * 397) ^ m_DisplayEndLineNo.GetHashCode();
      hashCode = (hashCode * 397) ^ m_DisplayRecordNo.GetHashCode();
      hashCode = (hashCode * 397) ^ m_DisplayStartLineNo.GetHashCode();
      hashCode = (hashCode * 397) ^ m_FileLastWriteTimeUtc.GetHashCode();
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
      hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(m_SourceSetting);
      hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(m_SqlStatement);
      hashCode = (hashCode * 397) ^ m_SqlTimeout;
      hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(m_TemplateName);
      hashCode = (hashCode * 397) ^ m_TreatNbspAsSpace.GetHashCode();
      hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(m_TreatTextAsNull);
      hashCode = (hashCode * 397) ^ m_Validate.GetHashCode();
      hashCode = (hashCode * 397) ^ (m_ValidationResult?.GetHashCode() ?? 0);
      hashCode = (hashCode * 397) ^ Errors.CollectionHashCode();
      hashCode = (hashCode * 397) ^ ReadToEndOfFile.GetHashCode();
      hashCode = (hashCode * 397) ^ Samples.CollectionHashCode();
      hashCode = (hashCode * 397) ^ SkipEmptyLines.GetHashCode();
      hashCode = (hashCode * 397) ^ (int)TrimmingOption;
      return hashCode;
     }
    }
    */

    /// <summary>
    ///  Notifies the completed property changed.
    /// </summary>
    /// <param name="info">The info.</param>
    public virtual void NotifyPropertyChanged(string info)
    {
      if (PropertyChanged == null) return;
      try
      {
        // ReSharper disable once PolymorphicFieldLikeEventInvocation
        PropertyChanged(this, new PropertyChangedEventArgs(info));
      }
      catch (TargetInvocationException)
      {
      }
    }

    [XmlIgnore]
    public virtual Func<string> GetEncryptedPassphraseFunction { get; set; }
  }
}