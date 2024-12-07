<Query Kind="Statements">
  <Namespace>System.Net.Http</Namespace>
  <AutoDumpHeading>true</AutoDumpHeading>
  <RuntimeVersion>9.0</RuntimeVersion>
</Query>

//Skipping the oauth flow. Do it in browser and pull the full cookie into your environment variable as below
var cookieHeaderValue = Environment.GetEnvironmentVariable("AoC2004-FullCookie");

if(string.IsNullOrWhiteSpace(cookieHeaderValue))
	"Can't continue unless you're logged in through your cookie".Dump("Error");

using var client = new HttpClient();

client.DefaultRequestHeaders.Add("Cookie", cookieHeaderValue);

var data = await client.GetStringAsync("https://adventofcode.com/2024/day/1/input");

var parsedLists = data.Split('\n', StringSplitOptions.RemoveEmptyEntries)
    .Select(x => x.Split(' ', StringSplitOptions.RemoveEmptyEntries))
    .Select(x => (Left: int.Parse(x[0]), Right: int.Parse(x[1])))
    .Aggregate((Lefts: Array.Empty<int>().AsEnumerable(), Rights: Array.Empty<int>().AsEnumerable()), (a, x) => (Lefts: a.Lefts.Append(x.Left), Rights: a.Rights.Append(x.Right)));
    
var distance = parsedLists.Lefts.OrderBy(x => x)
    .Zip(parsedLists.Rights.OrderBy(x => x))
    .Select(x => x.First >= x.Second ? x.First - x.Second : x.Second - x.First)
    .Sum();

distance.Dump("Distance");