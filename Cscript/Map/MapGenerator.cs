using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public static class MapGenerator
{
    /// <summary>
    /// 生成一个地图，使用细胞自动机算法，传入的第一个地形类型作为边界地形
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="terrainTypes"></param>
    /// <param name="initialWeights"></param>
    /// <param name="iterations"></param>
    /// <returns></returns>
    public static Map GenerateMapByCellularAutomata(int width, int height,
        Dictionary<string, float> initialWeights,
        int iterations, bool noExit = false)
    {
        string[] terrainTypes = initialWeights.Keys.ToArray();
        var map = new Map(width, height);

        // 初始化地图（按权重随机地形）
        ChangeMapByWeight(LogicMapLayer.BaseGround, initialWeights, map);

        // 进行细胞演化
        ChangeMapByEnvolve(LogicMapLayer.BaseGround, terrainTypes[0], map, iterations);
        // 设置入口出口
        map.Entrance = map.RandomEmptyGrid().Position;
        if (!noExit)
        {
            var exit = FloodFindFarthest(map, map.Entrance);
            map.SetExit();
        }
        return map;
    }


    /// <summary>
    /// 生成一个地图,使用BSP算法
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="initialWeights"></param>
    /// <param name="minRoomSize"></param>
    /// <param name="maxDepth"></param>
    /// <param name="noExit"></param>
    /// <returns></returns>
    public static Map GenerateMapByBSP(int width, int height, string wall, string floor,
        int minRoomSize = 6, int maxDepth = 5, bool noExit = false)
    {
        var map = new Map(width, height);
        // 初始化为全墙
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                map.Grid[x, y] = new Grid(new Vector2I(x, y), wall);

        var rooms = new List<Rect>();
        SplitRegion(new Rect(1, 1, width - 2, height - 2), maxDepth, minRoomSize, rooms);

        // 生成房间
        for (int i = 0; i < rooms.Count; i++)
        {
            var room = rooms[i];
            // 先计算最大合法的 room 宽高
            int maxRoomWidth = Math.Max(minRoomSize, room.Width);
            int maxRoomHeight = Math.Max(minRoomSize, room.Height);

            if (maxRoomWidth < minRoomSize || maxRoomHeight < minRoomSize)
                continue; // 跳过这个太小的房间块

            int roomWidth = Random.Shared.Next(minRoomSize, maxRoomWidth);
            int roomHeight = Random.Shared.Next(minRoomSize, maxRoomHeight);

            int maxX = room.Right - roomWidth + 1;
            int maxY = room.Bottom - roomHeight + 1;

            int roomX = Random.Shared.Next(room.Left, Math.Max(room.Left, maxX + 1));
            int roomY = Random.Shared.Next(room.Top, Math.Max(room.Top, maxY + 1));

            rooms[i] = new Rect(roomX, roomY, roomWidth, roomHeight);
            for (int x = roomX; x < roomX + roomWidth; x++)
                for (int y = roomY; y < roomY + roomHeight; y++)
                    map.Grid[x, y] = new Grid(new Vector2I(x, y), floor);
        }
        rooms = Rect.SortRectsNearest(rooms);
        List<Vector2I> doors = [];
        // 连接房间
        for (int i = 1; i < rooms.Count; i++)
        {
            Vector2I a = GetRoomCenter(rooms[i - 1]);
            Vector2I b = GetRoomCenter(rooms[i]);

            doors.AddRange(ConnectRoomsWithDoor(map, rooms[i - 1], rooms[i], floor));

        }
        // 设置入口与出口
        map.Entrance = map.RandomEmptyGrid().Position;
        if (!noExit)
        {
            map.SetExit();
        }

        foreach (var door in doors)
            map.Grid[door.X, door.Y] = new Grid(door, floor, "DoorClosed");
        return map;
    }

    /// <summary>
    /// 得到一个随机地形类型，按照给定的权重分布
    /// </summary>
    /// <param name="types"></param>
    /// <param name="weights"></param>
    /// <returns></returns>
    private static string GetRandomTerrain(string[] types, Dictionary<string, float> weights)
    {
        float total = 0;
        foreach (var type in types)
            total += weights[type];

        float r = (float)(GD.Randf() * total);
        float current = 0;

        foreach (var type in types)
        {
            current += weights[type];
            if (r <= current)
                return type;
        }

        return types[^1]; // 兜底
    }
    /// <summary>
    /// Determines the most common terrain type in the 3x3 neighborhood around a specified cell in the map.
    /// </summary>
    /// <remarks>The method considers the 3x3 neighborhood centered on the cell at (<paramref name="x"/>,
    /// <paramref name="y"/>), including the cell itself. If the cell is at the edge of the map, the <paramref
    /// name="border"/> terrain type is used for out-of-bounds neighbors.</remarks>
    /// <param name="map">The map containing the grid of terrain cells.</param>
    /// <param name="x">The x-coordinate of the target cell.</param>
    /// <param name="y">The y-coordinate of the target cell.</param>
    /// <param name="border">The terrain type to use for cells outside the map boundaries.  This value is treated as the terrain type for
    /// out-of-bounds cells.</param>
    /// <returns>The terrain type that appears most frequently in the 3x3 neighborhood around the specified cell. If multiple
    /// terrain types have the same frequency, the method returns one of them arbitrarily.</returns>
    private static string EvolveTerrain(LogicMapLayer mapLayer, Map map, int x, int y, string border)
    {
        var counter = new Dictionary<string, int>();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;

                string terrain;

                if (IsInBounds(map, nx, ny))
                {
                    switch (mapLayer)
                    {
                        case LogicMapLayer.Stand:
                            terrain = map.Grid[nx, ny].TerrainStand;
                            break;
                        case LogicMapLayer.BaseGround:
                            terrain = map.Grid[nx, ny].TerrainBaseGround;
                            break;
                        default:
                            throw new ArgumentException("Unsupported map layer type");
                    }
                }
                else
                    terrain = border; // 边界外默认是某个类型，例如 Wall

                if (!counter.ContainsKey(terrain))
                    counter[terrain] = 0;

                counter[terrain]++;
            }
        }

        // 找到出现次数最多的地形
        string mostCommon = "Grass";
        int maxCount = -1;

        foreach (var kvp in counter)
        {
            if (kvp.Value > maxCount)
            {
                maxCount = kvp.Value;
                mostCommon = kvp.Key;
            }
        }

        return mostCommon;
    }

    private static bool IsInBounds(Map map, int x, int y)
    {
        return x >= 0 && y >= 0 && x < map.Width && y < map.Height;
    }
    public static Vector2I FloodFindFarthest(Map map, Vector2I start)
    {
        var visited = new bool[map.Width, map.Height];
        var queue = new Queue<(Vector2I pos, int dist)>();
        queue.Enqueue((start, 0));
        visited[start.X, start.Y] = true;

        Vector2I farthest = start;
        int maxDist = 0;

        while (queue.Count > 0)
        {
            var (pos, dist) = queue.Dequeue();

            if (dist > maxDist)
            {
                maxDist = dist;
                farthest = pos;
            }

            foreach (var dir in MathEx.NearVectors)
            {
                var next = pos + dir;
                if (map.CheckGrid(next) && !visited[next.X, next.Y] && map.GetGrid(next).IsWalkable)
                {
                    visited[next.X, next.Y] = true;
                    queue.Enqueue((next, dist + 1));
                }
            }
        }

        return farthest;
    }
    public static void SplitRegion(Rect region, int depth, int minSize, List<Rect> output)
    {
        if (depth <= 0 || region.Width < minSize * 2 + 1 && region.Height < minSize * 2 + 1)
        {
            output.Add(region);
            return;
        }

        bool splitHorizontally = region.Width < region.Height;

        if (splitHorizontally)
        {
            int splitY = Random.Shared.Next(region.Top + minSize, region.Bottom - minSize);
            SplitRegion(new Rect(region.Left, region.Top, region.Width, splitY - region.Top), depth - 1, minSize, output);
            SplitRegion(new Rect(region.Left, splitY, region.Width, region.Bottom - splitY), depth - 1, minSize, output);
        }
        else
        {
            int splitX = Random.Shared.Next(region.Left + minSize, region.Right - minSize);
            SplitRegion(new Rect(region.Left, region.Top, splitX - region.Left, region.Height), depth - 1, minSize, output);
            SplitRegion(new Rect(splitX, region.Top, region.Right - splitX, region.Height), depth - 1, minSize, output);
        }
    }

    public static Vector2I GetRoomCenter(Rect room)
    {
        return new Vector2I(room.Left + room.Width / 2, room.Top + room.Height / 2);
    }
    public static List<Vector2I> ConnectRoomsWithDoor(Map map, Rect ra, Rect rb, string floor, string door = "DoorClosed")
    {
        List<Vector2I> Doors = [];
        List<Vector2I> path = new();
        Vector2I a = GetRoomCenter(ra);
        Vector2I b = GetRoomCenter(rb);
        // L型路径
        bool horizontalFirst = Random.Shared.Next(2) == 0;

        if (horizontalFirst)
        {
            for (int x = Math.Min(a.X, b.X); x <= Math.Max(a.X, b.X); x++)
                path.Add(new Vector2I(x, a.Y));
            for (int y = Math.Min(a.Y, b.Y); y <= Math.Max(a.Y, b.Y); y++)
                path.Add(new Vector2I(b.X, y));
        }
        else
        {
            for (int y = Math.Min(a.Y, b.Y); y <= Math.Max(a.Y, b.Y); y++)
                path.Add(new Vector2I(a.X, y));
            for (int x = Math.Min(a.X, b.X); x <= Math.Max(a.X, b.X); x++)
                path.Add(new Vector2I(x, b.Y));
        }

        // 开始填充地板
        for (int i = 0; i < path.Count; i++)
        {
            var pos = path[i];
            int air = map.NearGrids(map.GetGrid(pos), 1).Where(x => x.IsWalkable).ToList().Count;
            // 只在开头和结尾考虑放门（且前面是墙，后面是地板）
            if ((ra.IsAdjacentOutside(pos) || rb.IsAdjacentOutside(pos)) && air < 7 && Random.Shared.NextDouble() < 1)
            {
                Doors.Add(new Vector2I(pos.X, pos.Y));
            }
            map.Grid[pos.X, pos.Y] = new Grid(pos, floor);
        }
        if (Doors.Count > 2)
            Doors = [Doors[0], Doors[^1]];
        return Doors;
    }
    public static void ChangeMapByRegionGrow
        (Map map, LogicMapLayer LayerIn, LogicMapLayer LayerOut, string terrainIn, string terrainOut,
        float numberMean, float numberDev, float sizeMean, float sizeDev)
    {
        int number = (int)GD.Randfn(numberMean, numberDev);
        for (int i = 0; i < number; i++)
        {
            int startX = GD.RandRange(1, map.Width - 1);
            int startY = GD.RandRange(1, map.Height - 1);

            int size = Math.Max(0, (int)GD.Randfn(sizeMean, sizeDev));
            Queue<(int x, int y)> queue = new();
            HashSet<(int x, int y)> visited = [];

            queue.Enqueue((startX, startY));
            visited.Add((startX, startY));

            int count = 0;

            while (queue.Count > 0 && count < size)
            {
                var (x, y) = queue.Dequeue();
                string tr = LayerIn switch
                {
                    LogicMapLayer.Stand => map.Grid[x, y].TerrainStand,
                    LogicMapLayer.BaseGround => map.Grid[x, y].TerrainBaseGround,
                    _ => throw new ArgumentException("Unsupported LayerIn type"),
                };
                if (!map.CheckGrid(x, y) || tr != terrainIn)
                    continue;

                switch (LayerOut)
                {
                    case LogicMapLayer.Stand:
                        map.Grid[x, y].TerrainStand = terrainOut;
                        break;
                    case LogicMapLayer.BaseGround:
                        map.Grid[x, y].TerrainBaseGround = terrainOut;
                        break;
                }
                count++;

                foreach (var (dx, dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                {
                    int nx = x + dx, ny = y + dy;
                    if (map.CheckGrid(nx, ny) && !visited.Contains((nx, ny)))
                    {
                        if (GD.Randf() < 0.7f)
                            queue.Enqueue((nx, ny));
                        visited.Add((nx, ny));
                    }
                }
            }
        }

    }
    public static void ChangeMapByWeight(LogicMapLayer mapLayer,
        Dictionary<string, float> initialWeights, Map map)
    {
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                var terrain = GetRandomTerrain([.. initialWeights.Keys], initialWeights);
                switch (mapLayer)
                {
                    case LogicMapLayer.Stand:
                        map.Grid[x, y].TerrainStand = terrain;
                        break;
                    case LogicMapLayer.BaseGround:
                        map.Grid[x, y].TerrainBaseGround = terrain;
                        break;
                }
            }
        }
    }

    public static void ChangeMapByEnvolve(LogicMapLayer mapLayer,
        string terrainBorder, Map map, int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            var newGrid = new Grid[map.Width, map.Height];
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    string terrain = EvolveTerrain(mapLayer, map, x, y, terrainBorder);
                    newGrid[x, y] = new Grid(new Vector2I(x, y),
                        map.Grid[x, y].TerrainBaseGround, map.Grid[x, y].TerrainStand);
                    switch (mapLayer)
                    {
                        case LogicMapLayer.Stand:
                            newGrid[x, y].TerrainStand = terrain;
                            break;
                        case LogicMapLayer.BaseGround:
                            newGrid[x, y].TerrainBaseGround = terrain;
                            break;
                    }
                }
            }
            map.Grid = newGrid;
        }
    }
    public static void ChangeMapByRoad(LogicMapLayer mapLayer,
    string terrain, Map map, out int initY, out int outY)
    {
        initY = GD.RandRange(0, map.Height - 1); // 起点
        int y = initY;
        int x = 0;

        int prevDir = 0;

        while (x < map.Width - 1)
        {
            map.Grid[x, y].TerrainBaseGround = terrain;
            map.Grid[x, y].TerrainStand = "";
            map.Grid[x + 1, y].TerrainBaseGround = terrain;
            map.Grid[x + 1, y].TerrainStand = "";

            // 根据马尔可夫链决定下一个方向
            int direction = NextDirection(prevDir);

            // 移动
            if (direction == 1 && y > 0) y--;                   
            else if (direction == 2 && y < map.Height - 1) y++; 
                                                                

            prevDir = direction;
            x++;
        }
        outY = y;
        int NextDirection(int prevDir)
        {
            double r = GD.Randf();

            switch (prevDir)
            {
                case 0: // 直行
                    if (r < 0.5) return 0; 
                    if (r < 0.75) return 1;
                    return 2;              

                case 1: // 上
                    if (r < 0.6) return 0; 
                    return 1;              

                case 2: // 下
                    if (r < 0.6) return 0; 
                    return 2;              
            }

            return 0;
        }
    }
}

    


