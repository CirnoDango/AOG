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
        SkillGroup = "Fist";
        SpCost = 3;
        Cooldown = 700;
        Targeting = new TargetType(new TargetRuleUnit(), 1, 1);
    }
    int[] t0 = { 2, 3, 4, 4 };
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], TextEx.Tr(Extra()[iLevel]));
    }
    protected override void StartActivate(SkillContext sc)
    {
        int number = t0[iLevel] + Math.Max(0, (sc.User.Ua.Dex - sc.UnitOne.Ua.Dex) / 5);
        if (sc.Level == 4)
            number += Math.Max(0, (sc.User.Ua.Str - sc.UnitOne.Ua.Str) / 5);
        for (int i = 0; i < number; i++)
            sc.UnitOne.Ua.CheckBodyHit(new Damage(8, DamageType.strike), sc.User, this);
    }
}
public class SpiralLightSteps : Skill
{
    public SpiralLightSteps()
    {
        SkillGroup = "Fist";
        SpCost = 5;
        Cooldown = 1600;
        Targeting = new TargetType(new TargetRuleSelf(), 1, 5);
    }
    int[] t0 = [3, 4, 5, 5];
    int[] t1 = [66, 88, 108, 108];

    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], t1[iLevel], TextEx.Tr(Extra()[iLevel]));
    }
    protected override void StartActivate(SkillContext sc)
    {
        sc.User.GetStatus(new SSpiralLightSteps(t1[iLevel], 100*t0[iLevel], this, sc.Level));
    }
}
public class CrimsonEnergyRelease : Skill
{
    public CrimsonEnergyRelease()
    {
        SkillGroup = "Fist";
        EffectType = EffectType.Passive;
    }
    private Func<Unit, Unit, Damage, Damage> _event;
    int[] t0 = { 8,12,16,16 };
    string[] extra = [
    " sCrimsonEnergyRelease0 ",
    " sCrimsonEnergyRelease0 ",
    " sCrimsonEnergyRelease0 ",
    " sCrimsonEnergyRelease1 ",
    ];
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], extra[iLevel]);
    }
    public override void OnLoad(Unit unit)
    {
        _event = (user, target, initDmg) =>
        {
            unit.GetStatus(new SCrimsonEnergyRelease());
            return initDmg;
        };
        unit.Ue.OnTakeBulletDamage.Add(_event);
    }
    public override void OffLearn(Unit unit)
    {
        unit.Ue.OnTakeBulletDamage.Remove(_event);
    }
}
public class IntenseRainbowFist : Skill
{
    public IntenseRainbowFist()
    {
        SkillGroup = "Fist";
        MpCost = 5;
        Cooldown = 1400;
        Targeting = new TargetType(new TargetRuleUnit(), 1, 1);
    }
    int[] t0 = { 1,2,3,3 };
    public override float GetSpCost()
    {
        if (iLevel == 4) return 30;
        return 5;
    }
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel]);
    }
    protected override void StartActivate(SkillContext sc)
    {
        Damage dmg = sc.UnitOne.Ua.TakeBodyDamage(new Damage(20, DamageType.strike), sc.User, this);
        foreach(Unit unit in sc.UnitOne.Up.CurrentGrid.NearGrids(t0[iLevel])
            .Where(x => x.unit != null)
            .Select(x => x.unit)
            .Where(x => x.Friendness * sc.User.Friendness < 0))
        {
            unit.Ua.TakeBulletDamage(new Damage(dmg.Value / 2, DamageType.sonic), sc.User, this);
        }
        
    }
}
public class DapengFellingFist : SpellCard
{
    public DapengFellingFist()
    {
        SkillGroup = "Fist";
        SpCost = 40;
        Duration = 600;
        Cooldown = 36;
        Targeting = new TargetType(new TargetRuleSelf(), 1, 1);
    }
    int[] t0 = [26, 36, 46, 46];
    public override string GetDescription()
    {
        return string.Format(EffectTr(), t0[iLevel], TextEx.Tr(Extra()[iLevel]));
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
        sc.UnitOne.Ua.TakeBodyDamage(new Damage(t0[iLevel], DamageType.strike), sc.User, this);
        sc.UnitOne.GetStatus(new Stun(500));
        sc.UnitOne.Up.KnockBack(5, sc);
        OnSpellEnd(sc);
    }
}


