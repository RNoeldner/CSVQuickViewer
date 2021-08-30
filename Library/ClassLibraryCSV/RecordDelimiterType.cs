using System.ComponentModel;

namespace CsvTools
{
  public enum RecordDelimiterType
	{
		[Description("")]
		None,

		[Description("Line feed")]
		LF,

		[Description("Carriage Return")]
		CR = 2,

		[Description("Carriage Return / Line feed")]
		CRLF = 3,

		[Description("Line feed / Carriage Return")]
		LFCR = 4,

		[Description("Record Seperator")]
		RS = 5,

		[Description("Unit Seperator")]
		US = 6
	}
}