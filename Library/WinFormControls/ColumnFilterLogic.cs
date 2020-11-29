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

namespace CsvTools
{
  using JetBrains.Annotations;
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Diagnostics;
  using System.Globalization;
  using System.Text;

  /// <summary>
  ///   DataGridViewColumnFilter based on operations and values
  /// </summary>
  [DebuggerDisplay("ColumnFilterLogic({m_FilterExpressionOperator}, {m_FilterExpressionValue}, {Active})")]
  public class ColumnFilterLogic : INotifyPropertyChanged
  {
    /// <summary>
    ///   begins
    /// </summary>
    private const string c_OperatorBegins = "xxx…";

    /// <summary>
    ///   &gt;
    /// </summary>
    private const string c_OperatorBigger = ">";

    /// <summary>
    ///   &gt;=
    /// </summary>
    private const string c_OperatorBiggerEqual = ">=";

    /// <summary>
    ///   ...xxx...
    /// </summary>
    private const string c_OperatorContains = "…xxx…";

    /// <summary>
    ///   ...xxx
    /// </summary>
    private const string c_OperatorEnds = "…xxx";

    /// <summary>
    ///   =
    /// </summary>
    private const string c_OperatorEquals = "=";

    /// <summary>
    ///   (Not Blank)
    /// </summary>
    private const string c_OperatorIsNotNull = "(Not Blank)";

    /// <summary>
    ///   (Blank)
    /// </summary>
    private const string c_OperatorIsNull = "(Blank)";

    /// <summary>
    ///   shorter than
    /// </summary>
    private const string c_OperatorLonger = "longer";

    /// <summary>
    ///   &lt;&gt;
    /// </summary>
    private const string c_OperatorNotEqual = "<>";

    /// <summary>
    ///   shorter
    /// </summary>
    private const string c_OperatorShorter = "shorter";

    /// <summary>
    ///   &lt;
    /// </summary>
    private const string c_OperatorSmaller = "<";

    /// <summary>
    ///   &lt;=
    /// </summary>
    private const string c_OperatorSmallerEqual = "<=";

    /// <summary>
    ///   The m_ column data type
    /// </summary>
    private readonly Type m_ColumnDataType;

    /// <summary>
    ///   Flag indicating if the whole filter is active
    /// </summary>
    private bool m_Active;

    [NotNull]
    private string m_DataPropertyName;

    [NotNull]
    private string m_DataPropertyNameEscape;

    /// <summary>
    ///   The m_ filter expression
    /// </summary>
    [NotNull]
    private string m_FilterExpressionOperator = string.Empty;

    [NotNull]
    private string m_FilterExpressionValue = string.Empty;

    private string m_Operator = c_OperatorEquals;

    private DateTime m_ValueDateTime;

    [NotNull]
    private string m_ValueText = string.Empty;

    /// <summary>
    ///   Initializes a new instance of the <see cref="ColumnFilterLogic" /> class.
    /// </summary>
    /// <param name="columnDataType">Type of the column data.</param>
    /// <param name="dataPropertyName">Name of the data property.</param>
    public ColumnFilterLogic(Type columnDataType, string dataPropertyName)
    {
      DataPropertyName = dataPropertyName ?? throw new ArgumentNullException(nameof(dataPropertyName));
      m_ColumnDataType = columnDataType ?? throw new ArgumentNullException(nameof(columnDataType));
    }

    /// <summary>
    ///   Occurs when filter should be executed
    /// </summary>
    public event EventHandler ColumnFilterApply;

    /// <summary>
    ///   Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    public static string OperatorIsNull => c_OperatorIsNull;

    /// <summary>
    ///   Gets a value indicating whether this <see cref="DataGridViewColumnFilterControl" /> is active.
    /// </summary>
    /// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
    public virtual bool Active
    {
      get => m_Active;
      set
      {
        m_Active = value;

        // If set active from the outside, make sure the Expression is correct
        if (m_Active)
          m_Active = BuildFilterExpression();
      }
    }

    /// <summary>
    ///   Gets the type of the column data.
    /// </summary>
    /// <value>The type of the column data.</value>
    public virtual Type ColumnDataType => m_ColumnDataType;

    [NotNull]
    public string DataPropertyName
    {
      get => m_DataPropertyName;
      private set
      {
        m_DataPropertyName = value ?? string.Empty;

        // Un-escape the name in case its escaped
        if (m_DataPropertyName.StartsWith("[", StringComparison.Ordinal)
            && m_DataPropertyName.EndsWith("]", StringComparison.Ordinal))
        {
          m_DataPropertyName = m_DataPropertyName.Substring(1, m_DataPropertyName.Length - 2).Replace(@"\]", "]")
            .Replace(@"\\", @"\");
        }

        m_DataPropertyNameEscape = $"[{m_DataPropertyName.SqlName()}]";
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
        // if a Value value filer is active ignore Operator filer

        //if (m_FilterExpressionOperator.Length > 0 && m_Operator != c_OperatorIsNotNull
        //                                          && m_FilterExpressionValue.Length > 0)
        //  return $"{m_FilterExpressionOperator} AND {m_FilterExpressionValue}";
        return m_FilterExpressionValue.Length > 0 ? m_FilterExpressionValue : m_FilterExpressionOperator;
      }
    }

    /// <summary>
    ///   Gets or sets the operator, setting the operator will build the filter
    /// </summary>
    /// <value>The operator.</value>
    [NotNull]
    public virtual string Operator
    {
      get => m_Operator;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_Operator.Equals(newVal, StringComparison.Ordinal)) return;
        m_Operator = newVal;
        FilterChanged();
        NotifyPropertyChanged(nameof(Operator));
      }
    }

