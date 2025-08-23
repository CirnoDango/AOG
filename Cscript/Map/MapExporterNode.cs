using Godot;
using System.Collections.Generic;

[Tool]
public partial class MapExporterNode : Node
{
    [Export]
    public string mapName;
    [Export]
    public TileMapLayer tbase;
    [Export]
    public TileMapLayer tstand;
    [Export]
    public bool ExportNow
    {
        get => false;
        set
        {
            if (value)
            {
                GD.Print("点击 Inspector 开关 -> 执行导出");
                // 这里写你调用静态导出方法的逻辑
                var terrainToId = new Dictionary<string, int>
                {
                    { "Grass",      1 },
                    { "Water",      2 },
                    { "Road",       3 },
                    { "Wall",       4 },
                    { "Stone",      5 },
                    { "Floor",      6 },
                    { "Stair",      7 },
                    { "DoorClosed", 1 },
                    { "DoorOpen",   2 },
                    { "Forest",     3 }
                };
                // 假设 Map 在同一个场景里
                var mapNode = BuildLogicFromTileMap();
                if (mapNode != null)
                {
                    MapBuilder.ExportMapAsIdArray(mapNode, terrainToId, "res://map.json", mapName);
                }
            }
        }
    }

    public Map BuildLogicFromTileMap()
    {
        var tileMapBase = tbase;
        TileMapLayer tileMapStand = tstand;
        var tileSet = tileMapBase.TileSet;
        var tileSetS = tileMapStand.TileSet;
        var usedCells = tileMapBase.GetUsedCells();

        // 获取地图大小
        Rect2I bounds = tileMapBase.GetUsedRect();
        int width = bounds.Size.X;
        int height = bounds.Size.Y;

        var map = new Map(width, height)
        {
            Width = width,
            Height = height,
            Grid = new Grid[width, height]
        };

        foreach (var cell in usedCells)
        {
            var offset = bounds.Position;
            Vector2I localPos = cell - offset;

            // 👉 获取 BaseGround 的地形 ID
            var tileDataBase = tileMapBase.GetCellTileData(cell);
            int? terrainIdBase = tileDataBase?.Terrain;
            if (terrainIdBase == null || !terrainIdBase.HasValue)
            {
                GD.PrintErr($"位置 {cell} 没有 BaseGround 地形");
                continue;
            }

            string tBaseGround = tileSet.GetTerrainName(0, terrainIdBase.Value);

            // 👉 获取 Stand 的地形 ID（可选）
            string tStand = "";
            //if (tileMapStand.IsInsideTile(cell))
            {
                var tileDataStand = tileMapStand.GetCellTileData(cell);
                int? terrainIdStand = tileDataStand?.Terrain;
                if (terrainIdStand != null && terrainIdStand.HasValue)
                {
                    tStand = tileSetS.GetTerrainName(0, terrainIdStand.Value);
                }
            }
            //GD.Print(tStand);
            // 创建 Grid
            map.Grid[localPos.X, localPos.Y] = new Grid(localPos, tBaseGround, tStand);
        }

        return map;
    }
}


