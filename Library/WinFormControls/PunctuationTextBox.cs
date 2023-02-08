using System.ComponentModel;
using System.Windows.Forms;

namespace CsvTools
{
  [DefaultBindingProperty(nameof(Character))]
  public sealed class PunctuationTextBox : TextBox
  {
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
      Validating += PunctuationTextBox_Validating;
    }

    private void PunctuationTextBox_Validating(object? sender, CancelEventArgs e)
      => Character = Text.FromText();
  }
}
