using CsvTools;
using Plugin.Settings;

namespace CSVQuickViewer.Xamarin
{
  /// <summary>
  /// This is the Settings static class that can be used in your Core solution or in any
  /// of your client applications. All settings are laid out the same exact way with getters
  /// and setters.
  /// </summary>
  public static class Settings
  {    
    public static int CheckedRecords
    {
      get => CrossSettings.Current.GetValueOrDefault(nameof(CheckedRecords), 3000);
      set => CrossSettings.Current.AddOrUpdateValue(nameof(CheckedRecords), value);
    }

    public static bool CheckNamedDates
    {
      get => CrossSettings.Current.GetValueOrDefault(nameof(CheckNamedDates), true);
      set => CrossSettings.Current.AddOrUpdateValue(nameof(CheckNamedDates), value);
    }

    public static bool DateParts
    {
      get => CrossSettings.Current.GetValueOrDefault(nameof(DateParts), true);
      set => CrossSettings.Current.AddOrUpdateValue(nameof(DateParts), value);
    }

    public static bool DetectNumbers
    {
      get => CrossSettings.Current.GetValueOrDefault(nameof(DetectNumbers), true);
      set => CrossSettings.Current.AddOrUpdateValue(nameof(DetectNumbers), value);
    }

    public static bool DetectPercentage
    {
      get => CrossSettings.Current.GetValueOrDefault(nameof(DetectPercentage), true);
      set => CrossSettings.Current.AddOrUpdateValue(nameof(DetectPercentage), value);
    }

    public static bool DetectBoolean
    {
      get => CrossSettings.Current.GetValueOrDefault(nameof(DetectBoolean), true);
      set => CrossSettings.Current.AddOrUpdateValue(nameof(DetectBoolean), value);
    }

    public static bool DetectDateTime
    {
      get => CrossSettings.Current.GetValueOrDefault(nameof(DetectDateTime), true);
      set => CrossSettings.Current.AddOrUpdateValue(nameof(DetectDateTime), value);
    }

    public static bool DetectGuid
    {
      get => CrossSettings.Current.GetValueOrDefault(nameof(DetectGuid), true);
      set => CrossSettings.Current.AddOrUpdateValue(nameof(DetectGuid), value);
    }

    public static string FalseValue
    {
      get => CrossSettings.Current.GetValueOrDefault(nameof(FalseValue), string.Empty);
      set => CrossSettings.Current.AddOrUpdateValue(nameof(FalseValue), value);
    }

    public static bool IgnoreIdColumns
    {
      get => CrossSettings.Current.GetValueOrDefault(nameof(IgnoreIdColumns), true);
      set => CrossSettings.Current.AddOrUpdateValue(nameof(IgnoreIdColumns), value);
    }

    public static int MinSamples
    {
      get => CrossSettings.Current.GetValueOrDefault(nameof(MinSamples), 5);
      set => CrossSettings.Current.AddOrUpdateValue(nameof(MinSamples), value);
    }

    public static int SampleValues
    {
      get => CrossSettings.Current.GetValueOrDefault(nameof(SampleValues), 150);
      set => CrossSettings.Current.AddOrUpdateValue(nameof(SampleValues), value);
    }

    public static bool SerialDateTime
    {
      get => CrossSettings.Current.GetValueOrDefault(nameof(SerialDateTime), true);
      set => CrossSettings.Current.AddOrUpdateValue(nameof(SerialDateTime), value);
    }

    public static string TrueValue
    {
      get => CrossSettings.Current.GetValueOrDefault(nameof(TrueValue), string.Empty);
      set => CrossSettings.Current.AddOrUpdateValue(nameof(TrueValue), value);
    }

    public static bool GuessCodePage
    {
      get => CrossSettings.Current.GetValueOrDefault(nameof(GuessCodePage), true);
      set => CrossSettings.Current.AddOrUpdateValue(nameof(GuessCodePage), value);
    }

    public static bool GuessDelimiter
    {
      get => CrossSettings.Current.GetValueOrDefault(nameof(GuessDelimiter), true);
      set => CrossSettings.Current.AddOrUpdateValue(nameof(GuessDelimiter), value);
    }

    public static bool GuessHasHeader
    {
      get => CrossSettings.Current.GetValueOrDefault(nameof(GuessHasHeader), true);
      set => CrossSettings.Current.AddOrUpdateValue(nameof(GuessHasHeader), value);
    }

    public static bool GuessNewLine
    {
      get => CrossSettings.Current.GetValueOrDefault(nameof(GuessNewLine), true);
      set => CrossSettings.Current.AddOrUpdateValue(nameof(GuessNewLine), value);
    }

    public static bool GuessComment
    {
      get => CrossSettings.Current.GetValueOrDefault(nameof(GuessComment), true);
      set => CrossSettings.Current.AddOrUpdateValue(nameof(GuessComment), value);
    }

    public static bool GuessQualifier
    {
      get => CrossSettings.Current.GetValueOrDefault(nameof(GuessQualifier), true);
      set => CrossSettings.Current.AddOrUpdateValue(nameof(GuessQualifier), value);
    }

    public static bool GuessStartRow
    {
      get => CrossSettings.Current.GetValueOrDefault(nameof(GuessStartRow), true);
      set => CrossSettings.Current.AddOrUpdateValue(nameof(GuessStartRow), value);
    }

    public static string CurrentFile
    {
      get => CrossSettings.Current.GetValueOrDefault(nameof(CurrentFile), string.Empty);
      set => CrossSettings.Current.AddOrUpdateValue(nameof(CurrentFile), value);
    }
  }
}