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
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

#nullable enable

namespace CsvTools
{
  /// <summary>
  ///   Class to provide HTML Copy and Past functionality to a DataGrid
  /// </summary>
  public sealed class DataGridViewCopyPaste
  {
    /// <summary>
    ///   The column header for error information
    /// </summary>
    private const string cErrorInfo = "Error Information";

    /// <summary>
    ///   The used HTML style
    /// </summary>
    private readonly HtmlStyle m_HtmlStyle;

    /// <summary>
    ///   Initializes a new instance of the <see cref="DataGridViewCopyPaste" /> class.
    /// </summary>
    /// <param name="htmlStyle">The HTML style.</param>
    public DataGridViewCopyPaste(in HtmlStyle? htmlStyle)
    {
      m_HtmlStyle = htmlStyle ?? HtmlStyle.Default;
    }

    /// <summary>
    ///   Copies the selected Cells into clipboard.
    /// </summary>
    /// <param name="dataGridView">The data grid view.</param>
    /// <param name="addErrorInfo">if set to <c>true</c> add error information.</param>
    /// <param name="cutLength">if set to <c>true</c> cut off long text.</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    public void SelectedDataIntoClipboard(
      DataGridView dataGridView,
      bool addErrorInfo,
      bool cutLength,
      CancellationToken cancellationToken)
    {
      dataGridView.RunWithHourglass(() =>
      {
        if (dataGridView.GetCellCount(DataGridViewElementStates.Selected) == 1)
          CopyOneCellIntoClipboard(dataGridView.SelectedCells[0]);
        else if (dataGridView.AreAllCellsSelected(false))
          CopyAllCellsIntoClipboard(
            addErrorInfo,
            cutLength,
            !Equals(dataGridView.AlternatingRowsDefaultCellStyle, dataGridView.RowsDefaultCellStyle),
            dataGridView.Columns,
            dataGridView.Rows,
            cancellationToken);
        else
          CopySelectedCellsIntoClipboard(
            addErrorInfo,
            cutLength,
            !Equals(dataGridView.AlternatingRowsDefaultCellStyle, dataGridView.RowsDefaultCellStyle),
            dataGridView.Columns,
            dataGridView.Rows,
            dataGridView.SelectedCells,
            cancellationToken);
      });
    }

    /// <summary>
    ///   Copies the one cell to the clipboard
    /// </summary>
    private static void CopyOneCellIntoClipboard(DataGridViewCell cell)
    {
      var dataObject = new DataObject();
      dataObject.SetData(DataFormats.Text, true, cell.Value.ToString());
      dataObject.SetClipboard();
    }

    /// <summary>
    ///   Escapes any tab in a text
    /// </summary>
    /// <param name="contents">The contents.</param>
    /// <returns></returns>
    private static string EscapeTab(string? contents)
    {
      if (contents is null || contents.Length == 0)
        return string.Empty;
      if (contents.Contains("\t") || contents.Contains("\n") || contents.Contains("\r"))
        return "\"" + contents.Replace("\"", "\"\"") + "\"";
      return contents;
    }

    /// <summary>
    ///   Determines whether there are row errors in the specified rows.
    /// </summary>
    /// <param name="topRow">The top row.</param>
    /// <param name="bottomRow">The bottom row.</param>
    /// <param name="rows">The rows.</param>
    /// <returns><c>true</c> if it has row errors; otherwise, <c>false</c>.</returns>
    private static bool HasRowErrors(int topRow, int bottomRow, DataGridViewRowCollection rows)
    {
      var hasRowErrors = false;
      for (var row = topRow; row < bottomRow; row++)
        if (!string.IsNullOrEmpty(rows[row].ErrorText))
        {
          hasRowErrors = true;
          break;
        }

      return hasRowErrors;
    }

    /// <summary>
    ///   Adds a cell value to the HTML and Text
    /// </summary>
    /// <param name="cell">The cell.</param>
    /// <param name="stringBuilder">The buffer.</param>
    /// <param name="sbHtml">The StringBuilder for HTML.</param>
    /// <param name="appendTab">if set to <c>true</c> [append tab].</param>
    /// <param name="addErrorInfo">if set to <c>true</c> [add error info].</param>
    /// <param name="cutLength">Maximum length of the resulting text</param>
    private static void AddCell(
      DataGridViewCell cell,
      StringBuilder stringBuilder,
      StringBuilder sbHtml,
      bool appendTab,
      bool addErrorInfo,
      bool cutLength)
    {
      var cellValue = cell.FormattedValue?.ToString() ?? string.Empty;
      if (cellValue.Length > 500 && cutLength)
        cellValue = cellValue.Substring(0, 80) + " […] " + cellValue.Substring(cellValue.Length - 20, 20);

      if (appendTab)
        stringBuilder.Append('\t');
      stringBuilder.Append(EscapeTab(cellValue));
      HtmlStyle.AddHtmlCell(
        sbHtml,
        cell.ValueType == typeof(string) ? HtmlStyle.Td : HtmlStyle.TdNonText,
        cellValue,
        cell.ErrorText,
        addErrorInfo);
    }

