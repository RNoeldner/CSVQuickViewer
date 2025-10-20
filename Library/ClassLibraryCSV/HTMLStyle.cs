/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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
#nullable enable

using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Web;

namespace CsvTools
{
  /// <summary>
  ///   Class with HTML styling, constants and helper function
  /// </summary>
  /// <remarks>
  ///   In theory every aspect is configurable, in practice only the Style needs to be changed
  /// </remarks>
  public sealed class HtmlStyle
  {
    /// <summary>Default HtmlStyle</summary>
    public static readonly HtmlStyle Default = new HtmlStyle(cDefaultStyle);

    private string m_Style;
    private const string cSecColor = "DarkBlue";
    private const string cBackCol = "#c8c8c8";

    private const string cDefaultStyle = "<STYLE type=\"text/css\">"
                                         + "html * {font-family:'Calibri','Trebuchet MS',Arial,Helvetica,sans-serif}\n"
                                         + "h1 {color:" + cSecColor + ";font-size:14px;font-weight:bold}\n"
                                         + "h2 {color:" + cSecColor + ";font-size:13px;font-weight:bold}\n"
                                         + "table {border-collapse:collapse;font-size:11px}\n"
                                         + "td {border:1px solid lightgrey;padding:2px}\n"
                                         + "td.info {mso-number-format:\\@;background:" + cBackCol +
                                         ";font-weight:bold}\n"
                                         + "td.inforight {mso-number-format:\\@;text-align:right;background:" +
                                         cBackCol + ";font-weight:bold}\n"
                                         + "td.value {text-align:right;color:" + cSecColor + "}\n"
                                         + "td.text {mso-number-format:\\@}\n"
                                         + "tr.alt {background:#eeeeee}\n"
                                         + "br {mso-data-placement:same-cell}\n"
                                         + "span {background:#f7f8e0}\n"
                                         + "span.err {color:#b40404}\n"
                                         + "span.war {color:#2e64fe}\n"
                                         + "</STYLE>";

    /// <summary>
    ///   Initializes a new instance of the <see cref="HtmlStyle" /> class.
    /// </summary>
    /// <param name="style">The style.</param>
    [JsonConstructor]
    public HtmlStyle(string? style)
    {
      m_Style = style ?? cDefaultStyle;
    }

    /// <summary>
    ///   Set the overall HTML Style Sheet.
    /// </summary>
    [DefaultValue(cDefaultStyle)]
    public string Style
    {
      get => m_Style;
      set => m_Style = value;
    }

    /// <summary>
    ///   Gets or sets the TD template.
    /// </summary>
    /// <value>The TD template.</value>
    public static string Br => "<br>";

    /// <summary>
    ///   Gets or sets the warning.
    /// </summary>
    /// <value>The HTML warning template</value>
    public static string Error => "<span class='err'>{0}</span>";

    /// <summary>
    ///   Gets or sets the warning.
    /// </summary>
    /// <value>The HTML warning template</value>
    public static string ErrorWarning => "<span class='err'>{0}</span>";

    /// <summary>
    ///   Gets or sets the table template.
    /// </summary>
    /// <value>The table template.</value>
    public static string H1 => "<h1>{0}</h1>";

    /// <summary>
    ///   Gets or sets the table template.
    /// </summary>
    /// <value>The table template.</value>
    public static string H2 => "<h2>{0}</h2>";

    /// <summary>
    ///   Gets or sets the table template.
    /// </summary>
    /// <value>The table template.</value>
    public static string TableClose => "</table>";

    /// <summary>
    ///   Gets or sets the table template.
    /// </summary>
    /// <value>The table template.</value>
    public static string TableOpen => "<table>\r\n";

    /// <summary>
    ///   Gets or sets the TD template.
    /// </summary>
    /// <value>The TD template.</value>
    public static string Td => "<td class='text'>{0}</td>";


    /// <summary>
    ///   Gets or sets the TD template.
    /// </summary>
    /// <value>The TD template.</value>
    public static string TdEmpty => "<td/>";


    /// <summary>
    ///   Gets or sets the TD template.
    /// </summary>
    /// <value>The TD template.</value>
    public static string TdNonText => "<td class='value'>{0}</td>";


