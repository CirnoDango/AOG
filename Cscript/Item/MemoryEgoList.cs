#pragma warning disable IDE1006 // 命名样式
using System.Collections.Generic;

public class mElite : Memory
{
    public mElite()
    {
        Name = "mElite";
        Weight = 0f;
        Description = "elite奖励";
        Group = "Ego";
    }

    public override void OnEquip(Unit unit)
    {
        unit.MaxHp += 20 * Setting.chaos;
        unit.Ua.uaPoint += (int)(2 * Setting.chaos);
        unit.Ua.skillPoint += 3;
        unit.equipment.TryEquip(GetItemName("BarrageSet",
            new Dictionary<string, object> { { "barrage", new Dictionary<string, object> { { "MaxComponents", 2 } } } }), unit);
    }
}
public class mGreat : Memory
{
    public mGreat()
    {
        Name = "mGreat";
        Weight = 0f;
        Description = "mGreat奖励";
        Group = "Ego";
    }

    public override void OnEquip(Unit unit)
    {
        unit.MaxHp += 20 * Setting.chaos;
        unit.Ua.uaPoint += (int)(6 * Setting.chaos);
        unit.MaxHp *= 1 + (0.2f * Setting.chaos);
        unit.MaxSp *= 1 + (0.1f * Setting.chaos);
        unit.MaxMp *= 1 + (0.1f * Setting.chaos);
        unit.Ua.skillPoint += 6;
        unit.equipment.TryEquip(GetItemName("BarrageSet",
            new Dictionary<string, object> { { "barrage", new Dictionary<string, object> { { "MaxComponents", 2 } } } }), unit);
    }
}
public class mBoss : Memory
{
    public mBoss()
    {
        Name = "mBoss";
        Weight = 0f;
        Description = "mBoss";
        Group = "Ego";
    }

    public override void OnEquip(Unit unit)
    {
        unit.MaxHp += 20 * Setting.chaos;
        unit.Ua.uaPoint += (int)(12 * Setting.chaos);
        unit.MaxHp *= 1 + (0.6f * Setting.chaos);
        unit.MaxSp *= 1 + (0.2f * Setting.chaos);
        unit.MaxMp *= 1 + (0.2f * Setting.chaos);
        unit.Ua.skillPoint += 10;
        unit.equipment.TryEquip(GetItemName("BarrageSet",
            new Dictionary<string, object> { { "barrage", new Dictionary<string, object> { { "MaxComponents", 2 } } } }), unit);
        unit.equipment.TryEquip(GetItemName("BarrageSet",
            new Dictionary<string, object> { { "barrage", new Dictionary<string, object> { { "MaxComponents", 2 } } } }), unit);
    }
}