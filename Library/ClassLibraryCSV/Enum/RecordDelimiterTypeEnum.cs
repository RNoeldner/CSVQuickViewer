using System.ComponentModel;

namespace CsvTools
{
  /// <summary>
  /// Line/Record Seperator for text files
  /// </summary>
  public enum RecordDelimiterTypeEnum
  {
    /// <summary>Unspecified</summary>
    None,

    /// <summary>Use Line feed</summary>
    [ShortDescription("LF")]
    [Description("Line feed (Unix)")]
    Lf,

    /// <summary>Use Carriage Return</summary>
    [ShortDescription("CR")]
    [Description("Carriage Return (uncommon)")]
    Cr = 2,

    /// <summary>Use Carriage Return / Line feed</summary>
    [ShortDescription("CR LF")]
    [Description("Carriage Return / Line feed (Windows)")]
    Crlf = 3,

    /// <summary>Use Line feed / Carriage Return</summary>
    [ShortDescription("LF CR")]
    [Description("Line feed / Carriage Return (rarely used)")]
    Lfcr = 4,

    /// <summary>Use Record Separator</summary>
    [ShortDescription("RS")]
    [Description("Record Separator (QNX rarely used)")]
    Rs = 5,

    /// <summary>Use Unit Separator</summary>
    [ShortDescription("US")]
    [Description("Unit Separator (rarely used)")]
    Us = 6,

    /// <summary>Use NewLine</summary>
    [ShortDescription("NL")]
    [Description("NewLine (IBM mainframe)")]
    Nl = 7
  }
}