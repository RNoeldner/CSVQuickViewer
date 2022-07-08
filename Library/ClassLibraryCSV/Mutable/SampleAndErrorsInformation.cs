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
  /// <summary>
  ///   This class is only used by the Validator but since we can not extend classes with new
  ///   properties, it needs to be defined here
  /// </summary>
  [Serializable]
  public sealed class SampleAndErrorsInformation : NotifyPropertyChangedBase, IWithCopyTo<SampleAndErrorsInformation>
  {
    private readonly UniqueObservableCollection<SampleRecordEntry> m_Errors = new UniqueObservableCollection<SampleRecordEntry>();

    private int m_NumErrors = -1;
    private readonly UniqueObservableCollection<SampleRecordEntry> m_Samples = new UniqueObservableCollection<SampleRecordEntry>();

    public SampleAndErrorsInformation()
    {
      m_Errors.CollectionItemPropertyChanged += (o, s) => NotifyPropertyChanged(nameof(Errors));
      m_Samples.CollectionItemPropertyChanged += (o, s) => NotifyPropertyChanged(nameof(Samples));
    }

    /// <summary>
    ///   Gets or sets information on the errors.
    /// </summary>
    /// <value>The errors.</value>
    public UniqueObservableCollection<SampleRecordEntry> Errors
    {
      get => m_Errors;
    }

    public bool ErrorsSpecified => Errors.Count > 0;

    /// <summary>
    ///   Gets or sets the number of entries in Errors, you can overwrite the real number here.
    /// </summary>
    /// <value>
    ///   The number of errors, usually matches the number of entries in <see cref="Errors" />
    /// </value>
    [XmlAttribute]
    [DefaultValue(-1)]
    public int NumErrors
    {
      get
      {
        if (m_NumErrors == -1 && Errors.Count > 0)
          return Errors.Count;
        return m_NumErrors;
      }
      set => SetField(ref m_NumErrors, Errors.Count > 0 && value < Errors.Count ? Errors.Count : value);
    }

    /// <summary>
    ///   Gets or sets information on the samples.
    /// </summary>
    /// <value>The samples.</value>
    public UniqueObservableCollection<SampleRecordEntry> Samples
    {
      get => m_Samples;
    }

    public bool SamplesSpecified => Samples.Count > 0;

    /// <inheritdoc />
    public object Clone()
    {
      var other = new SampleAndErrorsInformation();
      CopyTo(other);
      return other;
    }

    /// <summary>
    ///   Copies all properties to the other instance
    /// </summary>
    /// <param name="other">The other instance</param>
    public void CopyTo(SampleAndErrorsInformation other)
    {
      other.Samples.Clear();
      other.Samples.AddRange(Samples);
      other.Errors.Clear();
      other.Errors.AddRange(Errors);
      other.m_NumErrors = m_NumErrors;
    }

    /// <summary>
    ///   Check if two entries are the same
    /// </summary>
    /// <param name="other">The other ISampleAndErrorsInformation</param>
    /// <returns></returns>
    public bool Equals(SampleAndErrorsInformation? other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;

      return m_NumErrors  == other.m_NumErrors
            && Samples.Equals(other.Samples)
            && Errors.Equals(other.Errors);
    }
  }
}