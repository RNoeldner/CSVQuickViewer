using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class WarningListTests
  {
    [TestMethod]
    public void WarningListAddAndClear()
    {
      var messageList = new RowErrorCollection();
      Assert.AreEqual(0, messageList.CountRows);

      messageList.Add(null, new WarningEventArgs(1, 2, "Line1", 0, 0, null));
      Assert.AreEqual(1, messageList.CountRows);
      messageList.Clear();
      Assert.AreEqual(0, messageList.CountRows);
    }

    [TestMethod]
    public void WarningListAddEmpty()
    {
      var messageList = new RowErrorCollection();
      Assert.AreEqual(0, messageList.CountRows);

      try
      {
        messageList.Add(null, new WarningEventArgs(1, 2, null, 0, 0, null));
        Assert.Fail("Exception not thrown");
      }
      catch (System.ArgumentException)
      {
      }
      try
      {
        messageList.Add(null, new WarningEventArgs(1, 2, string.Empty, 0, 0, null));
        Assert.Fail("Exception not thrown");
      }
      catch (System.ArgumentException)
      {
      }
    }

    [TestMethod]
    public void WarningListCombineWarning()
    {
      var messageList = new RowErrorCollection();
      messageList.Add(null, new WarningEventArgs(1, 1, "Text1", 0, 0, null));
      messageList.Add(null, new WarningEventArgs(1, 1, "Text2", 0, 1, null));
      Assert.AreEqual(1, messageList.CountRows);
      Assert.IsTrue(
        "Text1" + ErrorInformation.cSeparator + "Text2" == messageList.Display ||
        "Text2" + ErrorInformation.cSeparator + "Text1" == messageList.Display);

      var ce = new ColumnErrorDictionary();
      messageList.TryGetValue(1, out ce);
      Assert.AreEqual(1, ce.Dictionary.Count);
      Assert.AreEqual(messageList.Display, ce.Display);
    }

    [TestMethod]
    public void WarningListDisplay2()
    {
      var messageList = new RowErrorCollection();
      messageList.Add(null, new WarningEventArgs(1, 1, "Text1", 0, 1, null));
      messageList.Add(null, new WarningEventArgs(1, 2, "Text2", 0, 2, null));
      Assert.IsTrue(
        "Text1" + ErrorInformation.cSeparator + "Text2" == messageList.Display ||
        "Text2" + ErrorInformation.cSeparator + "Text1" == messageList.Display);
    }

    [TestMethod]
    public void WarningListInit()
    {
      var messageList = new RowErrorCollection();
    }
  }
}