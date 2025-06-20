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


// Ignore Spelling: Utc

#nullable enable

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using static System.Environment;
using static System.Net.Mime.MediaTypeNames;

// ReSharper disable UseIndexFromEndExpression
// ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
// ReSharper disable ReplaceSubstringWithRangeIndexer


namespace CsvTools
{
  /// <summary>
  ///   Extensions for string in the file system
  /// </summary>
#pragma warning disable VSSpell001
  public static class FileSystemUtils
#pragma warning restore VSSpell001
  {
    /// <summary>
    ///   On windows, we need to take care of filename that might exceed 248 characters, they need to
    ///   be escaped.
    /// </summary>
    private const string cLongPathPrefix = @"\\?\";

    private const string cUncLongPathPrefix = @"\\?\UNC\";
    private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    /// <summary>
    /// Returns the full path of a file name, resolving any wild cards or environment variables
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="root"></param>
    /// <returns></returns>
    public static string FullPath(this string? fileName, in string? root) =>
      ResolvePattern(fileName.GetAbsolutePath(root)) ?? string.Empty;

    /// <summary>
    /// Creates a directory if it does not exist
    /// </summary>
    /// <param name="directoryName"></param>
    public static void CreateDirectory(in string? directoryName)
    {
      if (directoryName is null || directoryName.Length == 0)
        return;
      Directory.CreateDirectory(directoryName.LongPathPrefix());
    }

    /// <summary>
    ///   Deletes a file and creates a backup copy with a .bak extension
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="multipleBackups">if set to <c>true</c> multiple backup version are kept.</param>
    public static void DeleteWithBackup(in string fileName, bool multipleBackups)
    {
      try
      {
        if (!FileExists(fileName))
          return;

        var backupName = fileName + ".bak";
        if (multipleBackups)
        {
          var split = SplitPath(fileName);
          var names = Directory.EnumerateFiles(split.DirectoryName.LongPathPrefix(), split.FileName + "*.bak",
            SearchOption.TopDirectoryOnly);
          var numBackup = 1;
          foreach (var name in names)
          {
            var psStart = name.LastIndexOf('V');
            var psEnd = name.LastIndexOf('.');
            if (psStart != -1 && psEnd <= psStart + 5)
            {
              if (int.TryParse(name.Substring(psStart + 1, psEnd - psStart - 1), out var num))
                if (num > numBackup)
                  numBackup = num;
            }
          }

          backupName = fileName + $"_V{numBackup + 1}.bak";
        }
        else
        {
          FileDelete(backupName);
        }

        File.Move(fileName.LongPathPrefix(), backupName.LongPathPrefix());
      }
      catch (Exception ex)
      {
        Logger.Information(ex, "DeleteWithBackup {fileName}", fileName);
      }
    }

    /// <summary>
    /// Check if a directory exists.
    /// </summary>
    /// <param name="directoryName">Name of the directory.</param>    
    public static bool DirectoryExists(in string? directoryName) =>
      !(directoryName is null || directoryName.Length == 0) && Directory.Exists(directoryName.LongPathPrefix());

    /// <summary>
    ///   Folder of the executing assembly, mainly used in Unit testing
    /// </summary>
    /// <returns></returns>
    public static string ExecutableDirectoryName()
    {
      var directory = Assembly.GetExecutingAssembly().Location;
      if (directory.Length == 0)
        directory = Assembly.GetEntryAssembly()?.Location ?? string.Empty;

      return (directory.Length == 0
        ? Directory.GetCurrentDirectory()
        : Path.GetDirectoryName(directory.LongPathPrefix()))!;
    }

    /// <summary>
    /// Copy the content from one stream to another
    /// </summary>
    /// <param name="fromStream">From stream.</param>
    /// <param name="toStream">To stream.</param>
    /// <param name="progress">The progress.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public static async Task StreamCopy(
      Stream fromStream,
      Stream toStream,
      IProgress<ProgressInfo>? progress,
      CancellationToken cancellationToken)
    {
      var bytes = new byte[81920];
      int bytesRead;
      long totalReads = 0;
      // The original maximum value of the progress object, in case it implements the IProgressTime interface
      long oldMax = 0;
      if (progress is IProgressTime progressTime)
      {
        oldMax = progressTime.Maximum;
        progressTime.Maximum = fromStream.Length;
      }

      // invoked at regular intervals to report the progress
      var intervalAction = IntervalAction.ForProgress(progress);
      // Read data from the source stream until there is no more data or the operation is canceled
      while (!cancellationToken.IsCancellationRequested && (bytesRead = await fromStream
               .ReadAsync(bytes, 0, bytes.Length, cancellationToken)
               .ConfigureAwait(false)) > 0)
      {
        totalReads += bytesRead;
        // This line writes the data read from the source stream to the destination stream
        await toStream.WriteAsync(bytes, 0, bytesRead, cancellationToken).ConfigureAwait(false);
#pragma warning disable CS8604 // Possible null reference argument.
        intervalAction?.Invoke(progress, "Copy data", totalReads);
#pragma warning restore CS8604 // Possible null reference argument.
      }

      progress.SetMaximum(oldMax);
    }

