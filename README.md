# EasyDarkMode

Minimal Windows light/dark switcher (CLI + tray).

## CLI

Run `EasyDarkMode.exe` from a Release build (see `EasyDarkMode/bin/Release/net8.0-windows/` after `dotnet build -c Release`).

```powershell
.\EasyDarkMode.exe night     # go dark (saves current theme first)
.\EasyDarkMode.exe restore   # restore saved theme, or light if none
```

`dark` works like `night`; `light` / `day` work like `restore`. Run with no args for help.

## Tray icon

Run `EasyDarkMode.Gui.exe` (`EasyDarkMode.Gui/bin/Release/net8.0-windows/` after a Release build). The icon lives in the taskbar **notification area** (use **^** if it is hidden).

- **Double-click** the icon: switch between dark and restore.
- **Right-click**: choose light/dark or **Exit**.

## Changing the icon

Replace `TrayApp.ico` next to `EasyDarkMode.Gui.exe` and restart the app, or replace `EasyDarkMode.Gui/Resources/TrayApp.ico` and rebuild so the exe icon updates too.
