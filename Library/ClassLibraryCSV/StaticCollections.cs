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

#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace CsvTools
{
  /// <summary>
  /// Store for static collection
  /// </summary>
  public static class StaticCollections
  {
    /// <summary>
    ///   '/', '-', '.', ' '
    /// </summary>
    public static readonly char[] DateSeparatorChars = { '/', '-', '.', ' ' };

    /// <summary>
    ///   ',', '.', ' ', '’', '⹁' 
    /// </summary>
    public static readonly char[] DecimalGroupingChars = { ',', '.', ' ', '’', '⹁' };

    /// <summary>
    ///   '.', ',', '/'
    /// </summary>
    public static readonly char[] DecimalSeparatorChars = { '.', ',', '/' };

    /// <summary>
    ///  '\t', ',', ';', '،', '؛', '|', '¦', '￤', '*', '`', '\u001F', '\u001E', '\u001D', '\u001C'
    /// </summary>
    public static readonly char[] DelimiterChars = {
      '\t', ',', ';', '،', '؛', '|', '¦', '￤', '*', '`', '\u001F', '\u001E', '\u001D', '\u001C'
    };

    /// <summary>
    /// \ / and ?
    /// </summary>
    public static readonly char[] EscapePrefixChars = { '\\', '/', '?' };

    /// <summary>
    ///   ';', '|',  CR LF Tab
    /// </summary>
    public static readonly char[] ListDelimiterChars = { ';', '|', '\r', '\n', '\t' };

    /// <summary>
    /// '"', '\''
    /// </summary>
    public static readonly char[] PossibleQualifiers = { '"', '\'' };

    /// <summary>
    ///   ':','.','h'
    /// </summary>
    public static readonly char[] TimeSeparators = { ':', '.' };

    /// <summary>
    ///   '¤', '$', '₪', '£', '₹', '€', '₼', '₽', '₦', '৳', '¥', '₱', '₡', '₲', '؋', '֏', '₾', '​', '₸', '៛', '₩', '₭', '₮', '₴', '฿', '₺', '₫'
    /// </summary>
    internal static readonly char[] CurrencySymbols = { '¤', '$', '₪', '£', '₹', '€', '₼', '₽', '₦', '৳', '¥', '₱', '₡', '₲', '؋', '֏', '₾', '​', '₸', '៛', '₩', '₭', '₮', '₴', '฿', '₺', '₫' };

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    internal static readonly string[] FalseValues =
        {
      "False", "0",  "No", "n", "F", "Non", "Nein", "Falsch", "無", "无", "假", "없음", "거짓", "ไม่ใช่", "เท็จ", "नहीं", "झूठी", "نہيں", "نه", "نادرست", "لا",
      "كاذبة", "جھوٹا", "שווא", "לא", "いいえ", "Фалшиви", "Ні", "Нет", "Не", "ЛОЖЬ", "Ψευδείς", "Όχι", "Yanlış", "Viltus", "Valse", "Vale", "Väärä", "Tidak",
      "Sai", "Palsu", "nu", "Nr", "nie", "NEPRAVDA", "nem", "Nej", "nei", "nē", "Ne", "Não", "na", "off", "le", "Klaidingas", "Không", "inactive", "aus",
      "Hayır", "Hamis", "Foloz", "Ffug", "Faux", "Fałszywe", "Falso", "Falske", "Falska", "Falsk", "Fals", "Falošné", "Ei"
    };

    // used to get rid of numeric suffixes like 12th or 3rd
    internal static readonly Lazy<Regex> RegExNumberSuffixEnglish =
      new Lazy<Regex>(() => new Regex(@"\b(\d+)\w?(?:st|nd|rd|th)\b"));

    /// <summary>
    ///   The possible length of a date for a given format
    /// </summary>
    internal static readonly DateTimeFormatCollection StandardDateTimeFormats =
      new DateTimeFormatCollection("DateTimeFormats.txt");

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    internal static readonly string[] TrueValues =
    {
      "True", "1", "-1",  "yes", "y", "t", "on", "Wahr", "Sì", "Si", "Ja", "active", "an", "Правда", "Да", "Вярно", "Vero", "Veritable", "Vera", "Jah", "igen",
      "真實", "真实", "真", "是啊", "예", "사실", "อย่างแท้จริง", "ใช่", "हाँ", "सच", "نعم", "صحيح", "سچا", "درست است", "جی ہاں", "بله", "נכון", "כן", "はい", "Так",
      "Ναι", "Αλήθεια", "Ya", "Wir", "Waar", "Vrai", "Verdadero", "Verdade", "Totta", "Tõsi", "Tiesa", "Tak", "taip", "Sim", "Sí", "Sant", "Sanna", "Sandt",
      "Res", "Prawdziwe", "Pravda", "Patiess", "Oui", "Kyllä", "jā", "Iva", "Igaz", "Ie", "Gerçek", "Evet", "Đúng", "da", "Có", "Benar", "áno", "Ano",
      "Adevărat"
    };
  }
}