    /// <summary>
    ///   Gets or sets the TH template.
    /// </summary>
    /// <value>The TH template.</value>
    public static string Th => "<td class='info'>{0}</td>";

    /// <summary>
    ///   Gets or sets the TR template.
    /// </summary>
    /// <value>The TR template.</value>
    public static string TrClose => "</tr>\r\n";

    /// <summary>
    ///   Gets or sets the TR template.
    /// </summary>
    /// <value>The TR template.</value>
    public static string TrOpen => "<tr>\r\n";

    /// <summary>
    ///   Gets or sets the TR template alternate.
    /// </summary>
    /// <value>The TR template alternate.</value>
    public static string TrOpenAlt => "<tr class='alt'>\r\n";

    /// <summary>
    /// Gets or sets the HTML template for showing errors
    /// </summary>
    public static string ValueError => "{0}<br><span class='err'>{1}</span>";

    /// <summary>
    /// Gets or sets the HTML template for showing error and warnings
    /// </summary>
    public static string ValueErrorWarning => "{0}<br><span class='err'>{1}</span><br><span class='war'>{2}</span>";

    /// <summary>
    /// Gets or sets the HTML template for showing warning
    /// </summary>
    public static string ValueWarning => "{0}<br><span class='war'>{1}</span>";

    /// <summary>
    ///   Gets or sets the warning.
    /// </summary>
    /// <value>The HTML warning template</value>
    public static string Warning => "<span class='war'>{0}</span>";

    /// <summary>
    ///   Adds an HTML TD, with the given contend
    /// </summary>
    /// <param name="template">The template.</param>
    /// <param name="contents">The contents.</param>
    /// <returns></returns>
    public static string AddTd(string? template,
      params object?[]? contents)
    {
      if (template is null || contents is null || template.Length == 0)
        return string.Empty;

      for (var i = 0; i < contents.Length; i++)
        if (contents[i] != null)
          contents[i] = HtmlEncode(contents[i]?.ToString() ?? String.Empty).Replace(
            "�",
            "<span style=\"color:Red; font-size:larger\">&diams;</span>");

      return string.Format(CultureInfo.CurrentCulture, template, contents);
    }

    /// <summary>
    ///   HTML-encodes a string and returns the encoded string.
    /// </summary>
    /// <param name="text">The text string to encode.</param>
    /// <returns>The HTML-encoded text.</returns>
    /// <remarks>Taken from http://www.west-wind.com/weblog/posts/2009/Feb/05/Html-and-Uri-String-Encoding-without-SystemWeb</remarks>
    public static string HtmlEncode(string text)
    {
      text = text.HandleCrlfCombinations();

      var sb = new StringBuilder(text.Length);
      var len = text.Length;
      for (var i = 0; i < len; i++)
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
              sb.Append(((int) text[i]).ToString(CultureInfo.InvariantCulture));
              sb.Append(';');
            }
            else
            {
              sb.Append(text[i]);
            }

            break;
        }

