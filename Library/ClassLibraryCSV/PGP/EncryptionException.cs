#if SupportPGP
using System;

namespace CsvTools
{
  /// <inheritdoc />
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
#endif