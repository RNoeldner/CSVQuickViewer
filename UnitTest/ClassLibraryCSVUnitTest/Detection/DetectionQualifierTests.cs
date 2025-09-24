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
      /* Quoting1Reverse.txt is the same as Quoting1.txt but with ' as qualifier
       * Its very ounlikly to observe a stange file like this */
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
