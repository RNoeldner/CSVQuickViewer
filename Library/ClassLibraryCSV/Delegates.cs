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

using System.Threading;

namespace CsvTools
{
  /// <summary>
  ///   A function to check and address that used tables might not be current and need to be reread
  /// </summary>
  /// <param name="fileSetting">The file setting.</param>
  /// <param name="processDisplay">The process display.</param>
  public delegate void FileSettingChecker(IFileSetting fileSetting, IProcessDisplay processDisplay);

  /// <summary>
  ///   Gets the validation result but do not use the cache
  /// </summary>
  /// <param name="tableName">The name of the local table</param>
  /// <param name="cancellationToken">A CancellationToken to stop processing</param>
  /// <returns></returns>
  public delegate ValidationResult GetValidationResultNoCache(string tableName, CancellationToken cancellationToken);
}