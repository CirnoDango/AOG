using Godot;
using System;
using System.Collections.Generic;
public class PowerBlock : Item
{
    private Func<Unit, Unit, float, float> _modifierCallback;

    public PowerBlock()
    {
        Name = "PowerBlock";
        Weight = 2f;
        Description = "所有弹幕+1伤害。";
    }

    public override void OnEquip(Unit unit)
    {
        _modifierCallback = (user, target, dmg) =>
        {
            if (user != unit) return dmg;
            return dmg + 1;
        };

        GameEvents.OnTakeBulletDamage.Add(_modifierCallback);
    }

    public override void OnUnequip(Unit unit)
    {
        if (_modifierCallback != null)
            GameEvents.OnTakeBulletDamage.Remove(_modifierCallback);
    }
}

public class DangoLight : Item
{
    public DangoLight()
    {
        Name = "DangoLight";
        Weight = 4f;
        Description = "+2视野";
    }

    public override void OnEquip(Unit unit)
    {
        unit.Vision += 2;
    }

    public override void OnUnequip(Unit unit)
    {
        unit.Vision -= 2;
    }
}

public class DangoTriple : Item
{
    public DangoTriple()
    {
        Name = "DangoTriple";
        Weight = 3f;
        Description = "+2力量";
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
public class MagicPotion : SkillItem
{
    public int MpRecoverPercent { get; private set; } = 40;
    public MagicPotion()
    {
        Name = "MagicPotion";
        Weight = 2f;
        Description = "获得技能：MP恢复";
    }

    public override void ApplyParameters(Dictionary<string, object> parameters)
    {
        if(parameters != null)
        {
            if (parameters.TryGetValue("mp", out var val) && val is int mp)
            {
                MpRecoverPercent = mp;
            }
        }
        Skill = new Instance(this);
    }
    
    private class Instance : Skill, ISkillInstance
    {
        private readonly int _mp;
        public Instance(MagicPotion parent)
        {
            _mp = parent.MpRecoverPercent;
            Name = "MagicPotion";
            SkillGroup = "Item";
            Description = $"恢复{_mp}%MP";
            Cooldown = 2000;
            Targeting = new TargetType(Target.Self);
            Texture = GetTemplate(Name).Texture;
        }
        public Skill GetSkill(SkillItem parent)
        {
            return new Instance((MagicPotion)parent);
        }

        protected override void StartActivate(SkillContext sc)
        {
            sc.User.GetMp(sc.User.MaxMp * _mp / 100f);
        }
    }
}



public class Axe : Item
{
    public Axe()
    {
        Name = "Axe";
        Weight = 4f;
        Description = "体术伤害+力量*20%";
    }
    private Func<Unit, Unit, float, float> _modifierCallback;
    public override void OnEquip(Unit unit)
    {
        _modifierCallback = (user, target, dmg) =>
        {
            if (user != unit) return dmg;
            return dmg + (user.Ua.Str + 10) * 0.2f;
        };

        GameEvents.OnTakeBodyDamage.Add(_modifierCallback);
    }

    public override void OnUnequip(Unit unit)
    {
        if (_modifierCallback != null)
            GameEvents.OnTakeBodyDamage.Remove(_modifierCallback);
    }
}
public class Dagger : Item
{
    public Dagger()
    {
        Name = "Dagger";
        Weight = 4f;
        Description = "体术伤害+敏捷*30%";
    }
    private Func<Unit, Unit, float, float> _modifierCallback;
    public override void OnEquip(Unit unit)
    {
        _modifierCallback = (user, target, dmg) =>
        {
            if (user != unit) return dmg;
            return dmg + (user.Ua.Dex + 10) * 0.3f;
        };

        GameEvents.OnTakeBodyDamage.Add(_modifierCallback);
    }

    public override void OnUnequip(Unit unit)
    {
        if (_modifierCallback != null)
            GameEvents.OnTakeBodyDamage.Remove(_modifierCallback);
    }
}
public class Club : Item
{
    public Club()
    {
        Name = "Club";
        Weight = 4f;
        Description = "体术伤害+体质*30%";
    }
    private Func<Unit, Unit, float, float> _modifierCallback;
    public override void OnEquip(Unit unit)
    {
        _modifierCallback = (user, target, dmg) =>
        {
            if (user != unit) return dmg;
            return dmg + (user.Ua.Con + 10) * 0.3f;
        };

        GameEvents.OnTakeBodyDamage.Add(_modifierCallback);
    }

    public override void OnUnequip(Unit unit)
    {
        if (_modifierCallback != null)
            GameEvents.OnTakeBodyDamage.Remove(_modifierCallback);
    }
}
public class Rapier : Item
{
    public Rapier()
    {
        Name = "Rapier";
        Weight = 4f;
        Description = "体术伤害+灵力*30%";
    }
    private Func<Unit, Unit, float, float> _modifierCallback;
    public override void OnEquip(Unit unit)
    {
        _modifierCallback = (user, target, dmg) =>
        {
            if (user != unit) return dmg;
            return dmg + (user.Ua.Spi + 10) * 0.3f;
        };

        GameEvents.OnTakeBodyDamage.Add(_modifierCallback);
    }

    public override void OnUnequip(Unit unit)
    {
        if (_modifierCallback != null)
            GameEvents.OnTakeBodyDamage.Remove(_modifierCallback);
    }
}
public class QuarterStaff : Item
{
    public QuarterStaff()
    {
        Name = "QuarterStaff";
        Weight = 4f;
        Description = "体术伤害+魔力*30%";
    }
    private Func<Unit, Unit, float, float> _modifierCallback;
    public override void OnEquip(Unit unit)
    {
        _modifierCallback = (user, target, dmg) =>
        {
            if (user != unit) return dmg;
            return dmg + (user.Ua.Mag + 10) * 0.3f;
        };

        GameEvents.OnTakeBodyDamage.Add(_modifierCallback);
    }

    public override void OnUnequip(Unit unit)
    {
        if (_modifierCallback != null)
            GameEvents.OnTakeBodyDamage.Remove(_modifierCallback);
    }
}
public class BullWhip : Item
{
    public BullWhip()
    {
        Name = "BullWhip";
        Weight = 4f;
        Description = "体术伤害+灵巧*30%";
    }
    private Func<Unit, Unit, float, float> _modifierCallback;
    public override void OnEquip(Unit unit)
    {
        _modifierCallback = (user, target, dmg) =>
        {
            if (user != unit) return dmg;
            return dmg + (user.Ua.Cun + 10) * 0.3f;
        };

        GameEvents.OnTakeBodyDamage.Add(_modifierCallback);
    }

    public override void OnUnequip(Unit unit)
    {
        if (_modifierCallback != null)
            GameEvents.OnTakeBodyDamage.Remove(_modifierCallback);
    }
}


