using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace CsvTools
{
  public class CheckAndFilteredText : INotifyPropertyChanged
  {
    private readonly HashSet<string> m_AllItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private string m_Exclude = string.Empty;
    private string m_Filter = string.Empty;
    private string m_Text = string.Empty;
    private HashSet<string> m_CheckedItems;
    private HashSet<string> m_ShownItems;

    public string Text
    {
      get
      {
        return m_Text;
      }
      set
      {
        var newValue = value ?? string.Empty;
        if (m_Text == newValue) return;
        m_Text = value;
        var newSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var delimited = "," + m_Text.Trim().Replace(" ,", ",").Replace(", ", ",") + ",";
        foreach (var fld in m_AllItems)
        {
          if (delimited.IndexOf("," + fld + ",", StringComparison.OrdinalIgnoreCase) != -1)
            newSet.Add(fld);
        }
        if (!newSet.CollectionEqualWithOrder(CheckedItems))
        {
          m_CheckedItems = newSet;
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CheckedItems)));
        }
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
      }
    }

    public string Filter
    {
      get => m_Filter;
      set
      {
        var newVal = value ?? string.Empty;

        if (m_Filter.Equals(newVal))
          return;
        m_Filter = newVal;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Filter)));
        RefreshShown();
      }
    }

    public string Exclude
    {
      get => m_Exclude;
      set
      {
        var newVal = value ?? string.Empty;

        if (m_Exclude.Equals(newVal))
          return;
        m_Exclude = newVal;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Exclude)));
        RefreshShown();
      }
    }

    public ICollection<string> AllItems
    {
      get => m_AllItems;
      set
      {
        m_AllItems.Clear();
        if (value != null)
        {
          foreach (var item in value)
            if (!string.IsNullOrEmpty(item))
              m_AllItems.Add(item);
        }
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AllItems)));
        RefreshShown();
      }
    }

    public ICollection<string> ShownItems { get => m_ShownItems; }

    public ICollection<string> CheckedItems { get => m_CheckedItems; }

    public event PropertyChangedEventHandler PropertyChanged;

    private void RefreshShown()
    {
      var newSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      foreach (var fld in m_AllItems)
      {
        if (!fld.Equals(m_Exclude, StringComparison.OrdinalIgnoreCase) &&
            fld.IndexOf(m_Filter, StringComparison.OrdinalIgnoreCase) != -1)
        {
          newSet.Add(fld);
        }
      }

      if (!newSet.CollectionEqualWithOrder(ShownItems))
      {
        m_ShownItems = newSet;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShownItems)));
      }
    }

    public void SetChecked(string text, bool isChecked)
    {
      if (!m_AllItems.Contains(text))
        return;
      bool change = false;
      if (isChecked)
        change = m_CheckedItems.Add(text);
      else
        change = m_CheckedItems.Remove(text);
      if (change)
      {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CheckedItems)));
        var sb = new StringBuilder();
        if (CheckedItems.Count > 0)
        {
          foreach (var t in m_AllItems)
          {
            if (CheckedItems.Contains(t))
            {
              sb.AddComma();
              sb.Append(t);
            }
          }
        }
        m_Text = sb.ToString();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
      }
    }
  }
}
