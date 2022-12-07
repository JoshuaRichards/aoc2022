using System.Text.RegularExpressions;

namespace Day7;

public class Program
{
    public static void Main()
    {
        var root = BuildTree("input.txt");
        var part1 = root.RecurseDirs()
            .Select(d => d.GetSize())
            .Where(s => s <= 100_000)
            .Sum();
        Console.WriteLine(part1);

        var unusedSpace = 70000000 - root.GetSize();
        var target = 30_000_000 - unusedSpace;
        var part2 = root.RecurseDirs()
            .Select(d => d.GetSize())
            .Where(s => s >= target)
            .Min();
        Console.WriteLine(part2);
        // foreach (var line in root.PrintDir())
        // {
        //     Console.WriteLine(line);
        // }
    }

    public static Directory BuildTree(string input)
    {
        var rootDir = new Directory();
        const string ls = @"^\$ ls";
        const string cd = @"^\$ cd (.+)";
        const string dir = @"dir (.+)";
        const string file = @"(\d+) (.+)";

        var currentDirectory = rootDir;
        foreach (var line in File.ReadLines(input))
        {
            if (Regex.IsMatch(line, ls)) continue;
            if (Regex.IsMatch(line, dir)) continue;

            Match match;
            if ((match = Regex.Match(line, cd)).Success)
            {
                var dirName = match.Groups[1].Value;
                if (dirName == "/")
                {
                    currentDirectory = rootDir;
                    continue;
                }
                if (dirName == "..")
                {
                    currentDirectory = currentDirectory.Parent;
                    continue;
                }

                var dest = currentDirectory.ChildDirectories.FirstOrDefault(d => d.Name == dirName);
                if (dest == null)
                {
                    dest = new Directory
                    {
                        Name = dirName,
                        Parent = currentDirectory,
                    };
                    currentDirectory.ChildDirectories.Add(dest);
                }
                currentDirectory = dest;
                continue;
            }

            if ((match = Regex.Match(line, file)).Success)
            {
                var size = int.Parse(match.Groups[1].Value);
                var name = match.Groups[2].Value;

                if (currentDirectory.Files.Any(f => f.Name == name)) continue;
                var entry = new FileEntry
                {
                    Size = size,
                    Name = name,
                };
                currentDirectory.Files.Add(entry);
                continue;
            }
        }

        return rootDir;
    }

}

public class Directory
{
    private int? size = null;
    public string Name { get; set;} = "";
    public Directory Parent { get; set; } = null!;
    public List<Directory> ChildDirectories { get; } = new();

    public List<FileEntry> Files { get; } = new();

    public int GetSize()
    {
        if (size != null) return size.Value;

        var ret = Files.Sum(f => f.Size);
        ret += ChildDirectories.Sum(d => d.GetSize());

        size = ret;
        return ret;
    }

    public IEnumerable<Directory> RecurseDirs()
    {
        yield return this;
        foreach (var dir in ChildDirectories)
        {
            foreach (var child in dir.RecurseDirs())
            {
                yield return child;
            }
        }
    }

    public IEnumerable<string> PrintDir()
    {
        yield return $"d {Name} - {GetSize()}";
        foreach (var dir in ChildDirectories)
        {
            foreach (var line in dir.PrintDir())
            {
                yield return $"  {line}";
            }
        }
        foreach (var file in Files)
        {
            yield return $"  f {file.Name} - {file.Size}";
        }
    }
}

public class FileEntry
{
    public int Size { get; set; }
    public string Name { get; set; } = "";
}