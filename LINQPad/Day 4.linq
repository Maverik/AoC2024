<Query Kind="Program">
  <Namespace>System.Buffers</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <AutoDumpHeading>true</AutoDumpHeading>
  <RuntimeVersion>9.0</RuntimeVersion>
</Query>

/*
    SPDX-License-Identifier: CC-BY-NC-SA-4.0
    SPDX-FileCopyrightText: ©️ 2024 Maverik <http://github.com/Maverik>
*/

#load ".\AoC2024"

public static class Program
{
    public static void Main()
    {
        var data = AoC2024.GetInput();

        var input = data.Replace("\r\n", "\n").Append('\n').ToArray();
        var strideLength = Array.IndexOf(input, '\n') + 1;

        Part1(input, strideLength);
        Part2(input, strideLength);
    }


    static void Part1(ReadOnlySpan<char> input, in int strideLength)
    {
        ReadOnlySpan<char> lookupWord = "XMAS";
        var traceCount = 0;
        //var trace = new List<(Direction Direction, int Index, string Substring)>();

        for (var i = 0; i < input.Length; i++)
        {
            var currentChar = input[i];

            //We're assuming we match on character case
            //If it's not the pivoting character of what we're looking for keep moving
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

    static void Part2(ReadOnlySpan<char> input, in int strideLength)
    {
        ReadOnlySpan<char> lookupWord = "MAS";

        var matches = new List<(Direction Direction, int Index, string Substring)>();

        for (var i = 0; i < input.Length; i++)
        {
            var currentChar = input[i];

            //We're assuming we match on character case
            //If it's not the pivoting character of what we're looking for keep moving
            if (currentChar != lookupWord[1]) continue;

            //alias the index for lambda capture
            var pivotIndex = i;

            foreach (var direction in new[] { Direction.ToTopRight, Direction.ToBottomRight, Direction.ToBottomLeft, Direction.ToTopLeft })
            {
                //the wordlength is 2 as we're pivoting from midpoint so AS or AM
                var indices = DirectionalIndicesFromPivot(direction, pivotIndex, 2, strideLength);

                if (indices.ContainsAnyExceptInRange(0, input.Length - 1)) continue;

                var indexedLookup = GetSubstringFromIndices(input, indices);

                //Are we looking at AM or AS? if so, record it.
                if (indexedLookup[1].Equals(lookupWord[0]) || indexedLookup[1].Equals(lookupWord[2]))
                    matches.Add((direction, i, indexedLookup.ToString()));
            }
        }

        matches
            //The magic happens - Find all the pairs that share the pivot in diagonal opposites
            .Join(matches, x => (x.Index, x.Direction), x => (x.Index, FlipDiagonal(x.Direction)), (x, y) => (x.Index, Match: y.Substring[1] + x.Substring))
            .GroupBy(x => x.Index)
            //to form an X we need exactly 2 legs ending in AM & 2 in AS with their counters flipped
            //Since we only look for 4 diagnoal directions, it'll never exceed 4
            //and less means we don't have enoguh legs
            .Count(x => x.Count(xx => xx.Match == "MAS") == 2 && x.Count(xx => xx.Match == "SAM") == 2)
            .Dump("X-MAS Count");
    }

    static Direction FlipDiagonal(Direction direction) => direction switch
    {
        Direction.ToBottomLeft => Direction.ToTopRight,
        Direction.ToTopRight => Direction.ToBottomLeft,
        Direction.ToBottomRight => Direction.ToTopLeft,
        Direction.ToTopLeft => Direction.ToBottomRight,

        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
    };

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

    public static string GetInput()
    {
        var challengeDay = Util.CurrentQuery.Name;
        var dayNumber = challengeDay[^1];

        //Skipping the oauth flow. Do it in browser and pull the full cookie into your environment variable as below
        var cookieHeaderValue = Environment.GetEnvironmentVariable("AoC2024-FullCookie");

        if (string.IsNullOrWhiteSpace(cookieHeaderValue))
            "Can't continue unless you're logged in through your cookie".Dump("Error");

        using var client = new HttpClient();

        client.DefaultRequestHeaders.Add("Cookie", cookieHeaderValue);

        return Util.Cache(() => client.GetStringAsync("https://adventofcode.com/2024/day/" + dayNumber + "/input").GetAwaiter().GetResult(), challengeDay, TimeSpan.FromDays(1));
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
}