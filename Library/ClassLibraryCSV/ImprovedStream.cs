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


namespace CsvTools
{
  /// <summary>
  ///   A wrapper around file streams to handle pre and post processing, needed for sFTP, Encryption
  ///   and Compression
  /// </summary>
  public sealed class ImprovedStream : IImprovedStream
  {
    private readonly bool m_AssumeGZip;
    private readonly string m_BasePath;
    private readonly bool m_IsReading;

    private bool m_DisposedValue; // To detect redundant calls

    private ImprovedStream(string path, bool isReading)
    {
      m_IsReading = isReading;
      m_BasePath = path;
      m_AssumeGZip = path.AssumeGZip();
    }

    private FileStream BaseStream { get; set; }

    public double Percentage => (double)BaseStream.Position / BaseStream.Length;

    public Stream Stream { get; private set; }

    /// <summary>
    ///   Closes the stream in case of a file opened for writing it would be uploaded to the sFTP
    /// </summary>
    public void Close()
    {
      Stream?.Close();
      BaseStream?.Close();
    }

    public void Dispose() => Dispose(true);

    public void ResetToStart(Action<Stream> afterInit)
    {
      if (!m_IsReading)
        throw new FileException("The stream need to be opened for reading");
      try
      {
        // in case the stream is at the beginning do nothing
        if (Stream != null && Stream.CanSeek)
        {
          if (Stream.Position != 0)
            Stream.Position = 0;
        }
        else
        {
          if (Stream != null)
          {
            Stream.Close();

            // need to reopen the base stream
            BaseStream = FileSystemUtils.OpenRead(m_BasePath);
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

    /// <summary>
    ///   Opens a file for reading
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Path must be set - path</exception>
    public static IImprovedStream OpenRead(IFileSettingPhysicalFile setting)
    {
      if (setting is null)
        throw new ArgumentNullException(nameof(setting));
      return OpenRead(setting.FullPath);
    }

    /// <summary>
    ///   Opens a file for reading
    /// </summary>
    /// <param name="fileName">The path.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Path must be set - path</exception>
    public static IImprovedStream OpenRead(string fileName)
    {
      if (string.IsNullOrEmpty(fileName))
        throw new ArgumentException("Path must be provided", nameof(fileName));
      var retVal = OpenBaseStream(fileName);
      retVal.ResetToStart(null);
      return retVal;
    }

    /// <summary>
    ///   Opens an file for writing
    /// </summary>
    /// <returns>An improved stream object</returns>
    public static IImprovedStream OpenWrite(IFileSettingPhysicalFile setting)
    {
      if (setting is null)
        throw new ArgumentNullException(nameof(setting));
      return OpenWrite(setting.FullPath);
    }

    /// <summary>
    ///   Opens an file for writing
    /// </summary>
    /// <param name="fileName">The path.</param>
    /// <returns>An improved stream object</returns>
    public static IImprovedStream OpenWrite(string fileName)
    {
      if (string.IsNullOrEmpty(fileName))
        throw new ArgumentException("Path must be provided", nameof(fileName));
      FileSystemUtils.FileDelete(fileName);

      var retVal = new ImprovedStream(fileName, false);
      if (retVal.m_AssumeGZip)
      {
        retVal.BaseStream = FileSystemUtils.OpenWrite(retVal.m_BasePath);
        retVal.Stream = new GZipStream(retVal.BaseStream, CompressionMode.Compress);
        return retVal;
      }

      FileSystemUtils.FileDelete(retVal.m_BasePath);
      retVal.BaseStream = File.Create(retVal.m_BasePath);
      retVal.Stream = retVal.BaseStream;
      return retVal;
    }

    private void Dispose(bool disposing)
    {
      if (m_DisposedValue) return;
      if (!disposing) return;
      m_DisposedValue = true;
      Close();
      Stream?.Dispose();
      BaseStream?.Dispose();
    }

    /// <summary>
    ///   Opens the base stream, handling sFTP access
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>An improved stream where the base stream is set</returns>
    private static ImprovedStream OpenBaseStream(string path)
    {
      var retVal = new ImprovedStream(path, true);
      try
      {
        retVal.BaseStream = FileSystemUtils.OpenRead(retVal.m_BasePath);
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