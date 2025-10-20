/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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
// Ignore Spelling: Dropdown

#nullable enable

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   Form for a drop down
  /// </summary>
  public partial class FormSelectInDropdown : ResizeForm
  {

    private static void SetCombobox(ComboBox comboBox, in IEnumerable<string> dropdownTexts,
      string? preselect = null)
    {
      var index = 0;
      var preIndex = 0;
      comboBox.BeginUpdate();
      comboBox.Items.Clear();
      foreach (var item in dropdownTexts)
      {
        if (!string.IsNullOrEmpty(item))
          comboBox.Items.Add(item);
        if (string.Equals(preselect, item, StringComparison.OrdinalIgnoreCase))
          preIndex = index;
        index++;
      }

      comboBox.EndUpdate();
      if (comboBox.Items.Count == 0)
        throw new ArgumentException($"No value to Select in {nameof(dropdownTexts)}");
      comboBox.SelectedIndex = preIndex;
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormSelectInDropdown" /> class.
    /// </summary>
    /// <param name="dropdownTexts">The drop down texts.</param>
    /// <param name="preselect"></param>
    public FormSelectInDropdown(in IEnumerable<string> dropdownTexts, string? preselect = null)
    {
      if (dropdownTexts is null)
        throw new ArgumentNullException(nameof(dropdownTexts));
      InitializeComponent();

      SetCombobox(comboBox, dropdownTexts, preselect);

      if (comboBox.Items.Count ==0)
        throw new ArgumentException($"No value to Select in {nameof(dropdownTexts)}");
    }

    /// <summary>
    ///   Gets the selected text.
    /// </summary>
    /// <value>The selected text.</value>
    public virtual string SelectedText => comboBox.Text;
  }
}
