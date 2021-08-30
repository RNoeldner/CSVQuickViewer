using System;

namespace CsvTools
{
  public class RetryEventArgs : EventArgs
	{
		public RetryEventArgs(Exception ex) => Exception = ex;

		public Exception Exception { get; }

		public bool Retry { get; set; }
	}
}