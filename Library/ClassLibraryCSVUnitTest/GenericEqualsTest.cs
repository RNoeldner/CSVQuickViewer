using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace CsvTools.Tests
{
  [TestClass]
  public class GenericEqualsTest
  {
    [TestMethod]
    public void RunEquals()
    {
      foreach (var type in GetAllIEquatable())
      {
        var obj1 = Activator.CreateInstance(type);
        var obj3 = Activator.CreateInstance(type);

        var properties = type.GetProperties().Where(prop => prop.GetMethod != null && prop.SetMethod != null &&
                                                            (prop.PropertyType == typeof(int) ||
                                                             prop.PropertyType == typeof(long) ||
                                                             prop.PropertyType == typeof(string) ||
                                                             prop.PropertyType == typeof(bool) ||
                                                             prop.PropertyType == typeof(DateTime)));
        if (properties.IsEmpty())
          continue;
        // Set some properties that should not match the default
        foreach (var prop in properties)
        {
          if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(long))
          {
            prop.SetValue(obj1, 13);
            prop.SetValue(obj3, 13);
          }

          if (prop.PropertyType == typeof(bool))
          {
            prop.SetValue(obj1, !(bool)prop.GetValue(obj1));
            prop.SetValue(obj3, prop.GetValue(obj1));
          }

          if (prop.PropertyType == typeof(string))
          {
            prop.SetValue(obj1, "Raphael");
            prop.SetValue(obj3, prop.GetValue(obj1));
          }

          if (prop.PropertyType == typeof(DateTime))
          {
            prop.SetValue(obj1, new DateTime(2014, 12, 24));
            prop.SetValue(obj3, prop.GetValue(obj1));
          }
        }

        var methodEquals = type.GetMethod("Equals",
          BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, CallingConventions.Any,
          new[] { type }, null);
        if (methodEquals != null)
          try
          {
            var isEqual = (bool)methodEquals.Invoke(obj1, new[] { obj3 });
            Assert.IsTrue(isEqual, $"Type: {type.FullName}");

            isEqual = (bool)methodEquals.Invoke(obj1, new[] { obj1 });
            Assert.IsTrue(isEqual, $"Type: {type.FullName}");

            isEqual = (bool)methodEquals.Invoke(obj1, new object[] { null });
            Assert.IsFalse(isEqual, $"Type: {type.FullName}");

            // Chane only one Attribute at a time
            for (var c = 0; c < properties.Count(); c++)
            {
              var d = 0;
              PropertyInfo currentTest = null;
              var obj2 = Activator.CreateInstance(type);
              foreach (var prop in properties)
              {
                if (c != d)
                {
                  prop.SetValue(obj2, prop.GetValue(obj1));
                }
                else
                {
                  currentTest = prop;
                  if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(long))
                    prop.SetValue(obj2, 18);
                  else if (prop.PropertyType == typeof(bool))
                    prop.SetValue(obj2, !(bool)prop.GetValue(obj1));
                  else if (prop.PropertyType == typeof(string))
                    prop.SetValue(obj2, "Nöldner");
                  else if (prop.PropertyType == typeof(DateTime))
                    prop.SetValue(obj2, new DateTime(2015, 12, 24));
                }

                d++;
              }

              isEqual = (bool)methodEquals.Invoke(obj1, new[] { obj2 });
              Assert.IsFalse(isEqual, string.Format(System.Globalization.CultureInfo.InvariantCulture, "Type: {0}  Property:{1}", type.FullName, currentTest.Name));
            }
          }
          catch (Exception ex)
          {
            // Ignore all NotImplementedException these are cause by compatibility setting or mocks
            Debug.Write(ex.ExceptionMessages());
          }
      }
    }

    private IEnumerable<Type> GetAllIEquatable()
    {
      foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
        if (a.FullName.StartsWith("ClassLibraryCSV", StringComparison.Ordinal))
          foreach (var t in a.GetExportedTypes())
            if (t.IsClass && !t.IsAbstract)
              foreach (var i in t.GetInterfaces())
                if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEquatable<>))
                  yield return t;
    }
  }
}