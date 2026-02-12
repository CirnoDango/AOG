using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class ItemEffect : IEquipable
{
    public static List<ItemEffect> ItemEffectDeck { get; set; } = [];
    public abstract int Value { get; }
    public string Name => GetType().Name;
    public virtual float ParamF { get; set; }
    public virtual int ParamI { get; set; }
    public virtual string Description { get; }
    public virtual string GetDescription()
    {
        return string.Format(TranslationServer.Translate($"ie{Name}"), InfoParam);
    }
    public virtual string InfoParam
    {
        get
        {
            if(ParamF != 0)
                return $"{ParamF:F1}";
            return $"{ParamI}";
        }
    }
    public abstract void OnEquip(Unit unit);
    public abstract void OnUnequip(Unit unit);
    public virtual void RandomSummonParam() { }
    public static ItemEffect CreateItemEffect(string name)
    {
        var template = GetTemplate(name).MemberwiseClone();
        if (template == null) return null;
        ItemEffect ie = (ItemEffect)template;
        ie.RandomSummonParam();
        return ie;
    }
    public void ApplyItemEffect(Item item)
    {
        item.egos.Add(this);
        item.EquipEvent += OnEquip;
        item.UnequipEvent += OnUnequip;
    }
    public static ItemEffect GetTemplate(string name)
    {
        return ItemEffectDeck.FirstOrDefault(ie => ie.Name == name);
    }
    public static void LoadAllItemEffects()
    {
        var ItemEffectType = typeof(ItemEffect);
        var ItemEffects = new List<ItemEffect>();

        // 获取当前程序集下所有 Item 的非抽象子类
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => ItemEffectType.IsAssignableFrom(t) && !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null);

        foreach (var t in types)
        {
            if (Activator.CreateInstance(t) is ItemEffect instance)
                ItemEffects.Add(instance);
        }

        ItemEffectDeck = ItemEffects;
    }
}
