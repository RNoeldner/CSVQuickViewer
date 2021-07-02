using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  ///   Wrapper around another FileReader adding artificial fields, removing ignored columns, and having paging on the usually forward only IFileReader
  ///   Returned is a list of DynamicDataRecords for the current page
  /// </summary>
  public class PagedFileReader : List<DynamicDataRecord>, INotifyCollectionChanged, IDisposable
  {
    private readonly IFileReader m_FileReader;
    private readonly List<ICollection<DynamicDataRecord>> m_PagedDataCache;
    private readonly int m_PageSize;
    private readonly CancellationToken m_Token;
    private DataReaderWrapper? m_DataReaderWrapper;
    private bool m_DisposedValue;

    public PagedFileReader(IFileReader fileReader, int pageSize, CancellationToken token)
    {
      m_FileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
      m_Token = token;
      m_PageSize = pageSize;
      m_PagedDataCache = new List<ICollection<DynamicDataRecord>>();
    }

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    /// First page has number 1
    /// </summary>
    public int PageIndex { get; set; }

    public void Close()
    {
      m_PagedDataCache.Clear();
      m_DataReaderWrapper?.Close();
    }

    public void Dispose() => Dispose(true);

    public async Task MoveToFirstPageAsync() => await MoveToPageAsync(1);

    public async Task MoveToLastPageAsync() => await MoveToPageAsync(int.MaxValue);

    public async Task MoveToNextPageAsync() => await MoveToPageAsync(PageIndex + 1);

    public async Task MoveToPageAsync(int pageIndex)
    {
      if (pageIndex<1)
        pageIndex=1;
      if (m_DataReaderWrapper == null)
        return;

      var curPage = pageIndex;
      if (m_PagedDataCache.Count < curPage)
      {
        while (!m_Token.IsCancellationRequested
              && m_DataReaderWrapper.RecordNumber < pageIndex * m_PageSize
              && await m_DataReaderWrapper.ReadAsync(m_Token))
        {
          curPage =  (int) ((m_DataReaderWrapper.RecordNumber-1) / m_PageSize) + 1;

          // create the page if it does not exist
          if (m_PagedDataCache.Count<curPage)
            m_PagedDataCache.Add(new List<DynamicDataRecord>(m_PageSize));

          if (m_PagedDataCache[curPage-1].Count != m_PageSize)
            m_PagedDataCache[curPage-1].Add(new DynamicDataRecord(m_DataReaderWrapper));
        }
      }

      PageIndex = curPage;
      base.Clear();
      foreach (var item in m_PagedDataCache[curPage-1])
        base.Add(item);
      CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public async Task MoveToPreviousPageAsync() => await MoveToPageAsync(PageIndex - 1);

    /// <summary>
    /// Opens the file reader and reads the first page
    /// </summary>
    /// <param name="addErrorField">Add artificial field Error</param>
    /// <param name="addStartLine">Add artificial field Start Line</param>
    /// <param name="addEndLine">Add artificial field End Line</param>
    /// <param name="addRecNum">Add artificial field Records Number</param>
    public async Task OpenAsync(bool addErrorField = false, bool addStartLine = false, bool addEndLine = false, bool addRecNum = false)
    {
      await m_FileReader.OpenAsync(m_Token);
      m_DataReaderWrapper = new DataReaderWrapper(m_FileReader, 0, addErrorField, addStartLine, addEndLine, addRecNum);
      await MoveToPageAsync(1);
    }

    private void Dispose(bool disposing)
    {
      if (m_DisposedValue) return;
      if (disposing)
      {
        m_DisposedValue = true;
        m_DataReaderWrapper?.Dispose();
      }
    }
  }
}