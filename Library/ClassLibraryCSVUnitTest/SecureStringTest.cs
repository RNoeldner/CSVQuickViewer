using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  /// <summary>
  ///   Summary description for SecureStringTest
  /// </summary>
  [TestClass]
  public class SecureStringTest
  {
    [TestMethod]
    public void Encyrpt()
    {
      var encyrpted1 = "This is a a test".Encrypt();
      var encyrpted2 = "This is a a test".Encrypt();
      Assert.AreNotEqual(encyrpted1, encyrpted2);
    }

    [TestMethod]
    public void Decrypt()
    {
      const string testValue = "This is a a test";
      var encyrpted1 = testValue.Encrypt();
      Assert.AreEqual(testValue, encyrpted1.Decrypt());
    }

    [TestMethod]
    public void DecryptEmptyAndNull()
    {
      Assert.AreEqual(string.Empty, string.Empty.Decrypt());
      Assert.AreEqual(string.Empty, SecureString.Decrypt(null));
    }

    #region Additional test attributes

    //
    // You can use the following additional attributes as you write your tests:
    //
    // Use ClassInitialize to run code before running the first test in the class
    // [ClassInitialize()]
    // public static void MyClassInitialize(TestContext testContext) { }
    //
    // Use ClassCleanup to run code after all tests in a class have run
    // [ClassCleanup()]
    // public static void MyClassCleanup() { }
    //
    // Use TestInitialize to run code before running each test
    // [TestInitialize()]
    // public void MyTestInitialize() { }
    //
    // Use TestCleanup to run code after each test has run
    // [TestCleanup()]
    // public void MyTestCleanup() { }
    //

    #endregion Additional test attributes
  }
}