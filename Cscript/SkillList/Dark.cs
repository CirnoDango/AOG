using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using static MathEx;
public class NightBird : Skill
{
    public NightBird()
    {
        Name = "NightBird";
        SkillGroup = "Dark";
        SpCost = 3;
        Cooldown = 400;
    }
    int[] t0 = { 7, 9, 11, 11 };
    int[] t1 = { 4, 6, 8, 8 };
    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), t0[level - 1], t1[level - 1]);
    }

    public override TargetType GetTargeting(int level)
    {
        return new TargetType(Target.Grid, 1, new int[] { 8,8,8,12}[level - 1]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        int r = new int[] { 8, 8, 8, 12 }[sc.level - 1];
        ColorBullet c = (ColorBullet)(new List<int> {4, 7})[GD.RandRange(0, 1)];
        _ = new Bullet(sc.User, this, 10, sc.User.Position, sc.GridOne.Position, new Vector2(0, 0), -15, 2.00f, r, "Ring", c);
        _ = new Bullet(sc.User, this, 10, sc.User.Position, sc.GridOne.Position, new Vector2(0, 0), -10, 2.14f, r, "Ring", c);
        _ = new Bullet(sc.User, this, 10, sc.User.Position, sc.GridOne.Position, new Vector2(0, 0), -5, 2.28f, r, "Ring", c);
        _ = new Bullet(sc.User, this, 10, sc.User.Position, sc.GridOne.Position, new Vector2(0, 0), 0, 2.43f, r, "Ring", c);
        _ = new Bullet(sc.User, this, 10, sc.User.Position, sc.GridOne.Position, new Vector2(0, 0), 5, 2.57f, r, "Ring", c);
        _ = new Bullet(sc.User, this, 10, sc.User.Position, sc.GridOne.Position, new Vector2(0, 0), 10, 2.71f, r, "Ring", c);
        _ = new Bullet(sc.User, this, 10, sc.User.Position, sc.GridOne.Position, new Vector2(0, 0), 15, 2.86f, r, "Ring", c);
        if(sc.level >= 2)
        {
            _ = new Bullet(sc.User, this, 10, sc.User.Position, sc.GridOne.Position, new Vector2(0, 0), -2.5f, 2.36f, r, "Ring", c);
            _ = new Bullet(sc.User, this, 10, sc.User.Position, sc.GridOne.Position, new Vector2(0, 0), 2.5f, 2.50f, r, "Ring", c);
        }
        if (sc.level >= 3)
        {
            _ = new Bullet(sc.User, this, 10, sc.User.Position, sc.GridOne.Position, new Vector2(0, 0), -7.5f, 2.21f, r, "Ring", c);
            _ = new Bullet(sc.User, this, 10, sc.User.Position, sc.GridOne.Position, new Vector2(0, 0), 7.5f, 2.64f, r, "Ring", c);
        }
    }
    public override void AwakeBullet(SkillContext sc, Bullet bullet)
    {
        if (sc.UnitOne.Vision < 12)
        {
            bullet.damage *= (1 + 0.06f * (12 - sc.UnitOne.Vision));
        }
    }
}
public class MoonlightRay : Skill
{
    public MoonlightRay()
    {
        Name = "MoonlightRay";
        SkillGroup = "Dark";
        Description = "发射一道激光，造成20弹幕伤害，恢复击中伤害的10%HP";
        SpCost = 3;
        MpCost = 5;
        Cooldown = 800;
        Targeting = new TargetType(Target.Ray, 1, 12);
    }
    int[] t0 = { 50, 75, 100, 100 };
    string[] t1 = {
    "",
    "",
    "",
    "，目标眩晕3回合"
};
    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), t0[level - 1], t1[level - 1]);
    }

    protected override void StartActivate(SkillContext sc)
    {
        foreach(var g in sc.GridsTarget)
        {
            if(g.unit != null)
            {
                sc.User.BalanceSp(g.unit.TakeBulletDamage(40, sc.User, this)
                    * new float[] { .50f, .75f, 1, 1 }[sc.level - 1]);
            }
        }
    }
}
public class DarkSideOfTheMoon : Skill
{
    public DarkSideOfTheMoon()
    {
        Name = "DarkSideOfTheMoon";
        SkillGroup = "Dark";
        Description = "向一个敌人冲锋，造成30体术伤害，若处于视野外伤害+50%";
        SpCost = 3;
        Cooldown = 800;
    }
    public override TargetType GetTargeting(int level) 
    {
        return new TargetType(Target.Dash, 1, new int[] { 6, 9, 12, 12 }[level - 1]);
    }
    string[] t0 = {
    "50%",
    "50%",
    "50%",
    "100%"
};
    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), t0[level - 1]);
    }

    protected override void StartActivate(SkillContext sc)
    {
        int damage = 30;
        if (!sc.UnitOne.GridInVision().Contains(sc.User.CurrentGrid))
            if (sc.level == 4)
                damage = 60;
            else
                damage = 45;
        sc.UnitOne.CheckBodyHit(damage, sc.User, this);
        List<Grid> grids = sc.User.DashCheck(sc.UnitOne.CurrentGrid);
        grids.RemoveAt(grids.Count - 1);
        foreach (var grid in grids)
        {
            sc.User.MoveTo(grid);
        }
        
    }
}
public class MagicDark : Skill
{
    public MagicDark()
    {
        Name = "MagicDark";
        SkillGroup = "Dark";
        SpCost = 0;
        MpCost = 10;
    }
    public override float GetCooldown(int level)
    {
        return new int[] { 2400, 1800, 1200, 1200 }[level - 1];
    }
    int[] range = { 3, 4, 5, 5 };
    public override TargetType GetTargeting(int level)
    {
        return new TargetType(Target.Grid, 1, 10, range[level - 1]);
    }

