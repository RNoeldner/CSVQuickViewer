using System;
using System.IO;
using System.Text;
using System.Threading;

namespace CsvTools
{
  public partial class FindSkipRows : ResizeForm
  {
    private readonly ICsvFile m_FileSetting;
    private ISyntaxHighlighter m_HighLighter;
    private Stream? m_Stream;

    public FindSkipRows() : this(new CsvFile())
    {
    }

    public FindSkipRows(ICsvFile csvFile)
    {
      InitializeComponent();
      m_FileSetting = csvFile;
      fileSettingBindingSource.DataSource = csvFile;

      m_Stream = new ImprovedStream(new SourceAccess(csvFile));
      m_HighLighter = new SyntaxHighlighterDelimitedText(textBox, m_TextBoxQuote.Text, textBoxDelimiter.Text, m_FileSetting.EscapePrefixChar.ToStringHandle0(),
        textBoxComment.Text);
    }

    private void HighlightVisibleRange(int skipRows)
    {
      try
      {
        //expand visible range (+- margin)
        var startLine = Math.Max(skipRows, textBox.VisibleRange.Start.iLine - 20);
        var endLine = Math.Min(textBox.LinesCount - 1, textBox.VisibleRange.End.iLine + 100);
        if (startLine < endLine)
        {
          var range = new FastColoredTextBoxNS.Range(textBox, 0, startLine, 0, endLine);
          m_HighLighter.Highlight(range);
          m_HighLighter.SkipRows(skipRows);
        }
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "HighlightVisibleRange");
      }
    }

    private void ButtonSkipLine_Click(object? sender, EventArgs e)
    {
      using var frm = new FormProcessDisplay("Check", true, CancellationToken.None);
      frm.Show();
      frm.Maximum = 0;
      using (var streamReader = new ImprovedTextReader(m_Stream!, m_FileSetting.CodePageId))
      {        
        m_FileSetting.SkipRows = streamReader.GuessStartRow(textBoxDelimiter.Text, m_TextBoxQuote.Text, textBoxComment.Text, frm.CancellationToken);
      }

      HighlightVisibleRange(m_FileSetting.SkipRows);
    }

    private void DifferentSyntaxHighlighter(object? sender, EventArgs e)
    {
      m_HighLighter = new SyntaxHighlighterDelimitedText(textBox, m_TextBoxQuote.Text, textBoxDelimiter.Text, m_FileSetting.EscapePrefix, textBoxComment.Text);
      HighlightVisibleRange(m_FileSetting.SkipRows);
    }

    private void NumericUpDownSkipRows_ValueChanged(object? sender, EventArgs e)
    {
      HighlightVisibleRange(Convert.ToInt32(numericUpDownSkipRows.Value));
    }

    private void FindSkipRows_Load(object? sender, EventArgs e)
    {
      textBox.OpenBindingStream(m_Stream as Stream,
        Encoding.GetEncoding(m_FileSetting.CodePageId, new EncoderReplacementFallback("?"), new DecoderReplacementFallback("?")));
    }

    private void TextBox_VisibleRangeChangedDelayed(object? sender, EventArgs e)
    {
      HighlightVisibleRange(m_FileSetting.SkipRows);
    }
  }
}