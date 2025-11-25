using System;
using System.Diagnostics;

namespace CsvTools;

/// <summary>
///   Extremely lightweight time-to-completion estimator using a fixed-size
///   circular buffer and high-resolution monotonic timestamps (<see cref="Stopwatch"/>).
///
///   This implementation:
///   <list type="bullet">
///     <item><description>avoids all allocations during runtime (zero-alloc)</description></item>
///     <item><description>runs in O(1) time for each update</description></item>
///     <item><description>uses a fixed memory footprint</description></item>
///     <item><description>produces smooth, stable estimates based on recent velocity</description></item>
///     <item><description>is not thread-safe; callers must synchronize externally</description></item>
///   </list>
/// </summary>
public sealed class TimeToCompletion
{
  /// <summary>
  ///   Maximum displayed timespan or fallback when no estimate is possible.
  /// </summary>
  public static readonly TimeSpan Max = TimeSpan.FromDays(2);

  // Fixed circular buffer configuration
  private readonly int m_BufferSize;
  private readonly TimeSpan m_MaxAge;

  // Circular buffer storage for values and timestamps (Stopwatch ticks)
  private readonly long[] m_Values;
  private readonly long[] m_Timestamps;

  // Next index to write / current length
  private int m_Index;
  private int m_Count;

  // Target and last value
  private long m_TargetValue;
  private long m_LastValue;

  // Stopwatch frequency → ticks per second
  private readonly long m_TicksPerSecond = Stopwatch.Frequency;

  /// <summary>
  ///   Creates a high-performance progress estimator.
  /// </summary>
  /// <param name="targetValue">
  ///   The final value representing 100%. Must be ≥ 1.
  /// </param>
  /// <param name="bufferSize">
  ///   Maximum number of samples stored in the rolling history.
  ///   (32 is recommended for low jitter and minimal memory usage.)
  /// </param>
  /// <param name="historySeconds">
  ///   Maximum age (in seconds) of samples used for velocity estimation.
  /// </param>
  public TimeToCompletion(long targetValue = 1, int bufferSize = 32, double historySeconds = 15.0)
  {
    m_TargetValue = Math.Max(1, targetValue);
    m_BufferSize = bufferSize;
    m_MaxAge = TimeSpan.FromSeconds(historySeconds);

    // Allocate fixed-size circular buffers
    m_Values = new long[bufferSize];
    m_Timestamps = new long[bufferSize];
  }

  /// <summary>
  ///   Gets the computed remaining time. Defaults to <see cref="Max"/> if no
  ///   meaningful estimate can be made.
  /// </summary>
  public TimeSpan EstimatedTimeRemaining { get; private set; } = Max;

  /// <summary>
  ///   Human-readable string of remaining time (or empty if not applicable).
  /// </summary>
  public string EstimatedTimeRemainingDisplay =>
    DisplayTimespan(EstimatedTimeRemaining);

  /// <summary>
  ///   Returns the estimated remaining time prefixed with " - ", or empty when
  ///   no estimate is available (for UI display convenience).
  /// </summary>
  public string EstimatedTimeRemainingDisplaySeparator =>
    EstimatedTimeRemaining >= Max ? string.Empty : " - " + EstimatedTimeRemainingDisplay;

  /// <summary>
  ///   Current percentage (0–100) based purely on the last reported value,
  ///   not the velocity estimate.
  /// </summary>
  public double Percent { get; private set; }

  /// <summary>
  ///   Formatted percentage suitable for UI display.
  /// </summary>
  public string PercentDisplay => Percent < 10 ? $"{Percent:F1}%" : $"{Percent:F0}%";

  /// <summary>
  ///   Gets or sets the target (100% completion) value.
  ///   Values less than 1 are normalized to 1.
  /// </summary>
  public long TargetValue
  {
    get => m_TargetValue;
    set
    {
      m_TargetValue = Math.Max(1, value);
      Recalculate();
    }
  }

