using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CsvTools.Tests
{
  [TestClass]
  public class ExtensionsTests
  {
    [TestMethod]
    public void UpdateListViewColumnFormatTest()
    {
      var lv = new ListView();
      var colFmt = new List<Column>();

      {
        var item = lv.Items.Add("Test");
        item.Selected = true;
      }
      lv.UpdateListViewColumnFormat(colFmt);
      Assert.AreEqual(0, lv.Items.Count);

      {
        lv.Items.Add("Test1");
        var item = lv.Items.Add("Test");
        item.Selected = true;
      }

      colFmt.Add(new Column { Name = "Test" });
      lv.UpdateListViewColumnFormat(colFmt);
    }

    [TestMethod()]
    public void WriteBindingTest()
    {
      var obj = new DisplayItem<string>("15", "Text");
      var bindrc = new BindingSource
      {
        DataSource = obj
      };
      var bind = new Binding("Text", bindrc, "ID", true);
      var crtl = new TextBox();
      crtl.DataBindings.Add(bind);
      crtl.Text = "12";

      Assert.AreEqual(bind, crtl.GetTextBindng());
      crtl.WriteBinding();
    }

    [TestMethod()]
    public void DeleteFileQuestionTest()
    {
      Assert.AreEqual(true, ".\\Test.hshsh".DeleteFileQuestion(false));
    }

    [TestMethod()]
    public void GetEncryptedPassphraseTest()
    {
      var setting = new CsvFile()
      {
        FileName = "Test.pgp"
      };
      ApplicationSetting.ToolSetting.PGPInformation.AddPrivateKey(PGPKeyStorageTestHelper.PRIVATE);
      ApplicationSetting.ToolSetting.PGPInformation.EncryptedPassphase = "Hello";
      Assert.AreEqual("Hello", setting.GetEncryptedPassphraseOpenForm());
      setting.Passphrase = "World";
      Assert.AreEqual("World", setting.GetEncryptedPassphraseOpenForm());
    }

    [TestMethod()]
    public void GetProcessDisplayTitleTest()
    {
      var setting = new CsvFile()
      {
        FileName = "Folder\\Folder\\This is a very long file name that should be cut and then fit into 80 chars.txt"
      };
      Assert.AreEqual("This is a very long file name that should be cut and then fit into 80 chars.txt", setting.GetProcessDisplayTitle());
    }

    [TestMethod()]
    public void GetProcessDisplayTest()
    {
      var setting = new CsvFile()
      {
        FileName = "Folder\\This is a long file name that should be cut and fit into 80 chars.txt",
        ShowProgress = true
      };
      Assert.IsTrue(setting.GetProcessDisplay(null, true, System.Threading.CancellationToken.None) is IProcessDisplay, "GetProcessDisplay With Logger");
      Assert.IsTrue(setting.GetProcessDisplay(null, false, System.Threading.CancellationToken.None) is IProcessDisplay, "GetProcessDisplay Without Logger");
      var setting2 = new CsvFile()
      {
        FileName = "Folder\\This is a long file name that should be cut and fit into 80 chars.txt",
        ShowProgress = false
      };
      Assert.IsTrue(setting2.GetProcessDisplay(null, false, System.Threading.CancellationToken.None) is IProcessDisplay, "GetProcessDisplay without UI");
    }

    [TestMethod()]
    public void LoadWindowStateTest()
    {
    }

    [TestMethod()]
    public void SafeBeginInvokeTest()
    {
    }

    [TestMethod()]
    public void SafeInvokeTest()
    {
    }

    [TestMethod()]
    public void SafeInvokeNoHandleNeededTest()
    {
    }

    [TestMethod()]
    public void StoreWindowStateTest()
    {
    }

    [TestMethod()]
    public void UpdateListViewColumnFormatTest1()
    {
    }

    [TestMethod()]
    public void WriteFileWithInfoTest()
    {
    }
  }
}