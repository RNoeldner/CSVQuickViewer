using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <inheritdoc cref="List{T}" />
  /// <summary>
  ///   Wrapper around another FileReader adding artificial fields, removing ignored columns, and
  ///   having paging on the usually forward only IFileReader Returned is a list of
  ///   DynamicDataRecords for the current page
  /// </summary>
  public class PagedFileReader : List<DynamicDataRecord>, INotifyCollectionChanged
  {
    private readonly IFileReader m_FileReader;
    private readonly List<ICollection<DynamicDataRecord>> m_PagedDataCache;
    private readonly int m_PageSize;
    private DataReaderWrapper? m_DataReaderWrapper;

    public PagedFileReader(in IFileReader fileReader, int pageSize)
    {
      m_FileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
      m_PageSize = pageSize;
      m_PagedDataCache = new List<ICollection<DynamicDataRecord>>();
    }

    /// <summary>
    /// Occurs when the collection changes.
    /// </summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    ///   First page has number 1
    /// </summary>
    public int PageIndex { get; private set; }

    /// <summary>
    /// Closes this instance.
    /// </summary>
    public void Close()
    {
      m_PagedDataCache.Clear();
      m_DataReaderWrapper?.Close();
    }

    /// <summary>
    /// Moves to first page asynchronous.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    public async Task MoveToFirstPageAsync(CancellationToken cancellationToken) => await MoveToPageAsync(1, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Moves to last page asynchronous.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    public async Task MoveToLastPageAsync(CancellationToken cancellationToken) => await MoveToPageAsync(int.MaxValue, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Moves to next page asynchronous.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    public async Task MoveToNextPageAsync(CancellationToken cancellationToken) => await MoveToPageAsync(PageIndex + 1, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Moves to previous page asynchronous.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    public async Task MoveToPreviousPageAsync(CancellationToken cancellationToken) => await MoveToPageAsync(PageIndex - 1, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Opens the file reader and reads the first page
    /// </summary>
    /// <param name="addErrorField">Add artificial field Error</param>
    /// <param name="addStartLine">Add artificial field Start Line</param>
    /// <param name="addEndLine">Add artificial field End Line</param>
    /// <param name="addRecNum">Add artificial field Records Number</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    public async Task OpenAsync(
      bool addErrorField, bool addStartLine,
      bool addEndLine, bool addRecNum, 
      CancellationToken cancellationToken)
    {
      await m_FileReader.OpenAsync(cancellationToken).ConfigureAwait(false);
      m_DataReaderWrapper = new DataReaderWrapper(m_FileReader, addErrorField, addStartLine, addEndLine, addRecNum);
      await MoveToPageAsync(1, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Moves to page asynchronous.
    /// </summary>
    /// <param name="pageIndex">Index of the page.</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <exception cref="CsvTools.FileReaderOpenException"></exception>
    private async Task MoveToPageAsync(int pageIndex, CancellationToken cancellationToken )
    {
      if (m_DataReaderWrapper is null)
        throw new FileReaderOpenException();
      if (pageIndex < 1)
        pageIndex = 1;

      var curPage = 1;
      if (m_PagedDataCache.Count < pageIndex)
        while (!cancellationToken.IsCancellationRequested && m_DataReaderWrapper.RecordNumber < (long) pageIndex * m_PageSize
                                                && await m_DataReaderWrapper.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
          curPage = (int) ((m_DataReaderWrapper.RecordNumber - 1) / m_PageSize) + 1;

          // create the page if it does not exist
          if (m_PagedDataCache.Count < curPage)
            m_PagedDataCache.Add(new List<DynamicDataRecord>(m_PageSize));

          if (m_PagedDataCache[curPage - 1].Count != m_PageSize)
            m_PagedDataCache[curPage - 1].Add(new DynamicDataRecord(m_DataReaderWrapper));
        }

      Clear();

      PageIndex = curPage;
      foreach (var item in m_PagedDataCache[curPage - 1])
        Add(item);
      CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
  }
}