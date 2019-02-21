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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   An extension of the regular TreeView
  /// </summary>
  public class MultiselectTreeView : TreeView
  {
    private TreeNode m_FirstNode;

    /// <summary>
    ///   Gets or sets the selected tree node.
    /// </summary>
    /// <value>The selected tree node.</value>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ICollection<TreeNode> SelectedTreeNode { get; } = new HashSet<TreeNode>();

    /// <summary>
    ///   Raises the <see cref="AfterSelect" /> event.
    /// </summary>
    /// <param name="e">
    ///   A <see cref="TreeViewEventArgs" /> that contains the event data.
    /// </param>
    protected override void OnAfterSelect(TreeViewEventArgs e)
    {
      if (e.Node == null)
        return;
      base.OnAfterSelect(e);

      var bControl = ModifierKeys == Keys.Control;
      var bShift = ModifierKeys == Keys.Shift;

      if (bControl)
      {
        if (!SelectedTreeNode.Contains(e.Node)) // new node ?
        {
          SelectedTreeNode.Add(e.Node);
        }
        else // not new, remove it from the collection
        {
          RemovePaintFromNodes();
          SelectedTreeNode.Remove(e.Node);
        }

        PaintSelectedNodes();
      }
      else
      {
        // SHIFT is pressed
        if (bShift)
        {
          var myQueue = new List<TreeNode>();

          var uppernode = m_FirstNode;
          Contract.Assume(m_FirstNode != null);

          var bottomnode = e.Node;
          // case 1 : begin and end nodes are parent
          var bParent = IsParent(m_FirstNode, e.Node); // is m_firstNode parent (direct or not) of e.Node
          if (!bParent)
          {
            bParent = IsParent(bottomnode, uppernode);
            if (bParent) // swap nodes
            {
              var t = uppernode;
              uppernode = bottomnode;
              bottomnode = t;
            }
          }

          if (bParent)
          {
            var n = bottomnode;
            while (n != uppernode.Parent)
            {
              if (!SelectedTreeNode.Contains(n)) // new node ?
                myQueue.Add(n);

              n = n.Parent;
            }
          }
          // case 2 : nor the begin nor the end node are descendant one another
          else
          {
            if (uppernode.Parent == null && bottomnode.Parent == null ||
                uppernode.Parent != null && uppernode.Parent.Nodes.Contains(bottomnode)) // are they siblings ?
            {
              var nIndexUpper = uppernode.Index;
              var nIndexBottom = bottomnode.Index;
              if (nIndexBottom < nIndexUpper) // reversed?
              {
                var t = uppernode;
                uppernode = bottomnode;
                bottomnode = t;
                nIndexUpper = uppernode.Index;
                nIndexBottom = bottomnode.Index;
              }

              var n = uppernode;
              while (nIndexUpper <= nIndexBottom)
              {
                if (!SelectedTreeNode.Contains(n)) // new node ?
                  myQueue.Add(n);

                n = n.NextNode;

                nIndexUpper++;
              } // end while
            }
            else
            {
              if (!SelectedTreeNode.Contains(uppernode)) myQueue.Add(uppernode);
              if (!SelectedTreeNode.Contains(bottomnode)) myQueue.Add(bottomnode);
            }
          }

          foreach (var item in myQueue)
            SelectedTreeNode.Add(item);

          PaintSelectedNodes();
          m_FirstNode = e.Node; // let us chain several SHIFTs if we like it
        } // end if m_bShift
        else
        {
          // in the case of a simple click, just add this item
          if (SelectedTreeNode != null && SelectedTreeNode.Count > 0)
          {
            RemovePaintFromNodes();
            SelectedTreeNode.Clear();
          }

          SelectedTreeNode?.Add(e.Node);
        }
      }
    }

    /// <summary>
    ///   Raises the <see cref="BeforeSelect" /> event.
    /// </summary>
    /// <param name="e">
    ///   A <see cref="TreeViewCancelEventArgs" /> that contains the event data.
    /// </param>
    protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
    {
      base.OnBeforeSelect(e);

      var bControl = ModifierKeys == Keys.Control;
      var bShift = ModifierKeys == Keys.Shift;

      // selecting twice the node while pressing CTRL ?
      if (bControl && SelectedTreeNode.Contains(e.Node))
      {
        // un-select it (let framework know we don't want selection this time)
        e.Cancel = true;

        // update nodes
        RemovePaintFromNodes();
        SelectedTreeNode.Remove(e.Node);
        PaintSelectedNodes();
        return;
      }

      if (!bShift) m_FirstNode = e.Node; // store begin of shift sequence
    }

    /// <summary>
    ///   Raises the <see cref="KeyDown" /> event.
    /// </summary>
    /// <param name="e">
    ///   A <see cref="KeyEventArgs" /> that contains the event data.
    /// </param>
    protected override void OnKeyDown(KeyEventArgs e)
    {
      base.OnKeyDown(e);
      // Handle CRTL -A
      if (e.Control && e.KeyCode == Keys.A)
      {
        SelectedTreeNode.Clear();
        foreach (TreeNode item in Nodes)
          AddNodeWithSubnodes(item);

        PaintSelectedNodes();
        // SelectedTreeNode = this.Nodes;
      }

      // Handle CRTL -C
      if (!e.Control || e.KeyCode != Keys.C) return;
      {
        if (SelectedTreeNode.Count == 0)
          return;

        var minLevel = int.MaxValue;
        var maxLevel = int.MinValue;
        foreach (var item in SelectedTreeNode)
        {
          if (minLevel > item.Level)
            minLevel = item.Level;

          if (maxLevel < item.Level)
            maxLevel = item.Level;
        }

        var buffer = new StringBuilder();
        var sbHtml = new StringBuilder();
        var style = ApplicationSetting.HTMLStyle;

        sbHtml.AppendLine(style.TableOpen);
        foreach (var item in SelectedTreeNode.OrderBy(x => x.FullPath))
        {
          var text = item.Text;
          sbHtml.Append(style.TROpen);
          if (item.Tag is FormHierachyDisplay.TreeData data)
          {
            text = data.Title;
            if (!string.IsNullOrEmpty(data.Tag))
            {
              sbHtml.Append(HTMLStyle.AddTd("<td>{0}</td>", data.Tag));
              if (text.StartsWith(data.Tag, StringComparison.Ordinal))
                text = text.Substring(data.Tag.Length).TrimStart(' ', '-');
            }
            else
            {
              sbHtml.Append(style.TDEmpty);
            }
          }

          for (var level = minLevel; level <= maxLevel; level++)
          {
            buffer.Append("\t");
            if (level < item.Level)
              sbHtml.Append(style.TDEmpty);
            if (level != item.Level) continue;
            sbHtml.Append(HTMLStyle.AddTd("<td colspan='{0}'>{1}</td>", (maxLevel - level + 1).ToString(System.Globalization.CultureInfo.InvariantCulture), text));
            buffer.Append(item.Text);
          }

          sbHtml.AppendLine(style.TRClose);
          buffer.AppendLine();
        }

        sbHtml.AppendLine(style.TableClose);

        var dataObject = new DataObject();
        dataObject.SetData(DataFormats.Html, true, style.ConvertToHtmlFragment(sbHtml.ToString()));
        dataObject.SetData(DataFormats.Text, true, buffer.ToString());

        Clipboard.Clear();
        Clipboard.SetDataObject(dataObject, false, 5, 200);
      }
    }

    /// <summary>
    ///   Determines whether the specified parent node is parent.
    /// </summary>
    /// <param name="parentNode">The parent node.</param>
    /// <param name="childNode">The child node.</param>
    /// <returns><c>true</c> if the specified parent node is parent; otherwise, <c>false</c>.</returns>
    private static bool IsParent(TreeNode parentNode, TreeNode childNode)
    {
      if (parentNode == childNode)
        return true;

      var n = childNode;
      var bFound = false;
      while (!bFound && n != null)
      {
        n = n.Parent;
        bFound = n == parentNode;
      }

      return bFound;
    }

    /// <summary>
    ///   Adds the node with sub nodes.
    /// </summary>
    /// <param name="item">The item.</param>
    private void AddNodeWithSubnodes(TreeNode item)
    {
      if (item == null)
        return;
      SelectedTreeNode.Add(item);
      if (!item.IsExpanded) return;
      foreach (TreeNode subItem in item.Nodes)
        AddNodeWithSubnodes(subItem);
    }

    /// <summary>
    ///   Paints the selected nodes.
    /// </summary>
    private void PaintSelectedNodes()
    {
      foreach (var n in SelectedTreeNode)
      {
        n.BackColor = SystemColors.Highlight;
        n.ForeColor = SystemColors.HighlightText;
      }

      Refresh();
    }

    /// <summary>
    ///   Removes the paint from nodes.
    /// </summary>
    private void RemovePaintFromNodes()
    {
      foreach (var n in SelectedTreeNode)
      {
        n.BackColor = BackColor;
        n.ForeColor = ForeColor;
      }

      Refresh();
    }
  }
}