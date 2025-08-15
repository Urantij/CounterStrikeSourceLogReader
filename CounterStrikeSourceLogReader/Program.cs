using System.Text;
using System.Text.RegularExpressions;

namespace CounterStrikeSourceLogReader;

internal class Program
{
    private static string _path = "./log";
    private static string _outPath = "./file";

    private static Regex _regex = new("(?<target>.+)");

    private static string _outFormat = "[target]";
    // Я надеюсь, ивенты в вочере синхронные, и мне не нужно ниче локать и думать

    private static TimeSpan AfkTimeout { get; } = TimeSpan.FromHours(2);

    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        if (args.Length != 4)
        {
            Console.WriteLine(
                "нужны три аргумента: путь до логов; путь куда какать; регекс как парсить строки; аутпут формат строки;");
            Console.WriteLine("прес ентер ту екзит...");
            Console.ReadLine();
            return;
        }

        _path = args[0]; // "/run/media/punky/Master/Dumbass/FastPunk/Steam/steamapps/common/Counter-Strike Source/cstrike/console.log";
        _outPath = args[1]; // "./yes";
        _regex = new Regex(args[2], RegexOptions.Compiled);
        _outFormat = args[3];

        CancellationTokenSource cts = new();

        MyCoolWriter writer = new(_outPath);
        writer.Start();

        var killerInstinct = KillerInstinct.Create(AfkTimeout, cts);

        TrayBaker.Create(cts);

        FileSystemWatcher watcher = new(Path.GetDirectoryName(_path) ?? throw new Exception("Путь какой то дебильный."),
            Path.GetFileName(_path));
        // без файл нейма не видит что файл +тут или -тут
        watcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.FileName;

        MyCoolReader coolReader = new(_path, watcher, killerInstinct);
        coolReader.GotLine += line => CoolReaderOnGotLine(line, writer);
        coolReader.Start();

        cts.Token.Register(() =>
        {
            watcher.EnableRaisingEvents = false;

            coolReader.Stop();
            writer.Stop();
            watcher.Dispose();
        });

        watcher.EnableRaisingEvents = true;

        Stealth.Hide();

        Console.WriteLine("прес ентер ту екзит...");
        Console.ReadLine();

        cts.Cancel();
    }

    private static void CoolReaderOnGotLine(string line, MyCoolWriter writer)
    {
        var match = _regex.Match(line);
        if (!match.Success)
            return;

        string result = _outFormat;

        foreach (Group matchGroup in match.Groups)
        {
            // не знаю, есть ли тут нонейм группы, проверять впадлу
            if (string.IsNullOrEmpty(matchGroup.Name))
                continue;

            result = result.Replace($"[{matchGroup.Name}]", matchGroup.Value);
        }

        writer.Write(result);
    }
}