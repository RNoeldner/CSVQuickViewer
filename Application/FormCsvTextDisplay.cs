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

using JetBrains.Annotations;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   UserControl: CsvTextDisplay
  /// </summary>
  public partial class FormCsvTextDisplay : ResizeForm
  {
    private int m_CodePage;
    private int m_DisplayedAt;
    [NotNull] private string m_FullPath = string.Empty;
    private const int cBlockSize = 32768;

    /// <summary>
    ///   CTOR CsvTextDisplay
    /// </summary>
    public FormCsvTextDisplay()
    {
      InitializeComponent();
    }

    /// <summary>
    ///   CSV File to display
    /// </summary>
    public async Task SetCsvFileAsync(string fullPath, char qualifierChar, char delimiterChar, char escapeChar, int codePage)
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
          if (info.Length<cBlockSize*3)
          {
            try
            {
              var display = await GetTextAsync(0, cBlockSize*3);
              CSVTextBox.Text = display;
              CSVTextBox.ScrollBars = RichTextBoxScrollBars.Both;
              ScrollBarVertical.Visible = false;
            }
            catch (Exception ex)
            {
              CSVTextBox.Text = ex.Message;
            }
          }
          else
          {
            CSVTextBox.Text = null;
            CSVTextBox.DisplaySpace = true;
            CSVTextBox.Quote = qualifierChar;
            CSVTextBox.Delimiter = delimiterChar;
            CSVTextBox.Escape = escapeChar;
            CSVTextBox.MouseWheel += MouseWheelScroll;
            ScrollBarVertical.Visible=true;
            ScrollBarVertical.LargeChange = 8192;
            ScrollBarVertical.Maximum = info.Length.ToInt();

            // Starting task without error handler
            var display = await GetTextAsync(0, cBlockSize);
            CSVTextBox.Text = display;
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

    private bool IsReadig = false;
    private bool Stopp = false;

    private async Task<string> GetTextAsync(int newPos, int maxChar)
    {
      try
      {
        using (var iStream = FunctionalDI.OpenRead(m_FullPath))
        {
          if (Stopp) throw new OperationCanceledException();

          if (iStream.Stream.CanSeek && newPos != 0)
            iStream.Stream.Seek(newPos, SeekOrigin.Begin);

          if (Stopp) throw new OperationCanceledException();

          using (var stream = new StreamReader(iStream.Stream, Encoding.GetEncoding(m_CodePage), false))
          {
            var buffer = new char[maxChar];
            var pos = 0;
            var readChars = await stream.ReadAsync(buffer, 0, maxChar);

            // get to the line start, position might be in the mniddle of a line
            if (newPos != 0)
            {
              while (pos<readChars)
              {
                var chr = buffer[pos++];
                if (chr == '\r' || chr == '\n')
                {
                  if (pos+1<readChars)
                  {
                    var nextChar = buffer[pos+1];
                    if ((chr == '\r' && nextChar == '\n') || (chr == '\n' && nextChar == '\r'))
                      pos++;
                  }
                  break;
                }
              }
            }

            if (Stopp) throw new OperationCanceledException();

            return new string(buffer, pos, readChars-pos);
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
        IsReadig = false;
      }
    }

    private async void ValueChangedEvent(object sender, EventArgs e)
    {
      if (m_DisplayedAt != ScrollBarVertical.Value)
      {
        if (IsReadig)
        {
          Stopp=true;
          // wasit for it to finish
          await Task.Delay(200);
        }

        try
        {
          // reading the data is usually pretty fast (unless its encrypted)
          var display = await GetTextAsync(ScrollBarVertical.Value, cBlockSize);
          // Display of teh text is teh most time consuming part
          CSVTextBox.Text = display;
          m_DisplayedAt = ScrollBarVertical.Value;
        }
        catch
        {
          // ignore
        }
        Stopp= false;
      }
    }
  }
}