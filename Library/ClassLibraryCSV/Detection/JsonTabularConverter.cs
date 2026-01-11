using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace EIHControlCenter.Code
{
  /// <summary>
  /// Provides helper methods to convert JSON data into tabular form.
  /// Supports scalar properties, object flattening, arrays of primitives, 
  /// and arrays of objects with identity properties (id, name, title, label).
  /// </summary>
  public static class JsonTabularConverter
  {
    /// <summary>
    /// Represents a single column in a tabular representation of JSON data.
    /// Each column has a name and a function that extracts the corresponding value from a <see cref="JToken"/>.
    /// This allows flattening JSON objects, arrays, or scalar values into a table-like structure.
    /// </summary>
    public sealed record JsonColumn
    {
      /// <summary>
      /// The name of the column, used as a header when converting JSON to a table.
      /// </summary>
      public readonly string ColumnName;

      /// <summary>
      /// A function that, given a <see cref="JToken"/> representing a row, extracts the value for this column.
      /// The returned <see cref="JToken"/> may be a scalar, null, or a JValue from an array/object.
      /// </summary>
      public readonly Func<JToken, JToken?> ValueSelector;

      /// <summary>
      /// Initializes a new instance of the <see cref="JsonColumn"/> class.
      /// </summary>
      /// <param name="columnName">The name of the column.</param>
      /// <param name="valueSelector">The function to extract values from a JSON token.</param>
      public JsonColumn(string columnName, Func<JToken, JToken?> valueSelector)
      {
        ColumnName = columnName;
        ValueSelector = valueSelector;
      }
    }

    // ------------------------------------------------------------
    // Column Discovery
    // ------------------------------------------------------------

    /// <summary>
    /// Analyzes a JSON array and discovers columns suitable for tabular extraction.
    /// Scalars are directly converted to columns, objects are flattened by one level, 
    /// and arrays of objects are sampled to extract up to a limited number of scalar identity properties.
    /// Identity properties are prioritized as: id, code, name, title, label (including variants ending with _id, _code, etc.).
    /// </summary>
    /// <param name="array">The JSON array to analyze for columns.</param>
    /// <param name="limitProperties">
    /// Maximum number of properties to extract from objects inside arrays. 
    /// Defaults to 3. Only scalar properties are considered, nested objects or arrays are ignored.
    /// </param>
    /// <param name="token">Cancellation token to allow aborting the operation.</param>
    /// <returns>
    /// A read-only list of <see cref="JsonColumn"/> representing the discovered columns, 
    /// with unique column names even when arrays or objects have overlapping property names.
    /// </returns>
    public static IReadOnlyList<JsonColumn> DiscoverColumns(this JArray array, int limitProperties = 3, CancellationToken token = default)
    {
      var columns = new List<JsonColumn>();
      var columnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

      // Tracks which array properties we've already processed for object columns
      var processedArrayProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

      // Sample each entry in the array
      foreach (var entry in array)
      {
        token.ThrowIfCancellationRequested();

        foreach (var prop in entry.OfType<JProperty>())
        {
          var name = prop.Name;
          if (string.IsNullOrWhiteSpace(name)) continue;

          // Scalars
          if (prop.Value is JValue)
          {
            AddUniqueColumn(name, e => e[prop.Name]);
            continue;
          }

          // Arrays (primitive or array of objects)
          if (prop.Value is JArray arr)
          {
            if (!processedArrayProperties.Contains(name))
            {
              var objectCandidates = PickObjectProperties(limitProperties, arr.OfType<JObject>().Take(3).ToArray());

              foreach (var identityProp in objectCandidates)
              {
                var columnName = $"{name}.{identityProp.Name}";
                AddUniqueColumn(columnName, e =>
                {
                  var arrayToken = e[prop.Name] as JArray;
                  if (arrayToken == null) return null;
                  foreach (var obj in arrayToken.OfType<JObject>())
                  {
                    if (obj.TryGetValue(identityProp.Name, out var val))
                      return val;
                  }
                  return null;
                });
              }

              processedArrayProperties.Add(name);
              continue;
            }

            // Primitive array
            AddUniqueColumn(name, e => e[prop.Name]);
            continue;
          }

          // Single object → pick up to limitProperties scalar identity properties
          if (prop.Value is JObject obj)
          {
            var objectCandidates = PickObjectProperties(limitProperties, obj);

            foreach (var identityProp in objectCandidates)
            {
              var columnName = $"{name}.{identityProp.Name}";
              AddUniqueColumn(columnName, e =>
              {
                var objToken = e[prop.Name] as JObject;
                if (objToken == null) return null;
                objToken.TryGetValue(identityProp.Name, out var val);
                return val;
              });
            }
          }
        }
      }

      return columns;

      // ------------------------------------------------------------
      // Local helper: add a column with unique name
      // ------------------------------------------------------------
      bool AddUniqueColumn(string fullName, Func<JToken, JToken?> selector)
      {
        var dotIndex = fullName.IndexOf('.');
        string container = dotIndex > 0 ? fullName.Substring(0, dotIndex) : fullName;
        string property = dotIndex > 0 ? fullName.Substring(dotIndex + 1) : string.Empty;

        var baseContainer = container;
        var candidate = fullName;
        int suffix = 1;

        while (!columnNames.Add(candidate))
        {
          container = $"{baseContainer}{suffix++}";
          candidate = string.IsNullOrEmpty(property) ? container : $"{container}.{property}";
        }

        columns.Add(new JsonColumn(candidate, selector));
        return true;
      }
    }

    /// <summary>
    /// Collects up to <paramref name="limitProperties"/> scalar identity properties from one or more objects.
    /// Only considers JValue properties; nested objects or arrays are ignored.
    /// </summary>
    private static IReadOnlyCollection<JProperty> PickObjectProperties(int limitProperties, params JObject[] objects)
    {
      var candidates = new Dictionary<string, JProperty>(StringComparer.OrdinalIgnoreCase);

      foreach (var token in objects)
      {
        foreach (var p in token.Properties().Where(p => p.Value is JValue))
        {
          if (IsIdentityProperty(p.Name) && !candidates.ContainsKey(p.Name))
            candidates[p.Name] = p;

          if (candidates.Count == limitProperties) break;
        }
        if (candidates.Count == limitProperties) break;
      }

      // Fallback: pick first scalar if none found
      if (candidates.Count == 0)
      {
        var firstScalar = objects.SelectMany(o => o.Properties())
                                 .FirstOrDefault(p => p.Value is JValue);
        if (firstScalar != null)
          candidates[firstScalar.Name] = firstScalar;
      }

      return candidates.Values;
    }

    /// <summary>
    /// Determines whether a property name should be considered an identity property.
    /// </summary>
    private static bool IsIdentityProperty(string propertyName)
    {
      // id or _id
      if (propertyName.Equals("id", StringComparison.OrdinalIgnoreCase) ||
          propertyName.EndsWith("_id", StringComparison.OrdinalIgnoreCase)) return true;

      // code or _code
      if (propertyName.Equals("code", StringComparison.OrdinalIgnoreCase) ||
          propertyName.EndsWith("_code", StringComparison.OrdinalIgnoreCase)) return true;

      // name or _name
      if (propertyName.Equals("name", StringComparison.OrdinalIgnoreCase) ||
          propertyName.EndsWith("_name", StringComparison.OrdinalIgnoreCase)) return true;

      // title or _title
      if (propertyName.Equals("title", StringComparison.OrdinalIgnoreCase) ||
          propertyName.EndsWith("_title", StringComparison.OrdinalIgnoreCase)) return true;

      // label or _label
      if (propertyName.Equals("label", StringComparison.OrdinalIgnoreCase) ||
          propertyName.EndsWith("_label", StringComparison.OrdinalIgnoreCase)) return true;

      return false;
    }


    // ------------------------------------------------------------
    // JSON Content Processing
    // ------------------------------------------------------------

    /// <summary>
    /// Retrieves a JSON array from a JSON object by property name.
    /// Throws <see cref="InvalidDataException"/> if the property does not exist or is not an array.
    /// </summary>
    /// <param name="jsonData">The JSON object containing the array.</param>
    /// <param name="propertyName">The property name of the array (default: "data").</param>
    /// <returns>The JSON array.</returns>
    public static JArray GetJsonArray(this JObject jsonData, string propertyName = "data")
    {
      var container = jsonData[propertyName];
      if (container is not JArray array)
        throw new InvalidDataException($"Could not find data array '{propertyName}' in the JSON object.");

      return array;
    }

    /// <summary>
    /// Processes a JSON object by extracting a specified array (container) and converting it into tabular rows.
    /// If columns are not yet known, they are automatically discovered from the data.
    /// </summary>
    /// <param name="jsonData">The JSON object containing the data.</param>
    /// <param name="columns">The list of <see cref="JsonColumn"/> representing the table schema. 
    /// Can be empty, in which case it will be auto-discovered.</param>
    /// <param name="containerName">The property name of the JSON array to process.</param>
    /// <param name="writeHeader">Indicates whether to write column headers as the first row.</param>
    /// <param name="handleOneRow">Action to call for each row of string values (including headers).</param>
    /// <param name="token">Cancellation token to allow aborting processing.</param>
    /// <returns>The number of rows written (excluding the header row).</returns>
    public static int WriteJsonArrayAsTable(
        this JObject jsonData,
        List<JsonColumn> columns,
        string containerName,
        bool writeHeader,
        Action<IReadOnlyCollection<string>> handleOneRow,
        CancellationToken token)
    {
      // Retrieve the JSON array under the specified container name
      var array = GetJsonArray(jsonData, containerName);

      // If columns are empty, discover them automatically
      if (columns.Count == 0)
      {
        columns.AddRange(DiscoverColumns(array, 3, token));

        if (writeHeader)
        {
          // Write the column headers as the first row
          handleOneRow(columns.Select(c => c.ColumnName).ToArray());
        }
      }

      // Process each JSON element in the array and write rows
      return WriteRows(array, columns, handleOneRow, token);
    }

    // ------------------------------------------------------------
    // Row Processing
    // ------------------------------------------------------------

    /// <summary>
    /// Converts a JSON array into tabular rows using the specified columns.
    /// Handles scalars, dates, arrays of primitives, and arrays of objects (identity columns).
    /// </summary>
    /// <param name="items">The JSON array containing the rows.</param>
    /// <param name="columns">The list of <see cref="JsonColumn"/> defining the schema and value selectors.</param>
    /// <param name="handleOneRow">Action to call for each row of string values.</param>
    /// <param name="token">Cancellation token to allow aborting processing.</param>
    /// <returns>The number of rows written.</returns>
    public static int WriteRows(
        this JArray items,
        IReadOnlyCollection<JsonColumn> columns,
        Action<IReadOnlyCollection<string>> handleOneRow,
        CancellationToken token)
    {
      var written = 0;

      foreach (var item in items)
      {
        token.ThrowIfCancellationRequested();
        var row = new string[columns.Count];
        var index = 0;
        foreach (var col in columns)
        {
          string value = string.Empty;
          var tokenValue = col.ValueSelector(item);

          switch (tokenValue)
          {
            // ------------------------------------------------------------
            // Date scalar values
            // ------------------------------------------------------------
            case JValue jv when jv.Type == JTokenType.Date:
              value = ((DateTime) jv.Value!).ToString("s");
              break;

            // ------------------------------------------------------------
            // Other scalar values
            // ------------------------------------------------------------
            case JValue jv:
              value = jv.Value?.ToString()?.Trim() ?? string.Empty;
              break;

            // ------------------------------------------------------------
            // Arrays or enumerables (primitive or object identity columns)
            // Join multiple values into a single cell
            // ------------------------------------------------------------
            case IEnumerable<JToken> tokens:
            {
              var sb = new StringBuilder();
              foreach (var t in tokens)
              {
                if (t == null) continue;

                string text = t.Type == JTokenType.Date
                    ? ((DateTime) t).ToString("s")
                    : t.ToString()?.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(text)) continue;

                // Replace any commas to avoid breaking CSV formatting
                if (text.Contains(','))
                  text = text.Replace(",", "_");

                if (sb.Length > 0)
                  sb.Append(", ");

                sb.Append(text);
              }

              value = sb.ToString();
              break;
            }

            // ------------------------------------------------------------
            // Fallback for any other type
            // ------------------------------------------------------------
            default:
              value = tokenValue?.ToString()?.Trim() ?? string.Empty;
              break;
          }

          row[index++] = value;
        }

        handleOneRow(row);
        written++;
      }

      return written;
    }
  }
}
