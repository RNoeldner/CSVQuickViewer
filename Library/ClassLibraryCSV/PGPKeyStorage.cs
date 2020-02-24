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
			"dLz4/oPycBMLecvRato2sYqKHpUuvrOwsCa4N3b2DUnLfk8In4Vbyscj8HTex5r1u0OSfxMvEojPM+JMw7xNPbu3Yvpr+A3OD3tb0pq27DFZPkTmOcB/NrTnzo9F91jtnonqNlXRwJOCzu+rCsVqZ5YXg0cQKc"
				.Decrypt();

		private readonly List<string> m_EncryptedPrivateKeyRingBundle = new List<string>();
		private readonly List<string> m_EncryptedPublicKeyRingBundle = new List<string>();
		private bool m_AllowSavingPassphrase;
		private string m_EncryptedPassphrase = string.Empty;
		private IDictionary<string, PgpPublicKey> m_Recipients;

		[XmlIgnore]
		public virtual bool Specified => m_EncryptedPrivateKeyRingBundle.Count > 0 ||
																		 m_EncryptedPublicKeyRingBundle.Count > 0 ||
																		 !string.IsNullOrEmpty(EncryptedPassphase);

		[XmlElement("PrivateKeys")]
		public virtual string[] PrivateKeys
		{
			get
			{
				Contract.Ensures(Contract.Result<string[]>() != null);
				return m_EncryptedPrivateKeyRingBundle.ToArray();
			}
			set
			{
				m_EncryptedPrivateKeyRingBundle.Clear();
				if (value != null)
					m_EncryptedPrivateKeyRingBundle.AddRange(value);
				NotifyPropertyChanged(nameof(PrivateKeys));
				m_Recipients = null;
			}
		}

		[XmlElement("PublicKeys")]
		public virtual string[] PublicKeys
		{
			get
			{
				Contract.Ensures(Contract.Result<string[]>() != null);
				return m_EncryptedPublicKeyRingBundle.ToArray();
			}
			set
			{
				m_EncryptedPublicKeyRingBundle.Clear();
				if (value != null)
					m_EncryptedPublicKeyRingBundle.AddRange(value);
				NotifyPropertyChanged(nameof(PublicKeys));
				m_Recipients = null;
			}
		}

		[XmlElement]
		[DefaultValue("")]
		public virtual string EncryptedPassphase
		{
			get
			{
				Contract.Ensures(Contract.Result<string>() != null);
				return m_EncryptedPassphrase;
			}
			set
			{
				Contract.Ensures(m_EncryptedPassphrase != null);
				var newVal = value ?? string.Empty;
				if (m_EncryptedPassphrase.Equals(newVal, StringComparison.Ordinal))
					return;
				m_EncryptedPassphrase = newVal;
				NotifyPropertyChanged(nameof(EncryptedPassphase));
			}
		}

		[XmlIgnore]
		public virtual bool EncryptedPassphaseSpecified => m_AllowSavingPassphrase && m_EncryptedPassphrase.Length > 0;

		[XmlElement]
		[DefaultValue(false)]
		public virtual bool AllowSavingPassphrase
		{
			get => m_AllowSavingPassphrase;

			set
			{
				if (m_AllowSavingPassphrase == value)
					return;
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
				AllowSavingPassphrase.Equals(other.AllowSavingPassphrase)
				&& EncryptedPassphase.Equals(other.EncryptedPassphase, StringComparison.Ordinal)
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
			if (!IsValidKeyRingBundle(key, true, out _))
				return;
			foreach (var x in m_EncryptedPrivateKeyRingBundle)
				if (key.Equals(x.Decrypt(m_PgpDecryption), StringComparison.Ordinal))
					return;
			var encKey = key.Encrypt(m_PgpDecryption);
			m_EncryptedPrivateKeyRingBundle.Add(encKey);
			NotifyPropertyChanged(nameof(PrivateKeys));
			m_Recipients = null;
		}

		public virtual void AddPublicKey(string key)
		{
			if (!IsValidKeyRingBundle(key, false, out _))
				return;

			foreach (var x in m_EncryptedPublicKeyRingBundle)
				if (key.Equals(x.Decrypt(m_PgpDecryption), StringComparison.Ordinal))
					return;
			var encKey = key.Encrypt(m_PgpDecryption);
			m_EncryptedPublicKeyRingBundle.Add(encKey);
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

		/// <summary>
		///   Gets the Users in all Public keys
		/// </summary>
		/// <returns>A a list with one row for each key containing  comma separated UserIDs</returns>
		public virtual ICollection<string> GetPublicKeyRingBundleList()
		{
			var result = new List<string>();
			foreach (var pgpSec in GetPublicKeyRingBundles())
			{
				var sb = new StringBuilder();
				foreach (PgpPublicKeyRing kRing in pgpSec.GetKeyRings())
					foreach (PgpPublicKey key in kRing.GetPublicKeys())
					{
						if (!key.IsEncryptionKey)
							continue;
						foreach (var usr in key.GetUserIds())
						{
							sb.AddComma();
							sb.Append(usr);
						}
					}

				result.Add(sb.ToString());
			}

			return result;
		}

		public virtual IEnumerable<PgpPublicKeyRingBundle> GetPublicKeyRingBundles()
		{
			var valid = new List<PgpPublicKeyRingBundle>();

			if (PublicKeys == null)
				return valid;
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

			foreach (var encryptedKey in invalid)
				m_EncryptedPublicKeyRingBundle.Remove(encryptedKey);
			return valid;
		}

		public virtual ICollection<string> GetRecipientList() => GetRecipients().Keys;

		public virtual IDictionary<string, PgpPublicKey> GetRecipients()
		{
			if (m_Recipients != null)
				return m_Recipients;
			m_Recipients = new Dictionary<string, PgpPublicKey>(StringComparer.OrdinalIgnoreCase);

			if (PublicKeys != null)
				foreach (var encryptedKey in PublicKeys)
					using (var keyIn = new MemoryStream(Encoding.UTF8.GetBytes(encryptedKey.Decrypt(m_PgpDecryption))))
					{
						using (var decStream = PgpUtilities.GetDecoderStream(keyIn))
						{
							var pgpSec = new PgpPublicKeyRingBundle(decStream);
							foreach (PgpPublicKeyRing kRing in pgpSec.GetKeyRings())
								foreach (PgpPublicKey key in kRing.GetPublicKeys())
								{
									// no not return signature keys, keys that are reovoked or expired
									if (!key.IsEncryptionKey || key.IsRevoked() || !key.GetUserIds().GetEnumerator().MoveNext() || (key.GetValidSeconds() > 0 && key.CreationTime.AddSeconds(key.GetValidSeconds()) < DateTime.Now))
										continue;
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

			if (PrivateKeys != null)
				foreach (var encryptedKey in PrivateKeys)
					using (var keyIn = new MemoryStream(Encoding.UTF8.GetBytes(encryptedKey.Decrypt(m_PgpDecryption))))
					{
						var pgpSec = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(keyIn));

						foreach (PgpSecretKeyRing ring in pgpSec.GetKeyRings())
						{
							var key = ring.GetPublicKey();
							// no not return signature keys, keys that are reovoked or expired
							if (!key.IsEncryptionKey || key.IsRevoked() || !key.GetUserIds().GetEnumerator().MoveNext() || (key.GetValidSeconds() > 0 && key.CreationTime.AddSeconds(key.GetValidSeconds()) < DateTime.Now))
								continue;
								foreach (var userID in key.GetUserIds())
								if (!m_Recipients.ContainsKey(userID.ToString()))
									m_Recipients.Add(userID.ToString(), key);
							// get the strongest key
							foreach (var userID in key.GetUserIds())
								if (m_Recipients[userID.ToString()].BitStrength < key.BitStrength)
									m_Recipients[userID.ToString()] = key;
						}
					}

			return m_Recipients;
		}

		public virtual IEnumerable<PgpSecretKeyRingBundle> GetSecretKeyRingBundles()
		{
			var valid = new List<PgpSecretKeyRingBundle>();

			if (PrivateKeys == null)
				return valid;
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

			foreach (var encryptedKey in invalid)
				m_EncryptedPrivateKeyRingBundle.Remove(encryptedKey);
			return valid;
		}

		public virtual void NotifyPropertyChanged(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

		/// <summary>
		///   Decrypts the encrypted stream
		/// </summary>
		/// <param name="inputFile">To decrypt.</param>
		/// <param name="passphrase">The passphrase.</param>
		/// <returns></returns>
		/// <exception cref="PgpException">
		///   Encrypted message contains a signed message - not literal data.
		///   or
		///   Message is not a simple encrypted file - type unknown.
		/// </exception>
		public virtual Stream PgpDecrypt(Stream inputFile, System.Security.SecureString passphrase)
		{
			// Never use using here this would close the decoder stream and we would not be
			// able to read later on from the input stream
			var decoderStream = PgpUtilities.GetDecoderStream(inputFile);

			var encryptedDataList = (PgpEncryptedDataList)new PgpObjectFactory(decoderStream).NextPgpObject();

			var pgpObject = new PgpObjectFactory(GetDecryptedDataStream(encryptedDataList, passphrase)).NextPgpObject();
			if (pgpObject is PgpCompressedData data)
				pgpObject = new PgpObjectFactory(data.GetDataStream()).NextPgpObject();

			if (pgpObject is PgpLiteralData literalData)
				return literalData.GetInputStream();

			if (pgpObject is PgpOnePassSignatureList)
				throw new PgpException("Encrypted message contains a signed message - not literal data.");

			throw new PgpException("Message is not a simple encrypted file - type unknown.");
		}

		/// <summary>
		///   Gets the encrypted key identifier for an input stream
		/// </summary>
		/// <param name="inputFile">The input file.</param>
		/// <returns>The very first matching recipient</returns>
		public virtual string GetEncryptedKeyID(Stream inputFile)
		{
			Contract.Ensures(Contract.Result<string>() != null);
			using (var decoderStream = PgpUtilities.GetDecoderStream(inputFile))
			{
				var recipientsPublicKeys = GetRecipients();
				if (recipientsPublicKeys == null)
					throw new EncryptionException("Could not get list of recipients from known private or public keys, decoding is not possible");
				var encryptedDataList = (PgpEncryptedDataList)new PgpObjectFactory(decoderStream).NextPgpObject();
				if (encryptedDataList != null)
				{
					foreach (PgpPublicKeyEncryptedData data in encryptedDataList.GetEncryptedDataObjects())
					{
						foreach (var keyValue in recipientsPublicKeys)
						{
							if (keyValue.Value.KeyId != data.KeyId)
								continue;
							foreach (var userID in keyValue.Value.GetUserIds())
								return userID.ToString();
						}
						break;
					}
				}				
				throw new EncryptionException("Could not locate pgp encrypted data, decoding is not possible");
			}
		}

		/// <summary>
		/// This is not working as of now, ideally this would return a stream that can be used fro writing, but the result is not correct
		/// </summary>
		/// <param name="baseStream">The underlying base stream usually a file</param>
		/// <param name="recipients">teh recipient(s) of the file, only they can decrypt the content</param>
		/// <param name="encryptedStream">Sets the encrypted stream, needs to be closed so data is progressed</param>
		/// <param name="compressedStream">Sets the compress stream, this needs to be closed as well</param>
		/// <returns></returns>
		public virtual Stream PGPStream(Stream baseStream, string recipients, out Stream encryptedStream, out Stream compressedStream)
		{
			// Encryption
			var encryption = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.TripleDes, true, new SecureRandom());
			foreach (var key in GetEncryptionKey(recipients))
				encryption.AddMethod(key);

			encryptedStream = encryption.Open(baseStream, new byte[32768]);

			// Compression
			var compressor = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip);
			compressedStream = compressor.Open(encryptedStream);

			// Lteral
			var literalizer = new PgpLiteralDataGenerator();
			return literalizer.Open(compressedStream, PgpLiteralDataGenerator.Binary, "PGPStream", DateTime.UtcNow, new byte[32768]);
		}

		public virtual void PgpEncrypt(Stream toEncrypt, Stream outStream, string recipients, IProcessDisplay processDisplay)
		{
			var encryption = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.TripleDes, false, new SecureRandom());
			foreach (var key in GetEncryptionKey(recipients))
				encryption.AddMethod(key);

			using (var encryptedStream = encryption.Open(outStream, new byte[16384]))
			{
				var compressor = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip);
				using (var compressedStream = compressor.Open(encryptedStream))
				{
					var literalizer = new PgpLiteralDataGenerator();
					using (var literalStream = literalizer.Open(compressedStream, PgpLiteralDataGenerator.Utf8, "PGPStream",
						DateTime.UtcNow, new byte[16384]))
					{
						// we are done if the whole stream is processed
						if (processDisplay != null)
							processDisplay.Maximum = toEncrypt.Length;

						long processed = 0;
						int lengthRead;
						var displayMax = StringConversion.DynamicStorageSize(toEncrypt.Length);

						var intervalAction = processDisplay != null ? new IntervalAction(0.5) : null;
						var copyBuffer = new byte[32768];
						while ((lengthRead = toEncrypt.Read(copyBuffer, 0, copyBuffer.Length)) > 0)
						{
							processDisplay?.CancellationToken.ThrowIfCancellationRequested();
							literalStream.Write(copyBuffer, 0, lengthRead);
							processed += lengthRead;

							intervalAction?.Invoke(numRec =>
								{
									if (processDisplay is IProcessDisplayTime processDisplayTime)
									{
										processDisplayTime.TimeToCompletion.Value = numRec;
										processDisplay.SetProcess(
											$"PGP Encrypting  {processDisplayTime.TimeToCompletion.PercentDisplay}{processDisplayTime.TimeToCompletion.EstimatedTimeRemainingDisplaySeparator} - {StringConversion.DynamicStorageSize(numRec)}/{displayMax}",
											numRec, true);
									}
									else
									{
										processDisplay.SetProcess(
											$"PGP Encrypting - {StringConversion.DynamicStorageSize(numRec)}/{displayMax}", numRec, true);
									}
								}
							, processed);
						}
					}
				}
			}
		}

		public virtual void RemovePrivateKey(int index)
		{
			m_EncryptedPrivateKeyRingBundle.RemoveAt(index);
			NotifyPropertyChanged(nameof(PrivateKeys));
			m_Recipients = null;
		}

		public virtual void RemovePublicKey(int index)
		{
			m_EncryptedPublicKeyRingBundle.RemoveAt(index);
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

		private Stream GetDecryptedDataStream(PgpEncryptedDataList enc, System.Security.SecureString passphrase)
		{
			if (PrivateKeys == null)
				throw new PgpException("Secret keys not setup.");
			foreach (var encryptedKey in PrivateKeys)
				using (var keyIn = new MemoryStream(Encoding.UTF8.GetBytes(encryptedKey.Decrypt(m_PgpDecryption))))
				{
					var pgpSec = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(keyIn));
					foreach (PgpPublicKeyEncryptedData encryptedData in enc.GetEncryptedDataObjects())
					{
						var pgpSecKey = pgpSec.GetSecretKey(encryptedData.KeyId);
						if (pgpSecKey == null)
							continue;
						try
						{
							var key = pgpSecKey.ExtractPrivateKey(ToCharArray(passphrase));
							if (key != null)
								return encryptedData.GetDataStream(key);
						}
						catch (PgpException)
						{
						}
					}
				}
			throw new EncryptionException("No mathing private key found for encrypted data.");			
		}

		private IEnumerable<PgpPublicKey> GetEncryptionKey(string recipients)
		{
			var listOfKeys = new List<PgpPublicKey>();

			if (!string.IsNullOrEmpty(recipients))
			{
				var knownRecipients = GetRecipients();
				foreach (var recipient in recipients.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
				{
					PgpPublicKey found = null;
					foreach (var kvp in knownRecipients)
						if (kvp.Key.Equals(recipient, StringComparison.OrdinalIgnoreCase))
						{
							found = kvp.Value;
							break;
						}

					if (found == null)
						foreach (var kvp in knownRecipients)
							if (kvp.Key.ToUpperInvariant().Contains(recipient.ToUpperInvariant()))
							{
								found = kvp.Value;
								break;
							}

					if (found != null)
						listOfKeys.Add(found);
				}
			}

			if (listOfKeys.Count == 0)
				throw new EncryptionException($"No encryption key found for {recipients} in known key(s).");

			return listOfKeys;
		}

		public override bool Equals(object obj) => Equals(obj as PGPKeyStorage);

#pragma warning disable CA1819 // Properties should not return arrays: Needed for Serialization

#pragma warning restore CA1819 // Properties should not return arrays
	}
}