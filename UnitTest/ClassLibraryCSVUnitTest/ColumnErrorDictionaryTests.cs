using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class ColumnErrorDictionaryTests
  {
    [TestMethod]
    public async Task ColumnErrorDictionaryTest1Async()
    {
      var setting = new CsvFile
      {
        FileName = UnitTestInitializeCsv.GetTestPath("Sessions.txt"),
        HasFieldHeader = true,
        ByteOrderMark = true,
        FileFormat = {FieldDelimiter = "\t"}
      };
      setting.ColumnCollection.AddIfNew(new Column("Start Date") {Ignore = true});

      using (var processDisplay = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
      using (var reader = new CsvFileReader(setting, processDisplay))
      {
        await reader.OpenAsync(processDisplay.CancellationToken);
        var test1 = new ColumnErrorDictionary(reader);
        Assert.IsNotNull(test1);

        // Message in ignored column
        reader.HandleWarning(0, "Msg1");
        reader.HandleError(1, "Msg2");

        Assert.AreEqual("Msg2", test1.Display);
      }
    }

    [TestMethod]
    public void AddTest()
    {
      var test1 = new ColumnErrorDictionary();
      Assert.IsNotNull(test1);
      test1.Add(0, "Message");
      Assert.AreEqual("Message", test1.Display);
      test1.Add(0, "Another Message");
      Assert.AreEqual("Message" + ErrorInformation.cSeparator + "Another Message", test1.Display);
    }
  }
}