using Godot;
using System;
using System.Collections.Generic;
public class PowerBlock : Item, IEquipable
{
    private Func<Unit, Unit, Damage, Damage> _modifierCallback;

    public PowerBlock()
    {
        Name = "PowerBlock";
        Weight = 2f;
    }

    public override void OnEquip(Unit unit)
    {
        _modifierCallback = (user, target, dmg) => dmg + 1;
        unit.Ue.OnDealBulletDamage.Add(_modifierCallback);
    }

    public override void OnUnequip(Unit unit)
    {
        if (_modifierCallback != null)
            unit.Ue.OnDealBulletDamage.Remove(_modifierCallback);
    }
}

public class DangoLight : Item, IEquipable
{
    public DangoLight()
    {
        Name = "DangoLight";
        Weight = 4f;
    }

    public override void OnEquip(Unit unit)
    {
        unit.Up.Vision += 2;
    }

    public override void OnUnequip(Unit unit)
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
    }

    public override void OnEquip(Unit unit)
    {
        unit.Ua.Str += 2;
    }

    public override void OnUnequip(Unit unit)
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
    }
    public override void ApplyParameters(Dictionary<string, object> parameters)
    {
        if (parameters.TryGetValue("MpRecoverPercent", out var val))
        {
            MpRecoverPercent = (int)val;
        }
        base.ApplyParameters(parameters);
    }
    public override Item RandomSummonParam()
    {
        MpRecoverPercent = GD.RandRange(20, 50);
        return this;
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
    }
    private Func<Unit, Unit, Damage, Damage> _modifierCallback;
    public override void OnEquip(Unit unit)
    {
        _modifierCallback = (user, target, dmg) =>
        {
            return dmg + (user.Ua.Str + 10) * 0.2f;
        };

        unit.Ue.OnTakeBodyDamage.Add(_modifierCallback);
    }

    public override void OnUnequip(Unit unit)
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
    }
    private Func<Unit, Unit, Damage, Damage> _modifierCallback;
    public override void OnEquip(Unit unit)
    {
        _modifierCallback = (user, target, dmg) =>
        {
            return dmg + (user.Ua.Dex + 10) * 0.3f;
        };

        unit.Ue.OnTakeBodyDamage.Add(_modifierCallback);
    }

    public override void OnUnequip(Unit unit)
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
    }
    private Func<Unit, Unit, Damage, Damage> _modifierCallback;
    public override void OnEquip(Unit unit)
    {
        _modifierCallback = (user, target, dmg) =>
        {
            return dmg + (user.Ua.Con + 10) * 0.3f;
        };

        unit.Ue.OnTakeBodyDamage.Add(_modifierCallback);
    }

    public override void OnUnequip(Unit unit)
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
    }
    private Func<Unit, Unit, Damage, Damage> _modifierCallback;
    public override void OnEquip(Unit unit)
    {
        _modifierCallback = (user, target, dmg) =>
        {
            return dmg + (user.Ua.Spi + 10) * 0.3f;
        };

        unit.Ue.OnTakeBodyDamage.Add(_modifierCallback);
    }

    public override void OnUnequip(Unit unit)
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
    }
    private Func<Unit, Unit, Damage, Damage> _modifierCallback;
    public override void OnEquip(Unit unit)
    {
        _modifierCallback = (user, target, dmg) =>
        {
            return dmg + (user.Ua.Mag + 10) * 0.3f;
        };

        unit.Ue.OnTakeBodyDamage.Add(_modifierCallback);
    }

    public override void OnUnequip(Unit unit)
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
    }
    private Func<Unit, Unit, Damage, Damage> _modifierCallback;
    public override void OnEquip(Unit unit)
    {
        _modifierCallback = (user, target, dmg) =>
        {
            return dmg + (user.Ua.Cun + 10) * 0.3f;
        };

        unit.Ue.OnTakeBodyDamage.Add(_modifierCallback);
    }

    public override void OnUnequip(Unit unit)
    {
        if (_modifierCallback != null)
            unit.Ue.OnTakeBodyDamage.Remove(_modifierCallback);
    }
}