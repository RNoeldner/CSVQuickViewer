using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace CsvTools
{
  /// <summary>
  /// Combo box control displaying items in groups
  /// </summary>
  [LookupBindingProperties(
    "DataSource",
    "DisplayMember",
    "ValueMember",
    "GroupMember")]
  public class GroupingComboBox : ComboBox
  {
    private readonly TextFormatFlags m_TextFormatFlags = TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter;
    private BindingSource m_BindingSource;
    private bool m_DropdownClosed;
    private Font m_GroupFont;
    private PropertyDescriptor m_GroupPropertyDescriptor;
    private string m_GroupPropertyName = string.Empty;

    /// <summary>
    /// Initializes a new instance of the GroupedComboBox class.
    /// </summary>
    public GroupingComboBox()
    {
      base.DrawMode = DrawMode.OwnerDrawVariable;
      SetStyle(ControlStyles.SupportsTransparentBackColor, false);
      m_GroupFont = new Font(base.Font, FontStyle.Bold);
      SetComboBoxStyle();
    }

    // used for change detection and grouping
    // used in measuring/painting
    /// <summary>
    /// Gets or sets the data source for this GroupedComboBox.
    /// </summary>
    [DefaultValue("")]
    [RefreshProperties(RefreshProperties.Repaint)]
    [AttributeProvider(typeof(IListSource))]
    public new object DataSource
    {
      get
      {
        // binding source should be transparent to the user
        return (m_BindingSource != null) ? m_BindingSource.DataSource : null;
      }
      set
      {
        if (value != null)
        {
          m_BindingSource = value as BindingSource;
          if (m_BindingSource == null)
            m_BindingSource = new BindingSource(value, String.Empty);
          m_BindingSource.ListChanged += new ListChangedEventHandler(mBindingSource_ListChanged);
          SetPropertyDescriptor();
          SetDataSourceAndSort();
        }
        else
        {
          // remove binding
          base.DataSource = m_BindingSource = null;
        }
      }
    }

    /// <summary>
    /// Gets a value indicating whether the drawing of elements in the list will be handled by user code.
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new DrawMode DrawMode
    {
      get
      {
        return base.DrawMode;
      }
    }

    [Category("Appearance")]
    public Font GroupFont
    {
      get { return m_GroupFont; }
      set { m_GroupFont = value; }
    }

    /// <summary>
    /// Gets or sets the property to use when grouping items in the list.
    /// </summary>
    [Category("Data")]
    [DefaultValue("(none)")]
    public string GroupMember
    {
      get
      {
        return m_GroupPropertyName;
      }
      set
      {
        string newVal = (value == "(none)") ? string.Empty : value ?? String.Empty;
        if (!newVal.Equals(m_GroupPropertyName))
        {
          m_GroupPropertyName = newVal;
          // Can not do regular sorting if we have groups
          if (m_GroupPropertyName.Length > 0)
            base.Sorted = false;
          SetPropertyDescriptor();
          SetDataSourceAndSort();
        }
      }
    }

    /// <summary>
    /// Draws the ComboBox.
    /// </summary>
    /// <param name="graphics">The graphics.</param>
    /// <param name="bounds">The bounds.</param>
    /// <param name="state">The state.</param>
    internal static void DrawComboBox(Graphics graphics, Rectangle bounds, ComboBoxState state)
    {
      Rectangle comboBounds = bounds;
      comboBounds.Inflate(1, 1);
      ButtonRenderer.DrawButton(graphics, comboBounds, GetPushButtonState(state));

      Rectangle buttonBounds = new Rectangle(
        bounds.Left + (bounds.Width - 17),
        bounds.Top,
        17,
        bounds.Height - (state != ComboBoxState.Pressed ? 1 : 0)
      );

      Rectangle buttonClip = buttonBounds;
      buttonClip.Inflate(-2, -2);

      using (Region oldClip = graphics.Clip.Clone())
      {
        graphics.SetClip(buttonClip, System.Drawing.Drawing2D.CombineMode.Intersect);
        ComboBoxRenderer.DrawDropDownButton(graphics, buttonBounds, state);
        graphics.SetClip(oldClip, System.Drawing.Drawing2D.CombineMode.Replace);
      }
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.ComboBox" /> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
      if (m_BindingSource != null)
        m_BindingSource.Dispose();
      base.Dispose(disposing);
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.ComboBox.DrawItem" /> event.
    /// </summary>
    /// <param name="e">A <see cref="T:System.Windows.Forms.DrawItemEventArgs" /> that contains the event data.</param>
    protected override void OnDrawItem(DrawItemEventArgs e)
    {
      base.OnDrawItem(e);

      if ((e.Index >= 0) && (e.Index < Items.Count))
      {
        // get noteworthy states
        bool comboBoxEdit = (e.State & DrawItemState.ComboBoxEdit) == DrawItemState.ComboBoxEdit;
        bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        bool noAccelerator = (e.State & DrawItemState.NoAccelerator) == DrawItemState.NoAccelerator;
        bool disabled = (e.State & DrawItemState.Disabled) == DrawItemState.Disabled;
        bool focus = (e.State & DrawItemState.Focus) == DrawItemState.Focus;

        // determine grouping
        string groupText = GetGroupText(Items[e.Index]);
        bool isGroupStart = DetermineGroupStart(e.Index, groupText) && !comboBoxEdit;
        bool hasGroup = (groupText != String.Empty) && !comboBoxEdit;

        // the item text will appear in a different color, depending on its state
        Color textColor;
        if (disabled)
          textColor = SystemColors.GrayText;
        else if (!comboBoxEdit && selected)
          textColor = SystemColors.HighlightText;
        else
          textColor = ForeColor;

        // items will be indented if they belong to a group
        Rectangle itemBounds = Rectangle.FromLTRB(
          e.Bounds.X + (hasGroup ? 12 : 0),
          e.Bounds.Y + (isGroupStart ? (e.Bounds.Height / 2) : 0),
          e.Bounds.Right,
          e.Bounds.Bottom
        );
        Rectangle groupBounds = new Rectangle(
          e.Bounds.X,
          e.Bounds.Y,
          e.Bounds.Width,
          e.Bounds.Height / 2
        );

        if (isGroupStart && selected)
        {
          // ensure that the group header is never highlighted
          e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
          e.Graphics.FillRectangle(new SolidBrush(BackColor), groupBounds);
        }
        else if (disabled)
        {
          // disabled appearance
          e.Graphics.FillRectangle(Brushes.WhiteSmoke, e.Bounds);
        }
        else if (!comboBoxEdit)
        {
          // use the default background-painting logic
          e.DrawBackground();
        }

        // render group header text
        if (isGroupStart) TextRenderer.DrawText(
          e.Graphics,
          groupText,
          m_GroupFont,
          groupBounds,
          ForeColor,
          m_TextFormatFlags
        );

        // render item text
        TextRenderer.DrawText(
          e.Graphics,
          GetItemText(Items[e.Index]),
          Font,
          itemBounds,
          textColor,
          m_TextFormatFlags
        );

        // paint the focus rectangle if required
        if (focus && !noAccelerator)
        {
          if (isGroupStart && selected)
          {
            // don't draw the focus rectangle around the group header
            ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.FromLTRB(groupBounds.X, itemBounds.Y, itemBounds.Right, itemBounds.Bottom));
          }
          else
          {
            // use default focus rectangle painting logic
            e.DrawFocusRectangle();
          }
        }
      }
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.ComboBox.DropDown" /> event.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
    protected override void OnDropDown(EventArgs e)
    {
      base.OnDropDown(e);
      m_DropdownClosed = false;
      Invalidate();
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.ComboBox.DropDownClosed" /> event.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
    protected override void OnDropDownClosed(EventArgs e)
    {
      base.OnDropDownClosed(e);
      m_DropdownClosed = true;
      Invalidate();
    }

    /// <summary>
    /// Repaints the control when it receives input focus.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
    protected override void OnGotFocus(EventArgs e)
    {
      base.OnGotFocus(e);
      Invalidate();
    }

    /// <summary>
    /// Repaints the control when it loses input focus.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
    protected override void OnLostFocus(EventArgs e)
    {
      base.OnLostFocus(e);
      Invalidate();
    }

    /// <summary>
    /// Determines the size of a list item.
    /// </summary>
    /// <param name="e">The <see cref="T:System.Windows.Forms.MeasureItemEventArgs" /> that was raised.</param>
    protected override void OnMeasureItem(MeasureItemEventArgs e)
    {
      base.OnMeasureItem(e);

      e.ItemHeight = Font.Height;

      string groupText = GetGroupText(Items[e.Index]);
      if (DetermineGroupStart(e.Index, groupText))
      {
        // the first item in each group will be twice as tall in order to accommodate the group header
        e.ItemHeight *= 2;
        e.ItemWidth = Math.Max(
          e.ItemWidth,
          TextRenderer.MeasureText(
            e.Graphics,
            groupText,
            m_GroupFont,
            new Size(e.ItemWidth, e.ItemHeight),
            m_TextFormatFlags
          ).Width
        );
      }
    }

    /// <summary>
    /// When the parent control changes, updates the font used to render group names.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
    protected override void OnParentChanged(EventArgs e)
    {
      base.OnParentChanged(e);
      m_GroupFont = new Font(Font, FontStyle.Bold);
    }

    /// <summary>
    /// Redraws the control when the selected item changes.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
    protected override void OnSelectedItemChanged(EventArgs e)
    {
      base.OnSelectedItemChanged(e);
      Invalidate();
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Control.StyleChanged" /> event.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
    protected override void OnStyleChanged(EventArgs e)
    {
      base.OnStyleChanged(e);
      SetComboBoxStyle();
    }

    /// <summary>
    /// Set the right control style
    /// </summary>
    protected void SetComboBoxStyle()
    {
      SetStyle(ControlStyles.UserPaint, DropDownStyle == ComboBoxStyle.DropDownList);
      SetStyle(ControlStyles.AllPaintingInWmPaint, DropDownStyle == ComboBoxStyle.DropDownList);

      if (IsHandleCreated)
        RecreateHandle();
    }

    /// <summary>
    /// Converts a ComboBoxState into its equivalent PushButtonState value.
    /// </summary>
    /// <param name="combo">The state of the combo box.</param>
    /// <returns></returns>
    private static PushButtonState GetPushButtonState(ComboBoxState combo)
    {
      switch (combo)
      {
        case ComboBoxState.Disabled:
          return PushButtonState.Disabled;

        case ComboBoxState.Hot:
          return PushButtonState.Hot;

        case ComboBoxState.Pressed:
          return PushButtonState.Pressed;

        default:
          return PushButtonState.Normal;
      }
    }

    /// <summary>
    /// Determines whether the list item at the specified index is the start of a new group. In all
    /// cases, populates the string representation of the group that the item belongs to.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="currentGroupText">The group text.</param>
    /// <returns></returns>
    private bool DetermineGroupStart(int index, string currentGroupText)
    {
      if ((index >= 0) && (index < Items.Count))
      {
        if ((index == 0) && (!string.IsNullOrEmpty(currentGroupText)))
          return true;
        if (index > 0 && !currentGroupText.Equals(GetGroupText(Items[index - 1]), StringComparison.CurrentCultureIgnoreCase))
          return true;
      }
      return false;
    }

    private string GetGroupText(object item)
    {
      if (m_GroupPropertyDescriptor != null && item != null)
        // get the group value using the property descriptor
        return Convert.ToString(m_GroupPropertyDescriptor.GetValue(item));
      return string.Empty;
    }

    /// <summary>
    /// Determines the state in which to render the control (when using buffered painting).
    /// </summary>
    /// <returns></returns>
    private ComboBoxState GetRenderState()
    {
      if (!Enabled)
      {
        return ComboBoxState.Disabled;
      }
      else if (DroppedDown && !m_DropdownClosed)
      {
        return ComboBoxState.Pressed;
      }
      else if (ClientRectangle.Contains(PointToClient(Cursor.Position)))
      {
        if (((Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left) && !m_DropdownClosed)
        {
          return ComboBoxState.Pressed;
        }
        else
        {
          return ComboBoxState.Hot;
        }
      }
      else
      {
        return ComboBoxState.Normal;
      }
    }

    /// <summary>
    /// Re-synchronizes the internal sorted collection when the data source changes.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="ListChangedEventArgs"/> instance containing the event data.</param>
    private void mBindingSource_ListChanged(object sender, ListChangedEventArgs e)
    {
      SetDataSourceAndSort();
    }

    /// <summary>
    /// Sorts the data source and sets it
    /// </summary>
    private void SetDataSourceAndSort()
    {
      if (m_BindingSource == null)
        return;
      // Only do the sorting if the base class does not do this
      if (!base.Sorted && !String.IsNullOrEmpty(DisplayMember))
      {
        IComparer comparer = null;
        foreach (PropertyDescriptor descriptor in m_BindingSource.GetItemProperties(null))
        {
          if (descriptor.Name.Equals(DisplayMember))
          {
            comparer = new TwoLevelComparer(m_GroupPropertyDescriptor, descriptor);
            break;
          }
        }
        if (comparer == null)
          throw new ApplicationException("DisplayMember property not found");
        // rebuild the collection and sort using custom logic
        ArrayList arrayList = new ArrayList();
        foreach (object item in m_BindingSource)
          arrayList.Add(item);
        arrayList.Sort(comparer);
        base.DataSource = new BindingSource(arrayList, String.Empty);
      }
      else
        base.DataSource = m_BindingSource;
    }

    /// <summary>
    /// Sets the group property descriptor.
    /// </summary>
    private void SetPropertyDescriptor()
    {
      if (m_BindingSource == null)
        return;

      foreach (PropertyDescriptor descriptor in m_BindingSource.GetItemProperties(null))
      {
        if (m_GroupPropertyName.Equals(descriptor.Name))
        {
          m_GroupPropertyDescriptor = descriptor;
          break;
        }
      }
    }
  }

  /// <summary>
  /// A Comparer for a group or hierarchy structure
  /// </summary>
  internal class TwoLevelComparer : IComparer
  {
    private readonly PropertyDescriptor m_FirstLevel;
    private readonly PropertyDescriptor m_SecondLevel;

    /// <summary>
    /// Initializes a new instance of the <see cref="TwoLevelComparer"/> class.
    /// </summary>
    /// <param name="firstLevel">The first level.</param>
    /// <param name="secondLevel">The second level.</param>
    public TwoLevelComparer(PropertyDescriptor firstLevel, PropertyDescriptor secondLevel)
    {
      if (firstLevel == null)
        throw new ArgumentNullException("firstLevel");
      if (secondLevel == null)
        throw new ArgumentNullException("secondLevel");
      m_FirstLevel = firstLevel;
      m_SecondLevel = secondLevel;
    }

    /// <summary>
    /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>
    /// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero <paramref name="x" /> is less than <paramref name="y" />. Zero <paramref name="x" /> equals <paramref name="y" />. Greater than zero <paramref name="x" /> is greater than <paramref name="y" />.
    /// </returns>
    public int Compare(object x, object y)
    {
      int res = StringComparer.CurrentCultureIgnoreCase.Compare(Convert.ToString(m_FirstLevel.GetValue(x)), Convert.ToString(m_FirstLevel.GetValue(y)));
      if (res != 0)
        return res;
      else
        return StringComparer.CurrentCultureIgnoreCase.Compare(Convert.ToString(m_SecondLevel.GetValue(x)), Convert.ToString(m_SecondLevel.GetValue(y)));
    }
  }
}