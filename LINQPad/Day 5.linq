<Query Kind="Program">
  <Namespace>System.Buffers</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Numerics</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <AutoDumpHeading>true</AutoDumpHeading>
  <RuntimeVersion>9.0</RuntimeVersion>
</Query>

/*
    SPDX-License-Identifier: CC-BY-NC-SA-4.0
    SPDX-FileCopyrightText: ©️ 2024 Maverik <http://github.com/Maverik>
*/

public static class Program
{
    public static async Task Main()
    {
        //Skipping the oauth flow. Do it in browser and pull the full cookie into your environment variable as below
        var cookieHeaderValue = Environment.GetEnvironmentVariable("AoC2024-FullCookie");

        if (string.IsNullOrWhiteSpace(cookieHeaderValue))
            "Can't continue unless you're logged in through your cookie".Dump("Error");

        using var client = new HttpClient();

        client.DefaultRequestHeaders.Add("Cookie", cookieHeaderValue);

        var data = await Util.CacheAsync(async () => await client.GetStringAsync("https://adventofcode.com/2024/day/5/input"));

        var priorities = new Dictionary<byte, PriorityNumber<byte>>();

        using var reader = new StringReader(data);

        for (var line = reader.ReadLine(); line is { Length: > 1 }; line = reader.ReadLine())
        {
            if (!PriorityNumber<byte>.TryParse(line, null, out var result))
                continue;

            if (priorities.TryGetValue(result.Number, out var item))
                item.Merge(result);
            else
                priorities[result.Number] = result;
        }

        short middleSum = 0, correctedMiddleSum = 0;

        for (var line = reader.ReadLine(); line is { Length: > 1 }; line = reader.ReadLine())
        {
            var sequence = line.Split(',')
                .Select(PriorityNumber<byte>.Parse)
                .Select(x =>
                {
                    if (priorities.TryGetValue(x.Number, out var knownFollowers))
                        x.Followers = knownFollowers.Followers;

                    return x;
                })
                .ToArray();

            var correctSequence = sequence.OrderBy(x => x, PriorityUpdateComparer<byte>.Default).ToArray();

            if (sequence.SequenceEqual(correctSequence))
                middleSum += sequence[sequence.Length >> 1].Number;
            else
                correctedMiddleSum += correctSequence[correctSequence.Length >> 1].Number;
        }

        middleSum.Dump("Part 1");
        correctedMiddleSum.Dump("Part 2");
    }
}

public record struct PriorityNumber<T>() : IParsable<PriorityNumber<T>> where T : struct, INumber<T>
{
    public T Number;

    public SortedSet<T> Followers = [];

    public PriorityNumber<T> Merge(PriorityNumber<T> other)
    {
        if (Number.Equals(other.Number))
            Followers.UnionWith(other.Followers);

        return this;
    }

    #region IParsable<T>

    public static PriorityNumber<T> Parse(string s) => Parse(s, null);

    public static PriorityNumber<T> Parse(string s, IFormatProvider? provider) => TryParse(s, provider, out var result) ? result : throw new InvalidOperationException($"Cannot parse {s} as a valid string");

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out PriorityNumber<T> result)
    {
        if (s is not null) return TryParse(s.AsSpan(), provider, out result);

        result = default;

        return false;
    }

    public static bool TryParse(ReadOnlySpan<char> input, IFormatProvider? provider, out PriorityNumber<T> result)
    {
        result = new();

        var separatorIndex = input.IndexOf('|');

        if (separatorIndex < 1)
            return T.TryParse(input, provider, out result.Number);

        if (separatorIndex != input.LastIndexOf('|') || separatorIndex + 1 == input.Length)
            return false;

        if (!T.TryParse(input[..separatorIndex], provider, out var number) || !T.TryParse(input[(separatorIndex + 1)..], provider, out var before))
            return false;

        result.Number = number;
        result.Followers.Add(before);

        return true;
    }

    #endregion

    public static implicit operator PriorityNumber<T>(T number) => new() { Number = number };

    public static explicit operator T(PriorityNumber<T> number) => number.Number;
}

public struct PriorityUpdateComparer<T> : IComparer<PriorityNumber<T>> where T : struct, INumber<T>
{
    public static readonly PriorityUpdateComparer<T> Default = new();

    public int Compare(PriorityNumber<T> x, PriorityNumber<T> y)
    {
        if (x.Number.Equals(y.Number)) return 0;
        //if we're a number in the Before list of the other number, then we have to come after that number.
        else if (y.Followers.Contains(x.Number)) return 1;
        //if they're a number in our before list, then we must come before them.
        else if (x.Followers.Contains(y.Number)) return -1;

        //there is no priority set at this point, so we're going to return equal for sorting purposes.
        return 0;
    }
}