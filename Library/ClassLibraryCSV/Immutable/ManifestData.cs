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
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools;

/// <summary>
/// Represents the root structure of a JSON manifest file used to describe CSV or text data entities.
/// </summary>
public sealed record ManifestData
{
  internal const string cCsvManifestExtension = ".manifest.json";

  /// <summary>
  /// Initializes a new instance of the <see cref="ManifestData"/> record.
  /// </summary>
  /// <param name="pubName">The public-facing name of the entity.</param>
  /// <param name="heading">A descriptive display heading or long name.</param>
  /// <param name="desc">A detailed description of the entity's purpose or content.</param>
  /// <param name="delta">Indicates whether the entity supports delta (incremental) updates.</param>
  /// <param name="hydration">A string defining the hydration strategy or state for the entity.</param>
  /// <param name="hasUserDefinedFields">Indicates if the entity allows or contains custom, user-defined fields.</param>
  /// <param name="fields">An array of field definitions describing the data structure.</param>
  [JsonConstructor]
  public ManifestData(
    string? pubName,
    string? heading,
    string? desc,
    bool delta,
    string? hydration,
    bool? hasUserDefinedFields,
    ManifestField[]? fields)
  {
    PubName = pubName ?? string.Empty;
    Desc = desc ?? string.Empty;
    Heading = heading ?? string.Empty;
    Hydration = hydration ?? string.Empty;
    Fields = fields ?? Array.Empty<ManifestField>();
    HasUserDefinedFields = hasUserDefinedFields ?? false;
    Delta = delta;
  }
  
  /// <summary>
  /// Gets a value indicating whether the entity supports delta (incremental) processing.
  /// </summary>
  public bool Delta { get; }

  /// <summary>
  /// Gets the detailed description of the entity.
  /// </summary>
  public string Desc { get; }

  /// <summary>
  /// Gets the collection of fields associated with this entity.
  /// </summary>
  public ManifestField[] Fields { get; }

  /// <summary>
  /// Gets a value indicating whether user-defined (custom) fields are present.
  /// </summary>
  public bool HasUserDefinedFields { get; }

  /// <summary>
  /// Gets the display heading or formal name of the entity.
  /// </summary>
  public string Heading { get; }

  /// <summary>
  /// Gets the hydration context or metadata string.
  /// </summary>
  public string Hydration { get; }

  /// <summary>
  /// Gets the short public name used to identify the entity.
  /// </summary>
  public string PubName { get; }

  /// <summary>
  /// Reads and parses manifest data from the local file system.
  /// </summary>
  /// <param name="fileName">The path to the manifest file or the associated data file.</param>
  /// <returns>An <see cref="InspectionResult"/> containing the parsed columns and configuration.</returns>
  /// <exception cref="FileNotFoundException">Thrown when the manifest or the corresponding data file (.csv/.txt) cannot be located.</exception>
  /// <exception cref="InvalidOperationException">Thrown if the JSON content is malformed or cannot be deserialized.</exception>
  public static Task<InspectionResult> ReadManifestFileSystem(string fileName)
  {
    var posExt = fileName.LastIndexOf('.');
    var manifest = fileName.EndsWith(cCsvManifestExtension, StringComparison.OrdinalIgnoreCase)
      ? fileName
      : fileName.Substring(0, posExt) + cCsvManifestExtension;
    if (!FileSystemUtils.FileExists(manifest))
      throw new FileNotFoundException(manifest);

    var dataFile = manifest.ReplaceCaseInsensitive(cCsvManifestExtension, ".csv");
    Logger.Information($"Configuration read from manifest file {manifest}");

    if (FileSystemUtils.FileExists(dataFile))
      return ReadManifestFromStream(FileSystemUtils.OpenRead(manifest), dataFile, string.Empty);

    dataFile = manifest.ReplaceCaseInsensitive(cCsvManifestExtension, ".txt");
    return FileSystemUtils.FileExists(dataFile) ? ReadManifestFromStream(FileSystemUtils.OpenRead(manifest), dataFile, string.Empty) : throw new FileNotFoundException(dataFile);
  }

