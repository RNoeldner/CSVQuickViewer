#nullable enable
using System;
using System.Text;
using System.Threading;

namespace CsvTools
{
  public partial class FindSkipRows : ResizeForm
  {
    private readonly ICsvFile m_CsvFile;
    private SyntaxHighlighterDelimitedText m_HighLighter;


    public FindSkipRows() : this(new CsvFile(id: "csv", fileName: "Dummy.csv"))
    {
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public FindSkipRows(ICsvFile csvFile)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
      InitializeComponent();
      m_CsvFile = csvFile ?? throw new ArgumentNullException(nameof(csvFile));
      bindingSourceCsvFile.DataSource = csvFile;
      var sa = new SourceAccess(csvFile);
      if (sa.FileType != FileTypeEnum.Plain)
        throw new NotSupportedException("Any file that is not a plain text is not supported.");

      DifferentSyntaxHighlighter(this, EventArgs.Empty);
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
      using var formProgress = new FormProgress("Check", true, FontConfig, CancellationToken.None);
      formProgress.Show(this);
      formProgress.Maximum = 0;
      using var stream = new ImprovedStream(new SourceAccess(m_CsvFile));
      using var streamReader = new ImprovedTextReader(stream, m_CsvFile.CodePageId);
      m_CsvFile.SkipRows = streamReader.InspectStartRow(textBoxDelimiter.Character, m_TextBoxQuote.Character,
        textBoxEscape.Character, textBoxComment.Text,
        formProgress.CancellationToken);

      HighlightVisibleRange(m_CsvFile.SkipRows);
    }

    private void DifferentSyntaxHighlighter(object? sender, EventArgs e)
    {
      m_HighLighter.Dispose();
      m_HighLighter = new SyntaxHighlighterDelimitedText(textBox, m_TextBoxQuote.Character, textBoxDelimiter.Character,
            textBoxEscape.Character, textBoxComment.Text);
      HighlightVisibleRange(m_CsvFile.SkipRows);
    }

    private void NumericUpDownSkipRows_ValueChanged(object? sender, EventArgs e)
    {
      HighlightVisibleRange(Convert.ToInt32(numericUpDownSkipRows.Value));
    }

    private void FindSkipRows_Load(object? sender, EventArgs e)
    {
      textBox.OpenFile(m_CsvFile.FullPath,
        Encoding.GetEncoding(m_CsvFile.CodePageId, new EncoderReplacementFallback("?"),
          new DecoderReplacementFallback("?")));
    }

    private void TextBox_VisibleRangeChangedDelayed(object? sender, EventArgs e)
    {
      HighlightVisibleRange(m_CsvFile.SkipRows);
    }
  }
}