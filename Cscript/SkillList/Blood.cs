using Godot;
using System;
using System.Collections.Generic;
using static MathEx;
public class StarOfDavid: Skill
{
    public StarOfDavid()
    {
        SkillGroup = "Blood";
        SpCost = 7;
        Cooldown = 700;
        Targeting = new TargetType(new TargetRuleAny(), 1, 7);
    }
    int[] t0 = [12, 12, 18, 18];
    int[] t1 = [50, 50, 50, 100];
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], t1[iLevel]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        int dmg0 = 8;
        if (sc.Level > 1)
            dmg0 = 12;

        Bullet.CreateBullet(sc.User, this, new Damage(dmg0, DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
            new Vector2(0.866f, 0.5f), 0, 2, 7, ShapeBullet.Medium, ColorBullet.Cyan);
        Bullet.CreateBullet(sc.User, this, new Damage(dmg0, DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
            new Vector2(0.866f, -0.5f), 0, 2, 7, ShapeBullet.Medium, ColorBullet.Cyan);
        Bullet.CreateBullet(sc.User, this, new Damage(dmg0, DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
            new Vector2(0, 1), 0, 2, 7, ShapeBullet.Medium, ColorBullet.Cyan);
        Bullet.CreateBullet(sc.User, this, new Damage(dmg0, DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
            new Vector2(0, -1), 0, 2, 7, ShapeBullet.Medium, ColorBullet.Cyan);        
        Bullet.CreateBullet(sc.User, this, new Damage(dmg0, DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
            new Vector2(-0.866f, 0.5f), 0, 2, 7, ShapeBullet.Medium, ColorBullet.Cyan);
        Bullet.CreateBullet(sc.User, this, new Damage(dmg0, DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
            new Vector2(-0.866f, -0.5f), 0, 2, 7, ShapeBullet.Medium, ColorBullet.Cyan);
        Bullet.CreateBullet(sc.User, this, new Damage(6, DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
            new Vector2(0.866f, 0), 0, 2, 7, ShapeBullet.Small, ColorBullet.Red);
        Bullet.CreateBullet(sc.User, this, new Damage(6, DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
            new Vector2(0.433f, 0.75f), 0, 2, 7, ShapeBullet.Small, ColorBullet.Red);
        Bullet.CreateBullet(sc.User, this, new Damage(6, DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
            new Vector2(0.433f, -0.75f), 0, 2, 7, ShapeBullet.Small, ColorBullet.Red);
        Bullet.CreateBullet(sc.User, this, new Damage(6, DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
            new Vector2(-0.866f, 0), 0, 2, 7, ShapeBullet.Small, ColorBullet.Red);
        Bullet.CreateBullet(sc.User, this, new Damage(6, DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
            new Vector2(-0.433f, 0.75f), 0, 2, 7, ShapeBullet.Small, ColorBullet.Red);
        Bullet.CreateBullet(sc.User, this, new Damage(6, DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
            new Vector2(-0.433f, -0.75f), 0, 2, 7, ShapeBullet.Small, ColorBullet.Red);
        if (sc.Level >= 3)
        {
            Bullet.CreateBullet(sc.User, this, new Damage(dmg0, DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
            new Vector2(1.732f, 0), 0, 2, 7, ShapeBullet.Medium, ColorBullet.Cyan);
            Bullet.CreateBullet(sc.User, this, new Damage(dmg0, DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
            new Vector2(-1.732f, 0), 0, 2, 7, ShapeBullet.Medium, ColorBullet.Cyan);
            Bullet.CreateBullet(sc.User, this, new Damage(dmg0, DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
            new Vector2(0.866f, 1.5f), 0, 2, 7, ShapeBullet.Medium, ColorBullet.Cyan);
            Bullet.CreateBullet(sc.User, this, new Damage(dmg0, DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
            new Vector2(-0.866f, -1.5f), 0, 2, 7, ShapeBullet.Medium, ColorBullet.Cyan);
            Bullet.CreateBullet(sc.User, this, new Damage(dmg0, DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
            new Vector2(-0.866f, 1.5f), 0, 2, 7, ShapeBullet.Medium, ColorBullet.Cyan);
            Bullet.CreateBullet(sc.User, this, new Damage(dmg0, DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
            new Vector2(0.866f, -1.5f), 0, 2, 7, ShapeBullet.Medium, ColorBullet.Cyan);
        }
    }
    public override void AwakeBullet(SkillContext sc, Bullet bullet)
    {
        if(sc.UnitOne.Ua.CurrentHp > sc.UnitOne.Ua.MaxHp*0.9)
        {
            bullet.damage += bullet.damage * t1[iLevel] / 100;
        }
    }
}
public class ScarletNetherworld : SkillLong
{
    public ScarletNetherworld()
    {
        SkillGroup = "Blood";
        MpCost = 25;
        Cooldown = 1800;
        Duration = 500;
    }
    public List<Grid> Grids { get; set; } = [];
    public List<Unit> Units { get; set; } = [];
    private List<Sprite2D> Images = [];
    public Unit Summoner { get; set; }
    int[] t0 = [2, 3, 4, 4];
    int[] t1 = [5, 5, 5, 7];
    public override TargetType GetTargeting()
    {
        return new TargetType(new TargetRuleGrid(), 1, 10, t0[iLevel]);
    }
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], t1[iLevel]);
    }
    protected override void OnSkillStart(SkillContext sc)
    {
        Grids = [.. sc.GridOne.NearGrids(t0[iLevel])];
        foreach (Grid grid in Grids)
        {
            Images.Add(ImageEx.CreateGridImage(grid.Position, "res://Assets/GridEffect/ScarletNetherworld.png"));
        }
        Summoner = sc.User;
        AddTimedEvent(Linspace(100, 500, 5), (ctx, advanceTime) =>
        {
            foreach (Grid g in Grids)
            {
                if (g.unit == null || g.unit.symbol.Contains("吸血鬼"))
                    continue;
                g.unit.GetStatus(new SScarletNetherworld(0, 100, sc.User, this));
                g.unit.Ua.TakeBulletDamage(new Damage(10, DamageType.wither), sc.User, this);
            }
        });
    }
    public override void OnSkillEnd(SkillContext sc)
    {
        foreach (Sprite2D i in Images)
            i.QueueFree();
        Images.Clear();
        base.OnSkillEnd(sc);
    }
}
public class ScarletDevil : Skill
{
    public ScarletDevil()
    {
        SkillGroup = "Blood";
        EffectType = EffectType.Passive;
    }
    int[] t0 = [20, 30, 40, 40];
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], TextEx.Tr(Extra()[iLevel]));
    }
    public override void OnLoad(Unit unit)
    {
        DrainBlood = (user, target, dmg) =>
        {
            if (dmg.Type == DamageType.wither)
            {
                unit.Ua.HealHp((dmg * t0[unit.Us.GetSkill(Name).iLevel] / 100f).Value);
                if (unit.Us.GetSkill(Name).Level == 4)
                    return dmg * 1.2f;
            }
            return dmg;
        };
        unit.Ue.OnDealBodyDamage.Add(DrainBlood);
        unit.Ue.OnDealBulletDamage.Add(DrainBlood);
    }
    public override void OffLearn(Unit unit)
    {
        unit.Ue.OnDealBodyDamage.Remove(DrainBlood);
        unit.Ue.OnDealBulletDamage.Remove(DrainBlood);
    }
    private Func<Unit, Unit, Damage, Damage> DrainBlood;
}

public class ScarletShoot: Skill
{
    public ScarletShoot()
    {
        SkillGroup = "Blood";
        SpCost = 10;
        MpCost = 10;
        Cooldown = 1800;
        Targeting = new TargetType(new TargetRuleAny(), 1, 12);
    }
    int[] t0 = [12, 12, 12, 16];
    int[] t1 = [9, 13, 17, 17];
    int[] t2 = [24, 30, 36, 36];
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t2[iLevel]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        sc.User.TimeEnergy -= (sc.User.Ua.SpeedGlobal * sc.User.Ua.SpeedCombat / 100);
        for (int a = 0; a < 360; a += 30)
            Bullet.CreateBullet(sc.User, this, new Damage(t0[iLevel], DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
                new Vector2(0, 0), a, 4, 12, ShapeBullet.Big, ColorBullet.Red);
        Bullet.CreateBullet(sc.User, this, new Damage(t0[iLevel], DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
                new Vector2(0, 0), -4, 6, 12, ShapeBullet.Big, ColorBullet.Red);
        Bullet.CreateBullet(sc.User, this, new Damage(t0[iLevel], DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
                new Vector2(0, 0), 0, 6, 12, ShapeBullet.Big, ColorBullet.Red);
        Bullet.CreateBullet(sc.User, this, new Damage(t0[iLevel], DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
                new Vector2(0, 0), 4, 6, 12, ShapeBullet.Big, ColorBullet.Red);
        if(sc.Level >= 2)
        {
            Bullet.CreateBullet(sc.User, this, new Damage(t0[iLevel], DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
                new Vector2(0, 0), -8, 6, 12, ShapeBullet.Big, ColorBullet.Red);
            Bullet.CreateBullet(sc.User, this, new Damage(t0[iLevel], DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
                new Vector2(0, 0), 8, 6, 12, ShapeBullet.Big, ColorBullet.Red);
        }
        if (sc.Level >= 3)
        {
            Bullet.CreateBullet(sc.User, this, new Damage(t0[iLevel], DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
                new Vector2(0, 0), -12, 6, 12, ShapeBullet.Big, ColorBullet.Red);
            Bullet.CreateBullet(sc.User, this, new Damage(t0[iLevel], DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
                new Vector2(0, 0), 12, 6, 12, ShapeBullet.Big, ColorBullet.Red);
        }
        for (int i = 0; i < t1[iLevel]; i++)
        {
            Bullet.CreateBullet(sc.User, this, new Damage(t0[iLevel], DamageType.wither), sc.User.Up.Position, sc.GridOne.Position,
                new Vector2(0, 0), (float)GD.RandRange(-15f, 15f), (float)GD.RandRange(3f, 6f), 6, ShapeBullet.Ring, ColorBullet.Red);
        }
    }
}
public class ScarletGensokyo : SpellCard
{
    public ScarletGensokyo()
    {
        SkillGroup = "Blood";
        SpCost = 50;
        MpCost = 20;
        Cooldown = 3700;
        Duration = 500;
        Targeting = new TargetType(new TargetRuleSelf(), 1, 10);
    }
    public List<Grid> Grids { get; set; } = [];
    public List<Unit> Units { get; set; } = [];
    public List<Bullet> Sbullets { get; set; } = [];
    private List<Sprite2D> Images = [];
    int[] t0 = [5, 7, 9, 9];
    float[] t1 = [40, 50, 60, 60];
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], t1[iLevel], TextEx.Tr(Extra()[iLevel]));
    }
    protected override void OnSpellStart(SkillContext sc)
    {
        Info.Print($"{sc.User.TrName} 展开了 {TrName} ！");
        Grids = [.. sc.User.Up.CurrentGrid.NearGrids(t0[iLevel])];
        foreach (Grid grid in Grids)
        {
            Images.Add(ImageEx.CreateGridImage(grid.Position, "res://Assets/GridEffect/ScarletNetherworld.png"));
        }
        AddTimedEvent(Linspace(33, 500, 15), (ctx, advanceTime) =>
        {
            for (float a = (float)GD.RandRange(0f, 360f/7f); a < 359; a += 360f/7)
            {
                var bullet = Bullet.CreateBullet(sc.User, this, new Damage(10, DamageType.wither),
                sc.User.Up.Position, sc.User.Up.Position + Vector2I.Right,
                new Vector2(0, 0), a, 2, 10, ShapeBullet.Small, ColorBullet.Red);
                Sbullets.Add(bullet);
            }
        });
        AddTimedEvent(Linspace(100, 500, 5), (ctx, advanceTime) =>
        {
            for(int a = GD.RandRange(0, 30); a < 360; a += 30)
            {
                var bullet = Bullet.CreateBullet(sc.User, this, new Damage(20, DamageType.wither),
                sc.User.Up.Position, sc.User.Up.Position + Vector2I.Right,
                new Vector2(0, 0), a, 2, 10, ShapeBullet.Big, ColorBullet.Violet);
                bullet.Acceleration = bullet.Speed;
            }
            
        });
        AddTimedEvent(Linspace(100, 500, 5), (ctx, advanceTime) =>
        {
            foreach (Grid g in Grids)
            {
                if (g.unit == null || g.unit.symbol.Contains("吸血鬼"))
                    continue;
                g.unit.GetStatus(new SScarletNetherworld(0, 100, sc.User, this));
                g.unit.Ua.TakeBulletDamage(new Damage(10, DamageType.wither), sc.User, this);
                
            }
        });
    }
    protected override void OnSpellUpdate(SkillContext sc, float delta)
    {
        foreach(Bullet b in Sbullets)
        {
            b.Acceleration = b.Speed.Rotated(-90) / 3f;
        }
    }
    public override void OnSpellEnd(SkillContext sc)
    {
        foreach (Sprite2D i in Images)
            i.QueueFree();
        Images.Clear();
        base.OnSpellEnd(sc);
    }
    public override void AwakeBullet(SkillContext sc, Bullet bullet)
    {
        base.AwakeBullet(sc, bullet);
    }
}

