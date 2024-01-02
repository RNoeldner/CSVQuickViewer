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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace CsvTools
{
  /// <inheritdoc cref="CsvTools.IFileReader" />
  /// <summary>
  ///   Json text file reader
  /// </summary>
  public sealed class XmlFileReader : BaseFileReaderTyped, IFileReader
  {
    private Stream? m_Stream;
    private readonly XmlDocument m_Doc = new XmlDocument();
    // private XmlReader? m_XmlReader;
    private XmlNode? m_CurrentNode;

    // ReSharper disable once UnusedMember.Global
    public XmlFileReader(
      in Stream stream,
      in IEnumerable<Column>? columnDefinition,
      long recordLimit,
      bool trim,
      in string treatTextAsNull,
      bool treatNbspAsSpace,
      in TimeZoneChangeDelegate timeZoneAdjust,
      string destTimeZone,
      bool allowPercentage,
      bool removeCurrency)
      : base(string.Empty, columnDefinition, recordLimit, trim, treatTextAsNull, treatNbspAsSpace, timeZoneAdjust, destTimeZone, allowPercentage, removeCurrency) =>
      m_Stream = stream;

    public XmlFileReader(in string fileName,
      in IEnumerable<Column>? columnDefinition,
      long recordLimit,
      bool trim,
      string treatTextAsNull,
      bool treatNbspAsSpace,
      in TimeZoneChangeDelegate timeZoneAdjust,
      string destTimeZone,
      bool allowPercentage,
    bool removeCurrency)
      : base(fileName, columnDefinition, recordLimit, trim, treatTextAsNull, treatNbspAsSpace, timeZoneAdjust, destTimeZone, allowPercentage, removeCurrency)
    {
      if (string.IsNullOrEmpty(fileName))
        throw new ArgumentException("File can not be null or empty", nameof(fileName));
      if (!FileSystemUtils.FileExists(fileName))
        throw new FileNotFoundException(
          $"The file '{FileSystemUtils.GetShortDisplayFileName(fileName)}' does not exist or is not accessible.",
          fileName);
    }

    /// <inheritdoc />
    /// <summary>
    ///   Gets a value indicating whether this instance is closed.
    /// </summary>
    /// <value><c>true</c> if this instance is closed; otherwise, <c>false</c>.</value>
    public override bool IsClosed => m_Doc.ChildNodes.Count == 0;

    public override void Close()
    {
      base.Close();
      m_Doc.RemoveAll();

      if (!SelfOpenedStream) return;
      m_Stream?.Dispose();
      m_Stream = null;
    }

    public new void Dispose() => Dispose(true);

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    public new async ValueTask DisposeAsync()
    {
      if (m_Stream != null)
        await m_Stream.DisposeAsync().ConfigureAwait(false);
      Dispose(false);
    }
#endif

    private static IDictionary<string, string> ReadNode(XmlNode? check)
    {
      var columns = new Dictionary<string, string>();
      if (check != null)
      {
        if (check.ChildNodes.Count>0)
        {
          foreach (XmlNode attributes in check.ChildNodes)
          {
            if (attributes is null)
              continue;
            if (!columns.ContainsKey(attributes.Name))
              columns.Add(attributes.Name, attributes.InnerText?? string.Empty);
          }
        }
        if (check.Attributes != null)
        {
          foreach (XmlAttribute attributes in check.Attributes)
          {
            if (attributes is null)
              continue;
            if (!columns.ContainsKey($"_{attributes.Name}"))
              columns.Add($"_{attributes.Name}", attributes.InnerText?? string.Empty);
          }
        }
      }
      return columns;
    }

    public static string GetFullPath(XmlNode node) => 
      node.ParentNode is null ? string.Empty : $"{GetFullPath(node.ParentNode)}\\{node.ParentNode.Name}";

    public override async Task OpenAsync(CancellationToken token)
    {
      HandleShowProgress($"Opening XML file {FileName}", 0);
      await BeforeOpenAsync($"Opening XML file {FileSystemUtils.GetShortDisplayFileName(FileName)}")
        .ConfigureAwait(false);
      Retry:
      try
      {
        ResetPositionToStartOrOpen();
        // load the XML        
        //m_XmlReader = XmlReader.Create(m_Stream!);
        //m_XmlReader.ReadStartElement();

        m_Doc.Load(m_Stream!);
        // Support a root node, or an array                
        m_CurrentNode  = GetStartNode();
        if (m_CurrentNode != null)
          Logger.Information($"Start Node: {GetFullPath(m_CurrentNode)}");

        List<string> columnNames = new List<string>();
        for (int i = 0; i<10 && m_CurrentNode !=null; i++)
        {
          var values = ReadNode(m_CurrentNode);
          foreach (var column in values)
          {
            if (!columnNames.Contains(column.Key))
              columnNames.Add(column.Key);
          }
          m_CurrentNode = m_CurrentNode.NextSibling;
        }
        m_CurrentNode  = GetStartNode();

        // get column names for some time
        InitColumn(columnNames.Count);
        ParseColumnName(columnNames);
        FinishOpen();
        ResetPositionToStartOrOpen();
      }
      catch (Exception ex)
      {
        if (ShouldRetry(ex, token))
          goto Retry;
        Close();
        var appEx = new FileReaderException(
          "Error opening XML text file for reading.\nPlease make sure the file does exist, is of the right type and is not locked by another process.",
          ex);
        HandleError(-1, appEx.ExceptionMessages());
        HandleReadFinished();
        throw appEx;
      }
    }

    public override bool Read()
    {
      if (!EndOfFile)
      {
        var couldRead = GetNextRecord();
        if (couldRead) RecordNumber++;
        InfoDisplay(couldRead);

        if (couldRead && !IsClosed && RecordNumber <= RecordLimit)
          return true;
      }

      EndOfFile = true;
      HandleReadFinished();
      return false;
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public override async Task<bool> ReadAsync(CancellationToken cancellationToken)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
      return cancellationToken.IsCancellationRequested ? false : Read();
    }

    public override void ResetPositionToFirstDataRow() => ResetPositionToStartOrOpen();

    protected override void Dispose(bool disposing)
    {
      if (disposing) m_Stream?.Dispose();
      m_Stream = null;
    }

    /// <inheritdoc />
    /// <summary>
    ///   Gets the relative position.
    /// </summary>
    /// <returns>A value between 0 and MaxValue</returns>
    protected override double GetRelativePosition()
    {
      if (m_Stream is IImprovedStream imp)
        return imp.Percentage;

      return base.GetRelativePosition();
    }

    /// <summary>
    ///   Reads a data row from the JsonTextReader and stores the values and text, this will flatten
    ///   the structure of the Json file
    /// </summary>
    /// <returns>A collection with name and value of the properties</returns>
    private bool GetNextRecord()
    {
      try
      {
        // sore the parent Property Name in parentKey
        var keyValuePairs = ReadNode(m_CurrentNode);
        m_CurrentNode = m_CurrentNode?.NextSibling;

        StartLineNumber = RecordNumber;
        EndLineNumber = RecordNumber;

        var columnNumber = 0;
        foreach (var col in Column)
        {
          if (keyValuePairs.TryGetValue(col.Name, out var value))
            CurrentValues[columnNumber] = value;
          columnNumber++;
        }

        if (keyValuePairs.Count < FieldCount)
          HandleWarning(
            -1,
            $"Line {StartLineNumber} has fewer columns than expected ({keyValuePairs.Count}/{FieldCount}).");
        else if (keyValuePairs.Count > FieldCount)
          HandleWarning(
            -1,
            $"Line {StartLineNumber} has more columns than expected ({keyValuePairs.Count}/{FieldCount}). The data in extra columns is not read.");

        return keyValuePairs.Count>0;
      }
      catch (Exception ex)
      {
        // A serious error will be logged and its assume the file is ended
        HandleError(-1, ex.Message);
        EndOfFile = true;
        return false;
      }
    }

    private XmlNode? GetStartNode()
    {
      var items = m_Doc.ChildNodes.OfType<XmlNode>().Where(x => x.NodeType == XmlNodeType.Element).ToList();
      // go deeper until we have some a list of child nodes
      while (items != null && items.Count ==1)
        items = items.FirstOrDefault(x => x.ChildNodes.Count >1)?.ChildNodes.OfType<XmlNode>().ToList();
      return (items?.Count ?? 0) >0 ? items?.First() : null;
    }

    /// <summary>
    ///   Resets the position and buffer to the first line, excluding headers, use
    ///   ResetPositionToStart if you want to go to first data line
    /// </summary>
    private void ResetPositionToStartOrOpen()
    {
      if (SelfOpenedStream)
      {
        // Better would be DisposeAsync(), but method is synchronous
        m_Stream?.Dispose();
        m_Stream = FunctionalDI.GetStream(new SourceAccess(FullPath));
      }
      else
      {
        m_Stream!.Seek(0, SeekOrigin.Begin);
      }

      // End Line should be at 1, later on as the line is read the start line s set to this value
      StartLineNumber = 1;
      EndLineNumber = 1;
      RecordNumber = 0;

      EndOfFile = false;

      m_CurrentNode  = GetStartNode();
    }


  }
}