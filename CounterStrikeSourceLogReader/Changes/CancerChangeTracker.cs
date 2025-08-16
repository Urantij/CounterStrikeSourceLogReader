namespace CounterStrikeSourceLogReader.Changes;

// Ну это я виноват, что в микрософт бездари ебаные работают?
public class CancerChangeTracker : DefaultChangeTracker
{
    private readonly string _path;
    private readonly TimeSpan _checkDelay;

    // мне надоело ломать комедию.
    private readonly CancellationTokenSource _cts = new();

    public CancerChangeTracker(string path, TimeSpan checkDelay) : base(path)
    {
        _path = path;
        _checkDelay = checkDelay;
    }

    public override void Start()
    {
        bool fileExist = File.Exists(_path);

        FileSystemWatcher.Created += (_, _) => { fileExist = true; };
        FileSystemWatcher.Deleted += (_, _) => { fileExist = false; };

        Task.Run(async () =>
        {
            while (!_cts.IsCancellationRequested)
            {
                await Task.Delay(_checkDelay, _cts.Token);

                if (fileExist)
                {
                    // Да. Ну да. Ну да да. Да. Да. Ну да.
                    // пизда.
                    OnChanged();
                }
            }
        });

        base.Start();
    }

    public override void Stop()
    {
        _cts.Cancel();

        base.Stop();
    }
}