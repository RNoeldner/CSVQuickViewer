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

using Microsoft.Extensions.Logging;
using System;

namespace CsvTools;

/// <summary>
///   Central point for logging ILogger needs to be set once
/// </summary>
public static class Logger
{
  /// <summary>
  /// Gets or sets the logger instance.
  /// </summary>
  public static ILogger? LoggerInstance
  {
    get;
    set;
  }

  /// <summary>
  /// Logs a debug level message.
  /// </summary>
  /// <param name="message">The message.</param>
  /// <param name="args">The arguments.</param>
  public static void Debug(string? message, params object[] args)
  {
    if (message is null || message.Length == 0)
      return;
    LoggerInstance?.LogDebug(message, args);
  }

  /// <summary>
  /// Logs a error level message.
  /// </summary>
  /// <param name="message">The message.</param>
  /// <param name="args">Message arguments.</param>
  public static void Error(string? message, params object[] args)
  {
    if (message is null || message.Length == 0)
      return;
    LoggerInstance?.LogError(message, args);
  }


  /// <summary>Logs a message on error level.</summary>
  /// <param name="exception">Exception that need to be documented</param>
  /// <param name="message">The message.</param>
  /// <param name="args">Message arguments.</param>
  public static void Error(in Exception exception, string? message = null, params object[] args) =>
    LoggerInstance?.LogError(exception, message ?? exception.ExceptionMessages(2), args);

  /// <summary>Logs a message on information level.</summary>
  /// <param name="message">The message.</param>
  /// <param name="args">Message arguments.</param>
  public static void Information(string? message, params object[] args)
  {
    if (message is null || message.Length == 0)
      return;
    LoggerInstance?.LogInformation(message, args);
  }

  /// <summary>Logs a message on information level.</summary>
  /// <param name="exception">Exception that need to be documented</param>
  /// <param name="message">The message.</param>
  /// <param name="args">Message arguments.</param>
  public static void Information(in Exception exception, string? message, params object[] args)
  {
    if (message is null || message.Length == 0)
      return;
    LoggerInstance?.LogInformation(exception, message, args);
  }

  /// <summary>Logs a warning level message.</summary>
  /// <param name="message">The message.</param>
  /// <param name="args">Message arguments.</param>
  public static void Warning(string? message, params object[] args)
  {
    if (message is null || message.Length == 0)
      return;
    LoggerInstance?.LogWarning(message, args);
  }

  /// <summary>Logs a warning level message.</summary>
  /// <param name="exception">Exception that need to be documented</param>
  /// <param name="message">The message.</param>
  /// <param name="args">Message arguments.</param>
  public static void Warning(in Exception exception, string? message, params object[] args) =>
    LoggerInstance?.LogWarning(exception, message ?? string.Empty, args);
}