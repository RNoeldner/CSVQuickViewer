using System.ComponentModel;

namespace CsvTools
{
  public enum RecordDelimiterType
	{
		[Description("")]
		None,

		[Description("Line feed")]
		Lf,

		[Description("Carriage Return")]
		Cr = 2,

		[Description("Carriage Return / Line feed")]
		Crlf = 3,

		[Description("Line feed / Carriage Return")]
		Lfcr = 4,

		[Description("Record Seperator")]
		Rs = 5,

		[Description("Unit Seperator")]
		Us = 6
	}
}