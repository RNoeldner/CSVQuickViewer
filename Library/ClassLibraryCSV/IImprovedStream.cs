using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  public interface IImprovedStream : IDisposable
  {
    double Percentage { get; }

#region From Stream

    long Seek(long offset, SeekOrigin origin);

    int Read(byte[] buffer, int offset, int count);

    Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);

    void Write(byte[] buffer, int offset, int count);

    Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);

    bool CanRead { get; }
    bool CanSeek { get; }

    bool CanWrite { get; }

    long Length { get; }

    long Position { get; }

#endregion
  }
}