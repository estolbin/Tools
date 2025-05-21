using System.Globalization;
using CSharpFunctionalExtensions;

var result = ParseArgs(args)
    .TapError(error => Console.WriteLine(error))
    .Finally(result => 
    {
        if (result.IsFailure) Environment.Exit(1);
        return result.Value;
    });

(int year, int? month, int before, int after) = result;

//var currentDate = DateTime.Now;
var currentDate = month.HasValue
    ? new DateTime(year, month.Value, 1)
    : new DateTime(year, DateTime.Now.Month, 1);

var months = GetMonthsRange(currentDate, before, after).ToList();

var monthGroups = months
    .Select((m, index) => new { m, index })
    .GroupBy(x => x.index / 3)
    .Select(g => g.Select(x => x.m).ToList());

foreach (var group in monthGroups)
{
    var calendars = group.Select(m => GenerateCalendarLines(m.Year, m.Month)).ToList();
    int maxLines = calendars.Max(c => c.Count);
    var paddedCalendars = calendars.Select(c => PadCalendar(c, maxLines)).ToList();

    for (int i = 0; i < maxLines; i++)
    {
        var line = string.Join("  ", paddedCalendars.Select(c => c[i]));
        Console.WriteLine(line);
    }
    Console.WriteLine();
    
}
return;

// Основные функции
static Result<(int year, int? month, int before, int after)> ParseArgs(string[] args)
{
    int year = DateTime.Now.Year;
    int? month = null;
    int before = 0, after = 0;

    return Result.Success()
        .Tap(() => ValidateArgs(args))
        .Bind(() => ParseOptions(args, ref year, ref month, ref before, ref after))
        .Map(() => (year, month, before, after));
}

static void ValidateArgs(string[] args)
{

    for (int i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "-y":
            case "-m":
            case "-A":
            case "-B":
                {
                    if (i + 1 >= args.Length || !int.TryParse(args[i + 1], out _))
                        throw new ArgumentException($"Invalid value for '{args[i]}'.");

                    i++;
                    break;
                }
            default:
                throw new ArgumentException($"Unknown option '{args[i]}'.");
        }
    }
}

static Result ParseOptions(string[] args, ref int year, ref int? month, ref int before, ref int after)
{
    for (int i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "-y" when int.TryParse(args[++i], out int y):
                year = y;
                if (year < 1) return Result.Failure("Год должен быть больше нуля");
                break;
                
            case "-m" when int.TryParse(args[++i], out int m):
                if (m is < 1 or > 12) 
                    return Result.Failure("Месяц должен быть в пределах 1-12");
                month = m;
                break;
                
            case "-B" when int.TryParse(args[++i], out int b):
                before = b;
                break;
                
            case "-A" when int.TryParse(args[++i], out int a):
                after = a;
                break;
                
            default:
                return Result.Failure($"Invalid option: {args[i]}");
        }
    }
    
    if (!month.HasValue && after == 0 && before == 0 && args.Length > 0)
    {
        after = 11; // Полный год
        before = 0;
    }
    
    return Result.Success();
}

static List<string> GenerateCalendarLines(int year, int month)
{
    const int totalWidth = 20;
    var culture = CultureInfo.GetCultureInfo("ru-RU");

    var firstDay = new DateTime(year, month, 1);
    int firstDayOfWeekNum = FixSunday(firstDay.DayOfWeek);
    int leadingBlanks = firstDayOfWeekNum - 1;

    var daysNumbers = Enumerable.Range(1, DateTime.DaysInMonth(year, month));
    var allDays = Enumerable.Repeat(0, leadingBlanks).Concat(daysNumbers).ToList();

    var weeks = allDays
        .Select((num, index) => new { num, index })
        .GroupBy(x => x.index / 7, x => x.num)
        .Select(g => g.ToList())
        .ToList();

    // Формируем заголовок
    var header = firstDay.ToString("MMMM yyyy", culture);

    int totalPadding = totalWidth - header.Length;
    int leftPadding = totalPadding / 2 + header.Length;
    //int padding = (headerWidth - header.Length) / 2;
    var headerLine = header.PadLeft(leftPadding).PadRight(totalWidth);

    // Формируем дни недели
    var daysHeader = string.Join(" ", new[] { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" });

    // Собираем все строки
    var lines = new List<string> { headerLine, daysHeader };
    
    foreach (var week in weeks)
    {
        var weekLine = string.Join(" ", 
            week.Select(num => num == 0 ? "  " : num.ToString().PadLeft(2)));
        lines.Add(weekLine.PadRight(totalWidth));
    }

    return lines;
}

static List<string> PadCalendar(List<string> calendar, int targetHeight)
{
    var result = new List<string>(calendar);
    while (result.Count < targetHeight) result.Add(new string(' ', 20));
    return result;
}






static IEnumerable<DateTime> GetMonthsRange(DateTime baseDate, int before, int after)
{
    var start = baseDate.AddMonths(-before);
    var end = baseDate.AddMonths(after);

    return Enumerable.Range(0, (end.Year - start.Year) * 12 + end.Month - start.Month + 1)
        .Select(offset => start.AddMonths(offset))
        .Select(d => new DateTime(d.Year, d.Month, 1));
}

static int FixSunday(DayOfWeek d) => d == DayOfWeek.Sunday ? 7 : (int)d;
