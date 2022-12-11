namespace Day10;

public class Program
{
    public static void Main()
    {
        Part1();
        Part2();
    }

    public static void Part2()
    {
        var cycles = Enumerable.Range(1, 240);

        var positions = GetPositions();
        foreach (var cycle in cycles)
        {
            var xCoord = (cycle - 1) % 40;
            if (Math.Abs(xCoord - positions[cycle]) <= 1)
            {
                Console.Write("#");
            }
            else
            {
                Console.Write(".");
            }
            if (cycle % 40 == 0)
            {
                Console.WriteLine();
            }
        }
    }

    public static Dictionary<int, int> GetPositions()
    {
        int cycle = 1;
        int x = 1;
        var ret = new Dictionary<int, int>();
        ret[1] = 1;

        foreach (var (instruction, param) in ReadInstructions())
        {
            var (nextCycle, nextX) = RunInstruction(cycle, x, instruction, param);
            for (int i = cycle; i < nextCycle; i++)
            {
                ret[i] = x;
            }

            cycle = nextCycle;
            x = nextX;
            ret[cycle] = nextCycle;
        }

        return ret;
    }

    public static void Part1()
    {
        int cycle = 1;
        int x = 1;
        var importantCycles = new HashSet<int> { 20, 60, 100, 140, 180, 220 };
        var xValues = new List<int>();

        foreach (var (instruction, param) in ReadInstructions())
        {
            var (nextCycle, nextX) = RunInstruction(cycle, x, instruction, param);
            if (importantCycles.Any(c => cycle < c && nextCycle > c))
            {
                xValues.Add(x);
            }
            else if (importantCycles.Any(c => nextCycle == c))
            {
                xValues.Add(nextX);
            }

            cycle = nextCycle;
            x = nextX;

            if (cycle > importantCycles.Max()) break;
        }

        var part1 = importantCycles.Zip(xValues, (a, b) => a * b).Sum();
        Console.WriteLine(part1);

    }

    public static (int cycle, int x) RunInstruction(int cycle, int x, string instruction, int? param)
    {
        if (instruction == "noop")
        {
            return (cycle + 1, x);
        }
        if (instruction == "addx")
        {
            return (cycle + 2, x + param!.Value);
        }

        throw new InvalidOperationException();
    }


    public static IEnumerable<(string instruction, int? param)> ReadInstructions()
    {
        return File.ReadLines("input.txt")
            .Select(line => line.Split(" "))
            .Select(parts => (parts[0], (parts.Length == 2 ? int.Parse(parts[1]) as int? : null as int?)));
    }
}