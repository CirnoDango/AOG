using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Player : CharacterBody2D, IRegisterToG
{
    // 每次移动的格子大小（比如 16 或 32 像素）
    public int TileSize = 16;
    public static Unit PlayerUnit;
    public static Dictionary<string, HexStatus> skillTrees = [];
    public static Camera2D playerCamera;
    public int UaPoint
    {
        get => PlayerUnit.Ua.uaPoint;
        set => PlayerUnit.Ua.uaPoint = value;
    }
    public int SkillPoint
    {
        get => PlayerUnit.Ua.skillPoint;
        set => PlayerUnit.Ua.skillPoint = value;
    }
    public int TalentPoint
    {
        get => PlayerUnit.Ua.talentPoint;
        set => PlayerUnit.Ua.talentPoint = value;
    }
    public static Dictionary<Key, GameState> BoxEntryKeys => new()
    {
        {Key.I,Fsm.InventoryBoxState },
        {Key.B,Fsm.CombatAbilityBoxState },
        {Key.K,Fsm.UaSkillBoxState },
        {Key.M,Fsm.MemoryBoxState },
        {Key.O,Fsm.BarrageBoxState },
        {Key.T,Fsm.SkillTreeState },
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
        else if (G.I.Fsm.currentState is IBoxState)
            HandleBoxInput(@event);
    }
    public static void Init(string name, float zoom)
    {
        Unit unit = EnemyLoader.LoadEnemy(name, false);
        PlayerUnit = unit;
        foreach (var (skill, _) in unit.Us.skills)
        {
            G.I.SkillBar.LearnSkill(skill);
        }
        SpriteManager.LoadEnemy(unit);
        Root.rootnode.AddChild(unit.Up.sprite);
        unit.Up.sprite.Position = Setting.imagePx * unit.Up.Position;
        unit.Ua.InitializeHpSpBar();
        // 初始化事件：Ue
        unit.Ue.OnUnitUpdate += (unit, updateTime) => unit.Ua.HealHp(updateTime * unit.Ua.MaxHp / 10000);
        unit.Ue.OnUnitUpdate += (unit, updateTime) => unit.Ua.GetMp(updateTime * unit.Ua.Mag / 1000);
        unit.UnitAi = new UnitAi(unit);
        foreach (var gskill in Skill.SkillDeck)
        {
            if (gskill.SkillGroup == "")
                unit.Us.LearnSkill(gskill);
        }
        unit.TimeEnergy = 0;
        unit.UnitAi = null;
        zoom /= (Setting.imagePx / 16);
        playerCamera = new Camera2D
        {
            Position = Vector2.Zero,
            Zoom = new Vector2(zoom, zoom)
        };
        unit.Up.sprite.AddChild(playerCamera);  // 加到角色节点下
        playerCamera.MakeCurrent();  // 设置为当前摄像机
        playerCamera.Position = new Vector2I(Setting.imagePx/2, Setting.imagePx/2);
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
                    G.I.Fsm.ChangeState(Fsm.PlayerSkillState);
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
            if (key == Key.Kp5 || key == Key.R)
            {
                if (Skill.NameSkill.TryGetValue("Rest", out var restSkill))
                {
                    if (PlayerUnit.Us.currentSpellcard == null)
                        PlayerUnit.Ua.GetSp(-5);
                    restSkill.Activate(new SkillContext(PlayerUnit, PlayerUnit.Up.CurrentGrid));
                }
                return;
            }
            if (key == Key.Kp0 || key == Key.Space)
            {
                if (Skill.NameSkill.TryGetValue("Rest", out var restSkill))
                {
                    PlayerUnit.Ua.GetSp(10);
                    restSkill.Activate(new SkillContext(PlayerUnit, PlayerUnit.Up.CurrentGrid));
                }
                return;
            }
            if (key == Key.Down)
            {
                if(PlayerUnit.Up.CurrentGrid.TerrainBaseGround == "Stair")
                {
                    Scene.LeaveAndGo();
                }
                return;
            }
            if (BoxEntryKeys.TryGetValue(key, out var entry))
            {
                G.I.Fsm.ChangeState(BoxEntryKeys[key]);
            }
            if (key == Key.G)
            {
                Skill.NameSkill["Interact"].Activate(new SkillContext(PlayerUnit, PlayerUnit.Up.CurrentGrid));
                return;
            }

            // 处理方向输入
            if (MoveDirs.TryGetValue(key, out Vector2I dir) || MoveDirs2.TryGetValue(key, out dir))
            {
                var targetPos = PlayerUnit.Up.Position + dir;
                IInteractable grid = Scene.CurrentMap.GetGrid(targetPos);
                grid?.Interact(PlayerUnit);
            }
        }
    }

    public void RegisterToG(G g)
    {
        g.Player = this;
	}
}
