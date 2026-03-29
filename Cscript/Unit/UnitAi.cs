using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
public enum AiState
{
    Sleep, // 空闲状态
    Attack, // 攻击状态
    Track, // 移动状态
}
public enum AiMode
{
    Sleep,
    Attack,
}
public class UnitAi(Unit u)
{
    public Unit target = Player.PlayerUnit;
    public int friendness = u.Friendness;
    public int bestDistance;
    private Unit _parent = u;
    public Dictionary<string, float> Skills { get; set; } = [];
    public int PlayerDist => (int)Math.Round((_parent.Up.Position - target.Up.Position).Length());
    public AiState State { get; set; } = AiState.Sleep;
    public AiMode Mode { get; set; } = AiMode.Attack;
    public Vector2I PlayerPosition;
    public void Update()
    {
        if (Mode == AiMode.Sleep)
        {
            SleepAi(); return;
        }
        switch (State)
        {
            case AiState.Sleep:
                break;
            case AiState.Attack:
                if (!_parent.Up.GridInVision().Contains(target.Up.CurrentGrid))
                    State = AiState.Track;
                FindTarget();
                break;
            case AiState.Track:
                if (_parent.Up.GridInVision().Contains(target.Up.CurrentGrid))
                {
                    State = AiState.Attack; break;
                }
                if(_parent.Up.Position == target.Up.Position)
                {
                    _parent.UnitAi.SleepAi();
                    Scene.CurrentMap.WakeUnits.Remove(_parent);
                }
                    
                break;
        }
        switch (State)
        {
            case AiState.Sleep:
                SleepAi();
                break;
            case AiState.Attack:
                PlayerPosition = target.Up.Position;
                AttackAi(1, 3);
                break;
            case AiState.Track:
                TrackAi();
                break;
        }
    }
    public void AttackAi(float attackValue, float tacticMoveValue)
    {
        var skills = _parent.Us.skills;
        int bestDistance = _parent.UnitAi.bestDistance;
        // 2. 构建候选技能列表（技能+普通攻击+战术移动）
        List<(object, float weight)> candidates = new();

        foreach (var lvp in skills)
        {
            var skill = lvp.skill;
            float weight = lvp.weight;
            if (CanUse(skill.Name))
            {
                candidates.Add((skill, weight));
            }
        }
        if (CanUse("Attack"))
            candidates.Add(("Attack", attackValue));
        if (CanUse("TacticMove", bestDistance) && _parent.Ue.CheckMoveUsage(true))
            candidates.Add(("TacticMove", tacticMoveValue));
        // 3. 按权重随机选择一个使用
        if (candidates.Count > 0)
        {
            float totalWeight = 0;
            foreach (var lvp in candidates)
            {
                totalWeight += lvp.weight;
            }
            float choice = (float)GD.RandRange(0, totalWeight);
            float cumulative = 0;

            foreach (var (use, weight) in candidates)
            {
                cumulative += weight;
                if (choice <= cumulative)
                {
                    if (use is string s)
                    {
                        if ((string)use == "TacticMove") { TacticMove(); return; }
                        if ((string)use == "Attack") { _parent.Us.GetSkill("Attack").Use(SelectTarget(_parent.Us.GetSkill("Attack"))); return; }
                    }
                    else if(use is Skill si)
                        si.Use(SelectTarget(si));
                    return;
                }
            }
        }
        _parent.UnitAi.SleepAi();

        void TacticMove()
        {
            List<Vector2I> moves = [];
            foreach (var dir in Directions)
            {
                if (Math.Abs((int)Math.Round((_parent.Up.Position + dir - target.Up.Position).Length())
                    - bestDistance) < Math.Abs(PlayerDist - bestDistance))
                {
                    var gd = Scene.CurrentMap.GetGrid(_parent.Up.Position + dir);
                    if (gd != null && gd.IsWalkable && gd.unit == null)
                        moves.Add(dir);
                }
            }
            var grid = Scene.CurrentMap.GetGrid(_parent.Up.Position + moves[GD.RandRange(0, moves.Count - 1)]);
            Skill.NameSkill["Move"].Activate(new SkillContext(_parent, grid));
        }
    }
    public bool CanUse(string name, int bestDistance = 0)
    {
        switch (name)
        {
            case "Attack":
                if (PlayerDist == 1)
                    return true;
                break;
            case "TacticMove":
                foreach (var dir in Directions)
                {
                    if(Math.Abs((int)Math.Round((_parent.Up.Position + dir - target.Up.Position)
                        .Length()) - bestDistance) < Math.Abs(PlayerDist - bestDistance))
                    {
                        var grid = Scene.CurrentMap.GetGrid(_parent.Up.Position + dir);
                        if (grid != null && grid.IsWalkable && grid.unit == null)
                            return true;
                    }
                }
                break;
            default:
                return _parent.Us.GetSkill(name).CanUse(_parent) && 
                       _parent.Up.CheckSkillTarget(_parent.Us.GetSkill(name), target.Up.Position) == HighlightType.green;
        }
        return false;
    }
    public void FindTarget()
    {
        for(int i = 1; i <= _parent.Up.Vision; i++)
        {
            Unit t = _parent.Up.CurrentGrid.NearGrids(i).FirstOrDefault(x => x.unit != null && !x.unit.IsFriend(_parent)).unit;
            if (t != null)
            {
                target = t;
                State = AiState.Attack;
                return;
            }
        }
    }
    public SkillContext SelectTarget(Skill skill)
    {
        SkillContext sc = skill.GetTargeting().TargetRule.GetSc(
                    _parent, target.Up.CurrentGrid);
        sc.Level = skill.Level;
        return sc;
    }
    
    public void SleepAi()
    {
        Sleep().Activate(new SkillContext(_parent));
    }
    public static Skill Sleep()
    {
        return Skill.NameSkill["Rest"];
    }
    public static readonly List<Vector2I> Directions =
    [
        new Vector2I(0, -1),   // 上
        new Vector2I(1, -1),   // 右上
        new Vector2I(1, 0),    // 右
        new Vector2I(1, 1),    // 右下
        new Vector2I(0, 1),    // 下
        new Vector2I(-1, 1),   // 左下
        new Vector2I(-1, 0),   // 左
        new Vector2I(-1, -1),  // 左上
    ];
    public void TrackAi()
    {
        if (_parent.Up.Position == PlayerPosition) { State = AiState.Sleep; SleepAi(); return; }
        float playerDist = (_parent.Up.Position - PlayerPosition).Length();
        Vector2I direction = Vector2I.Zero;
        foreach (var dir in Directions)
        {
            Vector2I nextPosition = _parent.Up.Position + dir;
            Grid nextGrid = Scene.CurrentMap.GetGrid(nextPosition);
            if (nextGrid != null && nextGrid.IsWalkable && nextGrid.unit == null)
            {   
                float nextDist = (nextPosition - PlayerPosition).Length();
                if (nextDist < playerDist)
                {
                    direction = dir;    
                    playerDist = nextDist;
                }
            }
        }
        if (direction != Vector2I.Zero)
        {
            Skill.NameSkill["Move"].Activate(new SkillContext(_parent, Scene.CurrentMap.GetGrid(_parent.Up.Position + direction)));
        }
            
        else
        {
            State = AiState.Sleep;
            SleepAi();
        } 
    }
}
