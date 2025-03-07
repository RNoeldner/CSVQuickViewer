using System;

namespace CsvTools
{
  class DummyProgress : IProgress<ProgressInfo>
  {
    public void Report(ProgressInfo value)
    {
      // Debugger.Break();
    }
  }
}
