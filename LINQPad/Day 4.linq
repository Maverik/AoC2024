<Query Kind="Program">
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Buffers</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <AutoDumpHeading>true</AutoDumpHeading>
  <RuntimeVersion>9.0</RuntimeVersion>
</Query>

async Task Main()
{
    //Skipping the oauth flow. Do it in browser and pull the full cookie into your environment variable as below
    var cookieHeaderValue = Environment.GetEnvironmentVariable("AoC2004-FullCookie");

    if (string.IsNullOrWhiteSpace(cookieHeaderValue))
        "Can't continue unless you're logged in through your cookie".Dump("Error");

    using var client = new HttpClient();

    client.DefaultRequestHeaders.Add("Cookie", cookieHeaderValue);

    string data = await client.GetStringAsync("https://adventofcode.com/2024/day/4/input");
    
    const string lookupWord = "XMAS";

    var input = data.Replace("\r\n", "\n").Append('\n').ToArray();
    var strideLength = Array.IndexOf(input, '\n') + 1;

    var traceCount = 0;
    //var trace = new List<(Direction Direction, int Index, string Substring)>();

    for (var i = 0; i < input.Length; i++)
    {
        var currentChar = input[i];

        //We're assuming we match on character case
        //If it's not the starting character of what we're looking for keep moving
        if (currentChar != lookupWord[0]) continue;

        //alias the index for lambda capture
        var pivotIndex = i;

        foreach (var direction in Enum.GetValues<Direction>())
        {
            var indices = DirectionalIndicesFromPivot(direction, pivotIndex, lookupWord.Length, strideLength);

            if (indices.ContainsAnyExceptInRange(0, input.Length - 1)) continue;

            var indexedLookup = GetSubstringFromIndices(input, indices);

            if (indexedLookup.Equals(lookupWord, StringComparison.Ordinal))
            {
                traceCount++;
                //trace.Add((direction, i, indexedLookup.ToString()));
            }
        }
    }

    //trace.Dump($"{lookupWord}");
    traceCount.Dump($"{lookupWord} Count");
}

static ReadOnlySpan<char> GetSubstringFromIndices(ReadOnlySpan<char> input, ReadOnlySpan<int> indices)
{
    Span<char> buffer = new char[indices.Length];

    for (var i = 0; i < indices.Length; i++)
        buffer[i] = input[indices[i]];

    return buffer;
}

static ReadOnlySpan<int> DirectionalIndicesFromPivot(in Direction direction, int index, int armLength, int strideLength)
{
    var indexSequence = Enumerable.Range(0, armLength);

    return direction switch
    {
        Direction.ToRight => indexSequence.Select(x => x + index).ToArray(),
        Direction.ToLeft => indexSequence.Select(x => index - armLength + x + 1).Reverse().ToArray(),
        Direction.ToTop => indexSequence.Select(x => index - strideLength * x).ToArray(),
        Direction.ToBottom => indexSequence.Select(x => index + strideLength * x).ToArray(),
        Direction.ToTopRight => indexSequence.Select(x => index - strideLength * x + x).ToArray(),
        Direction.ToBottomRight => indexSequence.Select(x => index + strideLength * x + x).ToArray(),
        Direction.ToTopLeft => indexSequence.Select(x => index - strideLength * x - x).ToArray(),
        Direction.ToBottomLeft => indexSequence.Select(x => index + strideLength * x - x).ToArray(),
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
    };
}

enum Direction : byte
{
    ToRight,
    ToLeft,
    ToTop,
    ToBottom,
    ToTopRight,
    ToBottomRight,
    ToTopLeft,
    ToBottomLeft
}