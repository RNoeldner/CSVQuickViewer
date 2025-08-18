[Setup]
AppName=Csv Quick Viewer
AppVersion=AppVersion=1.7.14.619
DefaultDirName={localappdata}\CsvQuickViewer
DefaultGroupName=CsvQuickViewer
OutputDir=.\bin
OutputBaseFilename=CsvQuickViewerInstaller
PrivilegesRequiredOverridesAllowed=dialog
PrivilegesRequired=lowest
DisableDirPage=yes    

[Files]
Source: "c:\Share\Repos\CSVQuickViewer\Application\bin\Release\net472\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

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
function IsDotNet472Installed(): Boolean;
var
  ReleaseValue: Cardinal;
begin
   WizardForm.StatusLabel.Caption := 'Checking for .NET Framework 4.7.2 runtime...';
  WizardForm.Update; 
  Result := False;
  if RegQueryDWordValue(HKLM64, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', ReleaseValue) then
  begin
    if ReleaseValue >= 461808 then
      Result := True;
  end;
  
   if Result then
    WizardForm.StatusLabel.Caption := '.NET Framework 4.7.2 runtime detected.'
  else
    WizardForm.StatusLabel.Caption := '.NET Framework 4.7.2 runtime NOT found!';
end;


procedure InitializeWizard();
begin
  if not IsDotNet472Installed() then
  begin
     MsgBox('.NET Framework 4.7.2  runtime is not installed. The application will likely not run.' + #13#10 +
       'There is an additional installed for .NET 8' + #13#10 +
       'Or download it from https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472',
           mbError, MB_OK);
     WizardForm.Close;
    // Abort();  // Stop the installer immediately
  end;
end;


