﻿/*
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
#nullable enable

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using System.Linq;


// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace CsvTools
{
  /// <summary>
  ///   Helper class
  /// </summary>
  // ReSharper disable once HollowTypeName
  public static class CsvHelper
  {
    private static readonly IReadOnlyCollection<char> m_QualifiersToTest = new[] { '"', '\'' };

    /// <summary>
    ///   Refreshes the settings assuming the file has changed, checks CodePage, Delimiter, Start
    ///   Row and Header
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="identifierInContainer"></param>
    /// <param name="guessJson">if true trying to determine if file is a JSOn file</param>
    /// <param name="guessCodePage">if true, try to determine the code page</param>
    /// <param name="guessEscapePrefix">if <c>true</c>, try to determine the escape sequence</param>
    /// <param name="guessDelimiter">if true, try to determine the delimiter</param>
    /// <param name="guessQualifier">if true, try to determine the qualifier for text</param>
    /// <param name="guessStartRow">if true, try to determine the number of skipped rows</param>
    /// <param name="guessHasHeader">
    ///   if true, try to determine if the file does have a header row
    /// </param>
    /// <param name="guessNewLine">if true, try to determine what kind of new line we do use</param>
    /// <param name="guessCommentLine"></param>
    /// <param name="inspectionResult">Default in case inspection is wanted</param>
    /// <param name="fillGuessSettings"></param>
    /// <param name="pgpKey">Private PGP key in case reading an encrypted file</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">file name can not be empty - fileName</exception>
    public static async Task<InspectionResult> GetInspectionResultFromFileAsync(this string fileName,
      string identifierInContainer,
      bool guessJson, bool guessCodePage, bool guessEscapePrefix,
      bool guessDelimiter, bool guessQualifier,
      bool guessStartRow, bool guessHasHeader,
      bool guessNewLine, bool guessCommentLine, InspectionResult inspectionResult,
      FillGuessSettings fillGuessSettings, string pgpKey, CancellationToken cancellationToken)
    {
      if (string.IsNullOrEmpty(fileName))
        throw new ArgumentException("File name can not be empty", nameof(fileName));

      inspectionResult.FileName = fileName;
      var sourceAccess = new SourceAccess(fileName, pgpKey: pgpKey, identifierInContainer: identifierInContainer);

      inspectionResult.IdentifierInContainer = sourceAccess.IdentifierInContainer;
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      using var usedStream = await GetStreamInMemoryAsync(sourceAccess, cancellationToken).ConfigureAwait(false);
      var disallowedDelimiter = new List<char>();

      var delimiterByExtension = DetectionDelimiter.GetDelimiterByExtension(!string.IsNullOrEmpty(identifierInContainer) ? identifierInContainer : fileName);

      do
      {
        // IImprovedStream And MemoryStream do handle this properly
        usedStream.Seek(0, SeekOrigin.Begin);

        // Determine from file
        await usedStream.UpdateInspectionResultAsync(inspectionResult, guessJson,
          guessCodePage, guessEscapePrefix, guessDelimiter, guessQualifier,
          guessStartRow, guessHasHeader, guessNewLine, guessCommentLine, delimiterByExtension,
          disallowedDelimiter, cancellationToken).ConfigureAwait(false);

        // if it's a delimited file, but we do not have fields,
        // the delimiter must have been wrong, pick another one, after 3 though give up

        if (!inspectionResult.IsJson && !inspectionResult.IsXml)
        {
          try { Logger.Information("Checking field delimiter"); } catch { }
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
          await
#endif
          using var reader = GetFileReader(inspectionResult, usedStream);
          await reader.OpenAsync(cancellationToken).ConfigureAwait(false);
          if (reader.FieldCount <= 1)
          {
            try
            {
              Logger.Information(
              $"Found field delimiter {inspectionResult.FieldDelimiter} is not valid, checking for an alternative");
            }
            catch { }
            disallowedDelimiter.Add(inspectionResult.FieldDelimiter);
            // in case Delimiter is re-checked some test are less important
            guessCodePage = false;
            guessCommentLine = false;
            guessNewLine = false;
          }
          else
            break;
        }

        // no need to check for Json again
        guessJson = false;
      } while (!inspectionResult.IsJson && !inspectionResult.IsXml && disallowedDelimiter.Count < 3);

      // in case there was a problem with the disallowed delimiters
      if (disallowedDelimiter.Count == 3)
      {
        // assume the very first was correct
        inspectionResult.FieldDelimiter = disallowedDelimiter[0];
        // and rerun detection
        await usedStream.UpdateInspectionResultAsync(inspectionResult, guessJson,
          guessCodePage, guessEscapePrefix, guessDelimiter, guessQualifier,
          guessStartRow, guessHasHeader, guessNewLine, guessCommentLine, delimiterByExtension,
          Array.Empty<char>(), cancellationToken).ConfigureAwait(false);
      }

      if (fillGuessSettings.Enabled)
      {
        try { Logger.Information("Determining column format by reading samples"); } catch { }
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        await
#endif
        using var reader2 = GetFileReader(inspectionResult, usedStream);
        await reader2.OpenAsync(cancellationToken).ConfigureAwait(false);
        var (_, b) = await reader2.FillGuessColumnFormatReaderAsyncReader(
          fillGuessSettings, columnCollectionInput: null,
          addTextColumns: false, checkDoubleToBeInteger: true, treatTextAsNull: string.Empty,
          cancellationToken).ConfigureAwait(false);
        inspectionResult.Columns.AddRangeNoClone(b);
      }

      return inspectionResult;
    }

    /// <summary>
    /// Get a stream for the source access
    /// </summary>
    /// <param name="sourceAccess">The access information like filename or file type</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Stream> GetStreamInMemoryAsync(this SourceAccess sourceAccess,
      CancellationToken cancellationToken)
    {
      // even tough the definition reads it will return a Stream all implementation do return IImprovedStream
      var stream = FunctionalDI.GetStream(sourceAccess);
      try
      {
        // if the file is very big, do not take part of it we might lose too much information
        if (stream.Length > 268435456)
          return stream;

        // 2^26 Byte max Capacity : 64 MByte
        int capacity = Math.Min(stream.Length.ToInt(), 67108864);
        var memoryStream = new MemoryStream(capacity);
        await stream.CopyToAsync(memoryStream, capacity, cancellationToken).ConfigureAwait(false);
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        await stream.DisposeAsync();
#else
        stream.Dispose();
#endif
        return memoryStream;
      }
      catch (OutOfMemoryException)
      {
        return stream;
      }
    }

    /// <summary>
    /// Get a text reader form a stream, takes care of codePage and skip rows
    /// </summary>
    /// <param name="stream">The open read stream</param>
    /// <param name="codePageId">The encoding code page, if 0 the cope page is inspected</param>
    /// <param name="skipRows">The number of rows at the start of the stream to skip</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>A <see cref="ImprovedTextReader"/> that allows <see cref="ImprovedTextReaderPositionStore"/></returns>
    public static async Task<ImprovedTextReader> GetTextReaderAsync(this Stream stream, int codePageId, int skipRows,
        CancellationToken cancellationToken)
      => new ImprovedTextReader(stream,
        await stream.InspectCodePageAsync(codePageId, cancellationToken).ConfigureAwait(false), skipRows);

    /// <summary>
    /// Asynchronously inspects the given <see cref="Stream"/> to determine its most likely text encoding.
    /// </summary>
    /// <param name="stream">
    /// The input <see cref="Stream"/> from which to read the initial bytes for encoding detection.
    /// The stream must support reading.
    /// </param>
    /// <param name="token">
    /// A <see cref="CancellationToken"/> that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    ///   <item><description><c>codePage</c> – the detected encoding’s code page number.</description></item>
    ///   <item><description><c>bom</c> – <c>true</c> if a byte-order mark (BOM) was detected; otherwise <c>false</c>.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// The method reads up to 256 KB from the start of the stream (or less if the stream is smaller)
    /// to identify the encoding using BOM or heuristic detection.
    /// </para>
    /// <para>
    /// If the stream begins with a BOM, the corresponding encoding is returned immediately.
    /// Otherwise, <see cref="EncodingHelper.DetectEncodingNoBom(byte[])"/> is used to guess
    /// the most appropriate encoding, defaulting to UTF-8 for ASCII results.
    /// </para>
    /// <para>
    /// The stream’s position is reset to the beginning if it supports seeking.
    /// </para>
    /// </remarks>
    public static async Task<(int codePage, bool bom)> InspectCodePageAsync(this Stream stream, CancellationToken token)
    {
      if (stream == null)
        throw new ArgumentNullException(nameof(stream));
      if (!stream.CanRead)
        throw new ArgumentException("Stream must be readable.", nameof(stream));

      // Determine max read length (up to 256 KB)
      int maxLength = (stream is FileStream fs)
          ? (int) Math.Min(fs.Length, 262_144)
          : 262_144;

      byte[] buffer = new byte[maxLength];

      // Read initial bytes (up to buffer size)
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      int bytesRead = await stream.ReadAsync(buffer.AsMemory(0, Math.Min(4, buffer.Length)), token).ConfigureAwait(false);
#else
      int bytesRead = await stream.ReadAsync(buffer, 0, Math.Min(4, buffer.Length), token).ConfigureAwait(false);
#endif

      // Check for BOM-based encoding
      if (bytesRead >= 2)
      {
        var byBom = EncodingHelper.GetEncodingByByteOrderMark(buffer, Math.Min(4, bytesRead));
        if (byBom != null)
        {
          try
          {
            Logger.Information($"Detected encoding by BOM: {EncodingHelper.GetEncodingName(byBom, true)}");
          }
          catch { /* ignore logging errors */ }

          if (stream.CanSeek)
            stream.Seek(0, SeekOrigin.Begin);

          return (byBom.CodePage, true);
        }
      }

      // Read more for non-BOM detection if needed
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      int additionalBytes = await stream.ReadAsync(buffer.AsMemory(bytesRead, maxLength - bytesRead), token).ConfigureAwait(false);
#else
      int additionalBytes = await stream.ReadAsync(buffer, bytesRead, maxLength - bytesRead, token).ConfigureAwait(false);
