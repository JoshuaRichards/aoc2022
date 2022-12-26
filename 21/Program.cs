using System.Text.RegularExpressions;

namespace Day21;

public class Program
{
    public static void Main()
    {
        var monkes = GetMonkes();
        var root = BuildNode(monkes, "root");
        var part1 = root.Compute();
        Console.WriteLine(part1);

        var left = root.Left!;
        var right = root.Right!;
        while (left.Name != "humn" && right.Name != "humn")
        {
            var (humnSide, nonHumnSide) = left.HasHumn() ? (left, right) : (right, left);
            if (humnSide.Operator == "+")
            {
                (left, right) = SimplifyPlus(humnSide, nonHumnSide);
            }
            else if (humnSide.Operator == "-")
            {
                (left, right) = SimplifyMinus(humnSide, nonHumnSide);
            }
            else if (humnSide.Operator == "*")
            {
                (left, right) = SimplifyTimes(humnSide, nonHumnSide);
            }
            else
            {
                (left, right) = SimplifyDivide(humnSide, nonHumnSide);
            }
        }

        var part2 = left.Name == "humn" ? right.Compute() : left.Compute();
        Console.WriteLine(part2);
    }

    public static (Node, Node) SimplifyPlus(Node humn, Node nonHumn)
    {
        var (innerHumn, innerNonHumn) = humn.Left!.HasHumn() ? (humn.Left, humn.Right) : (humn.Right, humn.Left);
        var left = innerHumn;
        var right = new Node
        {
            Name = GenerateName(),
            Left = nonHumn,
            Right = innerNonHumn,
            Operator = "-",
        };
        return (left!, right);
    }

    public static (Node, Node) SimplifyMinus(Node humn, Node nonHumn)
    {
        return (humn.Left!, new Node
        {
            Name = GenerateName(),
            Left = nonHumn,
            Right = humn.Right!,
            Operator = "+",
        });
    }

    public static (Node, Node) SimplifyTimes(Node humn, Node nonHumn)
    {
        var (innerHumn, innerNonHumn) = humn.Left!.HasHumn() ? (humn.Left, humn.Right) : (humn.Right, humn.Left);
        var left = innerHumn;
        var right = new Node
        {
            Name = GenerateName(),
            Left = nonHumn,
            Right = innerNonHumn,
            Operator = "/",
        };
        return (left!, right);
    }

    public static (Node, Node) SimplifyDivide(Node humn, Node nonHumn)
    {
        var ret = (humn.Left!, new Node
        {
            Name = GenerateName(),
            Left = nonHumn,
            Right = humn.Right,
            Operator = "*",
        });
        return ret;
    }

    public static Node BuildNode(Dictionary<string, Monke> monkes, string name)
    {
        var monke = monkes[name];
        if (monke.Value is not null) return new Node
        {
            Name = name,
            Value = monke.Value,
        };

        return new Node
        {
            Name = name,
            Left = BuildNode(monkes, monke.Left!),
            Right = BuildNode(monkes, monke.Right!),
            Operator = monke.Operator,
        };
    }

    public static string GenerateName()
    {
        var chars = "abcdefghijklmnopqrstuvwxyz";
        var chosed = Enumerable.Range(0, 4).Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray();
        return "_" + new string(chosed);
    }

    public static Dictionary<string, Monke> GetMonkes()
    {
        var dict = new Dictionary<string, Monke>();

        foreach (var line in File.ReadLines("input.txt"))
        {
            var name = Regex.Match(line, @"^(\w+):").Groups[1].Value;
            Match match;

            if ((match = Regex.Match(line, @"(\d+)")).Success)
            {
                var value = int.Parse(match.Groups[1].Value);
                dict[name] = new Monke
                {
                    Name = name,
                    Value = value,
                };
                continue;
            }

            if ((match = Regex.Match(line, @"(\w+) ([\+\-\*\/]) (\w+)")).Success)
            {
                var left = match.Groups[1].Value;
                var right = match.Groups[3].Value;
                var @operator = match.Groups[2].Value;
                dict[name] = new Monke
                {
                    Name = name,
                    Left = left,
                    Right = right,
                    Operator = @operator,
                };
                continue;
            }
        }

        return dict;
    }

}

public record class Monke
{
    public required string Name { get; init; }
    public string? Left { get; init; }
    public string? Right { get; init; }
    public string? Operator { get; init; }
    public long? Value { get; init; }
}

public class Node
{
    public required string Name { get; init; }
    public Node? Left { get; init; }
    public Node? Right { get; init; }
    public string? Operator { get; init; }
    public long? Value { get; init; }

    public long Compute()
    {
        if (Value is not null) return Value.Value;

        return Operator switch
        {
            "*" => Left!.Compute() * Right!.Compute(),
            "+" => Left!.Compute() + Right!.Compute(),
            "-" => Left!.Compute() - Right!.Compute(),
            "/" => Left!.Compute() / Right!.Compute(),
            _ => throw new NotImplementedException(),
        };
    }

    public bool HasHumn()
    {
        if (Name == "humn") return true;
        if (Value is not null) return false;

        return Left!.HasHumn() || Right!.HasHumn();
    }

    public int Count()
    {
        if (Value is not null) return 1;

        return Left!.Count() + Right!.Count();
    }

    public override string ToString()
    {
        if (Value is not null) return Name;

        return $"({Left}) {Operator} ({Right})";
    }
}