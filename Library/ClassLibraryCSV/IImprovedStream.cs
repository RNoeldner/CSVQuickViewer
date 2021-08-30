using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  public interface IImprovedStream : IDisposable
  {
    bool CanRead { get; }

    bool CanSeek { get; }

    bool CanWrite { get; }

    long Length { get; }

    double Percentage { get; }

    long Position { get; }

    int Read(byte[] buffer, int offset, int count);

    Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);

    long Seek(long offset, SeekOrigin origin);

    void Write(byte[] buffer, int offset, int count);

    Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
  }
}