using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;

namespace CsvTools.Tests
{
  [TestClass]
  public class ColumnFilterLogicTests
  {
    [TestMethod]
    public void ColumnFilterLogicTest()
    {
      var crtl = new ColumnFilterLogic(typeof(double), "Column1");
      Assert.IsNotNull(crtl);
    }

    [TestMethod]
    public void ApplyFilterTest()
    {
      using (var crtl = new ColumnFilterLogic(typeof(double), "Column1"))
      {
        var called = false;
        crtl.ColumnFilterApply += delegate { called = true; };
        crtl.ApplyFilter();
        Assert.IsTrue(called);
      }
    }

    [TestMethod]
    public void BuildSQLCommandTest()
    {
      using (var crtl = new ColumnFilterLogic(typeof(double), "Column1"))
      {
        Assert.AreEqual("[Column1] = 2", crtl.BuildSQLCommand("2"));
      }

      using (var crtl = new ColumnFilterLogic(typeof(double), "[Column1]"))
      {
        Assert.AreEqual("[Column1] = 2", crtl.BuildSQLCommand("2"));
      }

      using (var crtl = new ColumnFilterLogic(typeof(string), "Column1"))
      {
        Assert.AreEqual("[Column1] = '2'", crtl.BuildSQLCommand("2"));
      }
    }

    [TestMethod]
    public void NotifyPropertyChangedTest()
    {
      using (var crtl = new ColumnFilterLogic(typeof(double), "Column1"))
      {
        string prop = null;
        crtl.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e) { prop = e.PropertyName; };
        crtl.ValueText = "2";
        Assert.AreEqual("ValueText", prop);
        Assert.AreEqual("[Column1] = 2", crtl.BuildSQLCommand(crtl.ValueText));
      }
    }

    [TestMethod]
    public void SetActiveTest()
    {
      using (var crtl = new ColumnFilterLogic(typeof(DateTime), "Column1"))
      {
        var dtm = DateTime.Now;
        crtl.SetFilter(dtm);
        crtl.Active = true;
        Assert.IsTrue(crtl.Active);
      }
    }

    [TestMethod]
    public void SetFilterTest()
    {
      using (var crtl = new ColumnFilterLogic(typeof(double), "Column1"))
      {
        crtl.SetFilter(2);
        Assert.AreEqual("[Column1] = 2", crtl.BuildSQLCommand(crtl.ValueText));
      }
    }
  }
}