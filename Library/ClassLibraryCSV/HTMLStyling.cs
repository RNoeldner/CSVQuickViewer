/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
 * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License along with this program.
 * If not, see http://www.gnu.org/licenses/ .
 *
 */

using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;

namespace CsvTools
{
  /// <summary>
  ///   Class with HTML styling, constants and helper function
  /// </summary>
  /// <remarks>
  ///   In theory every aspect is configurable, in practice only the Style needs to be changed
  /// </remarks>
  public class HTMLStyle
  {
    /// <summary>
    ///   Adds a HTML TD cell.
    /// </summary>
    /// <param name="sbHtml">A StringBuilder for the HTML.</param>
    /// <param name="tdTemplate">The table cell template.</param>
    /// <param name="regularText">The regular test for the cell.</param>
    /// <param name="errorText">Additional information displayed underneath.</param>
    /// <param name="addErrorInfo">if set to <c>true</c> add the errorText.</param>
    public static void AddHtmlCell(StringBuilder sbHtml, string tdTemplate, string regularText, string errorText,
      bool addErrorInfo)
    {
      if (sbHtml == null)
        return;
      if (!addErrorInfo || string.IsNullOrEmpty(errorText))
      {
        sbHtml.Append(AddTd(tdTemplate, regularText));
        return;
      }

      var errorsAndWarings = errorText.GetErrorsAndWarings();
      if (string.IsNullOrEmpty(regularText))
      {
        if (errorsAndWarings.Item2.Length == 0 && errorsAndWarings.Item1.Length > 0)
        {
          sbHtml.Append(string.Format(CultureInfo.CurrentCulture, tdTemplate, AddTd(c_Error, errorsAndWarings.Item1)));
          return;
        }

        if (errorsAndWarings.Item2.Length > 0 && errorsAndWarings.Item1.Length == 0)
        {
          sbHtml.Append(string.Format(CultureInfo.CurrentCulture, tdTemplate, AddTd(c_Warning, errorsAndWarings.Item2)));
          return;
        }

        sbHtml.Append(string.Format(CultureInfo.CurrentCulture, tdTemplate, AddTd(c_ErrorWarning, errorsAndWarings.Item1, errorsAndWarings.Item2)));
      }
      else
      {
        if (errorsAndWarings.Item2.Length == 0 && errorsAndWarings.Item1.Length > 0)
        {
          sbHtml.Append(string.Format(CultureInfo.CurrentCulture, tdTemplate, AddTd(c_ValueError, regularText, errorsAndWarings.Item1)));
          return;
        }

        if (errorsAndWarings.Item2.Length > 0 && errorsAndWarings.Item1.Length == 0)
        {
          sbHtml.Append(string.Format(CultureInfo.CurrentCulture, tdTemplate, AddTd(c_ValueWarning, regularText, errorsAndWarings.Item2)));
          return;
        }

        sbHtml.Append(string.Format(CultureInfo.CurrentCulture, tdTemplate,
          AddTd(c_ValueErrorWarning, regularText, errorsAndWarings.Item1, errorsAndWarings.Item2)));
      }
    }

    /// <summary>
    ///   Adds an HTML TD, with the given contend
    /// </summary>
    /// <param name="template">The template.</param>
    /// <param name="contents">The contents.</param>
    /// <returns></returns>
    public static string AddTd(string template, params object[] contents)
    {
      if (template == null || contents == null)
        return null;
      if (string.IsNullOrEmpty(template))
        return string.Empty;
      for (var i = 0; i < contents.Length; i++)
        contents[i] = HtmlEncode(contents[i].ToString()).Replace("�", "<span style=\"color:Red; font-size:larger\">&diams;</span>");

      return string.Format(CultureInfo.CurrentCulture, template, contents);
    }

