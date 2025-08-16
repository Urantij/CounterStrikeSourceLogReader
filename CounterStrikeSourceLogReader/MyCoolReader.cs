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
        // Я не знаю, насколько это реальный сценарий. Мне всё равно.
        // Но чтобы не читать кусок строки до её завершения, нужно дождаться, когда будет \n или чето в конце строки
        // это не было бы проблемой, если бы я ловил ивент, что файл изменился, а не читал бы рандомно его. СУКА
        // но это так геморно делать што й забью хуй. й устал

        // хотй... может как то по длине можно сверить...
        // нет, он двигает вперёд курсор перепрыгивая. ток если сравнивать последний символ

        if (_fs == null || _sr == null)
            return;

        long currentPosition = _sr.BaseStream.Position;

        while (true)
        {
            // это вроде всё синхронное, так что изменений быть не должно с sr
            string? line = _sr.ReadLine();

            if (line != null)
            {
                _fs.Position--;
                int revByte = _fs.ReadByte();

                if (revByte == 10) // \n
                {
                    currentPosition = _sr.BaseStream.Position;

                    _killer.Renew();

                    GotLine?.Invoke(line);
                }
                else
                {
                    _sr.BaseStream.Position = currentPosition;
                    _sr.DiscardBufferedData();
                    break;
                }
            }
            else
            {
                _sr.BaseStream.Position = currentPosition;
                _sr.DiscardBufferedData();
                break;
            }
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