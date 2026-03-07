using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using static MathEx;
public class Icefall : Skill
{
    public Icefall()
    {
        SkillGroup = "Freeze";
        SpCost = 3;
        Targeting = new TargetType(new TargetRuleAny(), 1, 10);
    }
    int[] t0 = { 3, 6, 6, 6 };
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], TextEx.Tr(Extra()[iLevel]));
    }
    public override float GetCooldown()
    {
        return new float[] { 300, 300, 200, 200 }[iLevel];
    }
    protected override void StartActivate(SkillContext sc)
    {
        Bullet.CreateBullet(sc.User, this, new Damage(15, DamageType.pierce), sc.User.Up.Position, sc.GridOne.Position, 
            new Vector2(0, 1), 0, 2, 10, ShapeBullet.Bullet, ColorBullet.Ice);
        Bullet.CreateBullet(sc.User, this, new Damage(15, DamageType.pierce), sc.User.Up.Position, sc.GridOne.Position, 
            new Vector2(0, 0), 0, 2, 10, ShapeBullet.Bullet, ColorBullet.Ice);
        Bullet.CreateBullet(sc.User, this, new Damage(15, DamageType.pierce), sc.User.Up.Position, sc.GridOne.Position, 
            new Vector2(0,-1), 0, 2, 10, ShapeBullet.Bullet, ColorBullet.Ice);
        if (sc.Level >= 2)
        {
            Bullet.CreateBullet(sc.User, this, new Damage(15, DamageType.pierce), sc.User.Up.Position, sc.GridOne.Position, 
                new Vector2(0, 1), Vector2.Right, 2.5f, 10, ShapeBullet.Bullet, ColorBullet.Ice);
            Bullet.CreateBullet(sc.User, this, new Damage(15, DamageType.pierce), sc.User.Up.Position, sc.GridOne.Position, 
                new Vector2(0, 0), Vector2.Right, 2.5f, 10, ShapeBullet.Bullet, ColorBullet.Ice);
            Bullet.CreateBullet(sc.User, this, new Damage(15, DamageType.pierce), sc.User.Up.Position, sc.GridOne.Position, 
                new Vector2(0, -1), Vector2.Right, 2.5f, 10, ShapeBullet.Bullet, ColorBullet.Ice);
        }
    }
    public override void AwakeBullet(SkillContext sc, Bullet bullet)
    {
        var s = sc.UnitOne.Status.FirstOrDefault(x => x is Frozen);
        if (s != null)
        {
            bullet.damage *= 10;
            if(sc.Level == 4)
            {
                s.Duration += 200;
                sc.UnitOne.TimeEnergy -= 200;
            }
        }
    }
}
public class MinusK : Skill
{
    public MinusK()
    {
        SkillGroup = "Freeze";
        SpCost = 5;
        MpCost = 5;
        Cooldown = 1800;
    }
    public override TargetType GetTargeting()
    {
        if (Level == 4)
            return Targeting = new TargetType(new TargetRuleEnemy(), 1, 10, 1);
        else
            return new TargetType(new TargetRuleEnemy(), 1, 10);
    }
    int[] t0 = { 3, 5, 7, 7 };
    string[] extra = {
    "",
    "",
    "",
    " sMinusK0 "
};
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], extra[iLevel]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        sc.UnitOne.Ua.TakeBulletDamage(new Damage(30, DamageType.cold), sc.User, this);
        sc.UnitOne.GetStatus(new Frozen(new int[] { 300, 500, 700, 700 }[iLevel]));
        if (sc.Level < 4) return;
        foreach (var grid in sc.UnitOne.Up.CurrentGrid.NearGrids(1))
        {
            if (grid.unit != null)
                grid.unit.GetStatus(new Frozen(300));
        }
    }
}
public class DiamondBlizzard : Skill
{
    public DiamondBlizzard()
    {
        SkillGroup = "Freeze";
        SpCost = 5;
        Cooldown = 800;
    }
    int[] t0 = { 2, 3, 4, 4 };
    string[] extra = [
    "",
    "",
    "",
    " sDiamondBlizzard0 "
    ];
    public override TargetType GetTargeting()
    {
        return new TargetType(new TargetRuleSelf(), 1, t0[iLevel]);
    }
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], extra[iLevel]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        foreach(Grid g in sc.User.Up.CurrentGrid.NearGrids(new int[] { 2, 3, 4, 4 }[iLevel]))
        {
            if (g.unit != null && g.unit != sc.User)
            {
                g.unit.Ua.CheckBodyHit(new Damage(30, DamageType.slash), sc.User, this);
                if (sc.Level == 4)
                {
                    g.unit.Up.KnockBack(4, sc);
                }
            }
        }
    }
    
}
public class PerfectGlacialist : Skill
{
    public PerfectGlacialist()
    {
        SkillGroup = "Freeze";
        SpCost = 5;
        MpCost = 5;
        Cooldown = 1800;
        Targeting = new TargetType(new TargetRuleAny(), 1, 10);
    }
    int[] t0 = { 18, 36, 54, 54 };
    string[] t1 = { " sPerfectGlacialist0 ", " sPerfectGlacialist0 ", " sPerfectGlacialist0 ", " sPerfectGlacialist1 " };
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t1[iLevel], t0[iLevel]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        Bullet.CreateBullet(sc.User, this, new Damage(15, DamageType.cold), sc.User.Up.Position, sc.GridOne.Position, 5, 10, ShapeBullet.Bullet, ColorBullet.White);
    }
    public override void ActivateBullet(SkillContext sc, Bullet bullet)
    {
        if (bullet.Shape == ShapeBullet.Micro) return;
        if (sc.UnitOne.Status.FirstOrDefault(x => x is Frozen) == null)
        {
            sc.UnitOne.GetStatus(new Frozen(400));
        }
        else
        {
            for (int i = 0; i < new int[] { 18, 36, 54, 54 }[iLevel]; i++)
            {
                Bullet.CreateBullet(sc.User, this, new Damage(8, DamageType.cold), sc.UnitOne.Up.Position, sc.UnitOne.Up.Position + RandomV2(), 
                    (float)GD.RandRange(1.5, 3), 6, ShapeBullet.Micro, ColorBullet.White);
            }
        }
    }
}
public class PerfectFreeze : SpellCard
{
    public PerfectFreeze()
    {
        SkillGroup = "Freeze";
        SpCost = 40;
        Duration = 300;
        Targeting = new TargetType(new TargetRuleSelf(), 1, 12);
    }
    int[] t0 = { 20, 30, 40, 40 };
    int[] t1 = { 4, 5, 6, 6 };
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], t1[iLevel]);
    }
    public override float GetCooldown()
    {
        return new int[] { 2800, 2800, 2800, 2400 }[iLevel];
    }
    protected override void OnSpellStart(SkillContext sc)
    {
        Info.Print($"{sc.User.TrName} 展开了 {TrName} ！ 冻结的气息弥漫四周……");
        AddTimedEvent(Linspace(20,300,70), (ctx, advanceTime) =>
        {
            var bullet = Bullet.CreateBullet(sc.User, this, new Damage(12, DamageType.cold), sc.User.Up.Position, sc.User.Up.Position + RandomV2(), 
                new Vector2(0, 0), Vector2.Right, (float)GD.RandRange(1.0, 4.0), 12, 
                ShapeBullet.Ring,(ColorBullet)(new List<int> { 0, 4, 9, 12, 13 })[GD.RandRange(0,4)] , advanceTime);
        });
    }

    protected override void OnSpellUpdate(SkillContext sc, float delta)
    {
        
    }
    public override void ActivateBullet(SkillContext sc, Bullet bullet)
    {
        if (GD.Randf() <  new float[] { 0.2f,0.3f,0.4f,0.4f }[iLevel])
            sc.UnitOne.GetStatus(new Frozen( new int[] { 400,500,600,600}[iLevel] ));
    }
}