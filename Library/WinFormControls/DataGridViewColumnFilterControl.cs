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
#nullable enable

namespace CsvTools
{
  using System;
  using System.Globalization;
  using System.Windows.Forms;

  /// <summary>
  ///   Control to allow entering filters
  /// </summary>
  public partial class DataGridViewColumnFilterControl : UserControl
  {
    private readonly ColumnFilterLogic m_DataGridViewColumnFilter;

    /// <summary>
    ///   Initializes a new instance of the <see cref="DataGridViewColumnFilterControl" /> class.
    /// </summary>
    /// <param name="dataGridViewColumn">The data grid view column.</param>
    public DataGridViewColumnFilterControl(in DataGridViewColumn dataGridViewColumn)
    {
      if (dataGridViewColumn is null)
        throw new ArgumentNullException(nameof(dataGridViewColumn));
      m_DataGridViewColumnFilter = new ColumnFilterLogic(dataGridViewColumn.ValueType, dataGridViewColumn.DataPropertyName);

      InitializeComponent();
      lblCondition.Text = dataGridViewColumn.HeaderText;

      var isDate = m_DataGridViewColumnFilter.ColumnDataType == typeof(DateTime);

      dateTimePickerValue.Visible = isDate;
      textBoxValue.Visible = !isDate;

      comboBoxOperator.BeginUpdate();
      comboBoxOperator.Items.Clear();
      comboBoxOperator.Items.AddRange(ColumnFilterLogic.GetOperators(m_DataGridViewColumnFilter.ColumnDataType));
      comboBoxOperator.SelectedIndex = 0;
      comboBoxOperator.EndUpdate();
    }

    /// <summary>
    ///   Gets the filter logic.
    /// </summary>
    /// <value>The filter logic.</value>
    public ColumnFilterLogic ColumnFilterLogic => m_DataGridViewColumnFilter;

    /// <summary>
    ///   Focuses the input.
    /// </summary>
    public void FocusInput()
    {
      if (m_DataGridViewColumnFilter.ColumnDataType == typeof(DateTime))
      {
        dateTimePickerValue.Focus();
      }
      else
      {
        textBoxValue.Focus();
        textBoxValue.SelectionStart = 0;
        textBoxValue.SelectionLength = textBoxValue.Text.Length;
      }
    }

    /// <summary>
    ///   Handles the SelectedIndexChanged event of the comboBoxOperator control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ComboBoxOperator_SelectedIndexChanged(object? sender, EventArgs e)
    {
      var isNotNullCompare = ColumnFilterLogic.IsNotNullCompare(comboBoxOperator.Text);
      try
      {
        dateTimePickerValue.Enabled = isNotNullCompare;
        textBoxValue.Enabled = isNotNullCompare;
        m_DataGridViewColumnFilter.Operator = comboBoxOperator.Text;
      }
      catch (Exception ex)
      {
        ParentForm.ShowError(ex);
      }
    }

    private void HandleEnterKeyPress(object? sender, KeyPressEventArgs e)
    {
      if (e.KeyChar != 13)
        return;
      try
      {
        e.Handled = true;
        m_DataGridViewColumnFilter.ApplyFilter();
      }
      catch (Exception ex)
      {
        ParentForm.ShowError(ex);
      }
    }

    private void UpdateValues(object? sender, EventArgs e)
    {
      try
      {
        if (m_DataGridViewColumnFilter.ColumnDataType == typeof(DateTime))
          m_DataGridViewColumnFilter.ValueDateTime = dateTimePickerValue.Value;
        else
        {
          errorProvider.SetError(textBoxValue, string.Empty);
          textBoxValue.Width = dateTimePickerValue.Width;
          if (textBoxValue.Text.Length>0)
          {
            switch (Type.GetTypeCode(m_DataGridViewColumnFilter.ColumnDataType))
            {
              case TypeCode.Byte:
              case TypeCode.Decimal:
              case TypeCode.Double:
              case TypeCode.Int16:
              case TypeCode.Int32:
              case TypeCode.Int64:
              case TypeCode.SByte:
              case TypeCode.Single:
              case TypeCode.UInt16:
              case TypeCode.UInt32:
              case TypeCode.UInt64:
                var nvalue = textBoxValue.Text.AsSpan().StringToDecimal(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.FromText(),
                               CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator.FromText(),
                               false, false) ??
                             textBoxValue.Text.AsSpan().StringToDecimal('.', char.MinValue, false, false);
                if (!nvalue.HasValue)
                {
                  textBoxValue.Width = dateTimePickerValue.Width - 20;
                  errorProvider.SetError(textBoxValue, "Not a valid numeric value");
                  return;
                }
                else
                {
                  textBoxValue.Text = nvalue.Value.ToString(CultureInfo.CurrentCulture);
                }
                break;
            }
          }
          m_DataGridViewColumnFilter.ValueText = textBoxValue.Text;
        }

      }
      catch (Exception ex)
      {
        ParentForm.ShowError(ex);
      }
    }   
  }
}