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

using System;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <inheritdoc />
#pragma warning disable S3881 // "IDisposable" should be implemented correctly
  public abstract class DisposableBase : IDisposable
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    , IAsyncDisposable
#endif
#pragma warning restore S3881 // "IDisposable" should be implemented correctly
  {
    /// <summary>
    ///   Stop the Dispose() method execution in case of it being called more than once.
    /// </summary>
    private bool m_DisposedValue;

    /// <summary>
    ///   Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing">
    ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
    ///   unmanaged resources.
    /// </param>
    protected abstract void Dispose(bool disposing);

    /// <inheritdoc />
    public void Dispose()
    {
      if (!m_DisposedValue)
        Dispose(true);
      m_DisposedValue = true;
      GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    ~DisposableBase() => Dispose(false);

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <inheritdoc />
    public virtual ValueTask DisposeAsync()
    {
      Dispose();
      return default;
    }
#endif
  }
}