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
#nullable enable

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace CsvTools.Tests
{
  public static class UnitTestStatic
	{
		private static readonly Random m_Random = new Random(Guid.NewGuid().GetHashCode());

		public static void AllPropertiesEqual(this object a, object b)
		{
			var properties = a.GetType().GetProperties().Where(prop => prop.GetMethod != null &&
																																 (prop.PropertyType == typeof(int) ||
																																	prop.PropertyType == typeof(long) ||
																																	prop.PropertyType == typeof(string) ||
																																	prop.PropertyType == typeof(bool) ||
																																	prop.PropertyType == typeof(DateTime)));
			foreach (var prop in properties)
				Assert.AreEqual(prop.GetValue(a), prop.GetValue(b),
					$"Type: {a.GetType().FullName}\nProperty:{prop.Name}\nValue A:{prop.GetValue(a)}\nnValue B:{prop.GetValue(b)}");
		}

		public static CsvFile ReaderGetAllFormats(string id = "AllFormats")
		{
			var readFile = new CsvFile
			{
				ID = id,
				FileName = Path.Combine(UnitTestInitializeCsv.GetTestPath("AllFormats.txt")),
				HasFieldHeader = true,
				FileFormat = { FieldDelimiter = "TAB" }
			};

			readFile.ColumnCollection.Add(new Column("DateTime", new ValueFormatMutable() { DataType = DataType.DateTime, DateFormat = @"dd/MM/yyyy" })
			{
				TimePart = "Time",
				TimePartFormat = "HH:mm:ss"
			});
			readFile.ColumnCollection.Add(new Column("Integer", DataType.Integer));
			readFile.ColumnCollection.Add(new ImmutableColumn("Numeric", new ImmutableValueFormat(DataType.Numeric, decimalSeparator: "."), 0));
			readFile.ColumnCollection.Add(new Column("Double", new ValueFormatMutable() { DataType = DataType.Double, DecimalSeparator = "." }));
			readFile.ColumnCollection.Add(new Column("Boolean", DataType.Boolean));
			readFile.ColumnCollection.Add(new Column("GUID", DataType.Guid));
			readFile.ColumnCollection.Add(new Column("Time", new ValueFormatMutable() { DataType = DataType.DateTime, DateFormat = "HH:mm:ss" }) { Ignore = true });
			return readFile;
		}

		public static CsvFile ReaderGetBasicCSV(string id = "BasicCSV")
		{
			var readFile = new CsvFile
			{
				ID = id,
				FileFormat = { CommentLine = "#" },
				FileName = Path.Combine(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"))
			};
			var examDateFld = new Column("ExamDate", DataType.DateTime);
			readFile.ColumnCollection.Add(examDateFld);

			examDateFld.ValueFormatMutable.DateFormat = @"dd/MM/yyyy";

			readFile.ColumnCollection.Add(new Column("Score", DataType.Integer));

			readFile.ColumnCollection.Add(new Column("Proficiency", DataType.Numeric));

			readFile.ColumnCollection.Add(new Column("IsNativeLang", DataType.Boolean));

			return readFile;
		}

		public static DataTable RandomDataTable(int records)
		{
			var dataTable = new DataTable { TableName = "DataTable", Locale = CultureInfo.InvariantCulture };

			dataTable.Columns.Add("ID", typeof(int));
			dataTable.Columns.Add("Text", typeof(string));
			dataTable.Columns.Add("ColText1", typeof(string));
			dataTable.Columns.Add("ColText2", typeof(string));
			dataTable.Columns.Add("ColTextDT", typeof(DateTime));
			var random = new Random(new Guid().GetHashCode());
			for (var i = 0; i < records; i++)
			{
				var row = dataTable.NewRow();
				row["ID"] = i;
				row["Text"] = i.ToString(CultureInfo.CurrentCulture);
				row["ColText1"] = $"Test{i + 1}";
				row["ColText2"] = $"Text {i * 2} !";
				row["ColTextDT"] = new DateTime(random.Next(1900, 2030), random.Next(1, 12), 1).AddDays(random.Next(1, 31));
				dataTable.Rows.Add(row);
			}

			return dataTable;
		}

		public static T ExecuteWithCulture<T>(Func<T> methodFunc, string cultureName)
		{
			var result = default(T);

			var thread = new Thread(() => { result = methodFunc(); }) { CurrentCulture = new CultureInfo(cultureName) };
			thread.Start();
			thread.Join();

#pragma warning disable CS8603 // Mögliche Nullverweisrückgabe.
			return result;
#pragma warning restore CS8603 // Mögliche Nullverweisrückgabe.
		}

		public static string? GetRandomText(int length)
		{
			if (length < 1)
				return null;
			// Space is in there a few times so we get more spaces
			var chars = " abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ 1234567890 !§$%&/()=?+*#,.-;:_ "
				.ToCharArray();
			var data = new byte[length];
			using (var crypto = new RNGCryptoServiceProvider())
			{
				crypto.GetNonZeroBytes(data);
			}

			var result = new StringBuilder(length);
			foreach (var b in data)
				result.Append(chars[b % chars.Length]);
			return result.ToString();
		}

		public static DataTable GetDataTable2(long numRecords = 100)
		{
			var dataTable = new DataTable { TableName = "ArtificialTable2", Locale = new CultureInfo("en-gb") };
			dataTable.Columns.Add("string", typeof(string));
			for (long i = 1; i <= numRecords; i++)
			{
				var dr = dataTable.NewRow();
				dr[0] = i.ToString(CultureInfo.InvariantCulture);
				dataTable.Rows.Add(dr);
			}

			return dataTable;
		}

		public static DataTable GetDataTable(int numRecords = 100, bool addError = true)
		{
			var dataTable = new DataTable { TableName = "ArtificialTable", Locale = new CultureInfo("en-gb") };
			dataTable.Columns.Add("string", typeof(string));
			dataTable.Columns.Add("int", typeof(int));
			dataTable.Columns.Add("DateTime", typeof(DateTime));
			dataTable.Columns.Add("bool", typeof(bool));
			dataTable.Columns.Add("double", typeof(double));
			dataTable.Columns.Add("numeric", typeof(decimal));
			dataTable.Columns.Add("AllEmpty", typeof(string));
			dataTable.Columns.Add("PartEmpty", typeof(string));
			dataTable.Columns.Add("ID", typeof(int));
			dataTable.Columns.Add(ReaderConstants.cStartLineNumberFieldName, typeof(long));
			if (addError)
				dataTable.Columns.Add(ReaderConstants.cErrorField, typeof(string));

			dataTable.BeginLoadData();
			for (var i = 1; i <= numRecords; i++) AddRowToDataTable(dataTable, i, addError);
			dataTable.EndLoadData();
			return dataTable;
		}

		public static void RunCopyTo(IEnumerable<Type> list)
		{
			if (list == null) throw new ArgumentNullException(nameof(list));
			foreach (var type in list)
				try
				{
					// if there is not parameter less constructor skip
					if (type.GetConstructor(Type.EmptyTypes)== null)
						continue;

					var obj1 = Activator.CreateInstance(type);
					var obj2 = Activator.CreateInstance(type);

					var properties = type.GetProperties().Where(
						prop => prop.GetMethod != null && prop.SetMethod != null
																					 && (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(string)
																																								|| prop.PropertyType == typeof(bool)
																																								|| prop.PropertyType == typeof(DateTime)
																					 )).ToArray();
					if (properties.Length == 0)
						continue;
					char start = 'a';
					foreach (var prop1 in properties)
					{
						if (prop1.PropertyType == typeof(int) || prop1.PropertyType == typeof(long) || prop1.PropertyType == typeof(byte))
							prop1.SetValue(obj1, 17);

						if (prop1.PropertyType == typeof(bool))
							prop1.SetValue(obj1, !(bool) prop1.GetValue(obj1));

						if (prop1.PropertyType == typeof(char))
							prop1.SetValue(obj1, start++);

						if (prop1.PropertyType == typeof(string))
							prop1.SetValue(obj1, start++ + "_Raphael");

						if (prop1.PropertyType == typeof(DateTime))
							prop1.SetValue(obj1, new DateTime(2014, 12, 24));

						if (prop1.PropertyType == typeof(decimal))
							prop1.SetValue(obj1, 1.56m);

						if (prop1.PropertyType == typeof(float))
							prop1.SetValue(obj1, 22.7f);

						if (prop1.PropertyType == typeof(double))
							prop1.SetValue(obj1, 31.7d);
					}

					var methodClone = type.GetMethod("Clone", BindingFlags.Public | BindingFlags.Instance);
					Assert.IsNotNull(methodClone, $"{type.FullName}.Clone()");
					try
					{
						var obj3 = methodClone.Invoke(obj1, null);
						Assert.IsInstanceOfType(obj3, type);
						foreach (var prop in properties)
							Assert.AreEqual(prop.GetValue(obj1), prop.GetValue(obj3), $"Type: {type.FullName} Property:{prop.Name}");
					}
					catch (Exception ex)
					{
						// Ignore all NotImplementedException these are cause by compatibility setting or mocks
						Debug.Write(ex.ExceptionMessages());
					}

					var methodCopyTo = type.GetMethod("CopyTo", BindingFlags.Public | BindingFlags.Instance);
					// Cloneable does mean you have to have CopyTo
					if (methodCopyTo == null) continue;

					try
					{
						// methodCopyTo.Invoke(obj1, new object[] { null });
						methodCopyTo.Invoke(obj1, new[] { obj2 });

						foreach (var prop in properties)
							Assert.AreEqual(prop.GetValue(obj1), prop.GetValue(obj2), $"Type: {type.FullName} Property:{prop.Name}");

						methodCopyTo.Invoke(obj1, new[] { obj1 });
					}
					catch (Exception ex)
					{
						// Ignore all NotImplementedException these are cause by compatibility setting or mocks
						Logger.Warning(ex.ExceptionMessages());
					}
				}
				catch (MissingMethodException)
				{
					// Ignore, there is no constructor without parameter
				}
				catch (AssertFailedException)
				{
					throw;
				}
				catch (Exception e)
				{
					Logger.Error(e, "Issue with {@type}", type);
					Assert.Fail($"Issue with {type.FullName}\n{e.Message}");
				}
		}

		public static void AddRowToDataTable(DataTable dataTable, int recNum, bool addError)
		{
			var minDate = DateTime.Now.AddYears(-20).Ticks;
			var maxDate = DateTime.Now.AddYears(5).Ticks;
			var dr = dataTable.NewRow();
			dr[0] = GetRandomText(50);
			if (recNum % 10 == 0)
				dr[0] = dr[0] + "\r\nA Second Line";

			dr[1] = m_Random.Next(-300, +600);

			if (m_Random.NextDouble() > .2)
			{
				var dtm = (((maxDate - minDate) * m_Random.NextDouble()) + minDate).ToInt64();
				dr[2] = new DateTime(dtm);
			}

			dr[3] = m_Random.Next(0, 2) == 0;

			dr[4] = m_Random.NextDouble() * 123.78;

			if (recNum % 3 == 0)
				dr[5] = m_Random.NextDouble();

			if (m_Random.NextDouble() > .4) dr[7] = GetRandomText(100);

			dr[8] = recNum; // ID
			dr[9] = recNum * 2; // #Line

			// Add Errors and Warnings to Columns and Rows
			var rand = m_Random.Next(0, 100);
			if (rand > 70)
			{
				var colNum = m_Random.Next(0, 10);
				if (rand < 85)
					dr.SetColumnError(colNum, "First Warning".AddWarningId());
				else if (rand > 85) dr.SetColumnError(colNum, @"First Error");

				// Add a possible second error in the same column
				rand = m_Random.Next(-2, 3);
				if (rand == 1)
					dr.SetColumnError(colNum, dr.GetColumnError(colNum).AddMessage("Second Warning".AddWarningId()));
				else if (rand == 2) dr.SetColumnError(colNum, dr.GetColumnError(colNum).AddMessage("Second Error"));
			}

			if (rand > 80) dr.RowError = rand > 90 ? @"Row Error" : @"Row Warning".AddWarningId();
			if (addError)
				dr[10] = dr.GetErrorInformation();

			dataTable.Rows.Add(dr);
		}

#pragma warning disable CA2211 // Non-constant fields should not be visible

		public static Column[] ColumnsDT2 =
		{
			new Column("string") //0
    };

		public static Column[] ColumnsDT =
		{
			new Column("string"), //0
      new Column("int", DataType.Integer), //1
      new Column("DateTime", DataType.DateTime), //2
      new Column("bool", DataType.Boolean), //3
      new Column("double", DataType.Double), //4
      new Column("numeric", DataType.Numeric), //5
      new Column("AllEmpty"), //6
      new Column("PartEmpty"), //7
      new Column("ID", DataType.Integer) //8
    };

#pragma warning restore CA2211 // Non-constant fields should not be visible
	}
}