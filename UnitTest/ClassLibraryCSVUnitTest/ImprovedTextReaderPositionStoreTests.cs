using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
// ReSharper disable StringLiteralTypo

namespace CsvTools.Tests
{
  [TestClass]
  [SuppressMessage("ReSharper", "UseAwaitUsing")]
  public class ImprovedTextReaderPositionStoreTests
  {
		[TestMethod]
		public async Task ImprovedTextReaderPositionStoreTestAsync()
    {
      using var impStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("txTranscripts.txt")));
      using var test = new ImprovedTextReader(impStream);
      var store = new ImprovedTextReaderPositionStore(test);
      Assert.AreEqual(1, test.LineNumber);
      Assert.AreEqual(
        "#UserID	CurriculumID	TranscriptStatus	RequestDateTime	RegistrationDateTime	CompletionDateTime",
        await test.ReadLineAsync());
      var lastLine = string.Empty;
      while (!store.AllRead()) lastLine = await test.ReadLineAsync();
      Assert.AreEqual(
        @"GCS_002846_Benavides	A23c25d3-3420-449c-a75b-0d74d29ddc38	Completed	13/03/2008 00:00:00	13/03/2008 00:00:00	13/03/2008 00:00:00",
        lastLine);
    }

		[TestMethod]
		public async Task ImprovedTextReaderPositionStoreTestFromMiddleAsync()
    {
      using var impStream = FunctionalDI.OpenStream(new SourceAccess(UnitTestStatic.GetTestPath("txTranscripts.txt")));
      using var test = new ImprovedTextReader(impStream, 65001, 1);
      Assert.AreEqual(2, test.LineNumber);
      Assert.AreEqual(
        @"GCS_004805_Osipova	023c25d3-3420-449c-a75b-0d74d29ddc38	Completed	04/02/2008 00:00:00	04/02/2008 00:00:00	04/02/2008 00:00:00",
        await test.ReadLineAsync());
      var lastLine1 = string.Empty;
      for (var i = 0; i < 5; i++)
        lastLine1 = await test.ReadLineAsync();
      var store = new ImprovedTextReaderPositionStore(test);
      var readLine = false;
      while (!store.AllRead())
      {
        var lastLine2 = await test.ReadLineAsync();
        // since there are buffers its we will not end up with the exact same line, but we need
        // to at least have read the line.
        if (lastLine2 == lastLine1)
          readLine = true;
      }

      Assert.IsTrue(readLine);
    }
	}
}