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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Directory = Pri.LongPath.Directory;
using File = Pri.LongPath.File;
using FileInfo = Pri.LongPath.FileInfo;
using Path = Pri.LongPath.Path;

namespace CsvTools
{
#pragma warning disable CA1060 // Move pinvokes to native methods class
  /// <summary>
  ///   Extensions for string in the file system
  /// </summary>
  public static class FileSystemUtils
#pragma warning restore CA1060 // Move pinvokes to native methods class
  {
    private const string c_LongPathPrefix = @"\\?\";
    private const string c_UncLongPathPrefix = @"\\?\UNC\";

    public static void CreateDirectory(string directoryName)
    {
      if (string.IsNullOrEmpty(directoryName)) return;

      Directory.CreateDirectory(directoryName);
    }

    public static StreamReader GetStreamReaderForFileOrResource(string file)
    {
      var fileName = (ExecutableDirectoryName() + "\\" + file).LongPathPrefix();
      try
      {
        if (File.Exists(fileName))
          return new StreamReader(fileName, true);

        var executingAssembly = Assembly.GetExecutingAssembly();
        // try the embedded resource          
        var stream = executingAssembly.GetManifestResourceStream("CsvTools." + file);
        if (stream != null)
          return new StreamReader(stream, true);
      }
      catch (Exception ex)
      {
        Debug.WriteLine("Error reading  " + fileName);
        Debug.WriteLine(ex.Message);
      }
      return null;
    }

    /// <summary>
    ///   Makes the backup.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="twoBackups">if set to <c>true</c> [two backups].</param>
    public static void DeleteWithBackup(string fileName, bool twoBackups)
    {
      Contract.Requires(fileName != null);
      try
      {
        if (!FileExists(fileName)) return;
        var backupName = fileName + ".bak";
        var backupName2 = fileName + "2.bak";
        if (twoBackups)
          FileDelete(backupName2);

        if (FileExists(backupName))
        {
          if (twoBackups)
            File.Move(backupName, backupName2);
          FileDelete(backupName);
        }

        File.Move(fileName, backupName);
      }
      catch
      {
        // ignored
      }
    }

    public static bool DirectoryExists(string directoryName)
    {
      if (string.IsNullOrEmpty(directoryName))
        return false;

      return Directory.Exists(directoryName);
    }

    /// <summary>
    ///   Folder of the executing assembly, mainly used in Unit testing
    /// </summary>
    /// <returns></returns>
    public static string ExecutableDirectoryName()
    {
      var diretory = Assembly.GetExecutingAssembly().Location;
      if (string.IsNullOrEmpty(diretory))
        diretory = Assembly.GetEntryAssembly().Location;
      if (string.IsNullOrEmpty(diretory))
        diretory = ".";
      return Path.GetDirectoryName(diretory);
    }

    public static void FileDelete(string fileName)
    {
      if (FileExists(fileName))
      {
        File.Delete(fileName);
      }
    }

    public static bool FileExists(string fileName)
    {
      if (string.IsNullOrEmpty(fileName))
        return false;
      return File.Exists(fileName);
    }

    public static Pri.LongPath.FileInfo FileInfo(string fileOrDirectory)
    {
      return new FileInfo(fileOrDirectory);
    }

    /// <summary>
    ///   Gets the absolute path.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="basePath">The base path.</param>
    /// <returns>The combined filename with the LongPathPrefix if necessary</returns>
    public static string GetAbsolutePath(this string fileName, string basePath)
    {
      Contract.Ensures(Contract.Result<string>() != null);
      if (string.IsNullOrEmpty(fileName))
        return string.Empty;

      if (!Path.IsPathRooted(fileName))
      {
        if (string.IsNullOrEmpty(basePath))
          return Path.GetFullPath(fileName);
        else
        {
          return Path.GetFullPath(Path.Combine(basePath, fileName));
        }
      }

      return fileName;
    }

    /// <summary>
    ///   Gets the name of the directory, unlike Path.GetDirectoryName is return the input in case the input was a directory
    ///   already
    /// </summary>
    /// <param name="fileOrDirectory">Name of the file or directory.</param>
    /// <returns>The folder / directory of the given file or directory</returns>
    public static string GetDirectoryName(this string fileOrDirectory)
    {
      if (string.IsNullOrEmpty(fileOrDirectory))
        return null;

      if (fileOrDirectory[0] == '.')
        fileOrDirectory = Path.GetFullPath(fileOrDirectory);

      if (DirectoryExists(fileOrDirectory))
        return fileOrDirectory;

      // get the directory from under it
      var lastIndex = fileOrDirectory.LastIndexOf('\\');
      if (lastIndex > 0)
        return fileOrDirectory.Substring(0, lastIndex).RemovePrefix();

      return null;
    }

    public static string[] GetFiles(string folder, string searchPattern)
    {
      Contract.Requires(!string.IsNullOrEmpty(folder));
      Contract.Requires(!string.IsNullOrEmpty(searchPattern));

      if (folder.IndexOfAny(new[] { '*', '?', '[', ']' }) == -1)
      {
        return Directory.GetFiles(folder, searchPattern, SearchOption.TopDirectoryOnly);
      }

      return new string[] { };
    }

    public static string GetLatestFileOfPattern(string folder, string searchPattern)
    {
      Contract.Requires(!string.IsNullOrEmpty(folder));
      Contract.Requires(!string.IsNullOrEmpty(searchPattern));
      if (!Directory.Exists(folder))
        return null;
      var files = GetFiles(folder, searchPattern);

      if (files.Length == 0)
        return null;
      if (files.Length == 1)
        return files[0];

      // If a pattern is present in the folder this is not going to work...
      var newset = new DateTime(0);
      string lastFile = null;
      foreach (var fileName in files)
      {
        var fileTime = FileInfo(fileName).LastWriteTime;
        if (fileTime <= newset) continue;
        newset = fileTime;
        lastFile = fileName;
      }

      return lastFile;
    }

    /// <summary>
    ///   Get a relative path to the file
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="basePath">The base path.</param>
    /// <returns>
    ///   A relative path if possible
    /// </returns>
    public static string GetRelativePath(this string fileName, string basePath)
    {
      if (string.IsNullOrEmpty(fileName) || fileName.StartsWith(".", StringComparison.Ordinal) ||
          fileName.IndexOf("\\", StringComparison.Ordinal) == -1)
        return fileName;

      if (string.IsNullOrEmpty(basePath))
        basePath = Directory.GetCurrentDirectory();

      if (fileName.Equals(basePath, StringComparison.OrdinalIgnoreCase))
        return ".";
      if (fileName.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
        return fileName.Substring(basePath.Length + 1);
      var otherDir = Path.GetFullPath(fileName).RemovePrefix();

      return GetRelativePathQuick(otherDir, basePath);
    }

    public static string GetRelativePathQuick(this string otherDir, string basePath)
    {
      if (otherDir.Equals(basePath, StringComparison.OrdinalIgnoreCase))
        return ".";
      if (otherDir.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
        return otherDir.Substring(basePath.Length + 1);

      var startPathParts = basePath.Split(Path.DirectorySeparatorChar);
      var destinationPathParts = otherDir.Split(Path.DirectorySeparatorChar);
      var sameCounter = 0;
      while (sameCounter < startPathParts.Length && sameCounter < destinationPathParts.Length &&
             startPathParts[sameCounter].Equals(destinationPathParts[sameCounter], StringComparison.OrdinalIgnoreCase))
        sameCounter++;
      if (sameCounter == 0)
        return otherDir;

      var sbuilder = new StringBuilder();
      for (var i = sameCounter; i < startPathParts.Length; i++)
        sbuilder.Append(".." + Path.DirectorySeparatorChar);

      for (var i = sameCounter; i < destinationPathParts.Length; i++)
        sbuilder.Append(destinationPathParts[i] + Path.DirectorySeparatorChar);
      sbuilder.Length--;

      return sbuilder.ToString();
    }

    /// <summary>
    ///   Gets a shortened name to the display file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static string GetShortDisplayFileName(string fileName, int length)
    {
      var ret = fileName.RemovePrefix();
      if (length <= 0 || string.IsNullOrEmpty(fileName) || fileName.Length <= length) return ret;
      var parts = fileName.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
      var fileNameOnly = parts[parts.Length - 1];

      // try to cut out directories
      if (parts.Length > 5)
        ret = $"{parts[0]}\\{parts[1]}\\…\\{parts[parts.Length - 3]}\\{parts[parts.Length - 2]}\\{fileNameOnly}";

      if (ret.Length > length && parts.Length > 3)
        ret = $"{parts[0]}\\…\\{parts[parts.Length - 2]}\\{fileNameOnly}";

      if (ret.Length > length && parts.Length > 3)
        ret = $"…\\{parts[parts.Length - 2]}\\{fileNameOnly}";

      // yet too long? only filename
      if (ret.Length > length)
        ret = fileNameOnly;

      // still too long?
      if (ret.Length <= length) return ret;
      var cut = length * 2 / 3;
      ret = ret.Substring(0, length - cut) + "…" + ret.Substring(ret.Length - cut + 1);

      return ret;
    }

    public static string GetShortestPath(this string fileName, string basePath)
    {
      var absolute = fileName.GetAbsolutePath(basePath);
      var relative = fileName.GetRelativePath(basePath);
      return relative.Length < absolute.Length ? relative : absolute;
    }

    public static string LongFileName(this string shortPath)
    {
      if (string.IsNullOrEmpty(shortPath)) return shortPath;
      if (shortPath.Contains("~"))
        return shortPath.LongFileNameKernel();
      if (shortPath.Contains(".\\"))
        return Path.GetFullPath(shortPath);

      return shortPath;
    }

    public static string LongPathPrefix(this string path)
    {
      // In case the directory is 248 we need long path as well
      if (string.IsNullOrEmpty(path) || path.Length < 248 ||
          path.StartsWith(c_LongPathPrefix, StringComparison.Ordinal) ||
          path.StartsWith(c_UncLongPathPrefix, StringComparison.Ordinal))
        return path;
      return path.StartsWith(@"\\", StringComparison.Ordinal)
        ? c_UncLongPathPrefix + path.Substring(2)
        : c_LongPathPrefix + path;
    }

    public static string RemovePrefix(this string path)
    {
      if (string.IsNullOrEmpty(path))
        return path;
      if (path.StartsWith(c_LongPathPrefix, StringComparison.Ordinal))
        return path.Substring(c_LongPathPrefix.Length);
      if (path.StartsWith(c_UncLongPathPrefix, StringComparison.Ordinal))
        return path.Substring(c_UncLongPathPrefix.Length);
      return path;
    }

    public static string ResolvePattern(string fileName)
    {
      Contract.Ensures(Contract.Result<string>() != null);
      if (string.IsNullOrEmpty(fileName))
        return string.Empty;
      if (fileName.IndexOfAny(new[] { '*', '?', '[', ']' }) == -1)
        return fileName;

      var split = SplitPath(fileName);
      var folder = split.DirectoryName;
      var searchPattern = split.FileName;

      var lastFile = GetLatestFileOfPattern(folder, searchPattern);

      if (!string.IsNullOrEmpty(lastFile))
        return lastFile;

      return fileName;
    }

    /// <summary>
    ///   Gets  filename that is usable in the file system.
    /// </summary>
    /// <param name="original">The original text.</param>
    /// <param name="replaceInvalid">The replace invalid.</param>
    /// <returns>
    ///   A text that is allowed in the file system as filename
    /// </returns>
    public static string SafePath(this string original, string replaceInvalid = "")
    {
      Contract.Requires(replaceInvalid != null);
      Contract.Ensures(Contract.Result<string>() != null);
      if (string.IsNullOrEmpty(original))
        return string.Empty;

      var sb = new StringBuilder(original.Length);
      var posFileName = original.LastIndexOf(Path.DirectorySeparatorChar);

      var invalidFile = new List<char>(Path.GetInvalidFileNameChars());
      var invalidPath = new List<char>(Path.GetInvalidPathChars());
      for (var i = 0; i < posFileName + 1; i++)
      {
        var c = original[i];
        if (!invalidPath.Contains(c))
          sb.Append(c);
        else
          sb.Append(replaceInvalid);
      }

      for (var i = posFileName + 1; i < original.Length; i++)
      {
        var c = original[i];
        if (!invalidFile.Contains(c))
          sb.Append(c);
        else
          sb.Append(replaceInvalid);
      }

      return sb.ToString();
    }

    public static string ShortFileName(this string longPath)
    {
      if (string.IsNullOrEmpty(longPath)) return longPath;

      var fi = new FileInfo(longPath);

      uint bufferSize = 512;
      var shortNameBuffer = new StringBuilder((int)bufferSize);

      // we might be asked to build a short path when the file does not exist yet, this would fail
      if (fi.Exists)
      {
        var length = GetShortPathName(longPath, shortNameBuffer, bufferSize);
        if (length > 0)
        {
          return shortNameBuffer.ToString().RemovePrefix();
        }
      }

      // if we have at least the directory shorten this...
      if (fi.Directory.Exists)
      {
        var length = GetShortPathName(fi.Directory.FullName, shortNameBuffer, bufferSize);
        if (length > 0)
          return (shortNameBuffer.ToString() + "\\" + fi.Name).RemovePrefix();
      }

      throw new ApplicationException($"Could not get a short path for the file ${longPath}");
    }

    public static SplitResult SplitPath(string path)
    {
      if (string.IsNullOrEmpty(path))
        return new SplitResult(string.Empty, string.Empty);

      if (path[0] == '.')
        path = Path.GetFullPath(path);

      var lastIndex = path.LastIndexOf(Path.DirectorySeparatorChar);
      if (lastIndex != -1)
        return new SplitResult(path.Substring(0, lastIndex), path.Substring(lastIndex + 1));
      else
        return new SplitResult(string.Empty, path);
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetLongPathName(string lpszShortPath, [Out] StringBuilder lpszLongPath, int cchBuffer);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetShortPathNameW", SetLastError = true)]
    private static extern int GetShortPathName(string pathName, StringBuilder shortName, uint cbShortName);

    private static string LongFileNameKernel(this string shortPath)
    {
      if (string.IsNullOrEmpty(shortPath)) return shortPath;
      var longNameBuffer = new StringBuilder(4000);
      var length = GetLongPathName(shortPath.LongPathPrefix(), longNameBuffer, longNameBuffer.Capacity);
      return length > 0 ? longNameBuffer.ToString(0, length) : shortPath;
    }

#pragma warning disable CA1034 // Nested types should not be visible
    public class SplitResult
#pragma warning restore CA1034 // Nested types should not be visible
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
      public readonly string DirectoryName;
      public readonly string FileName;
#pragma warning restore CA1051 // Do not declare visible instance fields

      public SplitResult(string dir, string file)
      {
        DirectoryName = dir;
        FileName = file;
      }
    }
  }
}