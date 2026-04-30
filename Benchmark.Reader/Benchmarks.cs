using BenchmarkDotNet.Attributes;
using CsvTools;
using Microsoft.VSDiagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmark.Reader
{
  // For more information on the VS BenchmarkDotNet Diagnosers see https://learn.microsoft.com/visualstudio/profiling/profiling-with-benchmark-dotnet
  [CPUUsageDiagnoser]
  [MemoryDiagnoser]
  public class Benchmarks
  {
    private static string _filePathAll;
    private static string _filePathLong;
    private byte[] _fileBytes;
    private Column[] _cachedColumns;
    private const int _rows = 1_000_000;

    [GlobalCleanup]
    public void Cleanup()
    {
      // Clear the byte array reference for GC
      _fileBytes = null;
    }

    [GlobalSetup]
    public void Setup()
    {
      _filePathAll = Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, "AllFormats.txt");
      _filePathLong = Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, "PackageAssets.csv");

      // 1. Explicit Validation
      if (!File.Exists(_filePathAll) || !File.Exists(_filePathLong))
      {
        throw new FileNotFoundException(
            $"Benchmark failed: Data file not found at {_filePathAll} / {_filePathLong}." +
            "Ensure the file is set to 'Copy to Output Directory' in your project settings.");
      }

      // Read the 1,671 lines
      string[] originalLines = File.ReadAllLines(_filePathLong);
      int originalCount = originalLines.Length;
      // Estimate capacity to avoid multiple internal re-allocations
      // (Average line length * target rows)
      var sb = new StringBuilder(originalLines[0].Length * _rows);

      for (int i = 0; i < _rows; i++)
      {
        // Use modulo to cycle through the 1,671 lines repeatedly
        sb.AppendLine(originalLines[i % originalCount]);
      }

      // Convert to UTF8 bytes for the MemoryStream
      _fileBytes = Encoding.UTF8.GetBytes(sb.ToString());

      // Clear SB to free memory early
      sb.Clear();

      // Pre-create columns to avoid allocation overhead in the Typed test
      _cachedColumns = new[] {
            new Column("DateTime", new ValueFormat(DataTypeEnum.DateTime), 0, true, true),
            new Column("Integer", new ValueFormat(DataTypeEnum.Integer), 1, true, true),
            new Column("Double", new ValueFormat(DataTypeEnum.Double), 2, true, true),
            new Column("Numeric", new ValueFormat(DataTypeEnum.Double), 3, true, true),
            new Column("Boolean", new ValueFormat(DataTypeEnum.Boolean), 6, true, true),
            new Column("GUID", new ValueFormat(DataTypeEnum.Guid), 7, true, true),
            new Column("Time", new ValueFormat(DataTypeEnum.DateTime), 8, true, true, timeZonePart: "TZ")
        };
    }

    public async Task ReadAsyncUntyped()
    {
      using var reader = new CsvFileReader(
        fileName: _filePathAll,
        trimmingOption: TrimmingOptionEnum.Unquoted, fieldDelimiterChar: '\t', fieldQualifierChar: '"');
      await reader.OpenAsync(CancellationToken.None);
      for (int i = 0; i < reader.FieldCount; i++)
      {
        _ = reader.GetColumn(i); // Access column metadata to ensure it's loaded
        reader.GetOrdinal(reader.GetName(i)); // Access column metadata to ensure it's loaded
      }
      while (await reader.ReadAsync(CancellationToken.None))
      {
        _ = reader.GetString(0);
        _ = reader.GetString(1);
        _ = reader.GetString(2);
        _ = reader.GetString(6);
      }
    }

    public async Task TypedGetValue()
    {
      using var reader = new CsvFileReader(
        fileName: _filePathAll, columnDefinition: _cachedColumns, trimmingOption: TrimmingOptionEnum.Unquoted, fieldDelimiterChar: '\t', fieldQualifierChar: '"');

      await reader.OpenAsync(CancellationToken.None);
      for (int i = 0; i < reader.FieldCount; i++)
      {
        _ = reader.GetColumn(i); // Access column metadata to ensure it's loaded
        reader.GetOrdinal(reader.GetName(i)); // Access column metadata to ensure it's loaded
      }
      while (await reader.ReadAsync(CancellationToken.None))
      {

        if (!reader.IsDBNull(0))
          _ = reader.GetDateTime(0);
        if (!reader.IsDBNull(1))
          _ = reader.GetInt32(1);
        if (!reader.IsDBNull(2))
          _ = reader.GetDouble(2);
        if (!reader.IsDBNull(6))
          _ = reader.GetBoolean(6);
      }
    }

    [Benchmark]
    public async Task MemoryBuffer()
    {
      using var stream = new MemoryStream(_fileBytes);
      using var reader = new CsvFileReader(stream: stream, trimmingOption: TrimmingOptionEnum.Unquoted, fieldDelimiterChar: ',', fieldQualifierChar: '"');

      await reader.OpenAsync(CancellationToken.None);
      for (int i = 0; i < reader.FieldCount; i++)
      {
        _ = reader.GetColumn(i); // Access column metadata to ensure it's loaded
        reader.GetOrdinal(reader.GetName(i)); // Access column metadata to ensure it's loaded
      }

      while (await reader.ReadAsync(CancellationToken.None))
      {
        for (int i = 0; i < reader.FieldCount; i++)
          _ = reader.GetString(i);
      }
    }
  }
}
