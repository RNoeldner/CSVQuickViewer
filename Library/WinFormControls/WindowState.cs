#nullable enable

using System.Collections.Generic;

namespace CsvTools
{
  using System;
  using System.ComponentModel;
  using System.Drawing;
  using System.Windows.Forms;
  using System.Xml.Serialization;

  [Serializable]
  public class WindowState : IEqualityComparer<WindowState>
  {
    public WindowState()
    {
    }

    public WindowState(Rectangle rect, FormWindowState state)
    {
      Left = rect.Left;
      Top = rect.Top;
      Width = rect.Width;
      Height = rect.Height;
      State = (int) state;
    }

    [XmlAttribute]
    public int Left;

    [XmlAttribute]
    public int Top;

    [XmlAttribute]
    public int Width;

    [XmlAttribute]
    public int Height;

    [XmlAttribute]
    public int State;

    /// <summary>
    ///   Store form specific values like selected Tab or Splitter distance, can store any value but -1
    /// </summary>
    [XmlAttribute]
    [DefaultValue(int.MinValue)]
    public int CustomInt = int.MinValue;

    /// <summary>
    ///   Store form specific values like a filterText
    /// </summary>
    [XmlAttribute]
    [DefaultValue("")]
    public string CustomText = string.Empty;

    public bool Equals(WindowState? x, WindowState? y)
    {
      if (ReferenceEquals(x, y)) return true;
      if (ReferenceEquals(x, null)) return false;
      if (ReferenceEquals(y, null)) return false;
      if (x.GetType() != y.GetType()) return false;
      return x.Left == y.Left && x.Top == y.Top && x.Width == y.Width && x.Height == y.Height && x.State == y.State && x.CustomInt == y.CustomInt && x.CustomText == y.CustomText;
    }

    public int GetHashCode(WindowState obj)
    {
      unchecked
      {
        var hashCode = obj.Left;
        hashCode = (hashCode * 397) ^ obj.Top;
        hashCode = (hashCode * 397) ^ obj.Width;
        hashCode = (hashCode * 397) ^ obj.Height;
        hashCode = (hashCode * 397) ^ obj.State;
        hashCode = (hashCode * 397) ^ obj.CustomInt;
        hashCode = (hashCode * 397) ^ obj.CustomText.GetHashCode();
        return hashCode;
      }
    }
  }
}