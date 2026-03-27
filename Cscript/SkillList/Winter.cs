using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using static MathEx;
using static System.Net.Mime.MediaTypeNames;
/// <summary>
/// 寒冬系技能
/// 包含：延长的冬日、花之凋零、波状光、北极的胜利者、符卡：寒潮
/// </summary>
public class LingeringCold : Skill
{
    public LingeringCold()
    {
        SkillGroup = "Winter";
        SpCost = 5;
        Cooldown = 500;
    }
    
    // 子弹数量：7/9/11/11
    int[] bulletCount = { 7, 9, 11, 11 };
    // 状态延长回合：1/1/1/2
    int[] extendTurns = { 100, 100, 100, 200 };
    
    public override string GetDescription()
    {
        return string.Format(EffectTr(), bulletCount[iLevel], extendTurns[iLevel]);
    }
    
    public override TargetType GetTargeting()
    {
        return new TargetType(new TargetRuleAny(), 1, 10);
    }
    
    protected override void StartActivate(SkillContext sc)
    {
        int count = bulletCount[iLevel];
        int range = 10;

        var c = (count - 1) / 2;
        // 发射子弹
        for (int i = 0; i < c; i++)
        {
            var percent = (i + 1) / c;
            var x = 1 - percent;
            float angle = 30*percent;
            float speed = 1.5f + 3 * percent;
            
            Bullet.CreateBullet(sc.User, this, new Damage(10, DamageType.cold), 
                sc.User.Up.Position, sc.GridOne.Position, new Vector2(x, 0), 
                angle, speed, range, ShapeBullet.Ring, ColorBullet.Cyan);
            Bullet.CreateBullet(sc.User, this, new Damage(10, DamageType.cold),
                sc.User.Up.Position, sc.GridOne.Position, new Vector2(x, 0),
                -angle, speed, range, ShapeBullet.Ring, ColorBullet.Cyan);
        }
        Bullet.CreateBullet(sc.User, this, new Damage(15, DamageType.cold),
                sc.User.Up.Position, sc.GridOne.Position, new Vector2(1, 0),
                0, 1.5f, range, ShapeBullet.Medium, ColorBullet.Cyan);
    }
    
    public override void AwakeBullet(SkillContext sc, Bullet bullet)
    {
        // 命中时延长目标随机一项负面状态
        if (sc.UnitOne != null && sc.UnitOne.Status.Count > 0)
        {
            var debuffs = sc.UnitOne.Status.Where(s => s.Type == StatusType.Negative).ToList();
            if (debuffs.Count > 0)
            {
                var randomDebuff = debuffs[GD.RandRange(0, debuffs.Count - 1)];
                randomDebuff.Duration += extendTurns[iLevel];
            }
        }
    }
}
public class FlowerWitherAway : Skill
{
    public FlowerWitherAway()
    {
        SkillGroup = "Winter";
        SpCost = 10;
        MpCost = 5;
        Cooldown = 2200;
        Targeting = new TargetType(new TargetRuleSelf(), 1, 3);
    }
    
    // 伤害：10/20/20/20
    int[] damage = { 5, 5, 5, 10 };
    // 体质降低：18/15/12/12
    int[] conReduce = { 18, 15, 12, 12 };
    // 持续回合：3/4/5/5
    int[] duration = { 3, 4, 5, 5 };
    // 范围：3/4/5/5
    int[] aoeRange = { 3, 4, 5, 5 };
    
    public override string GetDescription()
    {
        return string.Format(EffectTr(), damage[iLevel], conReduce[iLevel], duration[iLevel]);
    }
    
    public override TargetType GetTargeting()
    {
        return new TargetType(new TargetRuleAny(), 1, aoeRange[iLevel]);
    }
    
    protected override void StartActivate(SkillContext sc)
    {
        int dmg = damage[iLevel];
        int con = conReduce[iLevel];
        int dur = duration[iLevel];
        
        // 对范围内所有其他单位造成效果
        foreach (var grid in sc.GridsTarget)
        {
            if (grid.unit != null && grid.unit != sc.User)
            {
                grid.unit.GetStatus(new SFlowerWitherAway(damage[iLevel], conReduce[iLevel], duration[iLevel], sc.User, this));
            }
        }
    }
}
public class UndulationRay : Skill
{
    public UndulationRay()
    {
        SkillGroup = "Winter";
        SpCost = 5;
        Cooldown = 1200;
    }
    
    // 子弹数量：8/12/16/16
    int[] bulletCount = { 8, 12, 16, 16 };
    // 减速概率：30%/60%/60%/60%
    int[] slowChance = { 30, 30, 30, 60 };
    // 冷却时间：12/10/8/8
    int[] cooldowns = { 1200, 1000, 800, 800 };
    
    public override string GetDescription()
    {
        return string.Format(EffectTr(), bulletCount[iLevel], slowChance[iLevel]);
    }
    
    public override TargetType GetTargeting()
    {
        return new TargetType(new TargetRuleAny(), 1, 8);
    }
    
    protected override void StartActivate(SkillContext sc)
    {
        int count = bulletCount[iLevel];
        int range = 8;
        
        // 发射可穿透的激光子弹
        for (int i = 0; i < count; i++)
        {
            float angle = -20 + (40f / (count - 1)) * i;
            float speed = 4.0f;
            
            var bullet = Bullet.CreateBullet(sc.User, this, new Damage(15, DamageType.cold), 
                sc.User.Up.Position, sc.GridOne.Position, new Vector2(0, 0), 
                angle, speed, range, ShapeBullet.Laser, ColorBullet.Cyan);
            // 设置穿透
            bullet.Piercing = true;
        }
    }
    
