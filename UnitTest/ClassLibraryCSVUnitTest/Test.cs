using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools.Tests
{

public static class Utf8Fixer
  {
    public static string FixMisencodedUtf8(string garbled)
    {
      if (string.IsNullOrEmpty(garbled))
        return garbled;

      try
      {
        byte[] bytes = Encoding.GetEncoding(1252).GetBytes(garbled);
        return Encoding.UTF8.GetString(bytes);
      }
      catch
      {
        return garbled;
      }
    }

    public static void FixCsFilesInFolder(string folderPath)
    {
      if (!Directory.Exists(folderPath))
        throw new DirectoryNotFoundException($"Folder not found: {folderPath}");

      // Get all .cs files recursively
      string[] files = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);

      foreach (var file in files)
      {
        string[] lines = File.ReadAllLines(file, Encoding.Default); // read as Windows-1252
        bool changed = false;

        for (int i = 0; i < lines.Length; i++)
        {
          string fixedLine = FixMisencodedUtf8(lines[i]);
          if (fixedLine != lines[i])
          {
            lines[i] = fixedLine;
            changed = true;
          }
        }

        if (changed)
        {
          // Save back as UTF-8 without BOM
          File.WriteAllLines(file, lines, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
          Console.WriteLine($"Updated: {file}");
        }
      }
    }
  }

  [TestClass()]
  public class Utf8Fix
  {
    [TestMethod]
    public void CSVQuickViewer()
    {
      Utf8Fixer.FixCsFilesInFolder("C:\\Share\\Repos\\CSVQuickViewer");
    }
    [TestMethod]
    public void FileValidator()
    {
      Utf8Fixer.FixCsFilesInFolder("C:\\Share\\Repos\\FileValidator");        
    }
  }
}
