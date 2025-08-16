using NotificationIcon.NET;

namespace CounterStrikeSourceLogReader;

public class TrayBaker
{
    public static void Create(Temptation temptation, MyCoolWriter writer, CancellationTokenSource appCts)
    {
        Task.Run(() =>
        {
            try
            {
                if (!File.Exists("./icon")) File.Create("./icon").Close();

                MenuItem clearMenuItem = new("ПОЧИСТИТЬ")
                {
                    Click = (_, _) => Clear(temptation, writer)
                };
                MenuItem toggleMenuItem = new("тогле консоле")
                {
                    Click = (_, _) => Toggle()
                };
                MenuItem yameteMenuItem = new("хватит")
                {
                    Click = (_, _) => Yamete(appCts)
                };
                var icon = NotifyIcon.Create("./icon", [
                    clearMenuItem,
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

    private static void Clear(Temptation temptation, MyCoolWriter writer)
    {
        temptation.Clear();
        writer.Write("");
    }

    private static void Toggle()
    {
        Stealth.Toggle();
    }

    private static void Yamete(CancellationTokenSource cts)
    {
        cts.Cancel();
    }
}