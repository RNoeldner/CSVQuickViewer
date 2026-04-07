/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace CsvTools;

/// <summary>
///   Windows Form to Display the hierarchy
/// </summary>
public class FormHierarchyDisplay : ResizeForm
{
  private readonly CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();

  private readonly DataRow[] m_DataRow;

  private readonly DataTable m_DataTable;

  private readonly System.Timers.Timer m_TimerDisplay = new System.Timers.Timer();

  private readonly System.Timers.Timer m_TimerSearch = new System.Timers.Timer();
  private System.ComponentModel.IContainer? components;

  private ToolStripMenuItem copyPathTreeToolStripMenuItem;
  private ToolStripMenuItem copyPathUpToolStripMenuItem;
  private ComboBox m_ComboBoxDisplay1;
  private ComboBox m_ComboBoxDisplay2;
  private ComboBox m_ComboBoxId;
  private ComboBox m_ComboBoxParentId;

  private TableLayoutPanel m_TableLayoutPanel1;
  private TextBox m_TextBoxValue;
  private ToolTip m_ToolTip;
  private IEnumerable<TreeData> m_TreeData = new List<TreeData>();
  private MultiSelectTreeView m_TreeView = new MultiSelectTreeView();
  private ToolStripSeparator toolStripSeparator1;
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

  /// <summary>
  ///   Builds the tree.
  /// </summary>
  public void BuildTree(string parent, string id, string? display1 = null, string? display2 = null)
  {
    this.RunWithHourglass(() =>
    {
      using var formProgress = new FormProgress("Building Tree", m_CancellationTokenSource.Token);
      formProgress.Show(this);
      formProgress.Maximum = m_DataRow.GetLength(0) * 2;

      BuildTreeData(parent, id, display1, display2, formProgress, formProgress.CancellationToken);
      formProgress.Maximum = 0;
      ShowTree(formProgress.CancellationToken);
    });
  }

  public void CloseAll() => CloseAllToolStripMenuItem_Click(this, EventArgs.Empty);

