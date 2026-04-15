namespace EasyDarkMode;

internal static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        try
        {
            switch (args[0].ToLowerInvariant())
            {
                case "night":
                case "dark":
                    ThemeSwitcher.Night();
                    Console.WriteLine("Dark mode enabled.");
                    return 0;
                case "restore":
                case "light":
                case "day":
                    var fromBackup = ThemeSwitcher.TryRestore();
                    Console.WriteLine(fromBackup ? "Previous theme restored." : "No backup found; switched to light mode.");
                    return 0;
                case "help":
                case "-h":
                case "--help":
                    PrintUsage();
                    return 0;
                default:
                    Console.Error.WriteLine($"Unknown command: {args[0]}");
                    PrintUsage();
                    return 1;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine(
            """
            EasyDarkMode — minimal Windows light/dark switcher.

            Usage:
              EasyDarkMode night     Save current theme (if not already dark) and enable dark mode.
              EasyDarkMode restore   Restore the theme saved by the last 'night' command (or light if none).

            After a change, Windows Explorer restarts briefly so taskbars on all monitors match the new mode.

            For a system tray icon (notification area), run EasyDarkMode.Gui.exe.

            Examples:
              EasyDarkMode night
              EasyDarkMode restore
            """);
    }
}
