using System;
using System.Diagnostics;
using System.Globalization;

namespace CsvTools
{
  [DebuggerDisplay("{m_MinLength} - {m_MaxLength}")]
  public class DateTimeFormatInformation
  {
    private static int maxDayLong = int.MinValue;
    private static int maxDayMid = int.MinValue;
    private static int maxDesignator = 2;
    private static int maxMonthLong = int.MinValue;
    private static int maxMonthMid = int.MinValue;
    private static int minDayLong = int.MaxValue;
    private static int minDayMid = int.MaxValue;
    private static int minMonthLong = int.MaxValue;
    private static int minMonthMid = int.MaxValue;    
    private int m_MaxLength;
    private int m_MinLength;


    public DateTimeFormatInformation()
    {
    }

    public DateTimeFormatInformation(string format)
    {
      MinLength = format.Length;
      MaxLength = format.Length;
      NamedDate = (format.IndexOf("ddd", StringComparison.Ordinal) != -1 || format.IndexOf("MMM", StringComparison.Ordinal) != -1);

      if (minDayLong == int.MaxValue)
        DetermineLenth();

      format = SetMinMax(format, "dddd", ref m_MinLength, ref m_MaxLength, minDayLong, maxDayLong);
      format = SetMinMax(format, "ddd", ref m_MinLength, ref m_MaxLength, minDayMid, maxDayMid);
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

      format = SetMinMax(format, "MMMM", ref m_MinLength, ref m_MaxLength, minMonthLong, maxMonthLong);
      format = SetMinMax(format, "MMM", ref m_MinLength, ref m_MaxLength, minMonthMid, maxMonthMid);
      format = SetMinMax(format, "MM", ref m_MinLength, ref m_MaxLength, 2, 2);
      format = SetMinMax(format, "M", ref m_MinLength, ref m_MaxLength, 1, 2);

      format = SetMinMax(format, "ss", ref m_MinLength, ref m_MaxLength, 2, 2);
      format = SetMinMax(format, "s", ref m_MinLength, ref m_MaxLength, 1, 2);

      format = SetMinMax(format, "K", ref m_MinLength, ref m_MaxLength, 0, 6);
      format = SetMinMax(format, "zzz", ref m_MinLength, ref m_MaxLength, 0, 6);
      format = SetMinMax(format, "zz", ref m_MinLength, ref m_MaxLength, 3, 3);
      format = SetMinMax(format, "z", ref m_MinLength, ref m_MaxLength, 2, 3);

      SetMinMax(format, "tt", ref m_MinLength, ref m_MaxLength, 2, maxDesignator);
    }

    //  public string Format { get => m_Format; }
    public int MaxLength { get => m_MaxLength; set => m_MaxLength = value; }
    public int MinLength { get => m_MinLength; set => m_MinLength = value; }
    public bool NamedDate { get; set; } = false;

    private static void DetermineLenth()
    {
      for (int weekday = 0; weekday < 7; weekday++)
      {
        var cul = string.Format(CultureInfo.CurrentCulture, "{0:dddd}", DateTime.Now.AddDays(weekday));
        var incul = string.Format(CultureInfo.InvariantCulture, "{0:dddd}", DateTime.Now.AddDays(weekday));
        if (cul.Length < minDayLong)
          minDayLong = cul.Length;
        if (incul.Length < minDayLong)
          minDayLong = incul.Length;
        if (cul.Length > maxDayLong)
          maxDayLong = cul.Length;
        if (incul.Length > maxDayLong)
          maxDayLong = incul.Length;

        cul = string.Format(CultureInfo.CurrentCulture, "{0:ddd}", DateTime.Now.AddDays(weekday));
        incul = string.Format(CultureInfo.InvariantCulture, "{0:ddd}", DateTime.Now.AddDays(weekday));
        if (cul.Length < minDayMid)
          minDayMid = cul.Length;
        if (incul.Length < minDayMid)
          minDayMid = incul.Length;
        if (cul.Length > maxDayMid)
          maxDayMid = cul.Length;
        if (incul.Length > maxDayMid)
          maxDayMid = incul.Length;
      }

      for (int month = 0; month < 12; month++)
      {
        var cul = string.Format(CultureInfo.CurrentCulture, "{0:MMMM}", DateTime.Now.AddMonths(month));
        var incul = string.Format(CultureInfo.InvariantCulture, "{0:MMMM}", DateTime.Now.AddMonths(month));
        if (cul.Length < minMonthLong)
          minMonthLong = cul.Length;
        if (incul.Length < minMonthLong)
          minMonthLong = incul.Length;
        if (cul.Length > maxMonthLong)
          maxMonthLong = cul.Length;
        if (incul.Length > maxMonthLong)
          maxMonthLong = incul.Length;

        cul = string.Format(CultureInfo.CurrentCulture, "{0:MMM}", DateTime.Now.AddMonths(month));
        incul = string.Format(CultureInfo.InvariantCulture, "{0:MMM}", DateTime.Now.AddMonths(month));
        if (cul.Length < minMonthMid)
          minMonthMid = cul.Length;
        if (incul.Length < minMonthMid)
          minMonthMid = incul.Length;
        if (cul.Length > maxMonthMid)
          maxMonthMid = cul.Length;
        if (incul.Length > maxMonthMid)
          maxMonthMid = incul.Length;
      }

      {
        if (CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator.Length > 2)
          maxDesignator = CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator.Length;
        if (CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator.Length > 2)
          maxDesignator = CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator.Length;
      }
    }

    private static string SetMinMax(string format, string search, ref int min, ref int max, int minLength, int maxLength)
    {
      int pos = format.IndexOf(search, StringComparison.Ordinal);
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