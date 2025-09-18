#define MyAppTitle "Csv Quick Viewer"
#define MyAppName "CsvQuickViewer"
#define MyOutputDir "..\..\Application\bin\Release\net8.0-windows\"
#define MyAppVersion GetFileVersion(MyOutputDir + "\" +  MyAppName + ".exe")


[Setup]
AppVersion={#MyAppVersion}
OutputBaseFilename={#MyAppName}Installer_{#MyAppVersion}_Net8
DefaultDirName={localappdata}\{#MyAppName}
AppName={#MyAppName}
OutputDir=..
PrivilegesRequiredOverridesAllowed=dialog
PrivilegesRequired=lowest
DisableDirPage=no    
DisableProgramGroupPage=yes
SetupIconFile=..\..\Application\csv_text.ico

[Tasks]
Name: "desktopicon"; Description: "Create a Desktop icon"; GroupDescription: "Additional icons:"; Flags: unchecked
Name: "startmenuicon"; Description: "Create a Start Menu shortcut"; GroupDescription: "Additional icons:"
Name: "registerext"; Description: "Associate .EIHconf files with {#MyAppTitle}"; GroupDescription: "Optional actions:"

[Files]
Source: "{#MyOutputDir}*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

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
Type: dirifempty; Name: "{localappdata}\{#MyAppName}"
Type: dirifempty; Name: "{app}"

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
        'Do you want to remove all local settings and data for {#MyAppTitle}?'#13#10 +
        'This includes the settings, most recently used list and not saved configurations.',
        mbConfirmation, MB_YESNO) = IDYES;
    end;

    if RemoveData then
    begin
      DelTree(ExpandConstant('{localappdata}\{#MyAppName}'), True, True, False);
    end;
  end;
end;
