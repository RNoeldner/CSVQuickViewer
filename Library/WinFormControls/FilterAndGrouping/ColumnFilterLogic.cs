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

namespace CsvTools;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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
  ///   Flag indicating if the whole filter is oldActive
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
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
  public ColumnFilterLogic(Type columnDataType, string dataPropertyName)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
  {
    if (columnDataType is null) throw new ArgumentNullException(nameof(columnDataType));
    if (string.IsNullOrEmpty(dataPropertyName)) throw new ArgumentException($"'{nameof(dataPropertyName)}' cannot be null or empty.", nameof(dataPropertyName));

    m_DataPropertyName = dataPropertyName;
    ValidateDataPropertyName();
    DataType = columnDataType.GetDataType();
    if (DataType == DataTypeEnum.Double)
      DataType = DataTypeEnum.Numeric;
  }

  /// <summary>
  /// Rebuilds the value clusters for the specified data type.
  /// This operation clears any existing clusters and constructs
  /// a new cluster set based on the supplied values and clustering rules.
  /// </summary>
  /// <param name="values">The collection of raw values from which clusters are derived.</param>
  /// <param name="maxNumber">The upper bound on the number of clusters to generate.</param>
  /// <param name="combine">
  /// Indicates whether small or low-density clusters should be merged to meet 
  /// the maximum cluster count or to improve distribution.
  /// </param>
  /// <param name="even">
  /// If true, attempts to distribute values evenly across clusters; 
  /// otherwise, natural groupings are preserved.
  /// </param>
  /// <param name="maxSeconds">
  /// The maximum allowed processing time. The method stops early if this limit is reached.
  /// </param>
  /// <param name="progress">
  /// Provides progress updates and allows cancellation during cluster rebuilding.
  /// </param>
  /// <returns>
  /// A <see cref="BuildValueClustersResult"/> containing the newly generated clusters 
  /// and associated metadata.
  /// </returns>
  public BuildValueClustersResult ReBuildValueClusters(object[] values, int maxNumber,
    bool combine, bool even, double maxSeconds, IProgressWithCancellation progress)
  {
    if (values is null)
      throw new ArgumentNullException(nameof(values));

    if (maxNumber < 1 || maxNumber > 200)
      maxNumber = 200;

    // For guid it does not make much sense to build clusters, any other type has a limit of 100k, It's just too slow otherwise
    if (values.Length > 50000 && DataType == DataTypeEnum.Guid)
      return BuildValueClustersResult.TooManyValues;

    try
    {
      //----------------------------------------------------------------------
      // STRING / GUID / BOOLEAN
      //----------------------------------------------------------------------
      if (DataType == DataTypeEnum.String ||
          DataType == DataTypeEnum.Guid ||
          DataType == DataTypeEnum.Boolean)
      {
        ValueClusterCollection.Clear();

        (var countNull, var newGroups) = values.BuildValueClustersString(m_DataPropertyNameEscape, maxNumber, maxSeconds, progress);
        AddValueClusterNull(m_DataPropertyNameEscape, countNull);
        foreach (var newGroup in newGroups)
          AddOrUpdate(newGroup);
        return (newGroups.Count>0) ? BuildValueClustersResult.ListFilled : BuildValueClustersResult.NoValues;
      }

      //----------------------------------------------------------------------
      // DATE
      //----------------------------------------------------------------------
      if (DataType == DataTypeEnum.DateTime)
      {

        ValueClusterCollection.Clear();

        (var countNull, var newGroups) = even ? values.BuildValueClustersDateEven(m_DataPropertyNameEscape, maxNumber, maxSeconds, progress) : values.BuildValueClustersDate(m_DataPropertyNameEscape, maxNumber, combine, maxSeconds, progress);
        AddValueClusterNull(m_DataPropertyNameEscape, countNull);
        foreach (var newGroup in newGroups.Where(newGroup => newGroup.Start is DateTime start && newGroup.End is DateTime end && !HasEnclosingCluster(start, end)))
          AddOrUpdate(newGroup);

        return (newGroups.Count>0) ? BuildValueClustersResult.ListFilled : BuildValueClustersResult.NoValues;
      }

      //----------------------------------------------------------------------
      // INTEGER
      //----------------------------------------------------------------------
      if (DataType == DataTypeEnum.Integer)
      {
        ValueClusterCollection.Clear();

        (var countNull, var newGroups) = even ?
            values.BuildValueClustersLongEven(m_DataPropertyNameEscape, maxNumber, maxSeconds, progress) :
            values.BuildValueClustersLong(m_DataPropertyNameEscape, maxNumber, combine, maxSeconds, progress);
        AddValueClusterNull(m_DataPropertyNameEscape, countNull);
        foreach (var newGroup in newGroups.Where(g => g.Start is long start && g.End is long end && !HasEnclosingCluster(start, end)))
          AddOrUpdate(newGroup);

        return (newGroups.Count>0) ? BuildValueClustersResult.ListFilled : BuildValueClustersResult.NoValues;
      }

      //----------------------------------------------------------------------
      // NUMERIC / DOUBLE Using 2 decimal places for grouping
      //----------------------------------------------------------------------
      if (DataType == DataTypeEnum.Numeric || DataType == DataTypeEnum.Double)
      {
        ValueClusterCollection.Clear();

        (var countNull, var newGroups) = even ?
            values.BuildValueClustersDoubleEven(m_DataPropertyNameEscape, maxNumber, maxSeconds, progress) :
            values.BuildValueClustersDouble(m_DataPropertyNameEscape, maxNumber, combine, maxSeconds, progress);
        AddValueClusterNull(m_DataPropertyNameEscape, countNull);
        foreach (var newGroup in newGroups.Where(newGroup => newGroup.Start is double start && newGroup.End is double end && !HasEnclosingCluster(start, end)))
          AddOrUpdate(newGroup);

        return (newGroups.Count>0) ? BuildValueClustersResult.ListFilled : BuildValueClustersResult.NoValues;
      }

      return BuildValueClustersResult.WrongType;
    }
    // this makes it backward compatible
    catch (TimeoutException toe)
    {
      progress.Report(toe.Message);
      return BuildValueClustersResult.TooManyValues;
    }
    catch (OperationCanceledException tce)
    {
      progress.Report(tce.Message);
      return BuildValueClustersResult.TooManyValues;
    }
    catch (Exception ex)
    {
      progress.Report(ex.Message);
      return BuildValueClustersResult.Error;
    }
  }



  /// <summary>
  /// Adds a <see cref="ValueCluster"/> to the collection if it has a positive count
  /// and there is no existing cluster with the same display value (case-insensitive).
  /// Prevents duplicate clusters based on display text.
  /// </summary>
  /// <param name="item">The cluster to add.</param>
  /// <note>Not sure if we need this...</note>
  public void AddOrUpdate(ValueCluster item)
  {
    var oldActive = ActiveValueClusterCollection.FirstOrDefault(x => x.Display.Equals(item.Display));
    if (oldActive != null)
    {
      ActiveValueClusterCollection.Remove(oldActive);
      if (item.Count>0)
        ActiveValueClusterCollection.Add(item);
    }
    else if (item.Count>0)
      ValueClusterCollection.Add(item);
  }

  public void AddValueClusterNull(string escapedName, int count) =>
    AddOrUpdate(new ValueCluster(OperatorIsNull, $"({escapedName} IS NULL)", count));

  /// <summary>
  /// Determines whether any cluster in the collection fully encloses the specified range.
  /// </summary>
  /// <typeparam name="T">A value type that implements <see cref="IComparable{T}"/>.</typeparam>
  /// <param name="minValue">The start of the range to check.</param>
  /// <param name="maxValue">The end of the range to check.</param>
  /// 
  /// <returns>
  /// <c>true</c> if at least one cluster starts at or before <paramref name="minValue"/> 
  /// and ends at or after <paramref name="maxValue"/> (or is unbounded); otherwise, <c>false</c>.
  /// </returns>
  public bool HasEnclosingCluster<T>(T minValue, T maxValue) where T : struct, IComparable<T>
  {
    // For performance reason avoid Linq
    for (int i = 0; i < ActiveValueClusterCollection.Count; i++)
    {
      var cluster = ActiveValueClusterCollection[i];
      if (cluster.Start is T start && cluster.End is T end && start.CompareTo(minValue) <= 0 && end.CompareTo(maxValue) >= 0)
        return true;
    }
    return false;
  }

  public static string OperatorIsNull => cOperatorIsNull;

  /// <summary>
  ///   Gets a value indicating whether this <see cref="ColumnFilterLogic" /> is oldActive.
  /// </summary>
  /// <value><c>true</c> if oldActive; otherwise, <c>false</c>.</value>
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

  private void ValidateDataPropertyName()
  {
    // Unescape the name in case its escaped
    if (m_DataPropertyName.StartsWith("[", StringComparison.Ordinal)
        && m_DataPropertyName.EndsWith("]", StringComparison.Ordinal))
    {
      m_DataPropertyName = m_DataPropertyName.Substring(1, m_DataPropertyName.Length - 2).Replace(@"\]", "]")
        .Replace(@"\\", @"\");
    }

    m_DataPropertyNameEscape=$"[{m_DataPropertyName.SqlName()}]";
  }

  public string DataPropertyName=> m_DataPropertyName;

  public string DataPropertyNameEscaped => m_DataPropertyNameEscape;

  /// <summary>
  ///   Gets the filter expression.
  /// </summary>
  /// <value>The filter expression.</value>
  public string FilterExpression
  {
    get
    {
      // if a Value filer is oldActive ignore Operator filer
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

  public readonly List<ValueCluster> ValueClusterCollection = new List<ValueCluster>();
  public readonly List<ValueCluster> ActiveValueClusterCollection = new List<ValueCluster>();

  public void SetActiveStatus(ValueCluster cluster, bool active)
  {
    if (active)
      MoveItem(cluster, ValueClusterCollection, ActiveValueClusterCollection);
    else
      MoveItem(cluster, ActiveValueClusterCollection, ValueClusterCollection);

    BuildFilterExpressionValues();

    static void MoveItem(ValueCluster cluster, ICollection<ValueCluster> source, ICollection<ValueCluster> target)
    {
      if (source.Remove(cluster) && !target.Contains(cluster))
        target.Add(cluster);
    }
  }

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
    return !string.Equals(text, cOperatorIsNotNull, StringComparison.OrdinalIgnoreCase)&& !string.Equals(text, OperatorIsNull, StringComparison.OrdinalIgnoreCase);
  }

  /// <summary>
  ///   Applies the filter.
  /// </summary>
  public void ApplyFilter()
  {
    m_ValueText = m_ValueText.Trim();
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
    // move all oldActive values back to the available list
    for (int i = ActiveValueClusterCollection.Count - 1; i >= 0; i--)
    {
      var cluster = ActiveValueClusterCollection[i];
      ValueClusterCollection.Add(cluster);
      ActiveValueClusterCollection.RemoveAt(i);
    }

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
    // Making sure end up with "col LIKE '% %'"
    m_ValueText = m_ValueText.Trim();
    if (string.IsNullOrEmpty(m_ValueText) && (string.Equals(m_Operator, cOperatorContains, StringComparison.OrdinalIgnoreCase)|| string.Equals(m_Operator, cOperatorLonger, StringComparison.OrdinalIgnoreCase)|| string.Equals(m_Operator, cOperatorShorter, StringComparison.OrdinalIgnoreCase) || string.Equals(m_Operator, cOperatorBegins, StringComparison.OrdinalIgnoreCase) || string.Equals(m_Operator, cOperatorEnds, StringComparison.OrdinalIgnoreCase)))
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

    foreach (var value in ActiveValueClusterCollection)
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