﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvTools.Tests
{
  [TestClass]
  [SuppressMessage("ReSharper", "UseAwaitUsing")]
  public class FormHierarchyDisplayTest
  {
    [TestMethod]
    [Timeout(1000)]
    public void MultiselectTreeViewRegular()
    {
      var treeView = new MultiSelectTreeView();
      treeView.HtmlStyle = HtmlStyle.Default;
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
      treeView.AfterSelect += (o, a) => { firedAfter = true; };
      treeView.BeforeSelect += (o, a) => { firedBefore = true; };

      UnitTestStaticForms.ShowControl(() => treeView, .2, theTreeView =>
      {
        theTreeView.PressKey(Keys.Control | Keys.A);
        theTreeView.PressKey(Keys.Control | Keys.C);
        Application.DoEvents();
        theTreeView.SelectedNode = treeNode2;
        theTreeView.SelectAll();
        Application.DoEvents();
        var result = theTreeView.SelectedToClipboard();
        Assert.IsTrue(result.Contains(treeNode.Text));
        Assert.IsTrue(result.Contains(treeNode2.Text));
        Assert.IsTrue(result.Contains(treeNode3.Text));
      });
      Assert.IsTrue(firedAfter);
      Assert.IsTrue(firedBefore);
    }

    [TestMethod]
    [Timeout(1000)]
    public void MultiselectTreeViewTreeData()
    {
      var treeView = new MultiSelectTreeView();
      treeView.HtmlStyle = HtmlStyle.Default;
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
      treeView.AfterSelect += (o, a) => { firedAfter = true; };
      treeView.BeforeSelect += (o, a) => { firedBefore = true; };

      UnitTestStaticForms.ShowControl(() => treeView, .2, theTreeView =>
      {
        theTreeView.PressKey(Keys.Control | Keys.A);
        theTreeView.PressKey(Keys.Control | Keys.C);
        Application.DoEvents();
        theTreeView.SelectedNode = treeNode2;
        theTreeView.ExpandAll();
        theTreeView.SelectAll();
        Application.DoEvents();
        var result = theTreeView.SelectedToClipboard();
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
    [Timeout(1000)]
    public void FormHierarchyDisplay()
    {
      using var dataTable = UnitTestStaticData.GetDataTable(60);
      UnitTestStaticForms.ShowForm(() => new FormHierarchyDisplay(dataTable, dataTable.Select(), HtmlStyle.Default), 0.1, frm =>
      {
        frm.BuildTree("int", "ID");
      });
    }

    [TestMethod]
    [Timeout(2000)]
    public async Task FormHierarchyDisplay_DataWithCycleAsync()
    {
      using var dataTable = UnitTestStaticData.GetDataTable(60);
      // load the csvFile FileWithHierarchy
      using var formProgress = new FormProgress("FileWithHierarchy");
      formProgress.Show();
      
      using var csvDataReader = new CsvFileReader(UnitTestStatic.GetTestPath("FileWithHierarchy_WithCyle.txt"), fieldDelimiterChar:'\t');
      await csvDataReader.OpenAsync(formProgress.CancellationToken);

      var dt = await csvDataReader.GetDataTableAsync(TimeSpan.FromSeconds(30), true,
        false, false, false, null, formProgress.CancellationToken);

      UnitTestStaticForms.ShowForm(() => new FormHierarchyDisplay(dt, dataTable.Select(), HtmlStyle.Default), .1, frm =>
      {
        if (!(frm is { } hd))
          return;
        hd.BuildTree("ReferenceID1", "ID");
        hd.CloseAll();
        hd.ExpandAll();
      });
    }
  }
}