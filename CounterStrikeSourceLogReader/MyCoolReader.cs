namespace CounterStrikeSourceLogReader;

public class MyCoolReader
{
    private readonly KillerInstinct _killer;
    private readonly string _path;
    private readonly FileSystemWatcher _watcher;

    private FileStream? _fs;
    private StreamReader? _sr;

    public MyCoolReader(string path, FileSystemWatcher watcher, KillerInstinct killer)
    {
        _path = path;
        _watcher = watcher;
        _killer = killer;

        watcher.Created += LogCreated;
        watcher.Deleted += LogDeleted;
        watcher.Changed += LogChanged;
    }

    public event Action<string>? GotLine;

    public void Start()
    {
        if (File.Exists(_path)) SetupRead();
    }

    public void Stop()
    {
        _watcher.Created -= LogCreated;
        _watcher.Deleted -= LogDeleted;
        _watcher.Changed -= LogChanged;

        RemoveRead();
    }

    private void LogCreated(object sender, FileSystemEventArgs e)
    {
        SetupRead();
    }

    private void LogDeleted(object sender, FileSystemEventArgs e)
    {
        RemoveRead();
    }

    private void LogChanged(object sender, FileSystemEventArgs e)
    {
        _killer.Renew();

        var line = _sr?.ReadLine();
        while (line != null)
        {
            GotLine?.Invoke(line);

            line = _sr?.ReadLine();
        }
    }

    private void SetupRead()
    {
        // Что интересно, без ридврайт фаил шеира всё работает на линухе, но на винде кидает ексепш
        _fs = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        _sr = new StreamReader(_fs);
        _sr.ReadToEnd();
    }

    private void RemoveRead()
    {
        _sr?.Dispose();
        _fs?.Dispose();

        _fs = null;
        _sr = null;
    }
}