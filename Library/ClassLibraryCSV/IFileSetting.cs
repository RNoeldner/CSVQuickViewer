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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace CsvTools
{
  /// <summary>
  ///  Interface for a FileSetting
  /// </summary>
  public interface IFileSetting : INotifyPropertyChanged, ICloneable<IFileSetting>, IEquatable<IFileSetting>
  {
    /// <summary>
    ///  Occurs when a string value property changed providing information on old and new value
    /// </summary>
    event EventHandler<PropertyChangedEventArgs<string>> PropertyChangedString;

#pragma warning disable CA1721 // Property names should not match get methods
                              /// <summary>
                              ///  Gets or sets the column formats
                              /// </summary>
                              /// <value>The column format.</value>
    ObservableCollection<Column> Column { get; }
#pragma warning restore CA1721 // Property names should not match get methods

    /// <summary>
    ///  Gets or sets the consecutive empty rows.
    /// </summary>
    /// <value>The consecutive empty rows.</value>
    int ConsecutiveEmptyRows { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating whether to display line numbers.
    /// </summary>
    /// <value><c>true</c> if line no should be displayed; otherwise, <c>false</c>.</value>
    bool DisplayEndLineNo { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating whether display record numbers
    /// </summary>
    /// <value><c>true</c> if record no should be displayed; otherwise, <c>false</c>.</value>
    bool DisplayRecordNo { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating whether to display line numbers.
    /// </summary>
    /// <value><c>true</c> if line no should be displayed; otherwise, <c>false</c>.</value>
    bool DisplayStartLineNo { get; set; }

    ObservableCollection<SampleRecordEntry> Errors { get; }

    /// <summary>
    ///  Gets or sets the file format.
    /// </summary>
    /// <value>The file format.</value>
    FileFormat FileFormat { get; }

    /// <summary>
    ///  Gets or sets the Last Write Time of the files that has been read for this Setting
    /// </summary>
    /// <value>UTC time of last file write</value>
    DateTime FileLastWriteTimeUtc { get; set; }

    /// <summary>
    ///  Gets or sets the name of the file, this value could be a relative path
    /// </summary>
    /// <value>The name of the file.</value>
    string FileName { get; set; }

    /// <summary>
    ///  The Size of the file in Byte
    /// </summary>
    long FileSize { get; set; }

    /// <summary>
    ///  Gets or sets the Footer.
    /// </summary>
    /// <value>The Footer for outbound data.</value>
    string Footer { get; set; }

    /// <summary>
    ///  Gets the full path of the Filename
    /// </summary>
    /// <value>
    ///  The full path for <see cref="FileName" />
    /// </value>
    string FullPath { get; }

    Func<string> GetEncryptedPassphraseFunction { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating whether this instance has field header.
    /// </summary>
    /// <value><c>true</c> if this instance has field header; otherwise, <c>false</c>.</value>
    bool HasFieldHeader { get; set; }

    /// <summary>
    ///  Gets or sets the Header.
    /// </summary>
    /// <value>The Header for outbound data.</value>
    string Header { get; set; }

    /// <summary>
    ///  Gets or sets the ID.
    /// </summary>
    /// <value>The ID.</value>
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID")]
    string ID { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating whether this setting is critical for the export, meaning the processing will throw an
    ///  error in case of problems.
    ///  You can flag a setting to not be validated but it should show up as critical import step
    /// </summary>
    /// <value><c>true</c> if this file is of higher importance; otherwise, <c>false</c>.</value>
    bool InOverview { get; set; }

    /// <summary>
    ///  The identified to find this specific instance
    /// </summary>
    string InternalID { get; }

    /// <summary>
    ///  Gets or sets a value indicating whether this instance is enabled.
    /// </summary>
    /// <value><c>true</c> if this instance is enabled; otherwise, <c>false</c>.</value>
    bool IsEnabled { get; set; }

    /// <summary>
    ///  Gets or sets the field mapping.
    /// </summary>
    /// <value>The field mapping.</value>
    Collection<Mapping> Mapping { get; }

    Mapping GetMappingByField(string field);

    /// <summary>
    ///  Number of records with errors, -1 to indicate not known
    /// </summary>
    int NumErrors { get; set; }

    /// <summary>
    ///  Self Encrypted Passphrase for PGP decryption
    /// </summary>
    string Passphrase { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating whether the file has been read to completion
    /// </summary>
    /// <value><c>true</c> if line no should be displayed; otherwise, <c>false</c>.</value>
    /// <remarks>
    ///  This is used to determine if the maximum length of each column has been determined yet
    /// </remarks>
    bool ReadToEndOfFile { get; set; }

    /// <summary>
    ///  As the data is loaded and not further validation is done this will be set to true
    ///  Once validation is happening and validation errors are stored this is false again.
    ///
    ///  This is stored on FileSetting level even as it actually is used for determine
    ///  th freshness of a loaded data in the validator, but there is not suitable data structure
    /// </summary>
    bool RecentlyLoaded { get; set; }

    /// <summary>
    ///  Gets the root folder of the Tool Setting
    /// </summary>
    /// <value>
    ///  The root folder.
    /// </value>
    string Recipient { get; set; }

    /// <summary>
    ///  Gets or sets the record limit.
    /// </summary>
    /// <value>The record limit. if set to 0 there is no limit</value>
    uint RecordLimit { get; set; }

    ObservableCollection<SampleRecordEntry> Samples { get; }

    /// <summary>
    ///  Gets or sets a value indicating whether to show progress.
    /// </summary>
    /// <value><c>true</c> if progress should be displayed; otherwise, <c>false</c>.</value>
    bool ShowProgress { get; set; }

    bool SkipDuplicateHeader { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating if the reader will skip empty lines.
    /// </summary>
    /// <value>if <c>true</c> the reader or writer will skip empty lines.</value>
    bool SkipEmptyLines { get; set; }

    /// <summary>
    ///  Gets or sets the skip rows.
    /// </summary>
    /// <value>The skip rows.</value>
    int SkipRows { get; set; }

    ICollection<IFileSetting> SourceFileSettings { get; set; }

    /// <summary>
    ///  Gets or sets the SQL statement.
    /// </summary>
    /// <value>The SQL statement.</value>
    string SqlStatement { get; set; }

    /// <summary>
    ///  Gets or sets the SQL statement.
    /// </summary>
    /// <value>The SQL statement.</value>
    int SQLTimeout { get; set; }

    /// <summary>
    ///  Gets or sets the name of the template.
    /// </summary>
    /// <value>The name of the template.</value>
    string TemplateName { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating whether to treat NBSP as space.
    /// </summary>
    /// <value><c>true</c> if NBSP should be treated as space; otherwise, <c>false</c>.</value>
    bool TreatNBSPAsSpace { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating whether this instance should treat any text listed here as Null
    /// </summary>
    string TreatTextAsNull { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating whether this instance should treat the text "NULL" as Null
    /// </summary>
    /// <value>
    ///  <c>true</c> if any occurrence of "Null" should be treated as a Null; otherwise, <c>false</c>.
    /// </value>
    bool TreatTextNullAsNull { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating of and if training and leading spaces should be trimmed.
    /// </summary>
    /// <value><c>true</c> ; otherwise, <c>false</c>.</value>
    TrimmingOption TrimmingOption { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating whether this instance is imported, and should be validated
    /// </summary>
    /// <remarks>
    ///  Only used in CSV Validation to distinguish between imported files and extracts for reference checks
    /// </remarks>
    /// <value><c>true</c> if this file is imported; otherwise, <c>false</c>.</value>
    bool Validate { get; set; }

    /// <summary>
    ///  Gets or sets the validation result for this instance
    /// </summary>
    /// <value>
    ///  The validation result <see cref="ValidationResult" />
    /// </value>
    ValidationResult ValidationResult { get; set; }

    /// <summary>
    ///  Add or Updates a Field Mapping
    /// </summary>
    /// <param name="mapping">The field mapping.</param>
    /// <returns><c>true</c> if the mapping was added, if updated <c>false</c></returns>
    bool AddMapping(Mapping mapping);

    /// <summary>
    ///  Adds the <see cref="Column" /> format to the column list if it does not exist yet
    /// </summary>
    /// <remarks>If the column name already exist it does nothing but return the already defined column</remarks>
    /// <param name="column">The column format.</param>
    Column ColumnAdd(Column column);

    /// <summary>
    ///  Gets the <see cref="CsvTools.Column" /> with the specified field name.
    /// </summary>
    /// <value></value>
    Column GetColumn(string fieldName);

    /// <summary>
    ///  Gets the <see cref="CsvTools.Mapping" /> with the specified source.
    /// </summary>
    /// <param name="source">Name of the file column</param>
    /// <returns>Return all FieldMapping for a column. There can be multiple</returns>
    IEnumerable<Mapping> GetColumnMapping(string source);

    /// <summary>
    ///  Gets the right data reader for this File Setting
    /// </summary>
    IFileReader GetFileReader();

    /// <summary>
    ///  Gets the right data writer for this File Setting
    /// </summary>
    IFileWriter GetFileWriter(CancellationToken cancellationToken);

    /// <summary>
    ///  Removes a Field Mapping
    /// </summary>
    /// <param name="source">The source column name.</param>
    void RemoveMapping(string source);

    /// <summary>
    ///  Sorts the columns by a set of column names
    /// </summary>
    /// <param name="columnNames">The desired order of column names.</param>
    void SortColumnByName(IEnumerable<string> columnNames);
  }
}