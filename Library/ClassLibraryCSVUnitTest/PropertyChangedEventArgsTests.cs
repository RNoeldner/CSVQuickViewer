using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class PropertyChangedEventArgsTests
  {
    [TestMethod]
    public void PropertyChangedEventArgsTest()
    {
      var args = new PropertyChangedEventArgs<string>("propName", "old", "new");
      Assert.AreEqual("old", args.OldValue);
      Assert.AreEqual("new", args.NewValue);
      Assert.AreEqual("propName", args.PropertyName);

      args.NewValue = "new2";
      Assert.AreEqual("new2", args.NewValue);

      args.OldValue = "old2";
      Assert.AreEqual("old2", args.OldValue);
    }
  }
}