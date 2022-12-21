using System.Text.RegularExpressions;

namespace Day16;

public class Program
{
    public static void Main()
    {
        var graph = ReadValves().ToDictionary(v => v.Name);

        var part1 = Search(graph, 30);
        Console.WriteLine(part1);
    }

    public static int GetMaxFlow(Dictionary<string, Valve> graph, Valve current, string[] visited, int timeRemaining)
    {
        Console.WriteLine($"visiting {current}");
        visited = visited.Append(current.Name).ToArray();
        timeRemaining--;

        var candidates = current.Connections.Where(c => !visited.Contains(c)).ToArray();

        return (current.FlowRate * timeRemaining) + (candidates.Any() ? candidates.Max(c => GetMaxFlow(graph, graph[c], visited, timeRemaining - 1)) : 0);
    }

    public static int Search(Dictionary<string, Valve> graph, int timeRemaining)
    {
        var open = new Queue<State>();
        var shortKings = ShortKings(graph);
        open.Enqueue(new State
        {
            CurrentValve = "AA",
            OpenValves = graph.Values.Where(v => v.FlowRate == 0).ToDictionary(v => v.Name, _ => 0),
            TimeRemaining = timeRemaining,
        });
        var closed = new HashSet<State>();

        var best = new Dictionary<int, int>
        {
            [0] = open.First().Score,
        };

        var maxFlowRate = graph.Values.Max(v => v.FlowRate);

        while (open.Any())
        {
            var current = open.Dequeue();
            closed.Add(current);

            Console.WriteLine($"at {current.CurrentValve}, timeRemaining: {current.TimeRemaining}, {open.Count()} states left");

            var newBest = current.Score > best.GetValueOrDefault(current.TimeRemaining);
            if (newBest)
                best[current.TimeRemaining] = current.Score;

            var possibleMoves = GetPossibleMoves(current, graph, shortKings).Where(m => !closed.Contains(m));

            foreach (var move in possibleMoves) open.Enqueue(move);
        }

        return best.Values.Max();
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
            if (costToOpen > state.TimeRemaining) continue;

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
    public string CurrentValve { get; set; } = "";
    public Dictionary<string, int> OpenValves { get; set; } = new();
    public int TimeRemaining { get; set; }

    public int Score => OpenValves.Values.Sum();

    public override int GetHashCode()
    {
        var valveStr = string.Join("", OpenValves.Select(kvp => (kvp.Key, kvp.Value)).OrderBy(x => x));
        var fullstr = CurrentValve + valveStr + TimeRemaining;
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