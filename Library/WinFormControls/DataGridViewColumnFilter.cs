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

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;

namespace CsvTools
{
  /// <summary>
  ///   DataGridViewColumnFilter based on operations and values
  /// </summary>
  [DebuggerDisplay(
    "DataGridViewColumnFilterOperator({m_FilterExpressionOperator}, {m_FilterExpressionValue}, {Active})")]
  public class ColumnFilterLogic : INotifyPropertyChanged, IDisposable
  {
    /// <summary>
    ///   begins
    /// </summary>
    public const string cOPbegins = "xxx...";

    /// <summary>
    ///   &gt;
    /// </summary>
    public const string cOPbigger = ">";

    /// <summary>
    ///   &gt;=
    /// </summary>
    public const string cOPbiggerEqual = ">=";

    /// <summary>
    ///   ...xxx...
    /// </summary>
    public const string cOPcontains = "...xxx...";

    /// <summary>
    ///   ...xxx
    /// </summary>
    public const string cOPends = "...xxx";

    /// <summary>
    ///   =
    /// </summary>
    public const string cOPequal = "=";

    /// <summary>
    ///   (Not Blank)
    /// </summary>
    public const string cOPisNotNull = "(Not Blank)";

    /// <summary>
    ///   (Blank)
    /// </summary>
    public const string cOPisNull = "(Blank)";

    /// <summary>
    ///   shorter than
    /// </summary>
    public const string cOpLonger = "longer";

    /// <summary>
    ///   &lt;&gt;
    /// </summary>
    public const string cOPnotEqual = "<>";

    /// <summary>
    ///   shorter
    /// </summary>
    public const string cOPshorter = "shorter";

    /// <summary>
    ///   &lt;
    /// </summary>
    public const string cOPsmaller = "<";

    /// <summary>
    ///   &lt;=
    /// </summary>
    public const string cOPsmallerequal = "<=";

    /// <summary>
    ///   The m_ column data type
    /// </summary>
    private readonly Type m_ColumnDataType;

    private readonly string m_DataPropertyNameEscape;

    /// <summary>
    ///   Flag indicating if the whole filter is active
    /// </summary>
    private bool m_Active = false;

    /// <summary>
    ///   The m_ filter expression
    /// </summary>
    private string m_FilterExpressionOperator = string.Empty;

    private string m_FilterExpressionValue = string.Empty;
    private string m_Operator = cOPequal;
    private DateTime m_ValueDateTime;
    private string m_ValueText = string.Empty;

