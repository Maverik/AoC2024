<Query Kind="Program">
  <NuGetReference Prerelease="true">AngleSharp</NuGetReference>
  <Namespace>AngleSharp</Namespace>
  <Namespace>AngleSharp.Dom</Namespace>
  <Namespace>AngleSharp.Html</Namespace>
  <Namespace>AngleSharp.Html.Dom</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>AngleSharp.Text</Namespace>
  <Namespace>AngleSharp.Io</Namespace>
  <RemoveNamespace>System.Collections</RemoveNamespace>
  <RemoveNamespace>System.Data</RemoveNamespace>
  <RemoveNamespace>System.Linq.Expressions</RemoveNamespace>
  <RemoveNamespace>System.Reflection</RemoveNamespace>
  <RemoveNamespace>System.Text</RemoveNamespace>
  <RemoveNamespace>System.Text.RegularExpressions</RemoveNamespace>
  <RemoveNamespace>System.Transactions</RemoveNamespace>
  <RemoveNamespace>System.Xml</RemoveNamespace>
  <RemoveNamespace>System.Xml.XPath</RemoveNamespace>
  <AutoDumpHeading>true</AutoDumpHeading>
</Query>

/*
    SPDX-License-Identifier: CC-BY-NC-SA-4.0
    SPDX-FileCopyrightText: ©️ 2024 Maverik <http://github.com/Maverik>
*/

//This is a helper class and not meant to be executed directly
//Its #load'ed into actual query
public static class AoC2024
{
    public static string? GetInput(bool getDemoData = false)
    {
        var challengeDay = Util.CurrentQuery.Name;
        var dayNumber = challengeDay[^2..].TrimStart();

        if (!int.TryParse(dayNumber, out var _))
        {
            string.Concat("Expected a number for the challenge day (from query filename) but got: " + dayNumber).Dump("Error");
            return default;
        }

        //Skipping the oauth flow. Do it in browser and pull the full cookie into your environment variable as below
        var cookieHeaderValue = Environment.GetEnvironmentVariable("AoC2024-FullCookie");

        if (string.IsNullOrWhiteSpace(cookieHeaderValue))
        {
            "Can't continue unless you're logged in through your cookie".Dump("Error");
            return default;
        }

        var urlString = string.Concat("https://adventofcode.com/2024/day/", dayNumber, getDemoData ? null : "/input");
        var url = new Url(urlString);

        using var context = new BrowsingContext(Configuration.Default.WithDefaultCookies().WithDefaultLoader());

        foreach (var cookieSegment in cookieHeaderValue.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            context.SetCookie(url, cookieSegment);

        //Lets be kind to AoC server - the input isn't going to change. Cache it for a day at least
        var input = Util.Cache(() =>
        {
            using var response = context.OpenAsync(url).GetAwaiter().GetResult();

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                string.Concat("Request for challenge did not succeed. Got " + response.StatusCode.ToString() + " instead").Dump("Error");
                return default;
            }

            return getDemoData
                ? response.QuerySelector("article.day-desc > pre > code")?.InnerHtml
                : response.Body?.TextContent;
            
        }, urlString, TimeSpan.FromDays(1));
        
        if(input is null)
        {
            Util.Cache(() => input, urlString, TimeSpan.FromTicks(0));
        }
        
        return input;
    }
}