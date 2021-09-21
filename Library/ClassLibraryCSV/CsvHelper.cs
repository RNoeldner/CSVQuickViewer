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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  ///   Helper class
  /// </summary>
  public static class CsvHelper
  {
    /// <summary>
    ///   Check the file asynchronous
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="guessJson">if <c>true</c> trying to determine if file is a JSON file</param>
    /// <param name="guessCodePage">if <c>true</c>, try to determine the code page</param>
    /// <param name="guessDelimiter">if <c>true</c>, try to determine the delimiter</param>
    /// <param name="guessQualifier">if <c>true</c>, try to determine the qualifier for text</param>
    /// <param name="guessStartRow">if <c>true</c>, try to determine the number of skipped rows</param>
    /// <param name="guessHasHeader">
    ///   if true, try to determine if the file does have a header row
    /// </param>
    /// <param name="guessNewLine">if set to <c>true</c> determine combination of new line.</param>
    /// <param name="guessCommentLine"></param>
    /// <param name="fillGuessSettings">The fill guess settings.</param>
    /// <param name="processDisplay">The process display.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">processDisplay</exception>
    public static async Task<DelimitedFileDetectionResultWithColumns> AnalyseFileAsync(
      this string fileName,
      bool guessJson,
      bool guessCodePage,
      bool guessDelimiter,
      bool guessQualifier,
      bool guessStartRow,
      bool guessHasHeader,
      bool guessNewLine,
      bool guessCommentLine,
      FillGuessSettings fillGuessSettings,
      IProcessDisplay processDisplay)
    {
      if (processDisplay is null) throw new ArgumentNullException(nameof(processDisplay));
      if (string.IsNullOrEmpty(fileName))
        throw new ArgumentException("Argument can not be empty", nameof(fileName));

      if (fileName.IndexOf('~') != -1)
        fileName = fileName.LongFileName();

      var fileName2 = FileSystemUtils.ResolvePattern(fileName);
      if (fileName2 is null)
        throw new FileNotFoundException(fileName);
      var fileInfo = new FileSystemUtils.FileInfo(fileName2);

      Logger.Information("Examining file {filename}", FileSystemUtils.GetShortDisplayFileName(fileName2!, 40));
      Logger.Information($"Size of file: {StringConversion.DynamicStorageSize(fileInfo.Length)}");

#if !QUICK
			// load from Setting file
			if (fileName2!.EndsWith(CsvFile.cCsvSettingExtension, StringComparison.OrdinalIgnoreCase)
					|| FileSystemUtils.FileExists(fileName2 + CsvFile.cCsvSettingExtension))
			{
				var fileNameSetting = !fileName2.EndsWith(CsvFile.cCsvSettingExtension, StringComparison.OrdinalIgnoreCase)
																? fileName2 + CsvFile.cCsvSettingExtension
																: fileName2;
				var fileNameFile = fileNameSetting.Substring(0, fileNameSetting.Length - CsvFile.cCsvSettingExtension.Length);

				// we defiantly have a the extension with the name
				var fileSettingSer = SerializedFilesLib.LoadCsvFile(fileNameSetting);
				Logger.Information(
					"Configuration read from setting file {filename}",
					FileSystemUtils.GetShortDisplayFileName(fileNameSetting, 40));

				var columnCollection = new ColumnCollection();

				// un-ignore all ignored columns
				foreach (var col in fileSettingSer.ColumnCollection.Where(x => x.Ignore))
					columnCollection.Add(
						new ImmutableColumn(
							col.Name,
							col.ValueFormat,
							col.ColumnOrdinal,
							col.Convert,
							col.DestinationName,
							false,
							col.TimePart,
							col.TimePartFormat,
							col.TimeZonePart));

				return new DelimitedFileDetectionResultWithColumns(
					fileNameFile,
					fileSettingSer.SkipRows,
					fileSettingSer.CodePageId,
					fileSettingSer.ByteOrderMark,
					fileSettingSer.FileFormat.QualifyAlways,
					fileSettingSer.IdentifierInContainer,
					fileSettingSer.FileFormat.CommentLine,
					fileSettingSer.FileFormat.EscapeCharacter,
					fileSettingSer.FileFormat.FieldDelimiter,
					fileSettingSer.FileFormat.FieldQualifier,
					fileSettingSer.HasFieldHeader,
					false,
					fileSettingSer.NoDelimitedFile,
					fileSettingSer.FileFormat.NewLine,
					columnCollection,
					fileSettingSer is BaseSettingPhysicalFile bas ? bas.ColumnFile : string.Empty);
			}
#endif
      if (fileName2.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        try
        {
          var setting = await ManifestData.ReadManifestZip(fileName2).ConfigureAwait(false);
          Logger.Information("Data in zip {filename}", setting.IdentifierInContainer);
          return setting;
        }
        catch (FileNotFoundException e1)
        {
          Logger.Information(e1, "Trying to read manifest inside zip");
        }

      if (fileName2.EndsWith(ManifestData.cCsvManifestExtension))
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
                              processDisplay,
                              guessJson,
                              guessCodePage,
                              guessDelimiter,
                              guessQualifier,
                              guessStartRow,
                              guessHasHeader,
                              guessNewLine,
                              guessCommentLine).ConfigureAwait(false);

      processDisplay.SetProcess("Determining column format by reading samples", -1, true);

      using IFileReader reader = GetReaderFromDetectionResult(fileName2, detectionResult, processDisplay);
      await reader.OpenAsync(processDisplay.CancellationToken).ConfigureAwait(false);
      var (_, b) = await reader.FillGuessColumnFormatReaderAsyncReader(
                     fillGuessSettings,
                     null,
                     false,
                     true,
                     "NULL",
                     processDisplay.CancellationToken).ConfigureAwait(false);

      return new DelimitedFileDetectionResultWithColumns(detectionResult, b);
    }

    /// <summary>
    ///   Checks if teh comment line does make sense, or if its possibly better regarded as header row
    /// </summary>
    /// <param name="textReader">The stream reader with the data</param>
    /// <param name="delimiter">The delimiter.</param>
    /// <param name="commentLine">The characters for a comment line.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>true if the comment line seems to ne ok</returns>
    public static bool CheckLineCommentIsValid(
      this ImprovedTextReader textReader,
      string commentLine,
      string delimiter,
      CancellationToken cancellationToken)
    {
      // if there is no commentLine it can not be wrong if there is no delimiter it can not be wrong
      if (string.IsNullOrEmpty(commentLine) || string.IsNullOrEmpty(delimiter))
        return true;

      if (textReader is null) throw new ArgumentNullException(nameof(textReader));

      const int MaxRows = 100;
      var row = 0;
      var lineCommented = 0;
      var delim = delimiter.WrittenPunctuationToChar();
      var parts = 0;
      var partsComment = -1;
      while (row < MaxRows && !textReader.EndOfStream && !cancellationToken.IsCancellationRequested)
      {
        var line = textReader.ReadLine().TrimStart();
        if (string.IsNullOrEmpty(line))
          continue;

        if (line.StartsWith(commentLine, StringComparison.Ordinal))
        {
          lineCommented++;
          if (partsComment == -1)
            partsComment = line.Count(x => x == delim);
        }
        else
        {
          if (line.IndexOf(delim) != -1)
          {
            parts += line.Count(x => x == delim);
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

    public static async Task<int> CodePageResolve(
      this IImprovedStream improvedStream,
      int codePageId,
      CancellationToken cancellationToken)
    {
      if (codePageId < 0)
      {
        codePageId = (await improvedStream.GuessCodePage(cancellationToken).ConfigureAwait(false)).Item1;
        improvedStream.Seek(0, SeekOrigin.Begin);
      }

      return codePageId;
    }

    /// <summary>
    ///   Updates the detection result from stream.
    /// </summary>
    /// <param name="improvedStream">The improved stream.</param>
    /// <param name="fileName"></param>
    /// <param name="display">The display.</param>
    /// <param name="guessJson">if <c>true</c> trying to determine if file is a JSON file</param>
    /// <param name="guessCodePage">if <c>true</c>, try to determine the code page</param>
    /// <param name="guessDelimiter">if <c>true</c>, try to determine the delimiter</param>
    /// <param name="guessQualifier">if <c>true</c>, try to determine the qualifier for text</param>
    /// <param name="guessStartRow">if <c>true</c>, try to determine the number of skipped rows</param>
    /// <param name="guessHasHeader">
    ///   if true, try to determine if the file does have a header row
    /// </param>
    /// <param name="guessNewLine">if set to <c>true</c> determine combination of new line.</param>
    /// <param name="guessCommentLine"></param>
    public static async Task<DelimitedFileDetectionResult> GetDetectionResult(
      this IImprovedStream improvedStream,
      string fileName,
      IProcessDisplay display,
      bool guessJson,
      bool guessCodePage,
      bool guessDelimiter,
      bool guessQualifier,
      bool guessStartRow,
      bool guessHasHeader,
      bool guessNewLine,
      bool guessCommentLine)
    {
      if (improvedStream is null)
        throw new ArgumentNullException(nameof(improvedStream));
      if (string.IsNullOrEmpty(fileName))
        throw new ArgumentException("FileName can not be empty", nameof(fileName));
      if (display is null)
        throw new ArgumentNullException(nameof(display));

      var detectionResult = new DelimitedFileDetectionResult(fileName);
      if (!(guessJson || guessCodePage || guessDelimiter || guessStartRow || guessQualifier || guessHasHeader
            || guessCommentLine || guessNewLine))
        return detectionResult;

      if (guessCodePage)
      {
        if (display.CancellationToken.IsCancellationRequested)
          return detectionResult;
        improvedStream.Seek(0, SeekOrigin.Begin);
        display.SetProcess("Checking Code Page", -1, true);
        var (codePage, bom) = await improvedStream.GuessCodePage(display.CancellationToken).ConfigureAwait(false);
        detectionResult = new DelimitedFileDetectionResult(
          detectionResult.FileName,
          detectionResult.SkipRows,
          codePage,
          bom,
          detectionResult.QualifyAlways,
          detectionResult.IdentifierInContainer,
          detectionResult.CommentLine,
          detectionResult.EscapeCharacter,
          detectionResult.FieldDelimiter,
          detectionResult.FieldQualifier,
          detectionResult.HasFieldHeader,
          false,
          detectionResult.NoDelimitedFile,
          detectionResult.NewLine);
      }

      if (guessJson)
      {
        display.SetProcess("Checking Json format", -1, false);
        if (await improvedStream.IsJsonReadable(
              Encoding.GetEncoding(detectionResult.CodePageId),
              display.CancellationToken).ConfigureAwait(false))
          detectionResult = new DelimitedFileDetectionResult(
            detectionResult.FileName,
            0,
            detectionResult.CodePageId,
            detectionResult.ByteOrderMark,
            detectionResult.QualifyAlways,
            detectionResult.IdentifierInContainer,
            detectionResult.CommentLine,
            detectionResult.EscapeCharacter,
            detectionResult.FieldDelimiter,
            detectionResult.FieldQualifier,
            detectionResult.HasFieldHeader,
            true,
            detectionResult.NoDelimitedFile,
            detectionResult.NewLine);
      }

      if (detectionResult.IsJson)
      {
        display.SetProcess("Detected Json file", -1, false);
        return detectionResult;
      }

      if (guessCommentLine)
      {
        display.SetProcess("Checking comment line", -1, true);
        using var streamReader = await improvedStream.GetStreamReaderAtStart(
                                   detectionResult.CodePageId,
                                   detectionResult.SkipRows,
                                   display.CancellationToken).ConfigureAwait(false);
        detectionResult = new DelimitedFileDetectionResult(
          detectionResult.FileName,
          detectionResult.SkipRows,
          detectionResult.CodePageId,
          detectionResult.ByteOrderMark,
          detectionResult.QualifyAlways,
          detectionResult.IdentifierInContainer,
          streamReader.GuessLineComment(display.CancellationToken),
          detectionResult.EscapeCharacter,
          detectionResult.FieldDelimiter,
          detectionResult.FieldQualifier,
          detectionResult.HasFieldHeader,
          false,
          detectionResult.NoDelimitedFile,
          detectionResult.NewLine);
      }

      display.SetProcess("Checking delimited text file", -1, true);
      var oldDelimiter = detectionResult.FieldDelimiter.WrittenPunctuationToChar();
      // from here on us the encoding to read the stream again
      if (guessStartRow && oldDelimiter != 0)
      {
        if (display.CancellationToken.IsCancellationRequested)
          return detectionResult;
        using var streamReader = await improvedStream.GetStreamReaderAtStart(
                                   detectionResult.CodePageId,
                                   detectionResult.SkipRows,
                                   display.CancellationToken).ConfigureAwait(false);
        detectionResult = new DelimitedFileDetectionResult(
          detectionResult.FileName,
          streamReader.GuessStartRow(
            detectionResult.FieldDelimiter,
            detectionResult.FieldQualifier,
            detectionResult.CommentLine,
            display.CancellationToken),
          detectionResult.CodePageId,
          detectionResult.ByteOrderMark,
          detectionResult.QualifyAlways,
          detectionResult.IdentifierInContainer,
          detectionResult.CommentLine,
          detectionResult.EscapeCharacter,
          detectionResult.FieldDelimiter,
          detectionResult.FieldQualifier,
          detectionResult.HasFieldHeader,
          true,
          detectionResult.NoDelimitedFile,
          detectionResult.NewLine);
      }

      if (guessQualifier || guessDelimiter || guessNewLine)
      {
        using var textReader = await improvedStream.GetStreamReaderAtStart(
                                 detectionResult.CodePageId,
                                 detectionResult.SkipRows,
                                 display.CancellationToken).ConfigureAwait(false);
        if (guessDelimiter)
        {
          if (display.CancellationToken.IsCancellationRequested)
            return detectionResult;
          display.SetProcess("Checking Column Delimiter", -1, false);
          var (delimiter, noDelimiter) = textReader.GuessDelimiter(
            detectionResult.EscapeCharacter,
            display.CancellationToken);
          detectionResult = new DelimitedFileDetectionResult(
            detectionResult.FileName,
            detectionResult.SkipRows,
            detectionResult.CodePageId,
            detectionResult.ByteOrderMark,
            detectionResult.QualifyAlways,
            detectionResult.IdentifierInContainer,
            detectionResult.CommentLine,
            detectionResult.EscapeCharacter,
            delimiter,
            detectionResult.FieldQualifier,
            detectionResult.HasFieldHeader,
            detectionResult.IsJson,
            noDelimiter,
            detectionResult.NewLine);
        }

        if (guessNewLine)
        {
          if (display.CancellationToken.IsCancellationRequested)
            return detectionResult;
          display.SetProcess("Checking Record Delimiter", -1, false);
          improvedStream.Seek(0, SeekOrigin.Begin);
          detectionResult = new DelimitedFileDetectionResult(
            detectionResult.FileName,
            detectionResult.SkipRows,
            detectionResult.CodePageId,
            detectionResult.ByteOrderMark,
            detectionResult.QualifyAlways,
            detectionResult.IdentifierInContainer,
            detectionResult.CommentLine,
            detectionResult.EscapeCharacter,
            detectionResult.FieldDelimiter,
            detectionResult.FieldQualifier,
            detectionResult.HasFieldHeader,
            detectionResult.IsJson,
            detectionResult.NoDelimitedFile,
            textReader.GuessNewline(detectionResult.FieldQualifier, display.CancellationToken));
        }

        if (guessQualifier)
        {
          if (display.CancellationToken.IsCancellationRequested)
            return detectionResult;
          display.SetProcess("Checking Qualifier", -1, false);
          var qualifier = textReader.GuessQualifier(detectionResult.FieldDelimiter, display.CancellationToken);
          if (qualifier != '\0')
            detectionResult = new DelimitedFileDetectionResult(
              detectionResult.FileName,
              detectionResult.SkipRows,
              detectionResult.CodePageId,
              detectionResult.ByteOrderMark,
              detectionResult.QualifyAlways,
              detectionResult.IdentifierInContainer,
              detectionResult.CommentLine,
              detectionResult.EscapeCharacter,
              detectionResult.FieldDelimiter,
              char.ToString(qualifier),
              detectionResult.HasFieldHeader,
              detectionResult.IsJson,
              detectionResult.NoDelimitedFile,
              detectionResult.NewLine);
        }
      }

      if (!string.IsNullOrEmpty(detectionResult.CommentLine) && !detectionResult.NoDelimitedFile)
      {
        display.SetProcess("Validating comment line", -1, true);
        using var streamReader = await improvedStream.GetStreamReaderAtStart(
                                   detectionResult.CodePageId,
                                   detectionResult.SkipRows,
                                   display.CancellationToken).ConfigureAwait(false);
        if (!CheckLineCommentIsValid(
              streamReader,
              detectionResult.CommentLine,
              detectionResult.FieldDelimiter,
              display.CancellationToken))
          detectionResult = new DelimitedFileDetectionResult(
            detectionResult.FileName,
            detectionResult.SkipRows,
            detectionResult.CodePageId,
            detectionResult.ByteOrderMark,
            detectionResult.QualifyAlways,
            detectionResult.IdentifierInContainer,
            string.Empty,
            detectionResult.EscapeCharacter,
            detectionResult.FieldDelimiter,
            detectionResult.FieldQualifier,
            detectionResult.HasFieldHeader,
            false,
            detectionResult.NoDelimitedFile,
            detectionResult.NewLine);
      }

      // find start row again , with possibly changed FieldDelimiter
      if (guessStartRow && oldDelimiter != detectionResult.FieldDelimiter.StringToChar())
      {
        if (oldDelimiter != 0)
          Logger.Information("Checking start row again because previously assumed delimiter has changed");
        if (display.CancellationToken.IsCancellationRequested)
          return detectionResult;
        using var streamReader2 = await improvedStream
                                    .GetStreamReaderAtStart(detectionResult.CodePageId, 0, display.CancellationToken)
                                    .ConfigureAwait(false);
        streamReader2.ToBeginning();
        detectionResult = new DelimitedFileDetectionResult(
          detectionResult.FileName,
          streamReader2.GuessStartRow(
            detectionResult.FieldDelimiter,
            detectionResult.FieldQualifier,
            detectionResult.CommentLine,
            display.CancellationToken),
          detectionResult.CodePageId,
          detectionResult.ByteOrderMark,
          detectionResult.QualifyAlways,
          detectionResult.IdentifierInContainer,
          detectionResult.CommentLine,
          detectionResult.EscapeCharacter,
          detectionResult.FieldDelimiter,
          detectionResult.FieldQualifier,
          detectionResult.HasFieldHeader,
          detectionResult.IsJson,
          detectionResult.NoDelimitedFile,
          detectionResult.NewLine);
      }

      if (guessHasHeader)
      {
        if (display.CancellationToken.IsCancellationRequested)
          return detectionResult;
        display.SetProcess("Checking for Header Row", -1, false);
        detectionResult = new DelimitedFileDetectionResult(
          detectionResult.FileName,
          detectionResult.SkipRows,
          detectionResult.CodePageId,
          detectionResult.ByteOrderMark,
          detectionResult.QualifyAlways,
          detectionResult.IdentifierInContainer,
          detectionResult.CommentLine,
          detectionResult.EscapeCharacter,
          detectionResult.FieldDelimiter,
          detectionResult.FieldQualifier,
          (await GuessHasHeader(
             improvedStream,
             detectionResult.CodePageId,
             detectionResult.SkipRows,
             detectionResult.CommentLine,
             detectionResult.FieldDelimiter,
             display.CancellationToken).ConfigureAwait(false)).Item1,
          detectionResult.IsJson,
          detectionResult.NoDelimitedFile,
          detectionResult.NewLine);
      }

      return detectionResult;
    }

    /// <summary>
    ///   Refreshes the settings assuming the file has changed, checks CodePage, Delimiter, Start
    ///   Row and Header
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="display">The display.</param>
    /// <param name="guessJson">if true trying to determine if file is a JSOn file</param>
    /// <param name="guessCodePage">if true, try to determine the code page</param>
    /// <param name="guessDelimiter">if true, try to determine the delimiter</param>
    /// <param name="guessQualifier">if true, try to determine the qualifier for text</param>
    /// <param name="guessStartRow">if true, try to determine the number of skipped rows</param>
    /// <param name="guessHasHeader">
    ///   if true, try to determine if the file does have a header row
    /// </param>
    /// <param name="guessNewLine">if true, try to determine what kind of new line we do use</param>
    /// <param name="guessCommentLine"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">file name can not be empty - fileName</exception>
    public static async Task<DelimitedFileDetectionResult> GetDetectionResultFromFile(
      this string fileName,
      IProcessDisplay display,
      bool guessJson = false,
      bool guessCodePage = true,
      bool guessDelimiter = true,
      bool guessQualifier = true,
      bool guessStartRow = true,
      bool guessHasHeader = true,
      bool guessNewLine = true,
      bool guessCommentLine = true)
    {
      if (string.IsNullOrEmpty(fileName))
        throw new ArgumentException("file name can not be empty", nameof(fileName));
      if (display is null)
        throw new ArgumentNullException(nameof(display));
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
      await
#endif
      using var improvedStream = FunctionalDI.OpenStream(new SourceAccess(fileName));
      return await improvedStream.GetDetectionResult(
               fileName,
               display,
               guessJson,
               guessCodePage,
               guessDelimiter,
               guessQualifier,
               guessStartRow,
               guessHasHeader,
               guessNewLine,
               guessCommentLine).ConfigureAwait(false);
    }

    /// <summary>
    ///   Guesses the code page from a stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="token">The token.</param>
    /// <returns></returns>
    public static async Task<Tuple<int, bool>> GuessCodePage(this IImprovedStream stream, CancellationToken token)
    {
      // Read 256 kBytes
      var buff = new byte[262144];

      var length = await stream.ReadAsync(buff, 0, buff.Length, token).ConfigureAwait(false);
      if (length >= 2)
      {
        var byBom = EncodingHelper.GetEncodingByByteOrderMark(buff);
        if (byBom != null)
        {
          Logger.Information("Code Page: {encoding}", EncodingHelper.GetEncodingName(byBom, true));
          return new Tuple<int, bool>(byBom.CodePage, true);
        }
      }

      var detected = EncodingHelper.GuessEncodingNoBom(buff);
      if (detected.Equals(Encoding.ASCII))
        detected = Encoding.UTF8;
      Logger.Information("Code Page: {encoding}", EncodingHelper.GetEncodingName(detected, false));
      return new Tuple<int, bool>(detected.CodePage, false);
    }

    /// <summary>
    ///   Guesses the delimiter for a files. Done with a rather simple csv parsing, and trying to
    ///   find the delimiter that has the least variance in the read rows, if that is not possible
    ///   the delimiter with the highest number of occurrences.
    /// </summary>
    /// <param name="improvedStream">The improved stream.</param>
    /// <param name="codePageId">The code page identifier.</param>
    /// <param name="skipRows">The skip rows.</param>
    /// <param name="escapeCharacter">The escape character.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A character with the assumed delimiter for the file</returns>
    /// <remarks>No Error will not be thrown.</remarks>
    public static async Task<Tuple<string, bool>> GuessDelimiter(
      this IImprovedStream improvedStream,
      int codePageId,
      int skipRows,
      string escapeCharacter,
      CancellationToken cancellationToken)
    {
      if (improvedStream is null)
        throw new ArgumentNullException(nameof(improvedStream));
      using var textReader = await improvedStream.GetStreamReaderAtStart(codePageId, skipRows, cancellationToken)
                               .ConfigureAwait(false);
      textReader.ToBeginning();
      return textReader.GuessDelimiter(escapeCharacter, cancellationToken);
    }

    /// <summary>
    ///   Guesses the has header from reader.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="comment">The comment.</param>
    /// <param name="delimiter">The delimiter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="ApplicationException">
    ///   Empty Line or Control Characters in Column {headerLine} or Only one column: {headerLine}
    /// </exception>
    public static Tuple<bool, string> GuessHasHeader(
      this ImprovedTextReader reader,
      string? comment,
      string? delimiter,
      CancellationToken cancellationToken)
    {
      var headerLine = string.Empty;
      comment ??= string.Empty;
      var delimiterChar = delimiter?.WrittenPunctuationToChar() ?? '\0';
      while (string.IsNullOrEmpty(headerLine) && !reader.EndOfStream)
      {
        cancellationToken.ThrowIfCancellationRequested();
        headerLine = reader.ReadLine();
        if (!string.IsNullOrEmpty(comment) && headerLine.TrimStart().StartsWith(comment))
          headerLine = string.Empty;
      }

      try
      {
        if (string.IsNullOrEmpty(headerLine))
          throw new ApplicationException("Empty Line");

        if (headerLine.NoControlCharacters().Length < headerLine.Replace("\t", "").Length)
          throw new ApplicationException($"Control Characters in Column {headerLine}");

        var headerRow = headerLine.Split(delimiterChar).Select(x => x.Trim('\"')).ToList();

        // get the average field count looking at the header and 12 additional valid lines
        var fieldCount = headerRow.Count;

        // if there is only one column the header be number of letter and might be followed by a
        // single number
        if (fieldCount < 2)
        {
          if (!(headerLine.Length > 2 && Regex.IsMatch(headerLine, @"^[a-zA-Z]+\d?$")))
            throw new ApplicationException($"Only one column: {headerLine}");
        }
        else
        {
          var counter = 1;
          while (counter < 12 && !cancellationToken.IsCancellationRequested && !reader.EndOfStream)
          {
            var dataLine = reader.ReadLine();
            if (string.IsNullOrEmpty(dataLine)
                || (!string.IsNullOrEmpty(comment) && dataLine.TrimStart().StartsWith(comment)))
              continue;
            counter++;
            fieldCount += dataLine.Split(delimiterChar).Length;
          }

          var avgFieldCount = fieldCount / (double) counter;
          // The average should not be smaller than the columns in the initial row
          if (avgFieldCount < headerRow.Count)
            avgFieldCount = headerRow.Count;
          var halfTheColumns = (int) Math.Ceiling(avgFieldCount / 2.0);

          // Columns are only one or two char, does not look descriptive
          if (headerRow.Count(x => x.Length < 3) > halfTheColumns)
            throw new ApplicationException(
              $"Headers '{string.Join("', '", headerRow.Where(x => x.Length < 3))}' very short");

          // use the same routine that is used in readers to determine the names of the columns
          var (_, numIssues) = BaseFileReader.AdjustColumnName(headerRow, (int) avgFieldCount, null);

          // looking at the warnings raised
          if (numIssues >= halfTheColumns || numIssues > 2)
            throw new ApplicationException($"{numIssues} header where empty, duplicate or too long");

          var numeric = headerRow.Where(header => Regex.IsMatch(header, @"^\d+$")).ToList();
          var boolHead = headerRow.Where(header => StringConversion.StringToBooleanStrict(header, "1", "0") != null)
            .ToList();
          var specials = headerRow.Where(header => Regex.IsMatch(header, @"[^\w\d\-_\s<>#,.*\[\]\(\)+?!]")).ToList();
          if (numeric.Count + boolHead.Count + specials.Count >= halfTheColumns)
          {
            StringBuilder msg = new StringBuilder();
            if (numeric.Count > 0)
            {
              msg.Append("Headers ");
              foreach (var header in numeric)
              {
                msg.Append("'");
                msg.Append(header.Trim('\"'));
                msg.Append("',");
              }

              msg.Length--;
              msg.Append(" numeric");
            }

            if (boolHead.Count > 0)
            {
              if (msg.Length > 0)
                msg.Append(" and ");
              msg.Append("Headers ");
              foreach (var header in boolHead)
              {
                msg.Append("'");
                msg.Append(header.Trim('\"'));
                msg.Append("',");
              }

              msg.Length--;
              msg.Append(" boolean");
            }

            if (specials.Count > 0)
            {
              if (msg.Length > 0)
                msg.Append(" and ");
              msg.Append("Headers ");
              foreach (var header in specials)
              {
                msg.Append("'");
                msg.Append(header.Trim('\"'));
                msg.Append("',");
              }

              msg.Length--;
              msg.Append(" with uncommon characters");
            }

            throw new ApplicationException(msg.ToString());
          }
        }
      }
      catch (ApplicationException ex)
      {
        Logger.Information("Without Header Row {reason}", ex.Message);
        return new Tuple<bool, string>(false, ex.Message);
      }

      Logger.Information("Has Header Row");
      return new Tuple<bool, string>(true, "Header seems present");
    }

    /// <summary>
    ///   Guesses the has header from stream.
    /// </summary>
    /// <param name="improvedStream">The improved stream.</param>
    /// <param name="codePageId">The code page identifier.</param>
    /// <param name="skipRows">The skip rows.</param>
    /// <param name="commentLine">The comment line.</param>
    /// <param name="fieldDelimiter">The field delimiter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public static async Task<Tuple<bool, string>> GuessHasHeader(
      this IImprovedStream improvedStream,
      int codePageId,
      int skipRows,
      string commentLine,
      string fieldDelimiter,
      CancellationToken cancellationToken)
    {
      using var reader = await improvedStream.GetStreamReaderAtStart(codePageId, skipRows, cancellationToken)
                           .ConfigureAwait(false);
      return GuessHasHeader(reader, commentLine, fieldDelimiter, cancellationToken);
    }

    public static async Task<string> GuessLineComment(
      this IImprovedStream improvedStream,
      int codePageId,
      int skipRows,
      CancellationToken cancellationToken)
    {
      using var textReader = await improvedStream.GetStreamReaderAtStart(codePageId, skipRows, cancellationToken)
                               .ConfigureAwait(false);
      return textReader.GuessLineComment(cancellationToken);
    }

    public static string GuessLineComment(this ImprovedTextReader textReader, CancellationToken cancellationToken)
    {
      if (textReader is null) throw new ArgumentNullException(nameof(textReader));
      const int c_MaxRows = 50;
      var lastRow = 0;
      Dictionary<string, int> starts =
        new[] { "##", "//", "\\\\", "''", "#", "/", "\\", "'" }.ToDictionary(test => test, test => 0);

      textReader.ToBeginning();
      // Count the number of rows that start with teh checked comment chars
      while (lastRow < c_MaxRows && !textReader.EndOfStream && !cancellationToken.IsCancellationRequested)
      {
        cancellationToken.ThrowIfCancellationRequested();
        var line = textReader.ReadLine().TrimStart();
        if (line.Length == 0)
          continue;
        lastRow++;
        foreach (var test in starts.Keys)
          if (line.StartsWith(test, StringComparison.Ordinal))
          {
            starts[test]++;
            // do not check further once a line is counted, by having ## before # a line starting
            // with ## will not be counted twice
            break;
          }
      }

      var maxCount = starts.Max(x => x.Value);
      if (maxCount > 0)
      {
        var check = starts.First(x => x.Value == maxCount);
        Logger.Information("Comment Line: {comment}", check.Key);
        return check.Key;
      }

      Logger.Information("No Comment Line");
      return string.Empty;
    }

    /// <summary>
    ///   Try to guess the new line sequence
    /// </summary>
    /// <param name="improvedStream">The improved stream.</param>
    /// <param name="codePageId">The code page identifier.</param>
    /// <param name="skipRows">The skip rows.</param>
    /// <param name="fieldQualifier">The field qualifier.</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The NewLine Combination used</returns>
    public static async Task<RecordDelimiterType> GuessNewline(
      this IImprovedStream improvedStream,
      int codePageId,
      int skipRows,
      string fieldQualifier,
      CancellationToken cancellationToken)
    {
      using var textReader = await improvedStream.GetStreamReaderAtStart(codePageId, skipRows, cancellationToken)
                               .ConfigureAwait(false);
      return textReader.GuessNewline(fieldQualifier, cancellationToken);
    }

    /// <summary>
    ///   Try to guess the new line sequence
    /// </summary>
    /// <param name="improvedStream">The improved stream.</param>
    /// <param name="codePageId">The code page identifier.</param>
    /// <param name="skipRows">The skip rows.</param>
    /// <param name="fieldDelimiter">The field delimiter.</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The NewLine Combination used</returns>
    public static async Task<string?> GuessQualifier(
      this IImprovedStream improvedStream,
      int codePageId,
      int skipRows,
      string fieldDelimiter,
      CancellationToken cancellationToken)
    {
      using var textReader = await improvedStream.GetStreamReaderAtStart(codePageId, skipRows, cancellationToken)
                               .ConfigureAwait(false);
      var qualifier = textReader.GuessQualifier(fieldDelimiter, cancellationToken);
      if (qualifier != '\0')
        return char.ToString(qualifier);
      return null;
    }

    /// <summary>
    ///   Guesses the start row of a CSV file Done with a rather simple csv parsing
    /// </summary>
    /// <param name="textReader">The stream reader with the data</param>
    /// <param name="delimiter">The delimiter.</param>
    /// <param name="quote">The quoting char</param>
    /// <param name="commentLine">The characters for a comment line.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of rows to skip</returns>
    /// <exception cref="ArgumentNullException">commentLine</exception>
    public static int GuessStartRow(
      this ImprovedTextReader textReader,
      string delimiter,
      string quote,
      string commentLine,
      CancellationToken cancellationToken)
    {
      if (textReader is null) throw new ArgumentNullException(nameof(textReader));
      if (commentLine is null)
        throw new ArgumentNullException(nameof(commentLine));
      const int c_MaxRows = 50;
      var delimiterChar = delimiter.WrittenPunctuationToChar();
      var quoteChar = quote.WrittenPunctuationToChar();
      textReader.ToBeginning();
      var columnCount = new List<int>(c_MaxRows);
      var rowMapping = new Dictionary<int, int>(c_MaxRows);
      var colCount = new int[c_MaxRows];
      var isComment = new bool[c_MaxRows];
      var quoted = false;
      var firstChar = true;
      var lastRow = 0;
      var retValue = 0;

      while (lastRow < c_MaxRows && !textReader.EndOfStream && !cancellationToken.IsCancellationRequested)
      {
        var readChar = textReader.Read();

        // Handle Commented lines
        if (firstChar && commentLine.Length > 0 && !isComment[lastRow] && readChar == commentLine[0])
        {
          isComment[lastRow] = true;

          for (var pos = 1; pos < commentLine.Length; pos++)
          {
            var nextChar = textReader.Peek();
            if (nextChar == commentLine[pos]) continue;
            isComment[lastRow] = false;
            break;
          }
        }

        // Handle Quoting
        if (readChar == quoteChar && !isComment[lastRow])
        {
          if (quoted)
          {
            if (textReader.Peek() != '"')
              quoted = false;
            else
              textReader.MoveNext();
          }
          else
          {
            quoted |= firstChar;
          }

          continue;
        }

        switch (readChar)
        {
          // Feed and NewLines
          case '\n':
            if (!quoted)
            {
              lastRow++;
              firstChar = true;
              if (textReader.Peek() == '\r')
                textReader.MoveNext();
            }

            break;

          case '\r':
            if (!quoted)
            {
              lastRow++;
              firstChar = true;
              if (textReader.Peek() == '\n')
                textReader.MoveNext();
            }

            break;

          default:
            if (!isComment[lastRow] && !quoted && readChar == delimiterChar)
            {
              colCount[lastRow]++;
              firstChar = true;
              continue;
            }

            break;
        }

        // Its still the first char if its a leading space
        if (firstChar && readChar != ' ')
          firstChar = false;
      }

      cancellationToken.ThrowIfCancellationRequested();
      // remove all rows that are comment lines...
      for (var row = 0; row < lastRow; row++)
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
        if (avg > 1)
          // If the first rows would be a good fit return this
          if (columnCount[0] < avg)
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

      Logger.Information("Start Row: {row}", retValue);
      return retValue;
    }

    /// <summary>
    ///   Determines the start row in the file
    /// </summary>
    /// <param name="improvedStream">The improved stream.</param>
    /// <param name="codePageID">The code page identifier.</param>
    /// <param name="fieldDelimiter">The field delimiter character.</param>
    /// <param name="fieldQualifier">The field qualifier character.</param>
    /// <param name="commentLine">The comment line.</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The number of rows to skip</returns>
    public static async Task<int> GuessStartRow(
      this IImprovedStream improvedStream,
      int codePageID,
      string fieldDelimiter,
      string fieldQualifier,
      string commentLine,
      CancellationToken cancellationToken)
    {
      using var streamReader = await improvedStream.GetStreamReaderAtStart(codePageID, 0, cancellationToken)
                                 .ConfigureAwait(false);
      return streamReader.GuessStartRow(fieldDelimiter, fieldQualifier, commentLine, cancellationToken);
    }

    /// <summary>
    ///   Does check if quoting was actually used in the file
    /// </summary>
    /// <param name="improvedStream">The improved stream.</param>
    /// <param name="codePageId">The code page identifier.</param>
    /// <param name="skipRows">The skip rows.</param>
    /// <param name="fieldDelimiter">The field delimiter character.</param>
    /// <param name="fieldQualifier">The field qualifier character.</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns><c>true</c> if [has used qualifier] [the specified setting]; otherwise, <c>false</c>.</returns>
    public static async Task<bool> HasUsedQualifier(
      this IImprovedStream improvedStream,
      int codePageId,
      int skipRows,
      string fieldDelimiter,
      string fieldQualifier,
      CancellationToken cancellationToken)
    {
      // if we do not have a quote defined it does not matter
      if (string.IsNullOrEmpty(fieldQualifier) || cancellationToken.IsCancellationRequested)
        return false;
      var fieldDelimiterChar = fieldDelimiter.WrittenPunctuationToChar();
      var fieldQualifierChar = fieldQualifier.WrittenPunctuationToChar();
      using var streamReader = await improvedStream.GetStreamReaderAtStart(codePageId, skipRows, cancellationToken)
                                 .ConfigureAwait(false);
      streamReader.ToBeginning();
      var isStartOfColumn = true;
      while (!streamReader.EndOfStream)
      {
        if (cancellationToken.IsCancellationRequested)
          return false;
        var c = (char) streamReader.Read();
        if (c == '\r' || c == '\n' || c == fieldDelimiterChar)
        {
          isStartOfColumn = true;
          continue;
        }

        // if we are not at the start of a column we can get the next char
        if (!isStartOfColumn)
          continue;
        // If we are at the start of a column and this is a ", we can stop, this is a real qualifier
        if (c == fieldQualifierChar)
          return true;
        // Any non whitespace will reset isStartOfColumn
        if (c <= '\x00ff')
          isStartOfColumn = c == ' ' || c == '\t';
        else
          isStartOfColumn = CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.SpaceSeparator;
      }

      return false;
    }

    /// <summary>
    ///   Determines whether data in the specified stream is a JSON
    /// </summary>
    /// <param name="impStream">The imp stream.</param>
    /// <param name="encoding">The encoding.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if json could be read from stream; otherwise, <c>false</c>.</returns>
    public static async Task<bool> IsJsonReadable(
      this IImprovedStream impStream,
      Encoding encoding,
      CancellationToken cancellationToken)
    {
      if (!(impStream is Stream stream))
        return false;

      impStream.Seek(0, SeekOrigin.Begin);
      using var streamReader = new StreamReader(stream, encoding, true, 4096, true);
      using var jsonTextReader = new JsonTextReader(streamReader);
      jsonTextReader.CloseInput = false;
      try
      {
        if (await jsonTextReader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
          Logger.Information("Detected Json file");
          if (jsonTextReader.TokenType == JsonToken.StartObject || jsonTextReader.TokenType == JsonToken.StartArray
                                                                || jsonTextReader.TokenType
                                                                == JsonToken.StartConstructor)
          {
            await jsonTextReader.ReadAsync(cancellationToken).ConfigureAwait(false);
            await jsonTextReader.ReadAsync(cancellationToken).ConfigureAwait(false);
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

    private static DelimiterCounter GetDelimiterCounter(
      this ImprovedTextReader textReader,
      string escape,
      int numRows,
      CancellationToken cancellationToken)
    {
      if (textReader is null) throw new ArgumentNullException(nameof(textReader));

      var dc = new DelimiterCounter(numRows);
      var escapeCharacter = escape.WrittenPunctuationToChar();
      var quoted = false;
      var firstChar = true;
      var readChar = -1;
      //var contends = new StringBuilder();
      var textReaderPosition = new ImprovedTextReaderPositionStore(textReader);

      while (dc.LastRow < dc.NumRows && !textReaderPosition.AllRead() && !cancellationToken.IsCancellationRequested)
      {
        var lastChar = readChar;
        readChar = textReader.Read();
        //contends.Append(readChar);
        if (lastChar == escapeCharacter)
          continue;
        switch (readChar)
        {
          case '"':
            if (quoted)
            {
              if (textReader.Peek() != '"')
                quoted = false;
              else
                textReader.MoveNext();
            }
            else
            {
              quoted |= firstChar;
            }

            break;

          case '\n':
          case '\r':
            if (!quoted && !firstChar)
            {
              dc.LastRow++;
              firstChar = true;
              continue;
            }

            break;

          default:
            if (!quoted)
            {
              var index = dc.Separators.IndexOf((char) readChar);
              if (index != -1)
              {
                if (dc.SeparatorsCount[index, dc.LastRow] == 0)
                  dc.SeparatorRows[index]++;
                ++dc.SeparatorsCount[index, dc.LastRow];
                firstChar = true;
                continue;
              }
            }

            break;
        }

        // Its still the first char if its a leading space
        if (firstChar && readChar != ' ')
          firstChar = false;
      }

      return dc;
    }

    private static IFileReader GetReaderFromDetectionResult(
      string fileName,
      DelimitedFileDetectionResult detectionResult,
      IProcessDisplay processDisplay)
    {
      if (detectionResult.IsJson)
        return new JsonFileReader(fileName, null, 1000, processDisplay: processDisplay);
      return new CsvFileReader(
        fileName,
        detectionResult.CodePageId,
        !detectionResult.HasFieldHeader && detectionResult.SkipRows == 0 ? 1 : detectionResult.SkipRows,
        detectionResult.HasFieldHeader,
        null,
        TrimmingOption.Unquoted,
        detectionResult.FieldDelimiter,
        detectionResult.FieldQualifier,
        detectionResult.EscapeCharacter,
        0L,
        false,
        false,
        detectionResult.CommentLine,
        0,
        true,
        "",
        "",
        "",
        true,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        true,
        false,
        "NULL",
        true,
        4,
        "",
        processDisplay);
    }

    private static async Task<ImprovedTextReader> GetStreamReaderAtStart(
      this IImprovedStream improvedStream,
      int codePageId,
      int skipRows,
      CancellationToken cancellationToken)
    {
      var textReader = new ImprovedTextReader(
        improvedStream,
        await improvedStream.CodePageResolve(codePageId, cancellationToken).ConfigureAwait(false),
        skipRows);
      textReader.ToBeginning();
      return textReader;
    }

    /// <summary>
    ///   Guesses the delimiter for a files. Done with a rather simple csv parsing, and trying to
    ///   find the delimiter that has the least variance in the read rows, if that is not possible
    ///   the delimiter with the highest number of occurrences.
    /// </summary>
    /// <param name="textReader">The StreamReader with the data</param>
    /// <param name="escapeCharacter">The escape character.</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A character with the assumed delimiter for the file</returns>
    /// <exception cref="ArgumentNullException">streamReader</exception>
    /// <remarks>No Error will not be thrown.</remarks>
    private static Tuple<string, bool> GuessDelimiter(
      this ImprovedTextReader textReader,
      string escapeCharacter,
      CancellationToken cancellationToken)
    {
      if (textReader is null)
        throw new ArgumentNullException(nameof(textReader));
      var match = '\0';

      var dc = textReader.GetDelimiterCounter(escapeCharacter, 300, cancellationToken);
      var numberOfRows = dc.FilledRows;

      // Limit everything to 100 columns max, the sum might get too big otherwise 100 * 100
      var startRow = dc.LastRow > 60 ? 15 : dc.LastRow > 20 ? 5 : 0;

      var neededRows = (dc.FilledRows > 20 ? numberOfRows * 75 : numberOfRows * 50) / 100;

      cancellationToken.ThrowIfCancellationRequested();
      var validSeparatorIndex = new List<int>();
      for (var index = 0; index < dc.Separators.Length; index++)
      {
        // only regard a delimiter if we have 75% of the rows with this delimiter we can still have
        // a lot of commented lines
        if (dc.SeparatorRows[index] == 0 || (dc.SeparatorRows[index] < neededRows && numberOfRows > 3))
          continue;
        validSeparatorIndex.Add(index);
      }

      if (validSeparatorIndex.Count == 0)
      {
        // we can not determine by the number of rows That the delimiter with most occurrence in general
        var maxNum = int.MinValue;
        for (var index = 0; index < dc.Separators.Length; index++)
        {
          var sumCount = 0;
          for (var row = startRow; row < dc.LastRow; row++)
            sumCount += dc.SeparatorsCount[index, row];
          if (sumCount > maxNum)
          {
            maxNum = sumCount;
            match = dc.Separators[index];
          }
        }
      }
      else if (validSeparatorIndex.Count == 1)
      {
        // if only one was found done here
        match = dc.Separators[validSeparatorIndex[0]];
      }
      else
      {
        // otherwise find the best
        foreach (var index in validSeparatorIndex)
          for (var row = startRow; row < dc.LastRow; row++)
            if (dc.SeparatorsCount[index, row] > 100)
              dc.SeparatorsCount[index, row] = 100;

        double? bestScore = null;
        var maxCount = 0;

        foreach (var index in validSeparatorIndex)
        {
          cancellationToken.ThrowIfCancellationRequested();
          var sumCount = 0;
          // If there are enough rows skip the first rows, there might be a descriptive introduction
          // this can not be done in case there are not many rows
          for (var row = startRow; row < dc.LastRow; row++)
            // Cut of at 50 Columns in case one row is messed up, this should not mess up everything
            sumCount += dc.SeparatorsCount[index, row];

          // If we did not find a match with variance use the absolute number of occurrences
          if (sumCount > maxCount && !bestScore.HasValue)
          {
            maxCount = sumCount;
            match = dc.Separators[index];
          }

          // Get the average of the rows
          var avg = (int) Math.Round((double) sumCount / (dc.LastRow - startRow), 0, MidpointRounding.AwayFromZero);

          // Only proceed if there is usually more then one occurrence and we have more then one row
          if (avg < 1 || dc.SeparatorRows[index] == 1)
            continue;

          // First determine the variance, low value means and even distribution
          double cutVariance = 0;
          for (var row = startRow; row < dc.LastRow; row++)
          {
            var dist = dc.SeparatorsCount[index, row] - avg;
            if (dist > 2 || dist < -2)
              cutVariance += 8;
            else
              switch (dist)
              {
                case 2:
                case -2:
                  cutVariance += 4;
                  break;

                case 1:
                case -1:
                  cutVariance++;
                  break;
              }
          }

          // The score is dependent on the average columns found and the regularity
          var score = Math.Abs(avg - Math.Round(cutVariance / (dc.LastRow - startRow), 2));
          if (bestScore.HasValue && !(score > bestScore.Value))
            continue;
          match = dc.Separators[index];
          bestScore = score;
        }
      }

      var hasDelimiter = match != '\0';
      if (!hasDelimiter)
      {
        Logger.Information("Not a delimited file");
        return new Tuple<string, bool>("Tab", false);
      }

      var result = match == '\t' ? "Tab" : match.ToString(CultureInfo.CurrentCulture);
      Logger.Information("Column Delimiter: {delimiter}", result);
      return new Tuple<string, bool>(result, true);
    }

    private static RecordDelimiterType GuessNewline(
      this ImprovedTextReader textReader,
      string fieldQualifier,
      CancellationToken token)
    {
      if (textReader is null) throw new ArgumentNullException(nameof(textReader));
      const int c_NumChars = 8192;

      var currentChar = 0;
      var quoted = false;

      const int c_Cr = 0;
      const int c_LF = 1;
      const int c_CrLf = 2;
      const int c_LFCr = 3;
      const int c_RecSep = 4;
      const int c_UnitSep = 5;

      int[] count = { 0, 0, 0, 0, 0, 0 };

      // \r = CR (Carriage Return) \n = LF (Line Feed)
      var fieldQualifierChar = fieldQualifier.WrittenPunctuationToChar();
      var textReaderPosition = new ImprovedTextReaderPositionStore(textReader);
      while (currentChar < c_NumChars && !textReaderPosition.AllRead() && !token.IsCancellationRequested)
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
            count[c_RecSep]++;
            continue;
          case 31:
            count[c_UnitSep]++;
            continue;
          case 10:
          {
            if (textReader.Peek() == 13)
            {
              textReader.MoveNext();
              count[c_LFCr]++;
            }
            else
            {
              count[c_LF]++;
            }

            currentChar++;
            break;
          }
          case 13:
          {
            if (textReader.Peek() == 10)
            {
              textReader.MoveNext();
              count[c_CrLf]++;
            }
            else
            {
              count[c_Cr]++;
            }

            break;
          }
        }

        currentChar++;
      }

      var maxCount = count.Max();
      if (maxCount == 0)
        return RecordDelimiterType.None;
      var res = count[c_RecSep] == maxCount ? RecordDelimiterType.RS :
                count[c_UnitSep] == maxCount ? RecordDelimiterType.US :
                count[c_Cr] == maxCount ? RecordDelimiterType.CR :
                count[c_LF] == maxCount ? RecordDelimiterType.LF :
                count[c_LFCr] == maxCount ? RecordDelimiterType.LFCR :
                count[c_CrLf] == maxCount ? RecordDelimiterType.CRLF : RecordDelimiterType.None;
      Logger.Information("Record Delimiter: {recorddelimiter}", res.Description());
      return res;
    }

    private static char GuessQualifier(
      this ImprovedTextReader textReader,
      string delimiter,
      CancellationToken cancellationToken)
    {
      if (textReader is null) throw new ArgumentNullException(nameof(textReader));
      var delimiterChar = delimiter.WrittenPunctuationToChar();
      const int c_MaxLine = 30;
      var possibleQuotes = new[] { '"', '\'' };
      var counter = new int[possibleQuotes.Length];

      var textReaderPosition = new ImprovedTextReaderPositionStore(textReader);
      var max = 0;
      // skip the first line it usually a header
      for (var lineNo = 0;
           lineNo < c_MaxLine && !textReaderPosition.AllRead() && !cancellationToken.IsCancellationRequested;
           lineNo++)
      {
        var line = textReader.ReadLine();
        if (string.IsNullOrEmpty(line))
        {
          if (textReader.EndOfStream && !textReaderPosition.CanStartFromBeginning())
            break;
          continue;
        }

        var cols = line.Split(delimiterChar);
        foreach (var col in cols)
        {
          if (string.IsNullOrWhiteSpace(col))
            continue;

          var test = col.Trim();
          for (var testChar = 0; testChar < possibleQuotes.Length; testChar++)
          {
            if (test[0] != possibleQuotes[testChar]) continue;
            counter[testChar]++;
            // Ideally column need to start and end with the same characters (but end quote could be
            // on another line) if the start and end are indeed the same give it extra credit
            if (test.Length > 1 && test[0] == test[test.Length - 1])
              counter[testChar]++;
            if (counter[testChar] > max)
              max = counter[testChar];
          }
        }
      }

      var res = max < 1 ? '\0' : possibleQuotes.Where((t, testChar) => counter[testChar] == max).FirstOrDefault();
      if (res != '\0')
        Logger.Information("Column Qualifier: {qualifier}", res);
      else
        Logger.Information("No Column Qualifier");
      return res;
    }
  }
}