      return sb.ToString();
    }

    /// <summary>
    ///   Does a basic HTML Encoding, handling some special charters like linefeed, &lt;, &gt;, "
    ///   and &amp;
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns></returns>
    public static string? HtmlEncodeShort(string? text)
    {
      if (text is null)
        return null;
      text = text.HandleCrlfCombinations();
      var sb = new StringBuilder(text.Length);

      foreach (var oneChar in text)
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

      return sb.ToString();
    }

    /// <summary>
    ///   Get the JSON element / variable name
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns></returns>
    public static string JsonElementName(string text)
    {
      var allowed = text.ProcessByCategory(
        x => x == UnicodeCategory.TitlecaseLetter || x == UnicodeCategory.LowercaseLetter
                                                  || x == UnicodeCategory.UppercaseLetter
                                                  || x == UnicodeCategory.ModifierLetter
                                                  || x == UnicodeCategory.OtherLetter
                                                  || x == UnicodeCategory.LetterNumber
                                                  || x == UnicodeCategory.NonSpacingMark
                                                  || x == UnicodeCategory.DecimalDigitNumber
                                                  || x == UnicodeCategory.ConnectorPunctuation);
      if (allowed.Length <= 0)
        return allowed;
      var oc = CharUnicodeInfo.GetUnicodeCategory(allowed[0]);
      if (oc != UnicodeCategory.TitlecaseLetter && oc != UnicodeCategory.LowercaseLetter
                                                && oc != UnicodeCategory.UppercaseLetter
                                                && oc != UnicodeCategory.ModifierLetter
                                                && oc != UnicodeCategory.OtherLetter
                                                && oc != UnicodeCategory.LetterNumber)
        return "_" + allowed;

      return allowed;
    }

    /// <summary>
    /// Get a valid HTML document string builder that stats with common HTML tags
    /// </summary>
    /// <param name="hexColor">Background color in hex</param>
    /// <returns></returns>
    public StringBuilder StartHtmlDoc(string hexColor = "")
    {
      var text = new StringBuilder(500);
      text.AppendLine("<!DOCTYPE HTML public virtual \"-//W3C//DTD HTML 4.0 Transitional//EN\">");
      text.AppendLine(string.IsNullOrEmpty(hexColor) ? "<HTML>" : $"<HTML style=\"background-color: #{hexColor}\">");
      text.AppendLine("<HEAD>");
      text.AppendLine(Style);
      text.AppendLine("</HEAD>");
      text.AppendLine("<BODY>");
      return text;
    }

    /// <summary>
    ///   Replace special characters from an HTML text
    /// </summary>
    /// <param name="text">The text possibly containing HTML codes.</param>
    /// <returns>The same text with HTML Tags for linefeed, tab and quote</returns>
    public static string TextToHtmlEncode(string text)
    {
      if (text is null) throw new ArgumentNullException(nameof(text));

      if (text.StartsWith("<![CDATA[", StringComparison.OrdinalIgnoreCase)
          && text.EndsWith("]]>", StringComparison.OrdinalIgnoreCase))
        return text.Substring(9, text.Length - 12);
      return text.HandleCrlfCombinations("<br>").Replace('\t', ' ').Replace("  ", " ").Replace("  ", " ");
    }

    /// <summary>
    /// Resolve CDATA and d HTML encoded text to a normal string.
    /// </summary>
    /// <param name="text"></param>
    /// <returns>The text without HTML Encoding</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static string HtmlDecode(string text)
    {
      if (text is null) throw new ArgumentNullException(nameof(text));

      if (text.StartsWith("<![CDATA[", StringComparison.OrdinalIgnoreCase)
          && text.EndsWith("]]>", StringComparison.OrdinalIgnoreCase))
        return text.Substring(9, text.Length - 12);
      return HttpUtility.HtmlDecode(text);
    }

    /// <summary>
    ///   Get the XML element name
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>A valid XML Element Name</returns>
    /// <remarks>
    ///   Element names are case-sensitive, Element names must start with a letter or underscore,
    ///   Element names cannot start with the letters xml(or XML, or Xml, etc), Element names can
    ///   contain letters, digits, hyphens, underscores, and periods, Element names cannot contain spaces
    /// </remarks>
    public static string XmlElementName(string text)
    {
      var allowed = text.ProcessByCategory(
        x => x == UnicodeCategory.DashPunctuation || x == UnicodeCategory.LowercaseLetter
                                                  || x == UnicodeCategory.UppercaseLetter
                                                  || x == UnicodeCategory.DecimalDigitNumber);
      if (allowed.StartsWith("xml", StringComparison.OrdinalIgnoreCase))
        return "_" + allowed;
      if (allowed.Length <= 0)
        return string.Empty;
      var oc = CharUnicodeInfo.GetUnicodeCategory(allowed[0]);
      if (oc == UnicodeCategory.LowercaseLetter || oc == UnicodeCategory.UppercaseLetter || allowed[0] == '_')
        return allowed;

      return "_" + allowed;
    }

    /// <summary>
    ///   Adds a HTML TD cell.
    /// </summary>
    /// <param name="sbHtml">A StringBuilder for the HTML.</param>
    /// <param name="tdTemplate">The table cell template.</param>
    /// <param name="regularText">The regular test for the cell.</param>
    /// <param name="errorText">Additional information displayed underneath.</param>
    /// <param name="addErrorInfo">if set to <c>true</c> add the errorText.</param>
    public static void AddHtmlCell(
      StringBuilder sbHtml,
      string tdTemplate,
      string regularText,
      string errorText,
      bool addErrorInfo)
    {
      if (!addErrorInfo || string.IsNullOrEmpty(errorText))
      {
        sbHtml.Append(AddTd(tdTemplate, regularText));
        return;
      }

      var errorsAndWarnings = errorText.GetErrorsAndWarnings();
      if (string.IsNullOrEmpty(regularText))
      {
        if (errorsAndWarnings.Message.Length == 0 && errorsAndWarnings.Column.Length > 0)
        {
          sbHtml.Append(string.Format(CultureInfo.CurrentCulture, tdTemplate,
            AddTd(Error, errorsAndWarnings.Column)));
          return;
        }

        if (errorsAndWarnings.Message.Length > 0 && errorsAndWarnings.Column.Length == 0)
        {
          sbHtml.Append(
            string.Format(CultureInfo.CurrentCulture, tdTemplate,
              AddTd(Warning, errorsAndWarnings.Message)));
          return;
        }

        sbHtml.Append(
          string.Format(
            CultureInfo.CurrentCulture,
            tdTemplate,
            AddTd(ErrorWarning, errorsAndWarnings.Column, errorsAndWarnings.Message)));
      }
      else
      {
        if (errorsAndWarnings.Message.Length == 0 && errorsAndWarnings.Column.Length > 0)
        {
          sbHtml.Append(
            string.Format(
              CultureInfo.CurrentCulture,
              tdTemplate,
              AddTd(ValueError, regularText, errorsAndWarnings.Column)));
          return;
        }

        if (errorsAndWarnings.Message.Length > 0 && errorsAndWarnings.Column.Length == 0)
        {
          sbHtml.Append(
            string.Format(
              CultureInfo.CurrentCulture,
              tdTemplate,
              AddTd(ValueWarning, regularText, errorsAndWarnings.Message)));
          return;
        }

        sbHtml.Append(
          string.Format(
            CultureInfo.CurrentCulture,
            tdTemplate,
            AddTd(ValueErrorWarning, regularText, errorsAndWarnings.Column, errorsAndWarnings.Message)));
      }
    }

    /// <summary>
    ///   Convert the fragment of HTML into the Clipboards HTML format.
    /// </summary>
    /// <param name="fragment">The HTML to put onto the clipboard. It must be valid HTML!</param>
    /// <returns>A string that can be put onto the clipboard and will be recognized as HTML</returns>
    /// <exception cref="ArgumentException">Parameter can not be empty;fragment</exception>
    /// <remarks>The HTML format is found here http://msdn2.microsoft.com/en-us/library/aa767917.aspx</remarks>
    public string ConvertToHtmlFragment(string fragment)
    {
      // return $"<HTML><HEAD>{Style}</HEAD><BODY>{fragment}</BODY></HTML>";
      const string markerBlock =
        "Version:1.0\r\nStartHTML:{0,8}\r\nEndHTML:{1,8}\r\nStartFragment:{2,8}\r\nEndFragment:{3,8}\r\nStartSelection:{2,8}\r\nEndSelection:{3,8}\r\n{4}";

      var prefixLength = string.Format(CultureInfo.InvariantCulture, markerBlock, 0, 0, 0, 0, "")
        .Length;

      var html = string.Format(
        CultureInfo.InvariantCulture,
        "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\"><HTML><HEAD>{1}</HEAD><BODY><!--StartFragment-->{0}<!--EndFragment--></BODY></HTML>",
        fragment,
        Style);
      var startFragment = prefixLength + html.IndexOf(fragment, StringComparison.Ordinal);
      var endFragment = startFragment + fragment.Length;

      return string.Format(
        CultureInfo.InvariantCulture,
        markerBlock,
        prefixLength,
        prefixLength + html.Length,
        startFragment,
        endFragment,
        html);
    }
  }
}
