using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class MistyLake : Node
{
    public static Map Floor1 = new(60, 60);
    private TaskCompletionSource clickTcs;
    public static void Init()
    {
        MapGenerator.ChangeMapByRegionGrow(Floor1, LogicMapLayer.BaseGround, LogicMapLayer.BaseGround,
            "Grass", "Water", 15, 1, 100, 15);
        MapGenerator.ChangeMapByEnvolve(LogicMapLayer.BaseGround, "Grass", Floor1, 1);
        MapGenerator.ChangeMapByWeight(LogicMapLayer.Stand,
            new Dictionary<string, float>
            {
                { "Forest", 0.4f },
                { "", 0.6f }
            }, Floor1);
        MapGenerator.ChangeMapByEnvolve(LogicMapLayer.Stand, "Forest", Floor1, 2);
        foreach (var grid in Floor1.Grid)
        {
            if (grid.TerrainBaseGround == "Water" && grid.TerrainStand == "Forest")
            {
                grid.TerrainStand = "";
            }
        }
        MapGenerator.ChangeMapByRoad(LogicMapLayer.BaseGround, "Road", Floor1, out int y);
        Floor1.Entrance = new Vector2I(0, y);
        Scene.Enter(Floor1);
        Unit.OnPlayerdied += Playerdied;
    }

    private static void Playerdied()
    {
        DialogBox.SShow();
        G.I.DialogBox.ShowDialog([
            new("", LoadPortrait("null"),
            "游戏失败。")
        ]);
    }

    public override void _Ready()
    {
        Floor1.EnemySummonValue = new Dictionary<string, float>
        {
            { "dangoPea",       1f },
            { "dangoWater",     1f },
            { "dangoFist",      1f },
            { "dangoDark",      1f },
            { "dangoIce",       1f },
            { "dangoTwinpea",   1f },
            { "dangoRedBean",   1f },
            { "dangoSpellcard", 1f },
            { "dangoLight",     1f },
        };
        Fsm.StartState.OnEnd += Load;
        Fsm.StartState.OnEnd += Init;
        G.I.Fsm.ChangeState(Fsm.StartState);
        //Floor3.AfterEnter += Createboss;
    }
    public void Createboss()
    {
        //var boss = Floor3.CreateEnemy(MapGenerator.FloodFindFarthest(Floor3, Floor3.Entrance), "rumia");
        //boss.inventory.AddItem(Item.GetItemName("DangoLight"));
        //GameEvents.OnEnemyKilled += enemy =>
        //{
        //    if (enemy == boss)
        //    {
        //        Victory();
        //    }
        //};
    }
    public async void Load()
    {
        string player = GameData.SelectedCharacter;
        // 场景资源加载
        Player.Init(player, 3);
        switch (player)
        {
            case "cirno":
                Player.PlayerUnit.LearnSkillGroup("YinyangBall");
                break;
            case "marisa":
                Player.PlayerUnit.LearnSkillGroup("Star");
                break;
            case "rumia":
                Player.PlayerUnit.LearnSkillGroup("Dark");
                break;
        }
        G.I.SkillPanel.Refresh();
        G.I.SkillPanel.Add("YinyangBall");
        G.I.SkillPanel.Add("Star");
        G.I.SkillPanel.Add("Freeze");
        G.I.SkillPanel.Add("Dark");
        Player.PlayerUnit.Ua.skillPoint += 99;
        await ToSignal(GetTree().CreateTimer(0.01), "timeout");
        G.I.Fsm.ChangeState(Fsm.TalkState);
        G.I.DialogBox.Show();
        G.I.DialogBox.ShowDialog([
            new("任务目标：", LoadPortrait("null"),
            "找到发光团子。")
        ]); await Click();
        G.I.Fsm.ChangeState(Fsm.UpdateState);
        G.I.DialogBox.Hide();
    }
    
    public async void Victory()
    {
        G.I.Fsm.ChangeState(Fsm.TalkState);
        G.I.DialogBox.Show();
        G.I.DialogBox.ShowDialog([
            new("露米娅", LoadPortrait("rumia_break_redface_cry"),
            "呜……好不容易才拿到的超级发光团子……")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("露米娅", LoadPortrait("rumia_break_redface_cry"),
            "唔……我就吃了一口……就一口啊……你要吃就拿去好了……")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("露米娅", LoadPortrait("rumia_break_redface_cry2"),
            "……不过说真的，那团子是会发光的耶，很难得看到这么香又这么亮的甜点，不吃怎么行嘛！")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("露米娅", LoadPortrait("rumia_break_redface_relax"),
            "被你打败了也没办法啦。下次再抢到好吃的，我可不会告诉你在哪哟～♪")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("", LoadPortrait("null"),
            "恭喜获得胜利！")
        ]); await Click();
        G.I.Fsm.ChangeState(Fsm.UpdateState);
        G.I.DialogBox.Hide();
    }

    public async Task Click()
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
}
