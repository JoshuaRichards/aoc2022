namespace Day6;

public class Program
{
    public static void Main()
    {
        var input = File.ReadAllText("input.txt");

        Console.WriteLine(FindMarker(input, 4));
        Console.WriteLine(FindMarker(input, 14));
    }

    public static int FindMarker(string input, int seqLength)
    {
        return Enumerable.Range(0, input.Length)
            .First(i => input[i..(i + seqLength)].Distinct().Count() == seqLength)
            + seqLength;
    }
}