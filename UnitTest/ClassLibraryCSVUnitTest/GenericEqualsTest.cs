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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CsvTools.Tests
{
  [TestClass]
  public class GenericEqualsTest
  {
    [TestMethod]
    public void RunEquals()
    {


      var sb = new StringBuilder();
      foreach (var type in GetAllIEquatable())
        try
        {
          if (type.GetConstructor(Type.EmptyTypes)==null)
            continue;
          var obj1 = Activator.CreateInstance(type);
          var obj3 = Activator.CreateInstance(type);
          if (obj3 == null || obj1 == null)
            throw new NotSupportedException($"Could not create {type}");

          var properties = type.GetValueTypeProperty(null);
          if (properties.Count == 0)
            continue;
          var ignore = new List<PropertyInfo>();

          // Set some properties that should not match the default
          foreach (var prop in properties)
            if (!prop.ChangePropertyValue(obj1))
              ignore.Add(prop);

          var methodEquals = type.GetMethod(
            "Equals",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
            null,
            CallingConventions.Any,
            new[] { type },
            null);
          if (methodEquals != null)
            try
            {
              var isEqual = (bool) methodEquals.Invoke(obj1, new[] { obj3 });
              Assert.IsTrue(isEqual, $"Type: {type.FullName}");

              isEqual = (bool) methodEquals.Invoke(obj1, new[] { obj1 });
              Assert.IsTrue(isEqual, $"Type: {type.FullName}");

              isEqual = (bool) methodEquals.Invoke(obj1, new object[] { null });
              Assert.IsFalse(isEqual, $"Type: {type.FullName}");

              // Change only one Attribute at a time
              for (var c = 0; c < properties.Count(); c++)
              {
                var d = 0;
                var obj2 = Activator.CreateInstance(type);
                // make a copy
                foreach (var prop in properties)
                {
                  if (c != d)
                    prop.SetValue(obj2, prop.GetValue(obj1));
                  d++;
                }
                // change the one setting this is done to prevent other settings to reset the property
                foreach (var prop in properties)
                {
                  if (ignore.Contains(prop))
                    continue;
                  if (c == d)
                  {
                    if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(long))
                      prop.SetValue(obj2, 18);
                    else if (prop.PropertyType == typeof(bool) && prop.GetValue(obj1) is bool currentBool)
                      prop.SetValue(obj2, !currentBool);
                    else if (prop.PropertyType == typeof(string))
                      prop.SetValue(obj2, "Nöldner");
                    else if (prop.PropertyType == typeof(DateTime))
                      prop.SetValue(obj2, new DateTime(2015, 12, 24));
                    if (methodEquals.Invoke(obj1, new[] { obj2 }) is bool invokeOk && invokeOk)
                    {
                      sb.AppendLine(
                        $"Changing Property:{prop.Name} in Type: {type.FullName} was not seen as difference {prop.GetValue(obj1)}=>{prop.GetValue(obj2)} ");
                      Logger.Error("Type: {0}  Property:{1}", type.FullName, prop.Name);
                    }

                    break;
                  }
                  d++;
                }
              }
            }
            catch (Exception ex)
            {
              // Ignore all NotImplementedException these are cause by compatibility setting or mocks
              Logger.Error(ex.ExceptionMessages());
            }
        }
        catch (MissingMethodException)
        {
          // Ignore, there is no constructor without parameter
        }
        catch (Exception e)
        {
          Logger.Error(e, "Issue with {@type}", type);
          Assert.Fail($"Issue with {type.FullName}\n{e.Message}");
        }

      if (sb.Length > 0)
      {
        Assert.Fail(sb.ToString());
      }
    }

    private static IEnumerable<Type> GetAllIEquatable()
    {
      return AppDomain.CurrentDomain.GetAssemblies()
        .Where(a => a.FullName.StartsWith("ClassLibraryCSV", StringComparison.Ordinal))
        .SelectMany(a => a.GetExportedTypes(), (a, t) => new { a, t })
        .Where(t1 => t1.t.IsClass && !t1.t.IsAbstract)
        .SelectMany(t1 => t1.t.GetInterfaces(), (t1, i) => new { t1, i })
        .Where(t1 => t1.i.IsGenericType && t1.i.GetGenericTypeDefinition() == typeof(IEquatable<>))
        .Select(t1 => t1.t1.t);
    }
  }
}