    /// <summary>
    ///   HTML-encodes a string and returns the encoded string.
    /// </summary>
    /// <param name="text">The text string to encode.</param>
    /// <returns>The HTML-encoded text.</returns>
    /// <remarks>
    ///   Taken from http://www.west-wind.com/weblog/posts/2009/Feb/05/Html-and-Uri-String-Encoding-without-SystemWeb
    /// </remarks>
    public static string HtmlEncode(string text)
    {
      if (text == null)
        return null;

      text = StringUtils.HandleCRLFCombinations(text);

      var sb = new StringBuilder(text.Length);
      var len = text.Length;
      for (var i = 0; i < len; i++)
      {
        switch (text[i])
        {
          case '\n':
            sb.Append("<br style=\"mso-data-placement:same-cell;\">");
            break;

          case '<':
            sb.Append("&lt;");
            break;

          case '>':
            sb.Append("&gt;");
            break;

          case '"':
            sb.Append("&quot;");
            break;

          case '&':
            sb.Append("&amp;");
            break;

          default:
            if (text[i] > 159)
            {
              // decimal numeric entity
              sb.Append("&#");
              sb.Append(((int)text[i]).ToString(CultureInfo.InvariantCulture));
              sb.Append(";");
            }
            else
            {
              sb.Append(text[i]);
            }

            break;
        }
      }

      return sb.ToString();
    }

    /// <summary>
    ///   Does a basic HTML Encoding, handling some special charters like linefeed, &lt;, &gt;, " and &amp;
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns></returns>
    public static string HtmlEncodeShort(string text)
    {
      if (text == null)
        return null;
      text = StringUtils.HandleCRLFCombinations(text);
      var sb = new StringBuilder(text.Length);

      foreach (var oneChar in text)
      {
        switch (oneChar)
        {
          case '\n':
            sb.Append("<br>");
            break;

          case '<':
            sb.Append("&lt;");
            break;

          case '>':
            sb.Append("&gt;");
            break;

          case '"':
            sb.Append("&quot;");
            break;

          case '&':
            sb.Append("&amp;");
            break;

          default:
            sb.Append(oneChar);
            break;
        }
      }

      return sb.ToString();
    }

    /// <summary>
    ///   Get the JSON element / variable name
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns></returns>
    public static string JsonElementName(string text)
    {
      Contract.Ensures(Contract.Result<string>() != null);
      var allowed = StringUtils.ProcessByCategory(text, x => x == UnicodeCategory.TitlecaseLetter ||
                                                             x == UnicodeCategory.LowercaseLetter ||
                                                             x == UnicodeCategory.UppercaseLetter ||
                                                             x == UnicodeCategory.ModifierLetter ||
                                                             x == UnicodeCategory.OtherLetter ||
                                                             x == UnicodeCategory.LetterNumber ||
                                                             x == UnicodeCategory.NonSpacingMark ||
                                                             x == UnicodeCategory.DecimalDigitNumber ||
                                                             x == UnicodeCategory.ConnectorPunctuation);
      if (allowed.Length <= 0) return allowed;
      var oc = CharUnicodeInfo.GetUnicodeCategory(allowed[0]);
      if (oc != UnicodeCategory.TitlecaseLetter
          && oc != UnicodeCategory.LowercaseLetter
          && oc != UnicodeCategory.UppercaseLetter
          && oc != UnicodeCategory.ModifierLetter
          && oc != UnicodeCategory.OtherLetter
          && oc != UnicodeCategory.LetterNumber)
      {
        return "_" + allowed;
      }

      return allowed;
    }

    /// <summary>
    ///   Evaluates all characters in a string and returns a new string,
    ///   properly formatted for JSON compliance and bounded by double-quotes.
    /// </summary>
    /// <param name="text">string to be evaluated</param>
    /// <returns>new string, in JSON-compliant form</returns>
    public static string JsonEncode(string text)
    {
      if (text == null)
        return null;
      var output = new StringBuilder();
      foreach (var c in text)
      {
        if (c == 8) //Backspace
          output.Append("\\b");
        else if (c == 9) //Horizontal tab
          output.Append("\\t");
        else if (c == 10) //Newline
          output.Append("\\n");
        else if (c == 12) //Form feed
          output.Append("\\f");
        else if (c == 13) //Carriage return
          output.Append("\\n");
        else if (c == 34) //Double-quotes (")
          output.Append("\\\"");
        else if (c == 47) //Solidus   (/)
          output.Append("\\/");
        else if (c == 92) //Reverse solidus   (\)
          output.Append("\\\\");
        else if (c > 31)
          output.Append(c);
      }

      return "\"" + output + "\"";
    }

