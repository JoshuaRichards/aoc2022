namespace Day12;

public class Program
{
    public static void Main()
    {
        var grid = ReadGrid();
        var part1 = Dijstra(
            grid,
            grid.First(kvp => kvp.Value == 'S').Key,
            grid.Where(kvp => kvp.Value == 'E').Select(kvp => kvp.Key).ToArray(),
            (source, dest) => dest <= source + 1
        );
        Console.WriteLine(part1);

        var part2 = Dijstra(
            grid, grid.First(kvp => kvp.Value == 'E').Key,
            grid.Where(kvp => kvp.Value == 'S' || kvp.Value == 'a').Select(kvp => kvp.Key).ToArray(),
            (source, dest) => source <= dest + 1
        );
        Console.WriteLine(part2);
    }

    public static Dictionary<(int x, int y), char> ReadGrid()
    {
        var y = 0;
        var ret = new Dictionary<(int, int), char>();

        foreach (var line in File.ReadLines("input.txt"))
        {
            for (int x = 0; x < line.Length; x++)
            {
                ret[(x, y)] = line[x];
            }
            y++;
        }

        return ret;
    }

    public static IEnumerable<(int x, int y)> GetNeighbours(Dictionary<(int x, int y), char> grid, (int x, int y) source, Func<char, char, bool> isEdge)
    {
        return new (int x, int y)[] { (-1, 0), (1, 0), (0, -1), (0, 1) }
            .Select(dir => (source.x + dir.x, source.y + dir.y))
            .Where(point => grid.ContainsKey(point))
            .Where(point =>
            {
                var sourceValue = grid[source];
                var destValue = grid[point];
                if (sourceValue == 'S') sourceValue = 'a';
                if (sourceValue == 'E') sourceValue = 'z';
                if (destValue == 'S') destValue = 'a';
                if (destValue == 'E') destValue = 'z';

                return isEdge(sourceValue, destValue);
            });
    }

    public static double Dijstra(Dictionary<(int x, int y), char> graph, (int x, int y) source, (int x, int y)[] targets, Func<char, char, bool> isEdge)
    {
        var dist = new Dictionary<(int x, int y), double>();
        var prev = new Dictionary<(int x, int y), (int x, int y)>();
        var unvisited = new HashSet<(int x, int y)>(graph.Keys);
        foreach (var point in graph.Keys)
        {
            dist[point] = double.PositiveInfinity;
        }
        dist[source] = 0;

        while (unvisited.Any())
        {
            var u = unvisited.MinBy(point => dist[point]);
            unvisited.Remove(u);

            var neighbours = GetNeighbours(graph, u, isEdge)
                .Where(n => unvisited.Contains(n));
            foreach (var neighbour in neighbours)
            {
                var alt = dist[u] + 1;
                if (alt < dist[neighbour])
                {
                    dist[neighbour] = alt;
                    prev[neighbour] = u;
                }
            }
        }

        return targets.Min(target => dist[target]);
    }
}