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
#if !QUICK
using System.Collections.Generic;
#endif


namespace CsvTools
{
  /// <summary>
  ///   Extensions for string in the file system
  /// </summary>
  public static class FileSystemUtils
  {
    /// <summary>
    ///   On windows we need to take care of filename that might exceed 248 characters, they need to
    ///   be escaped.
    /// </summary>
    private const string cLongPathPrefix = @"\\?\";

    private const string cUncLongPathPrefix = @"\\?\UNC\";
    private static readonly bool m_IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static string FullPath(this string? fileName, in string? root) =>
      ResolvePattern(fileName.GetAbsolutePath(root)) ?? string.Empty;

#if !QUICK
    public static void CreateDirectory(in string? directoryName)
    {
      if (directoryName is null || directoryName.Length == 0)
        return;
      Directory.CreateDirectory(directoryName.LongPathPrefix());
    }

    /// <summary>
    ///   Makes the backup.
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
              if (int.TryParse(name.Substring(psStart+1, psEnd-psStart-1), out var num))
                if (num> numBackup)
                  numBackup= num;
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
#endif
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

#if !QUICK

    public static async Task StreamCopy(
      Stream fromStream,
      Stream toStream,
      IProgress<ProgressInfo>? progress,
      CancellationToken cancellationToken)
    {
      var bytes = new byte[81920];
      int bytesRead;
      long totalReads = 0;

      long oldMax = 0;
      if (progress is IProgressTime progressTime)
      {
        oldMax = progressTime.Maximum;
        progressTime.Maximum = fromStream.Length;
      }

      var intervalAction = IntervalAction.ForProgress(progress);
      while ((bytesRead = await fromStream.ReadAsync(bytes, 0, bytes.Length, cancellationToken)
               .ConfigureAwait(false)) > 0)
      {
        cancellationToken.ThrowIfCancellationRequested();
        totalReads += bytesRead;
        await toStream.WriteAsync(bytes, 0, bytesRead, cancellationToken).ConfigureAwait(false);
#pragma warning disable CS8604 // Possible null reference argument.
        intervalAction?.Invoke(progress, "Copy data", totalReads);
#pragma warning restore CS8604 // Possible null reference argument.
      }

      progress.SetMaximum(oldMax);
    }

    /// <summary>
    ///   Copy a file locally and provide progress
    /// </summary>
    /// <param name="sourceFile">The file to be copied from</param>
    /// <param name="destFile">The file to be created / overwritten</param>
    /// <param name="onlyChanged">
    ///   Checks if the source file is newer or has a different length, if not file will not be copied,
    /// </param>
    /// <param name="progress">A process display</param>
    /// ///
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

#endif

    public static void FileDelete(in string? fileName)
    {
      if (fileName is null || fileName.Length == 0) return;
      var fn = fileName.LongPathPrefix();
      if (File.Exists(fn))
        File.Delete(fn);
    }

    public static bool FileExists(in string? fileName) =>
      !(fileName is null || fileName.Length == 0) && File.Exists(fileName.LongPathPrefix());

    /// <summary>
    ///   Gets the absolute path.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="basePath">The base path.</param>
    /// <returns>The combined filename with the LongPathPrefix if necessary</returns>
    public static string GetAbsolutePath(this string? fileName, string? basePath)
    {
      if (fileName is null || fileName.Length == 0)
        return string.Empty;
      if (basePath is null || basePath.Length == 0)
        basePath = ".";
      try
      {
        if (Path.IsPathRooted(fileName))
          return fileName;

        var split = fileName.LastIndexOf(Path.DirectorySeparatorChar);
        if (split == -1)
          return Path.Combine(GetFullPath(basePath), fileName).RemovePrefix();

        // the Filename could contains wildcards, that is not supported when extending relative path
        // the path part though ca not contain wildcards, so combine base and path
        return string.Concat(GetFullPath(Path.Combine(basePath, fileName.Substring(0, split))), fileName.AsSpan(split))
          .RemovePrefix();
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "Getting absolute path for combination {filename} {root}", fileName, basePath);
        throw;
      }
    }

