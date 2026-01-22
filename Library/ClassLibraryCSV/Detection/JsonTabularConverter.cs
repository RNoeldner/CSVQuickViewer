using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace CsvTools;

/// <summary>
/// Provides helper methods to convert JSON data into tabular form.
/// Supports scalar properties, object flattening, arrays of primitives, 
/// and arrays of objects with identity properties (id, name, title, label).
/// Also supports streaming large JSON files without loading the entire content into memory.
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
      new(StringComparer.OrdinalIgnoreCase)  {
          "id",    "uuid",    "guid",    "uid",
          "url",    "uri",    "ip",    "api",
          "jwt",    "oauth",    "key",    "ver",
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
      if (lower.EndsWith("s", StringComparison.OrdinalIgnoreCase) ||
          lower.EndsWith("x", StringComparison.OrdinalIgnoreCase) ||
          lower.EndsWith("z", StringComparison.OrdinalIgnoreCase) ||
          lower.EndsWith("ch", StringComparison.OrdinalIgnoreCase) ||
          lower.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
        return name + "es";

      // consonant + y → ies
      if (lower.EndsWith("y", StringComparison.OrdinalIgnoreCase) && lower.Length > 1 &&
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
  private static readonly Dictionary<string, int> NamePriority =
      new(StringComparer.OrdinalIgnoreCase)
      {
        ["id"]    = 0,
        ["code"]  = 1,
        ["name"]  = 1,
        ["title"] = 2
      };

  /// <summary>
  /// Discovers tabular columns from a sequence of JSON objects.
  /// Only samples the first few objects for performance.
  /// Scalars are mapped directly, objects are flattened, and arrays are sampled.
  /// </summary>
  /// <param name="array">Sequence of JSON objects to analyze.</param>
  /// <param name="limitProperties">Max number of object properties per column (default 3).</param>
  /// <param name="token">Cancellation token.</param>
  /// <returns>
  /// A tuple:
  /// <list type="bullet">
  ///   <item>Read-only list of discovered <see cref="JsonColumn"/> objects.</item>
  ///   <item>Sampled JSON objects used for discovery.</item>
  /// </list>
  /// </returns>
  public static IReadOnlyList<JsonColumn> DiscoverColumns(this IEnumerable<JObject> array, int limitProperties = 3, CancellationToken token = default)
  {
    var columns = new List<JsonColumn>();
    var columnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var processedArrayProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    using var enumerator = array.GetEnumerator();

    for (int i = 0; i < 5 && enumerator.MoveNext(); i++)
    {
      token.ThrowIfCancellationRequested();
      foreach (var prop in enumerator.Current.OfType<JProperty>())
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
  /// Extracts values for a single JSON object according to the provided column definitions
  /// and emits them in column order via <paramref name="handleColumn" />.
  ///
  /// Responsibilities:
  /// - Maps scalar properties directly to a column
  /// - Flattens objects by extracting the configured object property
  /// - Aggregates arrays into a single cell using <paramref name="valueSeparator" />
  /// - Ensures stable column ordering and safe string output
  ///
  /// Design notes:
  /// - Arrays are flattened into a single cell rather than expanding rows
  /// - Missing or null values always produce an empty string
  /// - No exceptions are thrown for malformed or unexpected JSON shapes
  /// </summary>
  public static void HandleRow(
      this JObject currentObject,
      IReadOnlyCollection<JsonColumn> columns,
      char valueSeparator,
      Action<int, string> handleColumn,
      Action<int, object?>? handleColumnValue = null)
  {
    var index = 0;
    var sb = new StringBuilder(); // Reuse per row
                                  // Iterate columns in discovery order to guarantee stable output layout
    foreach (var col in columns)
    {
      string value = string.Empty;
      // Resolve the top-level JSON property for this column
      var tokenValue = currentObject[col.JsonProperty];
      if (tokenValue == null)
      {
        handleColumn(index++, string.Empty);
        continue;
      }
      var resultType = typeof(string);
      switch (tokenValue)
      {
        case JArray internalArray:
          sb.Clear(); // Reset for this column
                      // Arrays are flattened into a single cell
                      // using the configured list separator (e.g. ',' or '|')
          if (!string.IsNullOrEmpty(col.ObjectProperty))
            foreach (var jObject in internalArray.OfType<JObject>())
              HandleList(jObject[col.ObjectProperty], sb, col.PropertyType, valueSeparator);

          foreach (var t in internalArray.OfType<JValue>())
            HandleList(t, sb, col.PropertyType, valueSeparator);

          value = sb.ToString();
          break;

        case JObject jObject:
          // Single object: extract the configured object property
          value = HandleValueAsString(jObject[col.ObjectProperty], col.PropertyType);
          resultType = col.PropertyType;
          break;

        default:
          // Scalar value: convert directly
          value = HandleValueAsString(tokenValue, col.PropertyType);
          resultType = col.PropertyType;
          break;
      }
      handleColumnValue?.Invoke(index, HandleValueTyped(tokenValue, resultType));
      handleColumn(index++, value);
    }

    static void HandleList(JToken? t, StringBuilder sb, Type type, char valueSeparator)
    {
      // Convert the token to its string representation using column type rules
      string text = HandleValueAsString(t, type);
      // Replace occurrences of the list separator inside values
      // to avoid breaking CSV or delimited exports
      if (text.Contains(valueSeparator)) text = text.Replace(valueSeparator.ToString(), "_");
      // Append separator only between values
      if (sb.Length > 0) { sb.Append(valueSeparator); sb.Append(' '); }
      sb.Append(text);
    }

    static string HandleValueAsString(JToken? t, Type type)
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

    static object? HandleValueTyped(JToken? t, Type type)
    {
      if (t is null || t.Type == JTokenType.Null)
        return null;
      try
      {
        return type switch
        {
          var dt when dt == typeof(DateTime) => t.Value<DateTime>(),
          var b when b == typeof(bool) => t.Value<bool>(),
          var n when n == typeof(double) => t.Value<double>(),
          var l when l == typeof(long) => t.Value<long>(),
          _ => t.ToString()
        };
      }
      catch
      {
        // fallback if cast fails
        return t.ToString();
      }
    }
  }

  /// <summary>
  /// Streams JSON objects from a <see cref="TextReader"/> for large or nested JSON files.
  /// Scalars found at the root level are captured as metadata.
  /// Supports arrays at root, nested objects, and multiple top-level properties.
  /// </summary>
  /// <param name="reader">TextReader containing JSON content.</param>
  /// <returns>
  /// A tuple:
  /// <list type="bullet">
  ///   <item>An <see cref="IEnumerable{JObject}"/> of streamed JSON objects.</item>
  ///   <item>A dictionary of metadata scalars found at root level.</item>
  /// </list>
  /// </returns>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> is null.</exception>
  /// <exception cref="InvalidDataException">Thrown if JSON is empty or unsupported.</exception>
  /// Caller is responsible for keeping the TextReader open
  /// for the duration of enumeration.
  public static (IEnumerable<JObject> Items, Dictionary<string, JValue> Metadata) StreamJsonObjects(this TextReader reader)
  {
    if (reader == null) throw new ArgumentNullException(nameof(reader));

    var metadata = new Dictionary<string, JValue>(StringComparer.OrdinalIgnoreCase);

    IEnumerable<JObject> Enumerate()
    {
      var jsonReader = new JsonTextReader(reader) { SupportMultipleContent = true };

      // There is no async method for this so all keeps synchronous
      while (jsonReader.Read())
      {
        switch (jsonReader.TokenType)
        {
          case JsonToken.StartArray:
            while (jsonReader.Read())
            {
              if (jsonReader.TokenType == JsonToken.StartObject)
                yield return JObject.Load(jsonReader);
              else if (jsonReader.TokenType == JsonToken.EndArray)
                break;
            }
            break;

          case JsonToken.StartObject:
            var obj = JObject.Load(jsonReader);

            // Capture root-level scalar properties
            foreach (var prop in obj.Properties())
            {
              if (prop.Value is JValue jValue)
                metadata[prop.Name] = jValue;
            }

            bool yieldedArrayObjects = false;

            foreach (var prop in obj.Properties())
            {
              if (prop.Value is JArray arr)
              {
                foreach (var item in arr.OfType<JObject>())
                  yield return item;
                yieldedArrayObjects = true;
              }
              else if (prop.Value is JObject childObj)
              {
                // Yield top-level child objects (e.g., collection, collection2)
                yield return childObj;
                yieldedArrayObjects = true;
              }
            }

            // If nothing yielded (scalar-only root object), yield root itself
            if (!yieldedArrayObjects)
              yield return obj;

            break;

          case JsonToken.Comment:
          case JsonToken.None:
            continue;

          default:
            throw new InvalidDataException($"Unsupported JSON token: {jsonReader.TokenType}");
        }
      }
    }

    return (Enumerate(), metadata);
  }

  // ------------------------------------------------------------
  // Row Processing
  // ------------------------------------------------------------

  /// <summary>
  /// Reads JSON objects from a <see cref="TextReader"/>, discovers tabular columns from the first few rows,
  /// and writes all rows as strings using the provided callback. Supports streaming large JSON files without 
  /// loading the entire dataset into memory.
  /// </summary>
  /// <param name="reader">The <see cref="TextReader"/> containing JSON data. Caller is responsible for disposing it.</param>
  /// <param name="handleOneRow">
  /// Callback invoked for each row. Receives a read-only collection of string values in column order.
  /// Arrays are flattened into a single cell using <paramref name="valueSeparator"/>.
  /// </param>
  /// <param name="valueSeparator">
  /// Character used to join multiple values from arrays into a single cell (default is ',').
  /// Any occurrences of this character inside values are replaced with '_'.
  /// </param>
  /// <param name="sampleSize">Number of rows to check to determine the columns</param>
  /// <param name="cancellationToken">Token to cancel processing at any time.</param>
  /// <returns>
  /// A tuple containing:
  /// <list type="bullet">
  ///   <item><see cref="IReadOnlyCollection{JsonColumn}"/>: the discovered columns in order.</item>
  ///   <item><see cref="Dictionary{String, JValue}"/>: metadata scalars found at the root of the JSON.</item>
  /// </list>
  /// </returns>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="reader"/> or <paramref name="handleOneRow"/> is null.</exception>
  /// <remarks>
  /// Column discovery is performed by reading up to the first 5 rows. Those rows are immediately written
  /// using <paramref name="handleOneRow"/>, after which streaming continues for the remaining objects.  
  /// Column values are converted to strings, and arrays are joined using <paramref name="valueSeparator"/>.
  /// </remarks>
  public static (IReadOnlyCollection<JsonColumn> Columns, Dictionary<string, JValue> Metadata)
    StreamRows(this TextReader reader, Action<IReadOnlyCollection<string>> handleOneRow, char valueSeparator = ',', int sampleSize = 5, CancellationToken cancellationToken = default)
  {
    var (items, metadata) = reader.StreamJsonObjects();
    var enumerator = items.GetEnumerator();

    // Discover columns from first N rows
    var firstRows = new List<JObject>();
    for (int i = 0; i < sampleSize && enumerator.MoveNext(); i++)
      firstRows.Add(enumerator.Current);

    var columns = firstRows.DiscoverColumns(sampleSize, cancellationToken);
    if (columns.Count>0)
    {
      // Write first rows immediately
      foreach (var row in firstRows)
      {
        var columnValues = new string[columns.Count];
        row.HandleRow(columns, valueSeparator, (idx, val) => columnValues[idx]=val, null);
        // Pass copy to avoid overwrites
        handleOneRow(columnValues);
      }

      // Continue streaming the rest
      while (enumerator.MoveNext())
      {
        cancellationToken.ThrowIfCancellationRequested();
        var columnValues = new string[columns.Count];
        enumerator.Current.HandleRow(columns, valueSeparator, (idx, val) => columnValues[idx]=val, null);
        // Pass copy to avoid overwrites
        handleOneRow(columnValues);
      }
    }
    return (columns, metadata);
  }

  /// <summary>
  /// Collects up to <paramref name="limitProperties"/> scalar identity properties from one or more objects.
  /// Only considers scalar JValue properties; nested objects or arrays are ignored.
  /// </summary>
  private static IReadOnlyCollection<JProperty> PickObjectProperties(int limitProperties, params JObject[] objects)
  {
    var candidates = new Dictionary<string, JProperty>(StringComparer.OrdinalIgnoreCase);
    var firstSeen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    var counter = 0;
    // Collect all scalar properties
    var allScalars = objects
    .SelectMany(o => o.Properties())
    .Where(p => p.Value is JValue)
    .Select(p =>
    {
      if (!firstSeen.ContainsKey(p.Name))
        firstSeen[p.Name] = counter++;

      return new
      {
        Property = p,
        FirstSeenIndex = firstSeen[p.Name]
      };
    })
    .ToList();

    // Order by semantic importance first, then by structural appearance
    foreach (var prop in allScalars
            .OrderBy(p =>
            {
              // Exact match in NamePriority
              if (NamePriority.TryGetValue(p.Property.Name, out var exactPriority))
                return exactPriority;

              // Suffix match: _id, _code, _name, _title in NamePriority
              foreach (var kvp in NamePriority)
              {
                if (p.Property.Name.EndsWith("_" + kvp.Key, StringComparison.OrdinalIgnoreCase))
                  return kvp.Value + 50;
              }

              // All others
              return 100;
            })
            .ThenBy(p => p.FirstSeenIndex)
            .Select(x => x.Property))
    {
      if (!candidates.ContainsKey(prop.Name))
        candidates[prop.Name] = prop;
      if (candidates.Count == limitProperties) break;
    }

    // Fallback: pick first scalar if none found (should rarely happen)
    if (candidates.Count == 0 && allScalars.Count > 0)
    {
      var first = allScalars.OrderBy(p => p.FirstSeenIndex).Select(x => x.Property).First();
      candidates[first.Name] = first;
    }

    return candidates.Values;
  }
}
