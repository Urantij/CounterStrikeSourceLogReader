using System.Runtime.InteropServices;

namespace CounterStrikeSourceLogReader;

// https://stackoverflow.com/a/3571628
public class Stealth
{
    public static bool IsShown { get; private set; } = true;

    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    public static void Toggle()
    {
        if (!OperatingSystem.IsWindows()) return;

        if (IsShown)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    public static void Hide()
    {
        if (!OperatingSystem.IsWindows()) return;

        IsShown = false;

        var handle = GetConsoleWindow();
        ShowWindow(handle, SW_HIDE);
    }

    public static void Show()
    {
        if (!OperatingSystem.IsWindows()) return;

        IsShown = true;

        var handle = GetConsoleWindow();
        ShowWindow(handle, SW_SHOW);
    }
}