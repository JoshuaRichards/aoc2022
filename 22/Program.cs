using System.Text.RegularExpressions;

namespace Day22;

public class Program
{
    public static void Main()
    {
        var grid = ReadGrid();
        var instructions = ReadInstructions().ToArray();
        var position = grid.Keys.Where(k => k.Y == 0).MinBy(k => k.X);
        var direction = Direction.Right;
        // PrintState(grid);

        foreach (var instruction in instructions)
        {
            (direction, position) = instruction.Execute(grid, direction, position);
        }

        var part1 = (1000 * (position.Y + 1)) + (4 * (position.X + 1)) + (int)direction;
        Console.WriteLine(part1);
    }

    public static void PrintState(Dictionary<Point, char> grid)
    {
        if (System.Diagnostics.Debugger.IsAttached) return;

        var minX = 0;
        var maxX = grid.Keys.Select(k => k.X).Max();
        var minY = 0;
        var maxY = grid.Keys.Select(k => k.Y).Max();

        for (var y = minY; y <= maxY; y++)
        {
            for (var x = minX; x <= maxX; x++)
            {
                var c = grid.GetValueOrDefault((x, y), ' ');
                Console.Write(c);
            }
            Console.WriteLine();
        }
    }

    public static Dictionary<Point, char> ReadGrid()
    {
        var dict = new Dictionary<Point, char>();
        var y = 0;
        foreach (var line in File.ReadLines("input.txt").TakeWhile(line => !string.IsNullOrWhiteSpace(line)))
        {
            for (int x = 0; x < line.Length; x++)
            {
                if (line[x] == ' ') continue;
                dict[(x, y)] = line[x];
            }
            y++;
        }

        return dict;
    }

    public static IEnumerable<IInstruction> ReadInstructions()
    {
        var line = File.ReadLines("input.txt").Reverse().SkipWhile(line => string.IsNullOrWhiteSpace(line)).First();

        var matches = Regex.Matches(line, @"(\d+)([LR])");
        foreach (var match in matches.Cast<Match>())
        {
            yield return new MoveInstruction
            {
                Distance = int.Parse(match.Groups[1].Value),
            };
            yield return new RotateInstruction
            {
                Instruction = match.Groups[2].Value[0],
            };
        }
        var m = Regex.Match(line, @"(\d+)$");
        yield return new MoveInstruction
        {
            Distance = int.Parse(m.Groups[1].Value),
        };
    }

}

public enum Direction
{
    Right, Down, Left, Up,
}

public record struct Point
{
    public required int X { get; init; }
    public required int Y { get; init; }

    public static implicit operator Point((int, int) p) => new Point
    {
        X = p.Item1,
        Y = p.Item2
    };

    public static Point operator +(Point a, Point b) => new Point
    {
        X = a.X + b.X,
        Y = a.Y + b.Y,
    };
}

public interface IInstruction
{
    (Direction, Point) Execute(Dictionary<Point, char> grid, Direction direction, Point position);
}

public class MoveInstruction : IInstruction
{
    public required int Distance { get; init; }

    public (Direction, Point) Execute(Dictionary<Point, char> grid, Direction direction, Point position)
    {
        Point transform = direction switch
        {
            Direction.Right => (1, 0),
            Direction.Down => (0, 1),
            Direction.Left => (-1, 0),
            Direction.Up => (0, -1),
            _ => throw new NotImplementedException(),
        };

        var current = position;
        for (int i = 0; i < Distance; i++)
        {
            var next = current + transform;
            if (!grid.ContainsKey(next)) next = direction switch
            {
                Direction.Right => grid.Keys.Where(k => k.Y == current.Y).MinBy(k => k.X),
                Direction.Down => grid.Keys.Where(k => k.X == current.X).MinBy(k => k.Y),
                Direction.Left => grid.Keys.Where(k => k.Y == current.Y).MaxBy(k => k.X),
                Direction.Up => grid.Keys.Where(k => k.X == current.X).MaxBy(k => k.Y),
                _ => throw new NotImplementedException(),
            };
            if (grid[next] == '#') break;
            current = next;
        }

        return (direction, current);
    }
}

public class RotateInstruction : IInstruction
{
    public required char Instruction { get; init; }

    public (Direction, Point) Execute(Dictionary<Point, char> grid, Direction direction, Point position)
    {
        var directions = Enum.GetValues<Direction>();
        var rot = Instruction == 'L' ? -1 : 1;
        var current = Array.IndexOf(directions, direction);
        var nextDirection = directions[NonShitModulo(current + rot, directions.Length)];

        return (nextDirection, position);
    }

    private static int NonShitModulo(long a, int b)
    {
        return (int)((Math.Abs(a * b) + a) % b);
    }
}