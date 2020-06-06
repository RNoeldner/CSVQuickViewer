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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;
using Path = Pri.LongPath.Path;
using Directory = Pri.LongPath.Directory;
using File = Pri.LongPath.File;
using FileInfo = Pri.LongPath.FileInfo;

namespace CsvTools
{
  /// <summary>
  ///   Extensions for string in the file system
  /// </summary>
  public static class FileSystemUtils
  {
    private const string c_LongPathPrefix = @"\\?\";

    private const string c_UncLongPathPrefix = @"\\?\UNC\";


    public static void CreateDirectory([CanBeNull] string directoryName)
    {
      if (string.IsNullOrEmpty(directoryName))
        return;
      Directory.CreateDirectory(directoryName);
    }

    /// <summary>
    ///   Makes the backup.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="twoBackups">if set to <c>true</c> [two backups].</param>
    public static void DeleteWithBackup([NotNull] string fileName, bool twoBackups)
    {
      Contract.Requires(fileName != null);
      try
      {
        if (!FileExists(fileName))
          return;
        var backupName = fileName + ".bak";
        var backupName2 = string.Empty;
        if (twoBackups)
        {
          backupName2 = fileName + "2.bak";
          FileDelete(backupName2);
        }

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

    public static bool DirectoryExists([CanBeNull] string directoryName) =>
      !string.IsNullOrEmpty(directoryName) && Directory.Exists(directoryName);

    /// <summary>
    ///   Folder of the executing assembly, mainly used in Unit testing
    /// </summary>
    /// <returns></returns>
    [NotNull]
    public static string ExecutableDirectoryName()
    {
      var directory = Assembly.GetExecutingAssembly().Location;
      if (string.IsNullOrEmpty(directory))
        directory = Assembly.GetEntryAssembly()?.Location;
      if (string.IsNullOrEmpty(directory))
        directory = ".";
      return Path.GetDirectoryName(directory);
    }

    /// <summary>
    ///   Copy a file locally and provide progress
    /// </summary>
    /// <param name="sourceFile">The file to be copied from</param>
    /// <param name="destFile">The file to be created / overwritten</param>
    /// <param name="processDisplay">A process display</param>
    public static void FileCopy([NotNull] string sourceFile, [NotNull] string destFile, IProcessDisplay processDisplay)
    {
      if (FileExists(sourceFile))
        FileDelete(destFile);
      using (var fromStream = OpenRead(sourceFile))
      using (var toStream = OpenWrite(destFile))
      {
        var bytes = new byte[81920];
        int bytesRead;
        long totalReads = 0;

        long oldMax = 0;
        if (processDisplay != null)
        {
          oldMax = processDisplay.Maximum;
          processDisplay.Maximum = fromStream.Length;
        }

        var intervalAction = processDisplay == null ? null : new IntervalAction();
        while ((bytesRead = fromStream.Read(bytes, 0, bytes.Length)) > 0)
        {
          processDisplay?.CancellationToken.ThrowIfCancellationRequested();
          totalReads += bytesRead;
          toStream.Write(bytes, 0, bytesRead);
          intervalAction?.Invoke(pos => processDisplay.SetProcess("Copy file", pos, false), totalReads);
        }

        if (processDisplay != null)
        {
          processDisplay.Maximum = oldMax;
        }
      }
    }

    public static void FileDelete([CanBeNull] string fileName)
    {
      if (FileExists(fileName))
      {
        File.Delete(fileName);
      }
    }

    public static bool FileExists([CanBeNull] string fileName) =>
      !string.IsNullOrEmpty(fileName) && File.Exists(fileName);

    /// <summary>
    ///   Gets the absolute path.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="basePath">The base path.</param>
    /// <returns>The combined filename with the LongPathPrefix if necessary</returns>
    [NotNull]
    public static string GetAbsolutePath([CanBeNull] this string fileName, [CanBeNull] string basePath)
    {
      Contract.Ensures(Contract.Result<string>() != null);
      if (string.IsNullOrEmpty(fileName))
        return string.Empty;

      return !Path.IsPathRooted(fileName)
               ? Path.GetFullPath(string.IsNullOrEmpty(basePath) ? fileName : PathCombine(basePath, fileName))
               : fileName;
    }

    /// <summary>
    ///   Gets the name of the directory, unlike Path.GetDirectoryName is return the input in case the input was a directory
    ///   already
    /// </summary>
    /// <param name="fileOrDirectory">Name of the file or directory.</param>
    /// <returns>The folder / directory of the given file or directory</returns>
    [CanBeNull]
    [ContractAnnotation("null => null; notnull => notnull")]
    public static string GetDirectoryName([CanBeNull] this string fileOrDirectory)
    {
      if (string.IsNullOrEmpty(fileOrDirectory))
        return null;

      if (fileOrDirectory[0] == '.')
        fileOrDirectory = Path.GetFullPath(fileOrDirectory);

      if (DirectoryExists(fileOrDirectory))
        return fileOrDirectory;

      // get the directory from under it
      var lastIndex = fileOrDirectory.LastIndexOf('\\');
      return lastIndex > 0 ? fileOrDirectory.Substring(0, lastIndex).RemovePrefix() : null;
    }

    public static long FileLength([NotNull] string fileName) => new FileInfo(fileName).Length;
    public static DateTime GetLastWriteTimeUtc([NotNull] string fileName) => new FileInfo(fileName).LastWriteTimeUtc;
    public static DateTime SetLastWriteTimeUtc([NotNull] string fileName, DateTime newTime) => new FileInfo(fileName).LastWriteTimeUtc = newTime;

    [NotNull]
    [ItemNotNull]
    public static string[] GetFiles([NotNull] string folder, [NotNull] string searchPattern)
    {
      Contract.Requires(!string.IsNullOrEmpty(folder));
      Contract.Requires(!string.IsNullOrEmpty(searchPattern));

      return folder.IndexOfAny(new[] { '*', '?', '[', ']' }) == -1
               ? Directory.GetFiles(folder, searchPattern, SearchOption.TopDirectoryOnly)
               : new string[] { };
    }

    [CanBeNull]
    public static string GetLatestFileOfPattern([NotNull] string folder, [NotNull] string searchPattern)
    {
      Contract.Requires(!string.IsNullOrEmpty(folder));
      Contract.Requires(!string.IsNullOrEmpty(searchPattern));
      if (!Directory.Exists(folder))
        return null;
      var files = GetFiles(folder, searchPattern);

      switch (files.Length)
      {
        case 0:
          return null;
        case 1:
          return files[0];
      }

      // If a pattern is present in the folder this is not going to work
      var newSet = new DateTime(0);
      string lastFile = null;
      foreach (var fileName in files)
      {
        var fileTime = GetLastWriteTimeUtc(fileName);
        if (fileTime <= newSet)
          continue;
        newSet = fileTime;
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
    [NotNull]
    public static string GetRelativePath([CanBeNull] this string fileName, [CanBeNull] string basePath)
    {
      if (string.IsNullOrEmpty(fileName) || fileName.StartsWith(".", StringComparison.Ordinal)
                                         || fileName.IndexOf("\\", StringComparison.Ordinal) == -1)
        return fileName??string.Empty;

      if (string.IsNullOrEmpty(basePath))
        basePath = Directory.GetCurrentDirectory();

      if (fileName.Equals(basePath, StringComparison.OrdinalIgnoreCase))
        return ".";
      if (fileName.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
        return fileName.Substring(basePath.Length + 1);
      var otherDir = Path.GetFullPath(fileName);

      return GetRelativePathQuick(otherDir, basePath);
    }

    [NotNull]
    public static string GetRelativePathQuick([NotNull] this string otherDir, [NotNull] string basePath)
    {
      if (otherDir.Equals(basePath, StringComparison.OrdinalIgnoreCase))
        return ".";
      if (otherDir.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
        return otherDir.Substring(basePath.Length + 1);

      var startPathParts = basePath.Split(Path.DirectorySeparatorChar);
      var destinationPathParts = otherDir.Split(Path.DirectorySeparatorChar);
      var sameCounter = 0;
      while (sameCounter < startPathParts.Length && sameCounter < destinationPathParts.Length
                                                 && startPathParts[sameCounter].Equals(
                                                   destinationPathParts[sameCounter],
                                                   StringComparison.OrdinalIgnoreCase))
        sameCounter++;
      if (sameCounter == 0)
        return otherDir;

      var sBuilder = new StringBuilder();
      for (var i = sameCounter; i < startPathParts.Length; i++)
        sBuilder.Append(".." + Path.DirectorySeparatorChar);

      for (var i = sameCounter; i < destinationPathParts.Length; i++)
        sBuilder.Append(destinationPathParts[i] + Path.DirectorySeparatorChar);
      sBuilder.Length--;

      return sBuilder.ToString();
    }

    /// <summary>
    ///   Gets a shortened name to the display file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    [NotNull]
    public static string GetShortDisplayFileName([NotNull] string fileName, int length)
    {
      var ret = fileName.RemovePrefix();
      if (length <= 0 || string.IsNullOrEmpty(fileName) || fileName.Length <= length)
        return ret;
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
      if (ret.Length <= length)
        return ret;
      var cut = length * 2 / 3;
      ret = ret.Substring(0, length - cut) + "…" + ret.Substring(ret.Length - cut + 1);

      return ret;
    }

    [NotNull]
    public static string GetShortestPath([CanBeNull] this string fileName, [CanBeNull] string basePath)
    {
      var absolute = fileName.GetAbsolutePath(basePath);
      var relative = fileName.GetRelativePath(basePath);
      return relative.Length < absolute.Length ? relative : absolute;
    }

    public static string GetFileNameWithoutExtension([NotNull] string path) => Path.GetFileNameWithoutExtension(path);
    public static string PathCombine([NotNull] string path1, [NotNull] string path2) => Path.Combine(path1, path2);
    public static string GetFullPath([NotNull] string path) => Path.GetFullPath(path);
    public static FileStream OpenRead([NotNull] string fileName) => File.OpenRead(fileName);
    public static FileStream OpenWrite([NotNull] string fileName) => File.OpenWrite(fileName);
    public static FileStream Create([NotNull] string fileName, int bufferSize, FileOptions options) => File.Create(fileName, bufferSize, options);
    public static void WriteAllText([NotNull] string fileName, string contents) => File.WriteAllText(fileName, contents);

    public static StreamReader GetStreamReaderForFileOrResource([NotNull] string file)
    {
      var fileName = ExecutableDirectoryName() + "\\" + file;
      try
      {
        if (FileExists(fileName))
          return new StreamReader(OpenRead(fileName), true);

        var executingAssembly = Assembly.GetExecutingAssembly();
        var foundName = executingAssembly.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith("." + file));
        // try the embedded resource
        if (foundName != null)
          // ReSharper disable once AssignNullToNotNullAttribute
          return new StreamReader(executingAssembly.GetManifestResourceStream(foundName), true);
        var callingAssembly = Assembly.GetCallingAssembly();
        if (callingAssembly != executingAssembly)
        {
          foundName = callingAssembly.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith("." + file));
          if (foundName != null)
            // ReSharper disable once AssignNullToNotNullAttribute
            return new StreamReader(callingAssembly.GetManifestResourceStream(foundName), true);
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine("Error reading  " + fileName);
        Debug.WriteLine(ex.Message);
      }

      return null;
    }

    [CanBeNull]
    public static string LongFileName([CanBeNull] this string shortPath)
    {
      if (string.IsNullOrEmpty(shortPath))
        return shortPath;
      if (shortPath.Contains("~"))
        return shortPath.LongFileNameKernel();
      return shortPath.Contains(".\\") ? Path.GetFullPath(shortPath) : shortPath;
    }

    [CanBeNull]
    public static string LongPathPrefix([CanBeNull] this string path)
    {
      // In case the directory is 248 we need long path as well
      if (string.IsNullOrEmpty(path) || path.Length < 248 || path.StartsWith(c_LongPathPrefix, StringComparison.Ordinal)
          || path.StartsWith(c_UncLongPathPrefix, StringComparison.Ordinal))
        return path;
      return path.StartsWith(@"\\", StringComparison.Ordinal)
               ? c_UncLongPathPrefix + path.Substring(2)
               : c_LongPathPrefix + path;
    }

    [CanBeNull]
    [ContractAnnotation("null=>null; notNull=>notnull")]
    public static string RemovePrefix([CanBeNull] this string path)
    {
      if (string.IsNullOrEmpty(path))
        return path;
      if (path.StartsWith(c_LongPathPrefix, StringComparison.Ordinal))
        return path.Substring(c_LongPathPrefix.Length);
      return path.StartsWith(c_UncLongPathPrefix, StringComparison.Ordinal)
               ? path.Substring(c_UncLongPathPrefix.Length)
               : path;
    }

    [CanBeNull]
    public static string ResolvePattern(string fileName)
    {
      if (string.IsNullOrEmpty(fileName))
        return string.Empty;
      if (fileName.IndexOfAny(new[] { '*', '?', '[', ']' }) == -1)
        return fileName;

      var split = SplitPath(fileName);
      var folder = split.DirectoryName;
      var searchPattern = split.FileName;

      return GetLatestFileOfPattern(folder, searchPattern);
    }

    /// <summary>
    ///   Gets  filename that is usable in the file system.
    /// </summary>
    /// <param name="original">The original text.</param>
    /// <param name="replaceInvalid">The replace invalid.</param>
    /// <returns>
    ///   A text that is allowed in the file system as filename
    /// </returns>
    [NotNull]
    public static string SafePath([CanBeNull] this string original, [NotNull] string replaceInvalid = "")
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

    [CanBeNull]
    public static string ShortFileName([CanBeNull] this string longPath)
    {
      if (string.IsNullOrEmpty(longPath))
        return longPath;

      var fi = new FileInfo(longPath);

      const uint c_BufferSize = 512;
      var shortNameBuffer = new StringBuilder((int)c_BufferSize);

      // we might be asked to build a short path when the file does not exist yet, this would fail
      if (fi.Exists)
      {
        var length = GetShortPathName(longPath, shortNameBuffer, c_BufferSize);
        if (length > 0)
        {
          return shortNameBuffer.ToString().RemovePrefix();
        }
      }

      // if we have at least the directory shorten this
      if (!(fi.Directory?.Exists ?? false)) throw new Exception($"Could not get a short path for the file ${longPath}");
      {
        var length = GetShortPathName(fi.Directory.FullName, shortNameBuffer, c_BufferSize);
        if (length > 0)
          return (shortNameBuffer + (shortNameBuffer[shortNameBuffer.Length - 1] == '\\' ? string.Empty : "\\") + fi.Name)
            .RemovePrefix();
      }

      throw new Exception($"Could not get a short path for the file ${longPath}");
    }

    [NotNull]
    public static string GetFileName([CanBeNull] string path)
    {
      if (string.IsNullOrEmpty(path))
        return string.Empty;
      var lastIndex = path.LastIndexOf(Path.DirectorySeparatorChar);
      return (lastIndex != -1) ? path.Substring(lastIndex + 1) : path;
    }

    public static StreamWriter CreateText([NotNull] string path)
    {
      return File.CreateText(path);
    }

    public static string ReadAllText([NotNull] string path)
    {
      return File.ReadAllText(path);
    }

    [NotNull]
    public static SplitResult SplitPath([CanBeNull] string path)
    {
      if (string.IsNullOrEmpty(path))
        return new SplitResult(string.Empty, string.Empty);

      path = Path.GetFullPath(path);

      var lastIndex = path.LastIndexOf(Path.DirectorySeparatorChar);
      return lastIndex != -1
               ? new SplitResult(path.Substring(0, lastIndex), path.Substring(lastIndex + 1))
               : new SplitResult(path, string.Empty);
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetLongPathName(string lpszShortPath, [Out] StringBuilder lpszLongPath, int cchBuffer);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetShortPathNameW", SetLastError = true)]
    private static extern int GetShortPathName(string pathName, StringBuilder shortName, uint cbShortName);

    private static string LongFileNameKernel(this string shortPath)
    {
      if (string.IsNullOrEmpty(shortPath))
        return shortPath;
      var longNameBuffer = new StringBuilder(4000);
      var length = GetLongPathName(shortPath.LongPathPrefix(), longNameBuffer, longNameBuffer.Capacity);
      return length > 0 ? longNameBuffer.ToString(0, length) : shortPath;
    }

#pragma warning disable CA1034 // Nested types should not be visible

    public class SplitResult
#pragma warning restore CA1034
    {
      // Nested types should not be visible
      public SplitResult(string dir, string file)
      {
        DirectoryName = dir;
        FileName = file;
      }

      /// <summary>
      ///   Get the extension with the dot
      /// </summary>
      /// <example>Test.docx  -> .docx</example>
      public string Extension
      {
        get
        {
          var index = FileName.LastIndexOf('.');
          return index == -1 ? string.Empty : FileName.Substring(index);
        }
      }

      /// <summary>
      ///   Get the filename without the extension or the dot
      /// </summary>
      /// <example>Test.docx -> Test</example>
      public string FileNameWithoutExtension
      {
        get
        {
          var index = FileName.LastIndexOf('.');
          return index == -1 ? FileName : FileName.Substring(0, index);
        }
      }
#pragma warning disable CA1051 // Do not declare visible instance fields
      public readonly string DirectoryName;

      public readonly string FileName;
#pragma warning restore CA1051 // Do not declare visible instance fields
    }
  }
}