  /// <summary>
  /// Reads and parses manifest data from a compressed ZIP archive.
  /// </summary>
  /// <param name="fileName">The path to the .zip archive containing the manifest and data.</param>
  /// <returns>
  /// An <see cref="InspectionResult"/> if a manifest is found; otherwise, <see langword="null"/>.
  /// </returns>
  public static async Task<InspectionResult?> ReadManifestZip(string fileName)
  {
    using var archive = new ICSharpCode.SharpZipLib.Zip.ZipFile(fileName.LongPathPrefix());

    // Find Manifest 
    var manifestEntry = archive.Cast<ICSharpCode.SharpZipLib.Zip.ZipEntry>().FirstOrDefault(e => e.IsFile &&
      e.Name.EndsWith(cCsvManifestExtension, StringComparison.OrdinalIgnoreCase));
    if (manifestEntry is null)
      return null;
    Logger.Information($"Configuration read from manifest file {manifestEntry.Name}");


    return await ReadManifestFromStream(archive.GetInputStream(manifestEntry), fileName,
      manifestEntry.Name.Substring(0, manifestEntry.Name.Length - cCsvManifestExtension.Length) + ".csv").ConfigureAwait(false);
  }

  private static async Task<InspectionResult> ReadManifestFromStream(
    Stream manifestStream,
    string fileName,
    string identifierInContainer)
  {
    var strContend = await new StreamReader(manifestStream, Encoding.UTF8, true, 4096, false).ReadToEndAsync()
      .ConfigureAwait(false);
    var jsonManifest = JsonConvert.DeserializeObject<ManifestData>(strContend);
    if (jsonManifest is null)
      throw new InvalidOperationException("The manifest file could not be deserialized");
    var detectionResult = new InspectionResult
    {
      FileName =  fileName,
      SkipRows = 0,
      SkipRowsAfterHeader = 0,
      CodePageId= Encoding.UTF8.CodePage,
      ByteOrderMark= false,
      IdentifierInContainer = identifierInContainer,
      CommentLine = "#",
      FieldDelimiter= ',',
      NewLine=  RecordDelimiterTypeEnum.Lf,
      ContextSensitiveQualifier = false,
      HasFieldHeader = false
    };

    foreach (var fld in jsonManifest.Fields)
    {
      ValueFormat vf =
        fld.Type.ToLower(System.Globalization.CultureInfo.InvariantCulture).TrimEnd('?', ')', ',', '(', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9') switch
        {
          "int" or "long" or "byte" or "short" => new ValueFormat(DataTypeEnum.Integer),
          "decimal" or "single" or "double" => new ValueFormat(DataTypeEnum.Numeric, decimalSeparator: "."),
          "uuid" or "guid" => new ValueFormat(DataTypeEnum.Guid),
          "bit" => new ValueFormat(DataTypeEnum.Boolean),
          "date" or "localdate" => new ValueFormat(DataTypeEnum.DateTime, "yyyy/MM/dd", "-"),
          "datetime" or "localdatetime" => new ValueFormat(DataTypeEnum.DateTime, "yyyy/MM/ddTHH:mm:ss.FFFFFFF", "-"),
          "time" or "localtime" => new ValueFormat(DataTypeEnum.DateTime, "HH:mm:ss.FFFFFFF"),
          _ => ValueFormat.Empty
        };

      detectionResult.Columns.Add(new Column(fld.PubName, vf, fld.Ordinal, destinationName: fld.PubName));
    }

    return detectionResult;
  }

  /// <summary>
  /// Represents the metadata for an individual field within a <see cref="ManifestData"/> definition.
  /// </summary>
  public record ManifestField
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ManifestField"/> record.
    /// </summary>
    /// <param name="pubName">The public-facing name of the field.</param>
    /// <param name="heading">The formal display heading or long name.</param>
    /// <param name="desc">A description of the field's data.</param>
    /// <param name="type">The raw string representation of the data type (e.g., "int", "datetime").</param>
    /// <param name="ordinal">The zero-based index or position of the field in the data source.</param>
    [JsonConstructor]
    public ManifestField(string? pubName, string? heading, string? desc, string? type, int ordinal)
    {
      Desc = desc ?? string.Empty;
      Heading = heading ?? string.Empty;
      Ordinal = ordinal;
      PubName = pubName ?? string.Empty;
      Type = type ?? string.Empty;
    }

    /// <summary>
    /// Gets the description of the field.
    /// </summary>
    public string Desc { get; }

    /// <summary>
    /// Gets the zero-based position of the field within the record.
    /// </summary>
    public string Heading { get; }

    /// <summary>
    /// Ordinal number of the field
    /// </summary>
    public int Ordinal { get; }

    /// <summary>
    /// Gets the public name used to identify the field.
    /// </summary>
    public string PubName { get; }

    /// <summary>
    /// Gets the raw data type string defined in the manifest.
    /// </summary>
    public string Type { get; }
  }
}