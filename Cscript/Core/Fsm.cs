using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
public static class Game
{
    public static bool IsLoaded { get; set; } = false;
}
public partial class Fsm : Node, IRegisterToG
{
    public GameState currentState;
    public static GameState startState;
    public static GameState playerSkillState;
    public static GameState playerSkillTargetState;
    public static GameState enemySkillState;
    public static GameState updateState;
    public static GameState talkState;
    public static GameState inventoryBoxState;
    public static GameState combatAbilityBoxState;
    public static GameState uaSkillBoxState;
    public static GameState memoryBoxState;
    [Export]
    public Control InventoryBos;
    [Export]
    public CombatAbilityBox Cab;
    [Export]
    public Control Usbb;
    public void ChangeState(GameState newState)
    {
        currentState?.End();
        currentState = newState;
        currentState.Start();
    }

    public override void _Process(double delta)
    {
        currentState?.Update();
    }

    // 示例：初始化
    public override void _Ready()
    {
        TranslationServer.SetLocale("zh");
        startState = new StartState(this);
        playerSkillState = new PlayerSkillState(this);
        playerSkillTargetState = new PlayerSkillTargetState(this);
        enemySkillState = new EnemySkillState(this);
        updateState = new UpdatingState(this);
        talkState = new TalkState(this);
        inventoryBoxState = new InventoryState(this);
        combatAbilityBoxState = new CombatAbilityBoxState(this);
        uaSkillBoxState = new UaSkillBoxState(this);
        memoryBoxState = new MemoryBoxState(this);
        ChangeState(talkState);
    }

    public void RegisterToG(G g)
    {
        g.Fsm = this;
    }
}



public interface IGameState
{
    public void Start();
    public void Update();
    public void End();
}

public abstract class GameState(Fsm fsm): IGameState
{
    protected Fsm fsm = fsm;
    public event Action OnStart;
    public event Action OnEnd;
    public event Action OnUpdate;
    public virtual void Start()
    {
        OnStart?.Invoke();
    }
    public virtual void Update()
    {
        OnUpdate?.Invoke();
    }
    public virtual void End()
    {
        OnEnd?.Invoke();
    }
}
public class StartState(Fsm fsm) : GameState(fsm), IGameState
{
    public override void Update()
    {
        if(!Game.IsLoaded)
        {
            // 内部资源加载
            EnemyLoader.LoadEnemies();
            Skill.LoadSkillDeck();
            ItemLoader.LoadAllItems();
            SpriteManager.LoadSkills();
            Game.IsLoaded = true;
        }
        ItemLoader.LoadItemPng();
        
        // 跳转到更新状态
        fsm.ChangeState(Fsm.updateState);
    }
}
public class PlayerSkillState(Fsm fsm) : GameState(fsm), IGameState
{
    public event Action<InputEvent> OnInput;
    public override void Start()
    {
        G.I.SkillBar.UpdateSkillCooldowns();
        //GD.Print("现在是玩家输入状态");
    }
    public void HandleInput(InputEvent input)
    {
        OnInput?.Invoke(input);
    }
}
public class PlayerSkillTargetState(Fsm fsm) : GameState(fsm), IGameState
{
    public override void Update()
    {
        G.I.HighlightManager.Process();
    }
}
public class EnemySkillState(Fsm fsm) : GameState(fsm), IGameState
{
    public override void Update()
    {
        if (Scene.CurrentMap.ActiveUnit.unitAi != null)
        {
            Scene.CurrentMap.ActiveUnit.unitAi.Update();
        }
        else
        {
            Skill.NameSkill["Rest"].Activate(new SkillContext(Scene.CurrentMap.ActiveUnit));
        }
    }
}
public class UpdatingState(Fsm fsm) : GameState(fsm), IGameState
{
    public override void Update()
    {
        Unit activeUnit = GameTime.Update(null, GameTime.timePerFrame);
        if (activeUnit == null)
        {
        }
        else if (activeUnit == Player.PlayerUnit)
        {
            G.I.Fsm.ChangeState(Fsm.playerSkillState);
        }
        else
        {
            Scene.CurrentMap.ActiveUnit = activeUnit;
            G.I.Fsm.ChangeState(Fsm.enemySkillState);
            //activeUnit.unitAi?.AttackAi(1, 3);
        }
    }
}

public class TalkState(Fsm fsm) : GameState(fsm), IGameState
{

}
public class InventoryState(Fsm fsm) : GameState(fsm), IGameState
{
    public override void Start()
    {
        fsm.InventoryBos.Visible = true;
        ((InventoryBox)fsm.InventoryBos).Refresh(Player.PlayerUnit);

    }
    public override void End()
    {
        fsm.InventoryBos.Visible = false;
    }
}
public class CombatAbilityBoxState(Fsm fsm) : GameState(fsm), IGameState
{
    public override void Start()
    {
        fsm.Cab.Update(Player.PlayerUnit);
        fsm.Cab.Visible = true;
    }
    public override void End()
    {
        fsm.Cab.Visible = false;
    }
}
public class UaSkillBoxState(Fsm fsm) : GameState(fsm), IGameState
{
    public override void Start()
    {
        G.I.SkillPanel.Refresh();
        G.I.Ua.Refresh();
        fsm.Usbb.Visible = true;
    }
    public override void End()
    {
        fsm.Usbb.Visible = false;
    }
}
public class MemoryBoxState(Fsm fsm) : GameState(fsm), IGameState
{
    public override void Start()
    {
        G.I.MemoryBox.Refresh(Player.PlayerUnit);
        G.I.MemoryBox.Visible = true;

    }
    public override void End()
    {
        G.I.MemoryBox.Visible = false;
    }
}