using System.Text.RegularExpressions;

namespace Day16;

public class Program
{
    public static void Main()
    {
        var graph = ReadValves().ToDictionary(v => v.Name);
        var shortKings = ShortKings(graph);
        var maxFlows = MaxFlows(graph, shortKings, 30);
        var part1 = maxFlows.Values.Max();
        Console.WriteLine(part1);

        var garbageValves = graph.Values.Where(v => v.FlowRate == 0).Select(v => v.Name).ToHashSet();
        var maxFlows2 = MaxFlows(graph, shortKings, 26);
        Console.WriteLine($"got part 2 dict. {maxFlows2.Count()} entries");
        var part2 = maxFlows2.SelectMany(flow => maxFlows2.Select(otherFlow => (flow, otherFlow)))
            .Where(pair => pair.flow.Key.Split("|").Where(v => !garbageValves.Contains(v)).ToHashSet().Intersect(pair.otherFlow.Key.Split("|")).Count() == 0)
            .Max(pair => pair.flow.Value + pair.otherFlow.Value);
        Console.WriteLine(part2);
    }

    public static Dictionary<string, int> MaxFlows(Dictionary<string, Valve> graph, Dictionary<(string, string), int> shortKings, int remaining)
    {
        var ret = new Dictionary<string, int>();
        var queue = new Queue<State>();
        queue.Enqueue(new State
        {
            CurrentValve = "AA",
            OpenValves = graph.Values.Where(v => v.FlowRate == 0).ToDictionary(v => v.Name, _ => 0),
            TimeRemaining = remaining,
        });

        while (queue.Any())
        {
            var current = queue.Dequeue();
            foreach (var next in GetPossibleMoves(current, graph, shortKings))
            {
                ret[next.OpenBoi] = Math.Max(ret.GetValueOrDefault(next.OpenBoi), next.Score);
                queue.Enqueue(next);
            }
        }

        return ret;
    }

    public static State[] GetPossibleMoves(State state, Dictionary<string, Valve> graph, Dictionary<(string, string), int> shortKings)
    {
        if (state.TimeRemaining == 0)
            return new State[0];
        if (state.OpenValves.Count() == graph.Count())
            return new State[0];
        var ret = new List<State>();

        var destinations = graph.Values.Where(valve => valve.Name != state.CurrentValve && !state.OpenValves.ContainsKey(valve.Name));
        foreach (var dest in destinations)
        {
            var costToOpen = shortKings[(state.CurrentValve, dest.Name)] + 1;
            if (costToOpen >= state.TimeRemaining) continue;

            ret.Add(new State
            {
                CurrentValve = dest.Name,
                OpenValves = state.OpenValves
                    .Append(new KeyValuePair<string, int>(dest.Name, dest.FlowRate * (state.TimeRemaining - costToOpen)))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                TimeRemaining = state.TimeRemaining - costToOpen,
            });
        }

        return ret.ToArray();
    }

    public static Dictionary<(string, string), int> ShortKings(Dictionary<string, Valve> graph)
    {
        var ret = new Dictionary<(string, string), double>();
        var combos = graph.Keys.SelectMany(source => graph.Keys.Select(dest => (source, dest)));
        foreach (var combo in combos) ret[combo] = double.PositiveInfinity;

        var actualEdges = graph.Values.SelectMany(valve => valve.Connections.Select(dest => (valve.Name, dest)));
        foreach (var edge in actualEdges)
        {
            ret[edge] = 1;
        }
        foreach (var v in graph.Keys) ret[(v, v)] = 0;

        foreach (var k in graph.Keys)
        {
            foreach (var i in graph.Keys)
            {
                foreach (var j in graph.Keys)
                {
                    if (ret[(i, j)] > ret[(i, k)] + ret[(k, j)])
                    {
                        ret[(i, j)] = ret[(i, k)] + ret[(k, j)];
                    }
                }
            }
        }

        return ret.ToDictionary(kvp => kvp.Key, kvp => (int)kvp.Value);
    }

    public static IEnumerable<Valve> ReadValves()
    {
        return File.ReadLines("input.txt")
            .Select(line => Regex.Match(line, @"Valve (\w+) has flow rate\=(\d+); tunnels? leads? to valves? (?:(\w+)(?:\, )?)+"))
            .Select(match => new Valve(
                Name: match.Groups[1].Value,
                FlowRate: int.Parse(match.Groups[2].Value),
                Connections: match.Groups[3].Captures.Select(c => c.Value).ToArray()
            ));
    }
}

public record struct Valve(
    string Name,
    int FlowRate,
    string[] Connections
);

public class State
{
    public string CurrentValve { get; init; } = "";

    public Dictionary<string, int> OpenValves { get; init; } = new();

    public int TimeRemaining { get; init; }

    private readonly Lazy<int> _score;
    public int Score => _score.Value;

    private readonly Lazy<string> _openBoi;
    public string OpenBoi => _openBoi.Value;

    public State()
    {
        _score = new Lazy<int>(() => OpenValves.Values.Sum());
        _openBoi = new Lazy<string>(() => string.Join("|", OpenValves.Keys.OrderBy(x => x)));
    }

    public override int GetHashCode()
    {
        var valveStr = string.Join(":", OpenValves.Select(kvp => (kvp.Key, kvp.Value)).OrderBy(x => x));
        var fullstr = string.Join("|", new[] { CurrentValve, valveStr, TimeRemaining.ToString() });
        return fullstr.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (obj is not State right) return false;
        return this == right;
    }

    public static bool operator !=(State? left, State? right)
    {
        return !(left == right);
    }
    public static bool operator ==(State? left, State? right)
    {
        if (left is null && right is null) return true;
        if ((left is null) != (right is null)) return true;

        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        if (left.CurrentValve != right.CurrentValve) return false;
        if (left.TimeRemaining != right.TimeRemaining) return false;

        return left.OpenValves
            .Select(kvp => (kvp.Key, kvp.Value))
            .ToHashSet()
            .SetEquals(
                right.OpenValves.Select(kvp => (kvp.Key, kvp.Value))
            );
    }
}