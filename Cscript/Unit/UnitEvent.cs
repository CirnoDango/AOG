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
    // 可拓展更多
    public List<Func<Unit, Unit, float, float>> OnTakeBulletDamage = [];
    public float TakeBulletDamage(Unit target, float baseDamage)
    {
        float modifiedDamage = baseDamage;
        foreach (var modifier in OnTakeBulletDamage)
        {
            modifiedDamage = modifier(_parent, target, modifiedDamage);
        }
        return modifiedDamage;
    }
    public List<Func<Unit, Unit, float, float>> OnTakeBodyDamage = [];
    public float TakeBodyDamage(Unit target, float baseDamage)
    {
        float modifiedDamage = baseDamage;
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
    public event Action<Unit> OnUnitMove;
    public void UnitMoved()
    {
        OnUnitMove?.Invoke(_parent);
    }
    public event Action<Unit, Bullet> OnCreateBullet;
    public void CreateBullet(Bullet bullet)
    {
        OnCreateBullet?.Invoke(_parent, bullet);
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
