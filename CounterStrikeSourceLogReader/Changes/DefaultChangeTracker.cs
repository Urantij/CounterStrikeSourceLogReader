namespace CounterStrikeSourceLogReader.Changes;

public class DefaultChangeTracker
{
    private readonly FileSystemWatcher _fileSystemWatcher;

    public event Action? Changed;
    public event Action? Created;
    public event Action? Deleted;

    public DefaultChangeTracker(string path)
    {
        _fileSystemWatcher = new FileSystemWatcher(
            Path.GetDirectoryName(path) ?? throw new Exception("Путь какой то дебильный."),
            Path.GetFileName(path));
        // без файл нейма не видит что файл +тут или -тут
        _fileSystemWatcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.FileName;
    }

    public void Start()
    {
        // ну да второй старт сделает дуплики ивентов. ы.
        _fileSystemWatcher.Changed += (_, e) =>
        {
            if (e.ChangeType == WatcherChangeTypes.Changed) Changed?.Invoke();
        };
        _fileSystemWatcher.Created += (_, _) => Created?.Invoke();
        _fileSystemWatcher.Deleted += (_, _) => Deleted?.Invoke();
        _fileSystemWatcher.EnableRaisingEvents = true;
    }

    public void Stop()
    {
        _fileSystemWatcher.EnableRaisingEvents = false;
        _fileSystemWatcher.Dispose();
    }
}