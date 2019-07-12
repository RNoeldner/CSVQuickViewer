using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   Combo box control displaying items in groups
  /// </summary>
  [LookupBindingProperties(
    "DataSource",
    "DisplayMember",
    "ValueMember",
    "GroupMember")]
  public sealed class GroupingComboBox : ComboBox
  {
    private readonly TextFormatFlags m_TextFormatFlags =
      TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine |
      TextFormatFlags.VerticalCenter;

    private BindingSource m_BindingSource;
    private PropertyDescriptor m_DisplayPropertyDescriptor;
    private PropertyDescriptor m_GroupPropertyDescriptor;
    private string m_GroupPropertyName = "(none)";
    private System.Timers.Timer m_TimerFilter = new System.Timers.Timer();

    /// <summary>
    ///   Initializes a new instance of the GroupedComboBox class.
    /// </summary>
    public GroupingComboBox()
    {
      base.DrawMode = DrawMode.OwnerDrawVariable;

      SetStyle(ControlStyles.SupportsTransparentBackColor, false);
      GroupFont = new Font(Font, FontStyle.Bold);
      TextChanged += delegate
      { m_TimerFilter.Stop(); m_TimerFilter.Start(); };
      AutoCompleteMode = AutoCompleteMode.None;
      AutoCompleteSource = AutoCompleteSource.None;
      DisplayMemberChanged += GroupingComboBox_DisplayMemberChanged;
      SetComboBoxStyle();

      m_TimerFilter.Interval = 600;
      m_TimerFilter.AutoReset = true;

      m_TimerFilter.Elapsed += delegate
      {
        m_TimerFilter.Stop();
        if (SelectedIndex >= 0)
          return;

        SetDataSourceAndSort();

        DroppedDown = (Text.Length > 2);
      };
    }

    // used for change detection and grouping
    // used in measuring/painting
    /// <summary>
    ///   Gets or sets the data source for this GroupedComboBox.
    /// </summary>
    [DefaultValue(null)]
    [RefreshProperties(RefreshProperties.Repaint)]
    [AttributeProvider(typeof(IListSource))]
    public new object DataSource
    {
      get => m_BindingSource?.DataSource;
      set
      {
        m_BindingSource = value as BindingSource;
        if (m_BindingSource == null)
          m_BindingSource = new BindingSource(value, string.Empty);
        m_BindingSource.ListChanged += BindingSourceListChanged;
        SetPropertyDescriptor();
        SetDataSourceAndSort();
      }
    }

    /// <summary>
    ///   Gets a value indicating whether the drawing of elements in the list will be handled by user code.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new DrawMode DrawMode => base.DrawMode;

    [Category("Appearance")] public Font GroupFont { get; set; }

    /// <summary>
    ///   Gets or sets the property to use when grouping items in the list.
    /// </summary>
    [Category("Data")]
    [DefaultValue("(none)")]
    public string GroupMember
    {
      get => m_GroupPropertyName;
      set
      {
        var newVal = value == "(none)" ? string.Empty : value ?? string.Empty;
        if (newVal.Equals(m_GroupPropertyName))
          return;
        m_GroupPropertyName = newVal;
        // Can not do regular sorting if we have groups
        if (m_GroupPropertyName.Length > 0)
          Sorted = false;
        SetPropertyDescriptor();
        SetDataSourceAndSort();
      }
    }

    public bool IsFiltering { get; private set; } = false;

    /// <summary>
    ///   Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.ComboBox" /> and optionally releases
    ///   the managed resources.
    /// </summary>
    /// <param name="disposing">
    ///   true to release both managed and unmanaged resources; false to release only unmanaged
    ///   resources.
    /// </param>
    protected override void Dispose(bool disposing)
    {
      if (m_BindingSource != null)
        m_BindingSource.Dispose();
      if (GroupFont != null)
        GroupFont.Dispose();
      if (m_TimerFilter != null)
        m_TimerFilter.Dispose();
      base.Dispose(disposing);
    }

    /// <summary>
    ///   Raises the <see cref="E:System.Windows.Forms.ComboBox.DrawItem" /> event.
    /// </summary>
    /// <param name="e">A <see cref="T:System.Windows.Forms.DrawItemEventArgs" /> that contains the event data.</param>
    protected override void OnDrawItem(DrawItemEventArgs e)
    {
      base.OnDrawItem(e);

      if (e.Index < 0 || e.Index >= Items.Count)
        return;
      // get noteworthy states
      var comboBoxEdit = (e.State & DrawItemState.ComboBoxEdit) == DrawItemState.ComboBoxEdit;
      var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
      var noAccelerator = (e.State & DrawItemState.NoAccelerator) == DrawItemState.NoAccelerator;
      var disabled = (e.State & DrawItemState.Disabled) == DrawItemState.Disabled;
      var focus = (e.State & DrawItemState.Focus) == DrawItemState.Focus;

      // determine grouping
      var groupText = GetGroupText(Items[e.Index]);
      var isGroupStart = DetermineGroupStart(e.Index, groupText) && !comboBoxEdit;
      var hasGroup = !string.IsNullOrEmpty(groupText) && !comboBoxEdit;

      // the item text will appear in a different color, depending on its state
      Color textColor;
      if (disabled)
        textColor = SystemColors.GrayText;
      else if (!comboBoxEdit && selected)
        textColor = SystemColors.HighlightText;
      else
        textColor = ForeColor;

      // items will be indented if they belong to a group
      var itemBounds = Rectangle.FromLTRB(
        e.Bounds.X + (hasGroup ? 12 : 0),
        e.Bounds.Y + (isGroupStart ? e.Bounds.Height / 2 : 0),
        e.Bounds.Right,
        e.Bounds.Bottom
      );
      var groupBounds = new Rectangle(
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
      if (isGroupStart)
      {
        TextRenderer.DrawText(
          e.Graphics,
          groupText,
          GroupFont,
          groupBounds,
          ForeColor,
          m_TextFormatFlags
        );
      }

      // render item text GetItem Text is not working properly after the list was empty once
      TextRenderer.DrawText(
        e.Graphics,
        GetText(Items[e.Index]),
        Font,
        itemBounds,
        textColor,
        m_TextFormatFlags
      );

      // paint the focus rectangle if required
      if (!focus || noAccelerator)
        return;
      if (isGroupStart && selected)
      {
        ControlPaint.DrawFocusRectangle(e.Graphics,
          Rectangle.FromLTRB(groupBounds.X, itemBounds.Y, itemBounds.Right, itemBounds.Bottom));
      }
      else
        e.DrawFocusRectangle();
    }

    /// <summary>
    ///   Raises the <see cref="E:System.Windows.Forms.ComboBox.DropDown" /> event.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
    protected override void OnDropDown(EventArgs e)
    {
      base.OnDropDown(e);
      Invalidate();
    }

    /// <summary>
    ///   Raises the <see cref="E:System.Windows.Forms.ComboBox.DropDownClosed" /> event.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
    protected override void OnDropDownClosed(EventArgs e)
    {
      base.OnDropDownClosed(e);
      Invalidate();
    }

    /// <summary>
    ///   Repaints the control when it receives input focus.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
    protected override void OnGotFocus(EventArgs e)
    {
      base.OnGotFocus(e);
      Invalidate();
    }

    /// <summary>
    ///   Determines the size of a list item.
    /// </summary>
    /// <param name="e">The <see cref="T:System.Windows.Forms.MeasureItemEventArgs" /> that was raised.</param>
    protected override void OnMeasureItem(MeasureItemEventArgs e)
    {
      base.OnMeasureItem(e);

      e.ItemHeight = Font.Height;

      var groupText = GetGroupText(Items[e.Index]);
      if (!DetermineGroupStart(e.Index, groupText))
        return;
      // the first item in each group will be twice as tall in order to accommodate the group header
      e.ItemHeight *= 2;
      e.ItemWidth = Math.Max(
        e.ItemWidth,
        TextRenderer.MeasureText(
          e.Graphics,
          groupText,
          GroupFont,
          new Size(e.ItemWidth, e.ItemHeight),
          m_TextFormatFlags
        ).Width
      );
    }

    /// <summary>
    ///   When the parent control changes, updates the font used to render group names.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
    protected override void OnParentChanged(EventArgs e)
    {
      base.OnParentChanged(e);
      GroupFont = new Font(Font, FontStyle.Bold);
    }

    /// <summary>
    ///   Raises the <see cref="E:System.Windows.Forms.Control.StyleChanged" /> event.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
    protected override void OnStyleChanged(EventArgs e)
    {
      base.OnStyleChanged(e);
      SetComboBoxStyle();
    }

    /// <summary>
    ///   Re-synchronizes the internal sorted collection when the data source changes.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="ListChangedEventArgs" /> instance containing the event data.</param>
    private void BindingSourceListChanged(object sender, ListChangedEventArgs e) => SetDataSourceAndSort();

    /// <summary>
    ///   Determines whether the list item at the specified index is the start of a new group. In all
    ///   cases, populates the string representation of the group that the item belongs to.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="currentGroupText">The group text.</param>
    /// <returns></returns>
    private bool DetermineGroupStart(int index, string currentGroupText)
    {
      if (index < 0 || index >= Items.Count)
        return false;
      if (index == 0 && !string.IsNullOrEmpty(currentGroupText))
        return true;
      if (currentGroupText != null && (index > 0 &&
                                       !currentGroupText.Equals(GetGroupText(Items[index - 1]), StringComparison.CurrentCultureIgnoreCase)))
      {
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

    private string GetText(object item)
    {
      if (m_DisplayPropertyDescriptor != null && item != null)
        // get the group value using the property descriptor
        return Convert.ToString(m_DisplayPropertyDescriptor.GetValue(item));
      return string.Empty;
    }

    private void GroupingComboBox_DisplayMemberChanged(object sender, EventArgs e) => SetPropertyDescriptor();

    /// <summary>
    ///   Set the right control style
    /// </summary>
    private void SetComboBoxStyle()
    {
      SetStyle(ControlStyles.UserPaint, DropDownStyle == ComboBoxStyle.DropDownList);
      SetStyle(ControlStyles.AllPaintingInWmPaint, DropDownStyle == ComboBoxStyle.DropDownList);

      if (IsHandleCreated)
        RecreateHandle();
    }

    /// <summary>
    ///   Sorts the data source and sets it
    /// </summary>
    private void SetDataSourceAndSort()
    {
      // Only do the sorting if the base class does not do this
      if (!Sorted && m_GroupPropertyDescriptor != null && m_DisplayPropertyDescriptor != null && m_BindingSource != null)
      {
        var comparer = new TwoLevelComparer(m_GroupPropertyDescriptor, m_DisplayPropertyDescriptor);

        if (comparer == null)
          throw new ConfigurationException("DisplayMember property not found");
        // rebuild the collection and sort using custom logic
        var arrayList = new ArrayList();
        foreach (var item in m_BindingSource)
        {
          if (Text.Length < 2 || comparer.SecondLevel.GetValue(item).ToString().Contains(Text, StringComparison.InvariantCultureIgnoreCase))
            arrayList.Add(item);
        }
        arrayList.Sort(comparer);
        if (base.DataSource == null || ((BindingSource)base.DataSource).Count != arrayList.Count || (arrayList.Count > 0 && ((BindingSource)base.DataSource).List[0] != arrayList[0]))
        {
          IsFiltering = true;
          base.DataSource = new BindingSource(arrayList, string.Empty);
          IsFiltering = false;
        }
      }
      else
      {
        base.DataSource = m_BindingSource;
      }
    }

    /// <summary>
    ///   Sets the group property descriptor.
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
        }
        if (DisplayMember.Equals(descriptor.Name))
        {
          m_DisplayPropertyDescriptor = descriptor;
        }
      }
    }
  }

  /// <summary>
  ///   A Comparer for a group or hierarchy structure
  /// </summary>
  internal class TwoLevelComparer : IComparer
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="TwoLevelComparer" /> class.
    /// </summary>
    /// <param name="firstLevel">The first level.</param>
    /// <param name="secondLevel">The second level.</param>
    public TwoLevelComparer(PropertyDescriptor firstLevel, PropertyDescriptor secondLevel)
    {
      FirstLevel = firstLevel ?? throw new ArgumentNullException("firstLevel");
      SecondLevel = secondLevel ?? throw new ArgumentNullException("secondLevel");
    }

    public PropertyDescriptor FirstLevel { get; }

    public PropertyDescriptor SecondLevel { get; }

    /// <summary>
    ///   Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>
    ///   A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in
    ///   the following table.Value Meaning Less than zero <paramref name="x" /> is less than <paramref name="y" />. Zero
    ///   <paramref name="x" /> equals <paramref name="y" />. Greater than zero <paramref name="x" /> is greater than
    ///   <paramref name="y" />.
    /// </returns>
    public int Compare(object x, object y)
    {
      if (y == null)
        throw new ArgumentNullException(nameof(y));
      if (x == null)
        throw new ArgumentNullException(nameof(x));
      var res = StringComparer.CurrentCultureIgnoreCase.Compare(Convert.ToString(FirstLevel.GetValue(x)),
        Convert.ToString(FirstLevel.GetValue(y)));
      if (res != 0)
        return res;
      return StringComparer.CurrentCultureIgnoreCase.Compare(Convert.ToString(SecondLevel.GetValue(x)),
        Convert.ToString(SecondLevel.GetValue(y)));
    }
  }
}