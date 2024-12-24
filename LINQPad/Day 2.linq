<Query Kind="Program">
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

        var reports = data.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(report => report.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToArray())
            .ToArray();

        //Part 1
        reports
            .Select(level => level.Zip(level.Skip(1)))
            .Select(x => x.Aggregate((SafeSoFar: true, Direction: 0), (state, levels) =>
            {
                if (!state.SafeSoFar) return state;

                var direction = (levels.First, levels.Second) switch
                {
                    (var x, var y) when x > y => 2, //Descending enum flag
                    (var x, var y) when x < y => 1, //Ascending enum flag
                    _ => 0, //Equality or default enum flag
                };

                var delta = levels.First >= levels.Second ? levels.First - levels.Second : levels.Second - levels.First;

                var safeSoFar = state.SafeSoFar
                        && direction is not 0
                        && (state.Direction is 0 || state.Direction == direction)
                        && delta is > 0 and < 4;

                return (SafeSoFar: safeSoFar, Direction: direction);
            }).SafeSoFar)
            .Count(x => x)
            .Dump("Safe reports");

        //Part 2
        reports
            .Select(report => Enumerable
                .Range(0, report.Length)
                .Select(i => report[..i].Concat(report[(i + 1)..])) //create dampned sequences
                .Select(level => level.Zip(level.Skip(1)))
                .Select(x => x.Aggregate((SafeSoFar: true, Direction: 0), (state, levels) =>
                {
                    if (!state.SafeSoFar) return state;

                    var direction = (levels.First, levels.Second) switch
                    {
                        (var x, var y) when x > y => 2, //Descending enum flag
                        (var x, var y) when x < y => 1, //Ascending enum flag
                        _ => 0, //Equality or default enum flag
                    };

                    var delta = levels.First >= levels.Second ? levels.First - levels.Second : levels.Second - levels.First;

                    var safeSoFar = state.SafeSoFar
                            && direction is not 0
                            && (state.Direction is 0 || state.Direction == direction)
                            && delta is > 0 and < 4;

                    return (SafeSoFar: safeSoFar, Direction: direction);
                }).SafeSoFar)
                .Any(x => x))
            .Count(x => x)
            .Dump("Safe reports with dampner");
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
