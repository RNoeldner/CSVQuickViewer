﻿/*
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
using System.Data;
using System.Dynamic;

namespace CsvTools
{
  /// <summary>
  /// Represents a dynamic data record
  /// </summary>
  public class DynamicDataRecord : DynamicObject
  {
    private readonly Dictionary<string, object> m_Properties;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicDataRecord"/> class.
    /// </summary>
    /// <param name="dataRecord">The data record.</param>
    public DynamicDataRecord(in IDataRecord dataRecord)
    {
      m_Properties = new Dictionary<string, object>(dataRecord.FieldCount);
      for (var i = 0; i < dataRecord.FieldCount; i++)
        m_Properties.Add(dataRecord.GetName(i), dataRecord.GetValue(i));
    }


    /// <inheritdoc/>
    public override bool TryGetMember(GetMemberBinder binder, out object? result) =>
      m_Properties.TryGetValue(binder.Name, out result);


    /// <inheritdoc/>
    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
      try
      {
        if (value != null)
          m_Properties[binder.Name] = value;
        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }
  }
}