using CounterStrikeSourceLogReader.Changes;

namespace CounterStrikeSourceLogReader;

internal class Program
{
    // Я надеюсь, ивенты в вочере синхронные, и мне не нужно ниче локать и думать

    private static TimeSpan AfkTimeout { get; } = TimeSpan.FromHours(2);

    private static TimeSpan PokeDelay { get; } = TimeSpan.FromSeconds(1);

    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        if (args.Length < 4)
        {
            Console.WriteLine(
                "нужны четыре аргумента: путь до логов; путь куда какать; регекс как парсить строки; аутпут формат строки;");
            Console.WriteLine("прес ентер ту екзит...");
            Console.ReadLine();
            return;
        }

        bool poking = args.Contains("--poke", StringComparer.OrdinalIgnoreCase);

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

        DefaultChangeTracker changeTracker = new(path);

        FilePoker poker = new(path, PokeDelay);

        MyCoolReader coolReader = new(path, changeTracker, killerInstinct);
        coolReader.GotLine += line => CoolReaderOnGotLine(line, temptation, writer);
        coolReader.Start();

        cts.Token.Register(() =>
        {
            poker.Stop();
            changeTracker.Stop();
            coolReader.Stop();
            writer.Stop();
        });

        changeTracker.Start();
        if (poking)
            poker.Start();

        Stealth.Hide();

        Console.WriteLine("прес ентер ту екзит...");
        Console.ReadLine();

        cts.Cancel();
    }

    private static void CoolReaderOnGotLine(string line, Temptation temptation, MyCoolWriter writer)
    {
        try
        {
            Console.WriteLine($"Проверяем строку \"{line}\"");

            string? result = temptation.Process(line);

            if (result == null)
                return;

            Console.WriteLine("Пишем строку...");

            writer.Write(result);
        }
        catch (Exception e)
        {
            Console.WriteLine("При попытке обработать строку из файла1 произошла ошибка");
            Console.WriteLine(e);
        }
    }
}