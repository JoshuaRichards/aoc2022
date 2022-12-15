using System.Text.RegularExpressions;

namespace Day15;

public static class Program
{
    public static void Main()
    {
        // Part1();
        var part2 = Part2();
        Console.WriteLine(part2);
    }
    public static void Part1()
    {
        var entries = ReadEntries().ToArray();

        var minSensorX = entries.Min(e => e.sensor.x);
        var maxSensorX = entries.Max(e => e.sensor.x);
        var maxManhattan = entries.Max(e => GetManhattanDistance(e.sensor, e.closestBeacon));

        var minX = minSensorX - maxManhattan;
        var maxX = maxSensorX + maxManhattan;
        var y = 2000000;
        // var y = 10;

        var forbotten = new List<Point>();
        var part1 = 0;
        for (var x = minX; x <= maxX; x++)
        {
            var point = new Point(x, y);
            // Console.WriteLine(point);
            var isForbotten = entries.Any(e => GetManhattanDistance(point, e.sensor) <= e.GetManhattanDistance());
            if (isForbotten)
            {
                part1++;
                forbotten.Add(point);
            }
            // if ((x - minX) > 100000) break;
        }

        part1 -= entries.Where(e => e.closestBeacon.y == y).Select(e => e.closestBeacon).Distinct().Count();

        Console.WriteLine(part1);
    }

    public static long Part2()
    {
        var entries = ReadEntries().ToArray();
        var magicNumber = 4_000_000;
        
        var candidates = new HashSet<Point>(
            entries.SelectMany(e => GetCandidates(e.sensor, e.GetManhattanDistance() + 1))
        );
        foreach (var entry in entries) candidates.Remove(entry.closestBeacon);

        var answer = candidates
            .Where(p => p.x >= 0 && p.x <= magicNumber && p.y >= 0 && p.y <= magicNumber)
            .Where(p => !entries.Any(e => GetManhattanDistance(p, e.sensor) <= e.GetManhattanDistance()))
            .ToArray();

        return (answer[0].x * magicNumber) + answer[0].y;
    }

    public static long GetManhattanDistance(Point source, Point dest)
    {
        var ret = Math.Abs(source.x - dest.x) + Math.Abs(source.y - dest.y);
        return ret;
    }

    public static long GetManhattanDistance(this Entry entry)
    {
        return GetManhattanDistance(entry.sensor, entry.closestBeacon);
    }

    public static IEnumerable<Point> GetCandidates(Point source, long distance)
    {
        var xDist = 0;
        var yDist = distance;

        while (xDist <= distance)
        {
            yield return new Point(source.x + xDist, source.y + yDist);
            yield return new Point(source.x + xDist, source.y - yDist);
            yield return new Point(source.x - xDist, source.y + yDist);
            yield return new Point(source.x - xDist, source.y - yDist);
            xDist++;
            yDist--;
        }
    }

    public static IEnumerable<Point> GetForbottens(Point source, int radius)
    {
        Console.WriteLine($"GetForbottens({source}, {radius})");
        radius = Math.Abs(radius);
        var xDist = 0;
        var yDist = radius;

        while (xDist <= radius)
        {
            Console.WriteLine($"xDist: {xDist}, yDist: {yDist}");
            var destinations = new[] {
                new Point(source.x + xDist, source.y + yDist),
                new Point(source.x + xDist, source.y - yDist),
                new Point(source.x - xDist, source.y + yDist),
                new Point(source.x - xDist, source.y - yDist),
            };

            var points = destinations.SelectMany(d => InterpolatePointsButManhattan(source, d));
            foreach (var point in points)
            {
                yield return point;
            }

            xDist++;
            yDist--;
        }
    }

    public static IEnumerable<Point> InterpolatePointsButManhattan(Point source, Point dest)
    {
        var points = InterpolatePoints(source, new Point(source.x, dest.y))
            .Concat(InterpolatePoints(new Point(source.x, dest.y), dest))
            .Concat(InterpolatePoints(source, new Point(dest.y, source.x)))
            .Concat(InterpolatePoints(new Point(dest.y, source.x), dest));

        return points;
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


    public static IEnumerable<Entry> ReadEntries()
    {
        foreach (var line in File.ReadLines("input.txt"))
        {
            var match = Regex.Match(line, @"x\=(-?\d+).*y\=(-?\d+).*x\=(-?\d+).*y\=(-?\d+)");
            var ints = match.Groups.Values.Skip(1).Select(g => g.Value).Select(int.Parse).ToArray();
            yield return new Entry(
                sensor: new Point(ints[0], ints[1]),
                closestBeacon: new Point(ints[2], ints[3])
            );
        }
    }
}

public record struct Point(long x, long y);
public record struct Entry(Point sensor, Point closestBeacon);