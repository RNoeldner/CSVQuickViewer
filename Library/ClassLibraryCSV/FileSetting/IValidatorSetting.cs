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

#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CsvTools
{
  /// <summary>
  ///  Interface for Settings used in the validator
  /// </summary>
  public interface IValidatorSetting : INotifyPropertyChanged, ICollectionIdentity, IFileSetting
  {
    /// <summary>
    /// Occurs when the identifier is changed, used to handle reference operations
    /// </summary>    
    event EventHandler<PropertyChangedEventArgs<string>>? IdChanged;

    /// <summary>
    ///   Gets or sets a comment text for the setting
    /// </summary>
    string Comment { get; set; }

    /// <summary>
    ///   Gets or sets the number records with errors
    /// </summary>
    long ErrorCount { get; set; }

    /// <summary>
    ///   Gets or sets the ID, on change raise <see cref="IdChanged"/>
    /// </summary>
    /// <value>The ID.</value>
    string ID { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether this setting is critical for the export, meaning
    ///   the processing will throw an error in case of problems. You can flag a setting to not be
    ///   validated, but it should show up as critical import step
    /// </summary>
    /// <value><c>true</c> if this file is of higher importance; otherwise, <c>false</c>.</value>
    bool InOverview { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether this instance is enabled.
    /// </summary>
    /// <value><c>true</c> if this instance is enabled; otherwise, <c>false</c>.</value>
    bool IsEnabled { get; set; }

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
    ///   Gets or sets a value indicating the display order of the setting
    /// </summary>
    int Order { get; set; }

    /// <summary>
    ///   Gets or sets the Last Write Time of the files that has been read for this Setting
    /// </summary>
    /// <value>UTC time of last file write</value>
    DateTime ProcessTimeUtc { get; set; }

    /// <summary>
    ///   As the data is loaded and not further validation is done this will be set to true Once
    ///   validation is happening and validation errors are stored this is false again. This is
    ///   stored on FileSetting level even as it actually is used for determine th freshness of a
    ///   loaded data in the validator, but there is not suitable data structure
    /// </summary>
    bool RecentlyLoaded { get; set; }

    /// <summary>
    ///   Storage for Sample and error records, used in the validator only, TODO: move to other
    ///   library or wait for Extension of Classes
    /// </summary>
    SampleAndErrorsInformation SamplesAndErrors { get; }

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
    IReadOnlyCollection<IValidatorSetting>? SourceFileSettings { get; set; }

    /// <summary>
    /// Gets or sets the SQL statement used in case the Setting does read data from the connected database
    /// This is used as source for writing and to collect data from the database
    /// </summary>
    /// <value>
    /// The SQL statement.
    /// </value>
    string SqlStatement { get; set; }

    /// <summary>
    ///   Status of long running processing on the FileSettings, used to synchronize over independent threads
    /// </summary>
    FileStettingStatus Status { get; set; }

    /// <summary>
    ///   Gets or sets the SQL statement.
    /// </summary>
    /// <value>The SQL statement.</value>
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
    ///   Examine the source and determine LatestSource
    /// </summary>
    void CalculateLatestSourceTime();

    /// <summary>
    /// The show progress
    /// </summary>
    bool ShowProgress { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether set latest source time should be used instead of the current time.
    /// </summary>
    /// <value>
    ///   <c>true</c> if [set latest source time for write]; otherwise, <c>false</c>.
    /// </value>
    bool SetLatestSourceTimeForWrite { get; set; }

    /// <summary>
    ///   Get a description of differences between two file settings, ideally they should be of same type
    /// </summary>
    /// <param name="other"></param>
    /// <returns>List of differences as string</returns>
    IEnumerable<string> GetDifferences(IFileSetting other);
  }
}