    int[] t0 = { 3, 4, 5, 5 };
    string[] t1 = {
    "",
    "",
    "",
    "，并对其中单位造成 15 点弹幕伤害"
};
    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), t0[level - 1], t1[level - 1]);
    }

    protected override void StartActivate(SkillContext sc)
    {
        foreach (var grid in sc.GridOne.NearGrids(new int[] { 3, 4, 5, 5 }[sc.level - 1]))
        {
            grid.unit?.GetStatus(new Dark(500));
            if (sc.level == 4)
                grid.unit?.TakeBulletDamage(15, sc.User, this);
        }
    }
}

public class Demarcation : SpellCard
{
    public Demarcation()
    {
        Name = "Demarcation";
        SkillGroup = "Dark";
        Description = "周围8格陷入黑暗，发出大量交叉子弹";
        SpNeed = 40;
        SpCost = 10;
        Cooldown = 2800;
        Duration = 300;
        Targeting = new TargetType(Target.Self, 1, 8);
    }
    int[] t0 = { 6, 8, 10, 10 };
    int[] t1 = { 3, 4, 5, 5 };
    string[] t2 = {
    "",
    "",
    "",
    "黑暗状态下被击破伤害减半"
};
    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), t0[level - 1], t1[level - 1], t2[level - 1]);
    }

    protected override void OnSpellStart(SkillContext sc)
    {
        List<Grid> lg = sc.User.CurrentGrid.NearGrids(new int[] { 6, 8, 10, 10 }[sc.level - 1]);
        lg.Remove(sc.User.CurrentGrid);
        foreach (var g in lg)
        {
            g.unit?.GetStatus(new Dark(new int[] { 300, 400, 500, 500 }[sc.level - 1]));
        }
        Info.Print($"{sc.User.TrName} 展开了 {TrName} ！ 周围陷入了黑暗！");
        AddTimedEvent(50, (ctx, advanceTime) =>
        {
            for(float i = 0; i < 360; i += 360 / 20f)
            {
                _ = new Bullet(sc.User, this, 15, sc.User.Position + AngleV2(i,1.5f), sc.User.Position + AngleV2(i, 1.5f) + AngleV2(i+90, 1.5f),
                2, 8, "Grain", ColorBullet.Blue, advanceTime);
                _ = new Bullet(sc.User, this, 15, sc.User.Position + AngleV2(i, 1.5f), sc.User.Position + AngleV2(i, 1.5f) + AngleV2(i + -90, 1.5f),
                2, 8, "Grain", ColorBullet.Blue, advanceTime);
            }
        });
        AddTimedEvent(150, (ctx, advanceTime) =>
        {
            for (float i = 0; i < 360; i += 360 / 20f)
            {
                _ = new Bullet(sc.User, this, 15, sc.User.Position + AngleV2(i, 1.5f), sc.User.Position + AngleV2(i, 1.5f) + AngleV2(i + 90, 1.5f),
                2, 8, "Grain", ColorBullet.Green, advanceTime);
                _ = new Bullet(sc.User, this, 15, sc.User.Position + AngleV2(i, 1.5f), sc.User.Position + AngleV2(i, 1.5f) + AngleV2(i + -90, 1.5f),
                2, 8, "Grain", ColorBullet.Green, advanceTime);
            }
        });
        AddTimedEvent(250, (ctx, advanceTime) =>
        {
            for (float i = 0; i < 360; i += 360 / 20f)
            {
                _ = new Bullet(sc.User, this, 15, sc.User.Position + AngleV2(i, 1.5f), sc.User.Position + AngleV2(i, 1.5f) + AngleV2(i + 90, 1.5f),
                2, 8, "Grain", ColorBullet.Red, advanceTime);
                _ = new Bullet(sc.User, this, 15, sc.User.Position + AngleV2(i, 1.5f), sc.User.Position + AngleV2(i, 1.5f) + AngleV2(i + -90, 1.5f),
                2, 8, "Grain", ColorBullet.Red, advanceTime);
            }
        });
        GameEvents.OnTakeSpellcardBreakDamage.Add(OnBreak);
    }
    public override void OnSpellEnd(SkillContext sc)
    {
        GameEvents.OnTakeSpellcardBreakDamage.Remove(OnBreak);
        base.OnSpellEnd(sc);
    }

    protected override void OnSpellUpdate(SkillContext sc, float delta)
    {

    }
    public float OnBreak(Unit breaker, Unit user, float damage)
    {
        if (breaker.Status.FirstOrDefault(x => x is Dark) != null && user.currentSpellcard == this && user.GetSkill(Name).Level == 4)
            return damage * 0.5f;
        return damage;
    }
}