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


using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace CsvTools.Tests
{
  public static class UnitTestStatic
  {
    private static UnitTestLogger? m_TestLogger;
    public static string LastLogMessage => m_TestLogger!.LastMessage;


    public static void RunSerializeAllProps<T>(T obj, in IReadOnlyCollection<string>? ignore = null) where T : class
    {
      foreach (var propertyInfo in obj.GetValueTypeProperty())
      {
        if (ignore != null && ignore.Contains(propertyInfo.Name))
          continue;
        try
        {
          if (!propertyInfo.ChangePropertyValue(obj))
            // could not change the property
            continue;
          var ret = RunSerialize(obj, false, true);
          Assert.AreEqual(propertyInfo.GetValue(obj), propertyInfo.GetValue(ret), $"Comparing changed value {propertyInfo.Name} of {obj.GetType().FullName}");
        }
        catch (Exception e)
        {
          Assert.Fail($"Could not clone, change or serialize {propertyInfo} in {obj.GetType().FullName}: \n{e.Message}");
        }
      }
    }

    public static T RunSerialize<T>(T obj, bool includeXml = true, bool includeJson = true) where T : class
    {
      if (obj == null)
        throw new ArgumentNullException("obj");

      T ret = obj;

      if (includeXml)
      {
        var serializer = new XmlSerializer(typeof(T));
        var testXml = obj.SerializeIndentedXml(serializer);
        Assert.IsFalse(string.IsNullOrEmpty(testXml));
        using TextReader reader = new StringReader(testXml);
        ret = serializer.Deserialize(reader) as T ?? throw new InvalidOperationException();
        Assert.IsNotNull(ret);
      }

      if (includeJson)
      {
        var testJson = obj.SerializeIndentedJson();
        Assert.IsFalse(string.IsNullOrEmpty(testJson));
        var pos = testJson.IndexOf("Specified\":", StringComparison.OrdinalIgnoreCase);
        Assert.IsFalse(pos != -1, $"Contains Specified as position {pos} in \n{testJson}");
        ret = JsonConvert.DeserializeObject<T>(testJson, SerializedFilesLib.JsonSerializerSettings.Value) ?? throw new InvalidOperationException();
        Assert.IsNotNull(ret);
      }

      return ret;
    }


#pragma warning disable CS8602
    public static void WriteToContext(this string s) => m_TestLogger!.Context.WriteLine(s);
#pragma warning restore CS8602

    public static readonly string ApplicationDirectory = Path.Combine(
      FileSystemUtils.ExecutableDirectoryName(),
      "TestFiles");

    public static CancellationToken Token { get; private set; } = CancellationToken.None;

    private static readonly Random m_Random = new Random(Guid.NewGuid().GetHashCode());

    public static ColumnMut[] ColumnsDt2 =
    {
      new ColumnMut("string") //0
    };

    public static ColumnMut[] ColumnsDt =
    {
      new ColumnMut("string"), //0
      new ColumnMut("int", new ValueFormat(DataTypeEnum.Integer)), //1
      new ColumnMut("DateTime", new ValueFormat(DataTypeEnum.DateTime)), //2
      new ColumnMut("bool", new ValueFormat(DataTypeEnum.Boolean)), //3
      new ColumnMut("double", new ValueFormat(DataTypeEnum.Double)), //4
      new ColumnMut("numeric", new ValueFormat(DataTypeEnum.Numeric)), //5
      new ColumnMut("AllEmpty"), //6
      new ColumnMut("PartEmpty"), //7
      new ColumnMut("ID", new ValueFormat(DataTypeEnum.Integer)) //8
    };

    public static MimicSQLReader MimicSQLReader { get; } = new MimicSQLReader();

    public static void AddRowToDataTable(DataTable dataTable, int recNum, bool addError)
    {
      var minDate = DateTime.Now.AddYears(-20).Ticks;
      var maxDate = DateTime.Now.AddYears(5).Ticks;
      var dr = dataTable.NewRow();
      dr[0] = GetRandomText(50);
      if (recNum % 10 == 0)
        dr[0] = dr[0] + "\r\nA Second Line";

      dr[1] = m_Random.Next(-300, +600);

      if (m_Random.NextDouble() > .2)
      {
        var dtm = (((maxDate - minDate) * m_Random.NextDouble()) + minDate).ToInt64();
        dr[2] = new DateTime(dtm);
      }

      dr[3] = m_Random.Next(0, 2) == 0;

      dr[4] = m_Random.NextDouble() * 123.78;

      if (recNum % 3 == 0)
        dr[5] = m_Random.NextDouble();

      if (m_Random.NextDouble() > .4) dr[7] = GetRandomText(100);

      dr[8] = recNum; // ID
      dr[9] = recNum * 2; // #Line

      // Add Errors and Warnings to Columns and Rows
      var rand = m_Random.Next(0, 100);
      if (rand > 70)
      {
        var colNum = m_Random.Next(0, 10);
        if (rand < 85)
          dr.SetColumnError(colNum, "First Warning".AddWarningId());
        else if (rand > 85) dr.SetColumnError(colNum, @"First Error");

        // Add a possible second error in the same column
        rand = m_Random.Next(-2, 3);
        if (rand == 1)
          dr.SetColumnError(colNum, dr.GetColumnError(colNum).AddMessage("Second Warning".AddWarningId()));
        else if (rand == 2) dr.SetColumnError(colNum, dr.GetColumnError(colNum).AddMessage("Second Error"));
      }

      if (rand > 80) dr.RowError = rand > 90 ? @"Row Error" : @"Row Warning".AddWarningId();
      if (addError)
        dr[10] = dr.GetErrorInformation();

      dataTable.Rows.Add(dr);
    }

    private static void CheckPropertiesEqual(in object a, in object b, in IEnumerable<PropertyInfo> properties)
    {
      foreach (var prop in properties)
        if (!Equals(prop.GetValue(a), prop.GetValue(b)))
          throw new Exception(
            $"Type: {a.GetType().FullName}\nProperty:{prop.Name}\nValue A:{prop.GetValue(a)}\nnValue B:{prop.GetValue(b)}");
    }

    public static ICollection<PropertyInfo> GetValueTypeProperty(this object myClass, IReadOnlyCollection<string>? ignore = null)
    {
      return myClass.GetType().GetProperties().Where(prop => !(ignore?.Contains(prop?.Name) ?? false) && prop.GetMethod != null &&
        (prop.PropertyType == typeof(int)
         || prop.PropertyType == typeof(long)
         || prop.PropertyType == typeof(string)
         || prop.PropertyType == typeof(bool)
         || prop.PropertyType == typeof(float)
         || prop.PropertyType == typeof(double)
         || prop.PropertyType == typeof(Guid)
         || prop.PropertyType == typeof(DateTime))).ToList();
    }

    public static bool ChangePropertyValue(this PropertyInfo prop, object obj1)
    {
      if (prop.PropertyType == typeof(byte))
      {
        byte newVal;
        do
        {
          newVal = Convert.ToByte(m_Random.Next(0, 255));
        } while ((byte) prop.GetValue(obj1) == newVal);

        prop.SetValue(obj1, newVal);
        return ((byte) prop.GetValue(obj1)) == newVal;
      }

      if (prop.PropertyType == typeof(int))
      {
        int newVal;
        do
        {
          newVal = m_Random.Next(-100, 10000);
        } while ((int) prop.GetValue(obj1) == newVal);

        prop.SetValue(obj1, newVal);
        return ((int) prop.GetValue(obj1)) == newVal;
      }

      if (prop.PropertyType == typeof(long))
      {
        long newVal;
        do
        {
          newVal = m_Random.Next(-100, 10000);
        } while ((long) prop.GetValue(obj1) == newVal);
        prop.SetValue(obj1, newVal);

        return ((long) prop.GetValue(obj1)) == newVal;
      }
      if (prop.PropertyType == typeof(long))
      {
        var newVal = (long) prop.GetValue(obj1) +1;

        prop.SetValue(obj1, newVal);
        return ((long) prop.GetValue(obj1)) == newVal;
      }
      if (prop.PropertyType == typeof(bool))
      {
        var newVal = !(bool) prop.GetValue(obj1);
        prop.SetValue(obj1, newVal);
        return ((bool) prop.GetValue(obj1)) == newVal;

      }

      if (prop.PropertyType == typeof(string))
      {
        var src = prop.GetValue(obj1) as string;
        var newVal = "RN";
        // ReSharper disable once ReplaceWithStringIsNullOrEmpty
        if (src != null && src.Length>0)
          newVal = GetRandomText(src.Length);
        prop.SetValue(obj1, newVal);

        return ((string) prop.GetValue(obj1)) == newVal;
      }

      if (prop.PropertyType == typeof(decimal))
      {
        decimal newVal;
        do
        {
          newVal = Convert.ToDecimal(m_Random.NextDouble());
        } while ((decimal) prop.GetValue(obj1) == newVal);

        prop.SetValue(obj1, newVal);
        return ((decimal) prop.GetValue(obj1)) == newVal;
      }

      if (prop.PropertyType == typeof(float))
      {
        float newVal;
        do
        {
          newVal = Convert.ToSingle(m_Random.NextDouble());
        } while (Math.Abs((float) prop.GetValue(obj1) - newVal) < .5);

        prop.SetValue(obj1, newVal);
        return Math.Abs(((float) prop.GetValue(obj1)) - newVal) < .01;
      }

      if (prop.PropertyType == typeof(double))
      {
        double newVal;
        do
        {
          newVal = m_Random.NextDouble();
        } while (Math.Abs((double) prop.GetValue(obj1) - newVal) < .5);

        prop.SetValue(obj1, newVal);
        return Math.Abs(((double) prop.GetValue(obj1)) - newVal) < .01;
      }

      if (prop.PropertyType == typeof(DateTime))
      {
        DateTime newVal;
        do
        {
          newVal = new DateTime(m_Random.Next(1980, 2030), m_Random.Next(1, 12), 1).AddDays(m_Random.Next(1, 31));
        } while ((DateTime) prop.GetValue(obj1) == newVal);
        prop.SetValue(obj1, newVal);
        return ((DateTime) prop.GetValue(obj1)) == newVal;
      }

      if (prop.PropertyType == typeof(Guid))
      {
        Guid newVal = Guid.NewGuid();
        prop.SetValue(obj1, newVal);
        return ((Guid) prop.GetValue(obj1)) == newVal;
      }

      return false;
    }

    public static void CheckAllPropertiesEqual(this object a, in object b, IReadOnlyCollection<string>? ignore = null)
    {
      if (ReferenceEquals(a, b))
        return;

      try
      {
        if (a is null)
          throw new ArgumentNullException(nameof(a));
        if (b is null)
          throw new ArgumentNullException(nameof(b));

        var valueProperties = a.GetValueTypeProperty(ignore);
        CheckPropertiesEqual(a, b, valueProperties);

        foreach (var prop in valueProperties.Where(prop =>
                   prop.PropertyType.AssemblyQualifiedName != null && !valueProperties.Contains(prop) &&
                   prop.PropertyType.AssemblyQualifiedName.StartsWith("CsvTools.", StringComparison.Ordinal)))
        {
          var obj1 = prop.GetValue(a);
          var obj2 = prop.GetValue(b);
          // ignore collections
          if (!(obj1 is IEnumerable))
            CheckAllPropertiesEqual(obj1, obj2);
        }
      }
      catch (Exception ex)
      {
        throw new ApplicationException($"Comparing: {a?.GetType()} with {b?.GetType()}", ex);
      }
    }

    public static void AssemblyInitialize(TestContext context)
    {
      MimicSql();
      Token = context.CancellationTokenSource.Token;
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12;

      Application.ThreadException += (sender, args) =>
      {
        if (!Token.IsCancellationRequested)
          WriteToContext(args.Exception.ToString());
        Assert.Fail(args.Exception.ToString());
      };
      AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs args)
      {
        if (!Token.IsCancellationRequested)
          WriteToContext(args.ExceptionObject.ToString());
        Assert.Fail(args.ExceptionObject.ToString());
      };
      m_TestLogger = new UnitTestLogger(context);
      Logger.LoggerInstance = m_TestLogger;
    }

    public static T ExecuteWithCulture<T>(Func<T> methodFunc, string cultureName)
    {
      var result = default(T);

      var thread = new Thread(() => { result = methodFunc(); }) { CurrentCulture = new CultureInfo(cultureName) };
      thread.Start();
      thread.Join();

#pragma warning disable CS8603
      return result;
#pragma warning restore CS8603
    }

    public static DataTable GetDataTable(int numRecords = 100, bool addError = true)
    {
      var dataTable = new DataTable { TableName = "ArtificialTable", Locale = new CultureInfo("en-gb") };
      dataTable.Columns.Add("string", typeof(string));
      dataTable.Columns.Add("int", typeof(int));
      dataTable.Columns.Add("DateTime", typeof(DateTime));
      dataTable.Columns.Add("bool", typeof(bool));
      dataTable.Columns.Add("double", typeof(double));
      dataTable.Columns.Add("numeric", typeof(decimal));
      dataTable.Columns.Add("AllEmpty", typeof(string));
      dataTable.Columns.Add("PartEmpty", typeof(string));
      dataTable.Columns.Add("ID", typeof(int));
      dataTable.Columns.Add(ReaderConstants.cStartLineNumberFieldName, typeof(long));
      if (addError)
        dataTable.Columns.Add(ReaderConstants.cErrorField, typeof(string));

      dataTable.BeginLoadData();
      for (var i = 1; i <= numRecords; i++) AddRowToDataTable(dataTable, i, addError);
      dataTable.EndLoadData();
      return dataTable;
    }

    public static DataTable GetDataTable2(long numRecords = 100)
    {
      var dataTable = new DataTable { TableName = "ArtificialTable2", Locale = new CultureInfo("en-gb") };
      dataTable.Columns.Add("string", typeof(string));
      for (long i = 1; i <= numRecords; i++)
      {
        var dr = dataTable.NewRow();
        dr[0] = i.ToString(CultureInfo.InvariantCulture);
        dataTable.Rows.Add(dr);
      }

      return dataTable;
    }

    public static string? GetRandomText(int length)
    {
      if (length < 1)
        return null;
      // Space is in there a few times so we get more spaces
      var chars = " abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ 1234567890 !§$%&/()=?+*#,.-;:_ "
        .ToCharArray();
      var data = new byte[length];
      using (var crypto = new RNGCryptoServiceProvider())
      {
        crypto.GetNonZeroBytes(data);
      }

      var result = new StringBuilder(length);
      foreach (var b in data)
        result.Append(chars[b % chars.Length]);
      return result.ToString();
    }

    public static string GetTestPath(string fileName) =>
      Path.Combine(ApplicationDirectory, fileName.TrimStart(' ', '\\', '/'));

    public static void MimicSql() => FunctionalDI.SqlDataReader = MimicSQLReader.ReadDataAsync;

    public static DataTable RandomDataTable(int records)
    {
      var dataTable = new DataTable { TableName = "DataTable", Locale = CultureInfo.InvariantCulture };

      dataTable.Columns.Add("ID", typeof(int));
      dataTable.Columns.Add("Text", typeof(string));
      dataTable.Columns.Add("ColText1", typeof(string));
      dataTable.Columns.Add("ColText2", typeof(string));
      dataTable.Columns.Add("ColTextDT", typeof(DateTime));
      var random = new Random(new Guid().GetHashCode());
      for (var i = 0; i < records; i++)
      {
        var row = dataTable.NewRow();
        row["ID"] = i;
        row["Text"] = i.ToString(CultureInfo.CurrentCulture);
        row["ColText1"] = $"Test{i + 1}";
        row["ColText2"] = $"Text {i * 2} !";
        row["ColTextDT"] = new DateTime(random.Next(1900, 2030), random.Next(1, 12), 1).AddDays(random.Next(1, 31));
        dataTable.Rows.Add(row);
      }

      return dataTable;
    }

    public static CsvFile ReaderGetAllFormats(string id = "AllFormats")
    {
      var readFile = new CsvFile(Path.Combine(GetTestPath("AllFormats.txt")),id)
      {
        HasFieldHeader = true,
        FieldDelimiter = "TAB"
      };

      readFile.ColumnCollection.Add(
        new Column("DateTime", new ValueFormat(dataType: DataTypeEnum.DateTime, dateFormat: @"dd/MM/yyyy"),
          timePart: "Time", timePartFormat: "HH:mm:ss"));
      readFile.ColumnCollection.Add(new Column("Integer", new ValueFormat(DataTypeEnum.Integer)));
      readFile.ColumnCollection.Add(
        new Column("Numeric", new ValueFormat(DataTypeEnum.Numeric, decimalSeparator: ".")));
      readFile.ColumnCollection.Add(
        new Column("Double", new ValueFormat(dataType: DataTypeEnum.Double, decimalSeparator: ".")));
      readFile.ColumnCollection.Add(new Column("Boolean", new ValueFormat(DataTypeEnum.Boolean)));
      readFile.ColumnCollection.Add(new Column("GUID", new ValueFormat(DataTypeEnum.Guid)));
      readFile.ColumnCollection.Add(
        new Column("Time", new ValueFormat(dataType: DataTypeEnum.DateTime, dateFormat: "HH:mm:ss"), ignore: true));
      return readFile;
    }

    public static CsvFile ReaderGetBasicCSV(string id = "BasicCSV")
    {
      var readFile = new CsvFile(Path.Combine(GetTestPath("BasicCSV.txt")),id) {  CommentLine = "#" };
      var examDateFld = new Column("ExamDate", new ValueFormat(DataTypeEnum.DateTime, @"dd/MM/yyyy"));
      readFile.ColumnCollection.Add(examDateFld);

      readFile.ColumnCollection.Add(new Column("Score", new ValueFormat(DataTypeEnum.Integer)));

      readFile.ColumnCollection.Add(new Column("Proficiency", new ValueFormat(DataTypeEnum.Numeric)));

      readFile.ColumnCollection.Add(new Column("IsNativeLang", new ValueFormat(DataTypeEnum.Boolean)));

      return readFile;
    }

    public static void RunCopyTo(IEnumerable<Type> list)
    {
      if (list == null) throw new ArgumentNullException(nameof(list));
      foreach (var type in list)
        try
        {
          // if there is not parameter less constructor skip
          if (type.GetConstructor(Type.EmptyTypes) == null)
            continue;

          var obj1 = Activator.CreateInstance(type);
          var obj2 = Activator.CreateInstance(type);

          var properties = type.GetProperties().Where(
            prop => prop.GetMethod != null && prop.SetMethod != null
                                           && (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(string)
                                             || prop.PropertyType == typeof(bool)
                                             || prop.PropertyType == typeof(DateTime))).ToArray();
          if (properties.Length == 0)
            continue;
          var start = 'a';
          foreach (var prop1 in properties)
          {
            if (prop1.PropertyType == typeof(int) || prop1.PropertyType == typeof(long)
                                                  || prop1.PropertyType == typeof(byte))
              prop1.SetValue(obj1, 17);

            if (prop1.PropertyType == typeof(bool))
              prop1.SetValue(obj1, !(bool) prop1.GetValue(obj1));

            if (prop1.PropertyType == typeof(char))
              prop1.SetValue(obj1, start++);

            if (prop1.PropertyType == typeof(string))
              prop1.SetValue(obj1, start++ + "_Raphael");

            if (prop1.PropertyType == typeof(DateTime))
              prop1.SetValue(obj1, new DateTime(2014, 12, 24));

            if (prop1.PropertyType == typeof(decimal))
              prop1.SetValue(obj1, 1.56m);

            if (prop1.PropertyType == typeof(float))
              prop1.SetValue(obj1, 22.7f);

            if (prop1.PropertyType == typeof(double))
              prop1.SetValue(obj1, 31.7d);
          }

          var methodClone = type.GetMethod("Clone", BindingFlags.Public | BindingFlags.Instance);
          if (methodClone is null)
            throw new Exception($"No clone method found for {type.FullName}");
          try
          {
            var obj3 = methodClone.Invoke(obj1, null);
            CheckPropertiesEqual(obj1, obj3, properties);
          }
          catch (Exception ex)
          {
            // Ignore all NotImplementedException these are cause by compatibility setting or mocks
            Debug.Write(ex.ExceptionMessages());
          }

          var methodCopyTo = type.GetMethod("CopyTo", BindingFlags.Public | BindingFlags.Instance);
          // Cloneable does mean you have to have CopyTo
          if (methodCopyTo == null) continue;

          try
          {
            // methodCopyTo.Invoke(obj1, new object[] { null });
            methodCopyTo.Invoke(obj1, new[] { obj2 });
            CheckPropertiesEqual(obj1, obj2, properties);

            methodCopyTo.Invoke(obj1, new[] { obj1 });
          }
          catch (Exception ex)
          {
            // Ignore all NotImplementedException these are cause by compatibility setting or mocks
            WriteToContext(ex.ExceptionMessages());
          }
        }
        catch (MissingMethodException)
        {
          // Ignore, there is no constructor without parameter
        }
        catch (Exception e)
        {
          WriteToContext($"Issue with {@type} {e.ExceptionMessages()}");
          throw;
        }
    }

    public static void RunTaskTimeout(Func<CancellationToken, Task> toDo, double timeout = 1,
      in CancellationToken token = default)
    {
      using var source = CancellationTokenSource.CreateLinkedTokenSource(token);
      Task.WhenAny(toDo.Invoke(source.Token), Task.Delay(TimeSpan.FromSeconds(timeout), source.Token));
      source.Cancel();
    }

    public static void ShowControl<T>(T ctrl, double before = .2, Action<T, Form>? toDo = null, double after = .2)
      where T : Control
    {
      using var cts = CancellationTokenSource.CreateLinkedTokenSource(Token);
      using var frm = new TestForm();
      frm.Closing += (s, e) =>
      {
        // ReSharper disable once AccessToDisposedClosure
        cts.Cancel();
      };
      frm.AddOneControl(ctrl, after * 6000d);
      ShowFormAndClose(frm, before, f => toDo?.Invoke(ctrl, f), after, cts.Token);
    }


    //private static void GetButtonsRecursive(Control rootControl, ICollection<Component> btns)
    //{
    //  foreach (Control ctrl in rootControl.Controls)
    //  {
    //    switch (ctrl)
    //    {
    //      case Button _:
    //        btns.Add(ctrl);
    //        break;
    //      case ToolStrip ts:
    //      {
    //        foreach (ToolStripItem i in ts.Items)
    //        {
    //          if (i is ToolStripButton)
    //            btns.Add(i);
    //        }

    //        break;
    //      }
    //      default:
    //      {
    //        if (ctrl.HasChildren)
    //          GetButtonsRecursive(ctrl, btns);
    //        break;
    //      }
    //    }
    //  }
    //}

    [DebuggerStepThrough]
    public static void WaitSomeTime(double seconds, in CancellationToken token)
    {
      var sw = new Stopwatch();
      sw.Start();
      while (sw.Elapsed.TotalSeconds < seconds && !token.IsCancellationRequested)
      {
        Application.DoEvents();
        Thread.Sleep(10);
      }
    }

    public static void ShowFormAndClose<T>(
      T typed, double before = 0, Action<T>? toDo = null, double after = 0, in CancellationToken token = default)
      where T : Form
    {
      var frm = typed as Form;
      var isClosed = false;
      frm.FormClosed += (s, o) =>
        isClosed = true;

      frm.TopMost = true;
      frm.ShowInTaskbar = false;
      try
      {
        frm.Show();
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        throw;
      }

      if (frm is ResizeForm res)
        res.ChangeFont(SystemFonts.DialogFont);

      frm.Focus();
      if (before > 0 && !isClosed)
        WaitSomeTime(before, token);

      if (toDo != null && !isClosed)
      {
        toDo.Invoke(typed);
        if (after > 0)
          WaitSomeTime(after, token);
      }

      if (!isClosed)
        frm.Close();
    }
  }
}