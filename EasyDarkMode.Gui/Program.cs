using System.Threading;
using EasyDarkMode;

namespace EasyDarkMode.Gui;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        using var mutex = new Mutex(true, @"Local\EasyDarkMode_Gui", out var created);
        if (!created)
        {
            MessageBox.Show(
                "EasyDarkMode is already running.",
                "EasyDarkMode",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        ApplicationConfiguration.Initialize();
        using var tray = new TrayApplicationContext();
        Application.Run(tray);
    }
}
