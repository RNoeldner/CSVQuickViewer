using System;
using System.IO;
using System.Text;
using System.Threading;

namespace CsvTools
{
  public partial class FindSkipRows : ResizeForm
  {
    private readonly ICsvFile fileSetting;
    private ISyntaxHighlighter m_HighLighter;
    private readonly IImprovedStream m_Stream;

    public FindSkipRows() : this(new CsvFile())
    {
    }

    public FindSkipRows(ICsvFile csvFile)
    {
      InitializeComponent();
      fileSetting = csvFile;
      fileSettingBindingSource.DataSource = csvFile;
      fileFormatBindingSource.DataSource = csvFile.FileFormat;
      DifferentSyntaxHighlighter(this, null);

      m_Stream = new ImprovedStream(new SourceAccess(csvFile));
      textBox.OpenBindingStream(m_Stream as Stream, Encoding.GetEncoding(fileSetting.CodePageId, new EncoderReplacementFallback("?"), new DecoderReplacementFallback("?")));
      HighlightVisibleRange(csvFile.SkipRows);
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
          m_HighLighter?.Highlight(range);

          if (skipRows <= 0) return;
          range = new FastColoredTextBoxNS.Range(textBox, 0, 0, 0, skipRows);
          m_HighLighter?.Comment(range);
        }
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "HighlightVisibleRange");
      }
    }

    private void buttonSkipLine_ClickAsync(object sender, EventArgs e)
    {
      using (var frm = new FormProcessDisplay("Check", true, CancellationToken.None))
      {
        frm.Show();
        frm.Maximum = 0;
        using (var streamReader = new ImprovedTextReader(m_Stream, fileSetting.CodePageId))
        {
          streamReader.ToBeginning();
          fileSetting.SkipRows = CsvHelper.GuessStartRow(streamReader, textBoxDelimiter.Text.WrittenPunctuationToChar(), m_TextBoxQuote.Text.WrittenPunctuationToChar(), textBoxComment.Text, frm.CancellationToken);
        }
        HighlightVisibleRange(fileSetting.SkipRows);
      }
    }

    private void DifferentSyntaxHighlighter(object sender, EventArgs e)
    {
      m_HighLighter = new SyntaxHighlighterDelimitedText(textBox, m_TextBoxQuote.Text.WrittenPunctuationToChar(), textBoxDelimiter.Text.WrittenPunctuationToChar(), fileSetting.FileFormat.EscapeCharacterChar, textBoxComment.Text);
      HighlightVisibleRange(fileSetting.SkipRows);
    }

    private void numericUpDownSkipRows_ValueChanged(object sender, EventArgs e)
    {
      HighlightVisibleRange(Convert.ToInt32(numericUpDownSkipRows.Value));
    }
  }
}