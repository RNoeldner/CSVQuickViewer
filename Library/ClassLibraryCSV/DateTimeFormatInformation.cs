using System;
using System.Diagnostics;
using System.Globalization;

namespace CsvTools
{
  [DebuggerDisplay("{m_MinLength} - {m_MaxLength}")]
  public class DateTimeFormatInformation
  {
    private static int m_MaxDayLong = int.MinValue;
    private static int m_MaxDayMid = int.MinValue;
    private static int m_MinDesignator = 1;
    private static int m_MaxDesignator = 2;
    private static int m_MaxMonthLong = int.MinValue;
    private static int m_MaxMonthMid = int.MinValue;
    private static int m_MinDayLong = int.MaxValue;
    private static int m_MinDayMid = int.MaxValue;
    private static int m_MinMonthLong = int.MaxValue;
    private static int m_MinMonthMid = int.MaxValue;
    private int m_MaxLength;
    private int m_MinLength;

    public DateTimeFormatInformation(string format)
    {
      MinLength = format.Length;
      MaxLength = format.Length;
      NamedDate = format.IndexOf("ddd", StringComparison.Ordinal) != -1 || format.IndexOf("MMM", StringComparison.Ordinal) != -1;

      if (m_MinDayLong == int.MaxValue)
        DetermineLength();

      format = SetMinMax(format, "dddd", ref m_MinLength, ref m_MaxLength, m_MinDayLong, m_MaxDayLong);
      format = SetMinMax(format, "ddd", ref m_MinLength, ref m_MaxLength, m_MinDayMid, m_MaxDayMid);
      format = SetMinMax(format, "dd", ref m_MinLength, ref m_MaxLength, 2, 2);
      format = SetMinMax(format, "d", ref m_MinLength, ref m_MaxLength, 1, 2);

      format = SetMinMax(format, "yyyy", ref m_MinLength, ref m_MaxLength, 4, 4);
      format = SetMinMax(format, "yy", ref m_MinLength, ref m_MaxLength, 2, 2);
      format = SetMinMax(format, "y", ref m_MinLength, ref m_MaxLength, 1, 2);

      format = SetMinMax(format, "HH", ref m_MinLength, ref m_MaxLength, 2, 2);
      format = SetMinMax(format, "H", ref m_MinLength, ref m_MaxLength, 1, 2);
      format = SetMinMax(format, "hh", ref m_MinLength, ref m_MaxLength, 2, 2);
      format = SetMinMax(format, "h", ref m_MinLength, ref m_MaxLength, 1, 2);

      format = SetMinMax(format, "mm", ref m_MinLength, ref m_MaxLength, 2, 2);
      format = SetMinMax(format, "m", ref m_MinLength, ref m_MaxLength, 1, 2);

      format = SetMinMax(format, "MMMM", ref m_MinLength, ref m_MaxLength, m_MinMonthLong, m_MaxMonthLong);
      format = SetMinMax(format, "MMM", ref m_MinLength, ref m_MaxLength, m_MinMonthMid, m_MaxMonthMid);
      format = SetMinMax(format, "MM", ref m_MinLength, ref m_MaxLength, 2, 2);
      format = SetMinMax(format, "M", ref m_MinLength, ref m_MaxLength, 1, 2);

      format = SetMinMax(format, "F", ref m_MinLength, ref m_MaxLength, 0, 1);

      format = SetMinMax(format, "ss", ref m_MinLength, ref m_MaxLength, 2, 2);
      format = SetMinMax(format, "s", ref m_MinLength, ref m_MaxLength, 1, 2);

      // interpreted as a standard date and time format specifier
      format = SetMinMax(format, "K", ref m_MinLength, ref m_MaxLength, 0, 6);

      // signed offset of the local operating system's time zone from UTC,
      format = SetMinMax(format, "zzz", ref m_MinLength, ref m_MaxLength, 6, 6);
      format = SetMinMax(format, "zz", ref m_MinLength, ref m_MaxLength, 3, 3);
      format = SetMinMax(format, "z", ref m_MinLength, ref m_MaxLength, 2, 3);

      // AM / PM
      SetMinMax(format, "tt", ref m_MinLength, ref m_MaxLength, m_MinDesignator, m_MaxDesignator);
    }

