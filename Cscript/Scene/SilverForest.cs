using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

public partial class SilverForest : Node, IMapNode
{
    public static Map Floor1 = new(60, 40);
    public static Map Floor2 = new(60, 60);
    public static Map Floor3 = new(60, 40);
    public static Map Floor4a = new(80, 30);
    public static Map Floor4b = new(80, 30);
    public static Map Floor5 = new(40, 120);
    public static Map Floor6 = new(60, 40);
    private static TaskCompletionSource clickTcs;
    public override void _Ready()
    {
        Fsm.StartState.OnEnd += Load;
        Fsm.StartState.OnEnd += Init;
        G.I.Fsm.ChangeState(Fsm.StartState);
        
    }
    public static bool inited = false;
    public void Init()
    {
        if (inited)
        {
            //return;
        }
        inited = true;
        Maps = [Floor1, Floor2, Floor3, Floor4a, Floor4b, Floor5, Floor6];
        CreateFloor1(Floor1);
        CreateFloor2(Floor2);
        CreateFloor3(Floor3);
        CreateFloor4(Floor4a);
        CreateFloor4(Floor4b);
        CreateFloor5(Floor5);
        CreateFloor6(Floor6);
        for (int ii = 0; ii < Maps.Count - 1; ii++)
        {
            Maps[ii].MapGoto = Maps[ii + 1];
        }
        Floor4a.AfterEnter += () => G.I.TileMapAllLayer.Background.Visible = true;
        Floor4a.AfterEnter += () => Player.PlayerUnit.Ue.OnUnitMove += MoveBg;
        Floor4b.AfterEnter += () => G.I.TileMapAllLayer.Background.Position = Vector2.Zero;
        Floor5.AfterEnter += () =>
        {
            G.I.TileMapAllLayer.Background.Visible = false;
            //Player.PlayerUnit.Ue.OnUnitMove -= MoveBg;
        };
        Scene.Enter(Floor1);
        var i = Item.CreateItem("MagicPotion", new Dictionary<string, object> { { "MpRecoverPercent", 30 } });
        ItemEffect.CreateItemEffect("AddMaxHp").ApplyItemEffect(i);
        Player.PlayerUnit.Inventory.AddItem(i);
        Unit.OnPlayerdied += Playerdied;
        static void MoveBg(Unit unit)
        {
            G.I.TileMapAllLayer.Background.Position = new Vector2(-25 * Player.PlayerUnit.Up.Position.X, 0);
        }
        static void CreateFloor1(Map floor)
        {
            foreach (var grid in floor.Grid)
                grid.TerrainBaseGround = "Snow";
            MapGenerator.ChangeMapByWeight(LogicMapLayer.Stand,
                new Dictionary<string, float>
                {
                { "SnowTree", 0.45f },
                { "", 0.55f }
                }, floor);
            MapGenerator.ChangeMapByEnvolve(LogicMapLayer.Stand, "SnowTree", floor, 2);
            foreach (var grid in floor.Grid)
            {
                if (GD.Randf() < 0.004f && grid.TerrainStand == "")
                {
                    grid.TerrainStand = "SnowMan";
                }
            }
            MapGenerator.ChangeMapByRoad(LogicMapLayer.BaseGround, "Grass", floor, out int y, out int ey);
            floor.Entrance = new Vector2I(0, y);
            floor.SetExit(floor.GetGrid(new Vector2I(59, ey)));
        }
        static void CreateFloor2(Map floor)
        {
            MapGenerator.ChangeMapByWeight(LogicMapLayer.Stand,
                new Dictionary<string, float>
                {
                { "SnowTree", 0.45f },
                { "", 0.55f }
                }, floor);
            MapGenerator.ChangeMapByEnvolve(LogicMapLayer.Stand, "", floor, 2);
            MapGenerator.ChangeMapByWeight(LogicMapLayer.BaseGround,
                new Dictionary<string, float>
                {
                { "Grass", 0.5f },
                { "Snow", 0.5f }
                }, floor);
            MapGenerator.ChangeMapByEnvolve(LogicMapLayer.BaseGround, "Grass", floor, 1);
            MapGenerator.ChangeMapByPutRoom(LogicMapLayer.BaseGround, floor,
                6, 5, "WoodWall", "WoodFloor");
            MapGenerator.ChangeMapByPutRect(LogicMapLayer.BaseGround, floor,
                4, 8, "Field");

            foreach (var grid in floor.Grid)
            {
                if (grid.TerrainBaseGround != "Grass" && grid.TerrainBaseGround != "Snow" && grid.TerrainStand != "DoorClosed")
                {
                    grid.TerrainStand = "";
                }
                if (GD.Randf() < 0.002f && grid.TerrainBaseGround == "Snow" && grid.TerrainStand == "")
                {
                    grid.TerrainStand = "SnowMan";
                }
            }
            floor.Entrance = new Vector2I(0, GD.RandRange(10, 50));
            floor.SetExit(floor.GetGrid(new Vector2I(59, GD.RandRange(10, 50))));
        }
        static void CreateFloor3(Map floor)
        {
            MapGenerator.ChangeMapByWeight(LogicMapLayer.BaseGround,
                new Dictionary<string, float>
                {
                { "Grass", 1f },
                }, floor);
            MapGenerator.ChangeMapByWeight(LogicMapLayer.Stand,
                new Dictionary<string, float>
                {
                { "Forest", 0.5f },
                { "", 0.5f }
                }, floor);
            MapGenerator.ChangeMapByEnvolve(LogicMapLayer.Stand, "Forest", floor, 2);
            MapGenerator.ChangeMapByRoad(LogicMapLayer.BaseGround, "Grass", floor, out int y, out int ey);
            floor.Entrance = new Vector2I(0, y);
            floor.SetExit(floor.GetGrid(new Vector2I(59, ey)));
        }
        static void CreateFloor4(Map floor)
        {
            foreach (var grid in floor.Grid)
                grid.TerrainBaseGround = "Sky";
            MapGenerator.ChangeMapByPutRect(LogicMapLayer.BaseGround, floor,
                8, 6, "Cloud");
            floor.Entrance = new Vector2I(0, GD.RandRange(10, 20));
            floor.SetExit(floor.GetGrid(new Vector2I(79, GD.RandRange(10, 20))));
        }
        static void CreateFloor5(Map floor)
        {
            foreach (var grid in floor.Grid)
                grid.TerrainBaseGround = "Wall";
            MapGenerator.ChangeMapByRoadEx(LogicMapLayer.BaseGround, "WideStage", floor, MapGenerator.RoadDirection.BottomToTop,
                20, out Vector2I start, out Vector2I end);
            floor.Entrance = start;
            floor.SetExit(floor.GetGrid(end));
        }
        static void CreateFloor6(Map floor)
        {
            foreach (var grid in floor.Grid)
                grid.TerrainBaseGround = "CherryFloor";
            MapGenerator.ChangeMapByWeight(LogicMapLayer.Stand,
                new Dictionary<string, float>
                {
                { "CherryTree", 0.45f },
                { "", 0.55f }
                }, floor);
            MapGenerator.ChangeMapByEnvolve(LogicMapLayer.Stand, "CherryTree", floor, 2);
            MapGenerator.ChangeMapByRoad(LogicMapLayer.BaseGround, "CherryFloor", floor, out int y, out int ey);
            floor.Entrance = new Vector2I(0, y);
        }
    }
    private static void Playerdied()
    {
        DialogBox.SShow();
        G.I.DialogBox.ShowDialog([
            new("", LoadPortrait("null"),
            "游戏失败。")
        ]);
    }

