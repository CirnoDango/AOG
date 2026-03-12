using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class TutorialE : Node
{
    public TileMapLayer map;
    public static int wavepass = 1;
    public static int enemydead = 0;
    private TaskCompletionSource _dangoDieTcs;
    private TaskCompletionSource<Item> _itemPickedTcs;
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
        if (enemy.Name == "marisa")
        {
            TutorialEnd();
        }
    }

    public override void _ExitTree()
    {
        //GameEvents.OnEnemyKilled -= OnEnemyKilled;
    }
    public async Task<Item> WaitForItemPickup(string itemName)
    {
        _itemPickedTcs = new TaskCompletionSource<Item>();
        GameEvents.OnItemPicked += OnItemPicked;

        Item picked = await _itemPickedTcs.Task;

        // 记得取消订阅
        GameEvents.OnItemPicked -= OnItemPicked;

        return picked;
    }

    private void OnItemPicked(Item item)
    {
        if (item.Name == "PowerBlock")
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
        G.I.Fsm.ChangeState(Fsm.StartState);
        Fsm.StartState.OnEnd += Init;
        await ToSignal(GetTree().CreateTimer(0.01), "timeout");
        GameEvents.OnEnemyKilled += OnEnemyKilled;
        Unit.OnPlayerdied += Playerdied;
        Run();
    }
    public void Init()
    {
        string player = GameData.SelectedCharacter;
        // 场景资源加载
        Player.Init("cirno", 1);
        Map tutorial = new("tutorial")
        {
            Entrance = new Vector2I(10, 5)
        };
        Scene.Enter(tutorial);
    }

    private async Task RunTutorial()
    {
        DialogBox.SShow();
        G.I.DialogBox.ShowDialog([
            new("【One Day】【Lakeside of Misty Lake】", LoadPortrait("null"),
        "Cirno runs into Marisa, who came out to practice magic.")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("Cirno", LoadPortrait("cirno_fight_surprise"),
        "Hey! Marisa, what are you doing on my turf?")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_unhappy"),
        "Your turf? This isn't some exclusive Cirno territory. I just came to find a place to practice.")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("Cirno", LoadPortrait("cirno_fight_happy"),
        "Practice? Perfect, I want to get stronger too! Teach me some tricks!")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_sweat"),
        "Huh? You? Get stronger? ...Well, I guess I could. But if you can't handle my training, don't run away halfway!")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("Cirno", LoadPortrait("cirno_fight_confident"),
        "I'm the strongest ice fairy! I'm not scared of you!")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_happy"),
        "Then stop talking and let's begin!")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_confident"),
        "First, try moving using the numpad or the keys on the left side of your keyboard.")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_confident"),
        "On the numpad, think of key 5 as the center. The other eight keys move you to the eight neighboring tiles.")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_confident"),
        "2 is down, 8 is up, 4 is left, 6 is right. 1, 3, 7, and 9 move diagonally.")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_confident"),
        "On the left side keyboard: S is down, W is up, A is left, D is right. Q, E, Z, and C are diagonal movement.")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_confident"),
        "Press 5 or F to wait for one turn.")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_confident"),
        "Remember: water and walls are impassable terrain. Don't go flying around randomly during practice!")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("Cirno", LoadPortrait("cirno_fight_cry"),
        "Eh!? That's not fair!")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_confident"),
        "Also, if there's an enemy in the direction you're moving, you'll attack instead of moving!")
        ]); await Click();
        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_confident"),
        "Give it a try. Move over and attack that dango!")
        ]); await Click(); DialogBox.SHide();

        GameEvents.OnEnemyKilled += OnEnemyKilled;
        var d = Scene.CurrentMap.CreateEnemy(new Vector2I(10, 10), "dangoPea", UnitEgo.normal, 0, false);
        d.UnitAi = new(d) { Mode = AiMode.Sleep };

        await dangodie(); DialogBox.SShow();

        G.I.DialogBox.ShowDialog([
            new("Cirno", LoadPortrait("cirno_fight_hahaha"),
        "Hahaha! That was easy!")
        ]); await Click();

        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_confident"),
        "Now look at the top-left corner of the screen.")
        ]); await Click();

        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_confident"),
        "These are your three resource bars. Red is Health (HP), yellow is Spirit Power (SP), and blue is Magic Power (MP).")
        ]); await Click();

        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_confident"),
        "If your HP reaches zero, you're defeated! And during a short battle, HP doesn't recover easily.")
        ]); await Click();

        G.I.DialogBox.ShowDialog([
            new("Cirno", LoadPortrait("cirno_fight_angry"),
        "I won't get defeated!")
        ]); await Click();

        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_confident"),
        "There are two types of damage that reduce HP: melee attacks and bullet damage.")
        ]); await Click();

        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_hahaha"),
        "Melee damage hits HP directly. But bullet damage can be grazed by strong fighters like us!")
        ]); await Click();

        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_confident"),
        "When you take bullet damage, you'll automatically attempt to graze it, increasing your SP! SP can be used to activate skills.")
        ]); await Click();

        Player.PlayerUnit.Us.LearnSkill("Icefall");

        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_confident"),
        "See the ice crystal icon on the left side of the screen? That's your skill: Icefall! Click it to use it!")
        ]); await Click();

        G.I.DialogBox.ShowDialog([
            new("Cirno", LoadPortrait("cirno_fight_hahaha"),
        "Whoa! Then if I keep grazing forever, I'll never get hit by bullets!")
        ]); await Click();

        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_confident_black"),
        "Of course not! Grazing doesn't always succeed. The number next to your SP bar shows the graze success rate. The higher your SP, the harder it is to graze. If SP is full, you'll get hit and lose HP.")
        ]); await Click();

        G.I.DialogBox.ShowDialog([
            new("Cirno", LoadPortrait("cirno_fight_unhappy"),
        "Uwaaaaah—")
        ]); await Click();

        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_confident"),
        "First build up SP by grazing, then finish your opponent with a skill!")
        ]); await Click();

        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_confident"),
        "Your skill panel is on the left side. Hover your mouse over it to see detailed descriptions." +
        "To use a skill, click the icon first, then click the target tile!")
        ]);

        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_confident"),
        "Now go ahead—use your skill to defeat that dango!")
        ]); await Click();

        DialogBox.SHide();

        GameEvents.OnEnemyKilled += OnEnemyKilled;
        var d2 = Scene.CurrentMap.CreateEnemy(new Vector2I(6, 9), "dangoPea", UnitEgo.normal, 0, false);
        Player.PlayerUnit.Ua.BodyDamageAccuracy -= 10;
        d2.Inventory.AddItem(Item.CreateItem("PowerBlock", 1));

        await dangodie(); DialogBox.SShow();

        Player.PlayerUnit.Ua.BodyDamageAccuracy += 10;

        G.I.DialogBox.ShowDialog([
            new("Cirno", LoadPortrait("cirno_fight_hahaha"),
        "Easy! I really am the strongest!")
        ]); await Click();

        G.I.DialogBox.ShowDialog([
            new("Cirno", LoadPortrait("cirno_fight_cautious"),
        "Huh? Something appeared on the ground.")
        ]); await Click();

        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_happy"),
        "That's a P-point item! Move onto the tile and press G to pick it up!")
        ]); await Click();

        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_confident"),
        "Then press I to open your inventory and equipment screen. Click the item to equip it! Try picking up the P-point and equipping it.")
        ]); await Click();

        DialogBox.SHide();
        await WaitForItemPickup("PowerBlock");
        DialogBox.SShow();

        G.I.DialogBox.ShowDialog([
            new("Cirno", LoadPortrait("cirno_fight_hahaha"),
        "Yay!")
        ]); await Click();

        Player.PlayerUnit.Us.LearnSkill("MinusK");
        Player.PlayerUnit.Us.LearnSkill("PerfectFreeze");

        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_confident"),
        "See that special framed icon in your skill bar? That's your Spell Card: [Perfect Freeze]!")
        ]); await Click();

        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_confident"),
        "Spell Cards are powerful skills that last for several turns, but they cost a lot of SP to activate!")
        ]); await Click();

        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_confident"),
        "If you take too much bullet damage while a Spell Card is active and its durability drops to zero, the Spell Card will break!" +
        "Not only will the effect end, but the one who breaks it takes massive HP damage!")
        ]); await Click();

        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_confident"),
        "As long as it doesn't break, activating a Spell Card will consume a lot of SP, letting you keep grazing!")
        ]); await Click();

        G.I.DialogBox.ShowDialog([
            new("Cirno", LoadPortrait("cirno_fight_hahaha"),
        "Umm... sounds complicated... but I guess I'll just freeze everyone!")
        ]); await Click();

        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_confident"),
        "Now let's fight!")
        ]); await Click();

        G.I.DialogBox.ShowDialog([
            new("Tutorial Objective:", LoadPortrait("null"),
        "Defeat Marisa.")
        ]); await Click();

        Scene.CurrentMap.CreateEnemy(new Vector2I(5, 5), "marisa", UnitEgo.normal);

        G.I.Fsm.ChangeState(Fsm.UpdateState);
        G.I.DialogBox.Hide();
    }
    public async void TutorialEnd()
    {
        G.I.DialogBox.Show();
        G.I.Fsm.ChangeState(Fsm.TalkState);

        G.I.DialogBox.ShowDialog([
            new("Cirno", LoadPortrait("cirno_fight_hahaha"),
        "For the strongest in Gensokyo, that was a piece of cake!")
        ]); await Click();

        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_sweat"),
        "Hey, hey, hey… since when did you get this strong?")
        ]); await Click();

        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_sweat"),
        "Nice work. Let's call it a day for today's training. (Maybe I went a little too easy on you...)")
        ]); await Click();

        G.I.DialogBox.ShowDialog([
            new("Thanks for playing!", LoadPortrait("null"),
        "This concludes the tutorial stage. Now go try the full stage!")
        ]); await Click();

        G.I.Fsm.ChangeState(Fsm.UpdateState);
        G.I.DialogBox.Hide();
    }

    public async void Playerdied()
    {
        G.I.DialogBox.Show();
        G.I.Fsm.ChangeState(Fsm.TalkState);

        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_hahaha"),
        "A baka is still a baka, huh? Guess you just can't learn how to fight.")
        ]); await Click();

        G.I.DialogBox.ShowDialog([
            new("Marisa", LoadPortrait("marisa_fight_hahaha"),
        "Oh well, I'll let you continue once. Consider it inserting another coin.")
        ]); await Click();

        Player.PlayerUnit.Ua.GetHp(200);

        G.I.Fsm.ChangeState(Fsm.UpdateState);
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
        return GD.Load<Texture2D>($"res://Assets/Portraits/{name}.png");
    }
    public override void _UnhandledInput(InputEvent @event)
    {
        if (clickTcs != null && @event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            clickTcs.TrySetResult();
        }
    }
}
