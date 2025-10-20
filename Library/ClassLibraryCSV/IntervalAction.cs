using CsvTools;
using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
///   Class to throttle calls to actions, ensuring they are invoked only after a specified interval.
/// </summary>
/// <remarks>
///   This class is <b>not thread-safe</b>. Using the same instance concurrently from multiple threads
///   may cause actions to be invoked more or less frequently than intended.
/// </remarks>
public sealed class IntervalAction
{
  private DateTime m_LastNotification = DateTime.MinValue;
  private TimeSpan m_NotifyAfter;

  /// <summary>
  ///   Initializes a new instance of the <see cref="IntervalAction"/> class with a default
  ///   notification interval of 0.25 seconds.
  /// </summary>
  public IntervalAction()
      : this(0.25d)
  {
  }

  /// <summary>
  ///   Factory method to create an <see cref="IntervalAction"/> for a progress reporter.
  /// </summary>
  /// <param name="progress">The progress reporter to use; returns null if <c>null</c>.</param>
  /// <returns>An <see cref="IntervalAction"/> instance if <paramref name="progress"/> is not null; otherwise null.</returns>
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull("progress")]
#endif
  public static IntervalAction? ForProgress(IProgress<ProgressInfo>? progress) =>
      progress is null ? null : new IntervalAction();

  /// <summary>
  ///   Initializes a new instance of the <see cref="IntervalAction"/> class with a custom interval.
  /// </summary>
  /// <param name="notifyAfterSeconds">
  ///   Minimum interval, in seconds, between invocations of the action.
  /// </param>
  public IntervalAction(double notifyAfterSeconds) => m_NotifyAfter = TimeSpan.FromSeconds(notifyAfterSeconds);

  /// <summary>
  ///   Sets the minimum interval, in seconds, between successive invocations.
  /// </summary>
  /// <value>The minimum interval in seconds.</value>
  [Obsolete("Use constructor with interval instead")]
  public double NotifyAfterSeconds
  {
    set => m_NotifyAfter = TimeSpan.FromSeconds(value);
  }

  /// <summary>
  ///   Invokes the specified action if the minimum interval has elapsed.
  ///   Updates the last invocation timestamp on execution.
  /// </summary>
  /// <param name="action">The action to invoke.</param>
  public void Invoke(Action action)
  {
    var now = DateTime.UtcNow;
    if (now - m_LastNotification < m_NotifyAfter)
      return;

    m_LastNotification = now;
    try
    {
      action();
    }
    catch (ObjectDisposedException) { }
    catch (Exception ex)
    {
      Logger.Warning(ex, "IntervalAction.Invoke {Method} failed: {Message}", action.Method, ex.Message);
    }
  }

  /// <summary>
  ///   Asynchronously invokes the specified <see cref="Func{Task}"/> if the minimum interval has elapsed
  ///   since the last invocation.
  /// </summary>
  /// <param name="asyncAction">The asynchronous action to invoke.</param>
  /// <param name="cancellationToken">
  ///   The cancellation token that can be passed to <paramref name="asyncAction"/> to 
  ///   signal cancellation of the operation.
  /// </param>
  /// <remarks>
  ///   Exceptions thrown by the action are logged but do not propagate.
  ///   <see cref="ObjectDisposedException"/> is silently ignored.
  /// </remarks>
  public async Task InvokeAsync(Func<CancellationToken, Task> asyncAction, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    var now = DateTime.UtcNow;
    if (now - m_LastNotification < m_NotifyAfter)
      return;

    m_LastNotification = now;
    try
    {
      await asyncAction(cancellationToken).ConfigureAwait(false);
    }
    catch (ObjectDisposedException) { }
    catch (Exception ex)
    {
      Logger.Warning(ex, "IntervalAction.InvokeAsync {Method} failed: {Message}", asyncAction.Method, ex.Message);
    }
  }

  /// <summary>
  ///   Reports progress using an <see cref="IProgress{ProgressInfo}"/> instance
  ///   if the minimum interval has elapsed.
  /// </summary>
  /// <param name="progress">The progress reporter.</param>
  /// <param name="text">The text description to report.</param>
  /// <param name="value">The numeric progress value.</param>
  public void Invoke(IProgress<ProgressInfo> progress, string text, long value)
      => Invoke(() => progress.Report(new ProgressInfo(text, value)));
}
