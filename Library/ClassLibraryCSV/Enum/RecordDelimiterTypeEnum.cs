using System.ComponentModel;

namespace CsvTools
{
  public enum RecordDelimiterTypeEnum
  {
    None,

    [ShortDescription("LF")]
    [Description("Line feed (Unix)")]
    Lf,

    [ShortDescription("CR")]
    [Description("Carriage Return (uncommon)")]
    Cr = 2,

    [ShortDescription("CR LF")]
    [Description("Carriage Return / Line feed (Windows)")]
    Crlf = 3,

    [ShortDescription("LF CR")]
    [Description("Line feed / Carriage Return (rarely used)")]
    Lfcr = 4,

    [ShortDescription("RS")]
    [Description("Record Separator (QNX rarely used)")]
    Rs = 5,

    [ShortDescription("US")]
    [Description("Unit Separator (rarely used)")]
    Us = 6,

    [ShortDescription("NL")]
    [Description("NewLine (IBM mainframe)")]
    Nl = 7
  }
}