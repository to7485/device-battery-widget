#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif

#define MyAppName "Device Battery Widget"
#define MyAppExeName "DeviceBatteryWidget.exe"
#define MySourceDir "..\artifacts\release\v" + MyAppVersion + "\win-x64-self-contained"

[Setup]
AppId={{E9E47DB4-E01B-44D1-884B-826562303393}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher=Device Battery Widget
DefaultDirName={localappdata}\Programs\DeviceBatteryWidget
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\artifacts\release\v{#MyAppVersion}
OutputBaseFilename=DeviceBatteryWidget-{#MyAppVersion}-win-x64-setup-unsigned
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
UninstallDisplayIcon={app}\{#MyAppExeName}

[Files]
Source: "{#MySourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
    RegDeleteValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run', 'DeviceBatteryWidget');
end;
