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

using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace CsvTools
{
  /// <summary>
  ///   Central point for logging ILogger needs to be set once
  /// </summary>
  public static class Logger
  {
    public static ILogger? LoggerInstance
    {
      get;
      set;
    }

    public static void Debug(string? message, params object[] args)
    {
      if (string.IsNullOrEmpty(message))
        return;
      LoggerInstance?.LogDebug(message!, args);
    }

    public static void Error(string? message, params object[] args)
    {
      if (string.IsNullOrEmpty(message))
        return;
      LoggerInstance?.LogError(message!, args);
    }

    public static void Error(Exception exception, string? message = null, params object[] args)
    {
      var ex = exception.Demystify();
      LoggerInstance?.LogError(ex, message!, args);
    }

    public static void Information(string? message, params object[] args)
    {
      if (string.IsNullOrEmpty(message))
        return;
      LoggerInstance?.LogInformation(message!, args);
    }

    public static void Information(Exception exception, string? message, params object[] args)
    {
      if (string.IsNullOrEmpty(message))
        return;
      LoggerInstance?.LogInformation(exception, message!, args);
    }

    public static void Warning(string? message, params object[] args)
    {
      if (string.IsNullOrEmpty(message))
        return;
      LoggerInstance?.LogWarning(message!, args);
    }

    public static void Warning(Exception exception, string? message, params object[] args)
    {
      var ex = exception.Demystify();
      LoggerInstance?.LogWarning(ex, message!, args);
    }
  }
}