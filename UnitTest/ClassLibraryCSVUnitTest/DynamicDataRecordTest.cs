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
using System.Threading.Tasks;

namespace CsvTools.Tests;

[TestClass()]
public class DynamicDataRecordTest
{
  [TestMethod()]
  public async Task GetDynamicMemberNames()
  {
    
    using (var reader = new CsvFileReader(UnitTestStatic.GetTestPath("AllFormats.txt"),
             65001, 0, 0, true, null, TrimmingOptionEnum.Unquoted, '\t', '"'
             , char.MinValue, 0, false,
false, "", 0,
true, "", "",
"", true, false,
true, false, false,
false, false, false, false,
false, false, "",
true, 1, "ID", System.TimeZoneInfo.Local.Id, true, false))
    {
      await reader.OpenAsync(UnitTestStatic.Token);
      await reader.ReadAsync();

      dynamic test = new DynamicDataRecord(reader);
      //Assert.AreEqual(-22477, test.Integer);

      await reader.ReadAsync();
      test = new DynamicDataRecord(reader);
      //Assert.AreEqual("zehzcv", test.String);
      //test.String = "2222";
      //Assert.AreEqual("2222", test.String);
    }
  }
}