using System.Numerics;
using System.Text.RegularExpressions;
using IntXLib;

namespace Day11;

public class Program
{
    public static void Main()
    {
        var part1 = GetMonkeyBusiness(20, 3);
        Console.WriteLine(part1);

        // GetMonkeyBusiness(900, 1);

        var part2 = GetMonkeyBusiness(10000, 1);
        Console.WriteLine(part2);

        // foreach (var rounds in new[] { 1, 20, 1000, 2000, 4000, 5000, 6000, 7000, 8000, 9000, 10_000 })
        // {
        //     GetMonkeyBusiness(rounds, 1);
        // }
    }

    public static long GetMonkeyBusiness(int rounds, int worryDivision)
    {
        var monkeys = ReadMonkeys().ToArray();

        var moduloBoi = monkeys.Select(m => m.Test).Aggregate(1L, (acc, next) => acc * next);
        for (int i = 0; i < rounds; i++)
        {
            // Console.WriteLine($"round {i}");
            foreach (var monkey in monkeys)
            {
                while (monkey.Items.Any())
                {
                    var item = monkey.Items.Dequeue();
                    item = monkey.Operation(item);
                    monkey.InspectionCount++;
                    item /= worryDivision;
                    item %= moduloBoi;

                    var test = item % monkey.Test == 0;
                    monkeys[test ? monkey.TrueDest : monkey.FalseDest].Items.Enqueue(item);
                }
            }
        }

        return monkeys.OrderByDescending(m => m.InspectionCount).Take(2).Aggregate(1L, (acc, b) => acc * b.InspectionCount);
    }

    public static IEnumerable<Monkey> ReadMonkeys()
    {
        Monkey? currentMonkey = null;

        foreach (var line in File.ReadLines("input.txt"))
        {
            Match match;
            if (string.IsNullOrWhiteSpace(line))
            {
                yield return currentMonkey!;
                currentMonkey = null;
                continue;
            }
            if (Regex.IsMatch(line, @"Monkey \d:"))
            {
                currentMonkey = new Monkey();
                continue;
            }

            if ((match = Regex.Match(line, @"Starting items: (?:(\d+), )*(\d+)")).Success)
            {
                var items = match.Groups[1].Captures.Select(c => c.Value).Append(match.Groups[2].Value);
                foreach (var item in items)
                {
                    currentMonkey!.Items.Enqueue(int.Parse(item));
                }
                continue;
            }

            if ((match = Regex.Match(line, @"Operation: new = old ([\*\+]) (old|\d+)")).Success)
            {
                var @operator = match.Groups[1].Value;
                var operandStr = match.Groups[2].Value;
                var operand = operandStr == "old" ? null : long.Parse(operandStr) as long?;
                currentMonkey!.Operation = (long old) =>
                {
                    var actualOperand = operand ?? old;
                    return @operator == "*" ? old * actualOperand : old + actualOperand;
                };

                continue;
            }

            if ((match = Regex.Match(line, @"Test: divisible by (\d+)")).Success)
            {
                currentMonkey!.Test = int.Parse(match.Groups[1].Value);
                continue;
            }

            if ((match = Regex.Match(line, @"If true: throw to monkey (\d)")).Success)
            {
                currentMonkey!.TrueDest = int.Parse(match.Groups[1].Value);
                continue;
            }

            if ((match = Regex.Match(line, @"If false: throw to monkey (\d)")).Success)
            {
                currentMonkey!.FalseDest = int.Parse(match.Groups[1].Value);
                continue;
            }

            throw new InvalidOperationException();
        }

        if (currentMonkey is not null)
        {
            yield return currentMonkey;
        }
    }
}

public class Monkey
{
    public Queue<long> Items { get; } = new();
    public Func<long, long> Operation { get; set; } = null!;
    public long Test { get; set; }
    public int TrueDest { get; set; }
    public int FalseDest { get; set; }
    public long InspectionCount { get; set; }
}