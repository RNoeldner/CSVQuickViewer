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
  using System.Collections.Generic;
  using System.Globalization;
  using System.Linq;
  using System.Text;
  using System.Threading;
  using System.Windows.Forms;

  /// <summary>
  ///   Control to allow entering filters
  /// </summary>
  public sealed partial class FromRowsFilter : ResizeForm
  {
    private readonly ColumnFilterLogic m_DataGridViewColumnFilter;
    private readonly ICollection<object> m_Values;
    private readonly int m_MaxCluster;
    private readonly CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();

    /// <summary>
    ///   Initializes a new instance of the <see cref="FromRowsFilter" /> class.
    /// </summary>
    /// <param name="dataGridViewColumnFilter">The data grid view column.</param>
    /// <param name="columnValues">The data in the column</param>
    /// <param name="maxCluster">Maximum number of clusters to show</param>
    public FromRowsFilter(in ColumnFilterLogic dataGridViewColumnFilter, in ICollection<object> columnValues,
      int maxCluster)
    {
      m_DataGridViewColumnFilter =
        dataGridViewColumnFilter ?? throw new ArgumentNullException(nameof(dataGridViewColumnFilter));
      m_Values = columnValues;
      m_MaxCluster = maxCluster;
      InitializeComponent();

      Text = $"Filter : {m_DataGridViewColumnFilter.DataPropertyName}";

      comboBoxOperator.BeginUpdate();
      comboBoxOperator.Items.Clear();
      // ReSharper disable once CoVariantArrayConversion
      comboBoxOperator.Items.AddRange(ColumnFilterLogic.GetOperators(m_DataGridViewColumnFilter.DataType));
      comboBoxOperator.SelectedIndex = 0;
      comboBoxOperator.EndUpdate();

      timerRebuild.Start();

      if (m_DataGridViewColumnFilter.DataType == DataTypeEnum.String ||
          m_DataGridViewColumnFilter.DataType == DataTypeEnum.Guid ||
          m_DataGridViewColumnFilter.DataType == DataTypeEnum.Boolean)
      {
        radioButtonCombine.Visible = false;
        radioButtonEven.Visible = false;
        radioButtonReg.Visible = false;
      }
    }

    private void FilterItems(string filter)
    {
      var filtered = m_DataGridViewColumnFilter.ValueClusterCollection.Where(x =>
          x.Active || string.IsNullOrEmpty(filter) ||
          x.Display.IndexOf(filter, StringComparison.OrdinalIgnoreCase) != -1)
        .ToArray();
      listViewCluster.BeginUpdate();
      listViewCluster.Items.Clear();
      foreach (var item in filtered)
      {
        var lvItem = listViewCluster.Items.Add(new ListViewItem(new[] { item.Display, item.Count.ToString("N0"), }));
        lvItem.Checked = item.Active;
      }

      foreach (var item in m_DataGridViewColumnFilter.ValueClusterCollection.Where(x => !filtered.Contains(x)))
      {
        var lvItem = listViewCluster.Items.Add(new ListViewItem(new[] { item.Display, item.Count.ToString("N0"), }));
        lvItem.ForeColor = System.Drawing.SystemColors.GrayText;
      }

      listViewCluster.EndUpdate();
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

    private void ButtonFilter_Click(object sender, EventArgs e)
    {
      if (m_DataGridViewColumnFilter.DataType == DataTypeEnum.DateTime)
        m_DataGridViewColumnFilter.ValueDateTime = dateTimePickerValue.Value;
      else
        m_DataGridViewColumnFilter.ValueText = textBoxValue.Text;
      m_DataGridViewColumnFilter.Operator = comboBoxOperator.Text;

      foreach (ListViewItem sel in listViewCluster.Items)
      {
        var vc = m_DataGridViewColumnFilter.ValueClusterCollection.First(x => x.Display == sel.Text);
        vc.Active = sel.Checked;
      }

      m_DataGridViewColumnFilter.ApplyFilter();
    }

    private void FromDataGridViewFilter_Load(object sender, EventArgs e)
    {
      FromDataGridViewFilter_Resize(sender, e);
      PanelTop_Resize(sender, e);

      if (m_DataGridViewColumnFilter.DataType == DataTypeEnum.DateTime)
      {
        if (m_DataGridViewColumnFilter.ValueDateTime >= dateTimePickerValue.MinDate &&
            m_DataGridViewColumnFilter.ValueDateTime <= dateTimePickerValue.MaxDate)
          dateTimePickerValue.Value = m_DataGridViewColumnFilter.ValueDateTime;
        else
          dateTimePickerValue.Value = DateTime.Now.Date;
        dateTimePickerValue.Visible = true;
      }
      else
      {
        textBoxValue.Text = m_DataGridViewColumnFilter.ValueText;
        dateTimePickerValue.Visible = false;
      }

      textBoxValue.Visible = !dateTimePickerValue.Visible;
    }

    private void TextBoxValue_TextChanged(object sender, EventArgs e)
    {
      timerFilter.Stop();
      timerFilter.Start();

      try
      {
        if (m_DataGridViewColumnFilter.DataType != DataTypeEnum.DateTime)
        {
          errorProvider.SetError(textBoxValue, string.Empty);
          if (textBoxValue.Text.Length == 0)
            return;

          if (m_DataGridViewColumnFilter.DataType == DataTypeEnum.Numeric ||
              m_DataGridViewColumnFilter.DataType == DataTypeEnum.Integer)
          {
            var stringToDecimal = textBoxValue.Text.AsSpan().StringToDecimal(
                                    CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.FromText(),
                                    CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator.FromText(),
                                    false, false) ??
                                  textBoxValue.Text.AsSpan().StringToDecimal('.', char.MinValue, false, false);
            if (!stringToDecimal.HasValue)
            {
              textBoxValue.Width = dateTimePickerValue.Width - 20;
              errorProvider.SetError(textBoxValue, "Not a valid numeric value");
            }
            else
            {
              if (m_DataGridViewColumnFilter.DataType == DataTypeEnum.Integer &&
                  (stringToDecimal.Value < long.MinValue || stringToDecimal.Value > long.MaxValue))
              {
                textBoxValue.Width = dateTimePickerValue.Width - 20;
                errorProvider.SetError(textBoxValue, "Out of integer rage");
              }

              textBoxValue.Text = stringToDecimal.Value.ToString(CultureInfo.CurrentCulture);
            }
          }
        }
      }
      catch (Exception ex)
      {
        ParentForm.ShowError(ex);
      }
    }

    private void PanelTop_Resize(object sender, EventArgs e)
    {
      if (m_DataGridViewColumnFilter.DataType == DataTypeEnum.DateTime)
        return;
      textBoxValue.Width = buttonFilter.Left - textBoxValue.Left - 5;
    }

    private void FromDataGridViewFilter_Resize(object sender, EventArgs e)
    {
      listViewCluster.Columns[0].Width = listViewCluster.Width - listViewCluster.Columns[1].Width - 28;
    }

    private void ListViewCluster_KeyUp(object sender, KeyEventArgs e)
    {
      if (!e.Control || e.KeyCode != Keys.C)
        return;
      var buffer = new StringBuilder();
      var dataObject = new DataObject();
      foreach (ListViewItem sel in listViewCluster.SelectedItems)
        buffer.AppendLine($"{sel.Text}\t{sel.SubItems[1].Text}");
      dataObject.SetData(DataFormats.Text, true, buffer.ToString());
      dataObject.SetClipboard();
      e.Handled = true;
    }

    private void FromDataGridViewFilter_Activated(object sender, EventArgs e)
    {
      if (m_DataGridViewColumnFilter.DataType == DataTypeEnum.DateTime)
        dateTimePickerValue.Focus();
      else
      {
        textBoxValue.Focus();
        textBoxValue.SelectionStart = 0;
        textBoxValue.SelectionLength = textBoxValue.Text.Length;
      }
    }

    private void TimerFilter_Tick(object sender, EventArgs e)
    {
      timerFilter.Stop();
      try
      {
        // Filter The check boxes
        if (m_DataGridViewColumnFilter.DataType != DataTypeEnum.DateTime)
          FilterItems(comboBoxOperator.Text.Contains("xxx") ? textBoxValue.Text : "");
      }
      catch (Exception ex)
      {
        ParentForm.ShowError(ex);
      }
    }

    private void timerRebuild_Tick(object sender, EventArgs e)
    {
      timerRebuild.Stop();

      using var frm = new FormProgress("Filter", false, FontConfig, m_CancellationTokenSource.Token);
      frm.SetMaximum(100);
      frm.Show();
      frm.Report(new ProgressInfo("Building clusters", 1));
      try
      {
        var result = m_DataGridViewColumnFilter.ValueClusterCollection.ReBuildValueClusters(
          m_DataGridViewColumnFilter.DataType, m_Values, m_DataGridViewColumnFilter.DataPropertyNameEscaped,
          m_DataGridViewColumnFilter.Active, m_MaxCluster, radioButtonCombine.Checked, radioButtonEven.Checked, 5.0, frm,
          frm.CancellationToken);
        if (result == BuildValueClustersResult.ListFilled)
        {
          FilterItems("");
        }
        else
        {
          listViewCluster.CheckBoxes = false;
          var explain = "Error collecting the values";
          switch (result)
          {
            case BuildValueClustersResult.WrongType:
              explain = "Data type did not match";
              break;
            case BuildValueClustersResult.TooManyValues:
              explain = "Too many values building would take too long";
              break;
            case BuildValueClustersResult.NoValues:
              explain = "No value found";
              break;
          }

          toolTip.SetToolTip(this.listViewCluster, explain);
          labelError.Text = explain;
          labelError.Visible = true;
          toolTip.SetToolTip(this.labelError, explain);
          listViewCluster.Enabled = false;
        }
      }
      catch (Exception ex)
      {
        frm.Close();
        this.ShowError(ex, "Filter");
      }
    }

    private void ClusterTypeChanged(object sender, EventArgs e)
    {
      timerRebuild.Stop();
      timerRebuild.Start();
    }

    private void FromRowsFilter_FormClosing(object sender, FormClosingEventArgs e)
    {
      m_CancellationTokenSource.Cancel();
    }
  }
}