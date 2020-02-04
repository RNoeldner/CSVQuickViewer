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

namespace CsvTools
{
  using System;
  using System.IO;
  using System.IO.Compression;
  using Org.BouncyCastle.Bcpg.OpenPgp;

  using File = Pri.LongPath.File;

  /// <summary>
  ///   A wrapper around file streams to handle pre and post processing, needed for sFTP, Encryption and Compression
  /// </summary>
  /// <seealso cref="System.IDisposable" />
  public sealed class ImprovedStream : IDisposable
  {
    /// <summary>
    ///   A PGP stream, has a few underlying streams that need to be closed in teh right order
    /// </summary>
    private Stream m_CompressStream = null;

    /// <summary>
    ///   A PGP stream, has a few underlying streams that need to be closed in teh right order
    /// </summary>
    private Stream m_EncryptedStream = null;

    private bool m_AssumeGZip;

    private bool m_AssumePGP;

    private string m_BasePath;

    private bool m_DisposedValue; // To detect redundant calls

    private string m_EncryptedPassphrase;

    public double Percentage => (double)BaseStream.Position / BaseStream.Length;

    public Stream Stream { get; set; }

    private FileStream BaseStream { get; set; }

    /// <summary>
    ///   Opens a file for reading
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="encryptedPassphrase">The encrypted passphrase.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Path must be set - path</exception>
    public static ImprovedStream OpenRead(string path, Func<string> encryptedPassphrase = null)
    {
      if (string.IsNullOrEmpty(path))
        throw new ArgumentException("Path must be set", nameof(path));
      var retVal = OpenBaseStream(path, encryptedPassphrase);
      retVal.ResetToStart(null);
      return retVal;
    }

    /// <summary>
    ///   Opens the a setting for reading
    /// </summary>
    /// <param name="setting">The setting.</param>
    /// <returns></returns>
    /// <exception cref="EncryptionException">
    ///   Please provide a passphrase.
    ///   or
    ///   Please reenter the passphrase, the passphrase could not be decrypted.
    /// </exception>
    public static ImprovedStream OpenRead(IFileSettingPhysicalFile setting)
    {
      var retVal = OpenBaseStream(setting.FullPath, setting.GetEncryptedPassphraseFunction);

      retVal.ResetToStart(
        delegate
          {
            if (!retVal.m_AssumePGP)
              return;

            // Store the passphrase for next use, this does not mean it is correct
            setting.Passphrase = retVal.m_EncryptedPassphrase;

            if (ApplicationSetting.PGPKeyStorage != null
                && ApplicationSetting.PGPKeyStorage.EncryptedPassphase.Length == 0)
              ApplicationSetting.PGPKeyStorage.EncryptedPassphase = retVal.m_EncryptedPassphrase;
          });

      return retVal;
    }

    /// <summary>
    ///   Opens an file for writing
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="recipient">The recipient.</param>
    /// <returns>An improved stream object  </returns>
    public static ImprovedStream OpenWrite(string path, string recipient = null)
    {
      FileSystemUtils.FileDelete(path.LongPathPrefix());

      var retVal = new ImprovedStream { m_AssumePGP = path.AssumePgp(), m_AssumeGZip = path.AssumeGZip(), };

      if (retVal.m_AssumeGZip)
      {
        retVal.BaseStream = File.Create(path.LongPathPrefix());
        retVal.Stream = new GZipStream(retVal.BaseStream, CompressionMode.Compress);
        return retVal;
      }

      if (retVal.m_AssumePGP)
      {
        retVal.BaseStream = File.Create(path.LongPathPrefix());
        retVal.Stream = ApplicationSetting.PGPKeyStorage.PGPStream(
          retVal.BaseStream,
          recipient,
          out retVal.m_EncryptedStream,
          out retVal.m_CompressStream);
        return retVal;
      }

      FileSystemUtils.FileDelete(path.LongPathPrefix());
      retVal.BaseStream = File.Create(path.LongPathPrefix());
      retVal.Stream = retVal.BaseStream;
      return retVal;
    }

    /// <summary>
    ///   Closes the stream in case of a file opened for writing it would be uploaded to the sFTP
    /// </summary>
    public void Close()
    {
      if (m_AssumePGP)
      {
        m_CompressStream?.Close();
        m_EncryptedStream?.Close();
      }

      Stream?.Close();
      BaseStream?.Close();
    }

    public void Dispose() => Dispose(true);

    public void ResetToStart(Action<Stream> afterInit)
    {
      try
      {
        // in case the stream is at the beginning do nothing
        if (Stream != null && Stream.CanSeek)
        {
          Stream.Position = 0;
        }
        else
        {
          if (Stream != null)
          {
            Stream.Close();

            // need to reopen the base stream
            BaseStream = File.OpenRead(m_BasePath);
          }

          if (m_AssumeGZip)
          {
            Logger.Debug("Decompressing GZip Stream {filename}", m_BasePath);
            Stream = new GZipStream(BaseStream, CompressionMode.Decompress);
          }
          else if (m_AssumePGP)
          {
            System.Security.SecureString decryptedPassphrase;

            // need to use the setting function, opening a form to enter the passphrase is not in this library
            if (string.IsNullOrEmpty(m_EncryptedPassphrase))
              throw new EncryptionException("Please provide a passphrase.");
            try
            {
              decryptedPassphrase = m_EncryptedPassphrase.Decrypt().ToSecureString();
            }
            catch (Exception)
            {
              throw new EncryptionException("Please reenter the passphrase, the passphrase could not be decrypted.");
            }

            try
            {
              Logger.Debug("Decrypt PGP Stream {filename}", m_BasePath);
              Stream = ApplicationSetting.PGPKeyStorage.PgpDecrypt(BaseStream, decryptedPassphrase);
            }
            catch (PgpException ex)
            {
              // removed possibly stored passphrase
              var recipient = ApplicationSetting.PGPKeyStorage?.GetEncryptedKeyID(BaseStream);
              throw new EncryptionException($"The message is encrypted for '{recipient}'.", ex);
            }
          }
          else
          {
            Stream = BaseStream;
          }
        }
      }
      finally
      {
        afterInit?.Invoke(Stream);
      }
    }

    private void Dispose(bool disposing)
    {
      if (m_DisposedValue) return;
      if (disposing)
      {
        Close();
        if (m_AssumePGP)
        {
          m_CompressStream?.Dispose();
          m_CompressStream = null;

          m_EncryptedStream?.Dispose();
          m_EncryptedStream = null;
        }

        Stream?.Dispose();
        BaseStream?.Dispose();
      }

      m_DisposedValue = true;
    }

    /// <summary>
    ///   Opens the base stream, handling sFTP access
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="encryptedPassphraseFunc">The encrypted passphrase function.</param>
    /// <returns>
    ///   An improved stream where the base stream is set
    /// </returns>
    private static ImprovedStream OpenBaseStream(string path, Func<string> encryptedPassphraseFunc)
    {
      var retVal = new ImprovedStream { m_AssumePGP = path.AssumePgp(), m_AssumeGZip = path.AssumeGZip() };
      if (retVal.m_AssumePGP && encryptedPassphraseFunc != null)
        retVal.m_EncryptedPassphrase = encryptedPassphraseFunc();
      try
      {
        retVal.m_BasePath = path;
        retVal.BaseStream = File.OpenRead(path);
        return retVal;
      }
      catch (Exception)
      {
        retVal.Close();
        throw;
      }
    }
  }
}