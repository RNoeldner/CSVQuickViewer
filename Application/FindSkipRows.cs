using System;
using System.IO;
using System.Text;
using System.Threading;

namespace CsvTools
{
  public partial class FindSkipRows : ResizeForm
  {
    private readonly ICsvFile m_CsvFile;
    private SyntaxHighlighterDelimitedText m_HighLighter;
    private readonly Stream m_Stream;

    public FindSkipRows() : this(new CsvFile())
    {
    }

    public FindSkipRows(ICsvFile csvFile)
    {
      InitializeComponent();
      m_CsvFile = csvFile ?? throw new ArgumentNullException(nameof(csvFile));
      bindingSourceCsvFile.DataSource = csvFile;

      m_Stream = new ImprovedStream(new SourceAccess(csvFile));
      m_HighLighter = new SyntaxHighlighterDelimitedText(textBox, m_TextBoxQuote.Text, textBoxDelimiter.Text, m_CsvFile.EscapePrefixChar.ToStringHandle0(),
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
      using var fromProgress = new FormProgress("Check", true, CancellationToken.None);
      fromProgress.Show();
      fromProgress.Maximum = 0;
      using (var streamReader = new ImprovedTextReader(m_Stream!, m_CsvFile.CodePageId))
      {        
        m_CsvFile.SkipRows = streamReader.GuessStartRow(textBoxDelimiter.Text, m_TextBoxQuote.Text, textBoxComment.Text, fromProgress.CancellationToken);
      }

      HighlightVisibleRange(m_CsvFile.SkipRows);
    }

    private void DifferentSyntaxHighlighter(object? sender, EventArgs e)
    {
      m_HighLighter.Dispose();
      m_HighLighter = new SyntaxHighlighterDelimitedText(textBox, m_TextBoxQuote.Text, textBoxDelimiter.Text, m_CsvFile.EscapePrefix, textBoxComment.Text);
      HighlightVisibleRange(m_CsvFile.SkipRows);
    }

    private void NumericUpDownSkipRows_ValueChanged(object? sender, EventArgs e)
    {
      HighlightVisibleRange(Convert.ToInt32(numericUpDownSkipRows.Value));
    }

    private void FindSkipRows_Load(object? sender, EventArgs e)
    {
      textBox.OpenBindingStream(m_Stream as Stream,
        Encoding.GetEncoding(m_CsvFile.CodePageId, new EncoderReplacementFallback("?"), new DecoderReplacementFallback("?")));
    }

    private void TextBox_VisibleRangeChangedDelayed(object? sender, EventArgs e)
    {
      HighlightVisibleRange(m_CsvFile.SkipRows);
    }
  }
}