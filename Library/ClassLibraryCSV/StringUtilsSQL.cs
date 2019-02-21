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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CsvTools
{
  /// <summary>
  ///   Extensions for string that are assumed to be SQL commands
  /// </summary>
  public static class StringUtilsSQL
  {
    private const string c_SQLKeywords =
      "ADD ALL ALTER AND ANY AS ASC AUTHORIZATION BACKUP BEGIN BETWEEN BREAK BROWSE BULK BY CASCADE CASE CHECK CHECKPOINT CLOSE CLUSTERED COALESCE COLLATE COLUMN COMMIT COMPUTE CONSTRAINT CONTAINS CONTAINSTABLE CONTINUE CONVERT CREATE CROSS CURRENT CURRENT_DATE CURRENT_TIME CURRENT_TIMESTAMP CURRENT_USER CURSOR DATABASE DBCC DEALLOCATE DECLARE DEFAULT DELETE DENY DESC DISK DISTINCT DISTRIBUTED DOUBLE DROP DUMP ELSE END ERRLVL ESCAPE EXCEPT EXEC EXECUTE EXISTS EXIT EXTERNAL FETCH FILE FILLFACTOR FOR FOREIGN FREETEXT FREETEXTTABLE FROM FULL FUNCTION GOTO GRANT GROUP HAVING HOLDLOCK IDENTITY IDENTITYCOL IDENTITY_INSERT IF IN INDEX INNER INSERT INTERSECT INTO IS JOIN KEY KILL LEFT LIKE LINENO LOAD MERGE NATIONAL NOCHECK NONCLUSTERED NOT NULL NULLIF OF OFF OFFSETS ON OPEN OPENDATASOURCE OPENQUERY OPENROWSET OPENXML OPTION OR ORDER OUTER OVER PERCENT PIVOT PLAN PRECISION PRIMARY PRINT PROC PROCEDURE PUBLIC RAISERROR READ READTEXT RECONFIGURE REFERENCES REPLICATION RESTORE RESTRICT RETURN REVERT REVOKE RIGHT ROLLBACK ROWCOUNT ROWGUIDCOL RULE SAVE SCHEMA SECURITYAUDIT SELECT SEMANTICKEYPHRASETABLE SEMANTICSIMILARITYDETAILSTABLE SEMANTICSIMILARITYTABLE SESSION_USER SET SETUSER SHUTDOWN SOME STATISTICS SYSTEM_USER TABLE TABLESAMPLE TEXTSIZE THEN TO TOP TRAN TRANSACTION TRIGGER TRUNCATE TRY_CONVERT TSEQUAL UNION UNIQUE UNPIVOT UPDATE UPDATETEXT USE USER VALUES VARYING VIEW WAITFOR WHEN WHERE WHILE WITH WITHIN GROUP WRITETEXT";

    private static readonly Lazy<Regex> m_PatternGo =
      new Lazy<Regex>(() => new Regex(@"\bGO\b(?!\')", RegexOptions.Compiled));

    // [xxx]
    private static readonly Lazy<Regex> m_PatternTable1 = new Lazy<Regex>(() => new Regex(@"\b(FROM|JOIN)\s+\[(.*)\]",
      RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline));

    // "xxx"
    private static readonly Lazy<Regex> m_PatternTable2 = new Lazy<Regex>(() => new Regex(@"\b(FROM|JOIN)\s+\""(.*)\""",
      RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline));

    /// No Backets
    private static readonly Lazy<Regex> m_PatternTable3 = new Lazy<Regex>(() => new Regex(@"\b(FROM|JOIN)\s+(\w*)\b",
      RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline));

    public static string ConnectionStringEscape(string contents)
    {
      Contract.Ensures(Contract.Result<string>() != null);
      if (string.IsNullOrEmpty(contents))
        return string.Empty;

      if (contents.IndexOf(';') > -1)
      {
        if (contents.StartsWith("\"", StringComparison.Ordinal))
          return "'" + contents + "'";
        else
          return "\"" + contents + "\"";
      }
      if (contents.IndexOf('\'') > -1)
      {
        return "\"" + contents + "\"";
      }
      if (contents.IndexOf('\"') > -1)
      {
        return "'" + contents + "'";
      }
      return contents;
    }

    /// <summary>
    ///   Gets the sql command text without comment.
    /// </summary>
    /// <param name="sqlText">The SQL text.</param>
    /// <param name="lineComment">The line comment.</param>
    /// <param name="blockStart">The block start.</param>
    /// <param name="blockEnd">The block end.</param>
    /// <returns>The SQL will have the same line numbers as before but any comments will be missing</returns>
    public static string GetCommandTextWithoutComment(this string sqlText,
      string lineComment = "--",
      string blockStart = "/*",
      string blockEnd = "*/")
    {
      Contract.Ensures(Contract.Result<string>() != null);

      if (string.IsNullOrEmpty(sqlText))
        return string.Empty;

      var regularCommand = true;
      var inName = false;
      var inText = false;
      var inLineComment = false;
      var inBlockComment = false;
      var sb = new StringBuilder();

      for (var i = 0; i < sqlText.Length; i++)
      {
        var current = sqlText[i];

        // quoted names
        if ((current == ']' || current == '"') && inName)
        {
          regularCommand = true;
          inName = false;
        }
        else if ((current == '[' || current == '"') && regularCommand)
        {
          regularCommand = false;
          inName = true;
        }

        // strings
        if (current == '\'' && inText)
        {
          regularCommand = true;
          inText = false;
        }
        else if (current == '\'' && regularCommand)
        {
          regularCommand = false;
          inText = true;
        }

        // linefeed with special handling for Line comments
        if (current == '\r' || current == '\n')
        {
          var addLineFeed = inBlockComment || inLineComment;

          // a linefeed will stop names and line comments
          if (inName || inLineComment)
          {
            regularCommand = true;
            inLineComment = false;
            inName = false;
          }

          // CRLF or LFCR will all become LF only
          if (i < sqlText.Length - 1 &&
              (current == '\n' && sqlText[i + 1] == '\r' ||
               current == '\r' && sqlText[i + 1] == '\n'))
            i++;
          current = '\n';
          if (addLineFeed)
          {
            sb.Append(current);
            continue;
          }
        }

        // LineComment
        if (i < sqlText.Length - lineComment.Length && sqlText.Substring(i, lineComment.Length) == lineComment &&
            regularCommand)
        {
          current = '\0';
          inLineComment = true;
          regularCommand = false;
        }
        else
        // BlockComment
        if (i < sqlText.Length - blockStart.Length && sqlText.Substring(i, blockStart.Length) == blockStart &&
            regularCommand)
        {
          current = '\0';
          inBlockComment = true;
          regularCommand = false;
          i += blockStart.Length - 1;
        }
        else if (i < sqlText.Length - blockEnd.Length && sqlText.Substring(i, blockEnd.Length) == blockEnd &&
                 inBlockComment)
        {
          current = '\0';
          inBlockComment = false;
          regularCommand = true;
          i += blockEnd.Length - 1;
        }

        if (current != '\0' && !inBlockComment && !inLineComment)
          sb.Append(current);
      }

      return sb.ToString();
    }

    /// <summary>
    ///   Gets the table names used in a command
    /// </summary>
    /// <param name="sqlText">The SQL text without comments</param>
    /// <returns></returns>
    public static IEnumerable<string> GetSQLTableNames(this string sqlText)
    {
      Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

      const string plcaeholder = "\u2028";
      var ret = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      if (string.IsNullOrEmpty(sqlText)) return ret;
      sqlText = sqlText.GetCommandTextWithoutComment();

      foreach (Match match in m_PatternTable1.Value.Matches(sqlText, 0))
        if (!string.IsNullOrEmpty(match.Groups[2].Value))
        {
          var tabName = match.Groups[2].Value.Replace("]]", plcaeholder);
          if (tabName.IndexOf(']') > 0)
            tabName = tabName.Substring(0, tabName.IndexOf(']'));
          ret.Add(tabName.Replace(plcaeholder, "]"));
        }

      foreach (Match match in m_PatternTable2.Value.Matches(sqlText, 0))
        if (!string.IsNullOrEmpty(match.Groups[2].Value))
        {
          var tabName = match.Groups[2].Value.Replace("]]", plcaeholder);
          if (tabName.IndexOf(']') > 0)
            tabName = tabName.Substring(0, tabName.IndexOf(']'));
          ret.Add(tabName.Replace(plcaeholder, "]"));
        }

      foreach (Match match in m_PatternTable3.Value.Matches(sqlText, 0))
        if (!string.IsNullOrEmpty(match.Groups[2].Value))
        {
          var tabName = match.Groups[2].Value.Replace("]]", plcaeholder);
          if (tabName.IndexOf(']') > 0)
            tabName = tabName.Substring(0, tabName.IndexOf(']'));
          ret.Add(tabName.Replace(plcaeholder, "]"));
        }

      return ret;
    }

    public static string RenameSQLTable(this string sql, string oldTableName, string newTableName)
    {
      if (string.IsNullOrEmpty(sql) || sql.IndexOf(oldTableName, StringComparison.OrdinalIgnoreCase) <= 0) return sql;
      // this should cover 80%
      //SQL = SQL.ReplaceCaseInsensitive(string.Format("[{0}]", SqlName(oldTableName)),
      //                                 string.Format("[{0}]", SqlName(newTableName)));
      //SQL = SQL.ReplaceCaseInsensitive(string.Format("\"{0}\"", SqlName(oldTableName)),
      //                                 string.Format("\"{0}\"", SqlName(newTableName)));
      //SQL = SQL.ReplaceCaseInsensitive(string.Format("'{0}'", SqlQuote(oldTableName)),
      //                                 string.Format("'{0}'", SqlQuote(newTableName)));

      var regex1 =
        new Regex(
          @"(?<start>\b(FROM|JOIN)\b\s*)(?<open>\[|\"")(?<tablename>" + Regex.Escape(oldTableName) +
          @")(?<close>\]|\"")", RegexOptions.IgnoreCase | RegexOptions.Multiline);
      sql = regex1.Replace(sql, "${start}${open}" + newTableName + "${close}");

      var regex2 =
        new Regex(
          @"(?<start>\b(FROM|JOIN)\b)(?<open>\s*)(?<tablename>" + Regex.Escape(oldTableName) + @")(?<close>\b)",
          RegexOptions.IgnoreCase | RegexOptions.Multiline);
      sql = regex2.Replace(sql, "${start}${open}" + newTableName + "${close}");

      return sql;
    }

    /// <summary>
    ///   Splits the command text by go and removes all comments
    /// </summary>
    /// <param name="sqlText">The SQL text.</param>
    /// <returns>A list of parts split by GO</returns>
    /// <remarks>
    ///   This is not 100% reliable, a GO in a string literal would split the text, risk reduced by making it Case
    ///   sensitive and not splitting by GO'.
    /// </remarks>
    public static IList<string> SplitCommandTextByGo(this string sqlText)
    {
      Contract.Ensures(Contract.Result<IList<string>>() != null);

      if (string.IsNullOrEmpty(sqlText))
        return new List<string>();

      var statements = new List<string>();
      foreach (var match in m_PatternGo.Value.Split(sqlText.GetCommandTextWithoutComment(), 0))
      {
        if (match == null)
          continue;
        var trimmed = match.Trim();
        if (trimmed.Length == 0)
          continue;
        statements.Add(trimmed);
      }

      return statements;
    }

    /// <summary>
    ///   Checks if a string is a SQL keyword
    /// </summary>
    /// <param name="contents">The contents.</param>
    /// <returns><c>true</c> if the value is a reserved SQL keyword, <c>false</c> otherwise</returns>
    public static bool SqlIsKeyword(string contents)
    {
      if (string.IsNullOrEmpty(contents)) return false;
      foreach (var keyword in c_SQLKeywords.Split(' '))
        if (contents.Equals(keyword, StringComparison.OrdinalIgnoreCase))
          return true;
      return false;
    }

    /// <summary>
    ///   Escapes SQL names; does not include the brackets or quotes
    /// </summary>
    /// <param name="contents">The column or table name.</param>
    /// <returns>The names as it can be placed into brackets</returns>
    public static string SqlName(string contents)
    {
      Contract.Ensures(Contract.Result<string>() != null);
      if (string.IsNullOrEmpty(contents))
        return string.Empty;

      return contents.Replace("]", "]]");
    }

    /// <summary>
    ///   Checks if a name needs quoting
    /// </summary>
    /// <param name="contents">The column or table name.</param>
    /// <returns><c>true</c> the name should be quoted, <c>false</c> otherwise</returns>
    public static bool SqlNameNeedsQuoting(string contents)
    {
      if (string.IsNullOrEmpty(contents))
        return false;

      // Shortcut: If a space preset we need to quote
      if (contents.IndexOf(' ') != -1 || contents.IndexOf('.') != -1)
        return true;

      // May not be keyword
      if (SqlIsKeyword(contents))
        return true;

      // The name must start with a Unicode letter, _, @, or #;
      var oc = CharUnicodeInfo.GetUnicodeCategory(contents[0]);
      if (oc != UnicodeCategory.UppercaseLetter
          && oc != UnicodeCategory.LowercaseLetter
          && oc != UnicodeCategory.OtherLetter
          && contents[0] != '_'
          && contents[0] != '@'
          && contents[0] != '#')
        return true;

      // followed by one or more letters, numbers, @, $, #, or _.
      foreach (var c in contents)
      {
        oc = CharUnicodeInfo.GetUnicodeCategory(c);
        if (oc != UnicodeCategory.UppercaseLetter
            && oc != UnicodeCategory.LowercaseLetter
            && oc != UnicodeCategory.OtherLetter
            && oc != UnicodeCategory.DecimalDigitNumber
            && c != '@'
            && c != '$'
            && c != '#'
            && c != '_')
          return true;
      }

      return false;
    }

    /// <summary>
    ///   Puts a name always in brackets
    /// </summary>
    /// <param name="contents">The column or table name.</param>
    /// <returns>The names in brackets (if needed)</returns>
    public static string SqlNameSafe(string contents)
    {
      Contract.Ensures(Contract.Result<string>() != null);
      return string.IsNullOrEmpty(contents) ? string.Empty : $"[{SqlName(contents)}]";
    }

    /// <summary>
    ///   Puts a name always in brackets
    /// </summary>
    /// <param name="contents">The column or table name.</param>
    /// <returns>The names in brackets (if needed)</returns>
    public static string SqlNameSmart(string contents)
    {
      Contract.Ensures(Contract.Result<string>() != null);
      if (string.IsNullOrEmpty(contents))
      {
        return string.Empty;
      }

      if (SqlNameNeedsQuoting(contents))
      {
        return $"[{SqlName(contents)}]";
      }
      else
      {
        return SqlName(contents);
      }
    }

    /// <summary>
    ///   SQLs the quote, does not include the outer quotes
    /// </summary>
    /// <param name="contents">The contents.</param>
    /// <returns></returns>
    public static string SqlQuote(string contents)
    {
      Contract.Ensures(Contract.Result<string>() != null);
      if (string.IsNullOrEmpty(contents))
        return string.Empty;

      return contents.Replace("'", "''");
    }
  }
}