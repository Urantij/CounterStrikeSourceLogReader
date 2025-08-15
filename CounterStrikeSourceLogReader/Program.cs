namespace CounterStrikeSourceLogReader;

internal class Program
{
    // Я надеюсь, ивенты в вочере синхронные, и мне не нужно ниче локать и думать

    private static TimeSpan AfkTimeout { get; } = TimeSpan.FromHours(2);

    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        if (args.Length != 4)
        {
            Console.WriteLine(
                "нужны четыре аргумента: путь до логов; путь куда какать; регекс как парсить строки; аутпут формат строки;");
            Console.WriteLine("прес ентер ту екзит...");
            Console.ReadLine();
            return;
        }

        // "/run/media/punky/Master/Dumbass/FastPunk/Steam/steamapps/common/Counter-Strike Source/cstrike/console.log";
        string path = args[0];
        // "./yes";
        string outPath = args[1];
        string regexFormat = args[2];
        string outFormat = args[3];

        CancellationTokenSource cts = new();

        Temptation temptation = new(regexFormat, outFormat);

        MyCoolWriter writer = new(outPath);
        writer.Start();

        var killerInstinct = KillerInstinct.Create(AfkTimeout, cts);

        TrayBaker.Create(cts);

        FileSystemWatcher watcher = new(Path.GetDirectoryName(path) ?? throw new Exception("Путь какой то дебильный."),
            Path.GetFileName(path));
        // без файл нейма не видит что файл +тут или -тут
        watcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.FileName;

        MyCoolReader coolReader = new(path, watcher, killerInstinct);
        coolReader.GotLine += line => CoolReaderOnGotLine(line, temptation, writer);
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

    private static void CoolReaderOnGotLine(string line, Temptation temptation, MyCoolWriter writer)
    {
        string? result = temptation.Process(line);

        if (result == null)
            return;

        writer.Write(result);
    }
}