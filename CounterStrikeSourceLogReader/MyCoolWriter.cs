namespace CounterStrikeSourceLogReader;

public class MyCoolWriter
{
    private readonly string _path;

    // аааа на винде он почему то иногда не пишет) он не чистил.
    // возможно он не пишет пустую строку и не устанавливает размер 0 на винде
    // private FileStream? _fs;

    public MyCoolWriter(string path)
    {
        _path = path;
    }

    public void Start()
    {
        // _fs = new FileStream(_path, FileMode.Create, FileAccess.Write);
        if (!File.Exists(_path))
        {
            File.Create(_path);
        }
    }

    public void Stop()
    {
        // _fs?.Dispose();
    }

    public void Write(string line)
    {
        // if (_fs == null)
        //     return;

        // var content = Encoding.UTF8.GetBytes(line);
        
        File.WriteAllText(_path, line);

        // _fs.SetLength(0);
        // _fs.Write(content);
        // _fs.Flush();
    }
}