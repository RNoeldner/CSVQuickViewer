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

namespace CsvTools
{
  [Flags]
  public enum FilterTypeEnum
  {
    [Description("Display rows that have no error nor warning")]
    [ShortDescription("No error or warning")]
    None = 0,

    [Description("Display rows that have a warning")]
    [ShortDescription("Only warnings")]
    ShowWarning = 1 << 0,

    [Description("Display rows that have an error")]
    [ShortDescription("Only errors")]
    ShowErrors = ShowWarning << 1,

    [Description("A true error is an error that has proper error information, in some cases only a placeholder text is stored as the real message is not known")]
    [ShortDescription("True Errors")]
    OnlyTrueErrors = ShowErrors << 1,

    [Description("Display rows that have an error or a warning")]
    [ShortDescription("Errors or warnings")]
    ErrorsAndWarning = ShowErrors | ShowWarning,

    [Description("Display rows that either an error, a true error or a warning")]
    [ShortDescription("All")]
    All = ShowErrors | ShowWarning | OnlyTrueErrors,
  }
}