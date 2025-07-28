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
        Name = "Icefall";
        SkillGroup = "Freeze";
        SpCost = 3;
        Targeting = new TargetType(Target.Grid, 1, 10);
    }
    int[] t0 = { 3, 6, 6, 6 };
    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), t0[level - 1]);
    }
    public override float GetCooldown(int level)
    {
        return new float[] { 300, 300, 200, 200 }[level - 1];
    }
    protected override void StartActivate(SkillContext sc)
    {
        _ = new Bullet(sc.User, this, 15, sc.User.Position, sc.GridOne.Position, new Vector2(0, 1), 0, 2, 10, "Needle", ColorBullet.Ice);
        _ = new Bullet(sc.User, this, 15, sc.User.Position, sc.GridOne.Position, new Vector2(0, 0), 0, 2, 10, "Needle", ColorBullet.Ice);
        _ = new Bullet(sc.User, this, 15, sc.User.Position, sc.GridOne.Position, new Vector2(0,-1), 0, 2, 10, "Needle", ColorBullet.Ice);
        Barrage.Test().Activate(sc);
        if(sc.level >= 2)
        {
            _ = new Bullet(sc.User, this, 15, sc.User.Position, sc.GridOne.Position, new Vector2(0, 1), Vector2.Right, 2.5f, 10, "Needle", ColorBullet.Ice);
            _ = new Bullet(sc.User, this, 15, sc.User.Position, sc.GridOne.Position, new Vector2(0, 0), Vector2.Right, 2.5f, 10, "Needle", ColorBullet.Ice);
            _ = new Bullet(sc.User, this, 15, sc.User.Position, sc.GridOne.Position, new Vector2(0, -1), Vector2.Right, 2.5f, 10, "Needle", ColorBullet.Ice);
        }
    }
    public override void AwakeBullet(SkillContext sc, Bullet bullet)
    {
        var s = sc.UnitOne.Status.FirstOrDefault(x => x is Frozen);
        if (s != null)
        {
            bullet.damage *= 10;
            if(sc.level == 4)
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
        Name = "MinusK";
        SkillGroup = "Freeze";
        SpCost = 5;
        MpCost = 5;
        Cooldown = 800;
    }
    public override TargetType GetTargeting(int level)
    {
        if (level == 4)
            return Targeting = new TargetType(Target.Enemy, 1, 10, 1);
        else
            return new TargetType(Target.Enemy, 1, 10);
    }
    int[] t0 = { 3, 5, 7, 7 };
    string[] extra = {
    "",
    "",
    "",
    "，1格内单位冻结3回合"
};
    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), t0[level - 1], extra[level - 1]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        sc.UnitOne.TakeBulletDamage(30, sc.User, this);
        sc.UnitOne.GetStatus(new Frozen(new int[] { 300, 500, 700, 700 }[sc.level - 1]));
        foreach (var grid in sc.UnitOne.CurrentGrid.NearGrids(1))
        {
            if (grid.unit != null)
                sc.UnitOne.GetStatus(new Frozen(300));
        }
    }
}
public class DiamondBlizzard : Skill
{
    public DiamondBlizzard()
    {
        Name = "DiamondBlizzard";
        SkillGroup = "Freeze";
        SpCost = 5;
        Cooldown = 800;
        Targeting = new TargetType(Target.Self);
    }
    int[] t0 = { 2, 3, 4, 4 };
    string[] extra = {
    "",
    "",
    "",
    "，并击退4格"
};
    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), t0[level - 1], extra[level - 1]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        foreach(Grid g in sc.User.CurrentGrid.NearGrids(new int[] { 2, 3, 4, 4 }[sc.level - 1]))
        {
            if (g.unit != null && g.unit != sc.User)
            {
                g.unit.CheckBodyHit(30, sc.User, this);
                if (sc.level == 4)
                {
                    Vector2I Going = (Vector2I)(g.unit.Position
                       + 4 * ((Vector2)(g.unit.Position - sc.User.Position)).Normalized() 
                       + new Vector2(0.5f, 0.5f));
                    List<Vector2I> gs = GetLine(sc.User.Position, Going);
                    foreach (var grid in gs)
                    {
                        if (Scene.CurrentMap.CheckWalkable(grid))
                        {
                            g.unit.MoveTo(Scene.CurrentMap.GetGrid(grid));
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
    
}
public class PerfectGlacialist : Skill
{
    public PerfectGlacialist()
    {
        Name = "PerfectGlacialist";
        SkillGroup = "Freeze";
        SpCost = 5;
        MpCost = 5;
        Cooldown = 800;
        Targeting = new TargetType(Target.Grid, 1, 10);
    }
    int[] t0 = { 18, 36, 54, 54 };
    string[] t1 = { "，若已冻结则", "，若已冻结则", "，若已冻结则", "并" };
    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), t1[level - 1], t0[level - 1]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        _ = new Bullet(sc.User, this, 15, sc.User.Position, sc.GridOne.Position, 5, 10, "Needle", ColorBullet.White);
    }
    public override void ActivateBullet(SkillContext sc, Bullet bullet)
    {
        if (bullet.Shape == "Small") return;
        if (sc.UnitOne.Status.FirstOrDefault(x => x is Frozen) == null)
        {
            sc.UnitOne.GetStatus(new Frozen(400));
        }
        else
        {
            for (int i = 0; i < new int[] { 18, 36, 54, 54 }[sc.level - 1]; i++)
            {
                _ = new Bullet(sc.User, this, 8, sc.UnitOne.Position, sc.UnitOne.Position + RandomV2(), 
                    (float)GD.RandRange(1.5, 3), 6, "Small", ColorBullet.White);
            }
        }
    }
}
public class PerfectFreeze : SpellCard
{
    public PerfectFreeze()
    {
        Name = "PerfectFreeze";
        SkillGroup = "Freeze";
        SpNeed = 40;
        SpCost = 10;
        Duration = 300;
        Targeting = new TargetType(Target.Self, 1, 12);
    }
    int[] t0 = { 20, 30, 40, 40 };
    int[] t1 = { 4, 5, 6, 6 };
    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), t0[level - 1], t1[level - 1]);
    }
    public override float GetCooldown(int level)
    {
        return new int[] { 2800, 2800, 2800, 2400 }[level - 1];
    }
    protected override void OnSpellStart(SkillContext sc)
    {
        Info.Print($"{sc.User.TrName} 展开了 {TrName} ！ 冻结的气息弥漫四周……");
        AddTimedEvent(Linspace(20,300,70), (ctx, advanceTime) =>
        {
            var bullet = new Bullet(sc.User, this, 12, sc.User.Position, sc.User.Position + RandomV2(), 
                new Vector2(0, 0), Vector2.Right, (float)GD.RandRange(1.0, 4.0), 12, 
                "Ring",(ColorBullet)(new List<int> { 0, 4, 9, 12, 13 })[GD.RandRange(0,4)] , advanceTime);
        });
    }

    protected override void OnSpellUpdate(SkillContext sc, float delta)
    {
        
    }
    public override void ActivateBullet(SkillContext sc, Bullet bullet)
    {
        if (GD.Randf() <  new float[] { 0.2f,0.3f,0.4f,0.4f }[sc.level - 1])
            sc.UnitOne.GetStatus(new Frozen( new int[] { 400,500,600,600}[sc.level - 1] ));
    }
}