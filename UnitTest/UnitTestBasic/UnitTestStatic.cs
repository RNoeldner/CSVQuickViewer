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



using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if XmlSerialization
using System.Xml.Serialization;
// ReSharper disable StringLiteralTypo
#endif


namespace CsvTools.Tests
{
  public static class UnitTestStatic
  {
    public static readonly string ApplicationDirectory = Path.Combine(
      (new FileInfo(Assembly.GetExecutingAssembly().Location)).DirectoryName,
      "TestFiles");

#pragma warning disable IDE0090
    // ReSharper disable once ArrangeObjectCreationWhenTypeEvident
    public static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());
#pragma warning restore IDE0090
#pragma warning disable CS8618
    private static UnitTestLogger m_TestLogger;
#pragma warning restore CS8618
    public static string LastLogMessage => m_TestLogger.LastMessage;

    public static CancellationToken Token { get; private set; } = CancellationToken.None;

    public static ILogger SetupTestContextLogger(TestContext context)
    {
      Token = context.CancellationTokenSource.Token;
#pragma warning disable CS0618
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12;
#pragma warning restore CS0618
      //AppDomain.CurrentDomain.UnhandledException += delegate (object _, UnhandledExceptionEventArgs args)
      //{
      //  if (!Token.IsCancellationRequested)
      //    WriteToContext(args.ExceptionObject.ToString()!);
      //  Assert.Fail(args.ExceptionObject.ToString());
      //};
      m_TestLogger = new UnitTestLogger(context);
      return m_TestLogger;
    }

    public static bool ChangePropertyValue(this PropertyInfo prop, object obj1)
    {
      if (prop.PropertyType == typeof(byte))
      {
        byte newVal;
        do
        {
          newVal = Convert.ToByte(Random.Next(0, 255));
        } while ((byte) (prop.GetValue(obj1)!) == newVal);

        prop.SetValue(obj1, newVal);
        return ((byte) prop.GetValue(obj1)!) == newVal;
      }

      if (prop.PropertyType == typeof(int))
      {
        int newVal;
        do
        {
          newVal = Random.Next(-100, 10000);
        } while ((int) prop.GetValue(obj1)! == newVal);

        prop.SetValue(obj1, newVal);
        return ((int) prop.GetValue(obj1)!) == newVal;
      }

      if (prop.PropertyType == typeof(long))
      {
        long newVal;
        do
        {
          newVal = Random.Next(-100, 10000);
        } while ((long) prop.GetValue(obj1)! == newVal);

        prop.SetValue(obj1, newVal);

        return ((long) prop.GetValue(obj1)!) == newVal;
      }

      if (prop.PropertyType == typeof(long))
      {
        var newVal = (long) prop.GetValue(obj1)! + 1;

        prop.SetValue(obj1, newVal);
        return ((long) prop.GetValue(obj1)!) == newVal;
      }

      if (prop.PropertyType == typeof(bool))
      {
        var newVal = !(bool) prop.GetValue(obj1)!;
        prop.SetValue(obj1, newVal);
        return ((bool) prop.GetValue(obj1)!) == newVal;
      }

      if (prop.PropertyType == typeof(string))
      {
        var src = prop.GetValue(obj1) as string;
        var newVal = "RN";
        // ReSharper disable once ReplaceWithStringIsNullOrEmpty
        if (src != null && src.Length > 0)
          newVal = GetRandomText(src.Length);
        prop.SetValue(obj1, newVal);

        return ((string) prop.GetValue(obj1)!) == newVal;
      }

      if (prop.PropertyType == typeof(decimal))
      {
        decimal newVal;
        do
        {
          newVal = Convert.ToDecimal(Random.NextDouble());
        } while ((decimal) prop.GetValue(obj1)! == newVal);

        prop.SetValue(obj1, newVal);
        return ((decimal) prop.GetValue(obj1)!) == newVal;
      }

      if (prop.PropertyType == typeof(float))
      {
        float newVal;
        do
        {
          newVal = Convert.ToSingle(Random.NextDouble());
        } while (Math.Abs((float) prop.GetValue(obj1)! - newVal) < .5);

        prop.SetValue(obj1, newVal);
        return Math.Abs(((float) prop.GetValue(obj1)!) - newVal) < .01;
      }

      if (prop.PropertyType == typeof(double))
      {
        double newVal;
        do
        {
          newVal = Random.NextDouble();
        } while (Math.Abs((double) prop.GetValue(obj1)! - newVal) < .5);

        prop.SetValue(obj1, newVal);
        return Math.Abs(((double) prop.GetValue(obj1)!) - newVal) < .01;
      }

      if (prop.PropertyType == typeof(DateTime))
      {
        DateTime newVal;
        do
        {
          newVal = new DateTime(Random.Next(1980, 2030), Random.Next(1, 12), 1).AddDays(Random.Next(1, 31));
        } while ((DateTime) prop.GetValue(obj1)! == newVal);

        prop.SetValue(obj1, newVal);
        return ((DateTime) prop.GetValue(obj1)!) == newVal;
      }

      if (prop.PropertyType == typeof(Guid))
      {
        Guid newVal = Guid.NewGuid();
        prop.SetValue(obj1, newVal);
        return ((Guid) prop.GetValue(obj1)!) == newVal;
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
            CheckAllPropertiesEqual(obj1!, obj2!);
        }
      }
      catch (Exception ex)
      {
        throw new ApplicationException($"Comparing: {a?.GetType()} with {b?.GetType()}", ex);
      }
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

