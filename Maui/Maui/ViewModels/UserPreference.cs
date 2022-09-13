using CsvTools;

namespace Maui
{
  /// <inheritdoc />
  public class PreferenceViewModel : BaseViewModel
  {
    private bool m_GuessCodePage = true;
    private bool m_GuessComment = true;
    private bool m_GuessDelimiter = true;
    private bool m_GuessHasHeader = true;
    private bool m_GuessQualifier = true;
    private bool m_GuessStartRow = true;
    private bool m_DisplayStartLineNo;
    private bool m_DisplayRecordNo;

    private long m_CheckedRecords = 30000;
    private bool m_CheckNamedDates = true;
    private bool m_DetectBoolean = true;
    private bool m_DetectDateTime = true;
    private bool m_DetectGuid;
    private bool m_DetectNumbers = true;
    private bool m_DetectPercentage = true;
    private bool m_EnabledFillGuess = true;
    private string m_FalseValue = "False";
    private bool m_IgnoreIdColumns = true;
    private int m_MinSamples = 5;
    private int m_SampleValues = 150;
    private bool m_SerialDateTime = true;
    private string m_TrueValue = "True";
    private bool m_DateParts;


    public PreferenceViewModel()
    {
      try
      {
        m_GuessCodePage = Preferences.Default.Get(nameof(GuessCodePage), true);
        m_GuessComment = Preferences.Default.Get(nameof(GuessComment), true);
        m_GuessDelimiter = Preferences.Default.Get(nameof(GuessDelimiter), true);
        m_GuessHasHeader = Preferences.Default.Get(nameof(GuessHasHeader), true);
        m_GuessQualifier = Preferences.Default.Get(nameof(GuessQualifier), true);
        m_GuessStartRow = Preferences.Default.Get(nameof(GuessStartRow), true);
        m_DisplayStartLineNo = Preferences.Default.Get(nameof(DisplayStartLineNo), true);
        m_DisplayRecordNo = Preferences.Default.Get(nameof(DisplayRecordNo), false);

        m_CheckedRecords = Preferences.Default.Get(nameof(CheckedRecords), 30000);
        m_CheckNamedDates = Preferences.Default.Get(nameof(CheckNamedDates), true);
        m_DetectBoolean = Preferences.Default.Get(nameof(DetectBoolean), true);
        m_DetectDateTime = Preferences.Default.Get(nameof(DetectDateTime), true);
        m_DetectGuid = Preferences.Default.Get(nameof(DetectGuid), false);
        m_DetectNumbers = Preferences.Default.Get(nameof(DetectNumbers), true);
        m_DetectPercentage = Preferences.Default.Get(nameof(DetectPercentage), true);
        m_EnabledFillGuess = Preferences.Default.Get(nameof(EnabledFillGuess), true);
        m_FalseValue = Preferences.Default.Get(nameof(FalseValue), "False");
        m_IgnoreIdColumns = Preferences.Default.Get(nameof(IgnoreIdColumns), true);
        m_MinSamples = Preferences.Default.Get(nameof(MinSamples), 5);
        m_SampleValues = Preferences.Default.Get(nameof(SampleValues), 150);
        m_SerialDateTime = Preferences.Default.Get(nameof(SerialDateTime), true);
        m_TrueValue = Preferences.Default.Get(nameof(TrueValue), "True");
        m_DateParts = Preferences.Default.Get(nameof(DateParts), false);
      }
      catch (Exception e)
      {
        // ignore
      }
    }
    public bool GuessCodePage { get => m_GuessCodePage; set { if (SetProperty(ref m_GuessCodePage, value)) Preferences.Default.Set(nameof(GuessCodePage), m_GuessCodePage); } }
    public bool GuessComment { get => m_GuessComment; set { if (SetProperty(ref m_GuessComment, value)) Preferences.Default.Set(nameof(GuessComment), m_GuessComment); } }
    public bool GuessDelimiter { get => m_GuessDelimiter; set { if (SetProperty(ref m_GuessDelimiter, value)) Preferences.Default.Set(nameof(GuessDelimiter), m_GuessDelimiter); } }
    public bool GuessHasHeader { get => m_GuessHasHeader; set { if (SetProperty(ref m_GuessHasHeader, value)) Preferences.Default.Set(nameof(GuessHasHeader), m_GuessHasHeader); } }
    public bool GuessQualifier { get => m_GuessQualifier; set { if (SetProperty(ref m_GuessQualifier, value)) Preferences.Default.Set(nameof(GuessQualifier), m_GuessQualifier); } }
    public bool GuessStartRow { get => m_GuessStartRow; set { if (SetProperty(ref m_GuessStartRow, value)) Preferences.Default.Set(nameof(GuessStartRow), m_GuessStartRow); } }
    public bool DisplayStartLineNo { get => m_DisplayStartLineNo; set { if (SetProperty(ref m_DisplayStartLineNo, value)) Preferences.Default.Set(nameof(DisplayStartLineNo), m_DisplayStartLineNo); } }
    public bool DisplayRecordNo { get => m_DisplayRecordNo; set { if (SetProperty(ref m_DisplayRecordNo, value)) Preferences.Default.Set(nameof(DisplayRecordNo), m_DisplayRecordNo); } }
    public long CheckedRecords { get => m_CheckedRecords; set { if (SetProperty(ref m_CheckedRecords, value)) Preferences.Default.Set(nameof(CheckedRecords), m_CheckedRecords); } }
    public bool CheckNamedDates { get => m_CheckNamedDates; set { if (SetProperty(ref m_CheckNamedDates, value)) Preferences.Default.Set(nameof(CheckNamedDates), m_CheckNamedDates); } }
    public bool DetectBoolean { get => m_DetectBoolean; set { if (SetProperty(ref m_DetectBoolean, value)) Preferences.Default.Set(nameof(DetectBoolean), m_DetectBoolean); } }
    public bool DetectDateTime { get => m_DetectDateTime; set { if (SetProperty(ref m_DetectDateTime, value)) Preferences.Default.Set(nameof(DetectDateTime), m_DetectDateTime); } }
    public bool DetectGuid { get => m_DetectGuid; set { if (SetProperty(ref m_DetectGuid, value)) Preferences.Default.Set(nameof(DetectGuid), m_DetectGuid); } }
    public bool DetectNumbers { get => m_DetectNumbers; set { if (SetProperty(ref m_DetectNumbers, value)) Preferences.Default.Set(nameof(DetectNumbers), m_DetectNumbers); } }
    public bool DetectPercentage { get => m_DetectPercentage; set { if (SetProperty(ref m_DetectPercentage, value)) Preferences.Default.Set(nameof(DetectPercentage), m_DetectPercentage); } }
    public bool EnabledFillGuess { get => m_EnabledFillGuess; set { if (SetProperty(ref m_EnabledFillGuess, value)) Preferences.Default.Set(nameof(EnabledFillGuess), m_EnabledFillGuess); } }
    public string FalseValue { get => m_FalseValue; set { if (SetProperty(ref m_FalseValue, value)) Preferences.Default.Set(nameof(FalseValue), m_FalseValue); } }
    public bool IgnoreIdColumns { get => m_IgnoreIdColumns; set { if (SetProperty(ref m_IgnoreIdColumns, value)) Preferences.Default.Set(nameof(IgnoreIdColumns), m_IgnoreIdColumns); } }
    public int MinSamples { get => m_MinSamples; set { if (SetProperty(ref m_MinSamples, value)) Preferences.Default.Set(nameof(MinSamples), m_MinSamples); } }
    public int SampleValues { get => m_SampleValues; set { if (SetProperty(ref m_SampleValues, value)) Preferences.Default.Set(nameof(SampleValues), m_SampleValues); } }
    public bool SerialDateTime { get => m_SerialDateTime; set { if (SetProperty(ref m_SerialDateTime, value)) Preferences.Default.Set(nameof(SerialDateTime), m_SerialDateTime); } }
    public string TrueValue { get => m_TrueValue; set { if (SetProperty(ref m_TrueValue, value)) Preferences.Default.Set(nameof(TrueValue), m_TrueValue); } }
    public bool DateParts { get => m_DateParts; set { if (SetProperty(ref m_DateParts, value)) Preferences.Default.Set(nameof(DateParts), m_DateParts); } }

    public FillGuessSettings GetFillGuessSettings() => new FillGuessSettings
    {
      CheckedRecords = m_CheckedRecords,
      CheckNamedDates = m_CheckNamedDates,
      DateParts = m_DateParts,
      DetectBoolean = m_DetectBoolean,
      DetectDateTime = m_DetectDateTime,
      DetectGuid = m_DetectGuid,
      DetectNumbers = m_DetectNumbers,
      DetectPercentage = m_DetectPercentage,
      Enabled = m_EnabledFillGuess,
      FalseValue = m_FalseValue,
      IgnoreIdColumns = m_IgnoreIdColumns,
      MinSamples = m_MinSamples,
      SampleValues = m_SampleValues,
      SerialDateTime = m_SerialDateTime,
      TrueValue = m_TrueValue
    };
  }
}
