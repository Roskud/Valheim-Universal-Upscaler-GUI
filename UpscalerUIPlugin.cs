using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ValheimUpscalerUI
{
    [BepInPlugin("com.valheim.upscalerui", "Valheim Upscaler Settings UI", "1.1.0")]
    [BepInDependency("dev.valheim.upscaler.inject", BepInDependency.DependencyFlags.SoftDependency)]
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
        private Rect _quickMenuRect = new Rect(40, 40, 420, 440);

        private void Awake()
        {
            Instance = this;

            QualityPresetConfig = Config.Bind("GraphicsSettings", "QualityPreset", 2, "0: Off, 1: Native (100%), 2: Quality (67%), 3: Balanced (58%), 4: Performance (50%), 5: Ultra Performance (33%)");
            BackendConfig = Config.Bind("GraphicsSettings", "Backend", 1, "0: FSR 3.1, 1: FSR 4 (RDNA4), 2: XeSS, 3: DLSS");
            FrameGenConfig = Config.Bind("GraphicsSettings", "FrameGeneration", false, "Frame Generation enabled");
            SharpnessConfig = Config.Bind("GraphicsSettings", "Sharpness", 0.5f, "Upscaler sharpness (0.0 to 1.0)");
            MenuHotkeyConfig = Config.Bind("Input", "MenuHotkey", KeyCode.F7, "Hotkey to toggle the quick Upscaler overlay menu");

            _harmony = new Harmony("com.valheim.upscalerui");
            _harmony.PatchAll();

            Debug.Log("[ValheimUpscalerUI] In-Game Upscaler UI Mod loaded successfully.");
        }

        private void Start()
        {
            ApplyAllSettings();
        }

        private void Update()
        {
            if (Input.GetKeyDown(MenuHotkeyConfig.Value))
            {
                _showQuickMenu = !_showQuickMenu;
                if (_showQuickMenu)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }

        private void OnGUI()
        {
            if (!_showQuickMenu) return;

            GUI.skin = null; // Standard Unity IMGUI skin
            _quickMenuRect = GUILayout.Window(998822, _quickMenuRect, DrawQuickMenuWindow, "Настройки Valheim Upscaler (FSR / XeSS / DLSS)");
        }

        private void DrawQuickMenuWindow(int windowId)
        {
            GUILayout.BeginVertical();

            GUILayout.Label("Видеокарта: " + SystemInfo.graphicsDeviceName, GUILayout.ExpandWidth(true));
            GUILayout.Space(5);

            GUILayout.Label("<b>Пресет масштабирования (Качество):</b>");
            string[] presetLabels = new string[]
            {
                "Выкл",
                "Нативное (100%)",
                "Качество (67%)",
                "Баланс (58%)",
                "Производ. (50%)",
                "Ультра (33%)"
            };

            int currentPreset = QualityPresetConfig.Value;
            int newPreset = GUILayout.SelectionGrid(currentPreset, presetLabels, 3);
            if (newPreset != currentPreset)
            {
                ApplyQualityPreset(newPreset);
            }

            GUILayout.Space(10);
            GUILayout.Label("<b>Технология апскейла (Backend):</b>");
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
            bool newFg = GUILayout.Toggle(currentFg, "  Генерация кадров (Frame Generation)");
            if (newFg != currentFg)
            {
                ApplyFrameGen(newFg);
            }

            GUILayout.Space(10);
            GUILayout.Label("Резкость (Sharpness): " + (SharpnessConfig.Value * 100f).ToString("F0") + "%");
            float newSharpness = GUILayout.HorizontalSlider(SharpnessConfig.Value, 0f, 1f);
            if (Math.Abs(newSharpness - SharpnessConfig.Value) > 0.02f)
            {
                ApplySharpness(newSharpness);
            }

            GUILayout.Space(15);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Применить все", GUILayout.Height(30)))
            {
                ApplyAllSettings();
            }
            if (GUILayout.Button("Закрыть (F7)", GUILayout.Height(30)))
            {
                _showQuickMenu = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, 10000, 25));
        }

        public static void ApplyQualityPreset(int presetIndex)
        {
            QualityPresetConfig.Value = presetIndex;

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
                    Debug.Log("[ValheimUpscalerUI] Upscaler disabled (Native game rendering).");
                    return;
                }

                // Enable Upscaler injection
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
                        // 0: Native (100%), 2: Quality (67%), 3: Balanced (58%), 4: Performance (50%), 5: UltraPerformance (33%)
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
                Debug.Log("[ValheimUpscalerUI] Applied Quality Preset index: " + presetIndex);
            }
            catch (Exception ex)
            {
                Debug.LogError("[ValheimUpscalerUI] Error applying quality preset: " + ex);
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
                            object fsr4Val = Enum.ToObject(enumType, 2); // UpscalerBackend.FSR4 = 2
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
                            object dlssVal = Enum.ToObject(enumType, 1); // UpscalerBackend.DLSS = 1
                            PropertyInfo prop = AccessTools.Property(cfgBackend.GetType(), "Value");
                            if (prop != null) prop.SetValue(cfgBackend, dlssVal, null);
                        }
                    }
                }
                else // Auto (FSR 3.1 or XeSS)
                {
                    if (injectPluginType != null)
                    {
                        FieldInfo cfgBackendField = AccessTools.Field(injectPluginType, "ConfigBackend");
                        object cfgBackend = cfgBackendField != null ? cfgBackendField.GetValue(null) : null;
                        if (cfgBackend != null)
                        {
                            Type enumType = cfgBackend.GetType().GetGenericArguments()[0];
                            object autoVal = Enum.ToObject(enumType, 0); // UpscalerBackend.Auto = 0
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
            // Dx12Upscaler in [Upscalers]
            string upscalerCode = backendCode;
            if (backendIndex == 1) // FSR4 uses fsr31 loader with UpscalerIndex=0
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
            paths.Add(Path.Combine(gameDir, "OptiScaler.ini"));

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string r2Ini = Path.Combine(appData, @"r2modmanPlus-local\Valheim\profiles\Default\OptiScaler.ini");
            paths.Add(r2Ini);

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
                if (CustomPresetRow != null && CustomPresetRow.gameObject != null) return;

                FieldInfo resRootField = AccessTools.Field(typeof(Valheim.SettingsGui.GraphicsSettings), "m_resolutionRoot");
                GameObject resRoot = resRootField != null ? resRootField.GetValue(__instance) as GameObject : null;
                if (resRoot == null) return;

                Transform listParent = resRoot.transform.parent;
                if (listParent == null) return;

                int baseSiblingIndex = resRoot.transform.GetSiblingIndex() + 1;

                // 1. Quality Preset Row (Clone of m_resolutionRoot)
                CustomPresetRow = UnityEngine.Object.Instantiate(resRoot, listParent);
                CustomPresetRow.name = "UpscalerPresetRow";
                CustomPresetRow.SetActive(true);
                CustomPresetRow.transform.SetSiblingIndex(baseSiblingIndex);

                PresetDropdown = PrepareRowDropdown(CustomPresetRow, "Масштабирование (Разрешение)", new List<string>
                {
                    "Масштаб: Выключено (100% Native)",
                    "Масштаб: Нативное (DLAA / FSR Native)",
                    "Масштаб: Качество (Quality - 67%)",
                    "Масштаб: Баланс (Balanced - 58%)",
                    "Масштаб: Быстродействие (Performance - 50%)",
                    "Масштаб: Ультра быстродействие (33%)"
                }, UpscalerUIPlugin.QualityPresetConfig.Value, (val) =>
                {
                    UpscalerUIPlugin.ApplyQualityPreset(val);
                });

                // 2. Backend Row (Clone of m_resolutionRoot)
                CustomBackendRow = UnityEngine.Object.Instantiate(resRoot, listParent);
                CustomBackendRow.name = "UpscalerBackendRow";
                CustomBackendRow.SetActive(true);
                CustomBackendRow.transform.SetSiblingIndex(baseSiblingIndex + 1);

                BackendDropdown = PrepareRowDropdown(CustomBackendRow, "Технология масштабирования", new List<string>
                {
                    "Технология: AMD FSR 3.1",
                    "Технология: AMD FSR 4 (RDNA4 / FP8)",
                    "Технология: Intel XeSS",
                    "Технология: NVIDIA DLSS"
                }, UpscalerUIPlugin.BackendConfig.Value, (val) =>
                {
                    UpscalerUIPlugin.ApplyBackend(val);
                });

                // 3. Frame Generation Row (Clone of m_qualityTogglePrefab if available, else m_resolutionRoot toggle)
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
                        toggleLabel.text = "Генерация кадров (Frame Generation)";
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

                // 4. Sharpness Slider Row (Clone of m_qualitySliderPrefab if available)
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
                        labelText.text = "Резкость (Sharpness)";
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

            // Set Row Label Text (the TMP_Text that is NOT part of dropdown)
            TMP_Text[] texts = rowGo.GetComponentsInChildren<TMP_Text>(true);
            foreach (var txt in texts)
            {
                if (!txt.transform.IsChildOf(dropdown.transform))
                {
                    txt.text = labelText;
                    break;
                }
            }

            // Hide any extraneous buttons or toggles in this row
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

            // Configure Dropdown
            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            dropdown.value = initialValue;
            dropdown.onValueChanged.AddListener(onChange);

            return dropdown;
        }
    }

    [HarmonyPatch(typeof(Valheim.SettingsGui.GraphicsSettings), "UpdateSettingAvailability")]
    public static class GraphicsSettings_UpdateSettingAvailability_Patch
    {
        public static void Postfix(Valheim.SettingsGui.GraphicsSettings __instance)
        {
            try
            {
                if (GraphicsSettings_InitializeUI_Patch.CustomPresetRow != null)
                    GraphicsSettings_InitializeUI_Patch.CustomPresetRow.SetActive(true);

                if (GraphicsSettings_InitializeUI_Patch.CustomBackendRow != null)
                    GraphicsSettings_InitializeUI_Patch.CustomBackendRow.SetActive(true);

                if (GraphicsSettings_InitializeUI_Patch.CustomFgRow != null)
                    GraphicsSettings_InitializeUI_Patch.CustomFgRow.SetActive(true);

                if (GraphicsSettings_InitializeUI_Patch.CustomSharpnessRow != null)
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
                if (GraphicsSettings_InitializeUI_Patch.PresetDropdown != null)
                {
                    GraphicsSettings_InitializeUI_Patch.PresetDropdown.SetValueWithoutNotify(UpscalerUIPlugin.QualityPresetConfig.Value);
                }
                if (GraphicsSettings_InitializeUI_Patch.BackendDropdown != null)
                {
                    GraphicsSettings_InitializeUI_Patch.BackendDropdown.SetValueWithoutNotify(UpscalerUIPlugin.BackendConfig.Value);
                }
                if (GraphicsSettings_InitializeUI_Patch.FgToggle != null)
                {
                    GraphicsSettings_InitializeUI_Patch.FgToggle.SetIsOnWithoutNotify(UpscalerUIPlugin.FrameGenConfig.Value);
                }
                if (GraphicsSettings_InitializeUI_Patch.SharpnessSlider != null)
                {
                    GraphicsSettings_InitializeUI_Patch.SharpnessSlider.SetValueWithoutNotify(Mathf.Round(UpscalerUIPlugin.SharpnessConfig.Value * 100f));
                }
                if (GraphicsSettings_InitializeUI_Patch.SharpnessValueText != null)
                {
                    GraphicsSettings_InitializeUI_Patch.SharpnessValueText.text = (UpscalerUIPlugin.SharpnessConfig.Value * 100f).ToString("F0") + "%";
                }
            }
            catch { }
        }
    }
}
