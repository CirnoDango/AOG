using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using static MathEx;
public class AsteroidBelt : Skill
{
    public AsteroidBelt()
    {
        SkillGroup = "Star";
        SpCost = 3;
        Cooldown = 400;
        Targeting = new TargetType(new TargetRuleSelf(), 0, 5);
    }
    int[] t0 = [18, 24, 30, 30];
    int[] t1 = [50, 50, 50, 100];
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], t1[iLevel]);
    }
    public override TargetType GetTargeting()
    {
        return new TargetType(new TargetRuleSelf(), 0, new int[] { 4, 5, 6, 6 }[iLevel]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        for (int i = 0; i < new int[] { 18, 24, 30, 30 }[iLevel]; i++)
        {
            Bullet.CreateBullet(sc.User, this, new Damage(4, DamageType.celestial), sc.User.Up.Position, sc.User.Up.Position + RandomV2(),
                (float)GD.RandRange(0.8f, 1.2f), new int[] { 4, 5, 6, 6 }[iLevel],
                ShapeBullet.Star, (ColorBullet)(new List<int> { 0, 2, 5, 7, 9, 12, 13 })[GD.RandRange(0, 6)]);
            Bullet.CreateBullet(sc.User, this, new Damage(4, DamageType.celestial), sc.User.Up.Position, sc.User.Up.Position + RandomV2(),
                (float)GD.RandRange(0.8f, 1.2f), new int[] { 4, 5, 6, 6 }[iLevel],
                ShapeBullet.Micro, (ColorBullet)(new List<int> { 0, 2, 5, 7, 9, 12, 13 })[GD.RandRange(0, 6)]);
        }
    }
    public override void ActivateBullet(SkillContext sc, Bullet bullet)
    {
        sc.UnitOne.Ua.TakeBulletDamage(bullet.damage * new float[] { 0.5f, 0.5f, 0.5f, 1 }[iLevel], sc.User, this);
    }
}
public class MilkyWay : Skill
{
    public MilkyWay()
    {
        SkillGroup = "Star";
        EffectType = EffectType.Passive;
    }
    private Action<Unit> onMoveHandler;
    int[] t0 = [12, 18, 24, 24];
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], TextEx.Tr(Extra()[iLevel]));
    }
    public override void OnLoad(Unit unit)
    {
        onMoveHandler = (movingUnit) =>
        {
            if (GD.Randf() < new float[] { 0.12f, 0.18f, 0.24f, 0.24f }[unit.Us.GetSkill(Name).iLevel])
            {
                if (unit.Up.RandomEnemyInVision(out Unit u))
                {
                    Bullet.CreateBullet(unit, this, new Damage(10, DamageType.celestial), unit.Up.Position, u.Up.Position, Vector2I.Zero, -10,
                        3, 12, ShapeBullet.Star, ColorBullet.Yellow);
                    Bullet.CreateBullet(unit, this, new Damage(10, DamageType.celestial), unit.Up.Position, u.Up.Position, Vector2I.Zero, 0,
                        3, 12, ShapeBullet.Star, ColorBullet.Yellow);
                    Bullet.CreateBullet(unit, this, new Damage(10, DamageType.celestial), unit.Up.Position, u.Up.Position, Vector2I.Zero, 10,
                        3, 12, ShapeBullet.Star, ColorBullet.Yellow);
                    return;
                }
            }
        };
        unit.Ue.OnUnitMove += onMoveHandler;
    }
    public override void OffLearn(Unit unit)
    {
        unit.Ue.OnUnitMove -= onMoveHandler;
    }
}
public class BlazingStar : Skill
{
    public BlazingStar()
    {
        SkillGroup = "Star";
        MpCost = 5;
        Cooldown = 1200;
    }
    int[] t0 = [6, 8, 10, 10];
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel]);
    }
    public override TargetType GetTargeting()
    {
        return Targeting = new TargetType(new TargetRuleDash(), 1, new int[] { 6, 8, 10, 10 }[iLevel]);
    }
    public override float GetSpCost()
    {
        return new int[] { -5, -5, -5, 5 }[iLevel];
    }
    protected override void StartActivate(SkillContext sc)
    {
        sc.UnitOne.Ua.CheckBodyHit(new Damage(30, DamageType.strike), sc.User, this);
        List<Grid> grids = sc.User.Up.DashCheck(sc.UnitOne.Up.CurrentGrid);
        grids.RemoveAt(grids.Count - 1);
        foreach (var grid in grids)
        {
            sc.User.Up.MoveTo(grid);
        }
        for (int i = 0; i < 20; i++)
        {
            Bullet.CreateBullet(sc.User, this, new Damage(8, DamageType.celestial), sc.User.Up.Position, sc.User.Up.Position + RandomV2(),
                (float)GD.RandRange(1.5f, 3.0f), 6,
                ShapeBullet.Star, (ColorBullet)(new List<int> { 0, 2, 5, 7, 9, 12, 13 })[GD.RandRange(0, 6)]);
        }
    }
}
public class DragonMeteor: Skill
{
    public DragonMeteor()
    {
        SkillGroup = "Star";
        Description = "召唤一颗陨石，对3格内敌人造成40弹幕伤害";
        SpCost = -5;
        MpCost = 10;
        Cooldown = 1600;
    }
    int[] t0 = [2,3,4,4];
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], TextEx.Tr(Extra()[iLevel]));
    }
    public override TargetType GetTargeting()
    {
        return new TargetType(new TargetRuleGrid(), 1, 8, new int[] { 2, 3, 4, 4 }[iLevel]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        foreach(var grid in sc.GridOne.NearGrids(new int[] { 2, 3, 4, 4 }[iLevel]))
        {
            if(grid.unit != null)
                grid.unit.Ua.TakeBulletDamage(new Damage(40, DamageType.celestial), sc.User, this);
        }
    }
}
public class StardustReverie : SpellCard
{
    public StardustReverie()
    {
        SkillGroup = "Star";
        Description = "持续发出大量星弹，发动时移动速度+100%";
        SpCost = 50;
        Cooldown = 3000;
        Duration = 500;
        Targeting = new TargetType(new TargetRuleEnemy(), 1, 12);
    }
    int[] t0 = [150, 190, 230, 230];
    int[] t1 = [50, 50, 50, 100];
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], t1[iLevel]);
    }
    protected override void OnSpellStart(SkillContext sc)
    {
        Info.Print($"{sc.User.TrName} 展开了 {TrName} ！");
        AddTimedEvent(Linspace(20, 500, new int[] { 70,110,150,150 }[iLevel]), (ctx, advanceTime) =>
        {
            var bullet = Bullet.CreateBullet(sc.User, this, new Damage(12, DamageType.celestial), sc.User.Up.Position, sc.User.Up.Position + RandomV2(),
                new Vector2(0, 0), Vector2.Right, (float)GD.RandRange(2.0, 6.0), 12,
                ShapeBullet.Star, (ColorBullet)(new List<int> { 0, 2, 5, 7, 9, 12, 13 })[GD.RandRange(0, 6)], advanceTime);
        });
        AddTimedEvent(Linspace(100, 500, 5), (ctx, advanceTime) =>
        {
            for(float a = 0; a<360; a += 360 / 16f)
            {
                var bullet = Bullet.CreateBullet(sc.User, this, new Damage(12, DamageType.celestial), sc.User.Up.Position, sc.UnitOne.Up.Position,
                new Vector2(0, 0), a, 3, 12,
                ShapeBullet.Star, ColorBullet.Yellow, advanceTime);
            }
        });
        sc.User.Ua.SpeedMove += new int[] { 50, 50, 50, 100 }[iLevel];
    }
    public override void OnSpellEnd(SkillContext sc)
    {
        sc.User.Ua.SpeedMove -= new int[] { 50, 50, 50, 100 }[iLevel];
        base.OnSpellEnd(sc);
    }
}