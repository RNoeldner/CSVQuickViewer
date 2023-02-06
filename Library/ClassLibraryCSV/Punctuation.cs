using System;

namespace CsvTools
{
  public class Punctuation
  {
    private char m_Char = char.MinValue;

    public bool IsEmpty
    {
      get => m_Char == char.MinValue;
    }

    public char Char
    {
      get => m_Char;
      set
      {
        if (m_Char != value)
          m_Char = value;
      }
    }

    public string Text
    {
      get => ToText(m_Char);
      set => m_Char =  FromText(value);
    }

    /// <summary>
    /// Set teh text if something did change return true
    /// </summary>
    /// <param name="value"></param>
    /// <returns><c>true</c> if value is changed</returns>
    public bool SetText(in string? value)
    {
      if (ToText(m_Char).Equals(value, StringComparison.Ordinal))
        return false;
      m_Char =  FromText(value);
      return true;
    }

    public Punctuation(char character)
    {
      m_Char = character;
    }

    public Punctuation(string text)
    {
      m_Char = FromText(text);
    }

    private static string ToText(char input)
    {
      return input switch
      {
        '\t' => "Tab",
        ' ' => "Space",
        '\u00A0' => "NBSP",
        '\u001F' => "US",
        '\u001E' => "RS",
        '\u001D' => "GS",
        '\u001C' => "FS",
        _ => input.ToStringHandle0()
      };
    }

