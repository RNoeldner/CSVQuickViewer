#define MyAppTitle "Csv Quick Viewer"
#define MyAppName "CsvQuickViewer"
#define MyOutputDir ".\bin\Release\net472"
#define MyAppVersion (FileExists(MyOutputDir + "\\" +  MyAppName + ".exe") ? GetVersionNumbersString(MyOutputDir + "\\" +  MyAppName + ".exe") : "1.0.0.0")

[Setup]
AppVersion={#MyAppVersion}
OutputBaseFilename={#MyAppName}Installer.Net472
DefaultDirName={localappdata}\{#MyAppName}
AppName={#MyAppName}
OutputDir=..\Setup
CloseApplications=yes
PrivilegesRequiredOverridesAllowed=dialog
PrivilegesRequired=lowest
DisableDirPage=no
DisableProgramGroupPage=yes
SetupIconFile=csv_text.ico

[Tasks]
Name: "desktopicon"; Description: "Create a Desktop icon"; GroupDescription: "Additional icons:"; Flags: unchecked
Name: "startmenuicon"; Description: "Create a Start Menu shortcut"; GroupDescription: "Additional icons:"
Name: "registerext"; Description: "Associate .EIHconf files with {#MyAppTitle}"; GroupDescription: "Optional actions:"

[Files]
Source: "{#MyOutputDir}\CSV*.*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs
Source: "{#MyOutputDir}\*"; DestDir: "{app}"; Flags: recursesubdirs

[Registry]
; --- Delimited Text Values  ---
Root: HKCU; Subkey: "Software\Classes\.csv"; ValueType: string; ValueName: ""; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\{#MyAppName}"; ValueType: string; ValueName: ""; ValueData: "Delimited Text File"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\{#MyAppName}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppName}.exe,0"
Root: HKCU; Subkey: "Software\Classes\{#MyAppName}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppName}.exe"" ""%1"""

; --- TAB (alias for TSV) ---
Root: HKCU; Subkey: "Software\Classes\.tab"; ValueType: string; ValueName: ""; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\.psv"; ValueType: string; ValueName: ""; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\.ssv"; ValueType: string; ValueName: ""; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\.dsv"; ValueType: string; ValueName: ""; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\.txt"; ValueType: string; ValueName: ""; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue

[Icons]
; Start Menu 
Name: "{autoprograms}\{#MyAppTitle}\{#MyAppTitle}"; Filename: "{app}\{#MyAppName}.exe"; Tasks: startmenuicon
Name: "{autoprograms}\{#MyAppTitle}\Uninstall {#MyAppTitle}"; Filename: "{uninstallexe}"; Tasks: startmenuicon

; Desktop shortcut 
Name: "{autodesktop}\{#MyAppTitle}"; Filename: "{app}\{#MyAppName}.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppName}.exe"; Description: "{cm:LaunchProgram,{#MyAppTitle}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Always clean up empty app folder
Type: dirifempty; Name: "{userappdata}\{#MyAppName}"
Type: dirifempty; Name: "{app}"


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

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  RemoveData: Boolean;
begin
  if CurUninstallStep = usUninstall then
  begin
    if UninstallSilent then
    begin
      // Silent uninstall: do not prompt
      RemoveData := False; // set True to automatically remove
    end
    else
    begin
      // Interactive uninstall: ask user
      RemoveData := MsgBox(
        'Do you want to remove all local settings and data for {#MyAppTitle}?',
        mbConfirmation, MB_YESNO) = IDYES;
    end;

    if RemoveData then
    begin
      DelTree(ExpandConstant('{userappdata}\{#MyAppName}'), True, True, False);
    end;
  end;
end;
