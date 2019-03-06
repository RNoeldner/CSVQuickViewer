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
using File = Pri.LongPath.File;
using FileInfo = Pri.LongPath.FileInfo;

namespace CsvTools
{
  /// <summary>
  /// A wrapper around file streams to handle pre and post processing, needed for sFTP, Encryption and Compression
  /// </summary>
  /// <seealso cref="System.IDisposable" />
  public class ImprovedStream : IDisposable
  {
    private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private bool AssumeGZip;
    private bool AssumePGP;
    private string BasePath;
    private string EncryptedPassphrase;
    private IProcessDisplay ProcessDisplay;
    private string Recipient;
    private string TempFile;
    private string WritePath;

    public double Percentage
    {
      get
      {
        return ((double)BaseStream.Position) / BaseStream.Length;
      }
    }

    public Stream Stream { get; set; }
    private FileStream BaseStream { get; set; }

    /// <summary>
    /// Opens a file for reading
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="decryptedPassphrase">The decrypted passphrase.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Path must be set - path</exception>
    public static ImprovedStream OpenRead(string path, Func<string> encryptedPassphrase = null)
    {
      if (string.IsNullOrEmpty(path))
      {
        throw new ArgumentException("Path must be set", nameof(path));
      }
      var retVal = OpenBaseStream(path, encryptedPassphrase);
      retVal.ResetToStart(null);
      return retVal;
    }

    /// <summary>
    /// Opens the a setting for reading
    /// </summary>
    /// <param name="setting">The setting.</param>
    /// <returns></returns>
    /// <exception cref="ApplicationException">
    /// Please provide a passphrase.
    /// or
    /// Please reenter the passphrase, the passphrase could not be decrypted.
    /// </exception>
    public static ImprovedStream OpenRead(IFileSetting setting)
    {
      var retVal = OpenBaseStream(setting.FullPath, setting.GetEncryptedPassphraseFunction);

      retVal.ResetToStart(delegate (Stream sr)
      {
        if (retVal.AssumePGP)
        {
          // Store the passphrase for next use, this does not mean it is correct
          setting.Passphrase = retVal.EncryptedPassphrase;

          if (ApplicationSetting.ToolSetting?.PGPInformation != null && ApplicationSetting.ToolSetting.PGPInformation.EncryptedPassphase.Length == 0)
            ApplicationSetting.ToolSetting.PGPInformation.EncryptedPassphase = retVal.EncryptedPassphrase;
        }
      });

      return retVal;
    }

