/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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
using Microsoft.Win32;
using System;
using System.Reflection;

namespace CSVQuickViewer
{
  public static class FileAssociation
  {

    public static void EnsureRegisteredInstalledApp()
    {
      EnsureRegistered(AppDomain.CurrentDomain.FriendlyName.Replace(".exe", ""), Assembly.GetExecutingAssembly().Location);
    }

    public static void EnsureRegistered(string appName, string appPath)
    {

      var extensions = new[]
      {
                new { Ext = ".csv", ProgId = $"{appName}.csvfile",  Description = "Comma-Separated Values File" },
                new { Ext = ".tsv", ProgId = $"{appName}.tsvfile",  Description = "Tab-Separated Values File" },
                new { Ext = ".txt", ProgId = $"{appName}.txtfile",  Description = "Text Document" }
            };

      foreach (var ext in extensions)
      {
        // Check if already associated with our ProgID
        using (var extKey = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{ext.Ext}"))
        {
          if (extKey == null || !ext.ProgId.Equals(extKey.GetValue("")))
            RegisterExtension(ext.Ext, ext.ProgId, ext.Description, appPath);
        }
      }
    }

    private static void RegisterExtension(string extension, string progId, string description, string appPath)
    {
      // Associate extension with ProgID
      using (var extKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{extension}"))
      {
        extKey.SetValue("", progId);
      }

      // Create ProgID entry with description, icon, and open command
      using (var progKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{progId}"))
      {
        progKey.SetValue("", description);

        using (var iconKey = progKey.CreateSubKey("DefaultIcon"))
          iconKey.SetValue("", $"\"{appPath}\",0");

        using (var shellKey = progKey.CreateSubKey(@"Shell\Open\Command"))
          shellKey.SetValue("", $"\"{appPath}\" \"%1\"");
      }
    }
  }
}