using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CsvTools.Tests
{
  [TestClass]
	public class WorkWithStream
	{
		[TestMethod]
		public async System.Threading.Tasks.Task AnalyseStreamAsyncFile()
		{
			var stream = FileSystemUtils.OpenRead(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"));

			ICollection<IColumn> determinedColumns;
			// Not closing the stream

			using (IProcessDisplay process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			{
				using (var impStream = new ImprovedStream(stream))
				{
					var result = await impStream.GetDetectionResult("stream", process, false, true, true, true, true, true, false, true);
					impStream.Seek(0, System.IO.SeekOrigin.Begin);

					using (var reader = new CsvFileReader(impStream, result.CodePageId, result.SkipRows, result.HasFieldHeader,
						new ColumnCollection(), TrimmingOption.Unquoted, result.FieldDelimiter, result.FieldQualifier,
						result.EscapeCharacter, 0, false, false, result.CommentLine, 0, true, string.Empty, string.Empty,
						string.Empty, true, false, true, false, false, false, false, false, false, true, true))
					{
						await reader.OpenAsync(process.CancellationToken);
						var (info, columns) = await reader.FillGuessColumnFormatReaderAsyncReader(new FillGuessSettings(),
							new ColumnCollection(), false, true, "null", process.CancellationToken);
						determinedColumns = columns.ToList();
						Assert.AreEqual(6, determinedColumns.Count(), "Recognized columns");
						Assert.AreEqual(6, info.Count, "Information Lines");
					}

					impStream.Seek(0, SeekOrigin.Begin);

					using (var reader = new CsvFileReader(impStream, result.CodePageId, result.SkipRows, result.HasFieldHeader,
						determinedColumns, TrimmingOption.Unquoted, result.FieldDelimiter, result.FieldQualifier,
						result.EscapeCharacter, 0, false, false, result.CommentLine, 0, true, string.Empty, string.Empty,
						string.Empty, true, false, true, false, false, false, false, false, false, true, true))
					{
						await reader.OpenAsync(process.CancellationToken);
						Assert.AreEqual(6, reader.FieldCount);
					}
				}
			}
		}

		[TestMethod]
		public async System.Threading.Tasks.Task AnalyseStreamAsyncGZip()
		{
			using (var stream = FileSystemUtils.OpenRead(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt.gz")))
			{
				ICollection<IColumn> determinedColumns;
				// Not closing the stream
				using (var impStream = new ImprovedStream(stream, SourceAccess.FileTypeEnum.GZip))
				using (IProcessDisplay process = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
				{
					var result = await impStream.GetDetectionResult("steam", process, false, true, true, true, true, true, false, false);

					impStream.Seek(0, System.IO.SeekOrigin.Begin);

					using (var reader = new CsvFileReader(impStream, result.CodePageId, result.SkipRows, result.HasFieldHeader,
						new ColumnCollection(), TrimmingOption.Unquoted, result.FieldDelimiter, result.FieldQualifier,
						result.EscapeCharacter, 0, false, false, result.CommentLine, 0, true, string.Empty, string.Empty,
						string.Empty, true, false, true, false, false, false, false, false, false, true, true))
					{
						await reader.OpenAsync(process.CancellationToken);
						var (info, columns) = await reader.FillGuessColumnFormatReaderAsyncReader(new FillGuessSettings(),
							new ColumnCollection(), false, true, "null", process.CancellationToken);
						determinedColumns = columns.ToList();
						Assert.AreEqual(6, determinedColumns.Count(), "Recognized columns");
						Assert.AreEqual(6, info.Count, "Information Lines");
					}

					impStream.Seek(0, System.IO.SeekOrigin.Begin);

					using (var reader = new CsvFileReader(impStream, result.CodePageId, result.SkipRows, result.HasFieldHeader,
						determinedColumns, TrimmingOption.Unquoted, result.FieldDelimiter, result.FieldQualifier,
						result.EscapeCharacter, 0, false, false, result.CommentLine, 0, true, string.Empty, string.Empty,
						string.Empty, true, false, true, false, false, false, false, false, false, true, true))
					{
						await reader.OpenAsync(process.CancellationToken);
						Assert.AreEqual(6, reader.FieldCount);
					}
				}
			}
		}
	}
}