    public override void AwakeBullet(SkillContext sc, Bullet bullet)
    {
        // 命中时有概率使目标减速
        if (sc.UnitOne != null && GD.RandRange(0, 100f) < slowChance[iLevel])
        {
            sc.UnitOne.GetStatus(new Slow(300));
        }
    }
}
public class NorthernWinner : Skill
{
    public NorthernWinner()
    {
        SkillGroup = "Winter";
        EffectType = EffectType.Passive;
    }
    
    // 寒冷抗性：200%
    int coldResist = 200;
    // 寒冷增益：20%/30%/40%/40%
    int[] coldBoost = { 20, 30, 40, 40 };
    // 冰冻概率：6%/9%/12%/12%
    int[] freezeChance = { 6, 9, 12, 12 };
    private Func<Unit, Unit, Damage, Damage> Freeze;
    public override string GetDescription()
    {
        return string.Format(EffectTr(), coldResist, coldBoost[iLevel], freezeChance[iLevel]);
    }
    
    public override TargetType GetTargeting()
    {
        return new TargetType(new TargetRuleSelf(), 1, 0);
    }

    public override void OnLoad(Unit unit)
    {
        Freeze = (user, target, damage) =>
        {
            if (damage.Type == DamageType.cold && GD.RandRange(0, 100f) < freezeChance[iLevel])
            {
                if (Level == 4 && GD.Randf() < 0.5f)
                    target.GetStatus(new SFlowerWitherAway(5, 18, 200, unit, this));
                else
                    target.GetStatus(new Frozen(200));
            }
            return damage;
        };
        unit.Ua.DamageTypeSet += new DamageTypeSet(DamageType.cold, coldBoost[iLevel] / 100f, 1);
        unit.Ua.DamageTypeSet += new DamageTypeSet(DamageType.cold, 2, -1);
        unit.Ue.OnDealBodyDamage.Add(Freeze);
        unit.Ue.OnDealBulletDamage.Add(Freeze);
    }
    public override void OnLevelUp(Unit unit)
    {
        if (Level <= 3)
            unit.Ua.DamageTypeSet += new DamageTypeSet(DamageType.cold, 0.1f, 1);
    }
}
public class ColdSnap : SpellCard
{
    public ColdSnap()
    {
        SkillGroup = "Winter";
        SpCost = 40;
        Cooldown = 3000;
        Duration = 6;
    }
    
    // 子弹数量：100/125/150/150
    int[] bulletCount = { 120, 144, 168, 168 };
    // 冰冻概率：30%/45%/60%/60%
    int[] freezeChance = { 30, 45, 60, 60 };
    
    public override string GetDescription()
    {
        return string.Format(EffectTr(), bulletCount[iLevel], freezeChance[iLevel]);
    }
    
    public override TargetType GetTargeting()
    {
        return new TargetType(new TargetRuleSelf(), 1, 10);
    }
    protected override void OnSpellStart(SkillContext sc)
    {
        Info.Print($"{sc.User.TrName} 展开了 {TrName} ！");
        AddTimedEvent(Linspace(50, 600, 12), (ctx, advanceTime) =>
        {
            var a = TimeElapsed * 1.67f / 57.3f;
            var offset = 1.5f * new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            for (int i = 0; i < bulletCount[iLevel]/24; i++)
            {
                var off = offset + RandomV2(1);
                var bullet = Bullet.CreateBullet(sc.User, this, new Damage(10, DamageType.cold), 
                    sc.User.Up.Position + offset, sc.User.Up.Position + 2*offset, 
                    RandomV2(1), a+(float)GD.RandRange(-10f, 10f), (float)GD.RandRange(2.0, 3.0), 10,
                    ShapeBullet.Small, ColorBullet.Blue, advanceTime);
                bullet.OverrideActive += OverrideActive;
            }
            offset = 1.5f * new Vector2(-Mathf.Cos(a), -Mathf.Sin(a));
            for (int i = 0; i < bulletCount[iLevel] / 24; i++)
            {
                var off = offset + RandomV2(1);
                var bullet = Bullet.CreateBullet(sc.User, this, new Damage(10, DamageType.cold),
                    sc.User.Up.Position + offset, sc.User.Up.Position + 2 * offset,
                    RandomV2(1), (float)GD.RandRange(-10f, 10f), (float)GD.RandRange(2.0, 3.0), 10,
                    ShapeBullet.Small, ColorBullet.Ice, advanceTime);
            }
        });
    }
    public static void OverrideActive(Bullet bullet, Unit target)
    {
        bullet.skill.AwakeBullet(new SkillContext(bullet.creator, target), bullet);
        target.Ua.TakeBulletDamage(bullet.damage, bullet.creator, bullet.skill, bullet.crit);
        bullet.skill.ActivateBullet(new SkillContext(bullet.creator, target), bullet);
        if (!bullet.Piercing)
            bullet.Destroy(3);
    }
    protected override void OnSpellUpdate(SkillContext sc, float delta)
    {

    }
    public override void ActivateBullet(SkillContext sc, Bullet bullet)
    {
        if (GD.Randf() < new float[] { 0.2f, 0.3f, 0.4f, 0.4f }[iLevel])
            sc.UnitOne.GetStatus(new Frozen(new int[] { 400, 500, 600, 600 }[iLevel]));
    }
}