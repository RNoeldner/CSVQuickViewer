using System;

namespace CsvTools
{
  public sealed class EncryptionException : ApplicationException
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