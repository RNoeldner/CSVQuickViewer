using BenchmarkDotNet.Attributes;
using System.Reflection;
using System.Text;

namespace Benchmark
{
  [MemoryDiagnoser]
  [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
  [RankColumn]
  public class BenchmarkCsvFilePerformance
  {
    [Params(50)]
    public int N;

    public static readonly string ApplicationDirectory = Path.Combine(
      new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName ?? throw new InvalidOperationException(),
      "TestFiles");

    [Benchmark(Baseline = true)]
    public async Task ReadFileStringAsync()
    {
      for (var i = 0; i < N; i++)
      {
        using (var reader = new CsvTools.Old.CsvFileReader(
        Path.Combine(ApplicationDirectory, "AllFormats.txt"), Encoding.UTF8.CodePage, 0,
        true, new CsvTools.Old.Column[]
        {
          new CsvTools.Old.Column("DateTime", new CsvTools.Old.ValueFormat(CsvTools.Old.DataTypeEnum.DateTime), 0, true, true),
          new CsvTools.Old.Column("Integer", new CsvTools.Old.ValueFormat(CsvTools.Old.DataTypeEnum.Integer), 1, true, true),
          new CsvTools.Old.Column("Double", new CsvTools.Old.ValueFormat(CsvTools.Old.DataTypeEnum.Double), 2, true, true),
          new CsvTools.Old.Column("Numeric", new CsvTools.Old.ValueFormat(CsvTools.Old.DataTypeEnum.Double), 3, true, true),
          new CsvTools.Old.Column("Boolean", new CsvTools.Old.ValueFormat(CsvTools.Old.DataTypeEnum.Boolean), 6, true, true),
          new CsvTools.Old.Column("GUID", new CsvTools.Old.ValueFormat(CsvTools.Old.DataTypeEnum.Guid), 7, true, true),
          new CsvTools.Old.Column("Time", new CsvTools.Old.ValueFormat(CsvTools.Old.DataTypeEnum.DateTime), 8, true, true, timeZonePart: "TZ")
        }, CsvTools.Old.TrimmingOptionEnum.All,
        '\t', '"', char.MinValue, 0, false, false, "", 0, true, "", "",
        "", true, false, false, true, true, false, true, true, true, true, false,
        treatTextAsNull: "NULL", skipEmptyLines: true, consecutiveEmptyRowsMax: 4,
        identifierInContainer: string.Empty, timeZoneAdjust: CsvTools.Old.StandardTimeZoneAdjust.ChangeTimeZone, destTimeZone: TimeZoneInfo.Local.Id, true, true))
        {
          await reader.OpenAsync(CancellationToken.None);
          var values = new object[reader.FieldCount];
          while (await reader.ReadAsync(CancellationToken.None))
          {
            reader.GetValues(values);
          }
        }
      }
    }

    [Benchmark]
    public async Task ReadFileSpanAsync()
    {
      for (var i = 0; i < N; i++)
      {
        using (var reader = new CsvTools.CsvFileReader(
        Path.Combine(ApplicationDirectory, "AllFormats.txt"), Encoding.UTF8.CodePage, 0,
        true, new CsvTools.Column[]
        {
          new CsvTools.Column("DateTime", new CsvTools.ValueFormat(CsvTools.DataTypeEnum.DateTime), 0, true, true),
          new CsvTools.Column("Integer", new CsvTools.ValueFormat(CsvTools.DataTypeEnum.Integer), 1, true, true),
          new CsvTools.Column("Double", new CsvTools.ValueFormat(CsvTools.DataTypeEnum.Double), 2, true, true),
          new CsvTools.Column("Numeric", new CsvTools.ValueFormat(CsvTools.DataTypeEnum.Double), 3, true, true),
          new CsvTools.Column("Boolean", new CsvTools.ValueFormat(CsvTools.DataTypeEnum.Boolean), 6, true, true),
          new CsvTools.Column("GUID", new CsvTools.ValueFormat(CsvTools.DataTypeEnum.Guid), 7, true, true),
          new CsvTools.Column("Time", new CsvTools.ValueFormat(CsvTools.DataTypeEnum.DateTime), 8, true, true, timeZonePart: "TZ")
        }, CsvTools.TrimmingOptionEnum.All,
        '\t', '"', char.MinValue, 0, false, false, "", 0, true, "", "",
        "", true, false, false, true, true, false, true, true, true, true, false,
        treatTextAsNull: "NULL", skipEmptyLines: true, consecutiveEmptyRowsMax: 4,
        identifierInContainer: string.Empty, timeZoneAdjust: CsvTools.StandardTimeZoneAdjust.ChangeTimeZone, destTimeZone: TimeZoneInfo.Local.Id, true, true))
        {
          await reader.OpenAsync(CancellationToken.None);
          var values = new object[reader.FieldCount];
          while (await reader.ReadAsync(CancellationToken.None))
          {
            reader.GetValues(values);
          }
        }
      }
    }
  }
}
