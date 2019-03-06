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

using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;

namespace CsvTools
{
  /// <summary>
  ///   Pretty Good Privacy Storage
  /// </summary>
  /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
  [Serializable]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  public class PGPKeyStorage : INotifyPropertyChanged, IEquatable<PGPKeyStorage>, ICloneable<PGPKeyStorage>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  {
    private static readonly string m_PgpDecryption =
      "dLz4/oPycBMLecvRato2sYqKHpUuvrOwsCa4N3b2DUnLfk8In4Vbyscj8HTex5r1u0OSfxMvEojPM+JMw7xNPbu3Yvpr+A3OD3tb0pq27DFZPkTmOcB/NrTnzo9F91jtnonqNlXRwJOCzu+rCsVqZ5YXg==0cQKc"
        .Decrypt();

    private readonly List<string> m_PrivateKeyRingBundle = new List<string>();
    private readonly List<string> m_PublicKeyRingBundle = new List<string>();
    private bool m_AllowSavingPassphrase = false;
    private string m_EncryptedPassphase = string.Empty;
    private IDictionary<string, PgpPublicKey> m_Recipients;

    [XmlIgnore]
    public virtual bool Specified => m_PrivateKeyRingBundle.Count > 0 || m_PublicKeyRingBundle.Count > 0 || !string.IsNullOrEmpty(EncryptedPassphase);

#pragma warning disable CA1819 // Properties should not return arrays
    [XmlElement]
    public virtual string[] PrivateKeys
    {
      get
      {
        Contract.Ensures(Contract.Result<string[]>() != null);
        return m_PrivateKeyRingBundle.ToArray();
      }
      set
      {
        m_PrivateKeyRingBundle.Clear();
        if (value != null)
          m_PrivateKeyRingBundle.AddRange(value);
        NotifyPropertyChanged(nameof(PrivateKeys));
        m_Recipients = null;
      }
    }

    [XmlElement]
    public virtual string[] PublicKeys
    {
      get
      {
        Contract.Ensures(Contract.Result<string[]>() != null);
        return m_PublicKeyRingBundle.ToArray();
      }
      set
      {
        m_PublicKeyRingBundle.Clear();
        if (value != null)
          m_PublicKeyRingBundle.AddRange(value);
        NotifyPropertyChanged(nameof(PublicKeys));
        m_Recipients = null;
      }
    }
#pragma warning restore CA1819 // Properties should not return arrays
    [XmlElement]
    [DefaultValue("")]
    public virtual string EncryptedPassphase
    {
      get
      {
        Contract.Ensures(Contract.Result<string>() != null);
        return m_EncryptedPassphase;
      }
      set
      {
        Contract.Ensures(m_EncryptedPassphase != null);
        var newVal = value ?? string.Empty;
        if (m_EncryptedPassphase.Equals(newVal, StringComparison.Ordinal)) return;
        m_EncryptedPassphase = newVal;
        NotifyPropertyChanged(nameof(EncryptedPassphase));
      }
    }

    [XmlIgnore]
    public virtual bool EncryptedPassphaseSpecified => m_AllowSavingPassphrase && m_EncryptedPassphase.Length > 0;

    [XmlElement]
    [DefaultValue(false)]
    public virtual bool AllowSavingPassphrase
    {
      get => m_AllowSavingPassphrase;

      set
      {
        if (m_AllowSavingPassphrase == value) return;
        m_AllowSavingPassphrase = value;
        NotifyPropertyChanged(nameof(AllowSavingPassphrase));
      }
    }

    public virtual PGPKeyStorage Clone()
    {
      Contract.Ensures(Contract.Result<PGPKeyStorage>() != null);
      var ret = new PGPKeyStorage();
      CopyTo(ret);
      return ret;
    }

    public virtual void CopyTo(PGPKeyStorage other)
    {
      if (other == null)
        return;
      other.PrivateKeys = PrivateKeys;
      other.PublicKeys = PublicKeys;
      other.EncryptedPassphase = EncryptedPassphase;
      other.AllowSavingPassphrase = AllowSavingPassphrase;
    }

    public virtual bool Equals(PGPKeyStorage other)
    {
      if (other == null)
        return false;
      if (ReferenceEquals(other, this))
        return true;
      return
        EncryptedPassphase.Equals(other.EncryptedPassphase, StringComparison.Ordinal)
        && PrivateKeys.CollectionEqual(other.PrivateKeys)
        && PublicKeys.CollectionEqual(other.PublicKeys);
    }

    /// <summary>
    ///   Occurs when a property value changes.
    /// </summary>
    public virtual event PropertyChangedEventHandler PropertyChanged;

    public static bool IsValidKeyRingBundle(string input, bool privateKey, out string message)
    {
      message = string.Empty;
      if (string.IsNullOrEmpty(input))
        return false;
      using (var keyIn = new MemoryStream(Encoding.UTF8.GetBytes(input)))
      {
        try
        {
          if (privateKey)
          {
            var testPriv = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(keyIn));
            return testPriv.Count != 0;
          }

          {
            var testPub = new PgpPublicKeyRingBundle(PgpUtilities.GetDecoderStream(keyIn));
            return testPub.Count != 0;
          }
        }
        catch (Exception ex)
        {
          message = ex.SourceExceptionMessage();
          // ignored
        }
      }

      return false;
    }

