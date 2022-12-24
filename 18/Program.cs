namespace Day18;

public class Program
{
    public static void Main()
    {
        var points = ReadPoints();
        var part1 = points.Sum(p => ExposedFaces(points, p));
        Console.WriteLine(part1);

        var worter = GetWorter(points);
        var part2 = points.Sum(p => ExposedFaces2(points, worter, p));
        Console.WriteLine(part2);
    }

    public static int ExposedFaces(HashSet<Point> points, Point point)
    {
        return new[] { new Point(1, 0, 0), new Point(-1, 0, 0), new Point(0, 1, 0), new Point(0, -1, 0), new Point(0, 0, 1), new Point(0, 0, -1) }
            .Select(p => new Point(p.X + point.X, p.Y + point.Y, p.Z + point.Z))
            .Where(p => !points.Contains(p))
            .Count();
    }

    public static int ExposedFaces2(HashSet<Point> points, HashSet<Point> worter, Point point)
    {
        return new[] { new Point(1, 0, 0), new Point(-1, 0, 0), new Point(0, 1, 0), new Point(0, -1, 0), new Point(0, 0, 1), new Point(0, 0, -1) }
            .Select(p => new Point(p.X + point.X, p.Y + point.Y, p.Z + point.Z))
            .Where(p => worter.Contains(p))
            .Count();
    }

    public static HashSet<Point> ReadPoints()
    {
        return File.ReadLines("input.txt")
            .Select(line => line.Split(",").Select(int.Parse).ToArray())
            .Select(parts => new Point(parts[0], parts[1], parts[2]))
            .ToHashSet();
    }

    public static HashSet<Point> GetWorter(HashSet<Point> points)
    {
        var minX = points.Min(p => p.X) - 1;
        var maxX = points.Max(p => p.X) + 1;
        var minY = points.Min(p => p.Y) - 1;
        var maxY = points.Max(p => p.Y) + 1;
        var minZ = points.Min(p => p.Z) - 1;
        var maxZ = points.Max(p => p.Z) + 1;

        var start = new Point(minX, minY, minZ);
        var open = new HashSet<Point>();
        var closed = new HashSet<Point>();
        open.Add(start);

        while (open.Any())
        {
            var current = open.First();
            open.Remove(current);
            closed.Add(current);
            var neighbours = new (int x, int y, int z)[] { (1, 0, 0), (-1, 0, 0), (0, 1, 0), (0, -1, 0), (0, 0, 1), (0, 0, -1) }
                .Select(t => new Point(current.X + t.x, current.Y + t.y, current.Z + t.z))
                .Where(p => p.X >= minX && p.X <= maxX && p.Y >= minY && p.Y <= maxY && p.Z >= minZ && p.Z <= maxZ)
                .Where(p => !closed.Contains(p) && !points.Contains(p))
                .ToHashSet();

            foreach (var neighbour in neighbours)
            {
                open.Add(neighbour);
            }
        }

        Console.WriteLine($"found {closed.Count()} worter");

        return closed;
    }
}

public record struct Point(int X, int Y, int Z);