/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
 * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License along with this program.
 * If not, see http://www.gnu.org/licenses/ .
 *
 */

namespace CsvTools
{
  using System;
  using System.Diagnostics;
  using System.IO;
  using System.Security.Cryptography;
  using System.Text;

  /// <summary>
  ///   Class to encrypt and decrypt text, any information that needs to be stored in a secure way should be encrypted
  /// </summary>
  [DebuggerStepThrough]
  public static class SecureString
  {
    private const string c_Base64 = "012345abcdefghijklmnopqrstuvwxyz6789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const int c_SlatSize = 8;
    private const int c_SlatSplit = 3;

    /// <summary>
    ///   A central Random instance that should be decently random, please use for any random number generation
    /// </summary>
    public static Random Random = new Random(Guid.NewGuid().GetHashCode());

    private static readonly byte[] m_InitVectorBytes =
      {
        112, 101, 109, 50, 97, 105, 108, 57, 117, 122, 108, 103, 122, 106, 55, 97
      };

    private static string m_Phrase;

    private static string DefaultPhrase =>
      m_Phrase ?? (m_Phrase = "reCffmj/JWCQmL60+zVmPxBwHEkiZCwC+B1wZsXn4BpjBUg8IJ5".Decrypt("g4yTwMwpRfz4a1hBFkQQ"));

    /// <summary>
    ///   Decrypts the Base64 encoded salted encrypted text using the specified password
    /// </summary>
    /// <param name="cipherText">The Base64 encoded encrypted cipher text.</param>
    /// <param name="pwd">The password</param>
    /// <returns>Plain decrypted text</returns>
    public static string Decrypt(this string cipherText, string pwd = null)
    {
      // any not encypted text does not need decryption
      if (!cipherText.IsEncyrpted())
        return cipherText;

      if (pwd == null)
        pwd = DefaultPhrase;

      var salt = cipherText.Substring(0, c_SlatSplit)
                 + cipherText.Substring(cipherText.Length - (c_SlatSize - c_SlatSplit));
      var base64 = cipherText.Substring(c_SlatSplit, cipherText.Length - c_SlatSize);

      // add the possibly removed padding
      if (base64.Length % 4 != 0)
        base64 += "=";
      if (base64.Length % 4 != 0)
        base64 += "=";
      var cipherTextBytes = Convert.FromBase64String(base64);

      using (var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC })
      {
        using (var passwordDeriveBytes = new PasswordDeriveBytes(pwd, Encoding.ASCII.GetBytes(salt)))
        {
          var decrypt = symmetricKey.CreateDecryptor(passwordDeriveBytes.GetBytes(32), m_InitVectorBytes);
          using (var cryptoStream = new CryptoStream(new MemoryStream(cipherTextBytes), decrypt, CryptoStreamMode.Read))
          {
            var plainTextBytes = new byte[cipherTextBytes.Length];
            var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
          }
        }
      }
    }

    /// <summary>
    ///   Encrypts the text using the specified password resulting in a Base64 encoded encrypted cipher
    /// </summary>
    /// <param name="plainText">The plain text.</param>
    /// <param name="pwd">The password.</param>
    /// <returns>Base64 encoded salted encrypted cipher</returns>
    public static string Encrypt(this string plainText, string pwd = null)
    {
      var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
      if (pwd == null)
        pwd = DefaultPhrase;

      var builder = new char[c_SlatSize];

      // pick random salt characters
      for (var i = 0; i < c_SlatSize; i++)
        builder[i] = c_Base64[Convert.ToInt32(Math.Floor(c_Base64.Length * Random.NextDouble()))];
      var salt = new string(builder);

      using (var symmetricKey = new RijndaelManaged { Mode = CipherMode.CBC })
      {
        using (var passwordDeriveBytes = new PasswordDeriveBytes(pwd, Encoding.ASCII.GetBytes(salt)))
        {
          var encrypt = symmetricKey.CreateEncryptor(passwordDeriveBytes.GetBytes(32), m_InitVectorBytes);
          var memoryStream = new MemoryStream();
          using (var cryptoStream = new CryptoStream(memoryStream, encrypt, CryptoStreamMode.Write))
          {
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            var text = Convert.ToBase64String(memoryStream.ToArray());

            // remove Base64 padding
            if (text.EndsWith("=="))
              text = text.Substring(0, text.Length - 2);
            else if (text.EndsWith("="))
              text = text.Substring(0, text.Length - 1);

            return salt.Substring(0, c_SlatSplit) + text + salt.Substring(c_SlatSplit);
          }
        }
      }
    }

    /// <summary>
    ///   Checks if the text is possibly an encyrpted text.
    /// </summary>
    /// <param name="cipherText"></param>
    /// <returns><c>true</c> if it is possibly an encrypted text, <c>false</c> if it could not be an encrypted text</returns>
    public static bool IsEncyrpted(this string cipherText)
    {
      if (cipherText == null)
        return false;

      // Any text (even empty) that is encyrpted is at least 30 chars
      if (cipherText.Length < 30)
        return false;

      // in case the text does conatin a non Base64 chars it is not encrypted
      var test = c_Base64 + "/+=";
      foreach (var t in cipherText)
        if (test.IndexOf(t) == -1)
          return false;

      return true;
    }
  }
}