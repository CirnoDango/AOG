using System;
using System.Collections.Generic;

public class UnitEvent(Unit parent)
{
    private Unit _parent = parent;
    public event Action<Unit> OnKilled;
    public void Killed()
    {
        OnKilled?.Invoke(_parent);
    }
    public List<Func<Unit, Unit, Damage, Damage>> OnDealBulletDamage = [];
    public Damage DealBulletDamage(Unit target, Damage baseDamage)
    {
        Damage modifiedDamage = baseDamage;
        foreach (var modifier in OnDealBulletDamage)
        {
            modifiedDamage = modifier(_parent, target, modifiedDamage);
        }
        return modifiedDamage;
    }
    public List<Func<Unit, Unit, Damage, Damage>> OnDealBodyDamage = [];
    public Damage DealBodyDamage(Unit target, Damage baseDamage)
    {
        Damage modifiedDamage = baseDamage;
        foreach (var modifier in OnDealBodyDamage)
        {
            modifiedDamage = modifier(_parent, target, modifiedDamage);
        }
        return modifiedDamage;
    }
    public List<Func<Unit, Unit, Damage, Damage>> OnTakeBulletDamage = [];
    public Damage TakeBulletDamage(Unit attacker, Damage baseDamage)
    {
        Damage modifiedDamage = baseDamage;
        foreach (var modifier in OnTakeBulletDamage)
        {
            modifiedDamage = modifier(_parent, attacker, modifiedDamage);
        }
        return modifiedDamage;
    }
    public List<Func<Unit, Unit, Damage, Damage>> OnTakeBodyDamage = [];
    public Damage TakeBodyDamage(Unit target, Damage baseDamage)
    {
        Damage modifiedDamage = baseDamage;
        foreach (var modifier in OnTakeBodyDamage)
        {
            modifiedDamage = modifier(_parent, target, modifiedDamage);
        }
        return modifiedDamage;
    }
    public List<Func<Unit, Unit, float, float>> OnTakeSpellcardBreakDamage = [];
    public float TakeSpellcardBreakDamage(Unit target, float baseDamage)
    {
        float modifiedDamage = baseDamage;
        foreach (var modifier in OnTakeSpellcardBreakDamage)
        {
            modifiedDamage = modifier(_parent, target, modifiedDamage);
        }
        return modifiedDamage;
    }
    public List<Func<Unit, Unit, Damage, Damage>> OnGraze = [];
    public Damage Graze(Unit attack, Damage baseDamage)
    {
        Damage modifiedDamage = baseDamage;
        foreach (var modifier in OnGraze)
        {
            modifiedDamage = modifier(_parent, attack, modifiedDamage);
        }
        return modifiedDamage;
    }
    public List<Func<Unit, Skill, bool, bool>> OnCheckSkillUsage = [];
    public bool CheckSkillUsage(Skill si, bool initCheck)
    {
        bool modifiedCheck = initCheck;
        foreach (var modifier in OnCheckSkillUsage)
        {
            modifiedCheck = modifier(_parent, si, initCheck);
        }
        return modifiedCheck;
    }
    public List<Func<Unit, bool, bool>> OnCheckMoveUsage = [];
    public bool CheckMoveUsage(bool initCheck)
    {
        bool modifiedCheck = initCheck;
        foreach (var modifier in OnCheckMoveUsage)
        {
            modifiedCheck = modifier(_parent, initCheck);
        }
        return modifiedCheck;
    }
    public List<Func<Status, bool>> OnGetStatus = [];
    public bool GetStatus(Status s)
    {
        foreach (var modifier in OnGetStatus)
        {
            if (!modifier(s))
                return false;
        }
        return true;
    }
    public event Action<Unit, SkillContext, Skill> OnUseSkill;
    public void UseSkill(SkillContext sc, Skill si)
    {
        OnUseSkill?.Invoke(_parent, sc, si);
    }
    public event Action<Unit> OnUnitMove;
    public void UnitMove()
    {
        OnUnitMove?.Invoke(_parent);
    }
    public event Action<Unit, Bullet> OnCreateBullet;
    public void CreateBullet(Bullet bullet)
    {
        OnCreateBullet?.Invoke(_parent, bullet);
    }
    public event Action<Unit, float> OnUnitUpdate;
    public void UnitUpdate(float updateTime)
    {
        OnUnitUpdate?.Invoke(_parent, updateTime);
    }
    public event Action<Unit> OnUnitTurnEnd;
    public void UnitTurnEnd()
    {
        OnUnitTurnEnd?.Invoke(_parent);
    }
    public event Action<SkillContext> OnAttack;
    public void Attack(SkillContext sc)
    {
        OnAttack?.Invoke(sc);
    }
    public event Action<Unit> OnCrit;
    public void Crit(Unit unit)
    {
        OnCrit?.Invoke(unit);
    }
    public event Action<Unit> OnEvade;
    public void Evade(Unit unit)
    {
        OnCrit?.Invoke(unit);
    }
}
