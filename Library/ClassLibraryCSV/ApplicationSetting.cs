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
using System.Data;
using System.Diagnostics.Contracts;
using System.Threading;

namespace CsvTools
{
  /// <summary>
  ///  Static class to access application wide settings, currently HTMLStyle, FillGuessSetting and a ColumnHeaderCache
  /// </summary>
  public static class ApplicationSetting
  {
    private static readonly FillGuessSettings m_FillGuessSettings = new FillGuessSettings();
    private static readonly HTMLStyle m_HTMLStyle = new HTMLStyle();

    private static Action<string, string, string, IProcessDisplay, bool> m_RemoteFileHandler =
     delegate (string path, string fileName, string localName, IProcessDisplay processDisplay, bool throwNotFileExists) { return; };

    private static Func<string, CancellationToken, IDataReader> m_SQLDataReader;
    private static IToolSetting m_ToolSetting = new DummyToolSetting();

    /// <summary>
    ///  Sets the cache to store already fetched parent,
    /// </summary>
    /// <value>
    ///  The cache source by destination.
    /// </value>
    /// <remarks>The key is the name of the template table, the value is a list of the combined IDs in that table</remarks>
    public static ICache<string, ICollection<string>> CacheList { get; } = new Cache<string, ICollection<string>>(600);

    public static ICollection<string> CacheSQLResult(string sql, Func<ICollection<string>> uncachedCall)
    {
      if (!ApplicationSetting.CacheList.TryGet(sql, out var parentIDs))
      {
        parentIDs = uncachedCall();
        ApplicationSetting.CacheList.Set(sql, parentIDs);
      }

      return parentIDs;
    }

    public static void FlushSQLResultByTable(string tableName)
    {
      if (string.IsNullOrEmpty(tableName))
        return;
      string key1 = CsvHelper.CacheListKeyColumnHeader(tableName, true);
      string key2 = CsvHelper.CacheListKeyColumnHeader(tableName, false);
      var uncache = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      foreach (string sql in CacheList.Keys)
      {
        if (sql == key1 || sql == key2)
        {
          uncache.Add(sql);
          continue;
        }
        var tables = StringUtilsSQL.GetSQLTableNames(sql);
        foreach (string tab in tables)
        {
          if (tableName.Equals(tab, StringComparison.OrdinalIgnoreCase))
          {
            uncache.Add(sql);
            break;
          }
        }
      }
      foreach (string sql in uncache)
      {
        CacheList.Remove(sql);
      }
    }

    /// <summary>
    ///  FillGuessSettings
    /// </summary>
    public static FillGuessSettings FillGuessSettings
    {
      get
      {
        Contract.Ensures(Contract.Result<FillGuessSettings>() != null);
        return m_FillGuessSettings;
      }
    }

    /// <summary>
    ///  The Application wide HTMLStyle
    /// </summary>
    public static HTMLStyle HTMLStyle
    {
      get
      {
        Contract.Ensures(Contract.Result<HTMLStyle>() != null);
        return m_HTMLStyle;
      }
    }

    /// <summary>
    ///  General Setting that determines if the menu is display in the bottom of a detail control
    /// </summary>
    public static bool MenuDown { get; set; } = false;

    public static Action<string, string, string, IProcessDisplay, bool> RemoteFileHandler { get => m_RemoteFileHandler; set => m_RemoteFileHandler = value; }

    /// <summary>
    /// Gets or sets the SQL data reader.
    /// </summary>
    /// <value>
    /// The SQL data reader.
    /// </value>
    /// <exception cref="ArgumentNullException">SQL Data Reader is not set</exception>
    public static Func<string, CancellationToken, IDataReader> SQLDataReader
    {
      get
      {
        Contract.Ensures(Contract.Result<Func<string, IDataReader>>() != null);
        if (m_SQLDataReader == null)
          throw new ArgumentNullException("SQL Data Reader is not set");
        return m_SQLDataReader;
      }
      set
      {
        Contract.Requires(value != null);
        m_SQLDataReader = value;
      }
    }

    public static IToolSetting ToolSetting
    {
      get
      {
        Contract.Ensures(Contract.Result<IToolSetting>() != null);
        return m_ToolSetting;
      }
      set => m_ToolSetting = value ?? new DummyToolSetting();
    }

    /// <summary>
    ///  Flushes cached items in the all caches
    /// </summary>
    public static void FlushAll()
    {
      CacheList?.Flush();
    }

    /// <summary>
    ///  Get file setting column by template field
    /// </summary>
    /// <param name="fileSetting">The file setting.</param>
    /// <param name="templateFieldName">The template column.</param>
    /// <returns>Null if the template table field is not mapped or the file setting does not have a typed column</returns>
    public static Column GetColumByField(this IFileSetting fileSetting, string templateFieldName)
    {
      Contract.Requires(fileSetting != null && !string.IsNullOrEmpty(templateFieldName));
      var map = fileSetting.GetMappingByField(templateFieldName);
      if (map == null)
        return null;

      return fileSetting.GetColumn(map.FileColumn);
    }

    /// <summary>
    ///  Get DB column by template column
    /// </summary>
    /// <param name="fileSetting">The file setting.</param>
    /// <param name="templateField">The template column.</param>
    /// <returns>Null if the template table field is not mapped</returns>
    public static string GetColumNameByField(this IFileSetting fileSetting, string templateField)
    {
      return fileSetting.GetMappingByField(templateField)?.FileColumn;
    }
  }
}