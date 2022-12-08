namespace Day8;

public class Program
{
    public static void Main()
    {
        var grid = ReadGrid();
        var part1 = grid.Values.Keys.Count(loc => grid.IsCellVisible(loc.x, loc.y));
        Console.WriteLine(part1);
        var part2 = grid.Values.Keys.Select(k => grid.ScenicScore(k.x, k.y)).Max();
        Console.WriteLine(part2);
    }

    public static Grid ReadGrid()
    {
        var y = 0;
        var ret = new Grid();
        foreach (var line in File.ReadLines("input.txt"))
        {
            ret.Width = line.Length;
            for (int x = 0; x < line.Length; x++)
            {
                ret.Values[(x, y)] = int.Parse(line[x].ToString());
            }
            y++;
        }
        ret.Height = y;
        return ret;
    }

}

public class Grid
{
    public Dictionary<(int x, int y), int> Values { get; } = new();
    public int Width { get; set; }
    public int Height { get; set; }

    public bool IsCellVisible(int x, int y)
    {
        return !GetNorthValues(x, y).Any(v => v >= Values[(x, y)]) ||
            !GetSouthValues(x, y).Any(v => v >= Values[(x, y)]) ||
            !GetEastValues(x, y).Any(v => v >= Values[(x, y)]) ||
            !GetWestValues(x, y).Any(v => v >= Values[(x, y)]);
    }

    public int ScenicScore(int x, int y)
    {
        return GetNorthValues(x, y).TakeWhileInclusive(v => v < Values[(x, y)]).Count() *
            GetSouthValues(x, y).TakeWhileInclusive(v => v < Values[(x, y)]).Count() *
            GetWestValues(x, y).TakeWhileInclusive(v => v < Values[(x, y)]).Count() *
            GetEastValues(x, y).TakeWhileInclusive(v => v < Values[(x, y)]).Count();
    }

    private IEnumerable<int> GetNorthValues(int X, int Y)
    {
        for (int y = Y - 1; y >= 0; y--)
        {
            yield return Values[(X, y)];
        }
    }

    private IEnumerable<int> GetSouthValues(int X, int Y)
    {
        for (int y = Y + 1; y < Height; y++)
        {
            yield return Values[(X, y)];
        }
    }

    private IEnumerable<int> GetWestValues(int X, int Y)
    {
        for (int x = X - 1; x >= 0; x--)
        {
            yield return Values[(x, Y)];
        }
    }

    private IEnumerable<int> GetEastValues(int X, int Y)
    {
        for (int x = X + 1; x < Width; x++)
        {
            yield return Values[(x, Y)];
        }
    }
}

public static class EnumerableExtensions
{
    public static IEnumerable<T> TakeWhileInclusive<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        foreach (var item in source)
        {
            yield return item;
            if (!predicate(item))
            {
                yield break;
            }
        }
    }
}