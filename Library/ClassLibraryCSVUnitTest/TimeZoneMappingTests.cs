using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace CsvTools.Tests
{
  [TestClass()]
  public class TimeZoneMappingTests
  {
    private readonly string tzBRT = "America/Sao_Paulo";
    private readonly string tzCET = "Europe/Paris";
    private readonly string tzIST = "Asia/Kolkata";
    private readonly string tzMSK = "Europe/Moscow";
    private readonly string tzPST = "America/Los_Angeles";

    [TestMethod()]
    public void ConvertTime()
    {
      // 26/06/2017 08:00	CET - 26/06/2017 09:00	MSK -  25/06/2017 23:00	PST - 26/06/2017 03:00	(UTC-03:00) Brasilia
      var berlin1 = new DateTime(2018, 06, 26, 8, 00, 00);

      Assert.AreEqual(new DateTime(2018, 06, 26, 9, 00, 00),
        TimeZoneMapping.ConvertTime(berlin1, "Europe/Paris", "Europe/Moscow"), "CET - MSK");
      Assert.AreEqual(new DateTime(2018, 06, 25, 23, 00, 00),
        TimeZoneMapping.ConvertTime(berlin1, "Europe/Paris", "America/Los_Angeles"), "CET - PST");
      Assert.AreEqual(new DateTime(2018, 06, 26, 3, 00, 00),
        TimeZoneMapping.ConvertTime(berlin1, "Europe/Paris", "America/Sao_Paulo"), "CET - BRT");

      // 26/03/2017 01:00	CET (winter time) - 26/03/2017 03:00	MSK - 25/03/2017 17:00	PST (Summer Time) . 25/03/2017 21:00	(UTC-03:00) Brasilia
      var berlin2 = new DateTime(2017, 03, 26, 1, 00, 00);

      Assert.AreEqual(new DateTime(2017, 03, 26, 3, 00, 00),
        TimeZoneMapping.ConvertTime(berlin2, "Europe/Paris", "Europe/Moscow"), "CET - MSK");
      Assert.AreEqual(new DateTime(2017, 03, 25, 17, 00, 00),
        TimeZoneMapping.ConvertTime(berlin2, "Europe/Paris", "America/Los_Angeles"), "CET - PST");
      Assert.AreEqual(new DateTime(2017, 03, 25, 21, 00, 00),
        TimeZoneMapping.ConvertTime(berlin2, "Europe/Paris", "America/Sao_Paulo"), "CET - BRT");

      // 12/03/2017 11:00	CET - 12/03/2017 13:00	MSK - 12/03/2017 03:00	PST - 12/03/2017 07:00	(UTC-03:00) Brasilia
      var berlin3 = new DateTime(2017, 03, 12, 11, 00, 00);

      Assert.AreEqual(new DateTime(2017, 03, 12, 13, 00, 00),
        TimeZoneMapping.ConvertTime(berlin3, "Europe/Paris", "Europe/Moscow"), "CET - MSK");
      Assert.AreEqual(new DateTime(2017, 03, 12, 03, 00, 00),
        TimeZoneMapping.ConvertTime(berlin3, "Europe/Paris", "America/Los_Angeles"), "CET - PST");
      Assert.AreEqual(new DateTime(2017, 03, 12, 07, 00, 00),
        TimeZoneMapping.ConvertTime(berlin3, "Europe/Paris", "America/Sao_Paulo"), "CET - BRT");

      // 18/10/2014 20:00	CET - 18/10/2014 15:00	(UTC-03:00) Brasilia
      Assert.AreEqual(new DateTime(2014, 10, 18, 15, 00, 00),
        TimeZoneMapping.ConvertTime(new DateTime(2014, 10, 18, 20, 00, 00), "Europe/Paris", "America/Sao_Paulo"), "CET - BRT");

      // 19/10/2014 20:00	CET - 19/10/2014 16:00	(UTC-03:00) Brasilia
      Assert.AreEqual(new DateTime(2014, 10, 19, 16, 00, 00),
        TimeZoneMapping.ConvertTime(new DateTime(2014, 10, 19, 20, 00, 00), "Europe/Paris", "America/Sao_Paulo"), "CET - BRT");

      // Do, 14. Jun 1945 at 09:00 BDST
      // Mi, 13. Jun 1945 at 22:00 CAWT
      Assert.AreEqual(new DateTime(1945, 06, 13, 22, 00, 00),
       TimeZoneMapping.ConvertTime(new DateTime(1945, 06, 14, 09, 00, 00), "Europe/London", "America/Anchorage"), "London - Anchorage");

      // Mi, 14. Nov 1945 at 07:00 GMT
      // Di, 13. Nov 1945 at 21:00 CAT
      Assert.AreEqual(new DateTime(1945, 11, 10, 21, 00, 00),
       TimeZoneMapping.ConvertTime(new DateTime(1945, 11, 11, 07, 00, 00), "Europe/London", "America/Anchorage"), "London - Anchorage");
    }

    [TestMethod()]
    public void CopareConversion()
    {
      var timezone = new string[] { "Romance Standard Time",
                                    "Russian Standard Time",
                                    "E. South America Standard Time",
                                    "Pacific Standard Time",
                                    "India Standard Time"};
      var tzs = new string[] { tzCET, tzMSK, tzBRT, tzPST, tzIST };
      var rnd = new Random(200);
      int correct = 0;
      int incorrect = 0;
      for (int i = 0; i < 10000; i++)
      {
        var src = rnd.Next(0, 4);
        var dest = rnd.Next(0, 4);
        var date = new DateTime(rnd.Next(1995, 2020), rnd.Next(1, 12), rnd.Next(1, 28), rnd.Next(10, 22), rnd.Next(0, 59), rnd.Next(0, 59), DateTimeKind.Unspecified);
        DateTime dateNET;
        try
        {
          dateNET = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(date, timezone[src], timezone[dest]);
        }
        catch (ArgumentException)
        {
          // some local dates are not valid as they fall into the period of when time is moved forward
          // so adjust by 1 hour.
          dateNET = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(date.AddHours(1), timezone[src], timezone[dest]);
        }
        var dateIANA = TimeZoneMapping.ConvertTime(date, tzs[src], tzs[dest]);
        if (dateIANA.Day != dateNET.Day || dateIANA.Hour != dateNET.Hour || dateIANA.Minute != dateNET.Minute)
        {
          incorrect++;
          Console.WriteLine($"{date} {tzs[src]} -> {dateIANA} {tzs[dest]} the .NET date {dateNET} seems off");
        }
        else
        {
          correct++;
        }
      }
      if (incorrect > correct)
        Assert.Fail();
      if (incorrect > 0)
        Assert.Inconclusive($"{incorrect} time zone conversions detected, {correct} have been correct");
    }

    [TestMethod()]
    public void GetAbbreviation()
    {
      var result1 = TimeZoneMapping.GetTZAbbreviation(tzCET);
      Assert.AreEqual("CET", result1);
      var result2 = TimeZoneMapping.GetTZAbbreviation(tzIST);
      Assert.AreEqual("IST", result2);
    }

    [TestMethod()]
    public void GetAlternateNames()
    {
      var result1 = TimeZoneMapping.AlternateTZNames(tzCET);
      Assert.IsTrue(result1.Contains("Romance Standard Time"));

      var result2 = TimeZoneMapping.AlternateTZNames(tzIST);
      Assert.IsTrue(result2.Contains("IST"));
    }

    [TestMethod()]
    public void HasDaylightSavingTime()
    {
      Assert.IsTrue(tzCET.SupportsDaylightSavingTime(2018));
    }

    [TestInitialize]
    public void Init()
    {
      tzCET.GetTZAbbreviation();
    }

    [TestMethod()]
    public void IsDaylightSavingTime()
    {
      Assert.IsTrue(tzCET.IsDaylightSavingTime(new DateTime(2018, 06, 26, 8, 00, 00)));
    }

    [TestMethod()]
    public void TimingConversionNET()
    {
      var timezone = new string[] { "Romance Standard Time",
                                    "Russian Standard Time",
                                    "E. South America Standard Time",
                                    "Pacific Standard Time",
                                    "India Standard Time"};

      var rnd = new Random(188);

      for (int i = 0; i < 10000; i++)
      {
        var src = rnd.Next(0, 4);
        var dest = rnd.Next(0, 4);
        var date = new DateTime(rnd.Next(1900, 2020), rnd.Next(1, 12), rnd.Next(1, 28), rnd.Next(0, 23), rnd.Next(0, 59), rnd.Next(0, 59), DateTimeKind.Unspecified);
        try
        {
          TimeZoneInfo.ConvertTimeBySystemTimeZoneId(date, timezone[src], timezone[dest]);
        }
        catch (ArgumentException)
        {
          // some local dates are not valid as they fall into the period of when time is moved forward
          // so adjust by 1 hour.
          date = date.AddHours(1);
          TimeZoneInfo.ConvertTimeBySystemTimeZoneId(date, timezone[src], timezone[dest]);
        }
      }
    }

    [TestMethod()]
    public void TimingConversionNew()
    {
      var tzs = new string[] { tzCET, tzMSK, tzBRT, tzPST, tzIST };

      Random rnd = new Random(188);
      for (int i = 0; i < 10000; i++)
      {
        var src = rnd.Next(0, 4);
        var dest = rnd.Next(0, 4);
        var date = new DateTime(rnd.Next(1900, 2020), rnd.Next(1, 12), rnd.Next(1, 28), rnd.Next(0, 23), rnd.Next(0, 59), rnd.Next(0, 59), DateTimeKind.Utc);
        TimeZoneMapping.ConvertTime(date, tzs[src], tzs[dest]);
      }
    }

    [TestMethod()]
    public void WithSameRule()
    {
      var result = TimeZoneMapping.WithSameRule(tzCET, 2017);
      bool found = false;
      foreach (var item in result)
        if (item.Equals("Europe/Berlin", StringComparison.OrdinalIgnoreCase))
          found = true;

      Assert.IsTrue(found);
    }
  }
}