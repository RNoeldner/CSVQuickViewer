/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
 * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License along with this program.
 * If not, see http://www.gnu.org/licenses/ .
 *
 */

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
	[TestClass]
	public class CsvFileReaderTest
	{
		[TestMethod]
		public async Task OpenBySetting()
		{
			var setting =
				new CsvFile(UnitTestInitializeCsv.GetTestPath("AllFormats.txt"))
				{
					JsonFormat = false,
					HasFieldHeader = true,
					FileFormat = new FileFormat { FieldDelimiter = "Tab" }
				};
			setting.ColumnCollection.AddIfNew(new Column("DateTime", DataType.DateTime));
			setting.ColumnCollection.AddIfNew(new Column("Integer", DataType.Integer));
			using (var dpd = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var reader = new CsvFileReader(setting, dpd))
			{
				await reader.OpenAsync(dpd.CancellationToken);
				Assert.AreEqual(false, reader.IsClosed);

				Assert.AreEqual(10, reader.VisibleFieldCount);
				Assert.IsTrue(reader.Read());
				Assert.IsTrue(reader.IsDBNull(0));
				Assert.IsFalse(await reader.IsDBNullAsync(1));
				Assert.AreEqual(-22477, reader.GetInt32(1));
				Assert.AreEqual("-22477", reader.GetString(1));
				Assert.AreEqual(6, reader.GetStream(1).Length);
				Assert.AreEqual("-22477", reader.GetTextReader(1).ReadToEnd());

				Assert.IsTrue(reader.Read(dpd.CancellationToken));
				Assert.AreEqual(true, reader.HasRows);
				// Read all
				var readEnumerator = reader.GetEnumerator();
				Assert.IsNotNull(readEnumerator);
				while (readEnumerator.MoveNext()) Assert.IsNotNull(readEnumerator.Current);
			}
		}

		[TestMethod]
		public async Task ShouldRetry()
		{
			bool called = false;
			var fn = UnitTestInitializeCsv.GetTestPath("txTranscripts.txt");
			using (var stream = new FileStream(fn.LongPathPrefix(), FileMode.Open, FileAccess.ReadWrite, FileShare.None))
			using (var reader = new CsvFileReader(fn, 650001, 0, true, fieldDelimiter: "\t"))
			{
				// lock file for reading
				reader.OnAskRetry += (sender, args) =>
				{
					stream?.Close();
					called = true;
					args.Retry = true;
				};
				await reader.OpenAsync(UnitTestInitializeCsv.Token);
				Assert.IsFalse(reader.IsClosed);
			}

			Assert.IsTrue(called);
		}

		[TestMethod]
		public async Task OpenByParams()
		{
			using (var reader = new CsvFileReader(UnitTestInitializeCsv.GetTestPath("AllFormats.txt"), 650001, 0, true,
				new IColumn[]
				{
					new ImmutableColumn("DateTime", new ImmutableValueFormat(DataType.DateTime), 0, true, "", true),
					new ImmutableColumn("Integer", new ImmutableValueFormat(DataType.Integer), 0, true, "", true)
				}, TrimmingOption.All))

			{
				await reader.OpenAsync(UnitTestInitializeCsv.Token);
				Assert.AreEqual(false, reader.IsClosed);
				Assert.AreEqual(1, reader.Percent);
				Assert.AreEqual(10 - 2, reader.VisibleFieldCount);
				Assert.IsTrue(reader.Read());

				Assert.AreEqual(true, reader.HasRows);
				while (await reader.ReadAsync(UnitTestInitializeCsv.Token))
				{
				}

				Assert.AreEqual(100, reader.Percent);
			}
		}

		[TestMethod]
		public async Task OpenByStream()
		{
			using (var stream = new ImprovedStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("AllFormats.txt"))))
			using (var reader = new CsvFileReader(stream, 650001, 0, true,
				new IColumn[]
				{
					new ImmutableColumn("DateTime", new ImmutableValueFormat(DataType.DateTime), 0, true, "", true),
					new ImmutableColumn("Integer", new ImmutableValueFormat(DataType.Integer), 0, true, "", true)
				}, TrimmingOption.All))

			{
				await reader.OpenAsync(UnitTestInitializeCsv.Token);
				Assert.AreEqual(false, reader.IsClosed);
				Assert.AreEqual(1, reader.Percent);
				Assert.AreEqual(10 - 2, reader.VisibleFieldCount);
				Assert.IsTrue(reader.Read());

				Assert.AreEqual(true, reader.HasRows);
				while (await reader.ReadAsync(UnitTestInitializeCsv.Token))
				{
				}

				Assert.AreEqual(100, reader.Percent);
			}
		}

		[TestMethod]
		public void InvalidCtor()
		{
			try
			{
				using (var reader = new CsvFileReader(""))
				{
				}
			}
			catch (ArgumentException)
			{
			}
			catch (Exception ex)
			{
				Assert.Fail("Wrong Exception Type: " + ex.GetType());
			}

			try
			{
				using (var reader = new CsvFileReader((string) null))
				{
				}
			}
			catch (ArgumentNullException)
			{
			}
			catch (Exception ex)
			{
				Assert.Fail("Wrong Exception Type: " + ex.GetType());
			}

			try
			{
				using (var reader = new CsvFileReader("(string) null.txt"))
				{
				}
			}
			catch (FileNotFoundException)
			{
			}
			catch (Exception ex)
			{
				Assert.Fail("Wrong Exception Type: " + ex.GetType());
			}

			try
			{
				using (var reader = new CsvFileReader((IImprovedStream) null))
				{
				}
			}
			catch (ArgumentNullException)
			{
			}
			catch (Exception ex)
			{
				Assert.Fail("Wrong Exception Type: " + ex.GetType());
			}
		}

		[TestMethod]
		public async Task NextResult()
		{
			using (var test = new CsvFileReader(UnitTestInitializeCsv.GetTestPath("Sessions.txt"),
				columnDefinition: new IColumn[] { new Column("Start Date", "MM/dd/yyyy") { TimePart = "Start Time", TimePartFormat = "HH:mm:ss" } },
				fieldDelimiter: "\t"))
			{
				Assert.IsFalse(test.NextResult());
				Assert.IsFalse(await test.NextResultAsync());

				await test.OpenAsync(UnitTestInitializeCsv.Token);

				Assert.IsTrue(test.Read(UnitTestInitializeCsv.Token));
			}
		}

		[TestMethod]
		public async Task ReadDateWithTimeAsync()
		{
			var setting = new CsvFile
			{
				FileName = UnitTestInitializeCsv.GetTestPath("Sessions.txt"),
				HasFieldHeader = true,
				ByteOrderMark = true,
				FileFormat = { FieldDelimiter = "\t" }
			};
			setting.ColumnCollection.AddIfNew(new Column("Start Date", "MM/dd/yyyy") { TimePart = "Start Time", TimePartFormat = "HH:mm:ss" });

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				await test.ReadAsync(processDisplay.CancellationToken);
				var cultureInfo = new CultureInfo("en-US");
				Assert.AreEqual("01/08/2013 07:00:00", test.GetDateTime(0).ToString("MM/dd/yyyy HH:mm:ss", cultureInfo));
				await test.ReadAsync(processDisplay.CancellationToken);
				// 01/19/2010 24:00:00 --> 01/20/2010 00:00:00
				Assert.AreEqual("01/20/2010 00:00:00", test.GetDateTime(0).ToString("MM/dd/yyyy HH:mm:ss", cultureInfo));
				await test.ReadAsync(processDisplay.CancellationToken);
				// 01/21/2013 25:00:00 --> 01/22/2013 01:00:00
				Assert.AreEqual("01/22/2013 01:00:00", test.GetDateTime(0).ToString("MM/dd/yyyy HH:mm:ss", cultureInfo));
			}
		}

		[TestMethod]
		public async Task ReadDateWithTimeAndTimeZoneAsync()
		{
			var setting = new CsvFile
			{
				FileName = UnitTestInitializeCsv.GetTestPath("Sessions.txt"),
				HasFieldHeader = true,
				ByteOrderMark = true,
				FileFormat = { FieldDelimiter = "\t" }
			};
			setting.ColumnCollection.AddIfNew(new Column("Start Date", "MM/dd/yyyy")
			{
				TimePart = "Start Time",
				TimePartFormat = "HH:mm:ss",
				TimeZonePart = "Time Zone"
			});

			// all will be converted to TimeZoneInfo.Local, but we concert then to UTC
			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				await test.ReadAsync(processDisplay.CancellationToken);
				var cultureInfo = new CultureInfo("en-US");
				// 01/08/2013 07:00:00 IST --> 01/08/2013 01:30:00 UTC
				Assert.AreEqual("01/08/2013 01:30:00",
					test.GetDateTime(0).ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss", cultureInfo));
				await test.ReadAsync(processDisplay.CancellationToken);
				// 01/19/2010 24:00:00 MST --> 01/20/2010 00:00:00 MST --> 01/20/2010 07:00:00 UTC
				Assert.AreEqual("01/20/2010 07:00:00",
					test.GetDateTime(0).ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss", cultureInfo));
				await test.ReadAsync(processDisplay.CancellationToken);
				// 01/21/2013 25:00:00 GMT --> 01/22/2013 01:00:00 GMT -->01/22/2013 01:00:00 UTC
				Assert.AreEqual("01/22/2013 01:00:00",
					test.GetDateTime(0).ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss", cultureInfo));
			}
		}

		[TestMethod]
		public async Task CsvDataReaderCancellationOnOpenAsync()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				FileFormat = { AlternateQuoting = true },
				TrimmingOption = TrimmingOption.All,
				FileName = UnitTestInitializeCsv.GetTestPath("AlternateTextQualifiers.txt")
			};
			using (var cts = CancellationTokenSource.CreateLinkedTokenSource(UnitTestInitializeCsv.Token))
			using (var processDisplay = new CustomProcessDisplay(cts.Token))
			{
				using (var test = new CsvFileReader(setting, processDisplay))
				{
					await test.OpenAsync(processDisplay.CancellationToken);
					cts.Cancel();
					Assert.IsFalse(await test.ReadAsync(processDisplay.CancellationToken));
				}
			}
		}

		[TestMethod]
		public async Task AlternateTextQualifiersDoubleQuotesAsync()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				FileFormat = { FieldDelimiter = ",", AlternateQuoting = true, DuplicateQuotingToEscape = true },
				FileName = UnitTestInitializeCsv.GetTestPath("AlternateTextQualifiersDoubleQuote.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("This is a \"Test\" of doubled quoted Text", test.GetString(1),
					" \"\"should be regarded as \"");
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("This is a \"Test\" of not repeated quotes", test.GetString(1), "Training space not trimmed");
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				var result = test.GetString(1).HandleCRLFCombinations();
				Assert.AreEqual("Tricky endig duplicated quotes that close...\nLine \"Test\"", result,
					"Ending with two double quotes but one is a closing quote");
			}
		}

		[TestMethod]
		public async Task AlternateTextQualifiersTrimQuoted()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				FileFormat = { FieldDelimiter = ",", AlternateQuoting = true },
				TrimmingOption = TrimmingOption.All,
				FileName = UnitTestInitializeCsv.GetTestPath("AlternateTextQualifiers.txt")
			};
			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("a", test.GetString(0), "Start of file with quote");
				Assert.AreEqual("a", test.GetValue(0), "Start of file with quote");
				Assert.AreEqual("b \"", test.GetString(1), "Containing Quote");
				Assert.AreEqual("d", test.GetString(3), "Leading space not trimmed");
				Assert.AreEqual("This is a\n\" in Quotes", test.GetString(4).HandleCRLFCombinations(),
					"Linefeed in quotes");
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("13", test.GetString(0), "Start of line with quote");
				Assert.AreEqual("This is a \"Test\"", test.GetString(3), "Containing Quote and quote at the end");
				Assert.AreEqual("18", test.GetString(5), "Line ending with quote");

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("19", test.GetString(0), "Training space not trimmed");
				Assert.AreEqual("20,21", test.GetString(1), "Delimiter in quotes");
				Assert.AreEqual("Another\nLine \"Test\"", test.GetString(2).HandleCRLFCombinations(),
					"Linefeed as last char in quotes");
				Assert.AreEqual("22", test.GetString(3), "Leading whitespace before start");
				Assert.AreEqual("24", test.GetString(5), "Leading space not trimmed & EOF");
			}
		}

		[TestMethod]
		public async Task AlternateTextQualifiersTrimUnquoted()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				WarnQuotes = true,
				FileFormat = { FieldDelimiter = ",", AlternateQuoting = true },
				TrimmingOption = TrimmingOption.Unquoted,
				FileName = UnitTestInitializeCsv.GetTestPath("AlternateTextQualifiers.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				var warningList = new RowErrorCollection(test);
				await test.OpenAsync(processDisplay.CancellationToken);
				warningList.HandleIgnoredColumns(test);

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("a", test.GetString(0), "Start of file with quote");
				Assert.AreEqual("b \"  ", test.GetString(1), "Containing Quote");
				Assert.IsTrue(warningList.Display.Contains("Field qualifier"));
				Assert.AreEqual("   d", test.GetString(3), "Leading space not trimmed");
				Assert.AreEqual("This is a\n\" in Quotes", test.GetString(4).HandleCRLFCombinations(),
					"Linefeed in quotes");
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("13", test.GetString(0), "Start of line with quote");
				Assert.AreEqual("This is a \"Test\"", test.GetString(3), "Containing Quote and quote at the end");
				Assert.AreEqual("18", test.GetString(5), "Line ending with quote");

				/*
"19  ","20,21","Another
Line "Test"", "22",23,"  24"
*/
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("19  ", test.GetString(0), "Training space not trimmed");
				Assert.AreEqual("20,21", test.GetString(1), "Delimiter in quotes");
				Assert.AreEqual("Another\nLine \"Test\"", test.GetString(2).HandleCRLFCombinations(),
					"Linefeed as last char in quotes");
				Assert.AreEqual("22", test.GetString(3), "Leading whitespace before start");
				Assert.AreEqual("  24", test.GetString(5), "Leading space not trimmed & EOF");
			}
		}

		[TestMethod]
		public async Task BasicEscapedCharacters()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				FileFormat = { FieldDelimiter = ",", CommentLine = "#", EscapeCharacter = "\\" },
				FileName = UnitTestInitializeCsv.GetTestPath("BasicEscapedCharacters.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("a\"", test.GetString(0), @"a\""");
				Assert.AreEqual("b", test.GetString(1), "b");
				Assert.AreEqual("c", test.GetString(2), "c");
				Assert.AreEqual("d", test.GetString(3), "d");
				Assert.AreEqual("e", test.GetString(4), "e");
				Assert.AreEqual("f", test.GetString(5), "f");
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(",9", test.GetString(2), @"\,9");
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("\\vy", test.GetString(5), @"\\\vy");
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("24\"", test.GetString(5), @"24\""");
				Assert.AreEqual(6, test.FieldCount);
			}
		}

		[TestMethod]
		public async Task TestWarnLinefeed()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				WarnLineFeed = true,
				FileFormat = { CommentLine = "#", EscapeCharacter = "\\", FieldQualifier = "\"", FieldDelimiter = "," },
				TrimmingOption = TrimmingOption.Unquoted,
				FileName = UnitTestInitializeCsv.GetTestPath("ComplexDataDelimiter.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				var message = string.Empty;
				test.Warning += delegate (object sender, WarningEventArgs args) { message = args.Message; };
				await test.ReadAsync(processDisplay.CancellationToken);
				await test.ReadAsync(processDisplay.CancellationToken);
				Assert.IsTrue(message.Contains("Linefeed"));
			}
		}

		[TestMethod]
		public async Task TestHighOccuranceQuestionMark()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				WarnLineFeed = true,
				FileFormat = { CommentLine = "#", EscapeCharacter = "\\", FieldQualifier = "\"", FieldDelimiter = "," },
				TrimmingOption = TrimmingOption.Unquoted,
				FileName = UnitTestInitializeCsv.GetTestPath("ComplexDataDelimiter.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				await test.ReadAsync(processDisplay.CancellationToken);
				await test.ReadAsync(processDisplay.CancellationToken);
				var message = string.Empty;
				test.Warning += delegate (object sender, WarningEventArgs args) { message = args.Message; };
				await test.ReadAsync(processDisplay.CancellationToken);
				Assert.IsTrue(message.Contains("occurrence") && message.Contains("?"));
			}
		}

		[TestMethod]
		public async Task ComplexDataDelimiter()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				FileFormat = { FieldDelimiter = ",", CommentLine = "#", EscapeCharacter = "\\", FieldQualifier = "\"" }
			};
			setting.FileFormat.FieldDelimiter = ",";
			setting.TrimmingOption = TrimmingOption.Unquoted;
			setting.FileName = UnitTestInitializeCsv.GetTestPath("ComplexDataDelimiter.txt");

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(6, test.FieldCount);
				Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
				Assert.AreEqual("5\"", test.GetString(4));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken)); // 2
				Assert.AreEqual("a", test.GetString(0));
				Assert.AreEqual("bta", test.GetString(1));
				Assert.AreEqual("c", test.GetString(2));
				Assert.AreEqual("\"d", test.GetString(3), "\"d");
				Assert.AreEqual("e", test.GetString(4));
				Assert.AreEqual("f\n  , ", test.GetString(5).HandleCRLFCombinations());

				Assert.AreEqual(4, test.StartLineNumber, "StartLineNumber");
				Assert.AreEqual(6, test.EndLineNumber, "EndLineNumber");

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken)); // 3
				Assert.AreEqual(6U, test.StartLineNumber, "StartLineNumber");
				Assert.AreEqual("6\"", test.GetString(5));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken)); // 4
				Assert.AreEqual("k\n", test.GetString(4).HandleCRLFCombinations());
				Assert.AreEqual(9, test.StartLineNumber, "StartLineNumber");

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken)); // 5
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken)); // 6
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken)); // 7
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken)); // 8

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken)); // 9
				Assert.AreEqual("19", test.GetString(0));
				Assert.AreEqual("20", test.GetString(1));
				Assert.AreEqual("21", test.GetString(2));
				Assert.AreEqual("2\"2", test.GetString(3));
				Assert.AreEqual("23", test.GetString(4));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken)); // 10
				Assert.AreEqual("f\n", test.GetString(5).HandleCRLFCombinations());
				Assert.AreEqual(23, test.StartLineNumber, "StartLineNumber");
			}
		}

		[TestMethod]
		public async Task ComplexDataDelimiter2()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				FileFormat = { CommentLine = "#", FieldQualifier = "\"", FieldDelimiter = "," },
				TrimmingOption = TrimmingOption.Unquoted,
				FileName = UnitTestInitializeCsv.GetTestPath("QuoteInText.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(6, test.FieldCount);

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken)); // 1
				Assert.AreEqual("4", test.GetString(3));
				Assert.AreEqual(" 5\"", test.GetString(4));
				Assert.AreEqual("6", test.GetString(5));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken)); // 2
				Assert.AreEqual("21", test.GetString(2));
				Assert.AreEqual("2\"2", test.GetString(3));
				Assert.AreEqual("23", test.GetString(4));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken)); // 3
				Assert.AreEqual("e", test.GetString(4));
				Assert.AreEqual("f\n", test.GetString(5).HandleCRLFCombinations());
			}
		}

		[TestMethod]
		public async Task ComplexDataDelimiterTrimQuotes()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				ConsecutiveEmptyRows = 5,
				FileFormat = { CommentLine = "#", EscapeCharacter = "\\", FieldQualifier = "\"", FieldDelimiter = "," },
				TrimmingOption = TrimmingOption.All,
				FileName = UnitTestInitializeCsv.GetTestPath("ComplexDataDelimiter.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(6, test.FieldCount);

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken)); // 1
				Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
				Assert.AreEqual("1", test.GetString(0));
				Assert.AreEqual("5\"", test.GetString(4));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken)); // 2
				Assert.AreEqual("a", test.GetString(0));
				Assert.AreEqual("bta", test.GetString(1));
				Assert.AreEqual("c", test.GetString(2));
				Assert.AreEqual("\"d", test.GetString(3), "\"d");
				Assert.AreEqual("e", test.GetString(4));
				Assert.AreEqual("f\n  ,", test.GetString(5).HandleCRLFCombinations());
				// Line streches over two line both are fine
				Assert.AreEqual(4, test.StartLineNumber, "LineNumber");

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken)); // 3
				Assert.AreEqual(6U, test.StartLineNumber, "LineNumber");
				Assert.AreEqual("6\"", test.GetString(5));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken)); // 4
				Assert.AreEqual("g", test.GetString(0));
				Assert.AreEqual("k", test.GetString(4));
				Assert.AreEqual(9, test.StartLineNumber, "LineNumber");

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken)); // 5
				Assert.AreEqual("7", test.GetString(0));
				Assert.AreEqual(11, test.StartLineNumber, "LineNumber");

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken)); // 6
				Assert.AreEqual("m", test.GetString(0));
				Assert.AreEqual(12, test.StartLineNumber, "LineNumber");

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken)); // 7
				Assert.AreEqual("13", test.GetString(0));
				Assert.AreEqual(17, test.StartLineNumber, "LineNumber");

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken)); // 8
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken)); // 9
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken)); // 10
				Assert.AreEqual("f", test.GetString(5));
				Assert.AreEqual(23, test.StartLineNumber, "LineNumber");
			}
		}

		[TestMethod]
		public async Task CSVTestEmpty()
		{
			var setting = new CsvFile { HasFieldHeader = false, ByteOrderMark = true, FileName = UnitTestInitializeCsv.GetTestPath("CSVTestEmpty.txt") };

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(0, test.FieldCount);
				Assert.IsFalse(await test.ReadAsync(processDisplay.CancellationToken));
			}
		}

		[TestMethod]
		public async Task DifferentColumnDelimiter()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				FileFormat = { FieldDelimiter = "PIPE" },
				FileName = UnitTestInitializeCsv.GetTestPath("DifferentColumnDelimiter.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(6, test.FieldCount);
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
				Assert.AreEqual("a", test.GetString(0));
				Assert.AreEqual("f", test.GetString(5));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("19", test.GetString(0));
				Assert.AreEqual("24", test.GetString(5));
				Assert.AreEqual(8U, test.StartLineNumber, "LineNumber");
				Assert.IsFalse(await test.ReadAsync(processDisplay.CancellationToken));
			}
		}

		[TestMethod]
		public async Task EscapedCharacterAtEndOfFile()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				FileFormat = { FieldDelimiter = ",", EscapeCharacter = "\\" },
				FileName = UnitTestInitializeCsv.GetTestPath("EscapedCharacterAtEndOfFile.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);

				Assert.AreEqual(6, test.FieldCount);
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("a\"", test.GetString(0), @"a\""");
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(",9", test.GetString(2), @"\,9");
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("\\vy", test.GetString(5), @"\\\vy");
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("24", test.GetString(5), @"24\");
			}
		}

		[TestMethod]
		public async Task EscapedCharacterAtEndOfRowDelimiter()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				TrimmingOption = TrimmingOption.None,
				TreatLFAsSpace = false,
				FileFormat = { FieldDelimiter = ",", EscapeCharacter = "\\" },
				FileName = UnitTestInitializeCsv.GetTestPath("EscapedCharacterAtEndOfRowDelimiter.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(6, test.FieldCount, "FieldCount");
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("l", test.GetString(5)); // Important to not trim the value otherwise the linefeed is gone
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("7", test.GetString(0), @"Next Row");
				Assert.AreEqual(4U, test.StartLineNumber, "StartLineNumber");
				Assert.AreEqual(4U, test.RecordNumber, "RecordNumber");
			}
		}

		[TestMethod]
		public async Task EscapedCharacterAtEndOfRowDelimiterNoEscape()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				FileFormat = { FieldDelimiter = ",", EscapeCharacter = "" },
				FileName = UnitTestInitializeCsv.GetTestPath("EscapedCharacterAtEndOfRowDelimiter.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);

				Assert.AreEqual(6, test.FieldCount);
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(@"l\", test.GetString(5), @"l\");
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("7", test.GetString(0), @"Next Row");
				Assert.AreEqual(4U, test.StartLineNumber);
				Assert.AreEqual(4U, test.RecordNumber);
			}
		}

		[TestMethod]
		public async Task EscapeWithoutTextQualifier()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				FileFormat = { FieldDelimiter = ",", EscapeCharacter = "\\", FieldQualifier = string.Empty },
				FileName = UnitTestInitializeCsv.GetTestPath("EscapeWithoutTextQualifier.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(6, test.FieldCount);
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(@"a\", test.GetString(0), @"a\\");
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(",9", test.GetString(2), @"\,9");
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("\\vy", test.GetString(5), @"\\\vy");
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("24\\", test.GetString(5), @"24\\");
			}
		}

		[TestMethod]
		public async Task HandlingDuplicateColumnNames()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = true,
				FileFormat = { FieldDelimiter = "," },
				FileName = UnitTestInitializeCsv.GetTestPath("HandlingDuplicateColumnNames.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				var message = string.Empty;
				test.Warning += delegate (object sender, WarningEventArgs args) { message = args.Message; };
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.IsTrue(message.Contains("exists more than once"));
			}
		}

		[TestMethod]
		public async Task LastRowWithRowDelimiter()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = true,
				FileFormat = { FieldDelimiter = "," },
				FileName = UnitTestInitializeCsv.GetTestPath("LastRowWithRowDelimiter.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(6, test.FieldCount);

				Assert.AreEqual("a", test.GetName(0));
				Assert.AreEqual("f", test.GetName(5));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(2U, test.StartLineNumber, "LineNumber");
				Assert.AreEqual("1", test.GetString(0));
				Assert.AreEqual("6", test.GetString(5));
			}
		}

		[TestMethod]
		public async Task LastRowWithRowDelimiterNoHeader()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				FileFormat = { FieldDelimiter = "," },
				FileName = UnitTestInitializeCsv.GetTestPath("LastRowWithRowDelimiter.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);

				Assert.AreEqual(6, test.FieldCount);

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
				Assert.AreEqual("a", test.GetString(0));
				Assert.AreEqual("f", test.GetString(5));
			}
		}

		[TestMethod]
		public async Task LongHeaders()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = true,
				FileFormat = { FieldDelimiter = ",", CommentLine = "#" },
				FileName = UnitTestInitializeCsv.GetTestPath("LongHeaders.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				var warningsList = new RowErrorCollection(test);
				await test.OpenAsync(processDisplay.CancellationToken);

				Assert.AreEqual(6, test.FieldCount);
				Assert.AreEqual("a", test.GetName(0));
				Assert.AreEqual("b", test.GetName(1));
				Assert.AreEqual(
					"Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquya"
						.Substring(0, 128), test.GetName(2));
				Assert.AreEqual("d", test.GetName(3));
				Assert.AreEqual("e", test.GetName(4));
				Assert.AreEqual("f", test.GetName(5));
				Assert.AreEqual(1, warningsList.CountRows, "Warnings");
				Assert.IsTrue(warningsList.Display.Contains("too long"));

				// check if we read the right line , and we do not end up in a commented line of read the
				// header again
				await test.ReadAsync(processDisplay.CancellationToken);
				Assert.AreEqual("1", test.GetString(0));
				Assert.AreEqual("6", test.GetString(5));
			}
		}

		[TestMethod]
		public async Task MoreColumnsThanHeaders()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				WarnEmptyTailingColumns = true,
				FileFormat = { FieldDelimiter = "," },
				FileName = UnitTestInitializeCsv.GetTestPath("MoreColumnsThanHeaders.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				var warningList = new RowErrorCollection(test);
				await test.OpenAsync(processDisplay.CancellationToken);

				warningList.HandleIgnoredColumns(test);

				Assert.AreEqual(6, test.FieldCount);
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
				Assert.AreEqual("a", test.GetString(0));
				Assert.AreEqual("f", test.GetString(5));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(0, warningList.CountRows);

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				// Still only 6 fields
				Assert.AreEqual(6, test.FieldCount);

				Assert.AreEqual(1, warningList.CountRows, "warningList.CountRows");
				Assert.IsTrue(warningList.Display.Contains(CsvFileReader.cMoreColumns));
				// Assert.IsTrue(warningList.Display.Contains("The existing data in these extra columns is
				// not read"));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));

				// Row 3 and Row 4 have extra columns
				Assert.AreEqual(2, warningList.CountRows, "warningList.CountRows");
			}
		}

		[TestMethod]
		public async Task NoFieldQualifier()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				FileFormat = { FieldDelimiter = ",", FieldQualifier = string.Empty },
				FileName = UnitTestInitializeCsv.GetTestPath("TextQualifierDataPastClosingQuote.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(6, test.FieldCount);
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				//"a"a,b,c,d,e,f
				Assert.AreEqual("\"a\"a", test.GetString(0));
				Assert.AreEqual("b", test.GetString(1));
				Assert.AreEqual("c", test.GetString(2));
				Assert.AreEqual("d", test.GetString(3));
				Assert.AreEqual("e", test.GetString(4));
				Assert.AreEqual("f", test.GetString(5));
				//1,2,"3" ignore me,4,5,6
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("1", test.GetString(0));
				Assert.AreEqual("2", test.GetString(1));
				Assert.AreEqual("\"3\" ignore me", test.GetString(2));
				Assert.AreEqual("4", test.GetString(3));
				Assert.AreEqual("5", test.GetString(4));
				Assert.AreEqual("6", test.GetString(5));
				Assert.IsFalse(await test.ReadAsync(processDisplay.CancellationToken));
			}
		}

		[TestMethod]
		public async Task PlaceHolderTest()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = true,
				FileFormat =
				{
					FieldDelimiter = ",",
					EscapeCharacter = string.Empty,
					DelimiterPlaceholder = @"<\d>",
					QuotePlaceholder = @"<\q>",
					NewLinePlaceholder = @"<\r>"
				},
				FileName = UnitTestInitializeCsv.GetTestPath("Placeholder.txt")
			};
			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("A \r\nLine\r\nBreak", test.GetString(1));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("Two ,Delimiter,", test.GetString(1));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("Two \"Quote\"", test.GetString(1));
			}
		}

		[TestMethod]
		public async Task ProcessDisplayUpdateShowProgress()
		{
			var setting = UnitTestHelper.ReaderGetAllFormats(null);
			var processDisplay = new MockProcessDisplay();
			var stopped = false;
			processDisplay.ProgressStopEvent += delegate { stopped = true; };
			Assert.AreEqual(null, processDisplay.Text);
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);

				for (var i = 0; i < 500; i++)
					Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
			}

			Assert.AreNotEqual(null, processDisplay.Text);
			Assert.IsFalse(stopped);
		}

		[TestMethod]
		public async Task ReadingInHeaderAfterComments()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = true,
				FileFormat = { CommentLine = "#" },
				FileName = UnitTestInitializeCsv.GetTestPath("ReadingInHeaderAfterComments.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);

				Assert.AreEqual(6, test.FieldCount);
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(4U, test.StartLineNumber, "LineNumber");
				Assert.AreEqual("1", test.GetString(0));
				Assert.AreEqual("6", test.GetString(5));
			}
		}

		[TestMethod]
		public async Task RowWithoutColumnDelimiter()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = true,
				FileFormat = { FieldDelimiter = "," },
				FileName = UnitTestInitializeCsv.GetTestPath("RowWithoutColumnDelimiter.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(1, test.FieldCount);
				Assert.AreEqual("abcdef", test.GetName(0));
				Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(2U, test.StartLineNumber, "LineNumber");
				Assert.AreEqual("123456", test.GetString(0));
			}
		}

		[TestMethod]
		public async Task SimpleDelimiterWithControlCharacters()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = true,
				FileFormat = { FieldDelimiter = "," },
				FileName = UnitTestInitializeCsv.GetTestPath("SimpleDelimiterWithControlCharacters.txt")
			};
			setting.FileFormat.CommentLine = "#";
			setting.WarnNBSP = true;
			setting.WarnUnknownCharacter = true;

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				var warningList = new RowErrorCollection(test);
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(6, test.FieldCount);
				Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("a", test.GetString(0));
				Assert.AreEqual("b", test.GetString(1));
				Assert.AreEqual("c", test.GetString(2));
				Assert.AreEqual("d", test.GetString(3));
				Assert.AreEqual(2U, test.StartLineNumber, "LineNumber");
				// NBSP and � past " so not in the field
				Assert.IsFalse(warningList.Display.Contains("Non Breaking Space"));
				Assert.IsFalse(warningList.Display.Contains("Unknown Character"));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(3U, test.StartLineNumber, "LineNumber");
				// g,h,i , j,k,l
				Assert.AreEqual("i", test.GetString(2));
				Assert.AreEqual("j", test.GetString(3));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("7", test.GetString(0));
				// 10 has a training NBSp;
				Assert.AreEqual("10", test.GetString(3));
				Assert.AreEqual(1, warningList.CountRows);
				Assert.IsTrue(warningList.Display.Contains("Non Breaking Space"));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				// m,n,o,p ,q,r
				Assert.AreEqual("p", test.GetString(3));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("15�", test.GetString(2));
				Assert.IsTrue(warningList.Display.Contains("Unknown Character"));
			}
		}

		[TestMethod]
		public async Task SimpleDelimiterWithControlCharactersUnknowCharaterReplacement()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = true,
				TreatUnknownCharacterAsSpace = true,
				TreatNBSPAsSpace = true,
				WarnNBSP = true,
				WarnUnknownCharacter = true,
				TrimmingOption = TrimmingOption.None,
				FileFormat = { FieldDelimiter = "," },
				FileName = UnitTestInitializeCsv.GetTestPath("SimpleDelimiterWithControlCharacters.txt")
			};
			setting.FileFormat.CommentLine = "#";

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				var warningList = new RowErrorCollection(test);
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(6, test.FieldCount);
				Assert.AreEqual("1", test.GetName(0));
				Assert.AreEqual("2", test.GetName(1));
				Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(3U, test.StartLineNumber, "LineNumber");
				// g,h,i , j,k,l
				Assert.AreEqual(" j", test.GetString(3));

				// #A NBSP: Create with Alt+01602 7,8,9,10 ,11,12
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("7", test.GetString(0));
				Assert.AreEqual("10 ", test.GetString(3));
				Assert.AreEqual(2, warningList.CountRows);
				Assert.IsTrue(warningList.Display.Contains("Non Breaking Space"));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				// m,n,o,p ,q,r
				Assert.AreEqual("p						", test.GetString(3));

				// 13,14,15�,16,17,18
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("15 ", test.GetString(2));
				Assert.AreEqual(3, warningList.CountRows);
				Assert.IsTrue(warningList.Display.Contains("Unknown Character"));
			}
		}

		[TestMethod]
		public async Task SkipAllRows()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				FileFormat = { FieldDelimiter = "," },
				SkipRows = 100,
				FileName = UnitTestInitializeCsv.GetTestPath("BasicCSV.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(0, test.FieldCount);
			}
		}

		[TestMethod]
		public async Task SkippingComments()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				FileFormat = { FieldDelimiter = "," },
				FileName = UnitTestInitializeCsv.GetTestPath("SkippingComments.txt")
			};
			setting.FileFormat.CommentLine = "#";
			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(6, test.FieldCount);
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(2U, test.StartLineNumber, "LineNumber");
				Assert.AreEqual(1U, test.RecordNumber, "RecordNumber");
				Assert.AreEqual("a", test.GetString(0));
				Assert.AreEqual("f", test.GetString(5));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(2U, test.RecordNumber, "RecordNumber");
				Assert.AreEqual(4U, test.StartLineNumber, "LineNumber");

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(3U, test.RecordNumber, "RecordNumber");
				Assert.AreEqual(5U, test.StartLineNumber, "LineNumber");

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(4U, test.RecordNumber, "RecordNumber");
				Assert.AreEqual(6U, test.StartLineNumber, "LineNumber");

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(5U, test.RecordNumber, "RecordNumber");
				Assert.AreEqual(7U, test.StartLineNumber, "LineNumber");

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(6U, test.RecordNumber, "RecordNumber");
				Assert.AreEqual(10U, test.StartLineNumber, "LineNumber");

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(7U, test.RecordNumber, "RecordNumber");
				Assert.AreEqual(11U, test.StartLineNumber, "LineNumber");

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(8U, test.RecordNumber, "RecordNumber");
				Assert.AreEqual(13U, test.StartLineNumber, "LineNumber");

				Assert.IsFalse(await test.ReadAsync(processDisplay.CancellationToken));
			}
		}

		[TestMethod]
		public async Task SkippingEmptyRowsWithDelimiter()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				FileFormat = { FieldDelimiter = "," },
				FileName = UnitTestInitializeCsv.GetTestPath("SkippingEmptyRowsWithDelimiter.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(6, test.FieldCount, "FieldCount");
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(3U, test.StartLineNumber, "LineNumber");
				Assert.AreEqual("a", test.GetString(0));
				Assert.AreEqual("b", test.GetString(1));
				Assert.AreEqual("f", test.GetString(5));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(14U, test.StartLineNumber, "LineNumber");
				Assert.AreEqual("19", test.GetString(0));
				// No next Line
				Assert.IsFalse(await test.ReadAsync(processDisplay.CancellationToken));
			}
		}

		[TestMethod]
		public async Task GetValue()
		{
			var setting = new CsvFile { HasFieldHeader = true, FileFormat = { FieldDelimiter = "," }, FileName = UnitTestInitializeCsv.GetTestPath("BasicCSV.txt") };
			setting.ColumnCollection.AddIfNew(new Column("ExamDate", @"dd/MM/yyyy"));
			setting.ColumnCollection.AddIfNew(new Column("ID", DataType.Integer));
			setting.ColumnCollection.AddIfNew(new Column("IsNativeLang", DataType.Boolean));

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(6, test.FieldCount);
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(new DateTime(2010, 1, 20), test.GetValue(2));
				Assert.AreEqual(true, test.GetValue(5));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));

				Assert.AreEqual(Convert.ToInt32(2), Convert.ToInt32(test.GetValue(0), CultureInfo.InvariantCulture));
				Assert.AreEqual("English", test.GetString(1));
				Assert.AreEqual("22/01/2012", test.GetString(2));
				Assert.AreEqual(new DateTime(2012, 1, 22), test.GetValue(2));
				Assert.AreEqual(false, test.GetValue(5));
			}
		}

		[TestMethod]
		public async Task SkipRows()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				FileFormat = { FieldDelimiter = "," },
				SkipRows = 2,
				FileName = UnitTestInitializeCsv.GetTestPath("BasicCSV.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(6, test.FieldCount);
				// Start at line 2
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(3U, test.StartLineNumber, "LineNumber");
				Assert.AreEqual(1U, test.RecordNumber, "RecordNumber");

				Assert.AreEqual("English", test.GetString(1));
				Assert.AreEqual("22/01/2012", test.GetString(2));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(4U, test.StartLineNumber, "LineNumber");
				Assert.AreEqual(2U, test.RecordNumber, "RecordNumber");

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(5U, test.StartLineNumber, "LineNumber");
				Assert.AreEqual(3U, test.RecordNumber, "RecordNumber");

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(6U, test.StartLineNumber, "LineNumber");
				Assert.AreEqual(4U, test.RecordNumber, "RecordNumber");
			}
		}

		[TestMethod]
		public async Task TestConsecutiveEmptyRows2()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = true,
				ConsecutiveEmptyRows = 2,
				SkipEmptyLines = false,
				FileFormat = { FieldDelimiter = "," },
				FileName = UnitTestInitializeCsv.GetTestPath("BasicCSVEmptyLine.txt")
			};
			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(6, test.FieldCount);
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken), "Read() 1");

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken), "Read() 2");
				Assert.AreEqual("20/01/2010", test.GetString(2));
				// First empty Row, continue
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken), "Read() 3");
				// Second Empty Row, Stop
				Assert.IsFalse(await test.ReadAsync(processDisplay.CancellationToken), "Read() 4");
			}
		}

		[TestMethod]
		public async Task TestConsecutiveEmptyRows3()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = true,
				ConsecutiveEmptyRows = 3,
				SkipEmptyLines = false,
				FileFormat = { FieldDelimiter = "," },
				FileName = UnitTestInitializeCsv.GetTestPath("BasicCSVEmptyLine.txt")
			};
			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(6, test.FieldCount);
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("20/01/2010", test.GetString(2));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("22/01/2012", test.GetString(2));
				// No other Line read they are all empty
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsFalse(await test.ReadAsync(processDisplay.CancellationToken));
			}
		}

		[TestMethod]
		public async Task TextQualifierBeginningAndEnd()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				FileFormat = { FieldDelimiter = ",", EscapeCharacter = "\\" },
				FileName = UnitTestInitializeCsv.GetTestPath("TextQualifierBeginningAndEnd.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(5, test.FieldCount);
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
				// a,b",c"c,d""d,"e""e",""f
				Assert.AreEqual("a", test.GetString(0));
				Assert.AreEqual("b\"", test.GetString(1));
				Assert.AreEqual("c\"c", test.GetString(2));
				Assert.AreEqual("d\"\"d", test.GetString(3));
				Assert.AreEqual("e\"e", test.GetString(4));

				//"g,h,i"",j,k,l"
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(2U, test.StartLineNumber, "LineNumber");
				Assert.AreEqual("g,h,i\",j,k,l", test.GetString(0));
				// "m",n\"op\"\"qr,"s\"tu\"\"vw",\"x""""""
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(3U, test.StartLineNumber, "LineNumber");
				Assert.AreEqual("m", test.GetString(0));
				Assert.AreEqual("n\"op\"\"qr", test.GetString(1));
				Assert.AreEqual("s\"tu\"\"vw", test.GetString(2));
				Assert.AreEqual("\"x\"\"\"\"\"\"", test.GetString(3));
			}
		}

		[TestMethod]
		public async Task TextQualifierDataPastClosingQuote()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				FileFormat = { FieldDelimiter = "," },
				FileName = UnitTestInitializeCsv.GetTestPath("TextQualifierDataPastClosingQuote.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(6, test.FieldCount);
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				//"a"a,b,c,d,e,f
				Assert.AreEqual("a", test.GetString(0));
				Assert.AreEqual("b", test.GetString(1));
				Assert.AreEqual("c", test.GetString(2));
				Assert.AreEqual("d", test.GetString(3));
				Assert.AreEqual("e", test.GetString(4));
				Assert.AreEqual("f", test.GetString(5));
				//1,2,"3" ignore me,4,5,6
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("1", test.GetString(0));
				Assert.AreEqual("2", test.GetString(1));
				Assert.AreEqual("3", test.GetString(2));
				Assert.AreEqual("4", test.GetString(3));
				Assert.AreEqual("5", test.GetString(4));
				Assert.AreEqual("6", test.GetString(5));
				Assert.IsFalse(await test.ReadAsync(processDisplay.CancellationToken));
			}
		}

		[TestMethod]
		public async Task TextQualifierNotClosedAtEnd()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				FileFormat = { FieldDelimiter = ",", EscapeCharacter = "\\" },
				FileName = UnitTestInitializeCsv.GetTestPath("TextQualifierNotClosedAtEnd.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(6, test.FieldCount);
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
				// "a","b ",c," , d",e,f
				Assert.AreEqual("a", test.GetString(0));
				Assert.AreEqual("b   ", test.GetString(1));
				Assert.AreEqual("c", test.GetString(2));
				Assert.AreEqual(" ,  d", test.GetString(3));
				Assert.AreEqual("e", test.GetString(4));
				Assert.AreEqual("f", test.GetString(5));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(4U, test.StartLineNumber, "LineNumber");
				//7," ,8, ",9,10,11,12
				Assert.AreEqual("7", test.GetString(0));
				Assert.AreEqual(" ,8, ", test.GetString(1));
				Assert.AreEqual("9", test.GetString(2));
				Assert.AreEqual("10", test.GetString(3));
				Assert.AreEqual("11", test.GetString(4));
				Assert.AreEqual("12", test.GetString(5));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(test.StartLineNumber >= 8 && test.StartLineNumber <= 9, "LineNumber");
				//"19 , ",20,21,22,23,"
				Assert.AreEqual("19 , ", test.GetString(0));
				Assert.AreEqual("20", test.GetString(1));
				Assert.AreEqual("21", test.GetString(2));
				Assert.AreEqual("22", test.GetString(3));
				Assert.AreEqual("23", test.GetString(4));
				//Assert.AreEqual(null, test.GetString(5));
				Assert.AreEqual(" \n 24", test.GetString(5).HandleCRLFCombinations());
			}
		}

		[TestMethod]
		public async Task TextQualifiers()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				FileFormat = { FieldDelimiter = "," },
				FileName = UnitTestInitializeCsv.GetTestPath("TextQualifiers.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(6, test.FieldCount);
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("a", test.GetString(0));
				Assert.AreEqual("b \"  ", test.GetString(1));
				Assert.AreEqual("c", test.GetString(2));
				Assert.AreEqual("   d", test.GetString(3));
				Assert.AreEqual("e", test.GetString(4));
				Assert.AreEqual("f", test.GetString(5));
				//1,2,3,4,5,6
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual("1", test.GetString(0));
				Assert.AreEqual("2", test.GetString(1));
				Assert.AreEqual("3", test.GetString(2));
				Assert.AreEqual("4", test.GetString(3));
				Assert.AreEqual("5", test.GetString(4));
				Assert.AreEqual("6", test.GetString(5));
			}
		}

		[TestMethod]
		public async Task TextQualifiersWithDelimiters()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				WarnDelimiterInValue = true,
				FileFormat = { FieldDelimiter = ",", EscapeCharacter = "\\" },
				FileName = UnitTestInitializeCsv.GetTestPath("TextQualifiersWithDelimiters.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				var warningList = new RowErrorCollection(test);
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(6, test.FieldCount);
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
				// "a","b ",c," , d",e,f
				Assert.AreEqual("a", test.GetString(0));
				Assert.AreEqual("b   ", test.GetString(1));
				Assert.AreEqual("c", test.GetString(2));
				Assert.AreEqual(" ,  d", test.GetString(3));
				Assert.AreEqual("e", test.GetString(4));
				Assert.AreEqual("f", test.GetString(5));
				Assert.AreEqual(1, warningList.CountRows, "Warnings count");
				Assert.IsTrue(warningList.Display.Contains("Field delimiter"));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(4U, test.StartLineNumber, "LineNumber");
				//7," ,8, ",9,10,11,12
				Assert.AreEqual("7", test.GetString(0));
				Assert.AreEqual(" ,8, ", test.GetString(1));
				Assert.AreEqual("9", test.GetString(2));
				Assert.AreEqual("10", test.GetString(3));
				Assert.AreEqual("11", test.GetString(4));
				Assert.AreEqual("12", test.GetString(5));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.IsTrue(test.StartLineNumber >= 8 && test.StartLineNumber <= 9, "LineNumber");

				Assert.AreEqual("19 , ", test.GetString(0));
				Assert.AreEqual("20", test.GetString(1));
				Assert.AreEqual("21", test.GetString(2));
				Assert.AreEqual("22", test.GetString(3));
				Assert.AreEqual("23", test.GetString(4));
				Assert.AreEqual(" \n 24", test.GetString(5).HandleCRLFCombinations());
			}
		}

		[TestMethod]
		public async Task TrimmingHeaders()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = true,
				FileFormat = { FieldDelimiter = ",", CommentLine = "#" },
				FileName = UnitTestInitializeCsv.GetTestPath("TrimmingHeaders.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				var warningList = new RowErrorCollection(test);
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(6, test.FieldCount);
				Assert.AreEqual("a", test.GetName(0));
				Assert.AreEqual("b", test.GetName(1));
				Assert.AreEqual("c", test.GetName(2));
				Assert.AreEqual("d", test.GetName(3));
				Assert.AreEqual("Column5", test.GetName(4));
				Assert.AreEqual("f", test.GetName(5));
				Assert.IsTrue(warningList.CountRows >= 1);
				Assert.IsTrue(warningList.Display.Contains("leading or tailing spaces"));
			}
		}

		[TestMethod]
		public async Task UnicodeUTF16BE()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				CodePageId = 1201,
				ByteOrderMark = true,
				FileFormat = { FieldDelimiter = "," },
				FileName = UnitTestInitializeCsv.GetTestPath("UnicodeUTF16BE.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);

				Assert.AreEqual(4, test.FieldCount);
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
				Assert.AreEqual("tölvuiðnaðarins", test.GetString(1));
				Assert.AreEqual("ũΩ₤", test.GetString(2));
				Assert.AreEqual("používat", test.GetString(3));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(2U, test.StartLineNumber, "LineNumber");

				Assert.AreEqual("různá", test.GetString(0));
				Assert.AreEqual("čísla", test.GetString(1));
				Assert.AreEqual("pro", test.GetString(2));
				Assert.AreEqual("członkowskich", test.GetString(3));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(3U, test.StartLineNumber, "LineNumber");

				Assert.AreEqual("rozumieją", test.GetString(0));
				Assert.AreEqual("přiřazuje", test.GetString(1));
				Assert.AreEqual("gemeinnützige", test.GetString(2));
				Assert.AreEqual("är också", test.GetString(3));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(4U, test.StartLineNumber, "LineNumber");

				Assert.AreEqual("sprachunabhängig", test.GetString(0));
				Assert.AreEqual("that's all", test.GetString(1));
				Assert.AreEqual("for", test.GetString(2));
				Assert.AreEqual("now", test.GetString(3));

				Assert.IsFalse(await test.ReadAsync(processDisplay.CancellationToken));
			}
		}

		[TestMethod]
		public async Task UnicodeUTF16LE()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				CodePageId = 1200,
				ByteOrderMark = true,
				FileFormat = { FieldDelimiter = "," },
				FileName = UnitTestInitializeCsv.GetTestPath("UnicodeUTF16LE.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(4, test.FieldCount);
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");
				Assert.AreEqual("tölvuiðnaðarins", test.GetString(1));
				Assert.AreEqual("ũΩ₤", test.GetString(2));
				Assert.AreEqual("používat", test.GetString(3));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(2U, test.StartLineNumber, "LineNumber");
				Assert.AreEqual("různá", test.GetString(0));
				Assert.AreEqual("čísla", test.GetString(1));
				Assert.AreEqual("pro", test.GetString(2));
				Assert.AreEqual("członkowskich", test.GetString(3));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(3U, test.StartLineNumber, "LineNumber");
				Assert.AreEqual("rozumieją", test.GetString(0));
				Assert.AreEqual("přiřazuje", test.GetString(1));
				Assert.AreEqual("gemeinnützige", test.GetString(2));
				Assert.AreEqual("är också", test.GetString(3));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(4U, test.StartLineNumber, "LineNumber");
				Assert.AreEqual("sprachunabhängig", test.GetString(0));
				Assert.AreEqual("that's all", test.GetString(1));
				Assert.AreEqual("for", test.GetString(2));
				Assert.AreEqual("now", test.GetString(3));

				Assert.IsFalse(await test.ReadAsync(processDisplay.CancellationToken));
			}
		}

		[TestMethod]
		public async Task TestStartRowAndFooter()
		{
			using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("LateStartRow.txt"))))
				Assert.AreEqual(10, await CsvHelper.GuessStartRowFromStream(improvedStream, 20127, '|', '"', "#", UnitTestInitializeCsv.Token));
		}

		[TestMethod]
		public async Task UnicodeUTF8()
		{
			var setting = new CsvFile
			{
				HasFieldHeader = false,
				CodePageId = 65001,
				FileFormat = { FieldDelimiter = "," },
				FileName = UnitTestInitializeCsv.GetTestPath("UnicodeUTF8.txt")
			};

			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(setting, processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(Encoding.UTF8, setting.CurrentEncoding);
				Assert.AreEqual(4, test.FieldCount);
				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(1U, test.StartLineNumber, "LineNumber");

				Assert.AreEqual("tölvuiðnaðarins", test.GetString(1));
				Assert.AreEqual("ũΩ₤", test.GetString(2));
				Assert.AreEqual("používat", test.GetString(3));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(2U, test.StartLineNumber, "LineNumber");

				Assert.AreEqual("různá", test.GetString(0));
				Assert.AreEqual("čísla", test.GetString(1));
				Assert.AreEqual("pro", test.GetString(2));
				Assert.AreEqual("członkowskich", test.GetString(3));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(3U, test.StartLineNumber, "LineNumber");

				Assert.AreEqual("rozumieją", test.GetString(0));
				Assert.AreEqual("přiřazuje", test.GetString(1));
				Assert.AreEqual("gemeinnützige", test.GetString(2));
				Assert.AreEqual("är också", test.GetString(3));

				Assert.IsTrue(await test.ReadAsync(processDisplay.CancellationToken));
				Assert.AreEqual(4U, test.StartLineNumber, "LineNumber");

				Assert.AreEqual("sprachunabhängig", test.GetString(0));
				Assert.AreEqual("that's all", test.GetString(1));
				Assert.AreEqual("for", test.GetString(2));
				Assert.AreEqual("now", test.GetString(3));

				Assert.IsFalse(await test.ReadAsync(processDisplay.CancellationToken));
			}
		}
	}
}