using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass()]
  public class RemoteAccessTests
  {
    public void CopyToTest()
    {
      // Covered by generic test
    }

    public void EqualsTest()
    {
      // Covered by generic test
    }

    [TestMethod()]
    public void RemoteAccessTest()
    {
      var testCase = new RemoteAccess();
      bool hasFired = false;
      testCase.PropertyChanged += delegate (object sender, System.ComponentModel.PropertyChangedEventArgs e)
      {
        hasFired = true;
      };
      testCase.HostName = "Test";
      Assert.AreEqual("Test", testCase.HostName);
      Assert.IsTrue(hasFired);
      hasFired = false;
      testCase.HostName = "Test";
      Assert.IsFalse(hasFired);

      testCase.User = "Hello";
      Assert.IsTrue(hasFired);
      Assert.AreEqual("Hello", testCase.User);
      hasFired = false;
      testCase.User = "Hello";
      Assert.IsFalse(hasFired);

      testCase.Password = "World";
      Assert.IsTrue(hasFired);
      Assert.AreEqual("World", testCase.Password);
      hasFired = false;
      testCase.Password = "World";
      Assert.IsFalse(hasFired);
    }
  }
}