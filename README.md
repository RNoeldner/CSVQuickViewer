# CSVQuickViewer

This is a Windows application to view delimited text or Json files. Json files are flattened to fit in tabular format.
It’s designed to be robust and easy to use.

The application has a MSI installer that does not need administrative right. 

The application can:
* Determine the appropriate Code Page (teh .NET 7 version deos not support as mayn code tables, by now some are rarely used)
* Determines the Field Delimiter & Record Separator
* Determine the Quoting Character
* Determine Escape Characters
* Determine Comment Text
* Determine the Start Row skipping comment lines
* Configurable handling of quotes, support exotic quoting like context senitive quoting
* Issues warning for column content that that does contain certain characters like Nonbreaking Space, Unknown Text placeholders, quotes or delimiters, as these chars can cause issues in other text parsers.

* Use typed values in contrast to text values (Switching between typed and text values in UI), it does currently check 386 date formats. 
* Support for basic HTML and Markdown handling
* Display of text file
* Support for Zip, gZip and PGP files (does not properly support display of raw file)
* Issues warning for column content cannot be parsed with the current setting e.g. a date in different format.
* Ability to combine Date and Time columns to a combined Date/Time column with support of time zone conversion, either by third column or fixed value.
* Automated re-alignment of columns in case a delimiter causes issues with column alignment (configurable, default off)
* Automated re-alignment of column in case a linefeed pushes columns into the next line (configurable, default off)

* Filtering of Columns
* Sorting of Columns
* Hiding/Reordering Columns
* Above column configuration can be saved and loaded
* Incremental Searching and highlighting for Text (This feature is rather slow, better use filtering)

* Display of Column Length
* Display of Hierarchy inside the file
* Display Duplicates Values
* Display Unique Values
* Filter for rows / columns with warnings

* HTML Copy and Paste for storing cut values in Excel / Word,  retaining value types

* Ability to store the filtered data into delimited text file

Donwload at: [Sourceforge](https://sourceforge.net/projects/csvquickviewer/files/latest/download)

This application does use various NuGet libraries:
* [Ben.Demystifier](https://github.com/benaadams/Ben.Demystifier): Improved display of stack trace
* [Serilog](https://serilog.net/): Logging Platform
* [FastColoredTextBox](https://www.codeproject.com/Articles/161871/Fast-Colored-TextBox-for-syntax-highlighting-2): Display of source file with highlighting
* [UTF.Unknown](https://github.com/CharsetDetector/UTF-unknown): Detect character set for files
* [Newtonsoft.Json](https://www.newtonsoft.com/json): Support for Json files, and serialisation of configuration
* [WindowsAPICodePack](https://github.com/contre/Windows-API-Code-Pack-1.1): Ability to use new Windows Vista functionality for file dialogs
* [Markdown](https://github.com/hey-red/Markdown): Ability to convert Markdown to HTML Text
* [SharpZipLib](https://github.com/icsharpcode/SharpZipLib): Compression Library, ability open compressed files on the fly
* [TimeZoneConverter](https://github.com/mattjohnsonpint/TimeZoneConverter): Support for Unix and Windows Timezone names
* [System.Text.Encoding.CodePages](https://dot.net/): Support for old code pages in .NET5+ like Windows-1252
* [BouncyCatle.OpenPgP](https://dot.net/): Support for PGP encraypted files reading (needs priivate kley and passphrase) and writing (needs public key of recipient)
* [Microsoft.Extensions.Logging.Abstractions](https://dot.net/): Interface for abstraction of logging
