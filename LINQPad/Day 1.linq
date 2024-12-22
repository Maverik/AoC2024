<Query Kind="Program">
  <Namespace>System.Net.Http</Namespace>
  <AutoDumpHeading>true</AutoDumpHeading>
  <RuntimeVersion>9.0</RuntimeVersion>
</Query>

/*
    SPDX-License-Identifier: CC-BY-NC-SA-4.0
    SPDX-FileCopyrightText: ©️ 2024 Maverik <http://github.com/Maverik>
*/

public static class Program
{
    public static void Main()
    {
        var data = GetInput();

        var parsedLists = data.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Select(x => (Left: int.Parse(x[0]), Right: int.Parse(x[1])))
            .Aggregate((Lefts: Enumerable.Empty<int>(), Rights: Enumerable.Empty<int>()), (a, x) => (Lefts: a.Lefts.Append(x.Left), Rights: a.Rights.Append(x.Right)));

        parsedLists.Lefts.OrderBy(x => x)
            .Zip(parsedLists.Rights.OrderBy(x => x))
            .Select(x => x.First >= x.Second ? x.First - x.Second : x.Second - x.First)
            .Sum()
            .Dump("Distance");

        //Part 2
        parsedLists.Lefts
            .GroupJoin(parsedLists.Rights, x => x, x => x, (left, rights) => left * rights.Count())
            .Sum()
            .Dump("Similarity Score");
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
}