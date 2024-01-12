using System;

namespace CsvTools
{
  /// <summary>
  /// Class to represent a character, but it does support recognition and conversion to written punctuation like "Tab"
  /// </summary>
  public static class Punctuation
  {

    /// <summary>
    ///   Gets a descriptive text for a character
    /// </summary>
    public static string Description(this char character)
      => character switch
      {
        '\t' => "Horizontal Tab",
        ' ' => "Space",
        '\u00A0' => "Non-breaking space",
        '\\' => "Backslash \\",
        '/' => "Slash /",
        ',' => "Comma ,",
        ';' => "Semicolon ;",
        ':' => "Colon :",
        '.' => "Dot .",
        '|' => "Pipe |",
        '"' => "Quotation marks \"",
        '\'' => "Apostrophe \'",
        '&' => "Ampersand &",
        '*' => "Asterisk *",
        '`' => "Tick Mark `",
        '✓' => "Check mark ✓",
        '\u001C' => "File Separator Char 28",
        '\u001D' => "Group Separator Char 29",
        '\u001E' => "Record Separator ␞",
        '\u001F' => "Unit Separator ␟",
        _ => character.ToString()
      };


    /// <summary>
    /// The printable text representation of the character, a tab will be shown as "Tab"
    /// </summary>
    public static string Text(this char character)
    => character switch
    {
      '\0' => string.Empty,
      '\t' => "Tab",
      '\r' => "CR",
      '\n' => "LF",
      ' ' => "Space",
      '\u00A0' => "NBSP",
      '\u001F' => "US",
      '\u001E' => "RS",
      '\u001D' => "GS",
      '\u001C' => "FS",
      _ => character.ToString()
    };

    /// <summary>
    /// Set the text if something did change, return true
    /// </summary>
    /// <param name="character">The storage</param>
    /// <param name="value"></param>
    /// <returns><c>true</c> if value is changed</returns>
    public static bool SetText(this ref char character, in string? value)
    {
      if (character.Text().Equals(value, StringComparison.Ordinal))
        return false;
      character = FromText(value);
      return true;
    }

    /// <summary>
    ///   Return a string resolving written punctuation
    /// </summary>
    /// <param name="inputString"></param>
    /// <returns></returns>
    public static string HandleLongText(this string? inputString)
    {
      if (inputString is null)
        return string.Empty;

      if (inputString.Length>1)
      {
        var from = inputString.FromText();
        if (from!=inputString[0])
          return from.ToString();
      }
      return inputString;
    }

    /// <summary>
    ///   Return a character resolving written punctuation
    /// </summary>
    /// <param name="inputString">The text to check</param>
    public static char FromText(this string? inputString)
    {
      if (inputString is null)
        return char.MinValue;
      if (inputString.Length == 1)
      {
        if (inputString.Equals("␍", StringComparison.Ordinal))
          return '\r';
        return inputString.Equals("␊", StringComparison.Ordinal) ? '\n' : inputString[0];
      }

      // Only do a trim if we do not have a single char, otherwise Space or Tab are removed
      var compareText = inputString.Trim();
      if (compareText.Length == 0)
        return char.MinValue;

      // if the text is longer we might have a text that represents a punctuation
      if (compareText.Equals("Tab", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Tabulator", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Horizontal Tab", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("HorizontalTab", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("\\t", StringComparison.OrdinalIgnoreCase))
        return '\t';

      if (compareText.Equals("Space", StringComparison.OrdinalIgnoreCase) ||
          compareText.Equals("\\s", StringComparison.OrdinalIgnoreCase))
        return ' ';

      if (compareText.Equals("hash", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("sharp", StringComparison.OrdinalIgnoreCase))
        return '#';

      if (compareText.Equals("whirl", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("at", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("monkey", StringComparison.OrdinalIgnoreCase))
        return '@';

      // ReSharper disable once StringLiteralTypo
      if (compareText.Equals("underbar", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("underscore", StringComparison.OrdinalIgnoreCase)
          // ReSharper disable once StringLiteralTypo
          || compareText.Equals("understrike", StringComparison.OrdinalIgnoreCase))
        return '_';

      if (compareText.Equals("Comma", StringComparison.OrdinalIgnoreCase))
        return ',';

      if (compareText.Equals("Dot", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Point", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Full Stop", StringComparison.OrdinalIgnoreCase))
        return '.';

      // ReSharper disable once StringLiteralTypo
      if (compareText.Equals("amper", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("ampersand", StringComparison.OrdinalIgnoreCase))
        return '&';

      if (compareText.Equals("Pipe", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Vertical bar", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("VerticalBar", StringComparison.OrdinalIgnoreCase))
        return '|';

      if (compareText.Equals("broken bar", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("BrokenBar", StringComparison.OrdinalIgnoreCase))
        return '¦';

      if (compareText.Equals("fullwidth broken bar", StringComparison.OrdinalIgnoreCase) || 
          compareText.Equals("FullwidthBrokenBar", StringComparison.OrdinalIgnoreCase))
        return '￤';

      if (compareText.Equals("Semicolon", StringComparison.OrdinalIgnoreCase))
        return ';';

      if (compareText.Equals("Colon", StringComparison.OrdinalIgnoreCase))
        return ':';

      // ReSharper disable once StringLiteralTypo
      if (compareText.StartsWith("Doublequote", StringComparison.OrdinalIgnoreCase)                    
          || compareText.Equals("Quote", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Quotation marks", StringComparison.OrdinalIgnoreCase))
        return '"';

      if (compareText.Equals("Apostrophe", StringComparison.OrdinalIgnoreCase)          
          || compareText.StartsWith("Singlequote", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("tick", StringComparison.OrdinalIgnoreCase))
        return '\'';

      if (compareText.Equals("Slash", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Stroke", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("forward slash", StringComparison.OrdinalIgnoreCase))
        return '/';

      if (compareText.Equals("backslash", StringComparison.OrdinalIgnoreCase)
          // ReSharper disable once StringLiteralTypo
          || compareText.Equals("backslant", StringComparison.OrdinalIgnoreCase))
        return '\\';

      if (compareText.Equals("Tick", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Tick Mark", StringComparison.OrdinalIgnoreCase))
        return '`';

      if (compareText.Equals("Star", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Asterisk", StringComparison.OrdinalIgnoreCase))
        return '*';

      if (compareText.Equals("NBSP", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Non-breaking space", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Non breaking space", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("NonBreakingSpace", StringComparison.OrdinalIgnoreCase))
        return '\u00A0';

      if (compareText.Equals("Return", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("CarriageReturn", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("CR", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Carriage return", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("\\r", StringComparison.OrdinalIgnoreCase))
        return '\r';

      if (compareText.Equals("Check mark", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Check", StringComparison.OrdinalIgnoreCase))
        return '✓';

      if (compareText.Equals("Feed", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("LineFeed", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("LF", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Line feed", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("\\n", StringComparison.OrdinalIgnoreCase))
        return '\n';

      if (compareText.StartsWith("Unit separator", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("US", StringComparison.OrdinalIgnoreCase))
        return '\u001F';

      if (compareText.StartsWith("Record separator", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("RS", StringComparison.OrdinalIgnoreCase))
        return '\u001E';

      if (compareText.StartsWith("Group separator", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("GS", StringComparison.OrdinalIgnoreCase))
        return '\u001D';

      if (compareText.StartsWith("File separator", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("FS", StringComparison.OrdinalIgnoreCase))
        return '\u001C';

      return compareText[0];
    }
  }
}
