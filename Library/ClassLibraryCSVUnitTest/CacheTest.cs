using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace CsvTools.Tests
{
  [TestClass]
  public class CacheTest
  {    
    [TestMethod]
    public void SetAndGet()
    {
      using (var cache = new Cache<int, string>())
      {
        cache.Set(1, "Eins");
        cache.Set(2, "Zwei");
        cache.Set(3, "Drei");
        Assert.AreEqual("Eins", cache.Get(1));
        Assert.AreEqual("Zwei", cache.Get(2));
        Assert.AreEqual("Drei", cache.Get(3));
        cache.TryGet(3, out var output);
        Assert.AreEqual("Drei", output);

        cache.TryGet(4, out output);
        Assert.AreEqual(null, output);
      }
    }

    [TestMethod]
    public void TestCleanUp()
    {
      using (var cache = new Cache<int, string>())
      {
        cache.Set(1, "Eins", 1);
        cache.Set(2, "Zwei", 2);
        cache.Set(3, "Drei", 3);
        Assert.AreEqual("Eins", cache.Get(1));
        Assert.AreEqual("Zwei", cache.Get(2));
        Assert.AreEqual("Drei", cache.Get(3));

        Thread.Sleep(1200);
        Assert.AreEqual("Zwei", cache.Get(2));
        Assert.AreEqual("Drei", cache.Get(3));
        Assert.AreEqual(null, cache.Get(1));
      }
    }

    [TestMethod]
    public void TestFlush()
    {
      using (var cache = new Cache<int, string>())
      {
        cache.Set(1, "Eins", 1);
        cache.Set(2, "Zwei", 2);
        cache.Set(3, "Drei", 3);
        Assert.AreEqual("Eins", cache.Get(1));
        Assert.AreEqual("Zwei", cache.Get(2));
        Assert.AreEqual("Drei", cache.Get(3));
        cache.Flush();

        Assert.AreEqual(null, cache.Get(2));
        Assert.AreEqual(null, cache.Get(3));
        Assert.AreEqual(null, cache.Get(1));
      }
    }
  }
}