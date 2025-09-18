[Setup]
AppName=Csv Quick Viewer
AppVersion=AppVersion=1.7.14.621
DefaultDirName={localappdata}\CsvQuickViewer
DefaultGroupName=CsvQuickViewer
OutputDir=.\bin
OutputBaseFilename=CsvQuickViewerInstaller
PrivilegesRequiredOverridesAllowed=dialog
PrivilegesRequired=lowest
DisableDirPage=yes    

[Files]
Source: "..\..\Application\bin\Release\net8.0-windows\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Registry]
; --- Delimited Text Values  ---
Root: HKCU; Subkey: "Software\Classes\.csv"; ValueType: string; ValueName: ""; ValueData: "CsvQuickViewerDelimitedText"; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\CsvQuickViewerDelimitedText"; ValueType: string; ValueName: ""; ValueData: "Delimited Text File"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\CsvQuickViewerDelimitedText\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\CsvQuickViewer.exe,0"
Root: HKCU; Subkey: "Software\Classes\CsvQuickViewerDelimitedText\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\CsvQuickViewer.exe"" ""%1"""

; --- TAB (alias for TSV) ---
Root: HKCU; Subkey: "Software\Classes\.tab"; ValueType: string; ValueName: ""; ValueData: "CsvQuickViewerDelimitedText"; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\.psv"; ValueType: string; ValueName: ""; ValueData: "CsvQuickViewerDelimitedText"; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\.ssv"; ValueType: string; ValueName: ""; ValueData: "CsvQuickViewerDelimitedText"; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\.dsv"; ValueType: string; ValueName: ""; ValueData: "CsvQuickViewerDelimitedText"; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\.txt"; ValueType: string; ValueName: ""; ValueData: "CsvQuickViewerDelimitedText"; Flags: uninsdeletevalue

[Icons]
Name: "{autoprograms}\CsvQuickViewer"; Filename: "{app}\CsvQuickViewer.exe"
Name: "{autodesktop}\CsvQuickViewer"; Filename: "{app}\CsvQuickViewer.exe"

[Code]
function IsDotNet8Installed(): Boolean;
var
  ResultCode: Integer;
  TempFile: string;
  Lines: TStringList;
  i: Integer;
begin
  WizardForm.StatusLabel.Caption := 'Checking for .NET 8 runtime...';
  WizardForm.Update;
  Result := False;
  TempFile := ExpandConstant('{tmp}\dotnet.txt');
  if Exec('cmd.exe', '/C dotnet --list-runtimes > "' + TempFile + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    Lines := TStringList.Create;
    try
      Lines.LoadFromFile(TempFile);
      for i := 0 to Lines.Count-1 do
        if Pos('Microsoft.NETCore.App 8.', Lines[i]) > 0 then
        begin
          Result := True;
          Break;
        end;
    finally
      Lines.Free;
    end;
  end;
  
   if Result then
    WizardForm.StatusLabel.Caption := '.NET 8 runtime detected.'
  else
    WizardForm.StatusLabel.Caption := '.NET 8 runtime NOT found!';
    
  WizardForm.Update;
end;

procedure InitializeWizard();
begin
  if not IsDotNet8Installed() then
  begin
     MsgBox('.NET 8 runtime is not installed. The application will likely not run.' + #13#10 +
       'It might run on a later version, but this is not guaranteed.' + #13#10 +
       'Please download .NET Desktop Runtime 8 from https://dotnet.microsoft.com/download/dotnet/8.0',
           mbError, MB_OK);
     WizardForm.Close;
    // Abort();  // Stop the installer immediately
  end;
end;
