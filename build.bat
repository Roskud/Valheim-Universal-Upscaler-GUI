@echo off
echo Building ValheimUpscalerPatcher.dll and ValheimUpscalerUI.dll...

set CSC="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
set VALHEIM_MANAGED="G:\SteamLibrary\steamapps\common\Valheim\valheim_Data\Managed"
if not exist %VALHEIM_MANAGED% set VALHEIM_MANAGED="C:\Program Files (x86)\Steam\steamapps\common\Valheim\valheim_Data\Managed"
set BEPINEX_CORE="C:\Users\olegt\AppData\Roaming\r2modmanPlus-local\Valheim\cache\denikson-BepInExPack_Valheim\5.4.2351\BepInExPack_Valheim\BepInEx\core"


echo Compiling Patcher...
%CSC% /noconfig /target:library /out:"Release\ValheimUpscalerPatcher.dll" -nostdlib ^
  /r:%VALHEIM_MANAGED%\mscorlib.dll ^
  /r:%VALHEIM_MANAGED%\System.dll ^
  /r:%VALHEIM_MANAGED%\System.Core.dll ^
  /r:%BEPINEX_CORE%\Mono.Cecil.dll ^
  "src\ValheimUpscalerPatcher.cs"

if %ERRORLEVEL% NEQ 0 (
    echo Patcher build failed!
    exit /b %ERRORLEVEL%
)

echo Compiling UI Plugin...
%CSC% /noconfig /target:library /out:"Release\ValheimUpscalerUI.dll" -nostdlib ^
  /r:%VALHEIM_MANAGED%\netstandard.dll ^
  /r:%VALHEIM_MANAGED%\mscorlib.dll ^
  /r:%VALHEIM_MANAGED%\System.dll ^
  /r:%VALHEIM_MANAGED%\System.Core.dll ^
  /r:%BEPINEX_CORE%\BepInEx.dll ^
  /r:%BEPINEX_CORE%\0Harmony.dll ^
  /r:%VALHEIM_MANAGED%\UnityEngine.dll ^
  /r:%VALHEIM_MANAGED%\UnityEngine.CoreModule.dll ^
  /r:%VALHEIM_MANAGED%\UnityEngine.InputLegacyModule.dll ^
  /r:%VALHEIM_MANAGED%\UnityEngine.UI.dll ^
  /r:%VALHEIM_MANAGED%\UnityEngine.IMGUIModule.dll ^
  /r:%VALHEIM_MANAGED%\Unity.TextMeshPro.dll ^
  /r:%VALHEIM_MANAGED%\assembly_valheim.dll ^
  /r:%VALHEIM_MANAGED%\gui_framework.dll ^
  "src\UpscalerUIPlugin.cs"

if %ERRORLEVEL% EQU 0 (
    copy /y "Release\ValheimUpscalerPatcher.dll" "thunderstore\patchers\ValheimUpscalerPatcher.dll"
    copy /y "Release\ValheimUpscalerUI.dll" "thunderstore\plugins\ValheimUpscalerUI.dll"
    echo Build succeeded!
) else (
    echo UI build failed!
)
