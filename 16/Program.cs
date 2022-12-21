using System.Text.RegularExpressions;

namespace Day16;

public class Program
{
    public static void Main()
    {
        var graph = ReadValves().ToDictionary(v => v.Name);

        // var part1 = Search(graph, 30);
        // Console.WriteLine(part1);

        var part2 = Search2(graph, 26);
        Console.WriteLine(part2);
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
            SelfValve = "AA",
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

            Console.WriteLine($"at {current.SelfValve}, timeRemaining: {current.TimeRemaining}, {open.Count()} states left");

            var newBest = current.Score > best.GetValueOrDefault(current.TimeRemaining);
            if (newBest)
                best[current.TimeRemaining] = current.Score;

            var possibleMoves = GetPossibleMoves(current, graph, shortKings).Where(m => !closed.Contains(m));

            foreach (var move in possibleMoves) open.Enqueue(move);
        }

        return best.Values.Max();
    }

    public static int Search2(Dictionary<string, Valve> graph, int timeRemaining)
    {
        var open = new Queue<State>();
        var shortKings = ShortKings(graph);
        open.Enqueue(new State
        {
            SelfValve = "AA",
            ElephantValve = "AA",
            OpenValves = graph.Values.Where(v => v.FlowRate == 0).ToDictionary(v => v.Name, _ => 0),
            TimeRemaining = timeRemaining,
        });
        // var closed = new HashSet<State>();

        var best = open.First();

        while (open.Any())
        {
            var current = open.Dequeue();
            // closed.Add(current);
            if (current.TimeRemaining < 0) continue;

            Console.WriteLine($"{current.TimeRemaining} remaining, {open.Count()} states left, best: {best.Score}");

            if (current.Score > best.Score) best = current;

            var possibleMoves = GetPossibleMoves2(current, graph, shortKings);
            foreach (var move in possibleMoves) open.Enqueue(move);
        }

        var path = new List<State>();
        var c = best;
        path.Add(c);
        while (c.Parent is not null) path.Add(c = c.Parent);
        path.Reverse();

        return best.Score;
    }

    public static State[] GetPossibleMoves(State state, Dictionary<string, Valve> graph, Dictionary<(string, string), int> shortKings)
    {
        if (state.TimeRemaining == 0)
            return new State[0];
        if (state.OpenValves.Count() == graph.Count())
            return new State[0];
        var ret = new List<State>();

        var destinations = graph.Values.Where(valve => valve.Name != state.SelfValve && !state.OpenValves.ContainsKey(valve.Name));
        foreach (var dest in destinations)
        {
            var costToOpen = shortKings[(state.SelfValve, dest.Name)] + 1;
            if (costToOpen > state.TimeRemaining) continue;

            ret.Add(new State
            {
                SelfValve = dest.Name,
                OpenValves = state.OpenValves
                    .Append(new KeyValuePair<string, int>(dest.Name, dest.FlowRate * (state.TimeRemaining - costToOpen)))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                TimeRemaining = state.TimeRemaining - costToOpen,
            });
        }

        return ret.ToArray();
    }

    public static State[] GetPossibleMoves2(State state, Dictionary<string, Valve> graph, Dictionary<(string, string), int> shortKings)
    {
        if (state.TimeRemaining == 0)
            return new State[0];
        if (state.OpenValves.Count() == graph.Count())
            return new State[0];
        var ret = new List<State>();

        var isGolden =
            state.OpenValves.TryGetValue("DD", out int pressure) && pressure == graph["DD"].FlowRate * 24 &&
            state.OpenValves.TryGetValue("JJ", out pressure) && pressure == graph["JJ"].FlowRate * 23 &&
            state.OpenValves.TryGetValue("BB", out pressure) && pressure == graph["BB"].FlowRate * 19 &&
            state.OpenValves.TryGetValue("HH", out pressure) && pressure == graph["HH"].FlowRate * 19 &&
            state.OpenValves.TryGetValue("CC", out pressure) && pressure == graph["CC"].FlowRate * 17;
        
        if (isGolden)
        {
            Console.WriteLine("asdfsafd");
        }

        var destinations = graph.Values.Where(d => !state.OpenValves.ContainsKey(d.Name)).ToArray();
        if (state.SelfOpenRemaining == 0 && state.ElephantOpenRemaining == 0)
        {
            var destPairs = destinations.SelectMany(d => destinations.Select(d2 => (d, d2)));
            foreach (var (self, el) in destPairs)
            {
                if (self.Name == el.Name) continue;
                var selfOpen = shortKings[(state.SelfValve, self.Name)] + 1;
                var elOpen = shortKings[(state.ElephantValve, el.Name)] + 1;
                var costToOpen = Math.Min(selfOpen, elOpen);
                if (costToOpen >= state.TimeRemaining) continue;

                var openValves = state.OpenValves.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                if (selfOpen == costToOpen) openValves[self.Name] = self.FlowRate * (state.TimeRemaining - costToOpen);
                if (elOpen  == costToOpen) openValves[el.Name] = el.FlowRate * (state.TimeRemaining - costToOpen);

                ret.Add(new State
                {
                    SelfValve = self.Name,
                    SelfOpenRemaining = selfOpen - costToOpen,
                    ElephantValve = el.Name,
                    ElephantOpenRemaining = elOpen - costToOpen,
                    OpenValves = openValves,
                    TimeRemaining = state.TimeRemaining - costToOpen,
                });
            }
        }
        else if (state.SelfOpenRemaining > 0)
        {
            var elDests = destinations.Where(d => d.Name != state.SelfValve);
            var statesCreated = 0;
            foreach (var dest in elDests)
            {
                var elOpen = shortKings[(state.ElephantValve, dest.Name)] + 1;
                var costToOpen = Math.Min(state.SelfOpenRemaining, elOpen);
                if (costToOpen >= state.TimeRemaining) continue;

                var openValves = state.OpenValves.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                if (state.SelfOpenRemaining == costToOpen) openValves[state.SelfValve] = graph[state.SelfValve].FlowRate * (state.TimeRemaining - costToOpen);
                if (elOpen == costToOpen) openValves[dest.Name] = dest.FlowRate * (state.TimeRemaining - costToOpen);

                ret.Add(new State
                {
                    SelfValve = state.SelfValve,
                    SelfOpenRemaining = state.SelfOpenRemaining - costToOpen,
                    ElephantValve = dest.Name,
                    ElephantOpenRemaining = elOpen - costToOpen,
                    OpenValves = openValves,
                    TimeRemaining = state.TimeRemaining - costToOpen,
                });
                statesCreated++;
            }
            if (statesCreated == 0)
            {
                ret.Add(new State
                {
                    SelfValve = state.SelfValve,
                    SelfOpenRemaining = 0,
                    ElephantValve = state.ElephantValve,
                    ElephantOpenRemaining = state.ElephantOpenRemaining,
                    OpenValves = state.OpenValves.Append(new KeyValuePair<string, int>(state.SelfValve, graph[state.SelfValve].FlowRate * (state.TimeRemaining - state.SelfOpenRemaining))).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    TimeRemaining = state.TimeRemaining - state.SelfOpenRemaining,
                });
            }
        }
        else if (state.ElephantOpenRemaining > 0)
        {
            var selfDests = destinations.Where(d => d.Name != state.ElephantValve);
            var statesCreated = 0;
            foreach (var dest in selfDests)
            {
                var selfOpen = shortKings[(state.SelfValve, dest.Name)] + 1;
                var costToOpen = Math.Min(state.ElephantOpenRemaining, selfOpen);
                if (costToOpen >= state.TimeRemaining) continue;

                var openValves = state.OpenValves.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                if (state.ElephantOpenRemaining == costToOpen) openValves[state.ElephantValve] = graph[state.ElephantValve].FlowRate * (state.TimeRemaining - costToOpen);
                if (selfOpen == costToOpen) openValves[dest.Name] = dest.FlowRate * (state.TimeRemaining - costToOpen);

                ret.Add(new State
                {
                    ElephantValve = state.ElephantValve,
                    ElephantOpenRemaining = state.ElephantOpenRemaining - costToOpen,
                    SelfValve = dest.Name,
                    SelfOpenRemaining = selfOpen - costToOpen,
                    OpenValves = openValves,
                    TimeRemaining = state.TimeRemaining - costToOpen,
                });
                statesCreated++;
            }
            if (statesCreated == 0)
            {
                ret.Add(new State
                {
                    SelfValve = state.SelfValve,
                    SelfOpenRemaining = state.SelfOpenRemaining,
                    ElephantValve = state.ElephantValve,
                    ElephantOpenRemaining = 0,
                    OpenValves = state.OpenValves.Append(new KeyValuePair<string, int>(state.ElephantValve, graph[state.ElephantValve].FlowRate * (state.TimeRemaining - state.ElephantOpenRemaining))).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    TimeRemaining = state.TimeRemaining - state.ElephantOpenRemaining,
                });
            }
        }
        else if (state.ElephantOpenRemaining > 0 && state.SelfOpenRemaining > 0)
        {
            Console.Write("what");
        }
        else
        {
            Console.WriteLine("what");
        }

        // foreach (var c in ret) c.Parent = state;

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
    public State? Parent { get; set; }

    public string SelfValve { get; set; } = "";

    public int SelfOpenRemaining { get; set; }

    public string ElephantValve { get; set; } = "";

    public int ElephantOpenRemaining { get; set; }

    public Dictionary<string, int> OpenValves { get; set; } = new();
    public int TimeRemaining { get; set; }

    public int Score => OpenValves.Values.Sum();

    public override int GetHashCode()
    {
        var valveStr = string.Join(":", OpenValves.Select(kvp => (kvp.Key, kvp.Value)).OrderBy(x => x));
        var fullstr = string.Join("|", new[] { SelfValve, SelfOpenRemaining.ToString(), ElephantValve, ElephantOpenRemaining.ToString(), valveStr, TimeRemaining.ToString() });
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
        if (left.SelfValve != right.SelfValve) return false;
        if (left.TimeRemaining != right.TimeRemaining) return false;
        if (left.ElephantValve != right.ElephantValve) return false;
        if (left.SelfOpenRemaining != right.SelfOpenRemaining) return false;
        if (left.ElephantOpenRemaining != right.SelfOpenRemaining) return false;

        return left.OpenValves
            .Select(kvp => (kvp.Key, kvp.Value))
            .ToHashSet()
            .SetEquals(
                right.OpenValves.Select(kvp => (kvp.Key, kvp.Value))
            );
    }
}