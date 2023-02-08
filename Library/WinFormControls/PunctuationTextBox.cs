using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace CsvTools
{
  [System.ComponentModel.DefaultBindingProperty("Character")]
  public partial class PunctuationTextBox : UserControl
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
        m_Character = value;
        textBox.Text = m_Character.Text();
      }
    }

    public PunctuationTextBox()
    {
      InitializeComponent();
      TextChanged += (sender, args) => m_Character.SetText(Text);
    }

    private void textBox_TextChanged(object sender, EventArgs e)
    {
      m_Character.SetText(textBox.Text);
    }
  }
}
