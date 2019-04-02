using System;
using System.Globalization;
using System.Threading;

namespace CsvTools.Tests
{
  internal static class UnitTestStatic
  {    
    public static T ExecuteWithCulture<T>(Func<T> methodFunc, string cultureName)
    {
      T result = default(T);

      var thread = new Thread(() => { result = methodFunc(); })
      {
        CurrentCulture = new CultureInfo(cultureName)
      };
      thread.Start();
      thread.Join();

      return result;
    }
  }
}