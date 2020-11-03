using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  ///   This class will make sure we go back to the beginning when starting in the middle of a stream and we reach the end.
  /// </summary>
  public class ImprovedTextReaderPositionStore
  {
    private readonly ImprovedTextReader m_ImprovedTextReader;

    // Keep in mind where we started, this could be half way through the files
    private readonly long m_LineStarted;

    // not using EndOfStream Property to make sure we do not loop more than once
    private bool m_ArrivedAtEndOnce;

    /// <summary>
    ///   This class will make sure we go back to the beginning when starting in the middle of a stream and we reach the end.
    /// </summary>
    public ImprovedTextReaderPositionStore(ImprovedTextReader improvedTextReader)
    {
      m_ImprovedTextReader = improvedTextReader;
      m_ArrivedAtEndOnce = false;
      m_LineStarted = improvedTextReader.LineNumber;
    }

    /// <summary>
    ///   True if we have read all data in the reader once
    /// </summary>
    public bool AllRead() => (m_ImprovedTextReader.EndOfStream && !CanStartFromBeginning()) ||
                             (m_ArrivedAtEndOnce && m_ImprovedTextReader.LineNumber > m_LineStarted);

    /// <summary>
    ///   Determines if we could reset the position to allow processing text that had been read before
    /// </summary>
    public bool CanStartFromBeginning()
    {
      if (m_ArrivedAtEndOnce || m_LineStarted <= 1) return false;
      m_ArrivedAtEndOnce = true;
      m_ImprovedTextReader.ToBeginning();
      return true;
    }
  }
}