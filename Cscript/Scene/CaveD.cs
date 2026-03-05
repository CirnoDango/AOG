using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class CaveD : Node
{

    public Map Floor1 = MapGenerator.GenerateMapByCellularAutomata(
    30, 30,
    new Dictionary<string, float>
    {
        { "Stone", 0.05f },
        { "Floor", 0.95f }
    }, 2);
    public Map Floor2 = MapGenerator.GenerateMapByCellularAutomata(
    60, 60,
    new Dictionary<string, float>
    {
        { "Stone", 0.45f },
        { "Floor", 0.55f }
    }, 2);
    public Map Floor3 = MapGenerator.GenerateMapByCellularAutomata(
    60, 60,
    new Dictionary<string, float>
    {
        { "Stone", 0.45f },
        { "Floor", 0.55f }
    }, 2, true);
    private TaskCompletionSource clickTcs;

    public override void _Ready()
    {
        Floor1.EnemySummonValue = new Dictionary<string, float>
        {
            { "dangoPea", 0f },
        };
        Floor2.EnemySummonValue = new Dictionary<string, float>
        {
            { "dangoPea", 0.5f },
            { "dangoWater", 0.4f },
            { "dangoFist", 0.15f },
        };
        Floor3.EnemySummonValue = new Dictionary<string, float>
        {
            { "dangoPea", 0.5f },
            { "dangoWater", 0.5f },
            { "dangoFist", 0.2f },
        };
        Fsm.StartState.OnEnd += Load;
        G.I.Fsm.ChangeState(Fsm.StartState);
        Floor3.AfterEnter += Createboss;
    }
    public void Createboss()
    {
        var boss = Floor3.CreateEnemy(MapGenerator.FloodFindFarthest(Floor3, Floor3.Entrance), "rumia");
        boss.Inventory.AddItem(Item.CreateItem("DangoLight"));
        boss.Ue.OnEnemyKilled += enemy =>
        {
            if (enemy == boss)
            {
                Victory();
            }
        };
    }
    public void Load()
    {
        string player = GameData.SelectedCharacter;
        // 场景资源加载
        Player.Init(player, 1);
        G.I.Player.TalentPoint = 3;
        Player.PlayerUnit.Us.LearnSkillGroup("Yinyangball");
        Player.PlayerUnit.Us.LearnSkillGroup("Dark");
        Player.PlayerUnit.Us.LearnSkillGroup("Star");
        Player.PlayerUnit.Us.LearnSkillGroup("Freeze");
        Player.PlayerUnit.Us.LearnSkillGroup("Fist");
        Player.PlayerUnit.Us.LearnSkillGroup("Circle");
        Player.PlayerUnit.Us.LearnSkillGroup("FireElement");
        Player.PlayerUnit.Us.LearnSkillGroup("Time");
        Player.PlayerUnit.Us.LearnSkillGroup("Blood");
        G.I.SkillPanel.Refresh();
        G.I.SkillTreeBox.Expert(0, 0);
        Player.PlayerUnit.Inventory.AddItem(Item.CreateItem("mSkill"));
        Player.PlayerUnit.Inventory.AddItem(Item.CreateItem("mUaAny"));
        Player.PlayerUnit.Inventory.AddItem(Item.CreateItem("mUaCun"));
        Floor1.MapGoto = Floor3;
        Floor2.MapGoto = Floor3;
        Floor1.SummonChest(200, 1);
        Floor2.SummonChest(20, 100);
        Floor1.Entrance = new Vector2I(5, 7);
        Scene.Enter(Floor1);
        Player.PlayerUnit.Inventory.AddItem(
            Item.CreateItem("MagicPotion", new Dictionary<string, object> { { "MpRecoverPercent", 60 } }));
        Player.PlayerUnit.Ua.Cun += 10;
        Player.PlayerUnit.Inventory.AddItem(Item.CreateItem("BarrageSet",
            new Dictionary<string, object> { { "barrage", new Dictionary<string, object> { { "MaxComponents", 3 } } } }));
        Player.PlayerUnit.Inventory.AddItem(Item.CreateItem("BarrageSet",
            new Dictionary<string, object> { { "barrage", new Dictionary<string, object> { { "MaxComponents", 5 } } } }));
        Player.PlayerUnit.Inventory.AddItem(Item.CreateItem("BarrageSet",
            new Dictionary<string, object> { { "barrage", new Dictionary<string, object> { { "MaxComponents", 6 } } } }));
        foreach(var i in Item.ItemDeck.Where(x=>x is BarrageComponent && x is not BulletModule))
            Player.PlayerUnit.Inventory.AddItem(Item.CreateItem(i.Name));
        Floor1.CreateEnemy(new Vector2I(2, 4), "rumia", UnitEgo.great);
        //Floor1.CreateEnemy(new Vector2I(2, 5), "cirno", UnitEgo.great);
        //Floor1.CreateEnemy(new Vector2I(2, 6), "meiling", UnitEgo.boss);
        //Floor1.CreateEnemy(new Vector2I(2, 7), "patchouli", UnitEgo.boss);
        //Floor1.CreateEnemy(new Vector2I(2, 8), "sakuya", UnitEgo.boss);
        //Floor1.CreateEnemy(new Vector2I(2, 9), "remilia", UnitEgo.eliteBoss);
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
