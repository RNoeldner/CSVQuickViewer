/*
 * Copyright (C) 2014 Raphael Nöldner : http://CSVReshaper.com
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
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace CsvTools
{
  /// <summary>
  ///   UserControl: CsvTextDisplay
  /// </summary>
  public partial class CsvTextDisplay : UserControl
  {
    private int m_CodePage;

    private int m_DisplayedAt;
    [NotNull] private string m_FullPath = string.Empty;

    /// <summary>
    ///   CTOR CsvTextDisplay
    /// </summary>
    public CsvTextDisplay()
    {
      InitializeComponent();
      CSVTextBox.MouseWheel += MouseWheelScroll;
    }

    /// <summary>
    ///   CSV File to display
    /// </summary>
    public async Task SetCsvFile(string fullPath, char qualifierChar, char delimiterChar, char escapeChar, int codePage)
    {
      if (string.IsNullOrEmpty(fullPath))
      {
        CSVTextBox.Text = null;
      }
      else
      {
        if (!FileSystemUtils.FileExists(fullPath))
        {
          CSVTextBox.DisplaySpace = false;
          CSVTextBox.Text = $@"
The file {fullPath} does not exist.";
        }
        else
        {
          CSVTextBox.Text = null;
          CSVTextBox.DisplaySpace = true;
          CSVTextBox.Quote = qualifierChar;
          CSVTextBox.Delimiter = delimiterChar;
          CSVTextBox.Escape = escapeChar;

          ScrollBarVertical.LargeChange = 4096;
          ScrollBarVertical.Maximum = string.IsNullOrEmpty(fullPath) ? 0 : FileSystemUtils.FileLength(fullPath).ToInt();
          m_FullPath = fullPath;
          m_CodePage = codePage;

          // Starting task without error handler
          await UpdateViewAsync();
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

    private async void ScrollEvent(object sender, ScrollEventArgs e)
    {
      if (m_DisplayedAt != ScrollBarVertical.Value && ScrollBarVertical.Enabled)
        await UpdateViewAsync();
    }

    private void SizeChangedEvent(object sender, EventArgs e) => CSVTextBox.Width = ScrollBarVertical.Left;

    private async Task UpdateViewAsync()
    {
      m_DisplayedAt = ScrollBarVertical.Value;
      if (string.IsNullOrEmpty(m_FullPath))
        return;
      var display = string.Empty;
      try
      {
        using (var iStream = FunctionalDI.OpenRead(m_FullPath))
        {
          using (var sr = new ImprovedTextReader(iStream, m_CodePage))
          {
            // Some stream do not support seek...
            if (iStream.Stream.CanSeek)
            {
              iStream.Stream.Seek(m_DisplayedAt, SeekOrigin.Begin);
              if (m_DisplayedAt != 0)
              {
                // find the line start
                var read = await sr.ReadAsync().ConfigureAwait(false);

                while (read != 13 && read != 10 && !sr.EndOfFile) await sr.ReadAsync();

                var next = await sr.PeekAsync().ConfigureAwait(false);
                if ((read == 13 && next == 10) || (read == 10 && next == 13))
                  await sr.ReadAsync().ConfigureAwait(false);
              }
              else
              {
                // Fill the buffer
                await sr.PeekAsync().ConfigureAwait(false);
              }
            }
            else
            {
              ScrollBarVertical.Enabled = false;
            }
            display = new string(sr.Buffer, 0, sr.BufferFilled);
          }
        }
      }
      catch (Exception exc)
      {
        display = exc.ExceptionMessages();
      }

      CSVTextBox.SafeInvokeNoHandleNeeded(() => CSVTextBox.Text = display, 1);
    }

    private async void ValueChangedEvent(object sender, EventArgs e)
    {
      if (m_DisplayedAt != ScrollBarVertical.Value)
        await UpdateViewAsync();
    }
  }
}