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

using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   Class to edit FillGuessSettings
  /// </summary>
  public partial class FillGuessSettingEdit : UserControl
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="FillGuessSettingEdit" /> class.
    /// </summary>
    public FillGuessSettingEdit() => InitializeComponent();

    public FillGuessSettings FillGuessSettings
    {
      set => fillGuessSettingsBindingSource.DataSource = value;
    }

    /// <summary>
    ///   Call this before close to make sure any change is stored
    /// </summary>
    public void BeforeClose()
    {
      // change the focus so data is stored.
      if (textBoxCheckedRecords.Focused)
        textBoxSampleValues.Focus();
      else
        textBoxCheckedRecords.Focus();
    }

    private void TextBoxSampleValues_Validating(object sender, System.ComponentModel.CancelEventArgs e)
    {
      if (int.TryParse(textBoxSampleValues.Text, out var max) && int.TryParse(textBoxMinSamples.Text, out var min))
        errorProvider.SetError(textBoxSampleValues, (max < min) ? "Maximum samples must be greater then minumu samples" : string.Empty);
    }

    private void TextBoxMinSamples_Validating(object sender, System.ComponentModel.CancelEventArgs e)
    {
      if (int.TryParse(textBoxSampleValues.Text, out var max) && int.TryParse(textBoxMinSamples.Text, out var min))
        errorProvider.SetError(textBoxMinSamples, (min < max) ? "Minmum samples must be less then maximum samples" : string.Empty);
    }
  }
}