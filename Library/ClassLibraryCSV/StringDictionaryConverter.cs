using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace CsvTools;
/// <summary>
/// Custom JsonConverter for Dictionary that supports:
/// - Object-style JSON: {"key":"value"}
/// - Array-style JSON: [{"key":"k","value":"v"}]
/// - Empty object {} or empty array []
/// - Silently ignores errors and returns an empty Dictionary on failure
/// </summary>
public class StringDictionaryConverter<TValue> : JsonConverter<IDictionary<string, TValue>>
{
  /// <inheritdoc/>
  public override void WriteJson(JsonWriter writer, IDictionary<string, TValue>? value, JsonSerializer serializer)
  {
    // Serialize normally as an object
    serializer.Serialize(writer, value);
  }

  /// <inheritdoc/>
  public override IDictionary<string, TValue>? ReadJson(JsonReader reader, Type objectType, IDictionary<string, TValue>? existingValue, bool hasExistingValue, JsonSerializer serializer)
  {
    try
    {
      var token = JToken.Load(reader);
      var dict = new DictionaryIgnoreCase<TValue>();

      switch (token.Type)
      {
        case JTokenType.Object:
          // Object-style JSON: {"key":"value"}
          foreach (var prop in (JObject) token)
          {
            dict[prop.Key] = prop.Value != null
                ? prop.Value.ToObject<TValue>(serializer)!
                : default!;
          }
          break;

        case JTokenType.Array:
          // Array-style JSON: [{"key":"k","value":"v"}]
          foreach (var item in (JArray) token)
          {
            var key = item["key"]?.ToString();
            if (!string.IsNullOrEmpty(key))
            {
              var valueToken = item["value"];
              dict[key!] = valueToken != null
                  ? valueToken.ToObject<TValue>(serializer)!
                  : default!;
            }
          }
          break;

        case JTokenType.Null:
          // Null token → return empty
          break;

        default:
          // Anything else → ignore
          break;
      }

      return dict;
    }
    catch
    {
      // On any deserialization error, return empty Dictionary
      return new DictionaryIgnoreCase<TValue>();
    }
  }

}
