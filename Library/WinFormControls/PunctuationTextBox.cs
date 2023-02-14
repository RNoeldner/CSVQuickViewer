using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace CsvTools
{

  [DefaultBindingProperty(nameof(Character))]
  public sealed class PunctuationTextBox : TextBox
  {
    public enum PunctuationType
    {
      None,
      Delimiter,
      Date,
      Time,
      Decimal,
      Grouping,
      Qualifier,
      Escape,
      List
    }

    private PunctuationType m_Type;

    [Browsable(true)]
    [Bindable(true)]
    [DefaultValue(PunctuationType.None)]
    public PunctuationType Type
    {
      get => m_Type;
      set
      {
        if (m_Type != value)
        {
          m_Type = value;
          if (m_Type == PunctuationType.Delimiter)
            m_Common=DelimiterCounter.GetPossibleDelimiters().ToArray();
          else if (m_Type == PunctuationType.Qualifier)
            m_Common=DetectionQualifier.PossibleQualifier.ToCharArray();
          else if (m_Type == PunctuationType.Escape)
            m_Common=DetectionEscapePrefix.PossibleEscapePrefix.ToCharArray().ToArray();
          else if (m_Type == PunctuationType.List)
            m_Common=";,\t\r\n".ToCharArray();
          else if (m_Type == PunctuationType.Date)
            m_Common=StringCollections.DateSeparators.Select(x => x.FromText()).ToArray();
          else if (m_Type == PunctuationType.Time)
            m_Common=":".ToCharArray();
          else if (m_Type == PunctuationType.Decimal)
            m_Common=StringCollections.DecimalSeparators.ToArray();
          else if (m_Type == PunctuationType.Grouping)
            m_Common=StringCollections.DecimalGroupings.ToArray();
        }
      }
    }
    private char[] m_Common = new char[] { };
    private char m_Character;

    [Browsable(true)]
    [Bindable(true)]
    [DefaultValue(char.MinValue)]
    public char Character
    {
      get => m_Character;
      set
      {
        if (m_Character != value)
        {
          m_Character = value;
          Text = m_Character.Text();
        }
      }
    }

    public PunctuationTextBox()
    {
      BindingContextChanged +=PunctuationTextBox_BindingContextChanged;
      Validating += PunctuationTextBox_Validating;
    }

    private void PunctuationTextBox_BindingContextChanged(object? sender, System.EventArgs e)
    {
      BindingContextChanged -=PunctuationTextBox_BindingContextChanged;
      SuspendLayout();
      AutoCompleteMode = AutoCompleteMode.SuggestAppend;
      AutoCompleteSource = AutoCompleteSource.CustomSource;
      AutoCompleteCustomSource.Clear();
      foreach (var chr in m_Common)
        AutoCompleteCustomSource.Add(chr.Text());
      ResumeLayout();
      BindingContextChanged +=PunctuationTextBox_BindingContextChanged;
    }

    private void PunctuationTextBox_Validating(object? sender, CancelEventArgs e)
    {
      Character = Text.FromText();
      if (m_Common.Length>0 && !m_Common.Contains(Character) && Character != char.MinValue)
      {
        e.Cancel = true;
        MessageBox.Show($"{Text} is not supported for {m_Type}\n\nChoose one of:\n{m_Common.Select(x => x.Text()).Join(", ")}", $"Invalid {m_Type}", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, 1);
      }
      else
      {
        Text = Character.Text();
      }

    }
  }
}
