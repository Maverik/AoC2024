<Query Kind="Program">
  <Namespace>System.Buffers</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <AutoDumpHeading>true</AutoDumpHeading>
  <RuntimeVersion>9.0</RuntimeVersion>
</Query>

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

        var data = await Util.CacheAsync(async () => await client.GetStringAsync("https://adventofcode.com/2024/day/6/input"));

        (byte X, byte Y) guardPosition = (0, 0);

        using var reader = new StringReader(data);
        var row = reader.ReadLine();

        Debug.Assert(row is not null);

        //we're going to add an edge on each side of the map so +2 & +2
        var stride = (byte)(row.Length + 2);

        var map = new MapMarker[stride, stride];

        for (var i = 0; i < stride; i++)
        {
            //top edge
            map[0, i] = MapMarker.Edge;
            //bottom edge
            map[stride - 1, i] = MapMarker.Edge;

            //left edge
            map[i, 0] = MapMarker.Edge;
            //right edge
            map[i, stride - 1] = MapMarker.Edge;
        }

        for (byte y = 1; row is { Length: > 0 }; row = reader.ReadLine(), y++)
        {
            for (byte x = 0; x < row.Length; x++)
            {
                switch (row[x])
                {
                    case obstacleMarker:
                        map[y, x + 1] = MapMarker.Obstacle;
                        break;
                    case guardMarker:
                        guardPosition = ((byte)(x + 1), y);
                        map[y, x + 1] = MapMarker.Guard | MapMarker.GuardMovingTop;
                        break;
                }
            }

        }

        Thread.Sleep(10000);

        await Part1(map, guardPosition);

    }

    static async Task Part1(MapMarker[,] map, (byte X, byte Y) guardPosition)
    {
        // lets get a bit fancy with our rendering!
        var mapContainer = new DumpContainer(map).Dump();
        
        //this 1 accounts for edge node we'll run into at the end (it needs traversal but we stop 1 short)
        short traversalCount = 1;
        var traversalCountContainer = new DumpContainer(traversalCount).Dump("Traversal Count");
        
        MapMarker marker;

        do
        {
            var lastGuardPosition = guardPosition;

            marker = map[guardPosition.Y, guardPosition.X];
            var direction = (MapMarker)((byte)marker & 0xF0);

            switch (direction)
            {
                case MapMarker.GuardMovingTop when (MapMarker)((byte)map[guardPosition.Y - 1, guardPosition.X] & 0x0F) is MapMarker.Free or MapMarker.Edge or MapMarker.Traversed:
                    guardPosition.Y = (byte)(guardPosition.Y - 1);

                    break;
                case MapMarker.GuardMovingRight when (MapMarker)((byte)map[guardPosition.Y, guardPosition.X + 1] & 0x0F) is MapMarker.Free or MapMarker.Edge or MapMarker.Traversed:
                    guardPosition.X = (byte)(guardPosition.X + 1);

                    break;
                case MapMarker.GuardMovingBottom when (MapMarker)((byte)map[guardPosition.Y + 1, guardPosition.X] & 0x0F) is MapMarker.Free or MapMarker.Edge or MapMarker.Traversed:
                    guardPosition.Y = (byte)(guardPosition.Y + 1);

                    break;
                case MapMarker.GuardMovingLeft when (MapMarker)((byte)map[guardPosition.Y, guardPosition.X - 1] & 0x0F) is MapMarker.Free or MapMarker.Edge or MapMarker.Traversed:
                    guardPosition.X = (byte)(guardPosition.X - 1);

                    break;

                case MapMarker.GuardMovingTop:
                case MapMarker.GuardMovingRight:
                case MapMarker.GuardMovingBottom:
                case MapMarker.GuardMovingLeft:
                    map[guardPosition.Y, guardPosition.X] = MapMarker.Guard | direction switch
                    {
                        MapMarker.GuardMovingTop => MapMarker.GuardMovingRight,
                        MapMarker.GuardMovingRight => MapMarker.GuardMovingBottom,
                        MapMarker.GuardMovingBottom => MapMarker.GuardMovingLeft,
                        MapMarker.GuardMovingLeft => MapMarker.GuardMovingTop,
                        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, $"Unexpected guard direction {direction}")
                    };
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, $"Unexpected guard direction {direction}");
            }

            if (guardPosition != lastGuardPosition && map[guardPosition.Y, guardPosition.X] != MapMarker.Edge)
            {                
                if(!map[guardPosition.Y, guardPosition.X].HasFlag(MapMarker.Traversed))
                    traversalCount++;

                map[lastGuardPosition.Y, lastGuardPosition.X] = MapMarker.Traversed | direction;
                map[guardPosition.Y, guardPosition.X] = MapMarker.Guard | direction;
            }

            //If you want to watch it all animate - uncomment the following and grab your coffee!
            //await Task.Delay(48).ConfigureAwait(false);
            //mapContainer.UpdateContent(map);
            
            //traversal would be one higher at the end because of the edge node.
            traversalCountContainer.UpdateContent(traversalCount);
            
        } while (map[guardPosition.Y, guardPosition.X] != MapMarker.Edge);
        
        mapContainer.UpdateContent(map);
        traversalCountContainer.UpdateContent(traversalCount);
    }

    const char obstacleMarker = '#';
    const char guardMarker = '^';
}


static object ToDump(object input)
{
    Util.HtmlHead.AddStyles(
        """
         table { background-color: #111; }
         td {
            text-align: center;
            vertical-align: middle;
            border: 1px solid rgba(192,255,255,0.09);
         }
         td:nth-child(even), tr:nth-child(even) {
            background-color: #111720;
         }
         """);

    return input switch
    {
        MapMarker marker => (marker & (MapMarker)0x0F) switch
        {
            MapMarker.Free => string.Empty,
            MapMarker.Edge => Util.WithStyle("❌", "font-size: 2ch; color: #FFFFFF80"),
            MapMarker.Obstacle => Util.WithStyle("🚩", "font-size: 2ch; color: #FFFFFFA0"),
            MapMarker.Traversed => (marker & (MapMarker)0xF0) switch
            {
                MapMarker.GuardMovingTop => Util.WithStyle("^", "color: #016064"),
                MapMarker.GuardMovingRight => Util.WithStyle(">️", "color: #63c5da"),
                MapMarker.GuardMovingBottom => Util.WithStyle("V", "color: #1f456E"),
                MapMarker.GuardMovingLeft => Util.WithStyle("<️", "color: #0492c2"),
                var x => x
            },
            MapMarker.Guard => Util.WithStyle("👮", "font-size: 2ch;"),
            var x => x
        },
        var x => x
    };
}

//Packed structure - lower 4 bits for markers, upper 4 bits for direction a guard is facing
enum MapMarker : byte
{
    Free = 0,
    Obstacle = 1,
    Guard = 2,
    Traversed = 3,
    Edge = 4,

    GuardMovingTop = 0 << 4,
    GuardMovingRight = 1 << 4,
    GuardMovingBottom = 2 << 4,
    GuardMovingLeft = 3 << 4,
}