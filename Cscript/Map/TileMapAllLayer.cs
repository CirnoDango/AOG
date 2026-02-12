using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection.Emit;

public partial class TileMapAllLayer : Node2D, IRegisterToG
{
    public TileMapLayer BaseGround { get; set; }
    public TileMapLayer Stand { get; set; }
    public TileMapLayer FogOfWar { get; set; }
    [Export]
    public TileMapLayer oBaseGround { get; set; }
    [Export]
    public TileMapLayer oStand { get; set; }
    [Export]
    public TileMapLayer oFogOfWar { get; set; }
    [Export]
    public Control GeneralMap {  get; set; }
    public Control ImportMap {  get; set; }
    public override void _Ready()
    {
        Visible = true;
        BaseGround = oBaseGround;
        Stand = oStand;
        FogOfWar = oFogOfWar;
    }
    public Map InitImportMap(Map map)
    {
        GeneralMap.Visible = false;
        var path = $"res://Nodes/Map/{map.Name}.tscn";
        var packedScene = GD.Load<PackedScene>(path);
        ImportMap = (Control)packedScene.Instantiate();
        AddChild(ImportMap);
        BaseGround = (TileMapLayer)ImportMap.GetNode("BaseGround");
        Stand = (TileMapLayer)ImportMap.GetNode("Stand");
        ((TileMapLayer)ImportMap.GetNode("FogOfWar")).Visible = oFogOfWar.Visible;
        FogOfWar = (TileMapLayer)ImportMap.GetNode("FogOfWar");
        return MapBuilder.BuildLogicFromTileMap(map);
    }
    public void InitGeneralMap()
    {
        GeneralMap.Visible = true;
        if (ImportMap != null)
            ImportMap.Visible = false;
        BaseGround = oBaseGround;
        Stand = oStand;
        FogOfWar = oFogOfWar;
    }
    public void ClearMap()
    {
        GeneralMap.Visible = true;
        ImportMap.QueueFree();
        BaseGround = oBaseGround;
        Stand = oStand;
        FogOfWar = oFogOfWar;
    }
    public void RegisterToG(G g)
    {
        g.TileMapAllLayer = this;
        ((Control)GetNode("Control")).Visible = true;
    }
}