namespace Day20;

public class Program
{
    public static void Main()
    {
        Part1();
        Part2();
    }

    public static void Part1()
    {
        var originalPositions = ReadInput().ToArray();
        var workArea = Enumerable.Range(0, originalPositions.Length).ToList();

        // Console.WriteLine(string.Join(", ", workArea.Select(j => originalPositions[j])));
        for (int i = 0; i < originalPositions.Length; i++)
        {
            var faceValue = originalPositions[i];
            var currentPos = workArea.IndexOf(i);

            workArea.RemoveAt(currentPos);
            var nextPos = NonShitModulo(currentPos + faceValue, workArea.Count);

            workArea.Insert(nextPos, i);
            // Console.WriteLine(string.Join(", ", workArea.Select(j => originalPositions[j])));
        }

        var newPositions = workArea.Select(j => originalPositions[j]).ToArray();
        var zeroPos = Array.IndexOf(newPositions, 0);
        var part1 = new[] {1000, 2000, 3000}.Select(x => newPositions[(zeroPos+x) % newPositions.Length]).Sum();
        Console.WriteLine(part1);
    }

    public static void Part2()
    {
        var originalPositions = ReadInput().Select(x => Convert.ToInt64(x)).Select(x => x * 811589153).ToArray();
        var workArea = Enumerable.Range(0, originalPositions.Length).ToList();

        // Console.WriteLine(string.Join(", ", workArea.Select(j => originalPositions[j])));
        for (int h = 0; h < 10; h++)
            for (int i = 0; i < originalPositions.Length; i++)
            {
                var faceValue = originalPositions[i];
                var currentPos = workArea.IndexOf(i);

                workArea.RemoveAt(currentPos);
                var nextPos = NonShitModulo(currentPos + faceValue, workArea.Count);

                workArea.Insert(nextPos, i);
                // Console.WriteLine(string.Join(", ", workArea.Select(j => originalPositions[j])));
            }

        var newPositions = workArea.Select(j => originalPositions[j]).ToArray();
        var zeroPos = Array.IndexOf(newPositions, 0);
        var part1 = new[] {1000, 2000, 3000}.Select(x => newPositions[(zeroPos+x) % newPositions.Length]).Sum();
        Console.WriteLine(part1);
    }

    public static IEnumerable<int> ReadInput()
    {
        return File.ReadLines("input.txt").Select(int.Parse);
    }

    public static int NonShitModulo(long a, int b)
    {
        return (int)((Math.Abs(a * b) + a) % b);
    }
}
