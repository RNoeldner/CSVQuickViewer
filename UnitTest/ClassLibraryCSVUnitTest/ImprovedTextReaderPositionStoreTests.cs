using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public class ImprovedTextReaderPositionStoreTests
  {
    [TestMethod]
    public void ImprovedTextReaderPositionStoreTest()
    {
      using (var impStream = new ImprovedStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("txTranscripts.txt"), true)))
      {
        using (var test = new ImprovedTextReader(impStream))
        {
          test.ToBeginning();
          var store = new ImprovedTextReaderPositionStore(test);
          Assert.AreEqual(1, test.LineNumber);
          Assert.AreEqual(
            "#UserID	CurriculumID	TranscriptStatus	RequestDateTime	RegistrationDateTime	CompletionDateTime",
            test.ReadLine());
          var lastLine = string.Empty;
          while (!store.AllRead()) lastLine = test.ReadLine();
          Assert.AreEqual(
            @"GCS_002846_Benavides	A23c25d3-3420-449c-a75b-0d74d29ddc38	Completed	13/03/2008 00:00:00	13/03/2008 00:00:00	13/03/2008 00:00:00",
            lastLine);
        }
      }
    }

    [TestMethod]
    public void ImprovedTextReaderPositionStoreTestFromMiddle()
    {
      using (var impStream = new ImprovedStream(new SourceAccess(UnitTestInitializeCsv.GetTestPath("txTranscripts.txt"), true)))
      {
        using (var test = new ImprovedTextReader(impStream, 65001, 1))
        {
          test.ToBeginning();
          Assert.AreEqual(2, test.LineNumber);
          Assert.AreEqual(
            @"GCS_004805_Osipova	023c25d3-3420-449c-a75b-0d74d29ddc38	Completed	04/02/2008 00:00:00	04/02/2008 00:00:00	04/02/2008 00:00:00",
            test.ReadLine());
          var lastLine1 = string.Empty;
          string lastLine2;
          for (var i = 0; i < 5; i++)
            lastLine1 = test.ReadLine();
          var store = new ImprovedTextReaderPositionStore(test);
          var readLine = false;
          while (!store.AllRead())
          {
            lastLine2 = test.ReadLine();
            // since there are buffers its we will not end up with the excact same line, but we need
            // to at least have read the line.
            if (lastLine2 == lastLine1)
              readLine = true;
          }

          Assert.IsTrue(readLine);
        }
      }
    }
  }
}