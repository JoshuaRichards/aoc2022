namespace Day9;

public class Program
{
    private static readonly Dictionary<string, (int x, int y)> OrdinalDirections = new()
    {
        ["L"] = (-1, 0),
        ["R"] = (1, 0),
        ["U"] = (0, 1),
        ["D"] = (0, -1),
    };

    private static readonly (int x, int y)[] Diagonals = new[]
    {
        (1, 1),
        (-1, -1),
        (-1, 1),
        (1, -1),
    };

    private static readonly (int x, int y)[] AllDirections = OrdinalDirections.Values.Concat(Diagonals).Append((0, 0)).ToArray();

    public static void Main()
    {
        Part1();
        Part2();
    }

    public static void Part1()
    {
        var visited = new HashSet<(int, int)>();
        var head = (0, 0);
        var tail = (0, 0);
        visited.Add(tail);

        foreach (var move in ReadInput())
        {
            // Console.WriteLine($"======{OrdinalDirections.First(kvp => kvp.Value == (move.x, move.y)).Key} - {move.distance}=====");
            // Console.WriteLine($"direction: {(move.x, move.y)}");
            for (int i = 0; i < move.distance; i++)
            {
                head = MoveHead(head, (move.x, move.y));
                // Console.WriteLine($"head moving to {head}");
                tail = MoveTail(head, tail);
                // Console.WriteLine($"tail moving to {tail}");
                // PrintLocations(head, tail);
                // Console.WriteLine();
                visited.Add(tail);
            }
        }

        Console.WriteLine(visited.Count());
    }

    public static void Part2()
    {
        var visited = new HashSet<(int, int)>();
        var head = (0, 0);
        var knots = Enumerable.Range(0, 9).Select(_ => (0, 0)).ToArray();
        visited.Add(head);

        foreach (var move in ReadInput())
        {
            // Console.WriteLine($"======{OrdinalDirections.First(kvp => kvp.Value == (move.x, move.y)).Key} - {move.distance}=====");
            // Console.WriteLine($"direction: {(move.x, move.y)}");
            for (int i = 0; i < move.distance; i++)
            {
                head = MoveHead(head, (move.x, move.y));
                // Console.WriteLine($"head moving to {head}");
                knots[0] = MoveTail(head, knots[0]);
                for (int j = 1; j < knots.Length; j++)
                {
                    knots[j] = MoveTail(knots[j - 1], knots[j]);
                }
                // Console.WriteLine($"tail moving to {tail}");
                // PrintLocations(head, tail);
                // Console.WriteLine();
                visited.Add(knots.Last());
            }
        }

        Console.WriteLine(visited.Count());
    }

    private static void PrintLocations((int x, int y) head, (int x, int y) tail)
    {
        var minX = 0;
        var minY = 0;
        var maxX = 5;
        var maxY = 5;
        for (int y = maxY; y >= minY; y--)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if ((x, y) == head)
                {
                    Console.Write("H");
                }
                else if ((x, y) == tail)
                {
                    Console.Write("T");
                }
                else if ((x, y) == (0, 0))
                {
                    Console.Write("s");
                }
                else
                {
                    Console.Write(".");
                }
            }
            Console.WriteLine();
        }
    }

    private static (int x, int y) MoveHead((int x, int y) head, (int x, int y) direction)
    {
        return (head.x + direction.x, head.y + direction.y);
    }

    private static (int x, int y) MoveTail((int x, int y) head, (int x, int y) tail)
    {
        if (ApplyDirections(tail, AllDirections).Contains(head)) return tail;

        // return ApplyDirections(tail, AllDirections)
        //     .First(target => ApplyDirections(head, AllDirections).Contains(target));
        return ApplyDirections(head, AllDirections)
            .First(target => ApplyDirections(tail, AllDirections).Contains(target));
    }

    private static IEnumerable<(int x, int y)> ApplyDirections((int x, int y) start, IEnumerable<(int x, int y)> directions)
    {
        return directions.Select(d => (start.x + d.x, start.y + d.y));
    }

    public static IEnumerable<(int x, int y, int distance)> ReadInput()
    {
        return File.ReadLines("input.txt")
            .Select(line => line.Split(" "))
            .Select(parts => new { Direction = OrdinalDirections[parts[0]], Distance = int.Parse(parts[1]) })
            .Select(o => (o.Direction.x, o.Direction.y, o.Distance));
    }
}