    /// <summary>
    ///   Gets or sets the value date time.
    /// </summary>
    /// <value>The value date time1.</value>
    public virtual DateTime ValueDateTime
    {
      get => m_ValueDateTime;
      set
      {
        if (m_ValueDateTime.Equals(value))
          return;
        m_ValueDateTime = value;
        NotifyPropertyChanged(nameof(ValueDateTime));
      }
    }

    /// <summary>
    ///   Gets or sets the value text.
    /// </summary>
    /// <value>The value text.</value>
    [NotNull]
    public virtual string ValueText
    {
      get => m_ValueText;
      set
      {
        var newVal = (value ?? string.Empty).Trim();
        if (m_ValueText.Equals(newVal, StringComparison.Ordinal))
          return;
        m_ValueText = newVal;
        NotifyPropertyChanged(nameof(ValueText));
      }
    }

    public ValueClusterCollection ValueClusterCollection { get; } = new ValueClusterCollection(50);

    public static object[] GetOperators(Type columnDataType)
    {
      var retValues = new List<object>();

      if (columnDataType == typeof(string))
        retValues.AddRange(new[] { c_OperatorContains, c_OperatorBegins, c_OperatorEnds });

      // everyone gets = / <>
      retValues.AddRange(new[] { c_OperatorEquals, c_OperatorNotEqual });

      if (columnDataType == typeof(string))
        retValues.AddRange(new[] { c_OperatorLonger, c_OperatorShorter });
      else if (columnDataType == typeof(DateTime) || columnDataType == typeof(int) || columnDataType == typeof(long)
               || columnDataType == typeof(double) || columnDataType == typeof(float)
               || columnDataType == typeof(decimal) || columnDataType == typeof(byte)
               || columnDataType == typeof(short))
        retValues.AddRange(
          new[] { c_OperatorSmaller, c_OperatorSmallerEqual, c_OperatorBiggerEqual, c_OperatorBigger });

      // everyone gets NUll comparison
      retValues.AddRange(new[] { OperatorIsNull, c_OperatorIsNotNull });
      return retValues.ToArray();
    }

