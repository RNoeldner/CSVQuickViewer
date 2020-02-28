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
  using File = Pri.LongPath.File;

  /// <summary>
  /// A wrapper around file streams to handle pre and post processing, needed for sFTP, Encryption
  /// and Compression
  /// </summary>
  public sealed class ImprovedStream : IImprovedStream
  {
    public ImprovedStream(string path)
    {
      m_BasePath = path;
      m_AssumeGZip = path.AssumeGZip();
    }

    private readonly bool m_AssumeGZip;

    private readonly string m_BasePath;

    private bool m_DisposedValue; // To detect redundant calls

    public double Percentage => (double)BaseStream.Position / BaseStream.Length;

    public Stream Stream { get; private set; }

    private FileStream BaseStream { get; set; }

    /// <summary>
    /// Opens a file for reading
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="encryptedPassphrase">The encrypted passphrase.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Path must be set - path</exception>
    public static IImprovedStream OpenRead(string path)
    {
      if (string.IsNullOrEmpty(path))
        throw new ArgumentException("Path must be set", nameof(path));
      var retVal = OpenBaseStream(path);
      retVal.ResetToStart(null);
      return retVal;
    }

    /// <summary>
    /// Opens an file for writing
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="recipient">The recipient.</param>
    /// <returns>An improved stream object</returns>
    public static IImprovedStream OpenWrite(string path)
    {
      FileSystemUtils.FileDelete(path.LongPathPrefix());

      var retVal = new ImprovedStream(path);

      if (retVal.m_AssumeGZip)
      {
        retVal.BaseStream = File.Create(path.LongPathPrefix());
        retVal.Stream = new GZipStream(retVal.BaseStream, CompressionMode.Compress);
        return retVal;
      }

      FileSystemUtils.FileDelete(path.LongPathPrefix());
      retVal.BaseStream = File.Create(path.LongPathPrefix());
      retVal.Stream = retVal.BaseStream;
      return retVal;
    }

    /// <summary>
    /// Closes the stream in case of a file opened for writing it would be uploaded to the sFTP
    /// </summary>
    public void Close()
    {
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
        Stream?.Dispose();
        BaseStream?.Dispose();
      }

      m_DisposedValue = true;
    }

    /// <summary>
    /// Opens the base stream, handling sFTP access
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="encryptedPassphraseFunc">The encrypted passphrase function.</param>
    /// <returns>An improved stream where the base stream is set</returns>
    private static ImprovedStream OpenBaseStream(string path)
    {
      var retVal = new ImprovedStream(path);
      try
      {
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