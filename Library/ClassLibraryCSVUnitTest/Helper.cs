using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CsvTools.Tests
{
  internal static class Helper
  {
    internal static void AllPropertiesEqual(this object a, object b)
    {
      var properties = a.GetType().GetProperties().Where(prop => prop.GetMethod != null &&
                                                                 (prop.PropertyType == typeof(int) ||
                                                                  prop.PropertyType == typeof(long) ||
                                                                  prop.PropertyType == typeof(string) ||
                                                                  prop.PropertyType == typeof(bool) ||
                                                                  prop.PropertyType == typeof(DateTime)));
      foreach (var prop in properties)
        Assert.AreEqual(prop.GetValue(a), prop.GetValue(b),
          $"Type: {a.GetType().FullName}  Property:{prop.Name}");
    }

    internal static CsvFile ReaderGetAllFormats(IToolSetting parent = null, string id = "AllFormats")
    {
      var readFile = new CsvFile
      {
        ID = id,
        FileName = Path.Combine(FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles", "AllFormats.txt"),
        HasFieldHeader = true
      };

      readFile.FileFormat.FieldDelimiter = "TAB";
      var timeFld = readFile.ColumnAdd(new Column { Name = "DateTime", DataType = DataType.DateTime });
      Debug.Assert(timeFld != null);
      timeFld.DateFormat = @"dd/MM/yyyy";
      timeFld.TimePart = "Time";
      timeFld.TimePartFormat = "HH:mm:ss";
      readFile.ColumnAdd(new Column { Name = "Integer", DataType = DataType.Integer });

      readFile.ColumnAdd(new Column { Name = "Numeric", DataType = DataType.Numeric });

      var numericFld = readFile.GetColumn("Numeric");
      Debug.Assert(numericFld != null);
      numericFld.DecimalSeparator = ".";

      var doubleFld = readFile.ColumnAdd(new Column { Name = "Double", DataType = DataType.Double });
      Debug.Assert(doubleFld != null);
      doubleFld.DecimalSeparator = ".";
      readFile.ColumnAdd(new Column { Name = "Boolean", DataType = DataType.Boolean });
      readFile.ColumnAdd(new Column { Name = "GUID", DataType = DataType.Guid });
      readFile.ColumnAdd(new Column
      {
        Name = "Time",
        Ignore = true
      });
      if (parent != null)
        parent.Input.Add(readFile);

      return readFile;
    }

    internal static CsvFile ReaderGetBasicCSV(IToolSetting parent = null, string id = "BasicCSV")
    {
      var readFile = new CsvFile();
      readFile.ID = id;
      readFile.FileFormat.CommentLine = "#";
      readFile.FileName = Path.Combine(FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles", "BasicCSV.txt");
      var examDateFld = readFile.ColumnAdd(new Column { Name = "ExamDate", DataType = DataType.DateTime });

      Debug.Assert(examDateFld != null);
      examDateFld.ValueFormat.DateFormat = @"dd/MM/yyyy";

      readFile.ColumnAdd(new Column { Name = "Score", DataType = DataType.Integer });
      readFile.ColumnAdd(new Column { Name = "Proficiency", DataType = DataType.Numeric });
      readFile.ColumnAdd(new Column { Name = "IsNativeLang", DataType = DataType.Boolean });

      if (parent != null)
        parent.Input.Add(readFile);

      return readFile;
    }
  }
}