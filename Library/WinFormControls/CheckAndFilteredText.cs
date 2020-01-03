using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace CsvTools
{
  /// <summary>
  /// </summary>
  /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
  public class CheckAndFilteredText : INotifyPropertyChanged
  {
    private readonly HashSet<string> m_AllItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private HashSet<string> m_CheckedItems;
    private string m_Exclude = string.Empty;
    private string m_Filter = string.Empty;
    private HashSet<string> m_ShownItems;
    private string m_Text = string.Empty;

    /// <summary>
    ///   Gets or sets the text.
    /// </summary>
    /// <value>
    ///   The text.
    /// </value>
    public string Text
    {
      get => m_Text;
      set
      {
        var newValue = value ?? string.Empty;
        if (m_Text == newValue) return;
        m_Text = value;
        var newSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var delimited = "," + m_Text.Trim().Replace(" ,", ",").Replace(", ", ",") + ",";
        foreach (var fld in m_AllItems)
          if (delimited.IndexOf("," + fld + ",", StringComparison.OrdinalIgnoreCase) != -1)
            newSet.Add(fld);
        if (!newSet.CollectionEqualWithOrder(CheckedItems))
        {
          m_CheckedItems = newSet;
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CheckedItems)));
        }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
      }
    }

    /// <summary>
    ///   Gets or sets the filter.
    /// </summary>
    /// <value>
    ///   The filter.
    /// </value>
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

    /// <summary>
    ///   Gets or sets the exclude.
    /// </summary>
    /// <value>
    ///   The exclude.
    /// </value>
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

    /// <summary>
    ///   Gets or sets all items.
    /// </summary>
    /// <value>
    ///   All items.
    /// </value>
    public ICollection<string> AllItems
    {
      get => m_AllItems;
      set
      {
        m_AllItems.Clear();
        if (value != null)
          foreach (var item in value)
            if (!string.IsNullOrEmpty(item))
              m_AllItems.Add(item);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AllItems)));
        RefreshShown();
      }
    }

    /// <summary>
    ///   Gets the shown items.
    /// </summary>
    /// <value>
    ///   The shown items.
    /// </value>
    public ICollection<string> ShownItems => m_ShownItems;

    /// <summary>
    ///   Gets the checked items.
    /// </summary>
    /// <value>
    ///   The checked items.
    /// </value>
    public ICollection<string> CheckedItems => m_CheckedItems;

    /// <summary>
    ///   As  property is changed this Event is called
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    private void RefreshShown()
    {
      var newSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      foreach (var fld in m_AllItems)
        if (!fld.Equals(m_Exclude, StringComparison.OrdinalIgnoreCase) &&
            fld.IndexOf(m_Filter, StringComparison.OrdinalIgnoreCase) != -1)
          newSet.Add(fld);

      if (!newSet.CollectionEqualWithOrder(ShownItems))
      {
        m_ShownItems = newSet;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShownItems)));
      }
    }

    /// <summary>
    /// Sets if an item is checked.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="isChecked">if set to <c>true</c> [is checked].</param>
    public void SetChecked(string text, bool isChecked)
    {
      if (!m_AllItems.Contains(text))
        return;
      var change = isChecked ? m_CheckedItems.Add(text) : m_CheckedItems.Remove(text);
      if (!change) return;

      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CheckedItems)));


      // Update the Text
      var sb = new StringBuilder();
      if (m_CheckedItems.Count > 0)
        foreach (var t in m_AllItems.Where(t => CheckedItems.Contains(t)))
        {
          sb.AddComma();
          sb.Append(t);
        }

      // do not use Text but the internal variable so CheckedItems is not rebuild
      m_Text = sb.ToString();
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
    }
  }
}