    /// <summary>
    ///   Replace special characters from an HTML text
    /// </summary>
    /// <param name="text">The text possibly containing HTML codes.</param>
    /// <returns>The same text with HTML Tags for linefeed, tab and quote</returns>
    public static string TextToHtmlEncode(string text)
    {
      if (text == null)
        return null;
      if (text.StartsWith("<![CDATA[", StringComparison.OrdinalIgnoreCase) && text.EndsWith("]]>", StringComparison.OrdinalIgnoreCase))
        return text.Substring(9, text.Length - 12);

      return StringUtils.HandleCRLFCombinations(text, "<br>").Replace((char)0xA0, ' ').Replace('\t', ' ')
        .Replace("  ", " ").Replace("  ", " ");
    }

    /// <summary>
    ///   Get the XML element name
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>A valid XML Element Name</returns>
    /// <remarks>
    ///   Element names are case-sensitive, Element names must start with a letter or underscore, Element names cannot
    ///   start with the letters xml(or XML, or Xml, etc), Element names can contain letters, digits, hyphens, underscores, and
    ///   periods, Element names cannot contain spaces
    /// </remarks>
    public static string XmlElementName(string text)
    {
      Contract.Ensures(Contract.Result<string>() != null);
      var allowed = StringUtils.ProcessByCategory(text, x => x == UnicodeCategory.DashPunctuation ||
                                                             x == UnicodeCategory.LowercaseLetter ||
                                                             x == UnicodeCategory.UppercaseLetter ||
                                                             x == UnicodeCategory.DecimalDigitNumber);
      if (allowed.StartsWith("xml", StringComparison.OrdinalIgnoreCase))
        return "_" + allowed;
      if (allowed.Length <= 0) return string.Empty;
      var oc = CharUnicodeInfo.GetUnicodeCategory(allowed[0]);
      if (oc == UnicodeCategory.LowercaseLetter || oc == UnicodeCategory.UppercaseLetter || allowed[0] == '_')
        return allowed;

      return "_" + allowed;
    }

    /// <summary>
    ///   Convert the fragment of HTML into the Clipboards HTML format.
    /// </summary>
    /// <param name="fragment">The HTML to put onto the clipboard. It must be valid HTML!</param>
    /// <returns>A string that can be put onto the clipboard and will be recognized as HTML</returns>
    /// <exception cref="ArgumentException">Parameter can not be empty;fragment</exception>
    /// <remarks>
    ///   The HTML format is found here http://msdn2.microsoft.com/en-us/library/aa767917.aspx
    /// </remarks>
    public string ConvertToHtmlFragment(string fragment)
    {
      Contract.Requires(!string.IsNullOrEmpty(fragment), "fragment");
      Contract.Ensures(Contract.Result<string>() != null);

      // Minimal implementation of HTML clipboard format
      const string source = "http://www.csvquickviewer.com/";

      const string markerBlock =
        "Version:1.0\r\n" +
        "StartHTML:{0,8}\r\n" +
        "EndHTML:{1,8}\r\n" +
        "StartFragment:{2,8}\r\n" +
        "EndFragment:{3,8}\r\n" +
        "StartSelection:{2,8}\r\n" +
        "EndSelection:{3,8}\r\n" +
        "SourceURL:{4}\r\n" +
        "{5}";

      var prefixLength = string.Format(CultureInfo.InvariantCulture, markerBlock, 0, 0, 0, 0, source, "").Length;

      var html = string.Format(CultureInfo.InvariantCulture,
        "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\"><HTML><HEAD>{1}</HEAD><BODY><!--StartFragment-->{0}<!--EndFragment--></BODY></HTML>",
        fragment, m_Style);
      var startFragment = prefixLength + html.IndexOf(fragment, StringComparison.Ordinal);
      var endFragment = startFragment + fragment.Length;

      return string.Format(CultureInfo.InvariantCulture, markerBlock, prefixLength, prefixLength + html.Length,
        startFragment, endFragment, source, html);
    }

