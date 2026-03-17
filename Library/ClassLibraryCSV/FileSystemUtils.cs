/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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

using System.Collections.Generic;


// ReSharper disable UseIndexFromEndExpression
// ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
// ReSharper disable ReplaceSubstringWithRangeIndexer


namespace CsvTools;

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

  private static readonly bool IsWindows =
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    OperatingSystem.IsWindows();
#else
  RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif

  /// <summary>
  /// Resolves a relative or patterned file path into a fully qualified absolute path.
  /// </summary>
  /// <param name="fileName">
  /// The file name, relative path, or pattern to resolve (e.g., "logs\app_%date%_*.log").
  /// If null or empty, an empty string is returned.
  /// </param>
  /// <param name="root">
  /// The base directory used to resolve <paramref name="fileName"/> if it is not already absolute.
  /// If null, the current working directory or executable directory is typically used (depending on <c>GetAbsolutePath</c> implementation).
  /// </param>
  /// <returns>
  /// A fully qualified absolute path with environment variables expanded and wildcards resolved 
  /// to the latest matching file. Returns <see cref="string.Empty"/> if the resolution fails.
  /// </returns>
  /// <remarks>
  /// This method performs three distinct steps:
  /// <list type="number">
  /// <item><description>Combines <paramref name="fileName"/> with <paramref name="root"/> to ensure a base anchor.</description></item>
  /// <item><description>Expands Environment Variables (e.g., %TEMP%) and custom placeholders (e.g., {date}).</description></item>
  /// <item><description>Evaluates wildcards (*, ?) to locate the most recent file matching the pattern.</description></item>
  /// </list>
  /// </remarks>
  public static string FullPath(this string? fileName, string? root) =>
    ResolvePattern(fileName.GetAbsolutePath(root)) ?? string.Empty;

  /// <summary>
  /// Ensures a directory exists by creating it and any missing parents. 
  /// Supports extended-length paths (beyond 260 characters).
  /// </summary>
  /// <param name="directoryName">
  /// The path of the directory to create. If <see langword="null"/> or empty, the method returns without action.
  /// </param>
  /// <remarks>
  /// This method uses <c>LongPathPrefix()</c> to bypass the legacy Windows <c>MAX_PATH</c> 
  /// limitation, allowing for directory structures deeper than 260 characters.
  /// </remarks>
  /// <exception cref="UnauthorizedAccessException">The caller does not have the required permission.</exception>
  /// <exception cref="IOException">The directory cannot be created (e.g., a file with the same name exists).</exception>
  public static void CreateDirectory(string? directoryName)
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
  public static void DeleteWithBackup(string fileName, bool multipleBackups)
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
        if (psStart != -1 && psEnd <= psStart + 5
          && int.TryParse(name.Substring(psStart + 1, psEnd - psStart - 1), out var num)
          && num > numBackup)
        {
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

  /// <summary>
  /// Check if a directory exists.
  /// </summary>
  /// <param name="directoryName">Name of the directory.</param>
  public static bool DirectoryExists(string? directoryName) =>
    !(directoryName is null || directoryName.Length == 0) && Directory.Exists(directoryName.LongPathPrefix());

  /// <summary>
  /// Retrieves the absolute path to the directory containing the application's executable or main assembly.
  /// </summary>
  /// <returns>
  /// The directory path. Falls back to <see cref="Directory.GetCurrentDirectory"/> 
  /// if the assembly location cannot be determined (e.g., in some unit test runners or single-file bundles).
  /// </returns>
  /// <remarks>
  /// This method attempts to find the location in the following order:
  /// <list type="number">
  /// <item><description>The Executing Assembly (the library containing this method).</description></item>
  /// <item><description>The Entry Assembly (the .exe or main process).</description></item>
  /// <item><description>The current working directory as a final fallback.</description></item>
  /// </list>
  /// It automatically handles long path prefixes via <c>LongPathPrefix()</c>.
  /// </remarks>
  public static string ExecutableDirectoryName()
  {
    // Try Entry Assembly first (usually what you want: the .exe or Test project)
    // Then try Executing Assembly (the DLL this code lives in)
    var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
    var location = assembly.Location;

    // In .NET Core/5+ Single-File apps, Location is empty. 
    // AppContext.BaseDirectory is the recommended modern way to get the app folder.
    if (string.IsNullOrEmpty(location))
    {
      return AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    var directory = Path.GetDirectoryName(location.LongPathPrefix());

    return string.IsNullOrEmpty(directory)
        ? Directory.GetCurrentDirectory()
        : directory;
  }

  /// <summary>
  ///   Deletes a file if it exists.
  /// </summary>
  /// <param name="fileName">Specify the file to be deleted</param>
  public static void FileDelete(string? fileName)
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
  public static bool FileExists(string? fileName) =>
    !(fileName is null || fileName.Length == 0) && File.Exists(fileName.LongPathPrefix());

  /// <summary>
  ///   Gets the absolute (rooted) path.
  /// </summary>
  /// <param name="fileName">Name of the file.</param>
  /// <param name="basePath">The base path.</param>
  /// <returns>The combined filename with the LongPathPrefix if necessary</returns>
  public static string GetAbsolutePath(this string? fileName, string? basePath = null)
  {
    if (string.IsNullOrWhiteSpace(fileName))
      return string.Empty;

    var expandedFileName = Environment.ExpandEnvironmentVariables(fileName).RemovePrefix();

    if (Path.IsPathRooted(expandedFileName))
      return Path.GetFullPath(expandedFileName);

    var adjustedBasePath = (basePath is null || basePath.Length == 0) ? "." : basePath;

    return GetFullPath(Path.Combine(adjustedBasePath, expandedFileName));
  }

  /// <summary>
  ///  Get the last file of a given pattern in a folder
  /// </summary>
  /// <param name="folder">The directory to look in</param>
  /// <param name="searchPattern">The pattern to look for</param>
  /// <returns>No matching file is found of the folder does not exist an empty string is returned</returns>
  public static string GetLatestFileOfPattern(string folder, string searchPattern)
  {
    if (string.IsNullOrEmpty(folder))
      folder = ".";
    else if (!DirectoryExists(folder))
      return string.Empty;

    // If a pattern is present in the folder this is not going to work
    var newSet = new DateTime(0, DateTimeKind.Utc);
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
  /// Replaces the directory portion of a file path with a placeholder if the file path starts with the specified directory.
  /// </summary>
  /// <param name="fileName">The full file path.</param>
  /// <param name="dir">The directory to replace.</param>
  /// <param name="placeHolder">The placeholder to use (e.g., ".", "%AppData%").</param>
  /// <returns>
  /// - <paramref name="placeHolder"/> if <paramref name="fileName"/> equals <paramref name="dir"/>.
  /// - A relative path using <paramref name="placeHolder"/> if <paramref name="fileName"/> starts with <paramref name="dir"/>.
  /// - Empty string if <paramref name="fileName"/> does not start with <paramref name="dir"/>.
  /// </returns>
  private static string UsePlaceHolder(string fileName, string dir, string placeHolder)
  {
    if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(dir))
      return string.Empty;

    // Normalize separators
    fileName = fileName.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
    dir = dir.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

    // Remove trailing separator from dir
    dir = dir.TrimEnd(Path.DirectorySeparatorChar);

    if (fileName.Equals(dir, StringComparison.OrdinalIgnoreCase))
      return placeHolder;

    if (!fileName.StartsWith(dir + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
      return string.Empty;

    var relativePart = fileName.Substring(dir.Length + 1);

    return string.Equals(placeHolder, ".", StringComparison.Ordinal) ? relativePart
      : placeHolder + Path.DirectorySeparatorChar + relativePart;
  }

  /// <summary>
  /// Overwrite absolute special folder paths in a fileName with placeholders.
  /// Example: C:\Users\Raphael\test.txt → %UserProfile%\test.txt
  /// </summary>
  public static string UseSpecialFolders(this string fileName)
  {
    if (!IsWindows)
      return fileName;

    var candidates = new List<string>();
    void TryAdd(Environment.SpecialFolder folder, string placeholder)
    {
      var replaced = UsePlaceHolder(fileName, Environment.GetFolderPath(folder), placeholder);
      if (replaced.Length > 0)
        candidates.Add(replaced);
    }

    TryAdd(Environment.SpecialFolder.DesktopDirectory, "%Desktop%");
    TryAdd(Environment.SpecialFolder.MyDocuments, "%Documents%");
    TryAdd(Environment.SpecialFolder.ApplicationData, "%AppData%");
    TryAdd(Environment.SpecialFolder.LocalApplicationData, "%LocalAppData%");
    TryAdd(Environment.SpecialFolder.UserProfile, "%UserProfile%");

    return candidates.Count == 0 ? fileName :
      // Return the shortest path replacement
      candidates.OrderBy(x => x.Length).First();
  }

  /// <summary>
  ///   Get a relative path to the file
  /// </summary>
  /// <param name="fileName">Name of the file.</param>
  /// <param name="basePath">The base path.</param>
  /// <returns>A relative path if possible</returns>
  public static string GetRelativePath(this string? fileName, string? basePath)
  {
    if (fileName is null || fileName.Length == 0)
      return string.Empty;


    // Expand environment variables and normalize
    fileName = Environment.ExpandEnvironmentVariables(fileName)
      .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
      .RemovePrefix();

    if (fileName.StartsWith(".", StringComparison.Ordinal)  || fileName.IndexOf(Path.DirectorySeparatorChar) == -1)
      return fileName;

    if (basePath is null || basePath.Length == 0)
      basePath = Directory.GetCurrentDirectory();
    else
      basePath = GetFullPath(basePath);

    var test = UsePlaceHolder(fileName, basePath, ".");
    if (test.Length > 0)
      return test;

    var parts = SplitPath(fileName);
    return GetRelativeFolder(parts.DirectoryName, basePath) + parts.FileName;
  }

  /// <summary>
  /// Calculates the relative path from a base directory to a target directory.
  /// </summary>
  /// <param name="otherDir">The target directory to which the relative path should point.</param>
  /// <param name="basePath">The starting directory used as the reference point.</param>
  /// <returns>
  /// A relative path string (e.g., "..\..\Folder\"). 
  /// Returns ".\" if the paths are identical.
  /// </returns>
  /// <remarks>
  /// This method performs a case-insensitive comparison of path segments.
  /// It is designed for directory-to-directory relativity and ensures the result 
  /// always concludes with a directory separator.
  /// </remarks>
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
    // Should end with \
    if (result[result.Length - 1] == Path.DirectorySeparatorChar)
      return result;
    sBuilder.Append(Path.DirectorySeparatorChar);
    return sBuilder.ToString();
  }

  /// <summary>
  /// Shortens a file path for display purposes by intelligently removing directory segments 
  /// or truncating the string to fit within a specified length.
  /// </summary>
  /// <param name="fileName">The full file path or resource name to shorten.</param>
  /// <param name="length">The maximum desired length of the resulting string. Defaults to 80.</param>
  /// <returns>
  /// A shortened string containing ellipses (…) where content was removed. 
  /// Returns the original string (sans prefix) if it already fits the length.
  /// </returns>
  /// <remarks>
  /// This method uses a multi-stage reduction strategy:
  /// <list type="number">
  /// <item><description>Retains the root and the last few directory levels.</description></item>
  /// <item><description>Progressively drops intermediate directories.</description></item>
  /// <item><description>Reduces to the filename only.</description></item>
  /// <item><description>Performs a "middle-out" character truncation if the filename alone is too long.</description></item>
  /// </list>
  /// </remarks>
  public static string GetShortDisplayFileName(this string fileName, int length = 80)
  {
    var ret = fileName.RemovePrefix();
    if (length <= 0 || string.IsNullOrEmpty(fileName) || fileName.Length <= length)
      return ret;
    var parts = fileName.Split([Path.DirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries);
    var fileNameOnly = parts[parts.Length - 1];

    // try to cut out directories
    if (parts.Length > 5)
      ret = $"{parts[0]}{Path.DirectorySeparatorChar}{parts[1]}{Path.DirectorySeparatorChar}…{Path.DirectorySeparatorChar}{parts[parts.Length - 3]}{Path.DirectorySeparatorChar}{parts[parts.Length - 2]}{Path.DirectorySeparatorChar}{fileNameOnly}";

    if (ret.Length > length && parts.Length > 3)
      ret = $"{parts[0]}{Path.DirectorySeparatorChar}…{Path.DirectorySeparatorChar}{parts[parts.Length - 2]}{Path.DirectorySeparatorChar}{fileNameOnly}";

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
  public static string SafePath(this string original, string replaceInvalid = "")
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
  /// Returns the most compact string representation of a path by comparing absolute, 
  /// relative, and special-folder (environment variable) formats.
  /// </summary>
  /// <param name="fileName">The path to be compressed.</param>
  /// <param name="basePath">The reference directory used to calculate the relative path.</param>
  /// <returns>
  /// The shortest available string representation. 
  /// Returns <see cref="string.Empty"/> if <paramref name="fileName"/> is null.
  /// </returns>
  /// <remarks>
  /// This method evaluates three formats and returns the one with the minimum character count:
  /// <list type="number">
  /// <item><description>The fully qualified absolute path.</description></item>
  /// <item><description>The path using special folders (e.g., "%AppData%\...").</description></item>
  /// <item><description>The relative path from <paramref name="basePath"/> (e.g., "..\data\...").</description></item>
  /// </list>
  /// </remarks>
  public static string GetShortestPath(this string? fileName, string? basePath)
  {
    if (fileName == null)
      return string.Empty;

    var absolute = fileName.GetAbsolutePath(basePath);

    var fileNameSpecial = UseSpecialFolders(absolute);
    if (fileNameSpecial.Length < absolute.Length)
      return fileNameSpecial;

    var relative = fileName.GetRelativePath(basePath);
    if (relative.Length < absolute.Length)
      return relative;

    return absolute;
  }

  /// <summary>Opens the file for writing</summary>
  /// <param name="fileName">Name of the file.</param>
  public static FileStream OpenWrite(string fileName) => File.OpenWrite(fileName.LongPathPrefix());

  /// <summary>
  /// Creates a file in a particular path.  If the file exists, it is replaced. The file is opened with ReadWrite access and cannot be opened by another application until it has been closed.  
  /// </summary>
  /// <param name="fileName">Name of the file.</param>
  /// <param name="bufferSize">Size of the buffer.</param>
  /// <param name="options">The options like RandomAccess or Asynchronous</param>    
  public static FileStream Create(string fileName, int bufferSize, in FileOptions options) =>
    File.Create(fileName.LongPathPrefix(), bufferSize, options);

  /// <summary>
  /// Writes all text into the file
  /// </summary>
  /// <param name="fileName">Name of the file.</param>
  /// <param name="contents">The contents.</param>
  public static void WriteAllText(string fileName, string contents) =>
    File.WriteAllText(fileName.LongPathPrefix(), contents);

  /// <summary>
  /// Writes all text to the given file
  /// </summary>
  /// <param name="fileName">Name of the file.</param>
  /// <param name="contents">The contents.</param>
  /// <param name="encoding">The encoding to be used</param>
  public static void WriteAllText(string fileName, string contents, in Encoding encoding) =>
    File.WriteAllText(fileName.LongPathPrefix(), contents, encoding);

  [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "GetShortPathNameW", SetLastError = true)]
  private static extern int GetShortPathName(string pathName, StringBuilder shortName, uint cbShortName);

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
/// Asynchronously creates a new file, writes the specified string to it, and then closes the file. 
/// Supports extended-length paths (beyond 260 characters).
/// </summary>
/// <param name="fileName">The path of the file to write to.</param>
/// <param name="contents">The string content to write to the file.</param>
/// <param name="cancellationToken">A token that can be used to cancel the write operation.</param>
/// <returns>A task that represents the asynchronous write operation.</returns>
/// <remarks>
/// This method uses <c>LongPathPrefix()</c> to ensure the file can be written even if the 
/// directory structure exceeds the Windows <c>MAX_PATH</c> limitation. If the target file 
/// already exists, it is overwritten.
/// </remarks>
    public static System.Threading.Tasks.Task WriteAllTextAsync(string fileName, string contents, System.Threading.CancellationToken cancellationToken) =>
      File.WriteAllTextAsync(fileName.LongPathPrefix(), contents, cancellationToken);
#endif

  /// <summary>
  /// Returns the absolute path for the specified path string, with support for paths exceeding 260 characters.
  /// </summary>
  /// <param name="path">The file or directory path to resolve.</param>
  /// <returns>
  /// The fully qualified absolute path, normalized and stripped of any internal long-path prefixes (e.g., \\?\).
  /// </returns>
  /// <remarks>
  /// This method temporarily applies a long-path prefix to bypass Windows <c>MAX_PATH</c> limitations 
  /// during resolution, but returns a "clean" path suitable for UI display or standard API usage.
  /// </remarks>
  /// <exception cref="ArgumentException"><paramref name="path"/> contains invalid characters.</exception>
  public static string GetFullPath(string path) => Path.GetFullPath(path.LongPathPrefix()).RemovePrefix();

  /// <summary>
  /// Opens the file for reading
  /// </summary>
  /// <param name="fileName">Name of the file.</param>    
  public static FileStream OpenRead(string fileName) => File.OpenRead(fileName.LongPathPrefix());

  /// <summary>
  /// Writes all bytes of content to the file
  /// </summary>
  /// <param name="fileName">Name of the file.</param>
  /// <param name="contents">The contents to be written.</param>
  public static void WriteAllBytes(string fileName, in byte[] contents) =>
    File.WriteAllBytes(fileName.LongPathPrefix(), contents);

  /// <summary>
  /// Gets the stream reader for a file or resource.
  /// </summary>
  /// <param name="resourceName">The name of the resource</param>  
  /// <exception cref="System.ArgumentException">Could not locate stream</exception>
  public static StreamReader GetStreamReaderForFileOrResource(string resourceName)
  {
    var fileName = Path.Combine(ExecutableDirectoryName(), resourceName);

    if (FileExists(fileName))
      return new StreamReader(OpenRead(fileName), detectEncodingFromByteOrderMarks: true);

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

    throw new FileNotFoundException(
        $"Could not locate '{resourceName}' as a physical file or an embedded resource.",
        fileName);
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
      return !shortPath.Contains("." + Path.DirectorySeparatorChar) ? shortPath : Path.GetFullPath(shortPath);
    else
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
  /// Removes the Windows long path prefix from a file path if present.
  /// </summary>
  /// <remarks>
  /// Windows uses special prefixes (e.g., "\\?\" or "\\?\UNC\") to support paths longer than 248 characters.
  /// This method strips those prefixes so the path can be used with standard .NET APIs.
  /// </remarks>
  /// <param name="path">The possibly prefixed file path.</param>
  /// <returns>The path without the long path prefix.</returns>
  public static string RemovePrefix(this string path)
  {
    if (!IsWindows)
      return path;

    // Local long path prefix: \\?\
    if (path.StartsWith(cLongPathPrefix, StringComparison.Ordinal))
      return path.Substring(cLongPathPrefix.Length);

    // UNC long path prefix: \\?\UNC\
    if (path.StartsWith(cUncLongPathPrefix, StringComparison.OrdinalIgnoreCase))
      return @"\\" + path.Substring(cUncLongPathPrefix.Length);

    return path;
  }

  /// <summary>
  /// Resolves a path by expanding environment variables, replacing custom placeholders, 
  /// and evaluating wildcard patterns to find the most recent matching file.
  /// </summary>
  /// <param name="fileName">The path or pattern to resolve (e.g., "%AppData%\Log_{date}_*.txt").</param>
  /// <returns>
  /// The path to the latest matching file if wildcards are present; 
  /// otherwise, the expanded string. Returns <see cref="string.Empty"/> if input is null.
  /// </returns>
  /// <remarks>
  /// Supported placeholders:
  /// <list type="bullet">
  /// <item><description><c>{date}</c>: Local date in yyyy-MM-dd format.</description></item>
  /// <item><description><c>{utc}</c>: UTC date in yyyy-MM-dd format.</description></item>
  /// </list>
  /// Standard environment variables (e.g., %TEMP%) are also expanded.
  /// </remarks>
  public static string ResolvePattern(this string fileName)
  {
    if (fileName is null || fileName.Length == 0)
      return string.Empty;

    // expand %AppData%, %LOCALAPPDATA% or %USERPROFILE% and alike
    var withoutPlaceHolder = Environment.ExpandEnvironmentVariables(fileName
        .PlaceholderReplace("date", DateTime.Now.ToString(CultureInfo.CurrentCulture))
        .PlaceholderReplace("utc", DateTime.UtcNow.ToString(CultureInfo.CurrentCulture))
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
  public static string GetFileName(this string? path)
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
  public static StreamWriter CreateText(string path) => File.CreateText(path.LongPathPrefix());

  /// <summary>
  /// Reads all text for a file
  /// </summary>
  /// <param name="path">The path to the file.</param>
  public static string ReadAllText(string path) => File.ReadAllText(path.LongPathPrefix());

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Reads all text for a file
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    public static System.Threading.Tasks.Task<string> ReadAllTextAsync(string path, System.Threading.CancellationToken cancellationToken)
      => File.ReadAllTextAsync(path.LongPathPrefix(), cancellationToken);
#endif


  /// <summary>
  /// Splits the full or relative file path into its directory and filename components.
  /// </summary>
  /// <param name="path">The full or relative path. May include environment variables or Windows long path prefixes.</param>
  /// <returns>
  /// A <see cref="SplitResult"/> containing:
  /// - <see cref="SplitResult.DirectoryName"/>: The directory portion without a trailing separator.
  /// - <see cref="SplitResult.FileName"/>: The file name portion (including extension).
  /// </returns>
  /// <remarks>
  /// - If <paramref name="path"/> is null or empty, both directory and filename are empty strings.
  /// - Environment variables (e.g., %USERPROFILE%) are expanded.
  /// - Relative paths are converted to absolute paths using <see cref="GetFullPath"/>.
  /// - Directory separators are normalized to <see cref="Path.DirectorySeparatorChar"/>.
  /// - Windows long path prefixes (e.g., "\\?\" or "\\?\UNC\") are removed.
  /// - DirectoryName never ends with a separator.
  /// </remarks>
  public static SplitResult SplitPath(string? path)
  {
    if (string.IsNullOrEmpty(path))
      return new SplitResult(string.Empty, string.Empty);

    // Expand environment variables first
    path = Environment.ExpandEnvironmentVariables(path);

    // Normalize separators
    path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

    // Remove Windows long path prefix if present
    path = path.RemovePrefix();

    // Convert relative paths to absolute
    try
    {
      if (path.IndexOf(Path.DirectorySeparatorChar) !=-1 && !Path.IsPathRooted(path))
        path = Path.GetFullPath(path);
    }
    catch
    {
      // Ignore GetFullPath might fail due to invalid char
    }
    var lastIndex = path.LastIndexOf(Path.DirectorySeparatorChar);

    return lastIndex != -1
      ? new SplitResult(path.Substring(0, lastIndex), path.Substring(lastIndex + 1))
      : new SplitResult(string.Empty, path);
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
      m_Info = new System.IO.FileInfo(fileName.LongPathPrefix());
      if (!m_Info.Exists)
        return;

      Exists = true;
      Length = m_Info.Length;
      m_LastWriteTimeUtc = m_Info.LastWriteTimeUtc;
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
  /// Represents the result of splitting a file path into directory and filename components.
  /// Provides convenient access to the file extension and name without extension.
  /// </summary>
  public class SplitResult
  {
    /// <summary>
    /// The directory portion of the path, without a trailing separator.
    /// </summary>
    public readonly string DirectoryName;


    /// <summary>
    /// The file name portion of the path, including its extension if present.
    /// </summary>
    public readonly string FileName;


    /// <summary>
    /// Initializes a new instance of the <see cref="SplitResult"/> class.
    /// </summary>
    /// <param name="dir">The directory path without trailing separator.</param>
    /// <param name="file">The file name with extension.</param>
    public SplitResult(string dir, string file)
    {
      DirectoryName = dir;
      FileName = file;
    }


    /// <summary>
    /// Gets the file extension (including the dot), or an empty string if none exists.
    /// Example: "Test.docx" → ".docx"
    /// </summary>
    public string Extension
    {
      get
      {
        var index = FileName.LastIndexOf('.');
        return index == -1 ? string.Empty : FileName.Substring(index);
      }
    }


    /// <summary>
    /// Gets the file name without its extension.
    /// Example: "Test.docx" → "Test"
    /// </summary>
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