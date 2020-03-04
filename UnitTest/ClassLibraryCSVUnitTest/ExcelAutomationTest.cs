using CsvTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace CsvFileTest_UnitTest
{
  [TestClass]
  public class ExcelAutomationTest
  {
    private string m_ApplicationDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\TestFiles";
    private string m_Output;

    [TestInitialize]
    public void Init()
    {
      m_Output = System.IO.Path.Combine(m_ApplicationDirectory, "ExcelAutomationStore.txt");
      if (File.Exists(m_Output))
        File.Delete(m_Output);
    }

    [TestMethod]
    public void ExcelFileReaderOpenWrongSheetName()
    {
      try
      {
        ExcelFile file = new ExcelFile();
        file.FileName = System.IO.Path.Combine(m_ApplicationDirectory, "DateTimeCombineExcel.xlsx");
        file.SheetName = "Combin ations";
        file.SheetRange = null;
        file.ShowProgress = false;

        using (ExcelFileReaderInterop reader = new ExcelFileReaderInterop())
        {
          reader.Open(file, System.Threading.CancellationToken.None, false);
          reader.SaveSheetAsText(null, null, m_Output);
        }
      }
      catch (Exception)
      {
        return;
      }
      Assert.Fail("Expected Exception");
    }

    [TestMethod]
    public void SaveSheetAsTextNoRangeProcess()
    {
      ExcelFile file = new ExcelFile();
      file.FileName = System.IO.Path.Combine(m_ApplicationDirectory, "DateTimeCombineExcel.xlsx");
      file.SheetName = "Combinations";
      file.SheetRange = null;
      file.ShowProgress = true;
      using (ExcelFileReaderInterop reader = new ExcelFileReaderInterop())
      {
        reader.Open(file, System.Threading.CancellationToken.None, false);
        reader.SaveSheetAsText(null, null, m_Output);
      }
      Assert.IsTrue(File.Exists(m_Output));
    }

    [TestMethod]
    public void SaveSheetAsTextWithRange()
    {
      ExcelFile file = new ExcelFile();
      file.FileName = System.IO.Path.Combine(m_ApplicationDirectory, "DateTimeCombineExcel.xlsx");
      file.SheetName = "Combinations";
      file.SheetRange = "A12:D24";
      file.ShowProgress = false;
      using (ExcelFileReaderInterop reader = new ExcelFileReaderInterop())
      {
        reader.Open(file, System.Threading.CancellationToken.None, false);
        reader.SaveSheetAsText(null, null, m_Output);
      }
      Assert.IsTrue(File.Exists(m_Output));
    }

    [TestMethod]
    public void OpenColoseWithRange()
    {
      ExcelFile file = new ExcelFile();
      file.FileName = System.IO.Path.Combine(m_ApplicationDirectory, "DateTimeCombineExcel.xlsx");
      file.SheetName = "Combinations";
      file.SheetRange = "A12:D24";
      file.ShowProgress = false;
      using (ExcelFileReaderInterop reader = new ExcelFileReaderInterop())
      {
        reader.Open(file, System.Threading.CancellationToken.None, true);
        Assert.IsFalse(reader.IsClosed);
        reader.Close();
        Assert.IsTrue(reader.IsClosed);
      }


    }
    [TestMethod]
    public void OpenwithoutRangeProcess()
    {
      ExcelFile file = new ExcelFile();
      file.FileName = System.IO.Path.Combine(m_ApplicationDirectory, "DateTimeCombineExcel.xlsx");
      file.SheetName = "Combinations";
      file.ShowProgress = true;
      file.HasFieldHeader = true;

      using (ExcelFileReaderInterop reader = new ExcelFileReaderInterop())
      {
        using (FormProcessDisplay disp = new FormProcessDisplay("Test", null, null))
        {
          reader.ProcessDisplay = disp;          
          reader.Open(file, System.Threading.CancellationToken.None, true);
          Assert.IsFalse(reader.IsClosed);
          reader.Close();
          Assert.IsTrue(reader.IsClosed);
        }
      }
    }

    [TestMethod]
    public void SaveSheetAsTextWithRangeAndProcess()
    {
      ExcelFile file = new ExcelFile();
      file.FileName = System.IO.Path.Combine(m_ApplicationDirectory, "DateTimeCombineExcel.xlsx");
      file.SheetName = "Combinations";
      file.SheetRange = "B1:B5";
      file.ShowProgress = false;
      using (ExcelFileReaderInterop reader = new ExcelFileReaderInterop())
      {
        using (FormProcessDisplay disp = new FormProcessDisplay("Test", null, null))
        {
          reader.ProcessDisplay = disp;
          reader.Open(file, System.Threading.CancellationToken.None, true);
          reader.SaveSheetAsText(disp, null, m_Output);
        }
      }
      Assert.IsTrue(File.Exists(m_Output));
    }
  }
}
