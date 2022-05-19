using System;

namespace CsvTools
{
  /// <summary>
  /// Interface to provde timezone adjustments
  /// </summary>
  public interface ITimeZoneAdjust
  {
    /// <summary>
    /// Timezone of teh local system, while importng data should be converted to this time zone, while exproting its assumed the dates they ned to be converted from there.
    /// </summary>
    string LocalTimeZone { get; set; }

    /// <summary>
    /// Convert incoming dates with timeezone to the local time
    /// </summary>
    /// <param name="input">The source date/time</param>
    /// <param name="srcTimeZone">The source timezone, the implemation  will decide which values are actually allowed</param>
    /// <param name="handleWarning">In case a conversion is not possible the information is apsssed on</param>
    /// <returns>The date time in loacl timzone</returns>
    DateTime AdjustTZ(in DateTime input, in string srcTimeZone, in string descTimeZone, in Action<string>? handleWarning);
  }
}
