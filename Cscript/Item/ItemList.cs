using Godot;
using System;
using System.Collections.Generic;
public interface IWeapon
{
    Damage Damage(Unit unit);
    float Accurency { get; }
    float CritRate { get; }
}
public class PowerBlock : Item, IEquipable
{
    private Func<Unit, Unit, Damage, Damage> _modifierCallback;

    public PowerBlock()
    {
        Name = "PowerBlock";
        Weight = 2f;
    }

    public override void OnLoad(Unit unit)
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

    public override void OnLoad(Unit unit)
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
    public float MpRecoverPercent { get; private set; } = 40;
    public MagicPotion()
    {
        Name = "MagicPotion";
        Weight = 2f;
    }
    public override void ApplyParameters(Dictionary<string, object> parameters)
    {
        if (parameters.TryGetValue("MpRecoverPercent", out var val))
        {
            MpRecoverPercent = Convert.ToSingle(val);
        }
        base.ApplyParameters(parameters);
    }
    public class Instance : Skill, ISkillInstance
    {
        private readonly float _mp;
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
    public int HpRecoverPercent { get; private set; } = 40;
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

public class Axe : Item, IEquipable, IWeapon
{
    public Axe()
    {
        Name = "Axe";
        Weight = 4f;
    }
    public Damage Damage(Unit unit)
    {
        return new Damage(unit.Ua.Str, DamageType.slash);
    }
    public float Accurency => 0.8f;
    public float CritRate => 0.05f;
}
public class Dagger : Item, IEquipable, IWeapon
{
    public Dagger()
    {
        Name = "Dagger";
        Weight = 4f;
    }
    public Damage Damage(Unit unit)
    {
        return new Damage(unit.Ua.Str*0.4f+unit.Ua.Dex*0.4f, DamageType.pierce);
    }
    public float Accurency => 0.9f;
    public float CritRate => 0.1f;
}
public class Club : Item, IEquipable, IWeapon
{
    public Club()
    {
        Name = "Club";
        Weight = 4f;
    }
    public Damage Damage(Unit unit)
    {
        return new Damage(unit.Ua.Str*0.5f+unit.Ua.Con*0.5f, DamageType.strike);
    }
    public float Accurency => 0.7f;
    public float CritRate => 0.1f;
}
public class Rapier : Item, IEquipable, IWeapon
{
    public Rapier()
    {
        Name = "Rapier";
        Weight = 4f;
    }
    public Damage Damage(Unit unit)
    {
        return new Damage(unit.Ua.Str*0.4f+unit.Ua.Spi*0.4f, DamageType.slash);
    }
    public float Accurency => 0.9f;
    public float CritRate => 0.05f;
}
public class QuarterStaff : Item, IEquipable, IWeapon
{
    public QuarterStaff()
    {
        Name = "QuarterStaff";
        Weight = 4f;
    }
    public Damage Damage(Unit unit)
    {
        return new Damage(unit.Ua.Str*0.4f+unit.Ua.Mag*0.4f, DamageType.strike);
    }
    public float Accurency => 0.8f;
    public float CritRate => 0.02f;
}
public class BullWhip : Item, IEquipable, IWeapon
{
    public BullWhip()
    {
        Name = "BullWhip";
        Weight = 4f;
    }
    public Damage Damage(Unit unit)
    {
        return new Damage(unit.Ua.Str * 0.4f + unit.Ua.Cun * 0.4f, DamageType.slash);
    }
    public float Accurency => 0.8f;
    public float CritRate => 0.1f;
}