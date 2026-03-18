using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
public enum LogicMapLayer
{
    BaseGround,Stand,FogOfWar
}
public static class MapBuilder
{

    // 单独定义在类中（或作为静态工具函数）

    /// <summary>
    /// Builds a logical representation of the map based on the tile map layers.
    /// </summary>
    /// <remarks>This method processes the base ground and stand layers of the tile map to construct a <see
    /// cref="Map"/> object. It extracts terrain information from the tiles and populates a grid structure within the
    /// map. The method assumes that the base ground layer contains mandatory terrain data, while the stand layer
    /// provides optional terrain data.</remarks>
    /// <returns>A <see cref="Map"/> object representing the logical structure of the tile map, including terrain information for
    /// each grid cell.</returns>
    public static Map BuildLogicFromTileMap(Map map)
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

        map.Width = width;
        map.Height = height;
        map.Grid = new Grid[width, height];

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
        Dictionary<int, List<Vector2I>> groundGroups = [];
        Dictionary<int, List<Vector2I>> standGroups = [];
        Dictionary<string, int> groundTerrains = [];
        Dictionary<string, int> standTerrains = [];
        groundTerrains.Add("", -1);
        standTerrains.Add("", -1);
        int n = G.I.TileMapAllLayer.BaseGround.TileSet.GetTerrainsCount(0);
        for (int i = 0; i < n; i++)
        {
            var name = G.I.TileMapAllLayer.BaseGround.TileSet.GetTerrainName(0, i);
            groundTerrains.Add(name, i);
        }
        int m = G.I.TileMapAllLayer.Stand.TileSet.GetTerrainsCount(0);
        for (int i = 0; i < m; i++)
        {
            var name = G.I.TileMapAllLayer.Stand.TileSet.GetTerrainName(0, i);
            standTerrains.Add(name, i);
        }
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                var grid = map.Grid[x, y];
                var pos = new Vector2I(x, y);
                if (!groundGroups.TryGetValue(groundTerrains[grid.TerrainBaseGround], out var groundList))
                {
                    groundList = [];
                    groundGroups[groundTerrains[grid.TerrainBaseGround]] = groundList;
                }
                groundList.Add(pos);
                if (!standGroups.TryGetValue(standTerrains[grid.TerrainStand], out var standList))
                {
                    standList = [];
                    standGroups[standTerrains[grid.TerrainStand]] = standList;
                }
                standList.Add(pos);
            }
        }
        foreach (var kv in groundGroups)
        {
            int terrain = kv.Key;
            var cells = kv.Value;
            G.I.TileMapAllLayer.BaseGround.SetCellsTerrainConnect([.. cells], 0, terrain);
        }

        foreach (var kv in standGroups)
        {
            int terrain = kv.Key;
            var cells = kv.Value;
            G.I.TileMapAllLayer.Stand.SetCellsTerrainConnect([.. cells], 0, terrain);
        }
    }

    /// <summary>
    /// Sets the terrain type for the specified logic map layer at the given grid position.
    /// </summary>
    /// <remarks>This method updates both the logical representation of the terrain in the specified grid cell
    /// and the corresponding visual tile map layer.</remarks>
    /// <param name="tileMap">The logic map layer to update. Must be one of the defined <see cref="LogicMapLayer"/> values.</param>
    /// <param name="grid">The grid cell where the terrain type will be set. Cannot be <see langword="null"/>.</param>
    /// <param name="terrainName">The name of the terrain to assign to the specified layer. Cannot be <see langword="null"/> or empty.</param>
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
    /// <summary>
    /// Updates the fog of war for the specified map by marking all tiles as obscured.
    /// </summary>
    /// <remarks>This method clears any existing fog of war data and sets all tiles in the map to a default
    /// "Fog" state. It is typically used to initialize or reset the fog of war layer for a map.</remarks>
    /// <param name="map">The map to apply the fog of war to. Must not be null.</param>
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
    /// <summary>
    /// Sets the terrain type for a specific tile in the tile map.
    /// </summary>
    /// <remarks>This method searches the tile map's tile set for a terrain with the specified name and
    /// applies it to the tile at the given position. If the terrain name does not exist in the tile set, no changes are
    /// made.</remarks>
    /// <param name="tileMap">The tile map layer where the terrain will be set.</param>
    /// <param name="pos">The position of the tile within the tile map, specified as a <see cref="Vector2I"/>.</param>
    /// <param name="terrainName">The name of the terrain to assign to the specified tile.</param>
    public static void SetTileMapTerrain(TileMapLayer tileMap, Vector2I pos, string terrainName)
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
}