using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;

namespace CsvTools.Tests
{
  [TestClass]
  public class ApplicationSettingTest
  {
    [TestMethod]
    public void ApplicationSettingCacheList()
    {
      var value = new[] { "A", "B" };
      ApplicationSetting.CacheList.Set("Hallo", value);
      Assert.AreSame(value, ApplicationSetting.CacheList.Get("Hallo"));
      ApplicationSetting.FlushAll();
      Assert.IsFalse(ApplicationSetting.CacheList.ContainsKey("Hello"));
    }

    [TestMethod]
    public void ApplicationSettingStatics()
    {
      Assert.IsTrue(ApplicationSetting.FillGuessSettings is FillGuessSettings);
      Assert.IsNotNull(ApplicationSetting.FillGuessSettings);
      Assert.IsTrue(ApplicationSetting.HTMLStyle is HTMLStyle);
      Assert.IsNotNull(ApplicationSetting.HTMLStyle);
    }

    [TestMethod]
    public void ApplicationSettingToolSetting()
    {
      Assert.IsTrue(ApplicationSetting.ToolSetting is IToolSetting);
      Assert.IsNotNull(ApplicationSetting.ToolSetting);
      var otherInstance = new DummyToolSetting();
      Assert.AreNotEqual(otherInstance, ApplicationSetting.ToolSetting);
      ApplicationSetting.ToolSetting = otherInstance;
      Assert.AreEqual(otherInstance, ApplicationSetting.ToolSetting);
    }

    [TestMethod]
    public void ApplicationSettingMenuDown()
    {
      ApplicationSetting.MenuDown = true;
      Assert.IsTrue(ApplicationSetting.MenuDown);
      ApplicationSetting.MenuDown = false;
      Assert.IsFalse(ApplicationSetting.MenuDown);
    }

    [TestMethod]
    public void SQLDataReaderText()
    {
      try
      {
        ApplicationSetting.SQLDataReader = null;
        var ignore = ApplicationSetting.SQLDataReader;
      }
      catch (ArgumentNullException)
      {
        // all good
      }
      ApplicationSetting.SQLDataReader = delegate (string s, CancellationToken ct) { return null; };
      var reader = ApplicationSetting.SQLDataReader;
      Assert.IsNotNull(reader);
    }

    private class MockCache<TKey, TValue> : ICache<TKey, TValue>
      where TKey : IComparable
      where TValue : class
    {
      /// <summary>
      ///   Dictionary that stores the cache items
      /// </summary>
      private readonly Dictionary<TKey, TValue> m_Dictionary = new Dictionary<TKey, TValue>();

      public bool Flushed;

      public ICollection<TKey> Keys => m_Dictionary.Keys;

      public TValue Get(TKey key)
      {
        return m_Dictionary[key];
      }

      public bool ContainsKey(TKey key)
      {
        return m_Dictionary.ContainsKey(key);
      }

      public void Remove(TKey key)
      {
        m_Dictionary.Remove(key);
      }

      public void Set(TKey key, TValue item)
      {
        m_Dictionary[key] = item;
      }

      public void Set(TKey key, TValue item, int lifetime)
      {
        m_Dictionary[key] = item;
      }

      public bool TryGet(TKey key, out TValue item)
      {
        // Lookup if an item with the specified key exists in the internal dictionary.
        return m_Dictionary.TryGetValue(key, out item);
      }

      public void Flush()
      {
        m_Dictionary.Clear();
        Flushed = true;
      }

      public void Dispose()
      {
      }
    }

    [TestMethod()]
    public void GetColumByFieldTest()
    {
      var csv = new CsvFile();
      Assert.IsNull(csv.GetColumByField("Hello"));

      var col = new Column()
      {
        Name = "Column"
      };
      csv.ColumnAdd(col);
      csv.Mapping.Add(new Mapping()
      {
        FileColumn = "Column",
        TemplateField = "Field"
      });
      Assert.AreEqual(col, csv.GetColumByField("Field"));
    }

    [TestMethod()]
    public void GetColumNameByFieldTest()
    {
      var csv = new CsvFile();
      Assert.IsNull(csv.GetColumNameByField("Hello"));

      csv.Mapping.Add(new Mapping()
      {
        FileColumn = "Column",
        TemplateField = "Field"
      });
      Assert.AreEqual("Column", csv.GetColumNameByField("Field"));
    }

    [TestMethod()]
    public void GetMappingByFieldTest()
    {
      var csv = new CsvFile();
      Assert.IsNull(csv.GetMappingByField(""));
      Assert.IsNull(csv.GetMappingByField("Hello"));

      var map = new Mapping()
      {
        FileColumn = "Column",
        TemplateField = "Field"
      };
      csv.Mapping.Add(map);
      Assert.AreEqual(map, csv.GetMappingByField("Field"));
    }
  }
}