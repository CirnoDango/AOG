using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Cave : Node, IRegisterToG
{
     
    public Map Floor1 = MapGenerator.GenerateMapByCellularAutomata(
    60, 60,
    new Dictionary<string, float>
    {
        { "Stone", 0.4f },
        { "Floor", 0.6f }
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
    public static void Init()
    {
        G.I.Cave.Floor1.Exit = G.I.Cave.Floor2;
        G.I.Cave.Floor2.Exit = G.I.Cave.Floor3;
        Scene.Enter(G.I.Cave.Floor1);
        Unit.OnPlayerdied += Playerdied;
    }

    private async static void Playerdied()
    {
        DialogBox.SShow();
        G.I.DialogBox.ShowDialog([
            new("???", LoadPortrait("null"),
            "唔姆姆，团子真好吃～♪")
        ]); await G.I.Cave.Click();
        G.I.DialogBox.ShowDialog([
            new("", LoadPortrait("null"),
            "发光团子被吃掉，游戏失败，如果想再试一次，请直接退出并重启游戏。")
        ]);
    }

    public override void _Ready()
    {
        Floor1.EnemySummonValue = new Dictionary<string, float>
        {
            { "dangoPea", 0.6f },
            { "dangoWater", 0.3f },
            { "dangoFist", 0.1f },
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
        Fsm.startState.OnEnd += Init;
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
    public async void Load()
    {
        string player = GameData.SelectedCharacter;
        // 场景资源加载
        Player.Init(player, 3);
        switch (player)
        {
            case "cirno":
                //Player.PlayerUnit.LearnSkillGroup("Freeze");
                break;
            case "marisa":
                Player.PlayerUnit.LearnSkillGroup("Star");
                break;
            case "rumia":
                Player.PlayerUnit.LearnSkillGroup("Dark");
                break;
        }
        G.I.SkillPanel.Refresh();
        G.I.SkillPanel.Add("Star");
        G.I.SkillPanel.Add("Freeze");
        G.I.SkillPanel.Add("Dark");
        await ToSignal(GetTree().CreateTimer(0.01), "timeout");
        G.I.Fsm.ChangeState(Fsm.talkState);
        G.I.DialogBox.Show();
        G.I.DialogBox.ShowDialog([
            new("【某日】【雾之湖边某地下洞穴】", LoadPortrait("null"),
            "最近你听说这附近出现了一个奇妙的会发光的团子。")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("", LoadPortrait("null"),
            "但是发光团子好像被谁给藏起来据为己有了，你决定进入这个可疑的洞穴一探究竟。")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("", LoadPortrait("null"),
            "你加快了前进的脚步，毕竟要是在找到之前被人吃掉了就完蛋了。")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("", LoadPortrait("null"),
            "（地穴一共有3层，前两层要先找到楼梯口，然后按↓方向键进入下一层）")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("任务目标：", LoadPortrait("null"),
            "找到发光团子。")
        ]); await Click();
        G.I.Fsm.ChangeState(Fsm.updateState);
        G.I.DialogBox.Hide();
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

    public void RegisterToG(G g)
    {
        g.Cave = this;
    }
}
