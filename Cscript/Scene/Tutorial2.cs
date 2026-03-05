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
    private void CheckPf3Get(Unit unit, Skill si)
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
        Player.Init("cirno", 1);
        Map tutorial = new("tutorial")
        {
            Entrance = new Vector2I(10, 5)
        };
        Scene.Enter(tutorial);
        tutorial.CreateEnemy(new Vector2I(10, 3), "reimu");
        tutorial.SummonChest(6, 1, tutorial.GetGrid(new Vector2I(3, 2)));
        G.I.SkillTreeBox.Expert("Freeze");
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
                "听说魔理沙教了你一些战斗技巧, 让我来教你成长技巧吧。"),
            new("首先按B(aBility)可以进入能力栏")
            );
        G.I.Fsm.Cab.Update(Player.PlayerUnit);
        G.I.Fsm.Cab.Visible = true;
        await ShowDialogSequence(
            new("灵梦", L("reimu_fight_confident"),
                "能力栏中可以查看当前的人物状态、能力属性和持有技能。"),
            new("而敌人的信息可以通过将鼠标悬停在单位图像上查看。")
            );
        G.I.Fsm.Cab.Visible = false;
        await ShowDialogSequence(
            new("灵梦", L("reimu_fight_confident"),
                "地图上的宝箱和敌人会掉落物品和记忆，他们是主要的战斗力提升来源。"),
            new("按I(Inventory)打开装备栏，在这里进行装备、卸下、丢弃操作。"),
            new("你的装备栏和背包都不能超过负重限制，而负重限制可以通过提高灵巧值来提升。"),
            new("按M(Memory)打开记忆栏，记忆会提供各种能力点数。"),
            new("记忆和装备不同的是，记忆一旦装备就不能再被卸下，记忆上限在进入新地图时自动提升。"),
            new("按K(sKill)进入技能栏，在这里可以花费能力点数提升属性和技能等级。")
            );
        G.I.SkillPanel.Refresh();
        G.I.Ua.Refresh();
        G.I.Fsm.Usbb.Visible = true;
        await ShowDialogSequence(
            new("灵梦", L("reimu_fight_confident"),
                "每个技能组中都有5个技能可以学习并升级，其中最后一个是符卡"),
            new("每个技能最多可以升到3级，如果精通这个技能组则可以升到4级！")
            );
        G.I.Fsm.Usbb.Visible = false;
        G.I.DialogBox.ShowDialog([
            new("灵梦", L("reimu_fight_confident"),
            "来试试吧，用你背包里的记忆学习3级的【perfect freeze】")
        ]); await Click();
        G.I.Fsm.ChangeState(Fsm.UpdateState);
        G.I.DialogBox.Hide();
        GameEvents.OnSkillLearned += CheckPf3Get;

        await Pf3Get();

        DialogBox.SShow();
        G.I.Fsm.ChangeState(Fsm.TalkState);
        G.I.DialogBox.ShowDialog([
            new("灵梦", L("reimu_fight_confident"),
            "按T(Talent)打开天赋栏，在这里可以学习和精通新的技能树。")
        ]); await Click();
        G.I.SkillTreeBox.Refresh();
        G.I.SkillTreeBox.Visible = true;
        await ShowDialogSequence(
            new("这里的六边形格子图就是天赋栏，部分格子包含的每个图标就是技能树。"),
            new("每个格子有3种状态。金色格子代表你精通该格子的技能树。"),
            new("金色格的所有相邻格子会变成蓝色，代表你学习了该格子的技能树。"),
            new("剩下的白色格就是未学习。每花1天赋点，可以点击一个蓝色格子，将其变成金色。")
            );
        G.I.SkillTreeBox.Visible = false;
        await ShowDialogSequence(
            new("想要发射弹幕，除了使用技能外，还可以使用叫“弹幕盒子”的特殊物品。"),
            new("每个弹幕盒子都包含几个弹幕组件，使用弹幕盒子会根据里面的弹幕组件发射不同的弹幕。"),
            new("弹幕组件有很多种，比如不同色不同形状的子弹、弹幕形状、火力增强等。"),
            new("O(danmOku)键可以打开弹幕编辑面板，自由组合弹幕组件！"),
            new("然后按G是和自己所在的格子互动，比如打开宝箱什么的。"),
            new("而要打开门的话，就朝着门移动就好啦！"),
            new("如果找到了地图的出口，在那里按↓就可以进入下一层了。"),
            new("我在你的背包里放了几个厉害的弹幕盒子，自己玩去吧小妖精~")
        );
        G.I.Fsm.ChangeState(Fsm.UpdateState); 
        G.I.DialogBox.Hide();
        Player.PlayerUnit.Ua.Cun += 100;
        Player.PlayerUnit.Inventory.AddItem(Item.CreateItem("BarrageSet",
            new Dictionary<string, object> { { "barrage", new Dictionary<string, object> { { "MaxComponents", 8 } } } }));
        Player.PlayerUnit.Inventory.AddItem(Item.CreateItem("BarrageSet",
            new Dictionary<string, object> { { "barrage", new Dictionary<string, object> { { "MaxComponents", 8 } } } }));
        Player.PlayerUnit.Inventory.AddItem(Item.CreateItem("BarrageSet",
            new Dictionary<string, object> { { "barrage", new Dictionary<string, object> { { "MaxComponents", 8 } } } }));
    }
}

