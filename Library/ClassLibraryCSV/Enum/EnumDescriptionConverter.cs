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
using System.ComponentModel;
using System.Globalization;

namespace CsvTools
{
  /// <inheritdoc />
  /// <summary>
  ///   Class to convert a Type into a text
  /// </summary>
  public class EnumDescriptionConverter : EnumConverter
  {
    private readonly Type m_EnumType;

    /// <inheritdoc />
    public EnumDescriptionConverter(Type enumType)
      : base(enumType) =>
      m_EnumType = enumType;

    /// <inheritdoc />
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
      sourceType == typeof(string);

    /// <summary>
    ///   Determines whether this instance [can convert to] the specified context.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="destinationType">Type of the destination</param>
    /// <returns></returns>
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) =>
      destinationType == typeof(string);

    /// <inheritdoc />
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
      if (value is null)
        throw new ArgumentNullException(nameof(value));
      foreach (var fi in m_EnumType.GetFields())
        if (Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute)) is DescriptionAttribute dna
            && (string) value == dna.Description)
          return Enum.Parse(m_EnumType, fi.Name);

      return Enum.Parse(m_EnumType, Convert.ToString(value));
    }

    /// <inheritdoc />
    /// <summary>
    ///   Converts the enumeration to a string, using the description attribute if defined
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="culture">The culture.</param>
    /// <param name="value">The value.</param>
    /// <param name="destinationType">Type of the destination</param>
    /// <returns></returns>
    public override object ConvertTo(
      ITypeDescriptorContext context,
      CultureInfo culture,
      object value,
      Type destinationType)
    {
      if (value is null)
        throw new ArgumentNullException(nameof(value));
      var fi = m_EnumType.GetField(Enum.GetName(m_EnumType, value));

      if (Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute)) is DescriptionAttribute dna)
        return dna.Description;
      // most enumeration have an underlying integer
      return value is int i ? i.ToString(culture) : Convert.ToString(value);
    }
  }
}