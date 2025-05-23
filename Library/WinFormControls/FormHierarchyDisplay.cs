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

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace CsvTools
{
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
    private System.ComponentModel.IContainer? components;

    private ComboBox m_ComboBoxDisplay1;
    private ComboBox m_ComboBoxDisplay2;
    private ComboBox m_ComboBoxId;
    private ComboBox m_ComboBoxParentId;

    private TableLayoutPanel m_TableLayoutPanel1;
    private TextBox m_TextBoxValue;
    private IEnumerable<TreeData> m_TreeData = new List<TreeData>();
    private ToolTip m_ToolTip;
    private MultiSelectTreeView m_TreeView = new MultiSelectTreeView();

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormHierarchyDisplay" /> class.
    /// </summary>
    /// <param name="dataTable">The data table.</param>
    /// <param name="dataRows">The filter.</param>
    /// <param name="hTmlStyle">The HTML style.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public FormHierarchyDisplay(in DataTable dataTable, in DataRow[] dataRows, in HtmlStyle hTmlStyle)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
      try
      {
        m_DataTable = dataTable ?? throw new ArgumentNullException(nameof(dataTable));
        m_DataRow = dataRows;
        InitializeComponent();
        m_TimerSearch.Elapsed += FilterValueChangedElapsed;
        m_TimerSearch.Interval = 200;
        m_TimerSearch.AutoReset = false;

        m_TimerDisplay.Elapsed += TimerDisplayElapsed;
        m_TimerDisplay.Interval = 1000;
        m_TimerDisplay.AutoReset = false;

        m_TreeView.HtmlStyle = hTmlStyle;
      }
      catch (Exception e)
      {
        try { Logger.Warning(e, "FormColumnUiRead ctor"); } catch { };
      }
    }

    /// <summary>
    ///   Builds the tree.
    /// </summary>
    public void BuildTree(string parent, string id, string? display1 = null, string? display2 = null)
    {
      this.RunWithHourglass(() =>
      {
        using var formProgress = new FormProgress("Building Tree", false, FontConfig, m_CancellationTokenSource.Token);
        formProgress.Show(this);
        formProgress.Maximum = m_DataRow.GetLength(0) * 2;

        BuildTreeData(parent, id, display1, display2, formProgress, formProgress.CancellationToken);
        formProgress.Maximum = 0;
        ShowTree(formProgress.CancellationToken);
      });
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      try { m_CancellationTokenSource.Cancel(); }
      catch
      {
        /* ignore */
      }

      if (disposing)
      {
        m_TimerDisplay.Dispose();
        m_TimerSearch.Dispose();
        m_CancellationTokenSource.Dispose();
      }

      base.Dispose(disposing);
    }

    /// <summary>
    ///   Adds the tree data node with child's.
    /// </summary>
    /// <param name="root">The root.</param>
    /// <param name="rootNode">The root node.</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    private void AddTreeDataNodeWithChild(in TreeData root, in TreeNode? rootNode,
      in CancellationToken cancellationToken)
    {
      root.Visited = true;
      var treeNode = new TreeNode(root.NodeTitle) { Tag = root };
      if (rootNode is null)
        m_TreeView.Nodes.Add(treeNode);
      else
        rootNode.Nodes.Add(treeNode);
      if (root.Children.Count > 0)
        treeNode.Nodes.AddRange(BuildSubNodes(root, cancellationToken));
    }

    /// <summary>
    ///   Builds the sub nodes.
    /// </summary>
    /// <param name="parent">The parent ID.</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns></returns>
    private TreeNode[] BuildSubNodes(in TreeData parent, in CancellationToken cancellationToken)
    {
      var treeNodes = new List<TreeNode>();
      foreach (var child in parent.Children)
      {
        cancellationToken.ThrowIfCancellationRequested();
        Extensions.ProcessUIElements();
        if (child.Visited)
        {
          var treeNode = new TreeNode("Cycle -> " + child.Title) { Tag = child };
          treeNodes.Add(treeNode);
        }
        else
        {
          child.Visited = true;
          var treeNode = new TreeNode(child.NodeTitle, BuildSubNodes(child, cancellationToken)) { Tag = child };
          treeNodes.Add(treeNode);
        }
      }

      return treeNodes.ToArray();
    }

    /// <summary>
    ///   Builds the tree data.
    /// </summary>
    private void BuildTreeData(in string parentCol, in string idCol, in string? display1, in string? display2,
      IProgress<ProgressInfo> process, in CancellationToken cancellationToken)
    {
      DataColumn dataColumnParent = m_DataTable.Columns[parentCol] ??
                                    throw new ArgumentException($"Could not find column {parentCol}");
      DataColumn dataColumnID =
        m_DataTable.Columns[idCol] ?? throw new ArgumentException($"Could not find column {idCol}");

      DataColumn? dataColumnDisplay1 = string.IsNullOrEmpty(display1) ? null : m_DataTable.Columns[display1];
      DataColumn? dataColumnDisplay2 = string.IsNullOrEmpty(display2) ? null : m_DataTable.Columns[display2];

      // Using a dictionary here to speed up lookups
      var treeDataDictionary = new Dictionary<string, TreeData>();
      var rootDataParentFound = new TreeData("{R}", "Parent found / No Parent");

      treeDataDictionary.Add(rootDataParentFound.ID, rootDataParentFound);

      var max = 0L;
      if (process is IProgressTime progressTime)
        max = progressTime.Maximum;
      var counter = 0;
      var intervalAction = new IntervalAction();
      foreach (var dataRow in m_DataRow)
      {
        cancellationToken.ThrowIfCancellationRequested();
        counter++;
        intervalAction.Invoke(process, $"Parent found {counter}/{max} ", counter);
        var id = dataRow[dataColumnID.Ordinal].ToString();
        if (id is null || id.Length == 0)
          continue;

        var title = new StringBuilder();
        if (dataColumnDisplay1 != null)
          title.Append(dataRow[dataColumnDisplay1.Ordinal]);
        if (dataColumnDisplay2 != null)
        {
          if (dataColumnDisplay1 != null)
            title.Append(" - ");
          title.Append(dataRow[dataColumnDisplay2.Ordinal]);
        }

        // Fallback 
        if (title.Length == 0)
          title.Append(id);

        var treeData = new TreeData(id, title.ToString(), dataRow[dataColumnParent.Ordinal].ToString());
        if (dataColumnDisplay1 != null)
          treeData.Tag = Convert.ToString(dataRow[dataColumnDisplay1.Ordinal]) ?? string.Empty;

        // Store the display
        if (!treeDataDictionary.ContainsKey(id))
          treeDataDictionary.Add(id, treeData);
      }

      // Generate a list of missing parents
      var additionalRootNodes = new HashSet<string>();
      foreach (var child in treeDataDictionary.Values)
        if (!string.IsNullOrEmpty(child.ParentID) && !treeDataDictionary.ContainsKey(child.ParentID))
          additionalRootNodes.Add(child.ParentID);

      var rootDataParentNotFound = new TreeData("{M}", "Parent not found");

      if (additionalRootNodes.Count > 0)
      {
        treeDataDictionary.Add(rootDataParentNotFound.ID, rootDataParentNotFound);
        counter = 0;
        max = additionalRootNodes.Count;
        process.SetMaximum(max);

        // Create new entries
        foreach (var parentID in additionalRootNodes)
        {
          cancellationToken.ThrowIfCancellationRequested();
          counter++;
          intervalAction.Invoke(process, $"Parent not found (Step 1) {counter}/{max} ", counter);
          var childData = new TreeData(parentID, $"{parentID}", rootDataParentNotFound.ID);
          treeDataDictionary.Add(parentID, childData);
        }
      }

      max = treeDataDictionary.Values.Count;
      process.SetMaximum(max);
      counter = 0;
      foreach (var child in treeDataDictionary.Values)
      {
        cancellationToken.ThrowIfCancellationRequested();
        counter++;
        intervalAction.Invoke(process, $"Parent not found (Step 2) {counter}/{max} ", counter);
        if (string.IsNullOrEmpty(child.ParentID) && child.ID != rootDataParentFound.ID
                                                 && child.ID != rootDataParentNotFound.ID)
          child.ParentID = rootDataParentFound.ID;
      }

      max = treeDataDictionary.Values.Count;
      process.SetMaximum(max);
      counter = 0;

      // Fill m_Children for the new nodes
      foreach (var child in treeDataDictionary.Values)
      {
        cancellationToken.ThrowIfCancellationRequested();
        counter++;
        intervalAction.Invoke(process, $"Set children {counter}/{max} ", counter);

        if (!string.IsNullOrEmpty(child.ParentID))
          treeDataDictionary[child.ParentID].Children.Add(child);
      }

      m_TreeData = treeDataDictionary.Values;
    }

    public void CloseAll() => CloseAllToolStripMenuItem_Click(this, EventArgs.Empty);

    private void CloseAllToolStripMenuItem_Click(object? sender, EventArgs e)
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

    public void ExpandAll() => ExpandAllToolStripMenuItem_Click(this, EventArgs.Empty);

    private void ExpandAllToolStripMenuItem_Click(object? sender, EventArgs e)
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

    private void FilterValueChangedElapsed(object? sender, ElapsedEventArgs e)
      => m_TextBoxValue!.SafeInvoke(() => m_TextBoxValue!.RunWithHourglass(() =>
      {
        try
        {
          using var formProgress = new FormProgress("Searching", false, FontConfig, m_CancellationTokenSource.Token);
          formProgress.Show(this);
          Search(m_TextBoxValue!.Text, m_TreeView.Nodes, formProgress.CancellationToken);
        }
        catch (Exception ex)
        {
          this.ShowError(ex);
        }
      }));

    private void FormHierarchyDisplay_FormClosing(object? sender, FormClosingEventArgs e) =>
      m_CancellationTokenSource.Cancel();

    /// <summary>
    ///   Handles the Load event of the HierarchyDisplay control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void FormHierarchyDisplay_Load(object? sender, EventArgs e)
    {
      try
      {
        foreach (DataColumn col in m_DataTable.Columns)
        {
          m_ComboBoxId!.Items.Add(col.ColumnName);
          m_ComboBoxDisplay1!.Items.Add(col.ColumnName);
          m_ComboBoxDisplay2!.Items.Add(col.ColumnName);
          m_ComboBoxParentId!.Items.Add(col.ColumnName);
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
    [SuppressMessage("ReSharper", "JoinDeclarationAndInitializer")]
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "RedundantCast")]
    [SuppressMessage("ReSharper", "RedundantDelegateCreation")]
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
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
      this.m_ComboBoxId = new System.Windows.Forms.ComboBox();
      this.m_ComboBoxParentId = new System.Windows.Forms.ComboBox();
      this.m_TreeView = new CsvTools.MultiSelectTreeView();
      this.m_TextBoxValue = new System.Windows.Forms.TextBox();
      this.m_ComboBoxDisplay2 = new System.Windows.Forms.ComboBox();
      this.m_ComboBoxDisplay1 = new System.Windows.Forms.ComboBox();
      this.m_ToolTip = new System.Windows.Forms.ToolTip(this.components);
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
      labelID.Location = new System.Drawing.Point(53, 7);
      labelID.Name = "labelID";
      labelID.Size = new System.Drawing.Size(18, 13);
      labelID.TabIndex = 3;
      labelID.Text = "ID";
      // 
      // labelDisplay
      // 
      labelDisplay.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelDisplay.AutoSize = true;
      labelDisplay.Location = new System.Drawing.Point(30, 34);
      labelDisplay.Name = "labelDisplay";
      labelDisplay.Size = new System.Drawing.Size(41, 13);
      labelDisplay.TabIndex = 5;
      labelDisplay.Text = "Display";
      // 
      // labelParent
      // 
      labelParent.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelParent.AutoSize = true;
      labelParent.Location = new System.Drawing.Point(19, 61);
      labelParent.Name = "labelParent";
      labelParent.Size = new System.Drawing.Size(52, 13);
      labelParent.TabIndex = 7;
      labelParent.Text = "Parent ID";
      // 
      // contextMenuStrip
      // 
      contextMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
      contextMenuStrip.Items.AddRange(
        new System.Windows.Forms.ToolStripItem[] { expandAllToolStripMenuItem, closeAllToolStripMenuItem });
      contextMenuStrip.Name = "contextMenuStrip";
      contextMenuStrip.Size = new System.Drawing.Size(131, 48);
      // 
      // expandAllToolStripMenuItem
      // 
      expandAllToolStripMenuItem.Name = "expandAllToolStripMenuItem";
      expandAllToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
      expandAllToolStripMenuItem.Text = "Expand All";
      expandAllToolStripMenuItem.Click += new System.EventHandler(this.ExpandAllToolStripMenuItem_Click);
      // 
      // closeAllToolStripMenuItem
      // 
      closeAllToolStripMenuItem.Name = "closeAllToolStripMenuItem";
      closeAllToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
      closeAllToolStripMenuItem.Text = "Close All";
      closeAllToolStripMenuItem.Click += new System.EventHandler(this.CloseAllToolStripMenuItem_Click);
      // 
      // labelFind
      // 
      labelFind.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelFind.AutoSize = true;
      labelFind.Location = new System.Drawing.Point(44, 87);
      labelFind.Name = "labelFind";
      labelFind.Size = new System.Drawing.Size(27, 13);
      labelFind.TabIndex = 5;
      labelFind.Text = "Find";
      // 
      // m_TableLayoutPanel1
      // 
      this.m_TableLayoutPanel1.ColumnCount = 3;
      this.m_TableLayoutPanel1.ColumnStyles.Add(
        new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 74F));
      this.m_TableLayoutPanel1.ColumnStyles.Add(
        new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.m_TableLayoutPanel1.ColumnStyles.Add(
        new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.m_TableLayoutPanel1.Controls.Add(labelID, 0, 0);
      this.m_TableLayoutPanel1.Controls.Add(labelDisplay, 0, 1);
      this.m_TableLayoutPanel1.Controls.Add(labelParent, 0, 2);
      this.m_TableLayoutPanel1.Controls.Add(this.m_ComboBoxId, 1, 0);
      this.m_TableLayoutPanel1.Controls.Add(this.m_ComboBoxParentId, 1, 2);
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
      this.m_TableLayoutPanel1.RowStyles.Add(
        new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.m_TableLayoutPanel1.Size = new System.Drawing.Size(502, 368);
      this.m_TableLayoutPanel1.TabIndex = 10;
      // 
      // m_ComboBoxID
      // 
      this.m_TableLayoutPanel1.SetColumnSpan(this.m_ComboBoxId, 2);
      this.m_ComboBoxId.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_ComboBoxId.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.m_ComboBoxId.FormattingEnabled = true;
      this.m_ComboBoxId.Location = new System.Drawing.Point(77, 3);
      this.m_ComboBoxId.Name = "m_ComboBoxId";
      this.m_ComboBoxId.Size = new System.Drawing.Size(422, 21);
      this.m_ComboBoxId.TabIndex = 0;
      this.m_ToolTip.SetToolTip(this.m_ComboBoxId, "Unique identifier for counting and determining child entries");
      this.m_ComboBoxId.SelectedIndexChanged += new System.EventHandler(this.TimeDisplayRestart);
      // 
      // m_ComboBoxParentID
      // 
      this.m_TableLayoutPanel1.SetColumnSpan(this.m_ComboBoxParentId, 2);
      this.m_ComboBoxParentId.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_ComboBoxParentId.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.m_ComboBoxParentId.FormattingEnabled = true;
      this.m_ComboBoxParentId.Location = new System.Drawing.Point(77, 57);
      this.m_ComboBoxParentId.Name = "m_ComboBoxParentId";
      this.m_ComboBoxParentId.Size = new System.Drawing.Size(422, 21);
      this.m_ComboBoxParentId.TabIndex = 1;
      this.m_ToolTip.SetToolTip(this.m_ComboBoxParentId, "Column with the Parent, can be used for grouping as well");
      this.m_ComboBoxParentId.SelectedIndexChanged += new System.EventHandler(this.TimeDisplayRestart);
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
      this.m_ComboBoxDisplay2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
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
      this.m_ComboBoxDisplay1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.m_ComboBoxDisplay1.FormattingEnabled = true;
      this.m_ComboBoxDisplay1.Location = new System.Drawing.Point(77, 30);
      this.m_ComboBoxDisplay1.Name = "m_ComboBoxDisplay1";
      this.m_ComboBoxDisplay1.Size = new System.Drawing.Size(208, 21);
      this.m_ComboBoxDisplay1.TabIndex = 16;
      this.m_ToolTip.SetToolTip(this.m_ComboBoxDisplay1, "If the display feidls is are not set ID is used");
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

    private void Search(string text, ICollection nodes, in CancellationToken token)
    {
      if (nodes is null || nodes.Count == 0)
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
    private void ShowTree(in CancellationToken cancellationToken)
    {
      if (m_TreeView is null)
        return;
      m_TreeView.BeginUpdate();
      try
      {
        m_TreeView.Nodes.Clear();

        foreach (var treeData in m_TreeData)
          treeData.Visited = false;
        try { Logger.Information("Adding Tree with children"); } catch { };
        foreach (var treeData in m_TreeData)
        {
          cancellationToken.ThrowIfCancellationRequested();
          Extensions.ProcessUIElements();
          if (string.IsNullOrEmpty(treeData.ParentID))
            AddTreeDataNodeWithChild(treeData, null, cancellationToken);
        }

        try { Logger.Information("Finding Cycles in Hierarchy");} catch { };
        var hasCycles = false;
        foreach (var treeData in m_TreeData)
        {
          cancellationToken.ThrowIfCancellationRequested();
          Extensions.ProcessUIElements();
          if (!treeData.Visited)
          {
            hasCycles = true;
            break;
          }
        }

        if (!hasCycles)
          return;

        try { Logger.Information("Adding Cycles");} catch { };
        var rootNode = new TreeNode("Cycles in Hierarchy");
        m_TreeView.Nodes.Add(rootNode);

        foreach (var treeData in m_TreeData)
        {
          cancellationToken.ThrowIfCancellationRequested();
          Extensions.ProcessUIElements();
          if (!treeData.Visited)
            MarkInCycle(treeData, new HashSet<TreeData>());
        }

        foreach (var root in m_TreeData)
        {
          cancellationToken.ThrowIfCancellationRequested();
          Extensions.ProcessUIElements();
          if (!root.Visited && root.InCycle)
            AddTreeDataNodeWithChild(root, rootNode, cancellationToken);
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

    private void TimeDisplayRestart(object? sender, EventArgs e)
    {
      m_TimerDisplay.Stop();
      m_TimerDisplay.Start();
    }

    private void TimerDisplayElapsed(object? sender, ElapsedEventArgs e)
    {
      m_ComboBoxDisplay1.SafeInvoke(
        () =>
        {
          if (string.IsNullOrEmpty(m_ComboBoxDisplay1.Text) && string.IsNullOrEmpty(m_ComboBoxDisplay2.Text))
            m_ComboBoxDisplay1.Text = m_ComboBoxDisplay2.Text;
        }
      );
      Task.Run(() =>
      {
        m_TimerDisplay.Stop();
        this.SafeBeginInvoke(
          () =>
          {
            if (m_ComboBoxId!.SelectedItem != null && m_ComboBoxParentId!.SelectedItem != null)
              BuildTree(m_ComboBoxParentId.Text,
                m_ComboBoxId.Text,
                m_ComboBoxDisplay1.Text,
                m_ComboBoxDisplay2.Text);
          });
      }, m_CancellationTokenSource.Token);
    }

    private void TimerSearchRestart(object? sender, EventArgs e)
    {
      m_TimerSearch.Stop();
      m_TimerSearch.Start();
    }

    public sealed class TreeData
    {
      public readonly ICollection<TreeData> Children = new List<TreeData>();
      public readonly string ID;
      public readonly string Title;
      public bool InCycle;
      private int m_StoreIndirect = -1;
      public string ParentID;
      public string Tag;
      public bool Visited;

      public TreeData(string id, string title, string? parentID = null)
      {
        if (string.IsNullOrEmpty(id))
          throw new ArgumentException(@"ID can not be empty", nameof(id));
        if (string.IsNullOrEmpty(title))
          throw new ArgumentException(@"Title can not be empty", nameof(title));
        ID = id;
        Title = title;
        ParentID = parentID ?? string.Empty;
        Tag = string.Empty;
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

      private static int GetInDirectChildren(TreeData root)
      {
        if (root.InCycle)
          return 0;

        return root.Children.Count + root.Children.Where(child => child.Children.Count > 0)
          .Sum(GetInDirectChildren);
      }
    }
  }
}