    // public string Format { get => m_Format; }
    public int MaxLength
    {
      get => m_MaxLength;
      private set => m_MaxLength = value;
    }

    public int MinLength
    {
      get => m_MinLength;
      private set => m_MinLength = value;
    }

    public bool NamedDate { get; }

    private static void DetermineLength()
    {
      for (var weekday = 0; weekday < 7; weekday++)
      {
        var currentCulture = string.Format(CultureInfo.CurrentCulture, "{0:dddd}", DateTime.Now.AddDays(weekday));
        var invariantCulture = string.Format(CultureInfo.InvariantCulture, "{0:dddd}", DateTime.Now.AddDays(weekday));
        if (currentCulture.Length < m_MinDayLong)
          m_MinDayLong = currentCulture.Length;
        if (invariantCulture.Length < m_MinDayLong)
          m_MinDayLong = invariantCulture.Length;
        if (currentCulture.Length > m_MaxDayLong)
          m_MaxDayLong = currentCulture.Length;
        if (invariantCulture.Length > m_MaxDayLong)
          m_MaxDayLong = invariantCulture.Length;

        currentCulture = string.Format(CultureInfo.CurrentCulture, "{0:ddd}", DateTime.Now.AddDays(weekday));
        invariantCulture = string.Format(CultureInfo.InvariantCulture, "{0:ddd}", DateTime.Now.AddDays(weekday));
        if (currentCulture.Length < m_MinDayMid)
          m_MinDayMid = currentCulture.Length;
        if (invariantCulture.Length < m_MinDayMid)
          m_MinDayMid = invariantCulture.Length;
        if (currentCulture.Length > m_MaxDayMid)
          m_MaxDayMid = currentCulture.Length;
        if (invariantCulture.Length > m_MaxDayMid)
          m_MaxDayMid = invariantCulture.Length;
      }

      for (var month = 0; month < 12; month++)
      {
        var currentCulture = string.Format(CultureInfo.CurrentCulture, "{0:MMMM}", DateTime.Now.AddMonths(month));
        var invariantCulture = string.Format(CultureInfo.InvariantCulture, "{0:MMMM}", DateTime.Now.AddMonths(month));
        if (currentCulture.Length < m_MinMonthLong)
          m_MinMonthLong = currentCulture.Length;
        if (invariantCulture.Length < m_MinMonthLong)
          m_MinMonthLong = invariantCulture.Length;
        if (currentCulture.Length > m_MaxMonthLong)
          m_MaxMonthLong = currentCulture.Length;
        if (invariantCulture.Length > m_MaxMonthLong)
          m_MaxMonthLong = invariantCulture.Length;

        currentCulture = string.Format(CultureInfo.CurrentCulture, "{0:MMM}", DateTime.Now.AddMonths(month));
        invariantCulture = string.Format(CultureInfo.InvariantCulture, "{0:MMM}", DateTime.Now.AddMonths(month));
        if (currentCulture.Length < m_MinMonthMid)
          m_MinMonthMid = currentCulture.Length;
        if (invariantCulture.Length < m_MinMonthMid)
          m_MinMonthMid = invariantCulture.Length;
        if (currentCulture.Length > m_MaxMonthMid)
          m_MaxMonthMid = currentCulture.Length;
        if (invariantCulture.Length > m_MaxMonthMid)
          m_MaxMonthMid = invariantCulture.Length;
      }

      {
        if (CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator.Length > m_MaxDesignator)
          m_MaxDesignator = CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator.Length;
        if (CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator.Length > m_MaxDesignator)
          m_MaxDesignator = CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator.Length;

        if (CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator.Length > m_MinDesignator)
          m_MinDesignator = CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator.Length;
        if (CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator.Length > m_MinDesignator)
          m_MinDesignator = CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator.Length;
      }
    }

    private static string SetMinMax(string format, string search, ref int min, ref int max, int minLength, int maxLength)
    {
      var pos = format.IndexOf(search, StringComparison.Ordinal);
      while (pos != -1)
      {
        min += minLength - search.Length;
        max += maxLength - search.Length;
        format = format.Remove(pos, search.Length);
        pos = format.IndexOf(search, StringComparison.Ordinal);
      }
      return format;
    }
  }
}