    /// <summary>
    ///  Get teh last file of a given pattern in a folder
    /// </summary>
    /// <param name="folder">The dierectory to look in</param>
    /// <param name="searchPattern">The pattern to look for</param>
    /// <returns>No matching file is found of teh folder does not exists an empty string is returned</returns>
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

      if (fileName.Equals(basePath, StringComparison.OrdinalIgnoreCase))
        return ".";
      if (fileName.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
        return fileName.Substring(basePath.Length + 1);
      var otherDir = Path.GetFullPath(fileName);

      var folder = GetRelativeFolder(otherDir, basePath);
      return folder.Substring(0, folder.Length - 1);
    }

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
    public static string GetShortDisplayFileName(in string fileName, int length = 80)
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
#if !QUICK
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
    /// <param name="replaceInvalid">The replace invalid.</param>
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

    public static string ShortFileName(this string longPath)
    {
      if (!m_IsWindows || string.IsNullOrEmpty(longPath))
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


    public static string GetShortestPath(this string? fileName, in string? basePath)
    {
      var absolute = fileName.GetAbsolutePath(basePath);
      var relative = fileName.GetRelativePath(basePath);
      return relative.Length < absolute.Length ? relative : absolute;
    }

    public static FileStream OpenWrite(in string fileName) => File.OpenWrite(fileName.LongPathPrefix());

    public static FileStream Create(in string fileName, int bufferSize, in FileOptions options) =>
      File.Create(fileName.LongPathPrefix(), bufferSize, options);

    public static void WriteAllText(in string fileName, in string contents) =>
      File.WriteAllText(fileName.LongPathPrefix(), contents);


    public static void WriteAllText(in string fileName, in string contents, in Encoding encoding) =>
      File.WriteAllText(fileName.LongPathPrefix(), contents, encoding);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetShortPathNameW", SetLastError = true)]
    private static extern int GetShortPathName(string pathName, StringBuilder shortName, uint cbShortName);

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    public static Task WriteAllTextAsync(in string fileName, in string contents, CancellationToken cancellationToken) =>
      File.WriteAllTextAsync(fileName.LongPathPrefix(), contents, cancellationToken);
#endif
#endif

    public static string GetFullPath(in string path) => Path.GetFullPath(path.LongPathPrefix()).RemovePrefix();

    public static FileStream OpenRead(in string fileName) => File.OpenRead(fileName.LongPathPrefix());

    public static void WriteAllBytes(in string fileName, in byte[] contents) =>
      File.WriteAllBytes(fileName.LongPathPrefix(), contents);

    public static StreamReader GetStreamReaderForFileOrResource(string file)
    {
      var fileName = Path.Combine(ExecutableDirectoryName(), file);
      try
      {
        if (FileExists(fileName))
          return new StreamReader(OpenRead(fileName), true);

        var executingAssembly = Assembly.GetExecutingAssembly();
        Stream? stream = null;
        var foundName = executingAssembly.GetManifestResourceNames()
          .FirstOrDefault(x => x.EndsWith("." + file, StringComparison.OrdinalIgnoreCase));
        // try the embedded resource
        if (foundName != null)
          stream =executingAssembly.GetManifestResourceStream(foundName);

        var callingAssembly = Assembly.GetCallingAssembly();
        if (callingAssembly != executingAssembly)
        {
          foundName = callingAssembly.GetManifestResourceNames()
            .FirstOrDefault(x => x.EndsWith("." + file, StringComparison.OrdinalIgnoreCase));
          if (foundName != null)
            stream =callingAssembly.GetManifestResourceStream(foundName);
        }
        if (stream != null)
          return new StreamReader(stream, true);
      }
      catch (Exception ex)
      {
        Logger.Error(ex);
      }

      throw new ArgumentException($"Could not locate stream for {file}");
    }

    public static string LongFileName(this string shortPath)
    {
      if (string.IsNullOrEmpty(shortPath))
        return shortPath;
      if (!m_IsWindows)
        return shortPath.Contains("." + Path.DirectorySeparatorChar) ? Path.GetFullPath(shortPath) : shortPath;

      if (shortPath.Contains("~"))
        return shortPath.LongFileNameKernel();

      return shortPath;
    }

    public static string LongPathPrefix(this string path)
    {
      // In case the directory is 248 we need long path as well
      if (!m_IsWindows || path.Length < 248 || path.StartsWith(cLongPathPrefix, StringComparison.Ordinal) ||
          path.StartsWith(cUncLongPathPrefix, StringComparison.OrdinalIgnoreCase))
        return path;
      return path.StartsWith(@"\\", StringComparison.Ordinal)
        ? cUncLongPathPrefix + path.Substring(2)
        : cLongPathPrefix + path;
    }

    public static ReadOnlySpan<char> RemovePrefix(this ReadOnlySpan<char> path)
    {
      if (!m_IsWindows || path.StartsWith(cLongPathPrefix, StringComparison.Ordinal))
        return path.Slice(cLongPathPrefix.Length);
      return path.StartsWith(cUncLongPathPrefix, StringComparison.Ordinal)
        ? path.Slice(cUncLongPathPrefix.Length)
        : path;
    }

    public static string RemovePrefix(this string path)
    {
      if (!m_IsWindows || path.StartsWith(cLongPathPrefix, StringComparison.Ordinal))
        return path.Substring(cLongPathPrefix.Length);
      return path.StartsWith(cUncLongPathPrefix, StringComparison.Ordinal)
        ? path.Substring(cUncLongPathPrefix.Length)
        : path;
    }

    /// <summary>
    /// Resolve placeholders in file names and find the latest fiel to match the pattern
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string ResolvePattern(in string fileName)
    {
      if (fileName == null || fileName.Length == 0)
        return string.Empty;

      var withoutPlaceHolder = fileName.PlaceholderReplaceFormat("date", DateTime.Now.ToString(CultureInfo.CurrentCulture))
                         .PlaceholderReplaceFormat("utc", DateTime.UtcNow.ToString(CultureInfo.CurrentCulture))
                         .PlaceholderReplace("CDate", string.Format(new CultureInfo("en-US"), "{0:dd-MMM-yyyy}", DateTime.Now))
                         .PlaceholderReplace("CDateLong", string.Format(new CultureInfo("en-US"), "{0:MMMM dd\\, yyyy}", DateTime.Now));

      // only if we have wildcards carry on
      if (fileName.IndexOfAny(new[] { '*', '?', '[', ']' }) == -1)
        return withoutPlaceHolder;

      // Handle Placeholders      
      var split = SplitPath(withoutPlaceHolder);

      // search for the file
      return GetLatestFileOfPattern(split.DirectoryName, split.FileName)!;
    }

    public static string GetFileName(in string? path)
    {
      if (path is null || path.Length == 0)
        return string.Empty;
      var lastIndex = path.LastIndexOf(Path.DirectorySeparatorChar);
      return lastIndex != -1 ? path.Substring(lastIndex + 1) : path;
    }

    public static StreamWriter CreateText(in string path) => File.CreateText(path.LongPathPrefix());

    public static string ReadAllText(in string path) => File.ReadAllText(path.LongPathPrefix());

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    public static Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken)
      => File.ReadAllTextAsync(path.LongPathPrefix(), cancellationToken);
#endif

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
    ///   In general a wrapper for for <see cref="System.IO.FileInfo" />, but it does allow to store
    ///   information from other sources (sFTP, Zip etc) Provides properties for a files, it has a
    ///   reduced property set. Allows the update of LastWriteTimeUtc
    /// </summary>
    public class FileInfo
    {
      private readonly System.IO.FileInfo? m_Info;

      private DateTime m_LastWriteTimeUtc = new DateTime(0, DateTimeKind.Utc);

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

      public FileInfo(string fileName, long length, DateTime lastWriteTimeUtc)
      {
        Name = fileName;
        Length = length;
        m_LastWriteTimeUtc = lastWriteTimeUtc;
      }

      public bool Exists { get; }

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

      public long Length { get; }

      public string Name { get; }
    }

    public class SplitResult
    {
      public readonly string DirectoryName;

      public readonly string FileName;

      // Nested types should not be visible
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