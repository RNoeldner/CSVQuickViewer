namespace CsvTools
{
  public interface IValidationResult
  {
    long NumberRecords { get; set; }
    long ErrorCount { get; set; }
    long WarningCount { get; set; }
  }
}