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
        foreach (var modifier in OnTakeBodyDamage)
        {
            modifiedDamage = modifier(user, target, modifiedDamage);
        }
        return modifiedDamage;
    }

    public static event Action<ItemInstance> OnItemPicked;
    public static void ItemPicked(ItemInstance item)
    {
        OnItemPicked?.Invoke(item);
    }
    public static event Action<Unit> OnUnitMove;
    public static void UnitMoved(Unit unit)
    {
        OnUnitMove?.Invoke(unit);
    }
}

