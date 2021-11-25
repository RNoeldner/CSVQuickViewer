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
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public class FormColumnUITests
  {
    [TestMethod]
    [Timeout(5000)]
    public void FormColumnUI()
    {
      var csvFile = new CsvFile(UnitTestStatic.GetTestPath("BasicCSV.txt"));
      var col = new Column("ExamDate", DataType.DateTime);
      csvFile.ColumnCollection.Add(col);

      using (var frm = new FormColumnUI(col, false, csvFile, new FillGuessSettings(), false, UnitTestStatic.HTMLStyle))
      {
        UnitTestStatic.ShowFormAndClose(frm);
      }
    }

    [TestMethod]
    [Timeout(2000)]
    public void FormColumnUI_Boolean()
    {
      var col = new Column("MyTest", DataType.Boolean) { True = "YO", False = "NOPE" };
      using (var frm = new FormColumnUI(col, false, new CsvFile(UnitTestStatic.GetTestPath("BasicCSV.txt")), new FillGuessSettings(), true,
        UnitTestStatic.HTMLStyle))
      {
        UnitTestStatic.ShowFormAndClose(frm);
      }
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormColumnUI_DateTime()
    {
      var col = new Column("MyTest", DataType.DateTime) { DateFormat = "dd/MM/yyyy", DateSeparator = ".", TimeSeparator = ":" };

      var df = new ValueFormatMutable() { DataType = DataType.DateTime, DateFormat = "dd/MMM/yyy", DateSeparator = "-", TimeSeparator = "#" };
      using (var frm = new FormColumnUI(col, false, new CsvFile(UnitTestStatic.GetTestPath("BasicCSV.txt")), new FillGuessSettings(), true,
        UnitTestStatic.HTMLStyle))
        UnitTestStatic.ShowFormAndClose(frm, .1, f => f.UpdateDateLabel(df, true, "HH:mm", "[UTC]"));

      using (var frm = new FormColumnUI(col, false, new CsvFile(UnitTestStatic.GetTestPath("BasicCSV.txt")), new FillGuessSettings(), true,
        UnitTestStatic.HTMLStyle))
        UnitTestStatic.ShowFormAndClose(frm, .1, f => f.UpdateDateLabel(df, false, "HH:mm:ss", "OtherColumn"));

      using (var frm = new FormColumnUI(col, false, new CsvFile(UnitTestStatic.GetTestPath("BasicCSV.txt")), new FillGuessSettings(), true,
        UnitTestStatic.HTMLStyle))
        UnitTestStatic.ShowFormAndClose(frm, .1, f => f.AddDateFormat("dd MMM yy HH:mm tt"));
    }

    [TestMethod]
    [Timeout(15000)]
    public async Task FormColumnUI_DisplayValues()
    {
      var csvFile = new CsvFile(UnitTestStatic.GetTestPath("BasicCSV.txt"));
      var col = new Column("ExamDate", DataType.DateTime);
      csvFile.ColumnCollection.Add(col);

      using (var form = new FormColumnUI(col, false, csvFile, new FillGuessSettings(), true, UnitTestStatic.HTMLStyle))
      {
        await UnitTestStatic.ShowFormAndCloseAsync(form, .2, frm => frm.DisplayValues(), UnitTestStatic.Token);
      }
    }

    [TestMethod]
    [Timeout(15000)]
    public async Task FormColumnUI_Guess()
    {
      var csvFile = new CsvFile(UnitTestStatic.GetTestPath("BasicCSV.txt"));
      var col = new Column("ExamDate", DataType.DateTime);
      csvFile.ColumnCollection.Add(col);
      using (var form = new FormColumnUI(col, false, csvFile, new FillGuessSettings(), true, UnitTestStatic.HTMLStyle))
      {
        await UnitTestStatic.ShowFormAndCloseAsync(form, .2, frm => frm.Guess(), UnitTestStatic.Token);
      }
    }

    [TestMethod]
    [Timeout(2000)]
    public void FormColumnUI_Numeric()
    {
      var col = new Column("MyTest", DataType.Numeric) { DecimalSeparator = ".", GroupSeparator = ",", NumberFormat = "0.00" };

      using (var frm = new FormColumnUI(col, false, new CsvFile(UnitTestStatic.GetTestPath("BasicCSV.txt")), new FillGuessSettings(), true,
        UnitTestStatic.HTMLStyle))
      {
        UnitTestStatic.ShowFormAndClose(frm, .1, f => f.UpdateNumericLabel(".", "00000", ""));
      }
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormColumnUI_Opt1()
    {
      var csvFile = new CsvFile(UnitTestStatic.GetTestPath("BasicCSV.txt"));
      var col = new Column("ExamDate", DataType.DateTime);
      csvFile.ColumnCollection.Add(col);

      using (var form = new FormColumnUI(col, false, csvFile, new FillGuessSettings(), true, UnitTestStatic.HTMLStyle))
      {
        form.ShowGuess = false;
        UnitTestStatic.ShowFormAndClose(form);
      }
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormColumnUI_Opt2()
    {
      var csvFile = new CsvFile(UnitTestStatic.GetTestPath("BasicCSV.txt"));
      var col = new Column("ID", DataType.Integer);
      csvFile.ColumnCollection.Add(col);

      using (var form = new FormColumnUI(col, false, csvFile, new FillGuessSettings(), false, UnitTestStatic.HTMLStyle))
      {
        UnitTestStatic.ShowFormAndClose(form);
      }
    }

    [TestMethod]
    [Timeout(2000)]
    public void FormColumnUI_TextPart()
    {
      var col = new Column("MyTest", DataType.TextPart) { PartSplitter = ":", Part = 2, PartToEnd = true };
      using (var frm = new FormColumnUI(col, false, new CsvFile(UnitTestStatic.GetTestPath("BasicCSV.txt")), new FillGuessSettings(), true,
        UnitTestStatic.HTMLStyle))
      {
        UnitTestStatic.ShowFormAndClose(frm, .1, f => f.SetPartLabels(":", 2, true));
      }
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormColumnUIGetColumnHeader2()
    {
      var csvFile = new CsvFile { ID = "Csv", FileName = UnitTestStatic.GetTestPath("BasicCSV.txt") };
      var col = new Column("Score", DataType.Double);
      csvFile.ColumnCollection.Add(col);

      using (var form = new FormColumnUI(col, false, csvFile, new FillGuessSettings(), true, UnitTestStatic.HTMLStyle))
      {
        UnitTestStatic.ShowFormAndClose(form);
      }
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormColumnUIGetColumnHeaderAsync()
    {
      var csvFile = new CsvFile(UnitTestStatic.GetTestPath("BasicCSV.txt")) { ID = "Csv" };

      csvFile.ColumnCollection.Add(new Column("ID", DataType.Integer));
      var col = new Column("ExamDate", DataType.DateTime);
      csvFile.ColumnCollection.Add(col);
      csvFile.ColumnCollection.Add(new Column("Score", DataType.Double));

      using (var form = new FormColumnUI(col, false, csvFile,
        new FillGuessSettings(), true, UnitTestStatic.HTMLStyle))
      {
        UnitTestStatic.ShowFormAndClose(form);
      }
    }
  }
}