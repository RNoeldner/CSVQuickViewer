namespace CsvTools
{
  /// <summary>
  ///   This class will make sure we go back to the beginning when starting in the middle of a
  ///   stream and we reach the end.
  /// </summary>
  public class ImprovedTextReaderPositionStore
  {
    private readonly ImprovedTextReader m_ImprovedTextReader;

    // Keep in mind where we started, this could be half way through the files
    private readonly long m_LineStarted;

    // not using EndOfStream Property to make sure we do not loop more than once
    private bool m_ArrivedAtEndOnce;

    /// <summary>
    ///   This class will make sure we go back to the beginning when starting in the middle of a
    ///   stream and we reach the end.
    /// </summary>
    public ImprovedTextReaderPositionStore(in ImprovedTextReader improvedTextReader)
    {
      m_ImprovedTextReader = improvedTextReader;
      m_ArrivedAtEndOnce = false;
      m_LineStarted = improvedTextReader.LineNumber;
    }

    /// <summary>
    ///   True if we have read all data in the reader once
    /// </summary>
    public bool AllRead() =>
      m_ImprovedTextReader.EndOfStream && !CouldStartFromBeginning()
      || m_ArrivedAtEndOnce && m_ImprovedTextReader.LineNumber > m_LineStarted;

    /// <summary>
    ///   Determines if we could reset the position to allow processing text that had been read before
    ///   If its supported it will do so.
    /// </summary>
    public bool CouldStartFromBeginning()
    {
      if (m_ArrivedAtEndOnce || m_LineStarted <= 1)
        return false;

      m_ArrivedAtEndOnce = true;
      if (!m_ImprovedTextReader.CanSeek)
        return false;

      // Jump to start of the file
      m_ImprovedTextReader.ToBeginning();
      return true;
    }
  }
}