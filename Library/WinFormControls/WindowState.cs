using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace CsvTools
{
  [Serializable]
  public class WindowState
  {
    [XmlAttribute]
#pragma warning disable CA1051 // Do not declare visible instance fields
    public int Left = 0;

    [XmlAttribute]
    public int Top = 0;

    [XmlAttribute]
    public int Width = 0;

    [XmlAttribute]
    public int Height = 0;

    [XmlAttribute]
    public int State = 0;
#pragma warning restore CA1051 // Do not declare visible instance fields
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
  }
}
