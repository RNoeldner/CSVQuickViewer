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
using System.ComponentModel;

namespace CsvTools
{
  /// <inheritdoc cref="System.ComponentModel.INotifyPropertyChanged" />
  /// <summary>
  ///   Interface for a FileSetting
  /// </summary>
  public interface IFileSetting : INotifyPropertyChanged, ICloneable, IEquatable<IFileSetting>
  {
    /// <summary>
    ///   Gets or sets the column formats
    /// </summary>
    /// <value>The column format.</value>
    ColumnCollection ColumnCollection { get; }

    /// <summary>
    ///   Gets or sets the consecutive empty rows.
    /// </summary>
    /// <value>The consecutive empty rows.</value>
    int ConsecutiveEmptyRows { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether to display line numbers.
    /// </summary>
    /// <value><c>true</c> if line no should be displayed; otherwise, <c>false</c>.</value>
    bool DisplayEndLineNo { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether display record numbers
    /// </summary>
    /// <value><c>true</c> if record no should be displayed; otherwise, <c>false</c>.</value>
    bool DisplayRecordNo { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether to display line numbers.
    /// </summary>
    /// <value><c>true</c> if line no should be displayed; otherwise, <c>false</c>.</value>
    bool DisplayStartLineNo { get; set; }

    /// <summary>
    ///   Gets or sets the number records with errors
    /// </summary>
    long ErrorCount { get; set; }

    /// <summary>
    ///   Gets or sets the Footer.
    /// </summary>
    /// <value>The Footer for outbound data.</value>
    string Footer { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether this instance has field header.
    /// </summary>
    /// <value><c>true</c> if this instance has field header; otherwise, <c>false</c>.</value>
    bool HasFieldHeader { get; set; }

    /// <summary>
    ///   Gets or sets the Header.
    /// </summary>
    /// <value>The Header for outbound data.</value>
    string Header { get; set; }

    /// <summary>
    ///   Gets or sets the ID.
    /// </summary>
    /// <value>The ID.</value>
    string ID { get; set; }

    /// <summary>
    ///   Gets the time of the last chnage in the setting, this is not used for equality but its copied over.
    /// </summary>
    /// <value>Time of last chnage in UTC</value>
    DateTime LastChange { get;  }

    /// <summary>
    ///   Gets or sets a value indicating whether this setting is critical for the export, meaning
    ///   the processing will throw an error in case of problems. You can flag a setting to not be
    ///   validated but it should show up as critical import step
    /// </summary>
    /// <value><c>true</c> if this file is of higher importance; otherwise, <c>false</c>.</value>
    bool InOverview { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating the display order of the setting 
    /// </summary>
    int Order { get; set; }

    /// <summary>
    ///   Gets or sets a commnet text for the setting 
    /// </summary>
    string Comment { get; set; }

    /// <summary>
    ///   The identified to find this specific instance
    /// </summary>
    string InternalID { get; }

    /// <summary>
    ///   Gets or sets a value indicating whether this instance is enabled.
    /// </summary>
    /// <value><c>true</c> if this instance is enabled; otherwise, <c>false</c>.</value>
    bool IsEnabled { get; set; }

    /// <summary>    
    /// When a file is encrypted the not encrypted version temporay file is removed 
    /// When data is sent into a steam the data can not be access
    /// Set to <c>true</c> a readable file is not removed / is created    
    /// </summary>
    bool KeepUnencrypted { get; set; }

    /// <summary>
    ///   The latest value of possible sources, e.G. the file time from the sources in a SQL, As
    ///   calculating might be time consuming, use CalculateLatestSource to rebuild the value
    /// </summary>
    DateTime LatestSourceTimeUtc { get; set; }

    /// <summary>
    ///   Gets or sets the field mapping.
    /// </summary>
    /// <value>The field mapping.</value>
    MappingCollection MappingCollection { get; }

    /// <summary>
    ///   Gets or sets the number records that have been processed
    /// </summary>
    /// <value>The number of processed records.</value>
    long NumRecords { get; set; }

    /// <summary>
    ///   Gets or sets the Last Write Time of the files that has been read for this Setting
    /// </summary>
    /// <value>UTC time of last file write</value>
    DateTime ProcessTimeUtc { get; set; }

    /// <summary>
    ///   As the data is loaded and not further validation is done this will be set to true Once
    ///   validation is happening and validation errors are stored this is false again.
    ///   This is stored on FileSetting level even as it actually is used for determine th freshness
    ///   of a loaded data in the validator, but there is not suitable data structure
    /// </summary>
    bool RecentlyLoaded { get; set; }

    /// <summary>
    ///   Gets or sets the record limit.
    /// </summary>
    /// <value>The record limit. if set to 0 there is no limit</value>
    long RecordLimit { get; set; }

    /// <summary>
    ///   Storage for Sample and error records, used in the validator only, TODO: move to other library or wait for Extension
    ///   of Classes
    /// </summary>
    SampleAndErrorsInformation SamplesAndErrors { get; }

    /// <summary>
    ///   Gets or sets a value indicating whether to show progress.
    /// </summary>
    /// <value><c>true</c> if progress should be displayed; otherwise, <c>false</c>.</value>
    bool ShowProgress { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether to ignore rows that match the header row, this
    ///   happens if two delimited files are appended without removing the header of the appended file
    /// </summary>
    bool SkipDuplicateHeader { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating if the reader will skip empty lines.
    /// </summary>
    /// <value>if <c>true</c> the reader or writer will skip empty lines.</value>
    bool SkipEmptyLines { get; set; }

    /// <summary>
    ///   Gets or sets the skip rows.
    /// </summary>
    /// <value>The skip rows.</value>
    int SkipRows { get; set; }

    /// <summary>
    ///   Storage for the settings used as direct or indirect sources.
    /// </summary>
    /// <remarks>
    ///   This is used for queries that might refer to data that is produced by other settings but
    ///   not for file setting pointing to a specific physical file
    /// </remarks>
    /// <example>
    ///   A setting A using setting B that is dependent on C1 and C2 both dependent on D-&gt; A is
    ///   {B,C1,C2,D}. B is {C1,C2,D}, C1 is {D} C2 is {D}
    /// </example>
    IReadOnlyCollection<IFileSetting>? SourceFileSettings { get; set; }

    /// <summary>
    ///   Gets or sets the SQL statement.
    /// </summary>
    /// <value>The SQL statement.</value>

    string SqlStatement { get; set; }

    /// <summary>
    ///   Gets or sets the name of the template.
    /// </summary>
    /// <value>The name of the template.</value>
    string TemplateName { get; set; }

    /// <summary>
    ///   Gets or sets the timeout value mainly used in Web or SQL Calls.
    /// </summary>
    /// <value>The timeout in seconds.</value>
    int Timeout { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether to treat NBSP as space.
    /// </summary>
    /// <value><c>true</c> if NBSP should be treated as space; otherwise, <c>false</c>.</value>
    bool TreatNBSPAsSpace { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether this instance should treat any text listed here as Null
    /// </summary>
    string TreatTextAsNull { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating of and if training and leading spaces should be trimmed.
    /// </summary>
    TrimmingOption TrimmingOption { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether this instance is imported, and should be validated
    /// </summary>
    /// <remarks>
    ///   Only used in CSV Validation to distinguish between imported files and extracts for
    ///   reference checks
    /// </remarks>
    /// <value><c>true</c> if this file is imported; otherwise, <c>false</c>.</value>
    bool Validate { get; set; }

    /// <summary>
    ///   Gets or sets the number records with warnings
    /// </summary>
    long WarningCount { get; set; }

    /// <summary>
    ///   Occurs when a string value property changed providing information on old and new value
    /// </summary>
    event EventHandler<PropertyChangedEventArgs<string>>? PropertyChangedString;

    /// <summary>
    ///   Examine the source and determine LatestSource
    /// </summary>
    void CalculateLatestSourceTime();

    /// <summary>
    /// Copy settings between two file settings
    /// </summary>
    /// <param name="other"></param>
    void CopyTo(IFileSetting other);


    /// <summary>
    /// Get a description of diffreences between two file settings, idelaly they should be of same type
    /// </summary>
    /// <param name="other"></param>
    /// <returns>List of diffrences as string</returns>
    IEnumerable<string> GetDifferences(IFileSetting other);
  }
}