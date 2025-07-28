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
    public static Map GenerateMapByCellularAutomata(
        int width,
        int height,
        Dictionary<string, float> initialWeights,
        int iterations, bool noExit = false)
    {
        string[] terrainTypes = initialWeights.Keys.ToArray();
        var map = new Map(width, height);

        // 初始化地图（按权重随机地形）
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var terrain = GetRandomTerrain(terrainTypes, initialWeights);
                map.Grid[x, y] = new Grid(new Vector2I(x, y), terrain);
            }
        }

        // 进行细胞演化
        for (int i = 0; i < iterations; i++)
        {
            var newGrid = new Grid[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    string newTerrain = EvolveTerrain(map, x, y, terrainTypes[0]);
                    newGrid[x, y] = new Grid(new Vector2I(x, y), newTerrain);
                }
            }

            map.Grid = newGrid;
        }
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
    public static Map GenerateMapByBSP(
    int width,
    int height,
    string wall, string floor,
    int minRoomSize = 6,
    int maxDepth = 5,
    bool noExit = false)
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

        // 连接房间
        for (int i = 1; i < rooms.Count; i++)
        {
            Vector2I a = GetRoomCenter(rooms[i - 1]);
            Vector2I b = GetRoomCenter(rooms[i]);
            ConnectRoomsWithDoor(map, rooms[i - 1], rooms[i], floor);
        }

        // 设置入口与出口
        map.Entrance = map.RandomEmptyGrid().Position;
        if (!noExit)
        {
            var exit = FloodFindFarthest(map, map.Entrance);
            map.SetExit();
        }

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
    /// 对于每个格子，根据周围8个格子的地形类型，演化出新的地形类型   
    /// </summary>
    /// <param name="map"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="border"></param>
    /// <returns></returns>
    private static string EvolveTerrain(Map map, int x, int y, string border)
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
                    terrain = map.Grid[nx, ny].TerrainBaseGround;
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
    public static void ConnectRoomsWithDoor(Map map, Rect ra, Rect rb, string floor, string door = "DoorClosed")
    {
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
            int dor = map.NearGrids(map.GetGrid(pos), 1).Where(x => x.TerrainStand == "DoorClosed").ToList().Count;
            // 只在开头和结尾考虑放门（且前面是墙，后面是地板）
            if ((ra.IsAdjacentOutside(pos) || rb.IsAdjacentOutside(pos)) && air < 5 && dor == 0 && Random.Shared.NextDouble() < 1)
            {
                map.Grid[pos.X, pos.Y] = new Grid(pos, floor, door);
            }
            else
            {
                map.Grid[pos.X, pos.Y] = new Grid(pos, floor);
            }
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