    public static bool IsNotNullCompare(string text)
    {
      if (string.IsNullOrEmpty(text))
        return false;
      return text != c_OperatorIsNotNull & text != OperatorIsNull;
    }

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
      if (valueText == OperatorIsNull)
        return string.Format(CultureInfo.InvariantCulture, "({0} IS NULL or {0} = '')", m_DataPropertyNameEscape);
      return string.Format(CultureInfo.InvariantCulture, "{0} = {1}", m_DataPropertyNameEscape, FormatValue(valueText, m_ColumnDataType));
    }

    /// <summary>
    ///   Notifies the property changed.
    /// </summary>
    /// <param name="info">The info.</param>
    protected virtual void NotifyPropertyChanged(string info) =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

    /// <summary>
    ///   Set the Filter to a value
    /// </summary>
    /// <param name="value">The typed value</param>
    public virtual void SetFilter(object value)
    {
      if (string.IsNullOrEmpty(value?.ToString()))
      {
        Operator = OperatorIsNull;
      }
      else
      {
        if (m_ColumnDataType == typeof(DateTime))
          ValueDateTime = (DateTime) value;
        else
          ValueText = value.ToString();
        Operator = c_OperatorEquals;
      }

      BuildFilterExpression();
    }

    /// <summary>
    ///   Formats the value
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="targetType">Type of the target.</param>
    /// <returns>A string with the formatted value</returns>
    [NotNull]
    private static string FormatValue(string value, Type targetType)
    {
      if (string.IsNullOrEmpty(value))
        return string.Empty;
      switch (Type.GetTypeCode(targetType))
      {
        case TypeCode.DateTime:
          throw new NotImplementedException("ValueDateTime Time should be used, not ValueText");
        // var dateValue = StringConversion.StringToDateTime( value,
        // CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern,
        // CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator,
        // CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator, false,
        // CultureInfo.CurrentCulture); return dateValue.HasValue ?
        // string.Format(CultureInfo.InvariantCulture, @"#{0:MM\/dd\/yyyy}#", dateValue.Value) : $"'{value.SqlQuote()}'";

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
          var decValue = StringConversion.StringToDecimal(
                           value,
                           CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.GetFirstChar(),
                           CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator.GetFirstChar(),
                           true) ?? StringConversion.StringToDecimal(value, '.', '\0', true);
          return string.Format(CultureInfo.InvariantCulture, "{0}", decValue);

        case TypeCode.Boolean:
          var boolValue = StringConversion.StringToBoolean(value, "x", null);
          if (boolValue.HasValue)
            return boolValue.Value ? "1" : "0";
          break;

        case TypeCode.Object:
          if (targetType == typeof(Guid))
            return $"'{value.SqlQuote()}'";
          break;

        case TypeCode.Empty:
        case TypeCode.DBNull:
          break;

        default:
          return $"'{value.SqlQuote()}'";
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
    [NotNull]
    private static string StringEscapeLike(string inputValue)
    {
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
    [NotNull]
    private string BuildFilterExpressionOperator()
    {
      switch (m_Operator)
      {
        case c_OperatorIsNull:
          return string.Format(
            CultureInfo.InvariantCulture,
            m_ColumnDataType == typeof(string) ? "({0} IS NULL or {0} = '')" : "{0} IS NULL",
            m_DataPropertyNameEscape);

        case c_OperatorIsNotNull when Type.GetTypeCode(m_ColumnDataType) == TypeCode.String:
          return string.Format(CultureInfo.InvariantCulture, "NOT({0} IS NULL or {0} = '')", m_DataPropertyNameEscape);

        case c_OperatorIsNotNull:
          return string.Format(CultureInfo.InvariantCulture, "NOT {0} IS NULL", m_DataPropertyNameEscape);
      }

      if (string.IsNullOrEmpty(m_ValueText) && (m_Operator == c_OperatorContains || m_Operator == c_OperatorLonger
                                                                                 || m_Operator == c_OperatorShorter
                                                                                 || m_Operator == c_OperatorBegins
                                                                                 || m_Operator == c_OperatorEnds))
      {
        return string.Empty;
      }

      switch (m_Operator)
      {
        case c_OperatorContains:
          if (!string.IsNullOrEmpty(m_ValueText))
          {
            return string.Format(
              CultureInfo.InvariantCulture,
              "{0} LIKE '%{1}%'",
              m_DataPropertyNameEscape,
              StringEscapeLike(m_ValueText));
          }

          break;

        case c_OperatorLonger:
          if (!string.IsNullOrEmpty(FormatValue(m_ValueText, typeof(int))))
            return string.Format(CultureInfo.InvariantCulture, "LEN({0})>{1}", m_DataPropertyNameEscape, m_ValueText);
          break;

        case c_OperatorShorter:
          if (!string.IsNullOrEmpty(FormatValue(m_ValueText, typeof(int))))
            return string.Format(CultureInfo.InvariantCulture, "LEN({0})<{1}", m_DataPropertyNameEscape, m_ValueText);
          break;

        case c_OperatorBegins:
          return string.Format(
            CultureInfo.InvariantCulture,
            "{0} LIKE '{1}%'",
            m_DataPropertyNameEscape,
            StringEscapeLike(m_ValueText));

        case c_OperatorEnds:
          return string.Format(
            CultureInfo.InvariantCulture,
            "{0} LIKE '%{1}'",
            m_DataPropertyNameEscape,
            StringEscapeLike(m_ValueText));

        default:
          string filterValue;

          if (m_ColumnDataType == typeof(DateTime))
          {
            // Filtering for Dates we need to ignore time
            filterValue = $@"#{m_ValueDateTime:MM\/dd\/yyyy}#";
            switch (m_Operator)
            {
              case c_OperatorEquals:
                return string.Format(
                  CultureInfo.InvariantCulture,
                  "({0} >= {1} AND {0} < #{2:MM\\/dd\\/yyyy}#)",
                  m_DataPropertyNameEscape,
                  filterValue,
                  m_ValueDateTime.AddDays(1));

              case c_OperatorNotEqual:
                return string.Format(
                  CultureInfo.InvariantCulture,
                  "({0} < {1} OR {0} > #{2:MM\\/dd\\/yyyy}#)",
                  m_DataPropertyNameEscape,
                  filterValue,
                  m_ValueDateTime.AddDays(1));

              default:
                return string.Format(
                  CultureInfo.InvariantCulture,
                  "{0} {1} {2}",
                  m_DataPropertyNameEscape,
                  m_Operator,
                  filterValue);
            }
          }
          else
          {
            if (string.IsNullOrEmpty(m_ValueText))
              return string.Empty;
            filterValue = FormatValue(m_ValueText, m_ColumnDataType);
          }

          if (!string.IsNullOrEmpty(filterValue))
          {
            return string.Format(
              CultureInfo.InvariantCulture,
              "{0} {1} {2}",
              m_DataPropertyNameEscape,
              m_Operator,
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
    [NotNull]
    private string BuildFilterExpressionValues()
    {
      var sql = new StringBuilder();
      var counter = 0;
      foreach (var value in ValueClusterCollection.GetActiveValueCluster())
      {
        if (counter > 0)
          sql.Append(" OR ");

        counter++;
        sql.Append(value.SQLCondition);
      }

      if (counter > 1)
        return "(" + sql + ")";
      return counter == 1 ? sql.ToString() : string.Empty;
    }

    /// <summary>
    ///   Called when the filter is changed.
    /// </summary>
    private void FilterChanged() => m_Active = BuildFilterExpression();
  }
}