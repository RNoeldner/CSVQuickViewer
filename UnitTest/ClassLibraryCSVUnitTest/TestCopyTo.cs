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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{

  [TestClass]
  public class TestCopyTo
  {
    private static IEnumerable<Type> GetAllICloneable(string startsWith) =>
      AppDomain.CurrentDomain.GetAssemblies()
        .Where(a => a.FullName.StartsWith(startsWith, StringComparison.Ordinal))
        .SelectMany(a => a.GetExportedTypes(), (a, t) => new { a, t })
        .Where(t1 => t1.t.IsClass && !t1.t.IsAbstract)
        .SelectMany(t1 => t1.t.GetInterfaces(), (t1, i) => new { t1, i })
        .Where(t1 => t1.i.IsGenericType && t1.i.GetGenericTypeDefinition() == typeof(ICloneable<>))
        .Select(t1 => t1.t1.t);


    public static void RunCopyTo([NotNull] IEnumerable<Type> list)
    {
      if (list == null) throw new ArgumentNullException(nameof(list));
      foreach (var type in GetAllICloneable("ClassLibraryCSV"))
        try
        {
          var obj1 = Activator.CreateInstance(type);
          var obj2 = Activator.CreateInstance(type);

          var properties = type.GetProperties().Where(
            prop => prop.GetMethod != null && prop.SetMethod != null
                                           && (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(string)
                                                                                || prop.PropertyType == typeof(bool)
                                                                                || prop.PropertyType == typeof(DateTime)
                                           )).ToArray();
          if (properties.Length == 0)
            continue;

          foreach (var prop1 in properties)
          {
            if (prop1.PropertyType == typeof(int))
              prop1.SetValue(obj1, 17);

            if (prop1.PropertyType == typeof(bool))
              prop1.SetValue(obj1, !(bool) prop1.GetValue(obj1));

            if (prop1.PropertyType == typeof(string))
              prop1.SetValue(obj1, "Raphael");

            if (prop1.PropertyType == typeof(DateTime))
              prop1.SetValue(obj1, new DateTime(2014, 12, 24));
          }

          var methodClone = type.GetMethod("Clone", BindingFlags.Public | BindingFlags.Instance);
          Assert.IsNotNull(methodClone, $"{type.FullName}.Clone()");
          try
          {
            var obj3 = methodClone.Invoke(obj1, null);
            Assert.IsInstanceOfType(obj3, type);
            foreach (var prop in properties)
              Assert.AreEqual(prop.GetValue(obj1), prop.GetValue(obj3), $"Type: {type.FullName} Property:{prop.Name}");
          }
          catch (Exception ex)
          {
            // Ignore all NotImplementedException these are cause by compatibility setting or mocks
            Debug.Write(ex.ExceptionMessages());
          }

          var methodCopyTo = type.GetMethod("CopyTo", BindingFlags.Public | BindingFlags.Instance);
          // Cloneable does mean you have to have CopyTo
          if (methodCopyTo==null) continue;

          try
          {
            methodCopyTo.Invoke(obj1, new object[] { null });
            methodCopyTo.Invoke(obj1, new[] { obj2 });

            foreach (var prop in properties)
              Assert.AreEqual(prop.GetValue(obj1), prop.GetValue(obj2), $"Type: {type.FullName} Property:{prop.Name}");

            methodCopyTo.Invoke(obj1, new[] { obj1 });
          }
          catch (Exception ex)
          {
            // Ignore all NotImplementedException these are cause by compatibility setting or mocks
            Debug.Write(ex.ExceptionMessages());
          }

        }
        catch (MissingMethodException)
        {
          // Ignore, there is no constructor without parameter
        }
        catch (AssertFailedException)
        {
          throw;
        }
        catch (Exception e)
        {
          Assert.Fail($"Issue with {type.FullName} {e.Message}");
        }
    }
    [TestMethod]
    public void RunCopyTo()
    {
      RunCopyTo(GetAllICloneable("ClassLibraryCSV"));
    }
  }
}