#nullable enable
using Newtonsoft.Json;
using System.ComponentModel;
using System.Windows.Forms;

namespace CsvTools
{
  public sealed class WindowState
  {
    public static readonly WindowState Default =
      new(10, 10, 600, 600);

    [JsonConstructor]
    public WindowState(int left, int top, int width, int height, FormWindowState state = FormWindowState.Normal,
      int customInt = -2147483648,
      string customText = "")
    {
      Left = left;
      Top = top;
      Width = width;
      Height = height;
      State = state;
      CustomInt = customInt;
      CustomText = customText;
    }

    public readonly int Left;
    public readonly int Top;
    public readonly int Width;
    public readonly int Height;

    [DefaultValue(FormWindowState.Normal)] public readonly FormWindowState State;

    /// <summary>
    ///   Store form specific values like selected Tab or Splitter distance, can store any value but -1
    /// </summary>
    [DefaultValue(-2147483648)] public readonly int CustomInt;

    /// <summary>
    ///   Store form specific values like a filterText
    /// </summary>
    [DefaultValue("")] public readonly string CustomText;
  }
}