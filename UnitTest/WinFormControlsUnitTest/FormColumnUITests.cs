/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
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

namespace CsvTools.Tests
{
  [TestClass]
  public class FormColumnUITests
  {
    [ClassInitialize]
    public static void SetupTests(TestContext testContext)
    {
      // need to set FileReaderWriterFactory  for FormColumnUiRead
      FunctionalDI.FileReaderWriterFactory = new ClassLibraryCsvFileReaderWriterFactory(StandardTimeZoneAdjust.ChangeTimeZone, new FillGuessSettings());
    }

    [TestMethod]
    [Timeout(2000)]
    public void FormColumnUI()
    {
      var csvFile = new CsvFileDummy { FileName = UnitTestStatic.GetTestPath("BasicCSV.txt") };
      var col = new Column("ExamDate", new ValueFormat(DataTypeEnum.DateTime));
      csvFile.ColumnCollection.Add(col);

      UnitTestStaticForms.ShowForm(() => new FormColumnUiRead(col, csvFile, FillGuessSettings.Default, false, true, true));
    }

    [TestMethod]
    [Timeout(2000)]
    public void FormColumnUI_Boolean()
    {
      var col = new Column("MyTest", new ValueFormat(DataTypeEnum.Boolean, asTrue: "YO", asFalse: "NOPE"));
      UnitTestStaticForms.ShowForm(() => new
        FormColumnUiRead(col, new CsvFileDummy() { FileName = UnitTestStatic.GetTestPath("BasicCSV.txt") },
          FillGuessSettings.Default,
          true, true, true));

    }

    [TestMethod]
    [Timeout(1000)]
    public void FormColumnUI_DateTime()
    {
      var csvFile = new CsvFileDummy() { FileName= UnitTestStatic.GetTestPath("BasicCSV.txt") };
      var col = new Column("MyTest",
        new ValueFormat(DataTypeEnum.DateTime, dateFormat: "dd/MM/yyyy", dateSeparator: ".", timeSeparator: ":"));

      UnitTestStaticForms.ShowForm(() => new FormColumnUiRead(col, csvFile, FillGuessSettings.Default, false, true, false));
    }


    [TestMethod]
    [Timeout(1000)]
    public void FormColumnUI_Opt1()
    {
      var csvFile = new CsvFileDummy() { FileName=  UnitTestStatic.GetTestPath("BasicCSV.txt") };
      var col = new Column("ExamDate", new ValueFormat(DataTypeEnum.DateTime));
      csvFile.ColumnCollection.Add(col);

      UnitTestStaticForms.ShowForm(() => new FormColumnUiRead(col, csvFile, FillGuessSettings.Default, true, true, true));
    }

    [TestMethod]
    [Timeout(2000)]
    public void FormColumnUI_Opt2()
    {
      var csvFile = new CsvFileDummy() { FileName = UnitTestStatic.GetTestPath("BasicCSV.txt") };
      var col = new Column("ID", new ValueFormat(DataTypeEnum.Integer));
      csvFile.ColumnCollection.Add(col);

      UnitTestStaticForms.ShowForm(() => new FormColumnUiRead(col, csvFile, FillGuessSettings.Default, false, true, true));
    }

    [TestMethod]
    [Timeout(2000)]
    public void FormColumnUI_TextPart()
    {
      var col = new Column("MyTest",
        new ValueFormat(DataTypeEnum.TextPart, partSplitter: ":", part: 2, partToEnd: true));
      UnitTestStaticForms.ShowForm(() => new FormColumnUiRead(col,
        new CsvFileDummy() { FileName= UnitTestStatic.GetTestPath("BasicCSV.txt") }, FillGuessSettings.Default, true, true, true));
    }

    [TestMethod]
    [Timeout(2000)]
    public void FormColumnUIGetColumnHeader2()
    {
      var csvFile = new CsvFileDummy() { FileName= UnitTestStatic.GetTestPath("BasicCSV.txt") };

      var col = new Column("Score", new ValueFormat(DataTypeEnum.Double));
      csvFile.ColumnCollection.Add(col);

      UnitTestStaticForms.ShowForm(() => new FormColumnUiRead(col, csvFile, FillGuessSettings.Default, true, true, true));
    }

    [TestMethod]
    [Timeout(1000)]
    public void FormColumnUIGetColumnHeaderAsync()
    {
      var csvFile = new CsvFileDummy() { FileName = UnitTestStatic.GetTestPath("BasicCSV.txt") };

      csvFile.ColumnCollection.Add(new Column("ID", new ValueFormat(DataTypeEnum.Integer)));
      var col = new Column("ExamDate", new ValueFormat(DataTypeEnum.DateTime));
      csvFile.ColumnCollection.Add(col);
      csvFile.ColumnCollection.Add(new Column("Score", new ValueFormat(DataTypeEnum.Double)));

      UnitTestStaticForms.ShowForm(() => new FormColumnUiRead(col, csvFile, FillGuessSettings.Default, true, true, true));
    }
  }
}