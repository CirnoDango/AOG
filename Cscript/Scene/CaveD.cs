using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class CaveD : Node
{

    public Map Floor1 = MapGenerator.GenerateMapByBSP(
    60, 60, "Stone", "Floor");
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
        Floor2.EnemySummonValue = new Dictionary<string, float>
        {
            { "dangoPea", 0.5f },
            { "dangoWater", 0.5f },
            { "dangoFist", 0.2f },
        };
        Fsm.startState.OnEnd += Load;
        G.I.Fsm.ChangeState(Fsm.startState);
        Floor3.AfterEnter += Createboss;
    }
    public void Createboss()
    {
        var boss = Floor3.CreateEnemy(MapGenerator.FloodFindFarthest(Floor3, Floor3.Entrance), "rumia");
        boss.inventory.AddItem(Item.GetItemName("DangoLight"));
        GameEvents.OnEnemyKilled += (Unit enemy) =>
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
        Player.Init(player, 3);
        Player.PlayerUnit.LearnSkillGroup("Freeze");
        Player.PlayerUnit.LearnSkillGroup("Dark");
        Player.PlayerUnit.LearnSkillGroup("Star");
        G.I.SkillPanel.Refresh();
        G.I.SkillPanel.Add("Star");
        G.I.SkillPanel.Add("Freeze");
        G.I.SkillPanel.Add("Dark");
        Player.PlayerUnit.inventory.AddItem(Item.GetItemName("mSkill"));
        Player.PlayerUnit.inventory.AddItem(Item.GetItemName("mUaAny"));
        Player.PlayerUnit.inventory.AddItem(Item.GetItemName("mUaCun"));
        Floor1.Exit = Floor2;
        Floor2.Exit = Floor3;
        Scene.Enter(Floor1);
        Floor1.SummonChest(10, 3);
        Player.PlayerUnit.inventory.AddItem(
            Item.GetItemName("MagicPotion", new Dictionary<string, object> { { "mp", 30 } }));
        Player.PlayerUnit.inventory.AddItem(
            Item.GetItemName("MagicPotion", new Dictionary<string, object> { { "mp", 60 } }));
    }
    
    public async void Victory()
    {
        G.I.Fsm.ChangeState(Fsm.talkState);
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
        G.I.Fsm.ChangeState(Fsm.updateState);
        G.I.DialogBox.Hide();
    }

    public async Task Click()
    {
        clickTcs = new TaskCompletionSource();
        await clickTcs.Task;
    }
    private static Texture2D LoadPortrait(string name)
    {
        return GD.Load<Texture2D>($"res://assets/Portraits/{name}.png");
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
