#nullable enable

using System;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   Class to store dialog choices in a persistent way
  /// </summary>
  public sealed class PersistentChoice
  {
    public PersistentChoice(in DialogResult option)
    {
      if (option != DialogResult.Yes && option != DialogResult.No)
        throw new ArgumentOutOfRangeException(nameof(option), option, @"Only Yes and No are supported");
      DialogResult = option;
    }

    public bool Chosen { get; set; }

    public DialogResult DialogResult { get; private set; }

    public int NumRecs { get; private set; }

    public void ProcessedOne() => NumRecs--;

    public void Reset(int counter)
    {
      NumRecs = counter;
      Chosen = false;
    }
  }
}