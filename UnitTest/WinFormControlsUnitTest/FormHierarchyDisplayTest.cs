using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvTools.Tests
{
  [TestClass]
  public class FormHierarchyDisplayTest
  {
    [TestMethod]
    [Timeout(5000)]
    public void MultiselectTreeViewRegular()
    {
      using (var treeView = new MultiselectTreeView())
      {
        treeView.HtmlStyle = UnitTestStatic.HtmlStyle;
        Assert.AreEqual(0, treeView.SelectedTreeNode.Count);

        var treeNode = new TreeNode("Test") { Tag = "test" };
        treeView.Nodes.Add(treeNode);

        var treeNode2 = new TreeNode("Test2") { Tag = "test2" };
        treeNode.Nodes.Add(treeNode2);

        var treeNode3 = new TreeNode("Test3") { Tag = "test3" };
        treeNode.Nodes.Add(treeNode3);

        var treeNode3A = new TreeNode("Test3a") { Tag = "test3" };
        treeNode3.Nodes.Add(treeNode3A);
        var treeNode3B = new TreeNode("Test3b") { Tag = "test3" };
        treeNode3.Nodes.Add(treeNode3B);

        var firedAfter = false;
        var firedBefore = false;
        treeView.AfterSelect += (s, args) => { firedAfter = true; };
        treeView.BeforeSelect += (s, args) => { firedBefore = true; };

        UnitTestStatic.ShowControl(treeView, .2, (control, form) =>
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
      using var treeView = new MultiselectTreeView();
      treeView.HtmlStyle = UnitTestStatic.HtmlStyle;
      Assert.AreEqual(0, treeView.SelectedTreeNode.Count);

      var td1 = new FormHierarchyDisplay.TreeData("1", "Test One") { Tag = "T1" };
      var td2 = new FormHierarchyDisplay.TreeData("2", "Test Two", td1.ID) { Tag = "T2" };
      var td3 = new FormHierarchyDisplay.TreeData("3", "Test Three", td1.ID) { Tag = "T3" };
      var td3A = new FormHierarchyDisplay.TreeData("4", "Test Three A", td3.ID) { Tag = "T3a" };
      var td3B = new FormHierarchyDisplay.TreeData("5", "Test Three B", td3.ID) { Tag = "T34" };

      var treeNode = new TreeNode(td1.Title) { Tag = td1 };
      treeView.Nodes.Add(treeNode);

      var treeNode2 = new TreeNode(td2.Title) { Tag = td2 };
      treeNode.Nodes.Add(treeNode2);

      var treeNode3 = new TreeNode(td3.Title) { Tag = td3 };
      treeNode.Nodes.Add(treeNode3);

      var treeNode3A = new TreeNode(td3A.Title) { Tag = td3A };
      treeNode3.Nodes.Add(treeNode3A);

      var treeNode3B = new TreeNode(td3B.Title) { Tag = td3B };
      treeNode3.Nodes.Add(treeNode3B);

      var firedAfter = false;
      var firedBefore = false;
      treeView.AfterSelect += (s, args) => { firedAfter = true; };
      treeView.BeforeSelect += (s, args) => { firedBefore = true; };

      UnitTestStatic.ShowControl(treeView, .2, (control, form) =>
      {
        if (!(control is { } text))
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
        Assert.IsTrue(result.Contains(treeNode3A.Text), result);
        Assert.IsTrue(result.Contains(treeNode3B.Text), result);
      });
      Assert.IsTrue(firedAfter);
      Assert.IsTrue(firedBefore);
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormHierarchyDisplay()
    {
      using (var dataTable = UnitTestStatic.GetDataTable(60))
      using (var form = new FormHierarchyDisplay(dataTable, dataTable.Select(), UnitTestStatic.HtmlStyle))
      {
        UnitTestStatic.ShowFormAndClose(form, 0.1, (frm) =>
        {
          if (!(frm is { } hd))
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
      var cvsSetting = new CsvFile(UnitTestStatic.GetTestPath("FileWithHierarchy_WithCyle.txt")) { FieldDelimiter = "\t" };
      using var csvDataReader = new CsvFileReader(cvsSetting.FullPath, cvsSetting.CodePageId, cvsSetting.SkipRows, cvsSetting.HasFieldHeader,
        cvsSetting.ColumnCollection, cvsSetting.TrimmingOption, cvsSetting.FieldDelimiter, cvsSetting.FieldQualifier, cvsSetting.EscapePrefix,
        cvsSetting.RecordLimit, cvsSetting.AllowRowCombining, cvsSetting.ContextSensitiveQualifier, cvsSetting.CommentLine, cvsSetting.NumWarnings,
        cvsSetting.DuplicateQualifierToEscape, cvsSetting.NewLinePlaceholder, cvsSetting.DelimiterPlaceholder, cvsSetting.QualifierPlaceholder,
        cvsSetting.SkipDuplicateHeader, cvsSetting.TreatLfAsSpace, cvsSetting.TreatUnknownCharacterAsSpace, cvsSetting.TryToSolveMoreColumns,
        cvsSetting.WarnDelimiterInValue, cvsSetting.WarnLineFeed, cvsSetting.WarnNBSP, cvsSetting.WarnQuotes, cvsSetting.WarnUnknownCharacter,
        cvsSetting.WarnEmptyTailingColumns, cvsSetting.TreatNBSPAsSpace, cvsSetting.TreatTextAsNull, cvsSetting.SkipEmptyLines, cvsSetting.ConsecutiveEmptyRows,
        cvsSetting.IdentifierInContainer, processDisplay);
      var dt = await csvDataReader.GetDataTableAsync(0, false, true, false, false, false, null,
                 processDisplay.CancellationToken);

      using var form = new FormHierarchyDisplay(dt!, dataTable.Select(), UnitTestStatic.HtmlStyle);
      UnitTestStatic.ShowFormAndClose(form, .1, (frm) =>
      {
        if (!(frm is { } hd))
          return;
        hd.BuildTree("ReferenceID1", "ID");
        hd.CloseAll();
        hd.ExpandAll();
      });
      form.Close();
    }
  }
}