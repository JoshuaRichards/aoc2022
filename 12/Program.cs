namespace Day12;

public class Program
{
    public static void Main()
    {
        var grid = ReadGrid();
        var part1 = Dijstra(grid, grid.First(kvp => kvp.Value == 'S').Key);
        Console.WriteLine(part1);

        var part2 = ReverseDijstra(grid, grid.First(kvp => kvp.Value == 'E').Key, grid.Where(kvp => kvp.Value == 'S' || kvp.Value == 'a').Select(kvp => kvp.Key).ToArray());
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

    public static IEnumerable<(int x, int y)> GetNeighbours(Dictionary<(int x, int y), char> grid, int x, int y)
    {
        return new (int x, int y)[] { (-1, 0), (1, 0), (0, -1), (0, 1) }
            .Select(dir => (x + dir.x, y + dir.y))
            .Where(point => grid.ContainsKey(point))
            .Where(point =>
            {
                var sourceValue = grid[(x, y)];
                var destValue = grid[point];
                if (sourceValue == 'S') sourceValue = 'a';
                if (sourceValue == 'E') sourceValue = 'z';
                if (destValue == 'S') destValue = 'a';
                if (destValue == 'E') destValue = 'z';

                return destValue <= sourceValue + 1;
            });
    }

    public static IEnumerable<(int x, int y)> GetReverseNeighbours(Dictionary<(int x, int y), char> grid, int x, int y)
    {
        return new (int x, int y)[] { (-1, 0), (1, 0), (0, -1), (0, 1) }
            .Select(dir => (x + dir.x, y + dir.y))
            .Where(point => grid.ContainsKey(point))
            .Where(point =>
            {
                var sourceValue = grid[(x, y)];
                var destValue = grid[point];
                if (sourceValue == 'S') sourceValue = 'a';
                if (sourceValue == 'E') sourceValue = 'z';
                if (destValue == 'S') destValue = 'a';
                if (destValue == 'E') destValue = 'z';

                return sourceValue <= destValue + 1;
            });
    }

    public static int Dijstra(Dictionary<(int x, int y), char> graph, (int x, int y) source)
    {
        Console.WriteLine(source);
        var dist = new Dictionary<(int x, int y), int>();
        var prev = new Dictionary<(int x, int y), (int x, int y)>();
        var unvisited = new HashSet<(int x, int y)>(graph.Keys);
        foreach (var point in graph.Keys)
        {
            dist[point] = int.MaxValue;
        }
        dist[source] = 0;

        var target = graph.First(kvp => kvp.Value == 'E').Key;
        while (unvisited.Any())
        {
            var u = unvisited.MinBy(point => dist[point]);
            unvisited.Remove(u);

            var neighbours = GetNeighbours(graph, u.x, u.y)
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

            if (u == target) break;
        }

        var current = target;
        var count = 0;
        while (current != source)
        {
            count++;
            if (prev.TryGetValue(current, out var next))
            {
                current = next;
            }
            else
            {
                return int.MaxValue;
            }
        }

        return count;
    }

    public static int ReverseDijstra(Dictionary<(int x, int y), char> graph, (int x, int y) source, (int x, int y)[] targets)
    {
        var dist = new Dictionary<(int x, int y), int>();
        var prev = new Dictionary<(int x, int y), (int x, int y)>();
        var unvisited = new HashSet<(int x, int y)>(graph.Keys);
        foreach (var point in graph.Keys)
        {
            dist[point] = int.MaxValue;
        }
        dist[source] = 0;

        while (unvisited.Any())
        {
            var u = unvisited.MinBy(point => dist[point]);
            unvisited.Remove(u);

            var neighbours = GetReverseNeighbours(graph, u.x, u.y)
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

        return targets.Min(target =>
        {
            var current = target;
            var count = 0;
            while (current != source)
            {
                count++;
                if (prev.TryGetValue(current, out var next))
                {
                    current = next;
                }
                else
                {
                    return int.MaxValue;
                }
            }

            return count;
        });

    }
}