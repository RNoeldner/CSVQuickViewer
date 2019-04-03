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
  [Serializable]
  public enum AccessProtocol
  {
    /// <summary>
    /// File System Copy
    /// </summary>
    Local = -1,

    /// <summary>
    /// sFTP Access
    /// </summary>
    Sftp = 0
  }

  [Serializable]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  public class RemoteAccess : INotifyPropertyChanged, ICloneable<RemoteAccess>, IEquatable<RemoteAccess>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  {
    private string m_EncryptedPassword = string.Empty;
    private string m_HostName = string.Empty;
    private AccessProtocol m_Protocol = AccessProtocol.Sftp;
    private int m_Port = 22;
    private string m_User = string.Empty;

    [XmlIgnore]
    [Browsable(true)]
    [ReadOnly(true)]
    public virtual bool CanConnect => !string.IsNullOrEmpty(m_HostName);

    [XmlAttribute]
    [DefaultValue(AccessProtocol.Sftp)]
    [Browsable(true)]
    [ReadOnly(false)]
    public virtual AccessProtocol Protocol
    {
      get => m_Protocol;
      set
      {
        if (m_Protocol.Equals(value)) return;
        m_Protocol = value;
        NotifyPropertyChanged(nameof(Protocol));
      }
    }

    [XmlAttribute]
    [DefaultValue(22)]
    [Browsable(true)]
    [ReadOnly(false)]
    public int Port
    {
      get { return m_Port; }
      set
      {
        if (value.Equals(m_Port)) return;
        m_Port = value;
        NotifyPropertyChanged(nameof(Port));
      }
    }

    [XmlAttribute]
    [DefaultValue("")]
    [Browsable(true)]
    [ReadOnly(false)]
    public virtual string HostName
    {
      get => m_HostName;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_HostName.Equals(newVal)) return;
        m_HostName = newVal;
        NotifyPropertyChanged(nameof(HostName));
        NotifyPropertyChanged(nameof(CanConnect));
      }
    }

    [XmlAttribute]
    [DefaultValue("")]
    [Browsable(true)]
    [ReadOnly(false)]
    public virtual string User
    {
      get => m_User;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_User.Equals(newVal)) return;
        m_User = newVal;
        NotifyPropertyChanged(nameof(User));
      }
    }

    [XmlAttribute]
    [DefaultValue("")]
    [Browsable(true)]
    [ReadOnly(false)]
    public virtual string Password
    {
      get => m_EncryptedPassword;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_EncryptedPassword.Equals(newVal)) return;
        m_EncryptedPassword = newVal;
        NotifyPropertyChanged(nameof(Password));
      }
    }

    public RemoteAccess Clone()
    {
      var newAccess = new RemoteAccess();
      CopyTo(newAccess);
      return newAccess;
    }

    public void CopyTo(RemoteAccess other)
    {
      if (other == null)
        return;
      other.Password = Password;
      other.User = User;
      other.HostName = HostName;
      other.Protocol = Protocol;
    }

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise,
    ///   <see langword="false" />.
    /// </returns>
    public bool Equals(RemoteAccess other)
    {
      if (other is null) return false;
      if (ReferenceEquals(this, other)) return true;
      return string.Equals(Password, other.Password) && m_Protocol == other.Protocol &&
             string.Equals(HostName, other.HostName, StringComparison.OrdinalIgnoreCase) &&
             string.Equals(User, other.User, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///   Occurs when a property value changes.
    /// </summary>
    public virtual event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///   Notifies the property changed.
    /// </summary>
    /// <param name="info">The info.</param>
    public virtual void NotifyPropertyChanged(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <param name="obj">The object to compare with the current object. </param>
    /// <returns>
    ///   <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object obj)
    {
      if (obj is null) return false;
      if (ReferenceEquals(this, obj)) return true;
      return (obj is RemoteAccess typed) && Equals(typed);
    }

    /*
    /// <summary>Serves as the default hash function. </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = m_EncryptedPassword != null ? m_EncryptedPassword.GetHashCode() : 0;
        hashCode = (hashCode * 397) ^ (int) m_Protocol;
        hashCode = (hashCode * 397) ^
                   (m_HostName != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(m_HostName) : 0);
        hashCode = (hashCode * 397) ^ (m_User != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(m_User) : 0);
        return hashCode;
      }
    }
    */
  }
}