    /// <summary>
    /// Opens an file for writing
    /// </summary>
    /// <param name="path">The path.</param>
    /// <remarks>
    /// There could be one steps a) in case its to be uploaded a temp file will be created
    /// </remarks>
    public static ImprovedStream OpenWrite(string path, IProcessDisplay processDisplay = null, string recipient = null)
    {
      var retVal = new ImprovedStream()
      {
        WritePath = path.RemovePrefix(),
        ProcessDisplay = processDisplay,
        Recipient = recipient
      };
      if (path.AssumePgp() || path.AssumeGZip())
      {
        Log.Debug("Creating temporary file");
        retVal.TempFile = Path.GetTempFileName();

        // download the file to a temp file
        retVal.BaseStream = File.Create(retVal.TempFile);
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
    /// Closes the stream in case of a file opened for writing it would be uploaded to the sFTP
    /// </summary>
    public void Close()
    {
      if (Stream != null)
        Stream.Close();
      if (BaseStream != null)
        BaseStream.Close();
      try
      {
        if (!string.IsNullOrEmpty(WritePath))
          CloseWrite();
      }
      finally
      {
        FileSystemUtils.FileDelete(TempFile);
      }
    }

    public void ResetToStart(Action<Stream> AfterInit)
    {
      try
      {
        // in case the stream is at the beginning do nothing
        if (Stream != null && Stream.CanSeek)
          Stream.Position = 0;
        else
        {
          if (Stream != null)
          {
            Stream.Close();
            // need to reopen the base stream
            BaseStream = File.OpenRead(BasePath);
          }

          if (AssumeGZip)
          {
            Log.Debug("Decompressing GZip Stream");
            Stream = new System.IO.Compression.GZipStream(BaseStream, System.IO.Compression.CompressionMode.Decompress);
          }

          else if (AssumePGP)
          {
            System.Security.SecureString DecryptedPassphrase = null;
            // need to use the setting function, opening a form to enter the passphrase is not in this library
            if (string.IsNullOrEmpty(EncryptedPassphrase))
              throw new ApplicationException("Please provide a passphrase.");
            try
            {
              DecryptedPassphrase = EncryptedPassphrase.Decrypt().ToSecureString();
            }
            catch (Exception)
            {
              throw new ApplicationException("Please reenter the passphrase, the passphrase could not be decrypted.");
            }

            try
            {
              Log.Debug("Decrypt PGP Stream");
              Stream = ApplicationSetting.ToolSetting.PGPInformation.PgpDecrypt(BaseStream, DecryptedPassphrase);
            }
            catch (Org.BouncyCastle.Bcpg.OpenPgp.PgpException ex)
            {
              // removed possibly stored passphrase
              var recipinet = string.Empty;
              try
              {
                recipinet = ApplicationSetting.ToolSetting?.PGPInformation?.GetEncryptedKeyID(BaseStream);
              }
              catch
              {
                // ignore
              }

              if (recipinet.Length > 0)
                throw new ApplicationException($"The message is encrypted for '{recipinet}'.", ex);
              else
                throw;
            }
          }
          else
            Stream = BaseStream;
        }
      }
      finally
      {
        AfterInit?.Invoke(Stream);
      }
    }

    /// <summary>
    /// Opens the base stream, handling sFTP access
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="encryptedPassphraseFunc">The encrypted passphrase function.</param>
    /// <returns>
    /// An improved stream where the base stream is set
    /// </returns>
    private static ImprovedStream OpenBaseStream(string path, Func<string> encryptedPassphraseFunc)
    {
      var retVal = new ImprovedStream
      {
        AssumePGP = path.AssumePgp(),
        AssumeGZip = path.AssumeGZip(),
      };
      if (retVal.AssumePGP && encryptedPassphraseFunc != null)
      {
        retVal.EncryptedPassphrase = encryptedPassphraseFunc();
      }
      try
      {
        retVal.BasePath = path;
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

      if (ProcessDisplay == null)
        ProcessDisplay = new DummyProcessDisplay();

      if (WritePath.AssumeGZip() || WritePath.AssumePgp())
        try
        {
          // Compress the file
          if (WritePath.AssumeGZip())
          {
            Log.Debug("Compressing temporary file to GZip file");
            using (var inFile = File.OpenRead(TempFile))
            {
              // Create the compressed file.
              using (var outFile = File.Create(WritePath))
              {
                using (var compress = new System.IO.Compression.GZipStream(outFile, System.IO.Compression.CompressionMode.Compress))
                {
                  var processDispayTime = ProcessDisplay as IProcessDisplayTime;
                  var inputBuffer = new byte[16384];
                  var max = (int)(inFile.Length / inputBuffer.Length);
                  ProcessDisplay.Maximum = max;
                  var count = 0;
                  int length;
                  while ((length = inFile.Read(inputBuffer, 0, inputBuffer.Length)) > 0)
                  {
                    compress.Write(inputBuffer, 0, length);
                    ProcessDisplay.CancellationToken.ThrowIfCancellationRequested();

                    if (processDispayTime != null)
                      ProcessDisplay.SetProcess(
                       $"GZip {processDispayTime.TimeToCompletion.PercentDisplay}{processDispayTime.TimeToCompletion.EstimatedTimeRemainingDisplaySeperator}", count);
                    else
                      ProcessDisplay.SetProcess($"GZip {count:N0}/{max:N0}",
                       count);
                    count++;
                  }
                }
              }
            }
          }
          // need to encrypt the file
          else if (WritePath.AssumePgp())
          {
            using (FileStream inputStream = new FileInfo(TempFile).OpenRead(),
                        output = new FileStream(WritePath.LongPathPrefix(), FileMode.Create))
            {
              Log.Debug("Encrypting temporary file to PGP file");
              ApplicationSetting.ToolSetting.PGPInformation.PgpEncrypt(inputStream, output, Recipient, ProcessDisplay);
            }
          }
        }
        finally
        {
          Log.Debug("Removing temporary file");
          File.Delete(TempFile);
        }
    }

    #region IDisposable Support

    private bool disposedValue = false; // To detect redundant calls

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
      // TODO: uncomment the following line if the finalizer is overridden above.
      // GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          Close();
          if (Stream != null)
            Stream.Dispose();
          if (BaseStream != null)
            BaseStream.Dispose();
          if (ProcessDisplay != null)
            ProcessDisplay.Dispose();

        }

        disposedValue = true;
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