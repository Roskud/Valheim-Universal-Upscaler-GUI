using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ValheimUpscalerUI
{
    [BepInPlugin("com.valheim.upscalerui", "Valheim Upscaler Settings UI", "1.1.2")]
    public class UpscalerUIPlugin : BaseUnityPlugin
    {
        public static UpscalerUIPlugin Instance;

        public static ConfigEntry<int> QualityPresetConfig;
        public static ConfigEntry<int> BackendConfig;
        public static ConfigEntry<bool> FrameGenConfig;
        public static ConfigEntry<float> SharpnessConfig;
        public static ConfigEntry<KeyCode> MenuHotkeyConfig;

        private Harmony _harmony;
        private bool _showQuickMenu = false;
        private Rect _quickMenuRect = new Rect(40, 40, 460, 490);
        private bool _wasF7Down = false;

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private bool IsF7Triggered()
        {
            bool isDown = false;
            try
            {
                // VK_F7 = 0x76
                if ((GetAsyncKeyState(0x76) & 0x8000) != 0)
                {
                    isDown = true;
                }
            }
            catch { }

            if (!isDown)
            {
                try
                {
                    if (Input.GetKey(KeyCode.F7) || (MenuHotkeyConfig != null && Input.GetKey(MenuHotkeyConfig.Value)))
                    {
                        isDown = true;
                    }
                }
                catch { }
            }

            bool triggered = isDown && !_wasF7Down;
            _wasF7Down = isDown;
            return triggered;
        }

        private void Awake()
        {
            Instance = this;

            QualityPresetConfig = Config.Bind("GraphicsSettings", "QualityPreset", 2, "0: Off, 1: Native (100%), 2: Quality (67%), 3: Balanced (58%), 4: Performance (50%), 5: Ultra Performance (33%)");
            BackendConfig = Config.Bind("GraphicsSettings", "Backend", 1, "0: FSR 3.1, 1: FSR 4 (RDNA4), 2: XeSS, 3: DLSS");
            FrameGenConfig = Config.Bind("GraphicsSettings", "FrameGeneration", false, "Frame Generation enabled");
            SharpnessConfig = Config.Bind("GraphicsSettings", "Sharpness", 0.5f, "Upscaler sharpness (0.0 to 1.0)");
            MenuHotkeyConfig = Config.Bind("Input", "MenuHotkey", KeyCode.F7, "Hotkey to toggle the quick Upscaler overlay menu");

            AutoDeployNativeFiles();

            _harmony = new Harmony("com.valheim.upscalerui");
            _harmony.PatchAll();

            Debug.Log("[ValheimUpscalerUI] In-Game Upscaler UI Mod loaded successfully.");
        }

        private void AutoDeployNativeFiles()
        {
            try
            {
                string gameDir = Path.GetDirectoryName(Application.dataPath);
                if (string.IsNullOrEmpty(gameDir)) return;

                // 1. Remove leftover version.dll if present
                string versionDll = Path.Combine(gameDir, "version.dll");
                if (File.Exists(versionDll))
                {
                    try
                    {
                        File.Delete(versionDll);
                        Debug.Log("[ValheimUpscalerUI] Removed conflicting version.dll to prevent Dx12 hook collision.");
                    }
                    catch { }
                }

                // 2. Locate native files from executing plugin directory
                string pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrEmpty(pluginDir) || !Directory.Exists(pluginDir)) return;

                string[] nativeFiles = new string[]
                {
                    "dxgi.dll",
                    "OptiScaler.ini",
                    "OptiScaler.dll",
                    "amd_fidelityfx_dx12.dll",
                    "amd_fidelityfx_framegeneration_dx12.dll",
                    "amd_fidelityfx_upscaler_dx12.dll",
                    "libxess.dll",
                    "libxess_fg.dll",
                    "fakenvapi.dll",
                    "fakenvapi.ini"
                };

                bool anyDeployed = false;
                foreach (string fileName in nativeFiles)
                {
                    string src = Path.Combine(pluginDir, fileName);
                    if (!File.Exists(src))
                    {
                        string sub1 = Path.Combine(pluginDir, "native", fileName);
                        string sub2 = Path.Combine(pluginDir, "runtimes", fileName);
                        if (File.Exists(sub1)) src = sub1;
                        else if (File.Exists(sub2)) src = sub2;
                    }

                    if (File.Exists(src))
                    {
                        string dst = Path.Combine(gameDir, fileName);
                        bool needsCopy = !File.Exists(dst);
                        if (!needsCopy)
                        {
                            FileInfo fiSrc = new FileInfo(src);
                            FileInfo fiDst = new FileInfo(dst);
                            if (fiSrc.Length != fiDst.Length) needsCopy = true;
                        }

                        if (needsCopy)
                        {
                            try
                            {
                                File.Copy(src, dst, true);
                                anyDeployed = true;
                                Debug.Log("[ValheimUpscalerUI] Auto-deployed to game root: " + fileName);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning("[ValheimUpscalerUI] Failed to auto-deploy " + fileName + ": " + ex.Message);
                            }
                        }
                    }
                }

                if (anyDeployed)
                {
                    Debug.Log("[ValheimUpscalerUI] All native upscaler runtimes successfully deployed!");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[ValheimUpscalerUI] Error in AutoDeployNativeFiles: " + ex);
            }
        }

        private void Start()
        {
            ApplyAllSettings();
        }

        private void Update()
        {
            if (IsF7Triggered())
            {
                _showQuickMenu = !_showQuickMenu;
                if (_showQuickMenu)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }

            if (_showQuickMenu)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    _showQuickMenu = false;
                }
            }
        }

        private void OnGUI()
        {
            if (!_showQuickMenu) return;

            GUI.depth = -1000;

            _quickMenuRect.width = 460;
            _quickMenuRect.height = 490;
            _quickMenuRect.x = Mathf.Clamp(_quickMenuRect.x, 10, Mathf.Max(10, Screen.width - _quickMenuRect.width - 10));
            _quickMenuRect.y = Mathf.Clamp(_quickMenuRect.y, 10, Mathf.Max(10, Screen.height - _quickMenuRect.height - 10));

            // Solid background box behind window to guarantee readability
            GUI.Box(new Rect(_quickMenuRect.x - 3, _quickMenuRect.y - 3, _quickMenuRect.width + 6, _quickMenuRect.height + 6), GUIContent.none);

            _quickMenuRect = GUILayout.Window(998822, _quickMenuRect, DrawQuickMenuWindow, "Valheim Universal Upscaler GUI");
        }

        private void DrawQuickMenuWindow(int windowId)
        {
            GUILayout.BeginVertical();

            GUILayout.Label("<b>GPU:</b> " + SystemInfo.graphicsDeviceName, GUILayout.ExpandWidth(true));
            GUILayout.Space(6);

            GUILayout.Label("<b>Upscaling Quality (Render Scale):</b>");
            string[] presetLabels = new string[]
            {
                "Off",
                "Native (100%)",
                "Quality (67%)",
                "Balanced (58%)",
                "Performance (50%)",
                "Ultra (33%)"
            };

            int currentPreset = QualityPresetConfig.Value;
            int newPreset = GUILayout.SelectionGrid(currentPreset, presetLabels, 3);
            if (newPreset != currentPreset)
            {
                ApplyQualityPreset(newPreset);
            }

            GUILayout.Space(10);
            GUILayout.Label("<b>Upscaling Technology:</b>");
            string[] backendLabels = new string[]
            {
                "AMD FSR 3.1",
                "AMD FSR 4 (FP8)",
                "Intel XeSS",
                "NVIDIA DLSS"
            };

            int currentBackend = BackendConfig.Value;
            int newBackend = GUILayout.SelectionGrid(currentBackend, backendLabels, 2);
            if (newBackend != currentBackend)
            {
                ApplyBackend(newBackend);
            }

            GUILayout.Space(10);
            bool currentFg = FrameGenConfig.Value;
            bool newFg = GUILayout.Toggle(currentFg, "  Enable Frame Generation");
            if (newFg != currentFg)
            {
                ApplyFrameGen(newFg);
            }

            GUILayout.Space(10);
            GUILayout.Label("<b>Sharpness:</b> " + (SharpnessConfig.Value * 100f).ToString("F0") + "%");
            float newSharpness = GUILayout.HorizontalSlider(SharpnessConfig.Value, 0f, 1f);
            if (Math.Abs(newSharpness - SharpnessConfig.Value) > 0.02f)
            {
                ApplySharpness(newSharpness);
            }

            GUILayout.Space(12);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply All Settings", GUILayout.Height(30)))
            {
                ApplyAllSettings();
            }
            if (GUILayout.Button("Close (F7)", GUILayout.Height(30)))
            {
                _showQuickMenu = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("<size=11><i>Tip: Press [Insert] to open OptiScaler's native in-game overlay menu.</i></size>");

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, 10000, 25));
        }

        public static void ApplyQualityPreset(int presetIndex)
        {
            QualityPresetConfig.Value = presetIndex;

            // 1. Update ValheimUpscaler.Inject engine if loaded
            try
            {
                Type injectPluginType = AccessTools.TypeByName("ValheimUpscaler.Inject.InjectPlugin");
                Type injectStateType = AccessTools.TypeByName("ValheimUpscaler.Inject.InjectState");

                if (presetIndex == 0) // Off
                {
                    if (injectStateType != null)
                    {
                        MethodInfo setInjMethod = AccessTools.Method(injectStateType, "SetInjectionEnabled", new Type[] { typeof(bool), typeof(string) });
                        if (setInjMethod != null) setInjMethod.Invoke(null, new object[] { false, "in-game-ui" });
                    }
                    if (injectPluginType != null)
                    {
                        FieldInfo customPresetField = AccessTools.Field(injectPluginType, "ConfigCustomResolutionPreset");
                        object customPresetEntry = customPresetField != null ? customPresetField.GetValue(null) : null;
                        if (customPresetEntry != null)
                        {
                            PropertyInfo prop = AccessTools.Property(customPresetEntry.GetType(), "Value");
                            if (prop != null) prop.SetValue(customPresetEntry, false, null);
                        }
                    }
                }
                else
                {
                    if (injectStateType != null)
                    {
                        MethodInfo setInjMethod = AccessTools.Method(injectStateType, "SetInjectionEnabled", new Type[] { typeof(bool), typeof(string) });
                        if (setInjMethod != null) setInjMethod.Invoke(null, new object[] { true, "in-game-ui" });
                    }

                    if (injectPluginType != null)
                    {
                        FieldInfo customPresetField = AccessTools.Field(injectPluginType, "ConfigCustomResolutionPreset");
                        object customPresetEntry = customPresetField != null ? customPresetField.GetValue(null) : null;
                        if (customPresetEntry != null)
                        {
                            PropertyInfo prop = AccessTools.Property(customPresetEntry.GetType(), "Value");
                            if (prop != null) prop.SetValue(customPresetEntry, true, null);
                        }

                        FieldInfo dlssModeField = AccessTools.Field(injectPluginType, "ConfigDLSSMode");
                        object dlssModeEntry = dlssModeField != null ? dlssModeField.GetValue(null) : null;
                        if (dlssModeEntry != null)
                        {
                            int dlssModeVal = 2; // Quality
                            if (presetIndex == 1) dlssModeVal = 0; // Native
                            else if (presetIndex == 2) dlssModeVal = 2; // Quality
                            else if (presetIndex == 3) dlssModeVal = 3; // Balanced
                            else if (presetIndex == 4) dlssModeVal = 4; // Performance
                            else if (presetIndex == 5) dlssModeVal = 5; // UltraPerformance

                            Type enumType = dlssModeEntry.GetType().GetGenericArguments()[0];
                            object enumVal = Enum.ToObject(enumType, dlssModeVal);
                            PropertyInfo prop = AccessTools.Property(dlssModeEntry.GetType(), "Value");
                            if (prop != null) prop.SetValue(dlssModeEntry, enumVal, null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[ValheimUpscalerUI] InjectPlugin preset sync: " + ex.Message);
            }

            // 2. Update OptiScaler.ini [UpscaleRatio]
            try
            {
                if (presetIndex == 0) // Off
                {
                    UpdateIniKey("[UpscaleRatio]", "UpscaleRatioOverrideEnabled", "false");
                    Debug.Log("[ValheimUpscalerUI] Upscaler disabled (Native game rendering).");
                }
                else
                {
                    string ratioVal = "1.5"; // Quality (67%)
                    if (presetIndex == 1) ratioVal = "1.0"; // Native (100%)
                    else if (presetIndex == 2) ratioVal = "1.5"; // Quality (67%)
                    else if (presetIndex == 3) ratioVal = "1.7"; // Balanced (58%)
                    else if (presetIndex == 4) ratioVal = "2.0"; // Performance (50%)
                    else if (presetIndex == 5) ratioVal = "3.0"; // Ultra Performance (33%)

                    UpdateIniKey("[UpscaleRatio]", "UpscaleRatioOverrideEnabled", "true");
                    UpdateIniKey("[UpscaleRatio]", "UpscaleRatioOverrideValue", ratioVal);
                    Debug.Log("[ValheimUpscalerUI] Applied Quality Preset index: " + presetIndex + " (Ratio: " + ratioVal + ")");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[ValheimUpscalerUI] Error applying quality preset: " + ex);
            }

            // 3. Drive game's internal 3D rendering resolution
            try
            {
                int screenH = Screen.height > 0 ? Screen.height : 1080;
                int targetVertical = screenH;
                if (presetIndex == 1) targetVertical = screenH;
                else if (presetIndex == 2) targetVertical = Mathf.RoundToInt(screenH / 1.5f);
                else if (presetIndex == 3) targetVertical = Mathf.RoundToInt(screenH / 1.7f);
                else if (presetIndex == 4) targetVertical = Mathf.RoundToInt(screenH / 2.0f);
                else if (presetIndex == 5) targetVertical = Mathf.RoundToInt(screenH / 3.0f);

                Type ufbType = AccessTools.TypeByName("UpscaledFrameBuffer");
                if (ufbType != null)
                {
                    FieldInfo autoResField = AccessTools.Field(ufbType, "m_autoTargetResolution");
                    if (autoResField != null) autoResField.SetValue(null, false);

                    FieldInfo targetResField = AccessTools.Field(ufbType, "m_targetResolutionVertical");
                    if (targetResField != null)
                    {
                        uint uintVal = presetIndex == 0 ? uint.MaxValue : (uint)targetVertical;
                        targetResField.SetValue(null, uintVal);
                    }

                    UnityEngine.Object[] ufbObjects = UnityEngine.Object.FindObjectsOfType(ufbType);
                    if (ufbObjects != null)
                    {
                        MethodInfo updateCamMethod = AccessTools.Method(ufbType, "UpdateCameraTarget");
                        foreach (var ufb in ufbObjects)
                        {
                            if (updateCamMethod != null) updateCamMethod.Invoke(ufb, null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[ValheimUpscalerUI] In-engine resolution scale adjustment: " + ex.Message);
            }
        }

        public static void ApplyBackend(int backendIndex)
        {
            BackendConfig.Value = backendIndex;

            try
            {
                // 0=fsr31, 1=fsr4, 2=xess, 3=dlss
                string backendCode = "fsr31";
                if (backendIndex == 1) backendCode = "fsr4";
                else if (backendIndex == 2) backendCode = "xess";
                else if (backendIndex == 3) backendCode = "dlss";

                Type backendSelectorType = AccessTools.TypeByName("ValheimUpscaler.Inject.BackendSelector");
                Type injectPluginType = AccessTools.TypeByName("ValheimUpscaler.Inject.InjectPlugin");

                if (backendIndex == 1) // FSR 4
                {
                    if (injectPluginType != null)
                    {
                        FieldInfo cfgBackendField = AccessTools.Field(injectPluginType, "ConfigBackend");
                        object cfgBackend = cfgBackendField != null ? cfgBackendField.GetValue(null) : null;
                        if (cfgBackend != null)
                        {
                            Type enumType = cfgBackend.GetType().GetGenericArguments()[0];
                            object fsr4Val = Enum.ToObject(enumType, 2);
                            PropertyInfo prop = AccessTools.Property(cfgBackend.GetType(), "Value");
                            if (prop != null) prop.SetValue(cfgBackend, fsr4Val, null);
                        }
                    }
                }
                else if (backendIndex == 3) // DLSS
                {
                    if (injectPluginType != null)
                    {
                        FieldInfo cfgBackendField = AccessTools.Field(injectPluginType, "ConfigBackend");
                        object cfgBackend = cfgBackendField != null ? cfgBackendField.GetValue(null) : null;
                        if (cfgBackend != null)
                        {
                            Type enumType = cfgBackend.GetType().GetGenericArguments()[0];
                            object dlssVal = Enum.ToObject(enumType, 1);
                            PropertyInfo prop = AccessTools.Property(cfgBackend.GetType(), "Value");
                            if (prop != null) prop.SetValue(cfgBackend, dlssVal, null);
                        }
                    }
                }
                else
                {
                    if (injectPluginType != null)
                    {
                        FieldInfo cfgBackendField = AccessTools.Field(injectPluginType, "ConfigBackend");
                        object cfgBackend = cfgBackendField != null ? cfgBackendField.GetValue(null) : null;
                        if (cfgBackend != null)
                        {
                            Type enumType = cfgBackend.GetType().GetGenericArguments()[0];
                            object autoVal = Enum.ToObject(enumType, 0);
                            PropertyInfo prop = AccessTools.Property(cfgBackend.GetType(), "Value");
                            if (prop != null) prop.SetValue(cfgBackend, autoVal, null);
                        }
                    }
                }

                if (backendSelectorType != null)
                {
                    PropertyInfo selProp = AccessTools.Property(backendSelectorType, "SelectedBackend");
                    if (selProp != null) selProp.SetValue(null, backendCode, null);
                    MethodInfo applyMethod = AccessTools.Method(backendSelectorType, "Apply", new Type[0]);
                    if (applyMethod != null) applyMethod.Invoke(null, null);
                }

                UpdateOptiScalerIniBackend(backendIndex, backendCode);
                Debug.Log("[ValheimUpscalerUI] Applied Backend: " + backendCode + " (index " + backendIndex + ")");
            }
            catch (Exception ex)
            {
                Debug.LogError("[ValheimUpscalerUI] Error applying backend: " + ex);
            }
        }

        public static void ApplyFrameGen(bool enabled)
        {
            FrameGenConfig.Value = enabled;

            try
            {
                Type injectPluginType = AccessTools.TypeByName("ValheimUpscaler.Inject.InjectPlugin");
                if (injectPluginType != null)
                {
                    FieldInfo fgField = AccessTools.Field(injectPluginType, "ConfigEnableFG");
                    object fgConfigEntry = fgField != null ? fgField.GetValue(null) : null;
                    if (fgConfigEntry != null)
                    {
                        PropertyInfo prop = AccessTools.Property(fgConfigEntry.GetType(), "Value");
                        if (prop != null) prop.SetValue(fgConfigEntry, enabled, null);
                    }
                }

                UpdateIniKey("[FrameGen]", "Enabled", enabled ? "true" : "false");
                Debug.Log("[ValheimUpscalerUI] Applied Frame Generation: " + enabled);
            }
            catch (Exception ex)
            {
                Debug.LogError("[ValheimUpscalerUI] Error applying frame generation: " + ex);
            }
        }

        public static void ApplySharpness(float sharpness)
        {
            SharpnessConfig.Value = Mathf.Clamp01(sharpness);

            try
            {
                Type nativeType = AccessTools.TypeByName("ValheimUpscaler.Inject.UpscalerNative");
                if (nativeType != null)
                {
                    MethodInfo setSharpnessMethod = AccessTools.Method(nativeType, "SetSharpness", new Type[] { typeof(float) });
                    if (setSharpnessMethod != null)
                    {
                        setSharpnessMethod.Invoke(null, new object[] { SharpnessConfig.Value });
                    }
                }

                UpdateIniKey("[Sharpness]", "Sharpness", SharpnessConfig.Value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
                UpdateIniKey("[Sharpness]", "OverrideSharpness", "true");
            }
            catch (Exception ex)
            {
                Debug.LogError("[ValheimUpscalerUI] Error applying sharpness: " + ex);
            }
        }

        public static void ApplyAllSettings()
        {
            ApplyBackend(BackendConfig.Value);
            ApplyQualityPreset(QualityPresetConfig.Value);
            ApplyFrameGen(FrameGenConfig.Value);
            ApplySharpness(SharpnessConfig.Value);
        }

        private static void UpdateOptiScalerIniBackend(int backendIndex, string backendCode)
        {
            string upscalerCode = backendCode;
            if (backendIndex == 1) // FSR 4
            {
                upscalerCode = "fsr31";
                UpdateIniKey("[FSR]", "UpscalerIndex", "0");
            }
            else if (backendIndex == 0) // FSR 3.1
            {
                upscalerCode = "fsr31";
                UpdateIniKey("[FSR]", "UpscalerIndex", "1");
            }

            UpdateIniKey("[Upscalers]", "Dx12Upscaler", upscalerCode);
        }

        private static void UpdateIniKey(string targetSection, string targetKey, string newValue)
        {
            List<string> paths = new List<string>();
            string gameDir = Path.GetDirectoryName(Application.dataPath);
            if (!string.IsNullOrEmpty(gameDir))
            {
                paths.Add(Path.Combine(gameDir, "OptiScaler.ini"));
            }

            try
            {
                string pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (!string.IsNullOrEmpty(pluginDir))
                {
                    paths.Add(Path.Combine(pluginDir, "OptiScaler.ini"));
                    paths.Add(Path.Combine(pluginDir, "runtimes", "OptiScaler.ini"));
                }
            }
            catch { }

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!string.IsNullOrEmpty(appData))
            {
                paths.Add(Path.Combine(appData, @"r2modmanPlus-local\Valheim\profiles\Default\OptiScaler.ini"));
            }

            foreach (string iniPath in paths)
            {
                try
                {
                    if (!File.Exists(iniPath)) continue;

                    string[] lines = File.ReadAllLines(iniPath);
                    bool inTargetSection = false;
                    bool modified = false;

                    for (int i = 0; i < lines.Length; i++)
                    {
                        string trimmed = lines[i].Trim();
                        if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                        {
                            inTargetSection = trimmed.Equals(targetSection, StringComparison.OrdinalIgnoreCase);
                            continue;
                        }

                        if (inTargetSection && trimmed.StartsWith(targetKey + "=", StringComparison.OrdinalIgnoreCase))
                        {
                            lines[i] = targetKey + "=" + newValue;
                            modified = true;
                            break;
                        }
                    }

                    if (modified)
                    {
                        File.WriteAllLines(iniPath, lines);
                    }
                }
                catch { }
            }
        }
    }

    [HarmonyPatch(typeof(Valheim.SettingsGui.GraphicsSettings), "InitializeUI")]
    public static class GraphicsSettings_InitializeUI_Patch
    {
        public static GameObject CustomPresetRow;
        public static GameObject CustomBackendRow;
        public static GameObject CustomFgRow;
        public static GameObject CustomSharpnessRow;

        public static TMP_Dropdown PresetDropdown;
        public static TMP_Dropdown BackendDropdown;
        public static Toggle FgToggle;
        public static Slider SharpnessSlider;
        public static TMP_Text SharpnessValueText;

        public static void Postfix(Valheim.SettingsGui.GraphicsSettings __instance)
        {
            try
            {
                FieldInfo resRootField = AccessTools.Field(typeof(Valheim.SettingsGui.GraphicsSettings), "m_resolutionRoot");
                GameObject resRoot = resRootField != null ? resRootField.GetValue(__instance) as GameObject : null;
                if (resRoot == null) return;

                Transform listParent = resRoot.transform.parent;
                if (listParent == null) return;

                // Check if our rows already exist in the active settings dialog
                Transform existingPreset = listParent.Find("UpscalerPresetRow");
                if (existingPreset != null)
                {
                    CustomPresetRow = existingPreset.gameObject;
                    Transform eb = listParent.Find("UpscalerBackendRow");
                    if (eb != null) CustomBackendRow = eb.gameObject;
                    Transform ef = listParent.Find("UpscalerFgRow");
                    if (ef != null) CustomFgRow = ef.gameObject;
                    Transform es = listParent.Find("UpscalerSharpnessRow");
                    if (es != null) CustomSharpnessRow = es.gameObject;

                    if (CustomPresetRow)
                        PresetDropdown = CustomPresetRow.GetComponentInChildren<TMP_Dropdown>(true);
                    if (CustomBackendRow)
                        BackendDropdown = CustomBackendRow.GetComponentInChildren<TMP_Dropdown>(true);
                    if (CustomFgRow)
                        FgToggle = CustomFgRow.GetComponentInChildren<Toggle>(true);
                    if (CustomSharpnessRow)
                    {
                        SharpnessSlider = CustomSharpnessRow.GetComponentInChildren<Slider>(true);
                        Transform valT = CustomSharpnessRow.transform.Find("Info/Value");
                        SharpnessValueText = valT != null ? valT.GetComponent<TMP_Text>() : null;
                    }
                    return;
                }

                int baseSiblingIndex = resRoot.transform.GetSiblingIndex() + 1;

                // 1. Quality Preset Row
                CustomPresetRow = UnityEngine.Object.Instantiate(resRoot, listParent);
                CustomPresetRow.name = "UpscalerPresetRow";
                CustomPresetRow.SetActive(true);
                CustomPresetRow.transform.SetSiblingIndex(baseSiblingIndex);

                PresetDropdown = PrepareRowDropdown(CustomPresetRow, "Upscaling Quality", new List<string>
                {
                    "Quality: Off (100% Native)",
                    "Quality: Native (DLAA / FSR Native)",
                    "Quality: Quality (67%)",
                    "Quality: Balanced (58%)",
                    "Quality: Performance (50%)",
                    "Quality: Ultra Performance (33%)"
                }, UpscalerUIPlugin.QualityPresetConfig.Value, (val) =>
                {
                    UpscalerUIPlugin.ApplyQualityPreset(val);
                });

                // 2. Backend Row
                CustomBackendRow = UnityEngine.Object.Instantiate(resRoot, listParent);
                CustomBackendRow.name = "UpscalerBackendRow";
                CustomBackendRow.SetActive(true);
                CustomBackendRow.transform.SetSiblingIndex(baseSiblingIndex + 1);

                BackendDropdown = PrepareRowDropdown(CustomBackendRow, "Upscaling Technology", new List<string>
                {
                    "Backend: AMD FSR 3.1",
                    "Backend: AMD FSR 4 (RDNA4 / FP8)",
                    "Backend: Intel XeSS",
                    "Backend: NVIDIA DLSS"
                }, UpscalerUIPlugin.BackendConfig.Value, (val) =>
                {
                    UpscalerUIPlugin.ApplyBackend(val);
                });

                // 3. Frame Generation Row
                FieldInfo togglePrefabField = AccessTools.Field(typeof(Valheim.SettingsGui.GraphicsSettings), "m_qualityTogglePrefab");
                GameObject togglePrefab = togglePrefabField != null ? togglePrefabField.GetValue(__instance) as GameObject : null;

                if (togglePrefab != null)
                {
                    CustomFgRow = UnityEngine.Object.Instantiate(togglePrefab, listParent);
                    CustomFgRow.name = "UpscalerFgRow";
                    CustomFgRow.SetActive(true);
                    CustomFgRow.transform.SetSiblingIndex(baseSiblingIndex + 2);

                    TMP_Text toggleLabel = CustomFgRow.GetComponentInChildren<TMP_Text>(true);
                    if (toggleLabel != null)
                    {
                        toggleLabel.text = "Frame Generation";
                    }

                    FgToggle = CustomFgRow.GetComponentInChildren<Toggle>(true);
                    if (FgToggle != null)
                    {
                        FgToggle.onValueChanged.RemoveAllListeners();
                        FgToggle.isOn = UpscalerUIPlugin.FrameGenConfig.Value;
                        FgToggle.onValueChanged.AddListener((isOn) =>
                        {
                            UpscalerUIPlugin.ApplyFrameGen(isOn);
                        });
                    }
                }

                // 4. Sharpness Slider Row
                FieldInfo sliderPrefabField = AccessTools.Field(typeof(Valheim.SettingsGui.GraphicsSettings), "m_qualitySliderPrefab");
                GameObject sliderPrefab = sliderPrefabField != null ? sliderPrefabField.GetValue(__instance) as GameObject : null;

                if (sliderPrefab != null)
                {
                    CustomSharpnessRow = UnityEngine.Object.Instantiate(sliderPrefab, listParent);
                    CustomSharpnessRow.name = "UpscalerSharpnessRow";
                    CustomSharpnessRow.SetActive(true);
                    CustomSharpnessRow.transform.SetSiblingIndex(baseSiblingIndex + 3);

                    Transform labelT = CustomSharpnessRow.transform.Find("Label");
                    TMP_Text labelText = labelT != null ? labelT.GetComponent<TMP_Text>() : null;
                    if (labelText != null)
                    {
                        labelText.text = "Sharpness";
                    }

                    Transform valT = CustomSharpnessRow.transform.Find("Info/Value");
                    SharpnessValueText = valT != null ? valT.GetComponent<TMP_Text>() : null;
                    if (SharpnessValueText != null)
                    {
                        SharpnessValueText.text = (UpscalerUIPlugin.SharpnessConfig.Value * 100f).ToString("F0") + "%";
                    }

                    SharpnessSlider = CustomSharpnessRow.GetComponentInChildren<Slider>(true);
                    if (SharpnessSlider != null)
                    {
                        SharpnessSlider.minValue = 0f;
                        SharpnessSlider.maxValue = 100f;
                        SharpnessSlider.wholeNumbers = true;
                        SharpnessSlider.onValueChanged.RemoveAllListeners();
                        SharpnessSlider.value = Mathf.Round(UpscalerUIPlugin.SharpnessConfig.Value * 100f);
                        SharpnessSlider.onValueChanged.AddListener((val) =>
                        {
                            float normalized = val / 100f;
                            UpscalerUIPlugin.ApplySharpness(normalized);
                            if (SharpnessValueText != null)
                            {
                                SharpnessValueText.text = val.ToString("F0") + "%";
                            }
                        });
                    }
                }

                // Recalculate layout height of listRoot
                FieldInfo listRootField = AccessTools.Field(typeof(Valheim.SettingsGui.GraphicsSettings), "m_listRoot");
                RectTransform listRoot = listRootField != null ? listRootField.GetValue(__instance) as RectTransform : null;
                FieldInfo vlgField = AccessTools.Field(typeof(Valheim.SettingsGui.GraphicsSettings), "m_verticalLayoutGroup");
                VerticalLayoutGroup vlg = vlgField != null ? vlgField.GetValue(__instance) as VerticalLayoutGroup : null;

                if (listRoot != null && vlg != null)
                {
                    float totalHeight = 0f;
                    for (int i = 0; i < listRoot.childCount; i++)
                    {
                        RectTransform child = listRoot.GetChild(i) as RectTransform;
                        if (child != null && child.gameObject.activeSelf)
                        {
                            totalHeight += child.rect.height + vlg.spacing;
                        }
                    }
                    listRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(listRoot);
                }

                Debug.Log("[ValheimUpscalerUI] In-game Settings -> Graphics rows successfully initialized.");
            }
            catch (Exception ex)
            {
                Debug.LogError("[ValheimUpscalerUI] Error injecting in-game settings rows: " + ex);
            }
        }

        private static TMP_Dropdown PrepareRowDropdown(GameObject rowGo, string labelText, List<string> options, int initialValue, UnityEngine.Events.UnityAction<int> onChange)
        {
            TMP_Dropdown dropdown = rowGo.GetComponentInChildren<TMP_Dropdown>(true);
            if (dropdown == null) return null;

            TMP_Text[] texts = rowGo.GetComponentsInChildren<TMP_Text>(true);
            foreach (var txt in texts)
            {
                if (!txt.transform.IsChildOf(dropdown.transform))
                {
                    txt.text = labelText;
                    break;
                }
            }

            Button[] buttons = rowGo.GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                if (!btn.transform.IsChildOf(dropdown.transform))
                {
                    btn.gameObject.SetActive(false);
                }
            }
            Toggle[] toggles = rowGo.GetComponentsInChildren<Toggle>(true);
            foreach (var tog in toggles)
            {
                if (!tog.transform.IsChildOf(dropdown.transform))
                {
                    tog.gameObject.SetActive(false);
                }
            }

            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            dropdown.value = initialValue;
            dropdown.onValueChanged.AddListener(onChange);

            return dropdown;
        }
    }

    [HarmonyPatch(typeof(Valheim.SettingsGui.GraphicsSettings), "OnTabOpen")]
    public static class GraphicsSettings_OnTabOpen_Patch
    {
        public static void Postfix(Valheim.SettingsGui.GraphicsSettings __instance)
        {
            try
            {
                if (GraphicsSettings_InitializeUI_Patch.CustomPresetRow)
                    GraphicsSettings_InitializeUI_Patch.CustomPresetRow.SetActive(true);

                if (GraphicsSettings_InitializeUI_Patch.CustomBackendRow)
                    GraphicsSettings_InitializeUI_Patch.CustomBackendRow.SetActive(true);

                if (GraphicsSettings_InitializeUI_Patch.CustomFgRow)
                    GraphicsSettings_InitializeUI_Patch.CustomFgRow.SetActive(true);

                if (GraphicsSettings_InitializeUI_Patch.CustomSharpnessRow)
                    GraphicsSettings_InitializeUI_Patch.CustomSharpnessRow.SetActive(true);
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(Valheim.SettingsGui.GraphicsSettings), "UpdateSettingAvailability")]
    public static class GraphicsSettings_UpdateSettingAvailability_Patch
    {
        public static void Postfix(Valheim.SettingsGui.GraphicsSettings __instance)
        {
            try
            {
                if (GraphicsSettings_InitializeUI_Patch.CustomPresetRow)
                    GraphicsSettings_InitializeUI_Patch.CustomPresetRow.SetActive(true);

                if (GraphicsSettings_InitializeUI_Patch.CustomBackendRow)
                    GraphicsSettings_InitializeUI_Patch.CustomBackendRow.SetActive(true);

                if (GraphicsSettings_InitializeUI_Patch.CustomFgRow)
                    GraphicsSettings_InitializeUI_Patch.CustomFgRow.SetActive(true);

                if (GraphicsSettings_InitializeUI_Patch.CustomSharpnessRow)
                    GraphicsSettings_InitializeUI_Patch.CustomSharpnessRow.SetActive(true);
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(Valheim.SettingsGui.GraphicsSettings), "UpdateUI")]
    public static class GraphicsSettings_UpdateUI_Patch
    {
        public static void Postfix(Valheim.SettingsGui.GraphicsSettings __instance)
        {
            try
            {
                if (GraphicsSettings_InitializeUI_Patch.PresetDropdown)
                {
                    GraphicsSettings_InitializeUI_Patch.PresetDropdown.SetValueWithoutNotify(UpscalerUIPlugin.QualityPresetConfig.Value);
                }
                if (GraphicsSettings_InitializeUI_Patch.BackendDropdown)
                {
                    GraphicsSettings_InitializeUI_Patch.BackendDropdown.SetValueWithoutNotify(UpscalerUIPlugin.BackendConfig.Value);
                }
                if (GraphicsSettings_InitializeUI_Patch.FgToggle)
                {
                    GraphicsSettings_InitializeUI_Patch.FgToggle.SetIsOnWithoutNotify(UpscalerUIPlugin.FrameGenConfig.Value);
                }
                if (GraphicsSettings_InitializeUI_Patch.SharpnessSlider)
                {
                    GraphicsSettings_InitializeUI_Patch.SharpnessSlider.SetValueWithoutNotify(Mathf.Round(UpscalerUIPlugin.SharpnessConfig.Value * 100f));
                }
                if (GraphicsSettings_InitializeUI_Patch.SharpnessValueText)
                {
                    GraphicsSettings_InitializeUI_Patch.SharpnessValueText.text = (UpscalerUIPlugin.SharpnessConfig.Value * 100f).ToString("F0") + "%";
                }
            }
            catch { }
        }
    }
}
