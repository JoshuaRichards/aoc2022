using System.Text.RegularExpressions;

namespace Day21;

public class Program
{
    public static void Main()
    {
        var monkes = GetMonkes();
        var part1 = monkes["root"].Compute();
        Console.WriteLine(part1);
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
                    Compute = () => value,
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
                    Compute = () =>
                    {
                        var answer = dict[left].Compute();
                        answer = @operator switch
                        {
                            "*" => answer * dict[right].Compute(),
                            "+" => answer + dict[right].Compute(),
                            "-" => answer - dict[right].Compute(),
                            "/" => answer / dict[right].Compute(),
                            _ => throw new InvalidOperationException(),
                        };
                        return answer;
                    }
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
    public required Func<long> Compute { get; init; }
}