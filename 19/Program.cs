using System.Text.RegularExpressions;

namespace Day19;

public class Program
{
    public static void Main()
    {
        var blueprints = GetBlueprints().ToDictionary(b => b.Id);
        // var part1 = 0;
        // foreach (var blueprintId in blueprints.Keys)
        // {
        //     var state = new State
        //     {
        //         BlueprintId = blueprintId,
        //         TimeRemaining = 24,
        //         OreBots = 1,
        //     };
        //     var max = MaxGeodes(blueprints, state);
        //     Console.WriteLine($"max geodes for blueprint {blueprintId} is {max}");
        //     part1 += max * blueprintId;
        //     maxGeodeLookup.Clear();
        // }
        var part2 = 1;
        foreach (var blueprintId in new[] { 1, 2, 3})
        {
            var state = new State
            {
                BlueprintId = blueprintId,
                TimeRemaining = 32,
                OreBots = 1,
            };
            var max = MaxGeodes(blueprints, state);
            Console.WriteLine($"max geodes for blueprint {blueprintId} is {max}");
            part2 *= max;
            maxGeodeLookup.Clear();
        }

        Console.WriteLine(part2);
    }

    private static readonly Dictionary<State, int> maxGeodeLookup = new();

    public static int MaxGeodes(Dictionary<int, Blueprint> blueprints, State state)
    {
        if (maxGeodeLookup.TryGetValue(state, out int ret))
        {
            // Console.WriteLine($"cache hit {state}");
            return ret;
        }
        // Console.WriteLine($"calculating max geodes for {state}");

        if (state.TimeRemaining == 0) return maxGeodeLookup[state] = state.Geodes;
        // foreach (var next in GetNextStates(blueprints, state))
        // {
        //     Console.WriteLine($"possible next state: {next}");
        // }
        return maxGeodeLookup[state] = GetNextStates(blueprints, state).Max(next => MaxGeodes(blueprints, next));
    }

    public static IEnumerable<State> GetNextStates(Dictionary<int, Blueprint> blueprints, State state)
    {
        var blueprint = blueprints[state.BlueprintId];
        if (state.BotBuildInProgress is not null)
        {
            state = state.BotBuildInProgress switch
            {
                BotType.Ore => state with { OreBots = state.OreBots + 1 },
                BotType.Clay => state with { ClayBots = state.ClayBots + 1 },
                BotType.Obsidian => state with { ObsidianBots = state.ObsidianBots + 1 },
                BotType.Geode => state with { GeodeBots = state.GeodeBots + 1 },
                _ => throw new NotImplementedException(),
            };
        }

        var nextState = state with
        {
            Ore = state.Ore + state.OreBots,
            Clay = state.Clay + state.ClayBots,
            Obsidian = state.Obsidian + state.ObsidianBots,
            Geodes = state.Geodes + state.GeodeBots,
            BotBuildInProgress = null,
            TimeRemaining = state.TimeRemaining - 1,
        };

        if (state.Ore >= blueprint.GeodeCost.ore && state.Obsidian >= blueprint.GeodeCost.obsidian)
        {
            yield return nextState with
            {
                Ore = nextState.Ore - blueprint.GeodeCost.ore,
                Obsidian = nextState.Obsidian - blueprint.GeodeCost.obsidian,
                BotBuildInProgress = BotType.Geode,
            };
            yield break;
        }

        if (state.Ore >= blueprint.ObsidianCost.ore && state.Clay >= blueprint.ObsidianCost.clay)
        {
            yield return nextState with
            {
                Ore = nextState.Ore - blueprint.ObsidianCost.ore,
                Clay = nextState.Clay - blueprint.ObsidianCost.clay,
                BotBuildInProgress = BotType.Obsidian,
            };
            yield break;
        }

        if (state.Ore >= blueprint.ClayCost)
        {
            yield return nextState with { Ore = nextState.Ore - blueprint.ClayCost, BotBuildInProgress = BotType.Clay };
        }

        if (state.Ore >= blueprint.OreCost)
        {
            yield return nextState with { Ore = nextState.Ore - blueprint.OreCost, BotBuildInProgress = BotType.Ore };
        }

        yield return nextState;
    }


    public static IEnumerable<Blueprint> GetBlueprints()
    {
        return File.ReadLines("input.txt")
            .Select(line => Regex.Match(line, @"Blueprint (\d+).*(\d+) ore.*(\d+) ore.*(\d+) ore and (\d+) clay.*(\d+) ore and (\d+) obsidian"))
            .Select(match => match.Groups.Cast<Group>().Skip(1).Select(g => g.Value).Select(int.Parse).ToArray())
            .Select(numbers => new Blueprint
            {
                Id = numbers[0],
                OreCost = numbers[1],
                ClayCost = numbers[2],
                ObsidianCost = (numbers[3], numbers[4]),
                GeodeCost = (numbers[5], numbers[6]),
            });
    }
}

public record struct Blueprint
{
    public int Id { get; set; }
    public int OreCost { get; set; }
    public int ClayCost { get; set; }
    public (int ore, int clay) ObsidianCost { get; set; }
    public (int ore, int obsidian) GeodeCost { get; set; }
}

public record class State
{
    public int BlueprintId { get; init; }
    public int Ore { get; init; }
    public int Clay { get; init; }
    public int Obsidian { get; init; }
    public int Geodes { get; init; }
    public int OreBots { get; init; }
    public int ClayBots { get; init; }
    public int ObsidianBots { get; init; }
    public int GeodeBots { get; init; }
    public int TimeRemaining { get; init; }
    public BotType? BotBuildInProgress { get; init; }
}

public enum BotType
{
    Ore,
    Clay,
    Obsidian,
    Geode,
}