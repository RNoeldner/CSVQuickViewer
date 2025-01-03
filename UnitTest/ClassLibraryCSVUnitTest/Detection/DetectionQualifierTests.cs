using Microsoft.VisualStudio.TestTools.UnitTesting;
using CsvTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass()]
  public class DetectionQualifierTests
  {
    [TestMethod]
    public async Task GuessQualifier_TextQualifiers()
    {
      using var improvedStream =
        new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("TextQualifiers.txt")));

      using var textReader =
        await improvedStream.GetTextReaderAsync(65001, 0, UnitTestStatic.Token).ConfigureAwait(false);
      var quoteTestResult = textReader.InspectQualifier('\t', '\\', string.Empty, StaticCollections.PossibleQualifiers, UnitTestStatic.Token);
      Assert.AreEqual('"', quoteTestResult.QuoteChar);
    }

    [TestMethod]
    public async Task GuessQualifier_Quoting1()
    {
      using var improvedStream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("Quoting1.txt")));

      using var textReader =
        await improvedStream.GetTextReaderAsync(65001, 0, UnitTestStatic.Token).ConfigureAwait(false);
      var quoteTestResult = textReader.InspectQualifier('\t', '\\', string.Empty, StaticCollections.PossibleQualifiers, UnitTestStatic.Token);
      Assert.AreEqual('"', quoteTestResult.QuoteChar);
      Assert.IsTrue(quoteTestResult.DuplicateQualifier);
    }

    [TestMethod]
    public async Task GuessQualifier_Quoting1Reverse()
    {
      using var improvedStream =
        new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("Quoting1Reverse.txt")));

      using var textReader =
        await improvedStream.GetTextReaderAsync(65001, 0, UnitTestStatic.Token).ConfigureAwait(false);
      var quoteTestResult = textReader.InspectQualifier('\t', '\\', "#", StaticCollections.PossibleQualifiers, UnitTestStatic.Token);
      Assert.AreEqual("'", quoteTestResult.QuoteChar.ToString());
      Assert.IsTrue(quoteTestResult.DuplicateQualifier);
      Assert.IsTrue(quoteTestResult.Score > 50);
    }

    [TestMethod]
    public async Task GuessQualifier_QuotingComments()
    {
      using var improvedStream =
        new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("QuotingComments.txt")));

      using var textReader =
        await improvedStream.GetTextReaderAsync(65001, 0, UnitTestStatic.Token).ConfigureAwait(false);
      var quoteTestResult = textReader.InspectQualifier('\t', '\\', "##", StaticCollections.PossibleQualifiers, UnitTestStatic.Token);
      Assert.AreEqual('"', quoteTestResult.QuoteChar);
    }
  }
}