using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  public static class PgpHelper
  {
    private static readonly Dictionary<string, SecureString> m_KnownKeys = new Dictionary<string, SecureString>();
    private static readonly Dictionary<string, string> m_KnownKeyFiles = new Dictionary<string, string>();
    public static string LookupKeyFile(string fileName) =>
      m_KnownKeyFiles.TryGetValue(fileName, out var value) ? value : string.Empty;
    public static void StoreKeyFile(string fileName, string keyFile)
    {
      if (!FileSystemUtils.FileExists(keyFile))
        return;
      m_KnownKeyFiles[fileName] = keyFile;
    }

    // This is not ideal as SecureString is Disposable...
    // need to call ClearPassphrase when application exists
    private static readonly Dictionary<string, SecureString> m_KnownPassphrase = new Dictionary<string, SecureString>();

    public static string LookupKey(string fileName) =>
      m_KnownKeys.TryGetValue(fileName, out var value) ? value.GetText() : string.Empty;

    public static void StoreKey(string fileName, string keyValue)
    {
      if (string.IsNullOrEmpty(keyValue))
        return;
      if (m_KnownKeys.ContainsKey(fileName))
      {
        m_KnownKeys[fileName].Dispose();
        m_KnownKeys[fileName] = keyValue.ToSecureString();
      }
      else
        m_KnownKeys.Add(fileName, keyValue.ToSecureString());
    }
    
    public static string LookupPassphrase(string fileName) =>
      m_KnownPassphrase.TryGetValue(fileName, out var value) ? value.GetText() : string.Empty;

    public static void StorePassphrase(string fileName, string passphrase)
    {
      if (m_KnownPassphrase.ContainsKey(fileName))
      {
        m_KnownPassphrase[fileName].Dispose();
        m_KnownPassphrase[fileName] = passphrase.ToSecureString();
      }
      else
        m_KnownPassphrase.Add(fileName, passphrase.ToSecureString());
    }

    public static void ClearPgpInfo()
    {
      foreach (var secureString in m_KnownPassphrase)
        secureString.Value.Dispose();
      m_KnownPassphrase.Clear();

      foreach (var secureString in m_KnownKeys)
        secureString.Value.Dispose();
      m_KnownKeys.Clear();
    }

    /// <summary>
    ///   Decrypt a file and create a not encrypted file
    /// </summary>
    /// <param name="pgpSec"></param>
    /// <param name="passPhrase">Passphrase of the private key used to decrypt the file</param>
    /// <param name="encryptedFileName">Path to file being read</param>
    /// <param name="unencryptedFileName">Path to file being created</param>
    /// <param name="progress">Display of progress</param>
    /// <param name="cancellationToken">CancellationToken if this needs to be stopped</param>
    public static async Task DecryptFileAsync(this PgpSecretKeyRingBundle pgpSec, string passPhrase,
      string encryptedFileName, string unencryptedFileName,
      IProgress<ProgressInfo>? progress,
      CancellationToken cancellationToken)
    {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      using var encryptedStream = FileSystemUtils.OpenRead(encryptedFileName);
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      using var outStream = FileSystemUtils.OpenWrite(unencryptedFileName);

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      using var closeFirst = GetReadStream(pgpSec, passPhrase, encryptedStream, out var closeSecond, out var closeThird);


      await CopyStreamsAsync(closeFirst, outStream, "PGP Decrypt", progress, cancellationToken).ConfigureAwait(false);

      closeFirst.Close();
      closeSecond.Close();
      closeThird.Close();

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await closeSecond.DisposeAsync();
#else
      closeSecond.Dispose();
#endif

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await closeThird.DisposeAsync();
#else
      closeThird.Dispose();
#endif
    }

    /// <summary>
    /// Returns teh key information from a file
    /// </summary>
    /// <param name="fileName">File that should be read or written, this is used to determine if its a pgp file</param>
    /// <param name="keyFile">The file with teh key contend</param>
    /// <returns>The contend of teh file if valid, otherwise ""</returns>
    public static string GetKeyAndValidate(in string fileName, in string keyFile)
    {

      if (fileName.AssumePgp() && FileSystemUtils.FileExists(keyFile))
      {
        var contend = FileSystemUtils.ReadAllText(keyFile);
        try
        {
          if (ParsePrivateKey(contend).Count > 0)
            return contend;
        }
        catch
        {
          // ignored
        }

        try
        {
          ParsePublicKey(contend);
          return contend;
        }
        catch
        {
          // ignored
        }
      }
      return string.Empty;
    }

    /// <summary>
    ///   PGP encrypt a not encrypted file
    /// </summary>
    /// <param name="publicKey">Public PGP Key</param>
    /// <param name="unencryptedFileName">Path to file being read</param>
    /// <param name="encryptedFileName">Path to file being created</param>
    /// <param name="progress">Display of progress</param>
    /// <param name="cancellationToken">CancellationToken if this needs to be stopped</param>
    /// <returns></returns>
    public static async Task EncryptFileAsync(this PgpPublicKey publicKey,
      string unencryptedFileName, string encryptedFileName,
      IProgress<ProgressInfo>? progress,
      CancellationToken cancellationToken)
    {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      using var toEncrypt = FileSystemUtils.OpenRead(unencryptedFileName);
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      using var outStream = FileSystemUtils.OpenWrite(encryptedFileName);
      await EncryptStreamAsync(publicKey, toEncrypt, outStream, progress, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///   Write data to the given output steam as encrypted data
    /// </summary>
    /// <param name="publicKey">Public PGP Key</param>
    /// <param name="toEncrypt">Input stream to read from</param>
    /// <param name="outStream">Output stream to write to</param>
    /// <param name="progress">Display of progress</param>
    /// <param name="cancellationToken">CancellationToken if this needs to be stopped</param>
    // ReSharper disable once MemberCanBePrivate.Global used in Unit Testing
    public static async Task EncryptStreamAsync(this PgpPublicKey publicKey,
      Stream toEncrypt, Stream outStream,
      IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
    {
      // 1. Encryption
      var encryption = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.TripleDes, true, new SecureRandom());
      // Add recipient keys
      encryption.AddMethod(publicKey);

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      using var encryptedStream = encryption.Open(outStream, new byte[32768]);

      // 2. Compression
      var compressor = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip);
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
        using var compressedStream = compressor.Open(encryptedStream);
      var name = "PGPStream";
      var dtm = DateTime.UtcNow;
      if (outStream is FileStream fs)
      {
        var fi = new FileSystemUtils.FileInfo(fs.Name);
        if (fi.Exists)
        {
          name = FileSystemUtils.SplitPath(fs.Name).FileName;
          dtm = fi.LastWriteTimeUtc;
        }
      }

      // 3. Literal Data
      var literal = new PgpLiteralDataGenerator();
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      using var literalStream = literal.Open(compressedStream, PgpLiteralDataGenerator.Binary, name, dtm, new byte[32768]);

      // Copy the data
      await CopyStreamsAsync(toEncrypt, literalStream, "PGP Encrypting", progress, cancellationToken)
        .ConfigureAwait(false);

      // Close in right order
      literalStream.Close();
      compressedStream.Close();
      encryptedStream.Close();
    }

    /// <summary>
    ///   Decrypts the encrypted stream
    /// </summary>
    /// <param name="pgpSec">The secret KeyRing Bundle, as read from private key file</param>
    /// <param name="passPhrase">The passphrase.</param>
    /// <param name="inputStream">Stream to decrypt.</param>
    /// <param name="closeSecond">Stream to close second when finished reading</param>
    /// <param name="closeThird">Stream to close third when finished reading</param>
    /// <returns></returns>
    /// <exception cref="PgpException">
    ///   Encrypted message contains a signed message - not literal data. or Message is not a simple
    ///   encrypted file - type unknown.
    /// </exception>
    public static Stream GetReadStream(this PgpSecretKeyRingBundle pgpSec, in string passPhrase, Stream inputStream,
      out Stream closeSecond, out Stream closeThird)
    {
      // Never use using here this would close the decoder stream and we would not be able to read
      // later on from the input stream
      var pgpObjectFactory = new PgpObjectFactory(PgpUtilities.GetDecoderStream(inputStream));

      // get the encrypted data list it can be the first or next object
      var pgpObject = pgpObjectFactory.NextPgpObject();

      if (!(pgpObject is PgpEncryptedDataList encryptedDataList))
        encryptedDataList = (PgpEncryptedDataList) pgpObjectFactory.NextPgpObject();

      foreach (var pgpEncryptedData in encryptedDataList.GetEncryptedDataObjects())
      {
        var encryptedData = (PgpPublicKeyEncryptedData) pgpEncryptedData;
        var pgpSecKey = pgpSec.GetSecretKey(encryptedData.KeyId);
        if (pgpSecKey is null)
          continue;
        try
        {
          var key = pgpSecKey.ExtractPrivateKey(passPhrase.ToCharArray());
          if (key is null) continue;
          closeThird = encryptedData.GetDataStream(key);
          pgpObjectFactory = new PgpObjectFactory(closeThird);
          // first Object
          pgpObject = pgpObjectFactory.NextPgpObject();

          // if its compressed get the PgpLiteralData
          if (pgpObject is PgpCompressedData compressedData)
          {
            closeSecond = compressedData.GetDataStream();
            pgpObjectFactory = new PgpObjectFactory(closeSecond);
            pgpObject = pgpObjectFactory.NextPgpObject();
            if (pgpObject is PgpOnePassSignatureList)
              pgpObject = pgpObjectFactory.NextPgpObject();

            if (pgpObject is PgpLiteralData literalData)
              return literalData.GetInputStream();
          }
        }
        catch (PgpException)
        {
          //ignore
        }
      }

      throw new PgpException("Could not decrypt input stream");
    }

    /// <summary>
    ///   This will build the streams used to write unencrypted data so it ends up encrypted in the
    ///   target stream
    /// </summary>
    /// <param name="publicKey">Public PGP Key</param>
    /// <param name="outStream">The underlying base stream to write to (usually a file)</param>
    /// <param name="closeSecond">
    ///   Compression stream, this needs to be closed after the literal stream
    /// </param>
    /// <param name="closeThird">Encryption stream, this needs to be closed last</param>
    /// <returns>
    /// the Literal stream to write to this has to be closed first
    /// </returns>
    public static Stream GetWriteStream(this PgpPublicKey publicKey, Stream outStream, out Stream closeSecond,
      out Stream closeThird)
    {
      // 1. Encryption
      var encryption = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.TripleDes, true, new SecureRandom());
      // Add recipient keys
      encryption.AddMethod(publicKey);
      closeThird = encryption.Open(outStream, new byte[32768]);

      // 2. Compression
      var compressor = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip);
      closeSecond = compressor.Open(closeThird);
      var name = "PGPStream";
      var dtm = DateTime.UtcNow;
      if (outStream is FileStream fs)
      {
        var fi = new FileSystemUtils.FileInfo(fs.Name);
        if (fi.Exists)
        {
          name = FileSystemUtils.SplitPath(fs.Name).FileName;
          dtm = fi.LastWriteTimeUtc;
        }
      }

      // 3. Literal Data
      var literal = new PgpLiteralDataGenerator();
      return literal.Open(closeSecond, PgpLiteralDataGenerator.Binary, name, dtm, new byte[32768]);
    }
    /// <summary>
    /// Parse the ASCII representation of a private key
    /// </summary>
    /// <param name="privateKey">The key as ASCII</param>
    /// <returns></returns>
    public static PgpSecretKeyRingBundle ParsePrivateKey(in string privateKey)
    {
      using var keyIn = new MemoryStream(Encoding.UTF8.GetBytes(privateKey));
      using var decoder = PgpUtilities.GetDecoderStream(keyIn);
      return new PgpSecretKeyRingBundle(decoder);
    }

    /// <summary>
    /// Parse the ASCII representation of a public key
    /// </summary>
    /// <param name="publicKey">The key as ASCII</param>
    /// <returns></returns>
    public static PgpPublicKey ParsePublicKey(in string publicKey)
    {
      using var keyIn = new MemoryStream(Encoding.UTF8.GetBytes(publicKey));
      using var decoder = PgpUtilities.GetDecoderStream(keyIn);
      var testPublic = new PgpPublicKeyRingBundle(PgpUtilities.GetDecoderStream(keyIn));
      foreach (PgpPublicKeyRing kRing in testPublic.GetKeyRings())
        foreach (PgpPublicKey key in kRing.GetPublicKeys())
        {
          // no not return signature keys, keys that are revoked or expired
          if (!key.IsEncryptionKey || key.IsRevoked() || !key.GetUserIds().GetEnumerator().MoveNext() ||
              (key.GetValidSeconds() > 0 && key.CreationTime.AddSeconds(key.GetValidSeconds()) < DateTime.Now))
            continue;
          return key;
        }

      throw new PgpException("Public Key is invalid");
    }

    /// <summary>
    ///   Copy the contend of one stream into another stream and show the progress
    /// </summary>
    /// <param name="source">Stream to read from</param>
    /// <param name="dest">Stream to write to</param>
    /// <param name="info">Information shown in the progress</param>
    /// <param name="progress">Display of progress</param>
    /// <param name="cancellationToken">CancellationToken if this needs to be stopped</param>
    /// <returns></returns>
    private static async Task CopyStreamsAsync(Stream source, Stream dest, string info,
      IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
    {
      long length = 0;
      try
      {
        length = source.Length;
      }
      catch (Exception)
      {
        // ignore some streams do not support length
      }

      progress.SetMaximum(length);

      long processed = 0;
      int lengthRead;
      var displayMax = length > 0 ? "/" + StringConversion.DynamicStorageSize(length) : string.Empty;

      var intervalAction = IntervalAction.ForProgress(progress);
      var copyBuffer = new byte[32768];
      while ((lengthRead =
               await source.ReadAsync(copyBuffer, 0, copyBuffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
      {
        cancellationToken.ThrowIfCancellationRequested();

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        await dest.WriteAsync(copyBuffer.AsMemory(0, lengthRead), cancellationToken).ConfigureAwait(false);
#else
        await dest.WriteAsync(copyBuffer, 0, lengthRead, cancellationToken).ConfigureAwait(false);
#endif
        processed += lengthRead;
        intervalAction?.Invoke(() =>
        {
          if (progress is IProgressTime displayTime)
          {
            // ReSharper disable once AccessToModifiedClosure
            displayTime.TimeToCompletion.Value = processed;
            // ReSharper disable once AccessToModifiedClosure
            displayTime.Report(new ProgressInfo(
              $"{info}  {displayTime.TimeToCompletion.PercentDisplay}{displayTime.TimeToCompletion.EstimatedTimeRemainingDisplaySeparator} - {StringConversion.DynamicStorageSize(processed)}{displayMax}",
              // ReSharper disable once AccessToModifiedClosure
              processed));
          }
          else
          {
            progress?.Report(
              // ReSharper disable once AccessToModifiedClosure
              new ProgressInfo($"{info} - {StringConversion.DynamicStorageSize(processed)}{displayMax}",
                // ReSharper disable once AccessToModifiedClosure
                processed));
          }
        });
      }

      await dest.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
  }
}