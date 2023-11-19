using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CsvTools.Tests
{
  [TestClass]
  public class ViewSettingTests
  {
    [TestMethod]
    public void StoreViewSettingTest1()
    {
      using (var dgv = new DataGridView())
      {
        var dgvc = new DataGridViewColumn(new DataGridViewTextBoxCell())
        {
          DataPropertyName = "test",
          ValueType = typeof(string)
        };
        dgv.Columns.Add(dgvc);
        var columnFilters = new List<ColumnFilterLogic> { new ColumnFilterLogic( typeof(string), "DataProp") };
        var text = ViewSetting.StoreViewSetting(dgv.Columns, columnFilters, dgv.SortedColumn, dgv.SortOrder);
        Assert.IsTrue(!string.IsNullOrEmpty(text));
      }
    }

    [TestMethod]
    public void StoreViewSettingTest2()
    {
      using (var dgv = new DataGridView())
      {
        var dgvc = new DataGridViewColumn(new DataGridViewTextBoxCell())
        {
          DataPropertyName = "test",
          ValueType = typeof(string)
        };
        dgv.Columns.Add(dgvc);
        var dgvc2 = new DataGridViewColumn(new DataGridViewTextBoxCell())
        {
          DataPropertyName = "test2",
          ValueType = typeof(string)
        };
        dgv.Columns.Add(dgvc2);
        var columnFilters = new List<ColumnFilterLogic> { new ColumnFilterLogic( typeof(string), "DataProp"), new ColumnFilterLogic( typeof(string), "DataProp2") };
        
        columnFilters[0].ValueClusterCollection.ValueClusters
          .Add(new ValueCluster("display", "cond", string.Empty, 0, "cond",null));
        columnFilters[0].Active = true;

        columnFilters[1].Operator = "=";
        columnFilters[1].ValueText = "Halloween";
        columnFilters[1].Active = true;

        var text = ViewSetting.StoreViewSetting(dgv.Columns, columnFilters, dgv.SortedColumn, dgv.SortOrder);
        //ViewSetting.ReStoreViewSetting(text, dgv.Columns, Array.Empty<ToolStripDataGridViewColumnFilter>(),
        //  i => columnFilters[i], null);
      }
    }

    [TestMethod]
    [Timeout(1000)]
    public void ReStoreViewSettingDetailControlAsync()
    {
      using (var dt = UnitTestStaticData.GetDataTable())
      {
        using (var dc = new DetailControl())
        {
          dc.HtmlStyle = HtmlStyle.Default;
          dc.DataTable = dt;
          dc.RefreshDisplay(FilterTypeEnum.All, UnitTestStatic.Token);
          dc.SetFilter(dt.Columns[0].ColumnName, ">", "Ha");
          var text = dc.GetViewStatus();

          var fn = System.IO.Path.Combine(FileSystemUtils.ExecutableDirectoryName(), "test.delete");
          FileSystemUtils.FileDelete(fn);
          FileSystemUtils.WriteAllText(fn, text);
          dc.ReStoreViewSetting(fn);
          FileSystemUtils.FileDelete(fn);
        }
      }
    }
  }
}