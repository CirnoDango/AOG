using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
public partial class Tutorial2 : Stage
{
    public TileMapLayer map;
    private TaskCompletionSource _Pf3GetTcs;
    public async Task Pf3Get()
    {
        _Pf3GetTcs = new TaskCompletionSource();
        await _Pf3GetTcs.Task;
    }
    private void CheckPf3Get(Unit unit, SkillInstance si)
    {
        if (si.Name == "PerfectFreeze" && si.Level == 3)
        {
            _Pf3GetTcs?.TrySetResult();
        }
    }
    public async void Run()
    {
        await RunTutorial();
    }
    public override async void _Ready()
    {
        map = G.I.TileMapAllLayer.BaseGround;
        G.I.Fsm.ChangeState(Fsm.StartState);
        Fsm.StartState.OnEnd += Init;
        await ToSignal(GetTree().CreateTimer(0.01), "timeout");
        Run();
    }
    public void Init()
    {
        // 场景资源加载
        Player.Init("cirno", 3);
        Map tutorial = MapBuilder.ImportMapFromJson("res://map.json", "tutorial");
        MapBuilder.BuildLogicFromTileMap();
        tutorial.Entrance = new Vector2I(10, 5);
        Scene.Enter(tutorial);
        tutorial.CreateEnemy(new Vector2I(10, 3), "reimu");
        tutorial.SummonChest(6, 1, tutorial.GetGrid(new Vector2I(3, 2)));
        G.I.SkillPanel.Add("Freeze");
        Player.PlayerUnit.Inventory.AddItem(Item.CreateItem("mSkill"));
        Player.PlayerUnit.Inventory.AddItem(Item.CreateItem("mSkill"));
        Player.PlayerUnit.Memorys.MaxEquipWeight += 15;
    }

    private async Task RunTutorial()
    {
        G.I.Fsm.ChangeState(Fsm.TalkState);
        DialogBox.SShow();
        await ShowDialogSequence(
            new("【次日】【雾之湖边】", L("null"),
                "琪露诺碰见了灵梦。"),
            new("灵梦", L("reimu_fight_confident"),
                "听说你很努力呢, 让我也来教你一些东西吧。"),
            new("休息状态可以对自己的灵力进行操控哦。"),
            new("按5或F的休息是使灵力消散，减少SP。"),
            new("而按0或者空格的休息则是聚集灵力，增加SP！"),
            new("按R，T，Y，U分别是打开装备栏、能力栏、技能栏、记忆栏。"),
            new("在你背包里的装备，只要不超过负重限制，都可以装备到装备栏中！"),
            new("而名字中带有记忆的，只能放进你的“记忆槽”！记忆是永久的，装备上了就不能再取下来。"),
            new("装备栏负重随着灵巧属性提升而上升，记忆负重则随着游戏进程不断深入自动增加。"),
            new("能力栏中可以查看当前的战斗属性，技能栏中可以使用自由属性点和技能点进行升级。"),
            new("每个技能组中都有5个技能可以学习并升级，其中最后一个是符卡"),
            new("再说一遍，R，T，Y，U分别是装备栏、能力栏、技能栏、记忆栏！也就是键盘第一排除了移动用按键的左边四个。"),
            new("记不起来就挨个按过去也行啦！"),
            new("来试试吧，用你背包里的记忆学习3级的【perfect freeze】")
        );
        G.I.Fsm.ChangeState(Fsm.UpdateState);
        G.I.DialogBox.Hide();
        GameEvents.OnSkillLearned += CheckPf3Get;

        await Pf3Get();

        DialogBox.SShow();
        G.I.Fsm.ChangeState(Fsm.TalkState);
        await ShowDialogSequence(
            new("除了使用学习的技能发射弹幕以外，使用弹幕盒子也可以发射弹幕。"),
            new("弹幕盒子里可以装备数个弹幕组件，使用时读取弹幕组件并发射弹幕。"),
            new("弹幕组件有很多种，比如不同色不同形状的子弹、弹幕形状、火力增强等。"),
            new("I键可以打开弹幕编辑面板，自由组合弹幕组件！"),
            new("然后按G是和自己所在的格子互动，比如打开宝箱什么的。"),
            new("而要打开门的话，就朝着门移动就好啦！"),
            new("如果找到了地图的出口，在那里按↓就可以进入下一层了。"),
            new("我在你的背包里放了几个厉害的弹幕盒子，自己玩去吧小妖精~")
        );
        G.I.Fsm.ChangeState(Fsm.UpdateState); 
        G.I.DialogBox.Hide();
        Player.PlayerUnit.Ua.Cun += 100;
        Player.PlayerUnit.Inventory.AddItem(Item.CreateItem("BarrageSet",
            new Dictionary<string, object> { { "barrage", new Dictionary<string, object> { { "MaxComponents", 6 } } } }));
        Player.PlayerUnit.Inventory.AddItem(Item.CreateItem("BarrageSet",
            new Dictionary<string, object> { { "barrage", new Dictionary<string, object> { { "MaxComponents", 6 } } } }));
        Player.PlayerUnit.Inventory.AddItem(Item.CreateItem("BarrageSet",
            new Dictionary<string, object> { { "barrage", new Dictionary<string, object> { { "MaxComponents", 6 } } } }));
    }
}

