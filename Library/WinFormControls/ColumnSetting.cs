﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CsvTools
{
  public class ColumnSetting
  {
    public class ValueFilter
    {
      public ValueFilter(string sQlCondition, string display)
      {
        SQLCondition = sQlCondition;
        Display = display;
      }

      public string SQLCondition { get; set; }
      public string Display { get; set; }

      [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
      public override int GetHashCode()
      {
        unchecked
        {
          return ((SQLCondition != null ? SQLCondition.GetHashCode() : 0) * 397) ^
                 (Display != null ? Display.GetHashCode() : 0);
        }
      }
    }

    public ColumnSetting(string dataPropertyName, bool visible, int sorted, int displayIndex, int width)
    {
      DataPropertyName = dataPropertyName;
      Visible = visible;
      DisplayIndex = displayIndex;
      Sort = sorted;
      Width = width;
    }

    public string DataPropertyName { get; set; }
    public bool Visible { get; set; }
    public int Sort { get; set; }
    public int Width { get; set; }
    public int DisplayIndex { get; set; }
    public string Operator { get; set; }

    public string ValueText { get; set; }

    public DateTime ValueDate { get; set; }

    public List<ValueFilter> ValueFilters { get; } = new List<ValueFilter>();

    public bool ShouldSerializeSort() => Sort != 0;
    public bool ShouldSerializeOperator() => !string.IsNullOrEmpty(Operator);

    public bool ShouldSerializeValueText() => !string.IsNullOrEmpty(ValueText);

    public bool ShouldSerializeValueDate() => ValueDate.Year > 1;
    public bool ShouldSerializeValueFilters() => ValueFilters.Any();


    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = DataPropertyName != null ? DataPropertyName.GetHashCode() : 0;
        hashCode = (hashCode * 397) ^ Visible.GetHashCode();
        hashCode = (hashCode * 397) ^ Sort;
        hashCode = (hashCode * 397) ^ Width;
        hashCode = (hashCode * 397) ^ DisplayIndex;
        hashCode = (hashCode * 397) ^ (Operator != null ? Operator.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (ValueText != null ? ValueText.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ ValueDate.GetHashCode();
        hashCode = (hashCode * 397) ^ (ValueFilters != null ? ValueFilters.GetHashCode() : 0);
        return hashCode;
      }
    }
  }
}