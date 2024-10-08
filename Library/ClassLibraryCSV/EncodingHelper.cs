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

using System.Text;
using UtfUnknown;

namespace CsvTools
{
  /// <summary>
  ///   Class to help with encodings
  /// </summary>
  public static class EncodingHelper
  {
    /// <summary>
    ///   The suffix of an encoding that does not have a BOM
    /// </summary>
    public const string cSuffixWithoutBom = " without BOM";

    /// <summary>
    ///   Gets a collection of the most common code pages.
    /// </summary>
    /// <value>An array of common code pages.</value>
    public static int[] CommonCodePages => new[]
    {
      Encoding.UTF8.CodePage, Encoding.Unicode.CodePage, Encoding.BigEndianUnicode.CodePage, 12000, 12001, 1252, 437,
      1250, 1253, 1255, 850, 852, 28591, 10029, 20127, 28597, 50220, 28592, 28595, 28598, 20866, 932, 54936
    };

#if NET5_0_OR_GREATER
    static EncodingHelper()
    {
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
#endif

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Determine the length of the BO´M
    /// </summary>
    /// <param name="codePage">Code Page</param>
    /// <returns>Length in Bytes</returns>
    public static byte BOMLength(int codePage)
    {
      // ReSharper disable once ConvertIfStatementToSwitchStatement
      if (codePage == Encoding.UTF8.CodePage)
        return 3;
      if (codePage == 12000 || codePage == 12001 || codePage == 54936)
        return 4;
      if (codePage == Encoding.Unicode.CodePage || codePage == Encoding.BigEndianUnicode.CodePage)
        return 2;

      return 0;
    }

    /// <summary>
    ///   Gets the encoding.
    /// </summary>
    /// <param name="codePage">The code page ID.</param>
    /// <param name="byteOrderMark">if set to <c>true</c> [byte order mark].</param>
    /// <returns></returns>
    public static Encoding GetEncoding(int codePage, bool byteOrderMark)
    {
      switch (codePage)
      {
        case 1200:
          return new UnicodeEncoding(false, byteOrderMark);

        case 1201:
          return new UnicodeEncoding(true, byteOrderMark);

        case 65001:
        case -1: // Treat Guess Code page as (UTF-8)
          return new UTF8Encoding(byteOrderMark);

        case 12000:
          return new UTF32Encoding(false, byteOrderMark);

        case 12001:
          return new UTF32Encoding(true, byteOrderMark);

        default:
          return Encoding.GetEncoding(codePage);
      }
    }

    /// <summary>Gets the code page by byte order mark.</summary>
    /// <param name="buff">The buff.</param>
    /// <param name="length"></param>
    /// <returns>The Code page id if, if the code page could not be identified 0</returns>
    public static Encoding? GetEncodingByByteOrderMark(byte[]? buff, int length)
    {
      if (buff is null || length < 2)
        return null;

      if (length >= 4)
      {
        // Start with longer chains, as UTF16_LE looks like UTF32_LE for the first 2 chars
        if (buff[0] == 0x00 && buff[1] == 0x00 && buff[2] == 0xFE && buff[3] == 0xFF)
          return Encoding.GetEncoding(12001);
        if (buff[0] == 0xFF && buff[1] == 0xFE && buff[2] == 0x00 && buff[3] == 0x00)
          return Encoding.UTF32;
        if (buff[0] == 0x84 && buff[1] == 0x31 && buff[2] == 0x95 && buff[3] == 0x33)
          return Encoding.GetEncoding(54936);
        if (buff[0] == 0x2B && buff[1] == 0x2F && buff[2] == 0x76
            && (buff[3] == 0x38 || buff[3] == 0x39 || buff[3] == 0x2B || buff[3] == 0x2f))
#pragma warning disable SYSLIB0001
          return Encoding.UTF7;
#pragma warning restore SYSLIB0001
      }

      if (length >= 3 && buff[0] == 0xEF && buff[1] == 0xBB && buff[2] == 0xBF)
        return Encoding.UTF8;

      if (buff[0] == 0xFE && buff[1] == 0xFF)
        return Encoding.BigEndianUnicode;
      if (buff[0] == 0xFF && buff[1] == 0xFE)
        return Encoding.Unicode;

      return null;
    }

    /// <summary>
    ///   Gets the name of the encoding.
    /// </summary>
    /// <param name="encoding">Encoding enumerable</param>
    /// <param name="hasBom">Flag indicating that byte order mark should be noted</param>
    /// <returns>The name</returns>
    public static string GetEncodingName(Encoding encoding, bool hasBom) => GetEncodingName(encoding.CodePage, hasBom);

    /// <summary>
    ///   Gets the name of the encoding.
    /// </summary>
    /// <param name="codePage">The code page ID.</param>
    /// <param name="hasBom">Flag indicating that byte order mark should be noted</param>
    /// <returns>The name</returns>
    public static string GetEncodingName(int codePage, bool hasBom)
    {
      const string suffixWithBom = " with BOM";
      var name = GetEncodingName(codePage);
      if (BOMLength(codePage) > 0)
        return name + (hasBom ? suffixWithBom : cSuffixWithoutBom);
      return name;
    }

    /// <summary>
    ///   Gets the name of the encoding.
    /// </summary>
    /// <param name="codePage">The code page ID.</param>
    /// <returns>The name</returns>
    public static string GetEncodingName(int codePage)
    {
      string name;

      switch (codePage)
      {
        case -1:
          return "<Determine code page>";
        case 65001:
          return "Unicode (UTF-8)";

        case 1200:
          return "Unicode (UTF-16) / ISO 10646 / UCS-2 Little-Endian";

        case 1201:
          return "Unicode (UTF-16 Big-Endian) / UCS-2 Big-Endian";

        case 12000:
          return "Unicode (UTF-32) / UCS-4 / ISO 10646";

        case 12001:
          return Encoding.GetEncoding(codePage).EncodingName;

        case 1252:
          name = Encoding.GetEncoding(codePage).EncodingName + " / Latin I";
          break;

        case 850:
          name = "Western European (DOS) / MS-DOS Latin 1";
          break;

        case 852:
          name = "Central European (DOS) / OEM Latin 2";
          break;

        case 437:
          name = "OEM United States / IBM PC:default / MS-DOS";
          break;

        default:
          name = Encoding.GetEncoding(codePage).EncodingName;
          break;
      }

      return $"CP {codePage} - {name}";
    }

    /// <summary>
    ///   Guesses the code page.
    /// </summary>
    /// <param name="buff">The buff containing the characters.</param>
    /// <returns><see cref="Encoding" /></returns>
    public static Encoding DetectEncodingNoBom(byte[]? buff)
    {
      if (buff is null || buff.Length < 1)
        return Encoding.UTF8;

      var results = CharsetDetector.DetectFromBytes(buff);
      if (results.Detected is null)
        return Encoding.UTF8;

      if (results.Detected.Confidence > 0.80)
        return results.Detected.Encoding;

      Logger.Warning(
        $"Confidence for detected character set {results.Detected.EncodingName} is only {results.Detected.Confidence:P}");
      foreach (var res in results.Details)
      {
        if (res.Confidence > 0.5 && res.Encoding.Equals(Encoding.UTF8))
          return Encoding.UTF8;
        if (res.Confidence > 0.5 && res.Encoding.Equals(Encoding.UTF32))
          return Encoding.UTF32;
        if (res.Confidence > 0.5 && res.Encoding.Equals(Encoding.Unicode))
          return Encoding.Unicode;
      }

      return results.Detected.Encoding;
    }
  }
}