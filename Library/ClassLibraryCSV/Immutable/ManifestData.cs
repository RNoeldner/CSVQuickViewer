/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
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
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools
{
  public sealed class ManifestData
  {
    internal const string cCsvManifestExtension = ".manifest.json";

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

    public bool Delta { get; }

    public string Desc { get; }

    public ManifestField[] Fields { get; }

    public bool HasUserDefinedFields { get; }

    public string Heading { get; }

    public string Hydration { get; }

    public string PubName { get; }

    public static async Task<InspectionResult> ReadManifestFileSystem(string fileName)
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
        return await ReadManifestFromStream(FileSystemUtils.OpenRead(manifest), dataFile, string.Empty)
          .ConfigureAwait(false);

      dataFile = manifest.ReplaceCaseInsensitive(cCsvManifestExtension, ".txt");
      if (FileSystemUtils.FileExists(dataFile))
        return await ReadManifestFromStream(FileSystemUtils.OpenRead(manifest), dataFile, string.Empty)
          .ConfigureAwait(false);
      throw new FileNotFoundException(dataFile);
    }

    public static async Task<InspectionResult?> ReadManifestZip(string fileName)
    {
      using var archive = new ZipFile(fileName.LongPathPrefix());

      // Find Manifest      
      var mainfestEntry = archive.GetFilesInZip().FirstOrDefault(x => x.Name.EndsWith(cCsvManifestExtension, StringComparison.OrdinalIgnoreCase));
      if (mainfestEntry == null)
        return null;
      Logger.Information("Configuration read from manifest file {filename}", mainfestEntry.Name);
          
      return await ReadManifestFromStream(archive.GetInputStream(mainfestEntry), fileName,
        mainfestEntry.Name.Substring(0, mainfestEntry.Name.Length - cCsvManifestExtension.Length) + ".csv").ConfigureAwait(false);
    }

    private static async Task<InspectionResult> ReadManifestFromStream(
      Stream manifestStream,
      string fileName,
      string identifierInContainer)
    {
      var strContend = await new StreamReader(manifestStream, Encoding.UTF8, true, 4096, false).ReadToEndAsync()
        .ConfigureAwait(false);
      var mani = JsonConvert.DeserializeObject<ManifestData>(strContend);
      if (mani is null)
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

      foreach (var fld in mani.Fields)
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

    public class ManifestField
    {
      [JsonConstructor]
      public ManifestField(string? pubName, string? heading, string? desc, string? type, int ordinal)
      {
        Desc = desc ?? string.Empty;
        Heading = heading ?? string.Empty;
        Ordinal = ordinal;
        PubName = pubName ?? string.Empty;
        Type = type ?? string.Empty;
      }

      public string Desc { get; }

      public string Heading { get; }

      public int Ordinal { get; }

      public string PubName { get; }

      public string Type { get; }
    }
  }
}