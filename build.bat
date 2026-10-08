@echo off
echo Building ValheimUpscalerUI.dll...

set CSC="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
set VALHEIM_MANAGED="C:\Program Files (x86)\Steam\steamapps\common\Valheim\valheim_Data\Managed"
set BEPINEX_CORE="C:\Program Files (x86)\Steam\steamapps\common\Valheim\BepInEx\core"
set RECON_PLUGIN="C:\Program Files (x86)\Steam\steamapps\common\Valheim\BepInEx\plugins\ValheimUpscalerRecon\ValheimUpscaler.Recon.dll"

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
  /r:%RECON_PLUGIN% ^
  "src\UpscalerUIPlugin.cs"

if %ERRORLEVEL% EQU 0 (
    echo Build succeeded!
) else (
    echo Build failed!
)
pause
