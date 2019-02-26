using log4net.Layout;
using log4net.Util;

namespace CsvTools
{
  public class CustomPatternLayout : PatternLayout
  {
    public CustomPatternLayout() => AddConverter(new ConverterInfo { Name = "encodedmessage", Type = typeof(EncodedMessagePatternConvertor) });
  }
}