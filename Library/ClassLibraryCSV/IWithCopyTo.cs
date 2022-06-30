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

namespace CsvTools
{
  /// <summary>
  ///   Interface to show taht a calls supports copy to
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public interface IWithCopyTo<T> : IEquatable<T>, ICloneable
  {
    /// <summary>
    ///   Copy all properties from one instance to another instance
    /// </summary>
    /// <param name="other">Another instance class of the same type</param>
    void CopyTo(T other);
  }
}