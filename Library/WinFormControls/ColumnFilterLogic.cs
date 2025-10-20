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
    public ColumnFilterLogic(in Type columnDataType, string dataPropertyName)
    {
      if (columnDataType is null) throw new ArgumentNullException(nameof(columnDataType));
      if (string.IsNullOrEmpty(dataPropertyName)) throw new ArgumentException($"'{nameof(dataPropertyName)}' cannot be null or empty.", nameof(dataPropertyName));

      m_DataPropertyNameEscape = string.Empty;
      m_DataPropertyName = string.Empty;
      DataPropertyName = dataPropertyName;
      DataType = columnDataType.GetDataType();
      if (DataType == DataTypeEnum.Double)
        DataType = DataTypeEnum.Numeric;

      ValueClusterCollection = new ValueClusterCollection();
    }

    public static string OperatorIsNull => cOperatorIsNull;

    /// <summary>
    ///   Gets a value indicating whether this <see cref="ColumnFilterLogic" /> is active.
    /// </summary>
    /// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
    public bool Active
    {
      get => m_Active;
      set
      {
        if (SetProperty(ref m_Active, value) && m_Active)
          ApplyFilter();
      }
    }

    /// <summary>
    ///   Gets the type of the column data.
    /// </summary>
    /// <value>The type of the column data.</value>
    public DataTypeEnum DataType { get; }

    public string DataPropertyName
    {
      get => m_DataPropertyName;
      private set
      {
        if (!SetProperty(ref m_DataPropertyName, value))
          return;
        // Unescape the name in case its escaped
        if (m_DataPropertyName.StartsWith("[", StringComparison.Ordinal)
            && m_DataPropertyName.EndsWith("]", StringComparison.Ordinal))
        {
          m_DataPropertyName = m_DataPropertyName.Substring(1, m_DataPropertyName.Length - 2).Replace(@"\]", "]")
            .Replace(@"\\", @"\");
        }

        m_DataPropertyNameEscape=$"[{m_DataPropertyName.SqlName()}]";
      }
    }

    public string DataPropertyNameEscaped => m_DataPropertyNameEscape;

    /// <summary>
    ///   Gets the filter expression.
    /// </summary>
    /// <value>The filter expression.</value>
    public string FilterExpression
    {
      get
      {
        // if a Value filer is active ignore Operator filer
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
          ApplyFilter();
      }
    }

    /// <summary>
    ///   Gets or sets the value date time.
    /// </summary>
    /// <value>The value date time1.</value>
    public DateTime ValueDateTime
    {
      get => m_ValueDateTime;
      set
      {
        if (SetProperty(ref m_ValueDateTime, value))
          // in case the text is changed rebuild the filter
          ApplyFilter();
      }
    }

    /// <summary>
    ///   Gets or sets the value text.
    /// </summary>
    /// <value>The value text.</value>
    public string ValueText
    {
      get => m_ValueText;
      set
      {
        if (SetProperty(ref m_ValueText, value))
          // in case the text is changed rebuild the filter
          ApplyFilter();
      }
    }

    public readonly ValueClusterCollection ValueClusterCollection;

    public static string[] GetOperators(DataTypeEnum columnDataType)
    {
      var retValues = new List<string>();
      if (columnDataType != DataTypeEnum.DateTime && columnDataType != DataTypeEnum.Boolean)
        retValues.AddRange(new[] { cOperatorContains, cOperatorBegins, cOperatorEnds });

      // everyone gets = / <>
      retValues.AddRange(new[] { cOperatorEquals, cOperatorNotEqual });

      if (columnDataType == DataTypeEnum.String)
        retValues.AddRange(new[] { cOperatorLonger, cOperatorShorter, cOperatorSmallerEqual, cOperatorBiggerEqual });

      else if (columnDataType == DataTypeEnum.DateTime || columnDataType == DataTypeEnum.Numeric || columnDataType == DataTypeEnum.Integer)
        retValues.AddRange(
          new[] { cOperatorSmaller, cOperatorSmallerEqual, cOperatorBiggerEqual, cOperatorBigger });

      // everyone gets NUll comparison
      retValues.AddRange(new[] { OperatorIsNull, cOperatorIsNotNull });
      return retValues.ToArray();
    }

    public static bool IsNotNullCompare(string text)
    {
      if (string.IsNullOrEmpty(text))
        return false;
      return text != cOperatorIsNotNull && text != OperatorIsNull;
    }

    /// <summary>
    ///   Applies the filter.
    /// </summary>
    public void ApplyFilter()
    {
      m_FilterExpressionOperator = BuildFilterExpressionOperator();
      m_FilterExpressionValue = BuildFilterExpressionValues();

      m_Active = m_FilterExpressionValue.Length > 0 || m_FilterExpressionOperator.Length > 0;
    }

    /// <summary>
    ///   Set the Filter to a value
    /// </summary>
    /// <param name="value">The typed value</param>
    public void SetFilter(in object value)
    {
      foreach (var cluster in ValueClusterCollection.GetActiveValueCluster())
        cluster.Active = false;

      if (string.IsNullOrEmpty(Convert.ToString(value)))
      {
        Operator = OperatorIsNull;
      }
      else
      {
        if (DataType == DataTypeEnum.DateTime)
          ValueDateTime = (DateTime) value;
        else
          // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
          ValueText = Convert.ToString(value) ?? string.Empty;
        Operator = cOperatorEquals;
      }

      ApplyFilter();
    }

    /// <summary>
    ///   Formats the value
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="targetType">Type of the target.</param>
    /// <returns>A string with the formatted value</returns>
    private static string FormatValue(ReadOnlySpan<char> value, DataTypeEnum targetType)
    {
      if (value.IsEmpty)
        return string.Empty;
      switch (targetType)
      {
        case DataTypeEnum.DateTime:
          throw new NotImplementedException("ValueDateTime Time should be used, not ValueText");

        case DataTypeEnum.Numeric:
          var decValue = value.StringToDecimal(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.FromText(),
                           CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator.FromText(),
                           false, false) ??
                         value.StringToDecimal('.', char.MinValue, false, false);
          if (decValue.HasValue)
            return string.Format(CultureInfo.InvariantCulture, "{0}", decValue.Value);
          else
            return "0";

        case DataTypeEnum.Integer:
          var lngValue = value.StringToDecimal(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.FromText(),
                             CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator.FromText(),
                             false, false) ??
                         value.StringToDecimal('.', char.MinValue, false, false);
          if (lngValue.HasValue)
            return string.Format(CultureInfo.InvariantCulture, "{0}", lngValue.Value.ToInt64());
          else
            return "0";

        case DataTypeEnum.Boolean:
          var boolValue = value.StringToBoolean("x".AsSpan(), ReadOnlySpan<char>.Empty);
          if (boolValue.HasValue)
            return boolValue.Value ? "1" : "0";
          break;

        default:
          return $"'{value.ToString().SqlQuote()}'";
      }

      return string.Empty;
    }

    /// <summary>
    ///   Builds the filter expression for this column for Operator based filter
    /// </summary>
    /// <returns>A SQL Condition to be used on DataTable</returns>
    private string BuildFilterExpressionOperator()
    {
      switch (m_Operator)
      {
        case cOperatorIsNull:
          return DataType == DataTypeEnum.String
            ? $"({m_DataPropertyNameEscape} IS NULL or {m_DataPropertyNameEscape} = '')"
            : $"{m_DataPropertyNameEscape} IS NULL";

        case cOperatorIsNotNull when DataType == DataTypeEnum.String:
          return $"NOT({m_DataPropertyNameEscape} IS NULL or {m_DataPropertyNameEscape} = '')";

        case cOperatorIsNotNull:
          return $"NOT {m_DataPropertyNameEscape} IS NULL";
      }

      if (string.IsNullOrEmpty(m_ValueText) && (m_Operator == cOperatorContains || m_Operator == cOperatorLonger || m_Operator == cOperatorShorter|| m_Operator == cOperatorBegins || m_Operator == cOperatorEnds))
      {
        return string.Empty;
      }

      switch (m_Operator)
      {
        case cOperatorContains:
          if (!string.IsNullOrEmpty(m_ValueText))
          {
            if (DataType == DataTypeEnum.String)
              return string.Format(CultureInfo.InvariantCulture, "{0} LIKE '%{1}%'", m_DataPropertyNameEscape,
                m_ValueText.StringEscapeLike());
            return string.Format(CultureInfo.InvariantCulture, "Convert({0},'System.String') LIKE '%{1}%'",
              m_DataPropertyNameEscape, m_ValueText.StringEscapeLike());
          }

          break;

        case cOperatorLonger:
          return string.Format(CultureInfo.InvariantCulture, "LEN({0})>{1}", m_DataPropertyNameEscape,
            FormatValue(m_ValueText.AsSpan(), DataTypeEnum.Integer));

        case cOperatorShorter:
          return string.Format(CultureInfo.InvariantCulture, "LEN({0})<{1}", m_DataPropertyNameEscape,
            FormatValue(m_ValueText.AsSpan(), DataTypeEnum.Integer));

        case cOperatorBegins:
          if (DataType == DataTypeEnum.String)
            return string.Format(CultureInfo.InvariantCulture, "{0} LIKE '{1}%'", m_DataPropertyNameEscape,
              m_ValueText.StringEscapeLike());
          return string.Format(CultureInfo.InvariantCulture, "Convert({0},'System.String') LIKE '{1}%'",
            m_DataPropertyNameEscape, m_ValueText.StringEscapeLike());

        case cOperatorEnds:
          if (DataType == DataTypeEnum.String)
            return $"{m_DataPropertyNameEscape} LIKE '%{m_ValueText.StringEscapeLike()}'";
          return $"Convert({m_DataPropertyNameEscape},'System.String') LIKE '%{m_ValueText.StringEscapeLike()}'";

        default:
          string filterValue;

          if (DataType == DataTypeEnum.DateTime)
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
          filterValue = FormatValue(m_ValueText.AsSpan(), DataType);

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
    /// <returns>A SQL Condition to be used on DataTable</returns>
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
  }
}
