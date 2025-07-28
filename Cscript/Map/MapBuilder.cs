using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
public enum LogicMapLayer
{
    BaseGround,Stand,FogOfWar
}
public static class MapBuilder
{

    // 单独定义在类中（或作为静态工具函数）

    public static Map BuildLogicFromTileMap()
    {
        var tileMapBase = G.I.TileMapAllLayer.BaseGround;
        TileMapLayer tileMapStand = G.I.TileMapAllLayer.Stand;
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


    public static void BuildTileMapFromLogic(Map map)
    {
        G.I.TileMapAllLayer.BaseGround.Clear();
        G.I.TileMapAllLayer.Stand.Clear();
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                var grid = map.Grid[x, y];
                var pos = new Vector2I(x, y);
                SetTileMapTerrain(G.I.TileMapAllLayer.BaseGround, pos, grid.TerrainBaseGround);
                SetTileMapTerrain(G.I.TileMapAllLayer.Stand, pos, grid.TerrainStand);
            }
        }
    }

    public static void SetLogicMapTerrain(LogicMapLayer tileMap, Grid grid, string terrainName)
    {
        switch (tileMap)
        {
            case LogicMapLayer.BaseGround:
                grid.TerrainBaseGround = terrainName;
                SetTileMapTerrain(G.I.TileMapAllLayer.BaseGround, grid.Position, terrainName);
                break;
            case LogicMapLayer.Stand:
                grid.TerrainStand = terrainName;
                SetTileMapTerrain(G.I.TileMapAllLayer.Stand, grid.Position, terrainName);
                break;
            case LogicMapLayer.FogOfWar:
                grid.TerrainFogOfWar = terrainName;
                SetTileMapTerrain(G.I.TileMapAllLayer.FogOfWar, grid.Position, terrainName);
                break;
        }
    }
    public static void BuildFogOfWar(Map map)
    {
        G.I.TileMapAllLayer.FogOfWar.Clear();
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                var g = map.Grid[x, y];
                g.TerrainFogOfWar = "Fog";
                SetLogicMapTerrain(LogicMapLayer.FogOfWar, g, "Fog");
            }
        }
    }
    private static void SetTileMapTerrain(TileMapLayer tileMap, Vector2I pos, string terrainName)
    {
        var tileSet = tileMap.TileSet;

        for (int terrainId = 0; terrainId < tileSet.GetTerrainsCount(0); terrainId++)
        {
            if (tileSet.GetTerrainName(0, terrainId) == terrainName)
            {
                tileMap.SetCellsTerrainConnect([pos], 0, terrainId);
                return;
            }
        }
    }

    public static void ExportMapAsIdArray(Map map, Dictionary<string, int> terrainToId, string path)
    {
        int width = map.Width;
        int height = map.Height;

        int[][] baseLayer = new int[height][];
        int[][] standLayer = new int[height][];

        for (int y = 0; y < height; y++)
        {
            baseLayer[y] = new int[width];
            standLayer[y] = new int[width];
            for (int x = 0; x < width; x++)
            {
                var g = map.Grid[x, y];
                baseLayer[y][x] = terrainToId.TryGetValue(g.TerrainBaseGround, out var baseId) ? baseId : 0;
                standLayer[y][x] = terrainToId.TryGetValue(g.TerrainStand, out var standId) ? standId : 0;
            }
        }

        var export = new MapExportData
        {
            Width = width,
            Height = height,
            Base = baseLayer,
            Stand = standLayer
        };

        var json = JsonSerializer.Serialize(export, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        using var file = FileAccess.Open("user://map.json", FileAccess.ModeFlags.Write);
        file.StoreString(json);
    }
    public static Map ImportMapFromJson(string path, Dictionary<int, string> idToTerrain)
    {
        if (!FileAccess.FileExists(path))
        {
            GD.PrintErr($"地图文件不存在：{path}");
            return null;
        }

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        string json = file.GetAsText();

        var mapData = JsonSerializer.Deserialize<MapExportData>(json);

        if (mapData == null)
        {
            GD.PrintErr("解析 JSON 失败！");
            return null;
        }

        var map = new Map(mapData.Width, mapData.Height)
        {
            Width = mapData.Width,
            Height = mapData.Height,
            Grid = new Grid[mapData.Width, mapData.Height]
        };

        for (int y = 0; y < mapData.Height; y++)
        {
            for (int x = 0; x < mapData.Width; x++)
            {
                int baseId = mapData.Base[y][x];
                int standId = mapData.Stand[y][x];

                string tBase = idToTerrain.TryGetValue(baseId, out var b) ? b : "None";
                string tStand = idToTerrain.TryGetValue(standId, out var s) ? s : "";

                map.Grid[x, y] = new Grid(new Vector2I(x, y), tBase, tStand);
            }
        }

        return map;
    }

    public class MapExportData
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int[][] Base { get; set; }
        public int[][] Stand { get; set; }
    }

}