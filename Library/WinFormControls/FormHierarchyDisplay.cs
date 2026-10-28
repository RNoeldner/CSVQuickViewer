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
      Extensions.ProcessUiElements();
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
    DataColumn dataColumnId = m_DataTable.Columns[idCol] ?? throw new KeyNotFoundException($"Column '{idCol}' not found in the data table.");

    DataColumn? dataColumnDisplay1 = string.IsNullOrEmpty(display1) ? null : m_DataTable.Columns[display1];
    DataColumn? dataColumnDisplay2 = string.IsNullOrEmpty(display2) ? null : m_DataTable.Columns[display2];

    // Using a dictionary here to speed up lookups
    var treeDataDictionary = new DictionaryIgnoreCase<TreeData>();
    var rootDataParentFound = new TreeData("{R}", "Parent found / No Parent");

    treeDataDictionary.Add(rootDataParentFound.Id, rootDataParentFound);

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
      var id = dataRow[dataColumnId.Ordinal].ToString();
      if (string.IsNullOrEmpty(id))
        continue;
      string titleText;
      var val1 = dataColumnDisplay1 != null ? dataRow[dataColumnDisplay1.Ordinal]?.ToString() : null;
      var val2 = dataColumnDisplay2 != null ? dataRow[dataColumnDisplay2.Ordinal]?.ToString() : null;

      if (!string.IsNullOrEmpty(val1) && !string.IsNullOrEmpty(val2))
        titleText = $"{val1} - {val2}";
      else if (!string.IsNullOrEmpty(val1))
        titleText = val1!;
      else if (!string.IsNullOrEmpty(val2))
        titleText = val2!;
      else
        titleText = id;

      var treeData = new TreeData(id, titleText, dataRow[dataColumnParent.Ordinal].ToString());
      if (dataColumnDisplay1 != null)
        treeData.Tag = Convert.ToString(dataRow[dataColumnDisplay1.Ordinal]) ?? string.Empty;

      // Store the display
      if (!treeDataDictionary.ContainsKey(id))
        treeDataDictionary.Add(id, treeData);
    }

    // Generate a list of missing parents
    var additionalRootNodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    foreach (var child in treeDataDictionary.Values)
    {
      if (!string.IsNullOrEmpty(child.ParentId) && !treeDataDictionary.ContainsKey(child.ParentId))
      {
        additionalRootNodes.Add(child.ParentId);
      }
    }

    var rootDataParentNotFound = new TreeData("{M}", "Parent not found");

    if (additionalRootNodes.Count > 0)
    {
      treeDataDictionary.Add(rootDataParentNotFound.Id, rootDataParentNotFound);
      counter = 0;
      max = additionalRootNodes.Count;
      process.SetMaximum(max);

      // Create new entries
      foreach (var parentId in additionalRootNodes)
      {
        cancellationToken.ThrowIfCancellationRequested();
        counter++;
        intervalAction.Invoke(process, $"Parent not found (Step 1) {counter}/{max} ", counter);
        var childData = new TreeData(parentId, $"{parentId}", rootDataParentNotFound.Id);
        treeDataDictionary.Add(parentId, childData);
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
      if (string.IsNullOrEmpty(child.ParentId) && !string.Equals(child.Id, rootDataParentFound.Id, StringComparison.OrdinalIgnoreCase)
                                               && !string.Equals(child.Id, rootDataParentNotFound.Id, StringComparison.OrdinalIgnoreCase))
      {
        child.ParentId = rootDataParentFound.Id;
      }
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

      if (!string.IsNullOrEmpty(child.ParentId))
        treeDataDictionary[child.ParentId].AddChild(child);
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

  private void CopyPathToolStripMenuItem_Click(object? sender, EventArgs e)
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

  private void CopyPathTreeToolStripMenuItem_Click(object? sender, EventArgs e)
  {
    var relevant = new HashSet<TreeNode>();

    foreach (var selected in m_TreeView.SelectedTreeNode)
    {
      var current = selected;
      while (current != null)
      {
        relevant.Add(current);
        current = current.Parent;
      }
    }

    int estimatedCapacity = Math.Max(64, relevant.Count * 45);
    var sb = new StringBuilder(estimatedCapacity);

    // Local Function to recursively append nodes to the StringBuilder
    void AppendTreeNode(TreeNode node, HashSet<TreeNode> relevant, List<bool> hasNextLevels)
    {
      if (!relevant.Contains(node))
        return;

      var parent = node.Parent;
      var siblings = parent?.Nodes ?? m_TreeView.Nodes;

      // Determine if this node is the last *relevant* sibling by scanning backwards
      bool isLast = true;
      for (int i = siblings.Count - 1; i > node.Index; i--)
      {
        if (relevant.Contains(siblings[i]))
        {
          isLast = false;
          break;
        }
      }

      // Prefix from parent levels
      for (int level = 0; level < hasNextLevels.Count; level++)
        sb.Append(hasNextLevels[level] ? "│   " : "    ");

      // Node prefix
      sb.Append(isLast ? "└── " : "├── ");
      sb.AppendLine(GetNodeText(node));

      // Recurse into children
      hasNextLevels.Add(!isLast);

      foreach (TreeNode child in node.Nodes)
        AppendTreeNode(child, relevant, hasNextLevels);

      hasNextLevels.RemoveAt(hasNextLevels.Count - 1);
    }

    foreach (TreeNode root in m_TreeView.Nodes)
      AppendTreeNode(root, relevant, new List<bool>());

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
    Label labelId;
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
    labelId = new Label();
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
    labelId.Anchor = AnchorStyles.Right;
    labelId.AutoSize = true;
    labelId.Location = new System.Drawing.Point(53, 7);
    labelId.Name = "labelId";
    labelId.Size = new System.Drawing.Size(18, 13);
    labelId.TabIndex = 3;
    labelId.Text = "ID";
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
    copyPathUpToolStripMenuItem.Click += CopyPathToolStripMenuItem_Click;
    // 
    // copyPathTreeToolStripMenuItem
    // 
    copyPathTreeToolStripMenuItem.Name = "copyPathTreeToolStripMenuItem";
    copyPathTreeToolStripMenuItem.ShortcutKeys =  Keys.Control | Keys.C;
    copyPathTreeToolStripMenuItem.Size = new System.Drawing.Size(300, 22);
    copyPathTreeToolStripMenuItem.Text = "Copy Hierarchy (Selected Nodes)";
    copyPathTreeToolStripMenuItem.Click += CopyPathTreeToolStripMenuItem_Click;
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
    m_TableLayoutPanel1.Controls.Add(labelId, 0, 0);
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
    m_TextBoxValue.KeyDown += TextBoxValue_KeyDown;
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

  private void TextBoxValue_KeyDown(object? sender, KeyEventArgs e)
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
    // If the node is already in the set, we've looped back to an ancestor.
    if (visitedEntries.Contains(treeData))
    {
      treeData.InCycle = true;
      return true;
    }

    // Add to current path
    visitedEntries.Add(treeData);

    try
    {
      // Check if any child leads to a cycle (short-circuiting foreach)
      bool hasCycle = false;
      foreach (var child in treeData.Children)
      {
        if (MarkInCycle(child, visitedEntries))
        {
          hasCycle = true;
          break;
        }
      }

      treeData.InCycle = hasCycle;
      return treeData.InCycle;
    }
    finally
    {
      // BACKTRACK: Remove the node so it doesn't interfere with other branches
      visitedEntries.Remove(treeData);
    }
  }

  private void Search(string text)
  {
    var validNode = new List<TreeNode>();
    var findRegex = new Regex("^" + Regex.Escape(text.Replace('%', '*')).Replace(@"\*", ".*").Replace(@"\?", "."), RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromMilliseconds(200));

    CollectMatches(m_TreeView.Nodes);

    if (validNode.Count == 0 || m_TreeView.SelectedNode == null)
      return;
    var currentFound = validNode.IndexOf(m_TreeView.SelectedNode);
    m_TreeView.BeginUpdate();
    try
    {
      if (currentFound==validNode.Count-1)
        currentFound=-1;
      for (int i = validNode.Count- (currentFound+1); i>1; i--)
      {
        if (!(validNode[currentFound+i].Parent?.IsExpanded?? false))
          validNode[currentFound+i].EnsureVisible();
      }

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

      // Add top-level nodes (replacing .Where() LINQ query)
      foreach (var rootData in m_TreeData)
      {
        if (string.IsNullOrEmpty(rootData.ParentId) && rootData.Children.Count > 0)
        {
          foreach (var _ in rootData.Children)
          {
            m_TreeView.Nodes.AddRange(BuildSubNodes(rootData, cancellationToken));
          }
        }
      }

      cancellationToken.ThrowIfCancellationRequested();
      Extensions.ProcessUiElements();

      // Check if everything has been visited (replacing .All() LINQ method)
      bool allVisited = true;
      foreach (var treeData in m_TreeData)
      {
        if (!treeData.Visited)
        {
          allVisited = false;
          break;
        }
      }
      if (allVisited)
        return;

      var rootNode = new TreeNode("-- Circular References --");
      m_TreeView.Nodes.Add(rootNode);

      foreach (var treeData in m_TreeData)
      {
        if (!treeData.Visited)
          MarkInCycle(treeData, new HashSet<TreeData>());
      }

      cancellationToken.ThrowIfCancellationRequested();
      Extensions.ProcessUiElements();

      // Add nodes that are in cycles (replacing .Where() LINQ query)
      foreach (var root in m_TreeData)
      {
        if (!root.Visited && root.InCycle)
          AddTreeDataNodeWithChild(root, rootNode, cancellationToken);
      }

      Extensions.ProcessUiElements();
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
            BuildTree(m_ComboBoxParentId.Text, m_ComboBoxId.Text, m_ComboBoxDisplay1.Text, m_ComboBoxDisplay2.Text);
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
    private readonly List<TreeData> m_Children = new();
    /// <summary>
    ///   Collection of child nodes belonging to this tree node.
    /// </summary>
    /// <summary>
    /// Gets the collection of child nodes belonging to this tree node.
    /// </summary>
    public IReadOnlyList<TreeData> Children => m_Children;

    /// <summary>
    ///   Unique identifier for the node.
    /// </summary>
    public readonly string Id;

    /// <summary>
    ///   Base display title of the node.
    /// </summary>
    public readonly string Title;

    /// <summary>
    ///   Gets or sets a value indicating whether the node is part of a circular reference.
    /// </summary>
    public bool InCycle;

    /// <summary>
    ///   Identifier of the parent node.
    /// </summary>
    public string ParentId;

    /// <summary>
    ///   Additional tag data associated with the node.
    /// </summary>
    public string Tag;

    /// <summary>
    ///   Gets or sets a value indicating whether the node has been visited during traversal.
    /// </summary>
    public bool Visited;
    private int m_StoreIndirect = -1;

    /// <summary>
    ///   Initializes a new instance of the <see cref="TreeData" /> class.
    /// </summary>
    /// <param name="id">The unique identifier for the node.</param>
    /// <param name="title">The display title of the node.</param>
    /// <param name="parentId">The identifier of the parent node, if any.</param>
    public TreeData(string id, string title, string? parentId = null)
    {
      if (string.IsNullOrEmpty(id))
        throw new ArgumentException("ID can not be empty", nameof(id));
      if (string.IsNullOrEmpty(title))
        throw new ArgumentException("Title can not be empty", nameof(title));
      Id = id;
      Title = title;
      ParentId = parentId ?? string.Empty;
      Tag = string.Empty;
    }

    /// <summary>
    ///   Gets the formatted title string displayed in the tree, appending direct and indirect child counts when applicable.
    /// </summary>
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
    /// <summary>
    /// Adds a child node to this tree node, safely updating the collection and invalidating the indirect count cache.
    /// </summary>
    /// <param name="child">The child node to add.</param>
    public void AddChild(TreeData child)
    {
      if (child is null) throw new ArgumentNullException(nameof(child));
      m_Children.Add(child);
      m_StoreIndirect = -1; // Invalidate indirect cache on structural updates
    }

    /// <summary>
    ///   Gets the count of immediate child nodes.
    /// </summary>
    private int DirectChildren => Children.Count;

    /// <summary>
    ///   Gets the total count of all recursive descendant nodes (cached after first calculation).
    /// </summary>
    private int InDirectChildren
    {
      get
      {
        if (m_StoreIndirect < 0)
          m_StoreIndirect = CalculateIndirectChildren(this);
        return m_StoreIndirect;
      }
    }

    /// <summary>
    ///   Recursively calculates the total number of indirect child descendants, avoiding infinite loops in cycles.
    /// </summary>
    /// <param name="node">The tree node to evaluate.</param>
    /// <returns>The total count of all nested descendants.</returns>
    private static int CalculateIndirectChildren(TreeData node)
    {
      if (node.InCycle)
        return 0;

      int count = node.Children.Count;
      foreach (var c in node.Children)
      {
        if (c.Children.Count > 0)
        {
          count += CalculateIndirectChildren(c);
        }
      }
      return count;
    }
  }
}