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

namespace CsvTools
{
  /// <summary>
  /// This class wraps Pri.LongPath.FileInfo and WinSCP.RemoteFileInfo so it does not matter
  /// if the file is on the sFTP or the fileSystem
  /// </summary>
  public class GenericFileInfo
  {
    public bool Exists { get; set; }
    public virtual string FullName { get; set; }

    public DateTime LastWriteTime
    {
      get
      {
        return LastWriteTimeUtc.ToLocalTime();
      }
      set
      {
        LastWriteTimeUtc = value.ToUniversalTime();
      }
    }

    public DateTime LastWriteTimeUtc { get; set; }
    public long Length { get; set; }
    public string Name { get; set; }

    public string Extension
    {
      get
      {
        var pos = FullName.LastIndexOf('.');
        if (pos == -1)
          return string.Empty;

        return FullName.Substring(pos);
      }
    }

    public static GenericFileInfo GetFileInfo(Pri.LongPath.FileInfo other)
    {
      return new GenericFileInfo()
      {
        Length = other.Exists ? other.Length : -1,
        LastWriteTimeUtc = other.LastWriteTimeUtc,
        Name = other.Name,
        FullName = other.FullName,
        Exists = other.Exists
      };
    }

    public static GenericFileInfo GetFileInfo(WinSCP.RemoteFileInfo other)
    {
      return new GenericFileInfo()
      {
        Length = other.Length,
        LastWriteTime = other.LastWriteTime,
        Name = other.Name,
        FullName = other.FullName.FTPPrefix(),
        Exists = true
      };
    }
  }
}