    /// <summary>
    ///   Appends a row error to the HTML Buffer
    /// </summary>
    /// <param name="stringBuilder">The buffer.</param>
    /// <param name="sbHtml">The StringBuilder for HTML.</param>
    /// <param name="errorText">The error Text</param>
    /// <param name="addErrorInfo">if set to <c>true</c> [add error info].</param>
    private static void AppendRowError(StringBuilder stringBuilder, StringBuilder sbHtml, string? errorText,
      bool addErrorInfo)
    {
      if (!addErrorInfo)
        return;
      if (errorText is null || errorText.Length == 0)
        sbHtml.Append(HtmlStyle.TdEmpty);
      else
        HtmlStyle.AddHtmlCell(sbHtml, HtmlStyle.Td, string.Empty, errorText, true);

      stringBuilder.Append('\t');
      stringBuilder.Append(EscapeTab(errorText));
    }

    /// <summary>
    ///   Copies all cells to the clipboard
    /// </summary>
    /// <param name="addErrorInfo">if set to <c>true</c> [add error information].</param>
    /// <param name="cutLength">if set to <c>true</c> [cut length].</param>
    /// <param name="alternatingRows">if set to <c>true</c> [alternating rows].</param>
    /// <param name="columns">The columns.</param>
    /// <param name="rows">The rows.</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    private void CopyAllCellsIntoClipboard(
      bool addErrorInfo,
      bool cutLength,
      bool alternatingRows,
      DataGridViewColumnCollection columns,
      DataGridViewRowCollection rows,
      CancellationToken cancellationToken)
    {
      var buffer = new StringBuilder();
      var sbHtml = new StringBuilder(HtmlStyle.TableOpen);

      sbHtml.Append(HtmlStyle.TrOpenAlt);
      var first = true;

      var visibleColumns = new SortedDictionary<int, DataGridViewColumn>();
      foreach (DataGridViewColumn c in columns)

        // Do not include the error field it will be retrieved from the error collection with nice coloring
        if (c.Visible && c.HeaderText != ReaderConstants.cErrorField)
          visibleColumns.Add(c.DisplayIndex, c);
      var hasRowError = HasRowErrors(0, rows.Count, rows);
      foreach (var headerText in from col in visibleColumns.Values
               let headerText = col.HeaderText
               select headerText)
      {
        sbHtml.Append(HtmlStyle.AddTd(HtmlStyle.Th, headerText));
        if (!first)
          buffer.Append('\t');
        else
          first = false;
        buffer.Append(EscapeTab(headerText));
      }

      if (hasRowError && addErrorInfo)
      {
        sbHtml.Append(HtmlStyle.AddTd(HtmlStyle.Th, cErrorInfo));
        buffer.Append('\t');
        buffer.Append(cErrorInfo);
      }

      sbHtml.AppendLine(HtmlStyle.TrClose);
      buffer.AppendLine();

      var trAlternate = false;
      var lastRefresh = DateTime.Now;
      for (var row = 0; row < rows.Count; row++)
      {
        if (rows[row].IsNewRow)
          break;
        sbHtml.Append(trAlternate ? HtmlStyle.TrOpenAlt : HtmlStyle.TrOpen);
        if (alternatingRows)
          trAlternate = !trAlternate;
        first = true;
        foreach (var col in visibleColumns.Values)
        {
          if (cancellationToken.IsCancellationRequested)
            return;
          var cell = rows[row].Cells[col.Index];
          AddCell(cell, buffer, sbHtml, !first, addErrorInfo, cutLength);
          first = false;
        }

        AppendRowError(buffer, sbHtml, rows[row].ErrorText, addErrorInfo && hasRowError);
        sbHtml.AppendLine(HtmlStyle.TrClose);
        buffer.AppendLine();
        if ((DateTime.Now - lastRefresh).TotalSeconds <= 0.2)
          continue;
        lastRefresh = DateTime.Now;
        Extensions.ProcessUIElements();
      }

      sbHtml.AppendLine(HtmlStyle.TableClose);

      var dataObject = new DataObject();
      dataObject.SetData(DataFormats.Html, true, m_HtmlStyle.ConvertToHtmlFragment(sbHtml.ToString()));
      dataObject.SetData(DataFormats.Text, true, buffer.ToString());
      dataObject.SetClipboard();
    }

    /// <summary>
    ///   Copies the selected cells into the clipboard.
    /// </summary>
    /// <param name="addErrorInfo">if set to <c>true</c> [add error information].</param>
    /// <param name="cutLength">if set to <c>true</c> [cut length].</param>
    /// <param name="alternatingRows">if set to <c>true</c> [alternating rows].</param>
    /// <param name="columns">The columns.</param>
    /// <param name="rows">The rows.</param>
    /// <param name="selectedCells">The selected cells.</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    private void CopySelectedCellsIntoClipboard(
      bool addErrorInfo,
      bool cutLength,
      bool alternatingRows,
      DataGridViewColumnCollection columns,
      DataGridViewRowCollection rows,
      DataGridViewSelectedCellCollection selectedCells,
      CancellationToken cancellationToken)
    {
      var buffer = new StringBuilder();
      var dataObject = new DataObject();

      // If there are multiple cells Add a header and a neat HTML table
      var sbHtml = new StringBuilder(HtmlStyle.TableOpen);

      var leftCol = int.MaxValue;
      var rightCol = int.MinValue;
      var topRow = int.MaxValue;
      var bottomRow = int.MinValue;

      foreach (DataGridViewCell cell in selectedCells)
      {
        if (cell.OwningColumn.DisplayIndex < leftCol)
          leftCol = cell.OwningColumn.DisplayIndex;
        if (cell.OwningColumn.DisplayIndex > rightCol)
          rightCol = cell.OwningColumn.DisplayIndex;
        if (cell.RowIndex < topRow)
          topRow = cell.RowIndex;
        if (cell.RowIndex > bottomRow)
          bottomRow = cell.RowIndex;
      }

      var hasRowError = HasRowErrors(topRow, bottomRow + 1, rows);
      var visibleColumns = new List<int>();

      sbHtml.Append(HtmlStyle.TrOpenAlt);
      for (var col = leftCol; col <= rightCol; col++)
        foreach (DataGridViewColumn displayCol in columns)
          if (displayCol.DisplayIndex == col && displayCol.Visible &&
              displayCol.HeaderText != ReaderConstants.cErrorField)
          {
            visibleColumns.Add(col);
            sbHtml.Append(HtmlStyle.AddTd(HtmlStyle.Th, displayCol.HeaderText));
            if (buffer.Length > 0)
              buffer.Append('\t');

            buffer.Append(EscapeTab(displayCol.HeaderText));
            break;
          }

      if (hasRowError && addErrorInfo)
      {
        sbHtml.Append(HtmlStyle.AddTd(HtmlStyle.Th, cErrorInfo));
        buffer.Append('\t');
        buffer.Append(cErrorInfo);
      }

      sbHtml.AppendLine(HtmlStyle.TrClose);
      buffer.AppendLine();

      var trAlternate = false;
      var lastRefresh = DateTime.Now;
      for (var row = topRow; row <= bottomRow; row++)
      {
        if (rows[row].IsNewRow)
          continue;
        if (cancellationToken.IsCancellationRequested)
          return;
        sbHtml.Append(trAlternate ? HtmlStyle.TrOpenAlt : HtmlStyle.TrOpen);
        if (alternatingRows)
          trAlternate = !trAlternate;
        var written = false;
        if (cancellationToken.IsCancellationRequested)
          return;
        for (var col = leftCol; col <= rightCol; col++)
        {
          if (!visibleColumns.Contains(col))
            continue;
          foreach (DataGridViewCell cell in selectedCells)
          {
            if (cell.RowIndex != row)
              continue;
            if (cell.OwningColumn.DisplayIndex != col)
              continue;
            AddCell(cell, buffer, sbHtml, col > leftCol, addErrorInfo, cutLength);
            written = true;
            break;
          }

          if (written)
            continue;
          buffer.Append('\t');
          sbHtml.Append(HtmlStyle.TdEmpty);
        }

        AppendRowError(buffer, sbHtml, rows[row].ErrorText, addErrorInfo && hasRowError);

        sbHtml.AppendLine(HtmlStyle.TrClose);
        buffer.AppendLine();
        if ((DateTime.Now - lastRefresh).TotalSeconds <= 0.2)
          continue;
        lastRefresh = DateTime.Now;
        Extensions.ProcessUIElements();
      }

      sbHtml.AppendLine(HtmlStyle.TableClose);
      dataObject.SetData(DataFormats.Html, true, m_HtmlStyle.ConvertToHtmlFragment(sbHtml.ToString()));
      dataObject.SetData(DataFormats.Text, true, buffer.ToString());
      dataObject.SetClipboard();
    }
  }
}