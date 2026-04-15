# EasyDarkMode

A small **Windows 10 / 11** utility for switching **app and system** appearance between **light** and **dark** without the scheduling and extras of larger tools. It saves your current theme when you go dark, restores it on demand, and restarts **File Explorer** so taskbars (including on secondary monitors) pick up the change reliably.

## Features

- **Command-line** (`EasyDarkMode.exe`) for scripts and shortcuts: `night` and `restore`.
- **System tray** (`EasyDarkMode.Gui.exe`): double-click the icon to toggle, or use the context menu.
- **Restore** reapplies the last saved light/dark pair (or full light if there is no backup yet).
- **Custom tray / exe icon** via `Resources/TrayApp.ico` (see [Icon](#icon)).

## Requirements

- Windows 10 or 11.
- [.NET 8 **Desktop** Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) (Windows x64), unless you publish self-contained (see [Build](#build)).

## Build

From the repository root:

```powershell
dotnet build -c Release
```

Outputs (framework-dependent):

| Project        | Output (example path) |
|----------------|------------------------|
| CLI            | `EasyDarkMode/bin/Release/net8.0-windows/EasyDarkMode.exe` |
| Tray GUI       | `EasyDarkMode.Gui/bin/Release/net8.0-windows/EasyDarkMode.Gui.exe` |

Self-contained single-file example (no separate runtime install on the target PC):

```powershell
dotnet publish .\EasyDarkMode.Gui\EasyDarkMode.Gui.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Usage

### CLI

```powershell
# Save current theme (if not already fully dark) and switch to dark
.\EasyDarkMode.exe night

# Restore saved theme, or switch both to light if no backup exists
.\EasyDarkMode.exe restore
```

Aliases: `night` / `dark`; `restore` / `light` / `day`. Run with no arguments (or `help`) for usage text.

### Tray application

1. Run `EasyDarkMode.Gui.exe`.
2. Find the icon in the **notification area** (may be under **Show hidden icons** `^`).
3. **Double-click** (left button) to toggle dark ↔ restore.
4. **Right-click** for **Switch to light/dark mode** and **Exit**.

Only one GUI instance runs at a time (mutex).

### Start with Windows (optional)

Place a shortcut to `EasyDarkMode.Gui.exe` in the Startup folder (`shell:startup` from Run or File Explorer).

## Icon

- **Project default:** `EasyDarkMode.Gui/Resources/TrayApp.ico` is embedded as the application icon and copied next to the built `.exe`.
- **Tray only, no rebuild:** exit the app, replace `TrayApp.ico` beside `EasyDarkMode.Gui.exe`, then start again (the tray loads that file first).
- **Permanent change:** replace `Resources/TrayApp.ico`, then rebuild. Multi-size `.ico` files (e.g. 16, 20, 24, 32, 48 px) look best at different DPI.

## How it works

- Reads and writes `AppsUseLightTheme` and `SystemUsesLightTheme` under  
  `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize`.
- Broadcasts `WM_SETTINGCHANGE` with `ImmersiveColorSet` so the shell is notified.
- Restarts **Explorer** after a change so **multi-monitor taskbars** stay in sync (a common Windows limitation when only the registry is updated).

Backup file: `%LocalAppData%\EasyDarkMode\theme-backup.json`.

## What this is not

- **No scheduling**, per-app rules, wallpaper sync, or other automation (by design).
- **No Quick Settings tile:** Windows does not offer a supported public API for third-party tiles in the Windows 11 Quick Settings grid. The tray is the supported integration point.

## Solution layout

| Project            | Role |
|--------------------|------|
| `EasyDarkMode.Core` | Theme registry logic, backup, Explorer refresh. |
| `EasyDarkMode`      | Console entry point. |
| `EasyDarkMode.Gui`  | WinForms tray host. |

## License

Specify a license in this repository (for example MIT) if you plan to share or accept contributions.
