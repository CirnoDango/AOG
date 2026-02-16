using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
public interface IMapNode
{
    List<Map> Maps { get; set; }
}

public partial class Eosd : Node, IMapNode
{
    public List<Map> Maps { get; set; }
    public static Map Stage1Mid = new(60, 40);
    public static Map Stage1Fin = new(60, 40);
    public static Map Stage2Mid = new(60, 40);
    public static Map Stage2Fin = new("diShuiLake");
    public static Map Stage3Mid = new(60, 40);
    public static Map Stage3Fin = new("doorOfScarletMansion");
    public static Map Stage4Mid = new(70, 70);
    public static Map Stage4Fin = new(70, 70);
    public static Map Stage5Mid = new(70, 70);
    public static Map Stage5Fin = new(70, 70);
    public static Map Stage6Mid = new(70, 70);
    public static Map Stage6Fin = new(70, 70);
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
        if (GameData.LoadPath == "")
        {
            Fsm.StartState.OnEnd += Load;
            Fsm.StartState.OnEnd += Init;
        }
        else
            Fsm.StartState.OnEnd += LoadSave;
        GameData.CurrentScene = this;
        G.I.Fsm.ChangeState(Fsm.StartState);

        static void CreateMansionFloor(ref Map map)
        {
            map = MapPrebuild.Generate(70, 70, MapPrebuilder.GetMapPrebuild("Scarlet4dCorner"), new Vector2I(35, 35),
                        10, "ScarletWall", MapPrebuilder.MapPrebuildDeck);
            map.Entrance = new Vector2I(40, 40);
            if(map == Stage6Fin)
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
        GameEvents.OnMapEnter += AutoSave;
        Player.PlayerUnit.Inventory.AddItem(Item.CreateItem("HealPotion"));
        var i = Item.CreateItem("MagicPotion", new Dictionary<string, object> { { "MpRecoverPercent", 30f} });
        Player.PlayerUnit.Inventory.AddItem(i);
        Player.PlayerUnit.Inventory.AddItem(Item.CreateItem("BarrageSet",
            new Dictionary<string, object> { { "barrage", new Dictionary<string, object> { { "MaxComponents", 2 } } } }));
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
    public async void PlayerdiedRevive()
    {
        G.I.DialogBox.Show();
        G.I.Fsm.ChangeState(Fsm.TalkState);
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_hahaha"),
            "baka果然还是baka呢")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_hahaha"),
            "没办法，就让你投币复活一次吧")
        ]); await Click();
        Player.PlayerUnit.Ua.GetHp(200);
        G.I.Fsm.ChangeState(Fsm.UpdateState);
        G.I.DialogBox.Hide();
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
        Stage3Fin.CreateEnemy(new Vector2I(20, 4), "meiling", UnitEgo.boss);
    }
    public void CreatePatchouli()
    {
        Stage4Fin.CreateEnemy(Stage4Fin.Exit, "patchouli", UnitEgo.boss);
    }
    public void CreateSakuya()
    {
        Stage5Fin.CreateEnemy(Stage5Fin.Exit, "sakuya", UnitEgo.boss);
    }
    Unit finalBoss;
    public void CreateRemilia()
    {
        finalBoss = Stage6Fin.CreateEnemy(Stage6Fin.Exit, "remilia", UnitEgo.eliteBoss);
        GameEvents.OnEnemyKilled += CheckFinalBoss;
    }
    public void CheckFinalBoss(Unit unit)
    {
        if (finalBoss != null && unit == finalBoss)
            Victory();
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
            "进入红魔馆，击败蕾米莉亚。")
        ]); await Click();
        G.I.Fsm.ChangeState(Fsm.UpdateState);
        G.I.DialogBox.Hide();
        
    }
    public void LoadSave()
    {
        string path = GameData.LoadPath;
        Save save = Save.LoadSaveFile(path);
        string playern = save.PlayerData.Name;
        Save.LoadPlayerInit(save, 0.8f);
        G.I.SkillPanel.Refresh();
        Player.PlayerUnit.Ua.skillPoint = save.SkillPoint;
        Player.PlayerUnit.Ua.uaPoint = save.UaPoint;
        G.I.Player.TalentPoint = save.TalentPoint;
        Player.PlayerUnit.Memorys.MaxEquipWeight = save.MaxMemory - 5;
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
        Scene.Enter(Maps[save.StageNumber]);
        GameEvents.OnMapEnter += AutoSave;
        // 5.技能
        foreach (var s in save.PlayerData.Skills)
        {
            Player.PlayerUnit.Us.LearnSkill(Skill.SkillDeck.FirstOrDefault(x => x.Name == s.Key), true);
            Player.PlayerUnit.Us.skills.FirstOrDefault(x => x.skill.Name == s.Key).skill.Level = (int)s.Value;
        }
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
    public void AutoSave()
    {
        if (Scene.CurrentMap == null)
            return;
        Save save = new Save();
        save.CreateSaveFile("Eosd", Maps.IndexOf(Scene.CurrentMap), "sav.sav");
    }
    public async void Victory()
    {
        G.I.Fsm.ChangeState(Fsm.TalkState);
        G.I.DialogBox.Show();
        G.I.DialogBox.Show();
        G.I.DialogBox.ShowDialog([
            new("", LoadPortrait("null"),
            "恭喜解决红雾异变，获得游戏胜利!v0.4版本的内容就到此为止了,敬请期待下次更新。")
        ]); await Click();
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