    public static ICollection<PropertyInfo> GetValueTypeProperty(this object myClass,
      IReadOnlyCollection<string>? ignore = null)
    {
      return myClass.GetType().GetProperties().Where(prop =>
        !(ignore?.Contains(prop?.Name) ?? false) && prop?.GetMethod != null &&
        (prop.PropertyType == typeof(int)
         || prop.PropertyType == typeof(long)
         || prop.PropertyType == typeof(string)
         || prop.PropertyType == typeof(bool)
         || prop.PropertyType == typeof(float)
         || prop.PropertyType == typeof(double)
         || prop.PropertyType == typeof(Guid)
         || prop.PropertyType == typeof(DateTime))).ToList();
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
              prop1.SetValue(obj1, !(bool) prop1.GetValue(obj1)!);

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
            CheckPropertiesEqual(obj1!, obj3!, properties);
          }
          catch (Exception ex)
          {
            // Ignore all NotImplementedException these are cause by compatibility setting or mocks
            WriteToContext(ex.Message);
          }

          var methodCopyTo = type.GetMethod("CopyTo", BindingFlags.Public | BindingFlags.Instance);
          // Cloneable does mean you have to have CopyTo
          if (methodCopyTo == null) continue;

          try
          {
            // methodCopyTo.Invoke(obj1, new object[] { null });
            methodCopyTo.Invoke(obj1, new[] { obj2 });
            CheckPropertiesEqual(obj1!, obj2!, properties);

            methodCopyTo.Invoke(obj1, new[] { obj1 });
          }
          catch (Exception ex)
          {
            // Ignore all NotImplementedException these are cause by compatibility setting or mocks
            WriteToContext(ex.Message);
          }
        }
        catch (MissingMethodException)
        {
          // Ignore, there is no constructor without parameter
        }
        catch (Exception e)
        {
          WriteToContext($"Issue with {@type} {e.Message}");
          throw;
        }
    }

    public static T RunSerialize<T>(T obj, bool includeXml = true, bool includeJson = true) where T : class
    {
      if (obj == null)
        throw new ArgumentNullException(nameof(obj));

      T ret = obj;

      if (includeXml)
      {
#if XmlSerialization
        var serializer = new XmlSerializer(typeof(T));
        var testXml = obj.SerializeIndentedXml(serializer);

        Assert.IsFalse(string.IsNullOrEmpty(testXml));
        using TextReader reader = new StringReader(testXml);
        ret = serializer.Deserialize(reader) as T ?? throw new InvalidOperationException();
        Assert.IsNotNull(ret);
#endif
      }

      if (includeJson)
      {
        var jsonSerializerSettings = new JsonSerializerSettings
        {
          TypeNameHandling = TypeNameHandling.Auto,
          DefaultValueHandling = DefaultValueHandling.Ignore,
          ContractResolver = new CamelCasePropertyNamesContractResolver(),
          NullValueHandling = NullValueHandling.Ignore,
          ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
          DateFormatHandling = DateFormatHandling.IsoDateFormat,
          DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };
        jsonSerializerSettings.Converters.Add(new StringEnumConverter());

        var testJson = JsonConvert.SerializeObject(obj, Formatting.Indented, jsonSerializerSettings);
        Assert.IsFalse(string.IsNullOrEmpty(testJson));
        var pos = testJson.IndexOf("Specified\":", StringComparison.OrdinalIgnoreCase);

        Assert.IsFalse(pos != -1, $"Contains Specified as position {pos} in \n{testJson}");
        ret = JsonConvert.DeserializeObject<T>(testJson, jsonSerializerSettings) ??
              throw new InvalidOperationException();
        Assert.IsNotNull(ret);
      }

      return ret;
    }

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
          var ret = RunSerialize(obj, false);
          Assert.AreEqual(propertyInfo.GetValue(obj), propertyInfo.GetValue(ret),
            $"Comparing changed value {propertyInfo.Name} of {obj.GetType().FullName}");
        }
        catch (Exception e)
        {
          Assert.Fail(
            $"Could not clone, change or serialize {propertyInfo} in {obj.GetType().FullName}: \n{e.Message}");
        }
      }
    }
#pragma warning disable CS8602
    public static void RunTaskTimeout(Func<CancellationToken, Task> toDo, double timeout = 1,
      in CancellationToken token = default)
    {
      using var source = CancellationTokenSource.CreateLinkedTokenSource(token);
      Task.WhenAny(toDo.Invoke(source.Token), Task.Delay(TimeSpan.FromSeconds(timeout), source.Token));
      source.Cancel();
    }

    public static void WriteToContext(this string s) => m_TestLogger.Context.WriteLine(s);
#pragma warning restore CS8602
    private static void CheckPropertiesEqual(in object a, in object b, in IEnumerable<PropertyInfo> properties)
    {
      foreach (var prop in properties)
        if (!Equals(prop.GetValue(a), prop.GetValue(b)))
          throw new Exception(
            $"Type: {a.GetType().FullName}\nProperty:{prop.Name}\nValue A:{prop.GetValue(a)}\nnValue B:{prop.GetValue(b)}");
    }
  }

  public class UnitTestLogger : ILogger
  {
    public readonly TestContext? Context;
    public string LastMessage;

    public UnitTestLogger(TestContext? context)
    {
      Context = context;
      LastMessage = string.Empty;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
      LastMessage = formatter.Invoke(state, exception);
      Context?.WriteLine($"{logLevel} - {LastMessage}");
    }

    public bool IsEnabled(LogLevel logLevel) => logLevel > LogLevel.Debug;

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullLogger.Instance.BeginScope(state);
  }

}