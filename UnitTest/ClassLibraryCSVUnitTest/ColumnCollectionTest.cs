using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CsvTools.Tests
{
  [TestClass]
	public class ColumnCollectionTest
	{
		[TestMethod]
		public void Add()
		{
			var test = new ColumnCollection();
			Assert.AreEqual(0, test.Count);
			var item1 = new Column("Test");
			test.Add(item1);
			Assert.AreEqual(1, test.Count);
			var item2 = new ImmutableColumn("Test", new ImmutableValueFormat(), 0);
			test.Add(item2);
			Assert.AreEqual(1, test.Count);
			test.Add(new Column());

			var exception = false;
			try
			{
				// ReSharper disable once AssignNullToNotNullAttribute
				test.Add(null);
			}
			catch (ArgumentException)
			{
				exception = true;
			}
			catch (Exception ex)
			{
				Assert.Fail("Wrong Exception Type: " + ex.GetType());
			}

			Assert.IsTrue(exception, "No Exception thrown");
		}

		[TestMethod]
		public void TestOrder()
		{
			var test = new ColumnCollection(new[] {new ImmutableColumn("ColA", new ImmutableValueFormat(), 1),
				new ImmutableColumn("ColB", new ImmutableValueFormat(), 2),
				new ImmutableColumn("ColC", new ImmutableValueFormat(), 3)});
			Assert.AreEqual(3, test.Count);

			int oldColOrd = 0;
			foreach (var col in test)
			{
				Assert.IsTrue(col.ColumnOrdinal>oldColOrd);
				oldColOrd = col.ColumnOrdinal;
			}
		}

		[TestMethod]
		public void Replace()
		{
			var test = new ColumnCollection(new[] {new ImmutableColumn("ColA", new ImmutableValueFormat(), 1),
																									 new ImmutableColumn("ColB", new ImmutableValueFormat(), 2),
																									 new ImmutableColumn("ColC", new ImmutableValueFormat(), 3)});
			var colBnew = new ImmutableColumn("ColB", new ImmutableValueFormat(DataType.Boolean), 2);
			test.Replace(colBnew);
			var colB = test.Get("ColB");
			Assert.AreEqual(3, test.Count);
			Assert.AreEqual(colB, (IColumn) colBnew);
			int oldColOrd = 0;
			foreach (var col in test)
			{
				Assert.IsTrue(col.ColumnOrdinal>oldColOrd);
				oldColOrd = col.ColumnOrdinal;
			}
		}

		[TestMethod]
		public void Get()
		{
			var test = new ColumnCollection();
			var item1 = new Column("Test");
			test.Add(item1);
			Assert.AreEqual(1, test.Count);
			var item2 = new Column("Test2");
			test.Add(item2);
			Assert.IsTrue(item1.Equals(test.Get("Test")));
			Assert.IsTrue(item1.Equals(test.Get("TEST")));
			Assert.IsTrue(item2.Equals(test.Get("tEst2") ));

			Assert.IsNull(test.Get(""));
			Assert.IsNull(test.Get(null));
		}

		[TestMethod]
		public void Clone()
		{
			var test1 = new ColumnCollection();
			test1.Add(new Column("Test1"));
			test1.Add(new Column("Test2"));
			test1.Add(new Column("Test3"));

			var test2 = (ColumnCollection) test1.Clone();

			Assert.IsTrue(test2.Equals(test1));
		}

		[TestMethod]
		public void CopyTo()
		{
			var test1 = new ColumnCollection();
			test1.Add(new Column("Test1"));
			test1.Add(new Column("Test2"));
			test1.Add(new Column("Test3"));
			var test2 = new ColumnCollection();
			test1.CopyTo(test2);

			Assert.IsTrue(test2.Equals(test1));
		}
	}
}