    public virtual void AddPrivateKey(string key)
    {
      if (!IsValidKeyRingBundle(key, true, out _)) return;
      var encKey = key.Encrypt(m_PgpDecryption);
      if (m_PrivateKeyRingBundle.Any(x => key.Equals(x.Decrypt(m_PgpDecryption), StringComparison.Ordinal))) return;
      m_PrivateKeyRingBundle.Add(encKey);
      NotifyPropertyChanged(nameof(PrivateKeys));
      m_Recipients = null;
    }

    public virtual void AddPublicKey(string key)
    {
      if (!IsValidKeyRingBundle(key, false, out _)) return;
      var encKey = key.Encrypt(m_PgpDecryption);
      if (m_PublicKeyRingBundle.Any(x => key.Equals(x.Decrypt(m_PgpDecryption), StringComparison.Ordinal))) return;
      m_PublicKeyRingBundle.Add(encKey);
      NotifyPropertyChanged(nameof(PublicKeys));
      m_Recipients = null;
    }

    public virtual ICollection<string> GetPrivateKeyRingBundleList()
    {
      var result = new List<string>();
      foreach (var pgpSec in GetSecretKeyRingBundles())
      {
        var sb = new StringBuilder();
        foreach (PgpSecretKeyRing ring in pgpSec.GetKeyRings())
          foreach (PgpSecretKey key in ring.GetSecretKeys())
            if (key.UserIds.Count() > 0)
            {
              foreach (var usr in key.UserIds)
                sb.AppendFormat("{0},", usr);
              sb.Length--;
            }

        result.Add(sb.ToString());
      }

      return result;
    }

    public virtual ICollection<string> GetPublicKeyRingBundleList()
    {
      var result = new List<string>();
      foreach (var pgpSec in GetPublicKeyRingBundles())
      {
        var sb = new StringBuilder();

        foreach (PgpPublicKeyRing kRing in pgpSec.GetKeyRings())
        {
          var key = kRing.GetPublicKeys().Cast<PgpPublicKey>().FirstOrDefault(k => k.IsEncryptionKey);
          if (!(key?.GetUserIds().Count() > 0)) continue;
          foreach (var usr in key.GetUserIds())
            sb.AppendFormat("{0},", usr);
          sb.Length--;
        }

        result.Add(sb.ToString());
      }

      return result;
    }

    public virtual IEnumerable<PgpPublicKeyRingBundle> GetPublicKeyRingBundles()
    {
      var valid = new List<PgpPublicKeyRingBundle>();

      if (PublicKeys == null) return valid;
      var invalid = new List<string>();
      foreach (var encryptedKey in PublicKeys)
        try
        {
          using (var keyIn = new MemoryStream(Encoding.UTF8.GetBytes(encryptedKey.Decrypt(m_PgpDecryption))))
          {
            valid.Add(new PgpPublicKeyRingBundle(PgpUtilities.GetDecoderStream(keyIn)));
          }
        }
        catch (Exception)
        {
          invalid.Add(encryptedKey);
        }

      foreach (var encryptedKey in invalid) m_PublicKeyRingBundle.Remove(encryptedKey);
      return valid;
    }

    public virtual ICollection<string> GetRecipientList()
    {
      return GetRecipients().Keys;
    }

    public virtual IDictionary<string, PgpPublicKey> GetRecipients()
    {
      if (m_Recipients != null) return m_Recipients;
      m_Recipients = new Dictionary<string, PgpPublicKey>(StringComparer.OrdinalIgnoreCase);

      if (PrivateKeys != null)
      {
        foreach (var encryptedKey in PrivateKeys)
          using (var keyIn = new MemoryStream(Encoding.UTF8.GetBytes(encryptedKey.Decrypt(m_PgpDecryption))))
          {
            var pgpSec = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(keyIn));

            foreach (PgpSecretKeyRing ring in pgpSec.GetKeyRings())
            {
              var key = ring.GetPublicKey();
              if (!key.IsEncryptionKey) continue;
              foreach (var userID in key.GetUserIds())
                if (!m_Recipients.ContainsKey(userID.ToString()))
                  m_Recipients.Add(userID.ToString(), key);
              // get the strongest key
              foreach (var userID in key.GetUserIds())
                if (m_Recipients[userID.ToString()].BitStrength < key.BitStrength)
                  m_Recipients[userID.ToString()] = key;
            }
          }
      }

