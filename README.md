# CSVQuickViewer

This is a Windows application to view delimited text files.
It’s designed to be robust and easy to use.

The application has a MSI installer that does not need administrative right. 

The application can:
* Guess the appropriate Code Page
* Guess the Delimiter
* Guess the Start Row skipping comment lines
* Use typed values in contracts to text values
* Filtering of Columns
* Sorting of Columns
* Hiding/Reordering Columns
* Incremental Searching for Text
* Determine Column Length
* Displaying a Hierarchy inside the file
* Display Duplicates Values
* Display Unique Values
* Filter for rows / Columns with warnings
* Handle PGP encrypted files
* HTML Copy and Paste for storing cut values in Excel / Word,  retaining value types

This application does use various NuGet libraies:
* BouncyCastle.OpenPgp https://www.nuget.org/packages/BouncyCastle.OpenPGP/
* Pri.LongPath  https://www.nuget.org/packages/Pri.LongPath/
* UDE.CSharp https://www.nuget.org/packages/UDE.CSharp/
* Costura.Fody 
* log4net
* NodaTime
