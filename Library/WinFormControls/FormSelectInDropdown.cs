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

    private void OK_Click(object? sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
      Hide();
    }

    private void Cancel_Click(object? sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }
  }
}