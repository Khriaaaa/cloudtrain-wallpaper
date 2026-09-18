; 云间列车动态壁纸 — NSIS 安装脚本
Unicode true
!include "MUI2.nsh"
!include "x64.nsh"

Name "云间列车动态壁纸"
OutFile "..\CloudTrainWallpaper-Setup.exe"
InstallDir "$LOCALAPPDATA\CloudTrainWallpaper"
RequestExecutionLevel user
SetCompressor /SOLID lzma

!define MUI_ICON "app.ico"
!define MUI_UNICON "app.ico"
!define MUI_ABORTWARNING

; 自启动询问页 (自绘勾选框用 MUI_COMPONENTS 页替代 → 用 .onSelChange 处理 Section)
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "SimpChinese"

InstType "完整安装" ; 默认: 程序 + 开机自启
InstType "仅安装，不自启动"

Section "云间列车壁纸程序" SEC_MAIN
  SectionIn RO
  SetOutPath "$INSTDIR"
  File /r "..\publish\CloudTrainWallpaper\*.*"

  ; 写卸载信息
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\CloudTrainWallpaper" "DisplayName" "云间列车动态壁纸"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\CloudTrainWallpaper" "UninstallString" '"$INSTDIR\Uninstall.exe"'
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\CloudTrainWallpaper" "DisplayIcon" '"$INSTDIR\CloudTrainWallpaper.exe"'
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\CloudTrainWallpaper" "Publisher" "Kyria"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\CloudTrainWallpaper" "DisplayVersion" "1.0.0"
SectionEnd

Section "开机自启动（推荐）" SEC_AUTOSTART
  SectionIn 1
  CreateShortCut "$SMSTARTUP\云间列车壁纸.lnk" "$INSTDIR\CloudTrainWallpaper.exe" "" "$INSTDIR\CloudTrainWallpaper.exe" 0
SectionEnd

Section ""
  ; 桌面快捷方式（方便手动启动/退出调试）
  CreateShortCut "$DESKTOP\云间列车壁纸.lnk" "$INSTDIR\CloudTrainWallpaper.exe" "" "$INSTDIR\CloudTrainWallpaper.exe" 0
  ; 装完直接启动
  Exec "$INSTDIR\CloudTrainWallpaper.exe"
SectionEnd

Function .onInit
  StrCpy $INSTDIR "$LOCALAPPDATA\CloudTrainWallpaper"
FunctionEnd

Section "un.Uninstall"
  ; 先让壁纸进程退出
  nsExec::ExecToLog 'taskkill /IM CloudTrainWallpaper.exe /F'
  Pop $0
  Delete "$INSTDIR\Uninstall.exe"
  RMDir /r "$INSTDIR"
  Delete "$SMSTARTUP\云间列车壁纸.lnk"
  Delete "$DESKTOP\云间列车壁纸.lnk"
  DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\CloudTrainWallpaper"
  ; settings 留着 (重装保留偏好); 勾选则删:
  DeleteRegKey HKCU "Software\CloudTrainWallpaper"
SectionEnd
