using System.Text.RegularExpressions;

namespace CounterStrikeSourceLogReader;

/// <summary>
/// Следит за темплейтом и позволяет сравнивать получаемую строку на необходимость написать её.
/// </summary>
public class Temptation
{
    private readonly string _template;
    private readonly Regex _regex;

    public Temptation(string template)
    {
        _template = template;
        _regex = new Regex(template, RegexOptions.Compiled);
    }

    /// <summary>
    /// Сравнивает строку с темплейтом. Возвращает нулл, если писать не нужно.
    /// </summary>
    /// <returns></returns>
    public string? Process(string line)
    {
        var match = _regex.Match(line);
        if (!match.Success)
            return null;

        string result = _template;

        foreach (Group matchGroup in match.Groups)
        {
            // не знаю, есть ли тут нонейм группы, проверять впадлу
            if (string.IsNullOrEmpty(matchGroup.Name))
                continue;

            result = result.Replace($"[{matchGroup.Name}]", matchGroup.Value);
        }

        return result;
    }
}