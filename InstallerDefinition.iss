[Setup]
AppName=WcfPad
AppVersion=0.1.1
OutputBaseFilename=WcfPad_0.1.1
DefaultDirName={pf}\WcfPad
DefaultGroupName=WcfPad
UninstallDisplayIcon={app}\WcfPad.exe
Compression=lzma2
SolidCompression=yes
OutputDir=dist

[Files]
Source: "README.md"; DestDir: "{app}"; Flags: isreadme
Source: "WcfPad.UI\bin\x86\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\WcfPad"; Filename: "{app}\WcfPad.UI.exe"; WorkingDir: "{app}"