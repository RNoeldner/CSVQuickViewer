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

namespace CsvTools
{
  using JetBrains.Annotations;
  using System.Drawing;
  using System.Windows.Forms;

  /// <summary>
  ///   CheckedListBox that can live in a ToolStrip
  /// </summary>
  public class ToolStripCheckedListBox : ToolStripControlHost
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="ToolStripCheckedListBox" /> class.
    /// </summary>
    public ToolStripCheckedListBox()
      : base(
        new CheckedListBox
        {
          BorderStyle = BorderStyle.FixedSingle,
          MultiColumn = true,
          CheckOnClick = true,
          SelectionMode = SelectionMode.One,
          ThreeDCheckBoxes = true,
          ColumnWidth = 300,
          MaximumSize = new Size(600, 600),
          BackColor = SystemColors.Window
        })
    {
    }

    /// <summary>
    ///   Tell the world that an item was checked
    /// </summary>
    public event ItemCheckEventHandler ItemCheck;

    /// <summary>
    ///   Gets the checked ListBox.
    /// </summary>
    /// <value>The checked ListBox.</value>
    [NotNull]
    public CheckedListBox CheckedListBoxControl
    {
      get
      {
        return Control as CheckedListBox;
      }
    }

    /// <summary>
    ///   Gets the items shown in the checked list box
    /// </summary>
    public CheckedListBox.ObjectCollection Items => CheckedListBoxControl.Items;

    /// <summary>
    ///   Listen for events on the underlying control
    /// </summary>
    /// <param name="control"></param>
    protected override void OnSubscribeControlEvents(Control control)
    {
      base.OnSubscribeControlEvents(control);

      var checkedListBoxControl = (CheckedListBox) control;
      checkedListBoxControl.ItemCheck += OnItemCheck;
    }

    /// <summary>
    ///   Stop listening for events on the underlying control
    /// </summary>
    /// <param name="control"></param>
    protected override void OnUnsubscribeControlEvents(Control control)
    {
      base.OnUnsubscribeControlEvents(control);

      var checkedListBoxControl = (CheckedListBox) control;
      checkedListBoxControl.ItemCheck -= OnItemCheck;
    }

    /// <summary>
    ///   Trigger the ItemCheck event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnItemCheck(object sender, ItemCheckEventArgs e) => ItemCheck?.Invoke(this, e);
  }
}