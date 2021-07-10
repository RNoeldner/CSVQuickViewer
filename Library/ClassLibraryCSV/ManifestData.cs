using Newtonsoft.Json;
using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CsvTools
{
  public class ManifestData
  {
    internal const string cCsvManifestExtension = ".manifest.json";

    [JsonConstructor]
    public ManifestData(string? pubName, string? heading, string? desc, bool delta, string? hydration,
                        bool hasUserDefinedFields, ManifestField[]? fields)
    {
      PubName= pubName?? string.Empty;
      Desc = desc ?? string.Empty;
      Heading = heading ?? string.Empty;
      Hydration = hydration ?? string.Empty;
      Fields = fields ?? Array.Empty<ManifestField>();
      HasUserDefinedFields= hasUserDefinedFields;
      Delta = delta;
    }

    public bool Delta { get; }
    public string Desc { get; }
    public ManifestField[] Fields { get; }
    public bool HasUserDefinedFields { get; }
    public string Heading { get; }
    public string Hydration { get; }
    public string PubName { get; }

    public static async Task<DelimitedFileDetectionResultWithColumns> ReadManifestFileSystem(string fileName)
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
        return await ReadManifestFromStream(FileSystemUtils.OpenRead(manifest), dataFile, string.Empty);

      dataFile = manifest.ReplaceCaseInsensitive(cCsvManifestExtension, ".txt");
      if (FileSystemUtils.FileExists(dataFile))
        return await ReadManifestFromStream(FileSystemUtils.OpenRead(manifest), dataFile, string.Empty);
      throw new FileNotFoundException(dataFile);
    }

    public static async Task<DelimitedFileDetectionResultWithColumns> ReadManifestZip(string fileName)
    {
      Logger.Debug("Opening Zip file {filename}", fileName);

      using var archive = new ZipFile(fileName.LongPathPrefix());
      // find Text and Manifest
      foreach (ZipEntry entryManifest in archive)
      {
        if (!entryManifest.IsFile || !entryManifest.Name.EndsWith(cCsvManifestExtension, StringComparison.OrdinalIgnoreCase))
          continue;
        foreach (ZipEntry entryFile in archive)
        {
          if (entryManifest == entryFile || !entryFile.IsFile || !entryFile.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            continue;
          Logger.Information("Configuration read from manifest file {filename}", entryManifest.Name);
          return await ReadManifestFromStream(archive.GetInputStream(entryManifest), fileName, entryFile.Name);
        }
      }

      throw new FileNotFoundException("Could not locate manifest and matching file");
    }

    private static async Task<DelimitedFileDetectionResultWithColumns> ReadManifestFromStream(Stream manifestStream, string fileName, string identifierInContainer)
    {
      var strContend = await new StreamReader(manifestStream, Encoding.UTF8, true, 4096, false).ReadToEndAsync();
      var mani = JsonConvert.DeserializeObject<ManifestData>(strContend);
      if (mani == null)
        throw new InvalidOperationException("The manifest file could not be deserialized");
      var fileSettingMani = new DelimitedFileDetectionResult(fileName, 0, Encoding.UTF8.CodePage, false, true, identifierInContainer, "#", "\\", ",", "\"", false, false, false, RecordDelimiterType.LF);

      var columnCollection = new List<IColumn>();
      foreach (var fld in mani.Fields)
      {
        IValueFormat vf;
        switch (fld.Type.ToLower().TrimEnd('?', ')', ',', '(', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'))
        {
          case "int":
          case "long":
          case "byte":
          case "short":
            vf = new ImmutableValueFormat(DataType.Integer);
            break;

          case "decimal":
          case "single":
          case "double":
            vf = new ImmutableValueFormat(DataType.Numeric, decimalSeparator: ".");
            break;

          case "uuid":
          case "guid":
            vf = new ImmutableValueFormat(DataType.Guid);
            break;

          case "bit":
            vf = new ImmutableValueFormat(DataType.Boolean);
            break;

          case "date":
          case "localdate":
            vf = new ImmutableValueFormat(DataType.DateTime, "yyyy/MM/dd", "-");
            break;

          case "datetime":
          case "localdatetime":
            vf = new ImmutableValueFormat(DataType.DateTime, "yyyy/MM/ddTHH:mm:ss.FFFFFFF", "-");
            break;

          case "time":
          case "localtime":
            vf = new ImmutableValueFormat(DataType.DateTime, "HH:mm:ss.FFFFFFF");
            break;

          default:
            vf = new ImmutableValueFormat();
            break;
        }

        columnCollection.Add(new ImmutableColumn(fld.PubName, vf, fld.Ordinal, destinationName: fld.PubName));
      }
      return new DelimitedFileDetectionResultWithColumns(fileSettingMani, columnCollection, string.Empty);
    }

    public class ManifestField
    {
      public string Desc { get; }
      public string Heading { get; }
      public int Ordinal { get; }
      public string PubName { get; }
      public string Type { get; }

      [JsonConstructor]
      public ManifestField(string? pubName, string? heading, string? desc, string? type, int ordinal)
      {
        Desc = desc ?? string.Empty;
        Heading = heading?? string.Empty;
        Ordinal = ordinal;
        PubName = pubName?? string.Empty;
        Type = type?? string.Empty;
      }
    }
  }
}