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
    
}

