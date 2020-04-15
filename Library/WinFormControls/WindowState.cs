namespace CsvTools
{
  using System;
  using System.ComponentModel;
  using System.Drawing;
  using System.Windows.Forms;
  using System.Xml.Serialization;

  [Serializable]
  public class WindowState
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
      State = (int)state;
    }

    [XmlAttribute]
#pragma warning disable CA1051 // Do not declare visible instance fields
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
    ///   Store form specific values like a filetrText
    /// </summary>
    [XmlAttribute]
    [DefaultValue("")]
    public string CustomText = string.Empty;

#pragma warning restore CA1051 // Do not declare visible instance fields
  }
}