; Ognmp iss
#define MyAppName "Ognmp"
#define MyAppVersion "0.2.0"
#define MyAppPublisher "OGSteam.fr"
#define MyAppURL "https://ogsteam.fr"
#define MyAppExeName "Ognmp.exe"
#define Year "2020"

;Signature
; Menu Tools -> Sign Tool
; Create an entry with :
; name = signtool
; parameters = signtool.exe sign /f "C:\OGSteam.fr.p12" /t http://timestamp.comodoca.com/authenticode /p "*******" $f
; you need signtool application and to generate the CA with Windows Server

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{8243F191-2683-42E0-86C2-6845851856F7}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
SignTool=signtool
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={sd}\{#MyAppName}
SourceDir=.
DefaultGroupName={#MyAppName}
VersionInfoDescription=Ognmp (version {#MyAppVersion})
VersionInfoCopyright=Copyright 2012-2019 Kurt Cancemi Copyright 2019-{#Year} OGSteam.fr
VersionInfoCompany=OGSteam
LicenseFile=./LICENSE
InfoBeforeFile=
InfoAfterFile=./setup/postinstall.txt
OutputDir=./compiledsetup
OutputBaseFilename=Ognmp-{#MyAppVersion}
SetupIconFile=./setup/OGnmp.ico
Compression=lzma2
LZMADictionarySize=24000
LZMANumBlockThreads=8
LZMAUseSeparateProcess=yes
SolidCompression=false
RestartIfNeededByRun=false
PrivilegesRequired=admin
DirExistsWarning=no

[Languages]
Name: french; MessagesFile: compiler:Default.isl

[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked


[Files]
Source: conf\fastcgi.conf; DestDir: {app}\conf; Flags: ignoreversion
Source: conf\fastcgi_params; DestDir: {app}\conf; Flags: ignoreversion
Source: conf\koi-utf; DestDir: {app}\conf; Flags: ignoreversion
Source: conf\koi-win; DestDir: {app}\conf; Flags: ignoreversion
Source: conf\mime.types; DestDir: {app}\conf; Flags: ignoreversion
Source: conf\nginx.conf; DestDir: {app}\conf; Flags: ignoreversion
Source: conf\localhost.crt; DestDir: {app}\conf; Flags: ignoreversion
Source: conf\localhost.key; DestDir: {app}\conf; Flags: ignoreversion
Source: conf\php_processes.conf; DestDir: {app}\conf; Flags: ignoreversion
Source: conf\scgi_params; DestDir: {app}\conf; Flags: ignoreversion
Source: conf\uwsgi_params; DestDir: {app}\conf; Flags: ignoreversion
Source: conf\win-utf; DestDir: {app}\conf; Flags: ignoreversion
Source: contribs\*; DestDir: {app}\contribs; Flags: ignoreversion recursesubdirs createallsubdirs
Source: logs\*; Excludes: ".gitignore"; DestDir: {app}\logs; Flags: ignoreversion recursesubdirs createallsubdirs
Source: mariadb\bin\*; Excludes: ".gitignore,*.pdb"; DestDir: {app}\mariadb\bin; Flags: ignoreversion
Source: mariadb\data\*; Excludes: ".gitignore"; DestDir: {app}\mariadb\data; Flags: ignoreversion recursesubdirs createallsubdirs
Source: mariadb\include\*; Excludes: ".gitignore"; DestDir: {app}\mariadb\include; Flags: ignoreversion recursesubdirs createallsubdirs
Source: mariadb\share\*; Excludes: ".gitignore"; DestDir: {app}\mariadb\share; Flags: ignoreversion recursesubdirs createallsubdirs
Source: php\*; DestDir: {app}\php; Excludes: ".gitignore,*.pdb"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: html\*; DestDir: {app}\html; Excludes: ".gitignore"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: temp\*; DestDir: {app}\temp; Excludes: ".gitignore"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: nginx.exe; DestDir: {app}; Flags: ignoreversion
Source: README.md; DestDir: {app}; Flags: ignoreversion
Source: LICENSE; DestDir: {app}; Flags: ignoreversion
Source: "vc_redist.x64.exe"; DestDir: {tmp}; Flags: ignoreversion deleteafterinstall
Source: Ognmp/Bin/Release/Ognmp.exe; DestDir: {app}; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: {group}\{#MyAppName}; Filename: {app}\{#MyAppExeName}
Name: {group}\{cm:UninstallProgram,{#MyAppName}}; Filename: {uninstallexe}
Name: {commondesktop}\{#MyAppName}; Filename: {app}\{#MyAppExeName}; Tasks: desktopicon

[Run]
Filename: {app}\{#MyAppExeName}; Description: {cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}; Flags: nowait postinstall shellexec runascurrentuser 
Filename: "{tmp}\vc_redist.x64.exe"; Parameters: "/install /passive /norestart"
Filename: "{app}\mariadb\bin\mysql_install_db.exe"; Parameters: "--password=password"