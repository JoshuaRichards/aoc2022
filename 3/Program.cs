namespace Day3;

public class Program
{
    public static void Main()
    {
        var part1 = ReadRucksacks()
            .Select(pair => pair.Item1.Intersect(pair.Item2))
            .Select(intersection => GetPriority(intersection.First()))
            .Sum();
        Console.WriteLine(part1);

        var part2 = ReadRucksacks2()
            .Select(group => group[0].Intersect(group[1]).Intersect(group[2]))
            .Select(intersection => GetPriority(intersection.First()))
            .Sum();
        Console.WriteLine(part2);
    }

    private static IEnumerable<(HashSet<char>, HashSet<char>)> ReadRucksacks()
    {
        return File.ReadLines("input.txt")
            .Select(line => (line.Substring(0, line.Length / 2), line.Substring(line.Length / 2)))
            .Select(pair => (new HashSet<char>(pair.Item1), new HashSet<char>(pair.Item2)));
    }

    private static IEnumerable<List<HashSet<char>>> ReadRucksacks2()
    {
        var group = new List<HashSet<char>>();
        foreach (var line in File.ReadLines("input.txt"))
        {
            group.Add(new HashSet<char>(line));
            if (group.Count() == 3)
            {
                yield return group;
                group = new List<HashSet<char>>();
            }
        }
        if (group.Any()) yield return group;
    }

    private static int GetPriority(char c)
    {
        if (Char.IsLower(c))
        {
            return c - 'a' + 1;
        }
        else
        {
            return c - 'A' + 1 + 26;
        }
    }
}