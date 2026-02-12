using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

public partial class Eosd : Node
{
    public List<Map> Maps = [];
    public static Map Stage1Mid = new(60, 40);
    public static Map Stage1Fin = new(60, 40);
    public static Map Stage2Mid = new(60, 40);
    public static Map Stage2Fin = new("diShuiLake");
    public static Map Stage3Mid = new(60, 60);
    public static Map Stage3Fin = new("doorOfScarletMansion");
    public static Map Stage4Mid = new(100, 100);
    public static Map Stage4Fin = new(100, 100);
    public static Map Stage5Mid = new(100, 100);
    public static Map Stage5Fin = new(100, 100);
    public static Map Stage6Mid = new(100, 100);
    public static Map Stage6Fin = new(100, 100);
    private static TaskCompletionSource clickTcs;
    public override void _Ready()
    {
        MapPrebuilder.LoadMapsFromFolder("res://Nodes/Prebuild/ScarletMansion");
        CreateMansionFloor(ref Stage4Mid);
        CreateMansionFloor(ref Stage4Fin);
        CreateMansionFloor(ref Stage5Mid);
        CreateMansionFloor(ref Stage5Fin);
        CreateMansionFloor(ref Stage6Mid);
        CreateMansionFloor(ref Stage6Fin);
        List<string> s123 =                           ["dangoPea", "dangoWater", "dangoFist", "dangoDark", "dangoIce", "dangoTwinpea", "dangoRedBean", "dangoSpellcard", "dangoLight"];
        Stage1Mid.EnemySummonValue = EnemySummonValue(s123,[0.20,         0.20,        0.10,        0.10,       0.05,           0.05,           0.05,             0.05,         0.05]);
        Stage1Fin.EnemySummonValue = EnemySummonValue(s123,[0.20,         0.20,        0.10,        0.30,       0.05,           0.05,           0.05,             0.05,         0.05]);
        Stage2Mid.EnemySummonValue = EnemySummonValue(s123,[0.20,         0.20,        0.10,        0.10,       0.15,           0.05,           0.05,             0.10,         0.10]);
        Stage2Fin.EnemySummonValue = EnemySummonValue(s123,[0.20,         0.20,        0.10,        0.10,       0.40,           0.05,           0.05,             0.10,         0.10]);
        Stage3Mid.EnemySummonValue = EnemySummonValue(s123,[0.20,         0.20,        0.10,        0.10,       0.10,           0.10,           0.10,             0.10,         0.10]);
        Stage3Fin.EnemySummonValue = EnemySummonValue(s123,[0.20,         0.20,        0.10,        0.10,       0.10,           0.20,           0.20,             0.10,         0.10]);
        List<string> s456 =                           ["dangoPea", "dangoFist", "dangoDark", "dangoTwinpea", "dangoSpellcard", "dangoFire", "dangoClock", "dangoRedwine"];
        Stage4Mid.EnemySummonValue = EnemySummonValue(s456,[0.10,        0.10,        0.10,           0.10,             0.10,        0.20,         0.10,           0.10]);
        Stage4Fin.EnemySummonValue = EnemySummonValue(s456,[0.10,        0.10,        0.10,           0.10,             0.10,        0.30,         0.15,           0.10]);
        Stage5Mid.EnemySummonValue = EnemySummonValue(s456,[0.10,        0.10,        0.10,           0.10,             0.10,        0.20,         0.20,           0.10]);
        Stage5Fin.EnemySummonValue = EnemySummonValue(s456,[0.10,        0.10,        0.10,           0.10,             0.10,        0.10,         0.30,           0.15]);
        Stage6Mid.EnemySummonValue = EnemySummonValue(s456,[0.10,        0.10,        0.10,           0.15,             0.15,        0.10,         0.20,           0.20]);
        Stage6Fin.EnemySummonValue = EnemySummonValue(s456,[0.10,        0.10,        0.10,           0.20,             0.20,        0.20,         0.20,           0.30]);
        Fsm.StartState.OnEnd += Load;
        Fsm.StartState.OnEnd += Init;
        G.I.Fsm.ChangeState(Fsm.StartState);

        static void CreateMansionFloor(ref Map map)
        {
            map = MapPrebuild.Generate(100, 100, MapPrebuilder.GetMapPrebuild("Scarlet4dCorner"), new Vector2I(50, 50),
                        10, "ScarletWall", MapPrebuilder.MapPrebuildDeck);
            map.Entrance = new Vector2I(55, 55);
            if(map == Stage6Mid)
            {
                map.FindExit();
                return;
            } 
            map.SetExit();
        }

        static Dictionary<string, float> EnemySummonValue(List<string> names, List<double> values)
        {
            Dictionary<string, float> keyValuePairs = [];
            for (int i = 0; i < names.Count; i++)
            {
                keyValuePairs.Add(names[i], (float)values[i]);
            }
            return keyValuePairs;
        }
    }
    public static bool inited = false;
    public void Init()
    {
        if (inited)
        {
            return;
        }
        inited = true;

        Maps = [Stage1Mid, Stage1Fin, Stage2Mid, Stage2Fin, Stage3Mid, Stage3Fin,
                Stage4Fin, Stage5Fin, Stage6Fin];
        CreateWildFloor(Stage1Mid);
        CreateWildFloor(Stage1Fin);
        CreateWildFloor(Stage2Mid);
        CreateWildFloor(Stage3Mid);
        for (int ii = 0; ii < Maps.Count - 1; ii++)
        {
            Maps[ii].MapGoto = Maps[ii + 1];
        }
        Stage2Fin.Entrance = new Vector2I(0, 32);
        Stage3Fin.Entrance = new Vector2I(18, 42);
        Stage1Fin.AfterEnter += CreateRumia;
        Stage2Fin.AfterEnter += CreateCirno;
        Stage3Fin.AfterEnter += CreateMeiling;
        Stage4Fin.AfterEnter += CreatePatchouli;
        Stage5Fin.AfterEnter += CreateSakuya;
        Stage6Fin.AfterEnter += CreateRemilia;
        Scene.Enter(Stage1Mid);

        var i = Item.CreateItem("MagicPotion", new Dictionary<string, object> { { "MpRecoverPercent", 30 } });
        Player.PlayerUnit.Inventory.AddItem(i);
        Player.PlayerUnit.Inventory.AddItem(Item.CreateItem("BarrageSet",
            new Dictionary<string, object> { { "barrage", new Dictionary<string, object> { { "MaxComponents", 2 } } } }));
        Unit.OnPlayerdied += Playerdied;
        
        static void CreateWildFloor(Map floor)
        {
            MapGenerator.ChangeMapByRegionGrow(floor, LogicMapLayer.BaseGround, LogicMapLayer.BaseGround,
                        "Grass", "Water", 15, 1, 100, 15);
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

    public static void CreateRumia()
    {
        Stage1Fin.CreateEnemy(Stage1Fin.Exit, "rumia", UnitEgo.great);
    }
    public void CreateCirno()
    {
        Stage2Fin.CreateEnemy(new Vector2I(20, 32), "cirno", UnitEgo.great);
    }
    public void CreateMeiling()
    {
        Stage3Fin.CreateEnemy(new Vector2I(20, 1), "meiling", UnitEgo.boss);
    }
    public void CreatePatchouli()
    {
        Stage4Fin.CreateEnemy(Stage4Fin.Exit, "patchouli", UnitEgo.boss);
    }
    public void CreateSakuya()
    {
        Stage5Fin.CreateEnemy(Stage5Fin.Exit, "sakuya", UnitEgo.boss);
    }
    public void CreateRemilia()
    {
        Stage6Fin.CreateEnemy(Stage6Fin.Exit, "remilia", UnitEgo.eliteBoss);
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
        Player.Init(player, 0.8f);
        G.I.SkillPanel.Refresh();
        switch (player)
        {
            case "reimu":
                G.I.SkillTreeBox.Expert("YinyangBall");
                break;
            case "marisa":
                G.I.SkillTreeBox.Expert("Star");
                break;
            case "rumia":
                G.I.SkillTreeBox.Expert("Dark");
                break;
            case "cirno":
                G.I.SkillTreeBox.Expert("Freeze");
                break;
            case "meiling":
                G.I.SkillTreeBox.Expert("Fist");
                break;
            case "patchouli":
                G.I.SkillTreeBox.Expert("FireElement");
                break;
            case "sakuya":
                G.I.SkillTreeBox.Expert("Time");
                break;
            case "remilia":
                G.I.SkillTreeBox.Expert("Blood");
                break;
        }
        Player.PlayerUnit.Ua.skillPoint += 6;
        Player.PlayerUnit.Ua.uaPoint += 5;
        Player.PlayerUnit.Memorys.MaxEquipWeight = 10;
        Player.PlayerUnit.Inventory.AddItem(Item.CreateItem("HealPotion"));
        Stage1Mid.SummonChest(9, 4);
        Stage2Mid.SummonChest(9, 4);
        Stage3Mid.SummonChest(9, 4);
        Stage4Mid.SummonChest(12, 4);
        Stage5Mid.SummonChest(12, 4);
        Stage6Mid.SummonChest(12, 4);
        Stage1Fin.SummonChest(9, 4);
        Stage4Fin.SummonChest(12, 4);
        Stage5Fin.SummonChest(12, 4);
        Stage6Fin.SummonChest(12, 4);
        await ToSignal(GetTree().CreateTimer(0.1), "timeout");
        G.I.Fsm.ChangeState(Fsm.TalkState);
        G.I.DialogBox.Show();
        G.I.DialogBox.ShowDialog([
            new("任务目标：", LoadPortrait("null"),
            "击败蕾米莉亚。")
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
