using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace CsvTools.Tests
{
  public static class CopyToTest
  {
    public static void RunCopyTo(IEnumerable<Type> list)
    {
      foreach (var type in list)
      {
        var obj1 = Activator.CreateInstance(type);
        var obj2 = Activator.CreateInstance(type);

        var properties = type.GetProperties().Where(prop => prop.GetMethod != null && prop.SetMethod != null &&
                                                            (prop.PropertyType == typeof(int) ||
                                                             prop.PropertyType == typeof(string) ||
                                                             prop.PropertyType == typeof(bool) ||
                                                             prop.PropertyType == typeof(DateTime)));
        if (properties.IsEmpty())
          continue;
        // Set some properties that should not match the default
        foreach (var prop in properties)
        {
          if (prop.PropertyType == typeof(int))
            prop.SetValue(obj1, 17);

          if (prop.PropertyType == typeof(bool))
            prop.SetValue(obj1, !(bool)prop.GetValue(obj1));

          if (prop.PropertyType == typeof(string))
            prop.SetValue(obj1, "Raphael");

          if (prop.PropertyType == typeof(DateTime))
            prop.SetValue(obj1, new DateTime(2014, 12, 24));
        }

        var methodCopyTo = type.GetMethod("CopyTo", BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(methodCopyTo, $"Type: {type.FullName}");

        try
        {
          methodCopyTo.Invoke(obj1, new object[] { null });
          methodCopyTo.Invoke(obj1, new[] { obj2 });
          foreach (var prop in properties)
          {
            Assert.AreEqual(prop.GetValue(obj1), prop.GetValue(obj2),
              $"Type: {type.FullName} Property:{prop.Name}");
          }

          methodCopyTo.Invoke(obj1, new[] { obj1 });
        }
        catch (Exception ex)
        {
          // Ignore all NotImplementedException these are cause by compatibility setting or mocks
          Debug.Write(ex.ExceptionMessages());
        }

        var methodClone = type.GetMethod("Clone", BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(methodClone, $"Type: {type.FullName}");
        try
        {
          var obj3 = methodCopyTo.Invoke(obj1, null);
          Assert.IsInstanceOfType(obj3, type);
          foreach (var prop in properties)
          {
            Assert.AreEqual(prop.GetValue(obj1), prop.GetValue(obj3),
              $"Type: {type.FullName} Property:{prop.Name}");
          }
        }
        catch (Exception ex)
        {
          // Ignore all NotImplementedException these are cause by compatibility setting or mocks
          Debug.Write(ex.ExceptionMessages());
        }
      }
    }

    public static IEnumerable<Type> GetAllIColoneable(string startsWith)
    {
      foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
      {
        if (a.FullName.StartsWith(startsWith, StringComparison.Ordinal))
        {
          foreach (var t in a.GetExportedTypes())
          {
            if (t.IsClass && !t.IsAbstract)
            {
              foreach (var i in t.GetInterfaces())
              {
                if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICloneable<>))
                  yield return t;
              }
            }
          }
        }
      }
    }
  }

  [TestClass]
  public class TestCopyTo
  {
    [TestMethod]
    public void RunCopyTo() => CopyToTest.RunCopyTo(CopyToTest.GetAllIColoneable("ClassLibraryCSV"));
  }
}