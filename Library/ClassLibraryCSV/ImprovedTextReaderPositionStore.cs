/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
 * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License along with this program.
 * If not, see http://www.gnu.org/licenses/ .
 *
 */
namespace CsvTools
{
  /// <summary>
  ///   This class will make sure we go back to the beginning when starting in the middle of a
  ///   stream, and we reach the end.
  /// </summary>
  public sealed class ImprovedTextReaderPositionStore
  {
    private readonly ImprovedTextReader m_ImprovedTextReader;

    // Keep in mind where we started, this could be halfway through the files
    private readonly long m_LineStarted;

    // not using EndOfStream Property to make sure we do not loop more than once
    private bool m_ArrivedAtEndOnce;

    /// <summary>
    ///   This class will make sure we go back to the beginning when starting in the middle of a
    ///   stream, and we reach the end.
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
      (m_ImprovedTextReader.EndOfStream && !CouldStartFromBeginning())
      || (m_ArrivedAtEndOnce && m_ImprovedTextReader.LineNumber > m_LineStarted);

    /// <summary>
    ///   Determines if we could reset the position to allow processing text that had been read before
    ///   If its supported it will do so.
    /// </summary>
    private bool CouldStartFromBeginning()
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
