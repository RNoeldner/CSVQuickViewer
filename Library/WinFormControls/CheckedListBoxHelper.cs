using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CsvTools
{
  public class CheckedListBoxHelper
  {
    private const char c_Seperator = ',';
    private readonly List<string> m_AllItems = new List<string>();
    private readonly CheckedListBox m_CheckedListBox;
    private readonly List<string> m_ShownItems = new List<string>();
    private readonly TextBox m_TextBox;
    private string m_Exclude = string.Empty;
    private string m_Filter = string.Empty;
    private string m_Text = string.Empty;

    private HashSet<string> m_TextBoxItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public CheckedListBoxHelper(TextBox textBox, CheckedListBox checkedListBox)
    {
      m_TextBox = textBox ?? throw new ArgumentNullException(nameof(textBox));
      m_CheckedListBox = checkedListBox ?? throw new ArgumentNullException(nameof(checkedListBox));
      m_CheckedListBox.ItemCheck += ItemCheck;
      m_TextBox.TextChanged += TextChanged;
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
        UpdateCheckedListBox();
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
        UpdateCheckedListBox();
      }
    }

    public IList<string> Items
    {
      get => m_AllItems;
      set
      {
        m_AllItems.Clear();
        if (value != null)
          m_AllItems.AddRange(value.Where(x => !string.IsNullOrEmpty(x)).Distinct(StringComparer.OrdinalIgnoreCase));

        UpdateCheckedListBox();
      }
    }

    public static HashSet<string> GetKeys(string firstEntry, string list)
    {
      var keyColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      if (!string.IsNullOrEmpty(firstEntry))
        keyColumns.Add(firstEntry);

      foreach (var keyColumn in list.Split(','))
      {
        if (!string.IsNullOrWhiteSpace(keyColumn))
          keyColumns.Add(keyColumn.Trim());
      }

      return keyColumns;
    }

    private void UpdateCheckedListBox() => m_CheckedListBox.SafeInvokeNoHandleNeeded(() =>
                                         {
                                           m_CheckedListBox.BeginUpdate();
                                           m_CheckedListBox.Items.Clear();
                                           m_ShownItems.Clear();
                                           foreach (var fld in m_AllItems)
                                           {
                                             if (!fld.Equals(m_Exclude, StringComparison.OrdinalIgnoreCase) &&
                                                 fld.IndexOf(m_Filter, StringComparison.OrdinalIgnoreCase) != -1)
                                             {
                                               m_ShownItems.Add(fld);
                                               m_CheckedListBox.Items.Add(fld);
                                             }
                                           }

                                           m_CheckedListBox.EndUpdate();
                                           TextChanged(m_TextBox, null);
                                         });

    private void ItemCheck(object sender, ItemCheckEventArgs e)
    {
      if (e.NewValue == CheckState.Checked)
        m_TextBoxItems.Add(m_ShownItems[e.Index]);
      else if (e.NewValue == CheckState.Unchecked)
        m_TextBoxItems.Remove(m_ShownItems[e.Index]);

      // sort and build nicer list
      var sb = new StringBuilder();
      if (m_TextBoxItems.Count > 0)
      {
        foreach (var t in m_AllItems)
        {
          if (m_TextBoxItems.Contains(t))
          {
            if (sb.Length > 0)
              sb.Append(c_Seperator);
            sb.Append(t);
          }
        }
      }

      m_Text = sb.ToString();
      m_TextBox.SafeInvoke(() =>
      {
        m_TextBox.TextChanged -= TextChanged;
        m_TextBox.Text = m_Text;
        foreach (Binding b in m_TextBox.DataBindings)
        {
          if (b.PropertyName == "Text")
            b.WriteValue();
        }

        m_TextBox.TextChanged += TextChanged;
      });
    }

    private void TextChanged(object sender, EventArgs e)
    {
      m_CheckedListBox.ItemCheck -= ItemCheck;
      m_TextBox.SafeInvokeNoHandleNeeded(() =>
      {
        m_Text = m_TextBox.Text.Trim();
        m_TextBoxItems = GetKeys(null, m_Text);

        for (var index = 0; index < m_ShownItems.Count; index++)
          m_CheckedListBox.SetItemChecked(index, m_TextBoxItems.Contains(m_ShownItems[index]));
      });
      m_CheckedListBox.ItemCheck += ItemCheck;
    }
  }
}