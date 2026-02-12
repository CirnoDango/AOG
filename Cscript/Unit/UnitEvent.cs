using System;
using System.Collections.Generic;

public class UnitEvent(Unit parent)
{
    private Unit _parent = parent;
    public event Action<Unit> OnEnemyKilled;
    public void EnemyKilled()
    {
        OnEnemyKilled?.Invoke(_parent);
    }
    public event Action<Unit, SkillInstance> OnSkillLearned;
    public void SkillLearned(SkillInstance si)
    {
        OnSkillLearned?.Invoke(_parent, si);
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
    public List<Func<Unit, Unit, Damage, Damage>> OnTakeSpellcardBreakDamage = [];
    public Damage TakeSpellcardBreakDamage(Unit target, Damage baseDamage)
    {
        Damage modifiedDamage = baseDamage;
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
    public List<Func<Unit, SkillInstance, bool, bool>> OnCheckSkillUsage = [];
    public bool CheckSkillUsage(SkillInstance si, bool initCheck)
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
    public event Action<Unit, SkillContext, SkillInstance> OnUseSkill;
    public void UseSkill(SkillContext sc, SkillInstance si)
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
}
