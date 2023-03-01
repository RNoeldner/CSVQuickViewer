using BenchmarkDotNet.Attributes;
using System.Reflection;
using System.Text;

namespace Benchmark
{
  [MemoryDiagnoser]
  [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
  [RankColumn]
  public class BenchmarkCsvFileAnalysePerformance
  {
    [Params(10,100)]
    public int N = 100;

    public static readonly string ApplicationDirectory = Path.Combine(
      new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName ?? throw new InvalidOperationException(),
      "TestFiles");

    //[Benchmark]
    //public async Task AnalyseStringAsync()
    //{
    //  var fillGuessSettings = new CsvTools.Old.FillGuessSettings(true, detectNumbers: true, detectDateTime: true,
    //    detectPercentage: true, detectBoolean: true, detectGuid: true,
    //    ignoreIdColumns: true);

    //  for (var i = 0; i < N; i++)
    //  {
    //    using (var reader = new CsvTools.Old.CsvFileReader(
    //    Path.Combine(ApplicationDirectory, "AllFormats.txt"), Encoding.UTF8.CodePage, 0,
    //    true, new CsvTools.Old.Column[]
    //    {
    //      new CsvTools.Old.Column("DateTime", new CsvTools.Old.ValueFormat(CsvTools.Old.DataTypeEnum.DateTime), 0, true, true),
    //      new CsvTools.Old.Column("Integer", new CsvTools.Old.ValueFormat(CsvTools.Old.DataTypeEnum.Integer), 0, true, true)
    //    }, CsvTools.Old.TrimmingOptionEnum.All,
    //    '\t', '"', char.MinValue, 0, false, false, "", 0, true, "", "",
    //    "", true, false, false, true, true, false, true, true, true, true, false,
    //    treatTextAsNull: "NULL", skipEmptyLines: true, consecutiveEmptyRowsMax: 4,
    //    identifierInContainer: string.Empty, timeZoneAdjust: CsvTools.Old.StandardTimeZoneAdjust.ChangeTimeZone, destTimeZone: TimeZoneInfo.Local.Id, true, true))
    //    {
    //      await reader.OpenAsync(CancellationToken.None);
    //      var (information, columns) =   await CsvTools.Old.DetermineColumnFormat.FillGuessColumnFormatReaderAsyncReader(reader, fillGuessSettings,
    //        new CsvTools.Old.ColumnCollection(), false, true, "<NULL>", CancellationToken.None);
    //    }
    //  }
    //}

    [Benchmark]
    public async Task AnalysFileSpanAsync()
    {
      var fillGuessSettings = new CsvTools.FillGuessSettings(detectNumbers: true, detectDateTime: true,
        detectPercentage: true, detectBoolean: true, detectGuid: true,
        ignoreIdColumns: true);

      for (var i = 0; i < N; i++)
      {
        using var reader = new CsvTools.CsvFileReader(
          Path.Combine(ApplicationDirectory, "AllFormats.txt"), Encoding.UTF8.CodePage, 0,
          true, new[]
          {
            new CsvTools.Column("DateTime", new CsvTools.ValueFormat(CsvTools.DataTypeEnum.DateTime), 0, true, true),
            new CsvTools.Column("Integer", new CsvTools.ValueFormat(CsvTools.DataTypeEnum.Integer), 0, true, true)
          }, CsvTools.TrimmingOptionEnum.All,
          '\t', '"', char.MinValue, 0, false, false, "", 0, true, "", "",
          "", true, false, false, true, true, false, true, true, true, true, false,
          treatTextAsNull: "NULL", skipEmptyLines: true, consecutiveEmptyRowsMax: 4,
          identifierInContainer: string.Empty, timeZoneAdjust: CsvTools.StandardTimeZoneAdjust.ChangeTimeZone, destTimeZone: TimeZoneInfo.Local.Id, true, true);
        await reader.OpenAsync(CancellationToken.None);
        await CsvTools.DetermineColumnFormat.FillGuessColumnFormatReaderAsyncReader(reader, fillGuessSettings,
          new CsvTools.ColumnCollection(), false, true, "<NULL>", CancellationToken.None);
      }
    }
  }
}
