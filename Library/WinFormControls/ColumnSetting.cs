#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace CsvTools
{
  public sealed class ColumnSetting
  {
    public ColumnSetting(in string dataPropertyName, bool visible, int sorted, int displayIndex, int width)
    {
      DataPropertyName = dataPropertyName;
      Visible = visible;
      DisplayIndex = displayIndex;
      Sort = sorted;
      Width = width;
    }

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

    public bool ShouldSerializeValueFilters() => ValueFilters.Any();

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
        hashCode = (hashCode * 397) ^ ValueFilters.GetHashCode();
        return hashCode;
      }
    }

    public class ValueFilter
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