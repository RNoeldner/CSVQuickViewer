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

      var test2 = new CheckResult();
      test2.ExampleNonMatch.Add("Test3");
      test2.ExampleNonMatch.Add("Test4");
      test2.PossibleMatch = true;

      test1.CombineCheckResult(test1);
      Assert.AreEqual(2, test1.ExampleNonMatch.Count());
      Assert.IsFalse(test1.PossibleMatch);
      test1.CombineCheckResult(test2);
      Assert.AreEqual(4, test1.ExampleNonMatch.Count());
      Assert.IsTrue(test1.PossibleMatch);
    }
  }
}