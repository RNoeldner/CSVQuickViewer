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
    private int m_NumErrors;
    private readonly UniqueObservableCollection<SampleRecordEntry> m_Errors;
    private readonly UniqueObservableCollection<SampleRecordEntry> m_Samples;

    [Obsolete("Used for XML Serialization")]
    public SampleAndErrorsInformation() : this(-1, null, null)
    {
    }

    [JsonConstructor]
    public SampleAndErrorsInformation(int? numErrors = -1, IEnumerable<SampleRecordEntry>? errors = null,
      IEnumerable<SampleRecordEntry>? samples = null)
    {
      m_NumErrors = numErrors ?? -1;
      m_Errors = new UniqueObservableCollection<SampleRecordEntry>();
      if (errors != null)
        m_Errors.AddRangeNoClone(errors);
      m_Errors.CollectionItemPropertyChanged += (o, s) => NotifyPropertyChanged(nameof(Errors));

      m_Samples = new UniqueObservableCollection<SampleRecordEntry>();
      if (samples != null)
        m_Samples.AddRangeNoClone(samples);
      m_Samples.CollectionItemPropertyChanged += (o, s) => NotifyPropertyChanged(nameof(Samples));
    }


    /// <summary>
    ///   Gets or sets information on the errors.
    /// </summary>
    /// <value>The errors.</value>
    public UniqueObservableCollection<SampleRecordEntry> Errors => m_Errors;

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
    public UniqueObservableCollection<SampleRecordEntry> Samples => m_Samples;

    /// <inheritdoc />
    public object Clone()
    {
      return new SampleAndErrorsInformation(m_NumErrors, m_Errors, m_Samples);
    }

    /// <summary>
    ///   Copies all properties to the other instance
    /// </summary>
    /// <param name="other">The other instance</param>
    public void CopyTo(SampleAndErrorsInformation other)
    {
      other.m_Samples.Clear();
      other.m_Samples.AddRange(m_Samples);
      other.m_Errors.Clear();
      other.m_Errors.AddRange(m_Errors);
      other.m_NumErrors = m_NumErrors;
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

      return m_NumErrors == other.m_NumErrors
             && m_Samples.Equals(other.m_Samples)
             && m_Errors.Equals(other.m_Errors);
    }
  }
}