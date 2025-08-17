using Godot;
using System;
using System.Collections.Generic;

public class GameEvents
{
    // 例如：敌人被杀
    public static event Action<Unit> OnEnemyKilled;

    public static void EnemyKilled(Unit enemy)
    {
        OnEnemyKilled?.Invoke(enemy);
    }

    // 可拓展更多
    public static List<Func<Unit, Unit, float, float>> OnTakeBulletDamage = [];
    public static float TakeBulletDamage(Unit user, Unit target, float baseDamage)
    {
        float modifiedDamage = baseDamage;
        foreach (var modifier in OnTakeBulletDamage)
        {
            modifiedDamage = modifier(user, target, modifiedDamage);
        }
        return modifiedDamage;
    }
    public static List<Func<Unit, Unit, float, float>> OnTakeBodyDamage = [];
    public static float TakeBodyDamage(Unit user, Unit target, float baseDamage)
    {
        float modifiedDamage = baseDamage;
        foreach (var modifier in OnTakeBodyDamage)
        {
            modifiedDamage = modifier(user, target, modifiedDamage);
        }
        return modifiedDamage;
    }
    public static List<Func<Unit, Unit, float, float>> OnTakeSpellcardBreakDamage = [];
    public static float TakeSpellcardBreakDamage(Unit user, Unit target, float baseDamage)
    {
        float modifiedDamage = baseDamage;
        foreach (var modifier in OnTakeSpellcardBreakDamage)
        {
            modifiedDamage = modifier(user, target, modifiedDamage);
        }
        return modifiedDamage;
    }

    public static List<Func<Unit, SkillInstance, bool, bool>> OnCheckSkillUsage = [];
    public static bool CheckSkillUsage(Unit user, SkillInstance si, bool initCheck)
    {
        bool modifiedCheck = initCheck;
        foreach (var modifier in OnCheckSkillUsage)
        {
            modifiedCheck = modifier(user, si, initCheck);
        }
        return modifiedCheck;
    }
    public static event Action<Item> OnItemPicked;
    public static void ItemPicked(Item item)
    {
        OnItemPicked?.Invoke(item);
    }
    public static event Action<Unit> OnUnitMove;
    public static void UnitMoved(Unit unit)
    {
        OnUnitMove?.Invoke(unit);
    }
    public static event Action<Unit, Bullet> OnCreateBullet;
    public static void CreateBullet(Unit unit, Bullet bullet)
    {
        OnCreateBullet?.Invoke(unit, bullet);
    }
    public static event Action<Unit> OnUnitTurnEnd;
    public static void UnitTurnEnd(Unit unit)
    {
        OnUnitTurnEnd?.Invoke(unit);
    }
}