public class Rect
{
    public int Left, Top, Width, Height;
    public int Right => Left + Width - 1;
    public int Bottom => Top + Height - 1;

    public Rect(int left, int top, int width, int height)
    {
        Left = left; Top = top; Width = width; Height = height;
    }
    public bool IsAdjacentOutside(Vector2I pos)
    {
        if (pos.Y >= Top && pos.Y <= Bottom)
        {
            if (pos.X == Left - 1 || pos.X == Right + 1)
                return true;
        }
        if (pos.X >= Left && pos.X <= Right)
        {
            if (pos.Y == Top - 1 || pos.Y == Bottom + 1)
                return true;
        }
        return false;
    }
    public static List<Rect> SortRectsNearest(List<Rect> rects)
    {
        if (rects == null || rects.Count <= 1)
            return rects;

        var sorted = new List<Rect>();
        var remaining = new HashSet<Rect>(rects);

        // 从第一个开始
        Rect current = rects[0];
        sorted.Add(current);
        remaining.Remove(current);

        while (remaining.Count > 0)
        {
            Rect nearest = remaining.OrderBy(r => Distance(current, r)).First();
            sorted.Add(nearest);
            remaining.Remove(nearest);
            current = nearest;
        }

        return sorted;
    }

    // 使用中心点之间的曼哈顿距离作为估计路径距离
    private static int Distance(Rect a, Rect b)
    {
        int ax = a.Left + a.Width / 2;
        int ay = a.Top + a.Height / 2;
        int bx = b.Left + b.Width / 2;
        int by = b.Top + b.Height / 2;
        return Math.Abs(ax - bx) + Math.Abs(ay - by);
    }
}







