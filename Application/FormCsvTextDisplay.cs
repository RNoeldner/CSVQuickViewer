/*
 * Copyright (C) 2014 Raphael Nöldner
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
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace CsvTools
{
  /// <summary>
  ///   UserControl: CsvTextDisplay
  /// </summary>
  public partial class FormCsvTextDisplay : ResizeForm
  {
    private int m_CodePage;
    private int m_DisplayedAt = -1;
    [NotNull] private string m_FullPath = string.Empty;
    private bool m_IsReading;
    private int m_SkipLines;
    private bool m_StopReading;

    /// <summary>
    ///   CTOR CsvTextDisplay
    /// </summary>
    public FormCsvTextDisplay() => InitializeComponent();

    /// <summary>
    ///   CSV File to display
    /// </summary>
    public async Task SetCsvFileAsync(string fullPath, char qualifierChar, char delimiterChar, char escapeChar,
      int codePage, int skipLines)
    {
      Text = fullPath;
      if (string.IsNullOrEmpty(fullPath))
      {
        ScrollBarVertical.Visible = false;
        CSVTextBox.Text = null;
      }
      else
      {
        var info = new FileSystemUtils.FileInfo(fullPath);
        if (!info.Exists)
        {
          ScrollBarVertical.Visible = false;
          CSVTextBox.Text = $@"
The file {fullPath} does not exist.";
        }
        else
        {
          m_FullPath = fullPath;
          m_CodePage = codePage;
          CSVTextBox.DisplaySpace = true;
          CSVTextBox.Quote = qualifierChar;
          CSVTextBox.Delimiter = delimiterChar;
          CSVTextBox.Escape = escapeChar;
          CSVTextBox.SkipLines = skipLines;
          m_SkipLines = skipLines;

          // read all and display all
          if (info.Length < 64000)
          {
            try
            {
              var display = await GetTextAsync(0, 64000);
              CSVTextBox.Text = display;
              ScrollBarVertical.Visible = false;
            }
            catch (Exception ex)
            {
              CSVTextBox.Text = ex.Message;
            }
          }
          // file is too big, read whats displayed at time of display
          else
          {
            CSVTextBox.ShowLineNumber = false;
            CSVTextBox.MouseWheel += MouseWheelScroll;
            ScrollBarVertical.Visible = true;
            ScrollBarVertical.SmallChange = 1024;
            ScrollBarVertical.LargeChange = 8192;
            ScrollBarVertical.Maximum = info.Length.ToInt();
            ValueChangedEvent(this, null);
          }
        }
      }
    }

    private void MouseWheelScroll(object sender, MouseEventArgs e)
    {
      var newValue = ScrollBarVertical.Value - e.Delta;

      if (newValue < ScrollBarVertical.Minimum)
        newValue = ScrollBarVertical.Minimum;

      if (newValue > ScrollBarVertical.Maximum)
        newValue = ScrollBarVertical.Maximum;

      ScrollBarVertical.Value = newValue;
    }

    private async Task<string> GetTextAsync(int newPos, int maxChar)
    {
      try
      {
        using (var iStream = FunctionalDI.OpenRead(m_FullPath))
        {
          if (m_StopReading) throw new OperationCanceledException();

          if (newPos != 0)
            iStream.Stream.Seek(newPos, SeekOrigin.Begin);

          if (m_StopReading) throw new OperationCanceledException();

          using (var stream = new StreamReader(iStream.Stream, Encoding.GetEncoding(m_CodePage), false))
          {
            var buffer = new char[maxChar];
            var pos = 0;
            var readChars = await stream.ReadAsync(buffer, 0, maxChar);
            if (newPos != 0)
            {
              // get to the line start, position might be in the middle of a line
              while (pos < readChars)
              {
                var chr = buffer[pos++];
                if (chr != '\r' && chr != '\n') continue;
                if (pos + 1 < readChars)
                {
                  var nextChar = buffer[pos + 1];
                  if ((chr == '\r' && nextChar == '\n') || (chr == '\n' && nextChar == '\r'))
                    pos++;
                }

                break;
              }

              pos++;
            }

            if (m_StopReading) throw new OperationCanceledException();

            return new string(buffer, pos, readChars - pos);
          }
        }
      }
      catch (OperationCanceledException)
      {
        throw;
      }
      catch (Exception exc)
      {
        return exc.ExceptionMessages();
      }
      finally
      {
        m_IsReading = false;
      }
    }

    private async void ValueChangedEvent(object sender, EventArgs e)
    {
      if (m_DisplayedAt == ScrollBarVertical.Value) return;
      if (m_IsReading)
      {
        m_StopReading = true;
        // wait for it to finish
        await Task.Delay(200);
      }

      try
      {
        CSVTextBox.SkipLines = (ScrollBarVertical.Value == 0) ? m_SkipLines : 0;
        CSVTextBox.Text = await GetTextAsync(ScrollBarVertical.Value, 32000);
        m_DisplayedAt = ScrollBarVertical.Value;
      }
      catch
      {
        // ignore
      }

      m_StopReading = false;
    }
  }
}