;
; Script generated by the ASCOM Driver Installer Script Generator 6.4.0.0
; Generated by Lung-Kai Cheng on 2019/4/7 (UTC)
;
[Setup]
AppID={{6ca208b7-9809-44c8-a717-486a889f1186}
AppName=Starbook Driver
AppVerName=Starbook Driver v0.7
AppVersion=0.7
AppPublisher=Lung-Kai Cheng <lkcheng89@gmail.com>
AppPublisherURL=mailto:lkcheng89@gmail.com
AppSupportURL=https://github.com/lkcheng89/starbook
AppUpdatesURL=https://github.com/lkcheng89/starbook
VersionInfoVersion=1.0.0
MinVersion=0,5.0.2195sp4
DefaultDirName="{cf}\ASCOM\Telescope"
DisableDirPage=yes
DisableProgramGroupPage=yes
OutputDir="."
OutputBaseFilename="StarbookDriver-v{#SetupSetting("AppVersion")}"
Compression=lzma
SolidCompression=yes
; Put there by Platform if Driver Installer Support selected
WizardImageFile="C:\Program Files (x86)\ASCOM\Platform 6 Developer Components\Installer Generator\Resources\WizardImage.bmp"
LicenseFile=".\LICENSE"
; {cf}\ASCOM\Uninstall\Telescope folder created by Platform, always
UninstallFilesDir="{cf}\ASCOM\Uninstall\Telescope\Starbook"SetupIconFile=.\ASCOM.ico

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Dirs]
Name: "{cf}\ASCOM\Uninstall\Telescope\Starbook"
; TODO: Add subfolders below {app} as needed (e.g. Name: "{app}\MyFolder")

[Files]
Source: ".\bin\Release\ASCOM.Starbook.Telescope.dll"; DestDir: "{app}"
; Require a read-me HTML to appear after installation, maybe driver's Help doc
Source: ".\CHANGES.txt"; DestDir: "{app}"; Flags: isreadme
; TODO: Add other files needed by your driver here (add subfolders above)


; Only if driver is .NET
[Run]
; Only for .NET assembly/in-proc drivers
Filename: "{dotnet4032}\regasm.exe"; Parameters: "/codebase ""{app}\ASCOM.Starbook.Telescope.dll"""; Flags: runhidden 32bit
Filename: "{dotnet4064}\regasm.exe"; Parameters: "/codebase ""{app}\ASCOM.Starbook.Telescope.dll"""; Flags: runhidden 64bit; Check: IsWin64




; Only if driver is .NET
[UninstallRun]
; Only for .NET assembly/in-proc drivers
Filename: "{dotnet4032}\regasm.exe"; Parameters: "-u ""{app}\ASCOM.Starbook.Telescope.dll"""; Flags: runhidden 32bit
; This helps to give a clean uninstall
Filename: "{dotnet4064}\regasm.exe"; Parameters: "/codebase ""{app}\ASCOM.Starbook.Telescope.dll"""; Flags: runhidden 64bit; Check: IsWin64
Filename: "{dotnet4064}\regasm.exe"; Parameters: "-u ""{app}\ASCOM.Starbook.Telescope.dll"""; Flags: runhidden 64bit; Check: IsWin64




[Code]
const
   REQUIRED_PLATFORM_VERSION = 6.2;    // Set this to the minimum required ASCOM Platform version for this application

//
// Function to return the ASCOM Platform's version number as a double.
//
function PlatformVersion(): Double;
var
   PlatVerString : String;
begin
   Result := 0.0;  // Initialise the return value in case we can't read the registry
   try
      if RegQueryStringValue(HKEY_LOCAL_MACHINE_32, 'Software\ASCOM','PlatformVersion', PlatVerString) then 
      begin // Successfully read the value from the registry
         Result := StrToFloat(PlatVerString); // Create a double from the X.Y Platform version string
      end;
   except                                                                   
      ShowExceptionMessage;
      Result:= -1.0; // Indicate in the return value that an exception was generated
   end;
end;

//
// Before the installer UI appears, verify that the required ASCOM Platform version is installed.
//
function InitializeSetup(): Boolean;
var
   PlatformVersionNumber : double;
 begin
   Result := FALSE;  // Assume failure
   PlatformVersionNumber := PlatformVersion(); // Get the installed Platform version as a double
   If PlatformVersionNumber >= REQUIRED_PLATFORM_VERSION then	// Check whether we have the minimum required Platform or newer
      Result := TRUE
   else
      if PlatformVersionNumber = 0.0 then
         MsgBox('No ASCOM Platform is installed. Please install Platform ' + Format('%3.1f', [REQUIRED_PLATFORM_VERSION]) + ' or later from http://www.ascom-standards.org', mbCriticalError, MB_OK)
      else 
         MsgBox('ASCOM Platform ' + Format('%3.1f', [REQUIRED_PLATFORM_VERSION]) + ' or later is required, but Platform '+ Format('%3.1f', [PlatformVersionNumber]) + ' is installed. Please install the latest Platform before continuing; you will find it at http://www.ascom-standards.org', mbCriticalError, MB_OK);
end;

// Code to enable the installer to uninstall previous versions of itself when a new version is installed
procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  UninstallExe: String;
  UninstallRegistry: String;
begin
  if (CurStep = ssInstall) then // Install step has started
	begin
      // Create the correct registry location name, which is based on the AppId
      UninstallRegistry := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#SetupSetting("AppId")}' + '_is1');
      // Check whether an extry exists
      if RegQueryStringValue(HKLM, UninstallRegistry, 'UninstallString', UninstallExe) then
        begin // Entry exists and previous version is installed so run its uninstaller quietly after informing the user
          MsgBox('Setup will now remove the previous version.', mbInformation, MB_OK);
          Exec(RemoveQuotes(UninstallExe), ' /SILENT', '', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode);
          sleep(1000);    //Give enough time for the install screen to be repainted before continuing
        end
  end;
end;

