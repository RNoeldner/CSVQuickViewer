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
#nullable enable

namespace CsvTools
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Globalization;
  using System.Text;

  /// <summary>
  ///   DataGridViewColumnFilter based on operations and values
  /// </summary>
  [DebuggerDisplay("ColumnFilterLogic({m_FilterExpressionOperator}, {m_FilterExpressionValue}, {Active})")]
  public sealed class ColumnFilterLogic : ObservableObject
  {
    /// <summary>
    ///   begins
    /// </summary>
    private const string cOperatorBegins = "xxx…";

    /// <summary>
    ///   &gt;
    /// </summary>
    private const string cOperatorBigger = ">";

    /// <summary>
    ///   &gt;=
    /// </summary>
    private const string cOperatorBiggerEqual = ">=";

    /// <summary>
    ///   ...xxx...
    /// </summary>
    private const string cOperatorContains = "…xxx…";

    /// <summary>
    ///   ...xxx
    /// </summary>
    private const string cOperatorEnds = "…xxx";

    /// <summary>
    ///   =
    /// </summary>
    private const string cOperatorEquals = "=";

    /// <summary>
    ///   (Not Blank)
    /// </summary>
    private const string cOperatorIsNotNull = "(Not Blank)";

    /// <summary>
    ///   (Blank)
    /// </summary>
    private const string cOperatorIsNull = "(Blank)";

    /// <summary>
    ///   shorter than
    /// </summary>
    private const string cOperatorLonger = "longer";

    /// <summary>
    ///   &lt;&gt;
    /// </summary>
    private const string cOperatorNotEqual = "<>";

    /// <summary>
    ///   shorter
    /// </summary>
    private const string cOperatorShorter = "shorter";

    /// <summary>
    ///   &lt;
    /// </summary>
    private const string cOperatorSmaller = "<";

    /// <summary>
    ///   &lt;=
    /// </summary>
    private const string cOperatorSmallerEqual = "<=";

    /// <summary>
    ///   The m_ column data type
    /// </summary>
    private readonly Type m_ColumnDataType;

    /// <summary>
    ///   Flag indicating if the whole filter is active
    /// </summary>
    private bool m_Active;

    private string m_DataPropertyName;

    private string m_DataPropertyNameEscape;

    /// <summary>
    ///   The m_ filter expression
    /// </summary>
    private string m_FilterExpressionOperator = string.Empty;

    private string m_FilterExpressionValue = string.Empty;

    private string m_Operator = cOperatorEquals;

    private DateTime m_ValueDateTime;

    private string m_ValueText = string.Empty;

    /// <summary>
    ///   Initializes a new instance of the <see cref="ColumnFilterLogic" /> class.
    /// </summary>
    /// <param name="columnDataType">Type of the column data.</param>
    /// <param name="dataPropertyName">Name of the data property.</param>
    public ColumnFilterLogic(in Type columnDataType, in string dataPropertyName)
    {
      m_DataPropertyNameEscape = string.Empty;
      m_DataPropertyName = string.Empty;
      DataPropertyName = dataPropertyName ?? throw new ArgumentNullException(nameof(dataPropertyName));
      m_ColumnDataType = columnDataType ?? throw new ArgumentNullException(nameof(columnDataType));
    }

    /// <summary>
    ///   Occurs when filter should be executed
    /// </summary>
    public event EventHandler? ColumnFilterApply;


    public static string OperatorIsNull => cOperatorIsNull;

    /// <summary>
    ///   Gets a value indicating whether this <see cref="DataGridViewColumnFilterControl" /> is active.
    /// </summary>
    /// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
    public bool Active
    {
      get => m_Active;
      set
      {        
        m_FilterExpressionValue = BuildFilterExpressionValues();
        if (SetProperty(ref m_Active, value) && m_Active)          
          // If set actived from the outside, make sure the Expression is correct
          m_Active = BuildFilterExpression();
      }
    }

    /// <summary>
    ///   Gets the type of the column data.
    /// </summary>
    /// <value>The type of the column data.</value>
    public Type ColumnDataType => m_ColumnDataType;

    public string DataPropertyName
    {
      get => m_DataPropertyName;
      private set
      {
        if (!SetProperty(ref m_DataPropertyName, value))
          return;
        // Un-escape the name in case its escaped
        if (m_DataPropertyName.StartsWith("[", StringComparison.Ordinal)
            && m_DataPropertyName.EndsWith("]", StringComparison.Ordinal))
        {
          m_DataPropertyName = m_DataPropertyName.Substring(1, m_DataPropertyName.Length - 2).Replace(@"\]", "]")
            .Replace(@"\\", @"\");
        }

        m_DataPropertyNameEscape=$"[{m_DataPropertyName.SqlName()}]";
      }
    }

    /// <summary>
    ///   Gets the filter expression.
    /// </summary>
    /// <value>The filter expression.</value>
    public string FilterExpression
    {
      get
      {
        // if a Value value filer is active ignore Operator filer
        return m_FilterExpressionValue.Length > 0 ? m_FilterExpressionValue : m_FilterExpressionOperator;
      }
    }

    /// <summary>
    ///   Gets or sets the operator, setting the operator will build the filter
    /// </summary>
    /// <value>The operator.</value>
    public string Operator
    {
      get => m_Operator;
      set
      {
        if (SetProperty(ref m_Operator, value))
          FilterChanged();
      }
    }

    /// <summary>
    ///   Gets or sets the value date time.
    /// </summary>
    /// <value>The value date time1.</value>
    public DateTime ValueDateTime
    {
      get => m_ValueDateTime;
      set => SetProperty(ref m_ValueDateTime, value);
    }

    /// <summary>
    ///   Gets or sets the value text.
    /// </summary>
    /// <value>The value text.</value>
    public string ValueText
    {
      get => m_ValueText;
      set => SetProperty(ref m_ValueText, value);
    }

    public ValueClusterCollection ValueClusterCollection { get; } = new ValueClusterCollection(50);

    public static object[] GetOperators(in Type columnDataType)
    {
      var retValues = new List<object>();

      if (columnDataType != typeof(DateTime) && columnDataType != typeof(bool))
        retValues.AddRange(new[] { cOperatorContains, cOperatorBegins, cOperatorEnds });

      // everyone gets = / <>
      retValues.AddRange(new[] { cOperatorEquals, cOperatorNotEqual });

      if (columnDataType == typeof(string))
        retValues.AddRange(new[] { cOperatorLonger, cOperatorShorter, cOperatorSmallerEqual, cOperatorBiggerEqual });

      else if (columnDataType == typeof(DateTime) || columnDataType == typeof(int) || columnDataType == typeof(long)
               || columnDataType == typeof(double) || columnDataType == typeof(float)
               || columnDataType == typeof(decimal) || columnDataType == typeof(byte)
               || columnDataType == typeof(short))
        retValues.AddRange(
          new[] { cOperatorSmaller, cOperatorSmallerEqual, cOperatorBiggerEqual, cOperatorBigger });

      // everyone gets NUll comparison
      retValues.AddRange(new[] { OperatorIsNull, cOperatorIsNotNull });
      return retValues.ToArray();
    }

    public static bool IsNotNullCompare(in string text)
    {
      if (string.IsNullOrEmpty(text))
        return false;
      return text != cOperatorIsNotNull && text != OperatorIsNull;
    }

    /// <summary>
    ///   Applies the filter.
    /// </summary>
    public void ApplyFilter() => ColumnFilterApply?.Invoke(this, EventArgs.Empty);

    /// <summary>
    ///   Builds the SQL command.
    /// </summary>
    /// <param name="valueText">The value text.</param>
    /// <returns></returns>
    public string BuildSqlCommand(in string valueText)
    {
      if (valueText == OperatorIsNull)
        return string.Format(CultureInfo.InvariantCulture, "({0} IS NULL or {0} = '')", m_DataPropertyNameEscape);
      return string.Format(CultureInfo.InvariantCulture, "{0} = {1}", m_DataPropertyNameEscape, FormatValue(valueText.AsSpan(), m_ColumnDataType));
    }


    /// <summary>
    ///   Set the Filter to a value
    /// </summary>
    /// <param name="value">The typed value</param>
    public void SetFilter(in object value)
    {
      if (string.IsNullOrEmpty(Convert.ToString(value)))
      {
        Operator = OperatorIsNull;
      }
      else
      {
        if (m_ColumnDataType == typeof(DateTime))
          ValueDateTime = (DateTime) value;
        else
          ValueText = Convert.ToString(value) ?? string.Empty;
        Operator = cOperatorEquals;
      }    
      foreach(var cluster in ValueClusterCollection.GetActiveValueCluster())
        cluster.Active = false;
      m_Active = BuildFilterExpression();
    }

    /// <summary>
    ///   Formats the value
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="targetType">Type of the target.</param>
    /// <returns>A string with the formatted value</returns>
    private static string FormatValue(ReadOnlySpan<char> value, Type targetType)
    {
      if (value.IsEmpty)
        return string.Empty;
      switch (Type.GetTypeCode(targetType))
      {
        case TypeCode.DateTime:
          throw new NotImplementedException("ValueDateTime Time should be used, not ValueText");

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
          var decValue = value.StringToDecimal(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.FromText(),
                           CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator.FromText(),
                           false, false) ??
                         value.StringToDecimal('.', char.MinValue, false, false);
          return string.Format(CultureInfo.InvariantCulture, "{0}", decValue);

        case TypeCode.Boolean:
          var boolValue = value.StringToBoolean("x".AsSpan(), ReadOnlySpan<char>.Empty);
          if (boolValue.HasValue)
            return boolValue.Value ? "1" : "0";
          break;

        case TypeCode.Object:
          if (targetType == typeof(Guid))
            return $"'{value.ToString().SqlQuote()}'";
          break;

        case TypeCode.Empty:
        case TypeCode.DBNull:
          break;

        default:
          return $"'{value.ToString().SqlQuote()}'";
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
    private static string StringEscapeLike(in string inputValue)
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
    private string BuildFilterExpressionOperator()
    {
      switch (m_Operator)
      {
        case cOperatorIsNull:
          return string.Format(
            CultureInfo.InvariantCulture,
            m_ColumnDataType == typeof(string) ? "({0} IS NULL or {0} = '')" : "{0} IS NULL",
            m_DataPropertyNameEscape);

        case cOperatorIsNotNull when Type.GetTypeCode(m_ColumnDataType) == TypeCode.String:
          return string.Format(CultureInfo.InvariantCulture, "NOT({0} IS NULL or {0} = '')", m_DataPropertyNameEscape);

        case cOperatorIsNotNull:
          return string.Format(CultureInfo.InvariantCulture, "NOT {0} IS NULL", m_DataPropertyNameEscape);
      }

      if (string.IsNullOrEmpty(m_ValueText) && (m_Operator == cOperatorContains || m_Operator == cOperatorLonger
                                                                                 || m_Operator == cOperatorShorter
                                                                                 || m_Operator == cOperatorBegins
                                                                                 || m_Operator == cOperatorEnds))
      {
        return string.Empty;
      }

      switch (m_Operator)
      {
        case cOperatorContains:
          if (!string.IsNullOrEmpty(m_ValueText))
          {
            if (m_ColumnDataType == typeof(string))
              return string.Format(
                CultureInfo.InvariantCulture,
                "{0} LIKE '%{1}%'",
                m_DataPropertyNameEscape,
                StringEscapeLike(m_ValueText));
            return string.Format(
              CultureInfo.InvariantCulture,
              "Convert({0},'System.String') LIKE '%{1}%'",
              m_DataPropertyNameEscape,
              StringEscapeLike(m_ValueText));
          }

          break;

        case cOperatorLonger:
          if (!string.IsNullOrEmpty(FormatValue(m_ValueText.AsSpan(), typeof(int))))
            return string.Format(CultureInfo.InvariantCulture, "LEN({0})>{1}", m_DataPropertyNameEscape, m_ValueText);
          break;

        case cOperatorShorter:
          if (!string.IsNullOrEmpty(FormatValue(m_ValueText.AsSpan(), typeof(int))))
            return string.Format(CultureInfo.InvariantCulture, "LEN({0})<{1}", m_DataPropertyNameEscape, m_ValueText);
          break;

        case cOperatorBegins:
          if (m_ColumnDataType == typeof(string))
            return string.Format(
              CultureInfo.InvariantCulture,
              "{0} LIKE '{1}%'",
              m_DataPropertyNameEscape,
              StringEscapeLike(m_ValueText));
          return string.Format(
            CultureInfo.InvariantCulture,
            "Convert({0},'System.String') LIKE '{1}%'",
            m_DataPropertyNameEscape,
            StringEscapeLike(m_ValueText));

        case cOperatorEnds:
          if (m_ColumnDataType == typeof(string))
            return string.Format(
              CultureInfo.InvariantCulture,
              "{0} LIKE '%{1}'",
              m_DataPropertyNameEscape,
              StringEscapeLike(m_ValueText));
          return string.Format(
            CultureInfo.InvariantCulture,
            "Convert({0},'System.String') LIKE '%{1}'",
            m_DataPropertyNameEscape,
            StringEscapeLike(m_ValueText));

        default:
          string filterValue;

          if (m_ColumnDataType == typeof(DateTime))
          {
            // Filtering for Dates we need to ignore time
            filterValue = $@"#{m_ValueDateTime:MM\/dd\/yyyy}#";
            return m_Operator switch
            {
              cOperatorEquals => string.Format(CultureInfo.InvariantCulture,
                "({0} >= {1} AND {0} < #{2:MM\\/dd\\/yyyy}#)", m_DataPropertyNameEscape, filterValue,
                m_ValueDateTime.AddDays(1)),
              cOperatorNotEqual => string.Format(CultureInfo.InvariantCulture,
                "({0} < {1} OR {0} > #{2:MM\\/dd\\/yyyy}#)", m_DataPropertyNameEscape, filterValue,
                m_ValueDateTime.AddDays(1)),
              _ => string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", m_DataPropertyNameEscape, m_Operator,
                filterValue)
            };
          }

          if (string.IsNullOrEmpty(m_ValueText))
            return string.Empty;
          filterValue = FormatValue(m_ValueText.AsSpan(), m_ColumnDataType);

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
    public string BuildFilterExpressionValues()
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