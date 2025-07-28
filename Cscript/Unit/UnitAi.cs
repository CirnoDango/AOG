using Godot;
using System;
using System.Collections.Generic;
using static Godot.TextServer;
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
    public Unit unit = u;
    public Dictionary<string, float> Skills { get; set; } = [];
    public int PlayerDist => (int)Math.Round((unit.Position - Player.PlayerUnit.Position).Length());
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
                if (!unit.GridInVision().Contains(Player.PlayerUnit.CurrentGrid))
                    State = AiState.Track;
                break;
            case AiState.Track:
                if (unit.GridInVision().Contains(Player.PlayerUnit.CurrentGrid))
                {
                    State = AiState.Attack; break;
                }
                if(unit.Position == Player.PlayerUnit.Position)
                    unit.unitAi.SleepAi();
                break;
        }
        switch (State)
        {
            case AiState.Sleep:
                SleepAi();
                break;
            case AiState.Attack:
                PlayerPosition = Player.PlayerUnit.Position;
                AttackAi(1, 3);
                break;
            case AiState.Track:
                TrackAi();
                break;
        }
    }
    public void AttackAi(float attackValue, float tacticMoveValue)
    {
        var skills = unit.skills;
        int bestDistance = 1;
        double bestScore = double.MinValue;

        // 1. 计算优势距离（1~32）以最大化技能收益
        for (int dist = 1; dist <= 32; dist++)
        {
            double score = 0;
            foreach (var lvp in skills)
            {
                var skill = lvp.skill;
                if (skill.Template.EffectType == EffectType.Passive || skill.Targeting.Range == -1)
                    continue;
                float weight = lvp.weight;
                int skillDistance = skill.Targeting.Range;
                if (skillDistance == 0) { continue; }
                double diff = Math.Abs(dist - skillDistance) + 0.3;
                score += weight / (diff * diff);
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestDistance = dist;
            }
        }
        bestDistance /= 2;

        // 2. 构建候选技能列表（技能+普通攻击+战术移动）
        List<(string, float weight)> candidates = new();

        foreach (var lvp in skills)
        {
            var skill = lvp.skill;
            float weight = lvp.weight;
            if (CanUse(skill.Name))
            {
                candidates.Add((skill.Name, weight));
            }
        }
        if (CanUse("Attack"))
            candidates.Add(("Attack", attackValue));
        if (CanUse("TacticMove", bestDistance))
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
                    if (use == "TacticMove") { TacticMove(); return; } 
                    unit.GetSkill(use).Use(SelectTarget(unit.GetSkill(use)));
                    return;
                }
            }
        }
        unit.unitAi.SleepAi();

        void TacticMove()
        {
            List<Vector2I> moves = [];
            foreach (var dir in Directions)
            {
                if (Math.Abs((int)Math.Round((unit.Position + dir - Player.PlayerUnit.Position).Length())
                    - bestDistance) < Math.Abs(PlayerDist - bestDistance))
                {
                    var gd = Scene.CurrentMap.GetGrid(unit.Position + dir);
                    if (gd != null && gd.IsWalkable && gd.unit == null)
                        moves.Add(dir);
                }
            }
            var grid = Scene.CurrentMap.GetGrid(unit.Position + moves[GD.RandRange(0, moves.Count - 1)]);
            Skill.NameSkill["Move"].Activate(new SkillContext(unit, grid));
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
                    if(Math.Abs((int)Math.Round((unit.Position + dir - Player.PlayerUnit.Position)
                        .Length()) - bestDistance) < Math.Abs(PlayerDist - bestDistance))
                    {
                        var grid = Scene.CurrentMap.GetGrid(unit.Position + dir);
                        if (grid != null && grid.IsWalkable && grid.unit == null)
                            return true;
                    }
                }
                break;
            default:
                return unit.GetSkill(name).CanUse(unit) && unit.CheckSkillTarget(unit.GetSkill(name), Player.PlayerUnit.Position) == HighlightType.green;
        }
        return false;
    }
    public SkillContext SelectTarget(SkillInstance skill)
    {
        return skill.Template.GetTargeting(skill.Level).Type switch
        {
            Target.Enemy or Target.Unit or Target.Dash => new SkillContext(unit, Player.PlayerUnit),
            Target.Grid => new SkillContext(unit, Player.PlayerUnit.CurrentGrid),
            _ => new SkillContext(unit),
        };
    }
    
    public void SleepAi()
    {
        Sleep().Activate(new SkillContext(unit));
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
        if (unit.Position == PlayerPosition) { State = AiState.Sleep; SleepAi(); return; }
        float playerDist = (unit.Position - PlayerPosition).Length();
        Vector2I direction = Vector2I.Zero;
        foreach (var dir in Directions)
        {
            Vector2I nextPosition = unit.Position + dir;
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
            Skill.NameSkill["Move"].Activate(new SkillContext(unit, Scene.CurrentMap.GetGrid(unit.Position + direction)));
        }
            
        else
        {
            State = AiState.Sleep;
            SleepAi();
        } 
    }
}
