using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using static MathEx;
public class MagicallyLuminousShanghaiDolls : Skill
{
    public MagicallyLuminousShanghaiDolls()
    {
        SkillGroup = "Doll";
        MpCost = 5;
        Cooldown = 1600;
        Targeting = new TargetType(new TargetRuleSelf(), 1, 8);
    }
    int[] t0 = { 600, 800, 1000, 1000 };
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], TextEx.Tr(Extra()[iLevel]));
    }
    protected override void StartActivate(SkillContext sc)
    {
        var doll = Scene.CurrentMap.CreateFriend(sc.User, "dollShanghai", t0[iLevel], UnitEgo.normal);
        doll.Ua.Str += sc.User.Ua.Cun + 10;
        doll.Ua.SpeedMove += 1;
        if (iLevel == 4)
            doll.Us.AddSkill(new WhirlwindSlash());
    }
}
public class HangedHouraiDolls : Skill
{
    public HangedHouraiDolls()
    {
        SkillGroup = "Doll";
        MpCost = 5;
        Cooldown = 2000;
        Targeting = new TargetType(new TargetRuleSelf(), 1, 10);
    }
    int[] t0 = { 400, 600, 800, 800 };
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], TextEx.Tr(Extra()[iLevel]));
    }
    protected override void StartActivate(SkillContext sc)
    {
        var doll = Scene.CurrentMap.CreateFriend(sc.User, "dollHourai", t0[iLevel], UnitEgo.normal);
        doll.Ua.Mag += sc.User.Ua.Cun + 10;
        doll.Us.AddSkill(new LaserShoot());
    }
}
public class FraternalFrenchDolls : Skill
{
    public FraternalFrenchDolls()
    {
        SkillGroup = "Doll";
        MpCost = 5;
        Cooldown = 2000;
        Targeting = new TargetType(new TargetRuleSelf(), 1, 10);
    }
    int[] t0 = { 600, 800, 1000, 1000 };
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], TextEx.Tr(Extra()[iLevel]));
    }
    protected override void StartActivate(SkillContext sc)
    {
        var doll = Scene.CurrentMap.CreateFriend(sc.User, "dollFrance", t0[iLevel], UnitEgo.normal);
        doll.Ua.Con += sc.User.Ua.Cun + 10;
        doll.Ua.Spi += sc.User.Ua.Cun + 10;
        doll.Us.AddSkill(new Taunt());
        if (iLevel == 4)
            doll.Us.AddSkill(new GroupHeal());

    }
}
public class RedHairedDutchDolls : Skill
{
    public RedHairedDutchDolls()
    {
        SkillGroup = "Doll";
        MpCost = 5;
        Cooldown = 2400;
        Targeting = new TargetType(new TargetRuleSelf(), 1, 10);
    }
    int[] t0 = { 400, 600, 800, 800 };
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], TextEx.Tr(Extra()[iLevel]));
    }
    protected override void StartActivate(SkillContext sc)
    {
        var doll = Scene.CurrentMap.CreateFriend(sc.User, "dollHolland", t0[iLevel], UnitEgo.normal);
        doll.Ua.Spi += sc.User.Ua.Cun + 10;
        doll.Ua.Cun = 20;
        doll.Equipment.TryEquip(Item.CreateItem("BarrageSet", 1,
            new Dictionary<string, object> { { "barrage", new Dictionary<string, object> { { "MaxComponents", 3 } } } }), doll);
        if (iLevel == 4)
        {
            doll.Equipment.TryEquip(Item.CreateItem("BarrageSet", 1,
            new Dictionary<string, object> { { "barrage", new Dictionary<string, object> { { "MaxComponents", 3 } } } }), doll);
        }
    }
}
public class ArtfulSacrifice : SpellCard
{
    public ArtfulSacrifice()
    {
        SkillGroup = "Doll";
        SpCost = 30;
        Duration = 500;
        Cooldown = 3000;
        Targeting = new TargetType(new TargetRuleSelf(), 1, 10);
    }
    int[] t0 = { 20, 30, 40, 40 };
    int[] t1 = { 4, 5, 6, 6 };
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], t1[iLevel]);
    }
    protected override void OnSpellStart(SkillContext sc)
    {
        Info.Print($"{sc.User.TrName} 展开了 {TrName} ！");
        AddTimedEvent(Linspace(20, 300, 70), (ctx, advanceTime) =>
        {
            var bullet = Bullet.CreateBullet(sc.User, this, new Damage(12, DamageType.cold), sc.User.Up.Position, sc.User.Up.Position + RandomV2(),
                new Vector2(0, 0), Vector2.Right, (float)GD.RandRange(1.0, 4.0), 12,
                ShapeBullet.Ring, (ColorBullet)(new List<int> { 0, 4, 9, 12, 13 })[GD.RandRange(0, 4)], advanceTime);
        });
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

public class WhirlwindSlash: Skill
{
    public WhirlwindSlash()
    {
        SkillGroup = "DollRelated";
        Cooldown = 200;
        Targeting = new TargetType(new TargetRuleSelf(), 1, 1);
    }
    protected override void StartActivate(SkillContext sc)
    {
        foreach (var g in sc.User.Up.CurrentGrid.NearGrids(1))
        {
            if (g.unit != null && !g.unit.IsFriend(sc.User))
                NameSkill["Attack"].Activate(new SkillContext(sc.User, g.unit));
        }
    }
}

public class LaserShoot : Skill
{
    public LaserShoot()
    {
        SkillGroup = "DollRelated";
        Cooldown = 200;
        Targeting = new TargetType(new TargetRuleAny(), 1, 10);
    }
    protected override void StartActivate(SkillContext sc)
    {
        Animation.CreateLaser(sc.User.Up.Position, sc.GridsTarget[^1].Position - sc.User.Up.Position, Colors.Gold);
        foreach (var g in sc.GridsTarget)
        {
            g.unit.Ua.TakeBulletDamage(new Damage(30, DamageType.celestial), sc.User, this);
        }
    }
}

public class Taunt : Skill
{
    public Taunt()
    {
        SkillGroup = "DollRelated";
        Cooldown = 2000;
        Targeting = new TargetType(new TargetRuleSelf(), 1, 8);
    }
    protected override void StartActivate(SkillContext sc)
    {
        foreach (var g in sc.User.Up.CurrentGrid.NearGrids(1))
        {
            if (g.unit != null && !g.unit.IsFriend(sc.User))
                g.unit.UnitAi.target = sc.User;
        }
    }
}

public class GroupHeal : Skill
{
    public GroupHeal()
    {
        SkillGroup = "DollRelated";
        Cooldown = 800;
        Targeting = new TargetType(new TargetRuleSelf(), 1, 8);
    }
    protected override void StartActivate(SkillContext sc)
    {
        foreach (var g in sc.User.Up.CurrentGrid.NearGrids(1))
        {
            if (g.unit != null && g.unit.IsFriend(sc.User))
                g.unit.Ua.HealHp(50);
        }
    }
}

public class Explosion : Skill
{
    public Explosion()
    {
        SkillGroup = "DollRelated";
        Cooldown = 800;
    }
    int[] t0 = [1, 2, 2, 2];
    int[] t1 = [30, 40, 50, 50];
    public override TargetType GetTargeting()
    {
        return new TargetType(new TargetRuleSelf(), 1, t0[iLevel]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        foreach (var g in sc.User.Up.CurrentGrid.NearGrids(t0[iLevel]))
        {
            if (g.unit != null && !g.unit.IsFriend(sc.User))
                g.unit.Ua.TakeBodyDamage(new Damage(t1[iLevel], DamageType.arcane), sc.User, this);
        }
    }
}