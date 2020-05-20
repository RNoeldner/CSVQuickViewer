# CSVQuickViewer

This is a Windows application to view delimited text or Json files.
It’s designed to be robust and easy to use.

The application has a MSI installer that does not need administrative right. 

The application can:
* Guess the appropriate Code Page
* Guess the Delimiter, Record Seperator
* Configurable handling of quotes
* Guess the Start Row skipping comment lines
* Use typed values in contracts to text values
* Ability to store the filtered data into delimited text file. 
* Filtering of Columns
* Sorting of Columns
* Hiding/Reordering Columns
* Incremental Searching for Text
* Determine Column Length
* Displaying a Hierarchy inside the file
* Display Duplicates Values
* Display Unique Values
* Filter for rows / Columns with warnings
* Automated re-alignmnet of columns in case a delimiter causes issues with column alignmnet
* Automated re-alignmnet of column in case a linefeed pushes columns into the next line
* HTML Copy and Paste for storing cut values in Excel / Word,  retaining value types

This application does use various NuGet libraies:
* Pri.LongPath (Support for long file names)
* Ude.NetStandard  (Mozilla Universal Charset Detector for dotnet)
* Newtonsoft.Json (Support for Json files)
* Costura.Fody (Embedding dll into executable)
* NLog (Logging Platform)
* WindowsAPICodePack (Ability to use new Windows Vista functionality)
