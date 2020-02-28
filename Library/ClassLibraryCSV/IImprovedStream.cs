using System;
using System.IO;

namespace CsvTools
{
  public interface IImprovedStream : IDisposable
  {
    double Percentage { get; }

    Stream Stream { get; }

    void ResetToStart(Action<Stream> afterInit);

    void Close();
  }
}