using System.Text.RegularExpressions;

namespace Day2;

public class Program
{
    private static readonly Dictionary<(Rps, Rps), bool?> Outcomes = new()
    {
        [(Rps.Rock, Rps.Rock)] = null,
        [(Rps.Rock, Rps.Paper)] = false,
        [(Rps.Rock, Rps.Scissors)] = true,
        [(Rps.Paper, Rps.Rock)] = true,
        [(Rps.Paper, Rps.Paper)] = null,
        [(Rps.Paper, Rps.Scissors)] = false,
        [(Rps.Scissors, Rps.Rock)] = false,
        [(Rps.Scissors, Rps.Paper)] = true,
        [(Rps.Scissors, Rps.Scissors)] = null,
    };

    private static readonly Dictionary<(Rps, bool?), Rps> Solutions = new()
    {
        [(Rps.Rock, false)] = Rps.Scissors,
        [(Rps.Rock, null)] = Rps.Rock,
        [(Rps.Rock, true)] = Rps.Paper,
        [(Rps.Paper, false)] = Rps.Rock,
        [(Rps.Paper, null)] = Rps.Paper,
        [(Rps.Paper, true)] = Rps.Scissors,
        [(Rps.Scissors, false)] = Rps.Paper,
        [(Rps.Scissors, null)] = Rps.Scissors,
        [(Rps.Scissors, true)] = Rps.Rock,
    };

    public static void Main()
    {
        var part1 = ReadGames().Select(GetScore).Sum();
        Console.WriteLine(part1);
        var part2 = ReadGames2().Select(GetScore).Sum();
        Console.WriteLine(part2);
    }

    private static int GetScore((Rps, Rps) game)
    {
        var (left, right) = game;
        var result = Outcomes[(right, left)];
        if (result == false) return 0 + (int)right;
        if (result == null) return 3 + (int)right;
        return 6 + (int)right;
    }

    private static IEnumerable<(Rps, Rps)> ReadGames()
    {
        return File.ReadLines("input.txt")
            .Select(line => Regex.Match(line, @"(\w)\s+(\w)"))
            .Where(m => m.Success)
            .Select(m => (GetMove(m.Groups[1].Value[0]), GetMove(m.Groups[2].Value[0])));
    }

    private static IEnumerable<(Rps, Rps)> ReadGames2()
    {
        return File.ReadLines("input.txt")
            .Select(line => Regex.Match(line, @"(\w)\s+(\w)"))
            .Where(m => m.Success)
            .Select(m => new { Left = GetMove(m.Groups[1].Value[0]), DesiredOutcome = GetDesiredOutcome(m.Groups[2].Value[0])})
            .Select(o => (o.Left, Solutions[(o.Left, o.DesiredOutcome)]));
    }

    private static bool? GetDesiredOutcome(char c)
    {
        return c switch
        {
            'X' => false,
            'Y' => null,
            'Z' => true,
            _ => throw new InvalidOperationException(),
        };
    }

    private static Rps GetMove(char c)
    {
        switch (c)
        {
            case 'A':
            case 'X':
                return Rps.Rock;
            case 'B':
            case 'Y':
                return Rps.Paper;
            case 'C':
            case 'Z':
                return Rps.Scissors;
        }

        throw new InvalidOperationException();
    }

    public enum Rps
    {
        Rock = 1,
        Paper = 2,
        Scissors = 3,
    }
}