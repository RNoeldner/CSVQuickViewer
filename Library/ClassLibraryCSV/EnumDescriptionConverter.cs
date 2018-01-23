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
using System.Diagnostics.Contracts;
using System.Globalization;

namespace CsvTools
{
  /// <summary>
  ///   Class to convert a Type into a text
  /// </summary>
  public class EnumDescriptionConverter : EnumConverter
  {
    private readonly Type m_EnumType;

    /// <summary>
    ///   Initializes a new instance of the <see cref="EnumDescriptionConverter" /> class.
    /// </summary>
    /// <param name="enumType">
    ///   A <see cref="T:System.Type" /> that represents the type of enumeration to associate with this
    ///   enumeration converter.
    /// </param>
    public EnumDescriptionConverter(Type enumType)
      : base(enumType)
    {
      Contract.Requires(enumType != null);
      Contract.Requires(enumType.IsEnum);
      Contract.Ensures(m_EnumType != null);

      m_EnumType = enumType;
    }

    /// <summary>
    ///   Determines whether this instance [can convert from] the specified context.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="sourceType">Type of the source.</param>
    /// <returns></returns>
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
      return sourceType == typeof(string);
    }

    /// <summary>
    ///   Determines whether this instance [can convert to] the specified context.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="destinationType">Type of the destination</param>
    /// <returns></returns>
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    {
      return destinationType == typeof(string);
    }

    /// <inheritdoc />
    /// <summary>
    ///   Converts the specified value object to an enumeration object, using the Description attribute first
    /// </summary>
    /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context.</param>
    /// <param name="culture">
    ///   An optional <see cref="T:System.Globalization.CultureInfo" />. If not supplied, the current
    ///   culture is assumed.
    /// </param>
    /// <param name="value">The <see cref="T:object" /> to convert.</param>
    /// <returns>
    ///   An <see cref="T:object" /> that represents the converted <paramref name="value" />.
    /// </returns>
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
      if (value == null) throw new ArgumentNullException(nameof(value));
      foreach (var fi in m_EnumType.GetFields())
      {
        var dna = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));

        if (dna != null && (string)value == dna.Description)
          return Enum.Parse(m_EnumType, fi.Name);
      }

      return Enum.Parse(m_EnumType, value.ToString());
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
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
      Type destinationType)
    {
      if (value == null) throw new ArgumentNullException(nameof(value));
      var fi = m_EnumType.GetField(Enum.GetName(m_EnumType, value));
      var dna = (DescriptionAttribute)Attribute.GetCustomAttribute(fi, typeof(DescriptionAttribute));

      if (dna != null)
        return dna.Description;
      // most enumeration have an underlying integer
      return value is int i ? i.ToString(culture) : value.ToString();
    }
  }
}