    public string TabTableToHTML(string text, bool firstLineHeader, bool addTable)
    {
      var sbHtml = new StringBuilder();
      if (addTable)
        sbHtml.Append(TableOpen);

      var lineNo = 0;
      foreach (var line in text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
      {
        lineNo++;
        if (lineNo == 1 && firstLineHeader)
        {
          sbHtml.Append(TROpenAlt);
          foreach (var column in line.Split(new[] { '\t' }, StringSplitOptions.None))
          {
            sbHtml.Append(HTMLStyle.AddTd(TH, column));
          }
          sbHtml.AppendLine(TRClose);
        }
        else
        {
          sbHtml.Append(TROpen);
          foreach (var column in line.Split(new[] { '\t' }, StringSplitOptions.None))
          {
            sbHtml.Append(HTMLStyle.AddTd(TD, column));
          }
          sbHtml.AppendLine(TRClose);
        }
      }
      if (addTable)
        sbHtml.Append(TableClose);
      return sbHtml.ToString();
    }

    #region Defaults

    private const string c_Br = "<br>";

    private const string c_Error = "<span class='err'>{0}</span>";

    private const string c_ErrorWarning = "<span class='err'>{0}<br></span><span class='war'>{1}</span>";

    private const string c_H1 =
      "<h1 style=\"color:DarkBlue; font-family:'Calibri','Trebuchet MS', Arial, Helvetica, sans-serif; font-size : 14px;\">{0}</h1>";

    private const string c_H2 =
      "<h2 style=\"color:DarkBlue; font-family:'Calibri','Trebuchet MS', Arial, Helvetica, sans-serif; font-size : 13px;\">{0}</h2>";

    private const string c_Style = "<STYLE type=\"text/css\">\r\n" +
                                   "  td { border: 1px solid lightgrey; padding:1px; }\r\n" +
                                   "  td.info { mso-number-format:\\@; background: #f0f8ff; font-weight:bold;}\r\n" +
                                   "  td.inforight { mso-number-format:\\@; text-align:right; background: #f0f8ff; font-weight:bold;}\r\n" +
                                   "  td.value { text-align:right; color:DarkBlue; }\r\n" +
                                   "  td.text { mso-number-format:\\@; color:black; }\r\n" +
                                   "  tr.alt { background: #EEEEEE; }\r\n" +
                                   "  br { mso-data-placement:same-cell; }\r\n" +
                                   "  span { background: #F7F8E0; }\r\n" +
                                   "  span.err { color:#B40404; }\r\n" +
                                   "  span.war { color:#2E64FE; }\r\n" +
                                   "</STYLE>";

    private const string c_TableClose = "</table>";

    private const string c_TableOpen =
      "<table style=\"border-collapse:collapse; padding:2px; font-family:'Calibri','Trebuchet MS', Arial, Helvetica, sans-serif; font-size : 11px;\">\r\n";

    private const string c_Td = "<td class='text'>{0}</td>";
    private const string c_TdEmpty = "<td/>";
    private const string c_TdNonText = "<td class='value'>{0}</td>";
    private const string c_Th = "<td class='info'>{0}</td>";
    private const string c_TrClose = "</tr>\r\n";
    private const string c_TrOpen = "<tr>\r\n";
    private const string c_TrOpenAlt = "<tr class='alt'>\r\n";
    private const string c_ValueError = "{0}<br><span class='err'>{1}</span>";
    private const string c_ValueErrorWarning = "{0}<br><span class='err'>{1}</span><br><span class='war'>{2}</span>";
    private const string c_ValueWarning = "{0}<br><span class='war'>{1}</span>";
    private const string c_Warning = "<span class='war'>{0}</span>";

    #endregion Defaults

    #region Private values

    private string m_Br = c_Br;
    private string m_H1 = c_H1;
    private string m_H2 = c_H2;
    private string m_Style = c_Style;
    private string m_TableClose = c_TableClose;
    private string m_TableOpen = c_TableOpen;
    private string m_Td = c_Td;
    private string m_TdEmpty = c_TdEmpty;
    private string m_TdNonText = c_TdNonText;
    private string m_Th = c_Th;
    private string m_TrClose = c_TrClose;
    private string m_TrOpen = c_TrOpen;
    private string m_TrOpenAlt = c_TrOpenAlt;
    private string m_Warning = c_Warning;

    #endregion Private values

    #region Properties

    /// <summary>
    ///   Gets or sets the TD template.
    /// </summary>
    /// <value>The TD template.</value>
    [DefaultValue(c_Br)]
    [XmlElement]
    public string BR
    {
      get => m_Br;

      set => m_Br = string.IsNullOrEmpty(value) ? c_Br : value;
    }

    /// <summary>
    ///   Gets or sets the table template.
    /// </summary>
    /// <value>The table template.</value>
    [DefaultValue(c_H1)]
    [XmlElement]
    public string H1
    {
      get => m_H1;

      set => m_H1 = string.IsNullOrEmpty(value) ? c_H1 : value;
    }

    /// <summary>
    ///   Gets or sets the table template.
    /// </summary>
    /// <value>The table template.</value>
    [DefaultValue(c_H2)]
    [XmlElement]
    public string H2
    {
      get => m_H2;

      set => m_H2 = string.IsNullOrEmpty(value) ? c_H2 : value;
    }

    /// <summary>
    ///   Set the overall HTML Style Sheet.
    /// </summary>
    [DefaultValue(c_Style)]
    [XmlElement]
    public string Style
    {
      get => m_Style;

      set => m_Style = string.IsNullOrEmpty(value) ? c_Style : value;
    }

    /// <summary>
    ///   Gets or sets the table template.
    /// </summary>
    /// <value>The table template.</value>
    [DefaultValue(c_TableClose)]
    [XmlElement]
    public string TableClose
    {
      get => m_TableClose;

      set => m_TableClose = string.IsNullOrEmpty(value) ? c_TableClose : value;
    }

    /// <summary>
    ///   Gets or sets the table template.
    /// </summary>
    /// <value>The table template.</value>
    [DefaultValue(c_TableOpen)]
    [XmlElement]
    public string TableOpen
    {
      get => m_TableOpen;

      set => m_TableOpen = string.IsNullOrEmpty(value) ? c_TableOpen : value;
    }

    /// <summary>
    ///   Gets or sets the TD template.
    /// </summary>
    /// <value>The TD template.</value>
    [DefaultValue(c_Td)]
    [XmlElement]
    public string TD
    {
      get => m_Td;

      set => m_Td = string.IsNullOrEmpty(value) ? c_Td : value;
    }

    /// <summary>
    ///   Gets or sets the TD template.
    /// </summary>
    /// <value>The TD template.</value>
    [DefaultValue(c_TdEmpty)]
    [XmlElement]
    public string TDEmpty
    {
      get => m_TdEmpty;

      set => m_TdEmpty = string.IsNullOrEmpty(value) ? c_TdEmpty : value;
    }

    /// <summary>
    ///   Gets or sets the TD template.
    /// </summary>
    /// <value>The TD template.</value>
    [DefaultValue(c_TdNonText)]
    [XmlElement]
    public string TDNonText
    {
      get => m_TdNonText;

      set => m_TdNonText = string.IsNullOrEmpty(value) ? c_TdNonText : value;
    }

    /// <summary>
    ///   Gets or sets the TH template.
    /// </summary>
    /// <value>The TH template.</value>
    [DefaultValue(c_Th)]
    [XmlElement]
    public string TH
    {
      get => m_Th;

      set => m_Th = string.IsNullOrEmpty(value) ? c_Th : value;
    }

    /// <summary>
    ///   Gets or sets the TR template.
    /// </summary>
    /// <value>The TR template.</value>
    [DefaultValue(c_TrClose)]
    [XmlElement]
    public string TRClose
    {
      get => m_TrClose;

      set => m_TrClose = string.IsNullOrEmpty(value) ? c_TrClose : value;
    }

    /// <summary>
    ///   Gets or sets the TR template.
    /// </summary>
    /// <value>The TR template.</value>
    [DefaultValue(c_TrOpen)]
    [XmlElement]
    public string TROpen
    {
      get => m_TrOpen;

      set => m_TrOpen = string.IsNullOrEmpty(value) ? c_TrOpen : value;
    }

    /// <summary>
    ///   Gets or sets the TR template alternate.
    /// </summary>
    /// <value>The TR template alternate.</value>
    [DefaultValue(c_TrOpenAlt)]
    [XmlElement]
    public string TROpenAlt
    {
      get => m_TrOpenAlt;

      set => m_TrOpenAlt = string.IsNullOrEmpty(value) ? c_TrOpenAlt : value;
    }

    /// <summary>
    ///   Gets or sets the warning.
    /// </summary>
    /// <value>The HTML warning template</value>
    [DefaultValue(c_Warning)]
    [XmlElement]
    public string Warning
    {
      get => m_Warning;

      set => m_Warning = string.IsNullOrEmpty(value) ? c_Warning : value;
    }

    #endregion Properties
  }
}