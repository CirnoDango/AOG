using Godot;
using System;
using System.Collections.Generic;
public class PowerBlock : Item, IEquipable
{
    private Func<Unit, Unit, float, float> _modifierCallback;

    public PowerBlock()
    {
        Name = "PowerBlock";
        Weight = 2f;
        Description = "所有弹幕+1伤害。";
    }

    public void OnEquip(Unit unit)
    {
        _modifierCallback = (user, target, dmg) => dmg + 1;
        unit.Ue.OnTakeBulletDamage.Add(_modifierCallback);
    }

    public void OnUnequip(Unit unit)
    {
        if (_modifierCallback != null)
            unit.Ue.OnTakeBulletDamage.Remove(_modifierCallback);
    }
}

public class DangoLight : Item, IEquipable
{
    public DangoLight()
    {
        Name = "DangoLight";
        Weight = 4f;
        Description = "+2视野";
    }

    public void OnEquip(Unit unit)
    {
        unit.Up.Vision += 2;
    }

    public void OnUnequip(Unit unit)
    {
        unit.Up.Vision -= 2;
    }
}

public class DangoTriple : Item, IEquipable
{
    public DangoTriple()
    {
        Name = "DangoTriple";
        Weight = 3f;
        Description = "+2力量";
    }

    public void OnEquip(Unit unit)
    {
        unit.Ua.Str += 2;
    }

    public void OnUnequip(Unit unit)
    {
        unit.Ua.Str -= 2;
    }
}
public interface ISkillInstance
{
    public Skill GetSkill(SkillItem parent);
}
public class MagicPotion : SkillItem<MagicPotion.Instance>
{
    public int MpRecoverPercent { get; private set; } = 40;
    public MagicPotion()
    {
        Name = "MagicPotion";
        Weight = 2f;
        Description = "获得技能：MP恢复";
    }
    public class Instance : Skill, ISkillInstance
    {
        private readonly int _mp;
        public Instance(MagicPotion parent)
        {
            _mp = parent.MpRecoverPercent;
            Texture = parent.Texture;
            Name = "MagicPotion";
            SkillGroup = "Item";
            Description = $"恢复{_mp}%MP";
            Cooldown = 2000;
            Targeting = new TargetType(Target.Self);
        }
        public Skill GetSkill(SkillItem parent)
        {
            return new Instance((MagicPotion)parent);
        }

        protected override void StartActivate(SkillContext sc)
        {
            sc.User.Ua.GetMp(sc.User.Ua.MaxMp * _mp / 100f);
        }
    }
}

public class HealPotion : SkillItem<HealPotion.Instance>
{
    public int HpRecoverPercent { get; private set; } = 60;
    public HealPotion()
    {
        Name = "HealPotion";
        Weight = 8f;
        Description = "获得技能：HP恢复";
    }

    public class Instance : Skill, ISkillInstance
    {
        private readonly int _hp;
        public Instance(HealPotion parent)
        {
            _hp = parent.HpRecoverPercent;
            Texture = parent.Texture;
            Name = "HealPotion";
            SkillGroup = "Item";
            Description = $"恢复{_hp}%HP";
            Cooldown = 2000;
            Targeting = new TargetType(Target.Self);
        }
        public Skill GetSkill(SkillItem parent)
        {
            return new Instance((HealPotion)parent);
        }

        protected override void StartActivate(SkillContext sc)
        {
            sc.User.Ua.GetHp(sc.User.Ua.MaxHp * _hp / 100f);
        }
    }
}

public class Axe : Item, IEquipable
{
    public Axe()
    {
        Name = "Axe";
        Weight = 4f;
        Description = "体术伤害+力量*20%";
    }
    private Func<Unit, Unit, float, float> _modifierCallback;
    public void OnEquip(Unit unit)
    {
        _modifierCallback = (user, target, dmg) =>
        {
            return dmg + (user.Ua.Str + 10) * 0.2f;
        };

        unit.Ue.OnTakeBodyDamage.Add(_modifierCallback);
    }

    public void OnUnequip(Unit unit)
    {
        if (_modifierCallback != null)
            unit.Ue.OnTakeBodyDamage.Remove(_modifierCallback);
    }
}
public class Dagger : Item, IEquipable
{
    public Dagger()
    {
        Name = "Dagger";
        Weight = 4f;
        Description = "体术伤害+敏捷*30%";
    }
    private Func<Unit, Unit, float, float> _modifierCallback;
    public void OnEquip(Unit unit)
    {
        _modifierCallback = (user, target, dmg) =>
        {
            return dmg + (user.Ua.Dex + 10) * 0.3f;
        };

        unit.Ue.OnTakeBodyDamage.Add(_modifierCallback);
    }

    public void OnUnequip(Unit unit)
    {
        if (_modifierCallback != null)
            unit.Ue.OnTakeBodyDamage.Remove(_modifierCallback);
    }
}
public class Club : Item, IEquipable
{
    public Club()
    {
        Name = "Club";
        Weight = 4f;
        Description = "体术伤害+体质*30%";
    }
    private Func<Unit, Unit, float, float> _modifierCallback;
    public void OnEquip(Unit unit)
    {
        _modifierCallback = (user, target, dmg) =>
        {
            return dmg + (user.Ua.Con + 10) * 0.3f;
        };

        unit.Ue.OnTakeBodyDamage.Add(_modifierCallback);
    }

    public void OnUnequip(Unit unit)
    {
        if (_modifierCallback != null)
            unit.Ue.OnTakeBodyDamage.Remove(_modifierCallback);
    }
}
public class Rapier : Item, IEquipable
{
    public Rapier()
    {
        Name = "Rapier";
        Weight = 4f;
        Description = "体术伤害+灵力*30%";
    }
    private Func<Unit, Unit, float, float> _modifierCallback;
    public void OnEquip(Unit unit)
    {
        _modifierCallback = (user, target, dmg) =>
        {
            return dmg + (user.Ua.Spi + 10) * 0.3f;
        };

        unit.Ue.OnTakeBodyDamage.Add(_modifierCallback);
    }

    public void OnUnequip(Unit unit)
    {
        if (_modifierCallback != null)
            unit.Ue.OnTakeBodyDamage.Remove(_modifierCallback);
    }
}
public class QuarterStaff : Item, IEquipable
{
    public QuarterStaff()
    {
        Name = "QuarterStaff";
        Weight = 4f;
        Description = "体术伤害+魔力*30%";
    }
    private Func<Unit, Unit, float, float> _modifierCallback;
    public void OnEquip(Unit unit)
    {
        _modifierCallback = (user, target, dmg) =>
        {
            return dmg + (user.Ua.Mag + 10) * 0.3f;
        };

        unit.Ue.OnTakeBodyDamage.Add(_modifierCallback);
    }

    public void OnUnequip(Unit unit)
    {
        if (_modifierCallback != null)
            unit.Ue.OnTakeBodyDamage.Remove(_modifierCallback);
    }
}
public class BullWhip : Item, IEquipable
{
    public BullWhip()
    {
        Name = "BullWhip";
        Weight = 4f;
        Description = "体术伤害+灵巧*30%";
    }
    private Func<Unit, Unit, float, float> _modifierCallback;
    public void OnEquip(Unit unit)
    {
        _modifierCallback = (user, target, dmg) =>
        {
            return dmg + (user.Ua.Cun + 10) * 0.3f;
        };

        unit.Ue.OnTakeBodyDamage.Add(_modifierCallback);
    }

    public void OnUnequip(Unit unit)
    {
        if (_modifierCallback != null)
            unit.Ue.OnTakeBodyDamage.Remove(_modifierCallback);
    }
}


