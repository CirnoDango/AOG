using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D, IRegisterToG
{
    // 每次移动的格子大小（比如 16 或 32 像素）
    public int TileSize = 16;
    public static Unit PlayerUnit;
    public int UaPoint = 80;
    public int SkillPoint = 80;
    public static Dictionary<Key, GameState> BoxEntryKeys => new()
    {
        {Key.R,Fsm.inventoryBoxState },
        {Key.T,Fsm.combatAbilityBoxState },
        {Key.Y,Fsm.uaSkillBoxState },
        {Key.U,Fsm.memoryBoxState }
    };
    // 按键到方向的映射表
    private readonly Dictionary<Key, Vector2I> MoveDirs = new()
	{
		{ Key.Kp1, new Vector2I(-1, 1) },
		{ Key.Kp2, new Vector2I(0, 1) },
		{ Key.Kp3, new Vector2I(1, 1) },
		{ Key.Kp4, new Vector2I(-1, 0) },
		{ Key.Kp6, new Vector2I(1, 0) },
		{ Key.Kp7, new Vector2I(-1, -1) },
		{ Key.Kp8, new Vector2I(0, -1) },
		{ Key.Kp9, new Vector2I(1, -1) }
	};
    private readonly Dictionary<Key, Vector2I> MoveDirs2 = new()
    {
        { Key.Z, new Vector2I(-1, 1) },
        { Key.S, new Vector2I(0, 1) },
        { Key.C, new Vector2I(1, 1) },
        { Key.A, new Vector2I(-1, 0) },
        { Key.D, new Vector2I(1, 0) },
        { Key.Q, new Vector2I(-1, -1) },
        { Key.W, new Vector2I(0, -1) },
        { Key.E, new Vector2I(1, -1) }
    };
    public override void _Input(InputEvent @event)
    {
        if (G.I.Fsm.currentState is PlayerSkillState)
            HandlePlayerSkillInput(@event);
        else if (G.I.Fsm.currentState is InventoryState or CombatAbilityBoxState or UaSkillBoxState
             or MemoryBoxState)
            HandleBoxInput(@event);
    }
    public static void Init(string name, float zoom)
    {
        Unit unit = EnemyLoader.LoadEnemy(name);
        SpriteManager.LoadEnemy(unit);
        GameLoader.rootnode.AddChild(unit.sprite);
        unit.sprite.Position = 16 * unit.Position;
        unit.InitializeHpSpBar();
        unit.unitAi = new UnitAi(unit);
        foreach (var gskill in Skill.SkillDeck)
        {
            if (gskill.SkillGroup == "")
                unit.LearnSkill(gskill);
        }
        unit.TimeEnergy = 0;
        unit.unitAi = null;
        var cam = new Camera2D
        {
            Position = Vector2.Zero,
            Zoom = new Vector2(zoom, zoom)
        };
        unit.sprite.AddChild(cam);  // 加到角色节点下
        cam.MakeCurrent();  // 设置为当前摄像机
        cam.Position = new Vector2I(8, 8);
        PlayerUnit = unit;
        PlayerUnit.friendness = 1;
        G.I.PlayerStatusBar.Init();
    }
    private void HandleBoxInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            var key = keyEvent.Keycode;
            if(BoxEntryKeys.TryGetValue(key, out var entry))
            {
                if (BoxEntryKeys[key] == G.I.Fsm.currentState)
                    G.I.Fsm.ChangeState(Fsm.playerSkillState);
                else
                    G.I.Fsm.ChangeState(BoxEntryKeys[key]);
            }
        }
    }
    private void HandlePlayerSkillInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            var key = keyEvent.Keycode;

            // 特判数字键盘5
            if (key == Key.Kp5 || key == Key.F)
            {
                if (Skill.NameSkill.TryGetValue("Rest", out var restSkill))
                {
                    if (PlayerUnit.currentSpellcard == null)
                        PlayerUnit.GetSp(-5);
                    restSkill.Activate(new SkillContext(PlayerUnit, PlayerUnit.CurrentGrid));
                }
                return;
            }
            if (key == Key.Kp0 || key == Key.Space)
            {
                if (Skill.NameSkill.TryGetValue("Rest", out var restSkill))
                {
                    PlayerUnit.GetSp(10);
                    restSkill.Activate(new SkillContext(PlayerUnit, PlayerUnit.CurrentGrid));
                }
                return;
            }
            if (key == Key.Down)
            {
                if(PlayerUnit.CurrentGrid.TerrainBaseGround == "Stair")
                {
                    Scene.Quit();
                }
                return;
            }
            if (BoxEntryKeys.TryGetValue(key, out var entry))
            {
                G.I.Fsm.ChangeState(BoxEntryKeys[key]);
            }
            if (key == Key.G)
            {
                Skill.NameSkill["Interact"].Activate(new SkillContext(PlayerUnit, PlayerUnit.CurrentGrid));
                return;
            }

            // 处理方向输入
            if (MoveDirs.TryGetValue(key, out Vector2I dir) || MoveDirs2.TryGetValue(key, out dir))
            {
                var targetPos = PlayerUnit.Position + dir;
                IInteractable grid = Scene.CurrentMap.GetGrid(targetPos);
                grid.Interact(PlayerUnit);
            }
        }
    }

    public void RegisterToG(G g)
    {
        g.Player = this;
	}
}
