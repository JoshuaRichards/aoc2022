using System.Text.RegularExpressions;

namespace Day4;

public class Program
{
    public static void Main()
    {
        var part1 = ReadInput()
            .Where(pair => pair.left.IsSupersetOf(pair.right) || pair.left.IsSubsetOf(pair.right))
            .Count();
        Console.WriteLine(part1);
        var part2 = ReadInput()
            .Where(pair => pair.left.Intersect(pair.right).Any())
            .Count();
        Console.WriteLine(part2);
    }

    private static IEnumerable<(HashSet<int> left, HashSet<int> right)> ReadInput()
    {
        foreach (var line in File.ReadLines("input.txt"))
        {
            var match = Regex.Match(line, @"(\d+)\-(\d+)\,(\d+)\-(\d+)");
            var ids = Enumerable.Range(1, 4)
                .Select(i => match.Groups[i].Value)
                .Select(int.Parse)
                .ToArray();
            var left = RangeToSet(ids[0], ids[1]);
            var right = RangeToSet(ids[2], ids[3]);
            yield return (left, right);
        }
    }

    private static HashSet<int> RangeToSet(int start, int end)
    {
        return new HashSet<int>(Enumerable.Range(start, end - start + 1));
    }
}