using CSVQuickViewer.Xamarin.Views;
using CsvTools;
using Xamarin.Forms;

namespace CSVQuickViewer.Xamarin.ViewModels
{
  public class DetectionSettingsViewModel : FillGuessSettings
  {
    public Command LoginCommand { get; }

    public DetectionSettingsViewModel()
    {
      CheckedRecords = Settings.CheckedRecords;
      CheckNamedDates = Settings.CheckNamedDates;
      DateParts = Settings.DateParts;
      DetectNumbers = Settings.DetectNumbers;
      DetectPercentage = Settings.DetectPercentage;
      DetectBoolean = Settings.DetectBoolean;
      DetectDateTime = Settings.DetectDateTime;
      DetectGUID = Settings.DetectGuid;
      FalseValue = Settings.FalseValue;
      IgnoreIdColumns = Settings.IgnoreIdColumns;
      MinSamples = Settings.MinSamples;
      SampleValues = Settings.SampleValues;
      SerialDateTime = Settings.SerialDateTime;
      TrueValue = Settings.TrueValue;

      LoginCommand = new Command(OnLoginClicked);
    }

    private async void OnLoginClicked(object obj)
    {
      Settings.CheckedRecords = (base.CheckedRecords> int.MaxValue) ? int.MaxValue : (base.CheckedRecords< 0) ? 0 : (int) base.CheckedRecords;
      Settings.CheckNamedDates = base.CheckNamedDates;
      Settings.DateParts = base.DateParts;
      Settings.DetectNumbers = base.DetectNumbers;
      Settings.DetectPercentage = base.DetectPercentage;
      Settings.DetectBoolean = base.DetectBoolean;
      Settings.DetectDateTime = base.DetectDateTime;
      Settings.DetectGuid = base.DetectGUID;
      Settings.FalseValue = base.FalseValue;
      Settings.IgnoreIdColumns = base.IgnoreIdColumns;
      Settings.MinSamples = base.MinSamples;
      Settings.SampleValues = base.SampleValues;
      Settings.SerialDateTime = base.SerialDateTime;
      Settings.TrueValue = base.TrueValue;

      await Shell.Current.GoToAsync($"//{nameof(SelectFilePage)}");
    }
  }
}