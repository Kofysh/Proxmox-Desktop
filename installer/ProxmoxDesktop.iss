#define MyAppName "Proxmox Desktop"
#ifndef MyAppVersion
  #define MyAppVersion GetEnv("APP_VERSION")
#endif
#define MyAppPublisher "Kofysh"
#define MyAppExeName "ProxmoxDesktop.exe"

[Setup]
AppId={{B7B3E3A0-4FA3-4B1C-9E4C-2EA4C6F5D86B}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\Proxmox Desktop
DefaultGroupName={#MyAppName}
OutputDir=..\output
OutputBaseFilename=ProxmoxDesktop-{#MyAppVersion}-setup-win-x64
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=lowest
SetupIconFile=..\Resources\Icons\default-icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
CloseApplications=yes
ChangesAssociations=no

[Files]
Source: "..\output\portable\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\output\vc_redist.x64.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"

[Run]
Filename: "{tmp}\vc_redist.x64.exe"; Parameters: "/install /quiet /norestart"; StatusMsg: "Installing Microsoft Visual C++ Runtime..."; Flags: waituntilterminated
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
