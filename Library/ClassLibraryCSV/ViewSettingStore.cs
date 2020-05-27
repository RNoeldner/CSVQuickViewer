using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CsvTools
{
  public class ColumnSetting : IEquatable<ColumnSetting>
  {
    public ColumnSetting(string dataPropertyName, bool visible, int sorted, int displayIndex, int width)
    {
      DataPropertyName = dataPropertyName;
      Visible = visible;
      DisplayIndex = displayIndex;
      Sort = sorted;
      Width = width;
    }

    public string DataPropertyName { get; set;  }
    public bool Visible { get; set; }
    public int Sort { get; set; }
    public int Width { get; set; }
    public int DisplayIndex { get; set; }

    public bool Equals(ColumnSetting other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return DataPropertyName == other.DataPropertyName && Visible == other.Visible && Sort == other.Sort &&
             Width == other.Width && DisplayIndex == other.DisplayIndex;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((ColumnSetting)obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = DataPropertyName != null ? DataPropertyName.GetHashCode() : 0;
        hashCode = (hashCode * 397) ^ Visible.GetHashCode();
        hashCode = (hashCode * 397) ^ Sort.GetHashCode();
        hashCode = (hashCode * 397) ^ Width;
        hashCode = (hashCode * 397) ^ DisplayIndex;
        return hashCode;
      }
    }
  }

  public class ValueFilter : IEquatable<ValueFilter>
  {
    public string SQLCondition { get; set; }
    public string Display { get; set; }

    public ValueFilter(string sQLCondition, string display)
    {
      SQLCondition = sQLCondition;
      Display = display;
    }

    public bool Equals(ValueFilter other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return SQLCondition == other.SQLCondition && Display == other.Display;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((ValueFilter)obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return ((SQLCondition != null ? SQLCondition.GetHashCode() : 0) * 397) ^ (Display != null ? Display.GetHashCode() : 0);
      }
    }
  }
  public class FilterSetting : IEquatable<FilterSetting>
  {
    public FilterSetting(string dataPropertyName, string condition, string valueText, DateTime valueDate)
    {
      DataPropertyName = dataPropertyName;
      Operator = condition;
      ValueText = valueText;
      ValueDate = valueDate;
    }

    public string DataPropertyName { get; set; }
    public string Operator { get; set; }
    public string ValueText { get; set; }
    public DateTime ValueDate { get; set; }

    public List<ValueFilter> ValueFilters { get; } = new List<ValueFilter>();

    public bool Equals(FilterSetting other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return DataPropertyName == other.DataPropertyName && Operator == other.Operator && ValueText == other.ValueText &&
             ValueDate.Equals(other.ValueDate) && Equals(ValueFilters, other.ValueFilters);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((FilterSetting)obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = (DataPropertyName != null ? DataPropertyName.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (Operator != null ? Operator.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (ValueText != null ? ValueText.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ ValueDate.GetHashCode();
        hashCode = (hashCode * 397) ^ (ValueFilters != null ? ValueFilters.GetHashCode() : 0);
        return hashCode;
      }
    }
  }

  public class ViewSettingStore
  {
    public List<ColumnSetting> Columns
    {
      get;
    } = new List<ColumnSetting>();

    public List<FilterSetting> Filter
    {
      get;
    } = new List<FilterSetting>();
  }
}