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
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CsvTools
{
  public static class StringCollections
  {
    public static readonly IReadOnlyCollection<string> DateSeparators =
      new HashSet<string>(new[] { CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator, "/", ".", "-" },
        StringComparer.Ordinal);

    internal static readonly IReadOnlyCollection<string> CurrencySymbols =
      new HashSet<string>(new[] { CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol, "$", "€", "£", "¥", "¢", "₨" },
        StringComparer.Ordinal);

    public static readonly IReadOnlyCollection<char> DecimalGroupings = new HashSet<char>(
      new[] { CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator[0], '.', ',', ' ' });

    public static readonly IReadOnlyCollection<char> DecimalSeparators = new HashSet<char>(
      new[] { CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0], '.', ',' });

    // used to get rid of numeric suffixes like 12th or 3rd
    internal static readonly Lazy<Regex> RegExNumberSuffixEnglish =
      new Lazy<Regex>(() => new Regex(@"\b(\d+)\w?(?:st|nd|rd|th)\b"));

    /// <summary>
    ///   The possible length of a date for a given format
    /// </summary>
    internal static readonly DateTimeFormatCollection StandardDateTimeFormats =
      new DateTimeFormatCollection("DateTimeFormats.txt");

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    internal static readonly string[] m_FalseValues =
    {
      "0", "False", "No", "n", "F", "Non", "Nein", "Falsch", "無", "无", "假", "없음", "거짓", "ไม่ใช่", "เท็จ", "नहीं", "झूठी", "نہيں", "نه", "نادرست", "لا",
      "كاذبة", "جھوٹا", "שווא", "לא", "いいえ", "Фалшиви", "Ні", "Нет", "Не", "ЛОЖЬ", "Ψευδείς", "Όχι", "Yanlış", "Viltus", "Valse", "Vale", "Väärä", "Tidak",
      "Sai", "Palsu", "nu", "Nr", "nie", "NEPRAVDA", "nem", "Nej", "nei", "nē", "Ne", "Não", "na", "off", "le", "Klaidingas", "Không", "inactive", "aus",
      "Hayır", "Hamis", "Foloz", "Ffug", "Faux", "Fałszywe", "Falso", "Falske", "Falska", "Falsk", "Fals", "Falošné", "Ei"
    };



    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    internal static readonly string[] m_TrueValues =
    {
      "1", "-1", "True", "yes", "y", "t", "on", "Wahr", "Sì", "Si", "Ja", "active", "an", "Правда", "Да", "Вярно", "Vero", "Veritable", "Vera", "Jah", "igen",
      "真實", "真实", "真", "是啊", "예", "사실", "อย่างแท้จริง", "ใช่", "हाँ", "सच", "نعم", "صحيح", "سچا", "درست است", "جی ہاں", "بله", "נכון", "כן", "はい", "Так",
      "Ναι", "Αλήθεια", "Ya", "Wir", "Waar", "Vrai", "Verdadero", "Verdade", "Totta", "Tõsi", "Tiesa", "Tak", "taip", "Sim", "Sí", "Sant", "Sanna", "Sandt",
      "Res", "Prawdziwe", "Pravda", "Patiess", "Oui", "Kyllä", "jā", "Iva", "Igaz", "Ie", "Gerçek", "Evet", "Đúng", "da", "Có", "Benar", "áno", "Ano",
      "Adevărat"
    };
  }
}