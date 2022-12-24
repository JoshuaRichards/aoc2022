using System.Security.Cryptography;

namespace Day17;


public class Program
{
    private const int maxX = 6;
    private static readonly Point[][] Tetrominos = new[]
    {
        new [] { new Point(0, 0), new Point(1, 0), new Point(2, 0), new Point(3, 0)},
        new [] { new Point(1, 0), new Point(0, 1), new Point(1, 1), new Point(2, 1), new Point(1, 2) },
        new [] { new Point(0, 0), new Point(1, 0), new Point(2, 0), new Point(2, 1), new Point(2, 2) },
        new [] { new Point(0, 0), new Point(0, 1), new Point(0, 2), new Point(0, 3) },
        new [] { new Point(0, 0), new Point(1, 0), new Point(0, 1), new Point(1, 1) },
    };

    public static void Main()
    {
        Part1();
        Part2();
    }

    public static void Part1()
    {
        var rockyBois = new HashSet<Point>();
        var directions = ReadDirections().GetEnumerator();
        var tetrominos = GetTetrominos().GetEnumerator();

        var maxY = 0L;
        for (int i = 0; i < 2022; i++)
        {
            tetrominos.MoveNext();
            var tetromino = tetrominos.Current;
            var spawnY = maxY + 4;
            var spawnX = 2;

            tetromino = tetromino.Select(p => new Point(p.X + spawnX, p.Y + spawnY)).ToArray();

            // PrintTheThing(rockyBois, tetromino, maxY + 6);

            while (true)
            {
                directions.MoveNext();
                var direction = directions.Current;
                var (next, done) = Step(tetromino, direction, rockyBois);

                // PrintTheThing(rockyBois, next, maxY + 6);

                tetromino = next;
                if (done) break;
            }

            maxY = Math.Max(maxY, tetromino.Max(p => p.Y));
            foreach (var point in tetromino) rockyBois.Add(point);
        }

        Console.WriteLine(maxY);
    }

    public static void Part2()
    {
        var rockyBois = new HashSet<Point>();
        var directions = ReadDirections().GetEnumerator();
        var tetrominos = GetTetrominos().GetEnumerator();

        var hashedBois = new Dictionary<string, (long, long, Point[], long?)>();

        var maxY = 0L;
        var target = 1_000_000_000_000;
        var shouldHash = true;
        for (long iteration = 0L; iteration < target; iteration++)
        {
            tetrominos.MoveNext();
            var tetromino = tetrominos.Current;
            var spawnY = maxY + 4;
            var spawnX = 2;

            if (shouldHash)
            {
                var bois = GetReachableBois(rockyBois, new Point(spawnX, spawnY)).ToArray();
                var hash = HashTheBois(bois);
                Console.WriteLine($"iteration {iteration} hash {hash}, {bois.Count()} bois");
                if (hashedBois.TryGetValue(hash, out var last))
                {
                    var (lastIteration, lastMinY, lastBois, lastIterationLength) = last;
                    var iterationDiff = (iteration - lastIteration);
                    if (lastIterationLength is null)
                    {
                        Console.WriteLine("found a candidate. adding iteration diff to dict");
                        hashedBois[hash] = (iteration, bois.Min(p => p.Y), bois, iterationDiff);
                        goto doneWithTheHashBs;
                    }
                    else if (lastIterationLength is not null && lastIterationLength != iterationDiff)
                    {
                        Console.WriteLine($"bad iteration diff of {iterationDiff}. last one was {lastIterationLength}. we're not doin it");
                        goto doneWithTheHashBs;
                    }
                    var heightDiff = bois.Min(p => p.Y) - lastMinY;

                    var remainingIterations = target - iteration;
                    var fastForward = (remainingIterations / iterationDiff);
                    Console.WriteLine($"hash {hash} was last found at iteration {lastIteration}. that was {iterationDiff} iterations ago.");
                    Console.WriteLine($"height diff is {heightDiff}");
                    Console.WriteLine($"there are {remainingIterations} iterations left. so we're skipping {fastForward} iterations.");
                    iteration += fastForward * iterationDiff;
                    Console.WriteLine($"we're on {iteration} iteration now.");
                    var newBois = bois.Select(p => new Point(p.X, p.Y + (heightDiff * fastForward)));
                    foreach (var boi in newBois) rockyBois.Add(boi);
                    spawnY += fastForward * heightDiff;
                    maxY += fastForward * heightDiff;
                    Console.WriteLine($"maxY is now {maxY}");
                    shouldHash = false;
                    Console.WriteLine("=====================bois now======================");
                    PrintTheThing(new HashSet<Point>(bois), new Point[0], bois.Max(p => p.Y) + 6);
                    Console.WriteLine("=====================bois then=====================");
                    PrintTheThing(new HashSet<Point>(lastBois), new Point[0], lastBois.Max(p => p.Y) + 6);
                    Console.WriteLine("===================================================");
                    Console.WriteLine($"oh and another thing maxY is {maxY}");
                }
                else
                {
                    if (Enumerable.Range(0, maxX + 1).All(x => bois.Any(p => p.X == x)))
                        hashedBois[hash] = (iteration, bois.Any() ? bois.Min(p => p.Y) : 0, bois, null);

                    rockyBois = new HashSet<Point>(bois);
                }
            }
            doneWithTheHashBs:

            tetromino = tetromino.Select(p => new Point(p.X + spawnX, p.Y + spawnY)).ToArray();

            // PrintTheThing(rockyBois, tetromino, maxY + 6);

            while (true)
            {
                directions.MoveNext();
                var direction = directions.Current;
                var (next, done) = Step(tetromino, direction, rockyBois);

                // PrintTheThing(rockyBois, next, maxY + 6);

                tetromino = next;
                if (done) break;
            }

            maxY = Math.Max(maxY, tetromino.Max(p => p.Y));
            foreach (var point in tetromino) rockyBois.Add(point);
        }

        Console.WriteLine(maxY);
    }

