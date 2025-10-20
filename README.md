# CSVQuickViewer

CSVQuickViewer is a Windows application for viewing delimited text and JSON files. JSON files are automatically flattened for tabular display. The application is designed to be robust, easy to use, and does not require administrative rights for installation.

## Quick Start

1. **Download** the latest version from [SourceForge](https://sourceforge.net/projects/csvquickviewer/files/latest/download).
2. **Install** by running the installer (no admin rights required).
3. **Launch** CSVQuickViewer and open a delimited text or JSON file.
4. **Explore** features such as filtering, sorting, column configuration, and exporting data.
## Features

### File Parsing
- **Automatic Code Page Detection:** Detects the correct encoding for most files. (.NET 7 version supports fewer code pages; rare ones are handled via third-party libraries.)
- **Delimiter & Separator Detection:** Determines field delimiter and record separator automatically.
- **Quoting & Escaping:** Detects quoting characters, escape characters, and comment text. Configurable quote handling, including context-sensitive quoting.
- **Header/Comment Skipping:** Skips header and comment rows automatically.
- **Warnings:** Issues warnings for problematic characters (e.g., non-breaking spaces, unknown placeholders, quotes, delimiters).

### Data Handling
* Supports typed values (switch between typed and text values in UI).  
### Data Handling
- **Typed/Text Values:** Switch between typed and text values in the UI.
- **Date Format Support:** Handles 386 date formats and combines date/time columns with timezone conversion.
- **Compressed Files:** Displays text files, including Zip and GZip formats.
- **Column Realignment:** Auto realignment of columns if delimiter or linefeed issues are detected (configurable).
- **Warnings for Unparseable Content:** Highlights columns with problematic or unparseable data.

### User Interface
* Filter, sort, hide, and reorder columns.  
### User Interface
- **Column Operations:** Filter, sort, hide, and reorder columns. Save/load column configuration.
- **Search & Highlight:** Incremental search and highlighting (note: slower than filtering).
- **Column Insights:** Displays column length, hierarchy, duplicate/unique values, and warnings.
- **HTML Copy/Paste:** Copy/paste to Excel/Word with value types preserved.
- **Export:** Export filtered data to delimited text files.

## Download
## Project Structure

- `Application/` – WinForms UI, main entry point, and user interaction logic.
- `Library/ClassLibraryCSV/` – Core parsing, data transformation, encoding detection, and utility classes.
- `UnitTest/` – Unit tests for core library and UI logic.
- `Setup/` – Installer scripts and portable builds.

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

