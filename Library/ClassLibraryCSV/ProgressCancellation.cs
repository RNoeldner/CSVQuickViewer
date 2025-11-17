/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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
using System.Diagnostics;
using System.Threading;

namespace CsvTools
{
  /// <summary>
  /// A minimal implementation of <see cref="IProgress{T}"/> that forwards reported
  /// <see cref="ProgressInfo"/> instances to a logging or callback delegate.
  /// 
  /// The class acts as a fallback progress reporter for components that expect
  /// progress reporting and cancellation support, but where the caller does not
  /// require UI updates or dedicated progress handling.
  ///
  /// Typical use cases include:
  /// <list type="bullet">
  ///   <item><description>Supplying a required progress reporter without implementing UI logic.</description></item>
  ///   <item><description>Ensuring a consistent way to expose a <see cref="CancellationToken"/>.</description></item>
  ///   <item><description>Providing optional lightweight logging of progress messages.</description></item>
  /// </list>
  /// </summary>
  [DebuggerStepThrough]
  public sealed class ProgressCancellation : IProgressWithCancellation
  {
    private readonly Action<ProgressInfo> m_OnReport;
    private static readonly Action<ProgressInfo> LoggerAction = (value) => Logger.Information(value.Text);

    /// <summary>
    /// A reusable default instance.
    /// </summary>
    public static readonly ProgressCancellation Instance = new ProgressCancellation(CancellationToken.None);
    
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="cancellationToken">
    /// The cancellation token to expose. If omitted, <see cref="CancellationToken.None"/> is used.
    /// </param> 
    public ProgressCancellation(CancellationToken cancellationToken)
    {
      CancellationToken = cancellationToken;
      m_OnReport = LoggerAction;
    }

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    /// <param name="cancellationToken">
    /// The cancellation token to expose. If omitted, <see cref="CancellationToken.None"/> is used.
    /// </param>
    /// <param name="onReportAction">
    /// An optional delegate that receives progress updates. If not provided,
    /// messages are forwarded to the default logger.
    /// </param>
    public ProgressCancellation(CancellationToken cancellationToken, Action<ProgressInfo> onReportAction )
    {
      CancellationToken = cancellationToken;
      m_OnReport = onReportAction;
    }

    /// <summary>
    /// Gets the cancellation token associated with this instance.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Reports a progress update by forwarding it to the configured delegate.
    /// </summary>
    public void Report(ProgressInfo value) => m_OnReport(value);
  }
}