    private static readonly SHA256 hashyBoi = SHA256.Create();
    public static string HashTheBois(Point[] theBois)
    {
        if (!theBois.Any()) return "";
        var minY = theBois.Min(p => p.Y);
        var normalisedBois = theBois.Select(p => new Point(p.X, p.Y - minY)).Select(p => (p.X, p.Y)).OrderBy(p => p);
        var bytes = normalisedBois.SelectMany(p => BitConverter.GetBytes(p.X).Concat(BitConverter.GetBytes(p.Y))).ToArray();

        var hashed = hashyBoi.ComputeHash(bytes);
        return string.Join("", hashed.Select(c => c.ToString("x2")));
    }

    public static IEnumerable<Point> GetReachableBois(HashSet<Point> rockyBois, Point spawnPoint)
    {
        IEnumerable<Point> GetNeighbours(Point point)
        {
            var transforms = new (int x, int y)[] { (1, 0), (0, 1), (-1, 0), (0, -1) };
            return transforms.Select(t => new Point(point.X + t.x, point.Y + t.y))
                .Where(p => p.X >= 0 && p.Y >= 1 && p.X <= maxX && p.Y <= spawnPoint.Y);
        }

        var open = new Queue<Point>();
        var closed = new HashSet<Point>();

        open.Enqueue(spawnPoint);

        while (open.Any())
        {
            var current = open.Dequeue();
            closed.Add(current);
            var neigbours = GetNeighbours(current);
            foreach (var boi in neigbours.Where(p => rockyBois.Contains(p)))
            {
                yield return boi;
            }
            foreach (var point in neigbours.Where(p => !rockyBois.Contains(p) && !closed.Contains(p)))
            {
                open.Enqueue(point);
            }
        }
    }

    public static void PrintTheThing(HashSet<Point> rockyBois, Point[] tetromino, long maxY)
    {
        if (System.Diagnostics.Debugger.IsAttached) return;
        long minY = rockyBois.Min(p => p.Y);

        for (long y = maxY; y >= minY; y--)
        {
            Console.Write("|");
            for (long x = 0; x < 7; x++)
            {
                var point = new Point(x, y);
                var isRock = rockyBois.Contains(point);
                var isTetromino = tetromino.Contains(point);

                if (isRock && isTetromino) Console.Write("X");
                else if (isRock) Console.Write("#");
                else if (isTetromino) Console.Write("@");
                else Console.Write(".");
            }
            Console.WriteLine("|");
        }
        Console.WriteLine("+-------+");
        // Console.ReadKey();
        // Console.Clear();
    }

    public static (Point[] next, bool done) Step(Point[] points, int direction, HashSet<Point> rockyBois)
    {
        var next = points.Select(p => new Point(p.X + direction, p.Y)).ToArray();
        if (next.Any(p => p.X < 0 || p.X > maxX || rockyBois.Contains(p)))
            next = points;

        var canFall = next.All(p => p.Y > 1 && !rockyBois.Contains(new Point(p.X, p.Y - 1)));
        if (!canFall) return (next, true);

        next = next.Select(p => new Point(p.X, p.Y - 1)).ToArray();
        return (next, false);
    }

    public static IEnumerable<Point[]> GetTetrominos()
    {
        for (int i = 0; ; i++)
        {
            if (i == Tetrominos.Length) i = 0;
            yield return Tetrominos[i];
        }
    }

    public static IEnumerable<int> ReadDirections()
    {
        var content = File.ReadAllText("input.txt").Trim();
        for (int i = 0; ; i++)
        {
            if (i == content.Length) i = 0;
            yield return content[i] == '<' ? -1 : 1;
        }
    }
}

public record struct Point(long X, long Y);