  /// <summary>
  ///   Gets or updates the current progress value.
  ///   Each update records a new sample and triggers recalculation.
  /// </summary>
  public long Value
  {
    get => m_LastValue;
    set
    {
      // Reject regressions, duplicates, or invalid values
      if (value <= m_LastValue || value > m_TargetValue)
        return;

      m_LastValue = value;
      AddSample(value);
      Recalculate();
    }
  }

  // ——————————————————————————————————————————————
  //         Internal mechanics: circular buffer
  // ——————————————————————————————————————————————

  /// <summary>
  ///   Inserts a new value/timestamp sample into the circular history buffer.
  /// </summary>
  private void AddSample(long value)
  {
    long now = Stopwatch.GetTimestamp();

    m_Values[m_Index] = value;
    m_Timestamps[m_Index] = now;

    // Advance cursor
    m_Index = (m_Index + 1) % m_BufferSize;

    // Increase count until the buffer is full
    if (m_Count < m_BufferSize)
      m_Count++;
  }

  /// <summary>
  ///   Recalculates velocity, percentage, and estimated remaining time using
  ///   only the newest and oldest valid samples inside <see cref="m_MaxAge"/>.
  /// </summary>
  private void Recalculate()
  {
    if (m_Count < 2)
    {
      EstimatedTimeRemaining = Max;
      Percent = (double) m_LastValue / m_TargetValue * 100.0;
      return;
    }
    
    var maxAgeTicks = (long) (m_MaxAge.TotalSeconds * m_TicksPerSecond);

    // Latest sample index (just behind the write position)
    var newest = (m_Index - 1 + m_BufferSize) % m_BufferSize;
    var newestValue = m_Values[newest];
    var newestTime = m_Timestamps[newest];

    // Walk backward to find the oldest sample within the allowed age
    var oldest = newest;
    for (var i = 1; i < m_Count; i++)
    {
      var idx = (newest - i + m_BufferSize) % m_BufferSize;
      var age = newestTime - m_Timestamps[idx];

      // Stop once we exceed the max age threshold
      if (age > maxAgeTicks)
        break;

      oldest = idx;
    }

    var firstValue = m_Values[oldest];
    var firstTime = m_Timestamps[oldest];

    var deltaValue = newestValue - firstValue;
    var deltaTicks = newestTime - firstTime;

    // Not enough movement → no meaningful velocity
    if (deltaValue <= 0 || deltaTicks <= 0)
    {
      EstimatedTimeRemaining = Max;
      Percent = (double) m_LastValue / m_TargetValue * 100.0;
      return;
    }

    // High-precision velocity (units per second)
    double velocity =
      deltaValue / (deltaTicks / (double) m_TicksPerSecond);

    // Estimate time left
    double remainingSeconds =
      (m_TargetValue - newestValue) / velocity;

    Percent = (double) newestValue / m_TargetValue * 100.0;

    // Valid result?
    EstimatedTimeRemaining =
      (remainingSeconds > 0 && remainingSeconds < Max.TotalSeconds)
        ? TimeSpan.FromSeconds(remainingSeconds)
        : Max;
  }

  /// <summary>
  ///   Converts a <see cref="TimeSpan"/> into a readable UI-friendly string.
  ///   Short times (&lt; 2 sec) are suppressed unless explicitly requested.
  /// </summary>
  public static string DisplayTimespan(TimeSpan value, bool cut2Sec = true)
  {
    if (value == Max || (cut2Sec && value.TotalSeconds < 2) || value.TotalDays > 2)
      return string.Empty;
    if (value.TotalSeconds < 2 && !cut2Sec)
      return $"{value:s\\.ff} sec";
    if (value.TotalMinutes < 1)
      return $"{value:%s} sec";
    if (value.TotalHours < 1)
      return $"{value:m\\:ss} min";
    return value.TotalHours < 24 ? $"{value:h\\:mm} hrs" : $"{value.TotalDays:F1} days";
  }
}