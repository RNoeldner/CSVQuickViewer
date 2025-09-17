# Copilot Instructions for CSVQuickViewer

## Project Overview
CSVQuickViewer is a Windows desktop application for viewing and analyzing delimited text and JSON files. It flattens JSON for tabular display and provides robust parsing, data handling, and a feature-rich UI. The codebase is organized into solution folders: `Application/` (WinForms UI), `Library/` (core logic), and `UnitTest/` (tests).

## Architecture & Key Components
- **Application/**: Contains WinForms UI (`FormMain.cs`, `FormEditSettings.cs`) and entry point (`Program.cs`). Handles user interaction, file dialogs, and view logic.
- **Library/ClassLibraryCSV/**: Core parsing, data transformation, and utility classes (e.g., `CsvHelper.cs`, `BiDirectionalDictionary.cs`, `EncodingHelper.cs`). Implements file format detection, quoting, date handling, and warnings.
- **UnitTest/**: Contains test projects for core library and UI logic.
- **Setup/**: Installer scripts and portable builds.

## Developer Workflows
- **Build**: Use Visual Studio or `dotnet build CSVQuickViewer.sln`.
- **Run**: Launch via Visual Studio or run `Application/CSVQuickViewer.exe` after build.
- **Test**: Run unit tests with `dotnet test` in the `UnitTest/` subfolders.
- **Installer**: Build installer from `Setup/` using provided scripts.

## Project-Specific Patterns
- **File Parsing**: Parsing logic is centralized in `Library/ClassLibraryCSV/`. Look for extension methods and helpers for delimiter, quoting, and encoding detection.
- **Warnings & Errors**: Warnings for problematic data are surfaced in the UI and logged via Serilog. See `CsvHelper.cs` and UI forms for handling.
- **Settings**: User settings and column configurations are managed via `ViewSettings.cs` and persisted in the application directory.
- **Data Types**: Typed values and text values are handled distinctly; switching is supported in the UI and core logic.
- **Date Handling**: Extensive date format support is implemented in `DateTimeFormatCollection.cs`.
- **Compressed Files**: Zip/GZip support via SharpZipLib, integrated in file reading helpers.

## External Dependencies
- NuGet packages: Serilog, Ben.Demystifier, FastColoredTextBox, UTF.Unknown, Newtonsoft.Json, WindowsAPICodePack, SharpZipLib, TimeZoneConverter, System.Text.Encoding.CodePages, Microsoft.Extensions.Logging.Abstractions.
- See `README.md` for details and links.

## Conventions & Tips
- **Naming**: UI forms prefixed with `Form`, helpers with `Helper`, settings with `ViewSetting*`.
- **Error Handling**: Use Serilog for logging; exceptions are demystified for clarity.
- **Extensibility**: Add new file formats or parsing logic in `Library/ClassLibraryCSV/`.
- **Testing**: Place new tests in the appropriate `UnitTest/` subfolder.

## Key Files & Directories
- `Application/FormMain.cs`: Main UI logic
- `Library/ClassLibraryCSV/CsvHelper.cs`: Core parsing
- `Library/ClassLibraryCSV/DateTimeFormatCollection.cs`: Date formats
- `Library/ClassLibraryCSV/EncodingHelper.cs`: Encoding detection
- `UnitTest/`: Test projects
- `Setup/`: Installer scripts

---
For questions or unclear patterns, review the `README.md` and source files listed above. If conventions or workflows are missing, ask for clarification or examples from maintainers.
