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
using System.Collections.Generic;

namespace CsvTools
{
  public sealed class ColumnSetting
  {
    public string DataPropertyName { get; }
    public bool Visible { get; }
    public int Sort { get; }
    public int Width { get; }
    public int DisplayIndex { get; }
    public string Operator { get; set; } = string.Empty;
    public string ValueText { get; set; } = string.Empty;
    public DateTime ValueDate { get; set; }
    public ICollection<ValueFilter> ValueFilters { get; } = new List<ValueFilter>();

    public bool ShouldSerializeSort() => Sort != 0;
    public bool ShouldSerializeOperator() =>
      !string.IsNullOrEmpty(Operator) && (ShouldSerializeValueText() || ShouldSerializeValueDate());
    public bool ShouldSerializeValueText() => !string.IsNullOrEmpty(ValueText);
    public bool ShouldSerializeValueDate() => ValueDate.Year > 1;
    public bool ShouldSerializeValueFilters() => ValueFilters.Count>0;

    /// <summary>
    /// Storage for information on columns needed for ViewSettings
    /// </summary>
    /// <param name="dataPropertyName"></param>
    /// <param name="visible"></param>
    /// <param name="sorted"></param>
    /// <param name="displayIndex"></param>
    /// <param name="width"></param>
    public ColumnSetting(string dataPropertyName, bool visible, int sorted, int displayIndex, int width)
    {
      DataPropertyName = dataPropertyName;
      Visible= visible;
      Sort = sorted;
      DisplayIndex = displayIndex;
      Width = width;
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = DataPropertyName.GetHashCode();
        hashCode = (hashCode * 397) ^ Visible.GetHashCode();
        hashCode = (hashCode * 397) ^ Sort;
        hashCode = (hashCode * 397) ^ Width;
        hashCode = (hashCode * 397) ^ DisplayIndex;
        //hashCode = (hashCode * 397) ^ Operator.GetHashCode();
        //hashCode = (hashCode * 397) ^ ValueText.GetHashCode();
        //hashCode = (hashCode * 397) ^ ValueDate.GetHashCode();
        hashCode = (hashCode * 397) ^ ValueFilters.Count;
        return hashCode;
      }
    }

    public sealed class ValueFilter
    {
      public ValueFilter(string sQlCondition, string display)
      {
        SQLCondition = sQlCondition;
        Display = display;
      }

      public string SQLCondition { get; }
      public string Display { get; }

      public override int GetHashCode()
      {
        unchecked
        {
          return (SQLCondition.GetHashCode() * 397) ^
                 Display.GetHashCode();
        }
      }
    }
  }
}