    /// <summary>
    ///   Initializes a new instance of the <see cref="ColumnFilterLogic" /> class.
    /// </summary>
    /// <param name="columnDataType">Type of the column data.</param>
    /// <param name="dataPropertyName">Name of the data property.</param>
    public ColumnFilterLogic(Type columnDataType, string dataPropertyName)
    {
      Contract.Requires(columnDataType != null);
      Contract.Requires(dataPropertyName != null);
      var dataPropertyName1 = dataPropertyName;
      // Un-escape the name again
      if (dataPropertyName.StartsWith("[", StringComparison.Ordinal) &&
          dataPropertyName.EndsWith("]", StringComparison.Ordinal))
      {
        dataPropertyName1 = dataPropertyName.Substring(1, dataPropertyName.Length - 2).Replace(@"\]", "]")
          .Replace(@"\\", @"\");
      }

      m_DataPropertyNameEscape = $"[{StringUtilsSQL.SqlName(dataPropertyName1)}]";

      m_ColumnDataType = columnDataType;
      //  m_ValueClusterCollection.CollectionChanged += delegate(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) { FilterChanged(); };
    }

    /// <summary>
    ///   Gets a value indicating whether this <see cref="DataGridViewColumnFilterControl" /> is active.
    /// </summary>
    /// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
    public virtual bool Active
    {
      set
      {
        m_Active = value;
        // If set active from the outside, make sure the Expression is correct
        if (m_Active)
          m_Active = BuildFilterExpression();
      }
      get => m_Active;
    }

    /// <summary>
    ///   Gets the type of the column data.
    /// </summary>
    /// <value>
    ///   The type of the column data.
    /// </value>
    public virtual Type ColumnDataType
    {
      get
      {
        Contract.Ensures(Contract.Result<Type>() != null);
        return m_ColumnDataType;
      }
    }

    /// <summary>
    ///   Gets the filter expression.
    /// </summary>
    /// <value>The filter expression.</value>
    public virtual string FilterExpression
    {
      get
      {
        Contract.Ensures(Contract.Result<string>() != null);
        if (m_FilterExpressionOperator.Length > 0 && m_Operator != cOPisNotNull && m_FilterExpressionValue.Length > 0)
          return $"{m_FilterExpressionOperator} AND {m_FilterExpressionValue}";
        if (m_FilterExpressionValue.Length > 0)
          return m_FilterExpressionValue;
        return m_FilterExpressionOperator;
      }
    }

    /// <summary>
    ///   Gets or sets the operator, setting the operator will build the filter
    /// </summary>
    /// <value>
    ///   The operator.
    /// </value>
    public virtual string Operator
    {
      get => m_Operator;
      set
      {
        Contract.Ensures(m_Operator != null);
        Contract.Assume(m_Operator != null);

        var newVal = value ?? string.Empty;
        if (!m_Operator.Equals(newVal, StringComparison.Ordinal))
        {
          m_Operator = newVal;
          FilterChanged();
          NotifyPropertyChanged(nameof(Operator));
        }
      }
    }

    /// <summary>
    ///   Gets or sets the value date time.
    /// </summary>
    /// <value>
    ///   The value date time1.
    /// </value>
    public virtual DateTime ValueDateTime
    {
      get => m_ValueDateTime;
      set
      {
        if (m_ValueDateTime.Equals(value)) return;
        m_ValueDateTime = value;
        NotifyPropertyChanged(nameof(ValueDateTime));
      }
    }

    /// <summary>
    ///   Gets or sets the value text.
    /// </summary>
    /// <value>
    ///   The value text.
    /// </value>
    public virtual string ValueText
    {
      get => m_ValueText;
      set
      {
        Contract.Ensures(m_ValueText != null);
        Contract.Assume(m_ValueText != null);

        var newVal = (value ?? string.Empty).Trim();
        if (m_ValueText.Equals(newVal, StringComparison.Ordinal)) return;
        m_ValueText = newVal;
        NotifyPropertyChanged(nameof(ValueText));
      }
    }

    internal ValueClusterCollection ValueClusterCollection { get; } = new ValueClusterCollection();

    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public virtual void Dispose() => ColumnFilterChanged?.Invoke(this, null);

    /// <summary>
    ///   Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///   Occurs when filter should be executed
    /// </summary>
    public event EventHandler ColumnFilterApply;

    /// <summary>
    ///   Occurs when the current visible <i>column filter</i> is changed.
    /// </summary>
    public event EventHandler ColumnFilterChanged;

    /// <summary>
    ///   Applies the filter.
    /// </summary>
    public virtual void ApplyFilter() => ColumnFilterApply?.Invoke(this, new EventArgs());

    /// <summary>
    ///   Builds the SQL command.
    /// </summary>
    /// <param name="valueText">The value text.</param>
    /// <returns></returns>
    public virtual string BuildSQLCommand(string valueText)
    {
      if (valueText == cOPisNull)
        return string.Format(CultureInfo.InvariantCulture, "({0} IS NULL or {0} = '')", m_DataPropertyNameEscape);
      return string.Format(CultureInfo.InvariantCulture, "{0} = {1}", m_DataPropertyNameEscape,
        FormatValue(valueText, m_ColumnDataType));
    }

    /// <summary>
    ///   Notifies the property changed.
    /// </summary>
    /// <param name="info">The info.</param>
    public virtual void NotifyPropertyChanged(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

    /// <summary>
    ///   Set the Filter to a value
    /// </summary>
    /// <param name="value">The typed value</param>
    public virtual void SetFilter(object value)
    {
      if (string.IsNullOrEmpty(value?.ToString()))
      {
        Operator = cOPisNull;
      }
      else
      {
        if (m_ColumnDataType == typeof(DateTime))
          ValueDateTime = (DateTime)value;
        else
          ValueText = value.ToString();
        Operator = cOPequal;
      }
    }

    /// <summary>
    ///   Formats the value
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="targetType">Type of the target.</param>
    /// <returns>A string with the formatted value</returns>
    private static string FormatValue(string value, Type targetType)
    {
      if (string.IsNullOrEmpty(value)) return string.Empty;
      switch (Type.GetTypeCode(targetType))
      {
        case TypeCode.DateTime:
          var dvalue = StringConversion.StringToDateTime(value,
            CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern,
            CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator,
            CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator, false, CultureInfo.CurrentCulture);
          return dvalue.HasValue ? string.Format(CultureInfo.InvariantCulture, @"#{0:MM\/dd\/yyyy}#", dvalue.Value) : $"'{StringUtilsSQL.SqlQuote(value)}'";

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
          var nvalue = StringConversion.StringToDecimal(value,
                         CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.GetFirstChar(),
                         CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator.GetFirstChar(), true) ??
                       StringConversion.StringToDecimal(value, '.', '\0', true);
          return string.Format(CultureInfo.InvariantCulture, "{0}", nvalue);

        case TypeCode.Boolean:
          var bvalue = StringConversion.StringToBoolean(value, "x", null);
          if (bvalue.HasValue)
            return bvalue.Value ? "1" : "0";
          break;

        default:
          return $"'{StringUtilsSQL.SqlQuote(value)}'";
      }

      return string.Empty;
    }

    /// <summary>
    ///   Strings with the right substitution to be used as filter If a pattern in a LIKE clause
    ///   contains any of these special characters * % [ ], those characters must be escaped in
    ///   brackets [ ] like this [*], [%], [[] or []].
    /// </summary>
    /// <param name="inputValue">The input.</param>
    /// <returns></returns>
    private static string StringEscapeLike(string inputValue)
    {
      Contract.Ensures(Contract.Result<string>() != null);
      if (string.IsNullOrEmpty(inputValue))
        return string.Empty;
      var returnVal = new StringBuilder(inputValue.Length);
      foreach (var c in inputValue)
      {
        switch (c)
        {
          case '%':
          case '*':
          case '[':
          case ']':
            returnVal.Append("[" + c + "]");
            break;

          case '\'':
            returnVal.Append("''");
            break;

          default:
            returnVal.Append(c);
            break;
        }
      }

      return returnVal.ToString();
    }

    /// <summary>
    ///   Builds the filter expression for this column
    /// </summary>
    /// <returns>true if the filter is set</returns>
    private bool BuildFilterExpression()
    {
      m_FilterExpressionOperator = BuildFilterExpressionOperator();
      m_FilterExpressionValue = BuildFilterExpressionValues();

      return m_FilterExpressionValue.Length > 0 || m_FilterExpressionOperator.Length > 0;
    }

    /// <summary>
    ///   Builds the filter expression for this column for Operator based filter
    /// </summary>
    /// <returns>a sql statement</returns>
    private string BuildFilterExpressionOperator()
    {
      Contract.Ensures(Contract.Result<string>() != null);
      if (m_Operator == cOPisNull)
      {
        if (Type.GetTypeCode(m_ColumnDataType) == TypeCode.String)
          return string.Format(CultureInfo.InvariantCulture, "({0} IS NULL or {0} = '')", m_DataPropertyNameEscape);
        else
          return string.Format(CultureInfo.InvariantCulture, "{0} IS NULL", m_DataPropertyNameEscape);
      }

      if (m_Operator == cOPisNotNull)
      {
        if (Type.GetTypeCode(m_ColumnDataType) == TypeCode.String)
          return string.Format(CultureInfo.InvariantCulture, "NOT({0} IS NULL or {0} = '')", m_DataPropertyNameEscape);
        else
          return string.Format(CultureInfo.InvariantCulture, "NOT {0} IS NULL", m_DataPropertyNameEscape);
      }

      if (string.IsNullOrEmpty(m_ValueText) &&
          (m_Operator == cOPcontains || m_Operator == cOpLonger || m_Operator == cOPshorter ||
           m_Operator == cOPbegins ||
           m_Operator == cOPends))
      {
        return string.Empty;
      }

      switch (m_Operator)
      {
        case cOPcontains:
          if (!string.IsNullOrEmpty(m_ValueText))
          {
            return string.Format(CultureInfo.InvariantCulture, "{0} LIKE '%{1}%'", m_DataPropertyNameEscape,
              StringEscapeLike(m_ValueText));
          }

          break;

        case cOpLonger:
          if (!string.IsNullOrEmpty(FormatValue(m_ValueText, typeof(int))))
            return string.Format(CultureInfo.InvariantCulture, "LEN({0})>{1}", m_DataPropertyNameEscape, m_ValueText);
          break;

        case cOPshorter:
          if (!string.IsNullOrEmpty(FormatValue(m_ValueText, typeof(int))))
            return string.Format(CultureInfo.InvariantCulture, "LEN({0})<{1}", m_DataPropertyNameEscape, m_ValueText);
          break;

        case cOPbegins:
          return string.Format(CultureInfo.InvariantCulture, "{0} LIKE '{1}%'", m_DataPropertyNameEscape,
            StringEscapeLike(m_ValueText));

        case cOPends:
          return string.Format(CultureInfo.InvariantCulture, "{0} LIKE '%{1}'", m_DataPropertyNameEscape,
            StringEscapeLike(m_ValueText));

        default:
          string filterValue;
          if (m_ColumnDataType == typeof(DateTime))
          {
            filterValue = string.Format(CultureInfo.InvariantCulture, @"#{0:MM\/dd\/yyyy}#", m_ValueDateTime);
          }
          else
          {
            if (string.IsNullOrEmpty(m_ValueText))
              return string.Empty;
            filterValue = FormatValue(m_ValueText, m_ColumnDataType);
          }

          if (!string.IsNullOrEmpty(filterValue))
          {
            return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", m_DataPropertyNameEscape, m_Operator,
              filterValue);
          }

          break;
      }

      return string.Empty;
    }

    /// <summary>
    ///   Builds the filter expression for this column for value based filter
    /// </summary>
    /// <returns>a sql statement</returns>
    private string BuildFilterExpressionValues()
    {
      Contract.Ensures(Contract.Result<string>() != null);
      var sql = new StringBuilder();
      var counter = 0;
      foreach (var value in ValueClusterCollection.GetActiveValueCluster())
      {
        if (counter > 0)
          sql.Append(" OR ");

        if (string.IsNullOrEmpty(value.SQLCondition))
          value.SQLCondition = BuildSQLCommand(value.Display);
        counter++;
        sql.Append(value.SQLCondition);
      }

      if (counter > 1)
        return "(" + sql + ")";
      if (counter == 1)
        return sql.ToString();
      return string.Empty;
    }

    /// <summary>
    ///   Called when the filter is changed.
    /// </summary>
    private void FilterChanged()
    {
      m_Active = BuildFilterExpression();
      // Notify any listeners
      ColumnFilterChanged?.Invoke(this, null);
    }
  }
}