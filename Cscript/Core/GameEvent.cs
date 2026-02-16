using Godot;
using System;
using System.Collections.Generic;

public class GameEvents
{
    public static event Action<Unit, SkillInstance> OnSkillLearned;
    public static void SkillLearned(Unit enemy, SkillInstance si)
    {
        OnSkillLearned?.Invoke(enemy, si);
    }
    public static event Action<Item> OnItemPicked;
    public static void ItemPicked(Item item)
    {
        OnItemPicked?.Invoke(item);
    }
    public static event Action<Unit, SkillContext, SkillInstance> OnUseSkill;
    public static void UseSkill(Unit user, SkillContext sc, SkillInstance si)
    {
        OnUseSkill?.Invoke(user, sc, si);
    }
    public static event Action<Unit> OnUnitMove;
    public static void UnitMove(Unit user)
    {
        OnUnitMove?.Invoke(user);
    }
    public static event Action<Bullet> OnBulletMove;//Into New Hex
    public static void BulletMove(Bullet bullet)
    {
        OnBulletMove?.Invoke(bullet);
    }
    public static event Action OnSceneQuit;
    public static void SceneQuit()
    {
        OnSceneQuit?.Invoke();
    }
    public static event Action OnMapEnter;
    public static void MapEnter()
    {
        OnMapEnter?.Invoke();
    }
    public static event Action<Unit> OnEnemyKilled;
    public static void EnemyKilled(Unit unit)
    {
        OnEnemyKilled?.Invoke(unit);
    }
}

