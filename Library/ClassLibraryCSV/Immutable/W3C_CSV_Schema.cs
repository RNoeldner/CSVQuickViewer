#nullable disable
namespace CsvTools
{
  using System;
  using System.Collections.Generic;

  using System.Globalization;
  using Newtonsoft.Json;
  using Newtonsoft.Json.Converters;

  public partial class W3C_CSV
  {
    [JsonProperty("@context", NullValueHandling = NullValueHandling.Ignore)]
    public virtual List<W3C_Context> Context { get; set; }

    [JsonProperty("tables", NullValueHandling = NullValueHandling.Ignore)]
    public virtual List<W3C_Table> Tables { get; set; }

    [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
    public virtual Uri Url { get; set; }

    [JsonProperty("tableSchema", NullValueHandling = NullValueHandling.Ignore)]
    public virtual W3C_TableSchemaDetails TableSchema { get; set; }
  }

  public partial class W3C_ContextLang
  {
    [JsonProperty("@language", NullValueHandling = NullValueHandling.Ignore)]
    public virtual string Language { get; set; }
  }

  public partial class W3C_TableSchemaDetails
  {
    [JsonProperty("columns", NullValueHandling = NullValueHandling.Ignore)]
    public virtual List<W3C_Column> Columns { get; set; }

    [JsonProperty("primaryKey", NullValueHandling = NullValueHandling.Ignore)]
    public virtual string PrimaryKey { get; set; }

    [JsonProperty("propertyUrl", NullValueHandling = NullValueHandling.Ignore)]
    public virtual Uri PropertyUrl { get; set; }

    [JsonProperty("aboutUrl", NullValueHandling = NullValueHandling.Ignore)]
    public virtual Uri AboutUrl { get; set; }

    [JsonProperty("foreignKeys", NullValueHandling = NullValueHandling.Ignore)]
    public virtual List<ForeignKey> ForeignKeys { get; set; }
  }

  public partial class W3C_Column
  {
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public virtual string Name { get; set; }

    [JsonProperty("titles", NullValueHandling = NullValueHandling.Ignore)]
    public virtual W3C_Titles? Titles { get; set; }

    [JsonProperty("dc:description", NullValueHandling = NullValueHandling.Ignore)]
    public virtual string Description { get; set; }

    [JsonProperty("datatype", NullValueHandling = NullValueHandling.Ignore)]
    public virtual W3C_Datatype? Datatype { get; set; }

    [JsonProperty("required", NullValueHandling = NullValueHandling.Ignore)]
    public virtual bool? ColumnRequired { get; set; }

    [JsonProperty("suppressOutput", NullValueHandling = NullValueHandling.Ignore)]
    public virtual bool? SuppressOutput { get; set; }

    [JsonProperty("virtual", NullValueHandling = NullValueHandling.Ignore)]
    public virtual bool? Virtual { get; set; }

    [JsonProperty("aboutUrl", NullValueHandling = NullValueHandling.Ignore)]
    public virtual Uri AboutUrl { get; set; }

    [JsonProperty("propertyUrl", NullValueHandling = NullValueHandling.Ignore)]
    public virtual Uri PropertyUrl { get; set; }

    [JsonProperty("valueUrl", NullValueHandling = NullValueHandling.Ignore)]
    public virtual Uri ValueUrl { get; set; }
  }

  public enum W3C_DatatypeBase { String, Date, Integer, DateTime, Time, DateTimeStamp, Decimal, Double, Boolean, Long, Int, Short, Byte, NonNegativeInteger, PositiveInteger, Duration, DayTimeDuration, YearMonthDuration, Float, GDay, GMonth, GMonthDay, GYear, GYearMonth, HexBinary, Xml, Json }
  public partial class W3C_DatatypeDetails
  {
    [JsonProperty("base", NullValueHandling = NullValueHandling.Ignore)]
    public virtual W3C_DatatypeBase Base { get; set; }

    [JsonProperty("format", NullValueHandling = NullValueHandling.Ignore)]
    public virtual W3C_Format? Format { get; set; }

    [JsonProperty("length", NullValueHandling = NullValueHandling.Ignore)]
    public virtual int? Length { get; set; }

    [JsonProperty("minLength", NullValueHandling = NullValueHandling.Ignore)]
    public virtual int? MinLength { get; set; }

    [JsonProperty("maxLength", NullValueHandling = NullValueHandling.Ignore)]
    public virtual int? MaxLength { get; set; }
  }

  public partial class W3C_NumericFormat
  {
    [JsonProperty("decimalChar", NullValueHandling = NullValueHandling.Ignore)]
    public virtual string DecimalChar { get; set; }

