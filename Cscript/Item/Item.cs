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
    string GetDescription();
}
public abstract partial class Item : IInteractable, IEquipable
{
    public static List<Item> ItemDeck { get; set; } = [];
    //用于查找并复制一个同名的物品
    public string Name => GetType().Name;
    public string TrName => $"i{Name}";
    public virtual float Weight { get; set; }
    public Texture2D Texture { get; set; }
    // 默认可以装备，非装备道具重写为 false
    public virtual bool CanEquip => true;
    // 实例特有属性
    public Vector2I Position { get; set; }
    public ItemDropped Sprite { get; set; }
    public Dictionary<string, object> Params { get; set; } = [];
    
    // 使用时或装备时的即时效果
    public event Action<Unit> EquipEvent;
    public event Action<Unit> UnequipEvent;
    // 存储ItemEffect
    public List<IEquipable> egos = [];
    // 掉落概率
    public float salvage = 0;
    // 宝箱权重
    public float chestValue = 1;
    // 附魔等级
    public int effectLevel = 0;
    public void Equip(Unit unit)
    {
        EquipEvent?.Invoke(unit);
    }
    public void Unequip(Unit unit)
    {
        UnequipEvent?.Invoke(unit);
    }
    public virtual void OnEquip(Unit unit) { }
    public virtual void OnLoad(Unit unit) { }
    public virtual void OnUnequip(Unit unit) { }
    public virtual string GetDescription()
    {
        string s = "";
        if (this is SkillItem si && this is not BarrageSet)
        {
            s += $"====={TextEx.Tr("技能")}=====";
            s += $"\n{TextEx.Tr("冷却时间")}：{si.Skill.Cooldown / 100:F0}";
            s += $"\n{TextEx.Tr($"is{Name}")}\n";
        }
        else
        {
            var v = TextEx.Tr($"id{Name}");
            if (v != "null")
                s = TextEx.Tr($"id{Name}") + "\n";
        }
            
        if (this is IWeapon iw)
            s += iw.Description();
        foreach (var e in egos)
        {
            s += e.GetDescription() + "\n";
        }
        if(this is BarrageComponent bc)
        {
            if (bc.SpCost > 0)
                s += $"{TextEx.Tr("SP消耗")}：{bc.SpCost}\n";
            if (bc.CoolDown > 0)
                s += $"{TextEx.Tr("冷却时间")}：{bc.CoolDown / 100}\n";
            if (bc.draw > 0)
                s += $"{TextEx.Tr("抽取")}：{bc.draw}\n";
        }
        return s.TrimEnd('\n');
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
        ((Item)template).Params = [];
        if (template == null) return null;
        Item item = (Item)template;
        item.egos = [];
        item.EquipEvent += item.OnEquip;
        item.EquipEvent += item.OnLoad;
        item.UnequipEvent += item.OnUnequip;
        item.ApplyParameters(parameters ?? []);
        return item;
    }
    public static Item CreateItem(string name, bool load, Dictionary<string, object> parameters = null)
    {
        var template = GetTemplate(name).MemberwiseClone();
        ((Item)template).Params = [];
        if (template == null) return null;
        Item item = (Item)template;
        item.egos = [];
        item.EquipEvent += item.OnLoad;
        item.UnequipEvent += item.OnUnequip;
        item.ApplyParameters(parameters ?? []);
        return item;
    }
    public static Item CreateItem(string name, float salvage, Dictionary<string, object> parameters = null)
    {
        Item item = CreateItem(name, parameters);
        item.salvage = salvage;
        return item;
    }
    public static Item GetTemplate(string name)
    {
        return ItemDeck.FirstOrDefault(item => item.Name == name);
    }
    public static Item GetWeightedRandomItem(List<Item> items)
    {
        float totalWeight = 0f;
        foreach (var it in items)
            totalWeight += Mathf.Max(0f, it.chestValue);

        if (totalWeight <= 0f)
            return null;

        float r = (float)GD.Randf() * totalWeight;
        float acc = 0f;

        foreach (var it in items)
        {
            acc += Mathf.Max(0f, it.chestValue);
            if (r <= acc)
                return it;
        }

        return items[^1];
    }

    public virtual void ApplyParameters(Dictionary<string, object> parameters)
    {
        if (Params == null)
            Params = new Dictionary<string, object>();

        foreach (var kv in parameters)
        {
            Params[kv.Key] = kv.Value;
        }
    }
    public virtual void GetParam()
    {

    }
    public virtual Item RandomSummonParam()
    {
        return this;
    }
    public void AddRandomEgo(int value)
    {
        if (this is Memory || this is BarrageComponent)
            return;
        int number = 0;
        List<string> egos = [];
        do
        {
            ItemEffect ie = ItemEffect.ItemEffectDeck[GD.RandRange(0, ItemEffect.ItemEffectDeck.Count - 1)];
            string ien = ie.Name;
            if (!egos.Contains(ien) && ie.Value <= value)
            {
                egos.Add(ien);
                value -= ie.Value;
                ItemEffect.CreateItemEffect(ien).ApplyItemEffect(this);
            }
            
        } while (number < 40 && value > 0);
    }
}
public abstract class SkillItem<TSkill> : SkillItem
    where TSkill : Skill
{
    public override void ApplyParameters(Dictionary<string, object> parameters)
    {
        base.ApplyParameters(parameters);
        Skill = (TSkill)Activator.CreateInstance(typeof(TSkill), this);
    }
}
public abstract class SkillFromItem : Skill
{
}
public abstract class SkillFromItem<TItem> : SkillFromItem
    where TItem : SkillItem
{
    protected SkillFromItem(TItem parent)
    {
        SkillGroup = "Item";
        Texture = parent.Texture;
        Description = $"\n{TextEx.Tr($"is{parent.Name}")}\n";
    }
}

public abstract class SkillItem : Item, IEquipable
{
    public override void OnLoad(Unit unit)
    {
        unit.Us.LearnSkill(Skill);
    }
    public override void OnUnequip(Unit unit)
    {
        unit.Us.UnLearnSkill(Skill);
    }
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
    public float MaxWeight = 40f;

    public float CurrentWeight => Items.Sum(item => item.Weight);
    public bool AddItem(Item item)
    {
        if (item == null)
            return false;
        if (CurrentWeight + item.Weight > MaxWeight)
        {
            Info.Print($"超重了无法添加物品 {TranslationServer.Translate(item.TrName)}！");
            return false;
        }
        Items.Add(item);
        if (Unit == Player.PlayerUnit)
        {
            Info.Print($"已捡起物品 {TranslationServer.Translate(item.TrName)}");
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
        item.Equip(unit);
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
        item.Unequip(unit);
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

