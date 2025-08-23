using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public abstract class Memory : Item
{
    public new string TrName => $"m{Name}";
    public new static bool CanEquip => false;
    public string Group { get; set; } = "";
}

public class MemoryBag : Equipment
{
    public MemoryBag(Unit unit) : base(unit)
    {
        MaxEquipWeight = unit.MemoryValue;
    }

    public new bool TryEquip(Item item, Unit unit)
    {
        if (item is not Memory)
            return false;
        if (CurrentEquipWeight + item.Weight > MaxEquipWeight)
            return false;
        EquippedItems.Add(item);
        //GameEvents.ItemPicked(item);
        item.OnEquip(unit);
        return true;
    }
}
