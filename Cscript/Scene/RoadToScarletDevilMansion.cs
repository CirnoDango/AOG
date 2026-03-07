using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

public partial class RoadToScarletDevilMansion : Node
{
    public static Map Floor1 = new(60, 60);
    public static Map Floor2 = new(60, 60);
    public static Map Floor3 = new(60, 60);
    public static Map doorOfScarletMansion = new("doorOfScarletMansion");
    private static TaskCompletionSource clickTcs;
    public override void _Ready()
    {
        Floor1.EnemySummonValue = new Dictionary<string, float>
        {
            { "dangoPea",       0f },
        };
        Floor2.EnemySummonValue = new Dictionary<string, float>
        {
            { "dangoPea",       0.1f },
            { "dangoWater",     0.3f },
            { "dangoFist",      0.1f },
            { "dangoDark",      0.3f },
            { "dangoIce",       0.3f },
            { "dangoTwinpea",   0.1f },
            { "dangoRedBean",   0.1f },
            { "dangoSpellcard", 0.1f },
            { "dangoLight",     0.1f },
        };
        Floor3.EnemySummonValue = new Dictionary<string, float>
        {
            { "dangoPea",       0.1f },
            { "dangoWater",     0.3f },
            { "dangoFist",      0.1f },
            { "dangoDark",      0.1f },
            { "dangoIce",       0.3f },
            { "dangoTwinpea",   0.2f },
            { "dangoRedBean",   0.2f },
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
            return;
        }
        inited = true;
        CreateFloor(Floor1);
        CreateFloor(Floor2);
        CreateFloor(Floor3);
        Floor1.MapGoto = Floor2;
        Floor2.MapGoto = Floor3;
        Floor3.MapGoto = doorOfScarletMansion;
        doorOfScarletMansion.Entrance = new Vector2I(18, 42);
        Scene.Enter(doorOfScarletMansion);
        var i = Item.CreateItem("MagicPotion", new Dictionary<string, object> { { "MpRecoverPercent", 30 } });
        ItemEffect.CreateItemEffect("AddMaxHp").ApplyItemEffect(i);
        Player.PlayerUnit.Inventory.AddItem(i);
        Unit.OnPlayerdied += Playerdied;
        Floor2.AfterEnter += CreateMidboss;
        doorOfScarletMansion.AfterEnter += Createboss;
        static void CreateFloor(Map floor)
        {
            MapGenerator.ChangeMapByRegionGrow(floor, LogicMapLayer.BaseGround, LogicMapLayer.BaseGround,
                        "Grass", "Water", 8, 1, 70, 15);
            MapGenerator.ChangeMapByEnvolve(LogicMapLayer.BaseGround, "Grass", floor, 1);
            MapGenerator.ChangeMapByWeight(LogicMapLayer.Stand,
                new Dictionary<string, float>
                {
                { "Forest", 0.4f },
                { "", 0.6f }
                }, floor);
            MapGenerator.ChangeMapByEnvolve(LogicMapLayer.Stand, "Forest", floor, 2);
            foreach (var grid in floor.Grid)
            {
                if (grid.TerrainBaseGround == "Water" && grid.TerrainStand == "Forest")
                {
                    grid.TerrainStand = "";
                }
            }
            MapGenerator.ChangeMapByPutRect(LogicMapLayer.BaseGround, floor, 10, 7, "Wall", "Floor");
            MapGenerator.ChangeMapByRoad(LogicMapLayer.BaseGround, "Road", floor, out int y, out int ey);
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
        Floor2.CreateEnemy(Floor2.Exit, "rumia", UnitEgo.great);
    }
    public void Createboss()
    {
        var boss = doorOfScarletMansion.CreateEnemy(new Vector2I(20, 32), "cirno", UnitEgo.boss);
        boss.Ue.OnKilled += enemy =>
        {
            Victory();
        };
    }
    public static bool loaded = false;
    public async void Load()
    {
        if (loaded)
        {
            return;
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
                G.I.SkillPanel.Add("Circle");
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
        Floor2.SummonChest(8, 3);
        Floor3.SummonChest(8, 3);
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
        G.I.Fsm.ChangeState(Fsm.TalkState);
        G.I.DialogBox.Show();
        await ShowDialogSequence(
            new("琪露诺", L("cirno_fight_cry"),
                "这儿……不是雾之湖？雾气、青蛙、大酱……都不在……？"),
            new("我……想回幻想乡……想回到湖边和大酱一起玩……"),
            new("呐，你知道回去的办法吗？我不想一个人待在这里……"),
            new("等我回去以后，一定要在湖面上做出更漂亮的冰雕……嗯！一定要……"),
            new("", L("null"),
                "恭喜获得游戏胜利!v0.3版本的内容就到此为止了,敬请期待下次更新。")
        );
        G.I.Fsm.ChangeState(Fsm.UpdateState);
        G.I.DialogBox.Hide();
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
            //clickTcs = null;
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
