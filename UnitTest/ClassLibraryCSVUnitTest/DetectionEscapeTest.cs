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
using System.Text;
using System.Threading.Tasks;
// ReSharper disable ConvertToUsingDeclaration

namespace CsvTools.Tests
{
  [TestClass]
  public class DetectionEscapeTest
  {

    [TestMethod]
    public async Task GuessHasHeaderAsync_NotEscaped()
    {
      using (var stream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCSV.txt"))))
      using (var reader = new ImprovedTextReader(stream))
      {
        Assert.AreEqual(char.MinValue, await reader.InspectEscapePrefixAsync(',', '"', UnitTestStatic.Token));
      }
      using (var stream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicCSV.txt"))))
      {
        using var textReader = await stream.GetTextReaderAsync(Encoding.UTF8.CodePage, 0, UnitTestStatic.Token);
        Assert.AreEqual(char.MinValue, await textReader.InspectEscapePrefixAsync(',', '"', UnitTestStatic.Token).ConfigureAwait(false));
      }
    }

    [TestMethod]
    public async Task GuessHasHeaderAsync_EscapedCharacterAtEndOfRowDelimiter()
    {
      using var stream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("EscapedCharacterAtEndOfRowDelimiter.txt")));
      using var reader = new ImprovedTextReader(stream);
      Assert.AreEqual(char.MinValue, await reader.InspectEscapePrefixAsync(',', '"', UnitTestStatic.Token));
    }


    [TestMethod]
    public async Task GuessHasHeaderAsync_Escaped()
    {
      using (var stream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicEscapedCharacters.txt"))))
      {
        using var reader = new ImprovedTextReader(stream);
        Assert.AreEqual('\\', await reader.InspectEscapePrefixAsync(',', '"', UnitTestStatic.Token));
      }

      using (var stream = new ImprovedStream(new SourceAccess(UnitTestStatic.GetTestPath("BasicEscapedCharacters.txt"))))
      {
        using var textReader = await stream.GetTextReaderAsync(Encoding.UTF8.CodePage, 0, UnitTestStatic.Token);
        Assert.AreEqual('\\', await textReader.InspectEscapePrefixAsync(',', '"', UnitTestStatic.Token).ConfigureAwait(false));
      }
    }
  }
}
