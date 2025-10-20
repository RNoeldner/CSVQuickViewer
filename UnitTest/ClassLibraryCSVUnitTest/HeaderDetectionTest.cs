/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
 * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License along with this program.
 * If not, see http://www.gnu.org/licenses/ .
 *
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable UseAwaitUsing
// ReSharper disable ConvertToUsingDeclaration

namespace CsvTools.Tests
{
  [TestClass]
  public class HeaderDetectionTest
  {
    [TestMethod]
    public async Task GuessHasHeaderAsync()
    {
      using (var stream =
             new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("HandlingDuplicateColumnNames.txt"))))
      {
        using var reader = new ImprovedTextReader(stream);
        var res = await reader.InspectHasHeaderAsync(',', char.MinValue, char.MinValue, "#", UnitTestStatic.Token);
        Assert.IsFalse(string.IsNullOrEmpty( res.message));
        Assert.IsTrue(res.hasHeader);
      }

      using (var stream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCSV.txt"))))
      {
        using var reader = new ImprovedTextReader(stream);
        Assert.AreEqual("", (await reader.InspectHasHeaderAsync(',', char.MinValue, char.MinValue, "#", UnitTestStatic.Token)).message);
      }



      using (var stream =
             new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("DifferentColumnDelimiter.txt"))))
      {
        using var reader = new ImprovedTextReader(stream);
        Assert.IsFalse(
          string.IsNullOrEmpty((await reader.InspectHasHeaderAsync(',', char.MinValue, char.MinValue, "", UnitTestStatic.Token)).message));
      }

      using (var stream = new FileStream(UnitTestStatic.GetTestPath("Sessions.txt"), FileMode.Open))
      {
        using var reader = new ImprovedTextReader(stream);
        Assert.AreEqual("", (await reader.InspectHasHeaderAsync('\t', char.MinValue, char.MinValue, "#", UnitTestStatic.Token)).message);
      }    
    }

    [TestMethod]
    public async Task GuessHeaderAllFormatsAsync()
    {
      using var improvedStream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("AllFormats.txt")));
      using var reader = new ImprovedTextReader(improvedStream);

      var result = await reader.InspectHasHeaderAsync('\t', char.MinValue, char.MinValue, "", UnitTestStatic.Token);      
      Assert.IsTrue(string.IsNullOrEmpty(result.message));
    }

    [TestMethod]
    public async Task GuessHeaderAllFormatsAsync2()
    {
      using var improvedStream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("AllFormats.txt")));
      // this time skipping the firs row we start wih a adata row
      using var reader = new ImprovedTextReader(improvedStream,skipLines:1);

      var result = await reader.InspectHasHeaderAsync('\t', char.MinValue, char.MinValue, "", UnitTestStatic.Token);
      Assert.AreNotEqual("",result.message);
    }

    [TestMethod]
    public async Task GuessHeaderBasicCsv()
    {
      using var improvedStream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCSV.txt")));
      using var reader = await improvedStream.GetTextReaderAsync(1200, 0, UnitTestStatic.Token);
      var result = await reader.InspectHasHeaderAsync(',', char.MinValue, char.MinValue, "", UnitTestStatic.Token);
      Assert.IsNotNull(result);
      Assert.IsTrue(string.IsNullOrEmpty(result.message));
    }

    [TestMethod]
    public async Task GuessHeaderBasicEscapedCharactersAsync()
    {
      
      using var improvedStream =
        new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicEscapedCharacters.txt")));
      using var reader = await improvedStream.GetTextReaderAsync(65001, 0, UnitTestStatic.Token);
      /* The file has very uncommon headers as they are all very short... 
       * its debaltable if these should be reagrded as header they could be regular columns as well */
      var res = await reader.InspectHasHeaderAsync(',', '"', '\\', "", UnitTestStatic.Token);      
      Assert.IsTrue(res.hasHeader);
    }

    [TestMethod]
    public async Task GuessHeaderLongHeadersAsync()
    {
      using var improvedStream =
        new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("LongHeaders.txt")));
      using var reader = await improvedStream.GetTextReaderAsync(65001, 0, UnitTestStatic.Token);
      var result = await reader.InspectHasHeaderAsync(',', '"', '\\', "#", UnitTestStatic.Token);
      
      Assert.IsTrue(result.message.StartsWith("Header", StringComparison.OrdinalIgnoreCase), result.message);
      Assert.IsTrue(result.message.EndsWith("too long", StringComparison.OrdinalIgnoreCase), result.message);
    }

    [TestMethod]
    public async Task GuessHeaderStrangeHeaders()
    {
      using var improvedStream =
        new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("StrangeHeaders.txt")));
      using var reader = await improvedStream.GetTextReaderAsync(1200, 0, UnitTestStatic.Token);
      var result = await reader.InspectHasHeaderAsync(',', char.MinValue, char.MinValue, "", UnitTestStatic.Token);
      Assert.IsNotNull(result);
      Assert.IsFalse(result.hasHeader);
    }

    [TestMethod]
    public void ParseLineText1()
    {
      using var stream = new MemoryStream();
      var writer = new StreamWriter(stream);
      writer.Write("1,2,3,4,5,6,7,8,9");
      writer.Flush();
      stream.Position = 0;
      using var reader = new ImprovedTextReader(stream, Encoding.UTF8.CodePage);

      var res = DetectionHeader.DelimitedRecord(reader, ',', char.MinValue, char.MinValue, "##").ToList();
      Assert.AreEqual(9, res.Count());
      Assert.AreEqual("1", res[0]);
      Assert.AreEqual("9", res[8]);
    }

    [TestMethod]
    public void ParseLineText2()
    {
      using var stream = new MemoryStream();
      var writer = new StreamWriter(stream);
      writer.Write("\"1\",2,3,4,5,\"6,\"\"7,8\",\"9\"");
      writer.Flush();
      stream.Position = 0;
      using var reader = new ImprovedTextReader(stream, Encoding.UTF8.CodePage);
      var res = DetectionHeader.DelimitedRecord(reader, ',', '"', '\\', "").ToList();
      Assert.AreEqual(7, res.Count());
      Assert.AreEqual("1", res[0]);
      Assert.AreEqual("6,\"7,8", res[5]);
      Assert.AreEqual("9", res[6]);
    }

    [TestMethod]
    public void ParseLineText3()
    {
      using var stream = new MemoryStream();
      var writer = new StreamWriter(stream);
      writer.Write("1\\,2,3,4,5,\"6,\\\"7,8\",9");
      writer.Flush();
      stream.Position = 0;
      using var reader = new ImprovedTextReader(stream, Encoding.UTF8.CodePage);
      var res = DetectionHeader.DelimitedRecord(reader, ',', '"', '\\', "#").ToList();
      Assert.AreEqual(6, res.Count());
      Assert.AreEqual("1,2", res[0]);
      Assert.AreEqual("6,\"7,8", res[4]);
      Assert.AreEqual("9", res[5]);
    }


    [TestMethod]
    public void ParseLineText4()
    {
      using var stream = new MemoryStream();
      var writer = new StreamWriter(stream);
      writer.Write("1\\,2,3,4,5,\"6,\"\"7,8\",\"9\r\nnl\"");
      writer.Flush();
      stream.Position = 0;
      using var reader = new ImprovedTextReader(stream, Encoding.UTF8.CodePage);
      var res = DetectionHeader.DelimitedRecord(reader, ',', '"', '\\', "#").ToList();
      Assert.AreEqual(6, res.Count());
      Assert.AreEqual("1,2", res[0]);
      Assert.AreEqual("6,\"7,8", res[4]);
      Assert.AreEqual("9\r\nnl", res[5]);
    }
  }
}

