namespace CsvTools
{
  public interface IValidationResult
  {
    long NumberRecords { get; }
    long ErrorCount { get; }
    long WarningCount { get; }
  }
}