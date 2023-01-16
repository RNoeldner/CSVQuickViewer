using CsvTools;

namespace Maui
{
  public static class MauiStatic
  {
    public static async Task<DetectionResult> GetDetectionResult(this string fullPath, CancellationToken cancellationToken)
    {
      var preference = new PreferenceViewModel();

      return await fullPath.AnalyzeFileAsync(false, preference.GuessCodePage,
        preference.GuessEscapePrefix,
        preference.GuessDelimiter,
        preference.GuessQualifier,
        preference.GuessStartRow,
        preference.GuessHasHeader,
        false,
        preference.GuessComment, preference.GetFillGuessSettings(), cancellationToken).ConfigureAwait(false);
    }
  }
}