    /// <summary>
    ///   Return a string resolving written punctuation
    /// </summary>
    /// <param name="inputString"></param>
    /// <returns>A string of length 1 or empty</returns>
    private static char FromText(in string? inputString)
    {
      if (inputString == null)
        return char.MinValue;

      var compareText = inputString.Trim();
      if (compareText.Length==0)
        return char.MinValue;

      if (compareText.Length == 1)
      {
        if (compareText.Equals("␍", StringComparison.Ordinal))
          return '\r';
        if (compareText.Equals("␊", StringComparison.Ordinal))
          return '\n';
        return compareText[0];
      }

      if (compareText.Equals("Tab", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Tabulator", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("\\t", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Horizontal Tab", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("HorizontalTab", StringComparison.OrdinalIgnoreCase))
        return '\t';

      if (compareText.Equals("Space", StringComparison.OrdinalIgnoreCase))
        return ' ';

      if (compareText.Equals("hash", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("sharp", StringComparison.OrdinalIgnoreCase))
        return '#';

      if (compareText.Equals("whirl", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("at", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("monkey", StringComparison.OrdinalIgnoreCase))
        return '@';

      if (compareText.Equals("underbar", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("underscore", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("understrike", StringComparison.OrdinalIgnoreCase))
        return '_';

      if (compareText.Equals("Comma", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Comma: ,", StringComparison.OrdinalIgnoreCase))
        return ',';

      if (compareText.Equals("Dot", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Point", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Full Stop", StringComparison.OrdinalIgnoreCase))
        return '.';

      if (compareText.Equals("amper", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("ampersand", StringComparison.OrdinalIgnoreCase) || compareText.Equals(
            "Ampersand: &",
            StringComparison.OrdinalIgnoreCase))
        return '&';

      if (compareText.Equals("Pipe", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Vertical bar", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("VerticalBar", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Pipe: |", StringComparison.OrdinalIgnoreCase))
        return '|';

      if (compareText.Equals("broken bar", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("BrokenBar", StringComparison.OrdinalIgnoreCase))
        return '¦';

      if (compareText.Equals("fullwidth broken bar", StringComparison.OrdinalIgnoreCase) || compareText.Equals(
            "FullwidthBrokenBar",
            StringComparison.OrdinalIgnoreCase))
        return '￤';

      if (compareText.Equals("Semicolon", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Semicolon: ;", StringComparison.OrdinalIgnoreCase))
        return ';';

      if (compareText.Equals("Colon", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Colon: :", StringComparison.OrdinalIgnoreCase))
        return ':';

      if (compareText.Equals("Doublequote", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Doublequotes", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Quote", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Quotation marks", StringComparison.OrdinalIgnoreCase) || compareText.Equals(
            "Quotation marks: \"",
            StringComparison.OrdinalIgnoreCase))
        return '"';

      if (compareText.Equals("Apostrophe", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Singlequote", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("tick", StringComparison.OrdinalIgnoreCase) || compareText.Equals(
            "Apostrophe: \'",
            StringComparison.OrdinalIgnoreCase))
        return '\'';

      if (compareText.Equals("Slash", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Stroke", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("forward slash", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Slash: /", StringComparison.OrdinalIgnoreCase))
        return '/';

      if (compareText.Equals("backslash", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("backslant", StringComparison.OrdinalIgnoreCase) || compareText.Equals(
            "Backslash: \\",
            StringComparison.OrdinalIgnoreCase))
        return '\\';

      if (compareText.Equals("Tick", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Tick Mark", StringComparison.OrdinalIgnoreCase))
        return '`';

      if (compareText.Equals("Star", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Asterisk", StringComparison.OrdinalIgnoreCase) || compareText.Equals(
            "Asterisk: *",
            StringComparison.OrdinalIgnoreCase))
        return '*';

      if (compareText.Equals("NBSP", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Non-breaking space", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Non breaking space", StringComparison.OrdinalIgnoreCase) || compareText.Equals(
            "NonBreakingSpace",
            StringComparison.OrdinalIgnoreCase))
        return '\u00A0';

      if (compareText.Equals("Return", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("CarriageReturn", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("\\r", StringComparison.Ordinal)
          || compareText.Equals("CR", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("␍", StringComparison.Ordinal) || compareText.Equals(
            "Carriage return",
            StringComparison.OrdinalIgnoreCase))
        return '\r';

      if (compareText.Equals("Check mark", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("Check", StringComparison.OrdinalIgnoreCase))
        return '✓';

      if (compareText.Equals("Feed", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("LineFeed", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("\\n", StringComparison.Ordinal)
          || compareText.Equals("LF", StringComparison.OrdinalIgnoreCase)
          || compareText.Equals("␊", StringComparison.Ordinal) || compareText.Equals(
            "Line feed",
            StringComparison.OrdinalIgnoreCase))
        return '\n';

      if (compareText.StartsWith("Unit separator", StringComparison.OrdinalIgnoreCase) || compareText.Contains("31")
          || compareText.Equals("␟", StringComparison.Ordinal)
          || compareText.Equals("US", StringComparison.OrdinalIgnoreCase))
        return '\u001F';

      if (compareText.StartsWith("Record separator", StringComparison.OrdinalIgnoreCase) || compareText.Contains("30")
          || compareText.Equals("␞", StringComparison.Ordinal)
          || compareText.Equals("RS", StringComparison.OrdinalIgnoreCase))
        return '\u001E';

      if (compareText.StartsWith("Group separator", StringComparison.OrdinalIgnoreCase) || compareText.Contains("29")
          || compareText.Equals("GS", StringComparison.OrdinalIgnoreCase))
        return '\u001D';

      if (compareText.StartsWith("File separator", StringComparison.OrdinalIgnoreCase) || compareText.Contains("28")
          || compareText.Equals("FS", StringComparison.OrdinalIgnoreCase))
        return '\u001C';

      return compareText[0];
    }

    /// <summary>
    ///   Gets a descriptive text for a char
    /// </summary>
    /// <param name="input">The input string.</param>
    public string Description
    {
      get => m_Char switch
      {
        '\t' => "Horizontal Tab",
        ' ' => "Space",
        (char) 0xA0 => "Non-breaking space",
        '\\' => "Backslash: \\",
        '/' => "Slash: /",
        ',' => "Comma: ,",
        ';' => "Semicolon: ;",
        ':' => "Colon: :",
        '|' => "Pipe: |",
        '\"' => "Quotation marks: \"",
        '\'' => "Apostrophe: \'",
        '&' => "Ampersand: &",
        '*' => "Asterisk: *",
        '`' => "Tick Mark: `",
        '✓' => "Check mark: ✓",
        '\u001F' => "Unit Separator: Char 31",
        '\u001E' => "Record Separator: Char 30",
        '\u001D' => "Group Separator: Char 29",
        '\u001C' => "File Separator: Char 28",
        _ => m_Char.ToString()
      };
    }
  }
}
