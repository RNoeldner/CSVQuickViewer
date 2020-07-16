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

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace CsvTools
{
  using System.Linq;

  /// <summary>
  ///   Windows Form to Display the hierarchy
  /// </summary>
  public class FormHierarchyDisplay : ResizeForm
  {
    private readonly CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();

    private readonly DataRow[] m_DataRow;

    private readonly DataTable m_DataTable;

    private readonly Timer m_TimerDisplay = new Timer();

    private readonly Timer m_TimerSearch = new Timer();

    /// <summary>
    ///   Required designer variable.
    /// </summary>
    private IContainer components;

    private FormProcessDisplay m_BuildProcess;

    private ComboBox m_ComboBoxDisplay1;

    private ComboBox m_ComboBoxDisplay2;

    private ComboBox m_ComboBoxID;

    private ComboBox m_ComboBoxParentID;

    private bool m_DisposedValue; // To detect redundant calls

    private TableLayoutPanel m_TableLayoutPanel1;

    private TextBox m_TextBoxValue;

    private IEnumerable<TreeData> m_TreeData = new List<TreeData>();

    private MultiselectTreeView m_TreeView;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormHierarchyDisplay" /> class.
    /// </summary>
    /// <param name="dataTable">The data table.</param>
    /// <param name="dataRows">The filter.</param>
    public FormHierarchyDisplay(DataTable dataTable, DataRow[] dataRows)
    {
      Contract.Requires(dataTable != null);
      Contract.Requires(dataRows != null);
      m_DataTable = dataTable;
      m_DataRow = dataRows;
      InitializeComponent();
      m_TimerSearch.Elapsed += FilterValueChangedElapsed;
      m_TimerSearch.Interval = 200;
      m_TimerSearch.AutoReset = false;

      m_TimerDisplay.Elapsed += TimerDisplayElapsed;
      m_TimerDisplay.Interval = 1000;
      m_TimerDisplay.AutoReset = false;
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      if (m_DisposedValue)
        return;
      if (disposing)
      {
        m_DisposedValue = true;
        components?.Dispose();
        m_TimerDisplay?.Dispose();
        m_TimerSearch?.Dispose();
        m_BuildProcess?.Dispose();
        m_CancellationTokenSource?.Dispose();
      }

      base.Dispose(disposing);
    }

    /// <summary>
    ///   Adds the tree data node with child's.
    /// </summary>
    /// <param name="root">The root.</param>
    /// <param name="rootNode">The root node.</param>
    /// <param name="process">Progress display</param>
    private void AddTreeDataNodeWithChild(TreeData root, TreeNode rootNode, IProcessDisplay process)
    {
      if (process == null) throw new ArgumentNullException(nameof(process));
      Contract.Requires(root != null);
      root.Visited = true;
      var treeNode = new TreeNode(root.NodeTitle) { Tag = root };
      if (rootNode == null)
        m_TreeView.Nodes.Add(treeNode);
      else
        rootNode.Nodes.Add(treeNode);
      if (root.Children.Count > 0)
        treeNode.Nodes.AddRange(BuildSubNodes(root, process));
    }

    /// <summary>
    ///   Builds the sub nodes.
    /// </summary>
    /// <param name="parent">The parent ID.</param>
    /// <param name="process">Progress display</param>
    /// <returns></returns>
    private TreeNode[] BuildSubNodes(TreeData parent, IProcessDisplay process)
    {
      if (process == null) throw new ArgumentNullException(nameof(process));
      Contract.Requires(parent != null);
      var treeNodes = new List<TreeNode>();
      foreach (var child in parent.Children)
      {
        process.CancellationToken.ThrowIfCancellationRequested();
        Extensions.ProcessUIElements();
        if (child.Visited)
        {
          var treeNode = new TreeNode("Cycle -> " + child.Title) { Tag = child };
          treeNodes.Add(treeNode);
        }
        else
        {
          child.Visited = true;
          var treeNode = new TreeNode(child.NodeTitle, BuildSubNodes(child, process)) { Tag = child };
          treeNodes.Add(treeNode);
        }
      }

      return treeNodes.ToArray();
    }

    /// <summary>
    ///   Builds the tree.
    /// </summary>
    public void BuildTree(string parent, string id, string display1 = null, string display2 = null)
    {
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        m_BuildProcess = new FormProcessDisplay("Building Tree", false, m_CancellationTokenSource.Token);
        m_BuildProcess.Show(this);
        m_BuildProcess.Maximum = m_DataRow.GetLength(0) * 2;

        BuildTreeData(parent, id, display1, display2, m_BuildProcess);

        m_BuildProcess.Maximum = 0;
        ShowTree(m_BuildProcess);
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, ex.Message);
      }
      finally
      {
        m_BuildProcess.Dispose();
        m_BuildProcess = null;
        Cursor.Current = oldCursor;
      }
    }

    /// <summary>
    ///   Builds the tree data.
    /// </summary>
    private void BuildTreeData(string parentCol, string idCol, string display1, string display2, IProcessDisplay process)
    {
      var intervalAction = new IntervalAction();

      var dataColumnParent = m_DataTable.Columns[parentCol];
      if (dataColumnParent == null)
        throw new ArgumentException("Could not find column {parentCol}");

      var dataColumnID = m_DataTable.Columns[idCol];
      if (dataColumnID == null)
        throw new ArgumentException("Could not find column {idCol}");

      var dataColumnDisplay1 = string.IsNullOrEmpty(display1) ? null : m_DataTable.Columns[display1];
      var dataColumnDisplay2 = string.IsNullOrEmpty(display2) ? null : m_DataTable.Columns[display2];

      // Using a dictionary here to speed up lookups
      var treeDataDictionary = new Dictionary<string, TreeData>();
      var rootDataParentFound = new TreeData { ID = "{R}", Title = "Parent found / No Parent" };

      treeDataDictionary.Add(rootDataParentFound.ID, rootDataParentFound);

      var max = 0L;
      if (process is IProcessDisplayTime processDisplayTime)
        max = processDisplayTime.Maximum;
      var counter = 0;
      foreach (var dataRow in m_DataRow)
      {
        process.CancellationToken.ThrowIfCancellationRequested();
        intervalAction.Invoke(
          count => process.SetProcess($"Parent found {count}/{max} ", count, false),
          counter++);
        var id = dataRow[dataColumnID.Ordinal].ToString();
        if (string.IsNullOrEmpty(id))
          continue;
        var treeData = new TreeData
        {
          ID = id,
          Title = dataColumnDisplay1 != null
            ? dataColumnDisplay2 != null
              ? dataRow[dataColumnDisplay1.Ordinal] + " - "
                                                    + dataRow[dataColumnDisplay2.Ordinal]
              : dataRow[dataColumnDisplay1.Ordinal].ToString()
            : id,
          ParentID = dataRow[dataColumnParent.Ordinal].ToString()
        };
        if (dataColumnDisplay1 != null)
          treeData.Tag = dataRow[dataColumnDisplay1.Ordinal].ToString();

        // Store the display
        if (!treeDataDictionary.ContainsKey(id))
          treeDataDictionary.Add(id, treeData);
      }

      // Generate a list of missing parents
      var additionalRootNodes = new HashSet<string>();
      foreach (var child in treeDataDictionary.Values)
        if (!string.IsNullOrEmpty(child.ParentID) && !treeDataDictionary.ContainsKey(child.ParentID))
          additionalRootNodes.Add(child.ParentID);

      var rootDataParentNotFound = new TreeData { ID = "{M}", Title = "Parent not found" };

      if (additionalRootNodes.Count > 0)
      {
        treeDataDictionary.Add(rootDataParentNotFound.ID, rootDataParentNotFound);
        counter = 0;
        max = additionalRootNodes.Count;
        process.SetMaximum(max);

        // Create new entries
        foreach (var parentID in additionalRootNodes)
        {
          process.CancellationToken.ThrowIfCancellationRequested();
          intervalAction.Invoke(
            count => process.SetProcess($"Parent not found (Step 1) {count}/{max} ", count, false),
            counter++);
          var childData = new TreeData
          {
            ParentID = rootDataParentNotFound.ID,
            ID = parentID,
            Title = $"{m_ComboBoxID.SelectedItem} - {parentID}"
          };
          treeDataDictionary.Add(parentID, childData);
        }
      }

      max= treeDataDictionary.Values.Count;
      process.SetMaximum(max);
      counter = 0;
      foreach (var child in treeDataDictionary.Values)
      {
        process.CancellationToken.ThrowIfCancellationRequested();
        intervalAction.Invoke(
          count => process.SetProcess($"Parent not found (Step 2) {count}/{max} ", count, false),
          counter++);
        if (string.IsNullOrEmpty(child.ParentID) && child.ID != rootDataParentFound.ID
                                                 && child.ID != rootDataParentNotFound.ID)
          child.ParentID = rootDataParentFound.ID;
      }

      max=  treeDataDictionary.Values.Count;
      process.SetMaximum(max);
      counter = 0;

      // Fill m_Children for the new nodes
      foreach (var child in treeDataDictionary.Values)
      {
        process.CancellationToken.ThrowIfCancellationRequested();
        intervalAction.Invoke(
          count => process.SetProcess($"Set children {count}/{max} ", count, false),
          counter++);
        if (!string.IsNullOrEmpty(child.ParentID))
          treeDataDictionary[child.ParentID].Children.Add(child);
      }

      m_TreeData = treeDataDictionary.Values;
    }

    private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
      try
      {
        m_TreeView.SuspendLayout();
        m_TreeView.BeginUpdate();
        m_TreeView.CollapseAll();
        m_TreeView.EndUpdate();
        m_TreeView.ResumeLayout();
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
      }
    }

    /// <summary>
    ///   Handles the SelectionChangeCommitted event of the comboBox control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ComboBox_SelectionChangeCommitted(object sender, EventArgs e)
    {
    }

    private void ExpandAllToolStripMenuItem_Click(object sender, EventArgs e)
    {
      try
      {
        m_TreeView.SuspendLayout();
        m_TreeView.BeginUpdate();
        m_TreeView.ExpandAll();
        m_TreeView.EndUpdate();
        m_TreeView.ResumeLayout();
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
      }
    }

    private void FilterValueChangedElapsed(object sender, ElapsedEventArgs e) =>

      // go to UI Main thread
      m_TextBoxValue.Invoke(
        (MethodInvoker) delegate
        {
          try
          {
            using (var proc = new FormProcessDisplay("Searching", false, m_CancellationTokenSource.Token))
            {
              proc.Show(this);
              Search(m_TextBoxValue.Text, m_TreeView.Nodes, proc.CancellationToken);
            }
          }
          catch (Exception ex)
          {
            this.ShowError(ex);
          }
        });

    /// <summary>
    ///   Handles the Load event of the HierarchyDisplay control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void FormHierarchyDisplay_Load(object sender, EventArgs e)
    {
      try
      {
        foreach (DataColumn col in m_DataTable.Columns)
        {
          m_ComboBoxID.Items.Add(col.ColumnName);
          m_ComboBoxDisplay1.Items.Add(col.ColumnName);
          m_ComboBoxDisplay2.Items.Add(col.ColumnName);
          m_ComboBoxParentID.Items.Add(col.ColumnName);
        }
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
      }
    }

    /// <summary>
    ///   Required method for Designer support - do not modify the contents of this method with the
    ///   code editor.
    /// </summary>
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "RedundantDelegateCreation")]
    [SuppressMessage("ReSharper", "LocalizableElement")]
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.Windows.Forms.Label labelID;
      System.Windows.Forms.Label labelDisplay;
      System.Windows.Forms.Label labelParent;
      System.Windows.Forms.ContextMenuStrip contextMenuStrip;
      System.Windows.Forms.ToolStripMenuItem expandAllToolStripMenuItem;
      System.Windows.Forms.ToolStripMenuItem closeAllToolStripMenuItem;
      System.Windows.Forms.Label labelFind;
      this.m_TableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.m_ComboBoxID = new System.Windows.Forms.ComboBox();
      this.m_ComboBoxParentID = new System.Windows.Forms.ComboBox();
      this.m_TreeView = new CsvTools.MultiselectTreeView();
      this.m_TextBoxValue = new System.Windows.Forms.TextBox();
      this.m_ComboBoxDisplay2 = new System.Windows.Forms.ComboBox();
      this.m_ComboBoxDisplay1 = new System.Windows.Forms.ComboBox();
      labelID = new System.Windows.Forms.Label();
      labelDisplay = new System.Windows.Forms.Label();
      labelParent = new System.Windows.Forms.Label();
      contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
      expandAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      closeAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      labelFind = new System.Windows.Forms.Label();
      contextMenuStrip.SuspendLayout();
      this.m_TableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // labelID
      // 
      labelID.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelID.AutoSize = true;
      labelID.Location = new System.Drawing.Point(52, 6);
      labelID.Name = "labelID";
      labelID.Size = new System.Drawing.Size(19, 15);
      labelID.TabIndex = 3;
      labelID.Text = "ID";
      // 
      // labelDisplay
      // 
      labelDisplay.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelDisplay.AutoSize = true;
      labelDisplay.Location = new System.Drawing.Point(24, 33);
      labelDisplay.Name = "labelDisplay";
      labelDisplay.Size = new System.Drawing.Size(47, 15);
      labelDisplay.TabIndex = 5;
      labelDisplay.Text = "Display";
      // 
      // labelParent
      // 
      labelParent.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelParent.AutoSize = true;
      labelParent.Location = new System.Drawing.Point(13, 60);
      labelParent.Name = "labelParent";
      labelParent.Size = new System.Drawing.Size(58, 15);
      labelParent.TabIndex = 7;
      labelParent.Text = "Parent ID";
      // 
      // contextMenuStrip
      // 
      contextMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
      contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            expandAllToolStripMenuItem,
            closeAllToolStripMenuItem});
      contextMenuStrip.Name = "contextMenuStrip";
      contextMenuStrip.Size = new System.Drawing.Size(150, 52);
      // 
      // expandAllToolStripMenuItem
      // 
      expandAllToolStripMenuItem.Name = "expandAllToolStripMenuItem";
      expandAllToolStripMenuItem.Size = new System.Drawing.Size(149, 24);
      expandAllToolStripMenuItem.Text = "Expand All";
      expandAllToolStripMenuItem.Click += new System.EventHandler(this.ExpandAllToolStripMenuItem_Click);
      // 
      // closeAllToolStripMenuItem
      // 
      closeAllToolStripMenuItem.Name = "closeAllToolStripMenuItem";
      closeAllToolStripMenuItem.Size = new System.Drawing.Size(149, 24);
      closeAllToolStripMenuItem.Text = "Close All";
      closeAllToolStripMenuItem.Click += new System.EventHandler(this.CloseAllToolStripMenuItem_Click);
      // 
      // labelFind
      // 
      labelFind.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelFind.AutoSize = true;
      labelFind.Location = new System.Drawing.Point(40, 86);
      labelFind.Name = "labelFind";
      labelFind.Size = new System.Drawing.Size(31, 15);
      labelFind.TabIndex = 5;
      labelFind.Text = "Find";
      // 
      // m_TableLayoutPanel1
      // 
      this.m_TableLayoutPanel1.ColumnCount = 3;
      this.m_TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 74F));
      this.m_TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.m_TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.m_TableLayoutPanel1.Controls.Add(labelID, 0, 0);
      this.m_TableLayoutPanel1.Controls.Add(labelDisplay, 0, 1);
      this.m_TableLayoutPanel1.Controls.Add(labelParent, 0, 2);
      this.m_TableLayoutPanel1.Controls.Add(this.m_ComboBoxID, 1, 0);
      this.m_TableLayoutPanel1.Controls.Add(this.m_ComboBoxParentID, 1, 2);
      this.m_TableLayoutPanel1.Controls.Add(this.m_TreeView, 0, 4);
      this.m_TableLayoutPanel1.Controls.Add(this.m_TextBoxValue, 1, 3);
      this.m_TableLayoutPanel1.Controls.Add(labelFind, 0, 3);
      this.m_TableLayoutPanel1.Controls.Add(this.m_ComboBoxDisplay2, 2, 1);
      this.m_TableLayoutPanel1.Controls.Add(this.m_ComboBoxDisplay1, 1, 1);
      this.m_TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_TableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.m_TableLayoutPanel1.Name = "m_TableLayoutPanel1";
      this.m_TableLayoutPanel1.RowCount = 5;
      this.m_TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.m_TableLayoutPanel1.Size = new System.Drawing.Size(502, 368);
      this.m_TableLayoutPanel1.TabIndex = 10;
      // 
      // m_ComboBoxID
      // 
      this.m_TableLayoutPanel1.SetColumnSpan(this.m_ComboBoxID, 2);
      this.m_ComboBoxID.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_ComboBoxID.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.m_ComboBoxID.FormattingEnabled = true;
      this.m_ComboBoxID.Location = new System.Drawing.Point(77, 3);
      this.m_ComboBoxID.Name = "m_ComboBoxID";
      this.m_ComboBoxID.Size = new System.Drawing.Size(422, 21);
      this.m_ComboBoxID.TabIndex = 0;
      this.m_ComboBoxID.SelectedIndexChanged += new System.EventHandler(this.TimeDisplayRestart);
      this.m_ComboBoxID.SelectionChangeCommitted += new System.EventHandler(this.ComboBox_SelectionChangeCommitted);
      // 
      // m_ComboBoxParentID
      // 
      this.m_TableLayoutPanel1.SetColumnSpan(this.m_ComboBoxParentID, 2);
      this.m_ComboBoxParentID.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_ComboBoxParentID.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.m_ComboBoxParentID.FormattingEnabled = true;
      this.m_ComboBoxParentID.Location = new System.Drawing.Point(77, 57);
      this.m_ComboBoxParentID.Name = "m_ComboBoxParentID";
      this.m_ComboBoxParentID.Size = new System.Drawing.Size(422, 21);
      this.m_ComboBoxParentID.TabIndex = 1;
      this.m_ComboBoxParentID.SelectedIndexChanged += new System.EventHandler(this.TimeDisplayRestart);
      this.m_ComboBoxParentID.SelectionChangeCommitted += new System.EventHandler(this.ComboBox_SelectionChangeCommitted);
      // 
      // m_TreeView
      // 
      this.m_TableLayoutPanel1.SetColumnSpan(this.m_TreeView, 3);
      this.m_TreeView.ContextMenuStrip = contextMenuStrip;
      this.m_TreeView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_TreeView.Location = new System.Drawing.Point(3, 110);
      this.m_TreeView.Name = "m_TreeView";
      this.m_TreeView.Size = new System.Drawing.Size(496, 255);
      this.m_TreeView.TabIndex = 9;
      // 
      // m_TextBoxValue
      // 
      this.m_TextBoxValue.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_TextBoxValue.Location = new System.Drawing.Point(77, 84);
      this.m_TextBoxValue.Name = "m_TextBoxValue";
      this.m_TextBoxValue.Size = new System.Drawing.Size(208, 20);
      this.m_TextBoxValue.TabIndex = 2;
      this.m_TextBoxValue.TextChanged += new System.EventHandler(this.TimerSearchRestart);
      // 
      // m_ComboBoxDisplay2
      // 
      this.m_ComboBoxDisplay2.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_ComboBoxDisplay2.FormattingEnabled = true;
      this.m_ComboBoxDisplay2.Location = new System.Drawing.Point(291, 30);
      this.m_ComboBoxDisplay2.Name = "m_ComboBoxDisplay2";
      this.m_ComboBoxDisplay2.Size = new System.Drawing.Size(208, 21);
      this.m_ComboBoxDisplay2.TabIndex = 15;
      this.m_ComboBoxDisplay2.SelectedIndexChanged += new System.EventHandler(this.TimeDisplayRestart);
      // 
      // m_ComboBoxDisplay1
      // 
      this.m_ComboBoxDisplay1.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_ComboBoxDisplay1.FormattingEnabled = true;
      this.m_ComboBoxDisplay1.Location = new System.Drawing.Point(77, 30);
      this.m_ComboBoxDisplay1.Name = "m_ComboBoxDisplay1";
      this.m_ComboBoxDisplay1.Size = new System.Drawing.Size(208, 21);
      this.m_ComboBoxDisplay1.TabIndex = 16;
      this.m_ComboBoxDisplay1.SelectedIndexChanged += new System.EventHandler(this.TimeDisplayRestart);
      // 
      // FormHierarchyDisplay
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(502, 368);
      this.Controls.Add(this.m_TableLayoutPanel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MinimumSize = new System.Drawing.Size(339, 196);
      this.Name = "FormHierarchyDisplay";
      this.Text = "Hierarchy";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormHierarchyDisplay_FormClosing);
      this.Load += new System.EventHandler(this.FormHierarchyDisplay_Load);
      contextMenuStrip.ResumeLayout(false);
      this.m_TableLayoutPanel1.ResumeLayout(false);
      this.m_TableLayoutPanel1.PerformLayout();
      this.ResumeLayout(false);

    }

    private bool MarkInCycle(TreeData treeData, ICollection<TreeData> visitedEntries)
    {
      Contract.Requires(treeData != null);
      Contract.Requires(visitedEntries != null);
      if (visitedEntries.Contains(treeData))
      {
        treeData.InCycle = true;
        return true;
      }

      visitedEntries.Add(treeData);
      foreach (var child in treeData.Children)
        if (MarkInCycle(child, visitedEntries))
          break;

      return false;
    }

    private void Search(string text, ICollection nodes, CancellationToken token)
    {
      if (nodes == null)
        return;
      if (nodes.Count == 0)
        return;
      token.ThrowIfCancellationRequested();
      foreach (TreeNode node in nodes)
        if (node.Text.Contains(text))
        {
          m_TreeView.Select();
          node.EnsureVisible();
          m_TreeView.SelectedNode = node;
          return;
        }

      foreach (TreeNode node in nodes)
        Search(text, node.Nodes, token);
    }

    /// <summary>
    ///   Shows the tree.
    /// </summary>
    private void ShowTree(IProcessDisplay process)
    {
      m_TreeView.BeginUpdate();
      try
      {
        m_TreeView.Nodes.Clear();
        if (m_TreeData == null)
          return;

        foreach (var treeData in m_TreeData)
          treeData.Visited = false;
        process.SetProcess("Adding Tree with children", -1, false);
        foreach (var treeData in m_TreeData)
        {
          process.CancellationToken.ThrowIfCancellationRequested();
          Extensions.ProcessUIElements();
          if (string.IsNullOrEmpty(treeData.ParentID))
            AddTreeDataNodeWithChild(treeData, null, process);
        }

        process.SetProcess("Finding Cycles in Hierarchy", -1, true);
        var hasCycles = false;
        foreach (var treeData in m_TreeData)
        {
          process.CancellationToken.ThrowIfCancellationRequested();
          Extensions.ProcessUIElements();
          if (!treeData.Visited)
          {
            hasCycles = true;
            break;
          }
        }

        if (!hasCycles)
          return;

        process.SetProcess("Adding Cycles", -1, false);
        var rootNode = new TreeNode("Cycles in Hierarchy");
        m_TreeView.Nodes.Add(rootNode);

        foreach (var treeData in m_TreeData)
        {
          process.CancellationToken.ThrowIfCancellationRequested();
          Extensions.ProcessUIElements();
          if (!treeData.Visited)
            MarkInCycle(treeData, new HashSet<TreeData>());
        }

        foreach (var root in m_TreeData)
        {
          process.CancellationToken.ThrowIfCancellationRequested();
          Extensions.ProcessUIElements();
          if (!root.Visited && root.InCycle)
            AddTreeDataNodeWithChild(root, rootNode, process);
        }
      }
      catch
      {
        m_TreeView.Nodes.Clear();
      }
      finally
      {
        m_TreeView.EndUpdate();
      }
    }

    private void TimeDisplayRestart(object sender, EventArgs e)
    {
      m_TimerDisplay.Stop();
      if (m_BuildProcess != null && !m_BuildProcess.CancellationToken.IsCancellationRequested)
        m_BuildProcess.Close();
      m_TimerDisplay.Start();
    }

    //TODO: Make this async
    private void TimerDisplayElapsed(object sender, ElapsedEventArgs e) =>
      Task.Run(() =>
      {
        m_TimerDisplay.Stop();
        this.SafeBeginInvoke(
          () =>
          {
            if (m_ComboBoxID.SelectedItem != null && m_ComboBoxParentID.SelectedItem != null)
              BuildTree(m_ComboBoxParentID.Text,
                m_ComboBoxID.Text,
                m_ComboBoxDisplay1.Text,
                m_ComboBoxDisplay2.Text);
          });
      }, m_CancellationTokenSource.Token);

    private void TimerSearchRestart(object sender, EventArgs e)
    {
      m_TimerSearch.Stop();
      m_TimerSearch.Start();
    }

    [DebuggerDisplay("TreeData {ID} {ParentID} {Visited} Children:{Children.Count}")]
    internal class TreeData
    {
      public readonly ICollection<TreeData> Children = new List<TreeData>();

      public string ID;

      public bool InCycle;

      private int m_StoreIndirect = -1;

      public string ParentID;

      public string Tag;

      public string Title;

      public bool Visited;

      private int DirectChildren => Children.Count;

      private int InDirectChildren
      {
        get
        {
          if (m_StoreIndirect < 0)
            m_StoreIndirect = GetInDirectChildren(this);
          return m_StoreIndirect;
        }
      }

      public string NodeTitle
      {
        get
        {
          if (DirectChildren <= 0)
            return Title;
          return DirectChildren == InDirectChildren
            ? $"{Title} - Direct {DirectChildren}"
            : $"{Title} - Direct {DirectChildren} - Indirect {InDirectChildren}";
        }
      }

      private static int GetInDirectChildren(TreeData root)
      {
        if (root == null || root.InCycle)
          return 0;

        return root.Children.Count + root.Children.Where(child => child.Children.Count > 0).Sum(child => GetInDirectChildren(child));
      }
    }

    private void FormHierarchyDisplay_FormClosing(object sender, FormClosingEventArgs e)
    {
      m_CancellationTokenSource.Cancel();
    }
  }
}