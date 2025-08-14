namespace CounterStrikeSourceLogReader;

public class KillerInstinct
{
    private readonly CancellationTokenSource _appCts;
    private readonly TimeSpan _time;

    private CancellationTokenSource _cts = new();

    /// <summary>
    ///     Создаёт киллера. Не запускает, юзай <see cref="Create" /> или <see cref="Renew" />
    /// </summary>
    /// <param name="time"></param>
    /// <param name="appCts"></param>
    public KillerInstinct(TimeSpan time, CancellationTokenSource appCts)
    {
        _time = time;
        _appCts = appCts;
    }

    public void Renew()
    {
        // одно лишнее срабатывание в начале, зато меньше строк
        _cts.Cancel();
        _cts = new CancellationTokenSource();
        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(_time, _cts.Token);
            }
            catch
            {
                // отменили
                return;
            }

            Console.WriteLine("таймер вышел, закругляемся");
            await _appCts.CancelAsync();
        });
    }

    /// <summary>
    ///     Создаёт и запускает киллера
    /// </summary>
    /// <param name="time"></param>
    /// <param name="appCts"></param>
    /// <returns></returns>
    public static KillerInstinct Create(TimeSpan time, CancellationTokenSource appCts)
    {
        KillerInstinct killer = new(time, appCts);

        killer.Renew();

        return killer;
    }
}