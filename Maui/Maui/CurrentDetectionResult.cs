using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui.Platforms
{
  public static class Current
  {
    public DetectionResult? DetectionResult
    {
      get;
      set;
    }

    async Task GetDetectionResult(string fullPath, CancellationToken cancellationToken)
    {
      try
      {
        Current.DetectionResult = await result.FullPath.GetDetectionResultFromFile(false, Preferences.Default.Get(nameof(PreferenceViewModel.GuessCodePage), true),
          Preferences.Default.Get(nameof(PreferenceViewModel.GuessEscapePrefix), true),
            Preferences.Default.Get(nameof(PreferenceViewModel.GuessDelimiter), true),
            Preferences.Default.Get(nameof(PreferenceViewModel.GuessQualifier), true),
            Preferences.Default.Get(nameof(PreferenceViewModel.GuessStartRow), true),
            Preferences.Default.Get(nameof(PreferenceViewModel.GuessHasHeader), true),
            false,
            Preferences.Default.Get(nameof(PreferenceViewModel.GuessComment), true), cancellationToken);
      }
      catch (Exception exc)
      {
        await Application.Current?.MainPage?.DisplayAlert("Error", exc.Message, "OK")!;
      }
    }
  }
}
