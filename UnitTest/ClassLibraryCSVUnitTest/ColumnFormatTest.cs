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
using System;

namespace CsvTools.Tests
{
  [TestClass]
	public class ColumnFormatTest
	{
		private readonly Column m_Column = new Column();

		[TestMethod]
		public void ColumnDetermineDataTypeFromType()
		{
			var s = "Test";
			var dt = DateTime.Now;
			decimal dec = 0;
			double dbl = 0;
			long lng = -5;
			var int32 = -5;
			var g = Guid.NewGuid();

			Assert.AreEqual(DataTypeEnum.Integer, lng.GetType().GetDataType());
			Assert.AreEqual(DataTypeEnum.Integer, int32.GetType().GetDataType());
			Assert.AreEqual(DataTypeEnum.Integer, Type.GetType("System.UInt16").GetDataType());
			Assert.AreEqual(DataTypeEnum.Integer, Type.GetType("System.Int64").GetDataType());
			Assert.AreEqual(DataTypeEnum.Double, dbl.GetType().GetDataType());
			Assert.AreEqual(DataTypeEnum.Double, Type.GetType("System.Single").GetDataType());
			Assert.AreEqual(DataTypeEnum.Numeric, dec.GetType().GetDataType());
			Assert.AreEqual(DataTypeEnum.DateTime, dt.GetType().GetDataType());
			Assert.AreEqual(DataTypeEnum.String, s.GetType().GetDataType());
			Assert.AreEqual(DataTypeEnum.Boolean, true.GetType().GetDataType());
			Assert.AreEqual(DataTypeEnum.Guid, g.GetType().GetDataType());
		}

		[TestMethod]
		public void ColumnEquals()
		{
			var target = new Column();
			m_Column.CopyTo(target);
			Assert.IsTrue(m_Column.Equals(target));
		}

		[TestMethod]
		public void ColumnGetDataType()
		{
			var test = new ValueFormatMutable();

			Assert.IsInstanceOfType(string.Empty, test.DataType.GetNetType());
			test.DataType = DataTypeEnum.DateTime;
			Assert.IsInstanceOfType(DateTime.Now, test.DataType.GetNetType());
			test.DataType = DataTypeEnum.Boolean;
			Assert.IsInstanceOfType(false, test.DataType.GetNetType());
			test.DataType = DataTypeEnum.Double;
			Assert.IsInstanceOfType(double.MinValue, test.DataType.GetNetType());
			test.DataType = DataTypeEnum.Numeric;
			Assert.IsInstanceOfType(decimal.MaxValue, test.DataType.GetNetType());
		}

		//[TestMethod]
		//public void ColumnGetHashCode()
		//{
		//  var test1 = new Column ();
		//  var test2 = new Column ();
		//  Assert.AreEqual(test1.GetHashCode(), test2.GetHashCode());
		//  Assert.AreEqual(test1.GetHashCode(), test1.GetHashCode());
		//}

		[TestMethod]
		public void ColumnNotEquals()
		{
			var target1 = new Column { Name = "Hello" };
			var target2 = new Column { Name = "World" };
			Assert.IsFalse(target1.Equals(target2));
		}

		[TestMethod]
		public void ColumnNotEqualsNull() => Assert.IsFalse(m_Column.Equals(null));

		[TestMethod]
		public void ColumnPropertyChanged()
		{
			var numCalled = 0;
			var test = new Column();
			test.PropertyChanged += delegate
			{
				numCalled++;
			};
			test.ValueFormatMutable.DataType = DataTypeEnum.DateTime;
			Assert.AreEqual(DataTypeEnum.DateTime, test.ValueFormatMutable.DataType);
			Assert.IsTrue(test.Convert);

			Assert.AreEqual(0, numCalled);

			test.Name = "Name";
			Assert.AreEqual(1, numCalled);
		}

		[TestMethod]
		public void ColumnToString() => Assert.IsNotNull(m_Column.ToString());

		[TestMethod]
		public void GetDataTypeDescriptionBool()
		{
			var target = new Column("Test", DataTypeEnum.Boolean);
			Assert.AreEqual("Boolean", target.GetTypeAndFormatDescription());
		}

		[TestMethod]
		public void GetDataTypeDescriptionDateTime()
		{
			var target =
				new Column("Test", DataTypeEnum.DateTime) { TimePart = "TPart", TimePartFormat = "YYYYMMDD", TimeZonePart = "'UTC'" };
			Assert.IsTrue(target.GetTypeAndFormatDescription().Contains("TPart", StringComparison.InvariantCultureIgnoreCase),
				"TimePart");
			Assert.IsTrue(
				target.GetTypeAndFormatDescription().Contains("YYYYMMDD", StringComparison.InvariantCultureIgnoreCase),
				"TimePartFormat");
			Assert.IsTrue(target.GetTypeAndFormatDescription().Contains("'UTC'", StringComparison.InvariantCultureIgnoreCase),
				"TimeZonePart");
		}

		[TestMethod]
		public void GetDataTypeDescriptionDouble()
		{
			var target = new Column("Test", new ValueFormatMutable() { DataType=DataTypeEnum.Numeric, NumberFormat = "00.000" });

			Assert.AreEqual("Money (High Precision) (00.000)", target.GetTypeAndFormatDescription());
		}

		[TestMethod]
		public void GetDataTypeDescriptionIgnore()
		{
			var target = new Column("Test") { Ignore = true };

			Assert.AreEqual("Text (Ignore)", target.GetTypeAndFormatDescription());
		}

		[TestInitialize]
		public void Init()
		{
			var valueFormatGerman = new ValueFormatMutable
			{
				DataType = DataTypeEnum.DateTime,
				DateFormat = @"dd/MM/yyyy",
				DateSeparator = ".",
				DecimalSeparator = ",",
				False = @"Falsch",
				GroupSeparator = ".",
				NumberFormat = "0.##",
				TimeSeparator = ":",
				True = @"Wahr"
			};

			var ff = new CsvFile();
			var col = new Column("StartDate", valueFormatGerman) { Ignore = true };

			ff.ColumnCollection.Add(col);
			Assert.AreEqual("StartDate", col.Name, "Name");
			Assert.AreEqual(DataTypeEnum.DateTime, col.ValueFormatMutable.DataType, "DataType");
			Assert.IsTrue(col.Convert, "Convert");
			Assert.IsTrue(col.Ignore, "Ignore");
		}

		//[TestMethod]
		//public void ColumnDetermineDataTypeFromTypeName()
		//{
		//  Assert.AreEqual(DataType.Integer, Column.DetermineDataTypeFromTypeName("System.Int32"));
		//  Assert.AreEqual(DataType.Double, Column.DetermineDataTypeFromTypeName("System.Double"));
		//  Assert.AreEqual(DataType.Numeric, Column.DetermineDataTypeFromTypeName("System.Decimal"));
		//  Assert.AreEqual(DataType.DateTime, Column.DetermineDataTypeFromTypeName("System.DateTime"));
		//  Assert.AreEqual(DataType.String, Column.DetermineDataTypeFromTypeName("string"));
		//  Assert.AreEqual(DataType.Boolean, Column.DetermineDataTypeFromTypeName("System.Boolean"));
		//  Assert.AreEqual(DataType.Guid, Column.DetermineDataTypeFromTypeName("System.Guid"));
		//}
	}
}