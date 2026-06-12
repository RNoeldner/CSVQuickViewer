using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace CsvTools;

/// <summary>
/// Represents a memory-efficient, type-safe discriminated union for common data types.
/// As a value type (struct), it minimizes GC pressure by avoiding heap allocations for 
/// numeric types and storing data contiguously in arrays.
/// </summary>
public readonly record struct TextValue
{
  /// <summary>
  /// Gets a static instance of an empty <see cref="TextValue"/>.
  /// </summary>
  public static readonly TextValue Empty = new TextValue();

  /// <summary>Gets the current data type stored in this instance.</summary>
  public DataTypeEnum Type { get; }

  /// <summary>Gets the string value. Returns <see cref="string.Empty"/> if not a string.</summary>
  public string Text { get; } = string.Empty;

  /// <summary>Gets the 64-bit integer value.</summary>
  public long Integer { get; }

  /// <summary>Gets the high-precision decimal value.</summary>
  public decimal Numeric { get; }

  /// <summary>Gets the double-precision floating-point value.</summary>
  public double Double { get; }

  /// <summary>Gets the date and time value.</summary>
  public DateTime DateTime { get; }

  /// <summary>Gets the boolean value.</summary>
  public bool Boolean { get; }

  #region Constructors
  /// <summary>
  /// Initializes a new instance of the <see cref="TextValue"/> struct.
  /// Defaults to <see cref="DataTypeEnum.String"/> with an empty string.
  /// </summary>
  public TextValue() : this(string.Empty) { }

  /// <summary>Initializes a new <see cref="TextValue"/> as a String.</summary>
  public TextValue(string value) { Type = DataTypeEnum.String; Text = value ?? string.Empty; }

  /// <summary>Initializes a new <see cref="TextValue"/> as an Integer.</summary>
  public TextValue(long value) { Type = DataTypeEnum.Integer; Integer = value; }

  /// <summary>Initializes a new <see cref="TextValue"/> as a Numeric (Decimal).</summary>
  public TextValue(decimal value) { Type = DataTypeEnum.Numeric; Numeric = value; }

  /// <summary>Initializes a new <see cref="TextValue"/> as a Double.</summary>
  public TextValue(double value) { Type = DataTypeEnum.Double; Double = value; }

  /// <summary>Initializes a new <see cref="TextValue"/> as a DateTime.</summary>
  public TextValue(DateTime value) { Type = DataTypeEnum.DateTime; DateTime = value; }

  /// <summary>Initializes a new <see cref="TextValue"/> as a Boolean.</summary>
  public TextValue(bool value) { Type = DataTypeEnum.Boolean; Boolean = value; }
  #endregion

  #region Explicit Operators (Using 'in' for performance)

  /// <summary>
  /// Explicitly converts a <see cref="TextValue"/> to a string.
  /// </summary>
  /// <param name="v">The value to convert. Passed by reference using 'in' to avoid copying the large struct.</param>
  /// <exception cref="InvalidCastException">Thrown if the underlying type is not String.</exception>
  public static explicit operator string(in TextValue v) =>
      v.Type == DataTypeEnum.String ? v.Text : throw new InvalidCastException($"Cannot cast {v.Type} to string.");

  /// <summary>
  /// Explicitly converts a <see cref="TextValue"/> to a long. Supports conversion from Integer, Numeric, and Double.
  /// </summary>
  /// <param name="v">The value to convert (passed by reference).</param>
  public static explicit operator long(in TextValue v) => v.Type switch
  {
    DataTypeEnum.Integer => v.Integer,
    DataTypeEnum.Numeric => (long) v.Numeric,
    DataTypeEnum.Double => (long) v.Double,
    _ => throw new InvalidCastException($"Cannot convert {v.Type} to long.")
  };

  /// <summary>
  /// Explicitly converts a <see cref="TextValue"/> to an int with overflow checking.
  /// </summary>
  /// <param name="v">The value to convert (passed by reference).</param>
  /// <exception cref="OverflowException">Thrown if the value exceeds the range of a 32-bit integer.</exception>
  public static explicit operator int(in TextValue v) => v.Type switch
  {
    DataTypeEnum.Integer => checked((int) v.Integer),
    DataTypeEnum.Numeric => (int) v.Numeric,
    DataTypeEnum.Double => checked((int) v.Double),
    _ => throw new InvalidCastException($"Cannot convert {v.Type} to int.")
  };

  /// <summary>
  /// Explicitly converts a <see cref="TextValue"/> to a decimal. Supports conversion from Numeric, Integer, and Double.
  /// </summary>
  /// <param name="v">The value to convert (passed by reference).</param>
  public static explicit operator decimal(in TextValue v) => v.Type switch
  {
    DataTypeEnum.Numeric => v.Numeric,
    DataTypeEnum.Integer => v.Integer,
    DataTypeEnum.Double => (decimal) v.Double,
    _ => throw new InvalidCastException($"Cannot cast {v.Type} to decimal.")
  };

  /// <summary>
  /// Explicitly converts a <see cref="TextValue"/> to a double. Supports conversion from Double, Integer, and Numeric.
  /// </summary>
  /// <param name="v">The value to convert (passed by reference).</param>
  public static explicit operator double(in TextValue v) => v.Type switch
  {
    DataTypeEnum.Double => v.Double,
    DataTypeEnum.Integer => v.Integer,
    DataTypeEnum.Numeric => (double) v.Numeric,
    _ => throw new InvalidCastException($"Cannot cast {v.Type} to double.")
  };

  /// <summary>
  /// Explicitly converts a <see cref="TextValue"/> to a DateTime.
  /// </summary>
  /// <param name="v">The value to convert (passed by reference).</param>
  public static explicit operator DateTime(in TextValue v) =>
      v.Type == DataTypeEnum.DateTime ? v.DateTime : throw new InvalidCastException($"Cannot cast {v.Type} to DateTime.");

  /// <summary>
  /// Explicitly converts a <see cref="TextValue"/> to a boolean.
  /// </summary>
  /// <param name="v">The value to convert (passed by reference).</param>
  public static explicit operator bool(in TextValue v) =>
      v.Type == DataTypeEnum.Boolean ? v.Boolean : throw new InvalidCastException($"Cannot cast {v.Type} to bool.");
  #endregion

  /// <summary>
  /// Boxes and returns the underlying value as a generic object.
  /// Use this primarily for interoperability or late-bound scenarios.
  /// </summary>
  public object GetValue() => Type switch
  {
    DataTypeEnum.String => Text,
    DataTypeEnum.Integer => Integer,
    DataTypeEnum.Numeric => Numeric,
    DataTypeEnum.Double => Double,
    DataTypeEnum.DateTime => DateTime,
    DataTypeEnum.Boolean => Boolean,
    _ => throw new InvalidOperationException("Unknown Type")
  };

  /// <summary>Returns the string representation of the stored value.</summary>
  public override string ToString() => GetValue().ToString() ?? string.Empty;
}

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
      {
        return name + "es";
      }

      // consonant + y → ies
      if (lower.EndsWith("y", StringComparison.OrdinalIgnoreCase) && lower.Length > 1 &&
          !"aeiou".Contains(lower[lower.Length - 2]))
      {
        return name.Substring(0, name.Length - 1) + "ies";
      }

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
      if (enumerator.Current == null) continue;
      foreach (var prop in enumerator.Current.OfType<JProperty>())
      {
        var name = prop.Name;
        if (string.IsNullOrWhiteSpace(name)) continue;

        switch (prop.Value)
        {
          // Scalar values are added as columns directly
          case JValue:
            AddUniqueColumn(name, string.Empty, GetDotNetType(prop.Value));
            break;
          // Single object → pick identity properties
          case JObject obj:
          {
            foreach (var identityProp in PickObjectProperties(limitProperties, obj))
              AddUniqueColumn(name, identityProp.Name, GetDotNetType(identityProp.Value));
            break;
          }
          // Arrays (primitive or objects)
          case JArray arr when !processedArrayProperties.Contains(name):
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
            if (objectSamples.Length == 0) continue;
            foreach (var identityProp in PickObjectProperties(limitProperties, objectSamples))
              AddUniqueColumn(name, identityProp.Name, typeof(string));
            processedArrayProperties.Add(name);
            break;
          }
        }
      }
    }

    return columns;

    // ------------------------------------------------------------
    // Local helper: add a column with a unique header
    // ------------------------------------------------------------
    void AddUniqueColumn(string propertyName, string objectProperty, Type type)
    {
      var json = new JsonColumn(propertyName, objectProperty, type);
      if (columnNames.Add(json.HeaderName))
      {
        columns.Add(json);
      }
    }

    Type GetDotNetType(JToken jToken) => jToken.Type switch
    {
      JTokenType.Integer => typeof(long),
      JTokenType.Float => typeof(double),
      JTokenType.String => typeof(string),
      JTokenType.Boolean => typeof(bool),
      JTokenType.Date => typeof(DateTime),
      JTokenType.Null => typeof(DBNull),
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
      Action<int, string, object?> handleColumn)
  {
    var index = 0;
    var sb = new StringBuilder(); // Reuse per row
                                  // Iterate columns in discovery order to guarantee stable output layout
    foreach (var col in columns)
    {
      string value;
      // Resolve the top-level JSON property for this column
      var tokenValue = currentObject[col.JsonProperty];
      if (tokenValue == null)
      {
        handleColumn(index++, string.Empty, null);
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
      handleColumn(index++, value, HandleValueTyped(tokenValue, resultType));
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
          var dt when dt == typeof(DateTime) => t.Value<DateTime>().ToString("s", CultureInfo.CurrentCulture),
          var b when b == typeof(bool) => t.Value<bool>().ToString(CultureInfo.CurrentCulture).ToLower(CultureInfo.CurrentCulture),
          var n when n == typeof(double) => t.Value<double>().ToString("G", CultureInfo.CurrentCulture),
          var l when l == typeof(long) => t.Value<long>().ToString(CultureInfo.CurrentCulture),
          _ => t.ToString().Trim() ?? string.Empty
        };
      }
      catch
      {
        // fallback if cast fails
        return t.ToString().Trim() ?? string.Empty;
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
  /// A tuple:
  /// <list type="bullet">
  ///   <item>An <see cref="IEnumerable{JObject}"/> of streamed JSON objects.</item>
  ///   <item>A dictionary of metadata scalars found at root level.</item>
  /// </list>
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
#if DEBUG
      var txt = reader.ReadToEnd();
      using var sr = new StringReader(txt);
      var jsonReader = new JsonTextReader(sr) { SupportMultipleContent = true };
#else
      var jsonReader = new JsonTextReader(reader) { SupportMultipleContent = true };
#endif
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

            // Sometime we get an extra layer and the actually array we look for is 
            // encapsulated e.G.    "result": { "items": [
            // this is track in hasArrayLevel1
            var hasArrayLevel1 = false;
            // Capture root-level scalar properties
            foreach (var prop in obj.Properties())
            {
              if (prop.Value is JValue jValue)
                metadata[prop.Name] = jValue;
              if (prop.Value is JArray)
                hasArrayLevel1=true;
            }
            if (!hasArrayLevel1)
            {
              foreach (var prop in obj.Properties())
              {
                if (prop.Value is JObject jObj)
                {
                  var sub = jObj.Properties().ToList();
                  // Only in case we do not 1 to 2 properties and we do not have any array
                  if (sub.Count < 3)
                  {
                    foreach (var subI in sub)
                    {
                      if (subI.Value is JArray arr)
                      {
                        foreach (var item in arr.OfType<JObject>())
                          yield return item;
                        break;
                      }
                    }
                  }
                }
              }
            }
            else
            {
              // Instaed of then making an yieldedArrayObjects we want these items
              bool yieldedArrayObjects = false;
              foreach (var propToken in obj.Properties().Select(prop => prop.Value))
              {
                switch (propToken)
                {
                  case JArray arr:
                  {
                    foreach (var item in arr.OfType<JObject>())
                      yield return item;
                    yieldedArrayObjects = true;
                    break;
                  }
                  case JObject childObj:
                    // Yield top-level child objects (e.g., collection, collection2)
                    yield return childObj;
                    yieldedArrayObjects = true;
                    break;
                }
              }

              // If nothing yielded (scalar-only root object), yield root itself
              if (!yieldedArrayObjects)
                yield return obj;
            }
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
    StreamRows(this TextReader reader, Action<IReadOnlyCollection<(string text, object? value)>> handleOneRow, char valueSeparator = ',', int sampleSize = 5, CancellationToken cancellationToken = default)
  {
    var (items, metadata) = reader.StreamJsonObjects();
    using var enumerator = items.GetEnumerator();
    
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
        var columnData = new (string text, object? value)[columns.Count];
        row.HandleRow(columns, valueSeparator, (idx, txt, val) => columnData[idx]=(txt, val));
        // Pass copy to avoid overwrites
        handleOneRow(columnData);
      }

      // Continue streaming the rest
      while (enumerator.MoveNext())
      {
        cancellationToken.ThrowIfCancellationRequested();
        var columnData = new (string text, object? value)[columns.Count];
        enumerator.Current?.HandleRow(columns, valueSeparator, (idx, txt, val) => columnData[idx] = (txt, val));
        // Pass copy to avoid overwrites
        handleOneRow(columnData);
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
        FirstSeenIndex = firstSeen[p.Name],
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
              foreach (var kvp in NamePriority.Where(kvp => p.Property.Name.EndsWith("_" + kvp.Key, StringComparison.OrdinalIgnoreCase)))
                return kvp.Value + 50;

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