    /// <summary>
    ///   Copies a file from one location to another asynchronously, while checking if the file has changed, reporting the progress and supporting cancellation.
    /// </summary>
    /// <param name="sourceFile">The file to be copied from</param>
    /// <param name="destFile">The file to be created / overwritten</param>
    /// <param name="onlyChanged">
    ///   Checks if the source file is newer or has a different length, if not file will not be copied,
    /// </param>
    /// <param name="progress">Progress used to report the progress of the copy operation</param>    
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    public static async Task FileCopy(
      string sourceFile,
      string destFile,
      bool onlyChanged,
      IProgress<ProgressInfo>? progress,
      CancellationToken cancellationToken)
    {
      if (onlyChanged)
      {
        var fiSource = new FileInfo(sourceFile);
        var fiDestInfo = new FileInfo(destFile);
        if (fiDestInfo.Exists && fiSource.LastWriteTimeUtc <= fiDestInfo.LastWriteTimeUtc
                              && fiSource.Length == fiDestInfo.Length)
          return;
      }

      // If the source file exists, delete the destination file if it exists
      if (FileExists(sourceFile))
        FileDelete(destFile);
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      using var fromStream = OpenRead(sourceFile);
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      using var toStream = OpenWrite(destFile);
      await StreamCopy(fromStream, toStream, progress, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///   Deletes a file if it exists.
    /// </summary>
    /// <param name="fileName">Specify the file to be deleted</param>
    public static void FileDelete(in string? fileName)
    {
      if (fileName is null || fileName.Length == 0) return;
      var fn = fileName.LongPathPrefix();
      if (File.Exists(fn))
        File.Delete(fn);
    }

    /// <summary>
    /// Checks if a file exists
    /// </summary>
    /// <param name="fileName">Name of the file.</param>    
    public static bool FileExists(in string? fileName) =>
      !(fileName is null || fileName.Length == 0) && File.Exists(fileName.LongPathPrefix());

    /// <summary>
    ///   Gets the absolute (rooted) path.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="basePath">The base path.</param>
    /// <returns>The combined filename with the LongPathPrefix if necessary</returns>
    public static string GetAbsolutePath(this string? fileName, string? basePath = null)
    {
      if (fileName is null || fileName.Length == 0)
        return string.Empty;
      if (basePath is null || basePath.Length == 0)
        basePath = ".";
      try
      {
        fileName = Environment.ExpandEnvironmentVariables(fileName);
        if (Path.IsPathRooted(fileName))
          return fileName;

        var split = fileName.LastIndexOf(Path.DirectorySeparatorChar);
        if (split == -1)
          return Path.Combine(GetFullPath(basePath), fileName).RemovePrefix();

        // the Filename could contain wild cards, that is not supported when extending relative path
        // the path part though ca not contain wild cards, so combine base and path
        return string.Concat(GetFullPath(Path.Combine(basePath, fileName.Substring(0, split))),
            fileName.Substring(split))
          .RemovePrefix();
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "Getting absolute path for combination {filename} {root}", fileName, basePath);
        throw;
      }
    }

    /// <summary>
    ///  Get the last file of a given pattern in a folder
    /// </summary>
    /// <param name="folder">The directory to look in</param>
    /// <param name="searchPattern">The pattern to look for</param>
    /// <returns>No matching file is found of the folder does not exist an empty string is returned</returns>
    public static string GetLatestFileOfPattern(string folder, in string searchPattern)
    {
      if (string.IsNullOrEmpty(folder))
        folder = ".";
      else if (!DirectoryExists(folder))
        return string.Empty;

      // If a pattern is present in the folder this is not going to work
      var newSet = new DateTime(0);
      var lastFile = string.Empty;
      foreach (var fileName in Directory.EnumerateFiles(
                 folder.LongPathPrefix(),
                 searchPattern,
                 SearchOption.TopDirectoryOnly))
      {
        var fileTime = new System.IO.FileInfo(fileName).LastWriteTimeUtc;
        if (fileTime <= newSet)
          continue;
        newSet = fileTime;
        lastFile = fileName;
      }

      return lastFile.RemovePrefix();
    }

    private static string ReplaceHolder(in string fileName, in string dir, in string placeHolder)
    {
      if (fileName.Equals(dir, StringComparison.OrdinalIgnoreCase))
        return placeHolder;

      if (fileName.StartsWith(dir, StringComparison.OrdinalIgnoreCase))
      {
        if (placeHolder == ".")
          return fileName.Substring(dir.Length + 1);
        return placeHolder + fileName.Substring(dir.Length);
      }

      return string.Empty;
    }

    private static string UseSpecialFolders(in string fileName)
    {
      if (IsWindows)
      {
        var test = ReplaceHolder(fileName, GetFolderPath(Environment.SpecialFolder.UserProfile),
        "%UserProfile%");
        if (test.Length > 0)
          return test;

        test = ReplaceHolder(fileName, GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "%AppData%");
        if (test.Length > 0)
          return test;

        test = ReplaceHolder(fileName, GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
          "%LocalAppData%");
        if (test.Length > 0)
          return test;
      }
      return fileName;
    }

    /// <summary>
    ///   Get a relative path to the file
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="basePath">The base path.</param>
    /// <returns>A relative path if possible</returns>
    public static string GetRelativePath(this string? fileName, string? basePath)
    {
      if (fileName is null || fileName.Length == 0 || fileName.StartsWith(".", StringComparison.Ordinal)
          || fileName.IndexOf(Path.DirectorySeparatorChar) == -1)
        return fileName ?? string.Empty;

      if (basePath is null || basePath.Length == 0)
        basePath = Directory.GetCurrentDirectory();

      var test = ReplaceHolder(fileName, GetFullPath(basePath), ".");
      if (test.Length > 0)
        return test;

      var otherDir = Path.GetFullPath(fileName);
      //TODO: this looks odd, try to send in filename with "%UserProfile%"
      var folder = GetRelativeFolder(otherDir, basePath);
      return folder.Substring(0, folder.Length - 1);
    }

    /// <summary>
    /// Gets the relative path
    /// </summary>
    /// <param name="otherDir">The directory to look for.</param>
    /// <param name="basePath">The base path for be assumed for relative path.</param>
    /// <returns></returns>
    public static string GetRelativeFolder(this string otherDir, string basePath)
    {
      if (otherDir.Equals(basePath, StringComparison.OrdinalIgnoreCase))
        return "." + Path.DirectorySeparatorChar;
      if (string.IsNullOrEmpty(otherDir))
        return "." + Path.DirectorySeparatorChar;
      if (basePath[basePath.Length - 1] != Path.DirectorySeparatorChar)
        basePath += Path.DirectorySeparatorChar;
      if (otherDir[otherDir.Length - 1] != Path.DirectorySeparatorChar)
        otherDir += Path.DirectorySeparatorChar;

      if (otherDir.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
        return otherDir.Substring(basePath.Length);

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
      for (var i = sameCounter; i < startPathParts.Length - 1; i++)
        sBuilder.Append(".." + Path.DirectorySeparatorChar);

      for (var i = sameCounter; i < destinationPathParts.Length - 1; i++)
        sBuilder.Append(destinationPathParts[i] + Path.DirectorySeparatorChar);
      sBuilder.Length--;

      var result = sBuilder.ToString();
      if (result[result.Length - 1] == Path.DirectorySeparatorChar) return result;
      sBuilder.Append(Path.DirectorySeparatorChar);
      return sBuilder.ToString();
    }

    /// <summary>
    ///   Gets a shortened name to the display file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public static string GetShortDisplayFileName(this string fileName, int length = 80)
    {
      var ret = fileName.RemovePrefix();
      if (length <= 0 || string.IsNullOrEmpty(fileName) || fileName.Length <= length)
        return ret;
      var parts = fileName.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
      var fileNameOnly = parts[parts.Length - 1];

      // try to cut out directories
      if (parts.Length > 5)
        ret =
          $"{parts[0]}{Path.DirectorySeparatorChar}{parts[1]}{Path.DirectorySeparatorChar}…{Path.DirectorySeparatorChar}{parts[parts.Length - 3]}{Path.DirectorySeparatorChar}{parts[parts.Length - 2]}{Path.DirectorySeparatorChar}{fileNameOnly}";

      if (ret.Length > length && parts.Length > 3)
        ret =
          $"{parts[0]}{Path.DirectorySeparatorChar}…{Path.DirectorySeparatorChar}{parts[parts.Length - 2]}{Path.DirectorySeparatorChar}{fileNameOnly}";

      if (ret.Length > length && parts.Length > 3)
        ret = $"…{Path.DirectorySeparatorChar}{parts[parts.Length - 2]}{Path.DirectorySeparatorChar}{fileNameOnly}";

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

    /// <summary>
    ///   Gets the name of the directory, unlike Path.GetDirectoryName is return the input in case
    ///   the input was a directory already
    /// </summary>
    /// <param name="fileOrDirectory">Name of the file or directory.</param>
    /// <returns>The folder / directory of the given file or directory</returns>
    public static string GetDirectoryName(this string fileOrDirectory)
    {
      if (string.IsNullOrEmpty(fileOrDirectory))
        return string.Empty;

      if (fileOrDirectory[0] == '.')
        fileOrDirectory = Path.GetFullPath(fileOrDirectory);

      if (DirectoryExists(fileOrDirectory))
        return fileOrDirectory;

      // get the directory from under it
      var lastIndex = fileOrDirectory.LastIndexOf(Path.DirectorySeparatorChar);
      return lastIndex > 0 ? fileOrDirectory.Substring(0, lastIndex).RemovePrefix() : string.Empty;
    }

    /// <summary>
    ///   Gets filename that is usable in the file system.
    /// </summary>
    /// <param name="original">The original text.</param>
    /// <param name="replaceInvalid">The replacement for invalid chars</param>
    /// <returns>A text that is allowed in the file system as filename</returns>
    public static string SafePath(this string original, in string replaceInvalid = "")
    {
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

    /// <summary>
    /// Retrieves the short path form of the specified path, see 8.3 aliasing for FAT file system
    /// </summary>
    /// <param name="longPath">The long path.</param>
    /// <returns>The abbreviated short name</returns>    
    public static string ShortFileName(this string longPath)
    {
      if (!IsWindows || string.IsNullOrEmpty(longPath))
        return longPath;
      var fi = new System.IO.FileInfo(longPath);
      const uint bufferSize = 512;
      var shortNameBuffer = new StringBuilder((int) bufferSize);

      // we might be asked to build a short path when the file does not exist yet, this would fail
      if (fi.Exists)
      {
        var length = GetShortPathName(longPath, shortNameBuffer, bufferSize);
        if (length > 0) return shortNameBuffer.ToString().RemovePrefix();
      }

      // if we have at least the directory shorten this
      if (fi.Directory?.Exists ?? false)
      {
        var length = GetShortPathName(fi.Directory.FullName, shortNameBuffer, bufferSize);
        if (length > 0)
          return (shortNameBuffer + (shortNameBuffer[shortNameBuffer.Length - 1] == Path.DirectorySeparatorChar
                    ? string.Empty
                    : Path.DirectorySeparatorChar.ToString()) +
                  fi.Name)
            .RemovePrefix();
      }

      throw new FileNotFoundException($"Could not get a short path for the file {longPath}");
    }

    /// <summary>
    /// Gets the shorter version of absolute and relative representation
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="basePath">The base path.</param>
    /// <returns></returns>
    public static string GetShortestPath(this string? fileName, in string? basePath)
    {
      if (fileName == null)
        return string.Empty;

      var absolute = fileName.GetAbsolutePath(basePath);
      var relative = fileName.GetRelativePath(basePath);

      if (relative.Length < absolute.Length)
        return relative;

      var fileNameSpecial = UseSpecialFolders(absolute);
      if (fileNameSpecial.Length < absolute.Length)
        return fileNameSpecial;

      return absolute;
    }

    /// <summary>Opens the file for writing</summary>
    /// <param name="fileName">Name of the file.</param>
    public static FileStream OpenWrite(in string fileName) => File.OpenWrite(fileName.LongPathPrefix());

    /// <summary>
    /// Creates a file in a particular path.  If the file exists, it is replaced. The file is opened with ReadWrite access and cannot be opened by another application until it has been closed.  
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="bufferSize">Size of the buffer.</param>
    /// <param name="options">The options like RandomAccess or Asynchronous</param>    
    public static FileStream Create(in string fileName, int bufferSize, in FileOptions options) =>
      File.Create(fileName.LongPathPrefix(), bufferSize, options);

    /// <summary>
    /// Writes all text into the file
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="contents">The contents.</param>
    public static void WriteAllText(in string fileName, in string contents) =>
      File.WriteAllText(fileName.LongPathPrefix(), contents);

    /// <summary>
    /// Writes all text to the given file
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="contents">The contents.</param>
    /// <param name="encoding">The encoding to be used</param>
    public static void WriteAllText(in string fileName, in string contents, in Encoding encoding) =>
      File.WriteAllText(fileName.LongPathPrefix(), contents, encoding);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetShortPathNameW", SetLastError = true)]
    private static extern int GetShortPathName(string pathName, StringBuilder shortName, uint cbShortName);

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Writes all text to the given file
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="contents">The contents.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task WriteAllTextAsync(in string fileName, in string contents, CancellationToken cancellationToken) =>
      File.WriteAllTextAsync(fileName.LongPathPrefix(), contents, cancellationToken);
#endif

    /// <summary>
    /// Gets the full path of a file, taking case of Prefixes
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    public static string GetFullPath(in string path) => Path.GetFullPath(path.LongPathPrefix()).RemovePrefix();

    /// <summary>
    /// Opens the file for reading
    /// </summary>
    /// <param name="fileName">Name of the file.</param>    
    public static FileStream OpenRead(in string fileName) => File.OpenRead(fileName.LongPathPrefix());

    /// <summary>
    /// Writes all bytes of content to the file
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="contents">The contents to be written.</param>
    public static void WriteAllBytes(in string fileName, in byte[] contents) =>
      File.WriteAllBytes(fileName.LongPathPrefix(), contents);

    /// <summary>
    /// Gets the stream reader for a file or resource.
    /// </summary>
    /// <param name="resourceName">The name of the resource</param>
    /// <returns></returns>
    /// <exception cref="System.ArgumentException">Could not locate stream</exception>
    public static StreamReader GetStreamReaderForFileOrResource(string resourceName)
    {
      var fileName = Path.Combine(ExecutableDirectoryName(), resourceName);
      try
      {
        if (FileExists(fileName))
          return new StreamReader(OpenRead(fileName), true);

        var executingAssembly = Assembly.GetExecutingAssembly();
        Stream? stream = null;
        var foundName = executingAssembly.GetManifestResourceNames()
          .FirstOrDefault(x => x.EndsWith("." + resourceName, StringComparison.OrdinalIgnoreCase));
        // try the embedded resource
        if (foundName != null)
          stream = executingAssembly.GetManifestResourceStream(foundName);

        var callingAssembly = Assembly.GetCallingAssembly();
        if (callingAssembly != executingAssembly)
        {
          foundName = callingAssembly.GetManifestResourceNames()
            .FirstOrDefault(x => x.EndsWith("." + resourceName, StringComparison.OrdinalIgnoreCase));
          if (foundName != null)
            stream = callingAssembly.GetManifestResourceStream(foundName);
        }

        if (stream != null)
          return new StreamReader(stream, true);
      }
      catch (Exception ex)
      {
        try { Logger.Error(ex); } catch { }
      }

      throw new ArgumentException($"Could not locate stream for {resourceName}");
    }

    /// <summary>
    ///   Get the long name of the file in case it was shorted with ~
    /// </summary>
    /// <param name="shortPath">The short path.</param>    
    public static string LongFileName(this string shortPath)
    {
      if (string.IsNullOrEmpty(shortPath))
        return shortPath;
      if (!IsWindows)
        return shortPath.Contains("." + Path.DirectorySeparatorChar) ? Path.GetFullPath(shortPath) : shortPath;

      return shortPath.Contains('~') ? shortPath.LongFileNameKernel() : shortPath;
    }

    /// <summary>
    /// Gets a prefix that allows .NET windows system to deal with filename that exceed 248 characters
    /// </summary>
    /// <param name="path">The path to the file.</param>    
    public static string LongPathPrefix(this string path)
    {
      // In case the directory is 248 we need long path as well
      if (!IsWindows || path.Length < 248 || path.StartsWith(cLongPathPrefix, StringComparison.Ordinal) ||
          path.StartsWith(cUncLongPathPrefix, StringComparison.OrdinalIgnoreCase))
        return path;
      return path.StartsWith(@"\\", StringComparison.Ordinal)
        ? cUncLongPathPrefix + path.Substring(2)
        : cLongPathPrefix + path;
    }


    /// <summary>
    /// Removed the prefix that allows .NET windows system to deal with filename that exceed 248 characters
    /// </summary>
    /// <param name="path">The possibly prefixed name of th file.</param>
    public static string RemovePrefix(this string path)
    {
      if (!IsWindows || path.StartsWith(cLongPathPrefix, StringComparison.Ordinal))
        return path.Substring(cLongPathPrefix.Length);
      return path.StartsWith(cUncLongPathPrefix, StringComparison.Ordinal)
        ? path.Substring(cUncLongPathPrefix.Length)
        : path;
    }

    /// <summary>
    /// Resolve placeholders in file names and find the latest field to match the pattern
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string ResolvePattern(in string fileName)
    {
      if (fileName is null || fileName.Length == 0)
        return string.Empty;

      // expand %AppData%, %LOCALAPPDATA% or %USERPROFILE% and alike
      var withoutPlaceHolder = Environment.ExpandEnvironmentVariables(fileName
        .PlaceholderReplaceFormat("date", DateTime.Now.ToString(CultureInfo.CurrentCulture))
        .PlaceholderReplaceFormat("utc", DateTime.UtcNow.ToString(CultureInfo.CurrentCulture))
        //.PlaceholderReplace("CDate", string.Format(new CultureInfo("en-US"), "{0:dd-MMM-yyyy}", DateTime.Now))
        //.PlaceholderReplace("CDateLong", string.Format(new CultureInfo("en-US"), "{0:MMMM dd\\, yyyy}", DateTime.Now))        
        );

      // only if we have wild cards carry on
      if (fileName.IndexOfAny(new[] { '*', '?', '[', ']' }) == -1)
        return withoutPlaceHolder;

      // Handle Placeholders      
      var split = SplitPath(withoutPlaceHolder);

      // search for the file
      return GetLatestFileOfPattern(split.DirectoryName, split.FileName);
    }

    /// <summary>
    /// Gets the name of the file.
    /// </summary>
    /// <param name="path">The full path possibly with directories</param>
    /// <returns></returns>
    public static string GetFileName(in string? path)
    {
      if (path is null || path.Length == 0)
        return string.Empty;
      var lastIndex = path.LastIndexOf(Path.DirectorySeparatorChar);
      return lastIndex != -1 ? path.Substring(lastIndex + 1) : path;
    }

    /// <summary>
    /// Creates a text file
    /// </summary>
    /// <param name="path">The path to the file.</param>
    public static StreamWriter CreateText(in string path) => File.CreateText(path.LongPathPrefix());

    /// <summary>
    /// Reads all text for a file
    /// </summary>
    /// <param name="path">The path to the file.</param>
    public static string ReadAllText(in string path) => File.ReadAllText(path.LongPathPrefix());

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Reads all text for a file
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    public static Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken)
      => File.ReadAllTextAsync(path.LongPathPrefix(), cancellationToken);
#endif

    /// <summary>
    /// Splits the full path of a file into directory and filename
    /// </summary>
    /// <param name="path">The full path.</param>    
    public static SplitResult SplitPath(string? path)
    {
      if (path is null || path.Length == 0)
        return new SplitResult(string.Empty, string.Empty);

      var lastIndex = path.LastIndexOf(Path.DirectorySeparatorChar);
      if (path.StartsWith(".", StringComparison.Ordinal))
      {
        if (lastIndex != -1)
          path = Path.GetFullPath(path.Substring(0, lastIndex)) + path.Substring(lastIndex);
        else
          path = Path.GetFullPath(path);
        lastIndex = path.LastIndexOf(Path.DirectorySeparatorChar);
      }

      return lastIndex != -1
        ? new SplitResult(path.Substring(0, lastIndex).RemovePrefix(), path.Substring(lastIndex + 1))
        : new SplitResult(string.Empty, path.RemovePrefix());
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetLongPathName(string lpszShortPath, [Out] StringBuilder lpszLongPath, int cchBuffer);

    private static string LongFileNameKernel(this string shortPath)
    {
      if (string.IsNullOrEmpty(shortPath))
        return shortPath;
      var longNameBuffer = new StringBuilder(4000);
      var length = GetLongPathName(shortPath.LongPathPrefix(), longNameBuffer, longNameBuffer.Capacity);
      return length > 0 ? longNameBuffer.ToString(0, length) : shortPath;
    }

    /// <summary>
    ///   In general a wrapper for <see cref="System.IO.FileInfo" />, but it does allow to store
    ///   information from other sources (sFTP, Zip etc.) Provides properties for a files, it has a
    ///   reduced property set. Allows the update of LastWriteTimeUtc
    /// </summary>
    public class FileInfo
    {
      private readonly System.IO.FileInfo? m_Info;
      private DateTime m_LastWriteTimeUtc = new DateTime(0, DateTimeKind.Utc);

      /// <summary>
      /// Initializes a new instance of the <see cref="FileInfo"/> class.
      /// </summary>
      /// <param name="fileName">Name of the file.</param>
      public FileInfo(string? fileName)
      {
        if (fileName is null || fileName.Length == 0)
        {
          Name = string.Empty;
          return;
        }

        Name = fileName.RemovePrefix();
        try
        {
          m_Info = new System.IO.FileInfo(fileName.LongPathPrefix());
          if (!m_Info.Exists)
            return;

          Exists = true;
          Length = m_Info.Length;
          m_LastWriteTimeUtc = m_Info.LastWriteTimeUtc;
        }
        catch (Exception ex)
        {
          Logger.Information(ex, "FileInfo {fileName}", Name);
        }
      }


      /// <summary>
      /// Initializes a new instance of the <see cref="FileInfo"/> class.
      /// </summary>
      /// <param name="fileName">Name of the file.</param>
      /// <param name="length">The length.</param>
      /// <param name="lastWriteTimeUtc">The last write time UTC.</param>
      public FileInfo(string fileName, long length, DateTime lastWriteTimeUtc)
      {
        Name = fileName;
        Length = length;
        m_LastWriteTimeUtc = lastWriteTimeUtc;
      }

      /// <summary>
      /// Gets a value indicating whether the file does exist already
      /// </summary>
      /// <value>
      ///   <c>true</c> if exists; otherwise, <c>false</c>.
      /// </value>
      public bool Exists { get; }

      /// <summary>
      /// Gets or sets the last write time UTC for the file
      /// </summary>
      /// <value>
      /// The last write time UTC.
      /// </value>
      public DateTime LastWriteTimeUtc
      {
        get => m_LastWriteTimeUtc;
        set
        {
          m_LastWriteTimeUtc = value;
          if (m_Info != null)
            m_Info.LastWriteTimeUtc = value;
        }
      }

      /// <summary>
      /// Gets the length of th file
      /// </summary>
      /// <value>
      /// The length.
      /// </value>
      public long Length { get; }

      /// <summary>
      /// Gets the name of the file
      /// </summary>
      /// <value>
      /// The name.
      /// </value>
      public string Name { get; }
    }

    /// <summary>
    ///   class storing the result for spited file names, mainly used to handle extension
    /// </summary>
    public class SplitResult
    {
      /// <summary>The directory name</summary>
      public readonly string DirectoryName;

      /// <summary>The file name with extension</summary>
      public readonly string FileName;

      /// <summary>
      /// Initializes a new instance of the <see cref="SplitResult"/> class.
      /// </summary>
      /// <param name="dir">The directory</param>
      /// <param name="file">The filename</param>
      public SplitResult(string dir, string file)
      {
        DirectoryName = dir;
        FileName = file;
      }

      /// <summary>
      ///   Get the extension with the dot
      /// </summary>
      /// <example>Test.docx -&gt; .docx</example>
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
      /// <example>Test.docx -&gt; Test</example>
      public string FileNameWithoutExtension
      {
        get
        {
          var index = FileName.LastIndexOf('.');
          return index == -1 ? FileName : FileName.Substring(0, index);
        }
      }
    }
  }
}