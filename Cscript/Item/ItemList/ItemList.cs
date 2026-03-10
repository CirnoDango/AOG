using Godot;
using System;
using System.Collections.Generic;

public interface IWeapon
{
    Damage BaseDamage();
    float Accurency { get; }
    float CritRate { get; }
    Dictionary<string, float> Param { get; }
    string Description()
    {
        string d = "";
        d += $"====={TextEx.Tr("武器")}=====\n{TextEx.Tr("伤害")}：";
        d += BaseDamage().ToString();
        foreach (var kv in Param)
            d += $" + {kv.Value * 100:F0}%{kv.Key.ToUpper()}";
        d += $"\n{TextEx.Tr("命中率")}：{Accurency * 100:F1}%";
        d += $"\n{TextEx.Tr("暴击率")}：{CritRate * 100:F1}%\n";
        return d;
    }
    public virtual void OnHit(Unit user, Unit target) { }
}
public class PowerBlock : Item, IEquipable
{
    private Func<Unit, Unit, Damage, Damage> _modifierCallback;

    public PowerBlock()
    {
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
public class MagicPotion : SkillItem<MagicPotion.SMagicPotion>
{
    public float MpRecoverPercent { get; private set; } = 40;
    public MagicPotion()
    {
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
    public class SMagicPotion : SkillFromItem<MagicPotion>
    {
        private readonly float _mp;
        public SMagicPotion(MagicPotion parent) : base(parent)
        {
            _mp = parent.MpRecoverPercent;
            SkillGroup = "Item";
            Description = $"恢复{_mp}%MP";
            Cooldown = 2000;
            Targeting = new TargetType(new TargetRuleSelf());
        }
        protected override void StartActivate(SkillContext sc)
        {
            sc.User.Ua.GetMp(sc.User.Ua.MaxMp * _mp / 100f);
        }
    }
}

public class HealPotion : SkillItem<HealPotion.SHealPotion>
{
    public int HpRecoverPercent { get; private set; } = 40;
    public HealPotion()
    {
        Weight = 8f;
    }

    public class SHealPotion : SkillFromItem<HealPotion>
    {
        private readonly int _hp;
        public SHealPotion(HealPotion parent) : base(parent)
        {
            _hp = parent.HpRecoverPercent;
            Texture = parent.Texture;
            SkillGroup = "Item";
            Description = $"恢复{_hp}%HP";
            Cooldown = 2000;
            Targeting = new TargetType(new TargetRuleSelf());
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
        Weight = 4f;
    }
    public Damage BaseDamage()
    {
        return new Damage(10, DamageType.slash);
    }
    public float Accurency => 0.8f;
    public float CritRate => 0.05f;
    public Dictionary<string, float> Param => new() { { "str", 1 } };
}
public class Dagger : Item, IEquipable, IWeapon
{
    public Dagger()
    {
        Weight = 4f;
    }
    public Damage BaseDamage()
    {
        return new Damage(10, DamageType.pierce);
    }
    public float Accurency => 0.9f;
    public float CritRate => 0.1f;
    public Dictionary<string, float> Param => new() { { "str", 0.4f }, { "dex", 0.4f } };
}
public class Club : Item, IEquipable, IWeapon
{
    public Club()
    {
        Weight = 4f;
    }
    public Damage BaseDamage()
    {
        return new Damage(10, DamageType.strike);
    }
    public float Accurency => 0.7f;
    public float CritRate => 0.1f;
    public Dictionary<string, float> Param => new() { { "str", 0.5f }, { "con", 0.5f } };
}
public class Rapier : Item, IEquipable, IWeapon
{
    public Rapier()
    {
        Weight = 4f;
    }
    public Damage BaseDamage()
    {
        return new Damage(10, DamageType.slash);
    }
    public float Accurency => 0.9f;
    public float CritRate => 0.05f;
    public Dictionary<string, float> Param => new() { { "str", 0.4f }, { "spi", 0.4f } };
}
public class QuarterStaff : Item, IEquipable, IWeapon
{
    public QuarterStaff()
    {
        Weight = 4f;
    }
    public Damage BaseDamage()
    {
        return new Damage(10, DamageType.strike);
    }
    public float Accurency => 0.8f;
    public float CritRate => 0.02f;
    public Dictionary<string, float> Param => new() { { "str", 0.4f }, { "mag", 0.4f } };
}
public class BullWhip : Item, IEquipable, IWeapon
{
    public BullWhip()
    {
        Weight = 4f;
    }
    public Damage BaseDamage()
    {
        return new Damage(10, DamageType.slash);
    }
    public float Accurency => 0.8f;
    public float CritRate => 0.1f;
    public Dictionary<string, float> Param => new() { { "str", 0.4f }, { "cun", 0.4f } };
}
