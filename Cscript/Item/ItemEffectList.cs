using Godot;

public class AddMaxHp : ItemEffect
{
    public override int Value => 3;
    public override void OnEquip(Unit unit)
    {
        unit.Ua.MaxHp += ParamF;
    }
    public override void OnUnequip(Unit unit)
    {
        unit.Ua.MaxHp -= ParamF;
    }
    public override void RandomSummonParam()
    {
        ParamF = (float)GD.Randfn(15, 3);
    }
}
public class AddMaxSp : ItemEffect
{
    public override int Value => 3;
    public override void OnEquip(Unit unit)
    {
        unit.Ua.MaxSp += ParamF;
    }
    public override void OnUnequip(Unit unit)
    {
        unit.Ua.MaxSp -= ParamF;
    }
    public override void RandomSummonParam()
    {
        ParamF = (float)GD.Randfn(10, 2);
    }
}
public class AddMaxMp : ItemEffect
{
    public override int Value => 3;
    public override void OnEquip(Unit unit)
    {
        unit.Ua.MaxMp += ParamF;
    }
    public override void OnUnequip(Unit unit)
    {
        unit.Ua.MaxMp -= ParamF;
    }
    public override void RandomSummonParam()
    {
        ParamF = (float)GD.Randfn(10, 2);
    }
}
public class HpRecover : ItemEffect
{
    public override int Value => 3;
    public override void OnEquip(Unit unit)
    {
        unit.Ue.OnUnitUpdate += OnUpdate;
    }
    public override void OnUnequip(Unit unit)
    {
        unit.Ue.OnUnitUpdate -= OnUpdate;
    }
    private void OnUpdate(Unit unit, float updateTime)
    {
        unit.Ua.HealHp(updateTime * ParamF / 100);
    }
    public override void RandomSummonParam()
    {
        ParamF = (float)GD.Randfn(3, 0.5);
    }
}
public class MpRecover : ItemEffect
{
    public override int Value => 3;
    public override void OnEquip(Unit unit)
    {
        unit.Ue.OnUnitUpdate += OnUpdate;
    }
    public override void OnUnequip(Unit unit)
    {
        unit.Ue.OnUnitUpdate -= OnUpdate;
    }
    private void OnUpdate(Unit unit, float updateTime)
    {
        unit.Ua.GetMp(updateTime * ParamF / 100);
    }
    public override void RandomSummonParam()
    {
        ParamF = (float)GD.Randfn(3, 0.5);
    }
}
public class AddSpeedGlobal : ItemEffect
{
    public override int Value => 3;
    public override void OnEquip(Unit unit)
    {
        unit.Ua.SpeedGlobal += ParamF;
    }
    public override void OnUnequip(Unit unit)
    {
        unit.Ua.SpeedGlobal -= ParamF;
    }
    public override void RandomSummonParam()
    {
        ParamF = (float)GD.Randfn(5, 1);
    }
}
public class AddSpeedCombat : ItemEffect
{
    public override int Value => 3;
    public override void OnEquip(Unit unit)
    {
        unit.Ua.SpeedCombat += ParamF;
    }
    public override void OnUnequip(Unit unit)
    {
        unit.Ua.SpeedCombat -= ParamF;
    }
    public override void RandomSummonParam()
    {
        ParamF = (float)GD.Randfn(8, 2);
    }
}
public class AddSpeedMove : ItemEffect
{
    public override int Value => 3;
    public override void OnEquip(Unit unit)
    {
        unit.Ua.SpeedMove += ParamF;
    }
    public override void OnUnequip(Unit unit)
    {
        unit.Ua.SpeedMove -= ParamF;
    }
    public override void RandomSummonParam()
    {
        ParamF = (float)GD.Randfn(15, 3);
    }
}
public class AddDamageBody : ItemEffect
{
    public override int Value => 3;
    public override void OnEquip(Unit unit)
    {
        unit.Ua.DamageBody += ParamF;
    }
    public override void OnUnequip(Unit unit)
    {
        unit.Ua.DamageBody -= ParamF;
    }
    public override void RandomSummonParam()
    {
        ParamF = (float)GD.Randfn(8, 2);
    }
}
public class AddDamageBullet : ItemEffect
{
    public override int Value => 3;
    public override void OnEquip(Unit unit)
    {
        unit.Ua.DamageBullet += ParamF;
    }
    public override void OnUnequip(Unit unit)
    {
        unit.Ua.DamageBullet -= ParamF;
    }
    public override void RandomSummonParam()
    {
        ParamF = (float)GD.Randfn(5, 1);
    }
}
public class AddHealRatio : ItemEffect
{
    public override int Value => 3;
    public override string InfoParam => $"{ParamF * 100:F1}";
    public override void OnEquip(Unit unit)
    {
        unit.Ua.HealRatio += ParamF;
    }
    public override void OnUnequip(Unit unit)
    {
        unit.Ua.HealRatio -= ParamF;
    }
    public override void RandomSummonParam()
    {
        ParamF = (float)GD.Randfn(0.1, 0.02);
    }
}
public class AddBodyDamageAccuracy : ItemEffect
{
    public override int Value => 3;
    public override string InfoParam => $"{ParamF * 100:F1}";
    public override void OnEquip(Unit unit)
    {
        unit.Ua.BodyDamageAccuracy += ParamF;
    }
    public override void OnUnequip(Unit unit)
    {
        unit.Ua.BodyDamageAccuracy -= ParamF;
    }
    public override void RandomSummonParam()
    {
        ParamF = (float)GD.Randfn(0.1, 0.02);
    }
}
public class AddBulletDamageAccuracy : ItemEffect
{
    public override int Value => 3;
    public override string InfoParam => $"{ParamF * 100:F1}";
    public override void OnEquip(Unit unit)
    {
        unit.Ua.BulletDamageAccuracy += ParamF;
    }
    public override void OnUnequip(Unit unit)
    {
        unit.Ua.BulletDamageAccuracy -= ParamF;
    }
    public override void RandomSummonParam()
    {
        ParamF = (float)GD.Randfn(0.1, 0.02);
    }
}
public class AddDamageEvasion : ItemEffect
{
    public override int Value => 3;
    public override string InfoParam => $"{ParamF * 100:F1}";
    public override void OnEquip(Unit unit)
    {
        unit.Ua.DamageEvasion += ParamF;
    }
    public override void OnUnequip(Unit unit)
    {
        unit.Ua.DamageEvasion -= ParamF;
    }
    public override void RandomSummonParam()
    {
        ParamF = (float)GD.Randfn(0.1, 0.02);
    }
}
public class AddBulletGraze : ItemEffect
{
    public override int Value => 3;
    public override string InfoParam => $"{ParamF * 100:F1}";
    public override void OnEquip(Unit unit)
    {
        unit.Ua.BulletGraze += ParamF;
    }
    public override void OnUnequip(Unit unit)
    {
        unit.Ua.BulletGraze -= ParamF;
    }
    public override void RandomSummonParam()
    {
        ParamF = (float)GD.Randfn(0.2, 0.04);
    }
}
public class AddStr : ItemEffect
{
    public override int Value => 3;
    public override void OnEquip(Unit unit)
    {
        unit.Ua.Str += ParamI;
    }
    public override void OnUnequip(Unit unit)
    {
        unit.Ua.Str -= ParamI;
    }
    public override void RandomSummonParam()
    {
        ParamI = GD.RandRange(1, 5);
    }
}
public class AddDex : ItemEffect
{
    public override int Value => 3;
    public override void OnEquip(Unit unit)
    {
        unit.Ua.Dex += ParamI;
    }
    public override void OnUnequip(Unit unit)
    {
        unit.Ua.Dex -= ParamI;
    }
    public override void RandomSummonParam()
    {
        ParamI = GD.RandRange(1, 5);
    }
}
public class AddCon : ItemEffect
{
    public override int Value => 3;
    public override void OnEquip(Unit unit)
    {
        unit.Ua.Con += ParamI;
    }
    public override void OnUnequip(Unit unit)
    {
        unit.Ua.Con -= ParamI;
    }
    public override void RandomSummonParam()
    {
        ParamI = GD.RandRange(1, 5);
    }
}
public class AddSpi : ItemEffect
{
    public override int Value => 3;
    public override void OnEquip(Unit unit)
    {
        unit.Ua.Spi += ParamI;
    }
    public override void OnUnequip(Unit unit)
    {
        unit.Ua.Spi -= ParamI;
    }
    public override void RandomSummonParam()
    {
        ParamI = GD.RandRange(1, 5);
    }
}
public class AddMag : ItemEffect
{
    public override int Value => 3;
    public override void OnEquip(Unit unit)
    {
        unit.Ua.Mag += ParamI;
    }
    public override void OnUnequip(Unit unit)
    {
        unit.Ua.Mag -= ParamI;
    }
    public override void RandomSummonParam()
    {
        ParamI = GD.RandRange(1, 5);
    }
}
public class AddCun : ItemEffect
{
    public override int Value => 3;
    public override void OnEquip(Unit unit)
    {
        unit.Ua.Cun += ParamI;
    }
    public override void OnUnequip(Unit unit)
    {
        unit.Ua.Cun -= ParamI;
    }
    public override void RandomSummonParam()
    {
        ParamI = GD.RandRange(1, 5);
    }
}
