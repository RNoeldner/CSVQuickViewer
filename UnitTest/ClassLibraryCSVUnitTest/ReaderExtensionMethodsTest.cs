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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
	public class ReaderExtensionMethodsTest
	{
		private readonly CsvFile m_ValidSetting = new CsvFile
		{
			FileName = UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"),
			FileFormat = { FieldDelimiter = ",", CommentLine = "#" }
		};

		[TestInitialize]
		public void Init()
		{
			m_ValidSetting.ColumnCollection.Add(new Column("Score", DataType.Integer));
			m_ValidSetting.ColumnCollection.Add(new Column("Proficiency", DataType.Numeric));
			m_ValidSetting.ColumnCollection.Add(new Column("IsNativeLang", DataType.Boolean));
			var cf = new Column("ExamDate", DataType.DateTime) { ValueFormatMutable = { DateFormat = @"dd/MM/yyyy" } };
			m_ValidSetting.ColumnCollection.Add(cf);
		}

		[TestMethod]
		public async Task GetColumnsOfReaderTest()
		{
			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			using (var test = new CsvFileReader(fileName: UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"),
				fieldDelimiter: ",", commentLine: "#", processDisplay: processDisplay))
			{
				await test.OpenAsync(processDisplay.CancellationToken);
				Assert.AreEqual(6, test.GetColumnsOfReader().Count);
			}
		}

		[TestMethod]
		public async Task GetEmptyColumnHeaderAsyncTest()
		{
			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			{
				using (var test = new CsvFileReader(m_ValidSetting.FullPath, m_ValidSetting.CodePageId, m_ValidSetting.SkipRows, m_ValidSetting.HasFieldHeader, m_ValidSetting.ColumnCollection, m_ValidSetting.TrimmingOption, m_ValidSetting.FileFormat.FieldDelimiter, m_ValidSetting.FileFormat.FieldQualifier, m_ValidSetting.FileFormat.EscapeCharacter, m_ValidSetting.RecordLimit, m_ValidSetting.AllowRowCombining, m_ValidSetting.FileFormat.AlternateQuoting, m_ValidSetting.FileFormat.CommentLine, m_ValidSetting.NumWarnings, m_ValidSetting.FileFormat.DuplicateQuotingToEscape, m_ValidSetting.FileFormat.NewLinePlaceholder, m_ValidSetting.FileFormat.DelimiterPlaceholder, m_ValidSetting.FileFormat.QuotePlaceholder, m_ValidSetting.SkipDuplicateHeader, m_ValidSetting.TreatLFAsSpace, m_ValidSetting.TreatUnknownCharacterAsSpace, m_ValidSetting.TryToSolveMoreColumns, m_ValidSetting.WarnDelimiterInValue, m_ValidSetting.WarnLineFeed, m_ValidSetting.WarnNBSP, m_ValidSetting.WarnQuotes, m_ValidSetting.WarnUnknownCharacter, m_ValidSetting.WarnEmptyTailingColumns, m_ValidSetting.TreatNBSPAsSpace, m_ValidSetting.TreatTextAsNull, m_ValidSetting.SkipEmptyLines, m_ValidSetting.ConsecutiveEmptyRows, m_ValidSetting.IdentifierInContainer, processDisplay))
				{
					await test.OpenAsync(processDisplay.CancellationToken);
					var result = await test.GetEmptyColumnHeaderAsync(processDisplay.CancellationToken);
					Assert.AreEqual(0, result.Count);
				}
			}
		}

		[TestMethod]
		public async Task GetDataTableAsync2()
		{
			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			{
				var test2 = (CsvFile) m_ValidSetting.Clone();
				test2.RecordLimit = 4;
				using (var test = new CsvFileReader(test2.FullPath, test2.CodePageId, test2.SkipRows, test2.HasFieldHeader, test2.ColumnCollection, test2.TrimmingOption, test2.FileFormat.FieldDelimiter, test2.FileFormat.FieldQualifier, test2.FileFormat.EscapeCharacter, test2.RecordLimit, test2.AllowRowCombining, test2.FileFormat.AlternateQuoting, test2.FileFormat.CommentLine, test2.NumWarnings, test2.FileFormat.DuplicateQuotingToEscape, test2.FileFormat.NewLinePlaceholder, test2.FileFormat.DelimiterPlaceholder, test2.FileFormat.QuotePlaceholder, test2.SkipDuplicateHeader, test2.TreatLFAsSpace, test2.TreatUnknownCharacterAsSpace, test2.TryToSolveMoreColumns, test2.WarnDelimiterInValue, test2.WarnLineFeed, test2.WarnNBSP, test2.WarnQuotes, test2.WarnUnknownCharacter, test2.WarnEmptyTailingColumns, test2.TreatNBSPAsSpace, test2.TreatTextAsNull, test2.SkipEmptyLines, test2.ConsecutiveEmptyRows, test2.IdentifierInContainer, processDisplay))
				{
					await test.OpenAsync(processDisplay.CancellationToken);

					DataTable dt = await test.GetDataTableAsync(-1, false, false, false, false, false, null,
						processDisplay.CancellationToken);
					Assert.AreEqual(test2.RecordLimit, dt.Rows.Count);
				}
			}
		}

		[TestMethod]
		public async Task GetDataTableAsync3()
		{
			using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
			{
				var test3 = new CsvFile(UnitTestInitializeCsv.GetTestPath("WithEoFChar.txt"))
				{
					FileFormat = { FieldDelimiter = "Tab" }
				};
				test3.ColumnCollection.Add(new Column("Memo") { Ignore = true });
				using (var test = new CsvFileReader(test3.FullPath, test3.CodePageId, test3.SkipRows, test3.HasFieldHeader, test3.ColumnCollection, test3.TrimmingOption, test3.FileFormat.FieldDelimiter, test3.FileFormat.FieldQualifier, test3.FileFormat.EscapeCharacter, test3.RecordLimit, test3.AllowRowCombining, test3.FileFormat.AlternateQuoting, test3.FileFormat.CommentLine, test3.NumWarnings, test3.FileFormat.DuplicateQuotingToEscape, test3.FileFormat.NewLinePlaceholder, test3.FileFormat.DelimiterPlaceholder, test3.FileFormat.QuotePlaceholder, test3.SkipDuplicateHeader, test3.TreatLFAsSpace, test3.TreatUnknownCharacterAsSpace, test3.TryToSolveMoreColumns, test3.WarnDelimiterInValue, test3.WarnLineFeed, test3.WarnNBSP, test3.WarnQuotes, test3.WarnUnknownCharacter, test3.WarnEmptyTailingColumns, test3.TreatNBSPAsSpace, test3.TreatTextAsNull, test3.SkipEmptyLines, test3.ConsecutiveEmptyRows, test3.IdentifierInContainer, processDisplay))
				{
					await test.OpenAsync(processDisplay.CancellationToken);

					DataTable dt = await test.GetDataTableAsync(-1, true, true, true, true, true, null,
						processDisplay.CancellationToken);
					// 10 columns 1 ignored one added for Start line one for Error Field one for Record No one
					// for Line end
					Assert.AreEqual((10 - 1) + 4, dt.Columns.Count);
					Assert.AreEqual(19, dt.Rows.Count);
				}
			}
		}
	}
}