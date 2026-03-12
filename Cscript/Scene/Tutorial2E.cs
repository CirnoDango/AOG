using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
public partial class Tutorial2E : Stage
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
            new("【The Next Day】【Lakeside of Misty Lake】", L("null"),
                "Cirno runs into Reimu."),
            new("Reimu", L("reimu_fight_confident"),
                "I heard Marisa taught you some combat tricks. Let me teach you how to grow stronger."),
            new("First, press B (aBility) to open the Ability panel.")
            );
        G.I.Fsm.Cab.Update(Player.PlayerUnit);
        G.I.Fsm.Cab.Visible = true;
        await ShowDialogSequence(
            new("Reimu", L("reimu_fight_confident"),
                "In the Ability panel you can check your current status, attributes, and learned skills."),
            new("You can also view enemy information by hovering your mouse over their unit icon.")
            );
        G.I.Fsm.Cab.Visible = false;
        await ShowDialogSequence(
            new("Reimu", L("reimu_fight_confident"),
                "Chests on the map and defeated enemies will drop items and Memories. They are the main way to increase your combat power."),
            new("Press I (Inventory) to open the equipment panel. Here you can equip, unequip, or discard items."),
            new("Both your equipment slots and inventory are limited by your carry weight. You can increase the limit by raising your Dexterity."),
            new("Press M (Memory) to open the Memory panel. Memories grant various ability points."),
            new("Unlike equipment, once a Memory is equipped it cannot be removed. Your Memory capacity increases automatically when entering a new map."),
            new("Press K (sKill) to open the Skill panel. Here you can spend ability points to improve attributes and skill levels.")
            );
        G.I.SkillPanel.Refresh();
        G.I.Ua.Refresh();
        G.I.Fsm.Usbb.Visible = true;
        await ShowDialogSequence(
            new("Reimu", L("reimu_fight_confident"),
                "Each skill group has five skills you can learn and upgrade. The last one is a Spell Card."),
            new("Each skill can be upgraded to level 3. If you master that skill group, it can reach level 4!")
            );
        G.I.Fsm.Usbb.Visible = false;
        G.I.DialogBox.ShowDialog([
            new("Reimu", L("reimu_fight_confident"),
        "Give it a try. Use the Memory in your inventory to learn level 3 of [Perfect Freeze].")
        ]); await Click();
        G.I.Fsm.ChangeState(Fsm.UpdateState);
        G.I.DialogBox.Hide();
        GameEvents.OnSkillLearned += CheckPf3Get;

        await Pf3Get();

        DialogBox.SShow();
        G.I.Fsm.ChangeState(Fsm.TalkState);
        G.I.DialogBox.ShowDialog([
            new("Reimu", L("reimu_fight_confident"),
        "Press T (Talent) to open the Talent panel. Here you can learn and master new skill trees.")
        ]); await Click();
        G.I.SkillTreeBox.Refresh();
        G.I.SkillTreeBox.Visible = true;
        await ShowDialogSequence(
            new("This hexagonal grid is the Talent panel. Each icon inside some tiles represents a skill tree."),
            new("Each tile has three states. A gold tile means you have mastered that skill tree."),
            new("All tiles adjacent to a gold tile become blue, meaning you have learned those skill trees."),
            new("The remaining white tiles are unlearned. Spending 1 talent point lets you click a blue tile to turn it gold.")
            );
        G.I.SkillTreeBox.Visible = false;
        await ShowDialogSequence(
            new("To fire danmaku, besides using skills, you can also use a special item called a 'Danmaku Box'."),
            new("Each Danmaku Box contains several bullet components. Using it will fire bullets based on those components."),
            new("There are many kinds of components, such as different bullet colors and shapes, patterns, and power boosts."),
            new("Press O (danmOku) to open the Danmaku editor and freely combine components!"),
            new("Press G to interact with the tile you're standing on, like opening chests."),
            new("To open a door, just move toward it."),
            new("If you find the map exit, press ↓ there to descend to the next floor."),
            new("I put a few powerful Danmaku Boxes in your inventory. Go have fun, little fairy~")
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

