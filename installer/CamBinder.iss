; CamBinder installer script.
; Build the app first: dotnet publish src\CamBinder.App\CamBinder.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish\CamBinder.App
; Then compile with: iscc installer\CamBinder.iss

#define MyAppName "CamBinder"
#define MyAppVersion "1.0.0"
#define MyAppExeName "CamBinder.App.exe"
#define MyPublishDir "..\publish\CamBinder.App"

[Setup]
AppId={{9F4C6E2A-6B0B-4B9D-9C7E-3D6E9E1E2C11}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=admin
OutputDir=..\publish\installer
OutputBaseFilename=CamBinderSetup
Compression=lzma2
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Files]
Source: "{#MyPublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Registry]
Root: HKCR; Subkey: "SystemFileAssociations\.pdf\shell\CamBind"; ValueType: string; ValueName: ""; ValueData: "Cambine"; Flags: uninsdeletekey
Root: HKCR; Subkey: "SystemFileAssociations\.pdf\shell\CamBind"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\{#MyAppExeName}"""
Root: HKCR; Subkey: "SystemFileAssociations\.pdf\shell\CamBind"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Player"
Root: HKCR; Subkey: "SystemFileAssociations\.pdf\shell\CamBind\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""
