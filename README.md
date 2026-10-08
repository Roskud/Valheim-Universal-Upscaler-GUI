# Valheim Universal Upscaler GUI

<p align="center">
  <img src="assets/banner.png" alt="Valheim Universal Upscaler Banner" width="850">
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Valheim-0.219+-orange?style=for-the-badge&logo=unity" alt="Valheim">
  <img src="https://img.shields.io/badge/BepInEx-5.4+-blue?style=for-the-badge" alt="BepInEx">
  <img src="https://img.shields.io/badge/AMD_FSR-3.1_%7C_4.0_(FP8)-red?style=for-the-badge&logo=amd" alt="AMD FSR">
  <img src="https://img.shields.io/badge/NVIDIA-DLSS-green?style=for-the-badge&logo=nvidia" alt="NVIDIA DLSS">
  <img src="https://img.shields.io/badge/Intel-XeSS-blue?style=for-the-badge&logo=intel" alt="Intel XeSS">
</p>

---

## 🇬🇧 English

### Overview
**Valheim Universal Upscaler GUI** is a full-featured in-game settings integration mod for Valheim that adds native UI controls directly inside **Settings ➔ Graphics** to configure modern upscaling technologies (AMD FSR 3.1, AMD FSR 4, Intel XeSS, NVIDIA DLSS), quality presets, frame generation, and sharpness on the fly.

No more tweaking `.ini` files or struggling with external overlays! Everything is seamlessly integrated into Valheim's graphics menu and accessible via an in-game hotkey HUD.

### Key Features
* **Native In-Game Graphics Menu**:
  * **Render Scale / Quality Presets**:
    * `Off` (100% Native game rendering)
    * `Native (DLAA / FSR Native AA)` (Ultra-sharp anti-aliasing at native resolution)
    * `Quality` (67% render scale — optimal balance of performance and visual fidelity)
    * `Balanced` (58% render scale)
    * `Performance` (50% render scale)
    * `Ultra Performance` (33% render scale)
  * **Upscaling Technology (Backend)**:
    * `AMD FSR 3.1`
    * `AMD FSR 4 (RDNA4 / FP8 Hardware Accelerated)` — recommended for AMD Radeon RX 9000 series!
    * `Intel XeSS`
    * `NVIDIA DLSS`
  * **Frame Generation**: One-click toggle for FSR / OptiFG frame generation.
  * **Sharpness**: Smooth slider from `0%` to `100%`.
* **In-Game Quick Overlay (`F7`)**:
  * Press **`F7`** anywhere in-game to toggle a floating quick-access HUD.
* **Compatibility**:
  * Fully compatible with **r2modman**, **Thunderstore Mod Manager**, and standard Steam installations.
  * Works alongside other popular mods (Jotunn, Epic MMO, Warfare, etc.).

---

### Installation

#### Via r2modman / Thunderstore Mod Manager (Recommended):
1. Install **Valheim Universal Upscaler GUI** directly from the mod manager.
2. Launch the game using **Start Modded**.

#### Manual Installation:
1. Ensure [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/) is installed.
2. Ensure OptiScaler / Valheim Universal Upscaler native files (`dxgi.dll`, `version.dll`) are in your Valheim root folder.
3. Extract `ValheimUpscalerUI.dll` into your `Valheim/BepInEx/plugins/` directory.
4. Launch Valheim with `-force-d3d12`.

---

## 🇷🇺 Русский

### Описание
**Valheim Universal Upscaler GUI** — это полноценный внутриигровой интерфейс для настройки апскейлинга нового поколения в Valheim. Мод добавляет официальные выпадающие списки и настройки прямо в раздел **«Настройки» ➔ «Графика»**, позволяя на лету управлять технологиями (AMD FSR 3.1, AMD FSR 4, Intel XeSS, NVIDIA DLSS), пресетами качества, генерацией кадров и резкостью.

Больше не нужно вручную редактировать конфигурационные файлы — все параметры доступны прямо в меню игры и через быстрый оверлей по горячей клавише!

### Основные возможности
* **Интеграция в настройки графики Valheim**:
  * **Пресеты качества масштабирования**:
    * `Выключено` (Стандартный нативный рендеринг)
    * `Нативное (DLAA / FSR Native)` (Максимальное качество сглаживания)
    * `Качество (Quality — 67%)` (Идеальный баланс высокого FPS и чёткости)
    * `Баланс (Balanced — 58%)`
    * `Быстродействие (Performance — 50%)`
    * `Ультра быстродействие (33%)`
  * **Технология апскейла (Backend)**:
    * `AMD FSR 3.1`
    * `AMD FSR 4 (RDNA4 / FP8)` — идеальный выбор для видеокарт AMD Radeon RX 9070 XT / RX 9000!
    * `Intel XeSS`
    * `NVIDIA DLSS`
  * **Генерация кадров (Frame Generation)**: Мгновенное включение/отключение OptiFG / FSR 3.1 FG.
  * **Резкость (Sharpness)**: Ползунок от `0%` до `100%`.
* **Быстрое оверлей-меню (`F7`)**:
  * Нажмите клавишу **`F7`** во время игры, чтобы открыть плавающую панель быстрых настроек.
* **Полная совместимость**:
  * Поддерживает запуск через **r2modman**, **Thunderstore**, а также чистый Steam.
  * Полностью совместим с популярными сборками модов (Jotunn, Therzie, Wacky и др.).

---

### Установка

#### Через r2modman / Thunderstore (Рекомендуется):
1. Найдите и установите **Valheim Universal Upscaler GUI** в менеджере модов.
2. Запустите игру через кнопку **Start Modded**.

#### Вручную:
1. Установите [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/).
2. Убедитесь, что файлы OptiScaler (`dxgi.dll`, `version.dll`) находятся в корне игры.
3. Поместите `ValheimUpscalerUI.dll` в папку `Valheim/BepInEx/plugins/ValheimUpscalerUI/`.
4. Запустите игру с параметром `-force-d3d12`.

---

## 📜 Credits & License
* Developed by **Roskud**.
* Powered by [BepInEx](https://github.com/BepInEx/BepInEx) and [HarmonyX](https://github.com/BepInEx/HarmonyX).
* Works in synergy with [OptiScaler](https://github.com/cdozdil/OptiScaler).
