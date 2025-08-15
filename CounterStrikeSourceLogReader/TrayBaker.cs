using NotificationIcon.NET;

namespace CounterStrikeSourceLogReader;

public class TrayBaker
{
    public static void Create(CancellationTokenSource appCts)
    {
        Task.Run(() =>
        {
            try
            {
                if (!File.Exists("./icon")) File.Create("./icon").Close();

                MenuItem toggleMenuItem = new("тогле консоле")
                {
                    Click = (_, _) => Toggle(appCts)
                };
                MenuItem yameteMenuItem = new("хватит")
                {
                    Click = (_, _) => Yamete(appCts)
                };
                var icon = NotifyIcon.Create("./icon", [
                    toggleMenuItem,
                    yameteMenuItem
                ]);
                icon.Show(appCts.Token);
            }
            catch (Exception e)
            {
                Console.WriteLine("не удалось трейн");
                Console.WriteLine(e);
            }
        }, appCts.Token);
    }

    private static void Toggle(CancellationTokenSource cts)
    {
        Stealth.Toggle();
    }

    private static void Yamete(CancellationTokenSource cts)
    {
        cts.Cancel();
    }
}