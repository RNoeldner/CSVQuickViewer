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

using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class FormsTest
  {
    private readonly string m_ApplicationDirectory = FileSystemUtils.ExecutableDirectoryName() + @"\TestFiles";

    [TestMethod]
    public void FormEditSettings()
    {
      using (var frm = new FormEditSettings(new ViewSettings()))
      {
        frm.Show();
        Application.DoEvents();
        System.Threading.Thread.Sleep(200);
      }
    }

    [TestMethod]
    public void FormMain_BasicCSV()
    {
      using (var frm = new FormMain(Path.Combine(m_ApplicationDirectory, "BasicCSV.txt.gz")))
      {
        frm.Show();
        while (!frm.LoadFinished)
        {
          Application.DoEvents();
          Thread.Sleep(200);
        }
        Assert.IsNotNull(frm.DataTable);
        Assert.AreEqual(7, frm.DataTable.Rows.Count);
      }
    }

    [TestMethod]
    public void FormMain_AllFormatsPipe()
    {
      using (var frm = new FormMain(Path.Combine(m_ApplicationDirectory, "AllFormatsPipe.txt")))
      {
        frm.Show();
        while (!frm.LoadFinished)
        {
          Application.DoEvents();
          Thread.Sleep(200);
        }
        Assert.IsNotNull(frm.DataTable);
        Assert.AreEqual(45, frm.DataTable.Rows.Count);
      }
    }
  }
}