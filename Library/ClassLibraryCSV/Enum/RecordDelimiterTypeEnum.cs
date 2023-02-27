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
    [Description("Carriage Return (rarly used)")]
    Cr = 2,

    [ShortDescription("CR LF")]
    [Description("Carriage Return / Line feed (Windows)")]
    Crlf = 3,

    [ShortDescription("LF CR")]
    [Description("Line feed / Carriage Return (rarly used)")]
    Lfcr = 4,

    [ShortDescription("RS")]
    [Description("Record Seperator (QNX rarly used)")]
    Rs = 5,

    [ShortDescription("US")]
    [Description("Unit Seperator (rarly used)")]
    Us = 6,

    [ShortDescription("NL")]
    [Description("NewLine (IBM mainframe)")]
    Nl = 7
  }
}