    [JsonProperty("groupChar", NullValueHandling = NullValueHandling.Ignore)]
    public virtual string GroupChar { get; set; }

    [JsonProperty("pattern", NullValueHandling = NullValueHandling.Ignore)]
    public virtual string Pattern { get; set; }
  }

  public partial class ForeignKey
  {
    [JsonProperty("columnReference", NullValueHandling = NullValueHandling.Ignore)]
    public virtual string ColumnReference { get; set; }

    [JsonProperty("reference", NullValueHandling = NullValueHandling.Ignore)]
    public virtual Reference Reference { get; set; }
  }

  public partial class Reference
  {
    [JsonProperty("resource", NullValueHandling = NullValueHandling.Ignore)]
    public virtual string Resource { get; set; }

    [JsonProperty("columnReference", NullValueHandling = NullValueHandling.Ignore)]
    public virtual string ColumnReference { get; set; }
  }

  public partial class W3C_Table
  {
    [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
    public virtual string Url { get; set; }

    [JsonProperty("tableSchema", NullValueHandling = NullValueHandling.Ignore)]
    public virtual W3C_TableSchema? TableSchema { get; set; }

    [JsonProperty("suppressOutput", NullValueHandling = NullValueHandling.Ignore)]
    public virtual bool? SuppressOutput { get; set; }
  }

  public partial struct W3C_Context
  {
    public W3C_ContextLang ContextLang;
    public Uri ContextUri;

    public static implicit operator W3C_Context(W3C_ContextLang ContextClass) => new W3C_Context { ContextLang = ContextClass };
    public static implicit operator W3C_Context(Uri PurpleUri) => new W3C_Context { ContextUri = PurpleUri };
  }

  public partial struct W3C_Format
  {
    public W3C_NumericFormat FormatClass;
    public string String;

    public static implicit operator W3C_Format(W3C_NumericFormat FormatClass) => new W3C_Format { FormatClass = FormatClass };
    public static implicit operator W3C_Format(string String) => new W3C_Format { String = String };
  }

  public partial struct W3C_Datatype
  {
    public W3C_DatatypeDetails Details;
    public W3C_DatatypeBase Base;

    public static implicit operator W3C_Datatype(W3C_DatatypeDetails DatatypeClass) => new W3C_Datatype { Details = DatatypeClass };
    public static implicit operator W3C_Datatype(W3C_DatatypeBase baseType) => new W3C_Datatype { Base = baseType };
  }

  public partial struct W3C_Titles
  {
    public string Simple;
    public List<string> StringArray;

    public static implicit operator W3C_Titles(string String) => new W3C_Titles { Simple = String };
    public static implicit operator W3C_Titles(List<string> StringArray) => new W3C_Titles { StringArray = StringArray };
  }

  public partial struct W3C_TableSchema
  {
    public string Simple;
    public W3C_TableSchemaDetails TableSchemaClass;

    public static implicit operator W3C_TableSchema(string String) => new W3C_TableSchema { Simple = String };
    public static implicit operator W3C_TableSchema(W3C_TableSchemaDetails TableSchemaClass) => new W3C_TableSchema { TableSchemaClass = TableSchemaClass };
  }

  internal static class Converter
  {
    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
      MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
      DateParseHandling = DateParseHandling.None,
      Converters =
            {
                W3C_ContextConverter.Singleton,
                W3C_DatatypeConverter.Singleton,
                W3C_FormatConverter.Singleton,
                W3C_TitlesConverter.Singleton,
                W3C_TableSchemaConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
    };
  }

  internal class W3C_ContextConverter : JsonConverter
  {
    public override bool CanConvert(Type t) => t == typeof(W3C_Context) || t == typeof(W3C_Context?);

    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    {
      switch (reader.TokenType)
      {
        case JsonToken.String:
        case JsonToken.Date:
          var stringValue = serializer.Deserialize<string>(reader);
          try
          {
            var uri = new Uri(stringValue);
            return new W3C_Context { ContextUri = uri };
          }
          catch (UriFormatException) { }
          break;
        case JsonToken.StartObject:
          var objectValue = serializer.Deserialize<W3C_ContextLang>(reader);
          return new W3C_Context { ContextLang = objectValue };
      }
      throw new Exception("Cannot unmarshal type W3C_Context");
    }

    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    {
      var value = (W3C_Context) untypedValue;
      if (value.ContextUri != null)
      {
        serializer.Serialize(writer, value.ContextUri.ToString());
        return;
      }
      if (value.ContextLang != null)
      {
        serializer.Serialize(writer, value.ContextLang);
        return;
      }
      throw new Exception("Cannot marshal type W3C_Context");
    }

