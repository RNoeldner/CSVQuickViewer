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
    /// Represents a single column in a tabular representation derived from JSON data.
    ///
    /// A <see cref="JsonColumn"/> describes how a value is extracted from a JSON array item
    /// and how it should appear in the resulting table. It supports:
    /// <list type="bullet">
    ///   <item>Direct scalar properties</item>
    ///   <item>Flattened object properties</item>
    ///   <item>Aggregated values from arrays (primitive or object-based)</item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// The column model separates the JSON propertyName objectProperty (<see cref="JsonProperty"/>)
    /// from the object-level objectProperty (<see cref="ObjectProperty"/>) used during value extraction.
    /// The final column header is derived from both values, allowing arrays of objects
    /// to expose multiple logical columns (e.g. <c>tags.id</c>, <c>tags.name</c>)
    /// while sharing the same JSON propertyName.
    /// </remarks>
    public sealed record JsonColumn
    {
      /// <summary>
      /// Gets the logical column name used as a header in the tabular output.
      /// Represents the top-level JSON objectProperty name (the propertyName), e.g., "tags" or "status".
      /// When combined with <see cref="ObjectProperty"/>, it forms the full column path (e.g., "tags.id").
      /// </summary>
      public readonly string JsonProperty;

      /// <summary>
      /// Gets the name of the objectProperty inside a JSON object propertyName.
      /// This value is used when the column represents a specific scalar objectProperty
      /// inside an object or an array of objects (e.g., "id" or "name").
      /// An empty value indicates that the column maps directly to a scalar value
      /// or a primitive array at the propertyName level.
      /// </summary>
      public readonly string ObjectProperty;

      /// <summary>
      /// Gets the .NET type of the objectProperty. Used for formatting values consistently.
      /// </summary>
      public readonly Type PropertyType;

      /// <summary>
      /// Gets the fully-qualified column header name as it appears in the tabular output.
      /// </summary>
      public string HeaderName =>
          string.IsNullOrEmpty(ObjectProperty)
              ? JsonProperty
              : $"{JsonProperty}.{Pluralize(ObjectProperty)}";


      private static readonly HashSet<string> Acronyms =
  new(StringComparer.OrdinalIgnoreCase)
  {
    "id",
    "uuid",
    "guid",
    "uid",
    "url",
    "uri",
    "ip",
    "api",
    "jwt",
    "oauth",
    "key",
    "ver",
    "sec"
  };
      private static string Pluralize(string name)
      {
        if (string.IsNullOrWhiteSpace(name))
          return name;

        var lower = name.ToLowerInvariant();

        // Handle snake_case: pluralize last segment only
        var lastUnderscore = name.LastIndexOf('_');
        if (lastUnderscore >= 0)
        {
          // Replace this line in Pluralize method:
          // with the following lines to avoid using System.Range/System.Index (C# 8+ feature):

          var prefix = name.Substring(0, lastUnderscore + 1);
          var tail = name.Substring(lastUnderscore + 1);
          return prefix + Pluralize(tail);
        }

        // Acronyms: id → ids, uuid → uuids, url → urls
        if (Acronyms.Contains(lower))
          return name + "s";

        // s, x, z, ch, sh → es
        if (lower.EndsWith("s") ||
            lower.EndsWith("x") ||
            lower.EndsWith("z") ||
            lower.EndsWith("ch") ||
            lower.EndsWith("sh"))
          return name + "es";

        // consonant + y → ies
        if (lower.EndsWith("y") && lower.Length > 1 &&
            !"aeiou".Contains(lower[lower.Length - 2]))
          return name.Substring(0, name.Length - 1) + "ies";

        // default
        return name + "s";
      }
      /// <summary>
      /// Initializes a new instance of the <see cref="JsonColumn"/> class.
      /// </summary>
      /// <param name="propertyName">The top-level JSON objectProperty name that acts as the propertyName. Required.</param>
      /// <param name="objectProperty">The scalar objectProperty inside the propertyName. Empty if mapping directly to the propertyName.</param>
      /// <param name="propertyType">The .NET type of the objectProperty.</param>
      /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyName"/> is null or empty.</exception>
      public JsonColumn(string propertyName, string objectProperty, Type propertyType)
      {
        if (string.IsNullOrEmpty(propertyName))
          throw new ArgumentNullException(nameof(propertyName));
        JsonProperty = propertyName;
        ObjectProperty = objectProperty ?? string.Empty;
        PropertyType = propertyType;
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
    /// <param name="limitProperties">Maximum number of properties to extract from objects inside arrays. Defaults to 3.</param>
    /// <param name="token">Cancellation token to allow aborting the operation.</param>
    /// <returns>A read-only list of <see cref="JsonColumn"/> representing the discovered columns.</returns>
    public static IReadOnlyList<JsonColumn> DiscoverColumns(this JArray array, int limitProperties = 3, CancellationToken token = default)
    {
      var columns = new List<JsonColumn>();
      var columnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      var processedArrayProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

      foreach (var entry in array)
      {
        token.ThrowIfCancellationRequested();

        foreach (var prop in entry.OfType<JProperty>())
        {
          var name = prop.Name;
          if (string.IsNullOrWhiteSpace(name)) continue;

          // Scalar values are added as columns directly
          if (prop.Value is JValue)
            AddUniqueColumn(name, string.Empty, GetDotNetType(prop.Value));
          // Single object → pick identity properties
          else if (prop.Value is JObject obj)
          {
            foreach (var identityProp in PickObjectProperties(limitProperties, obj))
              AddUniqueColumn(name, identityProp.Name, GetDotNetType(identityProp.Value));
          }
          // Arrays (primitive or objects)
          else if (prop.Value is JArray arr && !processedArrayProperties.Contains(name))
          {
            // Array of primitives → concatenate values into a single cell
            if (arr.Any(t => t is JValue))
            {
              AddUniqueColumn(name, string.Empty, typeof(string));
              processedArrayProperties.Add(name);
              continue;
            }

            // Array of objects → pick identity-based properties
            var objectSamples = arr.OfType<JObject>().Take(3).ToArray();
            if (objectSamples.Length > 0)
            {
              foreach (var identityProp in PickObjectProperties(limitProperties, objectSamples))
                AddUniqueColumn(name, identityProp.Name, typeof(string));
              processedArrayProperties.Add(name);
            }
          }
        }
      }

      return columns;

      // ------------------------------------------------------------
      // Local helper: add a column with a unique header
      // ------------------------------------------------------------
      bool AddUniqueColumn(string propertyName, string objectProperty, Type type)
      {
        var json = new JsonColumn(propertyName, objectProperty, type);
        if (columnNames.Add(json.HeaderName))
        {
          columns.Add(json);
          return true;
        }
        return false;
      }

      Type GetDotNetType(JToken token) => token.Type switch
      {
        JTokenType.Integer => typeof(long),
        JTokenType.Float => typeof(double),
        JTokenType.String => typeof(string),
        JTokenType.Boolean => typeof(bool),
        JTokenType.Date => typeof(DateTime),
        _ => typeof(string)
      };
    }

    /// <summary>
    /// Collects up to <paramref name="limitProperties"/> scalar identity properties from one or more objects.
    /// Only considers scalar JValue properties; nested objects or arrays are ignored.
    /// </summary>
    private static IReadOnlyCollection<JProperty> PickObjectProperties(int limitProperties, params JObject[] objects)
    {
      var candidates = new Dictionary<string, JProperty>(StringComparer.OrdinalIgnoreCase);

      // Collect all scalar properties
      var allScalars = objects
          .SelectMany(o => o.Properties().Where(p => p.Value is JValue))
          .ToList();

      // Helper: check property priority
      int GetPriority(string name)
      {
        if (name.Equals("id", StringComparison.OrdinalIgnoreCase)) return 0; // highest priority
        if (name.EndsWith("_id", StringComparison.OrdinalIgnoreCase)) return 1;
        return 2; // all others
      }

      // Order properties by priority and uniqueness
      foreach (var prop in allScalars.OrderBy(p => GetPriority(p.Name)))
      {
        if (!candidates.ContainsKey(prop.Name))
          candidates[prop.Name] = prop;
        if (candidates.Count == limitProperties) break;
      }

      // Fallback: pick first scalar if none found (should rarely happen)
      if (candidates.Count == 0 && allScalars.Count > 0)
        candidates[allScalars[0].Name] = allScalars[0];

      return candidates.Values;
    }


    /// <summary>
    /// Determines whether a objectProperty name should be considered an identity objectProperty.
    /// </summary>
    private static bool IsIdentityProperty(string propertyName)
    {
      return propertyName.Equals("id", StringComparison.OrdinalIgnoreCase) || propertyName.EndsWith("_id", StringComparison.OrdinalIgnoreCase) ||
             propertyName.Equals("code", StringComparison.OrdinalIgnoreCase) || propertyName.EndsWith("_code", StringComparison.OrdinalIgnoreCase) ||
             propertyName.Equals("name", StringComparison.OrdinalIgnoreCase) || propertyName.EndsWith("_name", StringComparison.OrdinalIgnoreCase) ||
             propertyName.Equals("title", StringComparison.OrdinalIgnoreCase) || propertyName.EndsWith("_title", StringComparison.OrdinalIgnoreCase) ||
             propertyName.Equals("label", StringComparison.OrdinalIgnoreCase) || propertyName.EndsWith("_label", StringComparison.OrdinalIgnoreCase);
    }

    // ------------------------------------------------------------
    // JSON Content Processing
    // ------------------------------------------------------------

    /// <summary>
    /// Retrieves a JSON array from a JSON object by objectProperty name.
    /// Throws <see cref="InvalidDataException"/> if the objectProperty does not exist or is not an array.
    /// </summary>
    public static JArray GetJsonArray(this JObject jsonData, string propertyName = "data")
    {
      var container = jsonData[propertyName];
      if (container is not JArray array)
        throw new InvalidDataException($"Could not find data array '{propertyName}' in the JSON object.");

      return array;
    }

    /// <summary>
    /// Processes a JSON object by extracting a specified array (propertyName) and converting it into tabular rows.
    /// Columns are discovered automatically if the list is empty.
    /// </summary>
    [Obsolete()]
    public static int WriteJsonArrayAsTable(
        this JObject jsonData,
        List<JsonColumn> columns,
        string arrayName,
        bool writeHeader,
        Action<IReadOnlyCollection<string>> handleOneRow,
        CancellationToken token)
    {
      var array = GetJsonArray(jsonData, arrayName);

      if (columns.Count == 0)
      {
        columns.AddRange(DiscoverColumns(array, 3, token));
        if (writeHeader)
          handleOneRow(columns.Select(c => c.HeaderName).ToArray());
      }

      return WriteRows(array, columns, handleOneRow, token);
    }

    // ------------------------------------------------------------
    // Row Processing
    // ------------------------------------------------------------

    /// <summary>
    /// Converts a JSON array into tabular rows using the specified columns.
    /// All values are returned as strings, including dates, numbers, and booleans.
    /// Arrays are concatenated into single-cell strings.
    /// </summary>
    public static int WriteRows(
        this JArray array,
        IReadOnlyCollection<JsonColumn> columns,
        Action<IReadOnlyCollection<string>> handleOneRow,
        CancellationToken token)
    {
      var written = 0;
      foreach (var arrayItem in array)
      {
        token.ThrowIfCancellationRequested();
        var row = new string[columns.Count];
        var index = 0;

        foreach (var col in columns)
        {
          string value = string.Empty;
          var tokenValue = arrayItem[col.JsonProperty];
          if (tokenValue == null)
          {
            row[index++] = string.Empty;
            continue;
          }

          switch (tokenValue)
          {
            case JArray internalArray:
              var sb = new StringBuilder();
              if (!string.IsNullOrEmpty(col.ObjectProperty))
                foreach (var jObject in internalArray.OfType<JObject>())
                  HandleList(jObject[col.ObjectProperty], sb, col.PropertyType);

              foreach (var t in internalArray.OfType<JValue>())
                HandleList(t, sb, col.PropertyType);

              value = sb.ToString();
              break;

            case JObject jObject:
              value = HandleValue(jObject[col.ObjectProperty], col.PropertyType);
              break;

            default:
              value = HandleValue(tokenValue, col.PropertyType);
              break;
          }
          row[index++] = value;
        }

        handleOneRow(row);
        written++;
      }

      return written;

      void HandleList(JToken? t, StringBuilder sb, Type type)
      {
        string text = HandleValue(t, type);
        // Replace commas inside values to avoid breaking CSV or list delimiters
        if (text.Contains(',')) text = text.Replace(",", "_");
        if (sb.Length > 0) sb.Append(", ");
        sb.Append(text);
      }

      string HandleValue(JToken? t, Type type)
      {
        if (t is null || t.Type == JTokenType.Null)
          return string.Empty;
        try
        {
          return type switch
          {
            var dt when dt == typeof(DateTime) => t.Value<DateTime>().ToString("s"),
            var b when b == typeof(bool) => t.Value<bool>().ToString().ToLower(),
            var n when n == typeof(double) => t.Value<double>().ToString("G"),
            var l when l == typeof(long) => t.Value<long>().ToString(),
            _ => t.ToString()?.Trim() ?? string.Empty
          };
        }
        catch
        {
          // fallback if cast fails
          return t.ToString()?.Trim() ?? string.Empty;
        }
      }
    }
  }
}
