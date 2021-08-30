using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvTools.Tests
{
  [TestClass]
  public class FormHierarchyDisplayTest
  {
    private readonly HTMLStyle m_HTMLStyle = new HTMLStyle();

    [TestMethod]
    [Timeout(5000)]
    public void MultiselectTreeViewRegular()
    {
      using (var treeView = new MultiselectTreeView())
      {
        treeView.HTMLStyle = m_HTMLStyle;
        Assert.AreEqual(0, treeView.SelectedTreeNode.Count);

        var treeNode = new TreeNode("Test") { Tag = "test" };
        treeView.Nodes.Add(treeNode);

        var treeNode2 = new TreeNode("Test2") { Tag = "test2" };
        treeNode.Nodes.Add(treeNode2);

        var treeNode3 = new TreeNode("Test3") { Tag = "test3" };
        treeNode.Nodes.Add(treeNode3);

        var treeNode3a = new TreeNode("Test3a") { Tag = "test3" };
        treeNode3.Nodes.Add(treeNode3a);
        var treeNode3b = new TreeNode("Test3b") { Tag = "test3" };
        treeNode3.Nodes.Add(treeNode3b);

        var firedAfter = false;
        var firedBefore = false;
        treeView.AfterSelect += (s, args) => { firedAfter = true; };
        treeView.BeforeSelect += (s, args) => { firedBefore = true; };

        UnitTestWinFormHelper.ShowControl(treeView, .2, (control, form) =>
        {
          if (!(control is MultiselectTreeView text))
            return;

          text.PressKey(Keys.Control | Keys.A);
          text.PressKey(Keys.Control | Keys.C);
          Application.DoEvents();
          treeView.SelectedNode = treeNode2;
          treeView.SelectAll();
          Application.DoEvents();
          var result = treeView.SelectedToClipboard();
          Assert.IsTrue(result.Contains(treeNode.Text));
          Assert.IsTrue(result.Contains(treeNode2.Text));
          Assert.IsTrue(result.Contains(treeNode3.Text));
        });
        Assert.IsTrue(firedAfter);
        Assert.IsTrue(firedBefore);
      }
    }

    [TestMethod]
    [Timeout(5000)]
    public void MultiselectTreeViewTreeData()
    {
      using (var treeView = new MultiselectTreeView())
      {
        treeView.HTMLStyle = m_HTMLStyle;
        Assert.AreEqual(0, treeView.SelectedTreeNode.Count);

        var td1 = new FormHierarchyDisplay.TreeData("1", "Test One") { Tag = "T1" };
        var td2 = new FormHierarchyDisplay.TreeData("2", "Test Two", td1.ID) { Tag = "T2" };
        var td3 = new FormHierarchyDisplay.TreeData("3", "Test Three", td1.ID) { Tag = "T3" };
        var td3a = new FormHierarchyDisplay.TreeData("4", "Test Three A", td3.ID) { Tag = "T3a" };
        var td3b = new FormHierarchyDisplay.TreeData("5", "Test Three B", td3.ID) { Tag = "T34" };

        var treeNode = new TreeNode(td1.Title) { Tag = td1 };
        treeView.Nodes.Add(treeNode);

        var treeNode2 = new TreeNode(td2.Title) { Tag = td2 };
        treeNode.Nodes.Add(treeNode2);

        var treeNode3 = new TreeNode(td3.Title) { Tag = td3 };
        treeNode.Nodes.Add(treeNode3);

        var treeNode3a = new TreeNode(td3a.Title) { Tag =td3a };
        treeNode3.Nodes.Add(treeNode3a);

        var treeNode3b = new TreeNode(td3b.Title) { Tag = td3b };
        treeNode3.Nodes.Add(treeNode3b);

        var firedAfter = false;
        var firedBefore = false;
        treeView.AfterSelect += (s, args) => { firedAfter = true; };
        treeView.BeforeSelect += (s, args) => { firedBefore = true; };

        UnitTestWinFormHelper.ShowControl(treeView, .2, (control, form) =>
        {
          if (!(control is MultiselectTreeView text))
            return;

          text.PressKey(Keys.Control | Keys.A);
          text.PressKey(Keys.Control | Keys.C);
          Application.DoEvents();
          treeView.SelectedNode = treeNode2;
          treeView.ExpandAll();
          treeView.SelectAll();
          Application.DoEvents();
          var result = treeView.SelectedToClipboard();
          Assert.IsTrue(result.Contains(treeNode.Text), result);
          Assert.IsTrue(result.Contains(treeNode2.Text), result);
          Assert.IsTrue(result.Contains(treeNode3.Text), result);
          Assert.IsTrue(result.Contains(treeNode3a.Text), result);
          Assert.IsTrue(result.Contains(treeNode3b.Text), result);
        });
        Assert.IsTrue(firedAfter);
        Assert.IsTrue(firedBefore);
      }
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormHierarchyDisplay()
    {
      using (var dataTable = UnitTestStatic.GetDataTable(60))
      using (var form = new FormHierarchyDisplay(dataTable, dataTable.Select(), m_HTMLStyle))
      {
        UnitTestWinFormHelper.ShowFormAndClose(form, 0.1, (frm) =>
        {
          if (!(frm is FormHierarchyDisplay hd))
            return;
          hd.BuildTree("int", "ID");
        });
      }
    }

    [TestMethod]
    [Timeout(5000)]
    public async Task FormHierarchyDisplay_DataWithCycleAsync()
    {
      using var dataTable = UnitTestStatic.GetDataTable(60);
      // load the csvFile FileWithHierarchy
      using var processDisplay = new FormProcessDisplay("FileWithHierarchy");
      processDisplay.Show();
      var cvsSetting = new CsvFile(UnitTestInitializeCsv.GetTestPath("FileWithHierarchy_WithCyle.txt"))
      { FileFormat = { FieldDelimiter = "\t" } };
      using var csvDataReader = new CsvFileReader(cvsSetting.FullPath, cvsSetting.CodePageId, cvsSetting.SkipRows, cvsSetting.HasFieldHeader, cvsSetting.ColumnCollection, cvsSetting.TrimmingOption, cvsSetting.FileFormat.FieldDelimiter, cvsSetting.FileFormat.FieldQualifier, cvsSetting.FileFormat.EscapeCharacter, cvsSetting.RecordLimit, cvsSetting.AllowRowCombining, cvsSetting.FileFormat.AlternateQuoting, cvsSetting.FileFormat.CommentLine, cvsSetting.NumWarnings, cvsSetting.FileFormat.DuplicateQuotingToEscape, cvsSetting.FileFormat.NewLinePlaceholder, cvsSetting.FileFormat.DelimiterPlaceholder, cvsSetting.FileFormat.QuotePlaceholder, cvsSetting.SkipDuplicateHeader, cvsSetting.TreatLFAsSpace, cvsSetting.TreatUnknownCharacterAsSpace, cvsSetting.TryToSolveMoreColumns, cvsSetting.WarnDelimiterInValue, cvsSetting.WarnLineFeed, cvsSetting.WarnNBSP, cvsSetting.WarnQuotes, cvsSetting.WarnUnknownCharacter, cvsSetting.WarnEmptyTailingColumns, cvsSetting.TreatNBSPAsSpace, cvsSetting.TreatTextAsNull, cvsSetting.SkipEmptyLines, cvsSetting.ConsecutiveEmptyRows, cvsSetting.IdentifierInContainer, processDisplay);
      var dt = await csvDataReader.GetDataTableAsync(0, false, true, false, false, false, null,
        processDisplay.CancellationToken);

      using var form = new FormHierarchyDisplay(dt!, dataTable.Select(), m_HTMLStyle);
      UnitTestWinFormHelper.ShowFormAndClose(form, .1, (frm) =>
      {
        if (!(frm is FormHierarchyDisplay hd))
          return;
        hd.BuildTree("ReferenceID1", "ID");
        hd.CloseAll();
        hd.ExpandAll();
      });
      form.Close();
    }
  }
}