using Newtonsoft.Json;
using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Text;

namespace CsvTools
{
  public class ManifestData
  {
    private const string cCsvManifestExtension = ".manifest.json";
    public bool Delta { get; set; }
    public string Desc { get; set; }
    public ManifestField[] Fields { get; set; }
    public bool HasUserDefinedFields { get; set; }
    public string Heading { get; set; }
    public string Hydration { get; set; }
    public string PubName { get; set; }

    public static ICsvFile ReadManifestFileSystem(string fileName)
    {
      var posExt = fileName.LastIndexOf('.');
      if (posExt == -1) return null;
      var manifest = fileName.EndsWith(cCsvManifestExtension, StringComparison.OrdinalIgnoreCase)
        ? fileName
        : fileName.Substring(0, posExt) + cCsvManifestExtension;
      if (FileSystemUtils.FileExists(manifest))
      {
        var dataFile = manifest.ReplaceCaseInsensitive(cCsvManifestExtension, ".csv");
        if (FileSystemUtils.FileExists(dataFile))
          return ReadManifestFromStream(FileSystemUtils.OpenRead(manifest), manifest, dataFile, string.Empty);

        dataFile = manifest.ReplaceCaseInsensitive(cCsvManifestExtension, ".txt");
        if (FileSystemUtils.FileExists(dataFile))
          return ReadManifestFromStream(FileSystemUtils.OpenRead(manifest), manifest, dataFile, string.Empty);
      }

      return null;
    }

    public static ICsvFile ReadManifestZip(string fileName)
    {
      if (!fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        return null;

      Logger.Debug("Opening Zip file {filename}", fileName);

      using (var archive = new ZipFile(fileName.LongPathPrefix()))
      {
        // find Text and Manifest

        foreach (ZipEntry entryManifest in archive)
        {
          if (!entryManifest.IsFile || !entryManifest.Name.EndsWith(cCsvManifestExtension, StringComparison.OrdinalIgnoreCase))
            continue;
          foreach (ZipEntry entryFile in archive)
          {
            if (entryManifest == entryFile || !entryFile.IsFile || !entryFile.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
              continue;
            return ReadManifestFromStream(archive.GetInputStream(entryManifest), entryManifest.Name, fileName, entryFile.Name);
          }
        }
      }

      return null;
    }

    private static ICsvFile ReadManifestFromStream(Stream manifestStream, string manifestName, string fileName, string identifierInContainer)
    {
      Logger.Information("Configuration read from manifest file {filename}", manifestName);
      var mani = JsonConvert.DeserializeObject<ManifestData>(new StreamReader(manifestStream, Encoding.UTF8, true, 4096, false).ReadToEnd());
      var fileSettingMani = new CsvFile
      {
        FileName = fileName,
        ID = fileName,
        HasFieldHeader = false,
        IdentifierInContainer = identifierInContainer,
        SkipRows = 0,
        FileFormat =
        {
          FieldDelimiter = ",", QualifyAlways = true, FieldQualifier = "\"", NewLine = RecordDelimiterType.LF
        }
      };
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
            vf = new ImmutableValueFormat(DataType.Numeric, decimalSeparatorChar: '.');
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

        fileSettingMani.ColumnCollection.Add(new Column(fld.PubName, vf) { ColumnOrdinal = fld.Ordinal, DestinationName = fld.PubName });
      }

      return fileSettingMani;
    }

    public class ManifestField
    {
      public string Desc
      {
        get; set;
      }
      public string Heading
      {
        get; set;
      }
      public int Ordinal
      {
        get; set;
      }
      public string PubName
      {
        get; set;
      }
      public string Type
      {
        get; set;
      }

      public ManifestField()
      {
      }

      public ManifestField(string desc, string heading, int ordinal, string pubName, string type)
      {
        Desc = desc;
        Heading = heading;
        Ordinal = ordinal;
        PubName = pubName;
        Type = type;
      }
    }
  }
}