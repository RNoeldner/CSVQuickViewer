namespace CsvTools
{
  public class ImprovedTextReaderPositionStore
  {
    private readonly ImprovedTextReader m_ImprovedTextReader;

    // Keep in mind wheer we started, this could be half way through the files
    private readonly long m_LineStarted;

    // not using EndOfStream Property to make sure we do not loop more than once
    private bool m_ArrivedAtEndOnce;

    public ImprovedTextReaderPositionStore(ImprovedTextReader improvedTextReader)
    {
      m_ImprovedTextReader = improvedTextReader;
      m_ArrivedAtEndOnce = false;
      m_LineStarted = improvedTextReader.LineNumber;
    }

    /// <summary>
    ///   True if we have read all data in the reader once
    /// </summary>
    public bool AllRead => this.m_ImprovedTextReader.EndOfFile && !this.CanStartFromBeginning
                        || this.m_ArrivedAtEndOnce && this.m_ImprovedTextReader.LineNumber > this.m_LineStarted;

    /// <summary>
    ///   Determines if we could reset the position to allow processing text taht had been read before
    /// </summary>
    public bool CanStartFromBeginning
    {
      get
      {
        if (!m_ArrivedAtEndOnce && m_LineStarted > 1)
        {
          m_ArrivedAtEndOnce = true;
          m_ImprovedTextReader.ToBeginning();
          return true;
        }
        return false;
      }
    }
  }
}