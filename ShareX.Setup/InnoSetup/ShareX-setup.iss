#define MyAppName "UpLa"
#ifndef Platform
  #define Platform "x64"
#endif
#if Platform == "arm64"
  #define RuntimeId "win-arm64"
  #define MyArchitecturesAllowed "arm64"
#else
  #define RuntimeId "win-x64"
  #define MyArchitecturesAllowed "x64compatible"
#endif
#define MyAppRootDirectory "..\.."
#define MyAppOutputDirectory MyAppRootDirectory + "\Output"
#define MyAppReleaseDirectory MyAppRootDirectory + "\ShareX\bin\Release\" + RuntimeId
#define MyAppFileName MyAppName + ".exe"
#define MyAppFilePath MyAppReleaseDirectory + "\" + MyAppFileName
#define MyAppVersion GetStringFileInfo(MyAppFilePath, "ProductVersion")
#define MyAppPublisher "upla.com.tr"
#define MyAppURL "https://upla.com.tr"
#define MyAppId "{{7C9794A9-0FAB-4308-84F0-22A706B8AE94}"
#define MyAppMutex "904F8705-9B7E-4789-84E9-1112370D9033"
#define OldAppId "82E6AC09-0FEF-4390-AD9F-0DD3F5561EFC"

[Setup]
AppCopyright=Copyright (c) 2007-2026 ShareX Team
AppId={#MyAppId}
AppMutex={#MyAppMutex}
AppName={#MyAppName}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppVerName={#MyAppName} {#MyAppVersion}
AppVersion={#MyAppVersion}
ArchitecturesAllowed={#MyArchitecturesAllowed}
ArchitecturesInstallIn64BitMode={#MyArchitecturesAllowed}
DefaultDirName={commonpf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
LicenseFile={#MyAppRootDirectory}\LICENSE.txt
MinVersion=10.0.14393
OutputBaseFilename={#MyAppName}-{#MyAppVersion}-setup-{#Platform}
OutputDir={#MyAppOutputDirectory}
PrivilegesRequired=none
SetupIconFile={#MyAppRootDirectory}\ShareX\ShareX_Icon.ico
SolidCompression=yes
UninstallDisplayIcon={app}\{#MyAppFileName}
UninstallDisplayName={#MyAppName}
VersionInfoCompany={#MyAppPublisher}
VersionInfoTextVersion={#MyAppVersion}
VersionInfoVersion={#MyAppVersion}

[Tasks]
Name: "CreateDesktopIcon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Check: OfferDesktopIconTask
Name: "CreateContextMenuButton"; Description: "Show ""Upload with {#MyAppName}"" button in Windows Explorer context menu"; GroupDescription: "Additional shortcuts:"; Check: OfferContextMenuTask
Name: "CreateEditContextMenuButton"; Description: "Show ""Edit with {#MyAppName}"" button in Windows Explorer context menu"; GroupDescription: "Additional shortcuts:"; Check: OfferEditContextMenuTask
Name: "CreateSendToIcon"; Description: "Create a send to shortcut"; GroupDescription: "Additional shortcuts:"; Check: OfferSendToTask
Name: "CreateStartupIcon"; Description: "Run {#MyAppName} when Windows starts"; GroupDescription: "Other tasks:"; Check: OfferStartupTask
Name: "DisablePrintScreenKeyForSnippingTool"; Description: "Disable Print Screen key for Snipping Tool"; GroupDescription: "Other tasks:"; Check: not IsUpdating

[Files]
Source: "{#MyAppReleaseDirectory}\*.exe"; DestDir: {app}; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\*.dll"; DestDir: {app}; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\*.json"; DestDir: {app}; Flags: ignoreversion
Source: "{#MyAppRootDirectory}\Licenses\*.txt"; DestDir: {app}\Licenses; Flags: ignoreversion
Source: "{#MyAppOutputDirectory}\*.exe"; DestDir: {app}; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\ShareX_File_Icon.ico"; DestDir: {app}; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\ar-YE\*.resources.dll"; DestDir: {app}\Languages\ar-YE; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\de\*.resources.dll"; DestDir: {app}\Languages\de; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\es\*.resources.dll"; DestDir: {app}\Languages\es; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\es-MX\*.resources.dll"; DestDir: {app}\Languages\es-MX; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\fa-IR\*.resources.dll"; DestDir: {app}\Languages\fa-IR; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\fr\*.resources.dll"; DestDir: {app}\Languages\fr; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\he-IL\*.resources.dll"; DestDir: {app}\Languages\he-IL; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\hu\*.resources.dll"; DestDir: {app}\Languages\hu; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\id-ID\*.resources.dll"; DestDir: {app}\Languages\id-ID; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\it-IT\*.resources.dll"; DestDir: {app}\Languages\it-IT; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\ja-JP\*.resources.dll"; DestDir: {app}\Languages\ja-JP; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\ko-KR\*.resources.dll"; DestDir: {app}\Languages\ko-KR; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\nl-NL\*.resources.dll"; DestDir: {app}\Languages\nl-NL; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\pl\*.resources.dll"; DestDir: {app}\Languages\pl; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\pt-BR\*.resources.dll"; DestDir: {app}\Languages\pt-BR; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\pt-PT\*.resources.dll"; DestDir: {app}\Languages\pt-PT; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\ro\*.resources.dll"; DestDir: {app}\Languages\ro; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\ru\*.resources.dll"; DestDir: {app}\Languages\ru; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\tr\*.resources.dll"; DestDir: {app}\Languages\tr; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\uk\*.resources.dll"; DestDir: {app}\Languages\uk; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\vi-VN\*.resources.dll"; DestDir: {app}\Languages\vi-VN; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\zh-CN\*.resources.dll"; DestDir: {app}\Languages\zh-CN; Flags: ignoreversion
Source: "{#MyAppReleaseDirectory}\zh-TW\*.resources.dll"; DestDir: {app}\Languages\zh-TW; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppFileName}"; WorkingDir: "{app}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"; WorkingDir: "{app}"
Name: "{userdesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppFileName}"; WorkingDir: "{app}"; Tasks: CreateDesktopIcon
Name: "{usersendto}\{#MyAppName}"; Filename: "{app}\{#MyAppFileName}"; WorkingDir: "{app}"; Tasks: CreateSendToIcon
Name: "{userstartup}\{#MyAppName}"; Filename: "{app}\{#MyAppFileName}"; WorkingDir: "{app}"; Parameters: "-silent"; Tasks: CreateStartupIcon

[Run]
Filename: "{app}\{#MyAppFileName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall; Check: not IsNoRun

[Registry]
Root: "HKCU"; Subkey: "Software\Classes\*\shell\{#MyAppName}"; ValueType: string; ValueData: "Upload with {#MyAppName}"; Tasks: CreateContextMenuButton
Root: "HKCU"; Subkey: "Software\Classes\*\shell\{#MyAppName}"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\{#MyAppFileName}"",0"; Tasks: CreateContextMenuButton
Root: "HKCU"; Subkey: "Software\Classes\*\shell\{#MyAppName}\command"; ValueType: string; ValueData: """{app}\{#MyAppFileName}"" ""%1"""; Tasks: CreateContextMenuButton
Root: "HKCU"; Subkey: "Software\Classes\Directory\shell\{#MyAppName}"; ValueType: string; ValueData: "Upload with {#MyAppName}"; Tasks: CreateContextMenuButton
Root: "HKCU"; Subkey: "Software\Classes\Directory\shell\{#MyAppName}"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\{#MyAppFileName}"",0"; Tasks: CreateContextMenuButton
Root: "HKCU"; Subkey: "Software\Classes\Directory\shell\{#MyAppName}\command"; ValueType: string; ValueData: """{app}\{#MyAppFileName}"" ""%1"""; Tasks: CreateContextMenuButton
Root: "HKCU"; Subkey: "Software\Classes\SystemFileAssociations\image\shell\{#MyAppName}ImageEditor"; ValueType: string; ValueData: "Edit with {#MyAppName}"; Tasks: CreateEditContextMenuButton
Root: "HKCU"; Subkey: "Software\Classes\SystemFileAssociations\image\shell\{#MyAppName}ImageEditor"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\{#MyAppFileName}"",0"; Tasks: CreateEditContextMenuButton
Root: "HKCU"; Subkey: "Software\Classes\SystemFileAssociations\image\shell\{#MyAppName}ImageEditor\command"; ValueType: string; ValueData: """{app}\{#MyAppFileName}"" -ImageEditor ""%1"""; Tasks: CreateEditContextMenuButton
Root: "HKCU"; Subkey: "Software\Classes\*\shell\{#MyAppName}"; Flags: dontcreatekey uninsdeletekey
Root: "HKCU"; Subkey: "Software\Classes\Directory\shell\{#MyAppName}"; Flags: dontcreatekey uninsdeletekey
Root: "HKCU"; Subkey: "Software\Classes\.sxie"; Flags: dontcreatekey uninsdeletekey
Root: "HKCU"; Subkey: "Software\Classes\{#MyAppName}.sxie"; Flags: dontcreatekey uninsdeletekey
Root: "HKCU"; Subkey: "Software\Classes\SystemFileAssociations\image\shell\{#MyAppName}ImageEditor"; Flags: dontcreatekey uninsdeletekey
Root: "HKCU"; Subkey: "Control Panel\Keyboard"; ValueType: dword; ValueName: "PrintScreenKeyForSnippingEnabled"; ValueData: "0"; Flags: uninsdeletevalue; Tasks: DisablePrintScreenKeyForSnippingTool

[Code]
// UpLa 1.0.0 (built from ShareX 14) was installed with the AppId and mutex of the official ShareX. It is removed before
// installing, but only when the uninstall entry has UpLa's own name and publisher, so an installed ShareX is never touched.
const
  OldAppUninstallKey = 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{#OldAppId}_is1';

var
  OldAppProgressPage: TOutputProgressWizardPage;

procedure InitializeWizard;
begin
  if not IsAdmin then
  begin
    WizardForm.DirEdit.Text := ExpandConstant('{userpf}\{#MyAppName}');
  end;

  OldAppProgressPage := CreateOutputProgressPage(SetupMessage(msgWizardPreparing), SetupMessage(msgPreparingDesc));
end;

function InitializeUninstall(): Boolean;
var
  ErrorCode: Integer;
begin
  if CheckForMutexes('{#MyAppMutex}') then
  begin
    if MsgBox('Uninstall has detected that {#MyAppName} is currently running.' + #13#10#13#10 + 'Would you like to close it?', mbError, MB_YESNO) = IDYES then
    begin
      Exec('taskkill.exe', '/f /im {#MyAppFileName}', '', SW_HIDE, ewWaitUntilTerminated, ErrorCode);
    end
    else
    begin
      Result := False;
      Exit;
    end;
  end;

  Result := True;
end;

function CmdLineParamExists(const value: string): Boolean;
var
  i: Integer;
begin
  Result := False;
  for i := 1 to ParamCount do
    if CompareText(ParamStr(i), value) = 0 then
    begin
      Result := True;
      Exit;
    end;
end;

function IsUpdating(): Boolean;
begin
  Result := CmdLineParamExists('/UPDATE');
end;

function IsNoRun(): Boolean;
begin
  Result := CmdLineParamExists('/NORUN');
end;

function DesktopIconExists(): Boolean;
begin
  Result := FileExists(ExpandConstant('{userdesktop}\{#MyAppName}.lnk'));
end;

function GetOldAppUninstallString(const RootKey: Integer; var UninstallString: String): Boolean;
var
  DisplayName, Publisher: String;
begin
  Result := False;

  if RegQueryStringValue(RootKey, OldAppUninstallKey, 'DisplayName', DisplayName) and (DisplayName = 'UpLa') then
  begin
    if RegQueryStringValue(RootKey, OldAppUninstallKey, 'Publisher', Publisher) and (Publisher <> 'upla.com.tr') then
    begin
      Exit;
    end;

    Result := RegQueryStringValue(RootKey, OldAppUninstallKey, 'UninstallString', UninstallString) and (UninstallString <> '');
  end;
end;

function IsOldAppInstalled(): Boolean;
var
  UninstallString: String;
begin
  Result := GetOldAppUninstallString(HKLM64, UninstallString) or GetOldAppUninstallString(HKLM32, UninstallString) or
    GetOldAppUninstallString(HKCU, UninstallString);
end;

// The updater of UpLa 1.0.0 also starts setup with /UPDATE, and its uninstaller deletes the shortcuts and the context
// menu button it created, so the ones it had are created again.
function OfferDesktopIconTask(): Boolean;
begin
  Result := (not IsUpdating and not DesktopIconExists) or (IsOldAppInstalled and DesktopIconExists);
end;

function OfferContextMenuTask(): Boolean;
begin
  Result := not IsUpdating or (IsOldAppInstalled and RegKeyExists(HKCU, 'Software\Classes\*\shell\{#MyAppName}'));
end;

// Only offered to bring back the button UpLa 1.0.0 had; new installs turn it on in the application settings.
function OfferEditContextMenuTask(): Boolean;
begin
  Result := IsOldAppInstalled and RegKeyExists(HKCU, 'Software\Classes\SystemFileAssociations\image\shell\{#MyAppName}ImageEditor');
end;

function OfferSendToTask(): Boolean;
begin
  Result := not IsUpdating or (IsOldAppInstalled and FileExists(ExpandConstant('{usersendto}\{#MyAppName}.lnk')));
end;

function OfferStartupTask(): Boolean;
begin
  Result := not IsUpdating or (IsOldAppInstalled and FileExists(ExpandConstant('{userstartup}\{#MyAppName}.lnk')));
end;

function CloseOldApp(): Boolean;
var
  i: Integer;
begin
  // When UpLa 1.0.0 starts this setup to update itself, it exits right after, so give it time to close.
  if IsUpdating then
  begin
    for i := 1 to 20 do
    begin
      if not CheckForMutexes('{#OldAppId}') then
      begin
        Break;
      end;

      Sleep(500);
    end;
  end;

  // Its uninstaller would offer to kill it, so the user is asked to close it. ShareX uses the same mutex.
  while CheckForMutexes('{#OldAppId}') do
  begin
    if SuppressibleMsgBox('The old version of {#MyAppName} has to be removed, but it is running.' + #13#10#13#10 + 'Please exit {#MyAppName} from its icon in the notification area (and close ShareX if it is running), then click Retry.', mbError, MB_RETRYCANCEL, IDCANCEL) <> IDRETRY then
    begin
      Result := False;
      Exit;
    end;
  end;

  Result := True;
end;

function RemoveOldApp(const RootKey: Integer): String;
var
  UninstallString, UninstallerPath: String;
  Started: Boolean;
  ResultCode: Integer;
begin
  Result := '';

  if not GetOldAppUninstallString(RootKey, UninstallString) then
  begin
    Exit;
  end;

  UninstallerPath := RemoveQuotes(UninstallString);

  if not FileExists(UninstallerPath) then
  begin
    Log('Uninstaller of the old version not found: ' + UninstallerPath);
    Exit;
  end;

  if not CloseOldApp then
  begin
    Result := 'The old version of {#MyAppName} is still running. Please exit it and run Setup again.';
    Exit;
  end;

  Log('Removing the old version: ' + UninstallerPath);

  OldAppProgressPage.SetText('Removing the old version of {#MyAppName}...', '');
  OldAppProgressPage.Show;
  try
    // The old uninstaller keeps the user settings in Documents\UpLa.
    Started := Exec(UninstallerPath, '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART', '', SW_SHOW, ewWaitUntilTerminated, ResultCode);
  finally
    OldAppProgressPage.Hide;
  end;

  if not Started then
  begin
    Result := 'Setup could not start the uninstaller of the old version of {#MyAppName}: ' + SysErrorMessage(ResultCode);
    Exit;
  end;

  Log(Format('Uninstaller of the old version exited with code %d.', [ResultCode]));

  if GetOldAppUninstallString(RootKey, UninstallString) then
  begin
    Result := 'The old version of {#MyAppName} could not be removed. Please uninstall it from Windows Settings (Apps), then run Setup again. Your {#MyAppName} settings are kept.';
  end;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  Result := RemoveOldApp(HKLM64);

  if Result = '' then
  begin
    Result := RemoveOldApp(HKLM32);
  end;

  if Result = '' then
  begin
    Result := RemoveOldApp(HKCU);
  end;
end;