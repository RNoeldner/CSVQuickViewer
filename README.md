# CSVQuickViewer

This is a Windows application to view delimited text or Json files.
It’s designed to be robust and easy to use.

The application has a MSI installer that does not need administrative right. 

The application can:
* Guess the appropriate Code Page
* Guess the Delimiter, Record Separator
* Configurable handling of quotes
* Guess the Start Row skipping comment lines
* Use typed values in contrast to text values (Switiching between typed and text values in UI)
* Ability to store the filtered data into delimited text file
* Filtering of Columns
* Sorting of Columns
* Hiding/Reordering Columns
* Above column configuration can be saved and loaded
* Incremental Searching and highlighting for Text
* Determine Column Length
* Displaying a Hierarchy inside the file
* Display Duplicates Values
* Display Unique Values
* Filter for rows / Columns with warnings
* Automated re-alignment of columns in case a delimiter causes issues with column alignment
* Automated re-alignment of column in case a linefeed pushes columns into the next line
* HTML Copy and Paste for storing cut values in Excel / Word,  retaining value types

This application does use various NuGet libraries:
* Pri.LongPath (Support for long file names)
* FastColoredTextBox (Diplsay of source file with highlighting even if file is very large)
* Ude.NetStandard  (Mozilla Universal Charset Detector for dotnet)
* Newtonsoft.Json (Support for Json files)
* Costura.Fody (Embedding dll into executable)
* NLog (Logging Platform)
* WindowsAPICodePack (Ability to use new Windows Vista functionality)
