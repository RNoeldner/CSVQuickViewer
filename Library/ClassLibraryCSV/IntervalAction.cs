using CsvTools;
using System;

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
  ///   Creates a new <see cref="IntervalAction"/> instance if the specified progress object is not null.
  /// </summary>
  /// <param name="progress">The progress object to report to. Returns null if this parameter is null.</param>
  /// <returns>An <see cref="IntervalAction"/> instance or null.</returns>
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
  [Obsolete("Use construtor with interval instaed")]
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
  ///   Invokes the specified <see cref="Action{T}"/> with a long parameter if the minimum interval has elapsed.
  /// </summary>
  /// <param name="action">The action to invoke.</param>
  /// <param name="number">The long parameter passed to the action.</param>
  public void Invoke(Action<long> action, long number)
      => Invoke(() => action(number));

  /// <summary>
  ///   Invokes the specified <see cref="Action{T1,T2,T3}"/> with three long parameters if the minimum interval has elapsed.
  /// </summary>
  /// <param name="action">The action to invoke.</param>
  /// <param name="number1">The first parameter.</param>
  /// <param name="number2">The second parameter.</param>
  /// <param name="number3">The third parameter.</param>
  public void Invoke(Action<long, long, long> action, long number1, long number2, long number3)
      => Invoke(() => action(number1, number2, number3));

  /// <summary>
  ///   Invokes the specified <see cref="Action{T}"/> with a string parameter if the minimum interval has elapsed.
  /// </summary>
  /// <param name="action">The action to invoke.</param>
  /// <param name="txt">The string parameter passed to the action.</param>
  public void Invoke(Action<string> action, string txt)
      => Invoke(() => action(txt));

  /// <summary>
  ///   Reports progress using an <see cref="IProgress{ProgressInfo}"/> instance
  ///   if the minimum interval has elapsed.
  /// </summary>
  /// <param name="progress">The progress reporter.</param>
  /// <param name="text">The text description to report.</param>
  /// <param name="value">The numeric progress value.</param>
  public void Invoke(IProgress<ProgressInfo> progress, string text, long value)
      => Invoke(() => progress.Report(new ProgressInfo(text, value)));

  /// <summary>
  ///   Reports progress using an <see cref="IProgress{ProgressInfo}"/> instance
  ///   if the minimum interval has elapsed.
  /// </summary>
  /// <param name="progress">The progress reporter.</param>
  /// <param name="text">The text description to report.</param>
  /// <param name="value">The floating-point progress value.</param>
  public void Invoke(IProgress<ProgressInfo> progress, string text, float value)
      => Invoke(() => progress.Report(new ProgressInfo(text, value)));
}
