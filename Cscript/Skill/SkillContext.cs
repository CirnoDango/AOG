using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public enum EffectType
{
    Activate, Passive
}
public interface ITargetRule
{
    public bool MapClick { get; }
    public HighlightType CheckSkillTarget(Unit user, Grid target, Skill skill);
    public virtual SkillContext GetSc(Unit user, Grid target)
    {
        return new SkillContext(user, target.unit);
    }
}
public class TargetRuleEnemy : ITargetRule
{
    public bool MapClick => true;
    public HighlightType CheckSkillTarget(Unit user, Grid target, Skill skill)
    {
        var unit = target.unit;
        if (unit != null && unit.Friendness * user.Friendness < 0)
            return HighlightType.green;
        else
            return HighlightType.blue;
    }
}
public class TargetRuleSelf : ITargetRule
{
    public bool MapClick => false;
    public HighlightType CheckSkillTarget(Unit user, Grid target, Skill skill)
    {
        return HighlightType.green;
    }
}
public class TargetRuleUnit : ITargetRule
{
    public bool MapClick => true;
    public HighlightType CheckSkillTarget(Unit user, Grid target, Skill skill)
    {
        return target.unit != null ? HighlightType.green : HighlightType.blue;
    }
}
public class TargetRuleGrid : ITargetRule
{
    public bool MapClick => true;
    public HighlightType CheckSkillTarget(Unit user, Grid target, Skill skill)
    {
        if (target.IsWalkable)
            return HighlightType.green;
        return HighlightType.blue;
    }
    public SkillContext GetSc(Unit user, Grid target)
    {
        return new SkillContext(user, target);
    }
}
public class TargetRuleGridEmpty : ITargetRule
{
    public bool MapClick => true;
    public HighlightType CheckSkillTarget(Unit user, Grid target, Skill skill)
    {
        if (target.IsWalkable && target.unit == null)
            return HighlightType.green;
        return HighlightType.blue;
    }
    public SkillContext GetSc(Unit user, Grid target)
    {
        return new SkillContext(user, target);
    }
}
public class TargetRuleAny : ITargetRule
{
    public bool MapClick => true;
    public HighlightType CheckSkillTarget(Unit user, Grid target, Skill skill)
    {
        return HighlightType.green;
    }
    public SkillContext GetSc(Unit user, Grid target)
    {
        return new SkillContext(user, target);
    }
}
public class TargetRuleDash : ITargetRule
{
    public bool MapClick => true;
    public HighlightType CheckSkillTarget(Unit user, Grid target, Skill skill)
    {
        List<Grid> g = user.Up.DashCheck(target);
        if (g == null || g.Count == 0 || g[^1].unit == null || g[^1].unit == Player.PlayerUnit)
            return HighlightType.blue;
        return HighlightType.green;
    }
    public SkillContext GetSc(Unit user, Grid target)
    {
        return new SkillContext(user, user.Up.DashCheck(target)[^1].unit);
    }
}
public class TargetRuleRay : ITargetRule
{
    public bool MapClick => true;
    public HighlightType CheckSkillTarget(Unit user, Grid target, Skill skill)
    {
        List<Grid> gr = user.Up.RayCheck(target);
        if (gr == null || gr.Count == 0)
            return HighlightType.blue;
        return HighlightType.green;
    }
    public SkillContext GetSc(Unit user, Grid target)
    {
        return new SkillContext(user, user.Up.RayCheck(target));
    }
}
public class TargetType
{
    public ITargetRule TargetRule { get; set; }
    public int Number { get; set; }
    public int Range { get; set; }
    public int BombRange { get; set; } = 0;
    public TargetType(ITargetRule type)
    {
        // 当 type == Target.Self 时，Range的作用仅为让UnitAi判断在离玩家多近时释放该技能。因此该重载只用于AI永不使用的技能。
        TargetRule = type;
        if (type is TargetRuleSelf)
        {
            Number = 1;
            Range = 0;
        }
        else
        {
            Number = 1;
            Range = 1;
        }
    }
    public TargetType(ITargetRule type, int num, int ran, int bombrange = 0)
    {
        TargetRule = type;
        Number = num;
        Range = ran;
        BombRange = bombrange;
    }
}
public class SkillContext
{
    public Unit User { get; set; }
    public List<Unit> UnitsTarget { get; set; } = [];
    public Unit UnitOne => UnitsTarget.FirstOrDefault();
    public List<Grid> GridsTarget { get; set; } = [];
    public Grid GridOne => GridsTarget.FirstOrDefault();
    private int _level = 1;
    public int Level
    {
        get { return _level; }
        set { _level = value; }
    }
    public SkillContext(Unit user, int level = 1)
    {
        User = user;
        this.Level = level;
    }
    public SkillContext(Unit user, Unit Single, int level = 1)
    {
        User = user;
        UnitsTarget.Add(Single);
        this.Level = level;
    }
    public SkillContext(Unit user, List<Unit> targets, int level = 1)
    {
        User = user;
        UnitsTarget = targets;
        this.Level = level;
    }
    public SkillContext(Unit user, Grid Single, int level = 1)
    {
        User = user;
        GridsTarget.Add(Single);
        this.Level = level;
    }
    public SkillContext(Unit user, List<Grid> targets, int level = 1)
    {
        User = user;
        GridsTarget = targets;
        this.Level = level;
    }
    // 其他可能的参数，比如技能等级、buff状态等
}
