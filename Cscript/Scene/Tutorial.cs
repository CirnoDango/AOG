using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Tutorial : Node
{
    public TileMapLayer map;
    public static int wavepass = 1;
    public static int enemydead = 0;
    private TaskCompletionSource _dangoDieTcs;
    private TaskCompletionSource<ItemInstance> _itemPickedTcs;
    public async Task dangodie()
    {
        _dangoDieTcs = new TaskCompletionSource();
        await _dangoDieTcs.Task;
    }

    private void OnEnemyKilled(Unit enemy)
    {
        if (enemy.Name == "dangoPea")
        {
            _dangoDieTcs?.TrySetResult();
        }
    }

    public override void _ExitTree()
    {
        GameEvents.OnEnemyKilled -= OnEnemyKilled;
    }
    public async Task<ItemInstance> WaitForItemPickup(string itemName)
    {
        _itemPickedTcs = new TaskCompletionSource<ItemInstance>();
        GameEvents.OnItemPicked += OnItemPicked;

        ItemInstance picked = await _itemPickedTcs.Task;

        // 记得取消订阅
        GameEvents.OnItemPicked -= OnItemPicked;

        return picked;
    }

    private void OnItemPicked(ItemInstance item)
    {
        if (item.Name == "PowerBlock") // 你可以替换成任何道具名或条件
        {
            _itemPickedTcs?.TrySetResult(item);
        }
    }
    public async void Run()
    {
        await RunTutorial();
    }
    public override async void _Ready()
    {
        map = G.I.TileMapAllLayer.BaseGround;
        G.I.Fsm.ChangeState(Fsm.startState);
        Fsm.startState.OnEnd += Init;
        await ToSignal(GetTree().CreateTimer(0.01), "timeout");
        Scene.CurrentMap.OnUnitDied += UpdateEnemy;
        Scene.CurrentMap.OnMarisaDied += TutorialEnd;
        Unit.OnPlayerdied += Playerdied;
        Run();
    }
    public void Init()
    {
        string player = GameData.SelectedCharacter;
        // 场景资源加载
        Player.Init("cirno", 3);
        Map tutorial = MapBuilder.BuildLogicFromTileMap();
        tutorial.Entrance = new Vector2I(10, 5);
        Scene.Enter(tutorial);
    }

    private async Task RunTutorial()
    {
        DialogBox.SShow();
        G.I.DialogBox.ShowDialog([
            new("【某日】【雾之湖边】", LoadPortrait("null"),
            "琪露诺碰见了出门练习魔法的魔理沙。")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("琪露诺", LoadPortrait("cirno_fight_surprise"),
            "呦呵！魔理沙，你怎么跑到我地盘来了？")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_unhappy"),
            "你的地盘？这可不是什么“琪露诺领土”，我只是来找个地方练练手。")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("琪露诺", LoadPortrait("cirno_fight_happy"),
            "练手？正好，我也要变强！你来教我点本事吧！")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_sweat"),
            "哈啊？你？变强？……不过嘛，也不是不行。你要是撑得过我的教学，可别临阵脱逃哦！")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("琪露诺", LoadPortrait("cirno_fight_confident"),
            "我可是最强的冰之妖精，才不会怕你呢！")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_happy"),
            "那就别废话了，我们开始吧！")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_confident"),
            "首先，试着使用小键盘的数字或者左侧键盘进行移动吧。")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_confident"),
            "小键盘里，把按键5看做中心，其他8个键就分别对应向相邻的8格移动。")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_confident"),
            "2是下，8是上，4是左，6是右，1、3、7、9则是斜线移动。")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_confident"),
            "左侧键盘的话，S是下，W是上，A是左，D是右，Q、E、Z、C则是斜线移动。")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_confident"),
            "按5/F则是原地休息一回合。")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_confident"),
            "注意，水面和墙是不能移动进去的地形，练习的时候不要乱飞啊！")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("琪露诺", LoadPortrait("cirno_fight_cry"),
            "诶！怎么这样！")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_confident"),
            "此外，如果移动方向上存在敌人的话，就不会进行移动，而是会进行近战攻击！")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_confident"),
            "试试看吧，移动过去攻击那个团子！")
        ]); await Click(); DialogBox.SHide();
        GameEvents.OnEnemyKilled += OnEnemyKilled;
        var d = Scene.CurrentMap.CreateEnemy(new Vector2I(10, 10), "dangoPea");
        for(int x = 1; x <= 7; x++)
        {
            for (int y = 1; y <= 5; y++)
            {
                //_ = Scene.CurrentMap.CreateEnemy(new Vector2I(x, y), "marisa");
            }
        }
        d.unitAi = new(d);
        d.unitAi.Mode = AiMode.Sleep;
        await dangodie(); DialogBox.SShow();
        G.I.DialogBox.ShowDialog([
            new("琪露诺", LoadPortrait("cirno_fight_hahaha"),
            "哈哈哈！这么简单就干掉了！")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_confident"),
            "现在看一看画面的左上角")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_confident"),
            "这里显示了你的三种不同资源值。红色条是生命值(HP)，黄色条是灵力值(SP)，蓝色条是魔力值(MP)。")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_confident"),
            "HP归零时，你就被打倒了！并且在一段短时间的战斗中，HP很难快速回复。")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("琪露诺", LoadPortrait("cirno_fight_angry"),
            "我才不会被打倒呢！")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_confident"),
            "会削减HP的伤害有两种。一种是近距离才能造成的体术伤害，一种是弹幕伤害。")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_hahaha"),
            "体术伤害会直接扣减HP，而弹幕伤害，对于像我们这样的强者，则可以用擦弹来化解！")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_confident"),
            "当你受到弹幕伤害时，会自动进行擦弹，并提升对应的SP值！获得的SP值可以用于发动招式。")
        ]); await Click();
        Player.PlayerUnit.LearnSkill("Icefall");
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_confident"),
            "看到画面左侧那个冰晶图标了吗？这是你的技能Icefall！点击就可以使用啦！")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("琪露诺", LoadPortrait("cirno_fight_hahaha"),
            "哇袄！那岂不是一直擦弹，就完全不会受到弹幕伤害了？")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_confident_black"),
            "当然不是！如果你擦弹太多使得SP值满了，又没有及时用出去，就会导致被击中，扣减HP值。")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("琪露诺", LoadPortrait("cirno_fight_unhappy"),
            "呜诶————")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_confident"),
            "先通过擦弹积累SP值，然后发动技能将对手干掉！或者原地休息一回合也可以降低SP值")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_confident"),
            "技能面板在屏幕左边，将鼠标停留在上面就能看到详细说明了！" +
            "发动时先点击技能图标，再点击发动目标格子就行啦！")
        ]);
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_confident"),
            "来吧，用技能干掉这个团子！")
        ]); await Click();
        DialogBox.SHide();
        GameEvents.OnEnemyKilled += OnEnemyKilled;
        var d2 = Scene.CurrentMap.CreateEnemy(new Vector2I(12, 16), "dangoPea");
        d2.Ua.Dex = 999;
        d2.inventory.AddItem(Item.GetItemName("PowerBlock"));
        await dangodie(); DialogBox.SShow();
        G.I.DialogBox.ShowDialog([
            new("琪露诺", LoadPortrait("cirno_fight_hahaha"),
            "轻松轻松！我果然是最强的！")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("琪露诺", LoadPortrait("cirno_fight_cautious"),
            "诶，地面上好像多出来了什么东西哦？")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_happy"),
            "这是P点道具！你可以走到道具掉落的格子，按G将道具捡起来！")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_confident"),
            "然后按R打开你的背包栏和装备栏，再点击背包中的道具，就可以将道具装备上了！来试试把P点捡起来然后装备上吧！")
        ]); await Click(); DialogBox.SHide(); await WaitForItemPickup("PowerBlock"); DialogBox.SShow();
        G.I.DialogBox.ShowDialog([
            new("琪露诺", LoadPortrait("cirno_fight_hahaha"),
            "好耶！")
        ]); await Click(); 
        Player.PlayerUnit.LearnSkill("MinusK");
        Player.PlayerUnit.LearnSkill("PerfectFreeze");
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_confident"),
            "看到技能栏中那个边框特别的图标了吗？这是你的符卡【Perfect Freeze】！")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_confident"),
            "符卡是一种强大的持续数个回合的技能，需要非常高的SP值才能发动！")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_happy"),
            "发动符卡之后，每回合会持续消耗SP值，SP的回复会停止，此时擦弹也不再使SP增加，而是减少！")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_confident"),
            "如果在符卡发动中受到太多弹幕伤害，SP降至零的话，符卡就会被击破！" +
            "不仅符卡效果中断，而且被击破者会受到大量HP伤害！")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_confident"),
            "只要不被击破，发动符卡就可以大幅消耗积累的SP值，从而能继续擦弹！")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("琪露诺", LoadPortrait("cirno_fight_hahaha"),
            "唔……听起来好复杂……不过总之把对手全部冻起来就好啦！")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_confident"),
            "接下来就进入实战试试看吧！")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("教程任务目标：", LoadPortrait("null"),
            "击败魔理沙。")
        ]); await Click();
        EnemyAutoSummon.Update(7);
        Info.Print("[color=red]1/5波敌人已生成[/color]");
        G.I.Fsm.ChangeState(Fsm.updateState);
        G.I.DialogBox.Hide();
    }
    public async void TutorialEnd()
    {
        G.I.DialogBox.Show();
        G.I.Fsm.ChangeState(Fsm.talkState);
        G.I.DialogBox.ShowDialog([
            new("琪露诺", LoadPortrait("cirno_fight_hahaha"),
            "对于我这个幻想乡最强，真是轻轻松松！")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_sweat"),
            "喂喂喂，你小子怎么变得这么厉害了")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_sweat"),
            "干得不错，今天的练习就到此为止吧（看来放水放得太狠了...）")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("感谢游玩！", LoadPortrait("null"),
            "教程关卡就到此结束了，不过在这后面我设置了无尽的怪物生成，" +
            "您也可以试试能用琪露诺坚持多久。")
        ]); await Click();
        G.I.Fsm.ChangeState(Fsm.updateState);
        G.I.DialogBox.Hide();
    }
    public async void Playerdied()
    {
        G.I.DialogBox.Show();
        G.I.Fsm  .ChangeState(Fsm.talkState);
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_hahaha"),
            "baka果然还是baka呢，不可能学得会战斗的吧")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("魔理沙", LoadPortrait("marisa_fight_hahaha"),
            "没办法，就让你投币复活一次吧")
        ]); await Click();
        Player.PlayerUnit.GetHp(200);
        G.I.Fsm.ChangeState(Fsm.updateState);
        G.I.DialogBox.Hide();
    }
    private TaskCompletionSource clickTcs;

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

    private async Task WaitUntil(Func<bool> condition)
    {
        while (!condition())
            await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
    }

    
    public void UpdateEnemy()
    {
        if (enemydead >= (6 + wavepass) * wavepass)
        {
            wavepass++;
            if (wavepass % 5 == 0)
            {
                for (int i = 0; i < wavepass / 5; i++)
                {
                    Scene.CurrentMap.CreateEnemy(new Vector2I(10 + i, 10), "marisa");
                }
                EnemyAutoSummon.Update(5 + 2 * wavepass - wavepass / 5);
            }
                

            else 
                EnemyAutoSummon.Update(5 + 2 * wavepass);
            Info.Print($"[color=red]{wavepass}/5波敌人已生成[/color]");
        }
    }
}

