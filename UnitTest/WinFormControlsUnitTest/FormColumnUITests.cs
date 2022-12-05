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
    [TestMethod]
    [Timeout(5000)]
    public void FormColumnUI()
    {
      var csvFile = new CsvFile(UnitTestStatic.GetTestPath("BasicCSV.txt"), "csv");
      var col = new Column("ExamDate", new ValueFormat(DataTypeEnum.DateTime));
      csvFile.ColumnCollection.Add(col);

      using var frm = new FormColumnUI(col, false, csvFile, FillGuessSettings.Default, false);
      UnitTestStatic.ShowFormAndClose(frm);
    }

    [TestMethod]
    [Timeout(2000)]
    public void FormColumnUI_Boolean()
    {
      var col = new Column("MyTest", new ValueFormat(DataTypeEnum.Boolean, asTrue: "YO", asFalse: "NOPE"));
      using var frm = new
        FormColumnUI(col, false, new CsvFile(UnitTestStatic.GetTestPath("BasicCSV.txt"), "csv"), FillGuessSettings.Default,
          true);
      UnitTestStatic.ShowFormAndClose(frm);
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormColumnUI_DateTime()
    {
      var csvFile = new CsvFile(UnitTestStatic.GetTestPath("BasicCSV.txt"), "csv");
      var col = new Column("MyTest",
        new ValueFormat(DataTypeEnum.DateTime, dateFormat: "dd/MM/yyyy", dateSeparator: ".", timeSeparator: ":"));

      using var form = new FormColumnUI(col, false, csvFile, FillGuessSettings.Default, true);
      UnitTestStatic.ShowFormAndClose(form);
    }


    [TestMethod]
    [Timeout(5000)]
    public void FormColumnUI_Opt1()
    {
      var csvFile = new CsvFile(UnitTestStatic.GetTestPath("BasicCSV.txt"), "csv");
      var col = new Column("ExamDate", new ValueFormat(DataTypeEnum.DateTime));
      csvFile.ColumnCollection.Add(col);

      using var form = new FormColumnUI(col, false, csvFile, FillGuessSettings.Default, true);
      UnitTestStatic.ShowFormAndClose(form);
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormColumnUI_Opt2()
    {
      var csvFile = new CsvFile(UnitTestStatic.GetTestPath("BasicCSV.txt"), "csv");
      var col = new Column("ID", new ValueFormat(DataTypeEnum.Integer));
      csvFile.ColumnCollection.Add(col);

      using var form = new FormColumnUI(col, false, csvFile, FillGuessSettings.Default, false);
      UnitTestStatic.ShowFormAndClose(form);
    }

    [TestMethod]
    [Timeout(2000)]
    public void FormColumnUI_TextPart()
    {
      var col = new Column("MyTest",
        new ValueFormat(DataTypeEnum.TextPart, partSplitter: ":", part: 2, partToEnd: true));
      using var frm = new FormColumnUI(col, false, new CsvFile(UnitTestStatic.GetTestPath("BasicCSV.txt"), "csv"),
        FillGuessSettings.Default, true);
      UnitTestStatic.ShowFormAndClose(frm, .1);
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormColumnUIGetColumnHeader2()
    {
      var csvFile = new CsvFile(UnitTestStatic.GetTestPath("BasicCSV.txt"), "Csv" );

      var col = new Column("Score", new ValueFormat(DataTypeEnum.Double));
      csvFile.ColumnCollection.Add(col);

      using var form = new FormColumnUI(col, false, csvFile, FillGuessSettings.Default, true);
      UnitTestStatic.ShowFormAndClose(form);
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormColumnUIGetColumnHeaderAsync()
    {
      var csvFile = new CsvFile(UnitTestStatic.GetTestPath("BasicCSV.txt"), "Csv");

      csvFile.ColumnCollection.Add(new Column("ID", new ValueFormat(DataTypeEnum.Integer)));
      var col = new Column("ExamDate", new ValueFormat(DataTypeEnum.DateTime));
      csvFile.ColumnCollection.Add(col);
      csvFile.ColumnCollection.Add(new Column("Score", new ValueFormat(DataTypeEnum.Double)));

      using var form = new FormColumnUI(col, false, csvFile,
        FillGuessSettings.Default, true);
      UnitTestStatic.ShowFormAndClose(form);
    }
  }
}