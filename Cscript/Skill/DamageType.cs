using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public struct Damage(float value, DamageType type)
{
    public float Value = value;
    public DamageType Type = type;
    public static Damage operator +(Damage a, Damage b)
    {
        return new Damage(a.Value + b.Value, a.Type);
    }
    public static Damage operator +(Damage a, float b)
    {
        return new Damage(a.Value + b, a.Type);
    }
    // 减法
    public static Damage operator -(Damage a, Damage b)
    {
        return new Damage(a.Value - b.Value, a.Type);
    }
    public static Damage operator -(Damage a, float b)
    {
        return new Damage(a.Value - b, a.Type);
    }
    // 乘法
    public static Damage operator *(Damage a, float multiplier)
    {
        return new Damage(a.Value * multiplier, a.Type);
    }
    public static Damage operator *(float multiplier, Damage a)
    {
        return new Damage(a.Value * multiplier, a.Type);
    }

    // 除法
    public static Damage operator /(Damage a, float divisor)
    {
        return new Damage(a.Value / divisor, a.Type);
    }

    public override string ToString()
    {
        //return $"{Value:F0} {Type}";
        return $"{Value:F0} {TranslationServer.Translate($"dt{Type}")}";
    }
    public Damage ApplyModifiers(DamageTypeSet attacker, DamageTypeSet defender)
    {
        Damage damage = new(Value, Type);
        attacker.DamageAdvantage.TryGetValue(Type, out float adv);
        defender.DamageResistance.TryGetValue(Type, out float res);
        if (adv >= res)
            damage.Value *= 1 + adv - res;
        else
            damage.Value /= 1 + res - adv;
        return damage;
    }
}
public enum DamageType
{
    slash, pierce, strike,
    fire, cold, lighting, wind, earth,
    arcane, holy, shadow, celestial,
    spirit, faith, barrier, nature, wither,
    timespace, fantasy, poison, sonic, fate, mind
}

public class DamageTypeSet
{
    public Dictionary<DamageType, float> DamageAdvantage = [];
    public Dictionary<DamageType, float> DamageResistance = [];
    public DamageTypeSet() { }
    public DamageTypeSet(DamageType type, float value, int advOrRes)
    {
        switch (advOrRes)
        {
            case -1:
                DamageResistance.Add(type, value);
                break;
            case 1:
                DamageAdvantage.Add(type, value);
                break;
        }
    }
    /// <summary>
    /// + 运算符：对应 DamageAdvantage / DamageResistance 相加
    /// </summary>
    public static DamageTypeSet operator +(DamageTypeSet a, DamageTypeSet b)
    {
        if (a == null || b == null) return a ?? b;

        var result = new DamageTypeSet();

        foreach (DamageType type in Enum.GetValues(typeof(DamageType)))
        {
            float adv = (a.DamageAdvantage.ContainsKey(type) ? a.DamageAdvantage[type] : 0f)
                      + (b.DamageAdvantage.ContainsKey(type) ? b.DamageAdvantage[type] : 0f);

            float res = (a.DamageResistance.ContainsKey(type) ? a.DamageResistance[type] : 0f)
                      + (b.DamageResistance.ContainsKey(type) ? b.DamageResistance[type] : 0f);

            if (adv != 0f) result.DamageAdvantage[type] = adv;
            if (res != 0f) result.DamageResistance[type] = res;
        }

        return result;
    }

    /// <summary>
    /// - 运算符：对应 DamageAdvantage / DamageResistance 相减
    /// </summary>
    public static DamageTypeSet operator -(DamageTypeSet a, DamageTypeSet b)
    {
        if (a == null) return null;
        if (b == null) return a;

        var result = new DamageTypeSet();

        foreach (DamageType type in Enum.GetValues(typeof(DamageType)))
        {
            float adv = (a.DamageAdvantage.ContainsKey(type) ? a.DamageAdvantage[type] : 0f)
                      - (b.DamageAdvantage.ContainsKey(type) ? b.DamageAdvantage[type] : 0f);

            float res = (a.DamageResistance.ContainsKey(type) ? a.DamageResistance[type] : 0f)
                      - (b.DamageResistance.ContainsKey(type) ? b.DamageResistance[type] : 0f);

            if (adv != 0f) result.DamageAdvantage[type] = adv;
            if (res != 0f) result.DamageResistance[type] = res;
        }

        return result;
    }
}


