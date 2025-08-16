using CounterStrikeSourceLogReader.Changes;

namespace CounterStrikeSourceLogReader;

public class MyCoolReader
{
    private readonly KillerInstinct _killer;
    private readonly string _path;
    private readonly DefaultChangeTracker _tracker;

    private FileStream? _fs;
    private StreamReader? _sr;

    public MyCoolReader(string path, DefaultChangeTracker tracker, KillerInstinct killer)
    {
        _path = path;
        _tracker = tracker;
        _killer = killer;

        tracker.Created += LogCreated;
        tracker.Deleted += LogDeleted;
        tracker.Changed += LogChanged;
    }

    public event Action<string>? GotLine;

    public void Start()
    {
        if (File.Exists(_path)) SetupRead();
    }

    public void Stop()
    {
        _tracker.Created -= LogCreated;
        _tracker.Deleted -= LogDeleted;
        _tracker.Changed -= LogChanged;

        RemoveRead();
    }

    private void LogCreated()
    {
        SetupRead();
    }

    private void LogDeleted()
    {
        RemoveRead();
    }

    private void LogChanged()
    {
        _killer.Renew();

        Console.WriteLine("Файл изменился, попробуем почитать...");

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