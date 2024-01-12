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
using System.Text;
using System.Threading;

namespace CsvTools
{
  /// <inheritdoc cref="IValidatorSetting" />  
  public abstract class BaseSettingsValidator : BaseSettings, IValidatorSetting
  {
    /// <summary>
    /// The default time if none is chosen
    /// </summary>    
    public static readonly DateTime ZeroTime = new DateTime(0, DateTimeKind.Utc);
    private readonly ReaderWriterLockSlim m_LockStatus = new ReaderWriterLockSlim();
    private string m_Comment = string.Empty;
    private long m_ErrorCount;
    private string m_Id;
    private bool m_InOverview;
    private bool m_IsEnabled = true;    
    private DateTime m_LatestSourceTimeUtc = ZeroTime;
    private long m_NumRecords;
    private int m_Order = 100;
    private DateTime m_ProcessTimeUtc = ZeroTime;
    private bool m_SetLatestSourceTimeForWrite;
    private bool m_ShowProgress = true;
    private IReadOnlyCollection<IValidatorSetting>? m_SourceFileSettings;
    private string m_SqlStatement = string.Empty;
    private FileStettingStatus m_Status = FileStettingStatus.None;
    private string m_TemplateName = string.Empty;
    private int m_Timeout = 90;
    private bool m_Validate = true;
    private long m_WarningCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseSettingsValidator"/> class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    protected BaseSettingsValidator(in string id)
    {
      m_Id = id ?? string.Empty;
      MappingCollection.CollectionChanged += (sender, e) =>
      {
        if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Add)
          NotifyPropertyChanged(nameof(MappingCollection));
      };
      MappingCollection.CollectionItemPropertyChanged +=
        (sender, e) => NotifyPropertyChanged(nameof(MappingCollection));
    }

    /// <summary>
    /// Occurs when the identifier is changed, used to handle reference operations
    /// </summary>
    public event EventHandler<PropertyChangedEventArgs<string>>? IdChanged;

    /// <inheritdoc />
    [JsonIgnore]
    public int CollectionIdentifier => ID.GetHashCode();

    /// <inheritdoc />
    [DefaultValue("")]
    public virtual string Comment
    {
      get => m_Comment;
      set => SetProperty(ref m_Comment, value ?? string.Empty);
    }

    /// <inheritdoc />
    [DefaultValue(0)]
    public virtual long ErrorCount
    {
      get => m_ErrorCount;
      set => SetProperty(ref m_ErrorCount, value);
    }

    /// <inheritdoc />
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
    [DefaultValue(false)]
    public virtual bool InOverview
    {
      get => m_InOverview;
      set => SetProperty(ref m_InOverview, value);
    }

    /// <inheritdoc />
    [DefaultValue(true)]
    public virtual bool IsEnabled
    {
      get => m_IsEnabled;
      set => SetProperty(ref m_IsEnabled, value);
    }
    

    /// <inheritdoc />    
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
    public MappingCollection MappingCollection { get; } = new MappingCollection();

    /// <inheritdoc />    
    [DefaultValue(0)]
    public virtual long NumRecords
    {
      get => m_NumRecords;
      set => SetProperty(ref m_NumRecords, value);
    }

    /// <inheritdoc />
    [DefaultValue(100)]
    public virtual int Order
    {
      get => m_Order;
      set => SetProperty(ref m_Order, value);
    }

    /// <inheritdoc />
    public virtual DateTime ProcessTimeUtc
    {
      get => m_ProcessTimeUtc;
      set => SetProperty(ref m_ProcessTimeUtc, value);
    }

    /// <inheritdoc />
    [JsonIgnore]
    [DefaultValue(false)]
    public virtual bool RecentlyLoaded { get; set; }

    /// <inheritdoc />
    public SampleAndErrorsInformation SamplesAndErrors { get; set; } = new SampleAndErrorsInformation();

    /// <inheritdoc />
    [DefaultValue(false)]
    public virtual bool SetLatestSourceTimeForWrite
    {
      get => m_SetLatestSourceTimeForWrite;
      set => SetProperty(ref m_SetLatestSourceTimeForWrite, value);
    }

    /// <inheritdoc />    
    [DefaultValue(true)]
    public virtual bool ShowProgress
    {
      get => m_ShowProgress;
      set => SetProperty(ref m_ShowProgress, value);
    }

    /// <inheritdoc />    
    [JsonIgnore]
    public IReadOnlyCollection<IValidatorSetting>? SourceFileSettings
    {
      get => m_SourceFileSettings;
      set
      {
        m_SourceFileSettings = value;
        NotifyPropertyChanged();
      }
    }

    /// <inheritdoc />    
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

    /// <inheritdoc />    
    [DefaultValue("")]
    public virtual string TemplateName
    {
      get => m_TemplateName;
      set => SetProperty(ref m_TemplateName, value ?? string.Empty);
    }

    /// <inheritdoc />    
    [DefaultValue(90)]
    public virtual int Timeout
    {
      get => m_Timeout;
      set => SetProperty(ref m_Timeout, value > 0 ? value : 0);
    }

    /// <inheritdoc />
    [DefaultValue(true)]
    public virtual bool Validate
    {
      get => m_Validate;
      set => SetProperty(ref m_Validate, value);
    }

    /// <inheritdoc />    
    [DefaultValue(0)]
    public virtual long WarningCount
    {
      get => m_WarningCount;
      set => SetProperty(ref m_WarningCount, value > 0 ? value : 0);
    }

    /// <inheritdoc />
    public virtual void CalculateLatestSourceTime() => LatestSourceTimeUtc = ProcessTimeUtc;

