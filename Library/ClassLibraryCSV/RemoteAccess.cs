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
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  public class RemoteAccess : INotifyPropertyChanged, ICloneable<RemoteAccess>, IEquatable<RemoteAccess>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  {
    private string m_EncryptedPassword = string.Empty;
    private string m_EncryptedHostName = string.Empty;
    private AccessProtocol m_Protocol = AccessProtocol.Sftp;
    private int m_Port = 22;
    //private string m_User = string.Empty;
    private string m_EncryptedUser = string.Empty;

    [XmlIgnore]
    [Browsable(true)]
    [ReadOnly(true)]
    public virtual bool CanConnect => !string.IsNullOrEmpty(m_EncryptedHostName);

    [XmlAttribute]
    [DefaultValue(AccessProtocol.Sftp)]
    [Browsable(true)]
    [ReadOnly(false)]
    public virtual AccessProtocol Protocol
    {
      get => m_Protocol;
      set
      {
        if (m_Protocol.Equals(value))
          return;
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
      get => m_Port;
      set
      {
        if (value.Equals(m_Port))
          return;
        m_Port = value;
        NotifyPropertyChanged(nameof(Port));
      }
    }

    [XmlAttribute("HostName")]
    [DefaultValue("")]
    [Browsable(true)]
    [ReadOnly(false)]
    public virtual string EncryptedHostName
    {
      get => m_EncryptedHostName;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_EncryptedHostName.Equals(newVal))
          return;
        // setting unencyrpted value will encrpt them automatically, in teh past HostName was not encyrpted
        if (!newVal.IsEncyrpted() && !string.IsNullOrEmpty(newVal))
          newVal = newVal.Encrypt();
        m_EncryptedHostName = newVal;
        NotifyPropertyChanged(nameof(EncryptedHostName));
        NotifyPropertyChanged(nameof(CanConnect));
      }
    }   

    [XmlAttribute("User")]
    [DefaultValue("")]
    [Browsable(true)]
    [ReadOnly(false)]
    public virtual string EncryptedUser
    {
      get => m_EncryptedUser;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_EncryptedUser.Equals(newVal))
          return;
        // setting unencyrpted value will encrpt them automatically, in teh past User was not encyrpted
        if (!newVal.IsEncyrpted() && !string.IsNullOrEmpty(newVal))
          newVal = newVal.Encrypt();
        m_EncryptedUser = newVal;
        NotifyPropertyChanged(nameof(EncryptedUser));
      }
    }

    [XmlAttribute("Password")]
    [DefaultValue("")]
    [Browsable(true)]
    [ReadOnly(false)]
    public virtual string EncryptedPassword
    {
      get => m_EncryptedPassword;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_EncryptedPassword.Equals(newVal))
          return;
        m_EncryptedPassword = newVal;
        NotifyPropertyChanged(nameof(EncryptedPassword));
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
      other.EncryptedPassword = EncryptedPassword;
      other.EncryptedUser = EncryptedUser;
      other.EncryptedHostName = EncryptedHostName;
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
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return string.Equals(EncryptedPassword, other.EncryptedPassword) && m_Protocol == other.Protocol &&
             string.Equals(EncryptedHostName, other.EncryptedHostName, StringComparison.Ordinal) &&
             string.Equals(EncryptedUser, other.EncryptedUser, StringComparison.Ordinal);
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
    public override bool Equals(object obj) => Equals(obj as RemoteAccess);

    /// <summary>Serves as the default hash function. </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = m_EncryptedPassword != null ? m_EncryptedPassword.GetHashCode() : 0;
        hashCode = (hashCode * 397) ^ (int)m_Protocol;
        hashCode = (hashCode * 397) ^
                   (m_EncryptedHostName != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(m_EncryptedHostName) : 0);
        hashCode = (hashCode * 397) ^ (EncryptedUser != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(EncryptedUser) : 0);
        return hashCode;
      }
    }
  }
}