    public static readonly W3C_ContextConverter Singleton = new W3C_ContextConverter();
  }

  internal class W3C_DatatypeConverter : JsonConverter
  {
    public override bool CanConvert(Type t) => t == typeof(W3C_Datatype) || t == typeof(W3C_Datatype?);

    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    {
      switch (reader.TokenType)
      {
        case JsonToken.String:
        case JsonToken.Date:
          return new W3C_Datatype { Base = serializer.Deserialize<W3C_DatatypeBase>(reader) };
        case JsonToken.StartObject:
          var objectValue = serializer.Deserialize<W3C_DatatypeDetails>(reader);
          return new W3C_Datatype { Details = objectValue };
      }
      throw new Exception("Cannot unmarshal type W3C_Datatype");
    }

    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    {
      var value = (W3C_Datatype) untypedValue;
      if (value.Details != null)
      {
        serializer.Serialize(writer, value.Details);
        return;
      }
      if (value.Base != W3C_DatatypeBase.String)
      {
        serializer.Serialize(writer, value.Base);
        return;
      }
      throw new Exception("Cannot marshal type W3C_Datatype");
    }

    public static readonly W3C_DatatypeConverter Singleton = new W3C_DatatypeConverter();
  }

  internal class W3C_FormatConverter : JsonConverter
  {
    public override bool CanConvert(Type t) => t == typeof(W3C_Format) || t == typeof(W3C_Format?);

    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    {
      switch (reader.TokenType)
      {
        case JsonToken.String:
        case JsonToken.Date:
          var stringValue = serializer.Deserialize<string>(reader);
          return new W3C_Format { String = stringValue };
        case JsonToken.StartObject:
          var objectValue = serializer.Deserialize<W3C_NumericFormat>(reader);
          return new W3C_Format { FormatClass = objectValue };
      }
      throw new Exception("Cannot unmarshal type W3C_Format");
    }

    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    {
      var value = (W3C_Format) untypedValue;
      if (value.String != null)
      {
        serializer.Serialize(writer, value.String);
        return;
      }
      if (value.FormatClass != null)
      {
        serializer.Serialize(writer, value.FormatClass);
        return;
      }
      throw new Exception("Cannot marshal type W3C_Format");
    }

    public static readonly W3C_FormatConverter Singleton = new W3C_FormatConverter();
  }

  internal class W3C_TitlesConverter : JsonConverter
  {
    public override bool CanConvert(Type t) => t == typeof(W3C_Titles) || t == typeof(W3C_Titles?);

    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    {
      switch (reader.TokenType)
      {
        case JsonToken.String:
        case JsonToken.Date:
          var stringValue = serializer.Deserialize<string>(reader);
          return new W3C_Titles { Simple = stringValue };
        case JsonToken.StartArray:
          var arrayValue = serializer.Deserialize<List<string>>(reader);
          return new W3C_Titles { StringArray = arrayValue };
      }
      throw new Exception("Cannot unmarshal type W3C_Titles");
    }

    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    {
      var value = (W3C_Titles) untypedValue;
      if (value.Simple != null)
      {
        serializer.Serialize(writer, value.Simple);
        return;
      }
      if (value.StringArray != null)
      {
        serializer.Serialize(writer, value.StringArray);
        return;
      }
      throw new Exception("Cannot marshal type W3C_Titles");
    }

    public static readonly W3C_TitlesConverter Singleton = new W3C_TitlesConverter();
  }

  internal class W3C_TableSchemaConverter : JsonConverter
  {
    public override bool CanConvert(Type t) => t == typeof(W3C_TableSchema) || t == typeof(W3C_TableSchema?);

    public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
    {
      switch (reader.TokenType)
      {
        case JsonToken.String:
        case JsonToken.Date:
          var stringValue = serializer.Deserialize<string>(reader);
          return new W3C_TableSchema { Simple = stringValue };
        case JsonToken.StartObject:
          var objectValue = serializer.Deserialize<W3C_TableSchemaDetails>(reader);
          return new W3C_TableSchema { TableSchemaClass = objectValue };
      }
      throw new Exception("Cannot unmarshal type W3C_TableSchema");
    }

    public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
    {
      var value = (W3C_TableSchema) untypedValue;
      if (value.Simple != null)
      {
        serializer.Serialize(writer, value.Simple);
        return;
      }
      if (value.TableSchemaClass != null)
      {
        serializer.Serialize(writer, value.TableSchemaClass);
        return;
      }
      throw new Exception("Cannot marshal type W3C_TableSchema");
    }

    public static readonly W3C_TableSchemaConverter Singleton = new W3C_TableSchemaConverter();
  }
}
