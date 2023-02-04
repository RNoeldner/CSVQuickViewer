using CsvTools;

namespace Maui
{
  public static class MauiStatic
  {
    public static async Task<InspectionResult> GetDetectionResult(this string fullPath, CancellationToken cancellationToken)
    {
      var preference = new PreferenceViewModel();

      return await fullPath.InspectFileAsync(false, preference.GuessCodePage,
        preference.GuessEscapePrefix, preference.GuessDelimiter, preference.GuessQualifier, preference.GuessStartRow, preference.GuessHasHeader, false,
        preference.GuessComment, preference.GetFillGuessSettings(), preference.DefaultInspectionResult, cancellationToken).ConfigureAwait(false);
    }
  }
}
