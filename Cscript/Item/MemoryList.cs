#pragma warning disable IDE1006 // 命名样式
public class mUaAny : Memory
{
    public mUaAny()
    {
        Weight = 1f;
        chestValue = 3;
    }

    public override void OnEquip(Unit unit)
    {
        unit.Ua.uaPoint += 1;
    }
}
public class mSkill : Memory
{
    public mSkill()
    {
        Weight = 5f;
        chestValue = 5;
    }

    public override void OnEquip(Unit unit)
    {
        unit.Ua.skillPoint += 2;
    }
}
public class mUaStr : Memory
{
    public mUaStr()
    {
        Weight = 3f;
    }

    public override void OnEquip(Unit unit)
    {
        unit.Ua.Str += 4;
    }
}

public class mUaDex : Memory
{
    public mUaDex()
    {
        Weight = 3f;
    }

    public override void OnEquip(Unit unit)
    {
        unit.Ua.Dex += 4;
    }
}

public class mUaCon : Memory
{
    public mUaCon()
    {
        Weight = 3f;
    }

    public override void OnEquip(Unit unit)
    {
        unit.Ua.Con += 4;
    }
}

public class mUaSpi : Memory
{
    public mUaSpi()
    {
        Weight = 3f;
    }

    public override void OnEquip(Unit unit)
    {
        unit.Ua.Spi += 4;
    }
}

public class mUaMag : Memory
{
    public mUaMag()
    {
        Weight = 3f;
    }

    public override void OnEquip(Unit unit)
    {
        unit.Ua.Mag += 4;
    }
}

public class mUaCun : Memory
{
    public mUaCun()
    {
        Weight = 3f;
    }

    public override void OnEquip(Unit unit)
    {
        unit.Ua.Cun += 4;
    }
}

