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
using System.Windows.Forms;

namespace CsvTools
{
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
    public ICsvFile CsvFile
    {
      set
      {
        if (value == null)
        {
          CSVTextBox.Text = null;
        }
        else
        {
          var file = FileSystemUtils.FileInfo(value.FullPath);

          if (!file.Exists)
          {
            CSVTextBox.DisplaySpace = false;
            CSVTextBox.Text = $"\n\nThe file {value.FileName} does not exist.";
          }
          else
          {
            CSVTextBox.Text = null;
            CSVTextBox.DisplaySpace = true;
            CSVTextBox.Quote = value.FileFormat.FieldQualifierChar;
            CSVTextBox.Delimiter = value.FileFormat.FieldDelimiterChar;
            CSVTextBox.Escape = value.FileFormat.EscapeCharacterChar;

            ScrollBarVertical.LargeChange = 4096;
            ScrollBarVertical.Maximum = (int)file.Length;
            m_CsvFile = value;

            UpdateView();
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

    private void ScrollEvent(object sender, ScrollEventArgs e)
    {
      if (m_DisplayedAt != ScrollBarVertical.Value && ScrollBarVertical.Enabled)
        UpdateView();
    }

    private void SizeChangedEvent(object sender, EventArgs e)
    {
      CSVTextBox.Width = ScrollBarVertical.Left;
    }

    private void UpdateView()
    {
      m_DisplayedAt = ScrollBarVertical.Value;
      if (string.IsNullOrEmpty(m_CsvFile.FileName))
        return;
      try
      {
        using (var procDisp = new ProcessDisplayTime(System.Threading.CancellationToken.None))
        using (var istream = ImprovedStream.OpenRead(m_CsvFile))
        using (var sr = new StreamReader(istream.Stream, m_CsvFile.GetEncoding(), m_CsvFile.ByteOrderMark))
        {
          // Some stream do not support seek...
          if (istream.Stream.CanSeek)
          {
            istream.Stream.Seek(m_DisplayedAt, SeekOrigin.Begin);
            if (m_DisplayedAt != 0)
            {
              // find the line start
              var read = sr.Read();
              while (read != 13 && read != 10 && !sr.EndOfStream)
                read = sr.Read();

              var next = sr.Peek();
              if (read == 13 && next == 10 || read == 10 && next == 13)
                sr.Read();
            }
          }
          else
          {
            ScrollBarVertical.Enabled = false;
          }

          var buffer = new char[32000];
          var len = sr.Read(buffer, 0, buffer.Length);
          CSVTextBox.Text = new string(buffer, 0, len);
        }
      }
      catch (Exception exc)
      {
        CSVTextBox.Text = exc.ExceptionMessages();
      }
    }

    private void ValueChangedEvent(object sender, EventArgs e)
    {
      if (m_DisplayedAt != ScrollBarVertical.Value)
        UpdateView();
    }
  }
}