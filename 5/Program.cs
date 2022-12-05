using System.Text.RegularExpressions;

namespace Day5;

public class Program
{
    public static void Main()
    {
        Console.WriteLine(Run(ExecuteMove));
        Console.WriteLine(Run(ExecuteMove2));
    }

    private static string Run(Action<Dictionary<int, Stack<char>>, MoveInstruction> mover)
    {
        var state = ReadInitialState();
        var moves = ReadMoves();

        foreach (var move in moves)
        {
            mover(state, move);
        }

        var chars = state.OrderBy(kvp => kvp.Key)
            .Where(kvp => kvp.Value.Any())
            .Select(kvp => kvp.Value.Peek())
            .ToArray();

        return new string(chars);
    }

    private static void ExecuteMove(Dictionary<int, Stack<char>> state, MoveInstruction move)
    {
        for (int i = 0; i < move.Amount; i++)
        {
            state[move.Destination].Push(state[move.Source].Pop());
        }
    }

    private static void ExecuteMove2(Dictionary<int, Stack<char>> state, MoveInstruction move)
    {
        var temp = new Stack<char>();
        for (int i = 0; i < move.Amount; i++)
        {
            temp.Push(state[move.Source].Pop());
        }

        while (temp.Any())
        {
            state[move.Destination].Push(temp.Pop());
        }
    }

    private static Dictionary<int, Stack<char>> ReadInitialState()
    {
        var section = File.ReadLines("input.txt")
            .TakeWhile(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        var labelLine = section.Last();

        var ret = new Dictionary<int, Stack<char>>();
        for (int i = 0; i < labelLine.Length; i++)
        {
            if (!int.TryParse(labelLine[i].ToString(), out int id))
            {
                continue;
            }

            var contents = section.Reverse().Skip(1)
                .Select(line => line[i])
                .TakeWhile(char.IsUpper);

            ret[id] = new Stack<char>(contents);
        }

        return ret;
    }

    private static IEnumerable<MoveInstruction> ReadMoves()
    {
        return File.ReadLines("input.txt")
            .SkipWhile(line => !line.StartsWith("move"))
            .Select(line => Regex.Match(line, @"move (\d+) from (\d) to (\d)"))
            .Select(match => new MoveInstruction
            {
                Amount = int.Parse(match.Groups[1].Value),
                Source = int.Parse(match.Groups[2].Value),
                Destination = int.Parse(match.Groups[3].Value),
            });
    }
}

public class MoveInstruction
{
    public int Amount { get; set; }
    public int Source { get; set; }
    public int Destination { get; set; }
}