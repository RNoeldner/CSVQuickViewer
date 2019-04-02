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

    internal static CsvFile ReaderGetAllFormats(string id = "AllFormats")
    {
      var readFile = new CsvFile
      {
        ID = id,
        FileName = Path.Combine(FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles", "AllFormats.txt"),
        HasFieldHeader = true
      };

      readFile.FileFormat.FieldDelimiter = "TAB";
      var timeFld = readFile.ColumnCollection.AddIfNew(new Column { Name = "DateTime", DataType = DataType.DateTime });
      Debug.Assert(timeFld != null);
      timeFld.DateFormat = @"dd/MM/yyyy";
      timeFld.TimePart = "Time";
      timeFld.TimePartFormat = "HH:mm:ss";
      readFile.ColumnCollection.AddIfNew(new Column { Name = "Integer", DataType = DataType.Integer });

      readFile.ColumnCollection.AddIfNew(new Column { Name = "Numeric", DataType = DataType.Numeric });

      var numericFld = readFile.ColumnCollection.Get("Numeric");
      Debug.Assert(numericFld != null);
      numericFld.DecimalSeparator = ".";

      var doubleFld = readFile.ColumnCollection.AddIfNew(new Column { Name = "Double", DataType = DataType.Double });
      Debug.Assert(doubleFld != null);
      doubleFld.DecimalSeparator = ".";
      readFile.ColumnCollection.AddIfNew(new Column { Name = "Boolean", DataType = DataType.Boolean });
      readFile.ColumnCollection.AddIfNew(new Column { Name = "GUID", DataType = DataType.Guid });
      readFile.ColumnCollection.AddIfNew(new Column
      {
        Name = "Time",
        Ignore = true
      });
      

      return readFile;
    }

    internal static CsvFile ReaderGetBasicCSV(string id = "BasicCSV")
    {
      var readFile = new CsvFile
      {
        ID = id
      };
      readFile.FileFormat.CommentLine = "#";
      readFile.FileName = Path.Combine(FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles", "BasicCSV.txt");
      var examDateFld = readFile.ColumnCollection.AddIfNew(new Column { Name = "ExamDate", DataType = DataType.DateTime });

      Debug.Assert(examDateFld != null);
      examDateFld.ValueFormat.DateFormat = @"dd/MM/yyyy";

      readFile.ColumnCollection.AddIfNew(new Column { Name = "Score", DataType = DataType.Integer });
      readFile.ColumnCollection.AddIfNew(new Column { Name = "Proficiency", DataType = DataType.Numeric });
      readFile.ColumnCollection.AddIfNew(new Column { Name = "IsNativeLang", DataType = DataType.Boolean });

      return readFile;
    }
  }
}