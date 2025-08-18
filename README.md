# CSVQuickViewer

CSVQuickViewer is a Windows application to view delimited text or JSON files. JSON files are flattened to fit in tabular format. It’s designed to be robust and easy to use.

The application has an installer that does not require administrative rights.

## Features

### File Parsing
* Detects appropriate Code Page (he .NET 7 version does not support as many code pages; currently some are rarely used, but supprted through 3rd party libarraies).  
* Determines field delimiter & record separator.  
* Detects quoting characters, escape characters, and comment text.  
* Skips header/comment rows automatically.  
* Configurable quote handling, including context-sensitive quoting.  
* Issues warnings for problematic characters (non-breaking spaces, unknown text placeholders, quotes, delimiters).

### Data Handling
* Supports typed values (switch between typed and text values in UI).  
* Handles 386 date formats.  
* Displays text files, including compressed formats (Zip, GZip).  
* Issues warnings for unparseable column content.  
* Combines date and time columns with timezone conversion.  
* Auto realignment of columns in case of delimiter or linefeed issues (configurable).

### User Interface
* Filter, sort, hide, and reorder columns.  
* Save and load column configuration.  
* Incremental search and highlighting (slower than filtering).  
* Displays column length, hierarchy, duplicate/unique values, and warnings.  
* HTML copy/paste to Excel/Word, retaining value types.  
* Export filtered data to delimited text files.

## Download
[Download Latest Version from SourceForge](https://sourceforge.net/projects/csvquickviewer/files/latest/download)

## Dependencies
This application uses several NuGet libraries:  
* [Ben.Demystifier](https://github.com/benaadams/Ben.Demystifier) – Improved stack trace display  
* [Serilog](https://serilog.net/) – Logging platform  
* [FastColoredTextBox](https://www.codeproject.com/Articles/161871/Fast-Colored-TextBox-for-syntax-highlighting-2) – Syntax highlighting for source files  
* [UTF.Unknown](https://github.com/CharsetDetector/UTF-unknown) – Detect character sets  
* [Newtonsoft.Json](https://www.newtonsoft.com/json) – JSON parsing and configuration serialization  
* [WindowsAPICodePack](https://github.com/contre/Windows-API-Code-Pack-1.1) – Windows file dialog enhancements  
* [SharpZipLib](https://github.com/icsharpcode/SharpZipLib) – On-the-fly compressed file support  
* [TimeZoneConverter](https://github.com/mattjohnsonpint/TimeZoneConverter) – Unix/Windows timezone support  
* [System.Text.Encoding.CodePages](https://dot.net/) – Support for old code pages in .NET 5+  
* [Microsoft.Extensions.Logging.Abstractions](https://dot.net/) – Logging abstraction interface

