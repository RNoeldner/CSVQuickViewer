using System.ComponentModel;

namespace CsvTools
{
  public enum RecordDelimiterTypeEnum
  {
    None,

    [Description("Line feed (Unix)")]
    [ShortDescription("LF")] 
    Lf,

    [Description("Carriage Return (rarly used)")]
    [ShortDescription("CR")] 
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

    [ShortDescription("Us")]
    [Description("Unit Seperator (rarly used)")] 
    Us = 6,

    [ShortDescription("NL")]
    [Description("NewLine (IBM mainframe)")] 
    Nl = 7
  }
}