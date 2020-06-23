// /*
// * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com *
// * This program is free software: you can redistribute it and/or modify it under the terms of the
//   GNU Lesser Public
// * License as published by the Free Software Foundation, either version 3 of the License, or (at
//   your option) any later version. *
// * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
//   without even the implied warranty
// * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for
//   more details. *
// * You should have received a copy of the GNU Lesser Public License along with this program.
// * If not, see http://www.gnu.org/licenses/ . *
// */

using System;
using System.Collections.Generic;

namespace CsvTools
{
  public static class ReaderConstants
  {
    /// <summary>
    ///   Field name of the LineNumber Field
    /// </summary>
    public const string cEndLineNumberFieldName = "#LineEnd";

    /// <summary>
    ///   Field name of the Error Field
    /// </summary>
    public const string cErrorField = "#Error";

    /// <summary>
    ///   Field name of the LineNumber Start Field
    /// </summary>
    public const string cPartitionField = "#Partition";

    /// <summary>
    ///   Field Name of the record number
    /// </summary>
    public const string cRecordNumberFieldName = "#Record";

    /// <summary>
    ///   Field name of the LineNumber Start Field
    /// </summary>
    public const string cStartLineNumberFieldName = "#Line";

    /// <summary>
    ///   Collection of the artificial field names
    /// </summary>
    public static readonly ICollection<string> ArtificialFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
      cRecordNumberFieldName,
      cStartLineNumberFieldName,
      cEndLineNumberFieldName,
      cErrorField,
      cPartitionField
    };
  }
}