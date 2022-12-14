using System.Text.RegularExpressions;

namespace Day14;

public class Program
{
    public static void TestLerp()
    {
        var i = 0;
        foreach (var point in InterpolatePoints(new Point(493, 125), new Point(493, 119)))
        {
            Console.WriteLine(point);
            if (i++ > 20) break;
        }

        Console.WriteLine("=====================");

        i = 0;
        foreach (var point in InterpolatePoints(new Point(504, 28), new Point(509, 28)))
        {
            Console.WriteLine(point);
            if (i++ > 20) break;
        }
    }

    public static void Main()
    {
        Part1();
        Part2();
    }

    public static void Part1()
    {
        var layout = new HashSet<Point>(ReadLayout());
        var maxY = layout.Max(p => p.y);
        int count = 0;
        while (true)
        {
            var sand = new Point(500, 0);
            var current = MoveSand(layout, sand);
            while (true)
            {
                var next = MoveSand(layout, current);
                if (next == current) break;
                current = next;
                if (current.y > maxY) break;
            }
            if (current.y > maxY) break;
            layout.Add(current);
            count++;
        }
        Console.WriteLine(count);
    }

    public static void Part2()
    {
        var layout = new HashSet<Point>(ReadLayout());
        var maxY = layout.Max(p => p.y) + 2;
        int count = 0;
        var origin = new Point(500, 0);
        while (true)
        {
            var current = origin;
            while (true)
            {
                var next = MoveSand(layout, current, maxY);
                if (next == current) break;
                current = next;
                if (current.y == maxY) break;
                if (current == origin) break;
            }
            if (current == origin) break;
            layout.Add(current);
            count++;
        }
        Console.WriteLine(count + 1);
    }

    public static Point MoveSand(HashSet<Point> layout, Point sand, int maxY = int.MaxValue)
    {
        var candidates = new[] {
            new Point(sand.x, sand.y + 1),
            new Point(sand.x - 1, sand.y + 1),
            new Point(sand.x + 1, sand.y + 1),
        };
        foreach (var candidate in candidates)
        {
            if (!layout.Contains(candidate) && candidate.y != maxY) return candidate;
        }

        return sand;
    }

    public static IEnumerable<Point> ReadLayout()
    {
        foreach (var line in File.ReadLines("input.txt"))
        {
            var matches = Regex.Matches(line, @"(\d+)\,(\d+)").ToArray();
            for (int i = 1; i < matches.Length; i++)
            {
                var points = matches[(i-1)..(i+1)]
                    .Select(m => new Point(
                        x: int.Parse(m.Groups[1].Value),
                        y: int.Parse(m.Groups[2].Value)
                    ))
                    .ToArray();
                foreach (var point in InterpolatePoints(points[0], points[1]))
                {
                    yield return point;
                }
            }
        }
    }


    public static IEnumerable<Point> InterpolatePoints(Point source, Point dest)
    {
        yield return source;
        var current = source;
        while (current != dest)
        {
            current = new Point(
                x: current.x - Math.Clamp(current.x.CompareTo(dest.x), -1, 1),
                y: current.y - Math.Clamp(current.y.CompareTo(dest.y), -1, 1)
            );
            yield return current;
        }
    }
}

public record struct Point(int x, int y);