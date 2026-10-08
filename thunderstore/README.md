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

No more tweaking `.ini` files or struggling with external overlays! Everything is seamlessly integrated into Valheim's graphics menu and accessible via an in-game hotkey HUD (**F7**).

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
    * `AMD FSR 4 (RDNA4 / FP8 Hardware Accelerated)` — recommended for AMD Radeon RX 9070 XT / RX 9000 series!
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

> [!IMPORTANT]
> **DirectX 12 Requirement**: Valheim runs in DirectX 11 by default. Modern upscalers (FSR, XeSS, DLSS) require DirectX 12.
> In your Steam Library, right-click **Valheim ➔ Properties ➔ General ➔ Launch Options** and enter:
> ```text
> -force-d3d12
> ```

#### 🚀 Option 1: Complete 1-Click All-in-One Setup (Recommended)
Everything pre-configured in a single download (OptiScaler engine, FSR 4 / XeSS runtimes, Doorstop, and GUI mod).
1. Go to [GitHub Releases](https://github.com/Roskud/Valheim-Universal-Upscaler-GUI/releases) and download **`Valheim_Universal_Upscaler_Full_Setup_v1.0.0.zip`**.
2. Extract the contents directly into your Valheim game folder (where `valheim.exe` is located, e.g. `steamapps\common\Valheim`).
3. Add `-force-d3d12` to Valheim's Steam Launch Options.
4. Launch Valheim (via Steam or mod manager) and enjoy!

#### 📦 Option 2: Via r2modman / Thunderstore Mod Manager
Because r2modman installs plugins strictly into `BepInEx/plugins/` and cannot modify game-root DirectX files:
1. In **r2modman**, install **`Valheim_Universal_Upscaler_GUI`** (it automatically installs `BepInExPack_Valheim`).
2. Drop the root DirectX hook files into the game folder:
   * Download the root files from our [GitHub Releases](https://github.com/Roskud/Valheim-Universal-Upscaler-GUI/releases) (`Valheim_Universal_Upscaler_Full_Setup_v1.0.0.zip`) and place `dxgi.dll`, `version.dll`, and `OptiScaler.ini` into your `Valheim` game folder.
3. Add `-force-d3d12` to Valheim's Steam Launch Options.
4. In r2modman, click the blue button **Start Modded** to launch!

---

## 🇷🇺 Русский

### Описание
**Valheim Universal Upscaler GUI** — это полноценный внутриигровой интерфейс для настройки апскейлинга нового поколения в Valheim. Мод добавляет официальные выпадающие списки и настройки прямо в раздел **«Настройки» ➔ «Графика»**, позволяя на лету управлять технологиями (AMD FSR 3.1, AMD FSR 4, Intel XeSS, NVIDIA DLSS), пресетами качества, генерацией кадров и резкостью.

Больше не нужно вручную редактировать конфигурационные файлы — все параметры доступны прямо в меню игры и через быстрый оверлей по горячей клавише (**F7**)!

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

> [!IMPORTANT]
> **Требуется DirectX 12**: Valheim по умолчанию запускается в режиме DirectX 11. Современным апскейлерам (FSR, XeSS, DLSS) необходим DirectX 12.
> В библиотеке Steam нажмите правой кнопкой на **Valheim ➔ Свойства ➔ Общие ➔ Параметры запуска** и впишите:
> ```text
> -force-d3d12
> ```

#### 🚀 Вариант 1: Установка «Всё в одном» в 1 клик (Рекомендуется)
Все необходимые файлы (движок OptiScaler, библиотеки FSR 4 / XeSS, загрузчик BepInEx и наш GUI-мод) собраны в одном готовом архиве:
1. Перейдите в [Релизы на GitHub](https://github.com/Roskud/Valheim-Universal-Upscaler-GUI/releases) и скачайте **`Valheim_Universal_Upscaler_Full_Setup_v1.0.0.zip`**.
2. Распакуйте содержимое архива в папку игры Valheim (где находится `valheim.exe`, например: `C:\Program Files (x86)\Steam\steamapps\common\Valheim`).
3. В свойствах Valheim в Steam в параметрах запуска укажите `-force-d3d12`.
4. Запустите Valheim и играйте! Настройки появятся в **Настройки ➔ Графика**, а оверлей открывается на **F7**.

#### 📦 Вариант 2: Через r2modman / Thunderstore Mod Manager
Менеджер r2modman устанавливает моды строго в изолированную папку `BepInEx/plugins/` и не может модифицировать системные файлы DirectX в корне игры Steam:
1. В **r2modman** нажмите **Install with Mod Manager** для мода **`Valheim_Universal_Upscaler_GUI`**.
2. Поместите системные файлы апскейлера в корень игры:
   * Скачайте базовые файлы из нашего [Full Setup архива на GitHub](https://github.com/Roskud/Valheim-Universal-Upscaler-GUI/releases) и поместите `dxgi.dll`, `version.dll` и `OptiScaler.ini` в папку `Valheim`.
3. В параметрах запуска Steam укажите `-force-d3d12`.
4. В r2modman обязательно запускайте игру через синюю кнопку **Start Modded**!

---

## 📜 Credits & License
* Developed by **Roskud**.
* Powered by [BepInEx](https://github.com/BepInEx/BepInEx) and [HarmonyX](https://github.com/BepInEx/HarmonyX).
* Works in synergy with [OptiScaler](https://github.com/cdozdil/OptiScaler).
