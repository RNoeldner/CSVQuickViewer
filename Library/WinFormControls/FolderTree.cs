using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;
using Directory = Pri.LongPath.Directory;

namespace CsvTools
{
  /// <summary>
  ///   Summary description for ExplorerTree.
  /// </summary>
  public class FolderTree : Form
  {
    private Button m_ButtonCancel;
    private Button m_ButtonOk;
    private ImageList m_ImageList;
    private TreeNode m_RootTreeNode;
    private TableLayoutPanel m_TableLayoutPanel;
    private TreeView m_TreeView;
    private IContainer components;
    private TextBox m_TxtPath;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FolderTree" /> class.
    /// </summary>
    public FolderTree()
    {
      InitializeComponent();
      ReBuildTree();
    }

    [Description("Selected Path")]
    public string SelectedPath
    {
      get => m_TxtPath.Text;
      set
      {
        var newVal = value ?? string.Empty;
        if (!newVal.Equals(m_TxtPath.Text, StringComparison.OrdinalIgnoreCase))
          NavigateToPath(value);
      }
    }

    public static bool HasDirectoryPermission(string directory, WindowsPrincipal currentuser,
      FileSystemRights desiredFlag)
    {
      var security = Directory.GetAccessControl(directory.GetDirectoryName());
      var result = false;
      foreach (FileSystemAccessRule rule in security.GetAccessRules(true, true, typeof(NTAccount)))
      {
        // check if its about the checked right
        if ((rule.FileSystemRights & desiredFlag) == 0)
          continue;

        // check if this is regarding the user
        if (rule.IdentityReference.Value.StartsWith("S-1-", StringComparison.Ordinal))
          if (!currentuser.IsInRole(new SecurityIdentifier(rule.IdentityReference.Value)))
            continue;
          else if (!currentuser.IsInRole(rule.IdentityReference.Value))
            continue;

        // An deny would overrule any allows
        if (rule.AccessControlType == AccessControlType.Deny)
          return false;

        // we need at least one Allow
        if (rule.AccessControlType == AccessControlType.Allow)
          result = true;
      }

      return result;
    }

    /// <summary>
    ///   Forces the control to invalidate its client area and immediately redraw itself and any child controls.
    /// </summary>
    public new void Refresh()
    {
      SetCurrentPath(null);
      ReBuildTree();
    }

    /// <summary>
    ///   Sets the current path.
    /// </summary>
    /// <param name="strPath">The string path.</param>
    public void SetCurrentPath(string strPath)
    {
      if (string.IsNullOrEmpty(strPath))
        strPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

      if (!FileSystemUtils.DirectoryExists(strPath))
        strPath = Application.StartupPath;

      SelectedPath = strPath;
    }

    private void buttonOK_Click(object sender, EventArgs e) => Close();

