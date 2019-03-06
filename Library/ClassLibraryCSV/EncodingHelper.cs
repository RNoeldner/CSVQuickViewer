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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text;
using Ude;

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

    private static readonly Lazy<int[]> m_CommonCodePages = new Lazy<int[]>(() => new[]
    {
   -1, (int) CodePage.UTF8, (int) CodePage.UTF16Le, (int) CodePage.UTF16Be, (int) CodePage.UTF32Le,
   (int) CodePage.UTF32Be, 1250, (int) CodePage.WIN1252, 1253, 1255, (int) CodePage.UTF7, 850, 852, 437, 28591,
   10029, 20127, 28597, 50220, 28592, 28595, 28598, 20866, 932, 54936
  });

    /// <summary>
    ///  A code page is a table of values that describes the character set for encoding a particular language.
    /// </summary>
    private enum CodePage
    {
      /// <summary>
      ///  Artificial CodePage to show no ode page has been determined
      /// </summary>
      None = 0,

      /// <summary>
      ///  The vast majority of code pages in current use are supersets of ASCII, a 7-bit code
      ///  representing 128 control codes and printable characters.
      /// </summary>
      ASCII = 20127,

      /// <summary>
      ///  ANSI/OEM Traditional Chinese (Taiwan; Hong Kong SAR, PRC); Chinese Traditional (Big5)
      /// </summary>
      BIG5 = 10002,

      /// <summary>
      ///  EUC Japanese
      /// </summary>
      EUCJP = 51932,

      /// <summary>
      ///  EUC Korean
      /// </summary>
      EUCKR = 51949,

      /// <summary>
      ///  GB18030 Simplified Chinese (4 byte); Chinese Simplified (GB18030)
      /// </summary>
      GB18030 = 54936,

      /// <summary>
      ///  OEM Cyrillic (primarily Russian)
      /// </summary>
      IBM855 = 855,

      /// <summary>
      ///  OEM Russian; Cyrillic (DOS)
      /// </summary>
      IBM866 = 866,

      /// <summary>
      ///  ISO 8859-7 Greek
      /// </summary>
      ISO88597 = 28597,

      /// <summary>
      ///  ISO 8859-2 Central European; Central European (ISO)
      /// </summary>
      ISO88592 = 28592,

      /// <summary>
      ///  ISO 8859-5 Cyrillic
      /// </summary>
      ISO88595 = 28595,

      /// <summary>
      ///  ISO 8859-8 Hebrew; Hebrew (ISO-Visual)
      /// </summary>
      ISO88598 = 28598,

      /// <summary>
      ///  Russian (KOI8-R); Cyrillic (KOI8-R)
      /// </summary>
      KOI8R = 20866,

      /// <summary>
      ///  Cyrillic (Mac)
      /// </summary>
      MacCyrillic = 10007,

      /// <summary>
      ///  ANSI/OEM Japanese; Japanese (Shift-JIS)
      /// </summary>
      ShiftJis = 932,

      /// <summary>
      ///  Unicode UTF-16, big endian byte order;
      /// </summary>
      UTF16Be = 1201,

      /// <summary>
      ///  Unicode UTF-16, little endian byte order (BMP of ISO 10646);
      /// </summary>
      UTF16Le = 1200,

      /// <summary>
      ///  Unicode UTF-32, big endian byte order
      /// </summary>
      UTF32Be = 12001,

      /// <summary>
      ///  Unicode UTF-32, little endian byte order
      /// </summary>
      UTF32Le = 12000,

      /// <summary>
      ///  Unicode (UTF-7)
      /// </summary>
      UTF7 = 65000,

      /// <summary>
      ///  Unicode (UTF-8)
      /// </summary>
      UTF8 = 65001,

      /// <summary>
      ///  ANSI Central European; Central European (Windows)
      /// </summary>
      WIN1250 = 1250,

      /// <summary>
      ///  ANSI Cyrillic; Cyrillic (Windows)
      /// </summary>
      WIN1251 = 1251,

      /// <summary>
      ///  ANSI Latin 1; Western European (Windows)
      /// </summary>
      WIN1252 = 1252,

      /// <summary>
      ///  ANSI Greek; Greek (Windows)
      /// </summary>
      WIN1253 = 1253,

      /// <summary>
      ///  ANSI Turkish; Turkish (Windows)
      /// </summary>
      WIN1255 = 1254,

      MSLatin = 850,
      OEMLatin = 852,
      MSDos = 437
    }

    /// <summary>
    ///  Gets a collection of the most common code pages.
    /// </summary>
    /// <value>An array of common code pages.</value>
    [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
    public static int[] CommonCodePages
    {
      get
      {
        Contract.Ensures(Contract.Result<int[]>() != null);
        return m_CommonCodePages.Value;
      }
    }

    /// <summary>
    ///   Gets the code page by byte order mark.
    /// </summary>
    /// <param name="buff">The buff.</param>
    /// <returns>The Code page id if, if the code page could not be identified 0</returns>
    public static int GetCodePageByByteOrderMark(byte[] buff)
    {
      if (buff == null)
        return (int)CodePage.None;

      if (buff.Length >= 4)
      {
        // Start with longer chains, as UTF16_LE looks like UTF32_LE for the first 2 chars
        if (buff[0] == 0x00 && buff[1] == 0x00 && buff[2] == 0xFE && buff[3] == 0xFF)
          return (int)CodePage.UTF32Be;
        if (buff[0] == 0xFF && buff[1] == 0xFE && buff[2] == 0x00 && buff[3] == 0x00)
          return (int)CodePage.UTF32Le;
        if (buff[0] == 0x84 && buff[1] == 0x31 && buff[2] == 0x95 && buff[3] == 0x33)
          return (int)CodePage.GB18030;
        if (buff[0] == 0x2B && buff[1] == 0x2F && buff[2] == 0x76 &&
          (buff[3] == 0x38 || buff[3] == 0x39 || buff[3] == 0x2B || buff[3] == 0xef))
        {
          return (int)CodePage.UTF7;
        }
      }

      if (buff.Length >= 3)
      {
        if (buff[0] == 0xEF && buff[1] == 0xBB && buff[2] == 0xBF)
          return (int)CodePage.UTF8;
      }

      if (buff.Length < 2) return (int)CodePage.None;
      if (buff[0] == 0xFE && buff[1] == 0xFF)
        return (int)CodePage.UTF16Be;
      if (buff[0] == 0xFF && buff[1] == 0xFE)
        return (int)CodePage.UTF16Le;

      return (int)CodePage.None;
    }

    /// <summary>
    ///   Gets the encoding.
    /// </summary>
    /// <param name="codePage">The code page ID.</param>
    /// <param name="byteOrderMark">if set to <c>true</c> [byte order mark].</param>
    /// <returns></returns>
    public static Encoding GetEncoding(int codePage, bool byteOrderMark)
    {
      Contract.Ensures(Contract.Result<Encoding>() != null);
      switch (codePage)
      {
        case (int)CodePage.UTF16Le:
          return new UnicodeEncoding(false, byteOrderMark);

        case (int)CodePage.UTF16Be:
          return new UnicodeEncoding(true, byteOrderMark);

        case (int)CodePage.UTF8:
        case -1: // Treat Guess Code page as (UTF-8)
          return new UTF8Encoding(byteOrderMark);

        case (int)CodePage.UTF32Le:
          return new UTF32Encoding(false, byteOrderMark);

        case (int)CodePage.UTF32Be:
          return new UTF32Encoding(true, byteOrderMark);

        default:
          return Encoding.GetEncoding(codePage);
      }
    }

    /// <summary>
    ///   Gets the name of the encoding.
    /// </summary>
    /// <param name="codePage">The code page ID.</param>
    /// <param name="hasBom">Flag indicating that byte order mark is present</param>
    /// <param name="showBom">Flag indicating that byte order mark information should be shown</param>
    /// <returns>The name</returns>
    public static string GetEncodingName(int codePage, bool showBom, bool hasBom)
    {
      Contract.Requires(codePage >= -1);
      Contract.Ensures(Contract.Result<string>() != null);

      const string suffixWithBom = " with BOM";
      string name;
      var suffixBom = hasBom ? suffixWithBom :
       showBom ? cSuffixWithoutBom : string.Empty;
      switch (codePage)
      {
        case -1:
          return "<Determine code page>";

        case (int)CodePage.UTF16Le:
          name = "Unicode (UTF-16) / ISO 10646 / UCS-2 Little-Endian" + suffixBom;
          break;

        case (int)CodePage.UTF16Be:
          name = "Unicode (UTF-16 Big-Endian) / UCS-2 Big-Endian" + suffixBom;
          break;

        case (int)CodePage.WIN1252:
          name = Encoding.GetEncoding(codePage).EncodingName + " / Latin I";
          break;

        case (int)CodePage.MSLatin:
          name = "Western European (DOS) / MS-DOS Latin 1";
          break;

        case (int)CodePage.OEMLatin:
          name = "Central European (DOS) / OEM Latin 2";
          break;

        case (int)CodePage.MSDos:
          name = "OEM United States / IBM PC:default / MS-DOS";
          break;

        default:
          name = Encoding.GetEncoding(codePage).EncodingName + suffixBom;
          break;
      }

      return $"CP {codePage} - {name}";
    }

    /// <summary>
    ///   Guesses the code page.
    /// </summary>
    /// <param name="buff">The buff containing the characters.</param>
    /// <param name="len">The length of the buffer.</param>
    /// <returns>The windows code page id</returns>
    public static int GuessCodePageNoBom(byte[] buff, int len)
    {
      if (buff == null)
        return (int)CodePage.UTF8;

      var detectedCodePage = CodePage.UTF8;

      var cdet = new CharsetDetector();
      cdet.Feed(buff, 0, len);
      cdet.DataEnd();

      if (cdet.Charset == null) return (int)detectedCodePage;
#pragma warning disable CA1308 // Normalize strings to uppercase
      if (cdet.Charset.ToLowerInvariant() == "ascii")
        detectedCodePage = CodePage.ASCII;
      else if (cdet.Charset.ToLowerInvariant() == "windows-1252")
        detectedCodePage = CodePage.WIN1252;
      else if (cdet.Charset.ToLowerInvariant() == "big-5")
        detectedCodePage = CodePage.BIG5;
      else if (cdet.Charset.ToLowerInvariant() == "euc-jp")
        detectedCodePage = CodePage.EUCJP;
      else if (cdet.Charset.ToLowerInvariant() == "euc-kr")
        detectedCodePage = CodePage.EUCKR;
      else if (cdet.Charset.ToLowerInvariant() == "gb18030")
        detectedCodePage = CodePage.GB18030;
      else if (cdet.Charset.ToLowerInvariant() == "shift-jis")
        detectedCodePage = CodePage.ShiftJis;
      else if (cdet.Charset.ToLowerInvariant() == "iso-8859-8")
        detectedCodePage = CodePage.ISO88598;
      else if (cdet.Charset.ToLowerInvariant() == "windows-1255")
        detectedCodePage = CodePage.WIN1255;
      else if (cdet.Charset.ToLowerInvariant() == "windows-1250")
        detectedCodePage = CodePage.WIN1250;
      else if (cdet.Charset.ToLowerInvariant() == "windows-1253")
        detectedCodePage = CodePage.WIN1253;
      else if (cdet.Charset.ToLowerInvariant() == "ibm866")
        detectedCodePage = CodePage.IBM866;
      else if (cdet.Charset.ToLowerInvariant() == "ibm855")
        detectedCodePage = CodePage.IBM855;
      else if (cdet.Charset.ToLowerInvariant() == "x-mac-cyrillic")
        detectedCodePage = CodePage.MacCyrillic;
      else if (cdet.Charset.ToLowerInvariant() == "iso-8859-5")
        detectedCodePage = CodePage.ISO88595;
      else if (cdet.Charset.ToLowerInvariant() == "windows-1251")
        detectedCodePage = CodePage.WIN1251;
      else if (cdet.Charset.ToLowerInvariant() == "iso-8859-7")
        detectedCodePage = CodePage.ISO88597;
      else if (cdet.Charset.ToLowerInvariant() == "iso-8859-2")
        detectedCodePage = CodePage.ISO88592;
      else if (cdet.Charset.ToLowerInvariant() == "koi8-r")
        detectedCodePage = CodePage.KOI8R;
      else if (cdet.Charset.ToLowerInvariant() == "utf-16le")
        detectedCodePage = CodePage.UTF16Le;
      else if (cdet.Charset.ToLowerInvariant() == "utf-16be")
        detectedCodePage = CodePage.UTF16Be;
      else if (cdet.Charset.ToLowerInvariant() == "utf-32be")
        detectedCodePage = CodePage.UTF32Be;
      else if (cdet.Charset.ToLowerInvariant() == "utf-32le")
        detectedCodePage = CodePage.UTF32Le;
#pragma warning restore CA1308 // Normalize strings to uppercase
      return (int)detectedCodePage;
    }
  }
}