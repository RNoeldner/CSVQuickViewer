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

namespace CsvTools
{
  /// <summary>
  /// Information describing an entity in Json data
  /// </summary>
  public sealed record ManifestData
  {
    internal const string cCsvManifestExtension = ".manifest.json";

    /// <summary>
    /// Manifest for Json files
    /// </summary>
    /// <param name="pubName">Public Name</param>
    /// <param name="heading">Long Name</param>
    /// <param name="desc">Description</param>
    /// <param name="delta">Does support delta</param>
    /// <param name="hydration">Hydration</param>
    /// <param name="hasUserDefinedFields"></param>
    /// <param name="fields">Fields</param>
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
    /// <c>true</c> if the entity does support delta
    /// </summary>
    public bool Delta { get; }

    /// <summary>
    /// Description for entity
    /// </summary>
    public string Desc { get; }

    /// <summary>
    /// Fields
    /// </summary>
    public ManifestField[] Fields { get; }

    /// <summary>
    /// Has CustomFields
    /// </summary>
    public bool HasUserDefinedFields { get; }

    /// <summary>
    /// Heading
    /// </summary>
    public string Heading { get; }

    /// <summary>
    /// Hydration
    /// </summary>
    public string Hydration { get; }

    /// <summary>
    /// Public Name
    /// </summary>
    public string PubName { get; }

    /// <summary>
    /// Reads the manifest data from a file
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public static Task<InspectionResult> ReadManifestFileSystem(string fileName)
    {
      var posExt = fileName.LastIndexOf('.');
      var manifest = fileName.EndsWith(cCsvManifestExtension, StringComparison.OrdinalIgnoreCase)
        ? fileName
        : fileName.Substring(0, posExt) + cCsvManifestExtension;
      if (!FileSystemUtils.FileExists(manifest))
        throw new FileNotFoundException(manifest);

      var dataFile = manifest.ReplaceCaseInsensitive(cCsvManifestExtension, ".csv");
      Logger.Information("Configuration read from manifest file {filename}", manifest);

      if (FileSystemUtils.FileExists(dataFile))
        return ReadManifestFromStream(FileSystemUtils.OpenRead(manifest), dataFile, string.Empty);

      dataFile = manifest.ReplaceCaseInsensitive(cCsvManifestExtension, ".txt");
      if (FileSystemUtils.FileExists(dataFile))
        return ReadManifestFromStream(FileSystemUtils.OpenRead(manifest), dataFile, string.Empty);
      throw new FileNotFoundException(dataFile);
    }

    /// <summary>
    /// Reads the manifest data from a zip file, looking for the first file that matches teh extension
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static async Task<InspectionResult?> ReadManifestZip(string fileName)
    {
      using var archive = new ICSharpCode.SharpZipLib.Zip.ZipFile(fileName.LongPathPrefix());

      // Find Manifest 
      var manifestEntry = archive.Cast<ICSharpCode.SharpZipLib.Zip.ZipEntry>().FirstOrDefault(e => e.IsFile &&
                                                e.Name.EndsWith(cCsvManifestExtension, StringComparison.OrdinalIgnoreCase));
      if (manifestEntry is null)
        return null;
      try { Logger.Information("Configuration read from manifest file {filename}", manifestEntry.Name); } catch { }


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
        ValueFormat vf;
        switch (fld.Type.ToLower().TrimEnd('?', ')', ',', '(', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'))
        {
          case "int":
          case "long":
          case "byte":
          case "short":
            vf = new ValueFormat(DataTypeEnum.Integer);
            break;

          case "decimal":
          case "single":
          case "double":
            vf = new ValueFormat(DataTypeEnum.Numeric, decimalSeparator: ".");
            break;

          case "uuid":
          case "guid":
            vf = new ValueFormat(DataTypeEnum.Guid);
            break;

          case "bit":
            vf = new ValueFormat(DataTypeEnum.Boolean);
            break;

          case "date":
          case "localdate":
            vf = new ValueFormat(DataTypeEnum.DateTime, "yyyy/MM/dd", "-");
            break;

          case "datetime":
          case "localdatetime":
            vf = new ValueFormat(DataTypeEnum.DateTime, "yyyy/MM/ddTHH:mm:ss.FFFFFFF", "-");
            break;

          case "time":
          case "localtime":
            vf = new ValueFormat(DataTypeEnum.DateTime, "HH:mm:ss.FFFFFFF");
            break;

          default:
            vf = ValueFormat.Empty;
            break;
        }

        detectionResult.Columns.Add(new Column(fld.PubName, vf, fld.Ordinal, destinationName: fld.PubName));
      }

      return detectionResult;
    }

    /// <summary>
    /// Field in a Manifest Json
    /// </summary>
    public record ManifestField
    {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="pubName">Public Name</param>
      /// <param name="heading">Long Name</param>
      /// <param name="desc">Description</param>
      /// <param name="type">Data Type</param>
      /// <param name="ordinal">Ordinal number</param>
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
      /// Description for field
      /// </summary>
      public string Desc { get; }

      /// <summary>
      /// Long Name 
      /// </summary>
      public string Heading { get; }

      /// <summary>
      /// Ordinal number of the field
      /// </summary>
      public int Ordinal { get; }

      /// <summary>
      /// Public Name
      /// </summary>
      public string PubName { get; }

      /// <summary>
      /// Data Type
      /// </summary>
      public string Type { get; }
    }
  }
}
