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

    public void OnEquip(Unit unit)
    {
        unit.Ua.MaxHp += 20 * Setting.chaos;
        unit.Ua.uaPoint += (int)(2 * Setting.chaos);
        unit.Ua.skillPoint += 3;
        unit.Equipment.TryEquip(CreateItem("BarrageSet",
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

    public void OnEquip(Unit unit)
    {
        unit.Ua.MaxHp += 20 * Setting.chaos;
        unit.Ua.uaPoint += (int)(6 * Setting.chaos);
        unit.Ua.MaxHp *= 1 + (0.2f * Setting.chaos);
        unit.Ua.MaxSp *= 1 + (0.1f * Setting.chaos);
        unit.Ua.MaxMp *= 1 + (0.1f * Setting.chaos);
        unit.Ua.skillPoint += 6;
        unit.Equipment.TryEquip(CreateItem("BarrageSet",
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

    public void OnEquip(Unit unit)
    {
        unit.Ua.MaxHp += 20 * Setting.chaos;
        unit.Ua.uaPoint += (int)(12 * Setting.chaos);
        unit.Ua.MaxHp *= 1 + (0.6f * Setting.chaos);
        unit.Ua.MaxSp *= 1 + (0.2f * Setting.chaos);
        unit.Ua.MaxMp *= 1 + (0.2f * Setting.chaos);
        unit.Ua.skillPoint += 10;
        unit.Equipment.TryEquip(CreateItem("BarrageSet",
            new Dictionary<string, object> { { "barrage", new Dictionary<string, object> { { "MaxComponents", 2 } } } }), unit);
        unit.Equipment.TryEquip(CreateItem("BarrageSet",
            new Dictionary<string, object> { { "barrage", new Dictionary<string, object> { { "MaxComponents", 2 } } } }), unit);
    }
}