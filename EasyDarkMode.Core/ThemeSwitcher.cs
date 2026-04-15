using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Win32;

namespace EasyDarkMode;

public static class ThemeSwitcher
{
    private const string PersonalizePath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string AppsKey = "AppsUseLightTheme";
    private const string SystemKey = "SystemUsesLightTheme";

    private static string StatePath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EasyDarkMode", "theme-backup.json");

    public static bool IsFullyDark()
    {
        using var key = Registry.CurrentUser.OpenSubKey(PersonalizePath, writable: false);
        if (key is null)
            return false;
        return ReadDword(key, AppsKey) == 0 && ReadDword(key, SystemKey) == 0;
    }

    public static void Night()
    {
        using var key = Registry.CurrentUser.OpenSubKey(PersonalizePath, writable: true)
            ?? throw new InvalidOperationException("Cannot open theme registry key.");

        var apps = ReadDword(key, AppsKey);
        var system = ReadDword(key, SystemKey);

        if (apps != 0 || system != 0)
        {
            var dir = Path.GetDirectoryName(StatePath)!;
            Directory.CreateDirectory(dir);
            var backup = new ThemeBackup(apps, system);
            File.WriteAllText(StatePath, JsonSerializer.Serialize(backup));
        }

        ApplyPersonalize(0, 0);
    }

    public static bool TryRestore()
    {
        ThemeBackup? backup = null;
        if (File.Exists(StatePath))
        {
            try
            {
                backup = JsonSerializer.Deserialize<ThemeBackup>(File.ReadAllText(StatePath));
            }
            catch
            {
                // ignore corrupt file
            }
        }

        if (backup is not null)
        {
            ApplyPersonalize(backup.AppsUseLightTheme, backup.SystemUsesLightTheme);
            try { File.Delete(StatePath); } catch { /* ignore */ }
            return true;
        }

        ApplyPersonalize(1, 1);
        return false;
    }

    private static int ReadDword(RegistryKey key, string name, int defaultValue = 1)
    {
        var v = key.GetValue(name);
        return v is int i ? i : defaultValue;
    }

    private static void WriteDword(RegistryKey key, string name, int value) =>
        key.SetValue(name, value, RegistryValueKind.DWord);

    private static void NotifyThemeChange()
    {
        const uint WM_SETTINGCHANGE = 0x001A;
        const uint SMTO_ABORTIFHUNG = 0x0002;
        NativeMethods.SendMessageTimeout(
            (IntPtr)0xffff,
            WM_SETTINGCHANGE,
            UIntPtr.Zero,
            "ImmersiveColorSet",
            SMTO_ABORTIFHUNG,
            500,
            out _);
    }

    private static void RestartExplorerSafely()
    {
        try
        {
            var explorerExe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
            if (!File.Exists(explorerExe))
                return;

            var explorers = Process.GetProcessesByName("explorer");
            try
            {
                foreach (var p in explorers)
                {
                    try
                    {
                        if (!p.HasExited)
                            p.Kill(entireProcessTree: false);
                    }
                    catch
                    {
                        // ignore per-process failures
                    }
                }

                foreach (var p in explorers)
                {
                    try
                    {
                        p.WaitForExit(3000);
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
            finally
            {
                foreach (var p in explorers)
                    p.Dispose();
            }

            if (Process.GetProcessesByName("explorer").Length == 0)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = explorerExe,
                    UseShellExecute = true,
                });
            }
        }
        catch
        {
            // best-effort; theme registry values are still updated
        }
    }

    private static void ApplyPersonalize(int appsLight, int systemLight)
    {
        using var key = Registry.CurrentUser.OpenSubKey(PersonalizePath, writable: true)
            ?? throw new InvalidOperationException("Cannot open theme registry key.");
        WriteDword(key, AppsKey, appsLight);
        WriteDword(key, SystemKey, systemLight);
        NotifyThemeChange();
        RestartExplorerSafely();
    }

    private static class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            uint Msg,
            UIntPtr wParam,
            string lParam,
            uint fuFlags,
            uint uTimeout,
            out UIntPtr lpdwResult);
    }
}

public sealed record ThemeBackup(int AppsUseLightTheme, int SystemUsesLightTheme);
