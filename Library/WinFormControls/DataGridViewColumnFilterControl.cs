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
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   Control to allow entering filters
  /// </summary>
  public partial class DataGridViewColumnFilterControl : UserControl
  {
    private readonly ColumnFilterLogic m_DataGridViewColumnFilter;

    /// <summary>
    ///   Initializes a new instance of the <see cref="DataGridViewColumnFilterControl" /> class.
    /// </summary>
    /// <param name="columnDataType">Type of the column data.</param>
    /// <param name="dataGridViewColumn">The data grid view column.</param>
    public DataGridViewColumnFilterControl(Type columnDataType, DataGridViewColumn dataGridViewColumn)
    {
      Contract.Requires(dataGridViewColumn != null);
      Contract.Requires(dataGridViewColumn.DataPropertyName != null);

      m_DataGridViewColumnFilter = new ColumnFilterLogic(columnDataType, dataGridViewColumn.DataPropertyName);
      m_DataGridViewColumnFilter.PropertyChanged += FilterLogic_PropertyChanged;
      InitializeComponent();
      lblCondition.Text = dataGridViewColumn.HeaderText;
      Contract.Assume(dateTimePickerValue != null);
      Contract.Assume(comboBoxOperator != null);
      Contract.Assume(textBoxValue != null);

      var isDate = m_DataGridViewColumnFilter.ColumnDataType == typeof(DateTime);

      dateTimePickerValue.Visible = isDate;
      textBoxValue.Visible = !isDate;

      comboBoxOperator.BeginUpdate();
      comboBoxOperator.Items.Clear();
      comboBoxOperator.Items.AddRange(m_DataGridViewColumnFilter.ColumnDataType == typeof(string)
        ? new object[]
        {
          ColumnFilterLogic.cOPcontains, ColumnFilterLogic.cOPbegins, ColumnFilterLogic.cOPends,
          ColumnFilterLogic.cOPequal, ColumnFilterLogic.cOPnotEqual, ColumnFilterLogic.cOPisNull,
          ColumnFilterLogic.cOPisNotNull, ColumnFilterLogic.cOpLonger, ColumnFilterLogic.cOPshorter
        }
        : new object[]
        {
          ColumnFilterLogic.cOPisNotNull, ColumnFilterLogic.cOPsmaller, ColumnFilterLogic.cOPsmallerequal,
          ColumnFilterLogic.cOPequal, ColumnFilterLogic.cOPnotEqual, ColumnFilterLogic.cOPbiggerEqual,
          ColumnFilterLogic.cOPbigger, ColumnFilterLogic.cOPisNull, ColumnFilterLogic.cOPisNotNull
        });
      comboBoxOperator.SelectedIndex = 0;
      comboBoxOperator.EndUpdate();
    }

    /// <summary>
    ///   Gets the filter logic.
    /// </summary>
    /// <value>The filter logic.</value>
    public ColumnFilterLogic ColumnFilterLogic
    {
      get
      {
        Contract.Ensures(Contract.Result<ColumnFilterLogic>() != null);
        return m_DataGridViewColumnFilter;
      }
    }

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
    private void ComboBoxOperator_SelectedIndexChanged(object sender, EventArgs e)
    {
      try
      {
        dateTimePickerValue.Enabled = comboBoxOperator.Text != ColumnFilterLogic.cOPisNotNull &&
                                      comboBoxOperator.Text != ColumnFilterLogic.cOPisNull;
        textBoxValue.Enabled = comboBoxOperator.Text != ColumnFilterLogic.cOPisNotNull &&
                               comboBoxOperator.Text != ColumnFilterLogic.cOPisNull;
        m_DataGridViewColumnFilter.Operator = comboBoxOperator.Text;
      }
      catch (Exception ex)
      {
        ParentForm.ShowError(ex);
      }
    }

    /// <summary>
    ///   Starts the clock to change the filter with a delay
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void FilterValueChanged(object sender, EventArgs e)
    {
      try
      {
        if (m_DataGridViewColumnFilter.ColumnDataType == typeof(DateTime))
        {
          if (!(sender is DateTimePicker dtp))
            return;

          m_DataGridViewColumnFilter.ValueDateTime = dtp.Value;
        }
        else
        {
          if (!(sender is TextBox tb))
            return;

          m_DataGridViewColumnFilter.ValueText = tb.Text;
        }
      }
      catch (Exception ex)
      {
        ParentForm.ShowError(ex);
      }
    }

    private void HandleEnterKeyPress(object sender, KeyPressEventArgs e)
    {
      if (e.KeyChar != 13) return;
      e.Handled = true;
      m_DataGridViewColumnFilter.ApplyFilter();
    }

    private void FilterLogic_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName)
      {
        case "ValueText":
          textBoxValue.Text = m_DataGridViewColumnFilter.ValueText;
          break;

        case "ValueDateTime":
          dateTimePickerValue.Value = m_DataGridViewColumnFilter.ValueDateTime;
          break;

        case "Operator":
          comboBoxOperator.Text = m_DataGridViewColumnFilter.Operator;
          break;
      }
    }

    private void TextBoxValue_Validated(object sender, EventArgs e)
    {
      errorProvider1.SetError(textBoxValue, "");
      textBoxValue.Width = dateTimePickerValue.Width;
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
          var nvalue = StringConversion.StringToDecimal(textBoxValue.Text,
                         CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.GetFirstChar(),
                         CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator.GetFirstChar(), true) ??
                       StringConversion.StringToDecimal(textBoxValue.Text, '.', '\0', true);
          if (!nvalue.HasValue)
          {
            textBoxValue.Width = dateTimePickerValue.Width - 20;
            errorProvider1.SetError(textBoxValue, "Not a valid numeric value");
          }
          else
          {
            textBoxValue.Text = nvalue.Value.ToString(CultureInfo.CurrentCulture);
          }

          break;
      }
    }
  }
}