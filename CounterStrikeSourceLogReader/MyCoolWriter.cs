using System.Text;

namespace CounterStrikeSourceLogReader;

public class MyCoolWriter
{
    private readonly string _path;

    private FileStream? _fs;

    public MyCoolWriter(string path)
    {
        _path = path;
    }

    public void Start()
    {
        _fs = new FileStream(_path, FileMode.Create, FileAccess.Write);
    }

    public void Stop()
    {
        _fs?.Dispose();
    }

    public void Write(string line)
    {
        if (_fs == null)
            return;

        var content = Encoding.UTF8.GetBytes(line);

        _fs.SetLength(0);
        _fs.Write(content);
        _fs.Flush();
    }
}