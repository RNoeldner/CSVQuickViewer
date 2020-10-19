using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools
{
  public class ManifestData
  {
    public class ManifestField
    {
      public string pubname;
      public string heading;
      public string desc;
      public string type;
      public int ordinal;
    }
    public string pubname;
    public string heading;
    public string desc;
    public bool delta;
    public string hydration;
    public bool hasuserdefinedfields;
    public ManifestField[] fields;

    public static ICsvFile ReadManifest(string manifest)
    {
      var mani = JsonConvert.DeserializeObject<ManifestData>(new StreamReader(FileSystemUtils.OpenRead(manifest), Encoding.UTF8, true, 4096, false).ReadToEnd());
      var fileName = manifest.Replace(CsvFile.cCsvManifestExtension, ".csv");
      var fileSettingMani = new CsvFile
      {
        FileName = fileName,
        ID = fileName,
        HasFieldHeader = false,
        SkipRows =0
      };
      fileSettingMani.FileFormat.FieldDelimiter =",";
      fileSettingMani.FileFormat.QualifyAlways = true;
      fileSettingMani.FileFormat.FieldQualifier ="\"";
      fileSettingMani.FileFormat.NewLine = RecordDelimiterType.LF;
      foreach (var fld in mani.fields)
      {
        IValueFormat vf;
        switch (fld.type.ToLower().TrimEnd('?', ')', ',', '(', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'))
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
            vf = new ImmutableValueFormat(DataType.String);
            break;
        }
        fileSettingMani.ColumnCollection.Add(new Column(fld.pubname, vf) { ColumnOrdinal = fld.ordinal, DestinationName= fld.pubname });
      }
      return fileSettingMani;
    }

  }
}

