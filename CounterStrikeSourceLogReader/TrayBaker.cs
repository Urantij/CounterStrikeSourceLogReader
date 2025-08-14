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

                var menuItem = new MenuItem("хватит")
                {
                    Click = (_, _) => Click(appCts)
                };
                var icon = NotifyIcon.Create("./icon", [
                    menuItem
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

    private static void Click(CancellationTokenSource cts)
    {
        cts.Cancel();
    }
}