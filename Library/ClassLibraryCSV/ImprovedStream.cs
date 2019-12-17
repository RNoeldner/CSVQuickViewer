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
using System.IO;
using System.IO.Compression;
using Org.BouncyCastle.Bcpg.OpenPgp;
using File = Pri.LongPath.File;
using FileInfo = Pri.LongPath.FileInfo;

namespace CsvTools
{
  /// <summary>
  ///   A wrapper around file streams to handle pre and post processing, needed for sFTP, Encryption and Compression
  /// </summary>
  /// <seealso cref="System.IDisposable" />
  public class ImprovedStream : IDisposable
  {
    private bool m_AssumeGZip;
    private bool m_AssumePGP;
    private string m_BasePath;
    private string m_EncryptedPassphrase;
    private IProcessDisplay m_ProcessDisplay;
    private string m_Recipient;
    private string m_TempFile;
    private string m_WritePath;

    public double Percentage => (double) BaseStream.Position / BaseStream.Length;

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
      if (string.IsNullOrEmpty(path)) throw new ArgumentException("Path must be set", nameof(path));
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

      retVal.ResetToStart(delegate
      {
        if (!retVal.m_AssumePGP) return;
        // Store the passphrase for next use, this does not mean it is correct
        setting.Passphrase = retVal.m_EncryptedPassphrase;

        if (ApplicationSetting.PGPKeyStorage != null && ApplicationSetting.PGPKeyStorage.EncryptedPassphase.Length == 0)
          ApplicationSetting.PGPKeyStorage.EncryptedPassphase = retVal.m_EncryptedPassphrase;
      });

      return retVal;
    }

    /// <summary>
    ///   Opens an file for writing
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="processDisplay">The process display.</param>
    /// <param name="recipient">The recipient.</param>
    /// <returns></returns>
    /// <remarks>
    ///   There could be one steps a) in case its to be uploaded a temp file will be created
    /// </remarks>
    public static ImprovedStream OpenWrite(string path, IProcessDisplay processDisplay = null, string recipient = null)
    {
      var retVal = new ImprovedStream
      {
        m_WritePath = path.RemovePrefix(),
        m_ProcessDisplay = processDisplay,
        m_Recipient = recipient
      };
      if (path.AssumePgp() || path.AssumeGZip())
      {
        var filename = Path.GetTempFileName();
        Logger.Debug("Creating temporary file {filename}", filename);
        retVal.m_TempFile = filename;

        // download the file to a temp file
        retVal.BaseStream = File.Create(retVal.m_TempFile);
      }
      else
      {
        FileSystemUtils.FileDelete(path.LongPathPrefix());
        retVal.BaseStream = File.Create(path.LongPathPrefix());
      }

      retVal.Stream = retVal.BaseStream;
      return retVal;
    }

    /// <summary>
    ///   Closes the stream in case of a file opened for writing it would be uploaded to the sFTP
    /// </summary>
    public void Close()
    {
      Stream?.Close();
      BaseStream?.Close();
      try
      {
        if (!string.IsNullOrEmpty(m_WritePath))
          CloseWrite();
      }
      finally
      {
        if (FileSystemUtils.FileExists(m_TempFile))
        {
          Logger.Debug("Removing temporary file {filename}", m_TempFile);
          File.Delete(m_TempFile);
        }
      }
    }

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
      var retVal = new ImprovedStream
      {
        m_AssumePGP = path.AssumePgp(),
        m_AssumeGZip = path.AssumeGZip()
      };
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

    private void CloseWrite()
    {
      // If we are writing we have possibly two steps,
      // a) compress / encrypt the data from temp
      // b) upload the data to sFTP
      if (m_WritePath.AssumeGZip() || m_WritePath.AssumePgp())
      {
        var selfOpened = false;
        if (m_ProcessDisplay == null)
        {
          selfOpened = true;
          m_ProcessDisplay = new DummyProcessDisplay();
        }

        try
        {
          // Compress the file
          if (m_WritePath.AssumeGZip())
          {
            Logger.Debug("Compressing temporary file to GZip file");
            var action = new IntervalAction(0.5);
            using (var inFile = File.OpenRead(m_TempFile))
            {
              // Create the compressed file.
              using (var outFile = File.Create(m_WritePath))
              {
                using (var compress = new GZipStream(outFile, CompressionMode.Compress))
                {
                  var inputBuffer = new byte[16384];

                  var displayMax = StringConversion.DynamicStorageSize(inFile.Length);
                  m_ProcessDisplay.Maximum = inFile.Length;
                  int length;
                  long processed = 0;
                  while ((length = inFile.Read(inputBuffer, 0, inputBuffer.Length)) > 0)
                  {
                    compress.Write(inputBuffer, 0, length);
                    processed += length;
                    m_ProcessDisplay.CancellationToken.ThrowIfCancellationRequested();

                    action.Invoke(() =>
                    {
                      if (m_ProcessDisplay is IProcessDisplayTime processDisplayTime)
                      {
                        processDisplayTime.TimeToCompletion.Value = processed;
                        m_ProcessDisplay.SetProcess(
                          $"GZip  {processDisplayTime.TimeToCompletion.PercentDisplay}{processDisplayTime.TimeToCompletion.EstimatedTimeRemainingDisplaySeperator} - {StringConversion.DynamicStorageSize(processed)}/{displayMax}",
                          processed);
                      }
                      else
                      {
                        m_ProcessDisplay.SetProcess(
                          $"GZip - {StringConversion.DynamicStorageSize(processed)}/{displayMax}", processed);
                      }
                    });
                  }
                }
              }
            }
          }
          // need to encrypt the file
          else if (m_WritePath.AssumePgp())
          {
            using (FileStream inputStream = new FileInfo(m_TempFile).OpenRead(),
              output = new FileStream(m_WritePath.LongPathPrefix(), FileMode.Create))
            {
              Logger.Debug("Encrypting temporary file {inputfilename} to PGP file {outputfilename}", m_TempFile,
                m_WritePath);
              ApplicationSetting.PGPKeyStorage.PgpEncrypt(inputStream, output, m_Recipient, m_ProcessDisplay);
            }
          }
        }
        finally
        {
          Logger.Debug("Removing temporary file {filename}", m_TempFile);
          File.Delete(m_TempFile);
          if (selfOpened)
            m_ProcessDisplay.Dispose();
        }
      }
    }

    #region IDisposable Support

    private bool m_DisposedValue; // To detect redundant calls

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true); // TODO: uncomment the following line if the finalizer is overridden above.// GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!m_DisposedValue)
      {
        if (disposing)
        {
          Close();
          Stream?.Dispose();
          BaseStream?.Dispose();
          m_ProcessDisplay?.Dispose();
        }

        m_DisposedValue = true;
      }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~ValidatorFileStream() {
    //  // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //  Dispose(false);
    // }

    #endregion IDisposable Support
  }
}