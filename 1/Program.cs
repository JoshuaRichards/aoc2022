namespace Day1;

public class Program
{
    public static void Main()
    {
        var part1 = TotalCalorieCounts().Max();
        Console.WriteLine(part1);
        var part2 = TotalCalorieCounts().OrderByDescending(x => x).Take(3).Sum();
        Console.WriteLine(part2);
    }

    private static IEnumerable<int> TotalCalorieCounts()
    {
        int currentCount = 0;
        foreach (var line in File.ReadLines("input.txt"))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                yield return currentCount;
                currentCount = 0;
                continue;
            }

            currentCount += int.Parse(line);
        }

        if (currentCount > 0)
        {
            yield return currentCount;
        }
    }
}