      if (PublicKeys != null)
      {
        foreach (var encryptedKey in PublicKeys)
          using (var keyIn = new MemoryStream(Encoding.UTF8.GetBytes(encryptedKey.Decrypt(m_PgpDecryption))))
          {
            using (var decStream = PgpUtilities.GetDecoderStream(keyIn))
            {
              var pgpSec = new PgpPublicKeyRingBundle(decStream);
              foreach (PgpPublicKeyRing kRing in pgpSec.GetKeyRings())
              {
                var key = kRing.GetPublicKeys().Cast<PgpPublicKey>().FirstOrDefault(k => k.IsEncryptionKey);
                if (key == null) continue;
                foreach (var userID in key.GetUserIds())
                  if (!m_Recipients.ContainsKey(userID.ToString()))
                    m_Recipients.Add(userID.ToString(), key);
                // get the strongest key
                foreach (var userID in key.GetUserIds())
                  if (m_Recipients[userID.ToString()].BitStrength < key.BitStrength)
                    m_Recipients[userID.ToString()] = key;
              }
            }
          }
      }
      return m_Recipients;
    }

    public virtual IEnumerable<PgpSecretKeyRingBundle> GetSecretKeyRingBundles()
    {
      var valid = new List<PgpSecretKeyRingBundle>();

      if (PrivateKeys == null) return valid;
      var invalid = new List<string>();
      foreach (var encryptedKey in PrivateKeys)
        try
        {
          using (var keyIn = new MemoryStream(Encoding.UTF8.GetBytes(encryptedKey.Decrypt(m_PgpDecryption))))
          {
            valid.Add(new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(keyIn)));
          }
        }
        catch (Exception)
        {
          invalid.Add(encryptedKey);
        }

      foreach (var encryptedKey in invalid) m_PrivateKeyRingBundle.Remove(encryptedKey);
      return valid;
    }

    public virtual void NotifyPropertyChanged(string info)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
    }

    /// <summary>
    /// Decrypts the encrypted stream
    /// </summary>
    /// <param name="inputFile">To decrypt.</param>
    /// <param name="passphrase">The passphrase.</param>
    /// <returns></returns>
    /// <exception cref="PgpException">
    /// Encrypted message contains a signed message - not literal data.
    /// or
    /// Message is not a simple encrypted file - type unknown.
    /// </exception>
    public virtual Stream PgpDecrypt(Stream inputFile, System.Security.SecureString passphrase)
    {
      // Never use using here this would close the decoder stream and we would not be
      // able to read later on from the input stream
      var decoderStream = PgpUtilities.GetDecoderStream(inputFile);

      var encryptedDataList = (PgpEncryptedDataList)new PgpObjectFactory(decoderStream)?.NextPgpObject();

      var pgpObject = new PgpObjectFactory(GetDecyptedDataStream(encryptedDataList, passphrase)).NextPgpObject();
      if (pgpObject is PgpCompressedData data)
        pgpObject = (new PgpObjectFactory(data.GetDataStream())).NextPgpObject();

      if (pgpObject is PgpLiteralData literalData)
        return literalData.GetInputStream();

      if (pgpObject is PgpOnePassSignatureList)
        throw new PgpException("Encrypted message contains a signed message - not literal data.");

      throw new PgpException("Message is not a simple encrypted file - type unknown.");
    }

    /// <summary>
    /// Gets the encrypted key identifier for an input stream
    /// </summary>
    /// <param name="inputFile">The input file.</param>
    /// <returns></returns>
    public virtual string GetEncryptedKeyID(Stream inputFile)
    {
      using (var decoderStream = PgpUtilities.GetDecoderStream(inputFile))
      {
        var encryptedDataList = (PgpEncryptedDataList)new PgpObjectFactory(decoderStream)?.NextPgpObject();
        var encryptedData = encryptedDataList.GetEncryptedDataObjects().Cast<PgpPublicKeyEncryptedData>().FirstOrDefault();

        var knownRecipient = GetRecipients()?.Values.Where(x => x.KeyId == encryptedData.KeyId).First();
        if (knownRecipient != null)
          foreach (var userID in knownRecipient.GetUserIds())
            return userID.ToString();
        else
          return $"Unknown recipient, KeyID {encryptedData.KeyId:X}";
      }
      return string.Empty;
    }

    public virtual void PgpEncrypt(Stream toEncrypt, Stream outStream, string recipient, IProcessDisplay processDisplay)
    {
      var encryptionKey = GetEncryptionKey(recipient);
      var encryptor = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.TripleDes, false, new SecureRandom());
      var literalizer = new PgpLiteralDataGenerator();
      var compressor = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip);
      encryptor.AddMethod(encryptionKey);

      using (var encryptedStream = encryptor.Open(outStream, new byte[16384]))
      {
        using (var compressedStream = compressor.Open(encryptedStream))
        {
          var copyBuffer = new byte[16384];
          using (var literalStream = literalizer.Open(compressedStream, PgpLiteralDataGenerator.Utf8, "PGPStream",
            DateTime.Now, new byte[4096]))
          {
            var processDispayTime = processDisplay as IProcessDisplayTime;
            var max = (int)(toEncrypt.Length / copyBuffer.Length);
            processDisplay.Maximum = max;
            var count = 0;
            int length;
            while ((length = toEncrypt.Read(copyBuffer, 0, copyBuffer.Length)) > 0)
            {
              processDisplay.CancellationToken.ThrowIfCancellationRequested();
              literalStream.Write(copyBuffer, 0, length);
              count++;
              processDisplay.SetProcess($"PGP Encrypting {StringConversion.DynamicStorageSize(toEncrypt.Length)} - Step {count:N0}/{max:N0}", count);
            }
          }
        }
      }
    }

    public virtual void RemovePrivateKey(int index)
    {
      m_PrivateKeyRingBundle.RemoveAt(index);
      NotifyPropertyChanged(nameof(PrivateKeys));
      m_Recipients = null;
    }

    public virtual void RemovePublicKey(int index)
    {
      m_PublicKeyRingBundle.RemoveAt(index);
      NotifyPropertyChanged(nameof(PublicKeys));
      m_Recipients = null;
    }

    private static char[] ToCharArray(System.Security.SecureString text)
    {
      var length = text.Length;
      var pointer = IntPtr.Zero;
      var chars = new char[length];

      try
      {
        pointer = Marshal.SecureStringToBSTR(text);
        Marshal.Copy(pointer, chars, 0, length);
        return chars;
      }
      finally
      {
        if (pointer != IntPtr.Zero)
          Marshal.ZeroFreeBSTR(pointer);
      }
    }

    private Stream GetDecyptedDataStream(PgpEncryptedDataList enc, System.Security.SecureString passphrase)
    {
      if (PrivateKeys == null) throw new PgpException("Secret key for message not found.");
      foreach (var encryptedKey in PrivateKeys)
        using (var keyIn = new MemoryStream(Encoding.UTF8.GetBytes(encryptedKey.Decrypt(m_PgpDecryption))))
        {
          var pgpSec = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(keyIn));
          foreach (PgpPublicKeyEncryptedData pked in enc.GetEncryptedDataObjects())
          {
            var pgpSecKey = pgpSec.GetSecretKey(pked.KeyId);
            if (pgpSecKey == null) continue;
            try
            {
              var key = pgpSecKey.ExtractPrivateKey(ToCharArray(passphrase));
              if (key != null)
                return pked.GetDataStream(key);
            }
            catch (PgpException)
            {
            }
          }
        }

      throw new PgpException("Secret key for message not found.");
    }

    private PgpPublicKey GetEncryptionKey(string recipient)
    {
      var recipients = GetRecipients();
      foreach (var kvp in recipients)
        if (kvp.Key.Equals(recipient, StringComparison.OrdinalIgnoreCase))
          return kvp.Value;

      foreach (var kvp in recipients)
        if (kvp.Key.ToUpperInvariant().Contains(recipient.ToUpperInvariant()))
          return kvp.Value;

      throw new PgpException($"No encryption key found for {recipient} in known public keys.");
    }

    public override bool Equals(object obj)
    {
      if (obj is null) return false;
      if (ReferenceEquals(this, obj)) return true;
      return (obj is PGPKeyStorage typed) && Equals(typed);
    }

    /*
    /// <summary>Serves as the default hash function. </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = m_PrivateKeyRingBundle != null ? m_PrivateKeyRingBundle.GetHashCode() : 0;
        hashCode = (hashCode * 397) ^ (m_PublicKeyRingBundle != null ? m_PublicKeyRingBundle.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ m_AllowSavingPassphrase.GetHashCode();
        hashCode = (hashCode * 397) ^ (m_EncryptedPassphase != null ? m_EncryptedPassphase.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (m_Recipients != null ? m_Recipients.GetHashCode() : 0);
        return hashCode;
      }
    }
    */
  }
}