    /// <inheritdoc />
    public virtual IEnumerable<string> GetDifferences(IValidatorSetting other)
    {
      if (!other.Comment.Equals(Comment, StringComparison.Ordinal)) yield return $"{nameof(Comment)} : {Comment} - {other.Comment}";
      if (!other.GetType().FullName!.Equals(GetType().FullName, StringComparison.OrdinalIgnoreCase)) yield return $"Type : {GetType().FullName} - {other.GetType().FullName}";
      if (!other.ID.Equals(ID, StringComparison.OrdinalIgnoreCase)) yield return $"{nameof(ID)}: {ID} - {other.ID}";
      if (!other.MappingCollection.Equals(MappingCollection)) yield return $"{nameof(MappingCollection)} different";
      if (!other.SamplesAndErrors.Equals(SamplesAndErrors)) yield return $"{nameof(SamplesAndErrors)} different";
      if (!other.SqlStatement.Equals(SqlStatement, StringComparison.Ordinal)) yield return $"{nameof(SqlStatement)} : {SqlStatement} - {other.SqlStatement}";
      if (!other.TemplateName.Equals(TemplateName, StringComparison.Ordinal)) yield return $"{nameof(TemplateName)} : {TemplateName} - {other.TemplateName}";
      if (other.InOverview != InOverview) yield return $"{nameof(InOverview)} : {InOverview} - {other.InOverview}";
      if (other.IsEnabled != IsEnabled) yield return $"{nameof(IsEnabled)} : {IsEnabled} - {other.IsEnabled}";      
      if (other.RecentlyLoaded != RecentlyLoaded) yield return $"{nameof(RecentlyLoaded)} : {RecentlyLoaded} - {other.RecentlyLoaded}";
      if (other.ShowProgress != ShowProgress) yield return $"{nameof(ShowProgress)} : {ShowProgress} - {other.ShowProgress}";
      if (other.Timeout != Timeout) yield return $"{nameof(Timeout)} : {Timeout} - {other.Timeout}";
      if (other.Validate != Validate) yield return $"{nameof(Validate)} : {Validate} - {other.Validate}";

      foreach (var ret in base.GetDifferences(other as IFileSetting))
        yield return ret;
    }

    /// <inheritdoc />
    public override string ToString()
    {
      var stringBuilder = new StringBuilder();
      stringBuilder.Append(GetType().Name);
      if (!string.IsNullOrEmpty(ID))
        stringBuilder.Append(' ');
      stringBuilder.Append(ID);
      return stringBuilder.ToString();
    }

    /// <summary>
    ///   Copies all values to other instance
    /// </summary>
    /// <param name="other">The other.</param>
    protected virtual void BaseSettingsCopyTo(in IValidatorSetting? other)
    {
      if (other is null)
        return;

      other.MappingCollection.Clear();
      other.MappingCollection.AddRange(MappingCollection);
      other.ShowProgress = ShowProgress;
      other.TemplateName = TemplateName;
      other.IsEnabled = IsEnabled;
      other.SetLatestSourceTimeForWrite = SetLatestSourceTimeForWrite;
      other.IsEnabled = IsEnabled;
      other.Validate = Validate;
      other.SqlStatement = SqlStatement;
      other.InOverview = InOverview;
      other.Timeout = Timeout;
      other.ProcessTimeUtc = ProcessTimeUtc;
      other.RecentlyLoaded = RecentlyLoaded;
      other.LatestSourceTimeUtc = LatestSourceTimeUtc;
      other.Order = Order;
      other.Comment = Comment;
      other.NumRecords = NumRecords;
      other.WarningCount = WarningCount;
      SamplesAndErrors.CopyTo(other.SamplesAndErrors);
      other.ErrorCount = ErrorCount;
      other.ID = ID;

      if (other is BaseSettings baseSettings)
        base.BaseSettingsCopyTo(baseSettings);
    }

    /// <summary>
    ///   Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other" />
    ///   parameter; otherwise, <see langword="false" />.
    /// </returns>
    protected virtual bool BaseSettingsEquals(in IValidatorSetting? other)
    {
      if (other is null)
        return false;
      if (!other.ID.Equals(ID, StringComparison.OrdinalIgnoreCase))
        return false;      
      if (other.RecentlyLoaded != RecentlyLoaded || other.IsEnabled != IsEnabled || other.InOverview != InOverview
          || other.Validate != Validate || other.ShowProgress != ShowProgress)
        return false;
      if (other.NumRecords != NumRecords || other.WarningCount != WarningCount || other.ErrorCount != ErrorCount)
        return false;

      if (other.Timeout != Timeout || other.SetLatestSourceTimeForWrite != SetLatestSourceTimeForWrite)
        return false;
      if ((other.ProcessTimeUtc - ProcessTimeUtc).TotalSeconds > 1)
        return false;
      if ((other.LatestSourceTimeUtc - LatestSourceTimeUtc).TotalSeconds > 1)
        return false;
      if (!other.TemplateName.Equals(TemplateName, StringComparison.Ordinal)
          || !other.SqlStatement.Equals(SqlStatement, StringComparison.OrdinalIgnoreCase))
        return false;

      if (!other.MappingCollection.Equals(MappingCollection))
        return false;
      if (!other.SamplesAndErrors.Equals(SamplesAndErrors))
        return false;
      if (other.Order != Order || !other.Comment.Equals(Comment))
        return false;


      if (other is BaseSettings baseSettings)
        base.BaseSettingsCopyTo(baseSettings);
      return true;
    }
  }
}