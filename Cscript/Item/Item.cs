using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public interface IInteractable
{
    void Interact(Unit unit);
}
public abstract class Item
{
    public static List<Item> ItemDeck { get; set; } = [];
    //用于查找并复制一个同名的物品
    public static ItemInstance GetItemName(string name, Dictionary<string, object> parameters = null)
    {
        var template = GetTemplate(name).MemberwiseClone();
        if (template == null) return null;
        var item = new ItemInstance((Item)template);
        if (item.Template is SkillItem skillItem)
        {
            skillItem.ApplyParameters(parameters);
        }
        return item;
    }

    public static Item GetTemplate(string name)
    {
        return ItemDeck.FirstOrDefault(item => item.Name == name);
    }
    public string Name { get; set; }
    public string TrName => $"i{Name}";
    public string Description { get; set; }
    public float Weight { get; set; }
    public Texture2D Texture { get; set; }
    // 默认可以装备，非装备道具重写为 false
    public virtual bool CanEquip => true;

    // 使用时或装备时的即时效果
    public virtual void OnEquip(Unit unit) { }
    public virtual void OnUnequip(Unit unit) { }
}
public abstract class SkillItem : Item
{
    public override void OnEquip(Unit unit)
    {
        unit.LearnSkill(Skill);
    }
    public override void OnUnequip(Unit unit) { unit.UnLearnSkill(Skill); }
    public Skill Skill { get; set; }
    public abstract void ApplyParameters(Dictionary<string, object> parameters);
    public void Activate(SkillContext sc)
    {
        Skill.Activate(sc);
    }
}
// 实际游戏中用于携带、掉落、使用的物品实体
public class ItemInstance : IInteractable
{
    public Item Template { get; private set; }

    // 实例特有属性
    public Vector2I Position 
    {
        get;
        set;
    } // 掉落在地图上的位置

    public ItemInstance(Item template)
    {
        Template = template;
    }
    public string Name => Template.Name;
    public string TrName => Template.TrName;
    public string Description => Template.Description;
    public Texture2D Texture => Template.Texture;
    public ItemDropped Sprite { get; set; }
    public float Weight => Template.Weight;
    public virtual bool CanEquip => Template.CanEquip;
    public virtual void OnEquip(Unit unit) => Template.OnEquip(unit);
    public virtual void OnUnequip(Unit unit) => Template.OnUnequip(unit);
    public void Interact(Unit unit)
    {
        unit.inventory.AddItem(this);
    }
    public static void SummonItem(Vector2I position, ItemInstance item)
    {
        LayerItemDropped.SummonItem(position, item);
    }
}
public class Inventory(Unit unit)
{
    public Unit Unit { get; set; } = unit;
    public List<ItemInstance> Items = [];
    public float MaxWeight = 20f;

    public float CurrentWeight => Items.Sum(item => item.Weight);

    public bool AddItem(ItemInstance item)
    {
        if (item == null)
            return false;
        if (CurrentWeight + item.Weight > MaxWeight)
        {
            Info.Print($"超重了，无法添加物品 {item.TrName}！");
            return false;
        }
        Items.Add(item);
        if (Unit == Player.PlayerUnit)
        {
            Info.Print($"已捡起物品 {item.TrName}");
            if (item.Sprite != null)
                LayerItemDropped.DeleteItem(item);
        }
            
        return true;
    }

    public void RemoveItem(ItemInstance item)
    {
        Items.Remove(item);
    }
    public void ThrowItem(ItemInstance item)
    {
        Items.Remove(item);
        ItemInstance.SummonItem(Unit.Position, item);
    }
}

public class Equipment(Unit unit)
{
    public Unit Unit { get; set; } = unit;
    public List<ItemInstance> EquippedItems = [];
    public float MaxEquipWeight = 20f;

    public float CurrentEquipWeight => EquippedItems.Sum(e => e.Weight);

    public bool TryEquip(ItemInstance item, Unit unit)
    {
        if (item.Template is Memory)
            return false;
        if (CurrentEquipWeight + item.Weight > MaxEquipWeight)
            return false;

        EquippedItems.Add(item);
        GameEvents.ItemPicked(item);
        item.OnEquip(unit);
        return true;
    }

    public void Unequip(ItemInstance item, Unit unit)
    {
        if (EquippedItems.Contains(item))
        {
            EquippedItems.Remove(item);
            item.OnUnequip(unit);
        }
    }
}

public static class ItemLoader
{
    public static void LoadAllItems()
    {
        var ItemType = typeof(Item); // 或 typeof(SpellCard) 如果你只要 spellcard
        var Items = new List<Item>();

        // 获取当前程序集下所有 Item 的非抽象子类
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => ItemType.IsAssignableFrom(t) && !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null);

        foreach (var t in types)
        {
            if (Activator.CreateInstance(t) is Item instance)
                Items.Add(instance);
        }

        Item.ItemDeck = Items;
    }

    public static void LoadItemPng()
    {
        foreach (var item in Item.ItemDeck)
        {
            // 检查名称有效性
            if (string.IsNullOrWhiteSpace(item.Name))
                continue;

            // 构建路径
            string path = $"res://assets/Item/{item.Name}.png";
            if (item is Memory)
                path = "res://assets/Item/memory.png";

            // 检查资源是否存在
            if (!ResourceLoader.Exists(path))
            {
                GD.PrintErr($"[LoadItemPng] 图片不存在: {path}");
                continue;
            }

            // 加载资源并赋值
            try
            {
                var texture = GD.Load<Texture2D>(path);
                item.Texture = texture;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[LoadItemPng] 加载失败: {path}，错误信息: {ex.Message}");
            }
        }
    }

}

