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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace CsvTools
{
  /// <summary>
  ///   Helper class
  /// </summary>
  public static class CsvHelper
  {
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
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>
    ///   <see cref="DetectionResult" /> with found information, or default if that test was not done
    /// </returns>
    public static async Task<DetectionResult> AnalyzeFileAsync(this string fileName,
      bool guessJson,
      bool guessCodePage,
      bool guessEscapePrefix,
      bool guessDelimiter,
      bool guessQualifier,
      bool guessStartRow,
      bool guessHasHeader,
      bool guessNewLine,
      bool guessCommentLine,
      FillGuessSettings fillGuessSettings,
      CancellationToken cancellationToken)
    {
      if (string.IsNullOrEmpty(fileName))
        throw new ArgumentException("Argument can not be empty", nameof(fileName));

      if (fileName.IndexOf('~') != -1)
        fileName = fileName.LongFileName();

      var fileName2 = FileSystemUtils.ResolvePattern(fileName);
      if (fileName2 is null)
        throw new FileNotFoundException(fileName);
      var fileInfo = new FileSystemUtils.FileInfo(fileName2);

      Logger.Information("Examining file {filename}", FileSystemUtils.GetShortDisplayFileName(fileName2, 40));
      Logger.Information($"Size of file: {StringConversion.DynamicStorageSize(fileInfo.Length)}");

#if !QUICK
      // load from Setting file
      if (fileName2.EndsWith(CsvFile.cCsvSettingExtension, StringComparison.OrdinalIgnoreCase)
          || FileSystemUtils.FileExists(fileName2 + CsvFile.cCsvSettingExtension))
      {
        var fileNameSetting = !fileName2.EndsWith(CsvFile.cCsvSettingExtension, StringComparison.OrdinalIgnoreCase)
          ? fileName2 + CsvFile.cCsvSettingExtension
          : fileName2;
        var fileNameFile = fileNameSetting.Substring(0, fileNameSetting.Length - CsvFile.cCsvSettingExtension.Length);

        try
        {
          // we defiantly have a the extension with the name
          var fileSettingSer = await fileNameSetting.DeserializeFileAsync<CsvFile>().ConfigureAwait(false);
          Logger.Information(
            "Configuration read from setting file {filename}",
            FileSystemUtils.GetShortDisplayFileName(fileNameSetting, 40));

          var columnCollection = new ColumnCollection();

          // un-ignore all ignored columns
          foreach (var col in fileSettingSer.ColumnCollection.Where(x => x.Ignore))
            columnCollection.Add(
              new Column(
                col.Name,
                col.ValueFormat,
                col.ColumnOrdinal,
                false,
                col.Convert,
                col.DestinationName,
                col.TimePart, col.TimePartFormat, col.TimeZonePart));

          return new DetectionResult(fileNameFile)
          {
            SkipRows = fileSettingSer.SkipRows,
            CodePageId = fileSettingSer.CodePageId,
            ByteOrderMark = fileSettingSer.ByteOrderMark,
            IdentifierInContainer = fileSettingSer.IdentifierInContainer,
            CommentLine = fileSettingSer.CommentLine,
            EscapePrefix = fileSettingSer.EscapePrefix,
            FieldDelimiter = fileSettingSer.FieldDelimiter,
            FieldQualifier= fileSettingSer.FieldQualifier,
            ContextSensitiveQualifier= fileSettingSer.ContextSensitiveQualifier,
            DuplicateQualifierToEscape = fileSettingSer.DuplicateQualifierToEscape,
            HasFieldHeader = fileSettingSer.HasFieldHeader,
            IsJson= false,
            NoDelimitedFile = fileSettingSer.NoDelimitedFile,
            NewLine = fileSettingSer.NewLine,
            Columns = columnCollection,
            ColumnFile = fileSettingSer is BaseSettingPhysicalFile bas ? bas.ColumnFile : string.Empty,
          };
        }
        catch (Exception e)
        {
          Logger.Warning(e, "Could not parse setting file {filename}", FileSystemUtils.GetShortDisplayFileName(fileNameSetting, 40));
        }
      }
#endif
      if (fileName2.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
      {
        var setting = await ManifestData.ReadManifestZip(fileName2).ConfigureAwait(false);
        if (setting is null)
        {
          Logger.Information("Trying to read manifest inside zip");
        }
        else
        {
          Logger.Information("Data in zip {filename}", setting.IdentifierInContainer);
          return setting;
        }
      }

      if (fileName2.EndsWith(ManifestData.cCsvManifestExtension, StringComparison.OrdinalIgnoreCase))
        try
        {
          var settingFs = await ManifestData.ReadManifestFileSystem(fileName2).ConfigureAwait(false);
          Logger.Information("Data in {filename}", settingFs.FileName);
          return settingFs;
        }
        catch (FileNotFoundException e2)
        {
          Logger.Information(e2, "Trying to read manifest");
        }

      // Determine from file
      var detectionResult = await GetDetectionResultFromFile(
        fileName2,
        guessJson,
        guessCodePage,
        guessEscapePrefix,
        guessDelimiter,
        guessQualifier,
        guessStartRow,
        guessHasHeader,
        guessNewLine,
        guessCommentLine,
        cancellationToken).ConfigureAwait(false);

      Logger.Information("Determining column format by reading samples");
#if NETSTANDARD2_1_OR_GREATER
      await
#endif
      using var reader = GetReaderFromDetectionResult(fileName2, detectionResult);
      await reader.OpenAsync(cancellationToken).ConfigureAwait(false);

      var (_, b) = await reader.FillGuessColumnFormatReaderAsyncReader(
        fillGuessSettings,
        null,
        false,
        true,
        "NULL",
        cancellationToken).ConfigureAwait(false);
      detectionResult.Columns.AddRangeNoClone(b);

      return detectionResult;
    }

    /// <summary>Checks if the comment line does make sense, or if its possibly better regarded as header row</summary>
    /// <param name="textReader">The text reader to read the data</param>
    /// <param name="commentLine">The characters for a comment line.</param>
    /// <param name="delimiter">The delimiter.</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>true if the comment line seems to ne ok</returns>
    public static async Task<bool> CheckLineCommentIsValidAsync(
      this ImprovedTextReader textReader,
      string commentLine,
      string delimiter,
      CancellationToken cancellationToken)
    {
      // if there is no commentLine it can not be wrong if there is no delimiter it can not be wrong
      if (string.IsNullOrEmpty(commentLine) || string.IsNullOrEmpty(delimiter))
        return true;

      if (textReader is null) throw new ArgumentNullException(nameof(textReader));

      const int maxRows = 100;
      var row = 0;
      var lineCommented = 0;
      var delimiterChar = delimiter.WrittenPunctuationToChar();
      var parts = 0;
      var partsComment = -1;
      while (row < maxRows && !textReader.EndOfStream && !cancellationToken.IsCancellationRequested)
      {
        var line = (await textReader.ReadLineAsync().ConfigureAwait(false)).TrimStart();
        if (string.IsNullOrEmpty(line))
          continue;

        if (line.StartsWith(commentLine, StringComparison.Ordinal))
        {
          lineCommented++;
          if (partsComment == -1)
            partsComment = line.Count(x => x == delimiterChar);
        }
        else
        {
          if (line.IndexOf(delimiterChar) != -1)
          {
            parts += line.Count(x => x == delimiterChar);
            row++;
          }
        }
      }

      // if we could not find a commented line exit and assume the comment line is wrong.
      if (lineCommented == 0)
        return false;

      // in case we have 3 or more commented lines assume the comment was ok
      if (lineCommented > 2)
        return true;

      // since we did not properly parse the delimited text accounting for quoting (delimiter in
      // column or newline splitting columns) apply some variance to it
      return partsComment < Math.Round(parts * .9 / row) || partsComment > Math.Round(parts * 1.1 / row);
    }

    /// <summary>
    ///   Updates the detection result from stream.
    /// </summary>
    /// <param name="stream">The stream to read data from</param>
    /// <param name="detectionResult">Passed is detection result</param>
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
    /// <param name="disallowedDelimiter">Delimiter to exclude in recognition, as they have been ruled out before</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    public static async Task GetDetectionResult(this Stream stream,
      DetectionResult detectionResult,
      bool guessJson,
      bool guessCodePage,
      bool guessEscapePrefix,
      bool guessDelimiter,
      bool guessQualifier,
      bool guessStartRow,
      bool guessHasHeader,
      bool guessNewLine,
      bool guessCommentLine,
      IEnumerable<char>? disallowedDelimiter,
      CancellationToken cancellationToken)
    {
      if (stream is null)
        throw new ArgumentNullException(nameof(stream));

      if (!(guessJson || guessCodePage || guessDelimiter || guessStartRow || guessQualifier || guessHasHeader ||
            guessCommentLine || guessNewLine))
        return;


      if (guessCodePage)
      {
        cancellationToken.ThrowIfCancellationRequested();

        stream.Seek(0, SeekOrigin.Begin);
        Logger.Information("Checking Code Page");
        var (codePage, bom) = await stream.GuessCodePage(cancellationToken).ConfigureAwait(false);
        detectionResult.CodePageId = codePage;
        detectionResult.ByteOrderMark = bom;
      }
      else
      {
        // assume its UTF8 with BOM
        detectionResult.CodePageId = Encoding.UTF8.CodePage;
        detectionResult.ByteOrderMark = true;
      }

      if (guessJson)
      {
        cancellationToken.ThrowIfCancellationRequested();

        Logger.Information("Checking Json format");

        detectionResult.IsJson = await stream.IsJsonReadable(Encoding.GetEncoding(detectionResult.CodePageId), cancellationToken).ConfigureAwait(false);
      }

      if (detectionResult.IsJson)
      {
        Logger.Information("Detected Json file, no further checks done");
        return;
      }

      if (guessEscapePrefix)
      {
        using var textReader = new ImprovedTextReader(stream, detectionResult.CodePageId, detectionResult.SkipRows);
        Logger.Information("Checking Escape Prefix");
        detectionResult.EscapePrefix =  await textReader.GuessEscapePrefixAsync(detectionResult.FieldDelimiter, detectionResult.FieldQualifier, cancellationToken);
      }


      if (guessCommentLine)
      {
        cancellationToken.ThrowIfCancellationRequested();
        Logger.Information("Checking comment line");
        using var textReader = new ImprovedTextReader(stream, detectionResult.CodePageId, detectionResult.SkipRows);
        detectionResult.CommentLine =  await textReader.GuessLineCommentAsync(cancellationToken).ConfigureAwait(false);
      }


      var oldDelimiter = detectionResult.FieldDelimiter.WrittenPunctuationToChar();
      // from here on us the encoding to read the stream again
      // ========== Start Row
      if (guessStartRow && oldDelimiter != 0)
      {
        cancellationToken.ThrowIfCancellationRequested();

        using var textReader = new ImprovedTextReader(stream, detectionResult.CodePageId, detectionResult.SkipRows);
        Logger.Information("Checking start line");

        detectionResult.SkipRows = textReader.GuessStartRow(detectionResult.FieldDelimiter, detectionResult.FieldQualifier, detectionResult.EscapePrefix, detectionResult.CommentLine, cancellationToken);
      }


      if (guessQualifier || guessDelimiter || guessNewLine)
      {
        Logger.Information("Re-Opening file");
        using var textReader = new ImprovedTextReader(stream, detectionResult.CodePageId, detectionResult.SkipRows);

        // ========== Delimiter
        if (guessDelimiter)
        {
          cancellationToken.ThrowIfCancellationRequested();
          Logger.Information("Checking Column Delimiter");
          var delimiterDet = await textReader.GuessDelimiterAsync(detectionResult.FieldQualifier, detectionResult.EscapePrefix, disallowedDelimiter, cancellationToken).ConfigureAwait(false);
          if (delimiterDet.MagicKeyword)
            detectionResult.SkipRows++;
          detectionResult.FieldDelimiter = delimiterDet.Delimiter;
          detectionResult.NoDelimitedFile = delimiterDet.IsDetected;
        }


        if (guessNewLine)
        {
          cancellationToken.ThrowIfCancellationRequested();
          Logger.Information("Checking Record Delimiter");
          stream.Seek(0, SeekOrigin.Begin);
          detectionResult.NewLine = textReader.GuessNewline(detectionResult.FieldQualifier, cancellationToken);
        }

        if (guessQualifier)
        {
          cancellationToken.ThrowIfCancellationRequested();
          Logger.Information("Checking Qualifier");
          var qualifier = DetectionQualifier.GuessQualifier(textReader, detectionResult.FieldDelimiter, detectionResult.EscapePrefix, new[] { '"', '\'' }, cancellationToken);

          detectionResult.FieldQualifier= char.ToString(qualifier.QuoteChar);
          detectionResult.ContextSensitiveQualifier= !(qualifier.DuplicateQualifier || qualifier.EscapedQualifier);
          detectionResult.DuplicateQualifierToEscape = qualifier.DuplicateQualifier;
        }
      }

      if (!string.IsNullOrEmpty(detectionResult.CommentLine) && !detectionResult.NoDelimitedFile)
      {
        cancellationToken.ThrowIfCancellationRequested();
        Logger.Information("Validating comment line");
        using var streamReader = new ImprovedTextReader(stream,
          await stream.CodePageResolve(detectionResult.CodePageId, cancellationToken).ConfigureAwait(false), detectionResult.SkipRows);
        if (!await CheckLineCommentIsValidAsync(
              streamReader,
              detectionResult.CommentLine,
              detectionResult.FieldDelimiter,
              cancellationToken).ConfigureAwait(false))
          detectionResult.CommentLine  = string.Empty;
      }

      if (guessStartRow)
      {
        cancellationToken.ThrowIfCancellationRequested();
        // find start row again , with possibly changed FieldDelimiter
        if (oldDelimiter != detectionResult.FieldDelimiter.StringToChar())
        {
          Logger.Information("Checking start row again because previously assumed delimiter has changed");
          using var textReader2 = new ImprovedTextReader(stream, detectionResult.CodePageId);
          detectionResult.SkipRows = textReader2.GuessStartRow(detectionResult.FieldDelimiter, detectionResult.FieldQualifier, detectionResult.EscapePrefix, detectionResult.CommentLine, cancellationToken);
        }
      }

      if (guessHasHeader)
      {
        cancellationToken.ThrowIfCancellationRequested();
        Logger.Information("Checking Header Row");

        var issue = await stream.GuessHasHeader(detectionResult.CodePageId, detectionResult.SkipRows, detectionResult.CommentLine,
          detectionResult.FieldDelimiter, detectionResult.FieldQualifier, detectionResult.EscapePrefix,
          cancellationToken).ConfigureAwait(false);
        Logger.Information(!string.IsNullOrEmpty(issue) ? $"Without Header Row {issue}" : "Has Header Row");
        detectionResult.HasFieldHeader = string.IsNullOrEmpty(issue);
      }
    }

#if !QUICK
    public static async Task DetectReadCsvAsync(this ICsvFile csvFile, CancellationToken cancellationToken)
    {
      var det = await csvFile.FileName.GetDetectionResultFromFile(false, true, true, true, true, true, true, false, true, cancellationToken).ConfigureAwait(false);
      csvFile.CodePageId = det.CodePageId;
      csvFile.ByteOrderMark = det.ByteOrderMark;
      csvFile.EscapePrefix= det.EscapePrefix;
      csvFile.FieldDelimiter = det.FieldDelimiter;
      csvFile.FieldQualifier = det.FieldQualifier;
      csvFile.SkipRows = det.SkipRows;
      csvFile.HasFieldHeader = det.HasFieldHeader;
      csvFile.CommentLine = det.CommentLine;
    }
#endif
    /// <summary>
    ///   Refreshes the settings assuming the file has changed, checks CodePage, Delimiter, Start
    ///   Row and Header
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
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
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">file name can not be empty - fileName</exception>
    public static async Task<DetectionResult> GetDetectionResultFromFile(this string fileName,
      bool guessJson, bool guessCodePage, bool guessEscapePrefix,
      bool guessDelimiter, bool guessQualifier,
      bool guessStartRow, bool guessHasHeader,
      bool guessNewLine, bool guessCommentLine,
      CancellationToken cancellationToken)
    {
      if (string.IsNullOrEmpty(fileName))
        throw new ArgumentException("File name can not be empty", nameof(fileName));

      var disallowedDelimiter = new List<char>();
      bool hasFields;
      var checks = GetNumberOfChecks(guessJson, guessCodePage, guessDelimiter, guessQualifier, guessStartRow,
        guessHasHeader, guessNewLine, guessCommentLine);

      var detectionResult = new DetectionResult(fileName);
      do
      {
        Logger.Information("Opening file");
        var sourceAccess = new SourceAccess(fileName);
#if NETSTANDARD2_1_OR_GREATER
        await
#endif
        using var improvedStream = FunctionalDI.OpenStream(sourceAccess);
        detectionResult.IdentifierInContainer = sourceAccess.IdentifierInContainer;
        // Determine from file
        await improvedStream!.GetDetectionResult(
          detectionResult,
          guessJson,
          guessCodePage,
          guessEscapePrefix,
          guessDelimiter,
          guessQualifier,
          guessStartRow,
          guessHasHeader,
          guessNewLine,
          guessCommentLine,
          disallowedDelimiter,
          cancellationToken).ConfigureAwait(false);

        hasFields = true;
        // if its a delimited file but we do not have fields,
        // the delimiter must have been wrong, pick another one, after 3 though give up
        if (!detectionResult.IsJson && disallowedDelimiter.Count < 3)
        {
          Logger.Information("Reading to check field delimiter", checks+1, true);
#if NETSTANDARD2_1_OR_GREATER
          await
#endif
          using var reader = GetReaderFromDetectionResult(fileName, detectionResult);
          await reader.OpenAsync(cancellationToken).ConfigureAwait(false);
          if (reader.FieldCount == 0)
          {
            Logger.Information(
              $"Found field delimiter {detectionResult.FieldDelimiter} is not valid, checking for an alternative", checks+2,
              true);
            hasFields = false;
            disallowedDelimiter.Add(detectionResult.FieldDelimiter.WrittenPunctuationToChar());
            // no need to check for Json again
            guessJson = false;
          }
        }
      } while (!hasFields);

      return detectionResult;
    }

    /// <summary>
    /// Returns a reader based on the detection result.
    /// </summary>
    /// <param name="fileName">Name of the file as its not stored in the detection results</param>
    /// <param name="detectionResult">The detection result.</param>
    /// <returns>Either a <see cref="JsonFileReader"/> or a <see cref="CsvFileReader"/></returns>
    public static IFileReader GetReaderFromDetectionResult(string fileName,
      DetectionResult detectionResult)
    {
      if (detectionResult.IsJson)
        return new JsonFileReader(fileName, detectionResult.Columns, 0L, false, string.Empty, false,
          StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id);
      return new CsvFileReader(
        fileName, detectionResult.CodePageId,
        detectionResult is { HasFieldHeader: false, SkipRows: 0 } ? 1 : detectionResult.SkipRows,
        detectionResult.HasFieldHeader, detectionResult.Columns, TrimmingOptionEnum.Unquoted, detectionResult.FieldDelimiter,
        detectionResult.FieldQualifier, detectionResult.EscapePrefix, 0L, false, false, detectionResult.CommentLine, 0,
        true, "", "", "", true, false, false, false, false,
        false, false, false, false, true, false, "NULL", true, 4, "", StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id);
    }

    /// <summary>
    ///   Guesses the code page from a stream.
    /// </summary>
    /// <param name="stream">The stream to read data from</param>
    /// <param name="token">The token.</param>
    /// <returns></returns>
    public static async Task<Tuple<int, bool>> GuessCodePage(this Stream stream, CancellationToken token)
    {
      // Read 256 kBytes
      var buff = new byte[262144];

      var length = await stream.ReadAsync(buff, 0, buff.Length, token).ConfigureAwait(false);
      if (length >= 2)
      {
        var byBom = EncodingHelper.GetEncodingByByteOrderMark(buff, 4);
        if (byBom != null)
        {
          Logger.Information($"Code Page: {EncodingHelper.GetEncodingName(byBom, true)}");
          return new Tuple<int, bool>(byBom.CodePage, true);
        }
      }

      var detected = EncodingHelper.GuessEncodingNoBom(buff);
      if (detected.Equals(Encoding.ASCII))
        detected = Encoding.UTF8;
      Logger.Information($"Code Page: {EncodingHelper.GetEncodingName(detected, false)}");
      return new Tuple<int, bool>(detected.CodePage, false);
    }

    public static async Task<string> GuessLineComment(
      this Stream stream,
      int codePageId,
      int skipRows,
      CancellationToken cancellationToken)
    {
      using var textReader = new ImprovedTextReader(stream,
        await stream.CodePageResolve(codePageId, cancellationToken).ConfigureAwait(false), skipRows);
      return await textReader.GuessLineCommentAsync(cancellationToken).ConfigureAwait(false);
    }


    /// <summary>Guesses the line comment</summary>
    /// <param name="textReader">The text reader to read the data</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>The determined comment</returns>
    /// <exception cref="System.ArgumentNullException">textReader</exception>
    public static async Task<string> GuessLineCommentAsync(this ImprovedTextReader textReader,
      CancellationToken cancellationToken)
    {
      if (textReader is null)
        throw new ArgumentNullException(nameof(textReader));

      var starts =
        new[] { "<!--", "##", "//", "\\\\", "''", "#", "/", "\\", "'" }.ToDictionary(test => test, _ => 0);

      // Comments are mainly at teh start of a file
      textReader.ToBeginning();
      for (int current = 0; current<50 && !textReader.EndOfStream && !cancellationToken.IsCancellationRequested; current++)
      {
        var line = (await textReader.ReadLineAsync().ConfigureAwait(false)).TrimStart();
        if (line.Length == 0)
          continue;
        foreach (var test in starts.Keys.Where(test => line.StartsWith(test, StringComparison.Ordinal)))
        {
          starts[test]++;
          // do not check further once a line is counted, by having ## before # a line starting with
          // ## will not be counted twice
          break;
        }
      }

      var maxCount = starts.Max(x => x.Value);
      if (maxCount > 0)
      {
        var check = starts.First(x => x.Value == maxCount);
        Logger.Information($"Comment Line: {check.Key}");
        return check.Key;
      }

      Logger.Information("No Comment Line");
      return string.Empty;
    }

    /// <summary>
    ///   Try to guess the new line sequence
    /// </summary>
    /// <param name="stream">The stream to read data from</param>
    /// <param name="codePageId">The code page identifier.</param>
    /// <param name="skipRows">The number of lines at beginning to disregard</param>
    /// <param name="fieldQualifier">Qualifier / Quoting of column to allow delimiter or linefeed to be contained in column</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>The NewLine Combination used</returns>
    public static async Task<RecordDelimiterTypeEnum> GuessNewline(
      this Stream stream,
      int codePageId,
      int skipRows,
      string fieldQualifier,
      CancellationToken cancellationToken)
    {
      using var textReader = new ImprovedTextReader(stream,
        await stream.CodePageResolve(codePageId, cancellationToken).ConfigureAwait(false), skipRows);
      return textReader.GuessNewline(fieldQualifier, cancellationToken);
    }

    /// <summary>
    ///   Guess the start row of a CSV file done with a rather simple csv parsing
    /// </summary>
    /// <param name="textReader">The text reader to read the data</param>
    /// <param name="delimiter">The delimiter.</param>
    /// <param name="quote">The quoting char</param>
    /// <param name="escapePrefix">The start of an escape sequence to allow delimiter or qualifier in column</param>
    /// <param name="commentLine">The characters for a comment line.</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>The number of rows to skip</returns>
    /// <exception cref="ArgumentNullException">commentLine</exception>
    public static int GuessStartRow(
      this ImprovedTextReader textReader,
      string delimiter,
      string quote,
      string escapePrefix,
      string commentLine,
      CancellationToken cancellationToken)
    {
      if (textReader is null)
        throw new ArgumentNullException(nameof(textReader));
      if (commentLine is null)
        throw new ArgumentNullException(nameof(commentLine));
      const int maxRows = 50;
      var delimiterChar = delimiter.WrittenPunctuationToChar();
      var quoteChar = quote.WrittenPunctuationToChar();
      var escapeChar = escapePrefix.WrittenPunctuationToChar();
      textReader.ToBeginning();
      var columnCount = new List<int>(maxRows);
      var rowMapping = new Dictionary<int, int>(maxRows);
      var colCount = new int[maxRows];
      var isComment = new bool[maxRows];
      var quoted = false;
      var firstChar = true;
      var currentRow = 0;
      var retValue = 0;

      while (currentRow < maxRows && !textReader.EndOfStream && !cancellationToken.IsCancellationRequested)
      {
        var readChar = textReader.Read();
        if (readChar==' ' || readChar == '\0')
          continue;

        // Handle Commented lines
        if (firstChar && commentLine.Length > 0 && readChar == commentLine[0])
        {
          isComment[currentRow] = true;

          for (var pos = 1; pos < commentLine.Length; pos++)
          {
            var nextChar = textReader.Peek();
            if (nextChar == commentLine[pos])
            {
              textReader.MoveNext();
              continue;
            }
            isComment[currentRow] = false;
            break;
          }
        }

        if (readChar == escapeChar && !isComment[currentRow])
          continue;

        // Handle Quoting
        if (readChar == quoteChar && !isComment[currentRow])
        {
          if (quoted)
          {
            if (textReader.Peek() != quoteChar)
              quoted = false;
            else
              textReader.MoveNext();
          }
          else
            quoted |= firstChar;
          continue;
        }

        switch (readChar)
        {
          // Feed and NewLines
          case '\n':
            if (!quoted)
            {
              currentRow++;
              firstChar = true;
              if (textReader.Peek() == '\r')
                textReader.MoveNext();
            }
            break;

          case '\r':
            if (!quoted)
            {
              currentRow++;
              firstChar = true;
              if (textReader.Peek() == '\n')
                textReader.MoveNext();
            }
            break;

          default:
            if (!isComment[currentRow] && !quoted && readChar == delimiterChar)
            {
              colCount[currentRow]++;
              firstChar = true;
            }
            break;
        }
      }

      cancellationToken.ThrowIfCancellationRequested();
      // remove all rows that are comment lines...
      for (var row = 0; row < currentRow; row++)
      {
        rowMapping[columnCount.Count] = row;
        if (!isComment[row])
          columnCount.Add(colCount[row]);
      }

      // if we do not more than 4 proper rows do nothing
      if (columnCount.Count > 4)
      {
        // In case we have a row that is exactly twice as long as the row before and row after,
        // assume its missing a linefeed
        for (var row = 1; row < columnCount.Count - 1; row++)
          if (columnCount[row + 1] > 0 && columnCount[row] == columnCount[row + 1] * 2
                                       && columnCount[row] == columnCount[row - 1] * 2)
            columnCount[row] = columnCount[row + 1];
        cancellationToken.ThrowIfCancellationRequested();
        // Get the average of the last 15 rows
        var num = 0;
        var sum = 0;
        for (var row = columnCount.Count - 1; num < 10 && row > 0; row--)
        {
          if (columnCount[row] <= 0)
            continue;
          sum += columnCount[row];
          num++;
        }

        var avg = (int) (sum / (double) (num == 0 ? 1 : num));
        // If there are not many columns do not try to guess
        if (avg > 1 && columnCount[0] < avg)
        {
          for (var row = columnCount.Count - 1; row > 0; row--)
            if (columnCount[row] > 0)
            {
              if (columnCount[row] >= avg - (avg / 10)) continue;
              retValue = rowMapping[row];
              break;
            }
            // In case we have an empty line but the next line are roughly good match take that
            // empty line
            else if (row + 2 < columnCount.Count && columnCount[row + 1] == columnCount[row + 2]
                                                 && columnCount[row + 1] >= avg - 1)
            {
              retValue = rowMapping[row + 1];
              break;
            }

          if (retValue == 0)
            for (var row = 0; row < columnCount.Count; row++)
              if (columnCount[row] > 0)
              {
                retValue = rowMapping[row];
                break;
              }
        }
      }

      Logger.Information($"Start Row: {retValue}");
      return retValue;
    }

    /// <summary>
    ///   Determines the start row in the file
    /// </summary>
    /// <param name="stream">The stream to read data from</param>
    /// <param name="codePageId">The code page identifier.</param>
    /// <param name="fieldDelimiter">The delimiter to separate columns</param>
    /// <param name="fieldQualifier">Qualifier / Quoting of column to allow delimiter or linefeed to be contained in column</param>
    /// <param name="escapePrefix">The start of an escape sequence to allow delimiter or qualifier in column</param>
    /// <param name="commentLine">The comment line.</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>The number of rows to skip</returns>
    public static async Task<int> GuessStartRow(
      this Stream stream,
      int codePageId,
      string fieldDelimiter,
      string fieldQualifier,
      string escapePrefix,
      string commentLine,
      CancellationToken cancellationToken)
    {
      using var streamReader = new ImprovedTextReader(stream,
        await stream.CodePageResolve(codePageId, cancellationToken).ConfigureAwait(false));
      return streamReader.GuessStartRow(fieldDelimiter, fieldQualifier, escapePrefix, commentLine, cancellationToken);
    }

    /// <summary>
    ///   Determines whether data in the specified stream is a JSON
    /// </summary>
    /// <param name="stream">The stream to read data from</param>
    /// <param name="encoding">The encoding.</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns><c>true</c> if json could be read from stream; otherwise, <c>false</c>.</returns>
    public static async Task<bool> IsJsonReadable(
      this Stream stream,
      Encoding encoding,
      CancellationToken cancellationToken)
    {
      stream.Seek(0, SeekOrigin.Begin);
      using var streamReader = new StreamReader(stream, encoding, true, 4096, true);
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
            Logger.Information("Detected Json file");
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

    internal static async Task<int> CodePageResolve(this Stream stream, int codePageId, CancellationToken cancellationToken)
    {
      if (codePageId > 0)
        return codePageId;

      codePageId = (await stream.GuessCodePage(cancellationToken).ConfigureAwait(false)).Item1;
      stream.Seek(0, SeekOrigin.Begin);

      return codePageId;
    }


    private static int GetNumberOfChecks(bool guessJson,
          bool guessCodePage,
      bool guessDelimiter,
      bool guessQualifier,
      bool guessStartRow,
      bool guessHasHeader,
      bool guessNewLine,
      bool guessCommentLine)
    {
      var checks = 0;
      if (guessJson)
        checks++;
      if (guessCodePage)
        checks++;
      if (guessDelimiter)
        checks++;
      if (guessStartRow)
        checks+=2;
      if (guessQualifier)
        checks++;
      if (guessHasHeader)
        checks++;
      if (guessCommentLine)
        checks++;
      if (guessNewLine)
        checks++;
      // Re-Opening
      if (guessQualifier || guessDelimiter || guessNewLine)
        checks++;
      return checks;
    }


    private static RecordDelimiterTypeEnum GuessNewline(
      this ImprovedTextReader textReader,
      string fieldQualifier,
      CancellationToken token)
    {
      if (textReader is null) throw new ArgumentNullException(nameof(textReader));
      const int numChars = 8192;

      var currentChar = 0;
      var quoted = false;

      const int cr = 0;
      const int lf = 1;
      const int crLf = 2;
      const int lfCr = 3;
      const int recSep = 4;
      const int unitSep = 5;

      int[] count = { 0, 0, 0, 0, 0, 0 };

      // \r = CR (Carriage Return) \n = LF (Line Feed)
      var fieldQualifierChar = fieldQualifier.WrittenPunctuationToChar();
      var textReaderPosition = new ImprovedTextReaderPositionStore(textReader);
      while (currentChar < numChars && !textReaderPosition.AllRead() && !token.IsCancellationRequested)
      {
        var readChar = textReader.Read();
        if (readChar == fieldQualifierChar)
        {
          if (quoted)
          {
            if (textReader.Peek() != fieldQualifierChar)
              quoted = false;
            else
              textReader.MoveNext();
          }
          else
          {
            quoted = true;
          }
        }

        if (quoted)
          continue;

        switch (readChar)
        {
          case 30:
            count[recSep]++;
            continue;
          case 31:
            count[unitSep]++;
            continue;
          case 10:
          {
            if (textReader.Peek() == 13)
            {
              textReader.MoveNext();
              count[lfCr]++;
            }
            else
            {
              count[lf]++;
            }

            currentChar++;
            break;
          }
          case 13:
          {
            if (textReader.Peek() == 10)
            {
              textReader.MoveNext();
              count[crLf]++;
            }
            else
            {
              count[cr]++;
            }

            break;
          }
        }

        currentChar++;
      }

      var maxCount = count.Max();
      if (maxCount == 0)
        return RecordDelimiterTypeEnum.None;

      var res = RecordDelimiterTypeEnum.None;
      if (count[recSep] == maxCount)
        res = RecordDelimiterTypeEnum.Rs;
      else if (count[unitSep] == maxCount)
        res = RecordDelimiterTypeEnum.Us;
      else if (count[cr] == maxCount)
        res = RecordDelimiterTypeEnum.Cr;
      else if (count[lf] == maxCount)
        res = RecordDelimiterTypeEnum.Lf;
      else if (count[crLf] == maxCount)
        res = RecordDelimiterTypeEnum.Crlf;
      else if (count[lfCr] == maxCount)
        res = RecordDelimiterTypeEnum.Lfcr;
      Logger.Information($"Record Delimiter: {res.Description()}");
      return res;
    }


  }
}