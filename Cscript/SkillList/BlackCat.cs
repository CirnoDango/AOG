using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using static MathEx;
public class FenghuangSpreadWings : Skill
{
    public FenghuangSpreadWings()
    {
        SkillGroup = "BlackCat";
        SpCost = 5;
        Cooldown = 500;
        Targeting = new TargetType(new TargetRuleAny(), 1, 12);
    }
    int[] t0 = [6, 8, 10, 10];
    int[] t1 = [50, 50, 50, 0];
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], t1[iLevel]);
    }
    public override float GetTimeCost()
    {
        return t1[iLevel];
    }
    protected override void StartActivate(SkillContext sc)
    {
        for (int i = 0; i < t0[iLevel] / 2; i++)
        {
            Bullet.CreateBullet(sc.User, this, new Damage(15, DamageType.strike), sc.User.Up.Position, sc.GridOne.Position, Vector2.Zero,
                (float)GD.RandRange(-8f, 8f), (float)GD.RandRange(4f, 8), 12,
                ShapeBullet.ArrowMedium, ColorBullet.Lime);
            Bullet.CreateBullet(sc.User, this, new Damage(15, DamageType.strike), sc.User.Up.Position, sc.GridOne.Position, Vector2.Zero,
                (float)GD.RandRange(-8f, 8f), (float)GD.RandRange(4f, 8), 12,
                ShapeBullet.ArrowMedium, ColorBullet.Azure);
        }
    }
}
public class SoaringSeimei: Skill
{
    public SoaringSeimei()
    {
        SkillGroup = "BlackCat";
        SpCost = 10;
        Cooldown = 2400;
    }
    int[] t0 = [5,6,7,7];
    int[] t1 = [6,6,6,8];
    public override TargetType GetTargeting()
    {
        return new TargetType(new TargetRuleGridEmpty(), 1, t0[iLevel]);
    }
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], TextEx.Tr(Extra()[iLevel]));
    }
    protected override void StartActivate(SkillContext sc)
    {
        var op = sc.User.Up.Position;
        sc.User.Up.MoveTo(sc.GridOne);
        ColorBullet c = ColorBullet.Red;
        foreach (var p in GetIntegerDistancePoints(op, sc.User.Up.Position))
        {
            for (float a = 0; a < 359; a += 360 / t1[iLevel])
            {
                Bullet.CreateBullet(sc.User, this, new Damage(15, DamageType.strike), sc.User.Up.Position, sc.User.Up.Position + RandomV2(), Vector2.Zero,
                    a, 4, 8,ShapeBullet.ArrowMedium, c);
            }
            if(c == ColorBullet.Red)
                c = ColorBullet.Blue;
            else
                c = ColorBullet.Red;
        }
    }
    private static List<Vector2> GetIntegerDistancePoints(Vector2 from, Vector2 to)
    {
        List<Vector2> result = new();

        Vector2 dir = to - from;
        float length = dir.Length();

        if (length < 0.001f)
            return result;

        Vector2 unit = dir / length;

        int steps = (int)MathF.Floor(length);

        for (int i = 1; i <= steps; i++)
        {
            Vector2 point = from + unit * i;
            result.Add(point);
        }

        return result;
    }
}
public class TianxianRumbling : Skill
{
    public TianxianRumbling()
    {
        SkillGroup = "BlackCat";
    }
    int[] t0 = [4,5,6,6];
    int[] t1 = [3,4,5,5];
    float[] t2 = [2000,2000,2000,1400];
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel]);
    }
    public override TargetType GetTargeting()
    {
        return Targeting = new TargetType(new TargetRuleSelf(), 1, 4);
    }
    public override float GetCooldown()
    {
        return t2[iLevel];
    }
    protected override void StartActivate(SkillContext sc)
    {
        foreach(var g in sc.User.Up.CurrentGrid.NearGrids(t0[iLevel]))
        {
            if(g.unit != null && !g.unit.IsFriend(sc.User))
            {
                if (sc.User.Ua.Dex - g.unit.Ua.Dex >= 10 || GD.Randf() < 0.5f)
                    g.unit.GetStatus(new Stun(t1[iLevel]));
            }
        }
    }
}
public class Kimontonkou : Skill
{
    public Kimontonkou()
    {
        SkillGroup = "BlackCat";
        Cooldown = 2600;
        Targeting = new TargetType(new TargetRuleSelf(), 1, 8);
    }
    int[] t0 = [4, 5, 6, 6];
    int[] t1 = [16,20,24,24];
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
public class SoaringBishamonten: SpellCard
{
    public SoaringBishamonten()
    {
        SkillGroup = "BlackCat";
        SpCost = 50;
        Cooldown = 3600;
        Duration = 400;
        Targeting = new TargetType(new TargetRuleSelf(), 1, 6);
    }
    int[] t0 = [80, 100, 120, 120];
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel]);
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