#endif

      int totalRead = bytesRead + additionalBytes;

      // Detect encoding heuristically
      var detected = EncodingHelper.DetectEncodingNoBom(buffer.AsSpan(0, totalRead).ToArray());
      if (detected.Equals(Encoding.ASCII))
        detected = Encoding.UTF8;

      try
      {
        Logger.Information($"Detected encoding (no BOM): {EncodingHelper.GetEncodingName(detected, false)}");
      }
      catch { /* ignore logging errors */ }

      if (stream.CanSeek)
        stream.Seek(0, SeekOrigin.Begin);

      return (detected.CodePage, false);
    }


    /// <summary>Analyzes a given the file asynchronously to determine proper read options</summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="guessJson">if <c>true</c> trying to determine if file is a JSON file</param>
    /// <param name="guessCodePage">if <c>true</c>, try to determine the code page</param>
    /// <param name="guessEscapePrefix">if <c>true</c>, try to determine the escape sequence</param>
    /// <param name="guessDelimiter">if <c>true</c>, try to determine the delimiter</param>
    /// <param name="guessQualifier">if <c>true</c>, try to determine the qualifier for text</param>
    /// <param name="guessStartRow">if <c>true</c>, try to determine the number of skipped rows</param>
    /// <param name="guessHasHeader">if true, try to determine if the file does have a header row</param>
    /// <param name="guessNewLine">if set to <c>true</c> determine combination of new line.</param>
    /// <param name="guessCommentLine"></param>
    /// <param name="fillGuessSettings">The fill guess settings.</param>
    /// <param name="selectFile">Function to be called if a file needs to be picked</param>
    /// <param name="defaultInspectionResult">Defaults in case some inspection are not wanted</param>
    /// <param name="privateKey"></param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long-running process</param>
    /// <returns>
    ///   <see cref="InspectionResult" /> with found information, or default if that test was not done
    /// </returns>
    public static async Task<InspectionResult> InspectFileAsync(
      this string fileName, bool guessJson,
      bool guessCodePage, bool guessEscapePrefix,
      bool guessDelimiter, bool guessQualifier, bool guessStartRow,
      bool guessHasHeader, bool guessNewLine, bool guessCommentLine,
      FillGuessSettings fillGuessSettings, Func<IReadOnlyCollection<string>, string>? selectFile,
      InspectionResult defaultInspectionResult, string privateKey, CancellationToken cancellationToken)
    {
      if (string.IsNullOrEmpty(fileName))
        throw new ArgumentException("Argument can not be empty", nameof(fileName));

      if (fileName.IndexOf('~') != -1)
        fileName = fileName.LongFileName();

      var fileName2 = FileSystemUtils.ResolvePattern(fileName);
      if (fileName2 is null)
        throw new FileNotFoundException(fileName);

      var fileInfo = new FileSystemUtils.FileInfo(fileName2);

      try { Logger.Information("Examining file {filename}", fileName2.GetShortDisplayFileName(40)); } catch { }
      try { Logger.Information($"Size of file: {StringConversion.DynamicStorageSize(fileInfo.Length)}"); } catch { }
      var selectedFile = string.Empty;

      // load from Setting file
      if (fileName2.EndsWith(SerializedFilesLib.cSettingExtension, StringComparison.OrdinalIgnoreCase)
          || FileSystemUtils.FileExists(fileName2 + SerializedFilesLib.cSettingExtension))
      {
        var fileNameSetting =
          !fileName2.EndsWith(SerializedFilesLib.cSettingExtension, StringComparison.OrdinalIgnoreCase)
            ? fileName2 + SerializedFilesLib.cSettingExtension
            : fileName2;
        try
        {
          // we defiantly have an extension with the name
          var inspectionResult = await fileNameSetting.DeserializeFileAsync<InspectionResult>().ConfigureAwait(false);
          try { Logger.Information("Configuration read from setting file {filename}", fileNameSetting.GetShortDisplayFileName(40)); } catch { }
          return inspectionResult;
        }
        catch (Exception e)
        {
          try { Logger.Warning(e, "Could not parse setting file {filename}", fileNameSetting.GetShortDisplayFileName(40)); } catch { }
        }
      }

      if (fileName2.AssumeZip())
      {
        var setting = await ManifestData.ReadManifestZip(fileName2).ConfigureAwait(false);
        if (!(setting is null))
        {
          try { Logger.Information("Data in zip {filename}", setting.IdentifierInContainer); } catch { }
          return setting;
        }

        using var zipFile = new ZipFile(fileName2);
        var filesInZip = ImprovedStream.SuitableZipEntries(zipFile).Select(x => x.Name);
        selectedFile = selectFile?.Invoke(filesInZip.ToList()) ?? filesInZip.FirstOrDefault();
        if (selectedFile is null)
          throw new FileNotFoundException("No suitable file found in the ZIP archive.");
      }

      if (fileName2.EndsWith(ManifestData.cCsvManifestExtension, StringComparison.OrdinalIgnoreCase))
        try
        {
          var settingFs = await ManifestData.ReadManifestFileSystem(fileName2).ConfigureAwait(false);
          try { Logger.Information("Data in {filename}", settingFs.FileName); } catch { }
          return settingFs;
        }
        catch (FileNotFoundException e2)
        {
          try { Logger.Information(e2, "Trying to read manifest"); } catch { }
        }

      // Determine from file
      return await GetInspectionResultFromFileAsync(fileName2, selectedFile, guessJson, guessCodePage,
        guessEscapePrefix, guessDelimiter,
        guessQualifier, guessStartRow, guessHasHeader, guessNewLine, guessCommentLine,
        defaultInspectionResult, fillGuessSettings, privateKey, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///   Determines whether data in the specified stream is an XML
    /// </summary>
    /// <param name="stream">The stream to read data from</param>
    /// <param name="encoding">The encoding.</param>
    /// <returns><c>true</c> if XML could be read from stream; otherwise, <c>false</c>.</returns>
    public static async Task<bool> InspectIsXmlReadableAsync(
      this Stream stream,
      Encoding encoding)
    {
      stream.Seek(0, SeekOrigin.Begin);
      using var streamReader = new StreamReader(stream, encoding, true, 4096, true);
      try
      {
        using var xmlReader =
          System.Xml.XmlReader.Create(streamReader, new System.Xml.XmlReaderSettings { Async = true });
        await xmlReader.MoveToContentAsync().ConfigureAwait(false);
        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }

    /// <summary>
    ///   Determines whether data in the specified stream is a JSON
    /// </summary>
    /// <param name="stream">The stream to read data from</param>
    /// <param name="encoding">The encoding.</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns><c>true</c> if json could be read from stream; otherwise, <c>false</c>.</returns>
    public static async Task<bool> InspectIsJsonReadableAsync(
      this Stream stream,
      Encoding encoding,
      CancellationToken cancellationToken)
    {
      stream.Seek(0, SeekOrigin.Begin);
      using var streamReader = new StreamReader(stream, encoding, true, 4096, true);
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      using var jsonTextReader = new JsonTextReader(streamReader);
      jsonTextReader.CloseInput = false;
      try
      {
        if (await jsonTextReader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
          // ReSharper disable once MergeIntoLogicalPattern
          if (jsonTextReader.TokenType == JsonToken.StartObject || jsonTextReader.TokenType == JsonToken.StartArray
                                                                || jsonTextReader.TokenType
                                                                == JsonToken.StartConstructor)
          {
            await jsonTextReader.ReadAsync(cancellationToken).ConfigureAwait(false);
            await jsonTextReader.ReadAsync(cancellationToken).ConfigureAwait(false);
            try { Logger.Information("Detected Json file"); } catch { }
            return true;
          }
        }
      }
      catch (JsonReaderException)
      {
        //ignore
      }

      return false;
    }

    /// <summary>
    ///   Updates the InspectionResult from stream.
    /// </summary>
    /// <param name="stream">The stream to read data from</param>
    /// <param name="inspectionResult">Passed is detection result</param>
    /// <param name="guessJson">if <c>true</c> trying to determine if file is a JSON file</param>
    /// <param name="guessCodePage">if <c>true</c>, try to determine the code page</param>
    /// <param name="guessEscapePrefix">if <c>true</c>, try to determine the escape sequence</param>
    /// <param name="guessDelimiter">if <c>true</c>, try to determine the delimiter</param>
    /// <param name="guessQualifier">if <c>true</c>, try to determine the qualifier for text</param>
    /// <param name="guessStartRow">if <c>true</c>, try to determine the number of skipped rows</param>
    /// <param name="guessHasHeader">
    ///   if true, try to determine if the file does have a header row
    /// </param>
    /// <param name="guessNewLine">if set to <c>true</c> determine combination of new line.</param>
    /// <param name="guessCommentLine">if set <c>true</c> determine if there is a comment line</param>
    /// <param name="probableDelimiter">Give this delimiter a higher score, commonly derived from file extension</param>
    /// <param name="disallowedDelimiter">Delimiter to exclude in recognition, as they have been ruled out before</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    public static async Task UpdateInspectionResultAsync(this Stream stream,
      InspectionResult inspectionResult,
      bool guessJson,
      bool guessCodePage,
      bool guessEscapePrefix,
      bool guessDelimiter,
      bool guessQualifier,
      bool guessStartRow,
      bool guessHasHeader,
      bool guessNewLine,
      bool guessCommentLine,
      char probableDelimiter,
      IReadOnlyCollection<char> disallowedDelimiter,
      CancellationToken cancellationToken)
    {
      if (stream is null)
        throw new ArgumentNullException(nameof(stream));

      if (!(guessJson || guessCodePage || guessDelimiter || guessStartRow || guessQualifier || guessHasHeader ||
            guessCommentLine || guessNewLine))
        return;
      /*
       *  We have cyclic dependencies, so test are possibly repeated to get a better result:
FieldDelimiter
  CommentLine
  FieldQualifier
  EscapePrefix

FieldQualifier
  EscapePrefix
  FieldDelimiter
  NewLine

EscapePrefix
  SkipRows
  FieldDelimiter
  HasHeader

CommentLine
  HasHeader
  SkipRows
       */
      if (guessCodePage)
      {
        cancellationToken.ThrowIfCancellationRequested();

        stream.Seek(0, SeekOrigin.Begin);
        try { Logger.Information("Checking Code Page"); } catch { }
        var (codePage, bom) = await stream.InspectCodePageAsync(cancellationToken).ConfigureAwait(false);
        inspectionResult.CodePageId = codePage;
        inspectionResult.ByteOrderMark = bom;
      }

      if (guessJson)
      {
        cancellationToken.ThrowIfCancellationRequested();
        try { Logger.Information("Checking XML format"); } catch { }
        inspectionResult.IsXml = await stream
          .InspectIsXmlReadableAsync(Encoding.GetEncoding(inspectionResult.CodePageId)).ConfigureAwait(false);


        if (inspectionResult.IsXml)
        {
          try { Logger.Information("Detected XML file, no further checks done"); } catch { }
          return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        try { Logger.Information("Checking Json format"); } catch { }
        inspectionResult.IsJson = await stream
          .InspectIsJsonReadableAsync(Encoding.GetEncoding(inspectionResult.CodePageId), cancellationToken)
          .ConfigureAwait(false);

        if (inspectionResult.IsJson)
        {
          try { Logger.Information("Detected Json file, no further checks done"); } catch { }
          return;
        }
      }

      // --- Initialize loop for dependent properties ---
      const int maxAttempts = 5;
      int attempt = 0;
      bool retry;
      do
      {
        attempt++;
        retry = false;

        if (guessStartRow)
          inspectionResult.SkipRows = 0;

        // --- Comment Line ---
        if (guessCommentLine)
        {
          cancellationToken.ThrowIfCancellationRequested();
          try { Logger.Information("Checking comment line"); } catch { }
          using var textReader = await stream
              .GetTextReaderAsync(inspectionResult.CodePageId, inspectionResult.SkipRows, cancellationToken)
              .ConfigureAwait(false);
          inspectionResult.CommentLine = await textReader.InspectLineCommentAsync(cancellationToken)
              .ConfigureAwait(false);
        }

        // --- Escape Prefix ---
        var newPrefix = inspectionResult.EscapePrefix;
        if (guessEscapePrefix)
        {
          try { Logger.Information("Checking Escape Prefix"); } catch { }
          using var textReader = await stream
              .GetTextReaderAsync(inspectionResult.CodePageId, inspectionResult.SkipRows, cancellationToken)
              .ConfigureAwait(false);
          newPrefix = await textReader.InspectEscapePrefixAsync(
              inspectionResult.FieldDelimiter, inspectionResult.FieldQualifier, cancellationToken)
              .ConfigureAwait(false);
        }

        // --- Delimiter / Qualifier / NewLine ---
        bool changedDelimiter = false;
        bool changedFieldQualifier = false;
        bool changedSkipRows = false;

        if (guessQualifier || guessDelimiter || guessNewLine)
        {
          using var textReader = await stream
              .GetTextReaderAsync(inspectionResult.CodePageId, inspectionResult.SkipRows, cancellationToken)
              .ConfigureAwait(false);

          // Qualifier
          if (guessQualifier)
          {
            cancellationToken.ThrowIfCancellationRequested();
            try { Logger.Information("Checking Qualifier"); } catch { }
            var qualifierResult = textReader.InspectQualifier(
                inspectionResult.FieldDelimiter, newPrefix, inspectionResult.CommentLine,
                m_QualifiersToTest, cancellationToken);
            changedFieldQualifier = inspectionResult.FieldQualifier != qualifierResult.QuoteChar;
            inspectionResult.FieldQualifier = qualifierResult.QuoteChar;
            inspectionResult.ContextSensitiveQualifier =
                !(qualifierResult.DuplicateQualifier || qualifierResult.EscapedQualifier);
            inspectionResult.DuplicateQualifierToEscape = qualifierResult.DuplicateQualifier;

            if (inspectionResult.DuplicateQualifierToEscape)
              newPrefix = char.MinValue;
          }

          // Delimiter
          if (guessDelimiter)
          {
            cancellationToken.ThrowIfCancellationRequested();
            try { Logger.Information("Checking Column Delimiter"); } catch { }
            var delimiterResult = await textReader.InspectDelimiterAsync(
                inspectionResult.FieldQualifier, newPrefix, disallowedDelimiter, probableDelimiter,
                cancellationToken).ConfigureAwait(false);
            if (delimiterResult.MagicKeyword)
              inspectionResult.SkipRows++;

            changedDelimiter = inspectionResult.FieldDelimiter != delimiterResult.Delimiter;
            inspectionResult.FieldDelimiter = delimiterResult.Delimiter;
            inspectionResult.NoDelimitedFile = delimiterResult.IsDetected;
          }

          // NewLine
          if (guessNewLine)
          {
            cancellationToken.ThrowIfCancellationRequested();
            try { Logger.Information("Checking Record Delimiter"); } catch { }

            stream.Seek(0, SeekOrigin.Begin);
            inspectionResult.NewLine = textReader.InspectRecordDelimiter(
                inspectionResult.FieldQualifier, cancellationToken);
          }
        }

        inspectionResult.EscapePrefix = newPrefix;

        // --- Retry conditions ---
        if (guessEscapePrefix && (changedDelimiter || changedFieldQualifier) && attempt < maxAttempts)
          retry = true;

        // --- StartRow Recheck ---
        if (guessStartRow)
        {
          cancellationToken.ThrowIfCancellationRequested();
          try { Logger.Information("Checking Start line"); } catch { }

          using var textReader = await stream.GetTextReaderAsync(inspectionResult.CodePageId, 0, cancellationToken)
              .ConfigureAwait(false);
          var newSkipRows = textReader.InspectStartRow(
              inspectionResult.FieldDelimiter, inspectionResult.FieldQualifier, inspectionResult.EscapePrefix,
              inspectionResult.CommentLine, cancellationToken);
          changedSkipRows = inspectionResult.SkipRows != newSkipRows;
          inspectionResult.SkipRows = newSkipRows;
        }

        if ((guessEscapePrefix || guessQualifier || guessDelimiter || guessCommentLine) &&
            (changedSkipRows || (inspectionResult.EscapePrefix != newPrefix && newPrefix != '\0') ||
             changedFieldQualifier) && attempt < maxAttempts)
        {
          retry = true;
        }

      } while (retry);
    }

    /// <summary>
    /// Read a CsfFile to check wither the settings are fine
    /// </summary>
    /// <param name="csvFile"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task InspectReadCsvAsync(this ICsvFile csvFile, CancellationToken cancellationToken)
    {
      var det = await csvFile.FileName.GetInspectionResultFromFileAsync(
        identifierInContainer: csvFile.IdentifierInContainer,
        guessJson: false,
        guessCodePage: true,
        guessEscapePrefix: true,
        guessDelimiter: true,
        guessQualifier: true,
        guessStartRow: true,
        guessHasHeader: true,
        guessNewLine: false,
        guessCommentLine: true,
        inspectionResult: new InspectionResult()
        {
          FileName = csvFile.FileName,
          IdentifierInContainer = csvFile.IdentifierInContainer,
          EscapePrefix = csvFile.EscapePrefixChar,
          FieldDelimiter = csvFile.FieldDelimiterChar,
          FieldQualifier = csvFile.FieldQualifierChar,
          SkipRows = csvFile.SkipRows,
          CommentLine = csvFile.CommentLine
        },
        fillGuessSettings: new FillGuessSettings(false),
        pgpKey: FunctionalDI.GetKeyAndPassphraseForFile(csvFile.FileName).key, cancellationToken: cancellationToken).ConfigureAwait(false);
      csvFile.CodePageId = det.CodePageId;
      csvFile.ByteOrderMark = det.ByteOrderMark;
      csvFile.EscapePrefixChar = det.EscapePrefix;
      csvFile.FieldDelimiterChar = det.FieldDelimiter;
      csvFile.FieldQualifierChar = det.FieldQualifier;
      csvFile.SkipRows = det.SkipRows;
      csvFile.HasFieldHeader = det.HasFieldHeader;
      csvFile.CommentLine = det.CommentLine;
    }

    internal static async Task<int> InspectCodePageAsync(this Stream stream, int codePageId,
      CancellationToken cancellationToken)
    {
      if (codePageId > 0)
        return codePageId;

      codePageId = (await stream.InspectCodePageAsync(cancellationToken).ConfigureAwait(false)).codePage;
      stream.Seek(0, SeekOrigin.Begin);

      return codePageId;
    }

    /// <summary>
    /// Get a file reader based on InspectionResult and stream
    /// </summary>
    /// <param name="inspectionResult">The Inspection Results with file information</param>
    /// <param name="stream">Optional: An already open memory stream</param>
    /// <returns>A <see cref="IFileReader"/> usually a <see cref="CsvFileReader"/></returns>
    public static IFileReader GetFileReader(this InspectionResult inspectionResult, in Stream? stream)
    {
      if (stream is MemoryStream memStream)
      {
        memStream.Seek(0, SeekOrigin.Begin);
        if (inspectionResult.IsJson)
          return new JsonFileReader(memStream, inspectionResult.Columns, 0L, false, string.Empty, false,
            StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, false, false);

        if (inspectionResult.IsXml)
          return new XmlFileReader(memStream, inspectionResult.Columns, 0L, false, string.Empty, false,
            StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, true, true);

        return new CsvFileReader(memStream,
          codePageId: inspectionResult.CodePageId,
          skipRows: inspectionResult is { HasFieldHeader: false, SkipRows: 0 } ? 1 : inspectionResult.SkipRows,
          hasFieldHeader: inspectionResult.HasFieldHeader,
          columnDefinition: inspectionResult.Columns,
          trimmingOption: TrimmingOptionEnum.Unquoted,
          fieldDelimiterChar: inspectionResult.FieldDelimiter,
          fieldQualifierChar: inspectionResult.FieldQualifier,
          escapeCharacterChar: inspectionResult.EscapePrefix,
          recordLimit: 0L, allowRowCombining: false,
          contextSensitiveQualifier: inspectionResult.ContextSensitiveQualifier,
          commentLine: inspectionResult.CommentLine, numWarning: 0,
          duplicateQualifierToEscape: inspectionResult.DuplicateQualifierToEscape,
          newLinePlaceholder: string.Empty, delimiterPlaceholder: string.Empty, quotePlaceholder: string.Empty,
          skipDuplicateHeader: true, treatLinefeedAsSpace: false, treatUnknownCharacterAsSpace: false,
          tryToSolveMoreColumns: false,
          warnDelimiterInValue: false, warnLineFeed: false, warnNbsp: false, warnQuotes: false,
          warnUnknownCharacter: false, warnEmptyTailingColumns: true,
          treatNbspAsSpace: false, treatTextAsNull: string.Empty,
          skipEmptyLines: true, consecutiveEmptyRowsMax: 4, timeZoneAdjust: StandardTimeZoneAdjust.ChangeTimeZone,
          returnedTimeZone: TimeZoneInfo.Local.Id,
          allowPercentage: true, removeCurrency: true);
      }

      if (inspectionResult.IsJson)
        return new JsonFileReader(inspectionResult.FileName, inspectionResult.Columns, 0L, false, string.Empty, false,
          StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id);

      if (inspectionResult.IsXml)
        return new XmlFileReader(inspectionResult.FileName, inspectionResult.Columns, 0L, false, string.Empty, false,
          StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, true, true);

      return new CsvFileReader(inspectionResult.FileName, inspectionResult.CodePageId,
        skipRows: inspectionResult is { HasFieldHeader: false, SkipRows: 0 } ? 1 : inspectionResult.SkipRows,
        inspectionResult.HasFieldHeader, inspectionResult.Columns, fieldDelimiterChar: inspectionResult.FieldDelimiter,
        fieldQualifierChar: inspectionResult.FieldQualifier, escapeCharacterChar: inspectionResult.EscapePrefix,
        recordLimit: 0L, allowRowCombining: false,
        contextSensitiveQualifier: inspectionResult.ContextSensitiveQualifier,
        commentLine: inspectionResult.CommentLine, numWarning: 0,
        duplicateQualifierToEscape: inspectionResult.DuplicateQualifierToEscape, newLinePlaceholder: string.Empty,
        delimiterPlaceholder: string.Empty,
        quotePlaceholder: string.Empty, skipDuplicateHeader: true, treatLinefeedAsSpace: false,
        treatUnknownCharacterAsSpace: false, tryToSolveMoreColumns: false,
        warnDelimiterInValue: false, warnLineFeed: false, warnNbsp: false, warnQuotes: false,
        warnUnknownCharacter: false, warnEmptyTailingColumns: true,
        treatNbspAsSpace: false, treatTextAsNull: string.Empty, skipEmptyLines: true, consecutiveEmptyRowsMax: 4,
        identifierInContainer: inspectionResult.IdentifierInContainer,
        timeZoneAdjust: StandardTimeZoneAdjust.ChangeTimeZone, returnedTimeZone: TimeZoneInfo.Local.Id,
        allowPercentage: true, removeCurrency: true);
    }
  }
}
