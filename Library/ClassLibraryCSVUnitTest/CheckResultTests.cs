using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class CheckResultTests
  {
    [TestMethod]
    public void CombineCheckResultTest()
    {
      var test1 = new CheckResult();
      test1.ExampleNonMatch.Add("Test1");
      test1.ExampleNonMatch.Add("Test2");
      test1.PossibleMatch = true;

      var test2 = new CheckResult();
      test2.ExampleNonMatch.Add("Test3");
      test2.PossibleMatch = true;

      var test3 = new CheckResult();
      test3.PossibleMatch = true;

      test1.KeepBestPossibleMatch(test1);
      Assert.AreEqual(2, test1.ExampleNonMatch.Count());
      Assert.IsTrue(test1.PossibleMatch);

      test1.KeepBestPossibleMatch(test2);
      Assert.AreEqual(1, test1.ExampleNonMatch.Count());
      Assert.IsTrue(test1.PossibleMatch);

      test1.KeepBestPossibleMatch(test3);
      Assert.AreEqual(0, test1.ExampleNonMatch.Count());
    }
  }
}