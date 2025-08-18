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
