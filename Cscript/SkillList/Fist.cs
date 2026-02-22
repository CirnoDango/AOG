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
        return string.Format(EffectTr(), t0[level - 1], TextEx.Tr(Extra()[level - 1]));
    }
    protected override void StartActivate(SkillContext sc)
    {
        int number = t0[sc.Level - 1] + Math.Max(0, (sc.User.Ua.Dex - sc.UnitOne.Ua.Dex) / 5);
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
        Name = "SpiralLightSteps";
        SkillGroup = "Fist";
        SpCost = 5;
        Cooldown = 1600;
        Targeting = new TargetType(Target.Self, 1, 5);
    }
    int[] t0 = [3, 4, 5, 5];
    int[] t1 = [66, 88, 108, 108];

    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), t0[level - 1], t1[level - 1], TextEx.Tr(Extra()[level - 1]));
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
    private Func<Unit, Unit, Damage, Damage> _event;
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
        Damage dmg = sc.UnitOne.Ua.TakeBodyDamage(new Damage(20, DamageType.strike), sc.User, this);
        foreach(Unit unit in sc.UnitOne.Up.CurrentGrid.NearGrids(t0[sc.Level - 1])
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
        Name = "DapengFellingFist";
        SkillGroup = "Fist";
        SpCost = 40;
        Duration = 600;
        Cooldown = 36;
        Targeting = new TargetType(Target.Self, 1, 1);
    }
    int[] t0 = [26, 36, 46, 46];
    public override string GetDescription(int level)
    {
        return string.Format(EffectTr(), t0[level - 1], TextEx.Tr(Extra()[level - 1]));
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
        sc.UnitOne.Ua.TakeBodyDamage(new Damage(t0[sc.Level - 1], DamageType.strike), sc.User, this);
        sc.UnitOne.GetStatus(new Stun(500));
        sc.UnitOne.Up.KnockBack(5, sc);
        OnSpellEnd(sc);
    }
}

public class SSpiralLightSteps : Status
{
    private Skill Parent;
    public float Speed
    {
        get => (float)Param;
        set
        {
            Param = value;
        }
    }
    public SSpiralLightSteps(float speed, float duration, Skill parent)
    {
        Name = "SpiralLightSteps";
        Duration = duration;
        Speed = speed;
        Parent = parent;
    }
    public override void OnGet(Unit unit, Status status)
    {
        unit.Ue.OnUnitMove += MoveDamage;
        CombineTime(unit, status);
    }
    public override void OnQuit(Unit unit)
    {
        unit.Ue.OnUnitMove -= MoveDamage;
        Quit(unit);
    }
    public void MoveDamage(Unit unit)
    {
        foreach (Unit target in unit.Up.CurrentGrid.NearGrids(2).Where(x => x != unit.Up.CurrentGrid).Select(g => g.unit))
        {
            target?.Ua.TakeBulletDamage(new Damage(15, DamageType.sonic), unit, Parent);
            target?.GetStatus(new Daze(300));
        }
    }
}
public class SCrimsonEnergyRelease : Status
{
    private int level;
    int[] t0 = [8, 12, 16, 16];
    public int Layer
    {
        get => (int)Param;
        set
        {
            Param = value;
        }
    }
    public SCrimsonEnergyRelease(int l = 1)
    {
        Name = "CrimsonEnergyRelease";
        Duration = 100;
        Layer = l;
    }
    public override void OnTakeBulletDamage(Unit unit, Skill skill, ref Damage damage)
    {
        damage -= damage * t0[level - 1] * Layer / 100f;
    }
    public override void OnDealBodyDamage(Unit unit, ref Damage damage)
    {
        damage += damage * t0[level - 1] * Layer / 100f;
    }
    public override void OnGet(Unit unit, Status status)
    {
        foreach (var s in unit.Status)
        {
            if (s.Name == Name)
            {
                s.Param += ((SCrimsonEnergyRelease)status).Layer;
                s.Param = Math.Min(s.Param, 10);
                return;
            }
        }
        Get(unit);
        unit.Status.Add(status);
        ((SCrimsonEnergyRelease)status).level =
            unit.Us.skills.FirstOrDefault(x => x.skill.Name == "CrimsonEnergyRelease").skill.Level;
    }
    public override void OnQuit(Unit unit)
    {
        if (level == 4 || Layer > 2)
        {
            Layer -= 2;
            Duration += 100;
        }
        else
            Quit(unit);
    }
}
