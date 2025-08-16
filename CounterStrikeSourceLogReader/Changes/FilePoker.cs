namespace CounterStrikeSourceLogReader.Changes;

// When a file is opened with a FileShare.Read mode, changes to it might not trigger FileSystemWatcher events until the file is closed or explicitly read.
// По какой то причине этот системный трекер просто не видит изменения, когда в файл пишет каесочка.
// Когда руками пишешь из блокнота, видит.
// И это только на винде. Круто. Здорово. Чудесно.
public class FilePoker
{
    private readonly string _path;
    private readonly TimeSpan _delay;

    private CancellationTokenSource _cts = new();

    public FilePoker(string path, TimeSpan delay)
    {
        _path = path;
        _delay = delay;
    }

    public void Start()
    {
        CancellationTokenSource cts = _cts = new CancellationTokenSource();

        Task.Run(async () =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_delay, cts.Token);
                }
                catch
                {
                    return;
                }

                try
                {
                    await File.ReadAllBytesAsync(_path, cts.Token);
                }
                catch
                {
                }
            }
        }, cts.Token);
    }

    public void Stop()
    {
        // будет ексепшн если стопнуть несколько раз. ы.
        _cts.Cancel();
        _cts.Dispose();
    }
}