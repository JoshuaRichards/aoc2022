namespace Day13;

public class Program
{
    public static void Main()
    {
        var pairs = ReadPackets()
            .Select((value, i) => new { value, i })
            .GroupBy(x => x.i / 2, x => x.value)
            .Select((g, i) => new { Left = g.First(), Right = g.Last(), Index = i + 1 })
            .ToArray();

        var part1 = pairs
            .Where(x => x.Left.Compare(x.Right) < 0)
            .Sum(x => x.Index);

        Console.WriteLine(part1);

        var dividerPackets = new[]
        {
            ReadArray("[2]]").Item1,
            ReadArray("[6]]").Item1,
        };

        var orderedPackets = ReadPackets().Concat(dividerPackets)
            .OrderBy(p => p)
            .Select((p, i) => new { Packet = p, Index = i + 1 })
            .ToArray();

        var part2 = orderedPackets
            .Where(o => dividerPackets.Any(dp => o.Packet.Compare(dp) == 0))
            .Select(o => o.Index)
            .Aggregate(1, (acc, next) => acc * next);
        
        Console.WriteLine(part2);
    }

    public static IEnumerable<Value> ReadPackets()
    {
        foreach (var line in File.ReadLines("input.txt"))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            yield return ReadArray(line[1..]).Item1;
        }
    }

    public static (Value, int) ReadArray(ReadOnlySpan<char> input)
    {
        var values = new List<Value>();
        int i;
        for (i = 0; i < input.Length; i++)
        {
            if (input[i] == '[')
            {
                var (value, count) = ReadArray(input[(i + 1)..]);
                values.Add(value);
                i += count;
                continue;
            }
            if (input[i] == ']')
            {
                i++;
                break;
            }
            if (char.IsDigit(input[i]))
            {
                var s = input[i].ToString();
                while (char.IsDigit(input[++i])) s += input[i];
                i--;
                values.Add(new Value { Int = int.Parse(s) });
                continue;
            }
            if (input[i] == ',') continue;
            throw new InvalidOperationException();
        }

        return (new Value { Array = values.ToArray() }, i);
    }
}

public class Value : IComparable<Value>
{
    public int? Int { get; set; }
    public Value[]? Array { get; set; }

    public int Compare(Value right)
    {
        ArgumentNullException.ThrowIfNull(right);
        if (Int is not null && right.Int is not null)
        {
            return Int.Value - right.Int!.Value;
        }

        if (Array is not null && right!.Array is not null)
        {
            for (int i = 0; i < Math.Min(Array.Length, right.Array.Length); i++)
            {
                var result = Array[i].Compare(right.Array[i]);
                if (result != 0) return result;
            }
            return Array.Length - right.Array.Length;
        }

        if (Array is not null && right!.Array is null)
        {
            return Compare(new Value { Array = new[] { right } });
        }

        if (Array is null && right!.Array is not null)
        {
            return new Value { Array = new[] { this } }.Compare(right);
        }

        throw new InvalidOperationException();
    }

    public int CompareTo(Value? other)
    {
        return Compare(other!);
    }
}