    public static void CreateMidboss()
    {

    }
    public void Createboss()
    {

    }
    public static bool loaded = false;

    public List<Map> Maps { get; set; }

    public async void Load()
    {
        if (loaded)
        {
            //return;
        }
        loaded = true;
        string player = GameData.SelectedCharacter;
        // 场景资源加载
        Player.Init(player, 1f);
        G.I.SkillPanel.Refresh();
        switch (player)
        {
            case "reimu":
                G.I.SkillPanel.Add("YinyangBall");
                break;
            case "marisa":
                G.I.SkillPanel.Add("Star");
                break;
            case "rumia":
                G.I.SkillPanel.Add("Dark");
                break;
            case "cirno":
                G.I.SkillPanel.Add("Freeze");
                break;
            case "meilin":
                G.I.SkillPanel.Add("Fist");
                break;
        }
        Player.PlayerUnit.Ua.skillPoint += 6;
        Player.PlayerUnit.Ua.uaPoint += 5;
        Player.PlayerUnit.Memorys.MaxEquipWeight = 5;
        Player.PlayerUnit.Inventory.AddItem(Item.CreateItem("HealPotion"));
        Floor1.SummonChest(8, 3);
        await ToSignal(GetTree().CreateTimer(0.1), "timeout");
        G.I.Fsm.ChangeState(Fsm.TalkState);
        G.I.DialogBox.Show();
        G.I.DialogBox.ShowDialog([
            new("任务目标：", LoadPortrait("null"),
            "查明雾之湖发生的变化。")
        ]); await Click();
        G.I.Fsm.ChangeState(Fsm.UpdateState);
        G.I.DialogBox.Hide();
        
    }
    
    public async void Victory()
    {

    }

    public static async Task Click()
    {
        clickTcs = new TaskCompletionSource();
        await clickTcs.Task;
    }
    private static Texture2D LoadPortrait(string name)
    {
        return GD.Load<Texture2D>($"res://Assets/Portraits/{name}.png");
    }
    public override void _UnhandledInput(InputEvent @event)
    {
        if (clickTcs != null && @event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            clickTcs.TrySetResult();
        }
    }
    public static Texture2D L(string name)
    {
        return GD.Load<Texture2D>($"res://Assets/Portraits/{name}.png");
    }
    public async Task ShowDialogSequence(params DialogLine[] lines)
    {
        foreach (var line in lines)
        {
            G.I.DialogBox.ShowDialog([line]);
            await Click();
        }
    }
}
