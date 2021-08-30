using System;

namespace CsvTools
{
  public class EncryptionException : ApplicationException
	{
		public EncryptionException(string message)
			: base(message)
		{
		}

		public EncryptionException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}