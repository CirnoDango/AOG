using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

public partial class SilverForest : Node
{
    public static Map Floor1 = new(60, 40);

    private static TaskCompletionSource clickTcs;
    public override void _Ready()
    {
        Floor1.EnemySummonValue = new Dictionary<string, float>
        {
            { "dangoPea",       0.1f },
            { "dangoWater",     0.3f },
            { "dangoFist",      0.1f },
            { "dangoDark",      0.2f },
            { "dangoIce",       0.3f },
            { "dangoTwinpea",   0.1f },
            { "dangoRedBean",   0.1f },
            { "dangoSpellcard", 0.1f },
            { "dangoLight",     0.1f },
        };
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
        CreateFloor(Floor1);
        Scene.Enter(Floor1);
        var i = Item.CreateItem("MagicPotion", new Dictionary<string, object> { { "MpRecoverPercent", 30 } });
        ItemEffect.CreateItemEffect("AddMaxHp").ApplyItemEffect(i);
        Player.PlayerUnit.Inventory.AddItem(i);
        Unit.OnPlayerdied += Playerdied;
        static void CreateFloor(Map floor)
        {
            foreach(var grid in floor.Grid)
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
