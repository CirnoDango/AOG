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
        Name = "AsteroidBelt";
        SkillGroup = "Star";
        SpCost = 3;
        Cooldown = 400;
        Targeting = new TargetType(Target.Self, 0, 5);
    }
    int[] t0 = [18, 24, 30, 30];
    int[] t1 = [50, 50, 50, 100];
    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), t0[level - 1], t1[level - 1]);
    }
    public override TargetType GetTargeting(int level)
    {
        return new TargetType(Target.Self, 0, new int[] { 4, 5, 6, 6 }[level - 1]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        for (int i = 0; i < new int[] { 18, 24, 30, 30 }[sc.level - 1]; i++)
        {
            _ = new Bullet(sc.User, this, 4, sc.User.Position, sc.User.Position + RandomV2(),
                (float)GD.RandRange(0.8f, 1.2f), new int[] { 4, 5, 6, 6 }[sc.level - 1],
                "Star", (ColorBullet)(new List<int> { 0, 2, 5, 7, 9, 12, 13 })[GD.RandRange(0, 6)]);
            _ = new Bullet(sc.User, this, 4, sc.User.Position, sc.User.Position + RandomV2(),
                (float)GD.RandRange(0.8f, 1.2f), new int[] { 4, 5, 6, 6 }[sc.level - 1],
                "Small", (ColorBullet)(new List<int> { 0, 2, 5, 7, 9, 12, 13 })[GD.RandRange(0, 6)]);
        }
    }
    public override void ActivateBullet(SkillContext sc, Bullet bullet)
    {
        sc.UnitOne.TakeBulletDamage(bullet.damage * new float[] { 0.5f, 0.5f, 0.5f, 1 }[sc.level - 1], sc.User, this);
    }
}
public class MilkyWay : Skill
{
    public MilkyWay()
    {
        Name = "MilkyWay";
        SkillGroup = "Star";
        EffectType = EffectType.Passive;
    }
    private Action<Unit> onMoveHandler;
    int[] t0 = [12, 18, 24, 24];
    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), t0[level - 1]);
    }
    public override void OnLearn(Unit unit)
    {
        onMoveHandler = (movingUnit) =>
        {
            if (movingUnit != unit) return;
            if (GD.Randf() < new float[] { 0.12f, 0.18f, 0.24f,0.24f }[unit.GetSkill(Name).Level - 1])
            {
                foreach (Grid g in unit.GridInVision())
                {
                    if (g.unit != null && g.unit.friendness * unit.friendness < 0)
                    {
                        _ = new Bullet(unit, this, 10, unit.Position, g.Position, Vector2I.Zero, -10,
                        3, 12, "Star", ColorBullet.Yellow);
                        _ = new Bullet(unit, this, 10, unit.Position, g.Position, Vector2I.Zero, 0,
                        3, 12, "Star", ColorBullet.Yellow);
                        _ = new Bullet(unit, this, 10, unit.Position, g.Position, Vector2I.Zero, 10,
                        3, 12, "Star", ColorBullet.Yellow);
                        return;
                    }
                }
            }
        };

        GameEvents.OnUnitMove += onMoveHandler;
    }
}
public class BlazingStar : Skill
{
    public BlazingStar()
    {
        Name = "BlazingStar";
        SkillGroup = "Star";
        MpCost = 5;
        Cooldown = 1200;
    }
    int[] t0 = [6, 8, 10, 10];
    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), t0[level - 1]);
    }
    public override TargetType GetTargeting(int level)
    {
        return Targeting = new TargetType(Target.Dash, 1, new int[] { 6, 8, 10, 10 }[level - 1]);
    }
    public override float GetSpCost(int level)
    {
        return new int[] { -5, -5, -5, 5 }[level - 1];
    }
    protected override void StartActivate(SkillContext sc)
    {
        sc.UnitOne.CheckBodyHit(30, sc.User, this);
        List<Grid> grids = sc.User.DashCheck(sc.UnitOne.CurrentGrid);
        grids.RemoveAt(grids.Count - 1);
        foreach (var grid in grids)
        {
            sc.User.MoveTo(grid);
        }
        for (int i = 0; i < 20; i++)
        {
            _ = new Bullet(sc.User, this, 8, sc.User.Position, sc.User.Position + RandomV2(),
                (float)GD.RandRange(1.5f, 3.0f), 6,
                "Star", (ColorBullet)(new List<int> { 0, 2, 5, 7, 9, 12, 13 })[GD.RandRange(0, 6)]);
        }
    }
}
public class DragonMeteor: Skill
{
    public DragonMeteor()
    {
        Name = "DragonMeteor";
        SkillGroup = "Star";
        Description = "召唤一颗陨石，对3格内敌人造成40弹幕伤害";
        SpCost = -5;
        MpCost = 10;
        Cooldown = 1600;
    }
    int[] t0 = [2,3,4,4];
    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), t0[level - 1]);
    }
    public override TargetType GetTargeting(int level)
    {
        return new TargetType(Target.Grid, 1, 8, new int[] { 2, 3, 4, 4 }[level - 1]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        foreach(var grid in sc.GridOne.NearGrids(new int[] { 2, 3, 4, 4 }[sc.level - 1]))
        {
            if(grid.unit != null)
                grid.unit.TakeBulletDamage(40, sc.User, this);
        }
    }
}
public class StardustReverie : SpellCard
{
    public StardustReverie()
    {
        Name = "StardustReverie";
        SkillGroup = "Star";
        Description = "持续发出大量星弹，发动时移动速度+100%";
        SpNeed = 50;
        SpCost = 6;
        Cooldown = 3000;
        Duration = 500;
        Targeting = new TargetType(Target.Enemy, 1, 12);
    }
    int[] t0 = [150, 190, 230, 230];
    int[] t1 = [50, 50, 50, 100];
    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), t0[level - 1], t1[level - 1]);
    }
    protected override void OnSpellStart(SkillContext sc)
    {
        Info.Print($"{sc.User.TrName} 展开了 {TrName} ！");
        AddTimedEvent(Linspace(20, 500, new int[] { 70,110,150,150 }[sc.level - 1]), (ctx, advanceTime) =>
        {
            var bullet = new Bullet(sc.User, this, 12, sc.User.Position, sc.User.Position + RandomV2(),
                new Vector2(0, 0), Vector2.Right, (float)GD.RandRange(2.0, 6.0), 12,
                "Star", (ColorBullet)(new List<int> { 0, 2, 5, 7, 9, 12, 13 })[GD.RandRange(0, 6)], advanceTime);
        });
        AddTimedEvent(Linspace(100, 500, 5), (ctx, advanceTime) =>
        {
            for(float a = 0; a<360; a += 360 / 16f)
            {
                var bullet = new Bullet(sc.User, this, 12, sc.User.Position, sc.UnitOne.Position,
                new Vector2(0, 0), a, 3, 12,
                "Star", ColorBullet.Yellow, advanceTime);
            }
        });
        sc.User.Ua.SpeedMove += new int[] { 50, 50, 50, 100 }[sc.level - 1];
    }
    public override void OnSpellEnd(SkillContext sc)
    {
        sc.User.Ua.SpeedMove -= new int[] { 50, 50, 50, 100 }[sc.level - 1];
        base.OnSpellEnd(sc);
    }
}


