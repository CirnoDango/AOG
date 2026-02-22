using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class MapPrebuild(int width, int height) : Map(width, height)
{
    // 0左 1上 2右 3下
    public List<Vector3I> exits = [];
    private static Random rng = new Random();

    // 出口对向映射
    private static readonly Dictionary<int, int> OppositeDir = new()
    {
        { 0, 2 }, { 1, 3 }, { 2, 0 }, { 3, 1 }
    };

    private static Vector2I DirToVec(int dir)
    {
        return dir switch
        {
            0 => new Vector2I(-1, 0), // left
            1 => new Vector2I(0, -1), // up (y decreases)
            2 => new Vector2I(1, 0),  // right
            3 => new Vector2I(0, 1),  // down (y increases)
            _ => new Vector2I(0, 0)
        };
    }

    /// <summary>
    /// 返回全局格子数组，并通过 out 返回已经放置的房间与其原点（便于调试/渲染）
    /// </summary>
    public static Map Generate(
        int globalWidth, int globalHeight,
        MapPrebuild startRoom, Vector2I startPos,
        int maxAttempts, string defaultBase,
        List<MapPrebuild> prefabRooms)
    {
        List<(MapPrebuild room, Vector2I origin)> placedRooms = [];

        var globalGrid = new Grid[globalWidth, globalHeight];
        for(int x = 0; x < globalWidth; x++)
        {
            for(int y = 0; y < globalHeight; y++)
            {
                globalGrid[x, y] = new(new Vector2I(x, y), defaultBase);
            }
        }

        var openExits = new Queue<(Vector3I exit, Vector2I roomOrigin, MapPrebuild room)>();

        // 放置起始房间
        PlaceRoom(globalGrid, startRoom, startPos, defaultBase);
        placedRooms.Add((startRoom, startPos));

        foreach (var exit in startRoom.exits)
            openExits.Enqueue((exit, startPos, startRoom));

        // 避免重复处理同一个世界出口（使用世界出口坐标做 key）
        var processedExits = new HashSet<string>();

        while (openExits.Count > 0)
        {
            var (exit, origin, room) = openExits.Dequeue();

            // 世界坐标（已放置房间的出口在全局坐标系的位置）
            int worldExitX = origin.X + exit.X;
            int worldExitY = origin.Y + exit.Y;

            string exitKey = $"{worldExitX},{worldExitY}";
            if (processedExits.Contains(exitKey))
                continue;
            // 标记为已处理（无论放置成功与否，本次出口已尝试）
            processedExits.Add(exitKey);
            // 目标方向：我们希望候选房间的出口的内部方向是 OppositeDir(exit.Z)
            int targetDir = OppositeDir[exit.Z];

            // 计算候选房间的出口应当落在哪个世界格（与当前出口“相邻对接”）
            Vector2I offsetToCandidateExit = DirToVec(targetDir); // e.g. if targetDir==3 (right) => (1,0)
            int desiredCandWorldX = worldExitX + offsetToCandidateExit.X;
            int desiredCandWorldY = worldExitY + offsetToCandidateExit.Y;

            bool placed = false;

            for (int attempt = 0; attempt < maxAttempts && !placed; attempt++)
            {
                var prefab = prefabRooms[rng.Next(prefabRooms.Count)];
                var candidate = prefab.GetRandomVariant();

                // 遍历 candidate 的出口，寻找与 targetDir 匹配的出口
                foreach (var candExit in candidate.exits)
                {
                    if (candExit.Z == targetDir)
                    {

                        // 我们希望 candExit 在世界上的位置恰好是 desiredCandWorld
                        int newOriginX = desiredCandWorldX - candExit.X;
                        int newOriginY = desiredCandWorldY - candExit.Y;
                        var newOrigin = new Vector2I(newOriginX, newOriginY);

                        // 检查能否放下（越界/重叠检测）
                        if (!CanPlace(globalGrid, candidate, newOrigin, defaultBase))
                            continue;

                        // 放置房间到全局地图
                        PlaceRoom(globalGrid, candidate, newOrigin, defaultBase);
                        placedRooms.Add((candidate, newOrigin));

                        // 将 candidate 的其它出口加入队列（跳过已对接的 candExit）
                        foreach (var e in candidate.exits)
                        {
                            // 若 e 的世界坐标与对接的 candExit 的世界坐标相同，则跳过（已经对接）
                            if (e.X == candExit.X && e.Y == candExit.Y && e.Z == candExit.Z)
                                continue;

                            // 计算该出口的世界坐标，若越界也可以先入队，后面会跳过
                            openExits.Enqueue((e, newOrigin, candidate));
                        }

                        placed = true;
                        break; // 跳出 candExit 循环
                    }
                    else
                        continue;
                }
            }

            // 如果 maxAttempts 都没放下，则放弃这个出口（processedExits 已标记，后续不会再尝试）
        }
        Map map = new(globalWidth, globalHeight)
        {
            Grid = globalGrid
        };
        for (int x = 0; x < globalWidth; x++)
        {
            for (int y = 0; y < globalHeight; y++)
            {
                map.Grid[x, y].Position = new Vector2I(x, y);
            }
        }
        return map;
    }

    private static bool CanPlace(Grid[,] globalGrid, MapPrebuild room, Vector2I origin, string defau)
    {
        int gw = globalGrid.GetLength(0);
        int gh = globalGrid.GetLength(1);

        for (int y = 0; y < room.Height; y++)
        {
            for (int x = 0; x < room.Width; x++)
            {
                int gx = origin.X + x;
                int gy = origin.Y + y;

                // 越界
                if (gx < 0 || gy < 0 || gx >= gw || gy >= gh)
                    return false;

                // 若房间此格为空则不影响
                if (room.Grid[x, y] == null) continue;

                // 若全局此格已有内容则冲突
                if (globalGrid[gx, gy].TerrainBaseGround != defau)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static void PlaceRoom(Grid[,] globalGrid, MapPrebuild room, Vector2I origin, string defau)
    {
        for (int y = 0; y < room.Height; y++)
        {
            for (int x = 0; x < room.Width; x++)
            {
                int gx = origin.X + x;
                int gy = origin.Y + y;
                if (room.Grid[x, y] != null)
                {
                    globalGrid[gx, gy] = new(new Vector2I(gx, gy), room.Grid[x, y].TerrainBaseGround, room.Grid[x, y].TerrainStand);
                    if (globalGrid[gx, gy].TerrainBaseGround == "")
                        globalGrid[gx, gy].TerrainBaseGround = defau;
                }
                    
            }
        }
    }
    public MapPrebuild GetRandomVariant()
    {
        Random rng = new();

        // 收集所有可能的变种
        var variants = new List<MapPrebuild>
        {
            // 原始
            this,

            // 单旋转
            Rotate(90),
            this.Rotate(180),
            this.Rotate(270),

            // 翻转
            this.FlipHorizontal(),
            this.FlipVertical(),

            // 翻转 + 旋转
            this.FlipHorizontal().Rotate(90),
            this.FlipVertical().Rotate(90)
        };

        // 随机取一个
        int index = rng.Next(variants.Count);
        MapPrebuild mp = variants[index];
        for (int y = 0; y < mp.Height; y++)
        {
            for (int x = 0; x < mp.Width; x++)
            {
                mp.Grid[x, y].Position = new Vector2I(x, y);
            }
        }
        return mp;
    }

    /// <summary>
    /// 旋转地图（度数只能是90/180/270）
    /// </summary>
    public MapPrebuild Rotate(int degree)
    {
        degree = ((degree % 360) + 360) % 360; // 归一化
        MapPrebuild newMap;

        if (degree == 90)
            newMap = Rotate90();
        else if (degree == 180)
            newMap = Rotate180();
        else if (degree == 270)
            newMap = Rotate270();
        else
            throw new ArgumentException("只支持90/180/270度旋转");

        return newMap;
    }

    private MapPrebuild Rotate90()
    {
        var newMap = new MapPrebuild(Height, Width);
        newMap.Grid = new Grid[Height, Width];

        // 旋转格子
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                int newX = y;
                int newY = Width - 1 - x;
                newMap.Grid[newX, newY] = Grid[x, y];
            }
        }

        // 旋转出口
        foreach (var e in exits)
        {
            int newX = e.Y;
            int newY = Width - 1 - e.X;
            int newDir = RotateDir90(e.Z);
            newMap.exits.Add(new Vector3I(newX, newY, newDir));
        }

        return newMap;
    }

    private MapPrebuild Rotate180()
    {
        var newMap = new MapPrebuild(Width, Height);
        newMap.Grid = new Grid[Width, Height];

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                int newX = Width - 1 - x;
                int newY = Height - 1 - y;
                newMap.Grid[newX, newY] = Grid[x, y];
            }
        }

        foreach (var e in exits)
        {
            int newX = Width - 1 - e.X;
            int newY = Height - 1 - e.Y;
            int newDir = RotateDir180(e.Z);
            newMap.exits.Add(new Vector3I(newX, newY, newDir));
        }

        return newMap;
    }

    private MapPrebuild Rotate270()
    {
        var newMap = new MapPrebuild(Height, Width);
        newMap.Grid = new Grid[Height, Width];

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                int newX = Height - 1 - y;
                int newY = x;
                newMap.Grid[newX, newY] = Grid[x, y];
            }
        }

        foreach (var e in exits)
        {
            int newX = Height - 1 - e.Y;
            int newY = e.X;
            int newDir = RotateDir270(e.Z);
            newMap.exits.Add(new Vector3I(newX, newY, newDir));
        }

        return newMap;
    }


    /// <summary>
    /// 左右翻转
    /// </summary>
    public MapPrebuild FlipHorizontal()
    {
        var newMap = new MapPrebuild(Width, Height);
        newMap.Grid = new Grid[Width, Height];

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                newMap.Grid[Width - 1 - x, y] = Grid[x, y];
            }
        }

        foreach (var e in exits)
        {
            int newX = Width - 1 - e.X;
            int newY = e.Y;
            int newDir = FlipDirHorizontal(e.Z);
            newMap.exits.Add(new Vector3I(newX, newY, newDir));
        }

        return newMap;
    }

    /// <summary>
    /// 上下翻转
    /// </summary>
    public MapPrebuild FlipVertical()
    {
        var newMap = new MapPrebuild(Width, Height);
        newMap.Grid = new Grid[Width, Height];

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                newMap.Grid[x, Height - 1 - y] = Grid[x, y];
            }
        }

        foreach (var e in exits)
        {
            int newX = e.X;
            int newY = Height - 1 - e.Y;
            int newDir = FlipDirVertical(e.Z);
            newMap.exits.Add(new Vector3I(newX, newY, newDir));
        }

        return newMap;
    }

    // 出口方向变换
    private int RotateDir270(int dir) => dir switch { 1 => 2, 2 => 3, 3 => 0, 0 => 1, _ => dir };
    private int RotateDir180(int dir) => dir switch { 1 => 3, 2 => 0, 3 => 1, 0 => 2, _ => dir };
    private int RotateDir90(int dir) => dir switch { 1 => 0, 2 => 1, 3 => 2, 0 => 3, _ => dir };

    private int FlipDirVertical(int dir) => dir switch { 1 => 3, 3 => 1, _ => dir };
    private int FlipDirHorizontal(int dir) => dir switch { 2 => 0, 0 => 2, _ => dir };
}
public class MapPrebuilder
{
    public static List<MapPrebuild> MapPrebuildDeck { get; set; } = [];
    public static MapPrebuild GetMapPrebuild(string name)
    {
        Console.WriteLine(MapPrebuildDeck.Count.ToString() + " maps");
        foreach (var m in MapPrebuildDeck)
            Console.WriteLine(m.Name);
        return MapPrebuildDeck
    .FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
    public static void LoadMapsFromFolder(string folderPath)
    {
        var maps = new List<MapPrebuild>();

        using DirAccess dir = DirAccess.Open(folderPath);
        if (dir == null)
        {
            GD.PrintErr($"无法打开文件夹: {folderPath}");
        }

        dir.ListDirBegin();

        string fileName = dir.GetNext();
        Console.WriteLine("open: " + fileName);
        while (fileName != "")
        {
            if (!dir.CurrentIsDir() && (fileName.EndsWith(".tscn.remap") || fileName.EndsWith(".tscn")))
            {
                string realName = fileName.Replace(".remap", "");

                string filePath = $"{folderPath}/{realName}";

                GD.Print("Loading map: ", filePath);

                try
                {
                    var map = LoadMap(filePath);

                    if (map != null)
                    {
                        map.Name = System.IO.Path.GetFileNameWithoutExtension(realName);

                        maps.Add(map);

                        GD.Print("Loaded OK: ", map.Name);
                    }
                    else
                    {
                        GD.PrintErr("LoadMap returned NULL");
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"加载地图失败: {ex}");
                }
            }

            fileName = dir.GetNext();
        }

        dir.ListDirEnd();

        MapPrebuildDeck.AddRange(maps);
    }

    public static MapPrebuild LoadMap(string path)
    {
        // 1. 加载场景
        PackedScene scene = GD.Load<PackedScene>(path);
        if (scene == null)
        {
            GD.PrintErr($"无法加载场景: {path}");
            return null;
        }

        // 2. 实例化
        Control root = scene.Instantiate<Control>();
        if (root == null)
        {
            GD.PrintErr("实例化失败，根节点不是 Control");
            return null;
        }

        // 3. 获取 TileMapLayer
        TileMapLayer layerBaseGround = root.GetNode<TileMapLayer>("BaseGround");
        TileMapLayer layerStand = root.GetNode<TileMapLayer>("Stand");
        TileMapLayer layerExit = root.GetNode<TileMapLayer>("Exit");
        // 4. 获取使用到的地图范围
        Rect2I usedRect = layerBaseGround.GetUsedRect();
        int width = usedRect.Size.X;
        int height = usedRect.Size.Y;

        // 5. 遍历每个格子
        MapPrebuild map = new(width, height);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                string tBaseGround = "";
                string tStand = "";
                Vector2I cell = new(x + usedRect.Position.X, y + usedRect.Position.Y);

                var tileDataBase = layerBaseGround.GetCellTileData(cell);
                int? terrainIdBase = tileDataBase?.Terrain;
                if (terrainIdBase == null || !terrainIdBase.HasValue) { }
                else
                    tBaseGround = layerBaseGround.TileSet.GetTerrainName(0, terrainIdBase.Value);

                var tileDataStand = layerStand.GetCellTileData(cell);
                int? terrainIdStand = tileDataStand?.Terrain;
                if (terrainIdStand == null || !terrainIdStand.HasValue) { }
                else
                    tStand = layerStand.TileSet.GetTerrainName(0, terrainIdStand.Value);

                var tileDataExit = layerExit.GetCellTileData(cell);
                int? terrainIdExit = tileDataExit?.Terrain;
                if (terrainIdExit == null || !terrainIdExit.HasValue) { }
                else
                    map.exits.Add(new Vector3I(x, y, (int)terrainIdExit));
                map.Grid[x, y] = new Grid(new Vector2I(x, y), tBaseGround, tStand);
            }
        }
        return map;
    }
}
