using System.Globalization;
using System.Text.RegularExpressions;

namespace CounterStrikeSourceLogReader;

enum TargetType
{
    Timespan
}

enum CompareType
{
    Less
}

class CompareInfo(TargetType targetType, CompareType compareType, string? format)
{
    public TargetType TargetType { get; } = targetType;
    public CompareType CompareType { get; } = compareType;
    public string? Format { get; } = format;

    public IComparable? CurrentValue { get; set; }
}

class GroupInfo(string name, CompareInfo? compare)
{
    public string Name { get; } = name;
    public CompareInfo? Compare { get; } = compare;

    public string? CurrentValue { get; set; }
}

/// <summary>
/// Следит за темплейтом и позволяет сравнивать получаемую строку на необходимость написать её.
/// Необходимость написать наступает, если строка совпадает с темплейтом, и хотя бы одна из её групп изменилась.
/// Если в группе есть тип и сравнение, группа считается изменённой, если условие сравнения будет выполнено.
/// </summary>
public class Temptation
{
    private readonly string _outputTemplate;
    private readonly Regex _regex;

    private readonly GroupInfo[] _groups;

    public Temptation(string regexString, string outputTemplate)
    {
        _outputTemplate = outputTemplate;

        // TODO можно было бы проверить, что все группы в темплейте присутствуют в регексе

        MatchCollection matches = Regex.Matches(outputTemplate,
            @"\[(?<name>.+?)(\:(?<type>.+?)\:(?<compare>.+?)(\:(?<format>.+?)){0,1}){0,1}\]");

        _groups = matches.Select(match =>
        {
            CompareInfo? compare = null;

            if (match.Groups["type"].Success)
            {
                TargetType targetType = Enum.Parse<TargetType>(match.Groups["type"].Value, true);
                CompareType compareType = Enum.Parse<CompareType>(match.Groups["compare"].Value, true);
                string? format = match.Groups["format"].Success ? match.Groups["format"].Value : null;

                compare = new CompareInfo(targetType, compareType, format);
            }

            return new GroupInfo(match.Groups["name"].Value, compare);
        }).ToArray();

        // нужно сделать "чистый" темплейт без доп инфы
        foreach (Match match in matches.OrderByDescending(m => m.Index))
        {
            if (!match.Groups["type"].Success)
                continue;

            _outputTemplate = _outputTemplate.Remove(match.Index, match.Length);
            _outputTemplate = _outputTemplate.Insert(match.Index, $"[{match.Groups["name"]}]");
        }

        _regex = new Regex(regexString, RegexOptions.Compiled);
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

        string result = _outputTemplate;

        bool changed = false;

        foreach (Group matchGroup in match.Groups)
        {
            // не знаю, есть ли тут нонейм группы, проверять впадлу
            if (string.IsNullOrEmpty(matchGroup.Name))
                continue;

            GroupInfo? groupInfo =
                _groups.FirstOrDefault(g => g.Name.Equals(matchGroup.Name, StringComparison.OrdinalIgnoreCase));

            // нонейм группы имеют нейм номерной.
            if (groupInfo == null)
                continue;

            result = result.Replace($"[{matchGroup.Name}]", matchGroup.Value);

            if (groupInfo.CurrentValue != null)
            {
                if (groupInfo.CurrentValue == matchGroup.Value)
                    continue;
            }

            groupInfo.CurrentValue = matchGroup.Value;

            if (groupInfo.Compare != null)
            {
                IComparable newValue = MakeComparableObject(groupInfo.CurrentValue, groupInfo.Compare.TargetType,
                    groupInfo.Compare.Format);
                IComparable? oldValue = groupInfo.Compare.CurrentValue;

                groupInfo.Compare.CurrentValue = newValue;

                if (oldValue != null)
                {
                    changed = Compare(oldValue, newValue, groupInfo.Compare.CompareType);
                }
                else
                {
                    changed = true;
                }
            }
            else
            {
                changed = true;
            }
        }

        if (changed)
            return result;

        return null;
    }

    private static bool Compare(IComparable oldValue, IComparable newValue, CompareType compareType)
    {
        int sort = newValue.CompareTo(oldValue);

        return compareType switch
        {
            CompareType.Less => sort < 0,
            _ => throw new Exception($"Неизвестное сравнение {compareType}")
        };
    }

    private static IComparable MakeComparableObject(string value, TargetType targetType, string? format)
    {
        return targetType switch
        {
            TargetType.Timespan => format != null
                ? TimeSpan.ParseExact(value, format, CultureInfo.InvariantCulture)
                : TimeSpan.Parse(value),
            _ => throw new Exception($"Неизвестный тип типочек {targetType}")
        };
    }
}