    private void NavigateToPath(string path)
    {
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        var current = m_RootTreeNode;
        var found = true;
        if (!path.EndsWith(@"\", StringComparison.Ordinal))
          path += @"\";

        while (found && current != null)
        {
          found = false;
          foreach (TreeNode child in current.Nodes)
          {
            var mypath = child.Tag.ToString();
            if (!mypath.EndsWith(@"\", StringComparison.Ordinal))
              mypath += @"\";

            if (!path.StartsWith(mypath, StringComparison.OrdinalIgnoreCase))
              continue;
            child.TreeView.Focus();
            child.EnsureVisible();
            child.Expand();
            current = child;
            found = true;
            if (path.Equals(mypath, StringComparison.OrdinalIgnoreCase))
            {
              child.TreeView.SelectedNode = child;
              return;
            }

            break;
          }
        }
      }
      catch (Exception e1)
      {
        System.Windows.Forms.MessageBox.Show("Error: " + e1.Message);
      }
      finally
      {
        Cursor.Current = oldCursor;
      }
    }

    private void PopulateNode(TreeNode parentNode)
    {
      if (parentNode == null || parentNode.Nodes.Count != 0)
        return;
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        var di = new DriveInfo(parentNode.Tag.ToString());
        var dirList = Directory.GetDirectories(parentNode.Tag.ToString());
        var directories = new SortedList<string, string>();
        var currentuser = new WindowsPrincipal(WindowsIdentity.GetCurrent());
        // check Permissions and sort
        foreach (var directory in dirList)
          // HasDirectoryPermission does not work on network
          if (di.DriveType == DriveType.Network ||
              HasDirectoryPermission(directory, currentuser, FileSystemRights.Write))
            directories.Add(directory, directory);
        var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        // add to tree
        foreach (var directory in directories.Values)
        {
          var imageIndex = 1;
          if (directory == myDocuments)
            imageIndex = 6;
          else if (directory == desktop)
            imageIndex = 7;

          parentNode.Nodes.Add(new TreeNode
          {
            Tag = directory,
            Text = directory.Substring(directory.LastIndexOf(@"\", StringComparison.Ordinal) + 1),
            ImageIndex = imageIndex,
            SelectedImageIndex = imageIndex
          });
        }
      }
      catch (SystemException)
      {
      }
      finally
      {
        Cursor.Current = oldCursor;
      }
    }

    /// <summary>
    ///   Rebuilds the complete tree.
    /// </summary>
    private void ReBuildTree()
    {
      m_TreeView.AfterExpand -= TreeNode_AfterExpand;
      m_TreeView.SuspendLayout();
      m_TreeView.Nodes.Clear();

      m_RootTreeNode = new TreeNode
      {
        Tag = "Computer",
        Text = "Computer",
        ImageIndex = 4
      };
      // m_RootTreeNode.SelectedImageIndex = 4;
      m_TreeView.Nodes.Add(m_RootTreeNode);

      foreach (var drive in DriveInfo.GetDrives())
      {
        var nodeDrive = new TreeNode
        {
          Tag = drive,
          Text = drive.Name
        };
        switch (drive.DriveType)
        {
          case DriveType.CDRom:
            nodeDrive.ImageIndex = 2;
            nodeDrive.SelectedImageIndex = 2;
            break;

          case DriveType.Network:
            nodeDrive.ImageIndex = 3;
            nodeDrive.SelectedImageIndex = 3;
            break;

          case DriveType.Removable:
            nodeDrive.ImageIndex = 5;
            nodeDrive.SelectedImageIndex = 5;
            break;

          default:
            nodeDrive.ImageIndex = 0;
            nodeDrive.SelectedImageIndex = 0;
            PopulateNode(nodeDrive);
            break;
        }

        m_RootTreeNode.Nodes.Add(nodeDrive);
      }

      m_TreeView.ResumeLayout(true);
      m_RootTreeNode.Expand();
      m_TreeView.AfterExpand += TreeNode_AfterExpand;
    }

    #region Component Designer generated code

    /// <summary>
    ///   Required method for Designer support - do not modify
    ///   the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FolderTree));
      this.m_TxtPath = new System.Windows.Forms.TextBox();
      this.m_TreeView = new System.Windows.Forms.TreeView();
      this.m_ImageList = new System.Windows.Forms.ImageList(this.components);
      this.m_ButtonOk = new System.Windows.Forms.Button();
      this.m_ButtonCancel = new System.Windows.Forms.Button();
      this.m_TableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.m_TableLayoutPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // m_TxtPath
      // 
      this.m_TxtPath.AllowDrop = true;
      this.m_TxtPath.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
      this.m_TxtPath.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
      this.m_TableLayoutPanel.SetColumnSpan(this.m_TxtPath, 2);
      this.m_TxtPath.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_TxtPath.Location = new System.Drawing.Point(3, 3);
      this.m_TxtPath.Name = "m_TxtPath";
      this.m_TxtPath.Size = new System.Drawing.Size(322, 26);
      this.m_TxtPath.TabIndex = 61;
      this.m_TxtPath.DragDrop += new System.Windows.Forms.DragEventHandler(this.txtPath_DragDrop);
      this.m_TxtPath.DragEnter += new System.Windows.Forms.DragEventHandler(this.txtPath_DragEnter);
      this.m_TxtPath.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtPath_KeyUp);
      // 
      // m_TreeView
      // 
      this.m_TreeView.AllowDrop = true;
      this.m_TreeView.BackColor = System.Drawing.Color.White;
      this.m_TableLayoutPanel.SetColumnSpan(this.m_TreeView, 2);
      this.m_TreeView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_TreeView.ImageIndex = 0;
      this.m_TreeView.ImageList = this.m_ImageList;
      this.m_TreeView.Location = new System.Drawing.Point(3, 35);
      this.m_TreeView.Name = "m_TreeView";
      this.m_TreeView.SelectedImageIndex = 0;
      this.m_TreeView.ShowLines = false;
      this.m_TreeView.ShowRootLines = false;
      this.m_TreeView.Size = new System.Drawing.Size(322, 417);
      this.m_TreeView.TabIndex = 59;
      this.m_TreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeNode_AfterSelect);
      this.m_TreeView.DragDrop += new System.Windows.Forms.DragEventHandler(this.txtPath_DragDrop);
      this.m_TreeView.DragEnter += new System.Windows.Forms.DragEventHandler(this.txtPath_DragEnter);
      this.m_TreeView.DoubleClick += new System.EventHandler(this.TreeNode_DoubleClick);
      // 
      // m_ImageList
      // 
      this.m_ImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("m_ImageList.ImageStream")));
      this.m_ImageList.TransparentColor = System.Drawing.Color.Transparent;
      this.m_ImageList.Images.SetKeyName(0, "Drive");
      this.m_ImageList.Images.SetKeyName(1, "Folder");
      this.m_ImageList.Images.SetKeyName(2, "CDRom");
      this.m_ImageList.Images.SetKeyName(3, "Network");
      this.m_ImageList.Images.SetKeyName(4, "Desktop");
      this.m_ImageList.Images.SetKeyName(5, "Removable");
      this.m_ImageList.Images.SetKeyName(6, "DocumentFolder");
      this.m_ImageList.Images.SetKeyName(7, "DesktopFolder");
      // 
      // m_ButtonOk
      // 
      this.m_ButtonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.m_ButtonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.m_ButtonOk.Location = new System.Drawing.Point(115, 458);
      this.m_ButtonOk.Name = "m_ButtonOk";
      this.m_ButtonOk.Size = new System.Drawing.Size(102, 34);
      this.m_ButtonOk.TabIndex = 62;
      this.m_ButtonOk.Text = "&OK";
      this.m_ButtonOk.UseVisualStyleBackColor = true;
      this.m_ButtonOk.Click += new System.EventHandler(this.buttonOK_Click);
      // 
      // m_ButtonCancel
      // 
      this.m_ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.m_ButtonCancel.Location = new System.Drawing.Point(223, 458);
      this.m_ButtonCancel.Name = "m_ButtonCancel";
      this.m_ButtonCancel.Size = new System.Drawing.Size(102, 34);
      this.m_ButtonCancel.TabIndex = 63;
      this.m_ButtonCancel.Text = "&Cancel";
      this.m_ButtonCancel.UseVisualStyleBackColor = true;
      this.m_ButtonCancel.Click += new System.EventHandler(this.buttonOK_Click);
      // 
      // m_TableLayoutPanel
      // 
      this.m_TableLayoutPanel.ColumnCount = 2;
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.Controls.Add(this.m_ButtonCancel, 1, 2);
      this.m_TableLayoutPanel.Controls.Add(this.m_TxtPath, 0, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_TreeView, 0, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_ButtonOk, 0, 2);
      this.m_TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_TableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      this.m_TableLayoutPanel.Name = "m_TableLayoutPanel";
      this.m_TableLayoutPanel.RowCount = 3;
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.Size = new System.Drawing.Size(328, 495);
      this.m_TableLayoutPanel.TabIndex = 64;
      // 
      // FolderTree
      // 
      this.AcceptButton = this.m_ButtonOk;
      this.BackColor = System.Drawing.SystemColors.Control;
      this.CancelButton = this.m_ButtonCancel;
      this.ClientSize = new System.Drawing.Size(328, 495);
      this.Controls.Add(this.m_TableLayoutPanel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "FolderTree";
      this.Text = "Select Folder";
      this.m_TableLayoutPanel.ResumeLayout(false);
      this.m_TableLayoutPanel.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion Component Designer generated code

    private void TreeNode_AfterExpand(object sender, TreeViewEventArgs e)
    {
      // Do not act on expanding the root node, its has been populated before
      // it has slow child nodes these are only opened when selected
      if (e.Node == m_RootTreeNode)
        return;
      foreach (TreeNode child in e.Node.Nodes)
        PopulateNode(child);
    }

    private void TreeNode_AfterSelect(object sender, TreeViewEventArgs e)
    {
      // The network drives are not populated by default do so now if needed
      PopulateNode(e.Node);
      m_TxtPath.Text = e.Node.Tag.ToString();
    }

    private void TreeNode_DoubleClick(object sender, EventArgs e)
    {
      var treeNode = m_TreeView.SelectedNode;
      if (treeNode == null)
        return;
      if (!m_TreeView.SelectedNode.IsExpanded)
        m_TreeView.SelectedNode.Collapse();
      else
        PopulateNode(treeNode);
    }

    private void txtPath_DragDrop(object sender, DragEventArgs e)
    {
      if (!e.Data.GetDataPresent(DataFormats.FileDrop, false))
        return;
      var files = (string[])e.Data.GetData(DataFormats.FileDrop);
      foreach (var fileName in files)
      {
        NavigateToPath(FileSystemUtils.DirectoryExists(fileName) ? fileName : fileName.GetDirectoryName());
        return;
      }
    }

    private void txtPath_DragEnter(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
        e.Effect = DragDropEffects.All;
    }

    private void txtPath_KeyUp(object sender, KeyEventArgs e)
    {
      // hitting ender of pasting information will navigate to it
      if (e.KeyCode != Keys.Enter && (e.KeyCode != Keys.V || e.Modifiers != Keys.Control) &&
          (e.KeyCode != Keys.Insert || e.Modifiers != Keys.Shift))
        return;
      NavigateToPath(m_TxtPath.Text);
      m_TxtPath.Focus();
    }
  }
}