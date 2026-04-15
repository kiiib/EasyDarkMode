using System.ComponentModel;
using System.Drawing;
using EasyDarkMode;

namespace EasyDarkMode.Gui;

// Runs in the Windows notification area (system tray), not as a floating window.
internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ToolStripMenuItem _toggleItem;

    public TrayApplicationContext()
    {
        _notifyIcon = new NotifyIcon
        {
            Icon = LoadTrayIcon(),
            Visible = true,
            Text = BuildTooltip(),
        };

        var menu = new ContextMenuStrip();
        _toggleItem = new ToolStripMenuItem("Toggle", null, (_, _) => ToggleTheme());
        menu.Items.Add(_toggleItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => ExitThread());
        menu.Opening += OnMenuOpening;

        _notifyIcon.ContextMenuStrip = menu;
        _notifyIcon.MouseDoubleClick += OnTrayDoubleClick;
    }

    private static Icon LoadTrayIcon()
    {
        // Prefer TrayApp.ico next to the .exe so you can swap icons without rebuilding.
        try
        {
            var bundled = Path.Combine(AppContext.BaseDirectory, "TrayApp.ico");
            if (File.Exists(bundled))
                return new Icon(bundled, 16, 16);
        }
        catch
        {
            // ignore
        }

        try
        {
            var path = Application.ExecutablePath;
            if (!string.IsNullOrEmpty(path))
            {
                var extracted = Icon.ExtractAssociatedIcon(path);
                if (extracted is not null)
                    return extracted;
            }
        }
        catch
        {
            // ignore
        }

        // Do not assign SystemIcons.Application directly — NotifyIcon.Dispose would dispose the shared system icon.
        return new Icon(SystemIcons.Application, 16, 16);
    }

    private string BuildTooltip()
    {
        try
        {
            return ThemeSwitcher.IsFullyDark()
                ? "EasyDarkMode — dark (double-click for light)"
                : "EasyDarkMode — light (double-click for dark)";
        }
        catch
        {
            return "EasyDarkMode";
        }
    }

    private void OnMenuOpening(object? sender, CancelEventArgs e) =>
        _toggleItem.Text = ThemeSwitcher.IsFullyDark()
            ? "Switch to light mode"
            : "Switch to dark mode";

    private void OnTrayDoubleClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            ToggleTheme();
    }

    private void ToggleTheme()
    {
        try
        {
            if (ThemeSwitcher.IsFullyDark())
                ThemeSwitcher.TryRestore();
            else
                ThemeSwitcher.Night();

            _notifyIcon.Text = BuildTooltip();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "EasyDarkMode", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }

        base.Dispose(disposing);
    }
}