  public void ExpandAll() => ExpandAllToolStripMenuItem_Click(this, EventArgs.Empty);

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
      m_TreeView.Dispose();
      m_ToolTip?.Dispose();
      m_CancellationTokenSource.Dispose();
      components?.Dispose();
    }

    base.Dispose(disposing);
  }

  private static string GetNodeText(TreeNode node)
  {
    var pos = node.Text.LastIndexOf(" - Direct ", StringComparison.Ordinal);
    if (pos==-1)
      return node.Text;
    else
      return node.Text.Substring(0, pos);
  }

  /// <summary>
  ///   Adds the tree data node with child's.
  /// </summary>
  /// <param name="root">The root.</param>
  /// <param name="rootNode">The root node.</param>
  /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
  private void AddTreeDataNodeWithChild(in TreeData root, in TreeNode? rootNode,
    CancellationToken cancellationToken)
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

  private void AppendTreeNode(
    StringBuilder sb,
    TreeNode node,
    HashSet<TreeNode> relevant,
    List<bool> hasNextLevels)
  {
    if (!relevant.Contains(node))
      return;

    var parent = node.Parent;
    var siblings = parent?.Nodes ?? m_TreeView.Nodes;

    // determine if this node is last *among relevant siblings*
    bool isLast = true;
    for (int i = node.Index + 1; i < siblings.Count; i++)
    {
      if (relevant.Contains(siblings[i]))
      {
        isLast = false;
        break;
      }
    }

    // prefix from parent levels
    for (int level = 0; level < hasNextLevels.Count; level++)
      sb.Append(hasNextLevels[level] ? "│   " : "    ");

    // node prefix
    sb.Append(isLast ? "└── " : "├── ");
    sb.AppendLine(GetNodeText(node)); // reuse your helper logic

    // recurse into children
    hasNextLevels.Add(!isLast);

    foreach (TreeNode child in node.Nodes)
      AppendTreeNode(sb, child, relevant, hasNextLevels);

    hasNextLevels.RemoveAt(hasNextLevels.Count - 1);
  }

  /// <summary>
  ///   Builds the sub nodes.
  /// </summary>
  /// <param name="parent">The parent ID.</param>
  /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
  /// <returns></returns>
  private TreeNode[] BuildSubNodes(in TreeData parent, CancellationToken cancellationToken)
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
  private void BuildTreeData(string parentCol, string idCol, string? display1, string? display2,
    IProgress<ProgressInfo> process, CancellationToken cancellationToken)
  {
    DataColumn dataColumnParent = m_DataTable.Columns[parentCol] ?? throw new KeyNotFoundException($"Column '{parentCol}' not found in the data table.");
    DataColumn dataColumnID = m_DataTable.Columns[idCol] ?? throw new KeyNotFoundException($"Column '{idCol}' not found in the data table.");

    DataColumn? dataColumnDisplay1 = string.IsNullOrEmpty(display1) ? null : m_DataTable.Columns[display1];
    DataColumn? dataColumnDisplay2 = string.IsNullOrEmpty(display2) ? null : m_DataTable.Columns[display2];

    // Using a dictionary here to speed up lookups
    var treeDataDictionary = new DictionaryIgnoreCase<TreeData>();
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
      intervalAction.Invoke(process, $"Parent found {counter:N0}/{max:N0} ", counter);
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
    var additionalRootNodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var child in from child in treeDataDictionary.Values
                          where !string.IsNullOrEmpty(child.ParentID) && !treeDataDictionary.ContainsKey(child.ParentID)
                          select child)
    {
      additionalRootNodes.Add(child.ParentID);
    }

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
      if (string.IsNullOrEmpty(child.ParentID) && !string.Equals(child.ID, rootDataParentFound.ID, StringComparison.OrdinalIgnoreCase)
                                               && !string.Equals(child.ID, rootDataParentNotFound.ID, StringComparison.OrdinalIgnoreCase))
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
      intervalAction.Invoke(process, $"Set children {counter:N0}/{max:N0} ", counter);

      if (!string.IsNullOrEmpty(child.ParentID))
        treeDataDictionary[child.ParentID].Children.Add(child);
    }

    m_TreeData = treeDataDictionary.Values;
  }

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
      Extensions.ShowError(ex);
    }
  }

  private void copyPathToolStripMenuItem_Click(object sender, EventArgs e)
  {
    // get the chain up till root for current node like Selected -> parent1 -> parent2
    var sb = new StringBuilder();
    foreach (TreeNode startNode in m_TreeView.SelectedTreeNode)
    {
      if (sb.Length> 0)
        sb.AppendLine();
      sb.Append(GetNodeText(startNode));

      var nextNode = startNode.Parent;
      while (nextNode != null)
      {
        sb.Append(" → ");
        sb.Append(GetNodeText(nextNode));
        nextNode = nextNode.Parent;
      }
    }

    sb.ToString().SetClipboard();
  }

  private void copyPathTreeToolStripMenuItem_Click(object sender, EventArgs e)
  {
    var relevant = new HashSet<TreeNode>();

    foreach (TreeNode selected in m_TreeView.SelectedTreeNode)
    {
      var current = selected;
      while (current != null)
      {
        relevant.Add(current);
        current = current.Parent;
      }
    }

    var sb = new StringBuilder();

    foreach (TreeNode root in m_TreeView.Nodes)
      AppendTreeNode(sb, root, relevant, new List<bool>());

    sb.ToString().SetClipboard();
  }

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
      Extensions.ShowError(ex);
    }
  }
  private void FilterValueChangedElapsed(object? sender, ElapsedEventArgs? e) => SearchNext();

  private void FormHierarchyDisplay_FormClosing(object? sender, FormClosingEventArgs e) =>
    m_CancellationTokenSource.Cancel();

  private void FormHierarchyDisplay_KeyUp(object? sender, KeyEventArgs e)
  {
    if (e.KeyValue == (char) Keys.F3)
    {
      e.Handled = true;
      e.SuppressKeyPress = true;
      SearchNext();
    }
  }

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
      Extensions.ShowError(ex);
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
    components = new System.ComponentModel.Container();
    Label labelID;
    Label labelDisplay;
    Label labelParent;
    ContextMenuStrip contextMenuStrip;
    ToolStripMenuItem expandAllToolStripMenuItem;
    ToolStripMenuItem closeAllToolStripMenuItem;
    Label labelFind;
    toolStripSeparator1 = new ToolStripSeparator();
    copyPathUpToolStripMenuItem = new ToolStripMenuItem();
    copyPathTreeToolStripMenuItem = new ToolStripMenuItem();
    m_TableLayoutPanel1 = new TableLayoutPanel();
    m_ComboBoxId = new ComboBox();
    m_ComboBoxParentId = new ComboBox();
    m_TreeView = new MultiSelectTreeView();
    m_TextBoxValue = new TextBox();
    m_ComboBoxDisplay2 = new ComboBox();
    m_ComboBoxDisplay1 = new ComboBox();
    m_ToolTip = new ToolTip(components);
    labelID = new Label();
    labelDisplay = new Label();
    labelParent = new Label();
    contextMenuStrip = new ContextMenuStrip(components);
    expandAllToolStripMenuItem = new ToolStripMenuItem();
    closeAllToolStripMenuItem = new ToolStripMenuItem();
    labelFind = new Label();
    contextMenuStrip.SuspendLayout();
    m_TableLayoutPanel1.SuspendLayout();
    SuspendLayout();
    // 
    // labelID
    // 
    labelID.Anchor = AnchorStyles.Right;
    labelID.AutoSize = true;
    labelID.Location = new System.Drawing.Point(53, 7);
    labelID.Name = "labelID";
    labelID.Size = new System.Drawing.Size(18, 13);
    labelID.TabIndex = 3;
    labelID.Text = "ID";
    // 
    // labelDisplay
    // 
    labelDisplay.Anchor = AnchorStyles.Right;
    labelDisplay.AutoSize = true;
    labelDisplay.Location = new System.Drawing.Point(6, 34);
    labelDisplay.Name = "labelDisplay";
    labelDisplay.Size = new System.Drawing.Size(65, 13);
    labelDisplay.TabIndex = 5;
    labelDisplay.Text = "Display Text";
    // 
    // labelParent
    // 
    labelParent.Anchor = AnchorStyles.Right;
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
    contextMenuStrip.Items.AddRange(new ToolStripItem[] { expandAllToolStripMenuItem, closeAllToolStripMenuItem, toolStripSeparator1, copyPathTreeToolStripMenuItem, copyPathUpToolStripMenuItem });
    contextMenuStrip.Name = "contextMenuStrip";
    contextMenuStrip.Size = new System.Drawing.Size(301, 98);
    // 
    // expandAllToolStripMenuItem
    // 
    expandAllToolStripMenuItem.Name = "expandAllToolStripMenuItem";
    expandAllToolStripMenuItem.ShortcutKeys =  Keys.Control | Keys.Alt | Keys.Right;
    expandAllToolStripMenuItem.Size = new System.Drawing.Size(300, 22);
    expandAllToolStripMenuItem.Text = "Expand All";
    expandAllToolStripMenuItem.Click += ExpandAllToolStripMenuItem_Click;
    // 
    // closeAllToolStripMenuItem
    // 
    closeAllToolStripMenuItem.Name = "closeAllToolStripMenuItem";
    closeAllToolStripMenuItem.ShortcutKeys =  Keys.Control | Keys.Alt | Keys.Left;
    closeAllToolStripMenuItem.Size = new System.Drawing.Size(300, 22);
    closeAllToolStripMenuItem.Text = "Collapse All";
    closeAllToolStripMenuItem.Click += CloseAllToolStripMenuItem_Click;
    // 
    // toolStripSeparator1
    // 
    toolStripSeparator1.Name = "toolStripSeparator1";
    toolStripSeparator1.Size = new System.Drawing.Size(187, 6);
    // 
    // copyPathUpToolStripMenuItem
    // 
    copyPathUpToolStripMenuItem.Name = "copyPathUpToolStripMenuItem";
    copyPathUpToolStripMenuItem.ShortcutKeys =  Keys.Control | Keys.Shift | Keys.C;
    copyPathUpToolStripMenuItem.Size = new System.Drawing.Size(300, 22);
    copyPathUpToolStripMenuItem.Text = "Copy Paths (Selected Nodes)";
    copyPathUpToolStripMenuItem.Click += copyPathToolStripMenuItem_Click;
    // 
    // copyPathTreeToolStripMenuItem
    // 
    copyPathTreeToolStripMenuItem.Name = "copyPathTreeToolStripMenuItem";
    copyPathTreeToolStripMenuItem.ShortcutKeys =  Keys.Control | Keys.C;
    copyPathTreeToolStripMenuItem.Size = new System.Drawing.Size(300, 22);
    copyPathTreeToolStripMenuItem.Text = "Copy Hierarchy (Selected Nodes)";
    copyPathTreeToolStripMenuItem.Click += copyPathTreeToolStripMenuItem_Click;
    // 
    // labelFind
    // 
    labelFind.Anchor = AnchorStyles.Right;
    labelFind.AutoSize = true;
    labelFind.Location = new System.Drawing.Point(30, 87);
    labelFind.Name = "labelFind";
    labelFind.Size = new System.Drawing.Size(41, 13);
    labelFind.TabIndex = 5;
    labelFind.Text = "Search";
    // 
    // m_TableLayoutPanel1
    // 
    m_TableLayoutPanel1.ColumnCount = 3;
    m_TableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 74F));
    m_TableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
    m_TableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
    m_TableLayoutPanel1.Controls.Add(labelID, 0, 0);
    m_TableLayoutPanel1.Controls.Add(labelDisplay, 0, 1);
    m_TableLayoutPanel1.Controls.Add(labelParent, 0, 2);
    m_TableLayoutPanel1.Controls.Add(m_ComboBoxId, 1, 0);
    m_TableLayoutPanel1.Controls.Add(m_ComboBoxParentId, 1, 2);
    m_TableLayoutPanel1.Controls.Add(m_TreeView, 0, 4);
    m_TableLayoutPanel1.Controls.Add(m_TextBoxValue, 1, 3);
    m_TableLayoutPanel1.Controls.Add(labelFind, 0, 3);
    m_TableLayoutPanel1.Controls.Add(m_ComboBoxDisplay2, 2, 1);
    m_TableLayoutPanel1.Controls.Add(m_ComboBoxDisplay1, 1, 1);
    m_TableLayoutPanel1.Dock = DockStyle.Fill;
    m_TableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
    m_TableLayoutPanel1.Name = "m_TableLayoutPanel1";
    m_TableLayoutPanel1.RowCount = 5;
    m_TableLayoutPanel1.RowStyles.Add(new RowStyle());
    m_TableLayoutPanel1.RowStyles.Add(new RowStyle());
    m_TableLayoutPanel1.RowStyles.Add(new RowStyle());
    m_TableLayoutPanel1.RowStyles.Add(new RowStyle());
    m_TableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
    m_TableLayoutPanel1.Size = new System.Drawing.Size(323, 399);
    m_TableLayoutPanel1.TabIndex = 10;
    // 
    // m_ComboBoxId
    // 
    m_TableLayoutPanel1.SetColumnSpan(m_ComboBoxId, 2);
    m_ComboBoxId.Dock = DockStyle.Top;
    m_ComboBoxId.DropDownStyle = ComboBoxStyle.DropDownList;
    m_ComboBoxId.FormattingEnabled = true;
    m_ComboBoxId.Location = new System.Drawing.Point(77, 3);
    m_ComboBoxId.Name = "m_ComboBoxId";
    m_ComboBoxId.Size = new System.Drawing.Size(243, 21);
    m_ComboBoxId.TabIndex = 0;
    m_ToolTip.SetToolTip(m_ComboBoxId, "Unique identifier for each node. Used to count and link child nodes.");
    m_ComboBoxId.SelectedIndexChanged += TimeDisplayRestart;
    // 
    // m_ComboBoxParentId
    // 
    m_TableLayoutPanel1.SetColumnSpan(m_ComboBoxParentId, 2);
    m_ComboBoxParentId.Dock = DockStyle.Top;
    m_ComboBoxParentId.DropDownStyle = ComboBoxStyle.DropDownList;
    m_ComboBoxParentId.FormattingEnabled = true;
    m_ComboBoxParentId.Location = new System.Drawing.Point(77, 57);
    m_ComboBoxParentId.Name = "m_ComboBoxParentId";
    m_ComboBoxParentId.Size = new System.Drawing.Size(243, 21);
    m_ComboBoxParentId.TabIndex = 1;
    m_ToolTip.SetToolTip(m_ComboBoxParentId, "References the parent node. Used to build the hierarchy or group related nodes.");
    m_ComboBoxParentId.SelectedIndexChanged += TimeDisplayRestart;
    // 
    // m_TreeView
    // 
    m_TableLayoutPanel1.SetColumnSpan(m_TreeView, 3);
    m_TreeView.ContextMenuStrip = contextMenuStrip;
    m_TreeView.Dock = DockStyle.Fill;
    m_TreeView.Location = new System.Drawing.Point(3, 110);
    m_TreeView.Name = "m_TreeView";
    m_TreeView.Size = new System.Drawing.Size(317, 286);
    m_TreeView.TabIndex = 9;
    m_TreeView.KeyUp += FormHierarchyDisplay_KeyUp;
    // 
    // m_TextBoxValue
    // 
    m_TextBoxValue.Dock = DockStyle.Top;
    m_TextBoxValue.Location = new System.Drawing.Point(77, 84);
    m_TextBoxValue.Name = "m_TextBoxValue";
    m_TextBoxValue.Size = new System.Drawing.Size(118, 20);
    m_TextBoxValue.TabIndex = 2;
    m_ToolTip.SetToolTip(m_TextBoxValue, "Search text. Supports wildcards (* and ?).");
    m_TextBoxValue.TextChanged += TimerSearchRestart;
    m_TextBoxValue.KeyDown += m_TextBoxValue_KeyDown;
    // 
    // m_ComboBoxDisplay2
    // 
    m_ComboBoxDisplay2.Dock = DockStyle.Top;
    m_ComboBoxDisplay2.DropDownStyle = ComboBoxStyle.DropDownList;
    m_ComboBoxDisplay2.FormattingEnabled = true;
    m_ComboBoxDisplay2.Location = new System.Drawing.Point(201, 30);
    m_ComboBoxDisplay2.Name = "m_ComboBoxDisplay2";
    m_ComboBoxDisplay2.Size = new System.Drawing.Size(119, 21);
    m_ComboBoxDisplay2.TabIndex = 15;
    m_ToolTip.SetToolTip(m_ComboBoxDisplay2, "Defines the text shown in the tree. Supports primary and secondary values (e.g. “Name – Details”).");
    m_ComboBoxDisplay2.SelectedIndexChanged += TimeDisplayRestart;
    // 
    // m_ComboBoxDisplay1
    // 
    m_ComboBoxDisplay1.Dock = DockStyle.Top;
    m_ComboBoxDisplay1.DropDownStyle = ComboBoxStyle.DropDownList;
    m_ComboBoxDisplay1.FormattingEnabled = true;
    m_ComboBoxDisplay1.Location = new System.Drawing.Point(77, 30);
    m_ComboBoxDisplay1.Name = "m_ComboBoxDisplay1";
    m_ComboBoxDisplay1.Size = new System.Drawing.Size(118, 21);
    m_ComboBoxDisplay1.TabIndex = 16;
    m_ToolTip.SetToolTip(m_ComboBoxDisplay1, "Defines the text shown in the tree. Supports primary and secondary values (e.g. “Name – Details”).");
    m_ComboBoxDisplay1.SelectedIndexChanged += TimeDisplayRestart;
    // 
    // FormHierarchyDisplay
    // 
    AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
    ClientSize = new System.Drawing.Size(323, 399);
    Controls.Add(m_TableLayoutPanel1);
    FormBorderStyle = FormBorderStyle.SizableToolWindow;
    MinimumSize = new System.Drawing.Size(339, 196);
    Name = "FormHierarchyDisplay";
    Text = "Tree / Hierarchy";
    FormClosing += FormHierarchyDisplay_FormClosing;
    Load += FormHierarchyDisplay_Load;
    KeyUp += FormHierarchyDisplay_KeyUp;
    contextMenuStrip.ResumeLayout(false);
    m_TableLayoutPanel1.ResumeLayout(false);
    m_TableLayoutPanel1.PerformLayout();
    ResumeLayout(false);
  }

  private void m_TextBoxValue_KeyDown(object? sender, KeyEventArgs e)
  {
    if (e.KeyValue == (char) Keys.Enter
      ||  e.KeyValue == (char) Keys.F3)
    {
      e.Handled = true;
      e.SuppressKeyPress = true;
      SearchNext();
    }
  }

  private bool MarkInCycle(TreeData treeData, HashSet<TreeData> visitedEntries)
  {
    if (visitedEntries.Contains(treeData))
    {
      treeData.InCycle = true;
      return true;
    }

    visitedEntries.Add(treeData);

    foreach (var child in treeData.Children)
    {
      if (MarkInCycle(child, visitedEntries))
      {
        treeData.InCycle = true; // Propagate cycle up the tree
        return true;             // Stop further processing once a cycle is found
      }
    }

    return false;
  }

  private void Search(string text)
  {
    var validNode = new List<TreeNode>();
    var findRegex = new Regex("^" + Regex.Escape(text.Replace('%', '*')).Replace(@"\*", ".*").Replace(@"\?", "."), RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));

    CollectMatches(m_TreeView.Nodes);

    if (validNode.Count == 0)
      return;
    var currentFound = validNode.IndexOf(m_TreeView.SelectedNode);
    m_TreeView.BeginUpdate();
    try
    {
      if (currentFound==validNode.Count-1)
        currentFound=-1;
      for (int i = validNode.Count- (currentFound+1); i>1; i--)
        if (!(validNode[currentFound+i].Parent?.IsExpanded?? false))
          validNode[currentFound+i].EnsureVisible();
      validNode[currentFound+1].EnsureVisible();
      m_TreeView.SelectedNode = validNode[currentFound+1];
    }
    finally
    {
      m_TreeView.EndUpdate();
    }

    void CollectMatches(TreeNodeCollection nodes)
    {
      foreach (TreeNode node in nodes)
      {
        if (findRegex.IsMatch(node.Text))
          validNode.Add(node);
        CollectMatches(node.Nodes);
      }
    }
  }

  private void SearchNext()
  {
    m_TextBoxValue!.SafeInvoke(() =>
    {
      var search = m_TextBoxValue?.Text.Trim() ?? string.Empty;
      if (search.Length<2)
        return;
      m_TextBoxValue!.RunWithHourglass(() =>
      {
        try
        {
          Search(search);
        }
        catch (Exception ex)
        {
          Extensions.ShowError(ex);
        }
      });
    });
  }
  /// <summary>
  ///   Shows the tree.
  /// </summary>
  private void ShowTree(CancellationToken cancellationToken)
  {
    if (m_TreeView is null)
      return;
    m_TreeView.BeginUpdate();
    try
    {
      m_TreeView.Nodes.Clear();

      foreach (var treeData in m_TreeData)
        treeData.Visited = false;

      // Add top-level nodes 
      foreach (var rootData in m_TreeData.Where(td => string.IsNullOrEmpty(td.ParentID) && td.Children.Count>0))
      {
        foreach(var nodes in rootData.Children)
          m_TreeView.Nodes.AddRange(BuildSubNodes(rootData, cancellationToken));
      }

      cancellationToken.ThrowIfCancellationRequested();
      Extensions.ProcessUIElements();
      // If everything has been visited, exit early
      if (m_TreeData.All(treeData => treeData.Visited))
        return;
      var rootNode = new TreeNode("-- Circular References --");
      m_TreeView.Nodes.Add(rootNode);

      foreach (var treeData in m_TreeData.Where(td => !td.Visited))
          MarkInCycle(treeData, new HashSet<TreeData>());

      cancellationToken.ThrowIfCancellationRequested();
      Extensions.ProcessUIElements();

      // Add nodes that are in cycles
      foreach (var root in m_TreeData.Where(td => !td.Visited && td.InCycle))
          AddTreeDataNodeWithChild(root, rootNode, cancellationToken);

      Extensions.ProcessUIElements();
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
    _ = Task.Run(() =>
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
    public readonly List<TreeData> Children = new List<TreeData>();
    public readonly string ID;
    public readonly string Title;
    public bool InCycle;
    public string ParentID;
    public string Tag;
    public bool Visited;
    private int m_StoreIndirect = -1;

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
          m_StoreIndirect = CalculateIndirectChildren(this);
        return m_StoreIndirect;
      }
    }

    private static int CalculateIndirectChildren(TreeData node)
    {
      if (node.InCycle)
        return 0;

      return node.Children.Count + node.Children.Sum(c => c.Children.Count > 0 ? CalculateIndirectChildren(c) : 0);
    }
  }
}