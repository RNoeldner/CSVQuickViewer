using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Serialization;

namespace CsvTools
{
  [Serializable]
  public class SampleAndErrorsInformation : INotifyPropertyChanged, ICloneable<SampleAndErrorsInformation>
  {
    private ObservableCollection<SampleRecordEntry> m_Errors = new ObservableCollection<SampleRecordEntry>();
    private int m_NumErrors = -1;
    private ObservableCollection<SampleRecordEntry> m_Samples = new ObservableCollection<SampleRecordEntry>();

    /// <summary>
    /// Gets or sets the number of entries in Errors, you can overwrite the real number here.
    /// </summary>
    /// <value>
    /// The number of errors, usually matches the number of entries in <see cref="Errors" />
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
        if (m_NumErrors == value)
          return;
        m_NumErrors = value;
        NotifyPropertyChanged(nameof(NumErrors));
      }
    }

    /// <summary>
    /// Gets or sets information on the errors.
    /// </summary>
    /// <value>
    /// The errors.
    /// </value>
    public ObservableCollection<SampleRecordEntry> Errors
    {
      get => m_Errors;
      set
      {
        var newVal = value ?? new ObservableCollection<SampleRecordEntry>();
        if (m_Errors.CollectionEqualWithOrder(newVal))
          return;
        m_Errors = newVal;
        if (m_NumErrors > 0 && Errors.Count > m_NumErrors)
          NumErrors = Errors.Count;
      }
    }

    /// <summary>
    /// Gets or sets information on the samples.
    /// </summary>
    /// <value>
    /// The samples.
    /// </value>
    public ObservableCollection<SampleRecordEntry> Samples
    {
      get => m_Samples;
      set
      {
        var newVal = value ?? new ObservableCollection<SampleRecordEntry>();
        if (m_Samples.CollectionEqualWithOrder(newVal))
          return;
        m_Samples = newVal;
      }
    }

    /// <summary>
    /// Clones this instance into a new instance of the same type
    /// </summary>
    /// <returns></returns>
    public SampleAndErrorsInformation Clone()
    {
      var other = new SampleAndErrorsInformation();
      CopyTo(other);
      return other;
    }

    /// <summary>
    /// Copies all properties to the other instance
    /// </summary>
    /// <param name="other">The other instance</param>
    public void CopyTo(SampleAndErrorsInformation other)
    {
      if (other == null)
        return;
      Samples.CollectionCopy(other.Samples);
      Errors.CollectionCopy(other.Errors);
      other.NumErrors = m_NumErrors;
    }

    /// <summary>
    /// Check if two entries are the same
    /// </summary>
    /// <param name="other">The other ISampleAndErrorsInformation</param>
    /// <returns></returns>
    public bool Equals(SampleAndErrorsInformation other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;

      return
        other.NumErrors == NumErrors &&
        Samples.CollectionEqualWithOrder(other.Samples) &&
        Errors.CollectionEqualWithOrder(other.Errors);
    }

    /// <summary>
    /// Event risen on completed property changed
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///   Notifies the completed property changed
    /// </summary>
    /// <param name="info">The info.</param>
    public virtual void NotifyPropertyChanged(string info)
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
  }
}