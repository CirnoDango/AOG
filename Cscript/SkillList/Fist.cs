using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using static MathEx;
public class Multistrike : Skill
{
    public Multistrike()
    {
        Name = "Multistrike";
        SkillGroup = "Fist";
        SpCost = 3;
        Cooldown = 700;
        Targeting = new TargetType(Target.Unit, 1, 1);
    }
    int[] t0 = { 2, 3, 4, 4 };
    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), t0[level - 1], Extra()[level - 1]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        int number = t0[sc.Level - 1] + Math.Max(0, (sc.User.Ua.Dex - sc.UnitOne.Ua.Dex) / 5);
        if (sc.Level == 4)
            number += Math.Max(0, (sc.User.Ua.Str - sc.UnitOne.Ua.Str) / 5);
        for (int i = 0; i < number; i++)
            sc.UnitOne.Ua.CheckBodyHit(8, sc.User, this);
    }
}
public class SpiralLightSteps : Skill
{
    public SpiralLightSteps()
    {
        Name = "SpiralLightSteps";
        SkillGroup = "Fist";
        SpCost = 5;
        Cooldown = 1600;
        Targeting = new TargetType(Target.Self);
    }
    int[] t0 = [3, 4, 5, 5];
    int[] t1 = [66, 88, 108, 108];

    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), t0[level - 1], t1[level - 1], Extra()[level - 1]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        sc.User.GetStatus(new SSpiralLightSteps(t1[sc.Level - 1], 100*t0[sc.Level - 1], this));
    }
}
public class CrimsonEnergyRelease : Skill
{
    public CrimsonEnergyRelease()
    {
        Name = "CrimsonEnergyRelease";
        SkillGroup = "Fist";
        EffectType = EffectType.Passive;
    }
    private Func<Unit, Unit, float, float> _event;
    int[] t0 = { 8,12,16,16 };
    string[] extra = [
    " sCrimsonEnergyRelease0 ",
    " sCrimsonEnergyRelease0 ",
    " sCrimsonEnergyRelease0 ",
    " sCrimsonEnergyRelease1 ",
    ];
    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), t0[level - 1], extra[level - 1]);
    }
    public override void OnLearn(Unit unit)
    {
        _event = (user, target, initDmg) =>
        {
            unit.GetStatus(new SCrimsonEnergyRelease(1));
            return initDmg;
        };

        unit.Ue.OnTakeBulletDamage.Add(_event);
    }
}
public class IntenseRainbowFist : Skill
{
    public IntenseRainbowFist()
    {
        Name = "IntenseRainbowFist";
        SkillGroup = "Fist";
        MpCost = 5;
        Cooldown = 1400;
        Targeting = new TargetType(Target.Unit, 1, 1);
    }
    int[] t0 = { 1,2,3,3 };
    public override float GetSpCost(int level)
    {
        if (level == 4) return 30;
        return 5;
    }
    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), t0[level - 1]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        float dmg = sc.UnitOne.Ua.TakeBodyDamage(20, sc.User, this);
        foreach(Unit unit in sc.UnitOne.Up.CurrentGrid.NearGrids(t0[sc.Level - 1])
            .Where(x => x.unit != null)
            .Select(x => x.unit)
            .Where(x => x.Friendness * sc.User.Friendness < 0))
        {
            unit.Ua.TakeBulletDamage(dmg / 2, sc.User, this);
        }
        
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
            for (int i = 0; i < new int[] { 18, 36, 54, 54 }[sc.Level - 1]; i++)
            {
                Bullet.CreateBullet(sc.User, this, 8, sc.UnitOne.Up.Position, sc.UnitOne.Up.Position + RandomV2(), 
                    (float)GD.RandRange(1.5, 3), 6, ShapeBullet.Micro, ColorBullet.White);
            }
        }
    }
}
public class DapengFellingFist : SpellCard
{
    public DapengFellingFist()
    {
        Name = "DapengFellingFist";
        SkillGroup = "Fist";
        SpNeed = 40;
        SpCost = 6;
        Duration = 600;
        Cooldown = 36;
        Targeting = new TargetType(Target.Self);
    }
    int[] t0 = [26, 36, 46, 46];
    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), t0[level - 1], Extra()[level - 1]);
    }
    protected override void OnSpellStart(SkillContext sc)
    {
        Info.Print($"{sc.User.TrName} 展开了 {TrName} ");
        sc.User.Ue.OnAttack += Attack;
    }
    public override void OnSpellEnd(SkillContext sc)
    {
        sc.User.Ue.OnAttack -= Attack;
        base.OnSpellEnd(sc);
    }
    private void Attack(SkillContext sc)
    {
        sc.UnitOne.Ua.TakeBodyDamage(t0[sc.Level - 1], sc.User, this);
        sc.UnitOne.GetStatus(new Stun(500));
        sc.UnitOne.Up.KnockBack(5, sc);
        OnSpellEnd(sc);
    }
}