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
}