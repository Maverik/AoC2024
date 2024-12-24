<Query Kind="Program">
  <Namespace>System.Buffers</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <RuntimeVersion>10.0.100-alpha.1.24622.1</RuntimeVersion>
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

        Debug.Assert(data is not null);

        (int X, int Y) guardPosition = (0, 0);

        using var reader = new StringReader(data);
        var row = reader.ReadLine();

        Debug.Assert(row is not null);

        //we're going to add an edge on each side of the map so +2 & +2
        var stride = (row.Length + 2);

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

        for (int y = 1; row is { Length: > 0 }; row = reader.ReadLine(), y++)
        {
            for (int x = 0; x < row.Length; x++)
            {
                switch (row[x])
                {
                    case obstacleMarker:
                        map[y, x + 1] = MapMarker.Obstacle;
                        break;
                    case guardMarker:
                        guardPosition = (x + 1, y);
                        map[y, x + 1] = MapMarker.Guard | MapMarker.TraversingTop;
                        break;
                }
            }
        }

        // lets get a bit fancy with our rendering!
        var mapContainer = new DumpContainer(map).Dump();

        //this 1 accounts for edge node we'll run into at the end (it needs traversal but we stop 1 short)
        ushort traversalCount = 1;
        var traversalCountContainer = new DumpContainer(traversalCount).Dump("Traversal Count");
        var loopCandidate = new List<(int X, int Y)>();

        //Part 1

        MapMarker marker;

        do
        {
            var lastGuardPosition = guardPosition;

            marker = map[guardPosition.Y, guardPosition.X];
            var direction = (MapMarker)((byte)marker & 0xF0);

            switch (direction)
            {
                case MapMarker.TraversingTop when MarkerType(map[guardPosition.Y - 1, guardPosition.X]) is MapMarker.Free or MapMarker.Edge or MapMarker.Traversed:
                    guardPosition.Y--;

                    break;
                case MapMarker.TraversingRight when MarkerType(map[guardPosition.Y, guardPosition.X + 1]) is MapMarker.Free or MapMarker.Edge or MapMarker.Traversed:
                    guardPosition.X++;

                    break;
                case MapMarker.TraversingBottom when MarkerType(map[guardPosition.Y + 1, guardPosition.X]) is MapMarker.Free or MapMarker.Edge or MapMarker.Traversed:
                    guardPosition.Y++;

                    break;
                case MapMarker.TraversingLeft when MarkerType(map[guardPosition.Y, guardPosition.X - 1]) is MapMarker.Free or MapMarker.Edge or MapMarker.Traversed:
                    guardPosition.X--;

                    break;

                case MapMarker.TraversingTop:
                case MapMarker.TraversingRight:
                case MapMarker.TraversingBottom:
                case MapMarker.TraversingLeft:
                    map[guardPosition.Y, guardPosition.X] = MapMarker.Guard | direction switch
                    {
                        MapMarker.TraversingTop => MapMarker.TraversingRight,
                        MapMarker.TraversingRight => MapMarker.TraversingBottom,
                        MapMarker.TraversingBottom => MapMarker.TraversingLeft,
                        MapMarker.TraversingLeft => MapMarker.TraversingTop,
                        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Unexpected traversal direction " + direction)
                    };
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, "Unexpected guard direction " + direction);
            }

            if (guardPosition != lastGuardPosition && map[guardPosition.Y, guardPosition.X] != MapMarker.Edge)
            {
                if (MarkerType(map[guardPosition.Y, guardPosition.X]) is not MapMarker.Traversed)
                    traversalCount++;
                else
                    loopCandidate.Add(guardPosition);

                map[lastGuardPosition.Y, lastGuardPosition.X] = MapMarker.Traversed | direction;
                map[guardPosition.Y, guardPosition.X] = MapMarker.Guard | direction;
            }

            //If you want to watch it all animate - uncomment the following and grab your coffee!
            //await Task.Delay(48).ConfigureAwait(false);
            //mapContainer.UpdateContent(map);

            //traversal would be one higher at the end because of the edge node.
            traversalCountContainer.UpdateContent(traversalCount);

        } while (map[guardPosition.Y, guardPosition.X] is not MapMarker.Edge);

        mapContainer.UpdateContent(map);
        traversalCountContainer.UpdateContent(traversalCount);


        loopCandidate.Dump();

        //Part 2
    }

    internal static MapMarker MarkerType(MapMarker value) => (MapMarker)((byte)value & 0x0F);
    internal static MapMarker TraversalDirection(MapMarker value) => (MapMarker)((byte)value & 0xF0);

    const char obstacleMarker = '#';
    const char guardMarker = '^';

    //Packed structure - lower 4 bits for markers, upper 4 bits for direction a guard is facing
    internal enum MapMarker : byte
    {
        Free = 0,
        Obstacle = 1,
        Guard = 2,
        Traversed = 3,
        Edge = 4,

        TraversingTop = 0 << 4,
        TraversingRight = 1 << 4,
        TraversingBottom = 2 << 4,
        TraversingLeft = 3 << 4,
    }
}

internal static object ToDump(object input)
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
        Program.MapMarker marker => Program.MarkerType(marker) switch
        {
            Program.MapMarker.Free => string.Empty,
            Program.MapMarker.Edge => Util.WithStyle("❌", "font-size: 2ch; color: #FFFFFF80"),
            Program.MapMarker.Obstacle => Util.WithStyle("🚩", "font-size: 2ch; color: #FFFFFFA0"),
            Program.MapMarker.Traversed => Program.TraversalDirection(marker) switch
            {
                Program.MapMarker.TraversingTop => Util.WithStyle("^", "color: #fbaed2"),
                Program.MapMarker.TraversingRight => Util.WithStyle(">️", "color: #63c5da"),
                Program.MapMarker.TraversingBottom => Util.WithStyle("V", "color: #cf71af"),
                Program.MapMarker.TraversingLeft => Util.WithStyle("<️", "color: #b6c699"),
                var x => x
            },
            Program.MapMarker.Guard => Util.WithStyle("👮", "font-size: 2ch;"),
            var x => x
        },
        var x => x
    };
}