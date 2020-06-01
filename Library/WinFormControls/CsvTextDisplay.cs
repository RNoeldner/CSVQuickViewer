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

namespace CsvTools
{
  using System;
  using System.IO;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Windows.Forms;

  /// <summary>
  ///   UserControl: CsvTextDisplay
  /// </summary>
  public partial class CsvTextDisplay : UserControl
  {
    private ICsvFile m_CsvFile;

    private int m_DisplayedAt;

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
    public async Task SetCsvFile(ICsvFile value)
    {
      if (value == null)
      {
        CSVTextBox.Text = null;
      }
      else
      {
        if (!FileSystemUtils.FileExists(value.FullPath))
        {
          CSVTextBox.DisplaySpace = false;
          CSVTextBox.Text = $@"

The file {value.FileName} does not exist.";
        }
        else
        {
          CSVTextBox.Text = null;
          CSVTextBox.DisplaySpace = true;
          CSVTextBox.Quote = value.FileFormat.FieldQualifierChar;
          CSVTextBox.Delimiter = value.FileFormat.FieldDelimiterChar;
          CSVTextBox.Escape = value.FileFormat.EscapeCharacterChar;

          ScrollBarVertical.LargeChange = 4096;
          ScrollBarVertical.Maximum = FileSystemUtils.FileLength(value.FullPath).ToInt();
          m_CsvFile = value;

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
      if (string.IsNullOrEmpty(m_CsvFile.FileName))
        return;
      try
      {
        using (new ProcessDisplayTime(CancellationToken.None))
        using (var iStream = FunctionalDI.OpenRead(m_CsvFile))
        using (var sr = new ImprovedTextReader(iStream, (await m_CsvFile.GetEncodingAsync()).CodePage))
        {
          // Some stream do not support seek...
          if (iStream.Stream.CanSeek)
          {
            iStream.Stream.Seek(m_DisplayedAt, SeekOrigin.Begin);
            if (m_DisplayedAt != 0)
            {
              // find the line start
              var read = await sr.ReadAsync();

              while (read != 13 && read != 10 && !sr.EndOfFile)
              {
                await sr.ReadAsync();
              }

              var next = await sr.PeekAsync();
              if (read == 13 && next == 10 || read == 10 && next == 13)
                await sr.ReadAsync();
            }
            else
            {
              // Fill the buffer
              await sr.PeekAsync();
            }
          }
          else
          {
            ScrollBarVertical.Enabled = false;
          }

          CSVTextBox.Text = new string(sr.Buffer, 0, sr.BufferFilled);
        }
      }
      catch (Exception exc)
      {
        CSVTextBox.Text = exc.ExceptionMessages();
      }
    }

    private async void ValueChangedEvent(object sender, EventArgs e)
    {
      if (m_DisplayedAt != ScrollBarVertical.Value)
        await UpdateViewAsync();
    }
  }
}