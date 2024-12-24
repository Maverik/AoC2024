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

        //Part 1
        parsedDotProduct(data).Dump("Multiplications Summed");

        //Part 2
        var matches = parseEnableDisableInstruction.Matches(data).Select(x => (x.Index, x.Length)).ToArray();

        var instructionEnabled = true;
        var start = 0;
        var dotProduct = 0;

        foreach (var match in matches)
        {
            if (instructionEnabled && match.Length is 7) //don't() - length 7
            {
                instructionEnabled = false;
                var instructionRun = data.Substring(start, match.Index - start);
                dotProduct += parsedDotProduct(instructionRun);
            }
            else if (!instructionEnabled && match.Length is 4) //do
            {
                instructionEnabled = true;
                start = match.Index + 4;
            }
        }

        //if we were enabled as last match, we need to process the remaining substring
        if (instructionEnabled && start < data.Length)
            dotProduct += parsedDotProduct(data.Substring(start, data.Length - start));

        dotProduct.Dump("Multiplications Summed per the instructions");
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

    static int parsedDotProduct(string input) => parseMulInstruction.Matches(input)
        .Select(x => int.Parse(x.Groups[1].Value) * int.Parse(x.Groups[2].Value))
        .Sum();

    static Regex parseMulInstruction = new Regex(@"mul\((\d{1,3}),(\d{1,3})\)", RegexOptions.Compiled);
    static Regex parseEnableDisableInstruction = new Regex(@"don't\(\)|do\(\)", RegexOptions.Compiled);
}