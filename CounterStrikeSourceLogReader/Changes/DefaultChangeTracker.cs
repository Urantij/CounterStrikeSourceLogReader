namespace CounterStrikeSourceLogReader.Changes;

public class DefaultChangeTracker
{
    protected readonly FileSystemWatcher FileSystemWatcher;

    // )))
    private readonly Lock _changeLock = new();

    public event Action? Changed;
    public event Action? Created;
    public event Action? Deleted;

    public DefaultChangeTracker(string path)
    {
        FileSystemWatcher = new FileSystemWatcher(
            Path.GetDirectoryName(path) ?? throw new Exception("Путь какой то дебильный."),
            Path.GetFileName(path));
        // без файл нейма не видит что файл +тут или -тут
        FileSystemWatcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.FileName;
    }

    public virtual void Start()
    {
        // ну да второй старт сделает дуплики ивентов. ы.
        FileSystemWatcher.Changed += (_, e) =>
        {
            if (e.ChangeType != WatcherChangeTypes.Changed) return;

            Console.WriteLine("Реально файл поменялся, штоо");
            OnChanged();
        };
        FileSystemWatcher.Created += (_, _) => Created?.Invoke();
        FileSystemWatcher.Deleted += (_, _) => Deleted?.Invoke();
        FileSystemWatcher.EnableRaisingEvents = true;
    }

    public virtual void Stop()
    {
        FileSystemWatcher.EnableRaisingEvents = false;
        FileSystemWatcher.Dispose();
    }

    protected void OnChanged()
    {
        lock (_changeLock)
        {
            Changed?.Invoke();
        }
    }
}