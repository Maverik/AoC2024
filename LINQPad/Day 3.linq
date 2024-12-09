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


    string data = await client.GetStringAsync("https://adventofcode.com/2024/day/3/input");

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

static int parsedDotProduct(string input) => parseMulInstruction.Matches(input)
    .Select(x => int.Parse(x.Groups[1].Value) * int.Parse(x.Groups[2].Value))
    .Sum();

static Regex parseMulInstruction = new Regex(@"mul\((\d{1,3}),(\d{1,3})\)", RegexOptions.Compiled);
static Regex parseEnableDisableInstruction = new Regex(@"don't\(\)|do\(\)", RegexOptions.Compiled);