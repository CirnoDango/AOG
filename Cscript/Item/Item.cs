using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public interface IInteractable
{
    void Interact(Unit unit);
}
public interface IEquipable
{
    void OnEquip(Unit unit);
    void OnUnequip(Unit unit);
}
public abstract partial class Item : IInteractable
{
    protected Item()
    {
        if (this is IEquipable equipable)
        {
            EquipEvent += equipable.OnEquip;
            UnequipEvent += equipable.OnUnequip; 
        }
    }
    public static List<Item> ItemDeck { get; set; } = [];
    //用于查找并复制一个同名的物品
    public string Name { get; set; }
    public string TrName => $"i{Name}";
    public string Description { get; set; }
    public virtual string GetDescription() { return Description; }
    public virtual float Weight { get; set; }
    public Texture2D Texture { get; set; }
    // 默认可以装备，非装备道具重写为 false
    public virtual bool CanEquip => true;
    // 实例特有属性
    public Vector2I Position { get; set; }
    public ItemDropped Sprite { get; set; }
    public Dictionary<string, float> Params { get; set; }
    
    // 使用时或装备时的即时效果
    public event Action<Unit> EquipEvent;
    public void Equip(Unit unit)
    {
        EquipEvent?.Invoke(unit);
    }
    public event Action<Unit> UnequipEvent;
    public void Unequip(Unit unit)
    {
        UnequipEvent?.Invoke(unit);
    }
    public void Interact(Unit unit)
    {
        unit.Inventory.AddItem(this);
    }
    public static void SummonItem(Vector2I position, Item item)
    {
        LayerItemDropped.SummonItem(position, item);
    }
    public static Item CreateItem(string name, Dictionary<string, object> parameters = null)
    {
        var template = GetTemplate(name).MemberwiseClone();
        if (template == null) return null;
        Item item = (Item)template;
        item.ApplyParameters(parameters ?? []);
        return item;
    }

    public static Item GetTemplate(string name)
    {
        return ItemDeck.FirstOrDefault(item => item.Name == name);
    }
    public virtual void ApplyParameters(Dictionary<string, object> parameters)
    {
        Params = parameters
            .Where(x => x.Value is float || x.Value is int || x.Value is double)
            .ToDictionary(
                x => x.Key, 
                x => Convert.ToSingle(x.Value)
            );
    }

    public virtual Item RandomSummonParam()
    {
        return this;
    }
}
public abstract class SkillItem<TSkillInstance> : SkillItem
    where TSkillInstance : Skill, ISkillInstance
{
    public override void ApplyParameters(Dictionary<string, object> parameters)
    {
        Skill = (TSkillInstance)Activator.CreateInstance(typeof(TSkillInstance), this);
    }
}

public abstract class SkillItem : Item, IEquipable
{
    public void OnEquip(Unit unit)
    {
        unit.Us.LearnSkill(Skill);
    }
    public void OnUnequip(Unit unit) { unit.Us.UnLearnSkill(Skill); }
    public Skill Skill { get; set; }
    public void Activate(SkillContext sc)
    {
        Skill.Activate(sc);
    }
}

public class Inventory(Unit unit)
{
    public Unit Unit { get; set; } = unit;
    public List<Item> Items = [];
    public float MaxWeight = 20f;

    public float CurrentWeight => Items.Sum(item => item.Weight);
    public bool AddItem(Item item)
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
    public void RemoveItem(Item item)
    {
        Items.Remove(item);
    }
    public void ThrowItem(Item item)
    {
        Items.Remove(item);
        Item.SummonItem(Unit.Up.Position, item);
    }
}

public class Equipment(Unit unit)
{
    public Unit Unit { get; set; } = unit;
    public List<Item> EquippedItems = [];
    public float MaxEquipWeight = 20f;

    public float CurrentEquipWeight => EquippedItems.Sum(e => e.Weight);

    public bool TryEquip(Item item, Unit unit)
    {
        if (item is Memory)
            return false;
        if (CurrentEquipWeight + item.Weight > MaxEquipWeight)
            return false;

        EquippedItems.Add(item);
        GameEvents.ItemPicked(item);
        ((IEquipable)item).OnEquip(unit);
        return true;
    }

    public void Unequip(Item item, Unit unit)
    {
        if (!EquippedItems.Contains(item))
        {
            return;
        }
        EquippedItems.Remove(item);
        unit.Inventory.Items.Add(item);
        ((IEquipable)item).OnUnequip(unit);
    }
}

public static class ItemLoader
{
    public static void LoadAllItems()
    {
        var ItemType = typeof(Item);
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
            string path = $"res://Assets/Item/{item.Name}.png";
            if (item is Memory)
                path = "res://Assets/Item/memory.png";
            else if (item is BarrageComponent)
                path = $"res://Assets/Barrage/{item.Name}.png";

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

